using System.Collections.Generic;

using HL.Catalog.ValueObjects;

namespace MyHerbalife3.Ordering.Rules.Promotional.zh_HK
{
    using MyHerbalife3.Ordering.Providers.Shipping;
    using MyHerbalife3.Shared.Providers.RulesManagement;
    using MyHerbalife3.Shared.Providers.RulesManagement.Interfaces;

    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            if (cart != null && (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartItemsRemoved))
            {
                var provider = ShippingProvider.GetShippingProvider(this.Country);
                if (provider != null)
                {
                    provider.SetShippingInfo(cart);
                    result.Result = RulesResult.Success;
                    cart.RuleResults.Add(result);
                }
            }
            return result;
        }
    }
}
