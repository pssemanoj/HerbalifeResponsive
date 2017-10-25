using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using HL.Common.Utilities;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.ru_KZ
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {

            List<string> codes = new List<string>(CountryType.KZ.HmsCountryCodes);
            codes.Add(CountryType.KZ.Key);
            bool isCOPKZ = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
            if (!isCOPKZ)
                return true;

            bool isExempt = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);

            var now = DateUtils.GetCurrentLocalTime("KZ");
            int numberTins = 0;
            foreach (TaxIdentification taxId in tins)
            {
                if (taxId.IDType != null && ((taxId.IDType.Key == "KZTX" || taxId.IDType.Key == "KZBL") && taxId.IDType.ExpirationDate > now))
                {
                    numberTins++;

                    if (numberTins == 2)    //DS should have both tins and active
                    {
                        isExempt = true;
                        break;
                    }
                }
            }

            return isExempt;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var currentLimits = base.GetPurchasingLimits(cart.DistributorID, string.Empty);
                var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
                // If it's threshold volume point for all product types is counted
                if (PurchasingLimitProvider.IsOrderThresholdMaxVolume(theLimits))
                {
                    Result = base.PerformRules(cart, reason, Result);
                }
                else if (!DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
                {
                    CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                    Result = base.PerformRules(cart, reason, Result);
                }
            }

            if (reason == ShoppingCartRuleReason.CartCreated && cart.CartItems.Any())
            {
                var currentLimits = base.GetPurchasingLimits(cart.DistributorID, string.Empty);
                var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
                // If it's threshold volume point for all product types is counted
                if (PurchasingLimitProvider.IsOrderThresholdMaxVolume(theLimits))
                {
                    Result = base.PerformRules(cart, reason, Result);
                }
                else if (!DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
                {
                    var purchasingLimitManager = PurchasingLimitManager(cart.DistributorID);
                    decimal DistributorRemainingVolumePoints = 0;
                    decimal DistributorRemainingDiscountedRetail = 0;
                    var myhlCart = cart as MyHLShoppingCart;
                    if (null == myhlCart)
                    {
                        LoggerHelper.Error(
                            string.Format("{0} myhlCart is null {1}", Locale, cart.DistributorID));
                        Result.Result = RulesResult.Failure;
                        return Result;
                    }

                    PurchasingLimits_V01 PurchasingLimits =
                        PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);

                    purchasingLimitManager.SetPurchasingLimits(PurchasingLimits);
                    
                    if (null == PurchasingLimits)
                    {
                        LoggerHelper.Error(
                            string.Format("{0} PurchasingLimits could not be retrieved for distributor {1}", Locale,
                                          cart.DistributorID));
                        Result.Result = RulesResult.Failure;
                        return Result;
                    }

                    DistributorRemainingVolumePoints =
                        DistributorRemainingDiscountedRetail = PurchasingLimits.RemainingVolume;

                    if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.Volume)
                    {
                        if (PurchasingLimits.maxVolumeLimit == -1)
                        {
                            return Result;
                        }
                        decimal cartVolume = (cart as MyHLShoppingCart).VolumeInCart;
                        if (DistributorRemainingVolumePoints - cartVolume < 0)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                          "NoCheckoutPurchaseLimitsExceeded"));
                            cart.RuleResults.Add(Result);
                        }
                    }
                    else
                    {
                        Result.Result = RulesResult.Success;
                    }
                }
                else
                {
                    Result = base.PerformRules(cart, reason, Result);
                }

            }
            return Result;    
        }
    }
}
