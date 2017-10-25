using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.CardRegistry.Global
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
                result.Add(PerformRules(cart, defaultResult, reason));
            }

            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            //Do the rules
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                return CartItemBeingAddedRuleHandler(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result)
        {
            //They can't shop without a registered credit card.
            if (HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry)
            {
                if (!HLConfigManager.Configurations.PaymentsConfiguration.AllowDirectDepositPayment &&
                    !HLConfigManager.Configurations.PaymentsConfiguration.AllowWirePayment)
                {
                    var payments = PaymentInfoProvider.GetPaymentInfo(cart.DistributorID, Locale);
                    if (null == payments || payments.Count == 0)
                    {
                        result.Result = RulesResult.Failure;
                        result.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "NoCard") as string);
                        cart.RuleResults.Add(result);
                        return result;
                    }
                }
            }

            return result;
        }
    }
}