using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.it_IT
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly List<string> AllowedSKUToAddPromoSKU = new List<string>
            {
                "U576",
                "U577",
                "U578",
                "U579",
                "U580",
                "U581"
            };

        private string promoSku = "U320";
        private string promoSkuU659 = "U659";
        private string promo_U688 = "U688";
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

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (cart != null &&
                    (cart.CurrentItems[0].SKU == promoSku || cart.CurrentItems[0].SKU == promo_U688 ||
                     cart.CurrentItems[0].SKU == promoSkuU659))
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(string.Empty);
                    cart.RuleResults.Add(Result);
                    return Result;
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsAdded)
            {
                //if(!ProductInCart(cart))
                //{
                //    return Result;  //Cart has no products - we're done.
                //}
                //If new volume > 999.
                if (hlCart != null && cart != null)
                {
                    if (hlCart.VolumeInCart > 999 && ProductInCart(cart))
                    {
                        quantity = 1;
                        if (!cart.CartItems.Any(i => i.SKU == promoSku))
                        {
                            Result = AddToCart(hlCart, Result, promoSku);
                        }
                    }
                    //If new volume >= 300.
                    if (hlCart.VolumeInCart >= 300 && ProductInCart(cart))
                    {
                        quantity = 1;
                        if (!cart.CartItems.Any(i => i.SKU == promoSkuU659))
                        {
                            Result = AddToCart(hlCart, Result, promoSkuU659);
                        }
                    }
                    else
                    {
                        hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku, promoSkuU659}), true);
                    }
                    if (GetQuantity(cart) == 0)
                    {
                        //Remove the promo SKU as parent quantity is become zero or not products in cart
                        if (cart.CartItems != null && cart.CartItems.Any(i => i.SKU == promo_U688))
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promo_U688}), true);
                            Result.Result = RulesResult.Success;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    else if (GetQuantity(cart) > 0)
                    {
                        //Remove promo SKU and add it again based on quantity
                        if (cart.CartItems != null && cart.CartItems.Any(i => i.SKU == promo_U688))
                        {
                            //if (AllowedSKUToAddPromoSKU.Any(s => s == cart.CartItems[0].SKU))
                            if ((from a in cart.CartItems
                                 from b in AllowedSKUToAddPromoSKU
                                 where a.SKU == b
                                 select b).Count() > 0)
                            {
                                hlCart.DeleteItemsFromCart(new List<string>(new[] {promo_U688}), true);
                            }
                        }
                        return AddToCart(hlCart, Result, promo_U688);
                    }
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                //If they're trying to delete it, stop them.
                if (cart != null &&
                    (cart.CurrentItems[0].SKU == promoSku || cart.CurrentItems[0].SKU == promo_U688 ||
                     cart.CurrentItems[0].SKU == promoSkuU659))
                {
                    cart.CurrentItems.Clear();
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
            {
                if (hlCart != null && cart != null)
                {
                    //If the vlume drops below 1000, remove the promo sku if it's already there
                    if (hlCart.VolumeInCart < 1000)
                    {
                        if (cart.CartItems != null && cart.CartItems.Any(i => i.SKU == promoSku))
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku}), true);
                            Result.Result = RulesResult.Success;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    //If the vlume drops below 300, remove the promo sku if it's already there
                    if (hlCart.VolumeInCart < 300)
                    {
                        if (cart.CartItems != null && cart.CartItems.Any(i => i.SKU == promoSkuU659))
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSkuU659}), true);
                            Result.Result = RulesResult.Success;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                }
                if (cart.CartItems != null &&
                    (cart.CartItems.Any(i => i.SKU == promoSku) || cart.CartItems.Any(i => i.SKU == promo_U688) ||
                     cart.CartItems.Any(i => i.SKU == promoSkuU659)))
                {
                    //if it's the last sku in the cart, remove it.
                    if (cart.CartItems.Count == 1)
                    {
                        hlCart.DeleteItemsFromCart(new List<string>(new[] {promoSku, promo_U688, promoSkuU659}),
                                                   true);
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                        return Result;
                    }
                }
                if (GetQuantity(cart) == 0)
                {
                    //Remove the promo SKU as parent quantity is become zero
                    hlCart.DeleteItemsFromCart(new List<string>(new[] {promo_U688}), true);
                    Result.Result = RulesResult.Success;
                    cart.RuleResults.Add(Result);
                    return Result;
                }
                else if (GetQuantity(cart) > 0)
                {
                    //Remove promo SKU and add it again based on quantity
                    if (cart.CartItems != null && cart.CartItems.Any(i => i.SKU == promo_U688))
                    {
                        //if (AllowedSKUToAddPromoSKU.Any(s => s == cart.CartItems[0].SKU))
                        if ((from a in cart.CartItems
                             from b in AllowedSKUToAddPromoSKU
                             where a.SKU == b
                             select b).Count() > 0)
                        {
                            hlCart.DeleteItemsFromCart(new List<string>(new[] {promo_U688}), true);
                        }
                    }
                    return AddToCart(hlCart, Result, promo_U688);
                }
            }

            return Result;
        }

        private ShoppingCartRuleResult AddToCart(MyHLShoppingCart cart,
                                                 ShoppingCartRuleResult Result,
                                                 string SkuToBeAdded)
        {
            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (!allSkus.Keys.Contains(SkuToBeAdded))
            {
                if (Environment.MachineName.IndexOf("PROD") < 0)
                {
                    var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                    string message =
                        string.Format(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "NoPromoSku").ToString(), country.DisplayName,
                            SkuToBeAdded);
                    LoggerHelper.Error(message);
                    Result.Result = RulesResult.Feedback;
                    Result.AddMessage(message);
                    cart.RuleResults.Add(Result);
                    return Result;
                }

                return Result;
            }

            //Promo items must have inventory
            var catItem = CatalogProvider.GetCatalogItem(SkuToBeAdded, Country);
            WarehouseInventory warehouseInventory = null;
            if (null != cart.DeliveryInfo && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                if (catItem.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if ((warehouseInventory as WarehouseInventory_V01).QuantityAvailable > 0)
                    {
                        cart.AddItemsToCart(
                            new List<ShoppingCartItem_V01>(new[]
                                {new ShoppingCartItem_V01(0, SkuToBeAdded, quantity, DateTime.Now)}), true);
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
            //if we don't get items of type product,check whether items are in allowed SKUs
            if (AllowedSKUToAddPromoSKU.Any(s => s == cart.CartItems[0].SKU))
            {
                hasProduct = true;
            }

            return hasProduct;
        }

        private int GetQuantity(ShoppingCart_V02 cart)
        {
            // in cart items find the quantities of allowed sku to add promo sku
            int promo_Quantity = 0;
            quantity = 0;

            var items = from a in cart.CartItems
                        from b in AllowedSKUToAddPromoSKU
                        where a.SKU == b
                        select a;

            foreach (var item in items)
            {
                promo_Quantity += item.Quantity;
            }

            int remainder = 0;
            quantity = Math.DivRem(promo_Quantity, 3, out remainder);
            return quantity;
        }
    }
}