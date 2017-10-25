using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingChinaSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class FreightSimulationResult
    {
        public decimal EstimatedFreight { get; set; }
        public string StoreName { get; set; }
    }
    /// <summary>
    /// shipping provider for China
    /// </summary>
    public class ShippingProvider_CN : ShippingProviderBase
    {
        const int SHIPPINGINFO_CACHE_MINUTES = 60 * 12;
        private static readonly bool EnableCnsmsAndInternetCommerce = bool.Parse(Settings.GetRequiredAppSetting("EnableCnsmsAndInternetCommerce"));
        

        public string GetUnsupportedAddress(string province, string city, string district)
        {
            string warning = string.Empty;
            try
            {
                var proxy = ServiceClientProvider.GetChinaShippingServiceProxy();
                GetUnsupportedAddressResponse response = proxy.GetUnsupportedAddress(new GetUnsupportedAddressRequest1(new GetUnsupportedAddressRequest
                    {
                        City = city,
                        Province = province,
                        District = district,
                    })).GetUnsupportedAddressResult;
                if (response != null)
                    return response.WarningMessage;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                            string.Format("GetUnsupportedAddress {0} {1} {2} ERR:{3}", province, city, district,
                                          ex));
          
            }
            return warning;
        }

        public string GetUnsupportedExpressAddress(string province, string city, string district,
                                                   string expressCompanyId)
        {
            var warning = string.Empty;
            try
            {
                var proxy = ServiceClientProvider.GetChinaShippingServiceProxy();
                var response = proxy.GetUnsupportedExpressAddress(new GetUnsupportedExpressAddressRequest1(new GetUnsupportedExpressAddressRequest()
                    {
                        City = city,
                        Province = province,
                        District = district,
                        ExpressCompany = expressCompanyId
                    })).GetUnsupportedExpressAddressResult ;
                if (response != null && response.Status == ServiceProvider.ShippingChinaSvc.ServiceResponseStatusType.Success)
                    return response.WarningMessage;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetUnsupportedExpressAddress {0} {1} {2} {3} ERR:{4}", province, city, district, expressCompanyId,
                                  ex));

            }
            return warning;
        }

        public List<ProvinceStoreMapping>  GetProvinceStoreMapping()
        {
            const string CacheKeyMapping = "ProvinceStoreMapping";

            List<ProvinceStoreMapping> mappings = HttpRuntime.Cache[CacheKeyMapping] as List<ProvinceStoreMapping>;

            if (mappings != null)
                return mappings;

            mappings = new List<ProvinceStoreMapping>();
            var proxy = ServiceClientProvider.GetChinaShippingServiceProxy();

            GetProvinceStoreMappingResponse result = proxy.GetProvinceStoreMapping(new GetProvinceStoreMappingRequest1(new GetProvinceStoreMappingRequest())).GetProvinceStoreMappingResult;
            if (result.Status == ServiceProvider.ShippingChinaSvc.ServiceResponseStatusType.Success && result.Mappings != null)
            {
                mappings = result.Mappings;
                HttpRuntime.Cache.Insert(CacheKeyMapping,
                                                 mappings,
                                                 null,
                                                 DateTime.Now.AddMinutes(SHIPPINGINFO_CACHE_MINUTES),
                                                 Cache.NoSlidingExpiration,
                                                 CacheItemPriority.NotRemovable,
                                                 null);
            }
            return mappings;
        }

        // Bug 145976: ChinaDO: Shipping Company not match with existing OO.
        // unable to reproduce the issue, suspect the way the data being cached is the cause, probably the cache name is not unique enough, hence disabling the cache, until further findings
        const bool EmployExpressDdlCache = false;
        const bool TraceExpressDdlPerformance = false;

        public override List<DeliveryOption> GetDeliveryOptionsListForMobileShipping(string Country,
            string locale,
            ShippingAddress_V01 address, string memberId)
        {
            var proxy = ServiceClientProvider.GetChinaShippingServiceProxy();

            var deliveryOptions = new List<DeliveryOption>();

            var AddressID = 0;
            if(EnableCnsmsAndInternetCommerce)
            {
                // Line3 stores CNSMS addressID
                if (address == null || address.Address == null || string.IsNullOrEmpty(address.Address.Line3))
                    return deliveryOptions;
                int.TryParse(address.Address.Line3, out AddressID);
                if (AddressID == 0)
                {
                    return deliveryOptions;
                }
            }
            else
            {
                AddressID = address.ID;
            }
            

            // bug: 131993 - to make the cache key factored by the address location (Cnty, Province, City, District), which should affect the express companies
            deliveryOptions = new List<DeliveryOption>();
            var result =
                proxy.GetGetExpressCompany(new GetGetExpressCompanyRequest(new GetExpressCompanyRequest_V01()
                {
                    DistributorID = memberId,
                    AddressID = AddressID,
                    IncludeFeeDetail = true,
                })).GetGetExpressCompanyResult as GetExpressCompanyResponse_V01;
            if (result != null && result.Status == ServiceProvider.ShippingChinaSvc.ServiceResponseStatusType.Success)
            {
                foreach (var express in result.ExpressCompanies)
                {
                    var delivery = new DeliveryOption();
                    delivery.Name = delivery.Description = express.ExpressCompanyName;
                    delivery.ID = delivery.Id = express.ExpressCompanyID;
                    delivery.FreightCode = express.ExpressCompanyID.ToString();
                    delivery.Information = express.ExpressCompanyInfo;
                    delivery.BasePrice = express.BasePrice;
                    delivery.FirstPrice = express.FirstPrice;
                    delivery.FirstWeight = express.FirstWeight;
                    delivery.RenewalPrice = express.RenewalPrice;
                    delivery.EstimatedFee = express.EstimatedFee;
                    deliveryOptions.Add(delivery);
                }
            }

            return deliveryOptions;
        }
        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string country, string locale,
                                                                               ShippingAddress_V01 address)
        {
            StringBuilder perfMsg = new StringBuilder();

            DateTime perf01 = DateTime.Now;

            perfMsg.AppendLine(string.Format("GetDeliveryOptionsListForShipping - start : {0}", perf01));

            var proxy = ServiceClientProvider.GetChinaShippingServiceProxy();

            List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
            int AddressID = 0;
            if (EnableCnsmsAndInternetCommerce)
            {
                // Line3 stores CNSMS addressID
                if (address == null || address.Address == null || string.IsNullOrEmpty(address.Address.Line3))
                    return deliveryOptions;
                int.TryParse(address.Address.Line3, out AddressID);
                if (AddressID == 0)
                {
                    return deliveryOptions;
                }
            }
            else
            {
                AddressID = address.ID;
            }

            var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
            if (member==null || member.Value == null)
                return deliveryOptions;

            // bug: 131993 - to make the cache key factored by the address location (Cnty, Province, City, District), which should affect the express companies
            var addr = address.Address;
            string CacheKey = string.Format("{0}{1}_{2}_{3}_{4}_{5}", "CN_EXPRESS_COMPANY_", member.Value.Id, addr.Country, addr.StateProvinceTerritory, addr.City, addr.CountyDistrict);
            if (EmployExpressDdlCache)
            {
                //deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
                //if (deliveryOptions != null)
                //{
                //    if (TraceExpressDdlPerformance)
                //    {
                //        perfMsg.AppendLine(string.Format("GetDeliveryOptionsListForShipping - return cache : {0}", DateTime.Now.Subtract(perf01).Milliseconds));
                //        System.Diagnostics.Debug.WriteLine(perfMsg.ToString());
                //    }
                //    return deliveryOptions;
                //}
            }

            deliveryOptions = new List<DeliveryOption>();
            GetExpressCompanyResponse_V01 result =
                proxy.GetGetExpressCompany(new GetGetExpressCompanyRequest(new GetExpressCompanyRequest_V01
                    {
                        DistributorID = member.Value.Id,
                        AddressID = AddressID,
                        IncludeFeeDetail = true,
                    })).GetGetExpressCompanyResult as GetExpressCompanyResponse_V01;
            if (result != null && result.Status == ServiceProvider.ShippingChinaSvc.ServiceResponseStatusType.Success)
            {
                var selectText =
                    (string)HttpContext.GetLocalResourceObject(HLConfigManager.Configurations.CheckoutConfiguration
                                                                               .CheckoutOptionsControl, "TextSelect", CultureInfo.CurrentCulture);
                deliveryOptions.Add(new DeliveryOption()
                {
                    Name = selectText,
                    Id = 0,
                    FreightCode = "0",
                    Information = selectText,
                    Description = selectText
                });
                foreach (var express in result.ExpressCompanies)
                {
                    DeliveryOption delivery = new DeliveryOption();
                    delivery.Name = delivery.Description = express.ExpressCompanyName;
                    delivery.ID = delivery.Id = express.ExpressCompanyID;
                    delivery.FreightCode = express.ExpressCompanyID.ToString();
                    delivery.Information = express.ExpressCompanyInfo;
                    delivery.BasePrice = express.BasePrice;
                    delivery.FirstPrice = express.FirstPrice;
                    delivery.FirstWeight = express.FirstWeight;
                    delivery.RenewalPrice = express.RenewalPrice;
                    delivery.EstimatedFee = express.EstimatedFee;
                    deliveryOptions.Add(delivery);
                }

                if (EmployExpressDdlCache)
                {
                    //HttpRuntime.Cache.Insert(CacheKey,
                    //                                 deliveryOptions,
                    //                                 null,
                    //                                 DateTime.Now.AddMinutes(SHIPPINGINFO_CACHE_MINUTES),
                    //                                 Cache.NoSlidingExpiration,
                    //                                 CacheItemPriority.NotRemovable,
                    //                                 null);
                }
            }

            if (TraceExpressDdlPerformance)
            {
                //perfMsg.AppendLine(string.Format("GetDeliveryOptionsListForShipping - hit db : {0}", DateTime.Now.Subtract(perf01).Milliseconds));
                //System.Diagnostics.Debug.WriteLine(perfMsg.ToString());
            }
            return deliveryOptions;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] { string.Empty, string.Empty };

            if (address != null && address.Address != null &&
                !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
            {
                List<ProvinceStoreMapping> mappings =  GetProvinceStoreMapping();
                if (mappings != null)
                {
                    ProvinceStoreMapping mapping = mappings.Find(m => m.ProvinceName !=null && m.ProvinceName.Trim() == address.Address.StateProvinceTerritory.Trim());
                    if (mapping != null)
                    {
                        freightCodeAndWarehouse[1] = mapping.StoreID.ToString();
                    }
                }
            }
            freightCodeAndWarehouse[0] = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
            return freightCodeAndWarehouse;
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type,
                                                           int deliveryOptionID, int shippingAddressID)
        {
            if (type == DeliveryOptionType.Shipping)
            {
                var shippingAddresses = GetDSShippingAddresses(distributorID, locale);
                ShippingAddress_V02 shippingAddress = shippingAddresses.Find(s => s.ID == shippingAddressID);
                DeliveryOption deliveryOption = null;
                var freightCodeAndWarehouse = GetFreightCodeAndWarehouseFromAddress(shippingAddress, null);
                if (freightCodeAndWarehouse != null)
                {
                    deliveryOption = new DeliveryOption(freightCodeAndWarehouse[1], freightCodeAndWarehouse[0],
                                                        DeliveryOptionType.Shipping,
                                                        string.Empty);
                }

                if (deliveryOption == null || shippingAddress == null)
                {
                    return null;
                }
                ShippingInfo shippingInfo = new ShippingInfo(deliveryOption, shippingAddress);
                shippingInfo.AddressType = "EXP"; // this is address type for shipping

                if (Settings.GetRequiredAppSetting("LogShipping", "false").ToLower() == "true")
                {
                    LogRequest(string.Format("GetShippingInfoFromID for shipping option: {0}",
                                             OrderCreationHelper.Serialize(shippingInfo)));
                }
                return shippingInfo;
            }
            else if (type == DeliveryOptionType.Pickup)
            {
                ShippingInfo shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);

                if(shippingInfo != null)
                {
                shippingInfo.FreightCode = "0";
                shippingInfo.AddressType = "SD"; // this is address type for pickup
                }

                //Stores Pickup Location phone in session and sets shippingInfo phone to DS phone
                //var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);

                //sessionInfo.SelectedPickupLocationPhone = shippingInfo.Address.Phone;
                //shippingInfo.Address.Phone = String.Empty;                
                
                return shippingInfo;
            }
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int pickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = GetDeliveryOptions(type,
                                                        new ShippingAddress_V01
                                                        {
                                                            Address = new Address_V01 { Country = "CN" }
                                                        });
                        if (doList != null)
                        {
                            var deliveryOption = doList.Find(d => d.Id == pickupLocationID);
                            if (deliveryOption != null)
                            {
                                var shippingInfo = new ShippingInfo(deliveryOption);                               
                                shippingInfo.Id = deliveryOptionID; // this is ID field from PickUpStore
                                shippingInfo.AddressType = "PUC" + deliveryOption.AddressType;
                                return shippingInfo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            const string CacheKeyPickup = "DeliveryOptions__CN";
            const string CacheKeyPickupFromThirdParty = "PickupThirdParty_CN";

            List<DeliveryOption> deliveryOptions = null;

            if (type == DeliveryOptionType.Pickup)
            {
                deliveryOptions = HttpRuntime.Cache[CacheKeyPickup] as List<DeliveryOption>;
                if (deliveryOptions== null)
                {
                    deliveryOptions = base.GetDeliveryOptions(type, address);
                    if (deliveryOptions != null)
                    {
                        Array.ForEach(deliveryOptions.ToArray(),
                                      p => p.Description = string.Format("{0}-{1}", p.State, p.Description));

                        HttpRuntime.Cache.Insert(CacheKeyPickup,
                                                 deliveryOptions,
                                                 null,
                                                 DateTime.Now.AddMinutes(SHIPPINGINFO_CACHE_MINUTES),
                                                 Cache.NoSlidingExpiration,
                                                 CacheItemPriority.NotRemovable,
                                                 null);
                    }
                }
                return deliveryOptions;
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                deliveryOptions = HttpRuntime.Cache[CacheKeyPickupFromThirdParty] as List<DeliveryOption>;
                if (deliveryOptions == null)
                {
                    deliveryOptions = new List<DeliveryOption>();
                    var proxy = ServiceClientProvider.GetShippingServiceProxy();
                    //Look if there is a postal code provided:

                    DeliveryPickupAlternativesResponse_V03 pickupAlternativesResponse = null;

                    pickupAlternativesResponse =
                        proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03
                            {
                                CountryCode = address.Address.Country,
                                State = address.Address.StateProvinceTerritory
                            })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

                    if (pickupAlternativesResponse != null &&
                        pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                    {
                        deliveryOptions.AddRange(
                            from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                            select new DeliveryOption(po));
                    }
                    HttpRuntime.Cache.Insert(CacheKeyPickupFromThirdParty,
                                        deliveryOptions,
                                        null,
                                        DateTime.Now.AddMinutes(SHIPPINGINFO_CACHE_MINUTES),
                                        Cache.NoSlidingExpiration,
                                        CacheItemPriority.NotRemovable,
                                        null);
                }
                return deliveryOptions;

            }
            return null;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                             DeliveryOptionType type,
                                             string description,
                                             bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            
            if (HttpContext.Current.Session["pickupPhone"] != null)
            {
                var list = (List<string>)HttpContext.Current.Session["pickupPhone"];
                if (list.Count > 1 && !string.IsNullOrEmpty(address.Phone))
                {
                    if (!address.Phone.Equals(list[0]) && string.IsNullOrEmpty(list[1]))
                    {
                        list[0] = address.Phone;
                        HttpContext.Current.Session["pickupPhone"] = list;
                    }
                }
                
            }
            else
            {
                var list = new List<string> { address.Phone, "" };
                HttpContext.Current.Session["pickupPhone"] = list;
            }
            
            var pickuplist = (List<string>)HttpContext.Current.Session["pickupPhone"];
            if (type == DeliveryOptionType.Shipping)
            {
                
                return includeName
                           ? string.Format("{0}<br>{1},{2}{3}{4}{5}", address.Recipient ?? string.Empty,
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.CountyDistrict, address.Address.Line1)
                           : string.Format("{0}, {1}{2}{3}{4}",
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.CountyDistrict, address.Address.Line1);
            }
            else if (type == DeliveryOptionType.Pickup)
            {                   
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();                
                var dsProfile = member.Value;
                //var sessionInfo = SessionInfo.GetSessionInfo(dsProfile.Id, System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                
                //string phoneNumber = "联系电话：" + (string.IsNullOrEmpty(sessionInfo.SelectedPickupLocationPhone) ? null : sessionInfo.SelectedPickupLocationPhone) + "<br>";
                string phoneNumber = "联系电话：" + (pickuplist != null && pickuplist.Count > 0 ? pickuplist[0] : string.Empty) + "<br>";

                string displayInfo = (phoneNumber ?? string.Empty) + string.Format("提货地址：{0}<br>{1},{2}", description,
                       address.Address.Line1, address.Address.Line2 ?? string.Empty);
                if (!string.IsNullOrEmpty(address.AreaCode))
                    displayInfo = displayInfo + string.Format("<br>工作时间：{0}", address.AreaCode);
                return displayInfo;
            }
            else
            {                    
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();                
                var dsProfile = member.Value;                
                //var sessionInfo = SessionInfo.GetSessionInfo(dsProfile.Id, System.Threading.Thread.CurrentThread.CurrentCulture.Name);

                //string phoneNumber = "联系电话：" + (string.IsNullOrEmpty(sessionInfo.SelectedPickupLocationPhone) ? null : sessionInfo.SelectedPickupLocationPhone) + "<br>";
                string phoneNumber = "联系电话：" + (pickuplist != null && pickuplist.Count > 0 ? pickuplist[0] : string.Empty) + "<br>";
                string displayInfo = (phoneNumber ?? string.Empty) + string.Format("提货地址：{0}<br>{1},{2}", description,
                       address.Address.Line1, address.Address.Line2 ?? string.Empty);
                if (!string.IsNullOrEmpty(address.AreaCode))
                    displayInfo = displayInfo + string.Format("<br>工作时间：{0}", address.AreaCode);
                return displayInfo;
            }
        }

        public FreightSimulationResult CalculateFreight(ServiceProvider.OrderChinaSvc.ShippingInfo_V01 shippingInfo, decimal weight)
        {
            var freightSimulationResult = new FreightSimulationResult();
            if (shippingInfo == null || shippingInfo.Address == null || weight == 0)
                return freightSimulationResult;

            if (Settings.GetRequiredAppSetting("LogShipping", "false").ToLower() == "true")
            {
                LogRequest(string.Format("ShippingInfo before CalculateFreight:  {0}",
                                         OrderCreationHelper.Serialize(shippingInfo)));
            }

            if (!string.IsNullOrEmpty(shippingInfo.Address.StateProvinceTerritory) &&
                !string.IsNullOrEmpty(shippingInfo.Address.City) &&
                !string.IsNullOrEmpty(shippingInfo.Address.CountyDistrict))
            {
                if (string.IsNullOrEmpty(shippingInfo.ShippingMethodID))
                {
                    var mappings = GetProvinceStoreMapping();
                    if (mappings != null)
                    {
                        var mapping = mappings.Find(m => m.ProvinceName == shippingInfo.Address.StateProvinceTerritory);
                        if (mapping != null)
                        {
                            shippingInfo.ShippingMethodID = mapping.StoreID.ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(shippingInfo.WarehouseCode))
                {
                    shippingInfo.WarehouseCode =
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                }

                var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                var result = proxy.GetFreightCharge(new ServiceProvider.OrderChinaSvc.GetFreightChargeRequest1(new ServiceProvider.OrderChinaSvc.GetFreightChargeRequest_V01()
                {
                    ShippingInfo = shippingInfo,
                    Weight = weight
                })).GetFreightChargeResult as ServiceProvider.OrderChinaSvc.GetFreightChargeResponse_V01;

                if (result != null && result.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    freightSimulationResult.EstimatedFreight = result.Freight;
                    freightSimulationResult.StoreName = result.StoreName;
                }
            }

            if (Settings.GetRequiredAppSetting("LogShipping", "false").ToLower() == "true")
            {
                LogRequest(string.Format("ShippingInfo after CalculateFreight: {0}",
                                         OrderCreationHelper.Serialize(shippingInfo)));
            }

            return freightSimulationResult;
        }


        public override List<string> GetStatesForCountry(string country)
        {
            return GetStatesForCountry(country, 60 * 12);
        }

        public override List<string> GetCitiesForState(string country, string state)
        {
            return GetCitiesForState(country, state, 60*12);
        }

        public override List<string> GetStreetsForCity(string country, string state, string city)
        {
            return GetStreetsForCity(country, state, city, 60*12);
        }
        public MyHLShoppingCart StandAloneAddressForDonation(MyHLShoppingCart myShoppingCart)
        {
           
            ShippingAddress_V02 shippingAddress = new ShippingAddress_V02();
            shippingAddress.Address = CreateDefaultAddress();
            shippingAddress.ID = -4;
            DeliveryOption deliveryOption = new DeliveryOption();
            shippingAddress.ID = -4;
            ShippingInfo shippingInfo = new ShippingInfo(deliveryOption, shippingAddress);
            shippingInfo.WarehouseCode = "888";
            shippingInfo.FreightCode = "3";
            shippingInfo.AddressType = "EXP";
            myShoppingCart.DeliveryInfo = shippingInfo;
            myShoppingCart.DeliveryInfo.Option = DeliveryOptionType.Shipping;
            myShoppingCart.DeliveryInfo.Address.Phone = "21-61033719";
            myShoppingCart.DeliveryInfo.Address.Recipient = "Test Recipient";
            return myShoppingCart;
        }
        public static Address_V01 CreateDefaultAddress()
        {
            var address = new Address_V01();
            address.Country = "CN";
            address.Line1 = "中路268号4801室";
            address.Line2 = "";
            address.Line3 = "";
            address.Line4 = "";
            address.City = "西藏";
            address.PostalCode = "200001";
            address.StateProvinceTerritory = "上海市";
            address.CountyDistrict = "黄埔区";
            return address;
        }

        public override string GetMapScript(ShippingMapType mapType)
        {
            string result = "";

            switch (mapType)
            {
                case ShippingMapType.Baidu:                   
                    try
                    {
                        if(ChinaShippingServiceProxy == null)
                            ChinaShippingServiceProxy = ServiceClientProvider.GetChinaShippingServiceProxy();

                        var response = ChinaShippingServiceProxy.GetBaiduMapJavascript(new GetBaiduMapJavascriptRequest1(new GetBaiduMapJavascriptRequest_V01
                        {
                            APIurl = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.MapURL,
                        })).GetBaiduMapJavascriptResult as GetBaiduMapJavascriptResponse_V01;

                        if (response != null && response.Status == ServiceProvider.ShippingChinaSvc.ServiceResponseStatusType.Success)
                            result = response.Script;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                                    string.Format("ShippingProvider_CN : GetMapScript ERR:{0}", ex));

                    }
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
