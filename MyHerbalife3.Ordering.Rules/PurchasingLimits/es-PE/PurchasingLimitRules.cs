using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_PE
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

            //Must have PETX TinCode to don't have volume limited
            //CR-GDO-PE-64 - Must have either PEID or PETX to purchase            
            
            List<string> codes = new List<string>(CountryType.PE.HmsCountryCodes);
            codes.Add(CountryType.PE.Key);
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                return tins.Find(p => p.IDType.Key == "PETX") != null;
            }
            else
            {
                var required =
                    (from t in tins
                     from r in new List<string>(new string[] {"PEID", "PETX"})
                     where t.IDType.Key == r
                     select t).ToList();
                return (null != required && required.Count == 2);
            }
        }
    }
}