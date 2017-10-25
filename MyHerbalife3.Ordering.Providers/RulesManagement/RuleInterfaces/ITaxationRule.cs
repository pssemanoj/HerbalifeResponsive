using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface ITaxationRule
    {
        void PerformTaxationRules(Order_V01 order, string locale);
    }
}