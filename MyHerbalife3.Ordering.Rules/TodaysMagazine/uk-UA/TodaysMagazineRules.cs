using System;
using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.Rules.TodaysMagazine.uk_UA
{
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.Providers;
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

    public class TodaysMagazineRules : MyHerbalifeRule, IShoppingCartRule
    {        
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
                {
                    RuleName = "TodaysMagazine Rules",
                    Result = RulesResult.Unknown
                };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {            
            if (cart == null)
            {
                return Result;
            }

            var myhlCart = cart as MyHLShoppingCart;
            if (myhlCart == null)
            {
                return Result;
            }

            if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartItemsRemoved ||
                reason == ShoppingCartRuleReason.CartCalculated || reason == ShoppingCartRuleReason.CartCreated)
            {
                //check volume in cart and the quantity of todays magazine sku
                myhlCart.TodaysMagaZineQuantity = AllowedTodaysMagazineQuantity(myhlCart.VolumeInCart);

                var tmSku = (from i in myhlCart.ShoppingCartItems
                             where i.SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku
                             select i).FirstOrDefault();
                if (tmSku != null)
                {
                    if (tmSku.Quantity > myhlCart.TodaysMagaZineQuantity)
                    {
                        myhlCart.DeleteTodayMagazine(tmSku.SKU);
                        myhlCart.AddTodayMagazine(myhlCart.TodaysMagaZineQuantity, tmSku.SKU);
                    }
                }
            }

            return Result;
        }

        private int AllowedTodaysMagazineQuantity(decimal volumeInCart)
        {
            if (volumeInCart < 1000)
            {
                return HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax;
            }

            var todaysMagazineQuantity = Convert.ToInt32(Math.Truncate(volumeInCart / 500));
            return todaysMagazineQuantity;
        }
    }
}
