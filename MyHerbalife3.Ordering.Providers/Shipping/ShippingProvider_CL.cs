using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Xml;
using System.Web.Caching;
using System.Xml.Linq;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_CL : ShippingProviderBase
    {
        private const string CityBoxType = "CityBox";
        private const string AgencyType = "Agency";
        private const string CacheKey = "DeliveryInfo_CL";
        private const string AgencyCacheKey = "DeliveryInfo_Agency_CL";
        private const int CL_SHIPPINGINFO_CACHE_MINUTES = 60;
        private const string FreightCode_CityBox = "CHB";

        public override string GetFreightVariant(ShippingInfo shippingInfo)
        {
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.Pickup)
                {
                    return !string.IsNullOrEmpty(shippingInfo.WarehouseCode) ? shippingInfo.WarehouseCode : "47";
                }
            }
            return null;            
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

                formattedAddress = includeName ? string.Format("{0}<br>{1}<br>{2}, {3}, {4}<br>{5}<br>{6}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0}<br>{1}, {2}, {3}<br>{4}<br>{5}",
                    address.Address.Line1, address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                formattedAddress = string.Format("{0}<br/>{1} {2} {3},<br/>{4}, {5} {6} {7}", 
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.Line3 ?? string.Empty, address.Address.Line4 ?? string.Empty,
                                                 address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode);
                do { formattedAddress = formattedAddress.Replace("  ", " "); } while (formattedAddress.IndexOf("  ") > -1);
                do { formattedAddress = formattedAddress.Replace("<br/> ,<br/>", "<br/>"); } while (formattedAddress.IndexOf("<br/> ,<br/>") > -1);
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}<br>{2}, {3}, {4}, {5}", description,
                    address.Address.Line1, address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                // Check the courier type
                var courierType = (address != null && !string.IsNullOrEmpty(address.Alias))
                                      ? address.Alias
                                      : CityBoxType;
                List<DeliveryOption> courierOptions;
                switch (courierType)
                {
                    case AgencyType:
                        courierOptions = GetDeliveryPickupAlternativesFromCache();
                        break;
                    default:
                        courierOptions = GetDeliveryOptionsFromCache("CL", System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
                        break;
                }
                return courierOptions;
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null == deliveryOptions)
            {
                deliveryOptions = GetDeliveryOptionsFromService(Country, locale, address);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }

            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
            CityBoxAlternativesResponse_V01 response = null;

            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(new GetCityBoxRequest_V01() { Country = "CL", FreightCode = FreightCode_CityBox, Warehouse = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse.ToString(),Locale = "es-CL"})).GetDeliveryOptionsResult as CityBoxAlternativesResponse_V01;
            DeliveryOption dlv;
            Address_V01 add;
            if (response != null && response.DeliveryAlternatives != null)
            {
                foreach (var location in response.DeliveryAlternatives)
                {
                    add = new Address_V01()
                        {
                            City = location.City,
                            Country = location.Country,
                            CountyDistrict = location.CountyDistrict,
                            Line1 = location.LocationDescription,
                            Line2 = "",
                            Line3 = location.Line1,
                            Line4 = location.Line2,
                            PostalCode = location.PostalCode,
                            StateProvinceTerritory = location.StateProvinceTerritory
                        };
                    int id = location.ID;
                    dlv =
                        new DeliveryOption(new ShippingAddress_V02(){ID = id, Recipient = location.LocationDescription, FirstName = string.Empty,
                                                                   LastName = string.Empty, MiddleName = string.Empty, Address = add, Phone = "", AltPhone = "", IsPrimary = false,
                                                                   Alias = location.LocationDescription, Created = DateTime.Now});
                    dlv.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                    dlv.FreightCode = FreightCode_CityBox;
                    deliveryOptions.Add(dlv);
                }
            }
            return deliveryOptions;
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(CacheKey,
                  options,
                  null,
                  DateTime.Now.AddMinutes(CL_SHIPPINGINFO_CACHE_MINUTES),
                  Cache.NoSlidingExpiration,
                  CacheItemPriority.Normal,
                  null);
            }
        }


        public override ShippingInfo GetShippingInfoFromID(string distributorID,
                                                          string locale,
                                                          DeliveryOptionType type,
                                                          int deliveryOptionID,
                                                          int shippingAddressID)
        {
            DeliveryOption deliveryOption = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode, null);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int PickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = GetDeliveryOptions(type,
                                                        new ShippingAddress_V01
                                                        {
                                                            Address = new Address_V01 { Country = "CL" },
                                                            Alias = vPickupLocation.PickupLocationType
                                                        });
                        
                        if (doList != null)
                        {
                            deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                            if (deliveryOption != null)
                            {
                                //deliveryOption.Id = deliveryOption.ID = deliveryOptionID;
                                var shippingInfo = new ShippingInfo(deliveryOption);
                                shippingInfo.Id = deliveryOptionID;
                                return shippingInfo;
                            }
                        }
                    }
                }
            }
            else
            {
                return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            }

            return null;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            var shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                {
                    string countryCode = locale.Substring(3, 2);
                    var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var courier = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                        if (courier != null)
                        {
                            return courier.PickupLocationID.ToString();
                        }
                    }
                    return string.Empty;
                }
                else
                {
                    return shippingInfo.Instruction;
                }
            }
            return string.Empty;
        }

        public override bool HasAdditionalPickupFromCourier()
        {
            return true;
        }

        public override string GetCourierTypeBySelection(string itemSelected)
        {
            return (itemSelected == "PickupFromCourier1") ? AgencyType : CityBoxType;
        }


        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                         string country,
                                                                                         string locale,
                                                                                         DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country, null);
            return GetpreferencesByCourier(pickupLocations, CityBoxType);
        }

        /// <summary>
        /// Get PickupLocationPreferences list for courier type
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="country">The country code.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="deliveryType">The delivery type</param>
        /// <param name="courierType">The courier type.</param>
        /// <returns></returns>
        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                         string country,
                                                                                         string locale,
                                                                                         DeliveryOptionType deliveryType,
                                                                                         string courierType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country, courierType);
            return GetpreferencesByCourier(pickupLocations, courierType);
        }

        private List<PickupLocationPreference_V01> GetpreferencesByCourier(List<PickupLocationPreference_V02> pickupLocations, string courierType)
        {
            var pickupPreferences = new List<PickupLocationPreference_V01>();
            switch (courierType)
            {
                case AgencyType:
                    if (pickupLocations.Any())
                    {
                        pickupPreferences = (from p in pickupLocations
                                             where p.PickupLocationType == AgencyType
                                             select
                                                 new PickupLocationPreference_V01
                                                     {
                                                         Country = p.Country,
                                                         DistributorID = p.DistributorID,
                                                         ID = p.ID,
                                                         IsPrimary = p.IsPrimary,
                                                         PickupLocationID = p.PickupLocationID,
                                                         PickupLocationNickname = p.PickupLocationNickname,
                                                         Version = p.PickupLocationType
                                                     }).ToList();
                    }
                    break;
                case CityBoxType:
                    if (pickupLocations.Any())
                    {
                        pickupPreferences = (from p in pickupLocations
                                             where p.PickupLocationType == "CityBox"
                                             select
                                                 new PickupLocationPreference_V01
                                                     {
                                                         Country = p.Country,
                                                         DistributorID = p.DistributorID,
                                                         ID = p.ID,
                                                         IsPrimary = p.IsPrimary,
                                                         PickupLocationID = p.PickupLocationID,
                                                         PickupLocationNickname = p.PickupLocationNickname,
                                                         Version = p.PickupLocationType
                                                     }).ToList();
                    }
                    break;
                default:
                    // Return all
                    if (pickupLocations.Any())
                    {
                        pickupPreferences = (from p in pickupLocations
                                             select
                                                 new PickupLocationPreference_V01
                                                 {
                                                     Country = p.Country,
                                                     DistributorID = p.DistributorID,
                                                     ID = p.ID,
                                                     IsPrimary = p.IsPrimary,
                                                     PickupLocationID = p.PickupLocationID,
                                                     PickupLocationNickname = p.PickupLocationNickname,
                                                     Version = p.PickupLocationType
                                                 }).ToList();
                    }
                    break;
            }
            return pickupPreferences;
        }

        private static List<DeliveryOption> GetDeliveryPickupAlternativesFromCache()
        {
            var deliveryOptions = HttpRuntime.Cache[AgencyCacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();

                try
                {
                    var pickupAlternativesResponse =
                        proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03
                        {
                            CountryCode = "CL"
                        })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

                    if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                    {
                        deliveryOptions = (from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                                           select new DeliveryOption(po, true)).ToList();
                    }

                    if (deliveryOptions.Any())
                    {
                        HttpRuntime.Cache.Insert(AgencyCacheKey, deliveryOptions, null,
                                                 DateTime.Now.AddMinutes(CL_SHIPPINGINFO_CACHE_MINUTES),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetDeliveryPickupAlternativesFromCache error: Country: CL, error: {0}",
                                      ex.Message));
                }
            }
            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }


        public override void PickupLocationsPreferencesLoaded(string distributorID, GetPickupLocationsPreferencesResponse prefs)
        {
            if (prefs != null )
            {
                if (prefs as GetPickupLocationsPreferencesResponse_V02 != null)
                {
                    
                    (prefs as GetPickupLocationsPreferencesResponse_V02).PickupLocationPreferences.Clear();
                }
            }
        }
        public override bool IsValidShippingAddress(MyHLShoppingCart sc)
        {
             if ((sc.DeliveryInfo.Option == DeliveryOptionType.Shipping || sc.DeliveryInfo.Option == DeliveryOptionType.Pickup) &&(
                 sc.DeliveryInfo.Address != null && sc.DeliveryInfo.Address.Address != null))
            
            return IsValidShippingAddress(sc.DeliveryInfo.Address.Address);

             return true;
        }
        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ((!string.IsNullOrWhiteSpace(a.PostalCode)) && (GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City))&&(GetCountiesForCity(a.Country,a.StateProvinceTerritory,a.City).Contains(a.CountyDistrict)))
                return true;
            else
                return false;
        }
    }
}
