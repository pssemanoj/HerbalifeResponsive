using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Security;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_TH : ShippingProviderBase
    {
        #region Constants
        const string SevenElevenLocationCacheKey = "DeliveryInfo_7Eleven";
        private const int SevenElevenCacheMinutes = 60 * 2;
        private const string SevenElevenNickName = "7Eleven";
        #endregion

        #region ShippingProviderBase Methods
        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type,
                                                     string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br/>{1},{2}<br/>{3}<br/>{4}, {5}, {6}<br/>{7}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.Line3 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br/>{2}<br/>{3}, {4}, {5}<br/>{6}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.Line3 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else if(type == DeliveryOptionType.PickupFromCourier)
            {
                string message = HttpContext.GetGlobalResourceObject("MYHL_Rules", "ViewMap").ToString();
                string gAddress = string.Format("{0}+{1}+{2}+{3}+{4}", address.Address.Line1.Replace(" ", "+"), address.Address.Line2.Replace(" ","+"), address.Address.City.Replace(" ", "+"), address.Address.StateProvinceTerritory.Replace(" ", "+"), address.Address.PostalCode.Replace(" ", "+"));
                formattedAddress = string.Format("{0}<br/>{1}, {2}<br/>{3} {4}<br/>{5}",
                                                 address.Address.Line1,
                                                 address.Address.Line2 ?? string.Empty, address.Address.City,
                                                 address.Address.StateProvinceTerritory, address.Address.PostalCode,
                                                 PlatformResources.GetGlobalResourceString("GlobalResources", "CountryName"));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", 
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,address.Address.CountyDistrict,
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

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] { "THF", "T5" };

            if (null != address && null != address.Address)
            {
                string state = address.Address.StateProvinceTerritory ?? string.Empty;

                if (state.Equals("กรุงเทพมหานคร") || state.Equals("นนทบุรี") || 
                    state.Equals("ปทุมธานี") || state.Equals("สมุทรปราการ"))
                {
                    freightCodeAndWarehouse[0] = "BKF";
                }
                else if(state == "*")
                {
                    var freightCodeAndWarehouseFromService = GetFreightCodeAndWarehouseFromService(state);
                    if (freightCodeAndWarehouseFromService != null)
                    {
                        freightCodeAndWarehouse[0] = freightCodeAndWarehouseFromService[0] ?? freightCodeAndWarehouse[0];
                        freightCodeAndWarehouse[1] = freightCodeAndWarehouseFromService[1] ?? freightCodeAndWarehouse[1];
            }
                }
            }
            else
            {
                freightCodeAndWarehouse[0] = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
            }
            return freightCodeAndWarehouse;
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 shipping)
        {
            if (shipping.Address != null)
            {
                shipping.Address.Line1 = GetSubstring(shipping.Address.Line1);
                shipping.Address.Line2 = GetSubstring(shipping.Address.Line2);
                shipping.Address.Line3 = GetSubstring(shipping.Address.Line3);
                shipping.Address.Line4 = GetSubstring(shipping.Address.Line4);
                shipping.Address.City = GetSubstring(shipping.Address.City);
                shipping.Address.CountyDistrict = GetSubstring(shipping.Address.CountyDistrict);
            }

        }
        public override bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            if (address != null)
            {
                address.Line1 = GetSubstring(address.Line1);
                address.Line2 = GetSubstring(address.Line2);
                address.Line3 = GetSubstring(address.Line3);
                address.Line4 = GetSubstring(address.Line4);
                address.City = GetSubstring(address.City);
                address.CountyDistrict = GetSubstring(address.CountyDistrict);
            }
            return true;
        }

        public override int SavePickupLocation(InsertCourierLookupRequest_V01 request)
        {
            var retValue = 0;

            if(request != null || request.CourierStoreNumber > 0 || !string.IsNullOrEmpty(request.CountryCode))
            {
                try
                {
                    // Call the Shipping service to Insert a new Pickup location
                    var service = ServiceClientProvider.GetShippingServiceProxy();
                    var response = service.InsertCourierLookup(new InsertCourierLookupRequest1(request)).InsertCourierLookupResult as InsertCourierLookupResponse_V01;

                    if(response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.ID;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("SavePickupLocation error: Country {0}, CourierStoreNumber {1}, error: {2}", request.CountryCode, request.CourierStoreNumber, ex.ToString()));
                }
            }

            return retValue;
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID,
                                                           string locale,
                                                           DeliveryOptionType type,
                                                           int deliveryOptionID,
                                                           int shippingAddressID)
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
                                deliveryOptions = UpdateDeliveryOptionsForDistributorFromService(distributorID, type);
                                deliveryOption = deliveryOptions.Find(d => d.Id == vPickupLocation.PickupLocationID);
                            }
                            if (deliveryOption != null)
                            {
                                shippingInfo = new ShippingInfo(deliveryOption) { Id = deliveryOptionID };

                                // Get freight and warehouse if there are not
                                if (string.IsNullOrEmpty(shippingInfo.FreightCode) || string.IsNullOrEmpty(shippingInfo.WarehouseCode))
                                {
                                    var freightCodeAndWarehouse = GetFreightCodeAndWarehouseFromService("*");
                                    if (string.IsNullOrEmpty(shippingInfo.FreightCode)) shippingInfo.FreightCode = freightCodeAndWarehouse[0];
                                    if (string.IsNullOrEmpty(shippingInfo.WarehouseCode)) shippingInfo.WarehouseCode = freightCodeAndWarehouse[1];
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

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryPickupAlternativesFromCache(address);
            }

            return base.GetDeliveryOptions(type, address);
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
            if (pickupLocations != null && pickupLocations.Any())
            {
                pickupPreferences = (from p in pickupLocations orderby p.IsPrimary descending
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
                case DeliveryOptionType.PickupFromCourier:
            return true;
                default:
                    return base.DisplayHoursOfOperation(option);
            }
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                     ? string.Empty
                                     : shoppingCart.DeliveryInfo.Instruction;
         
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                {
                    string countryCode = locale.Substring(3, 2);
                    List<PickupLocationPreference_V01> pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var pickupLocation = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                        // PickupFromCourier return instuction=RecipientNam,Phone
                        if (shoppingCart.DeliveryInfo != null && pickupLocation != null && pickupLocation.PickupLocationID > 0)
                        {
                            return instruction = string.Format("{0},{1} {2}", shoppingCart.DeliveryInfo.Address.Recipient  , shoppingCart.DeliveryInfo.Address.Phone,
                                         pickupLocation.PickupLocationID.ToString());
                        }
                    }

                    return string.Empty;
                }
            }
            return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
        }

        public override bool UpdatePrimaryPickupLocationPreference(int pickupLocationId)
        {
            if(pickupLocationId == 0)
            {
                return false;
            }

            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            try
            {
                ;
                var response = proxy.UpdatePickupLocationPreferences(new UpdatePickupLocationPreferencesRequest1(
                    new UpdatePickupLocationPreferencesRequest_V01() { ID = pickupLocationId })).UpdatePickupLocationPreferencesResult as UpdatePickupLocationPreferencesResponse_V01;

                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return pickupLocationId == response.ID;
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(
                        string.Format("UpdatePrimaryPickupLocationPreference error: Country: TH, error: {0}",
                                        ex.Message));
                return false;
            }

            return true;
        }
        
        #endregion

        #region Private Methods
        private string GetSubstring(string info)
        {
            if (string.IsNullOrEmpty(info))
                return info;
            
            Encoding enc_utf8 = new UTF8Encoding(false, true);
            if (enc_utf8.GetByteCount(info) <= 60)
                return info;

            var length = info.Length * 60 / enc_utf8.GetByteCount(info);
            if (enc_utf8.GetByteCount(info.Substring(0, length)) > 60)
                return info.Substring(0, length - 1);
            else
                return info.Substring(0, length);

        }

        private static string Get7ElevenCacheKey()
        {
            return string.Format("{0}_{1}", SevenElevenLocationCacheKey, CultureInfo.CurrentCulture.Name);
        }

        private List<DeliveryOption> UpdateDeliveryOptionsForDistributorFromService(string distributorId, DeliveryOptionType type)
        {
            var deliveryOptions = new List<DeliveryOption>();
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var cacheKey = Get7ElevenCacheKey();
                var locations = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<DeliveryOption>>;

                if (locations.ContainsKey(distributorId))
                    locations.Remove(distributorId);

                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                try
                {
                    var pickupAlternativesResponse =
                        proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05()
                        {
                            CountryCode = "TH",
                            DistributorId = distributorId
                        })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                    if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                    {
                        deliveryOptions.AddRange(from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
                        deliveryOptions.ForEach(po =>
                        {
                            po.DisplayName = string.Format("{0} #{1}", SevenElevenNickName, po.CourierStoreId);
                            po.Description = po.Name;
                        });
                    }

                    if (deliveryOptions.Any())
                    {
                        locations.Add(distributorId, deliveryOptions);
                        HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                    DateTime.Now.AddMinutes(SevenElevenCacheMinutes),
                                                    Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("UpdateDeliveryOptionsForDistributorFromService error: Country: TH, error: {0}",
                                        ex.Message));
                }

                if (locations.ContainsKey(distributorId))
                {
                    var locList = locations.FirstOrDefault(l => l.Key == distributorId);
                    deliveryOptions = locList.Value;
        }
    }
            return deliveryOptions;
        }

        private static List<DeliveryOption> GetDeliveryPickupAlternativesFromCache(ShippingAddress_V01 address)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var cacheKey = Get7ElevenCacheKey();
            var locations = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<DeliveryOption>>;

            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (member == null || member.Value == null)
                return deliveryOptions;

            var distributorId = member.Value.Id;

            if (locations == null || !locations.ContainsKey(distributorId))
            {
                if (locations == null) locations = new Dictionary<string, List<DeliveryOption>>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();

                try
                {
                    var pickupAlternativesResponse =
                        proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05()
                        {
                            CountryCode = "TH",
                            DistributorId = distributorId
                        })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                    if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                    {
                        deliveryOptions.AddRange(from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
                        deliveryOptions.ForEach(po =>
                        {
                            po.DisplayName = string.Format("{0} #{1}", SevenElevenNickName, po.CourierStoreId);
                            po.Description = po.Name;
                        });
                    }

                    if (deliveryOptions.Any())
                    {
                        locations.Add(distributorId, deliveryOptions);
                        HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                    DateTime.Now.AddMinutes(SevenElevenCacheMinutes),
                                                    Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetDeliveryPickupAlternativesFromCache error: Country: TH, error: {0}",
                                        ex.Message));
                }

            }

            if (locations.ContainsKey(distributorId))
            {
                var locList = locations.FirstOrDefault(l => l.Key == distributorId);
                deliveryOptions = locList.Value;
            }
            return deliveryOptions;
        }

        private string[] GetFreightCodeAndWarehouseFromService(string state)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "TH",
                Locale = "th-TH",
                State = state
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }
        #endregion

        public List<DeliveryOption> GetDeliveryOptionForDistributor(string distributorId, DeliveryOptionType type)
        {
            var deliveryOptions = new List<DeliveryOption>();
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var cacheKey = Get7ElevenCacheKey();
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
                                CountryCode = "TH",
                                DistributorId = distributorId
                            })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                        if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                        {
                            deliveryOptions.AddRange(from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
                            deliveryOptions.ForEach(po =>
                            {
                                po.DisplayName = string.Format("{0} #{1}", SevenElevenNickName, po.CourierStoreId);
                                po.Description = po.Name;
                            });
                        }

                        if (deliveryOptions.Any())
                        {
                            locations.Add(distributorId, deliveryOptions);
                            HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                        DateTime.Now.AddMinutes(SevenElevenCacheMinutes),
                                                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        }
        }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetDeliveryOptionForDistributor error: Country: TH, error: {0}",
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

    }
}
