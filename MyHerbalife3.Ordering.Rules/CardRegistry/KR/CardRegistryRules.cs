using System.Collections.Generic;
using System.Web;
using System.Linq;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using HL.Common.Logging;
using System;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.CardRegistry.KR
{
    public class CardRegistryRules : MyHerbalifeRule, IShoppingCartRule
    {
        /// <summary>
        ///     This rule returns success if the current cart contains Event Tickets
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "CardRegistry Rules";
            defaultResult.Result = RulesResult.Unknown;
            if (null != cart)
            {
                if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                {
                    result.Add(PerformRules(cart, defaultResult, reason));
                }
            }

            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded || reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                try
                {

                    string key = PaymentsConfiguration.GetPaymentInfoSessionKey(string.Empty, string.Empty);
                    var items = Session[key] as List<CreditPayment>;
                    if (null != items)
                    {
                        int maxCards = HLConfigManager.Configurations.PaymentsConfiguration.MaxCardsToDisplay;
                        if (items.Count(p => (!string.IsNullOrEmpty(p.AuthorizationCode))) == maxCards)
                        {
                            if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
                            {
                                cart.CurrentItems.Clear();
                            }
                            result.Result = RulesResult.Failure;
                            result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "MustDeleteACardToChangeItemsInCart").ToString()));
                            cart.RuleResults.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add or Delete from Cart Rule for Korean distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return result;
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result)
        {
           

            return result;
        }
    }
}