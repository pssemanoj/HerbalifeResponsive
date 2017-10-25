using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.hu_HU
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Must be VAT register to dont have volume limited
            bool isExempt = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            var firstCombination = tins.Find(p => p.IDType.Key == "HUVT");
            if (firstCombination != null)
            {
                var secondCombination = tins.Find(p => p.IDType.Key == "HUBL" || p.IDType.Key == "HUPT");
                if (secondCombination != null)
                {
                    isExempt = true;
                }
            }


            return isExempt;
        }
    }
}