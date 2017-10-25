using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Utilities;
using HL.Blocks.CircuitBreaker;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.ViewModel.Requests;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers
{
    public static class APFDueProvider
    {
        public const string APFDUE_CACHE_PREFIX = "APFDUE_";
        public const int APFDUE_CACHE_MINUTES = 15;

        public static bool UpdateAPFDuePaid(string distributorID, DateTime apfDue)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return false;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetCatalogServiceProxy();
                    var response =
                        proxy.UpdateAPFDue(new UpdateAPFDueRequest(new UpdateAPFPaidRequest_V01 { DistributorID = distributorID, DueDate = apfDue })).UpdateAPFDueResult as 
                        UpdateAPFPaidResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        clearAPFDuePaidFromCache(getCacheKey(distributorID));
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            return false;
        }

        public static bool GetAPFDuePaid(string distributorID, DateTime apfDue)
        {
            return getGetAPFDuePaidFromCache(distributorID, apfDue);
        }

        private static string getCacheKey(string distributorID)
        {
            return APFDUE_CACHE_PREFIX + distributorID;
        }

        private static bool getGetAPFDuePaidFromCache(string distributorID, DateTime apfDue)
        {
            object result;

            if (string.IsNullOrEmpty(distributorID))
            {
                return false;
            }

            // gets cache key 
            var cacheKey = getCacheKey(distributorID);

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey];

            if (null == result)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = loadAPFDuePaidFromService(distributorID, apfDue);
                    saveAPFDuePaidToCache(cacheKey, (bool)result);
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return (bool)result;
        }

        private static bool loadAPFDuePaidFromService(string distributorID, DateTime dueDate)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return false;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetCatalogServiceProxy();
                    var response =
                        proxy.GetAPFDue(new GetAPFDueRequest(new GetAPFPaidRequest_V01{ DistributorID = distributorID, DueDate = dueDate})).GetAPFDueResult as GetAPFPaidResponse_V01;
                    if (response != null)
                    {
                        return response.ReturnCode;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            return false;
        }

        private static void saveAPFDuePaidToCache(string cacheKey, bool ispaid)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     ispaid,
                                     null,
                                     DateTime.Now.AddMinutes(APFDUE_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);
        }

        private static void clearAPFDuePaidFromCache(string cacheKey)
        {
            HttpRuntime.Cache.Remove(cacheKey);
        }

        public static bool IsAPFExemptOn200VP(DistributorOrderingProfile distributorProfile, decimal currentOrderPoint)
        {
            bool Exempted = false;

            try
            {
                if (distributorProfile == null) return false;

                var volumes = getVolumePoints(distributorProfile.Id);
                var totalVolum = volumes.VolumePoints.Find(v => DateTime.Parse(v.VolumeMonth.ToString()).Month == DateTime.UtcNow.Month);
                if (distributorProfile.ApfDueDate != null && totalVolum != null && distributorProfile.ApfDueDate <= DateTime.UtcNow.Date && (totalVolum.Volume) >= 200)
                {
                    Exempted = true;
                }
                else if (distributorProfile.ApfDueDate != null && totalVolum != null && distributorProfile.ApfDueDate > DateTime.UtcNow.Date && (totalVolum.Volume + currentOrderPoint) >= 200)
                {
                    Exempted = true;
                }
                return Exempted;
            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error("APFDueProvider.IsAPFExemptOn200VP() Error \n" + ex.StackTrace);
                return Exempted;
            }

        }

        private static MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumeResponse_V01 getVolumePoints(string distributorId)
        {
            var cacheKey = "VOLAPFExempt_" + distributorId; 

            // tries to get object from cache
            var result = HttpRuntime.Cache[cacheKey];

            if (null != result)
            {
                return result as MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumeResponse_V01;
            }

            try
            {
                var proxy = ServiceClientProvider.GetDistributorServiceProxy();
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumeResponse_V01>();

                var getVolumeRequest = new MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumeRequest_V01
                {
                    DistributorID = distributorId,
                    StartDate = DateTimeUtils.GetFirstDayOfMonth(DateTime.UtcNow.AddMonths(-1)),
                    EndDate = DateTimeUtils.GetLastDayOfMonth(DateTime.UtcNow)
                };
                var response =
                    circuitBreaker.Execute(
                        () => proxy.GetDistributorVolumePoints(new MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumePointsRequest(getVolumeRequest)).GetDistributorVolumePointsResult as MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.GetDistributorVolumeResponse_V01);
                HttpRuntime.Cache.Insert(cacheKey,
                                         response,
                                         null,
                                         DateTime.Now.AddMinutes(APFDUE_CACHE_MINUTES),
                                         Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal,
                                         null);
                return response;
            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error("APFDueProvider.IsAPFExemptOn200VP() Error \n" + ex.StackTrace);
            }

            return null;
        }

        public static void UpdateAPFDue(string distributorID, DateTime due, ShoppingCartItemList cartItems)
        {
            if (IsAPFSkuPresent(cartItems))
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    var response =
                        proxy.UpdateAPFDue(new UpdateAPFDueRequest(new UpdateAPFPaidRequest_V01 { DistributorID = distributorID, DueDate = due})).UpdateAPFDueResult as
                        UpdateAPFPaidResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        saveAPFDuePaidToCache(getCacheKey(distributorID), true);
                    }
                }
            }
        }

        /// <summary>
        /// ShouldShowAPFModule
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public static bool ShouldShowAPFModule(string distributorId, string country)
        {
            var showModule = false;
            if (HLConfigManager.Configurations.APFConfiguration.APFRequired)
            {
                if (country.Equals("CN"))
                {
                    return ShouldShowAPFModule(distributorId);
                }
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                showModule = IsAPFDueAndNotPaid(distributorId, locale) || IsAPFDueWithinOneYear(distributorId, country) ||
                             IsAPFDueGreaterThanOneYear(distributorId, country);
            }

            return showModule;
        }

        /// <summary>
        /// China ShouldShowAPFModule
        /// </summary>
        /// <param name="distributorId"></param>
        /// <returns></returns>
        public static bool ShouldShowAPFModule(string distributorId)
        {
            bool shouldDisplay = false;
            DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorId, "CN");
            try
            {

            if (distributorOrderingProfile != null)
            {
                if (string.IsNullOrEmpty(distributorOrderingProfile.CNCustCategoryType))
                {
                    return false;
                }
                    if (distributorOrderingProfile.CNCustCategoryType.ToLower() == "psr")
                    return false;
                //--0(normal DS) 1(overdue within next 3 month) 2(in grace period) 3(has renewed) 4(not DS) 5(overdue)
                if (distributorOrderingProfile.CNAPFStatus == 2)
                {
                    return IsAPFDueAndNotPaid(distributorId);
                }
                if (distributorOrderingProfile.CNAPFStatus == 1) // if due in next 3 month, check if paid
                {
                    DateTime annualProcessingFeeDue = distributorOrderingProfile.ApfDueDate;

                    // APF due
                    var hmsAPFDue = new DateTime(annualProcessingFeeDue.Year,
                                                 annualProcessingFeeDue.Month,
                                                 annualProcessingFeeDue.Day);
                    shouldDisplay = !GetAPFDuePaid(distributorId, annualProcessingFeeDue);
                }
                if (distributorOrderingProfile.CNAPFStatus == 0) // if normal, should show APF from Month 1 - Month 12
                {
                    shouldDisplay = distributorOrderingProfile.ApfDueDate.AddYears(-1) <= DateTime.Today && DateTime.Today <= distributorOrderingProfile.ApfDueDate;
                }
                }
            }
            catch (Exception ex)
            {
                if (distributorOrderingProfile != null)
                    HL.Common.Logging.LoggerHelper.Error(
                        string.Format(
                            "there is an error in ShouldShowAPFModule DistributorId{0},CNCustCategoryType{1},CNAPFStatus{2},ApfDueDate{3}, error:{4}, stackTrace:{5}",
                            distributorId, distributorOrderingProfile.CNCustCategoryType,
                            distributorOrderingProfile.CNAPFStatus, distributorOrderingProfile.ApfDueDate, ex.Message, ex.StackTrace));
                else
                    HL.Common.Logging.LoggerHelper.Error(
                        string.Format(
                            "there is an error in ShouldShowAPFModule DistributorId{0},isdistributorOrderingProfile Null:{1}",
                            distributorId, "null"));

            }
            return shouldDisplay;
        }

        /// <summary>
        ///     Calculates the number of APFs for which DS is due
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        public static int APFQuantityDue(string distributorId, string locale)
        {
            var quantity = 0;
            if (IsAPFDueAndNotPaid(distributorId, locale))
            {
                quantity = CalcQuantity(GetAPFDueDate(distributorId, locale.Substring(3)));
            }

            return quantity;
        }

        /// <summary>
        ///     Determine the number of APFs due
        /// </summary>
        /// <param name="apfDue"></param>
        /// <returns></returns>
        private static int CalcQuantity(DateTime apfDue)
        {
            var now = DateTime.Now;
            var ts = now.Subtract(apfDue);
            return ((ts.Days / 365) + 1);
        }

        public static bool CanEditAPFOrder(string distributorID, string locale, string level)
        {
            return true; // !HLConfigManager.Configurations.APFConfiguration.AllowAddItemWhenAPFDue;
        }

        public static bool CanAddAPF(string distributorID)
        {
            var canAdd = false;
            var locale = Thread.CurrentThread.CurrentCulture.Name;
            var cart = ShoppingCartProvider.GetShoppingCart(distributorID, locale, true);
            //var quantityInCart = APFQuantityInCart(cart);
            canAdd = (IsAPFDueWithinOneYear(distributorID, locale.Substring(3)) ||
                      (GetDSLevel(distributorID) == "DS" &&
                       IsAPFDueAndNotPaid(distributorID, locale) &&
                       (CalcQuantity(GetAPFDueDate(distributorID, locale.Substring(3))) > APFQuantityInCart(cart))));

            return canAdd;
        }

        public static bool CantDeleteAllAPFs(string distributorID, string level)
        {
            var cantDelete = !IsAPFDueWithinOneYear(distributorID, Thread.CurrentThread.CurrentCulture.Name.Substring(3));

            if (string.IsNullOrEmpty(level))
            {
                level = GetDSLevel();
            }
            if (level == "DS")
            {
                cantDelete = false;
            }
            if (HLConfigManager.Configurations.APFConfiguration.HasExtendedLevelNotAllowToRemoveAPF)
            {
                List<string> DsSubType = HLConfigManager.Configurations.APFConfiguration.DsLevelNotAllowToRemoveAPF.Split(',').ToList();
                if (DsSubType.Contains(level))
                {
                    cantDelete = true;
                }
            }
            return cantDelete;
        }

        public static string GetDSLevel(string memberId)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            string type;
            if (null == member)
            {
                var loader = new DistributorProfileLoader();
                var profile = loader.Load(new GetDistributorProfileById { Id = memberId });
                type = profile.TypeCode;
            }
            else
            {
                type = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.TypeCode;
            }
            return type;
        }

        public static string GetDSLevel()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            return member != null && member.Value != null ? member.Value.TypeCode : string.Empty;
        }

        public static bool GetHapStatus(string memberId)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (null == member)
            {
                var loader = new DistributorProfileLoader();
                var profile = loader.Load(new GetDistributorProfileById { Id = memberId });
                return profile.IsHapStatus;
            }
            else
            {
                return ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.IsHapStatus;
            }
        }

        public static bool GetHapStatus()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            return member.Value.IsHapStatus;
        }

        public static DateTime GetAPFDueDate(string distributorID, string countryCode)
        {
            DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorID, countryCode);
            return distributorOrderingProfile.ApfDueDate;
        }

        public static string GetProcessCountry()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            return member != null ? member.Value.ProcessingCountryCode : null;
        }

        public static string GetProcessCountry(string memberId)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (null == member)
            {
                var loader = new DistributorProfileLoader();
                var profile = loader.Load(new GetDistributorProfileById { Id = memberId });
                return profile.ProcessingCountryCode;
            }
            else
            {
                return ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.ProcessingCountryCode;
            }
        }

        public static bool CanRemoveAPF(string distributorID, string locale, string level)
        {
            var canRemove = false;
            var config = HLConfigManager.Configurations.APFConfiguration;
            if (string.IsNullOrEmpty(level))
            {
                level = GetDSLevel();
            }
            if (IsAPFDueAndNotPaid(distributorID, locale))
            {
                if (config.StandaloneAPFOnlyAllowed && !config.HasExtendedLevelNotAllowToRemoveAPF)
                {
                    if (locale == "en-IN" && level == "DS")
                        canRemove = true;
                    else
                        return false;
                }
                if (level == "DS") //config.AllowDSRemoveAPFWhenDue && level == "DS")
                {
                    canRemove = true;
                }
                if (config.HasExtendedLevelNotAllowToRemoveAPF)
                {
                    List<string> DsSubType = HLConfigManager.Configurations.APFConfiguration.DsLevelNotAllowToRemoveAPF.Split(',').ToList();
                    if (DsSubType.Contains(level))
                    {
                        return false;
                    }
                }
                if (!canRemove)
                {
                    if (config.AllowAddAPF)
                    {
                        canRemove = true;
                    }
                }
            }
            else if(locale == "en-IN" && IsAPFDueWithinOneYear(distributorID, locale.Substring(3)))
            {
                if (level == "DS")
                    canRemove = true;
                else
                    return false;
            }
            else
            {
                canRemove = true;
            }
            return canRemove;
        }

        public static bool ShouldCheckPurchaseLocation(string processingCountryCode, string purchaseCountryCode, List<string> apfRestrictedByPurchaseLocation)
        {
            return (apfRestrictedByPurchaseLocation.Any(x => x == processingCountryCode) || apfRestrictedByPurchaseLocation.Any(x => x == purchaseCountryCode));
        }

        public static bool CanPurchaseApf(string processingCountryCode, string purchaseCountryCode, List<string> apfRestrictedByPurchaseLocation)
        {
            if (
                ShouldCheckPurchaseLocation(processingCountryCode, purchaseCountryCode, apfRestrictedByPurchaseLocation)
                && processingCountryCode != purchaseCountryCode
                && (
                !apfRestrictedByPurchaseLocation.Any(x => x == processingCountryCode)
                || !apfRestrictedByPurchaseLocation.Any(x => x == purchaseCountryCode)))
            {
                return false;
            }
            return true;
        }

        public static bool IsAPFDueAndNotPaid(string distributorID, string locale)
        {
            var isDue = false;

            var cop = GetProcessCountry(distributorID);
            DistributorProfileLoader loader = new DistributorProfileLoader();
            var profile = loader.Load(new GetDistributorProfileById() { Id = distributorID });
            if (cop == null)
            {
                
                cop = profile.ProcessingCountryCode;
            }

            if ((cop.Equals("CN")))
                return IsAPFDueAndNotPaid(distributorID);

            if (IsGlobalExemptCountryOfProcessing(cop))
                return isDue;

            string level = GetDSLevel(distributorID);
            if (string.IsNullOrEmpty(level))
            {
                level = profile.TypeCode;
            }
    
            if (HLConfigManager.Configurations.APFConfiguration.APFRequired)
            {
                DateTime annualProcessingFeeDue = GetAPFDueDate(distributorID, locale.Substring(3));
                if (
                    !HLConfigManager.Configurations.APFConfiguration.ApfExemptCountriesOfProcessing.Contains(
                        cop) && CanPurchaseApf(cop, locale.Substring(3), HLConfigManager.Configurations.APFConfiguration.ApfRestrictedByPurchaseLocation) )
                {
                    // APF due
                    var hmsAPFDue = new DateTime(annualProcessingFeeDue.Year,
                                                 annualProcessingFeeDue.Month,
                                                 annualProcessingFeeDue.Day);
                    var currentDate = DateUtils.ConvertToLocalDateTime(DateTime.Now, locale.Substring(3));
                    if (hmsAPFDue < currentDate)
                    //if (ods.Value.AnnualProcessingFeeDue < DateTime.Now)
                    {
                        isDue = true;
                        if (IsExemptDueToHAPStatus(level, GetHapStatus(distributorID)))
                        {
                            isDue = false;
                        }
                    }
                    if (isDue)
                    {
                        isDue = !GetAPFDuePaid(distributorID, annualProcessingFeeDue);
                    }
                }
            }

            return isDue;
        }


        public static bool IsAPFDueAndNotPaid(string distributorID)
        {
            var isDue = false;

            string level = GetDSLevel();
            if (string.IsNullOrEmpty(level))
            {
                DistributorProfileLoader loader = new DistributorProfileLoader();
                var profile = loader.Load(new GetDistributorProfileById() { Id = distributorID });
                 level = profile.TypeCode;
            }
            if (level == "SP")
            {
                return isDue;
            }
            DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorID, "CN");
            if (distributorOrderingProfile != null)
            {
                if (distributorOrderingProfile.CNAPFStatus != 1 && distributorOrderingProfile.CNAPFStatus != 2)
                    return isDue;
                DateTime annualProcessingFeeDue = distributorOrderingProfile.ApfDueDate;

                // APF due
                var hmsAPFDue = new DateTime(annualProcessingFeeDue.Year,
                                             annualProcessingFeeDue.Month,
                                             annualProcessingFeeDue.Day);
                var currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (hmsAPFDue <= currentDate)
                {
                    isDue = true;
                }
      
                if (isDue)
                {
                    isDue = !GetAPFDuePaid(distributorID, annualProcessingFeeDue);
                }

            }

            return isDue;
        }

        public static bool IsAPFSku(string ProdID)
        {
            return GetAPFSkuList().Contains(ProdID);
        }

        public static bool containsOnlyAPFSku(List<DistributorShoppingCartItem> cartItems)
        {
            var sku = from s in GetAPFSkuList()
                      from c in cartItems
                      where s == c.SKU.Trim()
                      select s;
            return (cartItems.Count == 0 ? false : sku.Count() == cartItems.Count);
        }

        public static bool containsOnlyAPFSku(List<ServiceProvider.CatalogSvc.ShoppingCartItem_V01> cartItems)
        {
            var sku = from s in GetAPFSkuList()
                      from c in cartItems
                      where s == c.SKU.Trim()
                      select s;
            return (cartItems.Count == 0 ? false : sku.Count() == cartItems.Count);
        }

        public static bool hasOnlyAPFSku(ServiceProvider.CatalogSvc.ShoppingCartItemList cartItems, string locale)
        {
            var sku = from s in GetAPFSkuList()
                      from c in cartItems
                      where s == c.SKU.Trim()
                      select s;
            return (cartItems.Count == 0 ? false : sku.Count() == cartItems.Count);
        }

        public static List<string> GetAPFSkusFromCart(ShoppingCartItemList cartItems)
        {
            var sku = from s in GetAPFSkuList()
                      from c in cartItems
                      where s == c.SKU.Trim()
                      select s;
            return sku.ToList();
        }

        public static bool IsAPFSkuPresent(ShoppingCartItemList cartItems)
        {
            var sku = from s in GetAPFSkuList()
                      from c in cartItems
                      where s == c.SKU.Trim()
                      select s;
            return sku.Count() != 0;
        }

        public static bool IsAPFSkuPresent(List<DistributorShoppingCartItem> cartItems)
        {
            var skuList = from s in GetAPFSkuList()
                          from c in cartItems
                          where s == c.SKU.Trim()
                          select s;
            return skuList.Count() != 0;
        }

        public static int APFQuantityInCart(MyHLShoppingCart cart)
        {
            var apfs = from s in GetAPFSkuList()
                       from c in cart.CartItems
                       where s == c.SKU.Trim()
                       select c;

            return (apfs.Count() > 0) ? apfs.First().Quantity : 0;
        }

        public static string GetAPFSku()
        {
            string level = GetDSLevel();
            if (level == "SP")
            {
                return HLConfigManager.Configurations.APFConfiguration.SupervisorSku;
            }
            else
            {
                return HLConfigManager.Configurations.APFConfiguration.DistributorSku;
            }
        }

        public static string GetAPFSku(string memberId)
        {
            string level = GetDSLevel(memberId);
            if (level == "SP")
            {
                return HLConfigManager.Configurations.APFConfiguration.SupervisorSku;
            }
            else
            {
                return HLConfigManager.Configurations.APFConfiguration.DistributorSku;
            }
        }

        public static List<string> GetAPFSkuList()
        {
            var APFSKUs = new List<string>();
            try
            {
                var apfConfig = HLConfigManager.Configurations.APFConfiguration;
                if (apfConfig != null)
                {
                    APFSKUs.AddRange(new string[] { apfConfig.SupervisorSku, apfConfig.DistributorSku });

                    if (!string.IsNullOrEmpty(apfConfig.AlternativeSku))
                    {
                        APFSKUs.Add(apfConfig.AlternativeSku);
                    }

                    APFSKUs = APFSKUs.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }

            return APFSKUs;
        }

        public static bool IsAPFDueWithinOneYear(string distributorId, string countryCode)
        {
            var isDue = false;
            //DateTime now = OrderCreationHelper.GetCurrentLocalTime();
            var apfConfig = HLConfigManager.Configurations.APFConfiguration;
            if (null != apfConfig)
            {
                if (IsExemptDueToHAPStatus(GetDSLevel(distributorId), GetHapStatus(distributorId)) || IsGlobalExemptCountryOfProcessing(distributorId))
                {
                    isDue = false;
                }
                else
                {
                    var utcDue = GetAPFDueDate(distributorId, countryCode).ToUniversalTime();
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && utcDue.Date >= DateTime.UtcNow.Date && ((utcDue - DateTime.UtcNow) < new TimeSpan(365, 0, 0, 0, 0)))
                    {
                        isDue = true;
                    }
                    else
                    {
                        if (utcDue.Date >= DateTime.UtcNow.Date && ((utcDue - DateTime.UtcNow).Days < 91))
                        {
                            isDue = true;
                        }
                    }
                }
            }

            return isDue;
        }

        public static bool IsAPFDueGreaterThanOneYear(string distributorId, string countryCode)
        {
            var isDue = false;
            //DateTime now = OrderCreationHelper.GetCurrentLocalTime();
            var apfConfig = HLConfigManager.Configurations.APFConfiguration;
            if (null != apfConfig)
            {

                // TODO : get HAP Status from proper profile
                if (IsExemptDueToHAPStatus(GetDSLevel(), false) || IsGlobalExemptCountryOfProcessing(distributorId))
                {
                    isDue = false;
                }
                else
                {
                    var utcDue = GetAPFDueDate(distributorId, countryCode);
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && utcDue > DateTime.UtcNow + new TimeSpan(366, 0, 0, 0, 0))
                    {
                        isDue = true;
                    }
                    else
                    {
                        if (utcDue > DateTime.UtcNow + new TimeSpan(91, 0, 0, 0, 0))
                        {
                            isDue = true;
                        }
                    }
                }

            }

            return isDue;
        }

        private static bool IsExemptDueToHAPStatus(string DSLevel, bool isHap)
        {
            var isExempt = false;
            if (isHap)
            {
                //if (levelType.LevelGroup.Key == "DS" && new List<string>(new string[] { "DS", "SC", "SB", "QP" }).Contains(levelType.Key))
                if (DSLevel == "DS")
                {
                    isExempt = true;
                }
            }

            return isExempt;
        }

        /// <summary>
        /// Indicates when the APF is exempted for a country that is not the COP.
        /// </summary>
        /// <param name="memberId">The member Id.</param>
        /// <returns></returns>
        private static bool IsGlobalExemptCountryOfProcessing(string memberId)
        {
            var cop = GetProcessCountry(memberId);
            return Settings.GetRequiredAppSetting("ApfGlobalExemptCountriesOfProcessing", string.Empty).Contains(cop) && !(Thread.CurrentThread.CurrentCulture.ToString().Substring(3).Equals(cop));
        }

        public static bool ShouldHideOrderQuickView(MyHLShoppingCart cart)
        {
            if (cart == null)
            {
                HL.Common.Logging.LoggerHelper.Error("APFDueProvider.ShouldHideOrderQuickView() : Null MyHLShoppingCart. Check that service can return a valid cart");
            }

            var hide = false;
            if (cart != null && cart.OrderCategory != OrderCategoryType.ETO) //We don't hassle them for ETOs
            {
                if (HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                {
                    if (!HLConfigManager.Configurations.APFConfiguration.ShowOrderQuickViewForStandaloneAPF)
                    {
                        if (IsAPFSkuPresent(cart.CartItems) || IsAPFDueAndNotPaid(cart.DistributorID, cart.Locale))
                        {
                            hide = true;
                        }
                    }
                }
            }

            return hide;
        }
    }
}