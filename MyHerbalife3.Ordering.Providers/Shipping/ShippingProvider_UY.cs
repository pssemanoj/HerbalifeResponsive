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
    ///     Shipping provider for UY
    /// </summary>
    public class ShippingProvider_UY : ShippingProviderBase
    {
        #region Constants and Fields

        private const string CacheKey = "DeliveryInfo_UY";
        private const string PUPCacheKey = "PickUpDeliveryInfo_UY";
        private const int UY_DELIVERYINFO_CACHE_MINUTES = 60;

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
                        Country = "UY",
                        State = string.Format("{0}", address.Address.StateProvinceTerritory),
                        Locale = "es-UY"
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

 
        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                    DeliveryOptionType type,
                                                    string description,
                                                    bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}, {6}<br>{7}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.StateProvinceTerritory,address.Address.City,address.Address.CountyDistrict,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode);
            }
            if (formattedAddress.IndexOf(",,") > -1 || formattedAddress.IndexOf(", ,") > -1)
            {
                return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
            }
            else
            {
                return formattedAddress;
            }
        }
        protected override bool IsValidShippingAddress(Address_V01 a)
        {

            if ((GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City)))
                return true;
            else
                return false;
        }
        #endregion
    }
}