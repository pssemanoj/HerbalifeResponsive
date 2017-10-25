using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.UI.Helpers;
//using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Globalization;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider;

    public class ShippingProvider_CA : ShippingProviderBase
    {
        private const string FedexLocationCacheKey = "DeliveryInfo_Fedex_CA";
        private const int FedexCacheMinutes = 60 * 2;
        private const string FedExNickName = "FedEx Office";
        private const string CAStatesCacheKey = "States_CA";
        private const int CAStatesCacheMinutes = 60;

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID,
                                                          string locale)
        {
            var currentSession = SessionInfo.GetSessionInfo(distributorID, locale);
            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && shoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO && currentSession.IsHAPMode)
            {
                if (string.IsNullOrEmpty(shoppingCart.OrderNumber))
                    return "Member Activated Online";
                else
                    return "Member amended order online";
            }

            return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale) + getShippingInfoForFedexHal(shoppingCart);
        }

        private string getShippingInfoForFedexHal(MyHLShoppingCart shoppingCart)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (currentShippingInfo != null)
            {
                if (currentShippingInfo.Option == DeliveryOptionType.PickupFromCourier && !string.IsNullOrEmpty(currentShippingInfo.Address.AltPhone))
                {
                    return string.Format("FedEx Phone: {0}", currentShippingInfo.Address.AltPhone);
                }
            }
            return string.Empty;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            var lstStates = base.GetStatesForCountry("CA");
            String state = address.Address.StateProvinceTerritory.ToLower();
            string stateName = string.Empty;
            //string stateName = lstStates.Find(c => c.Substring(0, 2).ToLower().Equals(state)).Substring(6);
            try
            {
                //stateName = (from s in lstStates where s.Contains(state) select s.Substring(6)).First();
                stateName = lstStates.FirstOrDefault(c => c.Substring(0, 2).ToLower().Contains(state));
                stateName = stateName != null ? stateName.Substring(6) : string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
            }

            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br>{1},{2}<br>{3}, {4} {5}<br>{6}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City, stateName, address.Address.PostalCode,
                                           formatPhone(address.Phone))
                           : string.Format("{0},{1}<br>{2}, {3} {4}<br>{5}",
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City, stateName, address.Address.PostalCode,
                                           formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                string message = HttpContext.GetGlobalResourceObject("MYHL_Rules", "ViewMap").ToString();
                string gAddress = string.Format("{0}+{1}+{2}+{3}", address.Address.Line1.Replace(" ", "+"), address.Address.City.Replace(" ", "+"), address.Address.StateProvinceTerritory.Replace(" ", "+"), address.Address.PostalCode.Replace(" ", "+"));
                return string.Format("{0}<br/>{1}, {2} {3}<br/> <a href='http://maps.google.com/?q={4}' target='_blank'>{5}</a>",
                                                 address.Address.Line1, address.Address.City,
                                                 address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode,gAddress,message);
            }

            return string.Format("{0}<br>{1},{2}<br>{3}, {4} {5}", description,
                                 address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                 stateName, address.Address.PostalCode);
        }

        public override List<string> GetStatesForCountry(string Country)
        {
            List<string> items = null;

            var resxReader = GlobalResourceHelper.GetGlobalEnumeratorElements("CanadaStates", Thread.CurrentThread.CurrentCulture);
            if (resxReader != null && resxReader.Count() > 0)
            {
                items = (from s in resxReader
                         select string.Format("{0} - {1}", s.Key, s.Value)).ToList<string>();
            }

            return items;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();
            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && OrderType.ToString().Equals("HSO"))
            {
                final.Add(new DeliveryOption(
                    new ShippingOption_V01(HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCodeForHAP, "FedEx", DateTime.MinValue, DateTime.MaxValue)));
                return final;
            }

            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();
            request.Country = Country;
            request.State = address.Address.StateProvinceTerritory.Trim();
            request.Locale = locale;
            var response =
                proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                final.Add(new DeliveryOption(option));
            }

            return final;
        }

        public override string[] GetFreightCodeAndWarehouseForTaxRate(Address_V01 address)
        {
            if (address != null)
            {
                return new[]
                    {
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                    };
            }
            return null;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var courierOptions = GetDeliveryPickupAlternativesFromCache(address);
                return courierOptions;
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForPickup(string country,
                                                               string locale,
                                                               ShippingAddress_V01 address)
        {
            var cacheKey = string.Format("{0}_FreightCodes", GetFedexCacheKey());
            var deliveryOptions = HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new DeliveryOptionForCountryRequest_V01
                {
                    Country = country,
                    State = "*",
                    Locale = locale
                };
                try
                {
                    var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                    if (response != null && response.DeliveryAlternatives.Count > 0)
                    {
                        deliveryOptions.AddRange(
                            response.DeliveryAlternatives.Where(option => option.FreightCode.StartsWith("H"))
                                    .Select(option => new DeliveryOption(option)));
                    }
                    if (deliveryOptions.Any())
                    {
                        HttpRuntime.Cache.Insert(cacheKey, deliveryOptions, null,
                                                 DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("GetDeliveryOptionsListForPickup error: Country: CA, error: {0}",
                                                     ex.Message));
                }
            }
            return deliveryOptions;
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type,
                                                           int deliveryOptionID, int shippingAddressID)
        {
            ShippingInfo shippingInfo = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode, null);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation =
                        pickupLocationPreference.Find(
                            p => p.PickupLocationID == deliveryOptionID || p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        var deliveryOptions = GetDeliveryOptionForDistributor(distributorID, type);
                        if (deliveryOptions != null)
                        {
                            var deliveryOption = deliveryOptions.Find(d => d.Id == vPickupLocation.PickupLocationID);
                            if (deliveryOption == null)
                            {
                                deliveryOption = GetDeliveryOptionFromId(vPickupLocation.PickupLocationID,
                                                                         DeliveryOptionType.PickupFromCourier);
                            }
                            if (deliveryOption != null)
                            {
                                shippingInfo = new ShippingInfo(deliveryOption) { Id = deliveryOptionID };
                                // Get freight and warehouse if there are not
                                shippingInfo.WarehouseCode = GetWareHouseCode("en-CA",
                                                                              shippingInfo.Address.Address
                                                                                          .StateProvinceTerritory);
                                if (string.IsNullOrEmpty(shippingInfo.FreightCode))
                                {
                                    var freights = GetDeliveryOptionsListForPickup("CA", "en-CA", null);
                                    var defFreight = freights.FirstOrDefault(o => o.IsDefault);
                                    if (defFreight != null)
                                    {
                                        shippingInfo.FreightCode = defFreight.FreightCode;
                                    }
                                }
                                shippingInfo.Address.AltPhone = shippingInfo.Address.Phone;
                                shippingInfo.Address.Phone = string.Empty;
                            }
                            return shippingInfo;
                        }
                    }
                }
            }
            else
            {
                shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID,
                                                          shippingAddressID);
            }
            return shippingInfo;
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                         string country,
                                                                         string locale,
                                                                         DeliveryOptionType deliveryType)
        {
            return GetPickupLocationsPreferences(distributorId, country, locale, deliveryType, null);
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                 string country,
                                                                                 string locale,
                                                                                 DeliveryOptionType deliveryType,
                                                                                 string courierType)
        {
            var pickupPreferences = new List<PickupLocationPreference_V01>();
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country, courierType);
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

                foreach (var location in pickupPreferences)
                {
                    // Verify if the location exists
                    var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                    if (shippingInfo == null)
                    {
                        location.PickupLocationNickname = "NOTAVAILABLE";
                        continue;
                    }
                }
            }
            return pickupPreferences.Where(l => l.PickupLocationNickname != "NOTAVAILABLE").ToList();
        }

        public override bool DisplayHoursOfOperation(DeliveryOptionType option)
        {
            switch (option)
            {
                case DeliveryOptionType.Pickup:
                case DeliveryOptionType.PickupFromCourier:
                    return true;
                default:
                    return false;
            }
        }

        private static List<DeliveryOption> GetDeliveryPickupAlternativesFromCache(ShippingAddress_V01 address)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var cacheKey = GetFedexCacheKey();
            var locations = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<DeliveryOption>>;
            if (locations == null || !locations.ContainsKey(address.Address.PostalCode))
            {
                if (locations == null) locations = new Dictionary<string, List<DeliveryOption>>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();

                try
                {
                    var pickupAlternativesResponse =
                        proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05()
                        {
                            PostalCode = address.Address.PostalCode,
                            CountryCode = "CA",
                            MaximumDistance = 32,
                            Unit = "KM"
                        })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                    if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                    {
                        // Sorting and parcing
                        var withDistanceList = (from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                                                where !string.IsNullOrEmpty(po.DistanceUnit)
                                                orderby po.Distance ascending
                                                select po).ToList();
                        int index = 1;
                        withDistanceList.ForEach(po =>
                        {
                            po.DisplayOrder = index;
                            index++;
                        });
                        var woDistanceList = (from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                                              where string.IsNullOrEmpty(po.DistanceUnit)
                                              orderby po.CourierName ascending
                                              select po).ToList();
                        woDistanceList.ForEach(po =>
                        {
                            po.DisplayOrder = index;
                            index++;
                        });

                        deliveryOptions.AddRange(from po in withDistanceList select new DeliveryOption(po, true));
                        deliveryOptions.AddRange(from po in woDistanceList select new DeliveryOption(po, true));
                        deliveryOptions.ForEach(po =>
                        {
                            po.DisplayName = string.Format("{0} #{1}", FedExNickName, po.CourierStoreId);
                            po.GeographicPoint = po.GeographicPoint.Replace("/", string.Empty);
                            po.Information = FormatAdditionalInfo(po.Information);
                        });
                    }

                    if (deliveryOptions.Any())
                    {
                        locations.Add(address.Address.PostalCode, deliveryOptions);
                        HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                 DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetDeliveryPickupAlternativesFromCache error: Country: CA, error: {0}",
                                      ex.Message));
                }
            }

            if (locations.ContainsKey(address.Address.PostalCode))
            {
                var locList = locations.FirstOrDefault(l => l.Key == address.Address.PostalCode);
                deliveryOptions = locList.Value;
            }
            return deliveryOptions;
        }

        public List<DeliveryOption> GetDeliveryOptionForDistributor(string distributorId, DeliveryOptionType type)
        {
            var deliveryOptions = new List<DeliveryOption>();
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var cacheKey = GetFedexCacheKey();
                var locations = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<DeliveryOption>>;
                if (locations == null || !locations.ContainsKey(distributorId))
                {
                    if (locations == null) locations = new Dictionary<string, List<DeliveryOption>>();
                    var proxy = ServiceClientProvider.GetShippingServiceProxy();

                    try
                    {
                        var pickupAlternativesResponse =
                            proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05()
                            {
                                CountryCode = "CA",
                                DistributorId = distributorId
                            })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                        if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                        {
                            deliveryOptions.AddRange(from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
                            deliveryOptions.ForEach(po =>
                            {
                                po.DisplayName = string.Format("{0} #{1}", FedExNickName, po.CourierStoreId);
                                po.GeographicPoint = po.GeographicPoint.Replace("/", string.Empty);
                                po.Information = FormatAdditionalInfo(po.Information);
                                po.Description = po.Name;
                            });
                        }

                        if (deliveryOptions.Any())
                        {
                            locations.Add(distributorId, deliveryOptions);
                            HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                        DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetDeliveryOptionForDistributor error: Country: CA, error: {0}",
                                            ex.Message));
                    }
                }
                if (locations.ContainsKey(distributorId))
                {
                    var locList = locations.FirstOrDefault(l => l.Key == distributorId);
                    deliveryOptions = locList.Value;
                }
            }
            return deliveryOptions;
        }

        public DeliveryOption GetDeliveryOptionFromId(int deliveryOptionId, DeliveryOptionType type)
        {
            DeliveryOption deliveryOption = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var cacheKey = GetFedexCacheKey();
                var locations = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<DeliveryOption>>;
                if (locations != null)
                {
                    foreach (var location in locations)
                    {
                        deliveryOption = location.Value.Find(o => o.Id == deliveryOptionId);
                        if (deliveryOption != null)
                        {
                            deliveryOption.Description = deliveryOption.Name;
                            break;
                        }
                    }
                }
            }
            return deliveryOption;
        }

        private string GetWareHouseCode(string locale, string stateCode)
        {
            var lstOptions = GetDeliveryOptions(locale);
            if (lstOptions != null && lstOptions.Count > 0)
            {
                string stateName = GetStateNameFromStateCode(stateCode);
                stateName = stateName.ToUpper();
                var option =
                    lstOptions.Find(delegate(DeliveryOption p) { return p.State.Trim().ToUpper() == stateName; });
                if (option != null)
                {
                    return option.WarehouseCode;
                }
            }
            return string.Empty;
        }

        public override string GetStateNameFromStateCode(string stateCode)
        {
            string stateName = string.Empty;
            var states = GetStatesForCountryFromCache("CA");
            if (states != null && states.Count() > 0)
            {
                var state = states.Where(s => s.Code.Equals(stateCode));
                var stateInfo = state.FirstOrDefault();
                stateName = stateInfo != null ? stateInfo.Name : string.Empty;
            }
            return stateName;
        }

        private List<USStates> GetStatesForCountryFromCache(string Country)
        {
            var states = HttpRuntime.Cache[CAStatesCacheKey] as List<USStates>;
            if (states == null)
            {
                states = new List<USStates>();
                // Get info from database
                var dbStates = base.GetStatesForCountry(Country);
                states.AddRange(from s in dbStates
                                select new USStates
                                {
                                    Code = s.Substring(0, s.IndexOf('-')).Trim(),
                                    Name = s.Substring(s.LastIndexOf('-') + 1).Trim(),
                                    DisplayName = s.Substring(s.LastIndexOf('-') + 1).Trim(),
                                    IsTerritory = false
                                });
                if (states.Count > 0)
                {
                    HttpRuntime.Cache.Insert(CAStatesCacheKey, states, null,
                                             DateTime.Now.AddMinutes(CAStatesCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
            }
            return states;
        }

        private static string GetFedexCacheKey()
        {
            return string.Format("{0}_{1}", FedexLocationCacheKey, CultureInfo.CurrentCulture.Name);
        }

        private static string FormatAdditionalInfo(string additionalInfo)
        {
            if (string.IsNullOrEmpty(additionalInfo))
                return additionalInfo;

            var addInfo = additionalInfo.Replace("|", "<br/>");
            addInfo = addInfo.Replace("MON",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl, "MON",
                                          CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("THU",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "THU", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("WED",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "WED", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("TUE",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "TUE", CultureInfo.CurrentCulture));
            ;
            addInfo = addInfo.Replace("FRI",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "FRI", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("SAT",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "SAT", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("SUN",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "SUN", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("CLOSED_ALL_DAY",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "CLOSED_ALL_DAY", CultureInfo.CurrentCulture));
            addInfo = addInfo.Replace("OPEN_ALL_DAY",
                                      (string)
                                      HttpContext.GetLocalResourceObject(
                                          HLConfigManager.Configurations.AddressingConfiguration.PickupControl,
                                          "OPEN_ALL_DAY", CultureInfo.CurrentCulture));
            return addInfo;
        }
    }
}