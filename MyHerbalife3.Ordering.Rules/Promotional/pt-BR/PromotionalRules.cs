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

namespace MyHerbalife3.Ordering.Rules.Promotional.pt_BR
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly DateTime MandatoryPromotionalStartDate = new DateTime(2013, 08, 23);
        private List<string> AllowedSKUToAddPromoSKU = new List<string> {"0020"};
        private string promoSku = "X250";

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

        /// <summary>
        ///     Performs the rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="Result">The result.</param>
        /// <returns></returns>
        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (!IsRuleTime())
            {
                return Result;
            }

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
            else if (reason == ShoppingCartRuleReason.CartItemsAdded)
            {
                var hlCart = cart as MyHLShoppingCart;
                int quantity0020 = 0;

                if (cart == null || hlCart == null)
                {
                    return Result;
                }
                if (cart != null && cart.CurrentItems[0].SKU == promoSku)
                {
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(string.Empty);
                    cart.RuleResults.Add(Result);
                    return Result;
                }
                if (cart.CartItems.Any(i => i.SKU == "0020"))
                {
                    quantity0020 = cart.CartItems.Find(m => m.SKU == "0020").Quantity;
                }
                if (quantity0020 == 0)
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
                else if (quantity0020 > 0)
                {
                    //check, cart contain the promo sku?
                    if (cart.CartItems.Any(i => i.SKU == promoSku))
                    {
                        return Result; //SKU already present - exit.
                    }
                    return AddToCart(hlCart, Result);
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
            {
                var hlCart = cart as MyHLShoppingCart;

                //If there are no sku to get the promo remaining in the cart, remove the promo sku if it's already there
                if (cart.CartItems.Any(i => i.SKU == promoSku))
                {
                    var catItems =
                        CatalogProvider.GetCatalogItems(
                            (from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
                    if (catItems != null)
                    {
                        bool has0020added = cart.CartItems.Any(i => i.SKU == "0020");
                        if (!has0020added)
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

        /// <summary>
        ///     Adds to cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="Result">The result.</param>
        /// <returns></returns>
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
                                {new ShoppingCartItem_V01(0, promoSku, 1, DateTime.Now)}), true);
                        Result.Result = RulesResult.Success;
                        cart.RuleResults.Add(Result);
                    }
                }
            }

            return Result;
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
            return true;
        }
    }
}