using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.sr_RS
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Must be VAT register to dont have volume limited
            bool isExempt = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            var firstCombination = tins.Find(p => p.IDType.Key == "SRTX");
            if (firstCombination != null)
            {
                isExempt = true;
            }
            return isExempt;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                       ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                GetPurchasingLimits(cart.DistributorID, string.Empty);
                Result = base.PerformRules(cart, reason, Result);
            }

            return Result;
        }
    }
}
