using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IOrderManagementRule
    {
        void PerformOrderManagementRules(ShoppingCart_V02 cart, Order_V01 order, string locale, OrderManagementRuleReason reason);
    }
}

