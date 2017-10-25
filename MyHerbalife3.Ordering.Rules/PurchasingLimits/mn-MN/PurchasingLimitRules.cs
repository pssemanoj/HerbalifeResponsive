using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.mn_MN
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {

            //bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            //if (!isExempt)
            //{
            //    return false;
            //}

            List<string> codes = new List<string>(CountryType.MN.HmsCountryCodes);
            codes.Add(CountryType.MN.Key);
            bool isCOPMN = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
            if (!isCOPMN)
                return false;

            bool isExempt = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);

            var now = DateUtils.GetCurrentLocalTime("MN");            
            foreach (TaxIdentification taxId in tins)
            {
                if (taxId.IDType != null && (taxId.IDType.Key == "MGTX" && taxId.IDType.ExpirationDate > now))
                {
                    isExempt = true;
                    break;                 
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

            return Result;
        }
    }
}
