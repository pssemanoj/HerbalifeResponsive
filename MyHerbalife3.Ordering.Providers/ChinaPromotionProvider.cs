using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class PromotionInformation
    {
        public List<MonthlyAmount> MonthlyInfo { get; set; }
        public List<CatalogItem> SKUList { get; set; }
        public bool IsEligible { get; set; }
        public bool GotPromoSKU { get; set; }
        public PromotionElement promoelement { get; set; }
    }

    public static partial class ChinaPromotionProvider
    {
        private static string Locale
        {
            get { return Thread.CurrentThread.CurrentCulture.ToString(); }
        }
        private static string _distributorID
        {
            get
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                return member.ToString();

            }
        }

        public static int DSFIRSTORDER_CACHE_MINUTES =
            Settings.GetRequiredAppSetting<int>("DSFirstOrderCacheMinutes");
        public static bool IsSKUDeletable(string distributorID, string sku)
        {
            var promoInfo = Settings.GetRequiredAppSetting("ChinaBadgePromo", string.Empty).Split(',');
            if (promoInfo != null)
            {
                if (promoInfo.Contains(sku))
                    return true;
            }
            return false;
        }
        private static DateTime convertDateTime(string datetime)
        {
            const string format = "MM-dd-yyyy";
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime dt;
                if (DateTime.TryParseExact(datetime, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt))
                {
                    return dt;
                }
            }
            return DateTime.MaxValue;
        }

        public static PromotionInformation GetPCPromotion(string customerProfileID, string memberId)
        {
            var cacheKey = string.Format("CN_{0}_{1}", "PCPromotionInfo", customerProfileID.Trim());
            var result =
                CacheFactory.Create().Retrieve(_ => getPCPromotion(customerProfileID, memberId), cacheKey,
                    TimeSpan.FromMinutes(30));
            return result;
        }

        public static PromotionInformation getPCPromotion(string customerProfileID, string memberId)
        {
            PromotionCollection promoColl =
                PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform,
                    "zh-CN");

            PromotionElement pcPromo = new PromotionElement();

            if (promoColl == null)
                return null;

            string sessionkey = PCPromoSessionKey(customerProfileID);

            bool isEligible = false;
            //sessionkey = string.Format("CN_{0}_{1}", "PCPromotionInfo", customerProfileID.Trim());
            var promotionInformation = new PromotionInformation();
            promotionInformation.SKUList = new List<CatalogItem>();
            //var sessionPromo = HttpContext.Current.Session[sessionkey] as PromotionInformation;
            //if (sessionPromo == null)
            //{
            foreach (var promo in promoColl)
            {
                if ((promo.PromotionType & PromotionType.Other) == PromotionType.Other &&
                    ((promo.Code == "PCPromo")))
                {
                    pcPromo = promo;
                    if (promo.SelectableSKUList == null || !promo.SelectableSKUList.Any())
                    {

                        foreach (var sku in promo.FreeSKUList)
                        {
                            var item = CatalogProvider.GetCatalogItem(sku.SKU, "CN");
                            if (item != null)
                                promotionInformation.SKUList.Add(item);
                        }
                        var result = IsEligible(customerProfileID, convertDateTime(promo.StartDate), convertDateTime(promo.EndDate),
                            promo.FreeSKUList.Select(f => f.SKU).ToList(), promo.AmountMinInclude,
                            promo.NumOfMonth, true);
                        isEligible = result.IsEligible;
                        promotionInformation.promoelement = promo;
                        promotionInformation.MonthlyInfo = result.Amounts;
                    }

                }
            }

            //}
            //else
            //{
            //    promotionInformation = sessionPromo;
            //}
            //var response = HttpContext.Current.Session[sessionkey] as GetPCPromotionResponse_V01;
            //if (response == null)
            //    return null;

            //var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(memberId, "CN");
            if (pcPromo.CustTypeList.Contains(distributorOrderingProfile.CNCustType))
            //if (pcPromo.CustTypeList.Contains(_distributorID))
            {
                promotionInformation.IsEligible = isEligible;
            }

            return promotionInformation;

        }

        public static PromotionInformation GetChinaPromotion(string distributorId)
        {
            PromotionCollection promoColl = LoadPromoConfig();
                //PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform,
                //    "zh-CN");
            string sessionkey = string.Format("CN_{0}_{1}", "ChinaPromotionInfo", distributorId.Trim());
            if (promoColl == null)
                return null;
            var promotionInformation = new PromotionInformation();
            promotionInformation.SKUList = new List<CatalogItem>();

            object sessionChinaPromo = null;
            if (null != HttpContext.Current.Session)
            {
                sessionChinaPromo = HttpContext.Current.Session[sessionkey];
            }
            if (sessionChinaPromo == null)
            {
                foreach (var promo in promoColl)
                {
                    if ((promo.PromotionType & PromotionType.Volume) == PromotionType.Volume &&
                        ((promo.Code == "SRChinaPromo")))
                    {
                        if (promo.SelectableSKUList == null || !promo.SelectableSKUList.Any())
                        {

                            foreach (var sku in promo.FreeSKUList)
                            {
                                var item = CatalogProvider.GetCatalogItem(sku.SKU, "CN");
                                if (item != null)
                                {
                                    var cacheKey = string.Format("GetSRPromoDetail_{0}", distributorId);
                                    var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
                                    if (results != null)
                                    {
                                        var skuList = results.Skus.Split(',').ToArray();
                                        if (!string.IsNullOrWhiteSpace(results.Skus) && skuList.Contains(item.SKU.Trim()))
                                        {
                                            promotionInformation.SKUList.Add(item);
                                        }
                                    }
                                }
                            }

                        }

                    }
                    else if ((promo.PromotionType & PromotionType.Special) == PromotionType.Special && (promo.Code == "ChinaBadgePromo"))
                    {
                        foreach (var sku in promo.FreeSKUList)
                        {
                            var item = CatalogProvider.GetCatalogItem(sku.SKU, "CN");
                            if (item != null)
                            {
                                var cacheKey = string.Format("GetBadgePromoDetail_{0}", distributorId);
                                var results = HttpRuntime.Cache[cacheKey] as GetBadgePinResponse_V01;
                                if (results != null)
                                {
                                    foreach (var Sku in results.BadgeDetails)
                                    {
                                        if (Sku.BadgeCode.Trim() == item.SKU)
                                        {
                                            promotionInformation.SKUList.Add(item);
                                        }

                                    }
                                }
                            }
                        }

                    }
                }
                if (null != HttpContext.Current.Session)
                {
                    HttpContext.Current.Session[sessionkey] = promotionInformation;
                }
            }
            else
            {
                if (null != HttpContext.Current.Session)
                {
                    return sessionChinaPromo as PromotionInformation;
                }
            }

            return promotionInformation;

        }

        public static bool IsEligible(string distributorID)
        {
            string sessionkey = PCPromoSessionKey(distributorID);
            if (HttpContext.Current.Session[sessionkey] == null)
            {
                return false;
            }
            var response = HttpContext.Current.Session[sessionkey] as GetPCPromotionResponse_V01;
            return response != null && response.IsEligible;
        }

        public static decimal GetCurrentMonthAmount(List<MonthlyAmount> amounts)
        {
            if (amounts != null)
            {
                DateTime currentDateTime = DateUtils.GetCurrentLocalTime("CN");
                MonthlyAmount amt =
                    amounts.Find(
                        a => a.Month.Month == currentDateTime.Month && a.Month.Year == currentDateTime.Year);
                if (amt != null)
                {
                    return amt.Amount;
                }
            }
            return decimal.Zero;
        }

        public static int GetNumberMonthQualified(List<MonthlyAmount> amounts, decimal shoppingCartTotal,
            decimal maxAmount)
        {
            decimal currentMonthAmount = GetCurrentMonthAmount(amounts);

            if (amounts != null)
            {
                int count = amounts.Count(x => x.Excess);
                if (currentMonthAmount < maxAmount && (currentMonthAmount + shoppingCartTotal) >= maxAmount)
                    count++;
                return count;
            }
            return 0;
        }

        public static void SetEligible(string distributorID, bool eligible)
        {
            string sessionkey = PCPromoSessionKey(distributorID);
            if (HttpContext.Current.Session[sessionkey] != null)
            {
                GetPCPromotionResponse_V01 response =
                    HttpContext.Current.Session[sessionkey] as GetPCPromotionResponse_V01;
                response.IsEligible = eligible;

            }
        }

        public static string PCPromoSessionKey(string distributorID)
        {
            return string.Format("CN_{0}_{1}", "PCPromo", distributorID.Trim());
        }

        public static void ReloadPCPromotion(string distributorID)
        {
            string sessionkey = PCPromoSessionKey(distributorID);
            HttpContext.Current.Session[sessionkey] = null;

        }

        public static GetPCPromotionResponse_V01 IsEligible(string customerProfileID, DateTime startDate,
            DateTime endDate, List<string> skuList, decimal limit,
            int numOfMonth, bool onlineOnly)
        {
            string sessionkey = PCPromoSessionKey(customerProfileID);
            //if (HttpContext.Current.Session[sessionkey] == null)
            //{
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new GetPCPromotionRequest_V01
                    {
                        CustomerProfileID = customerProfileID,
                        PromoStartDate = startDate,
                        FreeSKUList = skuList,
                        NumberOfMonth = numOfMonth,
                        Range = PromotionRange.Any,
                        VolumePointLimit = limit,
                        InternetOrderOnly = onlineOnly,
                        PromoEndDate = endDate,
                    };
                    var response =
                        proxy.GetPCPromotion(new GetPCPromotionRequest1(request)).GetPCPromotionResult as GetPCPromotionResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        //HttpContext.Current.Session[sessionkey] = response;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "InsertOrder failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            //}
            //else
            //{
            //    var response =
            //        HttpContext.Current.Session[sessionkey] as GetPCPromotionResponse_V01;
            //    return response ?? new GetPCPromotionResponse_V01();
            //}
            return new GetPCPromotionResponse_V01();
        }

        public static bool SaveEligibleForPromoToFromService(EligibleDistributorInfo_V01 promoInfo, int shoppingCartId,
            string orderNumber)
        {
            if (promoInfo == null || string.IsNullOrEmpty(promoInfo.DistributorId) ||
                string.IsNullOrEmpty(promoInfo.Locale) ||
                (shoppingCartId <= 0 && string.IsNullOrEmpty(orderNumber)))
            {
                return false;
            }


            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new UpdatePromotionRequest_V01
                    {
                        DistributorID = promoInfo.DistributorId,
                        Quantity = promoInfo.Quantity,
                        Locale = promoInfo.Locale,
                        SKU = promoInfo.Sku,
                        OrderNumber = orderNumber,
                        Platform = HLConfigManager.Platform,
                        ShoppingCartID = shoppingCartId,
                    };

                    var response = proxy.UpdatePromotion(new UpdatePromotionRequest1(request)).UpdatePromotionResult as UpdatePromotionResponse_V01;
                    if (response != null)
                    {
                        return response.Updated;
                    }
                }
                catch (Exception ex)
                {

                    HL.Common.Logging.LoggerHelper.Error(
                        string.Format("SaveEligibleForPromoToFromService DistributorId:{0} Locale:{1} ERR:{2}",
                            promoInfo.DistributorId, promoInfo.Locale, ex.ToString()));
                }
            }
            return false;
        }

        public static List<PromotionType> IsFirstOrder(string distributorID)
        {
            var PromotionTypes = new List<PromotionType>();
            PromotionTypes.Add(PromotionType.None);
            var cacheKey = string.Format("DSFirstOrder_{0}", distributorID);
            GetIsFirstOrderResponse_V01 results = HttpRuntime.Cache[cacheKey] as GetIsFirstOrderResponse_V01;
            if (results == null)
            {
                results = GetFirstOrderFromService(distributorID);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(DSFIRSTORDER_CACHE_MINUTES),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                    return results.PromotionTypes;
                }
                return PromotionTypes;
            }
            return results.PromotionTypes;

        }

        private static GetIsFirstOrderResponse_V01 GetFirstOrderFromService(string distributorID)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new GetIsFirstOrderRequest_V01
                    {
                        CustomerID = distributorID

                    };
                    var response =
                        proxy.GetIsFirstOrder(new GetIsFirstOrderRequest1(request)).GetIsFirstOrderResult as GetIsFirstOrderResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "InsertOrder failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static bool IsEligibleForPCPromoSurveyFreeSku(string distributorID, string CountryCode,
            DateTime startdate, DateTime enddate, string promoSkus, string promotionCode)
        {
            bool result = false;

            List<MyHLShoppingCartView> listofOrders = new MyHLShoppingCartView().GetOrdersWithDetail(distributorID,
                DistributorOrderingProfileProvider.GetProfile(distributorID, CountryCode).CNCustomorProfileID,
                CountryCode, startdate, enddate, China.OrderStatusFilterType.All, "", "", true);

            int totalFreePromoOrders = listofOrders.Count(z => z.CartItems.Any(x => x.SKU == promoSkus));

            if (totalFreePromoOrders < 3)
            {
                if (totalFreePromoOrders != 0)
                {
                    result = true;
                }
                else
                {
                    if (HttpContext.Current.Session["AttainedSurvey"] == null ||
                        !Convert.ToBoolean(HttpContext.Current.Session["AttainedSurvey"]))
                    {
                        if (GetPCPromoSurveyDetail(distributorID, CountryCode) == null)
                        {
                            if (HttpContext.Current.Session["SurveyCancelled"] == null ||
                                !Convert.ToBoolean(HttpContext.Current.Session["SurveyCancelled"]))
                            {
                                HttpContext.Current.Response.Redirect("Survey.aspx?@ctrl=" + promotionCode, true);
                            }
                            else if (Convert.ToBoolean(HttpContext.Current.Session["SurveyCancelled"]))
                            {
                                return false;
                            }
                        }

                        result = true;
                    }
                    else
                    {
                        HttpContext.Current.Session["AttainedSurvey"] = null;
                        result = true;
                    }
                }
            }
            return result;
        }

        public static void SubmitPCPromoSurvey(string distributorID, string country, DateTime surveydate,
            DateTime promotionalstartdate, DateTime promotionalenddate, string surveyname, string surveyreport)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new InsertPromoSurveyRequest_V01
                    {
                        Country = country,
                        DistributorId = distributorID,
                        PromotionalEndDate = promotionalenddate,
                        PromotionalStartDate = promotionalstartdate,
                        SurveyDate = surveydate,
                        SurveyName = surveyname,
                        SurveyReport = surveyreport
                    };
                    var response =
                        proxy.InsertPCPromoSurvey(new InsertPCPromoSurveyRequest(request)).InsertPCPromoSurveyResult as InsertPromoSurveyResponse_V01;
                    if (!(response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success))
                    {
                        HL.Common.Logging.LoggerHelper.Error(
                            string.Format("SubmitPCPromoSurvey DistributorId:{0} Locale:{1} Status: {2}",
                                distributorID, country, (response == null ? string.Empty : response.Status.ToString())));
                    }

                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "InsertPromoSurvey failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

        }


        public static DistributorSurveyDetail GetPCPromoSurveyDetail(string distributorID, string country)
        {
            var cacheKey = string.Format("GetPCPromoSurveyDetail_{0}", distributorID);
            var results = HttpRuntime.Cache[cacheKey] as DistributorSurveyDetail;
            if (results == null)
            {
                results = GetPCPromoSurveyDetailLoadFromService(distributorID, country);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                }
            }
            return results;
        }

        public static DistributorSurveyDetail GetPCPromoSurveyDetailLoadFromService(string distributorID, string country)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new GetPCPromoSurveyRequest_V01 { Country = country, DistributorId = distributorID };
                    var response =
                        proxy.GetPCPromoSurveyDetail(new GetPCPromoSurveyDetailRequest(request)).GetPCPromoSurveyDetailResult as GetPCPromoSurveyResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response.distributorSurveyDetail;
                    }
                    HL.Common.Logging.LoggerHelper.Error(
                        string.Format("GetPCPromoSurveyDetail DistributorId:{0} Locale:{1} ",
                            distributorID, country));
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "InsertPromoSurvey failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;

        }

        public static bool GetPCPromoSkus(string Sku)
        {
            // var PromoCol = new PromotionCollection();
            var PromoCol = PromotionCollectionCacheMedia as PromotionCollection;
            if (PromoCol == null)
            {
                if (HL.Common.Configuration.Settings.GetRequiredAppSetting("LoadPromotionsFromDb") == "1")
                {

                    LoadPromotionFromDb();
                }

                else
                {

                    PromoCol = PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform,
                     "zh-CN");
                }
            }

            if (PromoCol == null)
                return false;

            var promotionInformation = new PromotionInformation();
            promotionInformation.SKUList = new List<CatalogItem>();
            foreach (
                var item in
                    PromoCol.Where(promo => promo.FreeSKUList != null)
                        .SelectMany(
                            promo =>
                                promo.FreeSKUList.Select(sku => CatalogProvider.GetCatalogItem(sku.SKU, "CN"))
                                    .Where(item => item != null)))
            {
                promotionInformation.SKUList.Add(item);
            }
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (null != member)
            {
                var currentsession = SessionInfo.GetSessionInfo(member.UserName, "zh-CN");
                if (currentsession != null)
                {
                    if (currentsession.surveyDetails != null &&
                        currentsession.surveyDetails.SurveySKU.Trim().Equals(Sku) &&
                        !currentsession.surveyDetails.SurveyCompleted)
                    {
                        promotionInformation.SKUList.Add(CatalogProvider.GetCatalogItem(Sku, "CN"));
                    }

                }
            }
            return promotionInformation.SKUList.Find(p => p.SKU == Sku) != null;
        }

        public static string GetPCPromoCode(string Sku)
        {
            var promoColl = LoadPromoConfig();
                //PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform, "zh-CN");
            if (promoColl == null)
                return string.Empty;
            var promoCode = (from coll in promoColl
                             from list in coll.FreeSKUList
                             where list.SKU == Sku
                             select coll.Code).FirstOrDefault();

            return promoCode ?? string.Empty;
        }

        public static PromotionResponse_V01 GetEffectivePromotionList(string Local, DateTime? dateTime = null)
        {

            var proxy2 = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var req = new PromotionRequest_V01
                {
                    Local = Local,
                    DateTime = dateTime
                };
                var res = proxy2.GetEffectivePromotionList(new GetEffectivePromotionListRequest(req)).GetEffectivePromotionListResult as PromotionResponse_V01;
                if (res != null && res.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return res;
                }
                else
                {
                    var message = string.Format("GetEffectivePromotionList() Error . Local={0}", Local,
                        res != null ? res.Message : "Response Is Null");
                    HL.Common.Logging.LoggerHelper.Error(message);

                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetEffectivePromotionList failed: China Promotion Provier  Local={0}, exception: {1} ",
                        Local[0], ex));
            }
            return null;


        }

        public const string SessionKey = "PromotionInfo";

        private static object PromotionCollectionCacheMedia
        {
            // In web-api mode, HttpContext.Current.Session will be null, so use cache instead
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session == null)
                    return HttpContext.Current.Cache[SessionKey];

                return HttpContext.Current.Session[SessionKey];
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session == null)
                {
                    var chk = HttpContext.Current.Cache[SessionKey];
                    if (chk != null)
                    {
                        HttpContext.Current.Cache[SessionKey] = value;
                        return;
                    }

                    int afterMinutes = 60;

                    var cm = Settings.GetRequiredAppSetting("PromotionsCacheReloadAfterMinutes_Mobile");
                    if (cm != null) int.TryParse(cm, out afterMinutes);

                    DateTime absoluteExpiration = DateTime.Now.AddMinutes(afterMinutes);

                    HttpContext.Current.Cache.Add(SessionKey, value, null
                        , absoluteExpiration
                        , Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);

                    return;
                }

                HttpContext.Current.Session[SessionKey] = value;
            }
        }

        public static PromotionCollection LoadPromoConfig()
        {
            if (Settings.GetRequiredAppSetting("LoadPromotionsFromDb") == "1")
            {
                var DBinfo = PromotionCollectionCacheMedia as PromotionCollection;
                if (DBinfo == null)
                {
                    return LoadPromotionFromDb();
                }
            }
            else
            {

                var info = PromotionCollectionCacheMedia as PromotionCollection;
                if (info == null)
                {
                    info = PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform, "zh-CN");
                    PromotionCollectionCacheMedia = info;
                }
                return GetPromotion();
            }
            return PromotionCollectionCacheMedia as PromotionCollection;
        }

        private static PromotionCollection GetPromotion()
        {
            var promotion = PromotionCollectionCacheMedia as PromotionCollection;

            PromotionCollection col = new PromotionCollection();
            if (promotion == null) return col;
            DateTime currentDateTime = DateUtils.GetCurrentLocalTime("CN").Date;
            foreach (var promoItem in promotion)
            {
                DateTime startDateTime = convertDateTime(promoItem.StartDate);
                DateTime endDateTime = convertDateTime(promoItem.EndDate);
                if (startDateTime == DateTime.MaxValue || endDateTime == DateTime.MaxValue) continue;
                if (startDateTime <= currentDateTime && endDateTime >= currentDateTime)
                {
                    col.Add(promoItem);
                }
            }
            return col;
        }



        private static PromotionCollection LoadPromotionFromDb()
        {
            var promotion = GetEffectivePromotionList(Locale, DateTime.Now);
            string config = promotion.Promotion_Info;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(config);
            XmlNodeList publicationsNodeList = xmlDoc.SelectNodes("Promotion");
            XmlNodeList child = publicationsNodeList[0].SelectNodes("Promo");

            PromotionCollection ret = new PromotionCollection();
            foreach (XmlElement y in child)
            {
                PromotionElement pe = new PromotionElement();
                if (y.Attributes["code"] != null) pe.Code = y.Attributes["code"].Value;
                if (y.Attributes["startDate"] != null) pe.StartDate = y.Attributes["startDate"].Value;
                if (y.Attributes["endDate"] != null) pe.EndDate = y.Attributes["endDate"].Value;
                if (y.Attributes["promotionType"] != null) pe.PromotionType = ConvertToPromotionType(y.Attributes["promotionType"].Value);
                if (y.Attributes["custTypeList"] != null) pe.CustTypeList = (y.Attributes["custTypeList"].Value.Split(',').ToList());
                if (y.Attributes["custCategoryTypeList"] != null) pe.CustCategoryTypeList = (y.Attributes["custCategoryTypeList"].Value.Split(',').ToList());
                if (y.Attributes["amountMinInclude"] != null) pe.AmountMinInclude = Convert.ToDecimal(y.Attributes["amountMinInclude"].Value);
                if (y.Attributes["deliveryTypeList"] != null) pe.DeliveryTypeList = (y.Attributes["deliveryTypeList"].Value.Split(',').ToList());
                if (y.Attributes["excludedExpID"] != null) pe.excludedExpID = (y.Attributes["excludedExpID"].Value.Split(',').ToList());
                if (y.Attributes["hasIncrementaldegree"] != null) pe.HasIncrementaldegree = Convert.ToBoolean(y.Attributes["hasIncrementaldegree"].Value);
                if (y.Attributes["volumeMinInclude"] != null) pe.VolumeMinInclude = Convert.ToDecimal(y.Attributes["volumeMinInclude"].Value);
                if (y.Attributes["freeSKUList"] != null) pe.FreeSKUList = ConvertToFreeSkuCollection1(y.Attributes["freeSKUList"].Value);
                if (y.Attributes["selectableSKUList"] != null) pe.SelectableSKUList = ConvertToFreeSkuCollection1(y.Attributes["selectableSKUList"].Value);
                if (y.Attributes["freeSKUListForSelectableSku"] != null) pe.FreeSKUListForSelectableSku = ConvertToFreeSkuCollection1(y.Attributes["freeSKUListForSelectableSku"].Value);
                if (y.Attributes["onlineOrderOnly"] != null) pe.OnlineOrderOnly = Convert.ToBoolean(y.Attributes["onlineOrderOnly"].Value);
                if (y.Attributes["maxFreight"] != null) pe.MaxFreight = Convert.ToDecimal(y.Attributes["maxFreight"].Value);
                if (y.Attributes["dsStoreProvince"] != null) pe.DSStoreProvince = (y.Attributes["dsStoreProvince"].Value.Split(',').ToList());
                if (y.Attributes["shippedToProvince"] != null) pe.ShippedToProvince = (y.Attributes["shippedToProvince"].Value.Split(',').ToList());
                if (y.Attributes["amountMaxInclude"] != null) pe.AmountMaxInclude = Convert.ToDecimal(y.Attributes["amountMinInclude"].Value);
                if (y.Attributes["volumeMaxInclude"] != null) pe.VolumeMaxInclude = Convert.ToDecimal(y.Attributes["volumeMaxInclude"].Value);
                if (y.Attributes["amountMax"] != null) pe.AmountMax = Convert.ToDecimal(y.Attributes["amountMax"].Value);
                if (y.Attributes["amountMin"] != null) pe.AmountMin = Convert.ToDecimal(y.Attributes["amountMin"].Value);
                if (y.Attributes["volumeMax"] != null) pe.VolumeMax = Convert.ToDecimal(y.Attributes["volumeMax"].Value);
                if (y.Attributes["volumeMin"] != null) pe.VolumeMin = Convert.ToDecimal(y.Attributes["volumeMin"].Value);
                if (y.Attributes["YearlyPromo"] != null) pe.YearlyPromo = Convert.ToBoolean(y.Attributes["YearlyPromo"].Value);

                ret.Add(pe);
                PromotionCollectionCacheMedia = ret;
            }
            return ret;
        }
        public static YearlyPromoResponse_V01 CheckEligibleYearlyPromo(int distributorProfileID)
        {
            var res = new YearlyPromoResponse_V01();
            res.IsEgibility = false;

            var proxy2 = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var req = new YearlyPromoRequest_V01
                {
                    DistributorProfileID = distributorProfileID,

                };
                res = proxy2.CheckEligibleYearlyPromo(new CheckEligibleYearlyPromoRequest(req)).CheckEligibleYearlyPromoResult as YearlyPromoResponse_V01;
                if (res != null && res.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return res;
                }
                else
                {
                    var message = string.Format("CheckEligibleYearlyPromo() Error . distributorProfileID={0}", distributorProfileID,
                        res != null ? res.Message : "Response Is Null");
                    HL.Common.Logging.LoggerHelper.Error(message);

                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "CheckEligibleYearlyPromo failed: China order Provier  distributorProfileID={0}, exception: {1} ",
                        distributorProfileID, ex));
            }
            return res;


        }



        private static FreeSKUCollection ConvertToFreeSkuCollection1(string value)
        {
            var ret = new FreeSKUCollection();
            string freeSkUs = value;
            if (!string.IsNullOrEmpty(freeSkUs))
            {
                string[] skuQtyArr = freeSkUs.Split(',');
                foreach (var l in skuQtyArr)
                {
                    string[] skuQty = l.Split('|');
                    if (skuQty.Length != 0)
                    {
                        ret.Add(new FreeSKU { Quantity = int.Parse(skuQty[1]), SKU = skuQty[0] });
                    }
                }
            }
            return ret;
        }

        private static PromotionType ConvertToPromotionType(string val)
        {
            var ptList = val.Split(',').ToList();
            PromotionType ret = PromotionType.None;
            var vals = Enum.GetValues(typeof(PromotionType));
            foreach (var v in vals)
            {
                var ptStr = v.ToString();
                PromotionType pt;
                if (!Enum.TryParse(ptStr, out pt)) continue;
                if (ptList.Contains(ptStr)) ret = ret | pt;
            }
            return ret;
        }

        public static bool IsEligibleForSRPromotion(MyHLShoppingCart cart, string platform)
        {
            var cacheKey = string.Format("GetSRPromoDetail_{0}", cart.DistributorID);
            var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
            if (results == null)
            {
                results = GetSRPromoFromService(cart, platform);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                    if (!string.IsNullOrWhiteSpace(results.Skus))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(results.Skus))
            {
                return true;
            }
            else
            {
                return false;
            }



        }
        public static GetSRPromotionResponse_V01 GetSRPromoFromService(MyHLShoppingCart shoppingcart, string platform)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {

                try
                {

                    var request = new GetSRPromotionRequest_V01
                    {
                        CustomerID = shoppingcart.DistributorID,
                        OrderAmount = shoppingcart.Totals != null ? Convert.ToDouble((shoppingcart.Totals as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01).AmountDue) : 0,
                        OrderMonth = shoppingcart.OrderMonth.ToString(),
                        Platform = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                        VolumePoint = Convert.ToDouble(shoppingcart.VolumeInCart)
                    };
                    var response =
                        proxy.GetSRPromotionDetail(new GetSRPromotionDetailRequest { request = request }).GetSRPromotionDetailResult as GetSRPromotionResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response;
                    }
                    HL.Common.Logging.LoggerHelper.Info(
                        string.Format("GetSRPromoDetail DistributorId:{0} Locale:{1} ",
                            shoppingcart.DistributorID, shoppingcart.Locale));
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "Unable to get SR Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;

        }

        public static bool LockSRPromotion(MyHLShoppingCart shoppingcart, string ordernumber = null)
        {
            if (shoppingcart != null && IsEligibleForSRPromotion(shoppingcart, HLConfigManager.Platform))
            {
                using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    var cacheKey = string.Format("GetSRPromoDetail_{0}", shoppingcart.DistributorID);
                    var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
                    try
                    {
                        string orderid = string.Empty;
                        var currentsession = SessionInfo.GetSessionInfo(shoppingcart.DistributorID, shoppingcart.Locale);
                        if (currentsession != null)
                        {
                            orderid = currentsession.OrderNumber;
                            if (!string.IsNullOrWhiteSpace(ordernumber))
                            {
                                orderid = ordernumber;
                            }
                        }
                        var request = new GetSRPromotionRequest_V01
                        {
                            CustomerID = shoppingcart.DistributorID,
                            OrderMonth = shoppingcart.OrderMonth.ToString(),
                            OrderNumber = orderid,
                            Platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                            PromotionCode = results.PromotionCode,
                            VolumePoint = Convert.ToDouble(shoppingcart.VolumeInCart)
                        };
                        var response =
                            proxy.LockSRPromotionDetail(new LockSRPromotionDetailRequest { request = request }).LockSRPromotionDetailResult as GetSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {
                            HttpRuntime.Cache.Remove(string.Format("GetSRPromoDetail_{0}", shoppingcart.DistributorID));
                            return response.IsLocked;
                        }
                        HL.Common.Logging.LoggerHelper.Info(
                      string.Format("GetSRPromoDetail DistributorId:{0} Locale:{1} ",
                          shoppingcart.DistributorID, shoppingcart.Locale));
                    }
                    catch (Exception ex)
                    {
                        ex =
                            new ApplicationException(
                                "Unable to get SR Promotion: China Order Provier", ex);
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        return false;
                    }

                }
            }
            return false;
        }

        public static bool IsEligibleForBrochurePromotion(MyHLShoppingCart cart, string platform, string memberId)
        {

            var cacheKey = string.Format("GetBrochurePromoDetail_{0}", memberId);
            var results = HttpRuntime.Cache[cacheKey] as BrochurePromotionResponse_V01;
            if (results == null)
            {
                results = GetBrochurePromotion(cart, platform, memberId);
                if (results != null&& results.HasPromotion)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                       results,
                       null,
                       DateTime.Now.AddMinutes(20),
                       Cache.NoSlidingExpiration,
                       CacheItemPriority.Low,
                       null);
                    return results.HasPromotion;
                }
                else
                {
                    return false;
                }
               
            }
            else
            {
                return results.HasPromotion;
            }

        }

        public static bool IsEligibleForBadgePromotion(MyHLShoppingCart cart, string platform, string memberId)
        {
            BadgeDetails[] badgeDetails = null;
            return IsEligibleForBadgePromotion(cart, platform, memberId, out badgeDetails);
        }

        public static bool IsEligibleForBadgePromotion(MyHLShoppingCart cart, string platform, string memberId, out BadgeDetails[] badgeDetails)
        {
            badgeDetails = null;
            var cacheKey = string.Format("GetBadgePromoDetail_{0}", memberId);
            var results = HttpRuntime.Cache[cacheKey] as GetBadgePinResponse_V01;
            if (results == null)
            {
                results = GetBadgePromotion(cart, platform, memberId);
                if (results != null)
                {
                     HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);

                    badgeDetails = results.BadgeDetails;

                    if (results.BadgeDetails.Length > 0)
                        return true;
                }
                return false;
            }
            else
            {
                badgeDetails = results.BadgeDetails;
                return results.BadgeDetails.Length > 0;
            }

        }
        public static GetBadgePinResponse_V01 GetBadgePromotion(MyHLShoppingCart shoppingcart, string platform, string memberId)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {

                    var request = new GetBadgePinRequest_V01
                    {
                        DistributorID = memberId,
                        OrderMonth = shoppingcart.OrderMonth != 0 ? shoppingcart.OrderMonth.ToString() : GetOrderMonthString(shoppingcart.CountryCode),
                        Platform = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",

                    };
                    var response =
                       proxy.GetBadgePin(request) as GetBadgePinResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response;
                    }
                    HL.Common.Logging.LoggerHelper.Info(
                        string.Format("GetBadgePromotion DistributorId:{0} Locale:{1} ",
                            memberId, shoppingcart.Locale));
                }
                catch (Exception ex)
                {
                    ex =
                       new ApplicationException(
                           "Unable to get Badge Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static BrochurePromotionResponse_V01 GetBrochurePromotion(MyHLShoppingCart shoppingcart, string platform, string memberId)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {

                    var request = new BrochurePromotionRequest_V01
                    {
                        CustomerId = memberId,
                        OrderMonth = shoppingcart.OrderMonth != 0 ? shoppingcart.OrderMonth.ToString() : GetOrderMonthString(shoppingcart.CountryCode),
                        PlatForm = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",

                    };
                    var response =
                       proxy.GetBrochurePromotion(request) as BrochurePromotionResponse_V01;
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response;
                    }
                    HL.Common.Logging.LoggerHelper.Info(
                        string.Format("GetBrochurePromotion DistributorId:{0} Locale:{1} ",
                            memberId, shoppingcart.Locale));
                }
                catch (Exception ex)
                {
                    ex =
                       new ApplicationException(
                           "Unable to get Brochure Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static bool LockBrochurePromotion(MyHLShoppingCart shoppingcart, string ordernumber = null)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    string orderid = string.Empty;
                    var currentsession = SessionInfo.GetSessionInfo(shoppingcart.DistributorID, shoppingcart.Locale);
                    var memberId = currentsession.IsReplacedPcOrder ?
                        shoppingcart.SrPlacingForPcOriginalMemberId :
                        shoppingcart.DistributorID;

                    if (currentsession != null)
                    {
                        orderid = currentsession.OrderNumber;
                        if (!string.IsNullOrWhiteSpace(ordernumber))
                        {
                            orderid = ordernumber;
                        }
                    }
                
                    var request = new BrochurePromotionRequest_V02
                    {
                        CustomerId = memberId,
                        OrderMonth = shoppingcart.OrderMonth.ToString(),
                        OrderNo = "888-" + orderid,
                        PlatForm = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                    };
                    var response = proxy.LockBrochurePromotion(request) as BrochurePromotionResponse_V01;
                    HL.Common.Logging.LoggerHelper.Info(
                       string.Format("LockBrochurePromotion DistributorId:{0} Locale:{1} ",
                           memberId, shoppingcart.Locale));
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        HttpRuntime.Cache.Remove(string.Format("GetBrochurePromoDetail_{0}", memberId));
                        return true;
                    }else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                       new ApplicationException(
                           "Unable to Lock Brochure Promotion China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return false;
        }
        public static bool LockBadgePromotion(MyHLShoppingCart shoppingcart, string ordernumber = null)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    string orderid = string.Empty;
                    var currentsession = SessionInfo.GetSessionInfo(shoppingcart.DistributorID, shoppingcart.Locale);
                    var memberId = currentsession.IsReplacedPcOrder ?
                        shoppingcart.SrPlacingForPcOriginalMemberId :
                        shoppingcart.DistributorID;

                    if (currentsession != null)
                    {
                        orderid = currentsession.OrderNumber;
                        if (!string.IsNullOrWhiteSpace(ordernumber))
                        {
                            orderid = ordernumber;
                        }
                    }
                    var badgePromoSKu = Settings.GetRequiredAppSetting("ChinaBadgePromo", string.Empty).Split(',');
                    var itemsInBoth = shoppingcart.CartItems
                                             .Select(c => c.SKU)
                                             .Intersect(badgePromoSKu);

                    var skus = String.Join(",", itemsInBoth);

                    var request = new GetBadgePinRequest_V01
                    {
                        DistributorID = memberId,
                        OrderMonth = shoppingcart.OrderMonth.ToString(),
                        OrderNumber = "888-" + orderid,
                        BadgeCodes = skus,
                        Platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                    };

                    var response = proxy.LockBadgePin(request) as GetBadgePinResponse_V01;

                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        HttpRuntime.Cache.Remove(string.Format("GetBadgePromoDetail_{0}", memberId));
                        return response.IsLocked;
                    }

                    HL.Common.Logging.LoggerHelper.Info(
                        string.Format("LockBadgePromotion DistributorId:{0} Locale:{1} ",
                            memberId, shoppingcart.Locale));
                }
                catch (Exception ex)
                {
                    ex =
                       new ApplicationException(
                           "Unable to get Badge Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return false;
        }


        public static bool RollBackBrochurePromotion(string memberId, string ordernumber)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile";
                    var request = new BrochurePromotionRequest_V02
                    {
                        CustomerId = memberId,
                        OrderNo = "888-" + ordernumber,
                        PlatForm = platform,
                    };
                    var response = proxy.RollbackBrochurePromotion(request) as BrochurePromotionResponse_V01;
                    HL.Common.Logging.LoggerHelper.Info(
                       string.Format("RollbackBrochurePromotion DistributorId:{0} Locale:{1} ",
                           memberId, platform));
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        HttpRuntime.Cache.Remove(string.Format("GetBrochurePromoDetail_{0}", memberId));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                       new ApplicationException(
                           "Unable to Rollback Brochure Promotion China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return false;
        }
        //SR Promo for Phase 2
        public static bool IsEligibleForSRQGrowingPromotion(MyHLShoppingCart cart, string platform)
        {
            var cacheKey = string.Format("GetSRPromoQGrowingDetail_{0}", cart.DistributorID);
            var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
            if (results == null)
            {
                results = GetSRQGrowingPromoFromService(cart, platform);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                    if (!string.IsNullOrWhiteSpace(results.Skus))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(results.Skus))
            {
                return true;
            }
            else
            {
                return false;
            }



        }
        public static GetSRPromotionResponse_V01 GetSRQGrowingPromoFromService(MyHLShoppingCart shoppingcart, string platform)
        {


            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                var response = new GetSRPromotionResponse_V01();

                try
                {
                    var cacheKey = string.Format("GetSRPromoQGrowingDetail_{0}", shoppingcart.DistributorID);
                    var Result = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
                    response = Result;
                    if (response == null)
                    {
                        var request = new GetSRPromotionRequest_V01
                        {
                            CustomerID = shoppingcart.DistributorID,
                            OrderAmount = shoppingcart.Totals != null ? Convert.ToDouble((shoppingcart.Totals as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01).AmountDue) : 0,
                            OrderMonth = shoppingcart.OrderMonth.ToString(),
                            Platform = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                            VolumePoint = Convert.ToDouble(shoppingcart.VolumeInCart)
                        };
                        response =
                           proxy.GetSRPromotionDetailPhase2(new GetSRPromotionDetailPhase2Request { request = request }).GetSRPromotionDetailPhase2Result as GetSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {
                            HttpRuntime.Cache.Insert(cacheKey,
                           response,
                           null,
                           DateTime.Now.AddMinutes(20),
                           Cache.NoSlidingExpiration,
                           CacheItemPriority.Low,
                           null);
                            return response;
                        }
                        HL.Common.Logging.LoggerHelper.Info(
                            string.Format("GetSRPromoDetail DistributorId:{0} Locale:{1} ",
                                shoppingcart.DistributorID, shoppingcart.Locale));
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "Unable to get SR Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;

        }
        public static bool LockSRQGrowingPromotion(MyHLShoppingCart shoppingcart, string ordernumber = null)
        {
            if (shoppingcart != null)
            {
                using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    var cacheKey = string.Format("GetSRPromoQGrowingDetail_{0}", shoppingcart.DistributorID);
                    var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
                    try
                    {
                        string orderid = string.Empty;
                        var currentsession = SessionInfo.GetSessionInfo(shoppingcart.DistributorID, shoppingcart.Locale);
                        if (currentsession != null)
                        {
                            orderid = currentsession.OrderNumber;
                            if (!string.IsNullOrWhiteSpace(ordernumber))
                            {
                                orderid = ordernumber;
                            }
                        }
                        var request = new GetSRPromotionRequest_V01
                        {
                            CustomerID = shoppingcart.DistributorID,
                            OrderMonth = shoppingcart.OrderMonth.ToString(),
                            OrderNumber = "080-" + orderid,
                            Platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                        };
                        var response =
                            proxy.LockSRPromotionDetailPhase2(new LockSRPromotionDetailPhase2Request { request = request }).LockSRPromotionDetailPhase2Result as GetSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {
                            if (response.IsLocked)
                            {
                                HttpRuntime.Cache.Remove(string.Format("GetSRPromoQGrowingDetail_{0}", shoppingcart.DistributorID));
                            }
                            return response.IsLocked;
                        }
                        HL.Common.Logging.LoggerHelper.Info(
                      string.Format("GetSRPromoQGrowingDetail DistributorId:{0} Locale:{1} ",
                          shoppingcart.DistributorID, shoppingcart.Locale));
                    }
                    catch (Exception ex)
                    {
                        ex =
                            new ApplicationException(
                                "Unable to get SRPromoQGrowingDetail: China Order Provier", ex);
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        return false;
                    }

                }
            }
            return false;
        }

        public static bool IsEligibleForSRQExcellentPromotion(MyHLShoppingCart cart, string platform)
        {
            var cacheKey = string.Format("GetSRPromoQExcellentDetail_{0}", cart.DistributorID);
            var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
            if (results == null)
            {
                results = GetSRQExcellentPromoFromService(cart, platform);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        results,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                    if (!string.IsNullOrWhiteSpace(results.Skus))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(results.Skus))
            {
                return true;
            }
            else
            {
                return false;
            }



        }
        public static GetSRPromotionResponse_V01 GetSRQExcellentPromoFromService(MyHLShoppingCart shoppingcart, string platform)
        {

            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {

                try
                {

                    var cacheKey = string.Format("GetSRPromoQExcellentDetail_{0}", shoppingcart.DistributorID);
                    var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;

                    var response = results;
                    if (response == null)
                    {
                        var request = new GetSRPromotionRequest_V01
                        {
                            CustomerID = shoppingcart.DistributorID,
                            OrderAmount = shoppingcart.Totals != null ? Convert.ToDouble((shoppingcart.Totals as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01).AmountDue) : 0,
                            OrderMonth = shoppingcart.OrderMonth.ToString(),
                            Platform = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                            VolumePoint = Convert.ToDouble(shoppingcart.VolumeInCart)
                        };
                        response =
                           proxy.GetSRPromotionDetailPhase2_V01(new GetSRPromotionDetailPhase2_V01Request { request = request }).GetSRPromotionDetailPhase2_V01Result as GetSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {

                            HttpRuntime.Cache.Insert(cacheKey,
                                response,
                                null,
                                DateTime.Now.AddMinutes(20),
                                Cache.NoSlidingExpiration,
                                CacheItemPriority.Low,
                                null);
                            return response;
                        }
                        HL.Common.Logging.LoggerHelper.Info(
                            string.Format("GetSRPromoQExcellentDetail DistributorId:{0} Locale:{1} ",
                                shoppingcart.DistributorID, shoppingcart.Locale));
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "Unable to get SRPromoQExcellentDetail: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;

        }
        public static bool LockSRQExcellentPromotion(MyHLShoppingCart shoppingcart, string ordernumber = null)
        {
            if (shoppingcart != null)
            {
                using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    try
                    {
                        string orderid = string.Empty;
                        var currentsession = SessionInfo.GetSessionInfo(shoppingcart.DistributorID, shoppingcart.Locale);
                        if (currentsession != null)
                        {
                            orderid = currentsession.OrderNumber;
                            if (!string.IsNullOrWhiteSpace(ordernumber))
                            {
                                orderid = ordernumber;
                            }
                        }
                        var request = new GetSRPromotionRequest_V01
                        {
                            CustomerID = shoppingcart.DistributorID,
                            OrderMonth = shoppingcart.OrderMonth.ToString(),
                            OrderNumber = "080-" + orderid,
                            Platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile",
                        };
                        var response =
                            proxy.LockSRPromotionDetailPhase2_V01(new LockSRPromotionDetailPhase2_V01Request { request = request }).LockSRPromotionDetailPhase2_V01Result as GetSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {
                            if (response.IsLocked)
                            {
                                HttpRuntime.Cache.Remove(string.Format("GetSRPromoQExcellentDetail_{0}", shoppingcart.DistributorID));
                            }
                            return response.IsLocked;
                        }
                        HL.Common.Logging.LoggerHelper.Info(
                      string.Format("GetSRPromoQExcellentDetail DistributorId:{0} Locale:{1} ",
                          shoppingcart.DistributorID, shoppingcart.Locale));
                    }
                    catch (Exception ex)
                    {
                        ex =
                            new ApplicationException(
                                "Unable to get SRPromoQExcellentDetail: China Order Provier", ex);
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        return false;
                    }

                }
            }
            return false;
        }
        private static string GetOrderMonthString(string countrycode)
        {
            DateTime dtOrderMonth = new OrderMonth(countrycode).CurrentOrderMonth;
            return dtOrderMonth.ToString("yyyyMM");
        }

        #region new sr promotion

        private const string NEWSRPROMOTION_CACHEKEY_FORMAT = "GetNewSRPromotionDetail_{0}";
        public static bool IsEligibleForNewSRPromotion(MyHLShoppingCart cart, string platform)
        {
            var cacheKey = String.Format(NEWSRPROMOTION_CACHEKEY_FORMAT, cart.DistributorID);
            var response = HttpRuntime.Cache[cacheKey] as GetNewSRPromotionResponse_V01;
            if(response == null)
            {
                response = GetNewSRPromotion(cart, platform);
                if(response != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                        response,
                        null,
                        DateTime.Now.AddMinutes(20),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Low,
                        null);
                }
                else
                {
                    return false;
                }
            }

            return !String.IsNullOrWhiteSpace(response.Skus) && response.Quantity > 0;
        }

        private static GetNewSRPromotionResponse_V01 GetNewSRPromotion(MyHLShoppingCart cart, string platform)
        {
            using(var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new GetNewSRPromotionRequest_V01
                    {
                        CustomerID = cart.DistributorID,
                        OrderMonth = cart.OrderMonth != 0 ? cart.OrderMonth.ToString() : GetOrderMonthString(cart.CountryCode),
                        Platform = platform.ToLower().Equals("myhl") ? "GDO" : "Mobile"
                    };

                    var response = proxy.GetNewSrPromotion(request) as GetNewSRPromotionResponse_V01;
                    if (response != null && 
                        response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response;
                    }

                    HL.Common.Logging.LoggerHelper.Info(
                       string.Format("GetNewSRPromotion DistributorId:{0} Locale:{1} ", 
                       cart.DistributorID, 
                       cart.Locale));
                }
                catch(Exception ex)
                {
                    ex = new ApplicationException("Unable to get New SR Promotion: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return null;
        }

        public static bool LockNewSRPromotion(MyHLShoppingCart cart, string ordernumber)
        {
            if (IsEligibleForNewSRPromotion(cart, HLConfigManager.Platform))
            {
                using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    try
                    {
                        if (String.IsNullOrWhiteSpace(ordernumber))
                        {
                            var currentsession = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
                            if (currentsession != null)
                                ordernumber = currentsession.OrderNumber;
                        }

                        if (String.IsNullOrWhiteSpace(ordernumber))
                            throw new ArgumentNullException("ordernumber can't be empty");

                        var request = new GetNewSRPromotionRequest_V01
                        {
                            CustomerID = cart.DistributorID,
                            OrderMonth = cart.OrderMonth != 0 ? cart.OrderMonth.ToString() : GetOrderMonthString(cart.CountryCode),
                            OrderNumber = ordernumber,
                            Platform = HLConfigManager.Platform.ToLower().Equals("myhl") ? "GDO" : "Mobile"
                        };

                        var response = proxy.LockNewSrPromotion(request) as GetNewSRPromotionResponse_V01;
                        if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        {
                            HttpRuntime.Cache.Remove(String.Format(NEWSRPROMOTION_CACHEKEY_FORMAT, cart.DistributorID));
                            return response.IsLocked;
                        }

                        HL.Common.Logging.LoggerHelper.Info(
                          string.Format("LockNewSRPromotion DistributorId:{0} Locale:{1}",
                          cart.DistributorID,
                          cart.Locale));
                    }
                    catch (Exception ex)
                    {
                        ex = new ApplicationException("Unable to Lock New SR Promotion: China Order Provier", ex);
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                    }
                    
                }
            }

            return false;
        }

        #endregion
    }
}
