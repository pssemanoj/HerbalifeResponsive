using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.ID
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public const string COUNTRY_TIME = "ID";
        public const string ID_TIN_CODE = "IDID";
        public const string DUMMY_TIN_CODE = "1111111111";

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, Providers.FOP.IPurchaseRestrictionManager manager)
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

            if (!HLConfigManager.Configurations.DOConfiguration.EnforcesPurchasingPermissions)
            {
                var now = DateUtils.GetCurrentLocalTime(COUNTRY_TIME);                
                
                var idTIN = RetrieveIDTinCode(tins);
                bool isDummyTIN = IsDummyTin(idTIN);
                bool isIDCOP = IsIDCountryOfProcessing();

                if (idTIN != null && !isDummyTIN)
                {
                    if (idTIN.IDType.ExpirationDate > now) //tin non expired and processing country any
                    {
                        limitType = PurchaseLimitType.None;
                    }
                    else if (!isIDCOP)  //tin expired and foreign member
                    {
                        limitType = PurchaseLimitType.Volume;
                        setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1000.00m);
                    }
                }
                else if (isDummyTIN)  //with dummy TIN purchase restriction per order 1000 VPs
                {
                    limitType = PurchaseLimitType.Volume;
                    setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1000.00m);
                }
                else //no TINs found
                {
                    if (!isIDCOP)  //foreign members can purchase with a limit of 1000 VPs per order
                    {
                        limitType = PurchaseLimitType.Volume;
                        setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1000.00m);
                    }
                    else //ID members with no TIN codes can't buy
                    {
                        limitType = PurchaseLimitType.Volume;
                        setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 0.00m);
                    }
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
            //if (!GetPurchaseRestrictionManager(cart.DistributorID).CanPurchase)
            //{   
            //    cart.ItemsBeingAdded.Clear();
            //    Result.AddMessage(
            //        string.Format(
            //            HttpContext.GetGlobalResourceObject(
            //                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CantBuy").ToString()));
            //    Result.Result = RulesResult.Failure;             
            //    return Result;
            //}

            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
            if (cart == null || currentlimits == null)
                return Result;

            if (cart.ItemsBeingAdded == null || cart.ItemsBeingAdded.Count == 0)
                return Result;

            string processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
                       
            bool bCanPurchase = CanPurchase(cart.DistributorID);
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
                {
                    continue;
                }
                                
                if (APFDueProvider.IsAPFSku(item.SKU) || cart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    skuToAdd.Add(item.SKU);
                }
                else
                {                    
                    if (bCanPurchase)                    
                    {
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
                    }
                    else
                    {   
                        Result.Result = RulesResult.Failure;
                        errors.Add(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "CantBuy").ToString()));
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

                        //verifying VP limits didn't exceed
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
                            else  //it exceeded VP but it's not FOP
                            {
                                var itemExists = cart.CartItems.Find(i => i.SKU == item.SKU);
                                if (itemExists != null)  //item already exists in cart
                                {
                                    Result.Result = RulesResult.Failure;
                                    errors.Add(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsByIncreasingQuantity").ToString(), item.SKU, item.Quantity));
                                }
                                else
                                {
                                    Result.Result = RulesResult.Failure;
                                    errors.Add(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceeds").ToString(), item.SKU));
                                }
                            }
                        }
                        else // // VP limits didn't exceeded, add item
                        {
                            skuToAdd.Add(item.SKU);
                        }
                    }
                    else  //purchasing limits are not per VP nor per order: add item
                    {
                        skuToAdd.Add(item.SKU);
                    }
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

        private void setValues(PurchasingLimits_V01 l, PurchasingLimitRestrictionPeriod period, decimal maxValue)
        {
            l.RestrictionPeriod = period;
            l.maxVolumeLimit = l.RemainingVolume = maxValue;
            l.LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits;
        }

        private bool CanPurchase(string distributorID)
        {
            bool canPurchase = true;

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, false, true);
            var now = DateUtils.GetCurrentLocalTime(COUNTRY_TIME);
            var idTIN = RetrieveIDTinCode(tins);
            bool isDummyTIN = IsDummyTin(idTIN);
            bool isIDCOP = IsIDCountryOfProcessing();

            if (idTIN != null)
            {
                if (!isDummyTIN)
                {
                    if (isIDCOP && idTIN.IDType.ExpirationDate < now) //ID Member with expired TIN
                    {
                        canPurchase = false;
                    }
                }
            }
            else //no TINs found
            {
                if (isIDCOP)  //ID members can't purchase without TINs
                {
                    canPurchase = false;
                }
            }
            
            return canPurchase;
        }

        private bool CanPurchasePType(string distributorID)
        {
            bool canPurchasePType = true;

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, false, true);
            var now = DateUtils.GetCurrentLocalTime(COUNTRY_TIME);
            var idTIN = RetrieveIDTinCode(tins);
            bool isDummyTIN = IsDummyTin(idTIN);
            bool isIDCOP = IsIDCountryOfProcessing();
                        
            if (!isIDCOP)   //restriction only for foreign members only
            {
                if (idTIN == null)
                {
                    canPurchasePType = false;
                }
                else
                {
                    if (!isDummyTIN && idTIN.IDType.ExpirationDate < now)
                    {
                        canPurchasePType = false;
                    }
                }
            }

            return canPurchasePType;
        }

        private TaxIdentification RetrieveIDTinCode(List<TaxIdentification> tins)
        {
            return tins.Find(t => t.IDType.Key == ID_TIN_CODE);            
        }

        private bool IsDummyTin(TaxIdentification tin)
        {
            return (tin != null && tin.ID == DUMMY_TIN_CODE) ? true : false;
        }

        private bool IsIDCountryOfProcessing()
        {
            return (new List<string>(CountryType.ID.HmsCountryCodes)).Union(new List<string>() { CountryType.ID.Key }).Contains(DistributorProfileModel.ProcessingCountryCode) ?
                    true : false;
        }
    }
}
