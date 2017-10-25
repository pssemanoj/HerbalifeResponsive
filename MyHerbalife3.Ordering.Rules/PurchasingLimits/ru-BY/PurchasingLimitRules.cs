using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.ru_BY
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {        
        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        { 
            // List<string> codes = new List<string>(CountryType.BY.HmsCountryCodes);
            //codes.Add(CountryType.BY.Key);
            //bool isCOPBY = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
            //if (!isCOPBY)
            //    return true;
            //List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            //bool isExempt = tins != null && 
            //                tins.Find(p => p.IDType.Key == "BYTX") != null && 
            //                tins.Find(p => p.IDType.Key == "BYBL") != null && 
            //                tins.Find(p => p.IDType.Key == "BYID") != null && 
            //                tins.Find(p => p.IDType.Key == "BYNA") != null;
            //return isExempt;
            return true;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            //if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            //{
            //    var currentLimits = base.GetPurchasingLimits(cart.DistributorID, string.Empty);
            //    var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
            //    // If it's threshold volume point for all product types is counted
            //    if (PurchasingLimitProvider.IsOrderThresholdMaxVolume(theLimits))
            //    {
            //        Result = base.PerformRules(cart, reason, Result);
            //    }
            //    else if (!DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
            //    {
            //        CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
            //        Result = base.PerformRules(cart, reason, Result);
            //    }
            //}

            return Result;
        }

    }
}
