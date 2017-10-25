using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchasingLimits;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.HR
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        private List<string> baseIsolatedSkus = new List<string>() {};
        private List<string> baseStandAloneSkus = new List<string>() {};

        private List<string> IsolatedSkus { get; set; }
        private List<string> StandaloneSkus { get; set; }

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);

            PurchaseLimitType limitType = PurchaseLimitType.Volume;
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
                return;
            if (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRBL" }).Count() == 3)
            {
                limitType = PurchaseLimitType.None;
            }

            else if (tins != null 
                && 
                (   tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRRP" }).Count() == 3 ||
                    tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRPI" }).Count() == 3 )
                )
            {
                limitType = PurchaseLimitType.Volume;
            }
            limits.PurchaseLimitType = limitType;
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
            SetLimits(orderMonth, manager, limits);
        }

        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Success;

            if (null == cart)
            {
                LoggerHelper.Error(
                    string.Format("{0} cart is null {1}, MyHerbalife3.Ordering.Rules.PurchaseRestriction.HR.PurchaseRestrictionRules.", Locale, cart.DistributorID));
                Result.Result = RulesResult.Failure;
                return Result;
            }
            DateTime currentLocalDatetime = HL.Common.Utilities.DateUtils.ConvertToLocalDateTime(DateTime.Now, Country);
            if (currentLocalDatetime.Subtract(DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, Country).ApplicationDate).TotalHours < 24)
            {
                Result.Result = RulesResult.Failure;
                Result.AddMessage(
                    HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_Rules", HLConfigManager.Platform), "CroatiaNewMemberError").ToString());
                cart.RuleResults.Add(Result);
                cart.ItemsBeingAdded.Clear();
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
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded && cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
            {
                DefineRuleSkus();
                // Check for isolated skus been added
                var isolated = cart.ItemsBeingAdded.FirstOrDefault(i => IsolatedSkus.Contains(i.SKU));
                if (isolated != null && cart.ItemsBeingAdded.Select(s => s.SKU).Except(IsolatedSkus.Select(i => i)).Count() > 0)
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "StandaloneSku").ToString(), string.Join(" ", IsolatedSkus.ToArray())));
                    cart.RuleResults.Add(Result);
                    cart.ItemsBeingAdded.Clear();
                    return Result;
                }
                // Check for isolated skus against skus in cart
                if (isolated != null  && cart.CartItems.Any() && cart.CartItems.Where(s => s.SKU != isolated.SKU).Count() > 0)
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "StandaloneSku").ToString(), string.Join(" ", IsolatedSkus.ToArray())));
                    cart.RuleResults.Add(Result);
                    cart.ItemsBeingAdded.Clear();
                    return Result;
                }

                // Check for not combinable skus
                if ((cart.ItemsBeingAdded.Any(i => StandaloneSkus.Contains(i.SKU)) && cart.ItemsBeingAdded.Select(s => s.SKU).Except(StandaloneSkus.Select(i => i)).Count() > 0) ||
                (cart.CartItems.Any(i => StandaloneSkus.Contains(i.SKU)) && !cart.ItemsBeingAdded.Any(i => StandaloneSkus.Contains(i.SKU))) ||
                (cart.CartItems.Any(i => !StandaloneSkus.Contains(i.SKU)) && cart.ItemsBeingAdded.Any(i => StandaloneSkus.Contains(i.SKU))))
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "StandaloneSkus").ToString()));
                    cart.RuleResults.Add(Result);
                    cart.ItemsBeingAdded.Clear();
                    return Result;
                }
            }
            
            base.PerformRules(cart, reason, Result);
            return Result;

        }

        public override bool CanPurchase(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            if (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRBL" }).Count() == 3)
            {
                return true;
            }

            else if (tins != null
                &&
                (tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRRP" }).Count() == 3 ||
                    tins.Select(c => c.IDType.Key).Intersect(new[] { "HROI", "HRNA", "HRPI" }).Count() == 3)
                )
            {
                return true;
            }
            return false;
        }

        private void DefineRuleSkus()
        {
            StandaloneSkus =
                string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.StandaloneSkus)
                    ? baseStandAloneSkus
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.StandaloneSkus.Split(',').ToList();
            IsolatedSkus =
                string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.IsolatedSkus)
                    ? baseIsolatedSkus
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.IsolatedSkus.Split(',').ToList();
        }
    }
}