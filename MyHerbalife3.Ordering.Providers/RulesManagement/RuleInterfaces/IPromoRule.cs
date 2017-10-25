using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IPromoRule
    {
        List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason);
    }
}
