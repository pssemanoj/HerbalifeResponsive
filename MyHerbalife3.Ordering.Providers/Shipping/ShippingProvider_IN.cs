using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Ordering.AddressLookup.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using OrderTotals_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_IN : ShippingProviderBase
    {
        #region Constants and Fields

        public static readonly decimal CartAmountCap = 18000.00M;

        private const string CacheKey = "DeliveryInfo_IN";
        private const string StatesCacheKey = "States_IN";
        private const string MajorCityCacheKey = "MajorCity_IN";

        private const int InShippinginfoCacheMinutes = 60;
        #endregion

        #region Constructors and Destructors

        static ShippingProvider_IN()
        {
        }

        #endregion

        private class MajorCity
        {
            public string State { get; set; }
            public string City { get; set; }
            public string Description { get; set; }
        }

        #region Public Methods and Operators

        public override List<string> GetCitiesForState(string country, string state)
        {
            // Get cities from DB
            IAddressLookupProvider addressLookupProvider = new AddressLookupProviderBase();
            var citiesFromDB = addressLookupProvider.GetCitiesForState(country, state);

            // Include the major cities
            var majorCities = GetMajorCities(state);
            if (majorCities.Any())
            {
                var cities = new List<string>();
                cities.AddRange(majorCities);
                cities.AddRange(citiesFromDB);
                return cities;
            }

            return citiesFromDB;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                if (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                {
                    HttpRuntime.Cache.Remove(CacheKey);
                }
                return GetDeliveryOptionsFromCache(System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
            }

            var lstDeliveryOptions = base.GetDeliveryOptions(DeliveryOptionType.Pickup, address);
            if (lstDeliveryOptions != null)
            {
                lstDeliveryOptions = (from l in lstDeliveryOptions orderby l.Description select l).ToList();
            }
            return lstDeliveryOptions;
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
                    var address = new ShippingAddress_V02()
                    {
                        ID = shippingInfo.Address.ID,
                        Recipient = shippingInfo.Description,
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        MiddleName = string.Empty,
                        Address = shippingInfo.Address.Address,
                        Phone = string.Empty,
                        AltPhone = string.Empty,
                        IsPrimary = shippingInfo.Address.IsPrimary,
                        Alias = shippingInfo.Address.Alias,
                        Created = DateTime.Now
                    };
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
            return pickupLocations.Where(l => !string.IsNullOrEmpty(l.PickupLocationNickname)).ToList();
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
                                shippingInfo = new ShippingInfo(deliveryOptionForAddress) { Id = deliveryOptionID };
                                return shippingInfo;
                            }
                        }
                    }
                    return shippingInfo;
                }
            }

            shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            if (shippingInfo != null && type == DeliveryOptionType.Shipping)
            {
                var currentAddress = shippingInfo.Address;
                var stateCode = this.GetStateCodeGivenStateName(currentAddress.Address.StateProvinceTerritory.Trim());
                var srchCity = currentAddress.Address.City.Trim().ToLower();
                if (currentAddress != null)
                {
                    //1)get cart total & compare with CAP
                    var listDeliveryOptions =
                        base.GetDeliveryOptions(
                            DeliveryOptionType.Shipping, currentAddress as ShippingAddress_V02);

                    var availableOptions =
                        listDeliveryOptions.FindAll(p => p.State.ToLower().Trim() == stateCode.ToLower().Trim());
                    if (availableOptions.Count > 0)
                    {
                        var optionForMajorCity =
                            availableOptions.FirstOrDefault(
                                i =>
                                !string.IsNullOrEmpty(i.Address.City) &&
                                i.Address.City.Trim().ToLower() == srchCity.Trim().ToLower());
                        deliveryOptionForAddress = optionForMajorCity
                                                   ?? availableOptions.FindAll(p => p.Address.City == null).FirstOrDefault();

                        if (deliveryOptionForAddress != null)
                        {
                            shippingInfo.FreightCode = deliveryOptionForAddress.FreightCode;
                            shippingInfo.WarehouseCode = deliveryOptionForAddress.WarehouseCode;
                        }
                    }
                }
            }
            return shippingInfo;
        }

        public override List<string> GetStatesForCountry(string Country)
        {
            if (Country.Equals("en-IN"))
            {
                HttpRuntime.Cache.Remove(CacheKey);
                var pickupStores = GetDeliveryOptionsFromCache(Country, null);
                return
                    (from o in pickupStores select o.Address.StateProvinceTerritory).Distinct().OrderBy(s => s).ToList();
            }
            return base.GetStatesForCountry(Country);
        }

        public bool IsMajorCity(string state, string city)
        {
            bool isMajorCity = false;
            var stateId = this.GetStateCodeGivenStateName(state);
            if (!string.IsNullOrEmpty(stateId))
            {
                var majorCities = GetMajorCities();
                if (majorCities != null)
                {
                    isMajorCity =
                        majorCities.Any(i => i.State == stateId && i.City.Trim().ToLower().Equals(city.Trim().ToLower()));
                }
            }
            return isMajorCity;
        }

        public override void SetShippingInfo(ServiceProvider.CatalogSvc.ShoppingCart_V01 cart)
        {
            var thisCart = cart as MyHLShoppingCart;

            if (thisCart != null)
            {
                if (thisCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                   if (!this.SetFreightCodesForIBPOrders(thisCart)
                        && !APFDueProvider.containsOnlyAPFSku(thisCart.CartItems))
                    {
                        if (thisCart.Totals != null)
                        {
                            var state = (thisCart.DeliveryInfo.Address.Address.StateProvinceTerritory ?? string.Empty).Trim();
                            var city = (thisCart.DeliveryInfo.Address.Address.City ?? string.Empty).Trim();
                            if ((thisCart.Totals as OrderTotals_V01).ItemsTotal < CartAmountCap)
                            {
                                
                                //set cart warehouseid
                               
                                    if(IsMajorCity(state,city))
                                    {
                                        thisCart.DeliveryInfo.FreightCode = "FSL";
                                    }
                                    else
                                    {
                                        thisCart.DeliveryInfo.FreightCode = "IND";
                                    }
                                   
                                    ShoppingCartProvider.UpdateShoppingCart(thisCart); // save to database
                                
                            }
                            else
                            {
                                var provider = new ShippingProvider_IN();
                                var shippingInfo =
                                    provider.GetShippingInfoFromID(
                                        thisCart.DistributorID,
                                        thisCart.Locale,
                                        thisCart.DeliveryInfo.Option,
                                        thisCart.DeliveryOptionID,
                                        thisCart.DeliveryInfo.Address.ID);
                                if (shippingInfo!=null)
                                {
                                    if (IsMajorCity(state, city))
                                    {
                                        thisCart.DeliveryInfo.FreightCode = "FSL";
                                    }
                                    else
                                    {
                                        thisCart.DeliveryInfo.FreightCode = "IND";
                                    }
                                    thisCart.DeliveryInfo = shippingInfo; // will invoke save to database
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {               
            DeliveryOption deliveryOptionForAddress = null;

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                var currentAddress = shoppingCart.DeliveryInfo.Address;
                var stateCode = this.GetStateCodeGivenStateName(currentAddress.Address.StateProvinceTerritory.Trim());
                var srchCity = currentAddress.Address.City.Trim().ToLower();
                if (currentAddress != null)
                {
                    //1)get cart total & compare with CAP
                    var listDeliveryOptions =
                        base.GetDeliveryOptions(
                            DeliveryOptionType.Shipping, currentAddress as ShippingAddress_V02);

                    var availableOptions =
                        listDeliveryOptions.FindAll(p => p.State.ToLower().Trim() == stateCode.ToLower().Trim());
                    if (availableOptions.Count > 0)
                    {
                        var optionForMajorCity =
                            availableOptions.FirstOrDefault(
                                i =>
                                !string.IsNullOrEmpty(i.Address.City) &&
                                i.Address.City.Trim().ToLower() == srchCity.Trim().ToLower());
                        deliveryOptionForAddress = optionForMajorCity
                                                   ?? availableOptions.FindAll(p => p.Address.City == null).FirstOrDefault();

                        if (deliveryOptionForAddress != null)
                        {
                            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                                address.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                            else
                                shoppingCart.DeliveryInfo.WarehouseCode = deliveryOptionForAddress.WarehouseCode;
                        }
                    }
                }
            }
        }
        #endregion

        #region Methods

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = GetDeliveryOptionsFromService(locale);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }

            if (deliveryOptions == null)
            {
                return null;
            }

            if (address != null && address.Address != null)
            {
                var byState = from po in deliveryOptions
                              where
                                  po.Address.StateProvinceTerritory.ToUpper()
                                    .Equals(address.Address.StateProvinceTerritory.ToUpper())
                              select po;

                if (!string.IsNullOrEmpty(address.Address.City))
                {
                    var byCity = from po in byState
                                 where po.Address.City.ToUpper().Equals(address.Address.City.ToUpper())
                                 select po;
                    return byCity.OrderBy(d => d.DisplayName).ToList();
                }

                return byState.OrderBy(d => d.DisplayName).ToList();
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
                    CacheKey,
                    options,
                    null,
                    DateTime.Now.AddMinutes(InShippinginfoCacheMinutes),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                {
                    string countryCode = locale.Substring(3, 2);
                    List<PickupLocationPreference_V01> pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var pickupLocation = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                        var accessPoints = GetDeliveryOptionsFromCache(locale, null);
                        if (pickupLocation != null && accessPoints != null)
                        {
                            var accessPoint = accessPoints.Where(ap => ap.Id == pickupLocation.PickupLocationID).FirstOrDefault();
                            if (accessPoint != null)
                            {
                                return string.Format("{0} {1}", accessPoint.Id, accessPoint.Name);
                            }
                        }
                    }
                    return string.Empty;
                }
            }
            return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
        }

        private string GetStateCodeGivenStateName(string stateName)
        {
            var stateCode = string.Empty;
            try
            {
                var states = HttpRuntime.Cache[StatesCacheKey] as List<KeyValuePair<string, string>>;
                if (states == null)
                {
                    states = GlobalResourceHelper.GetGlobalEnumeratorElements("IndiaStates").ToList();
                    if (states.Count > 0)
                    {
                        HttpRuntime.Cache.Insert(StatesCacheKey, states, null,
                            DateTime.Now.AddMinutes(InShippinginfoCacheMinutes),
                            Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }

                if (states != null && states.Any())
                {
                    stateCode = states.FirstOrDefault(s => s.Value.ToLower() == stateName.ToLower()).Key;
                }
            }
            catch (Exception)
            {
                stateCode = string.Empty;
            }
            return stateCode;
        }

        private bool SetFreightCodesForIBPOrders(MyHLShoppingCart thisCart)
        {
            bool isIbp = false;

            if (string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.IBPSku))
            {
                return isIbp;
            }

            var ibpSkus = HLConfigManager.Configurations.DOConfiguration.IBPSku.Split(',').ToList();
            string oldFreightCode = thisCart.DeliveryInfo.FreightCode;

            if (thisCart.CartItems.Any() && thisCart.Totals != null)
            {
                if (thisCart.CartItems.Any(i => ibpSkus.Contains(i.SKU)))
                {
                    isIbp = true;
                    var state = (thisCart.DeliveryInfo.Address.Address.StateProvinceTerritory ?? string.Empty).Trim();
                    var city = (thisCart.DeliveryInfo.Address.Address.City ?? string.Empty).Trim();
                     // Check if it's a major city
                   if (IsMajorCity(state,city))
                    {
                        thisCart.DeliveryInfo.FreightCode = "FSL";
                    }
                    else
                    {
                        thisCart.DeliveryInfo.FreightCode = "IND";
                    }
                  
                    //check IBP with APF (User Story 164536)
                    
                    var apfSkus = APFDueProvider.GetAPFSkuList();
                    var thisSkus = thisCart.CartItems.Select(x => x.SKU).ToList();
                    thisSkus = thisSkus.Except(apfSkus).ToList();
                    var nonHAP = thisSkus.Except(ibpSkus).ToList();

                    if (thisCart.CartItems.Any(i => apfSkus.Contains(i.SKU)))
                    {
                        thisCart.DeliveryInfo.FreightCode = "NOF";
                    }
                }
            }
            if (thisCart.DeliveryInfo.FreightCode != oldFreightCode)
            {
                ShoppingCartProvider.UpdateShoppingCart(thisCart); // save to database
            }

            return isIbp;
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
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}{3}<br>{4}, {5}, {6}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty,
                    (!string.IsNullOrEmpty(address.Address.Line3) || !string.IsNullOrEmpty(address.Address.Line4)) ? string.Format("<br>{0},{1}", address.Address.Line3 ?? string.Empty, address.Address.Line4 ?? string.Empty) : string.Empty,
                    address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
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

        private List<MajorCity> GetMajorCities()
        {
            var majorCities = new List<MajorCity>();
            try
            {
                majorCities = HttpRuntime.Cache[MajorCityCacheKey] as List<MajorCity>;

                if (majorCities == null)
                {
                    var shippingInfo = base.GetDeliveryOptions("en-IN");
                    majorCities = (from i in shippingInfo 
                                   where !string.IsNullOrEmpty(i.Description) && i.Description.ToLower().Equals("major city")
                                   select
                                       new MajorCity
                                           {
                                               State = i.State, City = i.Address.City, Description = i.Description
                                           }).ToList();
                    if (majorCities.Count > 0)
                    {
                        HttpRuntime.Cache.Insert(StatesCacheKey, majorCities, null,
                            DateTime.Now.AddMinutes(InShippinginfoCacheMinutes),
                            Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
            }
            catch (Exception)
            {
                majorCities = null;
            }
            return majorCities;
        }

        private List<string> GetMajorCities(string state)
        {
            var majorCities = GetMajorCities();
            if (majorCities != null)
            {
                return (from i in majorCities
                        where i.State == state
                        select string.Format("{0}:{1}", i.Description, i.City)).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
      #endregion
    }
}