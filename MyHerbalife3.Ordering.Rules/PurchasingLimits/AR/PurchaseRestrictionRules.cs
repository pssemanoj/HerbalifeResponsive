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
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.AR
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        private const decimal MaxAmount = 999.99m;
        public override bool CanPurchase(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            var countryCodes = new List<string>(CountryType.AR.HmsCountryCodes);
            countryCodes.Add(CountryType.AR.Key);
            bool arCOP = countryCodes.Contains(DistributorProfileModel.ProcessingCountryCode);

            if (arCOP && (tins == null || tins.Count == 0))
            {
                return false;
            }

            if (tins != null && tins.Any())
            {
                List<string> arTinCodes = CountryTinCodeProvider.GetValidTinCodesForCountry(CountryType.AR.Key);
                var r = from a in arTinCodes
                        from b in tins
                        where a == b.IDType.Key
                        select a;
                //Must have a TinCode to purchase if AR member
                if (arCOP && (r.Count() == 0))
                    return false;

                bool hasARTX = (tins.Find(t => t.IDType.Key == "ARTX") != null);
                bool hasARVT = (tins.Find(t => t.IDType.Key == "ARVT") != null);
                bool hasCUIT = (tins.Find(t => t.IDType.Key == "CUIT") != null);
                bool hasARID = (tins.Find(t => t.IDType.Key == "ARID") != null);

                if ((hasARTX && hasARVT) || (hasARTX && hasARID && !hasCUIT) || (hasARVT && hasARID && !hasCUIT) || (hasARID && hasCUIT && !hasARTX && !hasARVT))
                {
                    return false;
                }
            }
            return true;
        }
        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            PurchaseLimitType limitType = PurchaseLimitType.Volume;
            PurchasingLimitRestrictionPeriod period = PurchasingLimitRestrictionPeriod.Unknown;

            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);

            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits,orderMonth,manager);
            if (limits == null)
                return;

            var processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
            var arCOP = (new List<string>(CountryType.AR.HmsCountryCodes)).Union(new List<string>() { CountryType.AR.Key }).Contains(processingCountryCode);
            bool isExempted = CatalogProvider.IsDistributorExempted(distributorId);
            var nonVolumeOrderingCountries = new List<string> { "BR", "UY", "BO", "PY" };
            //Members with any Sub types whose country of processing is not equal to Brazil/ Bolivia/ Paraguay/ Uruguay, 
            //If added to white list then respective Members can place ‘P’ type orders with a purchasing limit of 999.99 pesos/order.
            if (nonVolumeOrderingCountries.Contains(processingCountryCode))
            // equal to BR,UY,BO,PY
            {
                if (!isExempted) // not in whitelist
                {
                    manager.CanPurchasePType = false;
                }
            }
            var FOPlimits = GetFOPLimits(manager);
            if (FOPlimits != null && !FOPlimits.Completed)
            {
                limitType = PurchaseLimitType.Volume;
                period = PurchasingLimitRestrictionPeriod.OneTime;
            }
            else
            {
                if (!arCOP) // 
                {
                    limitType = PurchaseLimitType.TotalPaid;
                    period = PurchasingLimitRestrictionPeriod.PerOrder;
                    limits.RemainingVolume = limits.maxVolumeLimit = MaxAmount;
                }
                else
                {
                    limitType = PurchaseLimitType.Volume;
                    period = PurchasingLimitRestrictionPeriod.Monthly;
                }

            }
            limits.PurchaseLimitType = limitType;
            limits.RestrictionPeriod = period;

            if (limits.maxVolumeLimit == -1)
            {
                limits.RemainingVolume = -1;
                limits.PurchaseLimitType = PurchaseLimitType.None;
            }
            SetLimits(orderMonth, manager, limits);
        }
        public static bool checkVolumeLimits(MyHLShoppingCart cart, ref decimal previousVolumePoints, ShoppingCartRuleResult Result, PurchasingLimits_V01 currentLimits, string Locale, string Country, ShoppingCartItem_V01 currentItem)
        {
            decimal DistributorRemainingVolumePoints = 0;
            decimal NewVolumePoints = 0;

            if (currentLimits.PurchaseLimitType == PurchaseLimitType.None || currentLimits.LimitsRestrictionType != LimitsRestrictionType.PurchasingLimits)
            {
                return true;
            }
            if (null == currentLimits)
            {
                return false;
            }

            DistributorRemainingVolumePoints = currentLimits.RemainingVolume;

            var current = CatalogProvider.GetCatalogItem(currentItem.SKU, Country);
            if (current != null)
            {
                NewVolumePoints = current.VolumePoints * currentItem.Quantity;
            }

            if (NewVolumePoints > 0)
            {
                if (currentLimits.maxVolumeLimit == -1)
                {
                    return true;
                }
                decimal cartVolume = cart.VolumeInCart + previousVolumePoints;
                previousVolumePoints += NewVolumePoints;

                if (DistributorRemainingVolumePoints - (cartVolume + NewVolumePoints) < 0)
                {
                    Result.Result = RulesResult.Failure;
                    var orderMonth = new OrderMonth(Country);
                    var msg = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform) ?? string.Empty, "VolumePointExceedsOnOrderMonth") as string;
                    msg = string.Format(msg, orderMonth.CurrentOrderMonth.ToString("MM-yyyy"), DistributorRemainingVolumePoints);
                    Result.AddMessage(msg);
                    cart.RuleResults.Add(Result);
                    return false;
                }
            }
            return true;
        }

        private bool checkPerOrderLimit(MyHLShoppingCart cart, List<ShoppingCartItem_V01> previousItems, ShoppingCartRuleResult Result, string sku, int qty)
        {
            decimal MaxAmount = 999.99m;

            var calcTheseItems = new List<ShoppingCartItem_V01>();
            calcTheseItems.AddRange(from i in cart.CartItems
                                    where !APFDueProvider.IsAPFSku(i.SKU)
                                    select
                                        new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                 i.MinQuantity));
            calcTheseItems.AddRange(previousItems);

            var existingItem =
                        calcTheseItems.Find(ci => ci.SKU == sku);
            if (null != existingItem)
            {
                existingItem.Quantity += qty;
            }
            else
            {
                calcTheseItems.Add(new ShoppingCartItem_V01(0, sku, qty, DateTime.Now));
            }

            var Totals = cart.Calculate(calcTheseItems, false) as OrderTotals_V01;

            if (Totals != null && Totals.AmountDue  > MaxAmount)
            {
                var globalResourceObject =
                    HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_Rules", HLConfigManager.Platform), "AmountLimitExceeds");
                if (globalResourceObject != null)
                    Result.AddMessage(
                        string.Format(
                            globalResourceObject
                                .ToString(), MaxAmount.ToString()));
                Result.Result = RulesResult.Failure;
                return false;
            }
            return true;

        }
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Success;

            if (cart == null)
                return Result;

            if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
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

                bool bCheckPerOrder = false;
                var processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
                var arCOP = (new List<string>(CountryType.AR.HmsCountryCodes)).Union(new List<string>() { CountryType.AR.Key }).Contains(processingCountryCode);
                if (!arCOP) // 
                {
                    bCheckPerOrder = true;
                }

                decimal previousVolumePoints = decimal.Zero;

                List<string> skuToAdd = new List<string>();
                var previousItems = new List<ShoppingCartItem_V01>();

                foreach (var item in cart.ItemsBeingAdded)
                {
                    IPurchaseRestrictionManager manager = GetPurchaseRestrictionManager(cart.DistributorID);
                    if (!manager.CanPurchasePType)
                    {
                        var currentItem = CatalogProvider.GetCatalogItem(item.SKU, Country);
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
                                        globalResourceObject.ToString(), item.SKU));
                            cart.RuleResults.Add(Result);
                            continue;
                        }
                    }
                    if (bCheckPerOrder)
                    {
                        if (checkPerOrderLimit(cart, previousItems, Result, item.SKU, item.Quantity))
                        {
                            skuToAdd.Add(item.SKU);
                            previousItems.Add(item);
                        }
                    }
                    else
                    {
                        if (checkVolumeLimits(cart, ref previousVolumePoints, Result, currentlimits, this.Locale, this.Country, item))
                            skuToAdd.Add(item.SKU);
                    }
                }
                if (Result.Result != RulesResult.Failure)
                {
                    if (currentlimits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                        base.PerformRules(cart, reason, Result); // check FOP
                }
                cart.ItemsBeingAdded.RemoveAll(s => !skuToAdd.Contains(s.SKU));
            }
            return Result;
        }
    }
}
