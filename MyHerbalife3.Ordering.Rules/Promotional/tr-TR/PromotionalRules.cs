using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.tr_TR
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        private string promoSku = "P969";
        private int quantity = 1;

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
            var hlCart = cart as MyHLShoppingCart;
            if (hlCart == null)
            {
                return Result;
            }

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (cart.CartItems.Any(i => i.SKU == promoSku))
                {
                    return Result; //SKU already present - exit.
                }

                //If the item being added is a Product, add the promo sku.
                var current = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                if (null != current && current.ProductType == ProductType.Product)
                {
                    return AddToCart(hlCart, Result);
                }
                else
                {
                    //If there are already products in the cart, add the promo sku.
                    var catItems =
                        CatalogProvider.GetCatalogItems(
                            (from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
                    if (catItems != null)
                    {
                        if (catItems.Any(c => c.Value.ProductType == ProductType.Product))
                        {
                            return AddToCart(hlCart, Result);
                        }
                    }
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                //If they're trying to delete it, stop them.
                if (cart.CurrentItems[0].SKU == promoSku)
                {
                    cart.CurrentItems.Clear();
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
            {
                //If there are no Product types remaining in the cart, remove the promo sku if it's already there
                if (cart.CartItems.Any(i => i.SKU == promoSku))
                {
                    var catItems =
                        CatalogProvider.GetCatalogItems(
                            (from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
                    if (catItems != null)
                    {
                        if (!catItems.Any(c => c.Value.ProductType == ProductType.Product))
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku}), true);
                            Result.Result = RulesResult.Success;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    //if it's the last sku in the cart, remove it.
                    if (cart.CartItems.Count == 1)
                    {
                        hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku}), true);
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                        return Result;
                    }
                }
            }

            return Result;
        }

        private ShoppingCartRuleResult AddToCart(MyHLShoppingCart cart, ShoppingCartRuleResult Result)
        {
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
                    Result.Result = RulesResult.Feedback;
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
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                    }
                }
            }

            return Result;
        }
    }
}