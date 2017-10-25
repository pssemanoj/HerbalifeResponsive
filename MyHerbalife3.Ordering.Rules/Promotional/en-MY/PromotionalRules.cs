using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Utilities;

namespace MyHerbalife3.Ordering.Rules.Promotional.en_MY
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule, IPromoRule
    {
        private const string RequiredSKU = "K301";
        private const string PromoSKU = "K461";
        private DateTime dtBeginDate = new DateTime(2015, 02, 13);
        private DateTime dtEndDate = new DateTime(2020, 12, 31);
        private SessionInfo CurrentSession = null;

        #region IShoppingCartRule implementation

        /// <summary>
        /// Process shopping cart rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        /// <summary>
        /// Performs shopping cart rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            var hlCart = cart as MyHLShoppingCart;
            CurrentSession = SessionInfo.GetSessionInfo(DistributorProfileModel.Id, Locale);
            if (cart == null || hlCart == null || string.IsNullOrEmpty(RequiredSKU) || string.IsNullOrEmpty(PromoSKU) || !IsRuleTime())
            {
                return result;
            }

            switch (reason)
            {
                case ShoppingCartRuleReason.CartRetrieved: case ShoppingCartRuleReason.CartCreated:
                    if (cart.CartItems.Any(i => i.SKU.Equals(PromoSKU)) && hlCart.IsPromoDiscarted == false)
                        hlCart.IsPromoDiscarted = true;
                    break;
                case ShoppingCartRuleReason.CartItemsBeingAdded:
                    // Avoid the DS adds the promo sku to the cart
                    if (cart.CurrentItems[0].SKU.Equals(PromoSKU))
                    {
                        if (hlCart.IsPromoDiscarted && hlCart.OnCheckout) // Have promo added 
                        {
                            if (cart.CartItems.Any(i => i.SKU.Equals(PromoSKU))) // Increasing the Promo SKU
                            {
                                int requiredSkuQty = cart.CartItems.FirstOrDefault(i => i.SKU.Equals(RequiredSKU)).Quantity;
                                int promoSkuQty = cart.CartItems.FirstOrDefault(i => i.SKU.Equals(PromoSKU)).Quantity + cart.CurrentItems[0].Quantity;
                                if (promoSkuQty > requiredSkuQty)
                                {
                                    cart.CurrentItems.Clear();
                                    result.Result = RulesResult.Failure;
                                    result.AddMessage(string.Empty);
                                    cart.RuleResults.Add(result);
                                }
                            }
                        }
                        else
                        {
                            cart.CurrentItems.Clear();
                            result.Result = RulesResult.Failure;
                            result.AddMessage(string.Empty);
                            cart.RuleResults.Add(result);
                        }
                    }
                    else if (cart.CurrentItems[0].SKU.Equals(RequiredSKU) && hlCart.IsPromoDiscarted && !cart.CartItems.Any(i => i.SKU.Equals(RequiredSKU)))
                        hlCart.PromoQtyToAdd = cart.CurrentItems[0].Quantity;
                    break;
                case ShoppingCartRuleReason.CartItemsAdded:
                    if (cart.CurrentItems.Any(i => i.SKU.Equals(RequiredSKU)))
                    {
                        if (ApplyForPromo(ref cart, ref hlCart))
                        {
                            // Display PopUp - (Ordering/Controls/promotion_MY.ascx)
                            hlCart.DisplayPromo = true;
                        }
                        result.Result = RulesResult.Success;
                        cart.RuleResults.Add(result);
                    }
                    else if (cart.CurrentItems.Any(i => i.SKU.Equals(PromoSKU)))
                    {
                        result.Result = RulesResult.Success;
                        cart.RuleResults.Add(result);
                    }
                    break;
                case ShoppingCartRuleReason.CartItemsRemoved:
                    if (!cart.CartItems.Any(i => i.SKU.Equals(RequiredSKU)) && cart.CartItems.Any(i => i.SKU.Equals(PromoSKU)))
                    {
                        // Remove PromoSKU if RequiredSKU is removed
                        hlCart.DeleteItemsFromCart(new List<string>(new[] { PromoSKU }), true);
                        hlCart.IsPromoDiscarted = false;
                        result.Result = RulesResult.Success;
                        cart.RuleResults.Add(result);
                    }
                    else if (!cart.CartItems.Any(i => i.SKU.Equals(PromoSKU)) && hlCart.IsPromoDiscarted)
                        hlCart.IsPromoDiscarted = false;

                    break;
            }
            return result;
        }

        #endregion

        #region IPromoRule implementation
        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };

            var hlCart = cart as MyHLShoppingCart;

            // Add PromoSKU to Cart
            var products = new List<ShoppingCartItem_V01>(new[] { new ShoppingCartItem_V01(0, PromoSKU, hlCart.PromoQtyToAdd, DateTime.Now) });
            try
            {
                hlCart.AddItemsToCart(products, true);
                hlCart.IsPromoDiscarted = true;
                defaultResult.Result = RulesResult.Success;
            }
            catch { defaultResult.Result = RulesResult.Failure; }
            finally { hlCart.PromoQtyToAdd = 0; }

            result.Add(defaultResult);
            return result;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Defines if the rule is enabled by time.
        /// </summary>
        /// <returns></returns>
        private bool IsRuleTime()
        {
            //// Validate PromotionalBeginDate
            if (DateUtils.GetCurrentLocalTime(Country).Date < dtBeginDate)
            {
                return false;
            }

            //// Validate PromotionalEndDate
            if (DateUtils.GetCurrentLocalTime(Country).Date > dtEndDate)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate if the RequiredSKU and PromoSkU is available in Inventory.
        /// </summary>
        /// <returns></returns>
        private bool ApplyForPromo(ref ShoppingCart_V02 cart, ref MyHLShoppingCart hlCart)
        {
            var allSkus = CatalogProvider.GetAllSKU(Locale);

            // Validate if SKUs are available
            if (!allSkus.ContainsKey(RequiredSKU) || !allSkus.ContainsKey(PromoSKU))
                return false; // No requiredSKU and/or PromoSKU available in catalog

            // Validatation - Recalculate PromoSKU if the RequiredSKU has been recalculate (remove items)
            if (hlCart.CartItems.Any(i => i.SKU.Equals(PromoSKU)) && hlCart.PromoQtyToAdd > 0)
            {
                var sciRequiredSKU = hlCart.CartItems.FirstOrDefault(i => i.SKU.Equals(RequiredSKU));
                var sciPromoSKU = hlCart.CartItems.FirstOrDefault(i => i.SKU.Equals(PromoSKU));

                if (sciPromoSKU.Quantity > sciRequiredSKU.Quantity)
                {
                    // Recalculate - Remove PromoSKU
                    hlCart.DeleteItemsFromCart(new List<string>(new[] { PromoSKU }), true);
                    hlCart.AddItemsToCart(
                                new List<ShoppingCartItem_V01>(new[] { new ShoppingCartItem_V01(0, PromoSKU, sciRequiredSKU.Quantity, DateTime.Now) }), true);
                    hlCart.IsPromoDiscarted = true;
                }

                hlCart.PromoQtyToAdd = 0;
                return false;
            }

            // Validate if PromoSKU Qty is available in inventory
            WarehouseInventory warehouseInventory;
            var catItemPromo = CatalogProvider.GetCatalogItem(PromoSKU, Country);
            if (catItemPromo.InventoryList.TryGetValue(hlCart.DeliveryInfo.WarehouseCode, out warehouseInventory))
            {
                if (warehouseInventory != null)
                {
                    var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                    if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > 0)
                    {
                        hlCart.PromoQtyToAdd = cart.CurrentItems.FirstOrDefault(i => i.SKU.Equals(RequiredSKU)).Quantity;
                        hlCart.PromoQtyToAdd = warehouseInventory01.QuantityAvailable >= hlCart.PromoQtyToAdd ? hlCart.PromoQtyToAdd : warehouseInventory01.QuantityAvailable;
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion


    }
}