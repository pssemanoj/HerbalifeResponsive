using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.FR
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        private readonly List<string> ExemptSubTypes = new List<string>(new[] { "E", "N", "NA", "" });
        private readonly List<string> ResaleSubTypes = new List<string>(new[] { "RE" });
        private readonly List<string> RetailSubTypes = new List<string>(new[] { "PC" });

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
                return;

            DistributorOrderingProfile orderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorId,"FR");
            limits.PurchaseLimitType = orderingProfile.OrderSubType == "F" ? PurchaseLimitType.Volume : PurchaseLimitType.None;
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
            SetLimits(orderMonth, manager, limits);

        }

        public override void SetLimits(int orderMonth, IPurchaseRestrictionManager manager, PurchasingLimits_V01 limits)
        {
            if (manager.PurchasingLimits == null)
                manager.PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();

            var FOPlimits = GetFOPLimits(manager);
            if (FOPlimits != null && FOPlimits.maxVolumeLimit != -1 && !FOPlimits.Completed)
            {
                manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(FOPlimits);
            }
            else
            {
                manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(limits);
            }
            switch (manager.PurchasingLimits[orderMonth].LimitsRestrictionType)
            {
                // Purchasing Limits
                case LimitsRestrictionType.PurchasingLimits:
                    {
                        PurchaseRestrictionProvider.SetValues(manager.PurchasingLimits[orderMonth], HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionPeriod, manager.PurchasingLimits[orderMonth].PurchaseLimitType);
                    }
                    break;
                default:
                    // FOP and OT is volume
                    PurchaseRestrictionProvider.SetValues(manager.PurchasingLimits[orderMonth], PurchasingLimitRestrictionPeriod.OneTime, manager.PurchasingLimits[orderMonth].maxVolumeLimit > -1 ? PurchaseLimitType.Volume : PurchaseLimitType.None);
                    break;
            }
        }
        public override void SetOrderSubType(string orderSubType, string distributorId)
        {
            var purchaseRestrictionManager = this.GetPurchaseRestrictionManager(distributorId);
            if (purchaseRestrictionManager.PurchasingLimits == null)
                return;
            if (orderSubType.Equals("ETO"))
            {
                orderSubType = "E";
            }

            foreach (var limit in purchaseRestrictionManager.PurchasingLimits.Values)
            {
                if ((limit as PurchasingLimits_V01).LimitsRestrictionType != LimitsRestrictionType.PurchasingLimits)
                    continue;
                (limit as PurchasingLimits_V01).PurchaseSubType = orderSubType;

                if (RetailSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                {
                    (limit as PurchasingLimits_V01).PurchaseType = OrderPurchaseType.PersonalConsumption;
                    (limit as PurchasingLimits_V01).PurchaseLimitType = PurchaseLimitType.DiscountedRetail;
                }
                else if (ResaleSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                {
                    (limit as PurchasingLimits_V01).PurchaseType = OrderPurchaseType.Consignment;
                    (limit as PurchasingLimits_V01).PurchaseLimitType = PurchaseLimitType.None;
                }
                else
                {
                    (limit as PurchasingLimits_V01).PurchaseLimitType = PurchaseLimitType.None;

                    if (!ExemptSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "Unknown Distributor Subtype of \"{0}\" encountered in SetOrderSubType for FR, Distributor: {1}",
                                (limit as PurchasingLimits_V01).PurchaseSubType, distributorId));
                    }
                }
            }
        }

        private ShoppingCartRuleResult reportError(MyHLShoppingCart cart, ShoppingCartItem_V01 item, ShoppingCartRuleResult Result)
        {
            if (cart.CartItems.Exists(i => i.SKU == item.SKU))
            {
                Result.AddMessage(
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform),
                            "DiscountedRetailExceedsByIncreasingQuantity").ToString(),
                        item.SKU, item.Quantity));
            }
            else
            {
                Result.AddMessage(
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
            base.PerformRules(cart, reason, Result);
            if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
            {
                var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
                if (currentlimits.PurchaseLimitType == PurchaseLimitType.DiscountedRetail)
                {
                    var itemsToCalc = new List<ShoppingCartItem_V01>();
                    itemsToCalc.AddRange(cart.CartItems);

                    bool bExceed = false;
                    List<string> skuToAdd = new List<string>();

                    foreach (var item in cart.ItemsBeingAdded)
                    {
                        if (bExceed == true )
                        {
                            Result = reportError(cart, item, Result);
                            continue;
                        }
                        itemsToCalc.Add(item);
                        OrderTotals_V01 orderTotals = cart.Calculate(itemsToCalc, false) as OrderTotals_V01;
                        if (orderTotals != null)
                        {
                            decimal discountedRetailInCart = orderTotals.ItemTotalsList.Sum(x => (x as ItemTotal_V01).DiscountedPrice);
                            var currentItem = CatalogProvider.GetCatalogItem(item.SKU, Country);
                            if ((currentlimits.RemainingVolume - discountedRetailInCart < 0) && currentItem.ProductType == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType.Product)
                            {
                                Result.Result = RulesResult.Failure;
                                bExceed = true;
                                Result = reportError(cart, item, Result);
                                continue;
                            }
                            else
                            {
                                skuToAdd.Add(item.SKU);
                            }
                        }
                    }

                    cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));
                }
            }

            return Result;

        }
    }
}
