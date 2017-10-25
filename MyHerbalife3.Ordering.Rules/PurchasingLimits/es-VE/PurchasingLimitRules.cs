using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_VE
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        //public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        //{
        //    Dictionary<int, PurchasingLimits_V01> currentLimits = base.GetPurchasingLimits(distributorId, TIN);

        //    if (DistributorProfileModel.ProcessingCountryCode == "CO" || DistributorProfileModel.ProcessingCountryCode == "BR")
        //    {
        //        // not to display remaining vp
        //        currentLimits.Values.AsQueryable()
        //                     .ToList()
        //                     .ForEach(pl => pl.PurchaseLimitType = PurchaseLimitType.None);
        //    }

        //    return currentLimits;
        //}
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                               ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var purchasingLimitManager = PurchasingLimitManager(cart.DistributorID);
                if (purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan)
                {
                    return base.PerformRules(cart, reason, Result);
                }

                Result = checkVolumeLimits(cart as MyHLShoppingCart, Result, "es-VE", "VE");
            }
            return Result;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
            {
            //bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            //if (!isExempt)
            //{
            //    return false;
            //}

            //List<string> codes = new List<string>(CountryType.VE.HmsCountryCodes);
            //List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            //if (!codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            //{
            //    //no TIN code, has limits
            //    return (tins.Count() != 0);
            //}

            return false;
        }
    }
}