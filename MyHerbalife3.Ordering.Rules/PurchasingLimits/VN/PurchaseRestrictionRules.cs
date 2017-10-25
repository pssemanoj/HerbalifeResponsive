using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using HL.Common.Logging;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using HLConfigManager = MyHerbalife3.Ordering.Configuration.ConfigurationManagement.HLConfigManager;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.VN
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {

            //check if is a elearning and set the new limits for the second order 
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
            var currentLimits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.CheckELearning)
            {
                PurchaseLimitType limitType = PurchaseLimitType.Volume;
                currentLimits.PurchaseLimitType = limitType;
                var session = SessionInfo.GetSessionInfo(distributorId, Locale);
                if (PurchaseRestrictionProvider.RequireTraining(distributorId, this.Locale, this.Country))
                {
                    if (currentLimits.RemainingVolume != currentLimits.maxVolumeLimit && !session.LimitsHasModified)
                    {
                        var used = (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);
                        var shouldbe = 1100 - used;
                        currentLimits.PurchaseLimitType = limitType;
                        if (currentLimits.RemainingVolume != shouldbe && shouldbe > 0 && !session.LimitsHasModified)
                        {
                            currentLimits.RemainingVolume = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV - (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);
                            session.LimitsHasModified = true;
                        }
                        
                    }
                    //check for the members without limits and create
                    else if (currentLimits.RemainingVolume == -1 && currentLimits.maxVolumeLimit == -1 && !session.LimitsHasModified)
                    {
                        var currentLoggedInCounrtyCode = Locale.Substring(3);
                        DistributorLoader distributorLoader = new DistributorLoader();
                        var distributorProfile = distributorLoader.Load(distributorId, currentLoggedInCounrtyCode);

                        var remainingVolume = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV - distributorProfile.PersonallyPurchasedVolume;
                        //PurchasingLimits_V01 newLimits = new PurchasingLimits_V01();
                        currentLimits.Month = currentLimits.Month;
                        currentLimits.LastRead = DateTime.UtcNow;
                        currentLimits.RemainingVolume = distributorProfile.PersonallyPurchasedVolume != 0 ? remainingVolume : 0;
                        currentLimits.LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits;
                        currentLimits.PurchaseLimitType = limitType;
                        currentLimits.LastRead = DateTime.UtcNow;
                        currentLimits.maxVolumeLimit = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV;
                        session.LimitsHasModified = true;
                    }
                }
            }
            else
            {
                base.SetPurchaseRestriction(tins,orderMonth,distributorId,manager);
            }

            if (currentLimits == null)
            {
                return;
            }
            SetLimits(orderMonth, manager, currentLimits);
            
        }

        public override bool CanPurchase(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            bool canPurchase = false;
            var countryCodes = new List<string> { CountryType.VN.Key };
            countryCodes.AddRange(CountryType.VN.HmsCountryCodes);
            if (countryCodes.Contains(CountryOfProcessing))
            {
                canPurchase = tins != null && tins.Find(p => p.IDType.Key == "VNID") != null;
            }
            return canPurchase;
        }

        public override void SetLimits(int orderMonth, IPurchaseRestrictionManager manager, PurchasingLimits_V01 limits)
        {
            if (manager.PurchasingLimits == null)
                manager.PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();

            var FOPlimits = GetFOPLimits(manager);
            if (FOPlimits != null && FOPlimits.maxVolumeLimit != -1 && !FOPlimits.Completed)
            {
                if (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits && limits.maxVolumeLimit != -1)
                    manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(limits);
                else
                    manager.PurchasingLimits[orderMonth] = CopyPurchasingLimits(FOPlimits);
            }
            else
            {
                if (limits.maxVolumeLimit == -1)
                    limits.PurchaseLimitType = PurchaseLimitType.None;
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

        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.CheckELearning)
            {
                switch (reason)
                {
                    case ShoppingCartRuleReason.CartItemsBeingAdded:
                        if (!CanBuy_eLearningRule(cart as MyHLShoppingCart))
                        {
                            cart.CurrentItems.Clear();
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(PlatformResources.GetGlobalResourceString("ErrorMessage", "RequiredELearning"));
                            cart.RuleResults.Add(Result);

                            return Result;
                        }
                        break;
                    case ShoppingCartRuleReason.CartRetrieved:
                        break;
                }
            }
            return base.PerformRules(cart, reason, Result);
        }

        private bool CanBuy_eLearningRule(MyHLShoppingCart hlCart)
        {
            bool retVal = true;
            if ( PurchaseRestrictionProvider.RequireTraining(hlCart.DistributorID, hlCart.Locale,hlCart.CountryCode) )
            {
                //var currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(hlCart.DistributorID);
                var currentLimits = GetCurrentPurchasingLimits(hlCart.DistributorID, GetCurrentOrderMonth());

                decimal currentVolumePoints = hlCart.VolumeInCart;
                if (hlCart.ItemsBeingAdded != null && hlCart.ItemsBeingAdded.Any())
                {
                    foreach (var i in hlCart.ItemsBeingAdded)
                    {
                        var currentItem = CatalogProvider.GetCatalogItem(i.SKU, Country);
                        currentVolumePoints += currentItem.VolumePoints * i.Quantity;
                    }
                }
                
                currentVolumePoints += (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);

                if (currentVolumePoints > HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV)
                {
                    if (hlCart.ItemsBeingAdded != null)
                        hlCart.ItemsBeingAdded.Clear();
                    retVal = false;
                }
            }
            return retVal;
        }

    }
}
