namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// Shipping provider for HN
    /// </summary>
    public class ShippingProvider_HN : ShippingProviderBase
    {
        #region Public Methods and Operators

        List<string> freightCity = new List<string>() { "SAN PEDRO SULA", "TEGUCIGALPA" };
        private const string CacheKey = "DeliveryInfo_HN";
        private const int HN_DELIVERYINFO_CACHE_MINUTES = 60;

        /// <summary>
        /// Format the address object to send to HMS
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns></returns>
        public override bool FormatAddressForHMS(MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            var addrV01 = new Address_V01()
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                StateProvinceTerritory = address.StateProvinceTerritory,
                CountyDistrict = address.CountyDistrict,
                Country = address.Country
            };            

            if (address != null && !string.IsNullOrEmpty(address.StateProvinceTerritory)
                && !string.IsNullOrEmpty(address.City) && 
                ! IsValidShippingAddress(addrV01) ) //apply this when using OLD Address FORMAT
            {
                address.City = string.Format("{0}, {1}", address.City, address.StateProvinceTerritory);
                address.StateProvinceTerritory = string.Empty;
            }
            return true;
        }

        /// <summary>
        /// Format the Shipping Instructions
        /// </summary>
        /// <param name="shoppingCart">The cart</param>
        /// <param name="distributorId">DS id</param>
        /// <param name="locale">Locale</param>
        /// <returns></returns>
        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0} Gracias por su Orden", shoppingCart.DeliveryInfo.Instruction).Trim();
            }

            return string.Format(
                "{0} {1} Gracias por su Orden",
                shoppingCart.DeliveryInfo.Address.Recipient,
                shoppingCart.DeliveryInfo.Address.Phone);
        }

        /// <summary>
        /// Gets the freight and warehouse codes according address
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns></returns>
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] { string.Empty,string.Empty };

            if (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.City))
            {
                var cityState = string.Format("{0}|{1}|{2}", address.Address.StateProvinceTerritory, address.Address.City,
                                              address.Address.CountyDistrict == null ? string.Empty : address.Address.CountyDistrict);

                //Looking for WH and Freight code
                var options = HttpRuntime.Cache[CacheKey] as Dictionary<string, string[]>;
                if (options == null || !options.ContainsKey(cityState))
                {
                    var optionForCity = GetFreightCodeAndWarehouseFromService(cityState);
                    if (options == null) options = new Dictionary<string, string[]>();
                    if (optionForCity != null && !string.IsNullOrEmpty(optionForCity[0]) &&
                        !string.IsNullOrEmpty(optionForCity[1]))
                    {
                        options.Add(cityState, optionForCity);
                    }
                    HttpRuntime.Cache.Insert(CacheKey, options, null, DateTime.Now.AddMinutes(HN_DELIVERYINFO_CACHE_MINUTES),
                            Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }

                var cityOption = options.FirstOrDefault(o => o.Key == cityState);
                if (cityOption.Value != null && !string.IsNullOrEmpty(cityOption.Value[1]))
                {
                    freightCodeAndWarehouse[0] = cityOption.Value[0].Trim();
                    freightCodeAndWarehouse[1] = cityOption.Value[1].Trim();
                }
                else if (freightCity.Contains(address.Address.City.ToUpper()))
                {
                    freightCodeAndWarehouse[0] = "HNS";
                }
                return freightCodeAndWarehouse;
            }
            return freightCodeAndWarehouse;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(string state)
        {
            using (var proxy = ServiceClientProvider.GetShippingServiceProxy())
            {
                try
                {
                    var request = new DeliveryOptionForCountryRequest_V01
                    {
                        Country = "HN",
                        State = state,
                        Locale = "es-HN"
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
                    LoggerHelper.Error(string.Format("GetFreightCodeAndWarehouseFromService error: Country: HN, error: {0}", ex.ToString()));
                }
            }
            return null;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                string.IsNullOrWhiteSpace(a.City))
                // fails when State or County are empty (they're required)
                return false;

            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City))
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName ? string.Format("{0}<br/>{1}{2}<br/>{3}{4}, {5}<br>{6}",
                    address.Recipient ?? string.Empty,
                    address.Address.Line1,
                    !string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    !string.IsNullOrWhiteSpace(address.Address.CountyDistrict) ? address.Address.CountyDistrict + ", " : "",
                    address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone)) : string.Format("{0}{1}<br/>{2}{3}, {4}<br>{5}",
                    address.Address.Line1,
                    !string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    !string.IsNullOrWhiteSpace(address.Address.CountyDistrict) ? address.Address.CountyDistrict + ", " : "",
                    address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}{1}<br/>{2}<br/>{3}, {4}<br>{5}",
                    description,
                    address.Address.Line1,
                    ! string.IsNullOrEmpty(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode);
            }
            return formattedAddress;
        }

        #endregion
    }
}