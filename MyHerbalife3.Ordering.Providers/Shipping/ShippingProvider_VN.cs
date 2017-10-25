using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using System;
    using System.Web;
    using System.Web.Caching;
    using HL.Common.Configuration;
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider;

    public class ShippingProvider_VN : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_VN";
        private const int VN_DELIVERYINFO_CACHE_MINUTES = 60;

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                ShippingInfo shippingInfo = base.GetShippingInfoFromID(shoppingCart.DistributorID, shoppingCart.Locale, shoppingCart.DeliveryInfo.Option, shoppingCart.DeliveryOptionID, shoppingCart.ShippingAddressID);
                SessionInfo sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (shippingInfo != null)
                {
                    if (sessionInfo.IsEventTicketMode)
                    {
                        shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                        shoppingCart.DeliveryInfo.WarehouseCode = shippingInfo.WarehouseCode;
                        address.ShippingMethodID = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                        address.WarehouseCode = shippingInfo.WarehouseCode;
                    }
                    if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                    {
                        shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                        shoppingCart.DeliveryInfo.WarehouseCode = shippingInfo.WarehouseCode;
                        address.ShippingMethodID = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                        address.WarehouseCode = shippingInfo.WarehouseCode;
                    }                    
                }
            }
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            if (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.City))
            {
                var options = HttpRuntime.Cache[CacheKey] as Dictionary<string, string[]>;
                if (options == null || !options.ContainsKey(address.Address.City))
                {
                    var optionForCity = GetFreightCodeAndWarehouseFromService(address);
                    if (options == null) options = new Dictionary<string, string[]>();
                    if (optionForCity != null && !string.IsNullOrEmpty(optionForCity[0]) &&
                        !string.IsNullOrEmpty(optionForCity[1]))
                    {
                        options.Add(address.Address.City, optionForCity);
                    }
                    HttpRuntime.Cache.Insert(CacheKey, options, null, DateTime.Now.AddMinutes(VN_DELIVERYINFO_CACHE_MINUTES),
                         Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }

                var cityOption = options.FirstOrDefault(o => o.Key == address.Address.City);
                if (cityOption.Value != null && !string.IsNullOrEmpty(cityOption.Value[1]))
                {
                    freightCodeAndWarehouse[0] = cityOption.Value[0];
                    freightCodeAndWarehouse[1] = cityOption.Value[1];
                }

                return freightCodeAndWarehouse;
            }

            return freightCodeAndWarehouse;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(ShippingAddress_V01 address)
        {
            using (var proxy = ServiceClientProvider.GetShippingServiceProxy())
            {
                try
                {
                    var request = new DeliveryOptionForCountryRequest_V01
                    {
                        Country = "VN",
                        State = address.Address.City,
                        Locale = "vi-VN"
                    };

                    var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                    if (response != null && response.DeliveryAlternatives != null &&
                        response.DeliveryAlternatives.Count > 0)
                    {
                        var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
                        if (shippingOption != null)
                        {
                            return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("GetFreightCodeAndWarehouseFromService error: Country: VN, error: {0}", ex.ToString()));
                }
            }
            return null;
        }

    }
}
