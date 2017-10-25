using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.el_GR
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartCreated)
            {
                result = this.CheckForHFFSKU(cart, result);
                if (result.Result == RulesResult.Failure)
                {
                    LoggerHelper.Error(string.Join("\r\n", result.Messages.ToArray()) );
                }
            }
            return result;
        }

        private ShoppingCartRuleResult CheckForHFFSKU(ShoppingCart_V01 shoppingCart, ShoppingCartRuleResult ruleResult)
        {
            MyHLShoppingCart cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                // look for the HFF sku
                var itemsToRemove = from item in shoppingCart.CartItems
                                    where item.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                                    select item;

                if (itemsToRemove.Count() > 0)
                {
                    var SKUsToRemove = itemsToRemove.Select(s => s.SKU).ToList<string>();
                    ShoppingCartProvider.DeleteShoppingCart(cart, SKUsToRemove);
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
                        new Predicate<DistributorShoppingCartItem>(
                            delegate(DistributorShoppingCartItem x)
                                { return SKUsToRemove.Where(s => s == x.SKU).Count() > 0; }));
                }
            }
            return ruleResult;
        }
    }
}