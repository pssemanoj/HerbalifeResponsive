using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.en_US
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule, IHFFRule
    {
        private const string RuleName = "CartIntegrity Rules";

        /// <summary>
        ///     Indicates when the HFF module must to be shown.
        /// </summary>
        /// <param name="cart">The shopping cart.</param>
        /// <returns>If the HFF module must to be shown.</returns>
        public bool CanDonate(ShoppingCart_V02 cart)
        {
            bool canDonate = HLConfigManager.Configurations.DOConfiguration.AllowHFF;
            var myHLCart = cart as MyHLShoppingCart;
            if (myHLCart != null && myHLCart.OrderCategory == OrderCategoryType.RSO && myHLCart.DeliveryInfo != null &&
                myHLCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return canDonate;
            }

            if (myHLCart != null && myHLCart.OrderCategory == OrderCategoryType.RSO && myHLCart.DeliveryInfo != null &&
                myHLCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                var allowedWarehouses = new List<string> {"03", "09", "X6", "P9"};
                if (allowedWarehouses.Contains(myHLCart.DeliveryInfo.WarehouseCode))
                {
                    return canDonate;
                }
            }

            return false;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartWarehouseCodeChanged)
            {
                if (!CanDonate(cart))
                {
                    result = CheckForHFFSKU(cart, result);
                    if (result.Result == RulesResult.Failure)
                    {
                        LoggerHelper.Error(string.Join("\r\n", result.Messages.ToArray()));
                    }
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (cart.CurrentItems[0].SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku)
                {
                    if (!CanDonate(cart))
                    {
                        result = CheckForHFFSKU(cart, result);
                        result.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SKUNotAvailable")
                                           .ToString(), cart.CurrentItems[0].SKU));
                        result.Result = RulesResult.Failure;
                        LoggerHelper.Error(string.Join("\r\n", result.Messages.ToArray()));
                    }
                }
            }
            else if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                // Get the correct freight code for address, first time cart is retrieved
                var session = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
                var shoppingCart = cart as MyHLShoppingCart;
                if (shoppingCart != null && shoppingCart.DeliveryInfo != null &&
                    shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                    shoppingCart.DeliveryInfo.FreightCode == "NOF" && !session.IsEventTicketMode)
                {
                    shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                }
            }
            return result;
        }

        private ShoppingCartRuleResult CheckForHFFSKU(ShoppingCart_V01 shoppingCart, ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                // look for the HFF sku
                var itemsToRemove = from item in shoppingCart.CartItems
                                    where item.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                                    select item;

                if (itemsToRemove.Count() > 0)
                {
                    var SKUsToRemove = itemsToRemove.Select(s => s.SKU).ToList();
                    var deleted = ShoppingCartProvider.DeleteShoppingCart(cart, SKUsToRemove);
                    ruleResult.Result = RulesResult.Failure;
                    Array.ForEach(SKUsToRemove.ToArray(),
                                  a =>
                                  ruleResult.AddMessage(
                                      string.Format(
                                          "Invalid sku found. DS: {0}, CART: {1}, SKU {2} : removed from cart.",
                                          cart.DistributorID, cart.ShoppingCartID, a)));
                    // should be a safe removal
                    Array.ForEach(SKUsToRemove.ToArray(),
                                  a => shoppingCart.CartItems.Remove(shoppingCart.CartItems.Find(x => x.SKU == a)));
                    // for DistributorShoppingCartItem, have to find match 'cause it may not exist
                    cart.ShoppingCartItems.RemoveAll(
                        delegate(DistributorShoppingCartItem x)
                            { return SKUsToRemove.Where(s => s == x.SKU).Count() > 0; });
                }
            }
            return ruleResult;
        }
    }
}