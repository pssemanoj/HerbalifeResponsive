using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Rules.APF.it_IT
{
    public class APFRules : MyHerbalifeRule, IShoppingCartRule
    {
        #region Consts

        private const string IS_EVENT_TICKET_SESSION_KEY = "Is_Event_Ticket";

        #endregion

        #region IShoppingCartRule interface implementation

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "APF Rules";
            defaultResult.Result = RulesResult.Unknown;

            if (null != cart)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }

            return result;
        }

        #endregion

        #region Private methods

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V01 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            if (null != cart)
            {
                //Do the rules
                if (reason == ShoppingCartRuleReason.CartRetrieved)
                {
                    return CartRetrievedRuleHandler(cart, result);
                }
            }
            return result;
        }

        #endregion

        #region Event methods

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart,
                                                                ShoppingCartRuleResult result)
        {
            try
            {
                if (null != cart)
                {
                    if (APFDueProvider.IsAPFSkuPresent(cart.CartItems) &&
                        APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale))
                    {
                        var limits =
                            PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);
                        if (null != limits && null != limits.PurchaseSubType)
                        {
                            if (limits.PurchaseType == ServiceProvider.OrderSvc.OrderPurchaseType.Consignment)
                                //No consignment allowed when APFDue
                            {
                                if (limits.PurchaseSubType == "A1") limits.PurchaseSubType = "A2";
                                if (limits.PurchaseSubType == "B1") limits.PurchaseSubType = "B2";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("APFRules.it-IT.ProcessAPF DS:{0} locale:{2} ERR:{1}", cart.DistributorID, ex, Locale));
            }

            return result;
        }

        #endregion
    }
}