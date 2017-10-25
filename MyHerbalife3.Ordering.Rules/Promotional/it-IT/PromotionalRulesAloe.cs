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

namespace MyHerbalife3.Ordering.Rules.Promotional.it_IT
{
    public class PromotionalRulesAloe : MyHerbalifeRule, IShoppingCartRule
    {
        private const int PromoQuantity = 1;

        private readonly List<string> AllowedSKUToAddPromoSKU =
            "2561,2562,2563,2564,2565,2566,0730,0736,0737,0738,0739,0740".Split(',').ToList();

        private readonly DateTime PromotionalEndDate = new DateTime(2013, 12, 10);

        private readonly decimal PromotionalRequiredVolumePoints = 50;
        public readonly string PromotionalSku = "X946";

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

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate))
            {
                DateTime beginDate;
                DateTime.TryParseExact(
                    HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate,
                    "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);

                if (DateUtils.GetCurrentLocalTime(Country).Date < beginDate.Date)
                {
                    // Promotion have not started
                    LoggerHelper.Info("Promo not started");
                    result.Result = RulesResult.Success;
                    result.AddMessage("PromoOutOfDate");
                    cart.RuleResults.Add(result);
                    return result;
                }
            }

            bool isPromoEnded = DateUtils.GetCurrentLocalTime(Country).Date > PromotionalEndDate;

            if (isPromoEnded || (AllowedSKUToAddPromoSKU.Count > 0 &&
                                 PromotionalRequiredVolumePoints > 0 &&
                                 !string.IsNullOrEmpty(PromotionalSku)))
            {
                if (isPromoEnded)
                {
                    // Remove the promo sku if it exists in cart
                    if (hlCart.CartItems.Any(i => i.SKU.Equals(PromotionalSku)))
                    {
                        hlCart.DeleteItemsFromCart(new List<string> {PromotionalSku}, true);
                    }

                    LoggerHelper.Info("Promo sku removed for ended promo");
                    result.Result = RulesResult.Success;
                    result.AddMessage("PromoOutOfDate");
                    cart.RuleResults.Add(result);
                    return result;
                }

                switch (reason)
                {
                    case ShoppingCartRuleReason.CartItemsBeingAdded:
                    case ShoppingCartRuleReason.CartItemsBeingRemoved:
                        // Avoid the DS adds or removes the promo sku from cart
                        if (cart.CurrentItems[0].SKU.Equals(PromotionalSku))
                        {
                            cart.CurrentItems.Clear();
                        }
                        break;
                    case ShoppingCartRuleReason.CartItemsAdded:
                    case ShoppingCartRuleReason.CartItemsRemoved:
                        result = CheckPromoInCart(cart, hlCart, result);
                        break;
                }
            }
            else
            {
                var message =
                    string.Format("Rule not executed: IsEnded={0}, cart {1}, AllowedSku quantity {2}, rule reason:{3}",
                                  isPromoEnded.ToString(), (cart == null ? "is null" : "is not null"),
                                  AllowedSKUToAddPromoSKU.Count.ToString(), reason.ToString());
                LoggerHelper.Info(message);
            }
            return result;
        }

        private bool IsElegibleForPromo(ShoppingCart_V02 cart, MyHLShoppingCart hlCart, out decimal volume)
        {
            var isElegible = false;
            volume = 0;

            LoggerHelper.Info(
                string.Format("CheckPromoInCart.ShoppingCart V02 items {0}",
                              string.Concat(cart.CartItems.Select(i => string.Format("{0},", i.SKU)))));
            LoggerHelper.Info(
                string.Format("CheckPromoInCart.MyHLShoppingCart items {0}",
                              string.Concat(hlCart.CartItems.Select(i => string.Format("{0},", i.SKU)))));
            LoggerHelper.Info(
                string.Format("AllowedSKUToAddPromoSKU {0}",
                              string.Concat(AllowedSKUToAddPromoSKU.Select(i => string.Format("{0},", i)))));

            var cartItemsV02 = (from a in AllowedSKUToAddPromoSKU
                                join b in cart.CartItems on a equals b.SKU
                                select
                                    new
                                        {
                                            Sku = b.SKU,
                                            Qty = b.Quantity,
                                            Volume = CatalogProvider.GetCatalogItem(b.SKU, "IT").VolumePoints
                                        }).ToList();

            if (cartItemsV02.Any())
            {
                isElegible = true;
                volume = cartItemsV02.Sum(oi => oi.Qty*oi.Volume);
            }
            else
            {
                var shoppingcartItemsV02 = (from a in AllowedSKUToAddPromoSKU
                                            join b in hlCart.ShoppingCartItems on a equals b.SKU
                                            select
                                                new {Sku = b.SKU, Qty = b.Quantity, Volume = b.CatalogItem.VolumePoints})
                    .ToList();

                if (shoppingcartItemsV02.Any())
                {
                    isElegible = true;
                    volume = shoppingcartItemsV02.Sum(oi => oi.Qty*oi.Volume);
                }
                else
                {
                    LoggerHelper.Info("No Items in Cart or MyHLCart. Rule fails");
                }
            }

            return isElegible;
        }

        private ShoppingCartRuleResult CheckPromoInCart(ShoppingCart_V02 shoppingCart,
                                                        MyHLShoppingCart cart,
                                                        ShoppingCartRuleResult result)
        {
            LoggerHelper.Info(string.Format("Entered CheckPromoInCart."));
            // Define the promo quantity to add
            decimal volumeInCartForPromo = 0;
            var applyForPromo = IsElegibleForPromo(shoppingCart, cart, out volumeInCartForPromo);

            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (applyForPromo && !allSkus.Keys.Contains(PromotionalSku))
            {
                LoggerHelper.Info("No promo sku in catalog");
                if (Environment.MachineName.IndexOf("PROD", StringComparison.Ordinal) < 0)
                {
                    var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                    string message = "NoPromoSku";
                    var globalResourceObject =
                        HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                            "NoPromoSku");
                    if (globalResourceObject != null)
                    {
                        message = string.Format(globalResourceObject.ToString(), country.DisplayName, PromotionalSku);
                    }
                    LoggerHelper.Error(message);
                    result.Result = RulesResult.Feedback;
                    result.AddMessage(message);
                    cart.RuleResults.Add(result);
                    return result;
                }
                return result;
            }

            // Define the promo quantity to add
            var elegibleQuantity = Convert.ToInt32(Math.Truncate(volumeInCartForPromo/PromotionalRequiredVolumePoints))*
                                   PromoQuantity;
            var promoSkuInCart = (from c in cart.CartItems where c.SKU.Equals(PromotionalSku) select c).FirstOrDefault();

            if (elegibleQuantity == 0 && promoSkuInCart == null)
            {
                // Nothing to do
                LoggerHelper.Info("Not elegible for promo and not promo sku in cart");
                result.Result = RulesResult.Success;
                cart.RuleResults.Add(result);
                return result;
            }

            if (promoSkuInCart != null)
            {
                if (promoSkuInCart.Quantity == elegibleQuantity)
                {
                    // Check if nothing to do
                    LoggerHelper.Info("Not quantity change to do in promo sku");
                    result.Result = RulesResult.Success;
                    cart.RuleResults.Add(result);
                    return result;
                }
                if (promoSkuInCart.Quantity > elegibleQuantity)
                {
                    // Remove the promo sku if quantity to add is minor than the quantity in cart
                    LoggerHelper.Info("Removing promo sku from cart");
                    cart.DeleteItemsFromCart(new List<string> {PromotionalSku}, true);
                    if (elegibleQuantity == 0)
                    {
                        result.Result = RulesResult.Success;
                        cart.RuleResults.Add(result);
                        return result;
                    }
                }
                else
                {
                    // Change item quantity adding the excess to the existent sku
                    elegibleQuantity -= promoSkuInCart.Quantity;
                }
            }

            // Adding promo if it has inventory and if it is allowed
            if (applyForPromo && cart.DeliveryInfo != null && !string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))
            {
                LoggerHelper.Info("Checking Inventory");
                WarehouseInventory warehouseInventory;
                var catItemPromo = CatalogProvider.GetCatalogItem(PromotionalSku, Country);
                if (catItemPromo.InventoryList.TryGetValue(cart.DeliveryInfo.WarehouseCode, out warehouseInventory))
                {
                    if (warehouseInventory != null)
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > elegibleQuantity)
                        {
                            cart.AddItemsToCart(
                                new List<ShoppingCartItem_V01>(new[]
                                    {new ShoppingCartItem_V01(0, PromotionalSku, elegibleQuantity, DateTime.Now)}), true);
                            result.Result = RulesResult.Success;
                            cart.RuleResults.Add(result);
                        }
                        else
                        {
                            LoggerHelper.Info("Warehouse information is null or not enough quantity is available");
                        }
                    }
                }
                else
                {
                    LoggerHelper.Info("Not inventory list was gotten for promo sku");
                }
            }
            return result;
        }
    }
}