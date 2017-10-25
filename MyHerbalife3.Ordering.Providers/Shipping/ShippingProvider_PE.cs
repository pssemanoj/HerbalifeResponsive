using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_PE : ShippingProviderBase
    {
        private const string cacheKey = "PROVINCES_PE";
        private const int cacheMinutes = 60;

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName ? string.Format("{0}<br/>{1}{2}<br/>{3}<br/>{4}, {5}<br/>{6}{7}", address.Recipient ?? string.Empty,
                    address.Address.Line1, string.IsNullOrEmpty(address.Address.Line2) ? string.Empty : string.Format(",{0}", address.Address.Line2), 
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, 
                    string.IsNullOrEmpty(address.Address.PostalCode) ? string.Empty : string.Format("{0}<br>", address.Address.PostalCode),
                    formatPhone(address.Phone)) :
                    string.Format("{0}{1}<br/>{2}<br/>{3}, {4}<br>{5}{6}",
                    address.Address.Line1, string.IsNullOrEmpty(address.Address.Line2) ? string.Empty : string.Format(",{0}", address.Address.Line2),
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, 
                    string.IsNullOrEmpty(address.Address.PostalCode) ? string.Empty : string.Format("{0}<br>", address.Address.PostalCode),
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}{2}<br>{3},{4}<br>{5}", description,
                    address.Address.Line1, string.IsNullOrEmpty(address.Address.Line2) ? string.Empty : string.Format(",{0}", address.Address.Line2),
                    address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
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
                    else
                    {
                        if (freightCodesAndWarehouses == null) freightCodesAndWarehouses = new Dictionary<string, string[]>();
                        freightCodesAndWarehouses.Add(address.Address.StateProvinceTerritory, freightCodeAndWarehouse);
                    }
                }
            }
            return freightCodeAndWarehouse;
        }

        public override bool IsValidShippingAddress(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null &&
                shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                shoppingCart.DeliveryInfo.Address != null && shoppingCart.DeliveryInfo.Address.Address != null &&
                (string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.CountyDistrict) ||
                 string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.PostalCode)))
            {
                return false;
            }
            return true;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(string state)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "PE",
                Locale = "es-PE",
                State = state
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                var freightCodeAndWarehouse = new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
                var freightCodesAndWarehouses = HttpRuntime.Cache[cacheKey] as Dictionary<string, string[]> ??
                                                new Dictionary<string, string[]>();
                freightCodesAndWarehouses.Add(state, freightCodeAndWarehouse);
                HttpRuntime.Cache.Insert(cacheKey, freightCodesAndWarehouses, null, DateTime.Now.AddMinutes(cacheMinutes),
                    Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                return freightCodeAndWarehouse;
            }
            return null;
        }
    }
}
