using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using HL.Common.Utilities;
using System;
using HL.Blocks.Caching.SimpleCache;
using System.Web;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.Global
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly ISimpleCache _Cache;
        
        private int _skuQuantityLimitPeriodByDay { get; set; }
        private int _startDate { get; set; }
        public SKULimitationsRules()
        {
            _Cache = CacheFactory.Create();
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        protected virtual bool shouldApplyRules(DateTime startDate, DateTime endDate, int limtDays)
        {
            int currentLocalDate = RecentOrdersHelper.ConvertDate(DateUtils.GetCurrentLocalTime(this.Country));
            int intStartDate = RecentOrdersHelper.ConvertDate(startDate);
            int intEndDate = limtDays <= 0 ? RecentOrdersHelper.ConvertDate(endDate) : RecentOrdersHelper.ConvertDate(startDate.AddDays(limtDays));
            if (currentLocalDate >= intStartDate && currentLocalDate <= intEndDate)
                return true;
            return false;
        }
        private string getCacheKey(string distributor, string locale)
        {
            return string.Format("ORDERS_{0}_{1}", distributor, locale);
        }

        private RecentOrdersHelper getInternetOrders(string distributor, string locale, DateTime currentLocalTime)
        {
            DateTime timeMax = currentLocalTime.Date.AddDays(_skuQuantityLimitPeriodByDay).AddTicks(-1);
           
            // cache till end of day + limit period
            return _Cache.Retrieve(delegate
            {
                return new RecentOrdersHelper(distributor, Locale, currentLocalTime, _skuQuantityLimitPeriodByDay, _startDate);
            }, getCacheKey(distributor, locale), TimeSpan.FromMinutes(timeMax.Minute - currentLocalTime.Minute));
        }
        private ShoppingCartRuleResult checkSKULimitForCountry(MyHLShoppingCart myCart, ShoppingCartRuleResult Result)
        {
            Dictionary<string,List<SKULimitationInfo>> limits = SKULimitationProvider.GetSKULimitationInfo();
            if ( limits != null)
            {
                if (limits.ContainsKey(Country))
                {
                    List<SKULimitationInfo> limitInfo = limits[Country];
                    foreach (var l in limitInfo)
                    {
                        if (limitInfo != null && limitInfo.Count > 0 && shouldApplyRules(l.StartDate, l.EndDate,l.LimitPeriodByDay))
                        {

                            if (myCart.CurrentItems == null)
                                return Result;

                            string sku = myCart.CurrentItems[0].SKU.Trim();
                            if (l.SKU.Trim().Equals(sku))
                            {
                                _skuQuantityLimitPeriodByDay = l.LimitPeriodByDay;
                                _startDate = RecentOrdersHelper.ConvertDate(l.StartDate);
                                bool bSKUQtyOverLimit = false;

                                int SKUQtyBeingAdded = myCart.CurrentItems[0].Quantity;
                                if (SKUQtyBeingAdded > l.MaxQuantity)
                                    bSKUQtyOverLimit = true;

                                int qtyInCart = 0;
                                if (bSKUQtyOverLimit == false)
                                {
                                    if (myCart.CartItems != null && myCart.CartItems.Where(x => x.SKU == sku).Any()) // already in cart
                                    {
                                        qtyInCart = myCart.CartItems.Where(x => x.SKU == sku).Sum(x => x.Quantity);
                                        if ((qtyInCart + SKUQtyBeingAdded) > l.MaxQuantity)
                                        {
                                            bSKUQtyOverLimit = true;
                                        }
                                    }
                                }
                                if (bSKUQtyOverLimit == false)
                                {
                                    int numSkusOrders = qtyInCart + SKUQtyBeingAdded; // what's in cart plus what's being added
                                    RecentOrdersHelper orders = getInternetOrders(myCart.DistributorID, this.Locale, DateUtils.GetCurrentLocalTime(this.Country));
                                    if (orders != null)
                                    {
                                        foreach (var o in orders.Orders)
                                        {
                                            foreach (var i in o.CartItems)
                                            {
                                                if (i.SKU == sku)
                                                {
                                                    numSkusOrders += i.Quantity;
                                                    //break;
                                                }
                                            }
                                        }
                                    }

                                    if (numSkusOrders > l.MaxQuantity)
                                    {
                                        bSKUQtyOverLimit = true;
                                    }
                                    //else
                                    //{
                                    //    if (myCart.CurrentItems[0].Quantity + numSkusOrders > l.MaxQuantity)
                                    //    {
                                    //        bSKUQtyOverLimit = true;
                                    //    }
                                    //}
                                }

                                if (bSKUQtyOverLimit == true)
                                {
                                    /// Your order of {0} exceeds the maximum quantity of {1} per member every {2} calendar days.
                                    Result.AddMessage(
                                   string.Format(
                                       HttpContext.GetGlobalResourceObject(
                                           string.Format("{0}_Rules", HLConfigManager.Platform),
                                           "SKULimitExceedByDay").ToString(),
                                       myCart.CurrentItems[0].SKU, l.MaxQuantity, l.LimitPeriodByDay));

                                    myCart.RuleResults.Add(Result);
                                    Result.Result = RulesResult.Failure;
                                }

                            }
                        }
                    }
                }
            }
            return Result;
        }

        private ShoppingCartRuleResult checkSKULineLimitPerOrder(MyHLShoppingCart myCart, ShoppingCartRuleResult Result)
        {
            int maxLineItems = HLConfigManager.Configurations.ShoppingCartConfiguration.MaxLineItemsInCart;
                        
            if (!Country.Equals(DistributorProfileModel.ProcessingCountryCode))
            {
                maxLineItems = HLConfigManager.Configurations.ShoppingCartConfiguration.MaxLineItemsInCartForeignMember;
            }            
            
            if (myCart != null && myCart.CartItems != null )
            {
                var newLineNum = myCart.CartItems.Count + (myCart.ItemsBeingAdded != null ? myCart.ItemsBeingAdded.Count : 1);
                if (newLineNum > maxLineItems)
                {
                    if (myCart.CartItems.Find(x => x.SKU.Trim() == myCart.CurrentItems[0].SKU.Trim()) == null)
                    {
                        var errorMessage =
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "AnySKUQuantityExceeds") ??
                            "Quantity of SKUs should not exceed above {0}";
                        Result.AddMessage(string.Format(errorMessage.ToString(), maxLineItems));

                        Result.Result = RulesResult.Failure;
                        myCart.RuleResults.Add(Result);                        
                    }
                }
            }
            
            return Result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                var shoppingCart = cart as MyHLShoppingCart;
                if (null != cart && shoppingCart != null)
                {
                    var sessionInfo = SessionInfo.GetSessionInfo(cart.DistributorID, Locale);
                    var primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
                    var secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
                    var distributorShoppingCartItems =
                        shoppingCart.ShoppingCartItems.Where(
                            c => c.SKU.Trim() == primarySku || c.SKU.Trim() == secondarySku);

                    if (null != distributorShoppingCartItems && distributorShoppingCartItems.Count() > 0)
                    {
                        if (null != sessionInfo)
                        {
                            if (!sessionInfo.IsTodaysMagazineInCart)
                            {
                                if (shoppingCart.ShoppingCartItems.Any(c => c.SKU.Trim() == primarySku))
                                    shoppingCart.DeleteTodayMagazine(primarySku);
                                else
                                    shoppingCart.DeleteTodayMagazine(secondarySku);
                            }
                        }
                    }

                    //HFF can now be standalone
                    var hffItem =
                        shoppingCart.ShoppingCartItems.Where(
                            c => (c.SKU.Trim() == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku) || 
                                HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(c.SKU.Trim())));
                    if (null != hffItem && hffItem.Count() > 0)
                    {
                        if (shoppingCart.DeliveryInfo != null)
                        {
                            if (shoppingCart.ShoppingCartItems.Count == 1)
                            {
                                shoppingCart.DeliveryInfo.FreightCode = "NOF";
                            }
                            else if (shoppingCart.DeliveryInfo.FreightCode == "NOF")
                            {
                                var provider = ShippingProvider.GetShippingProvider(null);
                                if (provider != null)
                                {
                                    var shippingInfo =
                                        provider.GetShippingInfoFromID(shoppingCart.DistributorID, Locale,
                                                                       shoppingCart.DeliveryInfo.Option,
                                                                       shoppingCart.DeliveryOptionID,
                                                                       shoppingCart.DeliveryInfo.Address.ID);
                                    shoppingCart.DeliveryInfo = shippingInfo;
                                }
                            }
                        }
                    }

                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                Result = checkSKULimitForCountry(cart as MyHLShoppingCart, Result);
                Result = checkSKULineLimitPerOrder(cart as MyHLShoppingCart, Result);
            }
            else if (reason == ShoppingCartRuleReason.CartClosed) // order placed
            {
                _Cache.Expire(typeof(RecentOrdersHelper), getCacheKey(cart.DistributorID, Locale));
            }
            return Result;
        }
    }
}