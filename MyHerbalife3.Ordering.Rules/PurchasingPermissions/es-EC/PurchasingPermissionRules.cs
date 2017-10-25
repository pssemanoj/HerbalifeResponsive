using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.es_EC
{
    /// <summary>
    ///     Used to perform the purshasing permissions rules for Ecuador.
    /// </summary>
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        /// <summary>
        ///     used to determine if a distributor can purshase or not.
        /// </summary>
        /// <param name="distributor">Distributor to evaluate.</param>
        /// <returns>True if can purshase.</returns>
        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = false;
            var codes = new List<string>(CountryType.EC.HmsCountryCodes);
            codes.Add(CountryType.EC.Key);

            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);

                //Must have ECID or ECTX TinCode to purchase
                if (tins.Count > 0)
                {
                    var requiredTins = new List<string>(new[] {"ECID", "ECTX "});
                    var tin = (from t in tins from r in requiredTins where t.IDType.Key == r select t).ToList();
                    if (tin != null && tin.Count > 0)
                    {
                        canPurchase = true;
                    }
                }
            }
            else
            {
                //No restriction for EC or foreign DS.(Orders without Tin code will error out and business will have to rekey these orders
                canPurchase = true;
            }

            return canPurchase;
        }

        /// <summary>
        ///     Process cart rules.
        /// </summary>
        /// <param name="cart">Shopping cart.</param>
        /// <param name="reason">Cart rules reason.</param>
        /// <returns>List of rules.</returns>
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        /// <summary>
        ///     Perform rules action.
        /// </summary>
        /// <param name="cart">Shopping cart.</param>
        /// <param name="reason">Cart rules reason.</param>
        /// <param name="result">Cart rule result.</param>
        /// <returns>Cart tule result.</returns>
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
                            "Error while performing Add to Cart Rule for HongKong distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}