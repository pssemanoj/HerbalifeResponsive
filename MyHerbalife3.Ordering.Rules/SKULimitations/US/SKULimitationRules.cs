using HL.Blocks.Caching.SimpleCache;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.US
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly string _limitQtySku = "017A";
        private readonly ISimpleCache _Cache;
        private int _skuQuantityLimitPeriodByDay { get; set; }

        public SKULimitationsRules()
        {
            _Cache = CacheFactory.Create();
            _skuQuantityLimitPeriodByDay = HLConfigManager.Configurations.ShoppingCartConfiguration.SKUQuantityLimitPeriodByDay - 1;
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
                return new RecentOrdersHelper(distributor, Locale, currentLocalTime, _skuQuantityLimitPeriodByDay, _skuQuantityLimitPeriodByDay);
            }, getCacheKey(distributor, locale), TimeSpan.FromMinutes(timeMax.Minute - currentLocalTime.Minute));
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                   ShoppingCartRuleReason reason,
                                                   ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                int skuLimit = 1;
                try
                {
                    skuLimit = int.Parse(Settings.GetRequiredAppSetting("USNumSkuLimitSku"));
                }
                catch 
                {
                                        
                }
                if (cart.CurrentItems == null)
                    return Result;
                string sku = cart.CurrentItems[0].SKU.Trim();
                if (_limitQtySku.Equals(sku))
                {
                    bool bSKUQtyOverLimit = false;

                    if (cart.CurrentItems[0].Quantity > skuLimit)
                        bSKUQtyOverLimit = true;

                    MyHLShoppingCart myCart = cart as MyHLShoppingCart;

                    if (bSKUQtyOverLimit == false)
                    {
                        if (myCart.CartItems !=null && myCart.CartItems.Where(x => x.SKU == sku).Any()) // already in cart
                        {
                            bSKUQtyOverLimit = true;
                        }
                    }
                    if (bSKUQtyOverLimit == false)
                    {
                        int numSkusOrders = 0;
                        RecentOrdersHelper orders = getInternetOrders(cart.DistributorID, this.Locale, DateUtils.GetCurrentLocalTime(this.Country));
                        if (orders != null)
                        {
                            foreach (var o in orders.Orders)
                            {
                                foreach (var i in o.CartItems)
                                {
                                    if (i.SKU == sku)
                                    {
                                        numSkusOrders = numSkusOrders + i.Quantity;
                                        break;
                                    }
                                }
                            }
                        }

                        if (numSkusOrders >= skuLimit)
                        {
                            bSKUQtyOverLimit = true;
                        }
                        else
                        {
                            if (cart.CurrentItems[0].Quantity + numSkusOrders > skuLimit)
                            {
                                bSKUQtyOverLimit = true;
                            }
                        }
                    }
                    
                    if (bSKUQtyOverLimit == true)
                    {
                        if (Locale == "es-US" || Locale == "es-PR")
                        {
                            Result.AddMessage(string.Format("Tu pedido de {0} excede la cantidad máxima de 1 por Asociado cada {1} días.", sku, HLConfigManager.Configurations.ShoppingCartConfiguration.SKUQuantityLimitPeriodByDay));
                        }
                        else
                        {
                            Result.AddMessage(string.Format("Your order of {0} exceeds the maximum quantity of 1 per member every {1} calendar days.", sku, HLConfigManager.Configurations.ShoppingCartConfiguration.SKUQuantityLimitPeriodByDay));
                        }
                        cart.RuleResults.Add(Result);
                        Result.Result = RulesResult.Failure;
                    }
                    
                }
            }
            //else if (reason == ShoppingCartRuleReason.CartCreated) only get orders when adding 017A to cart
            //{
            //    DateTime currentLocalTime = DateUtils.GetCurrentLocalTime(this.Country);
            //    RecentOrders orders = getInternetOrders(cart.DistributorID, this.Locale, currentLocalTime);
            //    if (orders.LastRetrieveDateTime.Date < currentLocalTime.Date)
            //    {
            //        _Cache.Expire(typeof(RecentOrders), getCacheKey(cart.DistributorID, Locale));
            //        getInternetOrders(cart.DistributorID, this.Locale, currentLocalTime);
            //    }
            //}
            else if (reason == ShoppingCartRuleReason.CartClosed) // order placed
            {
                _Cache.Expire(typeof(RecentOrdersHelper), getCacheKey(cart.DistributorID, Locale));
            }
            return Result;
        }
    }
}
