using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.en_US
{
    public class CartIntegrityRulesPresSummit : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const string PresSummitSkusCacheKey = "PresSummitSkus";
        private const int PresSummitCacheMinutes = 60 * 6;
        private const int PresidentSummitEventId = 1160;
        private const string Warehouse = "95";
        private readonly DateTime MandatoryPromotionalStartDate = new DateTime(2015, 01, 19);
        private readonly DateTime MandatoryPromotionalEndDate = new DateTime(2015, 02, 25);

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
                {
                    RuleName = RuleName, 
                    Result = RulesResult.Unknown
                };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                // Checking standalone skus
                result = CheckPresidentSummitSkus(cart, result);
            }
            else if (reason == ShoppingCartRuleReason.CartWarehouseCodeChanged)
            {
                // Valid skus in cart
                result = CheckPresidentSummitWarehouse(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CheckPresidentSummitSkus(ShoppingCart_V01 shoppingCart,
                                                                ShoppingCartRuleResult ruleResult)
        {
            if (shoppingCart != null)
            {
                var presidentSummitSkus = GetPresidentSummitSkus();
                var cart = shoppingCart as MyHLShoppingCart;
                if (presidentSummitSkus.Contains(shoppingCart.CurrentItems[0].SKU))
                {
                    if (!DistributorOrderingProfileProvider.IsEventQualified(PresidentSummitEventId, Locale))
                    {
                        var message = "SKUNotAvailable";
                        var globalResourceObject =
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                "SKUNotAvailable");
                        if (globalResourceObject != null)
                        {
                            message = string.Format(globalResourceObject.ToString(), shoppingCart.CurrentItems[0].SKU);
                        }
                        ruleResult.AddMessage(message);
                        ruleResult.Result = RulesResult.Failure;
                    }
                    else
                    {
                        if (cart != null && cart.DeliveryInfo != null &&
                            cart.DeliveryInfo.Option != DeliveryOptionType.Pickup &&
                            cart.DeliveryInfo.WarehouseCode != Warehouse)
                        {
                            var message = "SKUNotAvailable";
                            var globalResourceObject =
                                HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                    "SKUNotAvailable");
                            if (globalResourceObject != null)
                            {
                                message = string.Format(globalResourceObject.ToString(), shoppingCart.CurrentItems[0].SKU);
                            }
                            ruleResult.AddMessage(message);
                            ruleResult.Result = RulesResult.Failure;
                        }
                        else if (cart != null && cart.DeliveryInfo != null &&
                                 cart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                                 cart.DeliveryInfo.WarehouseCode == Warehouse && shoppingCart.CartItems != null &&
                                 shoppingCart.CartItems.Any(i => !presidentSummitSkus.Contains(i.SKU)))
                        {
                            var message = "StandaloneSku";
                            var globalResourceObject =
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                    "StandaloneSku");
                            if (globalResourceObject != null)
                            {
                                message = string.Format(globalResourceObject.ToString(),
                                                        shoppingCart.CurrentItems[0].SKU);
                            }
                            ruleResult.AddMessage(message);
                            ruleResult.Result = RulesResult.Failure;
                        }
                    }
                }
                else
                {
                    if (cart != null && cart.DeliveryInfo != null &&
                        cart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                        cart.DeliveryInfo.WarehouseCode == Warehouse)
                    {
                        var message = "SKUNotAvailable";
                        var globalResourceObject =
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                "SKUNotAvailable");
                        if (globalResourceObject != null)
                        {
                            message = string.Format(globalResourceObject.ToString(), shoppingCart.CurrentItems[0].SKU);
                        }
                        ruleResult.AddMessage(message);
                        ruleResult.Result = RulesResult.Failure;
                    }
                }
            }
            return ruleResult;
        }

        private ShoppingCartRuleResult CheckPresidentSummitWarehouse(ShoppingCart_V01 shoppingCart,
                                                                     ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null && cart.OrderCategory == OrderCategoryType.RSO && cart.DeliveryInfo != null)
            {
                var presidentSummitSkus = GetPresidentSummitSkus();
                if (cart.DeliveryInfo.Option == DeliveryOptionType.Pickup && cart.DeliveryInfo.WarehouseCode == Warehouse)
                {
                    // Remove items from other categories 
                    var itemsToRemove = from item in shoppingCart.CartItems
                                        where !presidentSummitSkus.Contains(item.SKU)
                                        select item;
                    if (itemsToRemove.Any())
                    {
                        var notValidSkus = itemsToRemove.Select(s => s.SKU).ToList();
                        cart.DeleteItemsFromCart(notValidSkus, true);
                        ruleResult.Result = RulesResult.Success;
                    }
                }
                else if (cart.DeliveryInfo.Option != DeliveryOptionType.Pickup || cart.DeliveryInfo.WarehouseCode != Warehouse)
                {
                    // Remove President Sumit items
                    var itemsToRemove = from item in shoppingCart.CartItems
                                        where presidentSummitSkus.Contains(item.SKU)
                                        select item;
                    if (itemsToRemove.Any())
                    {
                        var notValidSkus = itemsToRemove.Select(s => s.SKU).ToList();
                        cart.DeleteItemsFromCart(notValidSkus, true);
                        ruleResult.Result = RulesResult.Success;
                    }
                }
            }
            return ruleResult;
        }

        private List<string> GetPresidentSummitSkus()
        {
            var allProducts = HttpRuntime.Cache[PresSummitSkusCacheKey] as List<string>;
            if (allProducts == null || !allProducts.Any())
            {
                allProducts = new List<string>();
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    var prodictinfocatalog = CatalogProvider.GetProductInfoCatalog(Locale);
                    var presSummitCategory =
                        prodictinfocatalog.RootCategories.FirstOrDefault(
                            c =>
                            c.DisplayName.Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName));
                    if (presSummitCategory != null)
                    {
                        var products =
                            MyHerbalife3.Ordering.SharedProviders.CatalogProvider.GetProducts(presSummitCategory);
                        if (products != null && products.Any())
                        {
                            var skus = new List<SKU_V01>();
                            foreach (var p in products)
                            {
                                skus.AddRange(from s in p.Product.SKUs
                                              where !skus.Contains(s)
                                              select s);
                            }
                            allProducts = (from s in skus
                                           where s.IsDisplayable
                                           select s.SKU).ToList();
                            HttpRuntime.Cache.Insert(PresSummitSkusCacheKey, allProducts, null,
                                                     DateTime.Now.AddMinutes(PresSummitCacheMinutes),
                                                     Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                        }
                    }
                }
            }
            return allProducts;
        }

    }
}
