using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.fr_FR
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private readonly List<string> ExemptSubTypes = new List<string>(new[] {"E", "N", "NA", ""});
        private readonly List<string> ResaleSubTypes = new List<string>(new[] {"RE"});
        private readonly List<string> RetailSubTypes = new List<string>(new[] {"PC"});

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];

            if (TIN.Equals("ETO"))
            {
                TIN = "E";
            }

            theLimits.PurchaseSubType = TIN;

            if (RetailSubTypes.Contains(theLimits.PurchaseSubType))
            {
                theLimits.PurchaseType = OrderPurchaseType.PersonalConsumption;
                theLimits.PurchaseLimitType = PurchaseLimitType.DiscountedRetail;
            }
            else if (ResaleSubTypes.Contains(theLimits.PurchaseSubType))
            {
                theLimits.PurchaseType = OrderPurchaseType.Consignment;
                theLimits.PurchaseLimitType = PurchaseLimitType.None;
            }
            else
            {
                if (PurchasingLimitManager(distributorId).PurchasingLimitsRestriction !=
                    PurchasingLimitRestrictionType.MarketingPlan)
                {
                    theLimits.PurchaseLimitType = PurchaseLimitType.None;

                    if (!ExemptSubTypes.Contains(theLimits.PurchaseSubType))
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "Unknown Distributor Subtype of \"{0}\" encountered in PurchasingLimitRules for fr-FR, Distributor: {1}",
                                theLimits.PurchaseSubType, distributorId));
                    }
                }
            }

            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            //French SS imposed PC limits
            var profile = DistributorOrderingProfileProvider.GetProfile(distributorId, Country);
            if (profile != null)
                return profile.OrderSubType != "F";

            return isExempt;
        }
    }
}