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
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.Promotional.it_IT
{
    public class PromotionalRulesForDS : MyHerbalifeRule, IShoppingCartRule, IOrderManagementRule, IPromoRule
    {
        private readonly List<string> AlowedOrderSubTypes = "D2,A2,B2,C".Split(',').ToList();
        private readonly DateTime MandatoryPromotionalEndDate = new DateTime(2016, 05, 01);
        private readonly DateTime MandatoryPromotionalStartDate = new DateTime(2016, 02, 10);
        private readonly string PromotionWarehouse = "I2";
        private List<string> PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();

        public void PerformOrderManagementRules(ShoppingCart_V02 cart,
                                                Order_V01 order,
                                                string locale,
                                                OrderManagementRuleReason reason)
        {
            if (IsRuleTime() && reason == OrderManagementRuleReason.OrderBeingSubmitted)
            {
                var session = SessionInfo.GetSessionInfo(cart.DistributorID, locale);
                var hlCart = cart as MyHLShoppingCart;
                var promo = ShoppingCartProvider.GetEligibleForPromo(cart.DistributorID, cart.Locale);
                PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();

                var promoSkuInCart = promo != null ?
                    cart.CartItems.Where(i => i.SKU == promo.Sku).Select(i => i.SKU).ToList() :
                    cart.CartItems.Where(i => PromoSKUs.Contains(i.SKU)).Select(i => i.SKU).ToList();

                if (session != null && session.SelectedPaymentMethod > 1 && hlCart != null && promoSkuInCart.Count > 0)
                {
                    if (session.SelectedPaymentMethod > 1)
                    {
                        // Remove any promo sku in cart when order is unpaid
                        hlCart.DeleteItemsFromCart(promoSkuInCart, true);
                    }
                    else
                    {
                        // Validate if distributor is still eligible
                        if (promo == null && promoSkuInCart.Count > 0)
                        {
                            // Remove promo sku from cart
                            hlCart.DeleteItemsFromCart(promoSkuInCart, true);
                        }
                    }
                }
                if (session != null && session.SelectedPaymentMethod > 1)
                {
                    hlCart.IsPromoDiscarted = true;
                }
            }
        }

        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart,
                                                               List<string> skus,
                                                               ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
                {
                    RuleName = "Promotional Rules",
                    Result = RulesResult.Unknown
                };

            if (IsRuleTime())
            {
                if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
                {
                    PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();
                    var hlCart = cart as MyHLShoppingCart;
                    if (cart != null && hlCart != null)
                    {
                        if (skus.Any(i => PromoSKUs.Contains(i)))
                        {
                            hlCart.IsPromoDiscarted = true;
                            defaultResult.Result = RulesResult.Success;
                            cart.RuleResults.Add(defaultResult);
                        }
                    }
                }
            }
            result.Add(defaultResult);
            return result;
        }

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

            if (!IsRuleTime())
            {
                return result;
            }

            var promo = ShoppingCartProvider.GetEligibleForPromo(hlCart.DistributorID, hlCart.Locale);
            PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();

            switch (reason)
            {
                case ShoppingCartRuleReason.CartItemsBeingAdded:
                    // Avoid the DS adds any promo sku to cart
                    if ((promo != null && promo.Sku == cart.CurrentItems[0].SKU) || PromoSKUs.Any(i => i == cart.CurrentItems[0].SKU))
                    {
                        hlCart.IgnorePromoSKUAddition = true;
                        hlCart.CurrentItems.Clear();
                        result.Result = RulesResult.Success;
                        hlCart.RuleResults.Add(result);
                    }
                    break;
                case ShoppingCartRuleReason.CartCreated:
                case ShoppingCartRuleReason.CartItemsAdded:
                case ShoppingCartRuleReason.CartDeleted:
                case ShoppingCartRuleReason.CartWarehouseCodeChanged:
                case ShoppingCartRuleReason.CartCalculated:
                case ShoppingCartRuleReason.CartOrderSubTypeChanged:
                    if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartWarehouseCodeChanged) hlCart.IsPromoDiscarted = false;
                    result = CheckPromoInCart(hlCart, false, result);
                    break;
                case ShoppingCartRuleReason.CartPaymentOptionChanged:
                    result = CheckPromoInCart(hlCart, true, result);
                    break;
                case ShoppingCartRuleReason.CartItemsRemoved:
                    if (hlCart.CartItems.Count > 0) result = RemovePromoOnEmptyCart(hlCart, result);
                    break;
                case ShoppingCartRuleReason.CartClosed:
                    result = ClosePromo(hlCart, result);
                    break;
                case ShoppingCartRuleReason.CartItemsBeingRemoved:
                    if (IsRuleTime())
                    {
                        if (cart != null && hlCart != null)
                        {
                            if ((promo != null && promo.Sku == cart.CurrentItems[0].SKU) || PromoSKUs.Any(i => i == cart.CurrentItems[0].SKU))
                            {
                                hlCart.IsPromoDiscarted = true;
                                result.Result = RulesResult.Success;
                                hlCart.RuleResults.Add(result);
                            }
                        }
                    }
                    break;
            }
            return result;
        }

        private ShoppingCartRuleResult CheckPromoInCart(MyHLShoppingCart shoppingCart,
                                                        bool checkPayment,
                                                        ShoppingCartRuleResult result)
        {
            var promo = ShoppingCartProvider.GetEligibleForPromo(shoppingCart.DistributorID, shoppingCart.Locale);
            PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();

            var promoWarehouse = string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalWarehouse) ? 
                PromotionWarehouse : HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalWarehouse;

            var allSkus = CatalogProvider.GetAllSKU(Locale);
            if (promo != null && !allSkus.Keys.Contains(promo.Sku))
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
                        message = string.Format(globalResourceObject.ToString(), country.DisplayName, promo.Sku);
                    }
                    LoggerHelper.Error(message);
                    result.Result = RulesResult.Feedback;
                    result.AddMessage(message);
                    shoppingCart.RuleResults.Add(result);
                    return result;
                }
                return result;
            }

            result = RemovePromoOnEmptyCart(shoppingCart, result);

            int selectedPaymentMethod = 0;
            if (checkPayment)
            {
                var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                selectedPaymentMethod = session.SelectedPaymentMethod;
            }

            var promoSkuInCart = promo != null ? 
                shoppingCart.CartItems.Where(i => i.SKU == promo.Sku).Select(i => i.SKU).ToList() : 
                shoppingCart.CartItems.Where(i => PromoSKUs.Contains(i.SKU)).Select(i => i.SKU).ToList();

            if (promo == null && promoSkuInCart.Count == 0)
            {
                // Nothing to do
                LoggerHelper.Info("Not elegible for promo and not promo sku in cart");
                result.Result = RulesResult.Success;
                shoppingCart.RuleResults.Add(result);
                return result;
            }

            if (shoppingCart.Totals == null)
            {
                // Nothing to do
                LoggerHelper.Info("Not able to add or remove sku. Totals are null");
                result.Result = RulesResult.Failure;
                shoppingCart.RuleResults.Add(result);
                return result;
            }

            if (shoppingCart.CartItems.Count == 0)
            {
                // Just remove promo sku and nothing more to do
                LoggerHelper.Info("No items in cart to add promo");
                result.Result = RulesResult.Success;
                shoppingCart.RuleResults.Add(result);
                return result;
            }

            if (promoSkuInCart.Count > 0)
            {
                // Remove promoSkus from cart
                shoppingCart.DeleteItemsFromCart(promoSkuInCart, true);

                if (!AlowedOrderSubTypes.Contains(shoppingCart.OrderSubType) ||
                    (promo != null && !shoppingCart.CartItems.Any(i => !i.SKU.Equals(promo.Sku))) ||
                    (checkPayment && selectedPaymentMethod > 1) || shoppingCart.IsPromoDiscarted)
                {
                    // Just remove promo sku and nothing more to do
                    LoggerHelper.Info("Removed promo sku in cart");
                    result.Result = RulesResult.Success;
                    shoppingCart.RuleResults.Add(result);
                    return result;
                }
                else if (shoppingCart.IgnorePromoSKUAddition)
                {
                    shoppingCart.IgnorePromoSKUAddition = false;
                    result.Result = RulesResult.Failure;
                    string message = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "SKUCantBePurchased").ToString(), promoSkuInCart[0]);
                    result.AddMessage(message);
                    shoppingCart.RuleResults.Add(result);
            }
            }

            // Adding promo if it has inventory and if it is allowed
            if (promo != null && AlowedOrderSubTypes.Contains(shoppingCart.OrderSubType) && shoppingCart.CartItems.Any() &&
                shoppingCart.DeliveryInfo != null && !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) && shoppingCart.DeliveryInfo.WarehouseCode.Equals(promoWarehouse) &&
                ((checkPayment && selectedPaymentMethod <= 1) || !checkPayment) && !shoppingCart.IsPromoDiscarted && !APFDueProvider.IsAPFSkuPresent(shoppingCart.CartItems))
            {
                LoggerHelper.Info("Checking Inventory");
                WarehouseInventory warehouseInventory;
                var catItemPromo = CatalogProvider.GetCatalogItem(promo.Sku, Country);
                if (catItemPromo.InventoryList.TryGetValue(shoppingCart.DeliveryInfo.WarehouseCode,
                                                           out warehouseInventory))
                {
                    if (warehouseInventory != null)
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && warehouseInventory01.QuantityAvailable > promo.Quantity)
                        {
                            var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                            string message = "PromoInCart";
                            var globalResourceObject = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "PromoInCart");
                            if (globalResourceObject != null)
                            {
                                message = string.Format(globalResourceObject.ToString(), country.DisplayName);
                            }

                            shoppingCart.AddItemsToCart(
                                new List<ShoppingCartItem_V01>(new[]
                                    {new ShoppingCartItem_V01(0, promo.Sku, promo.Quantity, DateTime.Now)}), true);

                            if (result.Result != RulesResult.Failure)
                            {
                            result.Result = checkPayment ? RulesResult.Success : (shoppingCart.IsPromoNotified ? RulesResult.Feedback : RulesResult.Success);
                            result.AddMessage(message);
                            shoppingCart.RuleResults.Add(result);
                            }
                            shoppingCart.IsPromoNotified = false;
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

        private ShoppingCartRuleResult ClosePromo(MyHLShoppingCart shoppingCart, ShoppingCartRuleResult result)
        {
            var promo = ShoppingCartProvider.GetEligibleForPromo(shoppingCart.DistributorID, shoppingCart.Locale);
            PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();   
            
            if ((promo != null && shoppingCart.CartItems.Any(i => promo.Sku == i.SKU)) || 
                shoppingCart.CartItems.Any(i => PromoSKUs.Contains(i.SKU)))
            {
                if (promo != null)
                {
                    int storedShoppingCartId = 0;
                    string storedOrderNumber = string.Empty;
                    var updated = ShoppingCartProvider.SaveEligibleForPromo(promo, shoppingCart.ShoppingCartID,
                                                                            shoppingCart.OrderNumber,
                                                                            ref storedShoppingCartId,
                                                                            ref storedOrderNumber);
                    if (updated != null && updated.IsDisable && shoppingCart.ShoppingCartID == storedShoppingCartId)
                    {
                        result.Result = RulesResult.Success;
                        shoppingCart.RuleResults.Add(result);
                    }
                    else if (updated == null)
                    {
                        LoggerHelper.Info("Promo for DS was closed in another cart");
                        result.Result = RulesResult.Feedback;
                        shoppingCart.RuleResults.Add(result);
                    }
                }
                else
                {
                    LoggerHelper.Info("DS is not eligible for promo");
                    result.Result = RulesResult.Feedback;
                    shoppingCart.RuleResults.Add(result);
                }
            }
            return result;
        }

        private ShoppingCartRuleResult RemovePromoOnEmptyCart(MyHLShoppingCart shoppingCart, ShoppingCartRuleResult result)
        {
            var promo = ShoppingCartProvider.GetEligibleForPromo(shoppingCart.DistributorID, shoppingCart.Locale);
            PromoSKUs = HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Split(',').ToList();

            var promoSkuInCart = promo != null ?
                shoppingCart.CartItems.Where(i => i.SKU == promo.Sku).Select(i => i.SKU).ToList() :
                shoppingCart.CartItems.Where(i => PromoSKUs.Contains(i.SKU)).Select(i => i.SKU).ToList();

            shoppingCart.IsPromoNotified = !promoSkuInCart.Any();

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option != ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
            {
                // Remove promo if it is in cart
                if (promoSkuInCart.Any())
                {
                    shoppingCart.DeleteItemsFromCart(promoSkuInCart, true);
                    shoppingCart.IsPromoDiscarted = true;
                    result.Result = RulesResult.Success;
                    shoppingCart.RuleResults.Add(result);
                    return result;
                }
                else
                {
                    result.Result = RulesResult.Success;
                    shoppingCart.RuleResults.Add(result);
                    return result;
                }
            }

            if ((promo != null && !shoppingCart.CartItems.Any(i => promo.Sku != i.SKU) ) ||
                 (promo == null && !shoppingCart.CartItems.Any(i => PromoSKUs.Contains(i.SKU))) )
            {
                shoppingCart.DeleteItemsFromCart(promoSkuInCart, true);
                shoppingCart.IsPromoDiscarted = true;
                result.Result = RulesResult.Success;
                shoppingCart.RuleResults.Add(result);
                return result;
            }

            // Validate if the cart has only HFF and/or APF item
            var HFFsku = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku;
            var apfSkus = APFDueProvider.GetAPFSkuList();

            if (HFFsku != null && shoppingCart.CartItems.Any(i => i.SKU == HFFsku || apfSkus.Contains(i.SKU)))
            {
                if (promo != null && !shoppingCart.CartItems.Any(i => i.SKU != HFFsku && i.SKU != promo.Sku && !apfSkus.Contains(i.SKU)))
                {
                    shoppingCart.IsPromoDiscarted = true;
                    if (promoSkuInCart.Count > 0)
                    {
                        // Remove promoSkus from cart, cant apply for promo when have HFF SKU only
                        shoppingCart.DeleteItemsFromCart(promoSkuInCart, true);
                    }

                    // Just remove promo sku and nothing more to do
                    LoggerHelper.Info("No promo sku added when only HFF SKU is in cart");
                    result.Result = RulesResult.Success;
                    shoppingCart.RuleResults.Add(result);
                    return result;
                }
            }

            return result;
        }

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
            else
            {
                if (DateUtils.GetCurrentLocalTime(Country).Date > MandatoryPromotionalEndDate.Date)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
