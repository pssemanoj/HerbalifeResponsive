using System.Collections.Generic;

using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.ro_RO
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            // Distributors who have ROBL tin code do not have any restriction
            
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            if (tins.Find(p => p.IDType.Key == "APP") != null)
                return false;
            var robl = tins.Find(p => p.IDType.Key == "ROBL");
            return (robl != null);
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var currentLimits = base.GetPurchasingLimits(cart.DistributorID, string.Empty);
                var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
                // If it's threshold volume point for all product types is counted
                if (PurchasingLimitProvider.IsOrderThresholdMaxVolume(theLimits))
                {
                    Result = base.PerformRules(cart, reason, Result);
                }
                else if (!DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
                {
                    CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                    if (currentItem != null && currentItem.ProductType == ProductType.Product)
                    {
                        Result = base.PerformRules(cart, reason, Result);
                    }
                }
            }

            return Result;
        }
    }
}