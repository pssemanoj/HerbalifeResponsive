using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchaseRestriction;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.IT
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        private readonly List<string> ExemptSubTypes = new List<string>(new[] { "C" });
        private readonly List<string> ResaleSubTypes = new List<string>(new[] { "A1", "B1" });
        private readonly List<string> RetailSubTypes = new List<string>(new[] { "A2", "B2", "D", "D2", "E" });


        public override void SetOrderSubType(string orderSubType, string distributorId)
        {
            var purchaseRestrictionManager = this.GetPurchaseRestrictionManager(distributorId);
            if (purchaseRestrictionManager.PurchasingLimits == null)
                return;
            if (orderSubType.Equals("D") || orderSubType.Equals("ETO"))
            {
                orderSubType = "D2";
            }

            foreach (var limit in purchaseRestrictionManager.PurchasingLimits.Values)
            {
                if (limit == null)
                    continue;
                (limit as PurchasingLimits_V01).PurchaseSubType = orderSubType;

                ///for Members in FOP period. Members placing A2/B2/D2/E Italy order types should always be restricted to purchase up to the value returned in AvailablePCLimit, not in AvailableFOPLimit. Value in AvailableFOPLimit is valid only for A1/B1 Italy order types.
                ///
                var FOPlimits = GetFOPLimits(purchaseRestrictionManager);
                var purchasingLimits = GetLimits(LimitsRestrictionType.PurchasingLimits, limit.Year * 100 + limit.Month, purchaseRestrictionManager);

                if (RetailSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                {
                    (limit as PurchasingLimits_V01).PurchaseType = OrderPurchaseType.PersonalConsumption;
                    if (purchasingLimits != null)
                    {
                        (limit as PurchasingLimits_V01).PurchaseLimitType = purchasingLimits.maxVolumeLimit == -1 ? PurchaseLimitType.None : PurchaseLimitType.Volume;
                        (limit as PurchasingLimits_V01).maxVolumeLimit = purchasingLimits.maxVolumeLimit;
                        (limit as PurchasingLimits_V01).RemainingVolume = purchasingLimits.RemainingVolume;
                    }
                }
                else if (ResaleSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                {
                    (limit as PurchasingLimits_V01).PurchaseType = OrderPurchaseType.Consignment;
                    
                    if ((limit as PurchasingLimits_V01).LimitsRestrictionType == LimitsRestrictionType.FOP)
                    {
                        (limit as PurchasingLimits_V01).PurchaseLimitType = FOPlimits.maxVolumeLimit == -1 ? PurchaseLimitType.None : PurchaseLimitType.Volume;
                        (limit as PurchasingLimits_V01).maxVolumeLimit = FOPlimits.maxVolumeLimit;
                        (limit as PurchasingLimits_V01).RemainingVolume = FOPlimits.RemainingVolume;
                    }
                    else
                    {
                        (limit as PurchasingLimits_V01).PurchaseLimitType = PurchaseLimitType.Earnings;
                        (limit as PurchasingLimits_V01).PurchaseLimitType = (limit as PurchasingLimits_V01).MaxEarningsLimit == -1 || orderSubType != "B1" ? PurchaseLimitType.None : PurchaseLimitType.Earnings;
                    }
                }
                else
                {
                    if (!ExemptSubTypes.Contains((limit as PurchasingLimits_V01).PurchaseSubType))
                    {
                        //Error logging commented to avoid unnecessary logging since this condition doesn't take any action
                        //If message is re-enabled change from "Distributor Subtype" to "Order Subtype"
                        //LoggerHelper.Error(
                        //    string.Format(
                        //        "Unknown Distributor Subtype of \"{0}\" encountered in PurchaseRestrictionRules for it-IT, Distributor: {1}",
                        //        (limit as PurchasingLimits_V01).PurchaseSubType, distributorId));
                    }
                }
            }
        }

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
                return;

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

        private ShoppingCartRuleResult reportError(MyHLShoppingCart cart, ShoppingCartItem_V01 item, ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Failure;
            if (cart.CartItems.Exists(i => i.SKU == item.SKU))
            {
                Result.AddMessage(
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform),
                            "EarningExceedsByIncreasingQuantity").ToString(),
                        item.SKU, item.Quantity));
            }
            else
            {
                Result.AddMessage(
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform), "EarningExceeds")
                                   .ToString(), item.SKU));
            }
            cart.RuleResults.Add(Result);
            return Result;
        }

        private decimal ProductEarningsInCart(List<ShoppingCartItem_V01> items, decimal discountPercentage)
        {
            decimal CartEarnings = 0.0M;
            foreach (ShoppingCartItem_V01 CartItem in items)
            {
                var item = CatalogProvider.GetCatalogItem(CartItem.SKU, this.Country);
                if (item.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                {
                    if (item != null)
                    {
                        CartEarnings += (item.EarnBase) * CartItem.Quantity * discountPercentage / 100;
                    }
                }
            }

            return CartEarnings;
        }
        private ShoppingCartRuleResult performRules(MyHLShoppingCart cart, ShoppingCartRuleResult Result, PurchasingLimits_V01 currentlimits)
        {
            if (!GetPurchaseRestrictionManager(cart.DistributorID).CanPurchase)
            {
                cart.ItemsBeingAdded.Clear();
                Result.AddMessage(
                   string.Format(
                       HttpContext.GetGlobalResourceObject(
                           string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CantBuy").ToString()));
                Result.Result = RulesResult.Failure;
                return Result;
            }

            if (cart.ItemsBeingAdded == null || cart.ItemsBeingAdded.Count == 0)
                return Result;

            bool bCanPurchasePType = CanPurchasePType(cart.DistributorID);
            var errors = new List<string>();
            decimal NewVolumePoints = decimal.Zero;
            decimal cartVolume = cart.VolumeInCart;
            bool bLimitExceeded = false;
            List<string> skuToAdd = new List<string>();

            foreach (var item in cart.ItemsBeingAdded)
            {
                var currentItem = CatalogProvider.GetCatalogItem(item.SKU, Country);
                if (currentItem == null)
                    continue;
                if (!bCanPurchasePType)
                {
                    if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                    {
                        Result.Result = RulesResult.Failure;
                        errors.Add(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                    "PurchaseLimitTypeProductCategory").ToString(), item.SKU));
                        continue;
                    }
                }
                if (currentlimits.PurchaseLimitType == PurchaseLimitType.Volume)
                {
                    if (currentlimits.maxVolumeLimit == -1)
                    {
                        skuToAdd.Add(item.SKU);
                        continue;
                    }
                    NewVolumePoints += currentItem.VolumePoints * item.Quantity;

                    if (currentlimits.RemainingVolume - (cartVolume + NewVolumePoints) < 0)
                    {
                        Result.Result = RulesResult.Failure;
                        if (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                        //MPE FOP
                        {
                            string processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
                            ///Order exceeds the allowable volume for First Order Program. The Volume on the order needs to be reduced by {0:F2} VPs. The following SKU(s) have not been added to the cart.
                            if (!bLimitExceeded) //  to add this message only once
                            {
                                if (currentlimits.PurchaseType == OrderPurchaseType.Consignment)
                                    errors.Add(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "FOPConsignmentVolumePointExceeds").ToString(), 1100, PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(processingCountryCode), PurchaseRestrictionProvider.GetThresholdPeriod(processingCountryCode)));
                                else if (currentlimits.PurchaseType == OrderPurchaseType.PersonalConsumption)
                                    errors.Add(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "FOPPersonalConsumptionVolumePointExceeds").ToString(), 1100, PurchaseRestrictionProvider.GetThresholdPeriod(processingCountryCode)));
                                else
                                {
                                    errors.Add(
                                       string.Format(
                                           HttpContext.GetGlobalResourceObject(
                                               string.Format("{0}_Rules", HLConfigManager.Platform),
                                               "FOPVolumePointExceeds").ToString(), 1100,
                                           PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(
                                               processingCountryCode),
                                           PurchaseRestrictionProvider.GetThresholdPeriod(processingCountryCode), -999));
                                    // -999 should be replaced with caluclated value.
                                }
                                bLimitExceeded = true;
                            }
                            /// Item SKU:{0}.
                            //errors.Add(
                            //    string.Format(
                            //        HttpContext.GetGlobalResourceObject(
                            //            string.Format("{0}_Rules", HLConfigManager.Platform),
                            //            "VolumePointExceedsThreshold").ToString(), item.SKU));
                        }
                        else
                        {
                            if (cart.CartItems.Exists(i => i.SKU == item.SKU))
                            {
                                ///The quantity of the item SKU:{0} can not be increased by {1} because it exceeds your volume points limit.
                                errors.Add(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "VolumePointExceedsByIncreasingQuantity").ToString(),
                                        item.SKU, item.Quantity));
                            }
                            else
                            {
                                ///Item SKU:{0} has not been added to the cart since by adding that into the cart, you exceeded your volume points  limit.
                                errors.Add(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "VolumePointExceeds").ToString(), item.SKU));
                            }
                        }
                    }
                    else
                    {
                        skuToAdd.Add(item.SKU);
                    }
                }
                else
                {
                    skuToAdd.Add(item.SKU);
                }
            }
            if (Result.Result == RulesResult.Failure && errors.Count > 0)
            {
                errors = errors.Select(x => x.Replace("-999", ((cartVolume + NewVolumePoints) - currentlimits.RemainingVolume).ToString())).ToList<string>();
                Array.ForEach(errors.ToArray(), a => Result.AddMessage(a));
            }
            cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));

            return Result;
        }
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
            if (cart == null || currentlimits == null)
                return Result;
            Result = performRules(cart, Result, currentlimits);
             if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
             {
                 if (currentlimits.PurchaseLimitType == PurchaseLimitType.Earnings && cart.OrderSubType == "B1")
                 {
                     bool bExceed = false;
                     List<string> skuToAdd = new List<string>();

                     var itemsToCalc = new List<ShoppingCartItem_V01>();
                     itemsToCalc.AddRange(cart.CartItems);

                     foreach (var item in cart.ItemsBeingAdded)
                     {
                         if (bExceed == true)
                         {
                             Result = reportError(cart, item, Result);
                             continue;
                         }
                         itemsToCalc.Add(item);
                         OrderTotals_V01 orderTotals = cart.Calculate(itemsToCalc,false) as OrderTotals_V01;
                         if (orderTotals != null)
                         {
                             decimal earningsInCart = ProductEarningsInCart(itemsToCalc,orderTotals.DiscountPercentage);

                             if (currentlimits.RemainingEarnings - earningsInCart < 0)
                             {
                                 bExceed = true;
                                 Result = reportError(cart, item, Result);
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
