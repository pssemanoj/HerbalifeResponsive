using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Security;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using System.Globalization;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.AddressValidationSvc;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_US : ShippingProviderBase
    {
        private const string USStatesCacheKey = "States_US";
        private const int USStatesCacheMinutes = 60*24;
        private const string FedexLocationCacheKey = "DeliveryInfo_Fedex_US";
        private const int FedexCacheMinutes = 60 * 2;
        private const string FedExNickName = "FedEx Office";

        public override ShippingInfo GetShippingInfoForMobile(string distributorID,
                                                           string locale,
                                                           DeliveryOptionType type, MyHLShoppingCart shoppingCart)
        {
            ShippingInfo shippingInfo = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode, null);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.FirstOrDefault();

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
                                shippingInfo = new ShippingInfo(deliveryOption) { Id = 0 };
                                // Get freight and warehouse if there are not
                                shippingInfo.WarehouseCode = getWareHouseCode("en-US",
                                                                              shippingInfo.Address.Address
                                                                                          .StateProvinceTerritory,
                                                                              string.Empty);
                                if (string.IsNullOrEmpty(shippingInfo.FreightCode))
                                {
                                    var freights = GetDeliveryOptionsListForPickup("US", "en-US", null);
                                    var defFreight = freights.FirstOrDefault(o => o.IsDefault);
                                    if (defFreight != null)
                                    {
                                        shippingInfo.FreightCode = defFreight.FreightCode;
                                    }
                                }
                                shippingInfo.Address.AltPhone = shippingInfo.Address.Phone;
                                shippingInfo.Address.Phone = string.Empty;
                            }
                            //return shippingInfo;
                        }
                    }
                }
            }

            return shippingInfo;
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
                                deliveryOption = GetDeliveryOptionFromId(vPickupLocation.PickupLocationID,
                                                                         DeliveryOptionType.PickupFromCourier);
                            }
                            if (deliveryOption != null)
                            {
                                shippingInfo = new ShippingInfo(deliveryOption) {Id = deliveryOptionID};
                                // Get freight and warehouse if there are not
                                shippingInfo.WarehouseCode = getWareHouseCode("en-US",
                                                                              shippingInfo.Address.Address
                                                                                          .StateProvinceTerritory,
                                                                              string.Empty);
                                if (string.IsNullOrEmpty(shippingInfo.FreightCode))
                                {
                                    var freights = GetDeliveryOptionsListForPickup("US", "en-US", null);
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
                if (shippingInfo != null && shippingInfo.Address != null && type == DeliveryOptionType.Shipping)
                {
                    var lstOptions = GetDeliveryOptions(DeliveryOptionType.Shipping, shippingInfo.Address);
                    if (lstOptions != null)
                    {
                        if (lstOptions.Count > 0)
                        {
                            string stateName =
                                GetStateNameFromStateCode(
                                    shippingInfo.Address.Address.StateProvinceTerritory.Trim().ToUpper());
                            var option =
                                lstOptions.Find(
                                    delegate(DeliveryOption p)
                                        { return p.State.Trim().ToLower() == stateName.Trim().ToLower(); });
                            if (option != null)
                            {
                                shippingInfo.WarehouseCode = option.WarehouseCode;
                            }
                        }
                    }
                }
            }
            return shippingInfo;
        }

        public override List<string> GetStatesForCountry(string Country)
        {
            return base.GetStatesForCountry(Country);
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
            //address.CountyDistrict = addressToValidate.CountyDistrict;
            address.Line1 = shippingAddress.Address.Line1;
            address.Line2 = shippingAddress.Address.Line2;
            address.PostalCode = shippingAddress.Address.PostalCode;
            address.StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory;
            request.Address = address;
                        
            var proxy = ServiceClientProvider.GetAddressValidationServiceProxy();
            //ExceptionFix: adding trycatch for service call.
            ValidateAddressResponse response = null;
            try
            {
                response = proxy.ValidateAddress(new ValidateAddressRequest1(request)).ValidateAddressResponse as ValidateAddressResponse;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("The service 'AddressValidation_PortTypeClient' has an exception. Message: {0}, StackTrace: {1}, TargetSite:{2}", ex.Message, ex.StackTrace,ex.TargetSite));
            }
            
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

        //Properties/Methods used only for US Extravaganza 2014, will be removed after the events
        #region Extravaganza2014
        private List<string> NamCountries = new List<string>() { "PR", "CA", "JM", "US" };

        public bool isUsDS
        {
            get
            {
                var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                return user.Value.ProcessingCountryCode == "US";
            }
        }

        public bool isNamDS
        {
            get
            {
                var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                return NamCountries.Contains(user.Value.ProcessingCountryCode);
            }
        }

        #endregion

        #region President Summit 2015

        private const int PresidentSummitEventId = 1160;

        #endregion

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if(type == DeliveryOptionType.Pickup)
            {
                var list = base.GetDeliveryOptions(type, address);
                if (isUsDS || !isNamDS)
                {
                    if(list != null)
                    list = list.Where(x => x.Description != "Extravaganza").ToList();
                }

                // Check for President Summit 2015 pickup location
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName) && !DistributorOrderingProfileProvider.IsEventQualified(PresidentSummitEventId, Thread.CurrentThread.CurrentCulture.Name))
                {
                    if (list != null)
                    list = list.Where(x => x.Description != HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName).ToList();
                }

                return list;
            }
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                var courierOptions = GetDeliveryPickupAlternativesFromCache(address);
                return courierOptions;
            }
            if (type == DeliveryOptionType.Shipping)
            {
                var shippingOptions = base.GetDeliveryOptions(type, address);
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShipToZipCode && address != null && address.Address != null && address.Address.PostalCode != null)
                {
                    var shippingOptionsByZip = shippingOptions.Where(o => !string.IsNullOrEmpty(o.PostalCode) && address.Address.PostalCode.StartsWith(o.PostalCode)).ToList();
                    if (shippingOptionsByZip.Any())
                    {
                        return shippingOptionsByZip;
                    }
                }

                // If not delivery option by zip code or not configured in this way then get the regular rows
                shippingOptions = shippingOptions.Where(o => string.IsNullOrEmpty(o.PostalCode)).ToList();
                return shippingOptions;
            }
            return base.GetDeliveryOptions(type, address);
        }

        public override bool ValidateShipping(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart != null && null != shoppingCart.DeliveryInfo)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    if (shoppingCart.DeliveryInfo != null &&
                        !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                        !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode))
                        return true;
                }
                else
                {
                    if (!APFDueProvider.containsOnlyAPFSku(shoppingCart.CartItems))
                    {
                        if (shoppingCart.DeliveryInfo != null &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode) &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Recipient))
                            return true;
                    }
                    else
                    {
                        if (shoppingCart.DeliveryInfo != null &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode))
                            return true;
                    }
                }
            }
            return false;
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
                           ? string.Format("{0}<br>{1},{2}<br>{3} , {4} {5}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode)
                           : string.Format("{0},{1}<br>{2} , {3} {4}",
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode);
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                string message = HttpContext.GetGlobalResourceObject("MYHL_Rules", "ViewMap").ToString();
                string gAddress = string.Format("{0}+{1}+{2}+{3}", address.Address.Line1.Replace(" ", "+"), address.Address.City.Replace(" ", "+"), address.Address.StateProvinceTerritory.Replace(" ", "+"), address.Address.PostalCode.Replace(" ", "+"));
                return string.Format("{0}<br/>{1}, {2} {3} <br/> <a href='http://maps.google.com/?q={4}' target='_blank'>{5}</a>",
                                                 address.Address.Line1, address.Address.City,
                                                 address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode, gAddress, message);
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3} , {4}, {5}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
        }

        public override string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            string formattedAddress = string.Format("{0}<br>{1},{2}<br>{3} , {4} {5}<br>{6}",
                                                    address.Recipient ?? string.Empty,
                                                    address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                    address.Address.City, address.Address.StateProvinceTerritory,
                                                    address.Address.PostalCode,
                                                    formatPhone(address.Phone));

            return formattedAddress.Replace("<br><br>", "<br>");
        }

        public override string GetStateNameFromStateCode(string stateCode)
        {
            string stateName = string.Empty;
            var states = GetStatesForCountryFromCache("US");
            if (states != null && states.Count() > 0)
            {
                var state = states.Where(s => s.Code.Equals(stateCode));
                var stateInfo = state.FirstOrDefault();
                stateName = stateInfo != null ? stateInfo.Name : string.Empty;
            }
            return stateName;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && OrderType.ToString().Equals("HSO"))
            {
                request.Country = Country;
                request.Locale = locale;
                request.State = "HAP";

                var stateCode = address.Address.StateProvinceTerritory;
                var states = GetStatesForCountryFromCache(Country);
                if (states != null && states.Count() > 0)
                {
                    var state = states.Where(s => s.Code.Equals(stateCode));
                    var stateInfo = state.FirstOrDefault();

                    if (stateCode == "HI")
                    {
                        request.State += "-" + stateInfo.Name;
                    }
                    else if(stateInfo.IsTerritory)
                    {
                        request.State += "-Territory";
                    }
                }

                var responseHap = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                foreach (ShippingOption_V01 option in responseHap.DeliveryAlternatives)
                {
                    final.Add(new DeliveryOption(option));
                }
                return final;
            }

            
            if (IsPoBox(address))
            {
                var poBoxOption = new DeliveryOption();
                poBoxOption.FreightCode = "UST";
                poBoxOption.Description = "US Priority Mail";
                final.Add(poBoxOption);
                return final;
            }

            request.Country = Country;
            request.State = GetStateNameFromStateCode(address.Address.StateProvinceTerritory);
            request.Locale = locale;
            var response =
                proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                final.Add(new DeliveryOption(option));
            }
            return final;
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
                    LoggerHelper.Error(string.Format("GetDeliveryOptionsListForPickup error: Country: US, error: {0}",
                                                     ex.Message));
                }
            }
            return deliveryOptions;
        }

        public override bool ShouldRecalculate(string oldFreightCode,
                                               string newFreightCode,
                                               Address_V01 oldaddress,
                                               Address_V01 newaddress)
        {
            if (oldFreightCode != newFreightCode)
                return true;
            if (oldaddress == null || newaddress == null ||
                !oldaddress.StateProvinceTerritory.Equals(newaddress.StateProvinceTerritory))
                return true;
            return false;
        }

        private string getWareHouseCode(string locale, string stateCode, string zipCode)
        {
            var lstOptions = GetDeliveryOptions(locale);
            if (lstOptions != null && lstOptions.Count > 0)
            {
                string stateName = GetStateNameFromStateCode(stateCode);
                stateName = stateName.ToUpper();
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShipToZipCode && !string.IsNullOrEmpty(zipCode))
                {
                    var shippingOptionsByZip = lstOptions.Where(o => !string.IsNullOrEmpty(o.PostalCode) && zipCode.StartsWith(o.PostalCode)).ToList();
                    if (shippingOptionsByZip.Any())
                    {
                        return shippingOptionsByZip.FirstOrDefault().WarehouseCode;
                    }
                }

                // If not delivery option by zip code or not configured in this way then get the regular value
                var option =
                    lstOptions.Find(delegate(DeliveryOption p) { return p.State.Trim().ToUpper() == stateName && string.IsNullOrEmpty(p.PostalCode); });
                if (option != null)
                {
                    return option.WarehouseCode;
                }
            }
            return string.Empty;
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart,
                                                              MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 shippingInfo)
        {
            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                shippingInfo.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                return;
            }

            //TaskID: 9024 fix
            if (shippingInfo != null && shippingInfo.Address != null && shoppingCart != null &&
                shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                string warehouse = getWareHouseCode(shoppingCart.Locale,
                                                    shippingInfo.Address.StateProvinceTerritory.Trim().ToUpper(),
                                                    shippingInfo.Address.PostalCode);
                if (warehouse != string.Empty)
                {
                    shippingInfo.WarehouseCode = warehouse;
                }
            }

            if (IsHFFStandalone(shoppingCart.CartItems))
            {
                shippingInfo.ShippingMethodID = "NOF";
            }
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                shippment.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                return true;
            }

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null &&
                shoppingCart.DeliveryInfo.Address.Address != null &&
                shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                string warehouse = getWareHouseCode(shoppingCart.Locale,
                                                    shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory
                                                                .Trim().ToUpper(),
                                                    shoppingCart.DeliveryInfo.Address.Address.PostalCode);
                if (warehouse != string.Empty)
                {
                    shippment.WarehouseCode = warehouse;
                }
            }

            if (IsHFFStandalone(shoppingCart.CartItems))
            {
                shippment.ShippingMethodID = "NOF";
            }

            return true;
        }

        public override string[] GetFreightCodeAndWarehouseForTaxRate(Address_V01 address)
        {
            if (address != null)
            {
                return new[]
                    {
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                        getWareHouseCode("en-US", address.StateProvinceTerritory, address.PostalCode)
                    };
            }
            return null;
        }

        public override bool AddressValidationRequired()
        {
            return true;
        }

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

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                shoppingCart.DeliveryInfo.WarehouseCode == "95")
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
                var memberSubType = distributorProfileModel != null ? distributorProfileModel.Value.SubTypeCode : string.Empty;
                return string.Format("Pres Summit 2015, {0} Member", memberSubType);
            }
            else
            {
                return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale) +
                       getShippingInfoForFedexHal(shoppingCart);
            }
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

        public List<string> GetStatesForCountryToDisplay(string Country)
        {
            var states = GetStatesForCountryFromCache(Country);
            var statesList = new List<string>();
            if (states != null && states.Count > 0)
            {
                statesList = (from s in states
                              select string.Format("{0} - {1}", s.Code, s.DisplayName)).ToList<string>();
            }
            return statesList;
        }

        private List<USStates> GetStatesForCountryFromCache(string Country)
        {
            var states = HttpRuntime.Cache[USStatesCacheKey] as List<USStates>;
            if (states == null)
            {
                states = new List<USStates>();
                // Get info from database
                var dbStates = base.GetStatesForCountry(Country);

                // To get the resx path through Unit Testing
                string Staticresxpath = HostingEnvironment.ApplicationPhysicalPath != null
                    ? Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_GlobalResources\\UsaStates.resx") : Path.Combine(
                     AppDomain.CurrentDomain.BaseDirectory.Substring(0,
                                                                     AppDomain.CurrentDomain.BaseDirectory.IndexOf(
                                                                         "MyHerbalife3.Ordering.Test")), @"MyHerbalife3.Ordering.Web\\App_GlobalResources\\UsaStates.resx");
                var resxPath = (HttpContext.Current != null) ? HttpContext.Current.Server.MapPath("~\\App_GlobalResources\\UsaStates.resx") : Staticresxpath;
                var resxReader = new ResXResourceReader(resxPath);
                var resxDictionary = resxReader.Cast<DictionaryEntry>().ToDictionary
                    (r => r.Key.ToString(), r => r.Value.ToString());
                resxReader.Close();

                states.AddRange(from s in dbStates
                                join r in resxDictionary on s equals r.Value
                                select new USStates
                                    {
                                        Code = r.Key,
                                        Name = r.Value.Trim(),
                                        DisplayName = r.Value.Trim(),
                                        IsTerritory = false
                                    });

              
                 // To get the resx path through Unit Testing
                 Staticresxpath = HostingEnvironment.ApplicationPhysicalPath != null
                    ? Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_GlobalResources\\USTerritories.resx") : Path.Combine(
                     AppDomain.CurrentDomain.BaseDirectory.Substring(0,
                                                                     AppDomain.CurrentDomain.BaseDirectory.IndexOf(
                                                                         "MyHerbalife3.Ordering.Test")), @"MyHerbalife3.Ordering.Web\\App_GlobalResources\\USTerritories.resx");
                 resxPath = (HttpContext.Current != null) ? HttpContext.Current.Server.MapPath("~\\App_GlobalResources\\USTerritories.resx") : Staticresxpath;
                 resxReader = new ResXResourceReader(resxPath);
                 resxDictionary = resxReader.Cast<DictionaryEntry>().ToDictionary
                    (r => r.Key.ToString(), r => r.Value.ToString());
                resxReader.Close();

                states.AddRange(from s in dbStates
                                join r in resxDictionary on s equals
                                    (r.Value.Contains('|') ? r.Value.Substring(0, r.Value.IndexOf("|")) : r.Value.Trim())
                                select new USStates
                                    {
                                        Code = r.Key,
                                        Name =
                                            r.Value.Contains('|')
                                                ? r.Value.Substring(0, r.Value.IndexOf("|"))
                                                : r.Value.Trim(),
                                        DisplayName =
                                            r.Value.Contains('|')
                                                ? r.Value.Substring(r.Value.IndexOf("|") + 1)
                                                : r.Value.Trim(),
                                        IsTerritory = true
                                    });

                if (states.Count > 0)
                {
                    HttpRuntime.Cache.Insert(USStatesCacheKey, states, null,
                                             DateTime.Now.AddMinutes(USStatesCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
            }
            return states;
        }

        private bool IsPoBox(ShippingAddress_V01 address)
        {
            if (address != null && address.Address != null && address.Address.Line1 != null)
            {
                var line1 = address.Address.Line1.Replace(" ", string.Empty).ToUpper();
                return System.Text.RegularExpressions.Regex.IsMatch(line1, @"^POBOX\S*$");
            }
            return false;
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
                            CountryCode = "US",
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
                        HttpRuntime.Cache.Insert(cacheKey, locations, null,
                                                 DateTime.Now.AddMinutes(FedexCacheMinutes),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetDeliveryPickupAlternativesFromCache error: Country: US, error: {0}",
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

        #region Customer shipping address validation

        /// <summary>
        /// Validate if the customer address is valid.
        /// </summary>
        /// <param name="countrycode">The country code.</param>
        /// <param name="customerAddress">The customer address.</param>
        /// <returns>Validation flag.</returns>
        public override bool CustomerAddressIsValid(string countrycode, ref ShippingAddress_V02 customerAddress)
        {
            var level2ErrorCodes = new List<string> { "FOUND", "E421", "E422", "E427", "E412", "E413", "E423", "E425", "E430", "E420", "E600" };
            ServiceProvider.AddressValidationSvc.Address avsAddress = null;
            string errorCode = string.Empty;
            ShippingProvider.GetShippingProvider(countrycode).ValidateAddress(customerAddress, out errorCode, out avsAddress);
            if (errorCode == null || level2ErrorCodes.Exists(l => l == errorCode))
            {
                customerAddress.Address.City = avsAddress.City;
                customerAddress.Address.CountyDistrict = avsAddress.CountyDistrict;
                customerAddress.Address.Line1 = avsAddress.Line1;
                customerAddress.Address.Line2 = avsAddress.Line2;
                customerAddress.Address.Line3 = avsAddress.Line3;
                customerAddress.Address.Line4 = avsAddress.Line4;
                customerAddress.Address.PostalCode = avsAddress.PostalCode;
                customerAddress.Address.StateProvinceTerritory = avsAddress.StateProvinceTerritory;

                if (string.IsNullOrEmpty(customerAddress.Address.CountyDistrict))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        #endregion

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
                                CountryCode = "US",
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
                            string.Format("GetDeliveryOptionForDistributor error: Country: US, error: {0}",
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

        /// <summary>
        /// Gets the recipient name in a custom format.
        /// </summary>
        /// <param name="currentShippingInfo">The shipping address.</param>
        /// <returns></returns>
        public override string GetRecipientName(ShippingInfo currentShippingInfo)
        {
            if (currentShippingInfo == null || currentShippingInfo.Address == null)
            {
                return string.Empty;
            }

            if (currentShippingInfo.Option == DeliveryOptionType.Pickup && currentShippingInfo.WarehouseCode == "95")
            {
                // This is the recipient formated for Pres Summit 2015
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var memberSubType = distributorProfileModel != null ? distributorProfileModel.Value.SubTypeCode : string.Empty;
                return string.Format("{0}, {1}", currentShippingInfo.Address.Recipient, memberSubType);
            }

            return currentShippingInfo.Address.Recipient;
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

        private bool IsHFFStandalone(List<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01> cartItems)
        {
            var hffSkus = new List<string> { HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku };
            hffSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            var sku = from h in hffSkus
                      from c in cartItems
                      where h == c.SKU.Trim()
                      select h;
            return (sku.Count() == cartItems.Count);
        }
    }

    public class USStates
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsTerritory { get; set; }
    }
}