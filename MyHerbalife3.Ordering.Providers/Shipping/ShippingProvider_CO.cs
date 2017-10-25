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
    public class ShippingProvider_CO : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_CO";
        private const string PUPCacheKey = "PickUpDeliveryInfo_CO";
        private const int CO_DELIVERYINFO_CACHE_MINUTES = 60;

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
                address.Address.Line4 = address.Recipient;
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1}{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1,
                                                       string.IsNullOrEmpty(address.Address.Line2)
                                                           ? string.Empty
                                                           : string.Format(",{0}", address.Address.Line2),
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       string.IsNullOrEmpty(address.Address.CountyDistrict)
                                                           ? string.Empty
                                                           : string.Format("{0}", address.Address.CountyDistrict),
                                                       //string.IsNullOrEmpty(address.Address.PostalCode)
                                                         //  ? string.Empty
                                                         //  : string.Format("{0}<br>", address.Address.PostalCode),
                                                       formatPhone(address.Phone))
                                       : string.Format("{0}{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1,
                                                       string.IsNullOrEmpty(address.Address.Line2)
                                                           ? string.Empty
                                                           : string.Format(",{0}", address.Address.Line2),
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       string.IsNullOrEmpty(address.Address.CountyDistrict)
                                                           ? string.Empty
                                                           : string.Format("{0}", address.Address.CountyDistrict),
                                                       formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                formattedAddress = includeName ?
                    string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}", address.Address.Line4 ?? string.Empty,
                    address.Address.Line2, address.Address.Line1 ?? string.Empty,
                    address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.CountyDistrict,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                    address.Address.Line2, address.Address.Line1 ?? string.Empty,
                    address.Address.City, address.Address.StateProvinceTerritory, address.Address.CountyDistrict,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}{2}<br>{3},{4}, {5}", description,
                                                 address.Address.Line1,
                                                 string.IsNullOrEmpty(address.Address.Line2)
                                                     ? string.Empty
                                                     : string.Format(",{0}", address.Address.Line2),
                                                 address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.CountyDistrict);
            }
            return formattedAddress;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] { string.Empty, string.Empty };

            if (address != null && address.Address != null &&
                !string.IsNullOrEmpty(address.Address.StateProvinceTerritory) &&
                !string.IsNullOrEmpty(address.Address.City))
            {
                string cityName = address.Address.City.ToUpper().Trim();
                var cityState = string.Format("{0}|{1}", address.Address.StateProvinceTerritory, address.Address.City);
                // Looking the warehouse
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
                    HttpRuntime.Cache.Insert(CacheKey, options, null, DateTime.Now.AddMinutes(CO_DELIVERYINFO_CACHE_MINUTES),
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
            using (var proxy = ServiceClientProvider.GetShippingServiceProxy())
            {
                try
                {
                    string state = string.IsNullOrWhiteSpace(address.Address.CountyDistrict)
                                      ? string.Format("{0}|{1}", address.Address.StateProvinceTerritory,
                                                address.Address.City)
                                      : string.Format("{0}|{1}|{2}", address.Address.StateProvinceTerritory,
                                                address.Address.City, address.Address.CountyDistrict);
                    var request = new DeliveryOptionForCountryRequest_V01
                    {
                        Country = "CO",
                        State   = state,
                        Locale = "es-CO"
                    };

                    var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                    if (response != null && response.DeliveryAlternatives != null &&
                        response.DeliveryAlternatives.Count > 0)
                    {
                        var shippingOption = response.DeliveryAlternatives.FirstOrDefault(o => o.IsDefault)!=null ? response.DeliveryAlternatives.FirstOrDefault(o => o.IsDefault) : response.DeliveryAlternatives.FirstOrDefault();
                        if (shippingOption != null)
                        {
                            return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("GetFreightCodeAndWarehouseFromService error: Country: CO, error: {0}", ex.ToString()));
                }
            }
            return null;
        }

        public override ShippingInfo GetShippingInfoFromID(
            string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            DeliveryOption deliveryOptionForAddress = null;
            ShippingInfo shippingInfo = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference =
                    this.GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        var pickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = this.GetDeliveryOptions(type, null);
                        if (doList != null)
                        {
                            deliveryOptionForAddress = doList.Find(d => d.Id == pickupLocationID);
                            if (deliveryOptionForAddress != null)
                            {
                                deliveryOptionForAddress.Address.Line4 = deliveryOptionForAddress.Description;
                                //*******************TBD ************************************************************
                                //deliveryOptionForAddress.WarehouseCode = "";
                                //deliveryOptionForAddress.FreightCode = "";
                                //*******************TBD ************************************************************
                                shippingInfo = new ShippingInfo(deliveryOptionForAddress) { Id = deliveryOptionID };
                                return shippingInfo;
                            }
                        }
                    }
                    return shippingInfo;
                }
            }
            else
            {
                return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            }

            return null;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                if (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                {
                    HttpRuntime.Cache.Remove(PUPCacheKey);
                }
                return GetDeliveryOptionsFromCache(System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
            }
            return base.GetDeliveryOptions(type, address);
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(
            string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            // Verify the alias for the locations to generate a display name if needed
            // and check if the location exists
            foreach (var location in pickupLocations)
            {
                // Verify if the location exists
                var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                if (shippingInfo == null)
                {
                    location.PickupLocationNickname = null;
                    continue;
                }

                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                    var address = new ShippingAddress_V02(
                        shippingInfo.Address.ID,
                        shippingInfo.Description,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        shippingInfo.Address.Address,
                        string.Empty,
                        string.Empty,
                        shippingInfo.Address.IsPrimary,
                        shippingInfo.Address.Alias,
                        DateTime.Now);
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
            return pickupLocations.Where(l => !string.IsNullOrEmpty(l.PickupLocationNickname)).ToList();
        }


        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                if (shippingInfo != null)
                {
                    if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        string countryCode = locale.Substring(3, 2);
                        List<PickupLocationPreference_V01> pickupLocationPreference =
                            GetPickupLocationsPreferences(distributorId, countryCode);
                        if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                        {
                            var pickupLocation = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                            var accessPoints = GetDeliveryOptionsFromCache(locale, null);
                            if (pickupLocation != null && accessPoints != null)
                            {
                                var ppp =
                                    accessPoints.Where(ap => ap.Id == pickupLocation.PickupLocationID).FirstOrDefault();
                                if (ppp != null)
                                {
                                    return !string.IsNullOrEmpty(ppp.Description) ? ppp.Description : ppp.Name;
                                }
                            }
                        }
                    }
                }
                return string.Empty;
            }
            else
            {
                return base.GetShippingInstructionsForDS(shoppingCart, distributorId, locale);
            }
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                       string locale,
                                                                       ShippingAddress_V01 address)
        {

            if (address != null)
            {
                var deliveryOptions = GetDeliveryOptionsFromService(Country, locale, address);
                 var final = new List<DeliveryOption>();
               

                foreach ( var option in deliveryOptions)
                {
                    final.Add(new DeliveryOption(option));
                }
                return final;

            }
            return null;
        }

        public override string GetDifferentHtmlFragment(string deliverymethodoption)
        {
            string option = deliverymethodoption.Trim();
            if (!string.IsNullOrEmpty(option))
            {
                switch (option)
                {
                    case "CFE":
                        {
                            return "ExpressDelivery.html";
                        }
                    default:
                        {
                            return "shippingmethod.html";
                        }
                }
            }
            return string.Empty;
        }

        public override string GetWarehouseFromShippingMethod(string freighcode, ShippingAddress_V01 address)
        {

            List<ShippingOption_V01> deliveryOptions = GetDeliveryOptionsFromService("CO", "es-CO", address);
            var shippingOption = deliveryOptions.Where(s => s.FreightCode.Equals(freighcode));
            if (shippingOption!=null) 
                 return  shippingOption.FirstOrDefault().WarehouseCode;
            return string.Empty;
        }
        #region Private Methods

        private static List<ShippingOption_V01> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();
            request.Country = Country;
            request.State = string.IsNullOrWhiteSpace(address.Address.CountyDistrict)
                                      ? string.Format("{0}|{1}", address.Address.StateProvinceTerritory,
                                                address.Address.City)
                                      : string.Format("{0}|{1}|{2}", address.Address.StateProvinceTerritory,
                                                address.Address.City, address.Address.CountyDistrict);
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;

            return response.DeliveryAlternatives;

        }
        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[PUPCacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = GetDeliveryOptionsFromService(locale);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }
            if (deliveryOptions == null)
            {
                return null;
            }
            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string locale)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            DeliveryPickupAlternativesResponse_V04 pickupAlternativesResponse = null;
            pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V04 { Locale = locale })).GetDeliveryPickupAlternativesResult as
                DeliveryPickupAlternativesResponse_V04;
            if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
            {
                deliveryOptions.AddRange(
                    from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
            }
            return deliveryOptions;
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(
                    PUPCacheKey,
                    options,
                    null,
                    DateTime.Now.AddMinutes(CO_DELIVERYINFO_CACHE_MINUTES),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ((string.IsNullOrWhiteSpace(a.PostalCode)) && (GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory))&&(GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City)))
                return true;
           else
                return false;
         
        }
        #endregion

    }
}