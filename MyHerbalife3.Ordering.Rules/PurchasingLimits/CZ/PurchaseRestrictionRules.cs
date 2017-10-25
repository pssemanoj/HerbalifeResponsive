using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchaseRestriction;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.CZ
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            if (manager.ApplicableLimits == null)
                return;

            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);

            PurchaseLimitType limitType = PurchaseLimitType.Volume;
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
            {
                return;
            }

            var now = DateUtils.GetCurrentLocalTime("CZ");
            foreach (TaxIdentification taxId in tins)
            {
                if (taxId.IDType != null && (taxId.IDType.Key == "CZBL" && taxId.IDType.ExpirationDate > now))
                {
                    limitType = PurchaseLimitType.None;
                    break;
                }
            }

            if (limits != null)
            {
                if (limits.maxVolumeLimit == -1 && limits.MaxEarningsLimit == -1)
                {
                    limits.PurchaseLimitType = PurchaseLimitType.None;
                }
                else
                {
                    limits.PurchaseLimitType = limitType;
                }
                SetLimits(orderMonth, manager, limits);
            }
        }
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
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

            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
            if (cart == null || currentlimits == null)
                return Result;

            if (cart.ItemsBeingAdded == null || cart.ItemsBeingAdded.Count == 0)
                return Result;

            string processingCountryCode = DistributorProfileModel.ProcessingCountryCode;

            //bool bCanPurchasePType = CanPurchasePType(cart.DistributorID);
            var errors = new List<string>();
            decimal NewVolumePoints = decimal.Zero;
            decimal cartVolume = cart.VolumeInCart;
            bool bLimitExceeded = false;
            List<string> skuToAdd = new List<string>();

            foreach (var item in cart.ItemsBeingAdded)
            {
                var currentItem = CatalogProvider.GetCatalogItem(item.SKU, Country);
                if (currentItem == null)
                {
                    continue;
                }
                                
                if (currentlimits.PurchaseLimitType == PurchaseLimitType.Volume || currentlimits.RestrictionPeriod == PurchasingLimitRestrictionPeriod.PerOrder)
                {
                    if (currentlimits.maxVolumeLimit == -1)
                    {
                        skuToAdd.Add(item.SKU);
                        continue;
                    }
                    NewVolumePoints += currentItem.VolumePoints * item.Quantity;

                    if (currentlimits.RemainingVolume - (cartVolume + NewVolumePoints) < 0)
                    {   
                        if (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP || currentlimits.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold)
                        //MPE FOP
                        {
                            Result.Result = RulesResult.Failure;
                            ///Order exceeds the allowable volume for First Order Program. The Volume on the order needs to be reduced by {0:F2} VPs. The following SKU(s) have not been added to the cart.
                            if (!bLimitExceeded) //  to add this message only once
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
                                bLimitExceeded = true;
                            }
                            /// Item SKU:{0}.
                            errors.Add(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "VolumePointExceedsThreshold").ToString(), item.SKU));
                        }
                        else
                        {
                            if (currentItem.ProductType != ServiceProvider.CatalogSvc.ProductType.Product)
                            {
                                skuToAdd.Add(item.SKU);
                            }
                            else
                            {
                                Result.Result = RulesResult.Failure;
                                errors.Add(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "PurchaseLimitTypeProductCategory").ToString(), item.SKU));                                
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
                if (cart.OnCheckout && (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP || currentlimits.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold))
                {
                    Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "FOPVolumePointExceedsOnCheckout").ToString(), 1100, PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(processingCountryCode), PurchaseRestrictionProvider.GetThresholdPeriod(processingCountryCode), (cartVolume + NewVolumePoints) - currentlimits.RemainingVolume));
                }
                else
                {
                    errors = errors.Select(x => x.Replace("-999", ((cartVolume + NewVolumePoints) - currentlimits.RemainingVolume).ToString())).ToList<string>();
                    Array.ForEach(errors.ToArray(), a => Result.AddMessage(a));
                }
            }
            cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));

            return Result;
        }    
    }
}
