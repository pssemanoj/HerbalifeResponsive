using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction
{
    public class PurchaseRestrictionRuleBase : MyHerbalifeRule, IPurchaseRestrictionRule
    {
        #region Fields

        #endregion Fields

        #region Constructor
        public PurchaseRestrictionRuleBase()
        {
        }
        #endregion Constructor

        #region IPurchaseRestrictionRule

        public virtual void SetOrderSubType(string orderSubType, string distributorId)
        {
        }

        public virtual bool PurchasingLimitsAreExceeded(int orderMonth, string distributorId)
        {
            return false;
        }

        /// <summary>
        /// this is called when cart created or when cache expires
        /// </summary>
        /// <param name="purchaseRestrictionManager"></param>
        /// <param name="locale"></param>
        /// <param name="orderMonth"></param>
        /// <param name="distributorId"></param>
        /// <param name="orderSubType"></param>
        public virtual void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            //if (manager.PurchasingLimits == null)
            //    return;
            if ( manager.ApplicableLimits == null)
                return;
            string processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
            if ((manager.CanPurchase = CanPurchase(tins, processingCountryCode, Country)) == true)
            {
                manager.CanPurchasePType = CanPurchasePType(tins, processingCountryCode, Country);
            }
            else
            {
                manager.CanPurchasePType = false;
            }
            if (manager.CanPurchasePType == true && Settings.GetRequiredAppSetting<bool>("CheckGBNewMember", false))
            {
                if (processingCountryCode == "GB" && Country != "GB" )
                {
                    DateTime currentLocalDatetime = HL.Common.Utilities.DateUtils.ConvertToLocalDateTime(DateTime.Now, Country);
                    if (currentLocalDatetime.Subtract(DistributorOrderingProfileProvider.GetProfile(distributorId, Country).ApplicationDate).TotalDays < 7)
                    {
                        manager.CanPurchase = false;
                    }
                }
            }

            if (processingCountryCode == "IN")
            {
                manager.ExtendRestrictionErrorMessage = new[] { "FB01", "FB02", "FB03", "FB04", "FB05", "FBL1", "FBL2", "FBL3", "FBL4", "FBL5", "FBOR" }.Intersect(from t in tins select t.IDType.Key).ToList().Count == 0;
        }
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCartRuleReason reason, MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart == null)
                return null;

            // if reason is CartCreated
            // set parameters
            if (reason == ShoppingCartRuleReason.CartCreated)
            {
                IPurchaseRestrictionManager manager =  GetPurchaseRestrictionManager(shoppingCart.DistributorID);
                SetPurchaseRestriction(manager.Tins, shoppingCart.OrderMonth, shoppingCart.DistributorID, manager);
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded || reason == ShoppingCartRuleReason.CartBeingPaid || reason == ShoppingCartRuleReason.CartClosed)
            {
                var result = new List<ShoppingCartRuleResult>();
                var defaultResult = new ShoppingCartRuleResult();
                defaultResult.RuleName = "PurchaseRestriction Rules";
                defaultResult.Result = RulesResult.Unknown;
                result.Add(PerformRules(shoppingCart, reason, defaultResult));
                return result;
            }
            return shoppingCart.RuleResults;
        }

        /// <summary>
        /// PerformRules -- process Volume restriction and product type restriction
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="reason"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        protected virtual ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            if (!GetPurchaseRestrictionManager(cart.DistributorID).CanPurchase)
            {
                cart.ItemsBeingAdded.Clear();
                Result.AddMessage(
                   string.Format(
                       HttpContext.GetGlobalResourceObject(
                           string.Format("{0}_ErrorMessage", HLConfigManager.Platform),"CantBuy").ToString()));
                Result.Result = RulesResult.Failure;
                return Result;
            }

            string extraerror = string.Empty;

            if (GetPurchaseRestrictionManager(cart.DistributorID).ExtendRestrictionErrorMessage)
            {
                extraerror = string.Format(
                       HttpContext.GetGlobalResourceObject(
                           string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "IndiaFSSAIMessage").ToString());
            }

            var currentlimits = GetCurrentPurchasingLimits(cart.DistributorID, GetCurrentOrderMonth());
            if (cart == null || currentlimits == null)
                return Result;

            if (cart.ItemsBeingAdded == null || cart.ItemsBeingAdded.Count == 0)
                return Result;

            string processingCountryCode = DistributorProfileModel.ProcessingCountryCode;

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
                                    "PurchaseLimitTypeProductCategory").ToString(), item.SKU) + extraerror);
                        continue;
                    }
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
                        Result.Result = RulesResult.Failure;
                        if (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP || currentlimits.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold)
                        //MPE FOP
                        {
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
                            //}
                        }
                        else
                        {
                            if (cart.ItemsBeingAdded.Exists(i => i.SKU == item.SKU))
                            {
                                if (HLConfigManager.Platform == "iKiosk")
                                {
                                    errors.Add(
                                            string.Format(
                                                HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                                    "VolumePointExceeds").ToString(), item.SKU));
                                }
                                else
                                {
                                    ///The quantity of the item SKU:{0} can not be increased by {1} because it exceeds your volume points limit.
                                    errors.Add(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "VolumePointExceedsByIncreasingQuantity").ToString(),
                                        item.SKU, item.Quantity) + extraerror);

                                }
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
                                                "FOPVolumePointExceedsOnCheckout").ToString(), 1100, PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(processingCountryCode), PurchaseRestrictionProvider.GetThresholdPeriod(processingCountryCode), (cartVolume + NewVolumePoints) - currentlimits.RemainingVolume) + extraerror);
                }
                else
                {
                    errors = errors.Select(x => x.Replace("-999", ((cartVolume + NewVolumePoints) - currentlimits.RemainingVolume).ToString())).ToList<string>();
                    Array.ForEach(errors.ToArray(), a => Result.AddMessage(a));
                }
            }
            else
            {
                Result.Result = RulesResult.Success;
            }
            cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));

            return Result;
        }

        #endregion

        public virtual bool CanPurchase(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            return true;
        }

        public virtual bool CanPurchasePType(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            return true;
        }

        public virtual bool CanPurchasePType(string distributorId)
        {
            return GetPurchaseRestrictionManager(distributorId).CanPurchasePType;
        }

        public Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId)
        {
            // get purchasing limits
            return GetPurchaseRestrictionManager(distributorId).PurchasingLimits;
        }

        public PurchasingLimits_V01 GetCurrentPurchasingLimits(string distributorId, int orderMonth)
        {
            return GetPurchasingLimits(distributorId)[orderMonth];
        }

        public IPurchaseRestrictionManager GetPurchaseRestrictionManager(string distributorId)
        {

            return (new PurchaseRestrictionManagerFactory()).GetPurchaseRestrictionManager(distributorId);
        }

        public PurchasingLimits_V01 GetLimits(LimitsRestrictionType type, int orderMonth, IPurchaseRestrictionManager manager)
        {
            if (manager.ApplicableLimits != null)
            {
                foreach (var l in manager.ApplicableLimits)
                {
                    var v1 = l as PurchasingLimits_V01;
                    if (v1.LimitsRestrictionType == type && v1.Month == int.Parse(orderMonth.ToString().Substring(4, 2)))
                    {
                        return v1;
                    }
                }
            }
            return null;
        }

        public virtual void SetLimits(int orderMonth, IPurchaseRestrictionManager manager, PurchasingLimits_V01 limits)
        {
            if (manager.PurchasingLimits == null)
                manager.PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();

            var FOPlimits = GetFOPLimits(manager);
            if (FOPlimits != null && FOPlimits.maxVolumeLimit != -1 && !FOPlimits.Completed)
            {
                if (limits.PurchaseLimitType == PurchaseLimitType.Volume)
                {
                    if (limits.maxVolumeLimit < FOPlimits.maxVolumeLimit && limits.maxVolumeLimit > -1)
                        manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(limits);
                    else
                        manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(FOPlimits);
                }
                else
                    manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(FOPlimits);
            }
            else
                manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(limits);

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

        public PurchasingLimits_V01 GetFOPLimits(IPurchaseRestrictionManager manager)
        {
            if (manager.ApplicableLimits != null)
            {
                foreach (var l in manager.ApplicableLimits)
                {
                    var v1 = l as PurchasingLimits_V01;
                    if (v1.LimitsRestrictionType == LimitsRestrictionType.FOP)
                    {
                        return v1;
                    }
                }
            }
            return null;
        }

        public int GetCurrentOrderMonth()
        {
            return 200000 + OrderMonth.GetCurrentOrderMonth();
        }

        public PurchasingLimits_V01 CopyPurchasingLimits(PurchasingLimits_V01 source)
        {
            PurchasingLimits_V01 target = new PurchasingLimits_V01();
            target.Completed = source.Completed;
            target.Expiry = source.Expiry;
            target.FirstOrderDate = source.FirstOrderDate;
            target.Items = source.Items;
            target.LastRead = source.LastRead;
            target.LimitsRestrictionType = source.LimitsRestrictionType;
            target.MaxEarningsLimit = source.MaxEarningsLimit;
            target.maxVolumeLimit = source.maxVolumeLimit;
            target.Month = source.Month;
            target.NextEarningsLimit = source.NextEarningsLimit;
            target.NextMaxEarningsLimit = source.NextMaxEarningsLimit;
            target.NextMaxVolumeLimit = source.NextMaxVolumeLimit;
            target.NextVolumeLimit = source.NextVolumeLimit;
            target.OutstandingOrders = source.OutstandingOrders;
            target.PurchaseLimitType = source.PurchaseLimitType;
            target.PurchaseSubType = source.PurchaseSubType;
            target.PurchaseType = source.PurchaseType;
            target.RemainingEarnings = source.RemainingEarnings;
            target.RemainingVolume = source.RemainingVolume;
            target.RestrictionPeriod = source.RestrictionPeriod;
            target.ThresholdPeriod = source.ThresholdPeriod;
            target.Year = source.Year;
            return target;
        }
    }
}
