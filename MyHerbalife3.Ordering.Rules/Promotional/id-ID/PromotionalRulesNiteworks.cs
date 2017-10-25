using System.Globalization;
using System.Web;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.id_ID
{
    public class PromotionalRulesNiteworks : MyHerbalifeRule, IShoppingCartRule, ISavedCartManagementRule
    {
        private class RequiredProduct
        {
            public string Sku { get; set; }
            public int Quantity { get; set; }
        }

        private readonly RequiredProduct RequiredSku = new RequiredProduct { Sku = "475P", Quantity = 1 };
        private RequiredProduct PromoSku = new RequiredProduct { Sku = "476P", Quantity = 1 };
        private readonly DateTime MandatoryPromotionalStartDate = new DateTime(2015, 10, 15);

        #region IShoppingCartRule implementation

        /// <summary>
        /// Processes the cart.
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

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            if (!IsRuleTime())
            {
                return result;
            }

            switch (reason)
            {
                case ShoppingCartRuleReason.CartItemsBeingAdded:
                    if (cart.CurrentItems[0].SKU.Equals(PromoSku.Sku))
                    {
                        cart.CurrentItems.Clear();
                    }
                    else if (cart.CurrentItems[0].SKU.Equals(RequiredSku.Sku))
                    {
                        hlCart.PromoSkuDiscarted = string.Empty;
                        return result;
                    }
                    break;
                case ShoppingCartRuleReason.CartItemsBeingRemoved:
                    if (cart.CurrentItems[0].SKU.Equals(PromoSku.Sku))
                    {
                        hlCart.PromoSkuDiscarted = PromoSku.Sku;
                        return result;
                    }
                    break;
                case ShoppingCartRuleReason.CartRuleFailed:
                case ShoppingCartRuleReason.CartItemsAdded:
                    result = CheckPromoInCart(cart, hlCart, result, false);
                    break;
                case ShoppingCartRuleReason.CartItemsRemoved:
                    result = CheckPromoInCart(cart, hlCart, result, true);
                    break;
            }
            return result;
        }

        #endregion

        #region ISavedCartManagementRule implementation

        /// <summary>
        /// Porcess the Saved Cart rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessSavedCartManagementRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Saved Cart Management Rules",
                Result = RulesResult.Unknown
            };

            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            // Each time a saved cart or a copy from order is retrieved, check the promo in cart
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (hlCart.IsSavedCart || hlCart.IsFromCopy)
                {
                    var isPromoInCart = cart.CartItems != null && cart.CartItems.Any(i => i.SKU.Equals(PromoSku.Sku));
                    if (isPromoInCart)
                    {
                        hlCart.DeleteItemsFromCart(new List<string> { PromoSku.Sku }, true);
                    }
                    result.Add(CheckPromoInCart(cart, hlCart, defaultResult, false));
                    ShoppingCartProvider.UpdateShoppingCart(hlCart);
                }
            }
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
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate))
            {
                DateTime beginDate;
                if (!DateTime.TryParseExact(
                    HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate,
                    "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate))
                {
                    beginDate = MandatoryPromotionalStartDate;
                }
                if (DateUtils.GetCurrentLocalTime(Country).Date < beginDate.Date)
                {
                    return false;
                }
            }
            else
            {
                if (DateUtils.GetCurrentLocalTime(Country).Date < MandatoryPromotionalStartDate.Date)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate))
            {
                DateTime endDate;
                if (DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate,
                                           "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                {
                    if (DateUtils.GetCurrentLocalTime(Country).Date > endDate.Date)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validates if the promo should be in cart.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="cart">The MyHL shopping cart.</param>
        /// <param name="result">The promo rule result.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult CheckPromoInCart(ShoppingCart_V02 shoppingCart, MyHLShoppingCart cart, ShoppingCartRuleResult result, bool removed)
        {
            // Check the order type
            var session = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
            if (session.IsEventTicketMode)
            {
                return result;
            }

            // Check if APF standalone order
            if (APFDueProvider.hasOnlyAPFSku(cart.CartItems, cart.Locale))
            {
                return result;
            }

            // Check HFF standalone
            if (ShoppingCartProvider.IsStandaloneHFF(cart.ShoppingCartItems))
            {
                return result;
            }

            // Check if promo sku has been removed
            if (removed && (cart.CartItems.Count == 0 || (!string.IsNullOrEmpty(cart.PromoSkuDiscarted) && cart.PromoSkuDiscarted.Contains(PromoSku.Sku))))
            {
                result.Result = RulesResult.Success;
                return result;
            }

            // Check if promo sku should be in cart
            var isRuleTime = IsRuleTime();
            var isPromoInCart = cart.CartItems != null && cart.CartItems.Any(i => i.SKU.Equals(PromoSku.Sku));
            var promoInCart = isPromoInCart ? cart.CartItems.FirstOrDefault(i => i.SKU.Equals(PromoSku.Sku)) : null;
            var applyForPromo = cart.CartItems != null && cart.CartItems.Any(i => i.SKU.Equals(RequiredSku.Sku) && i.Quantity >= RequiredSku.Quantity);

            // Define the quantity to add
            var requiredSkuQuantityInCart = cart.CartItems == null ? 0 : cart.CartItems.Where(i => i.SKU == RequiredSku.Sku).Sum(i => i.Quantity);
            var remainder = 0;
            var allowedQuantity = Math.DivRem(requiredSkuQuantityInCart, RequiredSku.Quantity, out remainder);

            if (!applyForPromo || !isRuleTime)
            {
                if (isPromoInCart)
                {
                    cart.DeleteItemsFromCart(new List<string> { PromoSku.Sku }, true);
                    var message = "HTML|PromotionalSKU087PRemoved.html";
                    result.AddMessage(message);
                    result.Result = RulesResult.Feedback;
                }

                // Nothing to do
                cart.RuleResults.Add(result);
                return result;
            }
            
            if (promoInCart != null)
            {
                if (promoInCart.Quantity == allowedQuantity)
                {
                    // Nothing to do
                    result.Result = RulesResult.Success;
                    cart.RuleResults.Add(result);
                    return result;
                }
                if (promoInCart.Quantity > allowedQuantity)
                {
                    // Remove the promo sku if quantity to add is minor than the quantity in cart
                    cart.DeleteItemsFromCart(new List<string> { PromoSku.Sku }, true);
                    var message = "HTML|PromotionalSKU087PRemoved.html";
                    result.AddMessage(message);
                    result.Result = RulesResult.Feedback;
                    isPromoInCart = false;
                }
                else
                {
                    if (cart.CurrentItems != null && cart.CurrentItems.Count > 0 && cart.CurrentItems[0].SKU.Equals(PromoSku.Sku))
                    {
                        allowedQuantity = promoInCart.Quantity;
                        cart.IsPromoNotified = false;
                    }
                    cart.DeleteItemsFromCart(new List<string> { PromoSku.Sku }, true);
                    isPromoInCart = false;
                    cart.IsPromoNotified = true;
                }
            }
            else
            {
                cart.IsPromoNotified = true;
            }

            // Check promo sku  in catalog info
            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (!allSkus.Keys.Contains(PromoSku.Sku))
            {
                if (Environment.MachineName.IndexOf("PROD", StringComparison.Ordinal) < 0)
                {
                    var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                    string message = "NoPromoSku";
                    var globalResourceObject = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "NoPromoSku");
                    if (globalResourceObject != null)
                    {
                        message = string.Format(globalResourceObject.ToString(), country.DisplayName, PromoSku.Sku);
                    }
                    result.Result = RulesResult.Feedback;
                    result.AddMessage(message);
                    cart.RuleResults.Add(result);
                    return result;
                }
                return result;
            }

            // Adding promo sku if possible
            if (!isPromoInCart && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                WarehouseInventory warehouseInventory;
                var catItemPromo = CatalogProvider.GetCatalogItem(PromoSku.Sku, Country);
                if (catItemPromo.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if (warehouseInventory != null)
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > allowedQuantity)
                        {
                            if (cart.IsPromoNotified)
                            {
                                var message = "HTML|PromotionalSku087PAdded.html";
                                result.AddMessage(message);
                                result.Result = RulesResult.Feedback;
                                cart.IsPromoNotified = false;
                            }
                            else
                            {
                                result.Result = RulesResult.Success;
                            }
                            cart.AddItemsToCart(new List<ShoppingCartItem_V01>(new[] { new ShoppingCartItem_V01(0, PromoSku.Sku, allowedQuantity, DateTime.Now) }), true);
                            cart.RuleResults.Add(result);
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
