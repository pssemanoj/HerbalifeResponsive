using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.hr_HR
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            var limitsType = PurchaseLimitType.ProductCategory;

            if (!DistributorIsExemptFromPurchasingLimits(distributorId))
            {
                List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
                if ((tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRRP" }).Count() == 3) ||
                    (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRPI" }).Count() == 3)
                    )
                {
                    limitsType = PurchaseLimitType.None;
                }
                else
                {
                    limitsType = PurchaseLimitType.Volume;
                }
            }
            else
            {
                limitsType = PurchaseLimitType.None;
            }

            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);
            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            return (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRBL" }).Count() == 3);
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsAdded)
            {
                var myhlCart = cart as MyHLShoppingCart;

                if (null == myhlCart)
                {
                    LoggerHelper.Error(
                        string.Format("{0} myhlCart is null {1}", Locale, cart.DistributorID));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }
                List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                if (tins != null && tins.Find(t => t.IDType.Key == "OBRT") != null)
                {
                    ///Poštovani - ukoliko imate registriran OBRT, možete platisa sa vašom Business karticom ili vašom osobnom kreditnom karticom ukoliko kupnja ne prelazi 5000 HRK. U slučaju da plaćate bankovnim transferom možete platiti sa vašeg Business transakcijskog računa vezanog uz vaš Obrt ili sa vašeg osobnog računa ukoliko kupnja ne prelazi 5000 HRK
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                "Croatia_SpecialMessage").ToString()));
                    cart.RuleResults.Add(Result);
                }
            }
            return base.PerformRules(cart, reason, Result);
        }
    }
}