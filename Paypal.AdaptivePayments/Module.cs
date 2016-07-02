using System;
using Microsoft.Practices.Unity;
using Paypal.AdaptivePayments.Managers;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace Paypal.AdaptivePayments
{
    public class Module : ModuleBase
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void PostInitialize()
        {
            var settings = _container.Resolve<ISettingsManager>().GetModuleSettings("Paypal.AdaptivePayments");

            Func<PaypalAdaptivePaymentsPaymentMethod> paypalBankCardsAdaptivePaymentsPaymentMethodFactory = () => new PaypalAdaptivePaymentsPaymentMethod
            {
                Name = "Paypal Adaptive Payments",
                Description = "Paypal Adaptive Payments integration",
                LogoUrl = "https://raw.githubusercontent.com/VirtoCommerce/vc-module-Paypal-AdaptivePayments/master/Paypal.AdaptivePayments/Content/paypal_2014_logo.png",
                Settings = settings
            };

            _container.Resolve<IPaymentMethodsService>().RegisterPaymentMethod(paypalBankCardsAdaptivePaymentsPaymentMethodFactory);
        }

        #endregion
    }
}
