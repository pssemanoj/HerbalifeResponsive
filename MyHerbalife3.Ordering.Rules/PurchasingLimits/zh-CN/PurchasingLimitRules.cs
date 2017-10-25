using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Logging;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.zh_CN
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private SessionInfo CurrentSession = null;
        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = PurchaseLimitType.Volume);
            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
   
            return false;
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
            var dpm = DistributorProfileModel;
            CurrentSession = (dpm != null)
                ? SessionInfo.GetSessionInfo(dpm.Id, Locale)
                : SessionInfo.GetSessionInfo(cart.DistributorID, Locale);
            string dsid = string.Empty;
            if (CurrentSession.IsReplacedPcOrder)
            {
                dsid = CurrentSession.ReplacedPcDistributorOrderingProfile != null && CurrentSession.ReplacedPcDistributorOrderingProfile.Id != null ? CurrentSession.ReplacedPcDistributorOrderingProfile.Id : cart.DistributorID;
            }
            else
            {
                dsid = cart.DistributorID;
            }
          //   OrderTotals_V01 totals = (OrderTotals_V01)(cart as MyHLShoppingCart).Totals;
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded || reason == ShoppingCartRuleReason.CartRetrieved)
            {
                var Cart = cart as MyHLShoppingCart;
                var calcTheseItems = new List<ShoppingCartItem_V01>();
                
                var purchasingLimitManager = PurchasingLimitManager(dsid);
                var myhlCart = cart as MyHLShoppingCart;

                if (null == myhlCart)
                {
                    LoggerHelper.Error(
                        string.Format("{0} myhlCart is null {1}", Locale, dsid));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                PurchasingLimits_V01 PurchasingLimits =
                    PurchasingLimitProvider.GetCurrentPurchasingLimits(dsid);

                purchasingLimitManager.SetPurchasingLimits(PurchasingLimits);


                if (null == PurchasingLimits)
                {
                    LoggerHelper.Error(
                        string.Format("{0} PurchasingLimits could not be retrieved for distributor {1}", Locale,
                                      dsid));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                var shoppingCart = cart as MyHLShoppingCart;
                if (shoppingCart == null)
                    return Result;

                if (PurchasingLimits.MaxPCEarningsLimit == -1)
                {
                    return Result;
                }
                DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");
                if ((CurrentSession.ReplacedPcDistributorOrderingProfile != null && CurrentSession.ReplacedPcDistributorOrderingProfile.IsPC) || (distributorOrderingProfile != null && distributorOrderingProfile.IsPC))
                {
                    if (cart.CurrentItems != null)
                    {
                        calcTheseItems.AddRange(from i in cart.CurrentItems
                                                where !APFDueProvider.IsAPFSku(i.SKU)
                                                select
                                                    new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                             i.MinQuantity));
                    }
                    if (cart.CartItems != null)
                    {
                        calcTheseItems.AddRange(from i in cart.CartItems
                                                where !APFDueProvider.IsAPFSku(i.SKU)
                                                select
                                                    new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                             i.MinQuantity));
                    }
                    var totals = Cart.Calculate(calcTheseItems, false) as OrderTotals_V01;

                    //OrderTotals_V01 totals = (OrderTotals_V01)(cart as MyHLShoppingCart).Totals;
                    if (totals != null && totals.DiscountedItemsTotal > purchasingLimitManager.MaxPersonalPCConsumptionLimit)
                    {
                        if (cart.CurrentItems.Count > 0 )
                        {
                            var sessionInfo = SessionInfo.GetSessionInfo(cart.DistributorID, Locale);
                            if (sessionInfo != null && sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.CurrentItems !=null)
                            {
                                sessionInfo.ShoppingCart.CurrentItems.RemoveAll(x => x.SKU != null);
                            }
                        cart.CurrentItems.RemoveAll(s => s.SKU != null);
                        }
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "ErrorMessageToPCWhenhittheFOPLimits").ToString(),
                                        purchasingLimitManager.MaxPersonalPCConsumptionLimit));
                     }
                    else 
                    {
                        var ruleMessage = cart.RuleResults.FirstOrDefault(x => x.Messages != null && x.Messages.Count > 0 && x.RuleName == "PurchasingLimits Rules");
                        if(ruleMessage !=null)
                        {
                            cart.RuleResults.Remove(ruleMessage);
                        }
                    }
                   
                     cart.RuleResults.Add(Result);
                }
                else
                {
                    decimal DistributorRemainingVolumePoints = 0;
                    decimal NewVolumePoints = 0;


                    if (null == myhlCart)
                    {
                        LoggerHelper.Error(
                            string.Format("{0} myhlCart is null {1}", Locale, cart.DistributorID));
                        Result.Result = RulesResult.Failure;
                        return Result;
                    }

                    DistributorRemainingVolumePoints = PurchasingLimits.RemainingVolume;
                    if (cart.CurrentItems != null && cart.CurrentItems.Count > 0)
                    {
                    var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                    if (currentItem != null)
                    {
                            NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;
                        }
                    }

                    if (NewVolumePoints > 0 || purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan)
                    {
                        //if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.Volume)
                        {
                            if (PurchasingLimits.maxVolumeLimit == -1)
                            {
                                return Result;
                            }
                            decimal cartVolume = (cart as MyHLShoppingCart).VolumeInCart;

                            if (DistributorRemainingVolumePoints - (cartVolume + NewVolumePoints) < 0)
                            {
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(
                                            string.Format(
                                                HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                                    "VolumePointExceedsThresholdByIncreasingQuantity").ToString(),
                                                purchasingLimitManager.MaxPersonalPCConsumptionLimit));
                                cart.RuleResults.Add(Result);
                                //if (purchasingLimitManager.PurchasingLimitsRestriction ==
                                //    PurchasingLimitRestrictionType.MarketingPlan)
                                //    //MPE Thresholds
                                //{
                                //    if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                                //    {
                                //        Result.AddMessage(
                                //            string.Format(
                                //                HttpContext.GetGlobalResourceObject(
                                //                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                //                    "VolumePointExceedsThresholdByIncreasingQuantity").ToString(),
                                //                cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                                //    }
                                //else
                                //{
                                //    Result.AddMessage(
                                //        string.Format(
                                //            HttpContext.GetGlobalResourceObject(
                                //                string.Format("{0}_Rules", HLConfigManager.Platform),
                                //                "VolumePointExceedsThreshold").ToString(), cart.CurrentItems[0].SKU));
                                //}
                                //}
                                //else
                                //{
                                //    if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                                //    {
                                //        Result.AddMessage(
                                //            string.Format(
                                //                HttpContext.GetGlobalResourceObject(
                                //                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                //                    "VolumePointExceedsByIncreasingQuantity").ToString(),
                                //                cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                                //    }
                                //    else
                                //    {
                                //        Result.AddMessage(
                                //            string.Format(
                                //                HttpContext.GetGlobalResourceObject(
                                //                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                //                    "VolumePointExceeds").ToString(), cart.CurrentItems[0].SKU));
                                //    }
                                //}

                            }
                        }
                   
                    }
                }
            }
            return Result;
        }
    }
}