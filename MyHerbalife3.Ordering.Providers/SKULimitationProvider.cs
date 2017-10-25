using HL.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public static partial class SKULimitationProvider
    {
        /// <summary>
        /// GetSKULimitationInfo
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<SKULimitationInfo>> GetSKULimitationInfo()
        {
            string cacheKey = "SKULIMITATION";
            var limits = HttpRuntime.Cache[cacheKey] as Dictionary<string, List<SKULimitationInfo>>;
            if (limits == null)
            {

                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    try
                    {
                        var rspSKU = proxy.GetSKULimitation(new GetSKULimitationRequest1(new GetSKULimitationRequest())).GetSKULimitationResult as GetSKULimitationResponse;
                        if (rspSKU != null && rspSKU.SKULimitationInfoDict != null)
                        {
                            limits = rspSKU.SKULimitationInfoDict;
                            HttpRuntime.Cache.Insert(cacheKey, rspSKU.SKULimitationInfoDict, null, DateTime.Now.AddMinutes(60 * 24), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        HL.Common.Utilities.WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        LoggerHelper.Error(string.Format("GetSKULimitationInfo error, {0}", ex.ToString()));
                    }
                }
            }
            return limits;

        }

        /// <summary>
        /// called after order placed
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="orderMonth"></param>
        /// <param name="limits"></param>
        public static void SetSKUPurchaseRestrictionInfo(string distributorID, string orderMonth, List<PurchaseRestrictionInfo> limits)
        {
            string cacheKey = string.Format("JPLIMITS_{0}_{1}", distributorID, orderMonth);
            HttpContext.Current.Session[cacheKey] = limits;
        }


        /// <summary>
        /// JP only : SKU limits
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="orderMonth"></param>
        /// <returns></returns>
        public static List<PurchaseRestrictionInfo> SKUPurchaseRestrictionInfo(string distributorID, string orderMonth)
        {
            string cacheKey = string.Format("JPLIMITS_{0}_{1}", distributorID, orderMonth);
            var limits = HttpContext.Current.Session[cacheKey] as List<PurchaseRestrictionInfo>;
            if (limits != null)
                return limits;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var rspSKU = proxy.SKUPurchaseRestriction(new SKUPurchaseRestrictionRequest1(new SKUPurchaseRestrictionRequest_V01 { DistributorID = distributorID, OrderMonth = orderMonth })).SKUPurchaseRestrictionResult as SKUPurchaseRestrictionResponse_V01;
                    if (rspSKU != null && rspSKU.PurchaseRestriction != null)
                    {
                        limits = rspSKU.PurchaseRestriction;
                        HttpContext.Current.Session[cacheKey] = limits;
                    }
                }
                catch (Exception ex)
                {
                    HL.Common.Utilities.WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                    LoggerHelper.Error(string.Format("SKUPurchaseRestrictionInfo error, {0}", ex.ToString()));
                }
            }
            return limits;
        }

        public static bool CheckSKULimitation(string country, MyHLShoppingCart cart, out List<string> errors)
        {
            string ruleName = "SkuLimitation Rules";
            errors = new List<string>();

            var SKULimitationInfo = GetSKULimitationInfo();
            if (SKULimitationInfo != null && cart.CartItems != null)
            {
                if (SKULimitationInfo.ContainsKey(country))
                {
                    List<ShoppingCartItem_V01> intersect = (from c in cart.CartItems
                                                            from s in SKULimitationInfo[country].Select(l => l.SKU.Trim())
                                                            where c.SKU == s
                                                            select c).ToList<ShoppingCartItem_V01>();
                    if (intersect.Count() > 0)
                    {
                        MyHLShoppingCart myCart = new MyHLShoppingCart { DistributorID = cart.DistributorID, Locale = cart.Locale, CartItems = new ShoppingCartItemList(), CountryCode = cart.CountryCode };
                        myCart.RuleResults = new List<ShoppingCartRuleResult>();
                        ServerRulesManager.Instance.ValidateSKULimitation(myCart, cart.Locale, intersect);
                        if (myCart.RuleResults.Any(
                                       rs =>
                                       rs.RuleName == ruleName && rs.Result == RulesResult.Failure))
                        {
                            var ruleResultMsgs =
                                    myCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == ruleName)
                                                .Select(r => r.Messages);
                            errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                            myCart.RuleResults.RemoveAll(x => x.RuleName == ruleName);
                            return false;
                        }
                    }

                }
            }
            if (country == "JP" && Settings.GetRequiredAppSetting<bool>("EnableJPSKURestriction", false))
            {
                ruleName = "PurchaseRestriction Rules";
                var allLimits = SKULimitationProvider.SKUPurchaseRestrictionInfo(cart.DistributorID, SKULimitationProvider.GetOrderMonthString());
                if(allLimits!=null && allLimits.Any())
                {
                    ShoppingCartProvider.processCart(cart, new List<ShoppingCartItem_V01>(), ShoppingCartRuleReason.CartBeingPaid);
                    if (cart.RuleResults.Any(
                                       rs =>
                                       rs.RuleName == ruleName && rs.Result == RulesResult.Failure))
                    {
                        var ruleResultMsgs =
                                cart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == ruleName)
                                            .Select(r => r.Messages);
                        errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                        cart.RuleResults.RemoveAll(x => x.RuleName == ruleName);
                        return false;
                    }
                }
            }
            return true;
        }

        public static string GetOrderMonthString()
        {
            DateTime dtOrderMonth = new OrderMonth("JP").CurrentOrderMonth;
            return dtOrderMonth.ToString("yyyyMM");
        }
    }
}
