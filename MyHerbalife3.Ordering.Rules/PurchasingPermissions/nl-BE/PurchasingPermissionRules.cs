using System.Collections.Generic;
using System.Linq;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.nl_BE
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = true;
            List<string> codes = new List<string>(CountryType.BE.HmsCountryCodes);
            codes.Add(CountryType.BE.Key);

            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
                if (tins.Count > 0)
                {
                    var beTins = (from t in tins
                                  where t.IDType.Key == "BEDI" || t.IDType.Key == "BENO" || t.IDType.Key == "BEVT"
                                  select t.IDType.Key);
                    if (beTins.Contains("BEDI") && (beTins.Contains("BENO") || beTins.Contains("BEVT")))
                    {
                        canPurchase = false;
                    }
                }
            }
            return canPurchase;
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
            //Nothing defined yet
            return Result;
        }
    }
}