using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.vi_VN
{
    public class PurchasingLimitRules_eLearning : Global.PurchasingLimitRules, IShoppingCartRule, IPurchasingLimitsRule
    {
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingLimits Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
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
                        checkLimits(cart as MyHLShoppingCart);
                        break;
                }
            }

            return base.PerformRules(cart, reason, Result);
        }

        private bool CanBuy_eLearningRule(MyHLShoppingCart hlCart)
        {
            bool retVal = true;
            var session = SessionInfo.GetSessionInfo(hlCart.DistributorID, hlCart.Locale);
            string trainingCode = HLConfigManager.Configurations.ShoppingCartConfiguration.TrainingCode;

            if (session.DsTrainings == null)
                session.DsTrainings = DistributorOrderingProfileProvider.GetTrainingList(hlCart.DistributorID, hlCart.CountryCode);

            if (session.DsTrainings != null && session.DsTrainings.Count > 0 && session.DsTrainings.Exists(t => t.TrainingCode == trainingCode && !t.TrainingFlag))
            {
                var currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(hlCart.DistributorID);
                decimal currentVolumePoints = hlCart.VolumeInCart;

                var currentItem = CatalogProvider.GetCatalogItem(hlCart.CurrentItems[0].SKU, Country);
                currentVolumePoints += currentItem.VolumePoints * hlCart.CurrentItems[0].Quantity;
                if (currentLimits.PurchaseLimitType == PurchaseLimitType.Volume)
                {
                    currentVolumePoints += (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);
                }

                if (currentVolumePoints > HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV)
                    retVal = false;
            }


            return retVal;
        }
        
        private void checkLimits(MyHLShoppingCart hlCart)
        {
            var currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(hlCart.DistributorID);

            var session = SessionInfo.GetSessionInfo(hlCart.DistributorID, hlCart.Locale);
            string trainingCode = HLConfigManager.Configurations.ShoppingCartConfiguration.TrainingCode;

            if (session.DsTrainings == null || session.DsTrainings.Count == 0)
                session.DsTrainings = DistributorOrderingProfileProvider.GetTrainingList(hlCart.DistributorID, hlCart.CountryCode);
            if (session.DsTrainings != null && session.DsTrainings.Count > 0 &&
                session.DsTrainings.Exists(t => t.TrainingCode == trainingCode && !t.TrainingFlag))
            {
                //if is true the member place an order without taking the training and the limits shoud be applicable for only 1100
                if (currentLimits.RemainingVolume != currentLimits.maxVolumeLimit && !session.LimitsHasModified)
                {
                    var used = (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);
                    var shouldbe = 1100 - used;
                    PurchaseLimitType limitType = PurchaseLimitType.Volume;
                    currentLimits.PurchaseLimitType = limitType;
                    if (currentLimits.RemainingVolume != shouldbe && shouldbe > 0 && !session.LimitsHasModified)
                    {
                        currentLimits.RemainingVolume = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV - (currentLimits.maxVolumeLimit - currentLimits.RemainingVolume);
                        session.LimitsHasModified = true;
                    }
                    string country = DistributorProfileModel.ProcessingCountryCode;
                    PurchasingLimitProvider.savePurchasingLimitsToCache(ConvertCurrentLimitsToSave(currentLimits), hlCart.DistributorID);
                    PurchasingLimitProvider.SavePurchaseLimitsToStore(country, hlCart.DistributorID);
                }
                //check for the members without limits and create
                else if (currentLimits.RemainingVolume == -1 && currentLimits.maxVolumeLimit == -1 && !session.LimitsHasModified)
                {
                    var currentLoggedInCounrtyCode = Locale.Substring(3);
                    DistributorLoader distributorLoader = new DistributorLoader();
                    var distributorProfile = distributorLoader.Load(hlCart.DistributorID, currentLoggedInCounrtyCode);

                    var remainingVolume = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV - distributorProfile.PersonallyPurchasedVolume;
                    PurchaseLimitType limitType = PurchaseLimitType.Volume;
                    //PurchasingLimits_V01 newLimits = new PurchasingLimits_V01();
                    currentLimits.Month = currentLimits.Month;
                    currentLimits.LastRead = DateTime.UtcNow;
                    currentLimits.RemainingVolume = distributorProfile.PersonallyPurchasedVolume != 0 ? remainingVolume : 0;
                    currentLimits.LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits;
                    currentLimits.PurchaseLimitType = limitType;
                    currentLimits.LastRead = DateTime.UtcNow;
                    currentLimits.maxVolumeLimit = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV;
                    session.LimitsHasModified = true;
                    string country = DistributorProfileModel.ProcessingCountryCode;
                    PurchasingLimitProvider.savePurchasingLimitsToCache(ConvertCurrentLimitsToSave(currentLimits), hlCart.DistributorID);
                    PurchasingLimitProvider.SavePurchaseLimitsToStore(country, hlCart.DistributorID);

                }
                
            }
        }

        private Dictionary<int, PurchasingLimits_V01> ConvertCurrentLimitsToSave(PurchasingLimits_V01 currentLimits)
        {
            var result = new Dictionary<int, PurchasingLimits_V01>();
            int ordermonth = GetOrderMonth();
            result.Add(ordermonth,currentLimits);
            return result;
        }

        
    }
}
