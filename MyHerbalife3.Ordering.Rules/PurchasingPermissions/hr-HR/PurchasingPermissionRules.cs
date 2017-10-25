using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.hr_HR
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            /// Foreign DS is not allowed to place orders in Croatia
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);

            if (!(new List<string>(CountryType.HR.HmsCountryCodes)).Union(new List<string>() { CountryType.HR.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                return false;

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

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                try
                {

                    if (!CanPurchase(cart.DistributorID, Country))
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                        cart.RuleResults.Add(Result);
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
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add to Cart Rule for Chilean distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}
