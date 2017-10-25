using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Caching;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_SV : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_SV";
        private const int SV_DELIVERYINFO_CACHE_MINUTES = 60;

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            string instruction = base.GetShippingInstructionsForDS(shoppingCart, distributorId, locale);
            return string.Format("{0},{1}", instruction ?? string.Empty, "Gracias por su Orden");
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            if (address != null && address.Address != null &&
                !string.IsNullOrEmpty(address.Address.StateProvinceTerritory) &&
                !string.IsNullOrEmpty(address.Address.City))
            {
                var cityState = string.Format("{0}|{1}", 
                        address.Address.StateProvinceTerritory, 
                        address.Address.City);

                //Looking for WH and Freight code
                var options = HttpRuntime.Cache[CacheKey] as Dictionary<string, string[]>;
                if (options == null || !options.ContainsKey(cityState))
                {
                    var optionForCity = GetFreightCodeAndWarehouseFromService(address);
                    if (options == null) options = new Dictionary<string, string[]>();
                    if (optionForCity != null && !string.IsNullOrEmpty(optionForCity[0]) &&
                        !string.IsNullOrEmpty(optionForCity[1]))
                    {
                        options.Add(cityState, optionForCity);
                    }
                    HttpRuntime.Cache.Insert(CacheKey, options, null, DateTime.Now.AddMinutes(SV_DELIVERYINFO_CACHE_MINUTES),
                         Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }

                var cityOption = options.FirstOrDefault(o => o.Key == cityState);
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
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "SV",
                Locale = "es-SV",
                State = string.Format("{0}|{1}",
                    address.Address.StateProvinceTerritory,
                    address.Address.City)
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }        

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                     string.IsNullOrWhiteSpace(a.City))
                // fails when State or City are not set (they're required)
                return false;

            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                 !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City))
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }
    }
}
