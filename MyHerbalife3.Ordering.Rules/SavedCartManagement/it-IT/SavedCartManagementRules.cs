using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Rules.SavedCartManagement.it_IT
{
    using MyHerbalife3.Ordering.Providers;
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    
    public class SavedCartManagementRules : MyHerbalifeRule, ISavedCartManagementRule
    {
        public List<ShoppingCartRuleResult> ProcessSavedCartManagementRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
                {
                    RuleName = "Saved Cart Management Rules",
                    Result = RulesResult.Unknown
                };

            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                // Each time a saved cart or a copy from order is retrieved, the order subtype should be empty
                if (cart != null)
                {
                    var shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart != null && (shoppingCart.IsSavedCart || shoppingCart.IsFromCopy ))
                    {
                        shoppingCart.OrderSubType = string.Empty;
                        defaultResult.Result = RulesResult.Success;
                    }
                }
            }
            return result;
        }
    }
}
