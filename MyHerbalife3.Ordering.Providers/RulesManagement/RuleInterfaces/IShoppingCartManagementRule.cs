using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IShoppingCartManagementRule
    {
        List<ShoppingCartRuleResult> ProcessCartManagementRules(ShoppingCart_V02 cart);
    }
}
