using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.ServiceModel;
using System.Threading;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web.Caching;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.AddressValidationSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    ///     Shipping provider for PR
    /// </summary>
    public class ShippingProvider_PR : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryMethod_PR";
        private const int PR_SHIPPINGINFO_CACHE_MINUTES = 60;
        private const string FedexLocationCacheKey = "DeliveryInfo_Fedex_PR";
        private const int FedexCacheMinutes = 60;
        private const string FedExNickName = "FedEx Office";

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID,
                                                           string locale)
        {
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

        public override List<string> GetStatesForCountry(string Country)
        {
            List<string> items = null;

            string canadaResourceFile = string.Format("~\\App_GlobalResources\\PuertoRicoStates.{0}.resx", Thread.CurrentThread.CurrentCulture.Name);
            var resxPath = HttpContext.Current.Server.MapPath(canadaResourceFile);

            if (!File.Exists(resxPath))
            {
                resxPath = HttpContext.Current.Server.MapPath("~\\App_GlobalResources\\PuertoRicoStates.resx");
            }

            if (File.Exists(resxPath))
            {
                // Get info from resx
                var resxReader = new ResXResourceReader(resxPath);
                var resxDictionary = resxReader.Cast<DictionaryEntry>().ToDictionary
                    (r => r.Key.ToString(), r => r.Value.ToString());
                resxReader.Close();

                items = (from s in resxDictionary
                         select string.Format("{0} - {1}", s.Key, s.Value)).ToList<string>();
            }

            return items;
        }

        public override bool ValidateAddress(ShippingAddress_V02 shippingAddress,
                                             out String errorCode,
                                             out ServiceProvider.AddressValidationSvc.Address avsAddress)
        {            
            errorCode = string.Empty;

            var request = new ValidateAddressRequest();
            var address = new ServiceProvider.AddressValidationSvc.Address();
            address.City = shippingAddress.Address.City;
            address.CountryCode = shippingAddress.Address.Country;
            address.Line1 = shippingAddress.Address.Line1;
            address.PostalCode = shippingAddress.Address.PostalCode;
            address.StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory;
            request.Address = address;

            // Avoid validation where ValidatePostalCode is disabled
            if (!HLConfigManager.Configurations.AddressingConfiguration.ValidatePostalCode)
            {
                avsAddress = request.Address;
                return true;
            }

            var proxy = ServiceClientProvider.GetAddressValidationServiceProxy();
            try
            {
                var response = proxy.ValidateAddress(new ValidateAddressRequest1(request)).ValidateAddressResponse;
                if (response != null)
                {
                    if (response.ValidationResult.Value.ToUpper() == "FOUND" ||
                        response.ValidationResult.ErrorCode.ToUpper() == "E421"
                        || response.ValidationResult.ErrorCode.ToUpper() == "E422" ||
                        response.ValidationResult.ErrorCode.ToUpper() == "E427"
                        || response.ValidationResult.ErrorCode.ToUpper() == "E412" ||
                        response.ValidationResult.ErrorCode.ToUpper() == "E413"
                        || response.ValidationResult.ErrorCode.ToUpper() == "E423" ||
                        response.ValidationResult.ErrorCode.ToUpper() == "E425"
                        || response.ValidationResult.ErrorCode.ToUpper() == "E420" ||
                        response.ValidationResult.ErrorCode.ToUpper() == "E430"
                        || response.ValidationResult.ErrorCode.ToUpper() == "E600")
                    {
                        // address is valid
                        errorCode = response.ValidationResult.ErrorCode;
                        avsAddress = response.Address;
                        return true;
                    }
                }
                // address is invalid
                errorCode = response.ValidationResult.ErrorCode;
                avsAddress = response.Address;
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("AVS Address Validation Failed:{0}", ex.Message));
                errorCode = ex.Message;
                avsAddress = null;
                return false;
            }
            
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            if (null == address || null == address.Address)
                return string.Empty;

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br/>{1}<br/>{2}, {3} {4}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, 
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode)
                           : string.Format("{0}<br/>{1}, {2} {3}",
                                           address.Address.Line1, 
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode);
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
            else
            {
                return string.Format("{0}<br/>{1}<br/>{2}, {3} {4}", description,
                                     address.Address.Line1, address.Address.City,
                                     address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
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

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                              string locale,
                                                                              ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            List<DeliveryOption> result = null;
            if (null != deliveryOptions && deliveryOptions.Count > 0)
            {
                string state = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "Puerto Rico";
                result = deliveryOptions.Where(d => d.State == state).ToList();
                if (result.Count == 0)
                {
                    result = GetDeliveryOptionsFromService(Country, locale, address);
                    SaveDeliveryOptionsToCache(result);
                }
            }
            else
            {
                result = GetDeliveryOptionsFromService(Country, locale, address);
                SaveDeliveryOptionsToCache(result);
            }

            return result.OrderBy(d => d.displayIndex.ToString(CultureInfo.InvariantCulture) + "_" + d.DisplayName).ToList();



        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        {
            var result = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01 { Country = Country };
            request.State = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "Puerto Rico";
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                var currentOption = new DeliveryOption(option)
                {
                    Name = option.Description,
                    WarehouseCode = option.WarehouseCode,
                    State = request.State
                };
                result.Add(currentOption);
            }
            return result.OrderBy(d => d.displayIndex.ToString(CultureInfo.InvariantCulture) + "_" + d.DisplayName).ToList();
        }


        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null != deliveryOptions)
            {
                deliveryOptions.AddRange(options);
            }
            else
            {

                HttpRuntime.Cache.Insert(CacheKey,
                options,
                null,
                DateTime.Now.AddMinutes(PR_SHIPPINGINFO_CACHE_MINUTES),
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                null);

            }
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
                                shippingInfo = new ShippingInfo(deliveryOption)
                                    {
                                        Id = deliveryOptionID,
                                        WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                                    };
                                if (string.IsNullOrEmpty(shippingInfo.FreightCode))
                                {
                                    var freights = GetDeliveryOptionsListForPickup("PR", "es-PR", null);
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

        public List<DeliveryOption> GetDeliveryOptionForDistributor(string distributorId, DeliveryOptionType type)
        {
            var deliveryOptions = new List<DeliveryOption>();
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var locations = HttpRuntime.Cache[FedexLocationCacheKey] as Dictionary<string, List<DeliveryOption>>;
                if (locations == null || !locations.ContainsKey(distributorId))
                {
                    if (locations == null) locations = new Dictionary<string, List<DeliveryOption>>();
                    var proxy = ServiceClientProvider.GetShippingServiceProxy();

                    try
                    {
                        var pickupAlternativesResponse =
                            proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05()
                            {
                                CountryCode = "PR",
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
                            HttpRuntime.Cache.Insert(FedexLocationCacheKey, locations, null,
                                                        DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetDeliveryOptionForDistributor error: Country: PR, error: {0}",
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
                var locations = HttpRuntime.Cache[FedexLocationCacheKey] as Dictionary<string, List<DeliveryOption>>;
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

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var courierOptions = GetDeliveryPickupAlternativesFromCache(address);
                return courierOptions;
            }
            return base.GetDeliveryOptions(type, address);
        }

        private static List<DeliveryOption> GetDeliveryPickupAlternativesFromCache(ShippingAddress_V01 address)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var locations = HttpRuntime.Cache[FedexLocationCacheKey] as Dictionary<string, List<DeliveryOption>>;
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
                            CountryCode = "PR",
                            MaximumDistance = 20,
                            Unit = "MI"
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
                        HttpRuntime.Cache.Insert(FedexLocationCacheKey, locations, null,
                                                 DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetDeliveryPickupAlternativesFromCache error: Country: PR, error: {0}",
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

        public override List<DeliveryOption> GetDeliveryOptionsListForPickup(string country,
                                                                   string locale,
                                                                   ShippingAddress_V01 address)
        {
            var cacheKey = string.Format("{0}_FreightCodes_{1}", FedexLocationCacheKey, locale);
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
                    LoggerHelper.Error(string.Format("GetDeliveryOptionsListForPickup error: Country: PR, error: {0}",
                                                     ex.Message));
                }
            }
            return deliveryOptions;
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