using System.Collections.Generic;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_CO
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }
            List<string> codes = new List<string>(CountryType.CO.HmsCountryCodes);
            return (codes.Contains(DistributorProfileModel.ProcessingCountryCode));
        }
    }
}