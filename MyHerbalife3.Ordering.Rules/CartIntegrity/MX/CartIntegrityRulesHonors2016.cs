using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;


namespace MyHerbalife3.Ordering.Rules.CartIntegrity.MX
{
    public class CartIntegrityRulesHonors2016 : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const string Honors2016SkusCacheKey = "Honors2016Skus";
        private const int Honors2016CacheMinutes = 60 * 6;
        private const int Honors2016EventId = 2462;

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = RuleName,
                Result = RulesResult.Unknown
            };
            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress)
            {
                result.Add(PerformRules(cart, reason, defaultResult));
            }
            
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                // Checking standalone skus
                result = CheckHonors2016Skus(cart, result);
            }
            else if (reason == ShoppingCartRuleReason.CartWarehouseCodeChanged)
            {
                // Valid skus in cart
                result = CheckHonors2016Warehouse(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CheckHonors2016Skus(ShoppingCart_V01 shoppingCart,
                                                                ShoppingCartRuleResult ruleResult)
        {
            if (shoppingCart != null)
            {
                var honors2016Skus = GetHonors2016Skus();
                var cart = shoppingCart as MyHLShoppingCart;
                if (honors2016Skus.Contains(shoppingCart.CurrentItems[0].SKU))
                {
                    if (!DistributorOrderingProfileProvider.IsEventQualified(Honors2016EventId, Locale))
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
                            cart.DeliveryInfo.WarehouseCode != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
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
                                 cart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse &&
                                 shoppingCart.CartItems != null &&
                                 shoppingCart.CartItems.Any(i => !honors2016Skus.Contains(i.SKU) && !APFDueProvider.IsAPFSku(i.SKU)))
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
                        else if (cart != null && cart.DeliveryInfo != null &&
                                 cart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                                 cart.DeliveryInfo.WarehouseCode != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse && 
                                 shoppingCart.CurrentItems != null &&
                                 shoppingCart.CurrentItems.Any(i => honors2016Skus.Contains(i.SKU)))
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
                else
                {
                    if (cart != null && cart.DeliveryInfo != null &&
                        cart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                        cart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
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

        private ShoppingCartRuleResult CheckHonors2016Warehouse(ShoppingCart_V01 shoppingCart,
                                                                     ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null && cart.OrderCategory == OrderCategoryType.RSO && cart.DeliveryInfo != null)
            {
                var honors2016Skus = GetHonors2016Skus();
                if (cart.DeliveryInfo.Option == DeliveryOptionType.Pickup && cart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
                {
                    // Remove items from other categories 
                    var itemsToRemove = (from item in shoppingCart.CartItems
                                        where !honors2016Skus.Contains(item.SKU)
                                        select item).ToList();
                    if (!itemsToRemove.Any())
                    {
                        return ruleResult;
                    }

                    //Do not remove APF SKU if exists in the cart
                    var apfSku = APFDueProvider.GetAPFSku();
                    itemsToRemove.RemoveAll(a => a.SKU.Equals(apfSku));

                    if (itemsToRemove.Any())
                    {
                        var notValidSkus = itemsToRemove.Select(s => s.SKU).ToList();
                        cart.DeleteItemsFromCart(notValidSkus, true);
                        ruleResult.Result = RulesResult.Success;
                    }
                }
                else if (cart.DeliveryInfo.Option != DeliveryOptionType.Pickup || cart.DeliveryInfo.WarehouseCode != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
                {
                    // Remove Honors 2016 items
                    var itemsToRemove = (from item in shoppingCart.CartItems
                                        where honors2016Skus.Contains(item.SKU)
                                        select item).ToList();
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

        private List<string> GetHonors2016Skus()
        {
            var allProducts = HttpRuntime.Cache[Honors2016SkusCacheKey] as List<string>;
            if (allProducts == null || !allProducts.Any())
            {
                allProducts = new List<string>();
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    var prodictinfocatalog = CatalogProvider.GetProductInfoCatalog(Locale);
                    var honors2016Category =
                        prodictinfocatalog.RootCategories.FirstOrDefault(
                            c =>
                            c.DisplayName.Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName));
                    if (honors2016Category != null)
                    {
                        var products = MyHerbalife3.Ordering.SharedProviders.CatalogProvider.GetProducts(honors2016Category);
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
                            HttpRuntime.Cache.Insert(Honors2016SkusCacheKey, allProducts, null,
                                                     DateTime.Now.AddMinutes(Honors2016CacheMinutes),
                                                     Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                        }
                    }
                }
            }
            return allProducts;
        }

    }
}
