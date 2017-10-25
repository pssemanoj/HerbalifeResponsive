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

namespace MyHerbalife3.Ordering.Rules.Promotional.ru_RU
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly List<string> AllowedSKUToAddPromoSKU = new List<string> {"0105", "0106"};
        private string promoSku = "U354";
        private int quantity;

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
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (cart != null && cart.CurrentItems[0].SKU == promoSku)
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(string.Empty);
                    cart.RuleResults.Add(Result);
                    return Result;
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartItemsRemoved)
            {
                var hlCart = cart as MyHLShoppingCart;
                if (cart == null || hlCart == null)
                {
                    return Result;
                }

                int quantity0105 = 0;
                int quantity0106 = 0;

                if (cart.CartItems.Any(i => i.SKU == "0105"))
                {
                    quantity0105 = cart.CartItems.Find(m => m.SKU == "0105").Quantity;
                }

                if (cart.CartItems.Any(i => i.SKU == "0106"))
                {
                    quantity0106 = cart.CartItems.Find(m => m.SKU == "0106").Quantity;
                }

                int remainder = 0;
                quantity = Math.DivRem(quantity0105 + quantity0106, 3, out remainder);

                if (quantity == 0 || !ProductInCart(cart))
                {
                    //check whether cart contains promo SKU,if yes delete it.
                    if (cart.CartItems.Any(i => i.SKU == promoSku))
                    {
                        hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku}), true);
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                        return Result;
                    }
                }
                else if (quantity > 0)
                {
                    //Refresh the promo SKU quantity by removing and adding it again.
                    if (cart.CartItems.Any(i => i.SKU == promoSku))
                    {
                        //if (AllowedSKUToAddPromoSKU.Any(s => s == cart.CartItems[0].SKU))
                        if ((from a in cart.CartItems
                             from b in AllowedSKUToAddPromoSKU
                             where a.SKU == b
                             select b).Count() > 0)
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku}), true);
                        }
                    }
                }

                return AddToCart(hlCart, Result);
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                //If they're trying to delete/modify it, stop them.
                if (cart != null && cart.CurrentItems[0].SKU == promoSku)
                {
                    return Result;
                }
            }
            //else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
            //{
            //    //If there are no Product types remaining in the cart, remove the promo sku if it's already there
            //    if (cart.CartItems.Any(i => i.SKU == promoSku))
            //    {
            //        CatalogItemList catItems = CatalogProvider.GetCatalogItems((from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
            //        if (catItems != null)
            //        {
            //            if (!catItems.Any(c => c.Value.ProductType == HL.Common.ValueObjects.ProductType.Product))
            //            {
            //                hlCart.DeleteItemsFromCart(new List<string>(new string[] { promoSku }), true);
            //                Result.Result = RulesResult.Success;
            //                cart.RuleResults.Add(Result);
            //                return Result;
            //            }
            //        }
            //        //if it's the last sku in the cart, remove it.
            //        if (cart.CartItems.Count == 1)
            //        {
            //            hlCart.DeleteItemsFromCart(new List<string>(new string[] { promoSku }), true);
            //            Result.Result = RulesResult.Success;
            //            cart.RuleResults.Add(Result);
            //            return Result;
            //        }
            //    }
            //}

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
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                    }
                }
            }

            return Result;
        }

        private bool ProductInCart(ShoppingCart_V02 cart)
        {
            bool hasProduct = false;
            var catItems =
                CatalogProvider.GetCatalogItems((from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
            if (catItems != null)
            {
                if (catItems.Any(c => c.Value.ProductType == ProductType.Product))
                {
                    hasProduct = true;
                }
            }

            return hasProduct;
        }
    }
}