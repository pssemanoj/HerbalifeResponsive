using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.es_CL
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = false;
            var codes = new List<string>(CountryType.CL.HmsCountryCodes);
            codes.Add(CountryType.CL.Key);

            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true); //HMS made up the country code
                //Must have CIID TinCode to purchase
                if (tins.Count > 0)
                {
                    var tin = (from t in tins where t.IDType.Key == "CIID" select t).FirstOrDefault();
                    if (tin != null)
                    {
                        canPurchase = true;
                    }
                }
            }
            else
            {
                canPurchase = true;
            }

            return canPurchase;
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