using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ViewModel.Model;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.Global
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        public const string RuleName = "CartIntegrity Rules";

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
            if (reason == ShoppingCartRuleReason.CartCalculated)
            {
                result = CheckForInvalidSKUs(cart, result);
                result = CheckForDuplicateSKUs(cart, result);
                result = CheckForInvalidQuantities(cart, result);
                if (result.Result == RulesResult.Failure)
                {
                    LoggerHelper.Error(string.Join("\r\n", result.Messages.ToArray()));
                }
            }
            //prevent to able to add products blocked by the skuToNoDisplayForNonQualifyMembers and EventId config key
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var DSType = DistributorOrderingProfileProvider.CheckDsLevelType(cart.DistributorID, cart.Locale.Substring(3, 2));
                if (DSType == ServiceProvider.DistributorSvc.Scheme.Member)
                {
                    string errormessage =
                        HttpContext.GetGlobalResourceObject("MyHL_ErrorMessage", "PMTypeRestrictOrdering").ToString();

                    result.AddMessage(errormessage);
                    result.Result = RulesResult.Failure;
                }
                else
                    result = checkForConfigBlockedSKU(cart, result);
            }

            return result;
        }

        #region CartIntegrity rule methods

        private bool removeThisItem(string sku, ShoppingCart_V01 cart)
        {
            string[] skuToExclude =
                {
                    HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                    HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                    HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                };
            if (!skuToExclude.Any(s => s == sku))
            {
                return true;
            }
            return false;
        }

        private ShoppingCartRuleResult CheckForInvalidSKUs(ShoppingCart_V01 shoppingCart,
                                                           ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                DistributorShoppingCartItem cartItem;
                // pick up all ShoppingCartItem_V01 to be removed
                var itemsToRemove = from item in shoppingCart.CartItems
                                    where
                                        ((cartItem = cart.ShoppingCartItems.Find(x => x.SKU == item.SKU)) == null ||
                                         (cartItem.CatalogItem == null || string.IsNullOrEmpty(cartItem.Description))) &&
                                        removeThisItem(item.SKU, shoppingCart)
                                    select item;

                if (itemsToRemove.Count() > 0)
                {
                    var SKUsToRemove = itemsToRemove.Select(s => s.SKU).ToList();
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
                        delegate(DistributorShoppingCartItem x)
                        { return SKUsToRemove.Where(s => s == x.SKU).Count() > 0; });
                }
            }
            return ruleResult;
        }

        private ShoppingCartRuleResult CheckForDuplicateSKUs(ShoppingCart_V01 shoppingCart,
                                                             ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                var skuGroups = from s in cart.CartItems group s by s.SKU into g select new { SKU = g.Key, Items = g };

                foreach (var g in skuGroups)
                {
                    if (g.Items.Count() > 1)
                    {
                        cart.DeleteItemsFromCart(new List<string> { g.SKU }, true);
                        ruleResult.Result = RulesResult.Failure;
                        ruleResult.AddMessage(
                            string.Format("Duplicate sku found. DS: {0}, CART: {1}, SKU {2} removed from cart.",
                                          cart.DistributorID, cart.ShoppingCartID, g.SKU));
                    }
                }
            }

            return ruleResult;
        }

        public ShoppingCartRuleResult CheckForInvalidQuantities(ShoppingCart_V01 shoppingCart,
                                                                ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                var invalidKSUList = (from s in cart.CartItems where s.Quantity <= 0 select s.SKU).ToList<string>();
                if (invalidKSUList.Count() > 0)
                {
                    cart.DeleteItemsFromCart(invalidKSUList, true);
                    cart.AddItemsToCart(
                        (from s in invalidKSUList
                         select new ShoppingCartItem_V01
                         {
                             SKU = s,
                             Quantity = 1,
                             Updated = DateTime.Now
                         }
                        ).ToList(), true
                        );

                    ruleResult.Result = RulesResult.Failure;
                    Array.ForEach(invalidKSUList.ToArray(),
                                  a =>
                                  ruleResult.AddMessage(
                                      string.Format(
                                          "Invalid sku quantity found. DS: {0}, CART: {1}, SKU {2} : quantity updated to 1.",
                                          cart.DistributorID, cart.ShoppingCartID, a)));
                }
            }
            return ruleResult;
        }


        public ShoppingCartRuleResult checkForConfigBlockedSKU(ShoppingCart_V01 shoppingCart,
                                                                ShoppingCartRuleResult ruleResult)
        {

            if (EventId > 0 && !DistributorOrderingProfileProvider.IsEventQualified(EventId, shoppingCart.Locale))
            {
                var cart = shoppingCart as MyHLShoppingCart;
                List<string> listToHide =
                    HLConfigManager.Configurations.DOConfiguration.SkuToNoDisplayForNonQualifyMembers.Split(',').ToList();
                if (cart != null && listToHide.Contains(cart.CurrentItems[0].SKU))
                {
                    var message = "SKUNotAvailable";
                    var globalResourceObject =
                        HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                            "SKUNotAvailable");
                    if (globalResourceObject != null)
                    {
                        message = string.Format(globalResourceObject.ToString(), shoppingCart.CurrentItems[0].SKU);
                    }
                    
                    ruleResult.AddMessage(message);
                    ruleResult.Result = RulesResult.Failure;
            }
            }

            return ruleResult;
        }

        private int EventId
        {
            get
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.EventId))
                {
                    int value = 0;
                    int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out value);
                    return value;
                }
                else
                {
                    return 0;
                }

            }
        }


        #endregion
    }
}