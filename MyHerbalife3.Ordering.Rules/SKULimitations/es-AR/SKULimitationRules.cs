using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.es_AR
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const int NTSLines = 15;

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
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded && cart != null)
            {
                var item = from c in cart.CartItems
                           from n in cart.CurrentItems
                           where c.SKU == n.SKU
                           select c;

                if (item.ToList().Count == 0)
                {
                    if (cart.CartItems.Count >= NTSLines)
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_Rules", HLConfigManager.Platform), "AnySKUQuantityExceeds")
                                           .ToString(), NTSLines.ToString()));
                        cart.RuleResults.Add(Result);
                    }
                }
            }
            return Result;
        }
    }
}