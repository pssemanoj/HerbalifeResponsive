using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using System.Threading;
using System.Web.Caching;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Providers
{
    public static class PurchasingLimitProvider
    {
        private const string PURCHASING_LIMITS_CACHE_PREFIX = "PURCHASING_LIMITS_";

        public static List<string> nonExemptCountries = DistributorOrderingProfileProvider.MpeThresholdCountries;

        //private static int PURCHASING_LIMITS_CACHE_MINUTES =
        //    Settings.GetRequiredAppSetting<int>("PurchasingLimitsCacheExpireMinutes");

        public static bool IsMarketingPlanDistributor
        {
            get
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (null == member)
                {
                    return false;
                }
                return (
                    nonExemptCountries.Contains(
                    ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.ProcessingCountryCode) && ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.TypeCode == "DS");
            }
        }

        public static bool IsMarketingPlanDistributorById(string distributorId)
        {
            var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
            if (null == member)
            {
                var loader = new DistributorProfileLoader();
                var profile = loader.Load(new GetDistributorProfileById {Id = distributorId});
                if (null == profile)
                {
                    return false;
                }
                return nonExemptCountries.Contains(profile.ProcessingCountryCode) && (profile.TypeCode == "DS");
            }
            return (
                nonExemptCountries.Contains(
                    ((MembershipUser<DistributorProfileModel>) Membership.GetUser()).Value.ProcessingCountryCode) &&
                ((MembershipUser<DistributorProfileModel>) Membership.GetUser()).Value.TypeCode == "DS");
        }

        public static bool IsOrderThresholdMaxVolume(PurchasingLimits_V01 purchasingLimits)
        {
            decimal[] thresholdVolume = {1500.00M, 3999.99M, 1100.00M, 1030.00M, 2530.00M};
            if (null != purchasingLimits && thresholdVolume.Contains(purchasingLimits.maxVolumeLimit))
            {
                return true;
            }
            return false;
        }

        public static bool IsRestrictedByMarketingPlan(string distributorId)
        {
            if (IsMarketingPlanDistributorById(distributorId))
            {
                PurchasingLimits_V01 testLimits = GetCurrentPurchasingLimits(distributorId);
                return IsOrderThresholdMaxVolume(testLimits);
            }

            return false;
        }

        public static bool RequirePurchasingLimits(string distributorId,string countryCode)
        {
            if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                string type;
                if (null == member)
                {
                    var loader = new DistributorProfileLoader();
                    var profile = loader.Load(new GetDistributorProfileById() { Id = distributorId });
                    type = profile.TypeCode;
                }
                else
                {
                    type = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.TypeCode;
                }

                return FOP.PurchaseRestrictionProvider.HasPurchaseRestriction(distributorId, countryCode, type);
            }
            else
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                
                if (null == member)
                {
                    var loader = new DistributorProfileLoader();
                    var profile = loader.Load(new GetDistributorProfileById() { Id = distributorId });
                    var isMpDS= nonExemptCountries.Contains(profile.ProcessingCountryCode) && profile.TypeCode == "DS";
                    return isMpDS || HLConfigManager.Configurations.DOConfiguration.NonThresholdCountryRequiredPurchasingLimits || nonExemptCountries.Contains(countryCode);    
                }
                else
                {
                    return IsMarketingPlanDistributor || HLConfigManager.Configurations.DOConfiguration.NonThresholdCountryRequiredPurchasingLimits || nonExemptCountries.Contains(countryCode);    
                }
            }
        }

        public static PurchasingLimits_V01 GetCurrentPurchasingLimits(string distributorID)
        {
            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
            {
                IPurchasingLimitManager manager = PurchasingLimitManager(distributorID);
                return manager.GetPurchasingLimits(GetOrderMonth());
            }
            else
            {
                return GetPurchasingLimits(distributorID, string.Empty);
            }
        }

        // this is called from UI control
        public static PurchasingLimits_V01 GetPurchasingLimits(string distributorID, string TIN, string subtype = null)
        {
            if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
            {
                var currentSession = SessionInfo.GetSessionInfo(distributorID, Thread.CurrentThread.CurrentCulture.ToString());
                if (currentSession != null && currentSession.ShoppingCart != null)
                {
                    return FOP.PurchaseRestrictionProvider.GetPurchasingLimits(200000 + OrderMonth.GetCurrentOrderMonth(), distributorID, currentSession.ShoppingCart.OrderSubType);
                }
                else
                {
                    return FOP.PurchaseRestrictionProvider.GetPurchasingLimits(200000 + OrderMonth.GetCurrentOrderMonth(), distributorID, subtype);
                }
            }
            else
            {
                var purchasingLimits = GetPurchasingLimitsFromRulesEngine(distributorID, TIN);
                if (null != purchasingLimits && purchasingLimits.Count > 0)
                {
                    return purchasingLimits[GetOrderMonth()];
                }
            }
            return null;
        }

        //public static PurchasingLimits_V01 GetCurrentOrderMonthPurchasingLimits(string distributorID, int orderMonth)
        //{
        //    var purchasingLimits = getPurchasingLimitsFromCache(distributorID);
        //    if (null != purchasingLimits && purchasingLimits.Count > 0)
        //    {
        //        return purchasingLimits[orderMonth];
        //    }
        //    return null;
        //}

        //public static PurchasingLimits_V01 GetCurrentOrderMonthPurchasingLimits(string distributorID,
        //                                                                        string TIN,
        //                                                                        int orderMonth)
        //{
        //    var purchasingLimits = GetPurchasingLimitsFromRulesEngine(distributorID,
        //                                                              TIN);
        //    if (null != purchasingLimits && purchasingLimits.Count > 0)
        //    {
        //        return purchasingLimits[orderMonth];
        //    }
        //    return null;
        //}

        public static void Reload(string distributorId)
        {
            PurchasingLimitManager(distributorId).ReloadPurchasingLimits(distributorId);
        }

        public static void UpdatePurchasingLimits(PurchasingLimits_V01 limits, string distributorId)
        {
            PurchasingLimitManager(distributorId).UpdatePurchasingLimits(limits,GetOrderMonth());
        }

        public static void UpdatePurchasingLimits(PurchasingLimits_V01 limits, string distributorId, bool SaveToStore = false)
        {
            var currentLoggedInCountry = CultureInfo.CurrentCulture.Name.Substring(3);
            PurchasingLimitManager(distributorId).UpdatePurchasingLimits(limits, GetOrderMonth());
            SavePurchaseLimitsToStore(currentLoggedInCountry, distributorId);
        }

        public static void UpdatePurchasingLimits(PurchasingLimits_V01 limits,
                                                  string distributorId,
                                                  string country,
                                                  bool SaveToStore)
        {
            var purchasingLimitsVPUpdateForForeignCountry = new List<string>(Settings.GetRequiredAppSetting("PurchasingLimitsVPUpdateForForeignCountry","").Split(new char[] { ',' }));

             if (purchasingLimitsVPUpdateForForeignCountry.Contains(country) &&
                 System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(3, 2) != country &&
                 !IsRestrictedByMarketingPlan(distributorId))
                {
                    return;
                } 
            
            
            PurchasingLimitManager(distributorId).UpdatePurchasingLimits(limits, GetOrderMonth());
            SavePurchaseLimitsToStore(country, distributorId);
        }

        public static void UpdatePurchasingLimits(PurchasingLimits_V01 limits, string distributorId, int orderMonth)
        {
            PurchasingLimitManager(distributorId).UpdatePurchasingLimits(limits, orderMonth);
        }

        /// <summary>
        ///     OverLoaded method for the Iphone..
        /// </summary>
        /// <param name="limits"></param>
        /// <param name="distributorId"></param>
        /// <param name="orderMonth"></param>
        public static void UpdatePurchasingLimits(PurchasingLimits_V01 limits, string distributorId, string orderMonth)
        {
            var month = 0;
            if (int.TryParse(orderMonth.Substring(4), out month))
            {
                UpdatePurchasingLimits(limits, distributorId, month);
            }
        }

        private static Dictionary<int, PurchasingLimits_V01> getPurchasingLimitsFromCache(string distributorID)
        {
            return PurchasingLimitManager(distributorID).PurchasingLimits;
        }

        public static void savePurchasingLimitsToCache(Dictionary<int, PurchasingLimits_V01> PurchasingLimits,
                                                       string distributorId)
        {
            PurchasingLimitManager(distributorId).PurchasingLimits = PurchasingLimits;
        }

        private static Dictionary<int, PurchasingLimits_V01> GetPurchasingLimitsFromRulesEngine(string distributorId,
                                                                                                string TIN)
        {
            if (string.IsNullOrEmpty(distributorId))
            {
                return null;
            }
            else
            {
                try
                {
                    var purchasingLimits =
                        HLRulesManager.Manager.GetPurchasingLimits(distributorId, TIN);
                    if (purchasingLimits != null)
                        savePurchasingLimitsToCache(purchasingLimits, distributorId);
                    return purchasingLimits;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("Error updating Purchasing Limits for distributor: {0}\r\n{1}", distributorId,
                                      ex.Message));
                }
            }
            return null;
        }

        public static int GetOrderMonth()
        {
            DateTime current;
            string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(3);
            if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.Annually ||
                HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.Quarterly)
            {
                string distributorID = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.Id;
                IPurchasingLimitManager manager = new PurchasingLimitManagerFactory().GetPurchasingLimitManager(distributorID);

                if (manager != null &&
                   (manager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan |
                    IsMarketingPlanDistributor))
                {
                    OrderMonth orderMonth = new OrderMonth(countryCode);

                    current = orderMonth.CurrentOrderMonth;
                    return Int32.Parse(current.ToString("yyyyMM"));
                }
                current = DateUtils.GetCurrentLocalTime(countryCode);
            }
            else
            {
                OrderMonth orderMonth = new OrderMonth(countryCode);
                current = orderMonth.CurrentOrderMonth;
            }

            return Int32.Parse(current.ToString("yyyyMM"));
        }

        // FOP
        public static int GetOrderMonth(OrderMonth orderMonth, string distributorID, string countryCode)
        {
            DateTime current;
            if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.Annually ||
                HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.Quarterly)
            {
                IPurchasingLimitManager manager = new PurchasingLimitManagerFactory().GetPurchasingLimitManager(distributorID);

                if (manager != null &&
                   (manager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan |
                    IsMarketingPlanDistributor))
                {
                    current = orderMonth.CurrentOrderMonth;
                    return Int32.Parse(current.ToString("yyyyMM"));
                }
                current = DateUtils.GetCurrentLocalTime(countryCode);
            }
            else
            {
                current = orderMonth.CurrentOrderMonth;
            }

            return Int32.Parse(current.ToString("yyyyMM"));
        }

        public static PurchasingLimits_V01 GetPurchasingLimitsFromStore(string country, string distributorId)
        {
            PurchasingLimits_V01 result = null;
            var request = new GetPurchasingLimitsRequest_V01();
            GetPurchasingLimitsResponse_V01 response = null;
            request.Country = country;
            request.DistributorId = distributorId;

            //Passing the orderMonth as the parameter.

            request.OrderMonth = GetOrderMonth().ToString();

            OrderServiceClient proxy = null;
            try
            {
                proxy = ServiceClientProvider.GetOrderServiceProxy();
                response = proxy.GetPurchasingLimits(new GetPurchasingLimitsRequest1(request)).GetPurchasingLimitsResult as GetPurchasingLimitsResponse_V01;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Get PurchasingLimits failed for Distributor {0}, country Code: {1} {2}", distributorId,
                                  country, ex));
            }
            if (null != response && response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
            {
                result = response.PurchasingLimits as PurchasingLimits_V01;
            }
            //else
            //{
            //    LoggerHelper.Error("Successful call to GetPurchasingLimits did not succeed in returning data");
            //}

            return result;
        }

        public static void SavePurchaseLimitsToStore(string country, string distributorId)
        {
            //2.CurrentOrderMonth
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                return;
            var limits = getPurchasingLimitsFromCache(distributorId);
            if (null != limits && limits.Count > 0)
            {
                //var orderMonth = new OrderMonth(country);
               // orderMonth.ResolveOrderMonth();
                foreach (var limit in limits)
                {
                    var request = new SetPurchasingLimitsRequest_V01();
                    request.Country = country;
                    request.DistributorId = distributorId;
                    request.OrderMonth = limit.Key.ToString();

                    var purchasingLimit = limit.Value;
                    request.PurchasingLimits = purchasingLimit;

                    OrderServiceClient proxy = null;
                    try
                    {
                        proxy = ServiceClientProvider.GetOrderServiceProxy();
                        var response =
                            proxy.SetPurchasingLimits(new SetPurchasingLimitsRequest1(request)).SetPurchasingLimitsResult as SetPurchasingLimitsResponse_V01;
                        if (response.Status != ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "Save Purchasing Limits failed for Distributor {0}, Country {1}, Status{2}",
                                    distributorId, country, response.Status));
                        }
                    }
                    catch (Exception ex)
                    {
                            LoggerHelper.Error(
                            string.Format("Save PurchasingLimits failed for Distributor {0}, country Code: {1} {2}",
                                          distributorId, country, ex));
                    }
                }
            }
        }

        public static void ReconcileAfterPurchase(MyHLShoppingCart shoppingCart,
                                                  string distributorId,
                                                  string countryCode)
        {
            if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
            {
                FOP.PurchaseRestrictionProvider.ReconcileAfterPurchase(shoppingCart, distributorId, countryCode);
                return;
            }
            if (null != shoppingCart)
            {
                var currentLimits = GetCurrentPurchasingLimits(shoppingCart.DistributorID);
                var limitsType =
                    HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType;
                if (limitsType == PurchasingLimitRestrictionType.MarketingPlan)
                {
                    if (null == currentLimits || currentLimits.maxVolumeLimit < 0 ||
                        HLRulesManager.Manager.DistributorIsExemptFromPurchasingLimits(distributorId))
                    {
                        return;
                    }
                    else
                    {
                        var ods = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
                        countryCode = ods.Value.ProcessingCountryCode;
                    }
                }
                if (null != currentLimits)
                {
                    switch (currentLimits.PurchaseLimitType)
                    {
                        case PurchaseLimitType.Earnings:
                            {
                                currentLimits.RemainingEarnings -= shoppingCart.ProductEarningsInCart;
                                break;
                            }
                        case PurchaseLimitType.Volume:
                        case PurchaseLimitType.ProductCategory:
                            {
                                currentLimits.RemainingVolume -= shoppingCart.ProductVolumeInCart;
                                break;
                            }
                        case PurchaseLimitType.DiscountedRetail:
                            {
                                currentLimits.RemainingVolume -= shoppingCart.ProductDiscountedRetailInCart;
                                break;
                            }
                        case PurchaseLimitType.TotalPaid:
                            {
                                currentLimits.RemainingVolume -= (shoppingCart.Totals as  OrderTotals_V01).AmountDue;
                                break;
                            }
                    }

                    UpdatePurchasingLimits(currentLimits, shoppingCart.DistributorID, countryCode, true);
                    //if (countryCode != "CN")
                    //    Reload(shoppingCart.DistributorID);
                    //else
                    //{
                        PurchasingLimitManager(shoppingCart.DistributorID).ExpireCache();
                    //}
                }
            }
            else
            {
                LoggerHelper.Error(
                    string.Format(
                        "PurchasingLimitProvider.ReconcileAfterPurchase was passed a null cart and couldn't update the cached limits for Distributor: {0}, Country: {1}",
                        distributorId, countryCode));
            }
        }

        public static DistributorPurchasingLimitsSourceType GetDistributorPurchasingLimitsSource(string country,
                                                                                                 string distributorId)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                return DistributorPurchasingLimitsSourceType.HMS;

            var source = DistributorPurchasingLimitsSourceType.Unknown;
            string cacheKey = string.Format("{0}_{1}_{2}", "PLSOURCE", country, distributorId);
            string useFusion = HttpRuntime.Cache[cacheKey] as string;

            if (useFusion == null)
            {
            if (!string.IsNullOrEmpty(distributorId) && !string.IsNullOrEmpty(country))
            {
                try
                {
                    OrderServiceClient proxy = null;
                    proxy = ServiceClientProvider.GetOrderServiceProxy();
                    var request =
                        new GetDistributorPurchasingLimitsSourceRequest_V01();
                    request.DistributorID = distributorId;
                    request.CountryCode = country;
                    // RS exception in country for request
                    if (country.Equals("RS"))
                    {
                        request.CountryCode = HL.Common.ValueObjects.CountryType.RS.HmsCountryCodes.FirstOrDefault();
                    }

                    var response =
                        proxy.GetDistributorPurchasingLimitsSource(new GetDistributorPurchasingLimitsSourceRequest(request)).GetDistributorPurchasingLimitsSourceResult as
                        GetDistributorPurchasingLimitsSourceResponse_V01;
                    if (response.Status != ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                    {
                        LoggerHelper.Error(
                            string.Format("Save Purchasing Limits failed for Distributor {0}, Country {1}, Status{2}",
                                          distributorId, country, response.Status));
                    }
                    if (null != response)
                    {
                        source = response.Source;
                            useFusion = source == DistributorPurchasingLimitsSourceType.HMS ? Boolean.TrueString : Boolean.FalseString;
                            HttpRuntime.Cache.Insert(cacheKey, useFusion, null, DateTime.Now.AddMinutes(15), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error retrieving OutstandingOrders Status from BPEL service for: DS:{0} - Country:{1}, {2}",
                            distributorId, country, ex));
                }
            }
            }

            return source = string.IsNullOrEmpty(useFusion) ? DistributorPurchasingLimitsSourceType.InternetOrdering : (useFusion.Equals(Boolean.TrueString) ? DistributorPurchasingLimitsSourceType.HMS : DistributorPurchasingLimitsSourceType.InternetOrdering);
        }

        public static void SetPostErrorRemainingLimitsSummaryMessage(MyHLShoppingCart cart)
        {
            if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
            {
                return;
            }
            if (cart.RuleResults.Any(rs => (rs.Result == RulesResult.Failure && rs.RuleName == "PurchasingLimits Rules")))
            {
                if (cart.RuleResults.Any(rs => rs.Messages.Find(m => m.Equals("DisgardCommonMessage")) != null))
                {
                    Array.ForEach(cart.RuleResults.ToArray(), a => a.Messages.RemoveAll(delegate(string x) { return x.Equals("DisgardCommonMessage"); }));
                    return;
                }
                var currentLimits = GetCurrentPurchasingLimits(cart.DistributorID);
                if (currentLimits == null)
                    return;
                
                var message = string.Empty;
                switch (PurchasingLimitManager(cart.DistributorID).PurchasingLimitsRestriction)
                {
                    case PurchasingLimitRestrictionType.MarketingPlan:
                        {
                            switch (currentLimits.PurchaseLimitType)
                            {
                                case PurchaseLimitType.ProductCategory:
                                    {
                                        message = string.Empty;
                                        break;
                                    }
                                default:
                                    {
                                        var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
                                        if (RequirePurchasingLimits(cart.DistributorID, member.Value.ProcessingCountryCode))
                                        {
                                            message = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "PostErrorRemainingThresholdLimitsVolumeSummary") as string;    
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    default:
                        {
                            switch (currentLimits.PurchaseLimitType)
                            {
                                case PurchaseLimitType.DiscountedRetail:
                                    {
                                        message =
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "PostErrorRemainingPurchasingLimitsDiscountedRetailSummary") as string;
                                        break;
                                    }
                                case PurchaseLimitType.Volume:
                                    {
                                        message =
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "PostErrorRemainingThresholdLimitsVolumeSummary") as string;
                                        break;
                                    }
                                case PurchaseLimitType.Earnings:
                                    {
                                        message =
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "PostErrorRemainingPurchasingLimitsEarningsSummary") as string;
                                        break;
                                    }
                                default:
                                    {
                                        message = string.Empty;
                                        break;
                                    }
                            }
                            break;
                        }
                }

                if (!string.IsNullOrEmpty(message))
                {
                    var messageResult = new ShoppingCartRuleResult();
                    messageResult.RuleName = "PurchasingLimits Rules";
                    messageResult.Result = RulesResult.Failure;

                    //PurchasingLimitRestrictionType purchasingLimitRestrictionType = PurchasingLimitManager(cart.DistributorID).PurchasingLimitsRestriction;
                    //decimal cartVolume = (purchasingLimitRestrictionType == PurchasingLimitRestrictionType.MarketingPlan) ? (cart as MyHLShoppingCart).VolumeInCart : (cart as MyHLShoppingCart).ProductVolumeInCart;
                    var limits = GetCurrentPurchasingLimits(cart.DistributorID);
                    if (limits.PurchaseLimitType == PurchaseLimitType.DiscountedRetail)
                    {
                        var discountedRetail = (cart.Totals != null) ? cart.ProductDiscountedRetailInCart : 0.0M;
                        var remainingRetail = limits.RemainingVolume - discountedRetail;
                        messageResult.AddMessage(string.Format(message, remainingRetail));
                        cart.RuleResults.Insert(0, messageResult);
                    }
                    else
                    {
                        decimal cartVolume = (cart as MyHLShoppingCart).VolumeInCart;
                        messageResult.AddMessage(string.Format(message, currentLimits.RemainingVolume - cartVolume));
                        cart.RuleResults.Insert(0, messageResult);
                    }
                }
            }
        }

        public static string GetRemainingVolumePointExceededMessage(MyHLShoppingCart cart)
        {
            var currentLimits = GetCurrentPurchasingLimits(cart.DistributorID);
            var message = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "RemainingVolumePointExceeds") as string ?? string.Empty;
            message = string.Format(message, (cart.VolumeInCart - currentLimits.RemainingVolume));
            return message;
        }

        private static IPurchasingLimitManager PurchasingLimitManager(string id)
        {
            IPurchasingLimitManagerFactory purchasingLimitManagerFactory = new PurchasingLimitManagerFactory();
            return purchasingLimitManagerFactory.GetPurchasingLimitManager(id);
        }

        /// <summary>
        /// Returns the flag to display the limits based on any country rule.
        /// </summary>
        /// <returns></returns>
        public static bool DisplayLimits(string distributorId, string countryCode)
        {
            if (countryCode.Equals("BA"))
            {
                var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
                return tins.All(t => t.IDType.Key != "BATX");
            }
            return true;
        }
    }
}
