using PayPal.AdaptivePayments;
using PayPal.AdaptivePayments.Model;
using System.Collections.Generic;
using VirtoCommerce.Domain.Payment.Model;
using Xunit;
using System;
using System.Text;
using Xunit.Abstractions;

namespace Paypal.AdaptivePayments.UnitTests
{
    public class AdaptivePaymentsSDKTests
    {
        private static string PaypalModeConfigSettingName = "mode";
        private static string PaypalUsernameConfigSettingName = "account1.apiUsername";
        private static string PaypalPasswordConfigSettingName = "account1.apiPassword";
        private static string PaypalSignatureConfigSettingName = "account1.apiSignature";
        private static string PaypalAppIdConfigSettingName = "account1.applicationId";

        // private static string SandboxPaypalBaseUrlFormat = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_ap-payment&paykey={0}";

        private static string ReceiverAccountId = "GJT98UNQR7KWC";
        private static string Mode = "sandbox";
        private static string APIUsername = "em_api1.virtoway.com";
        private static string APIPassword = "T69S32TJRQRZ3B99";
        private static string APISignature = "An5ns1Kso7MWUdW4ErQKJJJ4qi4-AC8NsVtsG6F2RMTHYE-jlx5jBi0m";
        private static string AppId = "APP-80W284485P519543T";
        private static string PaymentCallbackRelativePath = "cart/exter";

        private readonly ITestOutputHelper output;

        public AdaptivePaymentsSDKTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Valid_payment_successfull_validation()
        {
            //arrange
            var service = new AdaptivePaymentsService(GetConfigiration());
            var request = CreatePayRequest();


            //act
            var payResponse = service.Pay(request);
            var setPaymentOptionsResponse = service.SetPaymentOptions(new SetPaymentOptionsRequest { payKey = payResponse.payKey, senderOptions = new SenderOptions { referrerCode = "Virto_SP" }, requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" } });
            // var executePaymentResponse = service.ExecutePayment(new ExecutePaymentRequest { payKey = payResponse.payKey, actionType = "PAY", requestEnvelope = new RequestEnvelope { errorLanguage = "en_US" } });

            //assert
            Assert.Equal("", GetErrors(payResponse.error));
            Assert.Equal("", GetErrors(setPaymentOptionsResponse.error));
            // Assert.Equal("", GetErrors(executePaymentResponse.error));

            output.WriteLine(payResponse.paymentExecStatus + ". PayKey: " + payResponse.payKey);
        }

        private Dictionary<string, string> GetConfigiration()
        {
            var retVal = new Dictionary<string, string>();

            retVal.Add(PaypalModeConfigSettingName, Mode);
            retVal.Add(PaypalUsernameConfigSettingName, APIUsername);
            retVal.Add(PaypalPasswordConfigSettingName, APIPassword);
            retVal.Add(PaypalSignatureConfigSettingName, APISignature);
            retVal.Add(PaypalAppIdConfigSettingName, AppId);

            return retVal;
        }

        private PayRequest CreatePayRequest()
        {
            var context = GetProcessPaymentEvaluationContext();
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

        private ProcessPaymentEvaluationContext GetProcessPaymentEvaluationContext()
        {
            var retVal = new ProcessPaymentEvaluationContext()
            {
                Order = new VirtoCommerce.Domain.Order.Model.CustomerOrder { Id = "dmqkwld3892", Currency = "USD" },
                Store = new VirtoCommerce.Domain.Store.Model.Store { Url = "http://localhost/storefront/Electronics" },
                Payment = new VirtoCommerce.Domain.Order.Model.PaymentIn { Sum = 15.51m, Id = "sw0231" }
            };

            return retVal;
        }

        private string GetErrors(List<ErrorData> errors)
        {
            var sb = new StringBuilder();
            foreach (var error in errors)
            {
                sb.AppendLine(error.errorId + ": " + error.message);
            }
            output.WriteLine(sb.ToString());
            return sb.ToString();
        }
    }
}
