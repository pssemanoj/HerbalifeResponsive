using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_PY
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Must be VAT register to dont have volume limited
            if (!base.DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                return false;
            }

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);

            return tins.Any(p => p.IDType.Key == "PYTX");
        }
    }
}