using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.AddressLookup.Providers;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Ordering.ServiceProvider;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using UpdateShoppingCartRequest_V02 = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.UpdateShoppingCartRequest_V02;
using UpdateShoppingCartResponse_V02 = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.UpdateShoppingCartResponse_V02;
using InvoiceHandlingType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.InvoiceHandlingType;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProviderBase : IShippingProvider
    {
        public const int SHIPPINGADDRESS_CACHE_MINUTES = 60;
        public const int DELIVERY_TIME_ESTIMATED_CACHE_MINUTES = 60;

        public const string DELIVERY_OPTIONS_CACHE_PREFIX = "DeliveryOptions_";
        public const string SESSION_SHIPPING_ADDRESS_CACHE_PREFIX = "SessionShippingAddress_";
        public const string SHIPPING_ADDRESS_CACHE_PREFIX = "ShippingAddress_";
        public const string OSHIPPING_ADDRESS_CACHE_PREFIX = "OrderShippingAddress_";

        // Preference
        public const string PICKUPLOC_PREFERENCE_CACHE_PREFIX = "PickupLocationPreference_";
        public const string SESSION_PICKUPLOC_PREFERENCE_CACHE_PREFIX = "SessionPickupLocationPreference_";

        private static List<string> NonInvoiceOptionsCountries = new List<string>(Settings.GetRequiredAppSetting("NonInvoiceOptionsCountries").Split(new[] { ',' }));

        /// <summary>
        ///     Cache duration for Shipping
        /// </summary>
        public const int ShippingCacheMinutes = 1440;

        private readonly ISimpleCache _cache = CacheFactory.Create();

        public OrderCategoryType OrderType { get; set; }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingChinaSvc.IChinaShipping ChinaShippingServiceProxy
        {
            get; set;
        }

        #region IShippingProvider Members

        public virtual List<string> GetCountiesForCity(string country, string state, string city)
        {
            string CacheKey = string.Format("{0}_{1}_{2}_{3}", "NEIGHBORHOODSFORCITY", country, state, city);
            List<string> lsNeighborhoods = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsNeighborhoods == null)
            {
                try
                {
                    // invoke the service and obtain a list of neighborhoods
                    var service = ServiceClientProvider.GetShippingServiceProxy();
                    var request = new CountiesForCityRequest_V01() { Country = country, State = state, City = city };
                    var response = service.GetCountiesForCity(new GetCountiesForCityRequest(request));
                    var result = response.GetCountiesForCityResult as CountiesForCityResponse_V01;
                    if (result != null && result.Counties != null && result.Counties.Count > 0 && !string.IsNullOrEmpty(result.Counties.FirstOrDefault()))
                        lsNeighborhoods = result.Counties;
                    else
                        lsNeighborhoods = new List<string>();
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("GetNeighborhoodsForCity error: Country {0}, State {1}, City {2}, error: {3}", country,
                        state, city, ex.ToString()));
                }

                // save to cache the query
                if (lsNeighborhoods != null && lsNeighborhoods.Count > 0)
                    HttpRuntime.Cache.Insert(CacheKey, lsNeighborhoods, null, DateTime.Now.AddMinutes(SHIPPINGADDRESS_CACHE_MINUTES), 
                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsNeighborhoods;
        }

        public virtual List<string> GetZipsForCounty(string country, string state, string city, string county)
        {
            string CacheKey = string.Format("{0}_{1}_{2}_{3}_{4}", "ZIPSFORCOUNTY", country, state, city, county);
            List<string> lsZips = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsZips == null)
            {
                try
                {
                    // invoke the service and obtain a list of neighborhoods
                    var service = ServiceClientProvider.GetShippingServiceProxy();
                    var request = new ZipsForCountyRequest_V01() { Country = country, State = state, City = city, County = county };
                    var response = service.GetZipsForCounty(new GetZipsForCountyRequest(request));
                    var result = response.GetZipsForCountyResult as ZipsForCountyResponse_V01;
                    lsZips = result.Zips;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("GetZipsForCounty error: Country {0}, State {1}, City {2}, County {3}, error: {4}", country,
                        state, city, county, ex.ToString()));
                }

                // save to cache the query
                if (lsZips != null)
                    HttpRuntime.Cache.Insert(CacheKey, lsZips, null, DateTime.Now.AddMinutes(SHIPPINGADDRESS_CACHE_MINUTES),
                        Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsZips;
        }

        public virtual List<ServiceProvider.ShippingSvc.Address_V01> AddressSearch(string SearchTerm)
        {
            return null;
        }

        public virtual List<DeliveryOption> GetDeliveryOptions(string locale)
        {
            return getDeliveryOptionsFromCache(locale);
        }

        public List<string> GetAddressField(AddressFieldForCountryRequest_V01 request)
        {
            try
            {
                string CacheKey = string.Format("FLD4CNTRY_{0}_{1}_{2}_{3}_{4}_{5}_{6}", request.AddressField, request.Country, request.State ?? string.Empty, 
                    request.Province ?? string.Empty, request.City ?? string.Empty, request.County ?? string.Empty, request.Neighborhood ?? string.Empty);
                var addressFields = HttpRuntime.Cache[CacheKey] as List<AddressFieldInfo>;
                if (addressFields == null)
                {
                    addressFields = AddressLookupProvider.GetAddressLookupProvider(request.Country).GetAddressFieldsForCountry(request);
                    if (addressFields != null && addressFields.Any())
                        HttpRuntime.Cache.Insert(CacheKey, addressFields, null, DateTime.Now.AddMinutes(SHIPPINGADDRESS_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                if (addressFields != null)
                    return addressFields.Where(x=>x.HasShipping=="Y").Select(a => a.AddressField).ToList();
            }
            catch (Exception ex) 
            {
                LoggerHelper.Error(string.Format("GetAddressField error: Field {0}, Country{1}, State {2}, City {3}, County {4}, error: {5}", 
                    request.AddressField, 
                    request.Country,
                    request.State, 
                    request.City, 
                    request.County, 
                    ex.ToString()));
            }

            return null;
        }

        public virtual List<string> GetStatesForCountry(string country)
        {
            return AddressLookupProvider.GetAddressLookupProvider(country).GetStatesForCountry(country);
        }

        public virtual List<string> GetStatesForCountry(string country, int cacheMinute)
        {
            string CacheKey = string.Format("{0}_{1}", "STATESFORCOUNTRY", country);
            List<string> lsStatesForCountry = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsStatesForCountry == null)
            {
                lsStatesForCountry = AddressLookupProvider.GetAddressLookupProvider(country).GetStatesForCountry(country);
                if (lsStatesForCountry != null)
                    HttpRuntime.Cache.Insert(CacheKey, lsStatesForCountry, null, DateTime.Now.AddMinutes(cacheMinute), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsStatesForCountry;
        }

        public virtual List<string> GetCitiesForState(string country, string state)
        {
            return AddressLookupProvider.GetAddressLookupProvider(country).GetCitiesForState(country, state);
        }

        public virtual List<string> GetCitiesForState(string country, string state, int cacheMinute)
        {
            string CacheKey = string.Format("{0}_{1}_{2}", "CITIESFORSTATE", country, state);
            List<string> lsCitiesForState = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsCitiesForState == null)
            {
                lsCitiesForState = AddressLookupProvider.GetAddressLookupProvider(country).GetCitiesForState(country, state);
                if (lsCitiesForState != null)
                    HttpRuntime.Cache.Insert(CacheKey, lsCitiesForState, null, DateTime.Now.AddMinutes(cacheMinute), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsCitiesForState;
        }

        public List<string> GetZipsForCity(string country, string state, string city)
        {
            return AddressLookupProvider.GetAddressLookupProvider(country).GetZipsForCity(country, state, city);
        }

        public List<StateCityLookup_V01> LookupCitiesByZip(string country, string ZipCode)
        {
            return AddressLookupProvider.GetAddressLookupProvider(country).LookupCitiesByZip(country, ZipCode);
        }

        public virtual bool ValidatePostalCode(string country, string state, string city, string postalCode)
        {
            return true;
        }

        public virtual List<string> GetStreetsForCity(string country, string state, string city)
        {
            return AddressLookupProvider.GetAddressLookupProvider(country).GetStreetsForCity(country, state, city);
        }

        public virtual List<string> GetStreetsForCity(string country, string state, string city, int cacheMinute)
        {
            string CacheKey = string.Format("{0}_{1}_{2}_{3}", "STREETSFORCITY", country, state, city);
            List<string> lsStreetsForCity = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsStreetsForCity == null)
            {
                lsStreetsForCity = AddressLookupProvider.GetAddressLookupProvider(country).GetStreetsForCity(country, state, city);
                if (lsStreetsForCity != null)
                    HttpRuntime.Cache.Insert(CacheKey, lsStreetsForCity, null, DateTime.Now.AddMinutes(cacheMinute), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsStreetsForCity;
        }

        public virtual string LookupZipCode(string state, string municipality, string colony)
        {
            return AddressLookupProvider.GetAddressLookupProvider(null).LookupZipCode(state, municipality, colony);
        }

        /// <summary>
        ///     GetShippingAddresses for a Distributor
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        public List<DeliveryOption> GetShippingAddresses(string distributorID, string locale)
        {
            try
            {
                var result = getShippingAddressesFromCache(distributorID, locale);
                var allResults = mergeAddressFromSession(result, distributorID, locale);

                var final = new List<DeliveryOption>();
                foreach (ShippingAddress_V02 address in allResults)
                {
                    //var address = new ShippingAddress_V02();
                    final.Add(new DeliveryOption(address));
                }

                return final;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetShippingAddresses error: DS {0}, error: {1}", distributorID, ex));
            }
            return null;
        }

        /// <summary>
        ///     Reload to cache the order shipping addresses from service.
        /// </summary>
        /// <param name="distributorID">Distributo ID parameter.</param>
        /// <param name="locale">Locale parameter.</param>
        public void ReloadOrderShippingAddressFromService(string distributorID, string locale)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);

            var result = sessionInfo.OrderShippingAddresses;
            try
            {
                // gets resultset from Business Layer is object not found in cache
                result = loadShippingAddressesFromService(distributorID, locale, true);
                // saves to cache is successful
                if (null != result)
                {
                    sessionInfo.OrderShippingAddresses = result; //Stash in Session now
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }
        }

        public List<DeliveryOption> GetShippingAddressesFromService(string distributorID, string locale)
        {
            try
            {
                var result = loadShippingAddressesFromService(distributorID, locale, true);
                var final = new List<DeliveryOption>();
                if (result != null)
                {
                    foreach (ShippingAddress_V02 address in result)
                    {
                        final.Add(new DeliveryOption(address));
                    }
                }
                return final;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetShippingAddresses error: DS {0}, error: {1}", distributorID, ex));
            }
            return null;

        }

        public int SaveShippingAddress(string distributorID,
                                       string locale,
                                       ShippingAddress_V02 shippingAddressToSave,
                                       bool tempAddress,
                                       bool addrNoChanged,
                                       bool bCheckNickname)
        {
            if (tempAddress)
            {
                return saveTempShippingAddress(distributorID, locale, shippingAddressToSave, addrNoChanged,
                                               bCheckNickname);
            }
            else
            {
                var addressId = saveShippingAddressToDB(distributorID, locale, shippingAddressToSave, addrNoChanged,
                                               bCheckNickname);
                try
                {
                    if (null != HttpContext.Current && null == HttpContext.Current.Session && addressId > 0)
                    //Session is null when called from Mobile web api
                    {
                        var cacheKey = string.Format("AllAddress_Mobile_{0}_{1}", distributorID, locale);
                        _cache.Expire(typeof(List<ShippingAddress_V02>), cacheKey);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("General", ex, "Clear Address cache failed After saveAddress");
                }
                return addressId;
            }

        }

        //id wins if >0
        public virtual ShippingAddress_V02 GetShippingAddressFromAddressGuidOrId(Guid addressGuid, int id)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new GetShippingAddressByIdRequest_V01();
                request.AddressId = addressGuid;
                if (id > 0)
                {
                    request.Id = id;
                }
                var response = proxy.GetShippingAddressById(new GetShippingAddressByIdRequest1(request)).GetShippingAddressByIdResult as GetShippingAddressByIdResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return response.ShippingAddress;
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetShippingAddressFromAddressGuid error: Guid {0} ex: {1}", addressGuid, ex));
                return null;
            }
        }

        public int DeleteShippingAddress(string distributorID, string locale, ShippingAddress_V02 address)
        {
            if (string.IsNullOrEmpty(distributorID) || null == address)
            {
                return 0;
            }

            bool delete = true;
            if (address.ID > 0)
            {
                try
                {
                    var proxy = ServiceClientProvider.GetShippingServiceProxy();
                    var request = new DeleteShippingAddressRequest_V01();
                    request.DistributorID = distributorID;
                    request.ID = address.ID;
                    var response = proxy.DeleteShippingAddress(new DeleteShippingAddressRequest1(request)).DeleteShippingAddressResult as DeleteShippingAddressResponse_V01;
                    if (response == null || response.Status != ServiceResponseStatusType.Success)
                    {
                        delete = false;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("DeleteShippingAddress error: DS {0}, addressID{2}, error: {1}", distributorID, ex,
                                      address.ID));
                }
            }

            if (delete)
            {
                var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                if (sessionInfo != null)
                {
                    var cached = sessionInfo.ShippingAddresses;
                    if (null != cached)
                    {
                        var addressToDelete = cached.Find(p => p.ID == address.ID);
                        if (addressToDelete != null)
                        {
                            cached.Remove(addressToDelete);
                            if (addressToDelete.IsPrimary) //Need to reload to get the new Primary
                            {
                                saveShippingAddressesToCache(distributorID, locale,
                                                             loadShippingAddressesFromService(distributorID, locale));
                                //do not lose the session addresses if the deleted address is primary.
                                if (addressToDelete.ID > 0)
                                {
                                    var shippingAddresses = loadShippingAddressesFromService(distributorID, locale);
                                    var sessionAddresses = cached.FindAll(p => p.ID < 0);
                                    if (sessionAddresses.Count() > 0)
                                    {
                                        if (shippingAddresses == null)
                                            shippingAddresses = new List<ShippingAddress_V02>();

                                        shippingAddresses.AddRange(sessionAddresses);
                                        saveShippingAddressesToCache(distributorID, locale, shippingAddresses);
                                    }
                                }
                            }
                        }

                        //sessionInfo.ShippingAddresses.Remove(address);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        ///     in case there is no shipping address, get address from ds mailing address
        /// </summary>
        /// <returns></returns>
        public ShippingAddress_V02 GetShippingAddressFromDefaultAddress(string distributorID)
        {
            var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (user != null)
            {
                // TODO: waiting for address
                //var address = user.Value.Address as Address_V01;
                //if (address == null)
                //{
                //    address = OrderCreationHelper.CreateDefaultAddress();
                //}
                string fullEnglishName = DistributorProfileModelHelper.FullEnglishName(user.Value);

                // TODO: waiting for address
                //if (address != null)
                //{
                //    return new ShippingAddress_V02(999, fullEnglishName,
                //                                   fullEnglishName.First,
                //                                   fullEnglishName.Last, string.Empty, address, string.Empty, string.Empty, false,
                //                                   string.Empty, DateTime.Now);
                //}
            }
            return null;
        }

        public virtual string GetFreightVariant(ShippingInfo shippingInfo)
        {
            return null;
        }

        public virtual List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address, string option)
        {
            return GetDeliveryOptions(type, address);
        }

        public virtual List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            var deliveryOptions = getDeliveryOptionsFromCache(Thread.CurrentThread.CurrentCulture.Name);
            if (deliveryOptions != null)
            {
                deliveryOptions = deliveryOptions.Where(d => d.Option == type).ToList();
            }
            return deliveryOptions;
        }

        public virtual bool UpdateShippingInfo(int shoppingCartID,
                                               ServiceProvider.CatalogSvc.OrderCategoryType type,
                                               MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.DeliveryOptionType option,
                                               int deliveryOptionID,
                                               int shippingAddressID)
        {
            if (shoppingCartID == 0)
            {
                return false;
            }
            if (shippingAddressID == -1 || deliveryOptionID == -1)
            {
                // only saved in session
                return true;
            }
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.UpdateShoppingCart(new ServiceProvider.CatalogSvc.UpdateShoppingCartRequest1(new UpdateShoppingCartRequest_V02(){ ShoppingCartID = shoppingCartID, DeliveryOptionID = deliveryOptionID,
                                                                                    OrderCategory = type,  DeliveryOption = option, ShippingAddressID =  shippingAddressID})).UpdateShoppingCartResult
                        as UpdateShoppingCartResponse_V02;
                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("Error updating Shopping Cart Shipping Info for Cart ID: {0}:\r\n{1}",
                                      shoppingCartID, ex.Message));
                }
            }
            return false;
        }

        public virtual ShippingInfo GetShippingInfoFromID(string distributorID,
                                                          string locale,
                                                          DeliveryOptionType type,
                                                          int deliveryOptionID,
                                                          int shippingAddressID)
        {
            var orginalType = type;

            if (orginalType == DeliveryOptionType.Unknown)
                type = DeliveryOptionType.Shipping;

            var deliveryOptions = getDeliveryOptionsFromCache(locale);
            DeliveryOption selectedDeliveryOption = null;

            if (type == DeliveryOptionType.Pickup)
            {
                if ((selectedDeliveryOption = deliveryOptions.Find(s => s.Id == deliveryOptionID)) != null)
                    return new ShippingInfo(selectedDeliveryOption, selectedDeliveryOption);
            }
            else if (type == DeliveryOptionType.Shipping)
            {
                var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                bool isETO = null == sessionInfo ? false : sessionInfo.IsEventTicketMode;
                var shippingOption = getDeliveryOptionFromID(deliveryOptionID, type, deliveryOptions,
                                                             isETO
                                                                 ? OrderCategoryType.ETO
                                                                 : OrderCategoryType.RSO);
                ShippingAddress_V02 shippingAddress = null;
                // if no shipping address id..

                var shippingAddresses = getShippingAddressesFromCache(distributorID, locale);
                if (shippingAddressID == 0)
                {
                    if (shippingAddresses != null && shippingAddresses.Count > 0)
                    {
                        if ((shippingAddress = shippingAddresses.Find(s => s.IsPrimary)) == null) // get primary
                            shippingAddress = shippingAddresses.First();

                        if (!string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                        {
                            var address = shippingAddresses.Find(oi => oi.ID == sessionInfo.CustomerAddressID);
                            if (address != null)
                            {
                                shippingAddress = address;
                            }
                        }
                    }
                }
                else
                {
                    if (shippingAddressID < 0)
                    {
                        var addOnCache = getShippingAddressesFromCache(distributorID, locale);
                        shippingAddresses = mergeAddressFromSession(addOnCache, distributorID, locale);
                    }
                    if (shippingAddresses != null && shippingAddresses.Count > 0)
                    {
                        shippingAddress = shippingAddresses.Find(s => s.ID == shippingAddressID);
                        if (shippingAddress == null)
                        {
                            // primaray address
                            //shippingAddress = shippingAddresses.Find(s => s.IsPrimary == true);
                            if ((shippingAddress = shippingAddresses.Find(s => s.IsPrimary)) == null)
                                // get primary
                                shippingAddress = shippingAddresses.First();
                        }
                    }
                }

                DeliveryOption deliveryOption = null;
                var freightCodeAndWarehouse = GetFreightCodeAndWarehouseFromAddress(shippingAddress, shippingOption);
                if (freightCodeAndWarehouse != null)
                {
                    deliveryOption = new DeliveryOption(freightCodeAndWarehouse[1], freightCodeAndWarehouse[0],
                                                        DeliveryOptionType.Shipping,
                                                        shippingOption != null
                                                            ? shippingOption.Description
                                                            : string.Empty);
                }
                else
                {
                    deliveryOption = shippingOption;
                }

                if (deliveryOption == null || shippingAddress == null)
                {
                    return null;
                }
                return new ShippingInfo(deliveryOption, shippingAddress);
            }
            return null;
        }

        public virtual DeliveryOption GetDefaultAddress()
        {
            return
                new DeliveryOption(new ShippingAddress_V02()
                {
                    ID = 0,
                    Recipient = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    MiddleName = string.Empty,
                    Address = new Address_V01(),
                    Phone = string.Empty,
                    AltPhone = string.Empty,
                    IsPrimary = false,
                    Alias = string.Empty,
                    Created = DateTime.Now
                });
        }

        public virtual Address_V01 GetHFFDefaultAddress(ShippingAddress_V01 address)
        {
            return new Address_V01();
        }

        public virtual string GetRecipientName(ShippingInfo currentShippingInfo)
        {
            return null;
        }

        public virtual DeliveryOption GetEventTicketShippingInfo()
        {
            DeliveryOption option = null;
            var shippingInfo = getDeliveryOptionsFromCache(Thread.CurrentThread.CurrentCulture.Name);

            if (null != shippingInfo)
            {
                var optionList =
                    (from si in shippingInfo where si.OrderCategory == MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.OrderCategoryType.ETO select si).ToList();
                if (optionList.Count() > 0)
                {
                    option = optionList.First();
                }
            }

            if (null == option)
            {
                LoggerHelper.Error(
                    string.Format("APF Warehouse DeliveryInfo not found for {0}",
                                  Thread.CurrentThread.CurrentCulture.Name));
            }

            return option;
        }

        public virtual DeliveryOption GetAPFShippingInfo()
        {
            DeliveryOption option = null;
            var shippingInfo = getDeliveryOptionsFromCache(Thread.CurrentThread.CurrentCulture.Name);

            string initialWarehouseCode = HLConfigManager.Configurations.APFConfiguration.InitialAPFwarehouse;
            string warehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
            string actualWarehouseCode = string.IsNullOrEmpty(initialWarehouseCode)
                                             ? warehouseCode
                                             : initialWarehouseCode;

            if (null != shippingInfo)
            {
                var optionList = (from si in shippingInfo
                                  where si.OrderCategory == MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.OrderCategoryType.APF &&
                                        si.WarehouseCode == actualWarehouseCode &&
                                        (null != si.Address && !string.IsNullOrEmpty(si.Address.Line1) &&
                                         !string.IsNullOrEmpty(si.Address.City))
                                  select si).ToList();
                if (optionList.Count() > 0)
                {
                    option = optionList.First();
                }
            }

            if (null == option)
            {
                LoggerHelper.Error(
                    string.Format("APF Warehouse DeliveryInfo not found for warehouse {0}, {1}", actualWarehouseCode,
                                  Thread.CurrentThread.CurrentCulture.Name));
            }

            return option;
        }

        /// <summary>
        ///     GetPickupLocationsPreferences for a Distributor
        ///     MEXICO ONLY
        /// </summary>
        /// <param>
        ///     <name>distributorId</name>
        /// </param>
        /// <param name="distributorId"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public virtual List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                        string country)
        {
            var prefList = getPickupLocationsPreferencesFromCache(distributorId, country);

            PickupLocationPreference_V01 pickupInSession = null;
            if (HttpContext.Current != null && HttpContext.Current.Session != null) // mobile web api can't access Session
            {
                var session = HttpContext.Current.Session;
                pickupInSession =
                session[getSessionPickupLocationPreferenceKey(distributorId, country)] as PickupLocationPreference_V01;
            }

            if (pickupInSession != null)
            {
                  if (prefList.All(p => p.ID != -1))
                        prefList.Add(pickupInSession);
            }
            return prefList;
        }

        public virtual List<PickupLocationPreference_V02> GetPickupLocationsPreferences(string distributorId,
                                                                                string country, string locationType)
        {
            var prefList = GetPickupLocationsPreferencesFromCache(distributorId, country, locationType);
            var session = HttpContext.Current.Session;
            var pickupInSession =
                session[getSessionPickupLocationPreferenceKey(distributorId, country)] as PickupLocationPreference_V02;
            if (pickupInSession != null)
            {
                if (prefList.All(p => p.ID != -1 && p.PickupLocationType != locationType))
                    prefList.Add(pickupInSession);
            }
            return prefList;
        }


        public int SavePickupLocationsPreferences(string distributorID,
                                                  bool toSession,
                                                  int pickupLocationID,
                                                  string pickupLocationNickname,
                                                  string pickupLocationName,
                                                  string country,
                                                  bool isPrimary)
        {
            var session = HttpContext.Current.Session;
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }
            else
            {
                if (toSession)
                {
                    var customIdForCache =
                        HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                        && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasPredefinedPickUp
                        && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PredefinedPickUpLocationName == pickupLocationNickname 
                        ? -99 : -1;
                    updatePickupPreferenceCache(distributorID, country, pickupLocationID, pickupLocationNickname,
                                                pickupLocationName, isPrimary, customIdForCache);
                    return customIdForCache;
                }
                else
                {
                    try
                    {
                        var pickupLocationPreferencesList = GetPickupLocationsPreferences(distributorID, country);
                        if (!pickupLocationNickname.Equals(string.Empty))
                        {
                            if (
                                pickupLocationPreferencesList.Exists(
                                    l =>
                                    l.PickupLocationNickname != null
                                    && l.PickupLocationNickname.Trim().ToLower() == pickupLocationNickname.Trim().ToLower()))
                            {
                                return -2;
                            }
                        }

                        if (pickupLocationID > 0)
                        {
                            if (pickupLocationPreferencesList.Exists(l => l.PickupLocationID == pickupLocationID))
                            {
                                return -3;
                            }
                        }

                        var proxy = ServiceClientProvider.GetShippingServiceProxy();
                        var request = new InsertPickupLocationPreferencesRequest_V01();
                        request.DistributorID = distributorID;
                        request.PickupLocationID = pickupLocationID;
                        request.PickupLocationNickname = pickupLocationNickname;
                        request.Country = country;
                        request.IsPrimary = isPrimary;

                        var response =
                            proxy.InsertPickupLocationsPreferences(new InsertPickupLocationsPreferencesRequest(request)).InsertPickupLocationsPreferencesResult as
                            InsertPickupLocationPreferencesResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            updatePickupPreferenceCache(distributorID, country, request.PickupLocationID,
                                                        pickupLocationNickname, pickupLocationName, isPrimary,
                                                        response.ID);

                            return response.ID;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "SavePickupLocationsPreferences error: DS {0}, error: {1}, country: {2}, pickupLocationID: {3}",
                                distributorID, ex, country, pickupLocationID));
                    }
                }
            }
            return 0;
        }

        public int SavePickupLocationsPreferences(string distributorID,
                                                  bool toSession,
                                                  int pickupLocationID,
                                                  string pickupLocationNickname,
                                                  string pickupLocationName,
                                                  string country,
                                                  bool isPrimary, string courierType)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }
            else
            {
                if (toSession)
                {
                    UpdatePickupPreferenceCache(distributorID, country, pickupLocationID, pickupLocationNickname,
                                                pickupLocationName, isPrimary, -1, courierType);
                    return pickupLocationID;
                }
                else
                {
                    try
                    {
                        var pickupLocationPreferencesList = GetPickupLocationsPreferences(distributorID, country, courierType);
                        if (!pickupLocationNickname.Equals(string.Empty))
                        {
                            if (pickupLocationPreferencesList.Exists(
                                    l =>
                                    l.PickupLocationNickname.Trim().ToLower() == pickupLocationNickname.Trim().ToLower()))
                            {
                                return -2;
                            }
                        }

                        if (pickupLocationID > 0)
                        {
                            if (pickupLocationPreferencesList.Exists(l => l.PickupLocationID == pickupLocationID && l.PickupLocationType == courierType))
                            {
                                return -3;
                            }
                        }

                        var proxy = ServiceClientProvider.GetShippingServiceProxy();
                        var request = new InsertPickupLocationPreferencesRequest_V02();
                        request.DistributorID = distributorID;
                        request.PickupLocationID = pickupLocationID;
                        request.PickupLocationNickname = pickupLocationNickname;
                        request.Country = country;
                        request.IsPrimary = isPrimary;
                        request.PickupLocationType = courierType;

                        var response =
                            proxy.InsertPickupLocationsPreferences(new InsertPickupLocationsPreferencesRequest(request)).InsertPickupLocationsPreferencesResult as
                            InsertPickupLocationPreferencesResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            UpdatePickupPreferenceCache(distributorID, country, request.PickupLocationID,
                                                        pickupLocationNickname, pickupLocationName, isPrimary,
                                                        response.ID, courierType);

                            return response.ID;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "SavePickupLocationsPreferences error: DS {0}, error: {1}, country: {2}, pickupLocationID: {3}",
                                distributorID, ex, country, pickupLocationID));
                    }
                }
            }
            return 0;
        }

        public int DeletePickupLocationsPreferences(string distributorID, int pickupLocationID, string country)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }
            else
            {
                var pickupPreferences = getPickupLocationsPreferencesFromCache(distributorID, country);
                var current =
                    (from p in pickupPreferences where p.PickupLocationID == pickupLocationID select p).ToList();
                if (null != current && current.Count > 0)
                {
                    pickupPreferences.Remove(current[0]);
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[getPickupLocationPreferenceKey(distributorID, country)] = pickupPreferences;
                    }
                    if (current[0].ID > 0)
                    {
                        try
                        {
                            var proxy = ServiceClientProvider.GetShippingServiceProxy();
                            var request = new DeletePickupLocationPreferencesRequest_V01();
                            request.DistributorID = distributorID;
                            request.PickupLocationID = pickupLocationID;
                            request.Country = country;

                            var response =
                                proxy.DeletePickupLocationPreferences(new DeletePickupLocationPreferencesRequest1(request)).DeletePickupLocationPreferencesResult as
                                DeletePickupLocationPreferencesResponse_V01;
                            if (response != null && response.Status == ServiceResponseStatusType.Success)
                            {
                                return 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "DeletePickupLocationsPreferences error: DS {0}, pickupLocationID{2}, error: {1}, country: {3}",
                                    distributorID, ex, pickupLocationID, country));
                        }
                    }
                }
            }
            return 0;
        }

        public int DeletePickupLocationsPreferences(string distributorID, int pickupLocationID, string country, string courierType)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }
            else
            {
                var pickupPreferences = GetPickupLocationsPreferencesFromCache(distributorID, country, courierType);
                var current =
                    (from p in pickupPreferences
                     where p.PickupLocationID == pickupLocationID && p.PickupLocationType == courierType
                     select p).ToList();
                if (current.Any())
                {
                    pickupPreferences.Remove(current[0]);
                    HttpContext.Current.Session[getPickupLocationPreferenceKey(distributorID, country)] = pickupPreferences;
                    if (current[0].ID > 0)
                    {
                        try
                        {
                            var proxy = ServiceClientProvider.GetShippingServiceProxy();
                            var request = new DeletePickupLocationPreferencesRequest_V02();
                            request.DistributorID = distributorID;
                            request.PickupLocationID = pickupLocationID;
                            request.Country = country;
                            request.PickupLocationType = courierType;

                            var response =
                                proxy.DeletePickupLocationPreferences(new DeletePickupLocationPreferencesRequest1(request)).DeletePickupLocationPreferencesResult as
                                DeletePickupLocationPreferencesResponse_V01;
                            if (response != null && response.Status == ServiceResponseStatusType.Success)
                            {
                                return 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "DeletePickupLocationsPreferences error: DS {0}, pickupLocationID{2}, error: {1}, country: {3}",
                                    distributorID, ex, pickupLocationID, country));
                        }
                    }
                }
            }
            return 0;
        }
        public virtual string FormatPickupLocationAddress(Address_V01 address)
        {
            return (address.Line1 + " " + address.Line2 + " " + address.Line3 + " " + address.Line4 +
                    "<br>" +
                    address.City + ", " + address.StateProvinceTerritory + " " + address.PostalCode);
        }

        public virtual string GetFreightCodeForState(string countrycode, string state, string locale)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new FreightCodeForStateRequest_V01();
                request.CountryCode = countrycode;
                request.State = state;
                request.Locale = locale;
                var response = proxy.GetFreightCodeForState(new GetFreightCodeForStateRequest(request)).GetFreightCodeForStateResult as FreightCodeForStateResponse_V01;
                return response.FreightCode;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetFreightCodeForState error: Country {0}, error: {1}", countrycode, ex));
            }
            return null;
        }

        public List<ShippingAddress_V02> GetShippingAddressesFromSession(string distributorID, string locale)
        {
            if (null == HttpContext.Current)
            {
                return null;
            }
            var session = HttpContext.Current.Session;
            string key = getSessionShippingAddressKey(distributorID, locale);
            if (session != null)
            {
                return session[key] as List<ShippingAddress_V02>;
            }
            return null;
        }

        public string getSessionShippingAddressKey(string distributorID, string locale)
        {
            return SESSION_SHIPPING_ADDRESS_CACHE_PREFIX + distributorID + "_" + locale;
        }

        public string getShippingAddressKey(string distributorID, string locale)
        {
            return SHIPPING_ADDRESS_CACHE_PREFIX + distributorID + "_" + locale;
        }

        public string getOrderShippingAddressKey(string distributorID, string locale)
        {
            return OSHIPPING_ADDRESS_CACHE_PREFIX + distributorID + "_" + locale;
        }

        //public ShippingAddress_V01 GetShippingAddress(string distributorID, int id)
        //{
        //    List<ShippingAddress_V01> addressList = getGetShippingAddressesFromCache(distributorID);
        //    if (addressList != null)
        //    {
        //        return addressList.Exists(p => p.ID == id) ? addressList.Where(p => p.ID == id).First() : null;
        //    }
        //    return null;
        //}

        public int ClearShippingAddressFromSession(string distributorID, string locale)
        {
            var session = HttpContext.Current.Session;
            session.Remove(getSessionShippingAddressKey(distributorID, locale));
            return -1;
        }

        private bool duplicateNickName(List<ShippingAddress_V02> addressList, ShippingAddress_V02 addressToCheck)
        {
            if (addressToCheck == null)
            {
                return false;
            }
            if (addressList == null || addressList.Count == 0)
            {
                return false;
            }
            string nickName = addressToCheck.Alias;

            if (!string.IsNullOrEmpty(nickName))
            {
                if (
                    addressList.Exists(
                        l => !string.IsNullOrEmpty(l.Alias)
                                 ? l.Alias.Trim().ToLower() == nickName.Trim().ToLower()
                                 : string.Empty == nickName.Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private bool duplicateShippingAddress(List<ShippingAddress_V02> addressList, ShippingAddress_V02 addressToCheck)
        {
            if (addressToCheck == null)
            {
                return false;
            }

            if (addressList == null || addressList.Count == 0)
            {
                return false;
            }

            if (addressList.Exists(l => l.Phone == addressToCheck.Phone &&
                                          (l.Address.City ?? "").ToUpper() == (addressToCheck.Address.City ?? "").ToUpper() &&
                                          (l.Address.Country ?? "").ToUpper() == (addressToCheck.Address.Country ?? "").ToUpper() &&
                                          l.Address.CountyDistrict == addressToCheck.Address.CountyDistrict &&
                                          (l.Address.Line2 ?? "").ToUpper() == (addressToCheck.Address.Line2 ?? "").ToUpper() &&
                                          (l.Address.Line1 ?? "").ToUpper() == (addressToCheck.Address.Line1 ?? "").ToUpper() &&
                                          l.Address.PostalCode == addressToCheck.Address.PostalCode &&
                                          (l.Address.StateProvinceTerritory ?? "").ToUpper() == (addressToCheck.Address.StateProvinceTerritory ?? "").ToUpper() &&
                                          (l.Recipient ?? "").ToUpper() == (addressToCheck.Recipient ?? "").ToUpper()))
            {
                return true;
            }

            return false;
        }

        private int saveTempShippingAddress(string distributorID,
                                            string locale,
                                            ShippingAddress_V02 shippingAddressToSave,
                                            bool addrNoChanged,
                                            bool bCheckNickname)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            var cachedAddresses = sessionInfo.ShippingAddresses;

            if (bCheckNickname)
            {
                if (duplicateNickName(cachedAddresses, shippingAddressToSave))
                {
                    return -3;
                }
            }

            var tempShippingAddresses = new List<ShippingAddress_V02>();
            if (cachedAddresses != null)
            {
                if (cachedAddresses.Count > 0)
                {
                    tempShippingAddresses = (from s in cachedAddresses where s.ID < 0 select s).ToList();
                }
            }

            if (!addrNoChanged)
            {
                // if saving new address and nick name is unique , don't check address
                if (shippingAddressToSave.ID != 0)
                {
                    if (duplicateShippingAddress(tempShippingAddresses, shippingAddressToSave))
                    {
                        //For temporary address, not required to save if it is duplicated, simply return the duplicated address id.
                        var matchedAddressID = (from a in tempShippingAddresses
                                                where a.Phone == shippingAddressToSave.Phone &&
                                                    (a.Address.City ?? "").ToUpper() == (shippingAddressToSave.Address.City ?? "").ToUpper() &&
                                                    (a.Address.Country ?? "").ToUpper() == (shippingAddressToSave.Address.Country ?? "").ToUpper() &&
                                                    a.Address.CountyDistrict == shippingAddressToSave.Address.CountyDistrict &&
                                                    (a.Address.Line2 ?? "").ToUpper() == (shippingAddressToSave.Address.Line2 ?? "").ToUpper() &&
                                                    (a.Address.Line1 ?? "").ToUpper() == (shippingAddressToSave.Address.Line1 ?? "").ToUpper() &&
                                                    a.Address.PostalCode == shippingAddressToSave.Address.PostalCode &&
                                                    (a.Address.StateProvinceTerritory ?? "").ToUpper() == (shippingAddressToSave.Address.StateProvinceTerritory ?? "").ToUpper() &&
                                                    (a.Recipient ?? "").ToUpper() == (shippingAddressToSave.Recipient ?? "").ToUpper()
                                                select a.ID).FirstOrDefault();

                        return matchedAddressID;
                    }
                }
            }
            if (tempShippingAddresses == null)
            {
                tempShippingAddresses = new List<ShippingAddress_V02>();
            }
            shippingAddressToSave.DisplayName = string.IsNullOrEmpty(shippingAddressToSave.Alias)
                                                    ? GetAddressDisplayName(shippingAddressToSave)
                                                    : shippingAddressToSave.Alias;

            if (tempShippingAddresses.Count > 0)
            {
                var addressExists = cachedAddresses.Find(p => p.ID == shippingAddressToSave.ID);
                if (addressExists != null)
                {
                    //Editing a non-saved address
                    cachedAddresses.Remove(addressExists);
                }
                else
                {
                    var lastTemp =
                        (from p in tempShippingAddresses
                         where p.ID == (from pi in tempShippingAddresses select pi.ID).Min()
                         select p).Single();
                    if (lastTemp == null)
                    {
                        shippingAddressToSave.ID = -4;
                    }
                    else
                    {
                        shippingAddressToSave.ID = lastTemp.ID - 1;
                    }
                }
            }
            else
            {
                if (shippingAddressToSave.ID >= 0)
                {
                    shippingAddressToSave.ID = -4;
                }
            }

            bool cachedAddressesWereEmpty = false;
            if (cachedAddresses == null)
            {
                cachedAddresses = new List<ShippingAddress_V02>();
                cachedAddressesWereEmpty = true;
            }

            cachedAddresses.Add(shippingAddressToSave);

            if (cachedAddressesWereEmpty)
            {
                sessionInfo.ShippingAddresses = cachedAddresses;
                SessionInfo.SetSessionInfo(distributorID, locale, sessionInfo);
            }

            return shippingAddressToSave.ID;
        }

        public int saveShippingAddressToDB(string distributorID,
                                           string locale,
                                           ShippingAddress_V02 shippingAddressToSave,
                                           bool addrNoChanged,
                                           bool bCheckNickname)
        {
            List<ShippingAddress_V02> cachedAddresses = null;
            var tempShippingAddresses = new List<ShippingAddress_V02>();
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (null != sessionInfo)
            {
                cachedAddresses = sessionInfo.ShippingAddresses;
                if (cachedAddresses != null && shippingAddressToSave!=null)
                {
                    if (cachedAddresses.Count > 0)
                    {
                        tempShippingAddresses = (from s in cachedAddresses where s.ID < 0 select s).ToList();
                        var tempAddressToBeDeleted = tempShippingAddresses.Find(s => s.ID == shippingAddressToSave.ID);
                        if (tempAddressToBeDeleted != null)
                            tempShippingAddresses.Remove(tempAddressToBeDeleted);
                    }
                }
            }

            if (string.IsNullOrEmpty(distributorID) || shippingAddressToSave == null)
            {
                return 0;
            }
            else
            {
                try
                {
                    if (bCheckNickname && null != cachedAddresses)
                    {
                        if (duplicateNickName(cachedAddresses, shippingAddressToSave))
                        {
                            return -3;
                        }
                    }
                    if (addrNoChanged == false)
                    {
                        // if saving new address and nick name is unique , don't check address
                        if (shippingAddressToSave.ID != 0)
                        {
                            if (duplicateShippingAddress(cachedAddresses, shippingAddressToSave))
                            {
                                return -2;
                            }
                        }
                    }

                    var proxy = ServiceClientProvider.GetShippingServiceProxy();
                    var request = new InsertShippingAddressRequest_V02();
                    request.Address = shippingAddressToSave.Address;
                    request.Alias = shippingAddressToSave.Alias;
                    request.DistributorID = distributorID;
                    request.PhoneNumber = shippingAddressToSave.Phone;
                    request.AltPhoneNumber = shippingAddressToSave.AltPhone;
                    request.IsPrimary = shippingAddressToSave.IsPrimary;
                    request.ID = shippingAddressToSave.ID;
                    request.Recipient = shippingAddressToSave.Recipient;
                    request.FirstName = shippingAddressToSave.FirstName;
                    request.LastName = shippingAddressToSave.LastName;
                    request.MiddleName = shippingAddressToSave.MiddleName;
                    if (null != shippingAddressToSave.AddressId && shippingAddressToSave.AddressId != Guid.Empty)
                    {
                        request.AddressId = shippingAddressToSave.AddressId;
                    }
                    if (!string.IsNullOrEmpty(shippingAddressToSave.CustomerId))
                    {
                        request.CustomerId = shippingAddressToSave.CustomerId;
                    }
                    request.HasAddressRestriction =  HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction ;
                    var response = proxy.InsertShippingAddress(new InsertShippingAddressRequest1(request)).InsertShippingAddressResult as InsertShippingAddressResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        if (null != sessionInfo)
                        {
                            var refreshed = loadShippingAddressesFromService(distributorID, locale);
                            refreshed.AddRange(tempShippingAddresses);
                            saveShippingAddressesToCache(distributorID, locale, refreshed);
                        }
                        return response.ID;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("SaveShippingAddress error: DS {0}, , IsShippingAddresstoSaveNull:{1},error: {2}", distributorID,(shippingAddressToSave==null)?"null":shippingAddressToSave.AddressId.ToString() ,ex));
                }
            }
            return 0;
        }

        public int SaveOrderShippingAddressToDB(string distributorID,
                                                string locale,
                                                ShippingAddress_V02 shippingAddressToSave,
                                                bool addrNoChanged,
                                                bool bCheckNickname,
                                                int shoppingCartID)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo == null) return 0;

            try
            {
                var cachedAddresses = sessionInfo.OrderShippingAddresses;
                var tempShippingAddresses = new List<ShippingAddress_V02>();
                if (cachedAddresses != null)
                {
                    if (cachedAddresses.Count > 0)
                    {
                        tempShippingAddresses = (from s in cachedAddresses where s.ID < 0 select s).ToList();
                        var tempAddressToBeDeleted = tempShippingAddresses.Find(s => s.ID == shippingAddressToSave.ID);
                        if (tempAddressToBeDeleted != null)
                            tempShippingAddresses.Remove(tempAddressToBeDeleted);
                    }
                }

                if (string.IsNullOrEmpty(distributorID) || shippingAddressToSave == null)
                {
                    return 0;
                }
                else
                {
                    try
                    {
                        var proxy = ServiceClientProvider.GetShippingServiceProxy();
                        var request = new InsertShippingAddressRequest_V03
                            {
                                Address = shippingAddressToSave.Address,
                                Alias = shippingAddressToSave.Alias,
                                DistributorID = distributorID,
                                PhoneNumber = shippingAddressToSave.Phone,
                                AltPhoneNumber = shippingAddressToSave.AltPhone,
                                IsPrimary = shippingAddressToSave.IsPrimary,
                                ID = shippingAddressToSave.ID,
                                Recipient = shippingAddressToSave.Recipient,
                                FirstName = shippingAddressToSave.FirstName,
                                LastName = shippingAddressToSave.LastName,
                                MiddleName = shippingAddressToSave.MiddleName,
                                UseOrdeShippingAddressTable = true,
                                ShoppingCartID = shoppingCartID
                            };

                        var response = proxy.InsertShippingAddress(new InsertShippingAddressRequest1(request)).InsertShippingAddressResult as InsertShippingAddressResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            var refreshed = loadShippingAddressesFromService(distributorID, locale, true);
                            refreshed.AddRange(tempShippingAddresses);
                            SaveOrderShippingAddressesToCache(distributorID, locale, refreshed);

                            return response.ID;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("SaveOrderShippingAddressToDB error: DS {0}, error: {1}", distributorID, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("SaveOrderShippingAddressToDB error: DS {0}, error: {1}", distributorID, ex));
            }

            return 0;
        }

        public List<ShippingAddress_V02> GetDSShippingAddresses(string distributorID, string locale)
        {
            try
            {
                if (null != HttpContext.Current && null == HttpContext.Current.Session)  //Session is null when called from Mobile web api
                {
                    var cacheKey = string.Format("AllAddress_Mobile_{0}_{1}", distributorID, locale);
                    return _cache.Retrieve(_ => getShippingAddressesFromCache(distributorID, locale), cacheKey,
                            TimeSpan.FromMinutes(20));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex, "Get Address from simple cache failed ");
            }

            return getShippingAddressesFromCache(distributorID, locale);
        }

        private List<ShippingAddress_V02> getShippingAddressesFromCache(string distributorID,
                                                                        string locale,
                                                                        bool orderShippingAddress = false)
        {
            List<ShippingAddress_V02> result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }

            // cache is  messed up and gives random result. sometimes missing  addresses. page refresh or clicking on save and make primarybuttons could reproduce the issue. defect 25324

            // tries to get object from cache
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (null != sessionInfo)
                result = orderShippingAddress ? sessionInfo.OrderShippingAddresses : sessionInfo.ShippingAddresses;

            var tempAddresses = orderShippingAddress
                                    ? (sessionInfo == null || sessionInfo.OrderShippingAddresses == null
                                           ? new List<ShippingAddress_V02>()
                                           : sessionInfo.OrderShippingAddresses.FindAll(oi => oi.ID < 0))
                                    : (sessionInfo == null || sessionInfo.ShippingAddresses == null
                                           ? new List<ShippingAddress_V02>()
                                           : sessionInfo.ShippingAddresses.FindAll(oi => oi.ID < 0));

            if (null == result || !string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = loadShippingAddressesFromService(distributorID, locale, orderShippingAddress);
                    // saves to cache is successful
                    if (null != result)
                    {
                        if (tempAddresses!=null && tempAddresses.Count > 0 && !string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                        {
                            result.AddRange(tempAddresses);
                        }
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            result = result.Where(x =>
                                                  !x.Address.Line4.StartsWith("MALO") &&
                                                  !x.Address.Line4.StartsWith("DE") &&
                                                  !x.Address.Line4.StartsWith("MAEN") &&
                                                  !x.Address.Line4.StartsWith("PALO") &&
                                                  !x.Address.Line4.StartsWith("PAEN")).ToList();
                        }
                        if (orderShippingAddress)
                        {
                            if (sessionInfo != null) sessionInfo.OrderShippingAddresses = result; //Stash in Session now
                        }
                        else
                        {
                            if (sessionInfo != null) sessionInfo.ShippingAddresses = result; //Stash in Session now

                        }

                    }
                    else
                    {
                        result = new List<ShippingAddress_V02>();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            // Defect: 14426 - Temporarily we are restricting English addresses for now.It should be reverted back and we have to restrict this in Shipping service for China
            else
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    result = result.Where(x =>
                                          !x.Address.Line4.StartsWith("MALO") &&
                                          !x.Address.Line4.StartsWith("DE") &&
                                          !x.Address.Line4.StartsWith("MAEN") &&
                                          !x.Address.Line4.StartsWith("PALO") &&
                                          !x.Address.Line4.StartsWith("PAEN")).ToList();
                    if (orderShippingAddress)
                    {
                        sessionInfo.OrderShippingAddresses = result; //Stash in Session now
                    }
                    else
                    {
                        sessionInfo.ShippingAddresses = result; //Stash in Session now
                    }
                }
            }

            return result;
        }

        private void saveShippingAddressesToCache(string distributorId,
                                                  string locale,
                                                  List<ShippingAddress_V02> addresses)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorId, locale);
            sessionInfo.ShippingAddresses = addresses;
        }

        private void SaveOrderShippingAddressesToCache(string distributorId,
                                                       string locale,
                                                       List<ShippingAddress_V02> addresses)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorId, locale);
            sessionInfo.OrderShippingAddresses = addresses;
        }

        /// <summary>
        ///     Load shipping address from service.
        /// </summary>
        /// <param name="distributorID">Distributor ID parameter.</param>
        /// <param name="locale">Locale parameter.</param>
        /// <param name="orderShippingAddress">True for OrderShippingAddress table usage.</param>
        /// <returns>List of addressed according the parameters.</returns>
        private List<ShippingAddress_V02> loadShippingAddressesFromService(string distributorID,
                                                                           string locale,
                                                                           bool orderShippingAddress = false)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }

            // List of addressed to return.
            List<ShippingAddress_V02> addrList;

            // Proxy initialization.
            var proxy = ServiceClientProvider.GetShippingServiceProxy();

            // Getting ship to countries
            var config = HLConfigManager.Configurations.CheckoutConfiguration;
            var countries = new List<string>(config.ShipToCountries.Split(new[] { ',' }));

            // Getting service response.
            var response =
                proxy.GetShippingAddress(new GetShippingAddressRequest1(new GetShippingAddressRequest_V03()
                {
                    ID = 0,
                    DistributorID = distributorID,
                    Countries = countries,
                    UseOrderShippingAddressTable = orderShippingAddress
                })).GetShippingAddressResult as
                GetShippingAddressResponse_V02;

            if (response != null && response.Status == ServiceResponseStatusType.Success &&
                response.AddressList != null)
            {
                if (response.AddressList.TryGetValue(distributorID, out addrList))
                {
                    foreach (ShippingAddress_V02 addr in addrList)
                    {
                        addr.Phone = addr.Phone ?? string.Empty;
                        addr.Alias = addr.Alias ?? string.Empty;
                        addr.DisplayName = string.IsNullOrEmpty(addr.Alias)
                                               ? GetAddressDisplayName(addr)
                                               : addr.Alias;
                    }
                    return addrList;
                }
            }

            // If we're here - we got nothing - Crawl to HMS-
            addrList = GetHMSPrimaryShippingAddress(distributorID, locale);
            if (countries.Count > 0 && addrList.Count > 0)
            {
                addrList = (from a in addrList
                            from c in countries
                            where a.Address.Country == c
                            select a).ToList<ShippingAddress_V02>();
            }
            if (addrList.Count > 0) return addrList;
            return null;
        }

        private List<ShippingAddress_V02> mergeAddressFromSession(List<ShippingAddress_V02> addressListFromDB,
                                                                  string distributorID,
                                                                  string locale)
        {            
            var newList = new List<ShippingAddress_V02>();
            if (addressListFromDB != null)
            {
                newList.AddRange(addressListFromDB);
            }

            var listFromSession = GetShippingAddressesFromSession(distributorID, locale);
            if (listFromSession != null)
            {
                newList.AddRange(listFromSession);
            }
            // order by alias first
            var list = (from a in newList
                            where string.IsNullOrEmpty(a.Alias) != true
                            orderby a.Alias
                            select a).ToList<ShippingAddress_V02>();
              // order items without alias next
                    list.AddRange((from a in newList
                                   where string.IsNullOrEmpty(a.Alias)
                                   orderby a.DisplayName
                                   select a).ToList<ShippingAddress_V02>());
                    return list;                         
        }

        private List<ShippingAddress_V02> GetHMSPrimaryShippingAddress(string distributorId, string locale)
        {
            CheckoutConfiguration config = HLConfigManager.Configurations.CheckoutConfiguration;
            var returnAddress = new List<ShippingAddress_V02>();
            if (config.InitialShippingAddressFromHMS)
            {
                //    DistributorOrder_pptClient proxy = new DistributorOrder_pptClient();
                //    proxy.Endpoint.Address = new System.ServiceModel.EndpointAddress(HL.Common.Configuration.Settings.GetRequiredAppSetting("HmsDistributorOrderUrl"));
                //    GetDSAddressRequest request = new GetDSAddressRequest();
                //    GetDSAddressResponse response = new GetDSAddressResponse();
                //    request.WSDistAddressRequest = new WSDistAddressRequestType();
                //    request.WSDistAddressRequest.AddressType = "SHIP_TO";
                //    request.WSDistAddressRequest.DistributorId = distributorId;
                //    request.WSDistAddressRequest.PrimaryFlag = "Y";
                //    request.WSDistAddressRequest.AddressStatus = "A";
                //    try
                //    {
                //        response.WSDistAddressResponse = proxy.GetDSAddress(request.WSDistAddressRequest);
                //        if (null != response.WSDistAddressResponse)
                //        {
                //            if (null != response.WSDistAddressResponse.AddressDetails
                //                && response.WSDistAddressResponse.AddressDetails.Length > 0
                //                && response.WSDistAddressResponse.ErrorDetails[0].ErrorCode == "0")
                //            {
                //                Address_V01 address = new Address_V01();
                //                address.City = response.WSDistAddressResponse.AddressDetails[0].City;
                //                address.Country = response.WSDistAddressResponse.AddressDetails[0].Country;
                //                address.CountyDistrict = response.WSDistAddressResponse.AddressDetails[0].County;
                //                address.Line1 = response.WSDistAddressResponse.AddressDetails[0].Address1;
                //                address.Line2 = response.WSDistAddressResponse.AddressDetails[0].Address2;
                //                address.Line3 = response.WSDistAddressResponse.AddressDetails[0].Address3;
                //                string name = response.WSDistAddressResponse.AddressDetails[0].Address4;
                //                address.PostalCode = response.WSDistAddressResponse.AddressDetails[0].PostalCode;
                //                if (address.PostalCode == null) address.PostalCode = string.Empty;
                //                address.StateProvinceTerritory = response.WSDistAddressResponse.AddressDetails[0].State;
                //                string phone = response.WSDistAddressResponse.AddressDetails[0].Phone;
                //                HttpContext context = HttpContext.Current;
                //                ShippingAddress_V02 theAddress = new ShippingAddress_V02(0, name, "", "", "", address, phone, phone, true, string.Empty, DateTime.Now);
                //                theAddress.Phone = phone;
                //                theAddress.Recipient = name;
                //                theAddress.DisplayName = GetAddressDisplayName(theAddress);
                //                returnAddress.Add(theAddress);
                //            }
                //            else
                //            {
                //                LoggerHelper.Error(string.Format("HMS Primary Shipping Address retrieval failed: DS{0}: Code{1}, Message{2}", distributorId, response.WSDistAddressResponse.ErrorDetails[0].ErrorCode, response.WSDistAddressResponse.ErrorDetails[0].ErrorMessage), "ShippingProvider");
                //            }
                //        }
                //        else
                //        {
                //            LoggerHelper.Error(string.Format("HMS Primary Shipping Address retrieval failed: DS{0}. The service call failed and returned Null", distributorId), "ShippingProvider");
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        LoggerHelper.Error(string.Format("HMS Primary Shipping Address retrieval failed: DS{0}: {1}", distributorId, ex.ToString()), "ShippingProvider");
                //    }
            }

            return returnAddress;
        }

        private Name_V01 MakeCareOfName(string name)
        {
            var theName = new Name_V01();
            var names = name.Split(new[] { ' ' });
            if (names.Length == 1)
            {
                theName.First = names[0];
            }
            else if (names.Length == 2)
            {
                theName.First = names[0];
                theName.Last = names[1];
            }
            else if (names.Length == 3)
            {
                theName.First = names[0];
                theName.Middle = names[1];
                theName.Last = names[2];
            }
            else
            {
                theName.First = name;
            }

            return theName;
        }

        private DeliveryOption getDeliveryOptionFromID(int deliveryOptionID,
                                                       DeliveryOptionType type,
                                                       List<DeliveryOption> deliveryOptions,
                                                       OrderCategoryType orderCategoryType)
        {
            if (deliveryOptions != null && deliveryOptions.Count > 0)
            {
                if (deliveryOptionID != 0)
                {
                    return deliveryOptions.Find(x => x.Id == deliveryOptionID && x.Option == type);
                }                
                    var options = deliveryOptions.Where(x => x.Option == type && x.OrderCategory.ToString() == orderCategoryType.ToString());
                    if (options!=null && options.Count() > 0)
                    {
                        return options.First();
                    }               
            }
            return null;
        }

        public string[] GetFreightCodeAndWarehouseFromAddress(ShippingAddress_V02 shippingAddress,
                                                              DeliveryOption shippingOption)
        {
            if (shippingAddress != null)
            {
                var freightCodeAndWarehouse = GetFreightCodeAndWarehouse(shippingAddress);

                //freight code
                if (freightCodeAndWarehouse != null && freightCodeAndWarehouse.Length > 0 &&
                    string.IsNullOrEmpty(freightCodeAndWarehouse[0]))
                {
                    if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                    // if not user select
                    {
                        freightCodeAndWarehouse[0] = shippingOption != null ? shippingOption.FreightCode : string.Empty;
                    }
                    if (string.IsNullOrEmpty(freightCodeAndWarehouse[0]))
                        freightCodeAndWarehouse[0] =
                            HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                }
                //warehouse
                if (freightCodeAndWarehouse != null && freightCodeAndWarehouse.Length > 1 && 
                    string.IsNullOrEmpty(freightCodeAndWarehouse[1]))
                {
                    freightCodeAndWarehouse[1] = shippingOption != null ? shippingOption.WarehouseCode : string.Empty;
                    if (string.IsNullOrEmpty(freightCodeAndWarehouse[1]))
                        freightCodeAndWarehouse[1] =
                            HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                }
                return freightCodeAndWarehouse;
            }
            return null;
        }

        private static List<ShippingAddress_V02> GetShippingAddressesFromDeliveryOptions(
            List<DeliveryOption> deliveryOptions)
        {
            var results = new List<ShippingAddress_V02>();
            foreach (DeliveryOption option in deliveryOptions)
            {
                results.Add(option);
            }

            return results;
        }

        private static List<DeliveryOption> getDeliveryOptionsFromService(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return null;
            }
            else
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var response =
                    proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V02() { CountryCode = countryCode })).GetDeliveryPickupAlternativesResult as
                    DeliveryPickupAlternativesResponse_V02;
                if (response != null && response.Status == ServiceResponseStatusType.Success &&
                    response.DeliveryPickupAlternatives != null)
                {
                    var deliveryOptions = new List<DeliveryOption>();
                    foreach (DeliveryPickupOption_V02 dpo in response.DeliveryPickupAlternatives)
                    {
                        var deliveryOption = new DeliveryOption(dpo);
                        if (!string.IsNullOrEmpty(dpo.State))
                        {
                            deliveryOption.State = dpo.State.Trim();
                        }
                        else
                        {
                            if (dpo.PickupAddress != null && dpo.PickupAddress.Address != null &&
                                dpo.PickupAddress.Address.StateProvinceTerritory != null)
                            {
                                deliveryOption.State = dpo.PickupAddress.Address.StateProvinceTerritory.Trim();
                            }
                        }

                        if (!string.IsNullOrEmpty(dpo.WarehouseCode))
                        {
                            deliveryOption.WarehouseCode = dpo.WarehouseCode;
                        }
                        else
                        {
                            if (dpo.ShippingSource != null)
                                deliveryOption.WarehouseCode = dpo.ShippingSource.Warehouse;
                        }
                        deliveryOptions.Add(deliveryOption);
                    }

                    if (Settings.GetRequiredAppSetting("LogShipping", "false").ToLower() == "true")
                    {
                        LogRequest(string.Format("GetDeliveryOptionsFromService: {0}",
                                                 OrderCreationHelper.Serialize(deliveryOptions)));
                    }

                    return deliveryOptions;
                }
            }
            return null;
        }

        private static List<DeliveryOption> getDeliveryOptionsFromCache(string countryCode)
        {
            List<DeliveryOption> result = null;

            if (string.IsNullOrEmpty(countryCode))
            {
                return result;
            }

            // gets cache key
            string cacheKey = getDeliveryOptionsCacheKey(countryCode);

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;

            if (null == result)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = getDeliveryOptionsFromService(countryCode);
                    // saves to cache is successful
                    if (null != result)
                    {
                        saveDeliveryOptionsFromCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static void saveDeliveryOptionsFromCache(string cacheKey, List<DeliveryOption> shippings)
        {
            if (shippings != null)
            {
                HttpRuntime.Cache.Insert(cacheKey,
                                         shippings,
                                         null,
                                         DateTime.Now.AddMinutes(ShippingCacheMinutes),
                                         Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal,
                                         null);
            }
        }

        private static void onShippingInfoCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:

                    try
                    {
                        string serviceKey = key.Replace(DELIVERY_OPTIONS_CACHE_PREFIX, string.Empty);
                        var result = getDeliveryOptionsFromService(serviceKey);

                        if (result != null)
                        {
                            // if success replace cache with new resultset
                            saveDeliveryOptionsFromCache(key, result);
                        }
                        else
                        {
                            // if failure re-insert from cache
                            saveDeliveryOptionsFromCache(key, (List<DeliveryOption>)sender);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    }

                    break;
                case CacheItemRemovedReason.Removed:
                case CacheItemRemovedReason.DependencyChanged:
                default:
                    break;
            }
        }

        private static string getDeliveryOptionsCacheKey(string countryCode)
        {
            return DELIVERY_OPTIONS_CACHE_PREFIX + countryCode;
        }

        //public virtual List<string> GetAllStates() { return null; }
        //public virtual List<string> GetMunicipalitiesForState(string state) { return null; }
        //public virtual List<string> GetColoniesForMunicipalityAndState(string state, string municipality) { return null; }
        //public virtual string LookupZipCode(string state, string municipality, string colony) { return null; }

        public virtual string GetShippingInstructions(ShippingInfo currentShippingInfo)
        {
            return null;
        }

        public virtual string GetCourierName(DeliveryOption currentShippingInfo)
        {
            return null;
        }

        protected string getPickupLocationPreferenceKey(string distributorID, string country)
        {
            return string.Format("{0}_{1}_{2}", distributorID, country, PICKUPLOC_PREFERENCE_CACHE_PREFIX);
        }

        private string getSessionPickupLocationPreferenceKey(string distributorID, string country)
        {
            return string.Format("{0}_{1}_{2}", distributorID, country, SESSION_PICKUPLOC_PREFERENCE_CACHE_PREFIX);
        }

        protected List<PickupLocationPreference_V01> getPickupLocationsPreferencesFromCache(string distributorID,
                                                                                            string country)
        {
            var result = new List<PickupLocationPreference_V01>();
            if (string.IsNullOrEmpty(distributorID) || string.IsNullOrEmpty(country))
            {
                return result;
            }

            // gets cache key
            string cacheKey = getPickupLocationPreferenceKey(distributorID, country);
            var session = HttpContext.Current.Session;

            if (null == session) // under mobile api context, session is null
            {
                result = null;
            }
            else
            {
                // tries to get object from cache
                result = session[cacheKey] as List<PickupLocationPreference_V01>;
            }

            if (null == result)
            {
                try
                {
                    result = loadPickupLocationsPreferencesFromService(distributorID, country);
                    if (result != null && result.Count > 0 && session != null) // don't bother with Session if running under mobile api context
                    {
                        session[cacheKey] = result;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetPickupLocationsPreferences error: DS {0}, error: {1}, country: {2}",
                                      distributorID, ex, country));
                    return result;
                }
            }
            return result;
        }

        protected List<PickupLocationPreference_V02> GetPickupLocationsPreferencesFromCache(string distributorID,
                                                                                            string country,
                                                                                            string locationType)
        {
            var result = new List<PickupLocationPreference_V02>();
            if (string.IsNullOrEmpty(distributorID) || string.IsNullOrEmpty(country))
            {
                return result;
            }

            // gets cache key
            string cacheKey = getPickupLocationPreferenceKey(distributorID, country);
            var session = HttpContext.Current.Session;

            // tries to get object from cache
            result = session[cacheKey] as List<PickupLocationPreference_V02>;

            if (result == null)
            {
                try
                {
                    result = LoadPickupLocationsPreferencesFromService(distributorID, country, locationType);
                    if (result != null)
                    {
                        session[cacheKey] = result;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetPickupLocationsPreferences error: DS {0}, error: {1}, country: {2}",
                                      distributorID, ex, country));
                    return result;
                }
            }
            return result;
        }

        public virtual void PickupLocationsPreferencesLoaded(string distributorID, GetPickupLocationsPreferencesResponse prefs)
        {
        }

        private List<PickupLocationPreference_V01> loadPickupLocationsPreferencesFromService(string distributorID,
                                                                                             string country)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var response =
                    proxy.GetPickupLocationsPreferences(new GetPickupLocationsPreferencesRequest1(new GetPickupLocationsPreferencesRequest_V01()
                    {
                        DistributorID = distributorID,
                        Country = country
                    })).GetPickupLocationsPreferencesResult as
                    GetPickupLocationsPreferencesResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success &&
                    response.PickupLocationPreferences != null)
                {
                    PickupLocationsPreferencesLoaded(distributorID, response);
                    return response.PickupLocationPreferences;
                }
            }
            return null;
        }

        private List<PickupLocationPreference_V02> LoadPickupLocationsPreferencesFromService(string distributorID,
                                                                                             string country,
                                                                                             string locationType)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var response =
                    proxy.GetPickupLocationsPreferences(new GetPickupLocationsPreferencesRequest1(new GetPickupLocationsPreferencesRequest_V02()
                    {
                        DistributorID = distributorID,
                        Country = country,
                        PickupLocationType = locationType
                    })).GetPickupLocationsPreferencesResult as
                    GetPickupLocationsPreferencesResponse_V02;
                if (response != null && response.Status == ServiceResponseStatusType.Success &&
                    response.PickupLocationPreferences != null)
                {
                    PickupLocationsPreferencesLoaded(distributorID, response);
                    return response.PickupLocationPreferences;
                }
            }
            return null;
        }

        private void updatePickupPreferenceCache(string distributorID,
                                                 string country,
                                                 int pickupLocationID,
                                                 string pickupLocationNickname,
                                                 string pickupLocationName,
                                                 bool isPrimary,
                                                 int id)
        {
            var pickupPreferences = getPickupLocationsPreferencesFromCache(distributorID, country);
            if (pickupPreferences == null)
            {
                pickupPreferences = new List<PickupLocationPreference_V01>();
            }

            if (id == -1)
            {
                var temp = (from p in pickupPreferences where p.ID == -1 select p).ToList();
                if (null != temp && temp.Count > 0)
                {
                    pickupPreferences.Remove(temp[0]);
                }
            }

            if (isPrimary)
            {
                var primaryPref = pickupPreferences.Find(p => p.IsPrimary);
                if (primaryPref != null)
                {
                    pickupPreferences.Find(p => p.IsPrimary).IsPrimary = false;
                }
            }

            var pickupPreference = new PickupLocationPreference_V01();
            pickupPreference.Country = country;
            pickupPreference.DistributorID = distributorID;
            pickupPreference.PickupLocationID = pickupLocationID;
            pickupPreference.PickupLocationNickname = pickupLocationNickname;
            pickupPreference.IsPrimary = isPrimary;
            pickupPreference.ID = id;

            if (!pickupPreferences.Exists(e => e.ID == id))
                pickupPreferences.Add(pickupPreference);
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session[getPickupLocationPreferenceKey(distributorID, country)] = pickupPreferences;
        }

        private void UpdatePickupPreferenceCache(string distributorID,
                                         string country,
                                         int pickupLocationID,
                                         string pickupLocationNickname,
                                         string pickupLocationName,
                                         bool isPrimary,
                                         int id, string courierType)
        {
            var pickupPreferences = GetPickupLocationsPreferencesFromCache(distributorID, country, courierType);
            if (pickupPreferences == null)
            {
                pickupPreferences = new List<PickupLocationPreference_V02>();
            }

            if (id == -1)
            {
                var temp =
                    (from p in pickupPreferences where p.ID == -1 && p.PickupLocationType == courierType select p)
                        .ToList();
                if (temp.Any())
                {
                    pickupPreferences.Remove(temp[0]);
                }
            }

            if (isPrimary)
            {
                var primaryPref = pickupPreferences.Find(p => p.IsPrimary);
                if (primaryPref != null)
                {
                    pickupPreferences.Find(p => p.IsPrimary).IsPrimary = false;
                }
            }

            var pickupPreference = new PickupLocationPreference_V02();
            pickupPreference.Country = country;
            pickupPreference.DistributorID = distributorID;
            pickupPreference.PickupLocationID = pickupLocationID;
            pickupPreference.PickupLocationNickname = pickupLocationNickname;
            pickupPreference.IsPrimary = isPrimary;
            pickupPreference.ID = id;
            pickupPreference.PickupLocationType = courierType;

            if (!pickupPreferences.Exists(e => e.ID == id && e.PickupLocationType == courierType))
                pickupPreferences.Add(pickupPreference);

            HttpContext.Current.Session[getPickupLocationPreferenceKey(distributorID, country)] = pickupPreferences;
        }

        public virtual string formatPhone(string phone)
        {
            if (phone != null)
            {
                bool start = phone.StartsWith("-");
                if (start) phone = phone.Remove(0, 1);

                bool end = phone.EndsWith("-");
                if (end) phone = phone.Remove(phone.Length - 1, 1);
            }

            return phone;
        }

        public virtual bool ShouldRecalculate(string oldFreightCode,
                                              string newFreightCode,
                                              Address_V01 oldaddress,
                                              Address_V01 newaddress)
        {
            return oldFreightCode != newFreightCode;
        }

        public virtual string GetAddressDisplayName(ShippingAddress_V02 address)
        {
            if ((address.Alias != null) && (address.Alias != string.Empty))
                return address.Alias;
            else
            {
                string displayAddress = string.Empty;
                if (!String.IsNullOrEmpty(address.FirstName) && !String.IsNullOrEmpty(address.LastName))
                {
                    displayAddress = string.Format("...{0},{1},{2},{3},{4} {5}", address.FirstName, address.LastName,
                                                   address.Address.Line1, address.Address.City,
                                                   address.Address.StateProvinceTerritory, address.Address.PostalCode);
                }
                else
                {
                    displayAddress = string.Format("...{0},{1},{2},{3} {4}", address.Recipient, address.Address.Line1,
                                                   address.Address.City, address.Address.StateProvinceTerritory,
                                                   address.Address.PostalCode);
                }
                if (displayAddress.IndexOf(",,") > -1)
                {
                    return displayAddress.Replace(",,,", ",").Replace(",,", ",");
                }
                else
                {
                    return displayAddress;
                }
            }
        }

        public virtual bool ValidateAddress(ShippingAddress_V02 address)
        {
            return true;
        }

        public virtual bool ValidateAddress(ShippingAddress_V02 address, out string errorCode, out ServiceProvider.AddressValidationSvc.Address avsAddress)
        {
            errorCode = string.Empty;
            avsAddress = null;

            return true;
        }

        public virtual bool ValidateShipping(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart != null &&
                shoppingCart.DeliveryInfo != null &&
                !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode))
                return true;
            return false;
        }

        public virtual bool ValidateAddress(MyHLShoppingCart shoppingCart)
        {
            return true;
        }

        public virtual string FormatShippingAddress(ShippingAddress_V01 address,
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
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
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

        public virtual bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            return false;
        }

        public virtual void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
        }

        public virtual bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.SubmitOrderBTSvc.Shipment shippment)
        {
            return false;
        }

        public virtual string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string name = string.Empty;
            if (!String.IsNullOrEmpty(address.Alias))
                name = address.Alias;
            else
                name = address.Recipient;

            string formattedAddress = string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}<br>{7}",
                                                    name,
                                                    address.Address.Line1,
                                                    address.Address.Line2 ?? string.Empty,
                                                    address.Address.City,
                                                    address.Address.CountyDistrict ?? string.Empty,
                                                    address.Address.PostalCode,
                                                    address.Address.StateProvinceTerritory,
                                                    formatPhone(address.Phone));
            formattedAddress = formattedAddress.Replace("<br><br><br>", "<br>");
            return formattedAddress.Replace("<br><br>", "<br>");
        }

        public virtual string GetShippingInstructionsForShippingInfo(ServiceProvider.CatalogSvc.ShoppingCart_V02 cart,
                                                                     MyHLShoppingCart shoppingCart,
                                                                     string distributorID,
                                                                     string locale)
        {
            return String.Empty;
        }

        public virtual string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                           string distributorID,
                                                           string locale)
        {
            string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                     ? string.Empty
                                     : shoppingCart.DeliveryInfo.Instruction;
            if (shoppingCart.DeliveryInfo.Address != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    return
                        instruction =
                        string.Format("{0},{1}", shoppingCart.DeliveryInfo.Address.Recipient,
                                      shoppingCart.DeliveryInfo.Address.Phone);
                }
            }
            return instruction;
        }


        public virtual int GetAllowPickupDays(DateTime date)
        {
            return HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowDaysPickUp;
        }

        public virtual bool ValidatePickupInstructionsDate(DateTime date)
        {
            return true;
        }

        public virtual bool ValidateShippingInstructionsDate(DateTime date)
        {
            return true;
        }

        public virtual List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                              string locale,
                                                                              ShippingAddress_V01 address)
        {
            var lstOptions = new List<DeliveryOption>();
            return lstOptions;
        }

        public virtual List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                      string locale,
                                                                      MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingAddress_V01 address)
        {
            var lstOptions = new List<DeliveryOption>();
            return lstOptions;
        }

        public virtual List<DeliveryOption> GetDeliveryOptionsListForMobileShipping(string Country,
            string locale,
            ShippingAddress_V01 address, string memberId)
        {
            var lstOptions = new List<DeliveryOption>();
            return lstOptions;

        }

        public virtual List<DeliveryOption> GetDeliveryOptionsListForPickup(string country, string locale,
                                                                            ShippingAddress_V01 address)
        {
            return new List<DeliveryOption>();
        }

        public virtual bool NeedEnterAddress(string distributorID, string locale)
        {
            var shippingAddresses = GetShippingAddresses(distributorID, locale);
            return (shippingAddresses == null || shippingAddresses.Count == 0);
        }

        public virtual string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            return new[] { string.Empty, string.Empty };
        }

        public virtual void SetShippingInfo(ServiceProvider.CatalogSvc.ShoppingCart_V01 cart)
        {
        }

        public virtual List<ServiceProvider.OrderSvc.InvoiceHandlingType> GetInvoiceOptions(ShippingAddress_V01 address,
                                                                   List<ServiceProvider.CatalogSvc.CatalogItem_V01> cartItems,
                                                                   ServiceProvider.CatalogSvc.ShoppingCart_V01 cart)
        {
            var shoppingCart = cart as MyHLShoppingCart;
            var listInvoiceOptions = new List<InvoiceHandlingType>();
            string country = shoppingCart.CountryCode;

            if (NonInvoiceOptionsCountries.Contains(country.ToUpper().Trim()))
            {
                return listInvoiceOptions;
            }

            var myhlCart = cart as MyHLShoppingCart;

            if (myhlCart == null)
            {
                return listInvoiceOptions;
            }

            if (myhlCart.CustomerOrderDetail != null)
            {
                var customerInvoiceOption =
                    (InvoiceHandlingType)
                    System.Enum.Parse(typeof (InvoiceHandlingType),
                                      HLConfigManager.Configurations.CheckoutConfiguration.DefaultCustomerInvoiceOption);
                listInvoiceOptions.Add(customerInvoiceOption);
                return listInvoiceOptions;
            }

            if (myhlCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
            {
                switch (country)
                {
                    case "US":
                    case "PR":
                    case "TT":
                    case "CA":
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                            break;
                        }
                    case "JM":
                    case "TH":
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            break;
                        }
                    case "AT":
                    case "IT":
                    case "CH":
                    case "DE":
                    case "DK":
                    case "ES":
                    case "GB":
                    case "NL":
                    case "NO":
                    case "PT":
                    case "SE":
                    case "AU":
                    case "VE":
                    case "CO":
                    case "FI":
                    case "IE":
                    case "PE":
                    case "CL":
                    case "UY":
                    case "PY":
                    case "RU":
                    case "FR":
                    case "BE":
                    case "PL":
                    case "TR":
                    case "IS":
                    case "GR":
                    case "IL":
                    case "RO":
                    case "BR":
                    case "CR":
                    case "ZA":
                    case "EC":
                    case "BO":
                    case "GT":
                    case "PA":
                    case "SV":
                    case "NI":
                    case "ID":
                    case "SG":
                    case "HN":
                    case "PH":
                    case "HU":
                    case "SR":
                    case "UA":
                    case "VN":
                    case "RS":
                    case "DO":
                    case "BA":
                    case "BY":
                    case "KZ":
                    case "MN":
                    case "CZ":
                    case "SK":
                    case "BG":
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            break;
                        }
                    case "AR":
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.ElectronicInvoice);
                            break;
                        }
                    case "MK":
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            break;
                        }
                    default:
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                            break;
                        }
                }
                return listInvoiceOptions;
            }

            if(myhlCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
            {
                switch (country)
                {
                    case "US":
                    case "CA":
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                }
                return listInvoiceOptions;
            }

            switch (country)
            {
                case "CA":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "TH":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                case "DE":
                    {                       
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "DK":
                    {
                        int postCode = 0;
                        bool success = int.TryParse(myhlCart.DeliveryInfo.Address.Address.PostalCode, out postCode);
                        if (success)
                        {
                            if (postCode >= 100 && postCode <= 970)
                            {
                                //Faroe Island
                                listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            }
                            else if (postCode.ToString().StartsWith("39"))
                            {
                                //Greenland
                                listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            }
                            else
                            {
                                //Default for EMEA
                                listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                                listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            }
                        }
                        else
                        {
                            //Default for EMEA
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        break;
                    }
                case "GB":
                    {
                        listInvoiceOptions = GetInvoiceOptionsForGB(listInvoiceOptions, myhlCart);
                        break;
                    }
                case "FI":
                    {
                        if (myhlCart.DeliveryInfo != null && myhlCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        else
                        {
                            int postCode = 0;
                            bool success = int.TryParse(myhlCart.DeliveryInfo.Address.Address.PostalCode, out postCode);
                            if (success)
                            {
                                /// “Åland”
                                if (postCode >= 22000 && postCode <= 22999)
                                {
                                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                                }
                                else
                                {
                                    //Default for EMEA
                                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                                }
                            }
                        }
                        break;
                    }
                case "IN":
                case "PE":
                case "VE":
                case "CL":
                case "GR":
                case "BR":
                case "CR":
                case "EC":
                case "SV":
                case "PA":
                case "GT":
                case "NI":
                case "HN":
                case "VN":
                case "DO":
                case "HR":
                case "BA":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "MY":
                case "BO":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                case "NL":
                    {
                        if (myhlCart.DeliveryInfo.FreightCode == "PU")
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        }
                        else
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        break;
                    }
                case "NO":
                    {
                        if (myhlCart.DeliveryInfo != null && myhlCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        else
                        {
                            int postCode = 0;
                            bool success = int.TryParse(myhlCart.DeliveryInfo.Address.Address.PostalCode, out postCode);
                            if (success)
                            {
                                if (postCode >= 9170 && postCode <= 9179)
                                {
                                    //Svalbard
                                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                                }
                                else
                                {
                                    //Default for NO
                                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                                }
                            }
                            else
                            {
                                //Default for NO
                                listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            }
                        }
                        break;
                    }
                case "IT":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        if (!string.IsNullOrEmpty(myhlCart.OrderSubType) &&
                            !"A1:B1".Contains(myhlCart.OrderSubType.Trim()))
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        break;
                    }
                case "AU":
                    {
                        if (myhlCart.DeliveryInfo != null && myhlCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HideWireForSpecialWhLocations
                    && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialWhlocations.Contains(myhlCart.DeliveryInfo.WarehouseCode))
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        }
                        else
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        }
                        break;
                    }
                case "PT":
                    if (myhlCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier ||
                        myhlCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    }
                    else
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                    break;
                case "TR": if (HLConfigManager.Configurations.CheckoutConfiguration.DefaultCustomerInvoiceOption != "SendToDistributor")
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                else
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                
                case "AT":
                case "CH":
                case "ES":
                case "IE":
                case "JP":
                case "MX":
                case "SE":
                    if (myhlCart.DeliveryInfo != null && myhlCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    }
                    else
                    {                      
                            listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);                      
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    }
                    break;
                case "TW":
                case "ZA":
                case "RU":
                case "BE":
                case "PL":
                case "IS":
                case "SP":
                case "SG":
                case "HK":
                case "HU":
                case "CO":
                case "PF":
                case "BY":
                case "CZ":
                case "BG":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                case "SI":
                    {
                        List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        if (tins != null && tins.Any(t => t.IDType.Key == "SITX"))
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        }
                        break;
                    }
                case "KR":
                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                    break;
                case "FR":
                    {
                        int postalCode;
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                        {
                            if (int.TryParse(address.Address.PostalCode, out postalCode))
                            {
                                if (postalCode < 97000)
                                {
                                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                                }
                            }
                        }
                        else
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        }
                        break;
                    }
                case "US":
                case "JM":
                case "TT":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "ID":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        break;
                    }
                case "AR":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.ElectronicInvoice);
                        break;
                    }
                case "IL":
                case "RO":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                case "SR":
                case "PY":
                case "UY":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "MO":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                case "PR":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
                case "UA":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    }
                    break;
                case "MK":
                    {
                        List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                        TaxIdentification tid = null;
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        if ((tid = tins.Find(t => t.IDType.Key == "MKTX")) != null)
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        }
                        break;
                    }
                case "RS":
                    {
                        List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                        TaxIdentification tid = null;
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        if ((tid = tins.Find(t => t.IDType.Key == "SRTX")) != null)
                        {
                            listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        }
                        break;
                    }
                case "SK":
                case "KZ":
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        break;
                    }
                default:
                    {
                        listInvoiceOptions.Add(InvoiceHandlingType.RecycleInvoice);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToCustomer);
                        listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                        listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                        break;
                    }
            }

            if (APFDueProvider.containsOnlyAPFSku(myhlCart.ShoppingCartItems))
            {
                if (country != "IN" && country != "RO")
                {
                    if (listInvoiceOptions.Contains(InvoiceHandlingType.WithPackage))
                    {
                        listInvoiceOptions.Remove(InvoiceHandlingType.WithPackage);
                    }
                }
                if(country=="DE")
                {
                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                }
            }

            if (APFDueProvider.IsAPFSkuPresent(myhlCart.ShoppingCartItems))
            {
                if (country == "TW")
                {
                    listInvoiceOptions.Clear();
                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                }
            }

            return listInvoiceOptions;
        }

        private List<InvoiceHandlingType> GetInvoiceOptionsForGB(List<InvoiceHandlingType> listInvoiceOptions,
                                                                 MyHLShoppingCart myhlCart)
        {
            if (myhlCart.DeliveryInfo.Address != null && myhlCart.DeliveryInfo.Address.Address != null)
            {
                string postalCode = myhlCart.DeliveryInfo.Address.Address.PostalCode != null
                                        ? myhlCart.DeliveryInfo.Address.Address.PostalCode.ToUpper()
                                        : "";
                if (postalCode.StartsWith("GY") || postalCode.StartsWith("JE"))
                {
                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                }
                else
                {
                    listInvoiceOptions.Add(InvoiceHandlingType.SendToCustomer);
                    listInvoiceOptions.Add(InvoiceHandlingType.SendToDistributor);
                    listInvoiceOptions.Add(InvoiceHandlingType.WithPackage);
                }
            }

            return listInvoiceOptions;
        }

        #endregion IShippingProvider Members

        public virtual ShippingInfo GetShippingInfoForMobile(string distributorID,
                                                           string locale,
                                                           DeliveryOptionType type, MyHLShoppingCart shoppingCart)
        {
            ShippingInfo shippingInfo = null;
            return shippingInfo;
        }

        public virtual List<string> GetStreets(string country, string state, string district)
        {
            return null;
        }

        public virtual List<string> GetZipCodes(string country, string state, string district, string city)
        {
            return null;
        }

        public virtual List<string> GetZipsForStreet(string country, string state, string city, string street)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new ZipsForStreetRequest_V01();
                request.Country = country;
                request.State = state;
                request.City = city;
                request.Street = street;
                var response = proxy.GetZipsForStreet(new GetZipsForStreetRequest(request)).GetZipsForStreetResult as ZipsForStreetResponse_V01;

                return response.Zips;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetZipsForStreet error: Country {0}, error: {1}", country, ex));
            }
            return null;
        }

        public virtual string[] GetFreightCodeAndWarehouseForTaxRate(Address_V01 address)
        {
            return null;
        }

        public virtual List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                        string country,
                                                                                        string locale,
                                                                                        DeliveryOptionType deliveryType)
        {
            return GetPickupLocationsPreferences(distributorId, country);
        }

        public virtual List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                        string country, string locale,
                                                                                        DeliveryOptionType deliveryType,
                                                                                        string courierType)
        {
            return new List<PickupLocationPreference_V01>();
        }

        public virtual string GetDifferentHtmlFragment(string deliverymethodoption)
        {
            return string.Empty;
        }
        public virtual string GetDifferentHtmlFragment(string freightcode, ShippingAddress_V01 address)
        {
            return string.Empty;
        }
        public virtual string GetDifferentHtmlFragment(MyHLShoppingCart shoppingCart)
        {
            return string.Empty;
        }


        #region Methods for delivery estimated

        private string GetDeliveryEstimatedCacheKey(ShippingInfo shippingInfo)
        {
            return string.Format("{0}_{1}_{2}_{3}", shippingInfo.Address.Address.Country,
                                 shippingInfo.Address.Address.StateProvinceTerritory,
                                 shippingInfo.Address.Address.PostalCode,
                                 shippingInfo.FreightCode);
        }

        private void SaveDeliveryEstimatedToCache(ShippingInfo shippingInfo, DeliveryZipCodeRange_V01 estimated)
        {
            if (shippingInfo != null && shippingInfo.Address != null)
            {
                var cacheKey = GetDeliveryEstimatedCacheKey(shippingInfo);
                HttpRuntime.Cache.Insert(cacheKey, estimated.DeliveryDays.ToString(), null,
                                         DateTime.Now.AddMinutes(DELIVERY_TIME_ESTIMATED_CACHE_MINUTES),
                                         Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
        }

        private int? GetDeliveryTimeEstimatedFromCache(ShippingInfo shippingInfo, string locale)
        {
            if (shippingInfo == null || shippingInfo.Address == null)
            {
                return null;
            }

            int estimatedTime = -1;
            DeliveryZipCodeRange_V01 estimated = null;
            var cacheKey = GetDeliveryEstimatedCacheKey(shippingInfo);

            if (!String.IsNullOrEmpty(cacheKey))
            {
                var result = HttpRuntime.Cache[cacheKey] as string;
                if (string.IsNullOrEmpty(result))
                {
                    try
                    {
                        var resultFromService = GetDeliveryEstimateFromService(shippingInfo, locale);
                        if (resultFromService != null)
                        {
                            estimated = resultFromService.FirstOrDefault();
                            if (estimated != null)
                            {
                                estimatedTime = estimated.DeliveryDays;
                                SaveDeliveryEstimatedToCache(shippingInfo, estimated);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    }
                }
                else if (!int.TryParse(result, out estimatedTime))
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return estimatedTime;
        }

        private List<DeliveryZipCodeRange_V01> GetDeliveryEstimateFromService(ShippingInfo shippingInfo, string locale)
        {
            try
            {
                if (shippingInfo == null || shippingInfo.Address == null)
                {
                    return null;
                }

                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new DeliveryEstimateRequest_V01();
                request.Country = shippingInfo.Address.Address.Country;
                request.State = shippingInfo.Address.Address.StateProvinceTerritory;
                request.PostalCode = shippingInfo.Address.Address.PostalCode;
                request.FreightCode = shippingInfo.FreightCode;
                var response = proxy.GetDeliveryEstimate(new GetDeliveryEstimateRequest(request)).GetDeliveryEstimateResult as DeliveryEstimateResponse_V01;
                return response.DeliveryZipCodes;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetDeliveryEstimateFromService error: Locale {0}, error: {1}", locale, ex));
            }
            return null;
        }

        #endregion Methods for delivery estimated

        #region Method for Shipping Instructions Properties

        public virtual bool ShippingInstructionDateTodayNotAllowed()
        {
            return false;
        }

        #endregion

        #region IShippingProvider Members for delivery estimate

        public virtual int? GetDeliveryEstimate(ShippingInfo shippingInfo, string locale)
        {
            return GetDeliveryTimeEstimatedFromCache(shippingInfo, locale);
        }

        #endregion IShippingProvider Members for delivery estimate

        #region Methods for validating Customer Shipping Address
        public virtual bool AddressValidationRequired()
        {
            return false;
        }
        public virtual bool DSAddressAsShippingAddress()
        {
            return false;
        }
        public virtual bool CustomerAddressIsValid(string countrycode, ref ShippingAddress_V02 customerAddress)
        {
            return true;
        }
        #endregion

        public virtual bool HasAdditionalPickupFromCourier()
        {
            return false;
        }

        public virtual bool HasAdditionalPickup()
        {
            return false;
        }

        public virtual string GetCourierTypeBySelection(string itemSelected)
        {
            return string.Empty;
        }

        public virtual bool DisplayHoursOfOperation(DeliveryOptionType option)
        {
            switch (option)
            {
                case DeliveryOptionType.Pickup:
                    return true;
                default:
                    return false;
            }
        }

        public virtual string GetStateNameFromStateCode(string stateCode)
        {
            return string.Empty;
        }

        protected static void LogRequest(string logData)
        {
            Logger.SetLogWriter(new LogWriterFactory().Create(), false);
            var entry = new LogEntry { Message = logData };
            Logger.Write(entry, "SelectedShippingAddress");
        }

        public virtual bool ValidateTotalAmountForPaymentOption(string paymentOption, decimal TotalAmount)
        {
            var isValid = true;
            return isValid;
        }

        /// <summary>
        /// If an address is provided, returns false when using OLD address format that needs to be updated by the user
        /// </summary>
        /// <param name="sc">The shopping cart instance containing all shipping information</param>
        /// <returns>False when address format is not VALID and needs to be updated by the user. True when address is not set or matches NEW format.</returns>
        public virtual bool IsValidShippingAddress(MyHLShoppingCart sc)
        {
            if (sc != null && sc.DeliveryInfo != null &&
               sc.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
               sc.DeliveryInfo.Address != null && sc.DeliveryInfo.Address.Address != null)
            {
                return IsValidShippingAddress(sc.DeliveryInfo.Address.Address);
            }
            return true;
        }

        /// <summary>
        /// Validates that <paramref name="addr"/> complies with NEW Address Format rules
        /// </summary>
        /// <param name="addr">The Address being checked</param>
        /// <returns>TRUE if valid, FALSE otherwise</returns>
        protected virtual bool IsValidShippingAddress(Address_V01 addr)
        {
            return true; // implement on a provider basis to allow custom fields
        }

        public virtual string GetMapScript(ShippingMapType mapType)
        {
            return string.Empty;
        }

        public virtual int SavePickupLocation(InsertCourierLookupRequest_V01 request)
        {
            return 0;
        }

        public virtual string GetWarehouseFromShippingMethod(string freighcode, ShippingAddress_V01 address)
        {
            return string.Empty;
        }

        public virtual bool UpdatePrimaryPickupLocationPreference(int pickupLocationId)
        {
            return true;
        }
    }
}
