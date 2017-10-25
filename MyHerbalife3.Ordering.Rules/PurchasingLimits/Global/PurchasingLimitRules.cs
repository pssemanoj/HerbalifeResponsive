using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.Global
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            if (DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                return null;
            }

            //Fetch these records from Web Service
            string country = DistributorProfileModel.ProcessingCountryCode;

            var purchasingLimitManager = PurchasingLimitManager(distributorId);
            purchasingLimitManager.SetPurchasingLimits(PurchasingLimitProvider.GetOrderMonth());
            PurchasingLimits_V01 currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
                //Get the current Limits if the exist
            PurchasingLimits_V01 storedLimits = PurchasingLimitProvider.GetPurchasingLimitsFromStore(Country,
                                                                                                     distributorId);
                //Get the saved limits for the DS and country

            PurchasingLimits_V01 theLimits = null;
            if (null != storedLimits && storedLimits.Id > 0) //Decide if we use the stored limits
            {
                storedLimits.MaxEarningsLimit = purchasingLimitManager.MaxEarningsLimit;
                storedLimits.maxVolumeLimit = purchasingLimitManager.MaxPersonalConsumptionLimit;

                if (IsBlackoutPeriod() || storedLimits.OutstandingOrders > 0 ||
                    PurchasingLimitProvider.GetDistributorPurchasingLimitsSource(Country, distributorId) ==
                    DistributorPurchasingLimitsSourceType.InternetOrdering)
                {
                    theLimits = storedLimits;
                    PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId);
                }
                else
                {
                    theLimits = currentLimits;
                }
            }
            else
            {
                theLimits = currentLimits;
            }

            if (null == theLimits) //We're bare and need the DS
            {
                theLimits = new PurchasingLimits_V01();
                var limit = purchasingLimitManager.ReloadPurchasingLimits(PurchasingLimitProvider.GetOrderMonth());
                if (null != currentLimits) //if We're already init'ed resolve against current refreshed DS
                {
                    if (currentLimits.RemainingVolume > purchasingLimitManager.RemainingPersonalConsumptionLimit)
                        currentLimits.RemainingVolume = purchasingLimitManager.RemainingPersonalConsumptionLimit;
                    if (currentLimits.RemainingEarnings > purchasingLimitManager.RemainingEarningsLimit)
                        currentLimits.RemainingEarnings = purchasingLimitManager.RemainingEarningsLimit;
                    currentLimits.MaxEarningsLimit = purchasingLimitManager.MaxEarningsLimit;
                    currentLimits.maxVolumeLimit = purchasingLimitManager.MaxPersonalConsumptionLimit;
                    theLimits = currentLimits;
                    theLimits.LastRead = DateTime.UtcNow;
                }
                else
                {
                    //Probably first time in - refresh from DS.
                    theLimits.RemainingVolume = purchasingLimitManager.RemainingPersonalConsumptionLimit;
                    theLimits.RemainingEarnings = purchasingLimitManager.RemainingEarningsLimit;
                    theLimits.MaxEarningsLimit = purchasingLimitManager.MaxEarningsLimit;
                    theLimits.maxVolumeLimit = purchasingLimitManager.MaxPersonalConsumptionLimit;
                    theLimits.LastRead = DateTime.UtcNow;
                }
            }

            if (null == storedLimits)
            {
                PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId, country, true);
            }
            else
            {
                PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId, PurchasingLimitProvider.GetOrderMonth());
            }
            var theCurrentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
            var limitsType = PurchaseLimitType.Volume;

            if (theCurrentLimits.maxVolumeLimit < 0)
            {
                limitsType = PurchaseLimitType.None;
            }

            purchasingLimitManager.PurchasingLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);

            return purchasingLimitManager.PurchasingLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            return !PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId);
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                               ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
            {
                return Result;
            }
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                GetPurchasingLimits(cart.DistributorID, string.Empty);
            }

            return base.PerformRules(cart, reason, Result);
        }
    }
}