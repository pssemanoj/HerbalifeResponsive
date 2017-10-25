using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Shared.Infrastructure.ValueObjects.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.SharedProviders
{
    /// <summary>
    /// Shared class to retrieve product catalog information.
    /// </summary>
    [DataObject]
    public static class CatalogProvider
    {
        #region Fields and constants
        public static int ProductinfoCacheMinutes = Settings.GetRequiredAppSetting<int>("ProductInfoCatalogCacheMinutes");
        private static readonly int CatalogCacheMinutes = Settings.GetRequiredAppSetting<int>("CatalogCacheExpireMinutes");
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the category information.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static ProductInfoCatalog_V01 GetProductInfoCatalog(string locale)
        {
            return GetProductInfoFromCache(locale);
        }

        /// <summary>
        /// Getd the product catalog.
        /// </summary>
        /// <param name="isoCountryCode">The country code</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static Catalog_V01 GetCatalog(string isoCountryCode)
        {
            return GetCatalogFromCache(isoCountryCode);
        }

        /// <summary>
        /// Gets the root category list.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="isEventTicketMode">The event ticket option flag.</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<Category_V02> GetRootCategories(string locale, bool isEventTicketMode)
        {
            var productInfo = GetProductInfoFromCache(locale);
            return (from r in productInfo.RootCategories
                    where ShouldTake(r, isEventTicketMode)
                    select r).ToList();
        }

        /// <summary>
        /// Gets the product/category list.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<ProductCategory> GetProducts(Category_V02 category)
        {
            var prodList = new List<ProductCategory>();
            prodList = GetAllProducts(category, category, prodList);
            return prodList;
        }

        #endregion

        #region Product/Category methods

        private static bool ShouldTake(Category_V02 category, bool isEventTicket)
        {
            if (category.Products != null)
            {
                foreach (ProductInfo_V02 prod in category.Products)
                {
                    if (ShouldTake(prod, isEventTicket))
                        return true;
                }
            }

            if (category.SubCategories != null)
            {
                foreach (Category_V02 sub in category.SubCategories)
                {
                    if (ShouldTake(sub, isEventTicket))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ShouldTake(ProductInfo_V02 prod, bool isEventTicket)
        {
            var take = isEventTicket
                           ? (prod.TypeOfProduct == ProductType.EventTicket)
                           : (prod.TypeOfProduct != ProductType.EventTicket);
            if (take)
            {
                if (prod != null && prod.SKUs != null)
                    take = prod.SKUs.Any(s => s.IsDisplayable);
            }

            return take;
        }

        public static Dictionary<string, SKU_V01> GetAllSKU(string locale)
        {
            var allSkus = new Dictionary<string, SKU_V01>();
            var productInfoCatalog = GetProductInfoFromCache(locale);
            var catalog = CatalogProvider.GetCatalog(locale.Substring(3));
            if (productInfoCatalog != null)
            {
                foreach (var item in productInfoCatalog.AllSKUs)
                {
                    if (catalog.Items.ContainsKey(item.Key))
                    {
                        allSkus.Add(item.Key, item.Value);
                    }
                }

                return allSkus;
            }

            return null;
        }

        private static List<ProductCategory> GetAllProducts(Category_V02 rooCategory, Category_V02 category, List<ProductCategory> allProducts)
        {
            if (category.SubCategories != null)
            {
                foreach (Category_V02 sub in category.SubCategories)
                {
                    allProducts = GetAllProducts(rooCategory, sub, allProducts);
                }
            }

            if (category.Products != null)
            {
                allProducts.AddRange(from p in category.Products
                                     where p.SKUs != null && p.SKUs.Count() > 0 && p.SKUs.Any(s => s.IsDisplayable)
                                     select
                                         new ProductCategory()
                                             {
                                                 RootCategory = rooCategory,
                                                 Category = category,
                                                 Product = p
                                             });
            }

            return allProducts;
        }

        #endregion

        #region Category methods

        private static string GetProductInfoCacheKey(string locale)
        {
            return string.Format("SHARED_PRODUCTINFO_{0}_{1}", HLConfigManager.Platform, locale);
        }

        private static ProductInfoCatalog_V01 GetProductInfoFromCache(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            locale = locale.Replace('_', '-');
            var cacheKey = GetProductInfoCacheKey(locale);
            var productInfo = HttpRuntime.Cache[cacheKey] as ProductInfoCatalog_V01;

            if (productInfo == null)
            {
                try
                {
                    productInfo = LoadProductInfoFromService(locale);
                    if (productInfo != null)
                    {
                        HttpRuntime.Cache.Insert(cacheKey, productInfo, null,
                                                 DateTime.Now.AddMinutes(ProductinfoCacheMinutes),
                                                 Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                                 OnCategoryCacheRemove);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("System.Exception", ex);
                }
            }

            return productInfo;
        }

        private static ProductInfoCatalog_V01 LoadProductInfoFromService(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            ProductInfoCatalog_V01 productInfo = null;
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new GetProductInfoRequest_V01(Settings.GetRequiredAppSetting("DWSCatalogPlatformKey", "dws_default"),locale);
                    var response = proxy.GetProductInfo(new GetProductInfoRequest1(request)).GetProductInfoResult as GetProductInfoResponse_V01;

                    if (response == null || response.Status != ServiceResponseStatusType.Success ||
                        response.ProductCatalog == null)
                    {
                        throw new ApplicationException(
                            "CatalogProvider.LoadProductInfoFromService error. GetProductInfoResponse indicates error. Locale : " +
                            locale);
                    }

                    productInfo = response.ProductCatalog;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("LoadProductInfoFromService error, locale: {0} {1}", locale, ex));
                }
            }

            return productInfo;
        }

        private static void SaveProductInfoCatalogToCache(string cacheKey, ProductInfoCatalog_V01 category)
        {
            HttpRuntime.Cache.Insert(cacheKey, category, null, DateTime.Now.AddMinutes(ProductinfoCacheMinutes),
                                     Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, OnCategoryCacheRemove);
        }

        private static void OnCategoryCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    {
                        var parts = key.Split('_');
                        var result = LoadProductInfoFromService((parts.Length == 3) ? parts[2] : parts[1]);

                        if (result != null)
                        {
                            SaveProductInfoCatalogToCache(key, result);
                        }
                        else
                        {
                            SaveProductInfoCatalogToCache(key, (ProductInfoCatalog_V01)sender);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Catalog methods

        private static string GetCacheKey(string isoCountryCode)
        {
            return string.Format("CATALOG_{0}", isoCountryCode);
        }

        private static Catalog_V01 GetCatalogFromCache(string isoCountryCode)
        {
            if (string.IsNullOrEmpty(isoCountryCode))
            {
                return null;
            }

            string cacheKey = GetCacheKey(isoCountryCode);
            var catalog = HttpRuntime.Cache[cacheKey] as Catalog_V01;

            if (catalog == null || catalog.Items == null)
            {
                try
                {
                    catalog = LoadCatalogFromService(isoCountryCode);
                    if (catalog != null)
                    {
                        SaveCatalogToCache(cacheKey, catalog);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("System.Exception", ex);
                }
            }

            return catalog;
        }

        private static Catalog_V01 LoadCatalogFromService(string isoCountryCode)
        {
            if (string.IsNullOrEmpty(isoCountryCode))
            {
                return null;
            }

            Catalog_V01 catalog = null;
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response = proxy.GetCatalog(new GetCatalogRequest1(new GetCatalogRequest_V01(isoCountryCode))).GetCatalogResult as GetCatalogResponse_V01;

                    if (response == null || response.Status != ServiceResponseStatusType.Success || response.Catalog == null)
                        throw new ApplicationException(
                            "CatalogProvider.LoadCatalogFromService error. GetCatalogResponse indicates error.");
                    catalog = response.Catalog;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("LoadCatalogFromService error, country Code: {0} {1}",
                                                     isoCountryCode, ex));
                }
            }
            return catalog;
        }

        private static void SaveCatalogToCache(string cacheKey, Catalog_V01 catalog)
        {
            HttpRuntime.Cache.Insert(cacheKey, catalog, null, DateTime.Now.AddMinutes(CatalogCacheMinutes),
                                     Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, OnCatalogCacheRemove);
        }

        private static void OnCatalogCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    try
                    {
                        var parts = key.Split('_');
                        var result = LoadCatalogFromService(parts[1]);

                        if (result != null)
                        {
                            SaveCatalogToCache(key, result);
                        }
                        else
                        {
                            SaveCatalogToCache(key, (Catalog_V01)sender);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Exception("System.Exception", ex);
                    }
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Product/category info model.
    /// </summary>
    public class ProductCategory
    {
        /// <summary>
        ///  Gets or sets the root category.
        /// </summary>
        public Category_V02 RootCategory { get; set; }
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public Category_V02 Category { get; set; }

        /// <summary>
        /// Gets or sets the product info.
        /// </summary>
        public ProductInfo_V02 Product { get; set; }
    }
}
