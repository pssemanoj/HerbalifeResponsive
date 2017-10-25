using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.uk_UA
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Must be VAT register to dont have volume limited
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            var uabl = tins.Find(p => p.IDType.Key == "UABL");
            isExempt = (uabl != null);
            return isExempt;
        }
    }
}