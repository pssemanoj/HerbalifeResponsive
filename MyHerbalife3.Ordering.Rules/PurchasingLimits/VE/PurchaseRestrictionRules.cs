using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.VE
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                             ShoppingCartRuleReason reason,
                                                             ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Success;

            if (cart == null)
                return Result;

            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, cart.OrderMonth);
            if (currentlimits.PurchaseLimitType == ServiceProvider.OrderSvc.PurchaseLimitType.None)
            {
                return Result;
            }

            if (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                base.PerformRules(cart, reason, Result); // check FOP

            if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
            {
                List<string> skuToAdd = new List<string>();
                decimal previousVolumePoints = decimal.Zero;
                foreach (var item in cart.ItemsBeingAdded)
                {
                    if (MyHerbalife3.Ordering.Rules.PurchaseRestriction.AR.PurchaseRestrictionRules.checkVolumeLimits(cart, ref previousVolumePoints, Result, currentlimits, "es-VE", "VE", item))
                        skuToAdd.Add(item.SKU);
                }
                cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));
            }

            return Result;
        }
    }
}
