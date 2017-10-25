using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using System.Web;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.es_ES
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const string Extravaganza2017CacheKey = "Extravaganza2017";
        private const int Extravaganza2017CacheMinutes = 60;

        private int EventId
        {
            get
            {
                return int.Parse(!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.EventId) ?
                    HLConfigManager.Configurations.DOConfiguration.EventId : "0");
            }
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = RuleName,
                Result = RulesResult.Unknown
            };
            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress && EventId > 0)
            {
                result.Add(PerformRules(cart, reason, defaultResult));
            }

            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                result = CheckQualifiedEventSkus(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CheckQualifiedEventSkus(ShoppingCart_V01 shoppingCart, ShoppingCartRuleResult ruleResult)
        {
            if (shoppingCart != null)
            {
                List<string> ConfigSKUList = HLConfigManager.Configurations.DOConfiguration.SkuForCurrentEvent.Split(',').ToList();
                var SKUList = GetExtravaganza2017Skus();
                if (SKUList != null && ConfigSKUList != null)
                {
                    SKUList.AddRange(from s in ConfigSKUList
                                     where !SKUList.Contains(s)
                                     select s);
                }
                
                var cart = shoppingCart as MyHLShoppingCart;

                if (SKUList.Any() && SKUList.Contains(cart.CurrentItems[0].SKU))
                {
                    bool memberWithTicket;
                    var memberQualified = DistributorOrderingProfileProvider.IsEventQualified(EventId, Locale, out memberWithTicket);
                    if (!memberQualified)
                    {
                        var message = "SKUNotAvailableForNonQualified";
                        var globalResourceObject = PlatformResources.GetGlobalResourceString("ErrorMessage", message);
                        if (!string.IsNullOrEmpty(globalResourceObject))
                        {
                            message = string.Format(globalResourceObject.ToString(), cart.CurrentItems[0].SKU);
                        }
                        ruleResult.AddMessage(message);
                        ruleResult.Result = RulesResult.Failure;
                    }
                }
            }
            return ruleResult;
        }
        private List<string> GetExtravaganza2017Skus()
        {
            var allProducts = HttpRuntime.Cache[Extravaganza2017CacheKey] as List<string>;
            if (allProducts == null || !allProducts.Any())
            {
                allProducts = new List<string>();
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    var prodictinfocatalog = CatalogProvider.GetProductInfoCatalog(Locale);
                    var extravaganza2017Category =
                        prodictinfocatalog.RootCategories.FirstOrDefault(
                            c =>
                            c.DisplayName.Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName));
                    if (extravaganza2017Category != null)
                    {
                        var products = SharedProviders.CatalogProvider.GetProducts(extravaganza2017Category);
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
                            HttpRuntime.Cache.Insert(Extravaganza2017CacheKey, allProducts, null,
                                                     DateTime.Now.AddMinutes(Extravaganza2017CacheMinutes),
                                                     Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                        }
                    }
                }
            }
            return allProducts;
        }
    }
}
