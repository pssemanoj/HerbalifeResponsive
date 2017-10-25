using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IDiscountRule
    {
        void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason);

        string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount);
    }
}

