#region

using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Text.RegularExpressions;
#endregion

namespace MyHerbalife3.Ordering.Providers
{
    public class ExpireCatalogCacheProvider : IExpireCatalogCacheProvider
    {
        public const string PRODUCTINFO_CACHE_PREFIX = "PRODUCTINFO_";
        public const string CATALOG_CACHE_PREFIX = "CATALOG_";
        public const string CacheRefencePrefix = "CACHEREFERENCE";
        public const string INVENTORY_CACHE_PREFIX = "INVENTORY_";
        public const string GridCaheKey = "Grid_";
        public ExpireCatalogCacheResponseViewModel ExpireCatalogCache(string cacheName, string inputCacheKey, string locale)
        {
            try
            {
                
                string cacheKey = string.Empty;
                if (cacheName.Trim().ToUpper() == "ALL")
                {
                    cacheKey = getProductInfoCacheKey(locale);
                    HttpRuntime.Cache.Remove(cacheKey);
                    cacheKey = GetCacheReferenceKey(locale, "ProductInfo");
                    HttpRuntime.Cache.Remove(cacheKey);
                    cacheKey = getCacheKey(locale.Substring(3));
                    HttpRuntime.Cache.Remove(cacheKey);
                    cacheKey = GetCacheReferenceKey(locale, "Catalog");
                    HttpRuntime.Cache.Remove(cacheKey);
                    ClearInventoryCache();

                    return new ExpireCatalogCacheResponseViewModel { ProductInfoCacheExpired = true, CatalogCacheExpired = true, InventoryCacheExpired = true, SentCacheKeyCleared = ClearSentCache(inputCacheKey) ? true : false };
                }
                else
                {
                    if (cacheName.Trim().ToUpper() == "PRODUCTINFO")
                    {
                        cacheKey = getProductInfoCacheKey(locale);
                        HttpRuntime.Cache.Remove(cacheKey);
                        cacheKey = GetCacheReferenceKey(locale, "ProductInfo");
                        HttpRuntime.Cache.Remove(cacheKey);
                        return new ExpireCatalogCacheResponseViewModel { ProductInfoCacheExpired = true, SentCacheKeyCleared = ClearSentCache(inputCacheKey) ? true : false };
                    }
                    
                    if (cacheName.Trim().ToUpper() == "CATALOG")
                    {
                        cacheKey = getCacheKey(locale.Substring(3));
                        HttpRuntime.Cache.Remove(cacheKey);
                        cacheKey = GetCacheReferenceKey(locale, "Catalog");
                        HttpRuntime.Cache.Remove(cacheKey);
                        return new ExpireCatalogCacheResponseViewModel { CatalogCacheExpired = true, SentCacheKeyCleared = ClearSentCache(inputCacheKey) ? true : false };
                    }

                    if (cacheName.Trim().ToUpper() == "INVENTORY")
                    {
                        ClearInventoryCache();
                        return new ExpireCatalogCacheResponseViewModel { InventoryCacheExpired = true, SentCacheKeyCleared = ClearSentCache(inputCacheKey) ? true : false };
                    }
                    
                    return new ExpireCatalogCacheResponseViewModel { ProductInfoCacheExpired = false, CatalogCacheExpired = false, InventoryCacheExpired = false };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
                return new ExpireCatalogCacheResponseViewModel { ProductInfoCacheExpired = false, CatalogCacheExpired = false, InventoryCacheExpired = false };
            }
            
        }
        private static string getProductInfoCacheKey(string locale)
        {
            return string.Format("{0}{1}_{2}", PRODUCTINFO_CACHE_PREFIX, HLConfigManager.Platform == "MyHLMobile" ? "MyHL" : HLConfigManager.Platform, locale);
        }

        private static string getCacheKey(string isoCountryCode)
        {
            return CATALOG_CACHE_PREFIX + isoCountryCode;
        }

        private static string GetCacheReferenceKey(string locale, string type)
        {
            return string.Format("{0}_{1}_{2}", CacheRefencePrefix, locale, type);
        }
        private static void ClearInventoryCache()
        {
            var enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = (string)enumerator.Key;
                if (!string.IsNullOrEmpty(key) && (key.StartsWith(INVENTORY_CACHE_PREFIX) || (key.StartsWith(GridCaheKey))))
                {
                    HttpRuntime.Cache.Remove(key);
                }
            }
            
        }
        private static Boolean ClearSentCache(string cachekey)
        {
            Boolean isCleared = false;
            if (null != cachekey && cachekey.Trim().Length > 2)
            {
                if (!cachekey.Trim().Contains(","))
                    cachekey += ",";
                string[] lines =  Regex.Split(cachekey.Trim(), ",");

                foreach (string ckey in lines)
                {
                    var enumerator = HttpRuntime.Cache.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var key = (string)enumerator.Key;
                        if (!string.IsNullOrEmpty(key) && (key.StartsWith(ckey)))
                        {
                            HttpRuntime.Cache.Remove(key);
                            isCleared = true;
                        }
                    }
                }
            }
                return isCleared;
        }
    }
}
