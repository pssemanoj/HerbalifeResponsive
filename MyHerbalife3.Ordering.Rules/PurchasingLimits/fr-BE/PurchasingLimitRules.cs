using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.fr_BE
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            isExempt = tins.Any(t => t.IDType.Key == "BENO" || t.IDType.Key == "BEVT");
            return isExempt;
        }
    }
}
