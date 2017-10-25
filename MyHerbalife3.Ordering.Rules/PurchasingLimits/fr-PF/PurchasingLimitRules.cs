using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.fr_PF
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        //private readonly List<string> ExemptSubTypes = new List<string>(new[] { "E", "N", "NA", "" });
        //private readonly List<string> ResaleSubTypes = new List<string>(new[] { "RE" });
        //private readonly List<string> RetailSubTypes = new List<string>(new[] { "PC" });


        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {

            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            //DS without FPBL are restricted to 5000VPs
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            isExempt = tins.Find(p => p.IDType.Key == "FPBL") != null;

            return isExempt;            
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
                    if (currentItem != null && (currentItem.ProductType == ProductType.Product || currentItem.ProductType == ProductType.PromoAccessory))
                    {
                        Result = base.PerformRules(cart, reason, Result);
                    }
                }
            }

            return Result;
        }
        
    }
}
