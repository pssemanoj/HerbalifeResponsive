using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.CartManagement.es_AR
{
    public class CartManagementRules : MyHerbalifeRule, IShoppingCartManagementRule
    {
        private const string _feeSku = "9901";

        public List<ShoppingCartRuleResult> ProcessCartManagementRules(ShoppingCart_V02 cart)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "Cart Management Rules";
            defaultResult.Result = RulesResult.Unknown;

            if (cart != null && cart.CartItems != null && cart.CartItems.Any(f => f.SKU == _feeSku))
            {
                (cart as MyHLShoppingCart).DeleteItemsFromCart(new List<string>(new string[] {_feeSku}), true);
            }

            return result;
        }
    }
}