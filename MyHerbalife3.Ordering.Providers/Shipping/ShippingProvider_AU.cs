using System;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_AU : ShippingProviderBase
    {
        private const string cacheKey = "PROVINCES_AU";
        private const int cacheMinutes = 60;

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (currentShippingInfo != null)
            {
                if (currentShippingInfo.Option == DeliveryOptionType.Pickup)
                {
                    return string.Format("{0}", currentShippingInfo.Address.Phone);
                }
                else
                {
                    return currentShippingInfo.Instruction;
                }
            }
            return string.Empty;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            string[] freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            if (address != null && address.Address != null &&
                !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
            {
                var freightCodesAndWarehouses = HttpRuntime.Cache[cacheKey] as Dictionary<string, string[]>;
                if (freightCodesAndWarehouses != null && freightCodesAndWarehouses.Count > 0 &&
                    freightCodesAndWarehouses.ContainsKey(address.Address.StateProvinceTerritory))
                {
                    var stored = freightCodesAndWarehouses.FirstOrDefault(i => i.Key == address.Address.StateProvinceTerritory);
                    freightCodeAndWarehouse[0] = stored.Value[0];
                    freightCodeAndWarehouse[1] = stored.Value[1];
                }
                else
                {
                    var freightCodeAndWarehouseFromService = GetFreightCodeAndWarehouseFromService(address.Address.StateProvinceTerritory);
                    if (freightCodeAndWarehouseFromService != null)
                    {
                        freightCodeAndWarehouse[0] = freightCodeAndWarehouseFromService[0] ?? freightCodeAndWarehouse[0];
                        freightCodeAndWarehouse[1] = freightCodeAndWarehouseFromService[1] ?? freightCodeAndWarehouse[1];
                    }
                }
            }

            return freightCodeAndWarehouse;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(string state)
        {
            var freightCodesAndWarehouses = HttpRuntime.Cache[cacheKey] as Dictionary<string, string[]> ?? new Dictionary<string, string[]>();

            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "AU",
                Locale = "en-AU",
                State = state
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                var freightCodeAndWarehouse = new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
                freightCodesAndWarehouses.Add(state, freightCodeAndWarehouse);
                HttpRuntime.Cache.Insert(cacheKey, freightCodesAndWarehouses, null, DateTime.Now.AddMinutes(cacheMinutes), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                return freightCodeAndWarehouse;
            }

            return null;
        }
       
    }
}
