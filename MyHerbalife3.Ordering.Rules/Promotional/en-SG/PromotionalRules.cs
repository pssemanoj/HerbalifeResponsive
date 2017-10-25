using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Utilities;

namespace MyHerbalife3.Ordering.Rules.Promotional.en_SG
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule, IPromoRule
    {
        private readonly List<string> AllowedSKUToAddPromoSKU_Z479 = new List<string> { "Z478", "Z476", "Z474" };
        private readonly List<string> AllowedSKUToAddPromoSKU_Z480 = new List<string> { "Z477", "Z475", "Z473" };
        private string promoSkuZ479 = "Z479";
        private string promoSkuZ480 = "Z480";
        private readonly DateTime MandatoryPromoStartDate = new DateTime(2013, 11, 09);
        private readonly DateTime MandatoryPromoEndDate = new DateTime(2020, 12, 31);

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
                    if (cart.CurrentItems[0].SKU == promoSkuZ479 || cart.CurrentItems[0].SKU == promoSkuZ480)
                    {
                        cart.CurrentItems.Clear();
                    }
                    break;

                case ShoppingCartRuleReason.CartItemsAdded:
                case ShoppingCartRuleReason.CartItemsRemoved:
                    result = CheckPromoIncart(hlCart, result, AllowedSKUToAddPromoSKU_Z479, promoSkuZ479);
                    result = CheckPromoIncart(hlCart, result, AllowedSKUToAddPromoSKU_Z480, promoSkuZ480);
                    break;
            }

            return result;
        }

        #endregion

        #region IPromoRule implementation
        
        /// <summary>
        /// Perfomrs promo rule process,
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="skus">The skus.</param>
        /// <param name="reason">The rule reason.</param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };

            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            if (!IsRuleTime())
            {
                return result;
            }

            // If it's a saved cart or a copy from order, check the promo in cart
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (!hlCart.IsPromoDiscarted && (hlCart.IsSavedCart || hlCart.IsFromCopy))
                {
                    result.Add(CheckPromoIncart(hlCart, defaultResult, AllowedSKUToAddPromoSKU_Z479, promoSkuZ479));
                    result.Add(CheckPromoIncart(hlCart, defaultResult, AllowedSKUToAddPromoSKU_Z480, promoSkuZ480));
                }
            }
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Validates if the promo should be in cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="result">The promo rule result.</param>
        /// <param name="requiredSkus">The required sku list.</param>
        /// <param name="promoSku">The promo sku.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult CheckPromoIncart(MyHLShoppingCart cart, ShoppingCartRuleResult result, List<string> requiredSkus, string promoSku)
        {
            // Check if promo sku should be in cart
            var quantityToAdd = cart.CartItems.Where(i => requiredSkus.Contains(i.SKU)).Sum(i => i.Quantity);
            var promoInCart = cart.CartItems.FirstOrDefault(i => i.SKU.Equals(promoSku));

            // If not elegibe for promo then nothing to do
            if (quantityToAdd == 0)
            {
                if (promoInCart != null)
                {
                    cart.DeleteItemsFromCart(new List<string> { promoSku }, true);
                }

                // Nothing to do
                result.Result = RulesResult.Success;
                cart.RuleResults.Add(result);
                return result;
            }

            // Define the quantity to add
            if (promoInCart != null)
            {
                if (promoInCart.Quantity == quantityToAdd)
                {
                    // Check if nothing to do
                    result.Result = RulesResult.Success;
                    cart.RuleResults.Add(result);
                    return result;
                }
                if (promoInCart.Quantity > quantityToAdd)
                {
                    // Remove the promo sku if quantity to add is minor than the quantity in cart
                    cart.DeleteItemsFromCart(new List<string> { promoSku }, true);
                }
                else
                {
                    // Change item quantity adding the excess to the existent sku
                    quantityToAdd -= promoInCart.Quantity;
                }
            }

            // Check promo items in catalog
            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (!allSkus.Keys.Contains(promoSku))
            {
                if (Environment.MachineName.IndexOf("PROD", StringComparison.Ordinal) < 0)
                {
                    var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                    string message = "NoPromoSku";
                    var globalResourceObject =
                        HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                            "NoPromoSku");
                    if (globalResourceObject != null)
                    {
                        message = string.Format(globalResourceObject.ToString(), country.DisplayName, promoSku);
                    }
                    result.Result = RulesResult.Feedback;
                    result.AddMessage(message);
                    cart.RuleResults.Add(result);
                }
                return result;
            }

            //Promo items must have inventory
            var catItem = CatalogProvider.GetCatalogItem(promoSku, Country);
            if (cart.DeliveryInfo != null && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                WarehouseInventory warehouseInventory = null;
                if (catItem.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if (warehouseInventory != null)
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > 0)
                        {
                            cart.AddItemsToCart(
                                new List<ShoppingCartItem_V01>(new[]
                                    {new ShoppingCartItem_V01(0, promoSku, quantityToAdd, DateTime.Now)}), true);
                            result.Result = RulesResult.Success;
                            cart.RuleResults.Add(result);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Defines if the rule is enabled by the time.
        /// </summary>
        /// <returns></returns>
        private bool IsRuleTime()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate))
            {
                DateTime beginDate;
                DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate,
                                       "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);

                if (DateUtils.GetCurrentLocalTime(Country).Date < beginDate.Date)
                {
                    return false;
                }
            }
            else
            {
                if (DateUtils.GetCurrentLocalTime(Country).Date < MandatoryPromoStartDate.Date)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate))
            {
                DateTime endDate;
                DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate,
                                       "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

                if (DateUtils.GetCurrentLocalTime(Country).Date > endDate.Date)
                {
                    return false;
                }
            }
            else
            {
                if (DateUtils.GetCurrentLocalTime(Country).Date > MandatoryPromoEndDate.Date)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
