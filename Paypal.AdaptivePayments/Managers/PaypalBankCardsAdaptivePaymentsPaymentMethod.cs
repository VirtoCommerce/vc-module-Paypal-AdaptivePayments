using PayPal.AdaptivePayments;
using PayPal.AdaptivePayments.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using VirtoCommerce.Domain.Payment.Model;

namespace Paypal.AdaptivePayments.Managers
{
    public class PaypalAdaptivePaymentsPaymentMethod : VirtoCommerce.Domain.Payment.Model.PaymentMethod
    {
        public PaypalAdaptivePaymentsPaymentMethod()
            : base("Paypal.AdaptivePayments")
        {
        }

        private static string PaypalAPIReceiverAccountIdStoreSetting = "Paypal.AdaptivePayments.ReceiverAccountId";
        private static string PaypalAPIModeStoreSetting = "Paypal.AdaptivePayments.Mode";
        private static string PaypalAPIUserNameStoreSetting = "Paypal.AdaptivePayments.APIUsername";
        private static string PaypalAPIPasswordStoreSetting = "Paypal.AdaptivePayments.APIPassword";
        private static string PaypalAPISignatureStoreSetting = "Paypal.AdaptivePayments.APISignature";
        private static string PaypalAppIdStoreSetting = "Paypal.AdaptivePayments.AppId";
        private static string PaymentCallbackRelativePathStoreSetting = "Paypal.AdaptivePayments.PaymentCallbackRelativePath";

        private static string PaypalModeConfigSettingName = "mode";
        private static string PaypalUsernameConfigSettingName = "account1.apiUsername";
        private static string PaypalPasswordConfigSettingName = "account1.apiPassword";
        private static string PaypalSignatureConfigSettingName = "account1.apiSignature";
        private static string PaypalAppIdConfigSettingName = "account1.applicationId";

        private static string SandboxPaypalBaseUrlFormat = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_ap-payment&paykey={0}";
        private static string LivePaypalBaseUrlFormat = "https://www.paypal.com/cgi-bin/webscr?cmd=_ap-payment&paykey={0}";

        private string Mode
        {
            get
            {
                return GetSetting(PaypalAPIModeStoreSetting);
            }
        }

        private string ReceiverAccountId
        {
            get
            {
                return GetSetting(PaypalAPIReceiverAccountIdStoreSetting);
            }
        }

        private string APIUsername
        {
            get
            {
                return GetSetting(PaypalAPIUserNameStoreSetting);
            }
        }

        private string APIPassword
        {
            get
            {
                return GetSetting(PaypalAPIPasswordStoreSetting);
            }
        }

        private string APISignature
        {
            get
            {
                return GetSetting(PaypalAPISignatureStoreSetting);
            }
        }

        private string PaymentCallbackRelativePath
        {
            get
            {
                return GetSetting(PaymentCallbackRelativePathStoreSetting);
            }
        }

        private string AppId
        {
            get
            {
                return GetSetting(PaypalAppIdStoreSetting);
            }
        }

        public override PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        public override PaymentMethodGroupType PaymentMethodGroupType
        {
            get { return PaymentMethodGroupType.Paypal; }
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentEvaluationContext context)
        {
            var retVal = new ProcessPaymentResult();

            if (context.Store == null)
                throw new NullReferenceException("Store is required!");

            if (string.IsNullOrEmpty(context.Store.SecureUrl) && string.IsNullOrEmpty(context.Store.Url))
                throw new NullReferenceException("Store must have Url or SecureUrl property assigned!");

            PayResponse payResponse = null;
            string errorText;
            try
            {
                var service = new AdaptivePaymentsService(GetConfiguration());
                var request = CreatePayRequest(context);

                payResponse = service.Pay(request);

                errorText = GetErrors(payResponse.error);

                //var setPaymentOptionsResponse = service.SetPaymentOptions(new SetPaymentOptionsRequest { payKey = payResponse.payKey, senderOptions = new SenderOptions { referrerCode = "Virto_SP" }, requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" } });
                //errorText += GetErrors(setPaymentOptionsResponse.error);

                //var executePaymentResponse = service.ExecutePayment(new ExecutePaymentRequest { payKey = payResponse.payKey, actionType = "PAY", requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" } });
                //errorText += GetErrors(executePaymentResponse.error);
            }
            catch (Exception ex)
            {
                errorText = ex.Message;
            }

            if (string.IsNullOrEmpty(errorText))
            {
                retVal.OuterId = payResponse.payKey;
                retVal.IsSuccess = true;
                retVal.RedirectUrl = string.Format(PaypalBaseUrlFormat, retVal.OuterId);
                retVal.NewPaymentStatus = PaymentStatus.Pending;
            }
            else
            {
                retVal.Error = errorText;
                retVal.NewPaymentStatus = PaymentStatus.Voided;
            }

            return retVal;
        }

        public override PostProcessPaymentResult PostProcessPayment(PostProcessPaymentEvaluationContext context)
        {
            var retVal = new PostProcessPaymentResult();

            var service = new AdaptivePaymentsService(GetConfiguration());

            var response = service.PaymentDetails(new PaymentDetailsRequest
            {
                payKey = context.OuterId,
                requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" }
            });

            if (response.status == "COMPLETED")
            {
                retVal.IsSuccess = true;
                retVal.NewPaymentStatus = PaymentStatus.Paid;
            }
            else if (response.status == "INCOMPLETE" && response.status == "ERROR" && response.status == "REVERSALERROR")
            {
                if (response.error != null && response.error.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var error in response.error)
                    {
                        sb.AppendLine(error.message);
                    }
                    retVal.ErrorMessage = sb.ToString();
                }
                else
                {
                    retVal.ErrorMessage = "payment canceled";
                }

                retVal.NewPaymentStatus = PaymentStatus.Voided;
            }
            else
            {
                retVal.NewPaymentStatus = PaymentStatus.Pending;
            }

            return retVal;
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            var retVal = new ValidatePostProcessRequestResult();

            var cancel = queryString["cancel"];
            var paykey = queryString["paykey"];

            if (!string.IsNullOrEmpty(cancel) && !string.IsNullOrEmpty(paykey))
            {
                bool cancelValue;
                if (bool.TryParse(cancel, out cancelValue))
                {
                    retVal.IsSuccess = !cancelValue;
                    retVal.OuterId = paykey;
                }
            }

            return retVal;
        }

        private PayRequest CreatePayRequest(ProcessPaymentEvaluationContext context)
        {
            var order = context.Order;
            var payment = context.Payment;
            var url = !string.IsNullOrEmpty(context.Store.SecureUrl) ? context.Store.SecureUrl : context.Store.Url;

            var receivers = new List<Receiver>();
            receivers.Add(new Receiver { accountId = ReceiverAccountId, amount = payment.Sum, invoiceId = payment.Id });

            PayRequest retVal = new PayRequest
            {
                requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" },
                currencyCode = order.Currency.ToString(),
                receiverList = new ReceiverList(receivers),
                actionType = "CREATE",
                cancelUrl = string.Format("{0}/{1}?cancel=true&orderId={2}", url, PaymentCallbackRelativePath, order.Id) + "&paykey=${paykey}",
                returnUrl = string.Format("{0}/{1}?cancel=false&orderId={2}", url, PaymentCallbackRelativePath, order.Id) + "&paykey=${paykey}"
            };

            return retVal;
        }

        private Dictionary<string, string> GetConfiguration()
        {
            var retVal = new Dictionary<string, string>();

            retVal.Add(PaypalModeConfigSettingName, Mode);
            retVal.Add(PaypalUsernameConfigSettingName, APIUsername);
            retVal.Add(PaypalPasswordConfigSettingName, APIPassword);
            retVal.Add(PaypalSignatureConfigSettingName, APISignature);
            retVal.Add(PaypalAppIdConfigSettingName, AppId);

            return retVal;
        }

        public string PaypalBaseUrlFormat
        {
            get
            {
                var retVal = string.Empty;

                if (Mode.ToLower().Equals("sandbox"))
                {
                    retVal = SandboxPaypalBaseUrlFormat;
                }
                else if (Mode.ToLower().Equals("live"))
                {
                    retVal = LivePaypalBaseUrlFormat;
                }

                return retVal;
            }
        }

        private string GetErrors(List<ErrorData> errors)
        {
            var sb = new StringBuilder();
            foreach (var error in errors)
            {
                sb.AppendLine(error.errorId + ": " + error.message);
            }
            return sb.ToString();
        }

        public override VoidProcessPaymentResult VoidProcessPayment(VoidProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override CaptureProcessPaymentResult CaptureProcessPayment(CaptureProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override RefundProcessPaymentResult RefundProcessPayment(RefundProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }
    }
}