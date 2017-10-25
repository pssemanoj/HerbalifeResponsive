using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.id_ID
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        private List<string> AllowedSKUToAddPromoSKU = new List<string> {"1031","1032","1033","1038","1039","1040"};
        private string promoSku = "8444";
        private int quantity=0;

        /// <summary>
        ///     Processes the cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "Promotional Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }


        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded )//|| reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                return IsPromoTryAdded(cart, Result, country);
            }
            else if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartItemsRemoved || 
                 (reason == ShoppingCartRuleReason.CartRetrieved && cart.CustomerOrderDetail != null && !string.IsNullOrEmpty(cart.CustomerOrderDetail.CustomerOrderID)))
            {
                var hlCart = cart as MyHLShoppingCart;
                if (cart == null || hlCart == null)
                {
                    return Result;
                }
                if (reason == ShoppingCartRuleReason.CartItemsRemoved && hlCart.IsPromoDiscarted)
                {
                    hlCart.IsPromoDiscarted = false;
                    Result.Result = RulesResult.Failure;
                    string message = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "NoModifyPromoSku").ToString());
                    Result.AddMessage(message);
                    return Result;
                }
                quantity = 0;
                //Calculate the qty of sku will be added
                quantity = cart.CartItems.Where(x => AllowedSKUToAddPromoSKU.Contains(x.SKU)).Sum(x => x.Quantity);
                if (quantity == 0 )
                {
                    //check whether cart contains promo SKU,if yes delete it.
                    if (cart.CartItems.Any(i => i.SKU == promoSku))
                    {
                        hlCart.DeleteItemsFromCart(new List<string>(new[] { promoSku }), true);
                    }
                    if (hlCart.IsPromoDiscarted)
                    {
                        hlCart.IsPromoDiscarted = false;
                        Result.Result = RulesResult.Failure;
                        string message = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "NoModifyPromoSku").ToString());
                        Result.AddMessage(message);
                    }
                    else
                    {
                        Result.Result = RulesResult.Success;
                    }
                    cart.RuleResults.Add(Result);
                    return Result;
                }
                else if (quantity > 0)
                {
                    //Refresh the promo SKU quantity by removing and adding it again.
                    if (cart.CartItems.Any(i => i.SKU == promoSku))
                    {
                        if ((from a in cart.CartItems
                             from b in AllowedSKUToAddPromoSKU
                             where a.SKU == b
                             select b).Count() > 0)
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] { promoSku }), true);
                        }
                    }
                }

                return AddToCart(hlCart, Result, quantity, reason, cart.CurrentItems != null && cart.CurrentItems.Count > 0 ? AllowedSKUToAddPromoSKU.Contains(cart.CurrentItems[0].SKU) : false);
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                //If they're trying to delete/modify it, stop them.
                if (cart != null && cart.CurrentItems[0].SKU == promoSku)
                {
                    var hlCart = cart as MyHLShoppingCart;
                    hlCart.IsPromoDiscarted = true;
                    cart.CurrentItems.Clear();
                    Result.Result = RulesResult.Success;
                    cart.RuleResults.Add(Result);
                    return Result;
                }
            }

            return Result;
        }

        private ShoppingCartRuleResult IsPromoTryAdded(ShoppingCart_V02 cart, ShoppingCartRuleResult Result, RegionInfo country)
        {
            if(cart.CurrentItems != null)
            if (cart != null && cart.CurrentItems[0].SKU == promoSku)
            {
                var hlCart = cart as MyHLShoppingCart;
                hlCart.IsPromoDiscarted = true;
                cart.CurrentItems.Clear();
                Result.Result = RulesResult.Success;
                cart.RuleResults.Add(Result);
                return Result;
            }
            return Result;
        }

        /// <summary>
        ///     Adds to cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="Result">The result.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult AddToCart(MyHLShoppingCart cart, ShoppingCartRuleResult Result, int quantity, ShoppingCartRuleReason reason, bool displayMessage)
        {
            cart.IsPromoDiscarted = false;
            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (!allSkus.Keys.Contains(promoSku))
            {
                if (Environment.MachineName.IndexOf("PROD") < 0)
                {
                    var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                    string message =
                        string.Format(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "NoPromoSku").ToString(), country.DisplayName, promoSku);
                    LoggerHelper.Error(message);
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(message);
                    cart.RuleResults.Add(Result);
                    return Result;
                }

                return Result;
            }

            //Promo items must have inventory
            var catItem = CatalogProvider.GetCatalogItem(promoSku, Country);
            WarehouseInventory warehouseInventory = null;
            if (null != cart.DeliveryInfo && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                if (catItem.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if ((warehouseInventory as WarehouseInventory_V01).QuantityAvailable > 0)
                    {
                        cart.AddItemsToCart(
                            new List<ShoppingCartItem_V01>(new[]
                                {new ShoppingCartItem_V01(0, promoSku, quantity, DateTime.Now)}), true);

                        string message = string.Empty;
                        if(reason == ShoppingCartRuleReason.CartItemsAdded)
                        {
                            if (displayMessage)
                            {
                                message =
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "PromotionalSkuAdded").ToString());
                                Result.Result = RulesResult.Feedback;
                            }
                            else if (cart.CurrentItems.Count > 0 && cart.CurrentItems[0].SKU == promoSku)
                            {
                                message = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),"NoModifyPromoSku").ToString());
                                Result.Result = RulesResult.Failure;
                            }
                            else
                            {
                                Result.Result = RulesResult.Success;
                            }
                        }
                        Result.AddMessage(message);
                        cart.IsPromoNotified = true;
                        cart.RuleResults.Add(Result);
                    }
                }
            }

            return Result;
        }

    }
}