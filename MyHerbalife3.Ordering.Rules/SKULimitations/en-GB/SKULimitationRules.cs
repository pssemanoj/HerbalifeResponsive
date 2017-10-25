using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.en_GB
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var SKUIds = new List<string[]>();
                SKUIds.Add(new[] {HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku, "3"});
                SKUIds.Add(new[] {"4150", "10"});

                if (cart.CurrentItems[0].SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku)
                {
                    var catItems = CatalogProvider.GetCatalogItems((from c in cart.CartItems
                                                                    select c.SKU.Trim()).ToList<string>(),
                                                                   Country);
                    if (catItems != null)
                    {
                        if (!catItems.Any(c => c.Value.ProductType == ProductType.Product))
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "AddProductFirst")
                                               .ToString(), cart.CurrentItems[0].SKU));
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                }

                foreach (string[] sku in SKUIds)
                {
                    int quantity = 0;
                    if (cart.CurrentItems[0].SKU == sku[0])
                    {
                        quantity += cart.CurrentItems[0].Quantity;
                        if (cart.CartItems.Exists(item => item.SKU == sku[0]))
                            quantity += cart.CartItems.Where(item => item.SKU == sku[0]).First().Quantity;
                        if (quantity > Convert.ToInt32(sku[1]))
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "SKUQuantityExceeds")
                                               .ToString(), sku[1], sku[0]));
                            cart.RuleResults.Add(Result);
                        }
                    }
                }
            }
            return Result;
        }
    }
}