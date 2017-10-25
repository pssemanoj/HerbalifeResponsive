using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Internal
{
    public partial class ExpireCatalogCache : System.Web.UI.Page
    {
        public const string PRODUCTINFO_CACHE_PREFIX = "PRODUCTINFO_";

        public const string CATALOG_CACHE_PREFIX = "CATALOG_";

        public const string CacheRefencePrefix = "CACHEREFERENCE";

        public const string INVENTORY_CACHE_PREFIX = "INVENTORY_";

        public const string GridCaheKey = "Grid_";

        protected void Page_Load(object sender, EventArgs e)
        {
            string cacheKey = getProductInfoCacheKey(CultureInfo.CurrentCulture.Name);

            HttpRuntime.Cache.Remove(cacheKey);

            cacheKey = getCacheKey(CultureInfo.CurrentCulture.Name.Substring(3));

            HttpRuntime.Cache.Remove(cacheKey);

            cacheKey = GetCacheReferenceKey(CultureInfo.CurrentCulture.Name, "ProductInfo");

            HttpRuntime.Cache.Remove(cacheKey);

            cacheKey = GetCacheReferenceKey(CultureInfo.CurrentCulture.Name, "Catalog");

            HttpRuntime.Cache.Remove(cacheKey);

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
    }
}