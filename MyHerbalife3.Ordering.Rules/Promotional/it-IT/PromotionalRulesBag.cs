using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.it_IT
{
    public class PromotionalRulesBag : MyHerbalifeRule, IShoppingCartRule, ISavedCartManagementRule, IPromoRule
    {
        private readonly DateTime MandatoryPromotionalStartDate = new DateTime(2014, 06, 23);
        private readonly string PromoSKU = "K126";

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

            switch (reason)
            {
                case ShoppingCartRuleReason.CartItemsBeingAdded:
                    // Avoid the DS adds the promo sku to the cart
                    if (cart.CurrentItems[0].SKU.Equals(PromoSKU))
                    {
                        cart.CurrentItems.Clear();
                    }
                    break;
                case ShoppingCartRuleReason.CartItemsBeingRemoved:
                    hlCart.IsPromoDiscarted = cart.CurrentItems[0].SKU.Equals(PromoSKU) ? true : hlCart.IsPromoDiscarted;
                    break;
                case ShoppingCartRuleReason.CartItemsAdded:
                case ShoppingCartRuleReason.CartItemsRemoved:
                    hlCart.IsPromoDiscarted = (reason == ShoppingCartRuleReason.CartItemsAdded) ? false : hlCart.IsPromoDiscarted;
                    if (!hlCart.IsPromoDiscarted)
                    {
                        result = CheckPromoInCart(cart, hlCart, result);    
                    }                    
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
                    result.Add(CheckPromoInCart(cart, hlCart, defaultResult));
                    ShoppingCartProvider.UpdateShoppingCart(hlCart);
                }
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
            if (cart == null || hlCart == null)
            {
                return result;
            }

            // If it's a saved cart or a copy from order, check the promo in cart
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (!hlCart.IsPromoDiscarted && (hlCart.IsSavedCart || hlCart.IsFromCopy))
                {
                    result.Add(CheckPromoInCart(cart, hlCart, defaultResult));
                }
            }
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Validates if the promo should be in cart.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="cart">The MyHL shopping cart.</param>
        /// <param name="result">The promo rule result.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult CheckPromoInCart(ShoppingCart_V02 shoppingCart,
                                                MyHLShoppingCart cart,
                                                ShoppingCartRuleResult result)
        {
            // Check the order type
            var session = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
            if (session.IsEventTicketMode)
            {
                return result;
            }

            // Check if promo sku should be in cart
            var promoInCart = cart.CartItems != null && cart.CartItems.Any(i => i.SKU.Equals(PromoSKU));
            var apfSkus = APFDueProvider.GetAPFSkuList();
            var applyForPromo = cart.CartItems != null &&
                                cart.CartItems.Any(
                                    i =>
                                    !apfSkus.Contains(i.SKU) &&
                                    !i.SKU.Equals(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku) &&
                                    !i.SKU.Equals(PromoSKU));

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

            if (!applyForPromo)
            {
                if (promoInCart)
                {
                    cart.DeleteItemsFromCart(new List<string> {PromoSKU}, true);
                }

                // Nothing to do
                result.Result = RulesResult.Success;
                cart.RuleResults.Add(result);
                return result;
            }

            //Check promo sku catalog info
            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (!allSkus.Keys.Contains(PromoSKU))
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
                        message = string.Format(globalResourceObject.ToString(), country.DisplayName, PromoSKU);
                    }
                    result.Result = RulesResult.Feedback;
                    result.Messages.Add(message);
                    cart.RuleResults.Add(result);
                    return result;
            }
                return result;
            }

            // Adding promo sku if possible
            var isRuleTime = IsRuleTime();
            if (!promoInCart && isRuleTime && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                WarehouseInventory warehouseInventory;
                var catItemPromo = CatalogProvider.GetCatalogItem(PromoSKU, Country);
                if (catItemPromo.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if (warehouseInventory != null)
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > 0)
                        {
                            cart.AddItemsToCart(
                                new List<ShoppingCartItem_V01>(new[]
                                    {new ShoppingCartItem_V01(0, PromoSKU, 1, DateTime.Now)}), true);
                            result.Result = RulesResult.Success;
                            cart.RuleResults.Add(result);
                        }
                    }
                }
            }
            else if (promoInCart && !isRuleTime)
            {
                // Remove the promo sku
                cart.DeleteItemsFromCart(new List<string> { PromoSKU }, true);
                result.Result = RulesResult.Success;
                cart.RuleResults.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Defines if the rule is enabled by time.
        /// </summary>
        /// <returns></returns>
        private bool IsRuleTime()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate))
            {
                DateTime beginDate;
                DateTime.TryParseExact(
                    HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate,
                    "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);

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
                DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate,
                                           "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

                if (DateUtils.GetCurrentLocalTime(Country).Date > endDate.Date)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
