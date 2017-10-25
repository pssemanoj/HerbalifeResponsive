using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.HFF.SamCam
{
    public class HFFRules : MyHerbalifeRule, IHFFRule
    {
        bool IHFFRule.CanDonate(ShoppingCart_V02 cart)
        {
            // Do not display the HFF control in COP
            return false;
        }
    }
}
