using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.el_GR
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private List<string> ResaleSubTypes = new List<string>(new string[] {"RE"});
        private List<string> RetailSubTypes = new List<string>(new string[] {"PC"});

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            Dictionary<int, PurchasingLimits_V01> currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            PurchasingLimits_V01 theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];

            theLimits.PurchaseSubType = TIN;

            if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId))
            {
                theLimits.PurchaseLimitType = PurchaseLimitType.Volume;
                return currentLimits;
            }
            if (DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                theLimits.PurchaseLimitType = PurchaseLimitType.None;
            }
            else
            {
                theLimits.PurchaseType = OrderPurchaseType.PersonalConsumption;
                theLimits.PurchaseLimitType = PurchaseLimitType.TotalPaid;
            }

            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Must be VAT register to dont have volume limited
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            var GRVT = tins.Find(p => p.IDType.Key == "GRVT");
            var GRTN = tins.Find(p => p.IDType.Key == "GRTN");
            var GRSS = tins.Find(p => p.IDType.Key == "GRSS");
            var GRBL = tins.Find(p => p.IDType.Key == "GRBL");
            return (GRVT != null && GRTN != null && GRSS != null && GRBL != null);
        }
    }
}