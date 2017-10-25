using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.pt_PT
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true); //HMS made up the country code
            //Must have POTX TinCode to purchase
            foreach (TaxIdentification t in tins)
            {
                if (t.IDType.Key == "POTX")
                {
                    canPurchase = true;
                }
            }

            if (!canPurchase)
            {
                //If they don't have the TinCode, they can only purchase if their mailing address is not in Portugal.
                var mailingAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, distributorID, countryCode);
                if (null != mailingAddress)
                {
                    if (!string.IsNullOrEmpty(mailingAddress.Country) && mailingAddress.Country != "PT")
                    {
                        canPurchase = true;
                    }
                }
            }
            return canPurchase;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
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