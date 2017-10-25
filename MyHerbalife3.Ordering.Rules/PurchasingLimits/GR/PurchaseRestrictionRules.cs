using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchasingLimits;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.GR
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public override void SetOrderSubType(string orderSubType, string distributorId)
        {
            var purchaseRestrictionManager = this.GetPurchaseRestrictionManager(distributorId);
            if (purchaseRestrictionManager.PurchasingLimits == null)
                return;

            foreach (var limit in purchaseRestrictionManager.PurchasingLimits.Values)
            {
                if ((limit as PurchasingLimits_V01).LimitsRestrictionType != LimitsRestrictionType.PurchasingLimits)
                    continue;
                (limit as PurchasingLimits_V01).PurchaseSubType = orderSubType;

                if ((limit as PurchasingLimits_V01).PurchaseLimitType == PurchaseLimitType.TotalPaid)
                {
                    (limit as PurchasingLimits_V01).PurchaseType = OrderPurchaseType.PersonalConsumption;
                }
            }
        }

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            PurchaseLimitType limitType = PurchaseLimitType.TotalPaid;

            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
                return;

            // have all four tins, no limit
            if (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "GRVT", "GRTN", "GRSS", "GRBL" }).Count() == 4)
            {
                limitType = PurchaseLimitType.None;
            }

            limits.PurchaseLimitType = limitType;
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
            SetLimits(orderMonth, manager, limits);

        }
        private ShoppingCartRuleResult reportError(MyHLShoppingCart cart, ShoppingCartItem_V01 item, ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Failure;
            if (cart.CartItems.Exists(i => i.SKU == item.SKU))
            {
                ///The quantity of the item SKU:{0} can not be increased by {1} because it exceeds your Purchasing Limit.
                Result.Messages.Add(
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform),
                            "DiscountedRetailExceedsByIncreasingQuantity").ToString(),
                        item.SKU, item.Quantity));
            }
            else
            {
                ///Item SKU:{0} has not been added to the cart since by adding that into the cart, you exceeded your Purchasing Limit.
                Result.Messages.Add(
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform),
                            "DiscountedRetailExceeds").ToString(), item.SKU));
            }
            cart.RuleResults.Add(Result);
            return Result;
        }
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Success;

            if (cart == null)
                return Result;

            base.PerformRules(cart, reason, Result);

            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
            if (currentlimits.PurchaseLimitType == PurchaseLimitType.None || currentlimits.LimitsRestrictionType != LimitsRestrictionType.PurchasingLimits)
            {
                return Result;
            }

            if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
            {
                var calcTheseItems = new List<ShoppingCartItem_V01>();
                calcTheseItems.AddRange(from i in cart.CartItems
                                        where !APFDueProvider.IsAPFSku(i.SKU)
                                        select
                                            new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                     i.MinQuantity));
                bool bExceed = false;
                List<string> skuToAdd = new List<string>();

                foreach (var item in cart.ItemsBeingAdded)
                {
                    if (bExceed == true)
                    {
                        Result = reportError(cart, item, Result);
                        continue;
                    }
                    var existingItem =
                        calcTheseItems.Find(ci => ci.SKU == item.SKU);
                    if (null != existingItem)
                    {
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        calcTheseItems.Add(new ShoppingCartItem_V01(0, item.SKU, item.Quantity, DateTime.Now));
                    }

                    // remove A and L type
                    var allItems =
                        CatalogProvider.GetCatalogItems(
                            (from s in calcTheseItems where s.SKU != null select s.SKU).ToList(), Country);
                    if (null != allItems && allItems.Count > 0)
                    {
                        var skuExcluded = (from c in allItems.Values
                                           where (c as CatalogItem_V01).ProductType != ServiceProvider.CatalogSvc.ProductType.Product
                                           select c.SKU);
                        calcTheseItems.RemoveAll(s => skuExcluded.Contains(s.SKU));
                    }

                    var totals = cart.Calculate(calcTheseItems, false);
                    if (null == totals)
                    {
                        var message =
                            string.Format(
                                "Purchasing Limits DiscountedRetail calculation failed due to Order Totals returning a null for Distributor {0}",
                                cart.DistributorID);
                        LoggerHelper.Error(message);
                        throw new ApplicationException(message);
                    }

                    if (currentlimits.RemainingVolume - (totals as OrderTotals_V01).AmountDue < 0)
                    {
                        bExceed = true;
                        Result = reportError(cart, item, Result);
                    }
                    else
                    {
                        skuToAdd.Add(item.SKU);
                    }
                }

                cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));

            }
            return Result;

        }
    }
}