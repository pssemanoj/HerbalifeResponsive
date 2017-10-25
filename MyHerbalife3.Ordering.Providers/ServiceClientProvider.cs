using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers.AddressValidationSVC;
using MyHerbalife3.Ordering.Providers.ChinaOrderSVC;
using MyHerbalife3.Ordering.Providers.ChinaShippingSVC;
using MyHerbalife3.Ordering.Providers.CommunicationSVC;
using MyHerbalife3.Ordering.Providers.DistributorSVC;
using MyHerbalife3.Ordering.Providers.EventSVC;
using MyHerbalife3.Ordering.Providers.OrderImportBtWS;
//using MyHerbalife3.Ordering.Providers.OrderSVC;
using MyHerbalife3.Ordering.Providers.ShoppingCartServiceReference;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;
using MyHerbalife3.Ordering.Providers.EmailPublisherSR;
using MyHerbalife3.Ordering.Providers.MobileAnalyticsSvc;

namespace MyHerbalife3.Ordering
{
    /// <summary>
    /// Proxy service provider.
    /// </summary>
    public static class ServiceClientProvider
    {
        //public static OrderServiceClient GetOrderServiceProxy()
        //{
        //    return ServiceClientFactory.CreateProxy<OrderServiceClient, IOrderService>(
        //        Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteUrl"),
        //        Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteSecureUrl", string.Empty),
        //        true);
        //}

        public static SubmitOrderClient GetSubmitOrderProxy()
        {
            return ServiceClientFactory.CreateProxy<SubmitOrderClient, SubmitOrder>(
                Settings.GetRequiredAppSetting("IAGlobalOrderingUrl"),
                Settings.GetRequiredAppSetting("IAGlobalOrderingSecureUrl", string.Empty),
                true);
        }

        public static PaymentGatewayBridgeSVC.PaymentGatewayBridgeInterfaceClient GetPaymentGatewayBridgeProxy()
        {
            return ServiceClientFactory.CreateProxy
                <MyHerbalife3.Ordering.Providers.PaymentGatewayBridgeSVC.PaymentGatewayBridgeInterfaceClient,
                    MyHerbalife3.Ordering.Providers.PaymentGatewayBridgeSVC.IPaymentGatewayBridgeInterface>(
                        Settings.GetRequiredAppSetting("IAPaymentGatewayBridgeUrl"),
                        Settings.GetRequiredAppSetting("IAPaymentGatewayBridgeSecureUrl", string.Empty),
                        true);
        }

        //public static CatalogInterfaceClient GetCatalogServiceProxy()
        //{
        //    return ServiceClientFactory.CreateProxy<CatalogInterfaceClient, ICatalogInterface>(
        //        Settings.GetRequiredAppSetting("IAGlobalOrderingCatalogUrl"),
        //        Settings.GetRequiredAppSetting("IAGlobalOrderingCatalogSecureUrl", string.Empty),
        //        true);
        //}

        public static ShoppingCartServiceClient GetShoppingCartServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<ShoppingCartServiceClient, IShoppingCartService>(
                Settings.GetRequiredAppSetting("IAShoppingCartUrl"),
                Settings.GetRequiredAppSetting("IAShoppingCartSecureUrl", string.Empty),
                true);
        }

        //public static ChinaInterfaceClient GetChinaOrderServiceProxy()
        //{
        //    return ServiceClientFactory.CreateProxy<ChinaInterfaceClient, IChinaInterface>(
        //        Settings.GetRequiredAppSetting("IAChinaOrderUrl"),
        //        Settings.GetRequiredAppSetting("IAChinaOrderSecureUrl", string.Empty),
        //        true);
        //}

        public static CommunicationServiceClient GetCommunicationServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<CommunicationServiceClient, ICommunicationService>(
                Settings.GetRequiredAppSetting("IACommunicationUrl"),
                Settings.GetRequiredAppSetting("IACommunicationSecureUrl", string.Empty),
                true);
        }

        public static DistributorServiceClient GetDistributorServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<DistributorServiceClient, IDistributorService>(
                Settings.GetRequiredAppSetting("IADistributorUrl"),
                Settings.GetRequiredAppSetting("IADistributorSecureUrl", string.Empty),
                true);
        }

        public static DistributorSVC.DistributorServiceClient GetDistributorServiceProxyChina()
        {
            return ServiceClientFactory.CreateProxy<DistributorSVC.DistributorServiceClient, DistributorSVC.IDistributorService>(
                Settings.GetRequiredAppSetting("IADistributorUrl"),
                Settings.GetRequiredAppSetting("IADistributorSecureUrl", string.Empty),
                true);
        }

        public static EventInterfaceClient GetEventServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<EventInterfaceClient, IEventInterface>(
                Settings.GetRequiredAppSetting("IAEventUrl"),
                Settings.GetRequiredAppSetting("IAEventSecureUrl", string.Empty),
                true);
        }

        public static ShippingSVC.ShippingClient GetShippingServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<ShippingSVC.ShippingClient, ShippingSVC.IShipping>(
                Settings.GetRequiredAppSetting("IAShippingUrl"),
                Settings.GetRequiredAppSetting("IAShippingSecureUrl", string.Empty),
                true);
        }

        public static ChinaShippingClient GetChinaShippingServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<ChinaShippingClient, IChinaShipping>(
                Settings.GetRequiredAppSetting("IAChinaShippingUrl"),
                Settings.GetRequiredAppSetting("IAChinaShippingSecureUrl", string.Empty),
                true);
        }

        public static ShippingMexicoSVC.ShippingClient GetMexicoShippingServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<ShippingMexicoSVC.ShippingClient, ShippingMexicoSVC.IShipping>(
                Settings.GetRequiredAppSetting("IAShippingMexicoUrl"),
                Settings.GetRequiredAppSetting("IAShippingMexicoSecureUrl", string.Empty),
                true);
        }

        public static AddressValidation_PortTypeClient GetAddressValidationServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<AddressValidation_PortTypeClient, AddressValidation_PortType>(
                Settings.GetRequiredAppSetting("IALegacyAddressValidationV02Url"),
                Settings.GetRequiredAppSetting("IALegacyAddressValidationV02SecureUrl", string.Empty),
                true);
        }

        public static Inbox_PortTypeClient GetEmailPublisherServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<EmailPublisherSR.Inbox_PortTypeClient, EmailPublisherSR.Inbox_PortType>(
                Settings.GetRequiredAppSetting("IAEmailPublisherV03Url"),
                Settings.GetRequiredAppSetting("IAEmailPublisherV03SecureUrl", string.Empty),
                true);
        }

        public static MobileAnalyticsClient GetMobileAnalyticsServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<MobileAnalyticsClient, MobileAnalyticsSvc.IMobileAnalytics>(
                Settings.GetRequiredAppSetting("IAMobileAnalyticsUrl"),
                Settings.GetRequiredAppSetting("IAMobileAnalyticsUrlSecureUrl", string.Empty),
                true);
        }
    }
}