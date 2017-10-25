using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.es_AR
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            List<TaxIdentification> dsTins = DistributorOrderingProfileProvider.GetTinList(distributorID, true); //HMS made up the country code
            var countryCodes = new List<string>(CountryType.AR.HmsCountryCodes);
            countryCodes.Add(CountryType.AR.Key);
            bool arCOP = countryCodes.Contains(DistributorProfileModel.ProcessingCountryCode);

            if (arCOP && (dsTins == null || dsTins.Count == 0))
            {
                return false;
            }

            if (dsTins != null && dsTins.Any())
            {
                List<string> arTinCodes = CountryTinCodeProvider.GetValidTinCodesForCountry(CountryType.AR.Key);
                var r = from a in arTinCodes
                        from b in dsTins
                        where a == b.IDType.Key
                        select a;
                //Must have a TinCode to purchase if AR member
                if (arCOP && (r.Count() == 0))
                    return false;

                bool hasARTX = (dsTins.Find(t => t.IDType.Key == "ARTX") != null);
                bool hasARVT = (dsTins.Find(t => t.IDType.Key == "ARVT") != null);
                bool hasCUIT = (dsTins.Find(t => t.IDType.Key == "CUIT") != null);
                bool hasARID = (dsTins.Find(t => t.IDType.Key == "ARID") != null);

                if ((hasARTX && hasARVT) || (hasARTX && hasARID && !hasCUIT) || (hasARVT && hasARID && !hasCUIT) || (hasARID && hasCUIT && !hasARTX && !hasARVT))
                {
                    return false;
                }
            }
            return true;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            return Result;
        }
    }
}