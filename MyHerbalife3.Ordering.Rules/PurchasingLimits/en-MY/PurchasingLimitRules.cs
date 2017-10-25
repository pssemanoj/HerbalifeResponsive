using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.en_MY
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            Dictionary<int, PurchasingLimits_V01> currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            PurchasingLimits_V01 theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
            PurchaseLimitType limitsType = PurchaseLimitType.ProductCategory;

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            bool bHasMYID = tins.Find(t => t.IDType.Key == "MYID") != null;
            if (!DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                limitsType = PurchaseLimitType.Volume;
                if (!bHasMYID)
                {
                    limitsType = PurchaseLimitType.ProductCategory;
                }
            }
            else
            {
                if (bHasMYID)
                {
                    limitsType = PurchaseLimitType.None;
                }
            }
            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);

            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            return tins.Find(p => p.IDType.Key == "MYID" && p.ID == "MY00") == null;
        }
    }
}