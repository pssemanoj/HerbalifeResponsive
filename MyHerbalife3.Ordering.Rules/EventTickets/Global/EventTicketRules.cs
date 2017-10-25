using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.EventTicket.Global
{
    public class EventTicketRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly List<string> _systemSKUS = new List<string> {"9901"};

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
            defaultResult.RuleName = "ETO Rules";
            defaultResult.Result = RulesResult.Unknown;
            if (null != cart)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }
            return result;
        }

        private bool isCheckEventTicket(string distributorID, string locale)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (null != sessionInfo)
            {
                return sessionInfo.IsEventTicketMode;
            }
            return false;
        }

        protected bool isCheckEventTicket(ShoppingCart_V01 cart)
        {
            return isCheckEventTicket(cart.DistributorID, cart.Locale);
        }

        protected virtual ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            //Do the rules
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                return CartRetrievedRuleHandler(cart, result);
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                return CartItemBeingAddedRuleHandler(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart,
                                                                ShoppingCartRuleResult result)
        {
            var myhlCart = cart as MyHLShoppingCart;
            if (null != myhlCart)
            {
                if (isCheckEventTicket(myhlCart.DistributorID, cart.Locale))
                {
                    if (myhlCart.DeliveryInfo != null)
                    {
                        if (
                            !string.IsNullOrEmpty(
                                HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode))
                            myhlCart.DeliveryInfo.FreightCode =
                                HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                        if (
                            !string.IsNullOrEmpty(
                                HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode))
                            myhlCart.DeliveryInfo.WarehouseCode =
                                HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
                    }
                }
            }
            cart.RuleResults.Add(result);
            return result;
        }

        private bool isEventTicketSKU(CatalogItem_V01 item, string sku)
        {
            return item.IsEventTicket &&
                   !_systemSKUS.Contains(sku) &&
                   !APFDueProvider.IsAPFSku(sku) &&
                   !HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Exists(s => s.Equals(sku)) &&
                   !(sku.Equals(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku) || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(sku)));
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result)
        {
            //cart.CurrentItems[0] contains the current item being added
            //because the provider only adds one at a time, we just need to return a single error, but aggregate to the cart errors for the UI

            string sku = cart.CurrentItems[0].SKU;
            var item = CatalogProvider.GetCatalogItem(sku, base.Country);
            if (null != item)
            {
                if (isEventTicketSKU(item, sku))
                {
                    if (!HLConfigManager.Configurations.DOConfiguration.AllowEventPurchasing)
                    {
                        result.Result = RulesResult.Failure;
                        result.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "EventPurchasingNotAvailable") as string, 2));
                        cart.RuleResults.Add(result);
                        return result;
                    }
                    var myhlCart = cart as MyHLShoppingCart;
                    if (null != myhlCart)
                    {
                        if (myhlCart.OrderCategory != OrderCategoryType.ETO)
                        {
                            result.Result = RulesResult.Failure;
                            var errorMessage =
                         HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                             "InvalidSKU") ??
                         "The sku {0} is not valid.";
                            result.AddMessage(string.Format(errorMessage.ToString(), sku));
                            cart.RuleResults.Add(result);
                            return result;
                        }
                        else
                        {
                            if (
                                !string.IsNullOrEmpty(
                                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode))
                                myhlCart.DeliveryInfo.FreightCode =
                                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;

                            if (
                                !string.IsNullOrEmpty(
                                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode))
                                myhlCart.DeliveryInfo.WarehouseCode =
                                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
                            if (HLConfigManager.Configurations.DOConfiguration.ShowEventTicketMessaging)
                            {
                                result.Result = RulesResult.Feedback;
                                result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                            "EventTicketSpecialMessage") as string, sku));
                                cart.RuleResults.Add(result);
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    var myhlCart = cart as MyHLShoppingCart;
                    if (null != myhlCart)
                    {
                        if (myhlCart.OrderCategory == OrderCategoryType.ETO)
                        {
                            result.Result = RulesResult.Failure;
                            result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "InvalidSKU") as
                                    string, sku));
                            cart.RuleResults.Add(result);
                        }
                    }
                }
            }

            return result;
        }

        
        /// <summary>
        /// Extract resx text from string.Format("{0}_Rules", HLConfigManager.Platform)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected string GetRulesResourceString(string key, string defaultValue = null)
        {
            var clsKey = string.Format("{0}_Rules", HLConfigManager.Platform);
            var value = HttpContext.GetGlobalResourceObject(clsKey, key);

            if (value == null || !(value is string))
            {
                HL.Common.Logging.LoggerHelper.Warn(string.Format("Missing {0} resource object. Key: {1}", clsKey, key));
                return defaultValue;
            }

            return value as string;
        }
    }
}