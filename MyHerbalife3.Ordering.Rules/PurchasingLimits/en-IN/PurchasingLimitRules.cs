using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.en_IN
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private readonly List<string> IndiaFullPurchaseTINs =
            new List<string>(new[] {"INTX", "INPP", "INRS", "INNR", "INAD", "INRC", "INVR", "INDL" });

        private readonly List<string> IndiaRestrictedPurchaseTINs = new List<string>(new[] {"INID"});

        private readonly List<string> IndiaFSSAIAllowedTINs = new List<string>(new[] { "FB01", "FB02", "FB03", "FB04", "FB05", "FBL1", "FBL2", "FBL3", "FBL4", "FBL5", "FBOR" });



        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            var limitsType = PurchaseLimitType.ProductCategory;

            if (!DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                limitsType = PurchaseLimitType.Volume;
            }
            else
            {
                var id =
                    (from t in DistributorOrderingProfileProvider.GetTinList(distributorId, true) select t.IDType.Key);
                if (IndiaFullPurchaseTINs.Intersect(id).ToList().Count > 0)
                {
                    limitsType = PurchaseLimitType.None;
                }
            }

            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);
            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            var tins =
                (from t in DistributorOrderingProfileProvider.GetTinList(distributorId, true) select t.IDType.Key);
            return IndiaRestrictedPurchaseTINs.Intersect(tins).ToList().Count == 0;
        }

        /// <summary>
        ///     The IShoppingCart Rule Interface implementation
        /// </summary>
        /// <param name="cart">The current Shopping Cart</param>
        /// <param name="reason">The Rule invoke Reason</param>
        /// <param name="Result">The Rule Results collection</param>
        /// <returns>The cumulative rule results - including the results of this iteration</returns>
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                               ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                decimal DistributorRemainingVolumePoints = 0;
                decimal NewVolumePoints = 0;

                if (cart.CurrentItems == null || cart.CurrentItems.Count == 0)
                {
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                var orderMonth = new OrderMonth(Country);

                PurchasingLimitManager(cart.DistributorID).SetPurchasingLimits(PurchasingLimitProvider.GetOrderMonth());

                var PurchasingLimits =
                    PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);

                if (null == PurchasingLimits)
                {
                    LoggerHelper.Error(
                        string.Format("{0} PurchasingLimits could not be retrieved for distributor {1}", Locale,
                                      cart.DistributorID));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                DistributorRemainingVolumePoints = PurchasingLimits.RemainingVolume;
                var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                if (currentItem != null)
                {
                    if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                    {
                        PurchasingLimitProvider.GetPurchasingLimits(cart.DistributorID, "ETO");
                    }
                    else
                    {
                        NewVolumePoints = currentItem.VolumePoints*cart.CurrentItems[0].Quantity;
                    }
                }

                // validate order threshold first
                if (PurchasingLimits.maxVolumeLimit == -1)
                {
                    return Result;
                }

                if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.ProductCategory && currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                "PurchaseLimitTypeProductCategory") as string ?? string.Empty,
                            cart.CurrentItems[0].SKU));
                    cart.RuleResults.Add(Result);
                    return Result;
                }

                // validate against order threshold first
                decimal cartVolume = (cart as MyHLShoppingCart).VolumeInCart;
                if (PurchasingLimitProvider.IsOrderThresholdMaxVolume(PurchasingLimits) ||
                    PurchasingLimits.PurchaseLimitType != PurchaseLimitType.None)
                {
                    if (DistributorRemainingVolumePoints - (cartVolume + NewVolumePoints) < 0)
                    {
                        Result.Result = RulesResult.Failure;

                        string message = string.Empty;

                        if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                        {
                            Result.Messages.Add(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "VolumePointExceedsThresholdByIncreasingQuantity") as string ?? string.Empty,
                                    cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));

                            
                        }
                        else
                        {
                            Result.Messages.Add(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "VolumePointExceedsThreshold") as string ?? string.Empty, cart.CurrentItems[0].SKU));
                            
                        }

                        if(Result.Messages.Any())
                        {
                            cart.RuleResults.Add(Result);
                        }
                        else
                        {
                            Result.Result = RulesResult.Success;
                        }
                            
                        return Result; // if fails, just return
                    }
                }

                if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.ProductCategory)
                {
                    if (DistributorRemainingVolumePoints -
                        ((cart as MyHLShoppingCart).VolumeInCart + NewVolumePoints) < 0)
                    {
                        Result.Result = RulesResult.Failure;
                        if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                        {
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "VolumePointExceedsThresholdByIncreasingQuantity") as string ?? string.Empty,
                                    cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                        }
                        else
                        {
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "VolumePointExceedsThreshold") as string ?? string.Empty, cart.CurrentItems[0].SKU));
                        }
                    }
                }
                else
                {
                    Result.Result = RulesResult.Success;
                }
            }
            return Result;
        }
    }
}