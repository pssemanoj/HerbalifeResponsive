using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    ///     Shipping provider for PY
    /// </summary>
    public class ShippingProvider_PY : ShippingProviderBase
    {
        #region Constants and Fields
        private const string CacheKey = "DeliveryInfo_PY";
        private const string PUPCacheKey = "PickUpDeliveryInfo_PY";
        private const int PY_DELIVERYINFO_CACHE_MINUTES = 60;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Get the freight and warehouse code for the address.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns></returns>
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            if (null != address && null != address.Address)
            {
                string state = address.Address.StateProvinceTerritory;

                if (!string.IsNullOrEmpty(state))
                {
                    var freightCodeAndWarehouseFromService = GetFreightCodeAndWarehouseFromService(address);
                    if (freightCodeAndWarehouseFromService != null)
                    {
                        freightCodeAndWarehouse[0] = freightCodeAndWarehouseFromService[0] ?? freightCodeAndWarehouse[0];
                        freightCodeAndWarehouse[1] = freightCodeAndWarehouseFromService[1] ?? freightCodeAndWarehouse[1];
                    }
                }
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
                        Country = "PY",
                        State = string.Format("{0}", address.Address.StateProvinceTerritory),
                        Locale = "es-PY"
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
                    LoggerHelper.Error(string.Format("GetFreightCodeAndWarehouseFromService error: Country: PY, error: {0}", ex.ToString()));
                }
            }
            return null;
        }

      
        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            AddressFieldForCountryRequest_V01 request = new AddressFieldForCountryRequest_V01()
            {
                AddressField = AddressPart.ZIPCODE,
                Country = a.Country,
                State = a.StateProvinceTerritory,
                City = a.City
            };
            List<string> lookupResults = GetAddressField(request);
            
            if ((!string.IsNullOrWhiteSpace(a.PostalCode)) && (GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City)) && (lookupResults.Contains(a.PostalCode)))
                return true;
            else
                return false;
        }
        #endregion
    }
}
