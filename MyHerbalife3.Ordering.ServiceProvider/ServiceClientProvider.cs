using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.AddressValidationSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.CommunicationSvc;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc;
using MyHerbalife3.Ordering.ServiceProvider.EventRestSvc;
using MyHerbalife3.Ordering.ServiceProvider.EventSvc;
using MyHerbalife3.Ordering.ServiceProvider.MobileAnalyticsSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.PaymentGatewayBridgeSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingChinaSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;
using System;
using MyHerbalife3.Ordering.ServiceProvider.NotificationSVC;
using MyHerbalife3.Ordering.ServiceProvider.NotificationAlertSVC;
using MyHerbalife3.Ordering.ServiceProvider.CRMContactSVC;

namespace MyHerbalife3.Ordering.ServiceProvider
{
    /// <summary>
    /// Proxy service provider.
    /// </summary>
    public static class ServiceClientProvider
    {
        #region Ordering

        public static SubmitOrderClient GetSubmitOrderProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<SubmitOrderClient, SubmitOrder>(
                Settings.GetRequiredAppSetting("IAGlobalOrderingUrl"),
                Settings.GetRequiredAppSetting("IAGlobalOrderingSecureUrl", string.Empty),
                trySecure);
        }

        public static OrderServiceClient GetOrderServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<OrderServiceClient, IOrderService>(
                Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteUrl"),
                Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteSecureUrl", string.Empty),
                trySecure);
        }

        public static ChinaInterfaceClient GetChinaOrderServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<ChinaInterfaceClient, IChinaInterface>(
                Settings.GetRequiredAppSetting("IAChinaOrderUrl"),
                Settings.GetRequiredAppSetting("IAChinaOrderSecureUrl", string.Empty),
                trySecure);
        }

        public static CustomerOrderServiceClient GetCustomerOrderServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<CustomerOrderServiceClient, ICustomerOrderService>(
                Settings.GetRequiredAppSetting("IACustomerOrderUrl"),
                Settings.GetRequiredAppSetting("IACustomerOrderSecureUrl", string.Empty),
                trySecure);
        }

        public static ShoppingCartServiceClient GetShoppingCartServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<ShoppingCartServiceClient, IShoppingCartService>(
                Settings.GetRequiredAppSetting("IAShoppingCartUrl"),
                Settings.GetRequiredAppSetting("IAShoppingCartSecureUrl", string.Empty),
                trySecure);
        }

        #endregion

        #region Payments

        public static PaymentGatewayBridgeInterfaceClient GetPaymentGatewayBridgeProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<PaymentGatewayBridgeInterfaceClient, IPaymentGatewayBridgeInterface>(
                Settings.GetRequiredAppSetting("IAPaymentGatewayBridgeUrl"), 
                Settings.GetRequiredAppSetting("IAPaymentGatewayBridgeSecureUrl", string.Empty), 
                trySecure);
        }

        #endregion

        #region Catalog

        public static CatalogInterfaceClient GetCatalogServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<CatalogInterfaceClient, ICatalogInterface>(
                Settings.GetRequiredAppSetting("IAGlobalOrderingCatalogUrl"),
                Settings.GetRequiredAppSetting("IAGlobalOrderingCatalogSecureUrl", string.Empty),
                trySecure);
        }

        #endregion

        #region Shipping

        public static ShippingSvc.ShippingClient GetShippingServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<ShippingSvc.ShippingClient, ShippingSvc.IShipping>(
                Settings.GetRequiredAppSetting("IAShippingUrl"),
                Settings.GetRequiredAppSetting("IAShippingSecureUrl", string.Empty),
                trySecure);
        }

        public static ShippingMexicoSvc.ShippingClient GetMexicoShippingServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<ShippingMexicoSvc.ShippingClient, ShippingMexicoSvc.IShipping>(
                Settings.GetRequiredAppSetting("IAShippingMexicoUrl"),
                Settings.GetRequiredAppSetting("IAShippingMexicoSecureUrl", string.Empty),
                trySecure);
        }

        public static ChinaShippingClient GetChinaShippingServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<ChinaShippingClient, IChinaShipping>(
                Settings.GetRequiredAppSetting("IAChinaShippingUrl"),
                Settings.GetRequiredAppSetting("IAChinaShippingSecureUrl", string.Empty),
                trySecure);
        }

        public static AddressValidation_PortTypeClient GetAddressValidationServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<AddressValidation_PortTypeClient, AddressValidation_PortType>(
                Settings.GetRequiredAppSetting("IALegacyAddressValidationV02Url"),
                Settings.GetRequiredAppSetting("IALegacyAddressValidationV02SecureUrl", string.Empty),
                trySecure);
        }

        #endregion

        #region Distributor

        public static DistributorServiceClient GetDistributorServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<DistributorServiceClient, IDistributorService>(
                Settings.GetRequiredAppSetting("IADistributorUrl"),
                Settings.GetRequiredAppSetting("IADistributorSecureUrl", string.Empty),
                trySecure);
        }

        public static DistributorCRMServiceClient GetDistributorCRMServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<DistributorCRMServiceClient, IDistributorCRMService>(
                Settings.GetRequiredAppSetting("IADistributorCrmUrl"),
                Settings.GetRequiredAppSetting("IADistributorCrmSecureUrl", string.Empty),
                trySecure);
        }

        public static EventInterfaceClient GetEventServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<EventInterfaceClient, IEventInterface>(
                Settings.GetRequiredAppSetting("IAEventUrl"),
                Settings.GetRequiredAppSetting("IAEventSecureUrl", string.Empty),
                trySecure);
        }

        public static IEventRestInterface GetEventDetailServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<EventRestInterfaceClient, IEventRestInterface>(
                Settings.GetRequiredAppSetting("IAEventDetailUrl"),
                Settings.GetRequiredAppSetting("IAEventDetailSecureUrl", string.Empty),
                trySecure);
        }

        #endregion

        #region Communications

        public static CommunicationServiceClient GetCommunicationServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<CommunicationServiceClient, ICommunicationService>(
                Settings.GetRequiredAppSetting("IACommunicationUrl"),
                Settings.GetRequiredAppSetting("IACommunicationSecureUrl", string.Empty),
                trySecure);
        }

        public static Inbox_PortTypeClient GetEmailPublisherServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<Inbox_PortTypeClient, Inbox_PortType>(
                Settings.GetRequiredAppSetting("IAEmailPublisherV03Url"),
                Settings.GetRequiredAppSetting("IAEmailPublisherV03SecureUrl", string.Empty),
                trySecure);
        }

        #endregion

        #region BI

        public static MobileAnalyticsClient GetMobileAnalyticsServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<MobileAnalyticsClient, MobileAnalyticsSvc.IMobileAnalytics>(
                Settings.GetRequiredAppSetting("IAMobileAnalyticsUrl"),
                Settings.GetRequiredAppSetting("IAMobileAnalyticsUrlSecureUrl", string.Empty),
                trySecure);
        }

        #endregion
        #region Notification
        public static NotificationServiceClient GetNotificationServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<NotificationServiceClient, INotificationService>(
                Settings.GetRequiredAppSetting("IANotificationServiceUrl"),
                Settings.GetRequiredAppSetting("IANotificationServiceSecureUrl", string.Empty),
                trySecure);
        }
        public static NotifyServiceClient GetNotificationAlartServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<NotifyServiceClient, INotifyService>(
                Settings.GetRequiredAppSetting("IAAlertNotificationServiceUrl"),
                Settings.GetRequiredAppSetting("IAAlertNotificationServiceSecureUrl", string.Empty),
                trySecure);
        }
        public static CrmContactServiceClient GetCRMContactServiceProxy()
        {
            var trySecure = Settings.GetRequiredAppSetting("WcfSslEnabled", true);
            return ServiceClientFactory.CreateProxy<CrmContactServiceClient, CrmContactService>(
                Settings.GetRequiredAppSetting("IACrmContactUrl"),
                Settings.GetRequiredAppSetting("IACrmContactUrlSecureUrl", string.Empty),
                trySecure);
        }
        #endregion
    }
}