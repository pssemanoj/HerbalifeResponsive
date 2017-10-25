using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_AR
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule
    {
        private const decimal MaxAmount = 999.99m;

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var codes = new List<string>(CountryType.AR.HmsCountryCodes);
            codes.Add(CountryType.AR.Key);
            var arCOP = codes.Contains(DistributorProfileModel.ProcessingCountryCode);

            var purchasingLimits = base.GetPurchasingLimits(distributorId, TIN);

            // inform UI to not show remaining info.
            var purchasingLimitManager = PurchasingLimitManager(distributorId);
            var currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
            if (currentLimits.RemainingVolume == MaxAmount || !arCOP || currentLimits.maxVolumeLimit == -1)
            {
                purchasingLimitManager.PurchasingLimits.Values.AsQueryable()
                              .ToList()
                              .ForEach(pl => pl.PurchaseLimitType = PurchaseLimitType.None);
                
            }
            return purchasingLimits;
        }

        private void checkPerOrderLimit(MyHLShoppingCart myCart, ShoppingCartRuleResult Result)
        {
            ShoppingCartItem_V01 backupItem = null;
            var existingItem =
                myCart.ShoppingCartItems.FirstOrDefault(p => p.SKU == myCart.CurrentItems[0].SKU);
            if (null != existingItem)
            {
                backupItem = new ShoppingCartItem_V01(existingItem.ID, existingItem.SKU,
                                                      existingItem.Quantity,
                                                      DateTime.Now, existingItem.MinQuantity);
            }

            myCart.AddItemsToCart(myCart.CurrentItems, true);

            var Totals = myCart.Calculate() as OrderTotals_V01;

            if (Totals != null && Totals.AmountDue > MaxAmount)
            {
                var globalResourceObject =
                    HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_Rules", HLConfigManager.Platform), "AmountLimitExceeds");
                if (globalResourceObject != null)
                    Result.AddMessage(
                        string.Format(
                            globalResourceObject
                                .ToString(), MaxAmount.ToString()));
                globalResourceObject =
                               HttpContext.GetGlobalResourceObject(
                                   string.Format("{0}_Rules", HLConfigManager.Platform),
                                   "DisgardCommonMessage");
                if (globalResourceObject != null)
                    Result.AddMessage(globalResourceObject.ToString());
                Result.Result = RulesResult.Failure;
            }

            myCart.DeleteItemsFromCart(new List<string> { myCart.CurrentItems[0].SKU }, true);

            if (backupItem != null)
            {
                myCart.AddItemsToCart(new List<ShoppingCartItem_V01> { backupItem }, true);
                myCart.Calculate();
            }
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                               ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            var myCart = cart as MyHLShoppingCart;
            if (myCart == null)
                return Result;

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var codes = new List<string>(CountryType.AR.HmsCountryCodes);
                codes.Add(CountryType.AR.Key);
                var arCOP = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
   
                if (!arCOP) // 
                {
                    //Members with any Sub types whose country of processing is not equal to Brazil/ Bolivia/ Paraguay/ Uruguay, 
                    //If added to white list then respective Members can place ‘P’ type orders with a purchasing limit of 999.99 pesos/order.
                    bool isExempted = CatalogProvider.IsDistributorExempted(myCart.DistributorID);
                    var nonVolumeOrderingCountries = new List<string> { "BR", "UY", "BO", "PY" };

                    if (nonVolumeOrderingCountries.Contains(DistributorProfileModel.ProcessingCountryCode))
                    // equal to BR,UY,BO,PY
                    {
                        if (isExempted) // in whitelist
                        {
                            checkPerOrderLimit(myCart, Result);
                        }
                        else // not in whitelist
                        {
                            var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                            if (currentItem.ProductType != ServiceProvider.CatalogSvc.ProductType.Literature)
                            {
                                Result.Result = RulesResult.Failure;
                                var globalResourceObject =
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "PurchaseLimitTypeProductCategory");
                                if (globalResourceObject != null)
                                    Result.AddMessage(
                                        string.Format(
                                            globalResourceObject.ToString(), cart.CurrentItems[0].SKU));
                                
                                globalResourceObject =
                                   HttpContext.GetGlobalResourceObject(
                                       string.Format("{0}_Rules", HLConfigManager.Platform),
                                       "DisgardCommonMessage");
                                if (globalResourceObject != null)
                                    Result.AddMessage(globalResourceObject.ToString());
                                cart.RuleResults.Add(Result);
                            }
                            else
                            {
                                checkPerOrderLimit(myCart, Result);
                            }
                        }
                    }
                    else // not equal to BR,UY,BO,PY
                    {
                        checkPerOrderLimit(myCart, Result);
                    }
                }
                else
                {
                    Result = checkVolumeLimits(myCart, Result, "es-AR", "AR");
                }
            }
            return Result;

        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            return false;
        }
    }
}