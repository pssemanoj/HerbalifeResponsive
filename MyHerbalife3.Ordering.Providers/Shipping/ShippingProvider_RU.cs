using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_RU : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_RU";
        private const string PUCourierCacheKey = "DeliveryCourierInfo_RU";

        private const int RU_SHIPPINGINFO_CACHE_MINUTES = 60;

        private void retrieveFreightCode(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc.ShippingInfo_V01 address)
        {
            // to correct prod issue where wrong freight code is set
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null &&
                shoppingCart.DeliveryInfo.Address.Address != null &&
                (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup) &&
                shoppingCart.OrderCategory == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType.RSO && shoppingCart.DeliveryInfo.FreightCode == "NOF")
            {
                // Let the ETO and APF preserve the freight
                SessionInfo sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                var isAPF = APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale);

                if (!sessionInfo.IsEventTicketMode && !isAPF)
                {
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        List<DeliveryOption> deliveryOptions = GetDeliveryOptions(DeliveryOptionType.Shipping,
                                                                                  shoppingCart.DeliveryInfo.Address);
                        if (deliveryOptions != null)
                        {
                            DeliveryOption op =
                                deliveryOptions.Find(
                                    d =>
                                    d.WarehouseCode == shoppingCart.DeliveryInfo.WarehouseCode &&
                                    d.State.Equals(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory));
                            if (op != null)
                            {
                                shoppingCart.DeliveryInfo.FreightCode = op.FreightCode;
                            }
                        }
                    }
                    else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        var pickupLocation = GetShippingInfoFromID(shoppingCart.DistributorID, shoppingCart.Locale,
                                                                   DeliveryOptionType.Pickup,
                                                                   shoppingCart.DeliveryInfo.Id, shoppingCart.DeliveryInfo.Address.ID);
                        if (pickupLocation != null)
                        {
                            shoppingCart.DeliveryInfo.FreightCode = pickupLocation.FreightCode;
                            shoppingCart.DeliveryInfo.WarehouseCode = pickupLocation.WarehouseCode;
                        }
                    }
                }
            } 
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            retrieveFreightCode(shoppingCart, null);
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            retrieveFreightCode(shoppingCart, null);
            if (shoppingCart.DeliveryInfo != null)
                shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
            return true;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address, string option)
        {
            if (type == DeliveryOptionType.Pickup)
            {
                var pickups = base.GetDeliveryOptions(type, address);
                if (option == "Pickup1")
                {
                    pickups = pickups.Where(p => p.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse).ToList();
                    pickups = GetPickupLocationByDistributor(pickups);
                }
                else
                {
                    pickups = pickups.Where(p => p.WarehouseCode != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse).ToList();
                }
                return pickups;
            }
            else
            {
                return GetDeliveryOptions(type, address);
            }
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.Pickup)
            {
                return base.GetDeliveryOptions(type, address);
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache("RU", Thread.CurrentThread.CurrentCulture.Name, address, type);
            }
            else
            {
                return GetDeliveryOptionsListForShipping("RU", "ru-RU", address);
            }
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        {
            return GetDeliveryOptionsFromCache(Country, locale, address, DeliveryOptionType.Shipping);
        }

        public override bool ShouldRecalculate(string oldFreightCode, string newFreightCode, Address_V01 oldaddress, Address_V01 newaddress)
        {
            if (oldFreightCode != newFreightCode)
                return true;
            if (oldaddress == null || newaddress == null || !oldaddress.StateProvinceTerritory.Equals(newaddress.StateProvinceTerritory))
                return true;
            return false;
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

                formattedAddress = includeName ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                var deliveryoptions = GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, address)
                    .FirstOrDefault(x => x.Address.City == address.Address.City 
                        && x.Address.Line1 == address.Address.Line1 && x.Address.Line2==address.Address.Line2
                        && x.Address.StateProvinceTerritory == address.Address.StateProvinceTerritory);
                
                formattedAddress = string.Format("{0}<br>{1} {2}<br>{3}, {4} {5} {6} {7}", address.Address.CountyDistrict,
                                                 address.Address.Line2, address.Address.Line1 ?? string.Empty,
                                                 deliveryoptions!=null?deliveryoptions.Description:string.Empty, 
                                                 address.Address.PostalCode,
                                                 address.Address.City, address.Address.StateProvinceTerritory, null!=deliveryoptions?deliveryoptions.Information:string.Empty);
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
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

        public override string GetShippingInstructionsForShippingInfo(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCart_V02 cart, MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping && shoppingCart.DeliveryInfo.Address != null)
            {
                List<DeliveryOption> deliveryOptions = GetDeliveryOptions(DeliveryOptionType.Shipping, shoppingCart.DeliveryInfo.Address);
                if (deliveryOptions != null)
                {
                    string warehouseCode = string.IsNullOrEmpty(cart.OrderSubType) ? shoppingCart.DeliveryInfo.WarehouseCode : cart.OrderSubType;
                    
                        DeliveryOption deliveryOption = deliveryOptions.Find(d => d.FreightCode == shoppingCart.DeliveryInfo.FreightCode && d.WarehouseCode == warehouseCode);
                        if (deliveryOption != null)
                        {
                            shoppingCart.DeliveryInfo.WarehouseCode = warehouseCode;
                            shoppingCart.DeliveryInfo.Name = deliveryOption.Name;
                            return shoppingCart.DeliveryInfo.Instruction = GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
                        }
                        else
                        {
                            deliveryOption = deliveryOptions.FirstOrDefault();
                            if (deliveryOption != null)
                            {
                                shoppingCart.DeliveryInfo.WarehouseCode = deliveryOption.WarehouseCode;
                                shoppingCart.DeliveryInfo.Name = deliveryOption.Name;
                                shoppingCart.DeliveryInfo.FreightCode = deliveryOption.FreightCode;
                                return shoppingCart.DeliveryInfo.Instruction = GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
                            }
                        }
                }
            }
            return String.Empty;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    string option = shoppingCart.DeliveryInfo.Name;
                    if (!string.IsNullOrEmpty(option))
                    {
                        if (option.IndexOf('-') > 1)
                        {
                            option = option.Split('-')[0].Trim();
                        }

                        switch (option)
                        {
                            case "Почта":
                                {
                                    return "Standard Спасибо за Ваш заказ";
                                }
                            case "Консолидированная":
                                {
                                    return string.Format("Consolidated {0} Спасибо за Ваш заказ", shoppingCart.DeliveryInfo.Address.Address.City);
                                }
                            case "Экспресс":
                                {
                                    return "Express Спасибо за Ваш заказ";
                                }
                            case "Экспресс доставка":
                                {
                                    return option + " Спасибо за Ваш заказ";
                                }
                            default:
                                {
                                    return string.Empty;
                                }
                        }
                    }
                }
                else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup && !string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse) &&
                    shoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
                {
                    if (shoppingCart.DeliveryInfo.Description == "Минск")
                        return "Получение в Минске";
                    if (shoppingCart.DeliveryInfo.Description == "Новосибирск")
                        return "Получение в Новосибирске";
                    return "Спасибо за Ваш заказ";
                }
                else
                {
                    return "Спасибо за Ваш заказ";
                }
            }
            return String.Empty;
        }

        

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[PUCourierCacheKey] as List<DeliveryOption>;
                List<DeliveryOption> result = null;
                if (null != deliveryOptions && deliveryOptions.Count > 0)
                {
                    result = deliveryOptions;
                }
                else
                {
                    result = GetDeliveryOptionsFromService(Country, locale, address, type);
                    SaveDeliveryOptionsToCache(result,type);
                }

                return result;

            }
            else
            {

                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
                List<DeliveryOption> result = null;
                if (null != deliveryOptions && deliveryOptions.Count > 0)
                {
                    string state = (address != null && address.Address != null &&
                                    !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                                       ? address.Address.StateProvinceTerritory
                                       : "Москва";
                    result = deliveryOptions.Where(d => d.State == state).ToList();
                    if (null == result || result.Count == 0)
                    {
                        result = GetDeliveryOptionsFromService(Country, locale, address, type);
                        SaveDeliveryOptionsToCache(result,type);
                    }
                }
                else
                {
                    result = GetDeliveryOptionsFromService(Country, locale, address, type);
                    SaveDeliveryOptionsToCache(result,type);
                }

                return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
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
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int PickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = GetDeliveryOptions(type,
                                                        new ShippingAddress_V01
                                                        {
                                                            Address = new Address_V01 { Country = "RU" }
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

                    location.PickupLocationNickname = shippingInfo.Description;
                }
            }
            return pickupLocations.Where(l => !string.IsNullOrEmpty(l.PickupLocationNickname)).ToList();
        }



        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options, DeliveryOptionType type)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[PUCourierCacheKey] as List<DeliveryOption>;
                if (null != deliveryOptions)
                {
                    deliveryOptions.AddRange(options);
                }
                else
                {

                    HttpRuntime.Cache.Insert(PUCourierCacheKey,
                                             options,
                                             null,
                                             DateTime.Now.AddMinutes(RU_SHIPPINGINFO_CACHE_MINUTES),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);

                }
            }
            else
            {
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
                if (null != deliveryOptions)
                {
                    deliveryOptions.AddRange(options);
                }
                else
                {

                    HttpRuntime.Cache.Insert(CacheKey,
                                             options,
                                             null,
                                             DateTime.Now.AddMinutes(RU_SHIPPINGINFO_CACHE_MINUTES),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);

                }
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            DeliveryPickupAlternativesResponse_V03 pickupAlternativesResponse = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> result = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03
                {
                    CountryCode =  Country,
                    State = string.Empty
                })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

                if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                {
                    bool courier = false;
                    if (type == DeliveryOptionType.PickupFromCourier)
                    {
                        courier = true;
                    }
                    result.AddRange(
                        from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                        select new DeliveryOption(po,courier));
                    Array.ForEach(result.ToArray(), a => a.Address = getAddress(pickupAlternativesResponse,a.Id));

                }

                return result;

            }
            else
            {
                List<DeliveryOption> result = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                DeliveryOptionForCountryRequest_V01 request = new DeliveryOptionForCountryRequest_V01();
                request.Country = Country;
                request.State = (address != null && address.Address != null &&
                                 !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                                    ? address.Address.StateProvinceTerritory
                                    : "Москва";
                request.Locale = locale;
                ShippingAlternativesResponse_V01 response =
                    proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
                {
                    DeliveryOption currentOption = new DeliveryOption(option);
                    currentOption.Name = option.Description;
                    currentOption.WarehouseCode = option.WarehouseCode;
                    currentOption.State = request.State;
                    currentOption.displayIndex = option.DisplayOrder;
                    currentOption.DisplayName = option.Description;
                    result.Add(currentOption);
                }
                //result.Sort((x, y) => x.displayIndex.CompareTo(y.displayIndex)); 
                return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
            }
        }

        private static Address_V01 getAddress(DeliveryPickupAlternativesResponse_V03 DPA, int id)
        {
            var address = DPA.DeliveryPickupAlternatives.Where(x=>x.ID==id).Select(x => x.PickupAddress.Address).First();
            return address;
        }


        public override bool FormatAddressForHMS(MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Address address)
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

        public override bool HasAdditionalPickup()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.EventId))
            {
                var hasLocations = false;
                var distributorEvents = GetEventsByDistributor();
                if (distributorEvents != null)
                {
                    foreach (var distributorEvent in distributorEvents)
                    {
                        hasLocations |= distributorEvent.Value;
                    }
                }
                return hasLocations;
            }
            return false;
        }

        public Dictionary<string, bool> GetEventsByDistributor()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.EventId))
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var cacheKey = string.Format("distributorEvents_{0}", distributorProfileModel.Value.Id);
                var distributorEvents = HttpRuntime.Cache[cacheKey] as Dictionary<string, bool>;
                if (distributorEvents == null || distributorEvents.Count == 0)
                {
                    var eventIds = HLConfigManager.Configurations.DOConfiguration.EventId.Split('|');
                    foreach (var id in eventIds)
                    {
                        var eventId = 0;
                        if (int.TryParse(id, out eventId) && eventId > 0)
                        {
                            var dsWithTicket = false;
                            var isQualified = DistributorOrderingProfileProvider.IsEventQualified(eventId, "ru-RU", out dsWithTicket);
                            if (distributorEvents == null) distributorEvents = new Dictionary<string, bool>();
                            distributorEvents.Add(id, isQualified && dsWithTicket);
                        }
                    }
                    HttpRuntime.Cache.Insert(cacheKey, distributorEvents, null, DateTime.Now.AddMinutes(RU_SHIPPINGINFO_CACHE_MINUTES),
                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                return distributorEvents;
            }
            return null;
        }

        private List<DeliveryOption> GetPickupLocationByDistributor(List<DeliveryOption> locations)
        {
            var distributorEvents = GetEventsByDistributor();
            if (distributorEvents != null)
            {
                var qualifiedLocations = new List<string>();
                foreach (var location in locations)
                {
                    if (!string.IsNullOrEmpty(location.PostalCode) && distributorEvents.ContainsKey(location.PostalCode))
                    {
                        var isQual = false;
                        if (distributorEvents.TryGetValue(location.PostalCode, out isQual) && isQual)
                        {
                            qualifiedLocations.Add(location.PostalCode);
                        }
                    }
                }
                locations.RemoveAll(l => !qualifiedLocations.Contains(l.PostalCode));
                return locations;
            }
            return new List<DeliveryOption>();
        }

        public override string GetDifferentHtmlFragment(string option)
        {
            if (option == "Pickup1")
            {
                return "reviewcartstep1Extravaganza.html";
            }
            return string.Empty;
        }


        private string GetSubstring(string info)
        {
            if (string.IsNullOrEmpty(info))
                return info;

            if (UTF8Encoding.Unicode.GetByteCount(info) <= 60)
                return info;

            var length = info.Length * 60 / UTF8Encoding.Unicode.GetByteCount(info);
            return info.Substring(0, length);
        }
    }
}
