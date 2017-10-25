using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ViewModel.Model.BackOrderDetail;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.LegacyProviders;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Providers
{
    [DataObject]
    public static partial class CatalogProvider
    {
        public const string PRODUCTINFO_CACHE_PREFIX = "PRODUCTINFO_";

        public const string CATALOG_CACHE_PREFIX = "CATALOG_";

        public const string EXEMPTED_DS_CACHE_PREFIX = "EXEMPTED_DS_";

        public static int EXEMPTED_DS_CACHE_MINUTES = Settings.GetRequiredAppSetting<int>("ExemptedDistributorCacheExpireMinutes");

        //public const string SKUBYLIST_CACHE_PREFIX = "SKUBYLIST_";
        //public static int SKUBYLIST_CACHE_MINUTES = HL.Common.Configuration.Settings.GetRequiredAppSetting<int>("CatalogCacheExpireMinutes");

        public const string PRODUCTINFO_BY_SEARCHTERM_CACHE_PREFIX = "ProductInfoBySearchTerm_";

        public static int PRODUCTINFO_CACHE_MINUTES =
            Settings.GetRequiredAppSetting<int>("ProductInfoCatalogCacheMinutes");

        public static int CATALOG_CACHE_MINUTES = Settings.GetRequiredAppSetting<int>("CatalogCacheExpireMinutes");

        public const string SIZE_CHART_CACHE_PREFIX = "SizeCharts_";

        public static TimeSpan ProductInfoBySearchTermSlidingCache = new TimeSpan(0,
                                                                                  Settings.GetRequiredAppSetting(
                                                                                      "ProductInfoCatalogCacheMinutes",
                                                                                      10), 0);
        private const string CacheAllProductDiscontinued = "AllProductDiscontinuedSku";

        public const string ALL_PRODUCTS_DISC_PREFIX = "ALL_PRODUCT_DISCONTINUED_SKU_";

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static Catalog_V01 GetCatalog(string isoCountryCode)
        {
            return getCatalogFromCache(isoCountryCode);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static CatalogItem_V01 GetCatalogItem(string sku, string isoCountryCode)
        {
            var result = GetCatalog(isoCountryCode);
            if (result != null && result.Items.ContainsKey(sku))
            {
                return result.Items[sku] as CatalogItem_V01;
            }
            return null;
        }

        public static CatalogItemList GetCatalogItems(List<string> skuList, string isoCountryCode)
        {
            var result = GetCatalog(isoCountryCode);
            if (result != null)
            {
                var catalogItemList = new CatalogItemList();
                foreach (string sku in skuList)
                {
                    if (result.Items.ContainsKey(sku))
                    {
                        catalogItemList.Add(sku, result.Items[sku]);
                    }
                }
                return catalogItemList;
            }
            return null;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static ProductInfoCatalog_V01 GetProductInfoCatalog(string locale)
        {
            return GetProductInfoCatalog(locale, string.Empty);
        }

        public static ProductAvailabilityType GetProductAvailability(SKU_V01 sku, string warehouse, DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown, string freightCode = null)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IgnoreInventory)
            {
                return ProductAvailabilityType.Available;
            }

            sku.ProductAvailability = ProductAvailabilityInWarehouse(sku, warehouse);
            var primaryWH = sku.CatalogItem != null ?
                                sku.CatalogItem.InventoryList != null ? 
                                    sku.CatalogItem.InventoryList.ContainsKey(warehouse) ?
                                        sku.CatalogItem.InventoryList[warehouse] as WarehouseInventory_V01 
                                        : null
                                    : null
                                : null;

            if (primaryWH != null && primaryWH.IsSplitAllowed)
            {
            if (sku.CatalogItem != null && sku.CatalogItem.ProductType == ProductType.Product && !sku.CatalogItem.IsFlexKit &&
                (sku.ProductAvailability == ProductAvailabilityType.Unavailable ||
                 sku.ProductAvailability == ProductAvailabilityType.AllowBackOrder))
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit) &&
                    HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Contains(warehouse) &&
                    deliveryOption == DeliveryOptionType.Shipping &&
                    (string.IsNullOrEmpty(freightCode) || string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.FreightCodesForSplit) ||
                     HLConfigManager.Configurations.DOConfiguration.FreightCodesForSplit.Contains(freightCode)))
                {
                    var primaryAvailability = sku.ProductAvailability;
                    var alternativeWhs = HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Split(',');
                    foreach (var wh in alternativeWhs)
                    {
                        if (wh == warehouse) continue;

                            var alternateWH = sku.CatalogItem != null ?
                                                sku.CatalogItem.InventoryList != null ? sku.CatalogItem.InventoryList[wh] as WarehouseInventory_V01 : null
                                                : null;

                            if (alternateWH != null && alternateWH.IsSplitAllowed)
                            {
                        var alternateAvailability = ProductAvailabilityInWarehouse(sku, wh);
                        if (alternateAvailability == ProductAvailabilityType.Available)
                        {
                            break;
                        }
                    }
                        }
                    if (sku.ProductAvailability != primaryAvailability &&
                        sku.ProductAvailability == ProductAvailabilityType.Available)
                    {
                        sku.ProductAvailability = ProductAvailabilityType.UnavailableInPrimaryWh;
                    }
                    else
                    {
                        // Let the availability from the primary warehouse
                        sku.ProductAvailability = primaryAvailability;
                    }
                }
            }
            }

            return sku.ProductAvailability;
        }

        public static bool IsPreordering(ShoppingCartItemList cart, string wareHousecode)
        {
            bool result = false;

            if (cart != null)
            {
                foreach (var cartitem in cart)
                {
                    result = IsPreordering(cartitem.SKU, wareHousecode);

                    if (result)
                        break;
                }
            }

            return result;
        }

        public static bool IsPreordering(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode)
        {
            bool result = false;

            if (shoppingCart != null)
            {
                foreach (var cartitem in shoppingCart)
                {
                    result = IsPreordering(cartitem.SKU, wareHousecode);

                    if (result)
                        break;
                }
            }

            return result;
        }

        public static void IsPreordering(MyHLShoppingCart cartItems, List<ShoppingCartItem_V01> currentitems,
                                         string wareHousecode, out bool preorderskus, out bool nonpreorderskus)
        {
            try
            {
                var warehouseInventorycartitemlist = new List<WarehouseInventory_V01>();
                var productIdList = new List<int>();
                //Providers.China.CatalogProvider.LoadInventory(wareHousecode);

                if (wareHousecode != null
                    && (!string.IsNullOrEmpty(wareHousecode) && HLConfigManager.Configurations.DOConfiguration.IsChina))
                {
                    Providers.China.CatalogProvider.LoadInventory(wareHousecode);
                }
                else
                {
                    preorderskus = false;
                    nonpreorderskus = false;
                }

                var productinfoCatalog =
                    GetProductInfoCatalog(System.Threading.Thread.CurrentThread.CurrentCulture.ToString(), wareHousecode);
                if (cartItems != null)
                {
                    foreach (var cartitem in cartItems.ShoppingCartItems)
                    {

                        CatalogItem_V01 catalogItem = CatalogProvider.GetCatalogItem(cartitem.SKU, "CN");

                        if (catalogItem != null && catalogItem.InventoryList != null &&
                            catalogItem.InventoryList.ContainsKey(wareHousecode))
                            warehouseInventorycartitemlist.Add(
                                catalogItem.InventoryList[wareHousecode] as WarehouseInventory_V01);

                        if (productinfoCatalog != null && productinfoCatalog.AllSKUs != null && productinfoCatalog.AllSKUs.ContainsKey(cartitem.SKU))
                        {
                            if (productinfoCatalog.AllSKUs[cartitem.SKU].Product != null)
                                productIdList.Add(productinfoCatalog.AllSKUs[cartitem.SKU].Product.ID);
                        }
                    }
                }
                if (currentitems != null)
                {
                    foreach (var cartitem in currentitems)
                    {

                        CatalogItem_V01 catalogItem = CatalogProvider.GetCatalogItem(cartitem.SKU, "CN");

                        if (catalogItem != null && catalogItem.InventoryList != null &&
                            catalogItem.InventoryList.ContainsKey(wareHousecode))
                            warehouseInventorycartitemlist.Add(
                                catalogItem.InventoryList[wareHousecode] as WarehouseInventory_V01);

                        if (productinfoCatalog != null && productinfoCatalog.AllSKUs != null && productinfoCatalog.AllSKUs.ContainsKey(cartitem.SKU))
                        {
                            if (productinfoCatalog.AllSKUs[cartitem.SKU].Product != null)
                                productIdList.Add(productinfoCatalog.AllSKUs[cartitem.SKU].Product.ID);
                        }
                    }
                }
                if (warehouseInventorycartitemlist.Count > 0)
                {
                    int productid = productIdList.FirstOrDefault();
                    if (productid > 0 && warehouseInventorycartitemlist.All(x => x.IsPreOrdering))
                    {
                        preorderskus = true;
                        nonpreorderskus = productIdList.Skip(1).All(x => x == productid);
                    }
                    else
                    {
                        preorderskus = false;
                        nonpreorderskus = false;
                    }
                }
                else
                {
                    preorderskus = false;
                    nonpreorderskus = false;
                }
            }
            catch (Exception ex)
            {
                preorderskus = false;
                nonpreorderskus = false;
                LoggerHelper.Error(
                    string.Format(
                        "Error in CatalogPrivider ,Details :{0}", ex.Message + ex.StackTrace));
            }
        }

        public static bool IsPreordering(string sku, string wareHousecode)
        {
            var warehouseInventorycartitemlist = new List<WarehouseInventory_V01>();
            var productIdList = new List<int>();

            try
            {
                if ((!string.IsNullOrEmpty(wareHousecode) && HLConfigManager.Configurations.DOConfiguration.IsChina))
                {
                    Providers.China.CatalogProvider.LoadInventory(wareHousecode);
                }
                else
                    return false;

                var productinfoCatalog = GetProductInfoCatalog(System.Threading.Thread.CurrentThread.CurrentCulture.ToString(), wareHousecode);

                if (!string.IsNullOrEmpty(sku))
                {
                    CatalogItem_V01 catalogItem = CatalogProvider.GetCatalogItem(sku, "CN");

                    if (catalogItem != null && catalogItem.InventoryList != null && catalogItem.InventoryList.ContainsKey(wareHousecode))
                        warehouseInventorycartitemlist.Add(catalogItem.InventoryList[wareHousecode] as WarehouseInventory_V01);

                    if (productinfoCatalog != null && productinfoCatalog.AllSKUs != null && productinfoCatalog.AllSKUs.ContainsKey(sku))
                    {
                        if (productinfoCatalog.AllSKUs[sku].Product != null)
                            productIdList.Add(productinfoCatalog.AllSKUs[sku].Product.ID);
                    }
                }

                if (warehouseInventorycartitemlist.Count > 0)
                {
                    int productid = productIdList.FirstOrDefault();
                    if (productid > 0 && warehouseInventorycartitemlist.All(x => x.IsPreOrdering))
                    {
                        bool all = productIdList.Skip(1).All(x => x == productid);
                        return all;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn(string.Format("CatalogProvider.IsPreordering : Warehouse: {0} ", ex));
            }

            return false;
        }

        public static bool IsAllPreorderingProducts(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode)
        {
            bool result = false;

            if (shoppingCart != null && shoppingCart.Count > 0)
            {
                result = true; //assume all are preordering at first.
                foreach (var cartitem in shoppingCart)
                {
                    result = IsPreordering(cartitem.SKU, wareHousecode);

                    if (!result) //any fails, break the verification.
                        break;
                }
            }

            return result;
        }

        private static ProductAvailabilityType ProductAvailabilityInWarehouse(SKU_V01 sku, string warehouse)
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList != null &&
                HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Count > 0)
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Exists(s => s == sku.SKU))
                {
                    // Check if the sku is blocked
                    if (sku.CatalogItem != null && sku.CatalogItem.InventoryList != null &&
                        sku.CatalogItem.InventoryList.ContainsKey(warehouse) &&
                        sku.CatalogItem.InventoryList[warehouse] is WarehouseInventory_V01)
                    {
                        var warehouseInv = sku.CatalogItem.InventoryList[warehouse] as WarehouseInventory_V01;
                        return warehouseInv.IsBlocked
                                   ? (sku.ProductAvailability = ProductAvailabilityType.Unavailable)
                                   : (sku.ProductAvailability = ProductAvailabilityType.Available);
                    }

                    return ProductAvailabilityType.Available;
                }
            }
            sku.ProductAvailability = ProductAvailabilityType.Unavailable;
            if (!sku.IsPurchasable)
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var SRPromoSku = Settings.GetRequiredAppSetting("ChinaSRPromo", string.Empty).Split('|');
                    if(SRPromoSku.Contains(sku.SKU))
                    {
                        sku.ProductAvailability = ProductAvailabilityType.Available;
                    }
                    else if (!ChinaPromotionProvider.GetPCPromoSkus(sku.SKU))
                    {
                        return (sku.ProductAvailability = ProductAvailabilityType.Unavailable);
                    }

                }
                else
                {
                    return (sku.ProductAvailability = ProductAvailabilityType.Unavailable);
                }

            }

            if (sku.CatalogItem == null)
            {
                return sku.ProductAvailability;
            }
            if (sku.CatalogItem.InventoryList == null)
            {
                LoggerHelper.Warn(string.Format("CatalogProvider.GetProductAvailability InventoryList is null for SKU: {0} and Warehouse: {1} ", sku.SKU, warehouse));
                return sku.ProductAvailability;
            }

            if (sku.CatalogItem.InventoryList.ContainsKey(warehouse) &&
                sku.CatalogItem.InventoryList[warehouse] is WarehouseInventory_V01)
            {
                var warehouseInv = sku.CatalogItem.InventoryList[warehouse] as WarehouseInventory_V01;

                if (sku.CatalogItem.IsEventTicket)
                {
                    return warehouseInv.IsBlocked
                               ? (sku.ProductAvailability = ProductAvailabilityType.Unavailable)
                               : (sku.ProductAvailability = ProductAvailabilityType.Available);
                }

                if (warehouseInv.IsBlocked)
                {
                    sku.ProductAvailability = ProductAvailabilityType.Unavailable;
                }

                // Non-inventory SKUs are always available, if warehouse sku is not blocked.
                else if (!sku.CatalogItem.IsInventory)
                {
                    sku.ProductAvailability = ProductAvailabilityType.Available;
                }

                else if (warehouseInv.QuantityAvailable <= 0)
                {
                    sku.ProductAvailability = warehouseInv.IsBackOrder
                                                  ? ProductAvailabilityType.AllowBackOrder
                                                  : ProductAvailabilityType.Unavailable;
                }
                else
                {
                    sku.ProductAvailability = ProductAvailabilityType.Available;
                }
            }
            else if (sku.CatalogItem.IsEventTicket)
            {
                return sku.ProductAvailability = ProductAvailabilityType.Available;
            }
            return sku.ProductAvailability;
        }

        public static void GetProductAvailability(
            List<SKU_V01> productInfoCatalog,
            string locale,
            string distributorID,
            string warehouse,
            DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown)
        {
            if (productInfoCatalog != null)
            {
                foreach (SKU_V01 sku in productInfoCatalog)
                {
                    GetProductAvailability(sku, warehouse, deliveryOption);
                }
            }
        }

        public static ProductInfoCatalog_V01 GetProductInfoCatalog(string locale, string warehouse)
        {
            var productInfoCatalog = getProductInfoFromCache(locale);
            if (productInfoCatalog != null)
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && !string.IsNullOrEmpty(warehouse) && productInfoCatalog.AllSKUs != null && productInfoCatalog.AllSKUs.Count > 0 && productInfoCatalog.AllSKUs.First().Value != null && productInfoCatalog.AllSKUs.First().Value.CatalogItem != null && (productInfoCatalog.AllSKUs.First().Value.CatalogItem.InventoryList == null || !productInfoCatalog.AllSKUs.First().Value.CatalogItem.InventoryList.ContainsKey(warehouse)))
                {
                    //to handle when cache expired and inventory information is not loaded.
                    Providers.China.CatalogProvider.LoadInventory(warehouse);
                }
            }

            return productInfoCatalog;
        }

        public static Dictionary<string, SKU_V01> GetAllSKU(string locale)
        {
            return GetAllSKU(locale, string.Empty);
        }

        /// <summary>
        /// For China Catalog, need to provide the selected warehouse id if the caller required to validate the product availability. Otherwise, intermitent of out of stock issue will happen whenever catalog catch expired.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        public static Dictionary<string, SKU_V01> GetAllSKU(string locale, string warehouse)
        {
            var productInfoCatalog = getProductInfoFromCache(locale);
            if (productInfoCatalog != null)
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && !string.IsNullOrEmpty(warehouse) && productInfoCatalog.AllSKUs != null && productInfoCatalog.AllSKUs.Count > 0 && productInfoCatalog.AllSKUs.First().Value != null && productInfoCatalog.AllSKUs.First().Value.CatalogItem != null && (productInfoCatalog.AllSKUs.First().Value.CatalogItem.InventoryList == null || !productInfoCatalog.AllSKUs.First().Value.CatalogItem.InventoryList.ContainsKey(warehouse)))
                {
                    //to handle when cache expired and inventory information is not loaded.
                    Providers.China.CatalogProvider.LoadInventory(warehouse);
                }

                return productInfoCatalog.AllSKUs;
            }

            return null;
        }

        /// <summary>
        ///     cache key for catalog
        /// </summary>
        /// <param name="isoCountryCode"></param>
        /// <returns></returns>
        private static string getCacheKey(string isoCountryCode)
        {
            return CATALOG_CACHE_PREFIX + isoCountryCode;
        }

        private static string getProductInfoCacheKey(string locale)
        {
            return string.Format("{0}{1}_{2}", PRODUCTINFO_CACHE_PREFIX, HLConfigManager.Platform == "MyHLMobile" ? "MyHL" : HLConfigManager.Platform, locale);
        }

        private static Catalog_V01 loadCatalogFromService(string isoCountryCode)
        {
            if (string.IsNullOrEmpty(isoCountryCode))
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response =
                            proxy.GetCatalog(new GetCatalogRequest1(new GetCatalogRequest_V01(isoCountryCode))).GetCatalogResult as GetCatalogResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.Catalog == null)
                            throw new ApplicationException(
                                "CatalogProvider.loadCatalogFromService error. GetCatalogResponse indicates error.");

                        // Save the catalog reference
                        var locale = CultureInfo.CurrentCulture.Name;
                        SetCacheReference(locale, CacheCatalogType, response.Reference);
                        return response.Catalog;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                   string.Format("loadCatalogFromService error, country Code: {0} {1}", isoCountryCode, ex));
                    }
                }
            }
            return null;
        }

        private static Dictionary<string, SKU_V01> getCatalogItems(string locale,
                                                                   Catalog_V01 catalog,
                                                                   Dictionary<string, SKU_V01> allSKUs)
        {
            var newAllSKUs = new Dictionary<string, SKU_V01>();
            foreach (SKU_V01 sku in allSKUs.Values)
            {
                CatalogItem catalogItem;
                if (catalog.Items.TryGetValue(sku.SKU, out catalogItem))
                {
                    sku.CatalogItem = catalogItem as CatalogItem_V01;
                    newAllSKUs[sku.SKU] = sku;
                }
            }
            return newAllSKUs;
        }

        private static ProductInfoCatalog_V01 loadProductInfoFromService(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }
            else
            {
                CatalogInterfaceClient proxy = null;

                using (proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response =
                            proxy.GetProductInfo(new GetProductInfoRequest1(new GetProductInfoRequest_V01(HLConfigManager.Platform == "MyHLMobile" ? "MyHL" : HLConfigManager.Platform, locale))).GetProductInfoResult as
                            GetProductInfoResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.ProductCatalog == null)
                        {
                            throw new ApplicationException(
                                "CatalogProvider.loadProductInfoFromService error. GetProductInfoResponse indicates error. Locale : " +
                                locale);
                        }

                        // Save the product info reference
                        SetCacheReference(locale, CacheProductInfoType, response.Reference);

                        var catalog = GetCatalog(locale.Substring(3));

                        response.ProductCatalog.AllSKUs =
                            new SKU_V01ItemCollection(getCatalogItems(locale, catalog, response.ProductCatalog.AllSKUs));
                        return response.ProductCatalog;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("loadProductInfoFromService error, locale: {0} {1}", locale, ex));
                    }
                }
            }

            return null;
        }

        public static CatalogItemList GetMobileDiscontinuedSku(string distributorId, string locale, List<ShoppingCartItem_V01> shoppingCartItems )
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetDiscontinuedSku(new GetDiscontinuedSkuRequest1(new GetDiscontinuedSkusRequest_V01 {DistributorId = distributorId,Locale = locale,ShoppingCartItemList= shoppingCartItems })).GetDiscontinuedSkuResult as GetDiscontinuedSkuResponse_V01;

                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success ||
                        response.DiscontinuedSku == null)
                        LoggerHelper.Error(string.Format("CatalogProvider.GetMobileDiscontinuedSku error. null response, country Code: {0}", "CN"));
                    if (response != null) return response.DiscontinuedSku;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("loadCatalogFromService for GetMobileDiscontinuedSku error, country Code: {0} {1}", "CN", ex));
                    return new CatalogItemList();
                }
            }
            return new CatalogItemList();
        }

        public static CatalogItemList GetDiscontinuedProductDetail(List<string> sku)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetDiscontinuedProductDetails(new GetDiscontinuedProductDetailsRequest(new GetCatalogItemsRequest_V01 {CountryCode = "CN",SKUs = sku})).GetDiscontinuedProductDetailsResult as GetCatalogItemsResponse_V01;

                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success ||
                        response.Products == null)
                        LoggerHelper.Error(string.Format("CatalogProvider.GetDiscontinuedProductDetail error. null response, country Code: {0}", "CN"));
                    return response.Products;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
               string.Format("loadCatalogFromService for GetDiscontinuedProductDetail error, country Code: {0} {1}", "CN", ex));
                }
            }
            return null;
        }

        private static Catalog_V01 getCatalogFromCache(string isoCountryCode)
        {
            Catalog_V01 result = null;

            if (string.IsNullOrEmpty(isoCountryCode))
            {
                return result;
            }

            var locale = CultureInfo.CurrentCulture.Name;
            var cachedReference = GetCacheReference(locale, CacheCatalogType);

            // gets cache key
            string cacheKey = getCacheKey(isoCountryCode);

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as Catalog_V01;

            if (null == result || null == result.Items || (!string.IsNullOrEmpty(cachedReference) && cachedReference.Equals("1")))
            {
                try
                {
                  // gets resultset from Business Layer is object not found in cache
                    result = loadCatalogFromService(isoCountryCode);
                    // saves to cache is successful
                    if (null != result)
                    {
                        saveCatalogToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static ProductInfoCatalog_V01 productInfoExistInCache(string locale)
        {
            ProductInfoCatalog_V01 result = null;

            if (string.IsNullOrEmpty(locale))
            {
                return result;
            }
            // this is to prevent passing wrong locale when cache expires.
            locale = locale.Replace('_', '-');

            // gets cache key
            string cacheKey = getProductInfoCacheKey(locale);

            // tries to get object from cache
            return (result = HttpRuntime.Cache[cacheKey] as ProductInfoCatalog_V01);
        }

        private static ProductInfoCatalog_V01 getProductInfoFromCache(string locale)
        {
            var cachedReference = GetCacheReference(locale, CacheProductInfoType);

            var result = productInfoExistInCache(locale);

            if (null == result || (!string.IsNullOrEmpty(cachedReference) && cachedReference.Equals("1")))
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = loadProductInfoFromService(locale);
                    // saves to cache is successful
                    if (null != result)
                    {
                        saveProductInfoCatalogToCache(getProductInfoCacheKey(locale), result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            else
            {
                // Checking the catalog info to be refreshed
                var cachedReferenceCatalog = GetCacheReference(locale, CacheCatalogType);
                if (!string.IsNullOrEmpty(cachedReferenceCatalog) && cachedReferenceCatalog.Equals("1"))
                {
                    var catalog = GetCatalog(locale.Substring(3));
                    result.AllSKUs = new SKU_V01ItemCollection(getCatalogItems(locale, catalog, result.AllSKUs));
                }
            }
            
            string country = locale.Substring(3).ToUpper();
            if (country == "US" || country == "PR")
            {

                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (membershipUser != null)
                {
                    var distId = membershipUser.Value.Id;
                    var session = SessionInfo.GetSessionInfo(distId, locale);

                    ServiceProvider.DistributorSvc.Scheme DistributorType = ServiceProvider.DistributorSvc.Scheme.Distributor;
                    if (session.DsType == null)
                    {
                        DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(distId, country);
                        session.DsType = DistributorType;
                        SessionInfo.SetSessionInfo(distId, locale, session);
                    }
                    if (DistributorType == ServiceProvider.DistributorSvc.Scheme.Member)
                    {
                        var removeSku = country == "US" ?
                            Settings.GetRequiredAppSetting("RemoveMemberUSSku", string.Empty).Split(',') :
                            Settings.GetRequiredAppSetting("RemoveMemberPRSku", string.Empty).Split(',');
                        if (result != null && result.AllSKUs != null && result.AllSKUs.Any())
                            result.AllSKUs.ToList().RemoveAll(i => removeSku.Contains(i.Key));
                    }
                }
            }

            return result;
        }

        private static void saveCatalogToCache(string cacheKey, Catalog_V01 catalog)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     catalog,
                                     null,
                                     DateTime.Now.AddMinutes(CATALOG_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);
        }

        private static void saveProductInfoCatalogToCache(string cacheKey, ProductInfoCatalog_V01 category)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     category,
                                     null,
                                     DateTime.Now.AddMinutes(PRODUCTINFO_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);
        }

        private static void onCatalogCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    try
                    {
                        var parts = key.Split('_');
                        var result = loadCatalogFromService(parts[1]);

                        if (result != null)
                        {
                            // if success replace cache with new resultset
                            saveCatalogToCache(key, result);
                        }
                        else
                        {
                            // if failure re-insert from cache
                            saveCatalogToCache(key, (Catalog_V01)sender);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    }
                    break;
                case CacheItemRemovedReason.Removed:
                case CacheItemRemovedReason.DependencyChanged:
                default:
                    break;
            }
        }

        private static void onCategoryCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    {
                        var parts = key.Split('_');
                        var result = loadProductInfoFromService((parts.Length == 3) ? parts[2] : parts[1]);

                        if (result != null)
                        {
                            // if success replace cache with new resultset
                            saveProductInfoCatalogToCache(key, result);
                        }
                        else
                        {
                            // if failure re-insert from cache
                            saveProductInfoCatalogToCache(key, (ProductInfoCatalog_V01)sender);
                        }
                    }
                    break;
                case CacheItemRemovedReason.Removed:
                case CacheItemRemovedReason.DependencyChanged:
                default:
                    break;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<string> GetSKUsByList(string locale, string platformName, string searchTerm)
        {
            return getSKUsByListFromCache(locale, platformName, searchTerm);
        }

        private static List<string> getSKUsByListFromCache(string locale, string platformName, string searchTerm)
        {
            List<string> result;

            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(platformName) || string.IsNullOrEmpty(searchTerm))
            {
                return null;
            }

            // gets cache key
            string cacheKey = getSKUByListCacheKey(locale, platformName, searchTerm);

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as List<string>;

            if (null == result || result.Count() == 0)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = GetSKUByListFromService(locale, platformName, searchTerm);
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveSKUByListToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static string getSKUByListCacheKey(string locale, string platformName, string searchTerm)
        {
            return PRODUCTINFO_BY_SEARCHTERM_CACHE_PREFIX + locale + "_" + platformName + " " + searchTerm;
        }

        private static void SaveSKUByListToCache(string cacheKey, List<string> searchResults)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     searchResults,
                                     null,
                                     Cache.NoAbsoluteExpiration,
                                     ProductInfoBySearchTermSlidingCache,
                                     CacheItemPriority.Normal,
                                     null);
        }

        public static List<string> GetSKUByListFromService(string locale, string platformName, string searchTerm)
        {
            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(platformName) || string.IsNullOrEmpty(searchTerm))
            {
                return null;
            }
            else
            {
                SearchProductsResponse_V01 response = null;

                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new SearchProductRequest_V01();
                        request.Platform = platformName;
                        request.Locale = locale;
                        request.SearchValue = searchTerm;
                        response = proxy.SearchProducts(new SearchProductsRequest(request)).SearchProductsResult;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success)
                        {
                            throw new ApplicationException("CatalogProvider.GetSKUByListFromService error. ");
                        }

                        // Filter results by active SKUs in the cache.
                        var resultSkus = response.SearchResultSkus;
                        resultSkus = resultSkus.Where(s => GetAllSKU(locale).ContainsKey(s)).ToList();

                        return resultSkus;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("loadCategoryFromService error, platform: {0}, locale:{1}, searchTerm:{2} {3}",
                                          platformName, locale, searchTerm, ex));
                    }
                }
            }
            return null;
        }

        public static bool IsDistributorExempted(string DistributorId)
        {
            bool result = false;
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                return result;
            }
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();

            if (null != member)
            {
                var ExemptedDS = GetExemptedDistributorsForCountry(member.Value.ProcessingCountryCode);
                if (ExemptedDS != null && ExemptedDS.Any(p => p.DistributorId.Trim().ToUpper() == DistributorId.ToUpper()))
                {
                    var IsExempt =
                        ExemptedDS.Find(
                            p =>
                            p.DistributorId.Trim().ToUpper() == DistributorId.ToUpper() && p.ApplicableCountry.Contains(currentLoggedInCounrtyCode));
                    if (IsExempt != null)
                    {
                        result = true;
                    }

                }
            }
            return result;
        }

        private static List<ExemptedDistributor_V01> GetExemptedDistributorsForCountry(string countryofprocessing)
        {
            return GetExemptedDistributorsForCountryCache(countryofprocessing);
        }

        private static List<ExemptedDistributor_V01> GetExemptedDistributorsForCountryCache(string countryofprocessing)
        {
            List<ExemptedDistributor_V01> result = null;

            if (string.IsNullOrEmpty(countryofprocessing))
            {
                return result;
            }

            // gets cache key
            string cacheKey = EXEMPTED_DS_CACHE_PREFIX + countryofprocessing;

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as List<ExemptedDistributor_V01>;

            if (result == null)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = LoadExemptedDistributorFromService(countryofprocessing);
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveExemptedDistributorToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static List<ExemptedDistributor_V01> LoadExemptedDistributorFromService(string countryofprocessing)
        {
            if (string.IsNullOrEmpty(countryofprocessing))
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        GetExemptedDistributorRequest_V01 request_V01 = new GetExemptedDistributorRequest_V01();
                        request_V01.CountryOfProcessing = countryofprocessing;
                        var response =
                            proxy.GetExemptedDistributors(new GetExemptedDistributorsRequest(request_V01)).GetExemptedDistributorsResult as GetExemptedDistributorResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.ExemptedDistributors == null)
                            throw new ApplicationException(
                                "CatalogProvider.LoadExemptedDistributorFromService error. GetExemptedDistributorResponse indicates error.");
                        return response.ExemptedDistributors.ToList();
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                   string.Format("LoadExemptedDistributorFromService error, country Code: {0} {1}", countryofprocessing, ex));
                    }
                }
            }
            return null;
        }

        private static void SaveExemptedDistributorToCache(string cacheKey, List<ExemptedDistributor_V01> ExemptedDistributors)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     ExemptedDistributors,
                                     null,
                                     DateTime.Now.AddMinutes(EXEMPTED_DS_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable,
                                     null);
        }

        #region Split order

        public static Dictionary<string, int> GetWhCodeAndQuantity(string sku, string countryCode, string currentWarehouse, int quantity, List<string> warehousesToSplit)
        {
            var qtyLines = new Dictionary<string, int>();
            var catalogItem = CatalogProvider.GetCatalogItem(sku, countryCode);
            var sysSkus = new List<string>()
                {
                    HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                    HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                    HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                };
            sysSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            if (!catalogItem.IsInventory || sysSkus.Contains(sku) || catalogItem.ProductType != ProductType.Product || catalogItem.IsFlexKit ||
                HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Contains(sku))
            {
                qtyLines.Add(currentWarehouse, quantity);
                return qtyLines;
            }

            var allWarehouses = new List<string> { currentWarehouse };
            allWarehouses.AddRange(warehousesToSplit.Where(w => w != currentWarehouse));
            var remainingQty = quantity;

            var backOrderWh = string.Empty;
            foreach (var warehouse in allWarehouses)
            {
                WarehouseInventory warehouseInv;
                if (catalogItem.InventoryList.TryGetValue(warehouse, out warehouseInv) && warehouseInv != null)
                {
                    var warehouseInv01 = warehouseInv as WarehouseInventory_V01;
                    if (warehouseInv01 != null)
                    {
                        if (warehouseInv01.QuantityAvailable > 0 && warehouseInv01.QuantityAvailable >= remainingQty)
                        {
                            qtyLines.Add(warehouse, remainingQty);
                            remainingQty -= remainingQty;
                        }
                        else if (warehouseInv01.IsBackOrder && string.IsNullOrEmpty(backOrderWh))
                        {
                            backOrderWh = warehouse;
                        }
                    }
                }
                if (remainingQty == 0)
                    break;
            }
            if (remainingQty > 0 && !string.IsNullOrEmpty(backOrderWh))
            {
                qtyLines.Add(backOrderWh, remainingQty);
            }

            return qtyLines;
        }

        #endregion

        #region President Summit

        private const int PresidentSummitEventId = 1160;
        private const int Honors2016EventId = 2462;

        public static int EventId
        {
            get
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.EventId))
                {
                    int value = 0;
                    int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out value);
                    return value;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static bool IsDisplayable(Category_V02 category, string locale)
        {
            if (locale == "en-US" || locale == "es-US")
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName) 
                    && category.DisplayName.Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    return DistributorOrderingProfileProvider.IsEventQualified(PresidentSummitEventId, locale);
                }
            }
            if (locale == "en-MX" &&
                !category.DisplayName.Equals("Event Category") &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName) &&
                !category.DisplayName.Trim().Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName.Trim()))
            {
                //For en-MX the only available category must be the special category
                return false;
            }
            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress && locale.Substring(3).Equals("MX"))
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName) 
                    && category.DisplayName.Trim().Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName.Trim()))
                {
                    return DistributorOrderingProfileProvider.IsEventQualified(Honors2016EventId, locale);
                }
            }
            if (locale.Substring(3).Equals("MX")
                && !HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName)
                && category.DisplayName.Trim().Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName.Trim()))
            {
                return false;
            }
            if (locale == "cs-CZ")
            {
                // US 262457: Do not display category if member is not qualified
                var eventId = 0;
                if (int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out eventId) && eventId > 0 
                    && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    var specialCategories = HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName.Split('|').ToList();
                    if (specialCategories.Count > 0 && specialCategories.Contains(category.DisplayName))
                    {
                        var memberWithTicket = false;
                        var memberQualified = DistributorOrderingProfileProvider.IsEventQualified(eventId, locale, out memberWithTicket);
                        return memberQualified && memberWithTicket;
                    }
                }
            }
            if (locale == "es-ES")
            {
                // User Story 408709:GDO Legacy: ES: EMEA Extravaganza Pre-sale Qualification Service Implementation
                var eventId = 0;
                bool isQualified = false;
                if (int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out eventId) && eventId > 0
                    && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
                {
                    var specialCategories = HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName.Split('|').ToList();
                    if (specialCategories.Count > 0 && specialCategories.Contains(category.DisplayName))
                    {
                        isQualified =  DistributorOrderingProfileProvider.IsEventQualified(eventId, locale);
                        return isQualified;
                    }
                }
            }

            if (locale == "ru-RU" && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName) 
                && category.DisplayName.Equals(HLConfigManager.Configurations.DOConfiguration.SpecialCategoryName))
            {
                // US 259914: Do not display category if member is not qualified
                return GetShippingProvider(locale.Substring(3)).HasAdditionalPickup();
            }

            //User Story 246155 to non display some skus for a non qualifying members
            if (EventId > 0 && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.SkuToNoDisplayForNonQualifyMembers))
            {
                List<string> listToHide =
                    HLConfigManager.Configurations.DOConfiguration.SkuToNoDisplayForNonQualifyMembers.Split(',').ToList();

                var skus = getSkus(category);

                if (skus.Any(listToHide.Contains))
                {
                    //get all the skus to non display
                    var actualSkus = skus.Where(listToHide.Contains).ToList();

                    disableProducts(category, actualSkus, DistributorOrderingProfileProvider.IsEventQualified(EventId, locale));
                }
            }

            return true;
        }

        #endregion

        #region private method

        private static List<string> getSkus(Category_V02 category)
        {
            List<string> Lkus = new List<string>();
            if (category.Products != null)
            {
                //var listin = category.Products;
                foreach (var productInfoV02 in category.Products)
                {
                    Lkus.AddRange(productInfoV02.SKUs.Select(x => x.SKU));
                }
            }
            if (category.SubCategories != null && category.SubCategories.Count() >= 1)
            {
                foreach (var subcat in category.SubCategories)
                {
                    Lkus.AddRange(getSkus(subcat));
                }
            }
            return Lkus;
        }

        private static void disableProducts(Category_V02 category, List<string> list, bool status)
        {
            foreach (var sku in list)
            {
                if (category.Products != null)
                {
                    foreach (var prod in category.Products)
                    {
                        var firstOrDefault = prod.SKUs.FirstOrDefault(x => x.SKU == sku);
                        if (firstOrDefault != null)
                        {
                            firstOrDefault.IsDisplayable = status;
                        }
                    }
                }

                if (category.SubCategories != null)
                {
                    foreach (Category_V02 sub in category.SubCategories)
                    {
                        disableProducts(sub, list, status);
                    }
                }
            }
            //return null;
        }


        #endregion private method

        #region Cache refresh verification

        /// <summary>
        /// The time to check the reference against the service.
        /// </summary>
        private static int CacheReferenceVerificationTime = Settings.GetRequiredAppSetting<int>("CacheVerificationMinutes");

        /// <summary>
        /// The key for the references cached.
        /// </summary>
        public const string CacheRefencePrefix = "CACHEREFERENCE";

        /// <summary>
        /// The cache catalog type.
        /// </summary>
        private const string CacheCatalogType = "Catalog";

        /// <summary>
        /// The cache product info type.
        /// </summary>
        private const string CacheProductInfoType = "ProductInfo";

        /// <summary>
        /// The cache inventory info type.
        /// </summary>
        private const string CacheInventoryType = "Inventory";

        private static string GetCacheReferenceKey(string locale, string type)
        {
            return string.Format("{0}_{1}_{2}", CacheRefencePrefix, locale, type);
        }

        private static string GetCacheReference(string locale, string type)
        {
            var referenceKey = GetCacheReferenceKey(locale, type);
            var reference = HttpRuntime.Cache[referenceKey] as string;
            return reference;
        }

        private static void SetCacheReference(string locale, string type, string reference)
        {
            var referenceKey = GetCacheReferenceKey(locale, type);
            if (!string.IsNullOrEmpty(reference))
            {
                HttpRuntime.Cache.Insert(referenceKey, reference.ToLower(), null,
                                         DateTime.Now.AddMinutes(CacheReferenceVerificationTime),
                                         Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                         OnCacheReferenceRemove);
            }
        }

        private static void OnCacheReferenceRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            if (reason == CacheItemRemovedReason.Expired)
            {
                var keyParts = key.Split('_');
                var localReference = sender as string;
                var serviceReference = GetReferenceFromService(keyParts[1], keyParts[2]);

                if (string.IsNullOrEmpty(serviceReference) || (!string.IsNullOrEmpty(localReference) && localReference.Equals(serviceReference)))
                {
                    if (!string.IsNullOrEmpty(serviceReference))
                    {
                        // Same reference
                        SetCacheReference(keyParts[1], keyParts[2], serviceReference);
                    }
                }
                else
                {
                    // Set a flag to refresh the values
                    SetCacheReference(keyParts[1], keyParts[2], "1");
                }
            }
        }

        private static string GetReferenceFromService(string locale, string type)
        {
            var reference = string.Empty;
            var typeList = new List<string> { CacheCatalogType, CacheProductInfoType, CacheInventoryType };
            if (!typeList.Contains(type))
            {
                return reference;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new GetCacheReferenceRequest_V01 { Locale = locale, Type = type, Platform = "MyHL" };
                        var response = proxy.GetCacheReference(new GetCacheReferenceRequest1(request)).GetCacheReferenceResult as GetCacheReferenceResponse_V01;

                        if (response == null || response.Status != ServiceResponseStatusType.Success)
                        {
                            throw new ApplicationException(
                                "CatalogProvider.GetReferenceFromService error. GetCacheReferenceResponse indicates error.");
                        }
                        reference = response.CacheReference;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(string.Format("GetReferenceFromService error, locale: {0} cache type: {1}: {2}", locale, type, ex));
                    }
                }
            }
            return reference;
        }

        #endregion
        
        #region BackOrderEnhancements


        private static Inventory_V01 GetInventoryCatalogForBackOrder(string locale)
        {
            Inventory_V01 result = GetInventoryCatalogForBackOrderFromCache(locale);
            return result;
        }


        private static Inventory_V01 GetInventoryCatalogForBackOrderFromCache(string locale)
        {
            Inventory_V01 result;

            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            string cacheKey = "BOFullInventory" + locale;

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as Inventory_V01;

            if (null == result || null == result.Items )
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = LoadInventoryCatalogForBackOrderFromService(locale);
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveInventoryCatalogForBackOrderToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static Inventory_V01 LoadInventoryCatalogForBackOrderFromService(string locale)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response = proxy.GetInventory(new GetInventoryRequest1(new GetInventoryRequest_V02
                    {
                        CountryCode = locale.Substring(3, 2),
                        Platform = "MyHL"
                    })).GetInventoryResult as
                        GetInventoryResponse_V01;

                    
                    if (response != null)
                    {
                        return response.Inventory;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
               string.Format("LoadInventoryCatalogForBackOrderFromService error, country: {0} - {1}", locale, ex));
                }
            }
            return null;
        }

        private static void SaveInventoryCatalogForBackOrderToCache(string cacheKey, Inventory_V01 Results)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     Results,
                                     null,
                                     Cache.NoAbsoluteExpiration,
                                     ProductInfoBySearchTermSlidingCache,
                                     CacheItemPriority.Normal,
                                     null);
        }


        public static BackOrderSKUDetailViewModel GetSingleSKUBackOrderDetails(string sku, string locale)
        {
            var cacheKey = string.Format("BackOrderDetailsViewModelTable_{0}", locale);
            var details = GetBackOrderDetailsViewModelFromCache(locale,cacheKey);
            if (details != null)
            {
                var skuDetails = details.FullCatalog.FirstOrDefault(x => x.SKU == sku) ??
                                 GetSKUBackOrderDetails(sku, locale);
                skuDetails.InventoryDetails = skuDetails.InventoryDetails.Where(x => x.Status != "N/A").OrderByDescending(d=>d.DeliveryType).ToList();
                return skuDetails;
            }
            return GetSKUBackOrderDetails(sku, locale);
        }


        public static List<WarehouseDetails> GetwareHouseDetailsforBackOrder(string locale)
        {
            var culture = CultureInfo.GetCultureInfo(locale);
            var listDetails = new List<WarehouseDetails>();
            var deliveryOptions = GetShippingProvider(locale.Substring(3).ToString()).GetDeliveryOptions(locale);
            var deliveryLocations = deliveryOptions.Where(x => x.Description != null && x.OrderCategory == ServiceProvider.ShippingSvc.OrderCategoryType.RSO).ToList();

            var optionList = new List<ServiceProvider.ShippingSvc.DeliveryOptionType>();
            var doConfig = HLConfigManager.CurrentPlatformConfigs[locale].DOConfiguration;
            var pickupConfigs = HLConfigManager.CurrentPlatformConfigs[locale].PickupOrDeliveryConfiguration;

            if (doConfig.AllowShipping)
                optionList.Add(ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping);
            if (pickupConfigs.AllowPickup)
                optionList.Add(ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup);
                    
            foreach (var deliveryLocation in deliveryLocations)
            {
                var currents = listDetails.Where(x => x.Name == deliveryLocation.Description)
                    .Where(y=>y.Option==deliveryLocation.Option.ToString())
                    .Where(c => c.WHCode == deliveryLocation.WarehouseCode);
                if (!currents.Any())
                    if (optionList.Contains(deliveryLocation.Option))
                {
                    WarehouseDetails detail = new WarehouseDetails();
                    detail.Name = deliveryLocation.Description;
                    detail.Option = deliveryLocation.Option.ToString();
                    detail.OptionText = GetBackOrderCommets(deliveryLocation.Option.ToString(), culture);
                    detail.WHCode = deliveryLocation.WarehouseCode;
                    listDetails.Add(detail);
                }
            }

            return listDetails.OrderByDescending(x=>x.Option).ToList();
        }

        public static BackOrderSKUDetailViewModel GetSKUBackOrderDetails(string sku, string locale)
        {
            var culture = CultureInfo.GetCultureInfo(locale);
            var response = new BackOrderSKUDetailViewModel();
            response.SKU = sku;
            var item = GetCatalogItem(sku, locale.Substring(3).ToString());
            if (null != item)
            {
                var deliveryOptions = GetShippingProvider(locale.Substring(3).ToString()).GetDeliveryOptions(locale);
                response.InventoryDetails = new List<BackOrderLocationViewModel>();
                response.SKUDescription = item.Description;
                if (null != item.InventoryList)
                {
                    var optionList = new List<ServiceProvider.ShippingSvc.DeliveryOptionType>();
                    var doConfig =
                HLConfigManager.CurrentPlatformConfigs[locale].DOConfiguration;
                    var pickupConfigs = HLConfigManager.CurrentPlatformConfigs[locale].PickupOrDeliveryConfiguration;

                    if (doConfig.AllowShipping)
                        optionList.Add(ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping);
                    if (pickupConfigs.AllowPickup)
                        optionList.Add(ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup);
                    foreach (var wh in item.InventoryList as WarehouseInventoryList)
                    {
                        var deliveryLocations =
                            deliveryOptions.Where(i => i.WarehouseCode == wh.Key).Where(x => x.Description != null).Where(y => optionList.Contains(y.Option)).Where(z => z.OrderCategory == ServiceProvider.ShippingSvc.OrderCategoryType.RSO).ToList();
                        
                        foreach (var deliveryOption in deliveryLocations)
                        {
                            SKU_V01 skuInfo;
                            var AllSKUS = CatalogProvider.GetAllSKU(locale);
                            
                            var exitInList =
                                response.InventoryDetails.Where(x => x.Location == deliveryOption.Description)
                                        .Where(f => f.DeliveryType == deliveryOption.Option.ToString());

                            if (optionList.Contains(deliveryOption.Option) && !exitInList.Any() && AllSKUS.TryGetValue(sku, out skuInfo))
                                {
                                    BackOrderLocationViewModel detail = new BackOrderLocationViewModel();
                                    detail.Location = deliveryOption.Description;
                                    detail.Type = GetBackOrderCommets(deliveryOption.Option.ToString(), culture);
                                    detail.DeliveryType = deliveryOption.Option.ToString();
                                    detail.WHCode = deliveryOption.WarehouseCode;
                                    #region status

                                    if (AllSKUS.TryGetValue(sku, out skuInfo))
                                    {
                                        MyHLShoppingCart cart = new MyHLShoppingCart();
                                        cart.DeliveryInfo = new ShippingInfo();
                                        cart.DeliveryInfo.Option = deliveryOption.Option;

                                        HLRulesManager.Manager.ProcessCatalogItemsForInventory(locale,
                                                                                               cart,
                                                                                               new List<SKU_V01>
                                                                                                   {
                                                                                                       skuInfo
                                                                                                   });
                                    var option = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), deliveryOption.Option.ToString());
                                    CatalogProvider.GetProductAvailability(skuInfo, wh.Key, option);
                                    if (skuInfo.ProductAvailability == ProductAvailabilityType.Available)
                                        {
                                            detail.Status = ProductAvailabilityType.Available.ToString();
                                            if (IsBlocked(skuInfo, wh.Key))
                                            {
                                                detail.Status = ProductAvailabilityType.Unavailable.ToString();
                                            }
                                        }
                                          
                                        else
                                        {
                                            if ((skuInfo.ProductAvailability == ProductAvailabilityType.AllowBackOrder && deliveryOption.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                                                || (skuInfo.ProductAvailability == ProductAvailabilityType.UnavailableInPrimaryWh && deliveryOption.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                                                )
                                            {
                                                detail.Status = ProductAvailabilityType.Unavailable.ToString();
                                            }
                                            else
                                                detail.Status = skuInfo.ProductAvailability.ToString();
                                        }
                                        
                                    }

                                    #endregion

                                    #region tooltipMessage

                                    StringBuilder tooltipmessage = new StringBuilder();
                                    //tooltipmessage.AppendLine("1");
                                    //if (deliveryLocation.Count() > 1)
                                    //{
                                    //tooltipmessage.Replace("1", "Ships To:");
                                    //var states = deliveryLocation.Select(x => x.State).Distinct().OrderBy(x => x);

                                    //foreach (var state in states)
                                    //{
                                    //  if (!string.IsNullOrEmpty(state))
                                    //    tooltipmessage.Append(state + ",");
                                    //}
                                    //}
                                    detail.ToolTipComments = tooltipmessage.ToString();

                                    #endregion

                                    var inventory = GetInventoryCatalogForBackOrder(locale);
                                    string etaNotAvailable = "";
                                    if (detail.Status == "Available")
                                    {
                                        detail.Comments = GetBackOrderCommets("Available", culture);
                                        detail.AvailabilityDate = "";
                                    }
                                    else if (detail.Status == "AllowBackOrder")
                                    {
                                        detail.AvailabilityDate = "";
                                        detail.Comments = GetBackOrderCommets("AllowBackOrder", culture);
                                    }
                                    else if (detail.Status == "UnavailableInPrimaryWh")
                                    {
                                        detail.AvailabilityDate = "";
                                        detail.Comments = GetBackOrderCommets("Available", culture);
                                    }
                                    else if (inventory != null && inventory.Items != null)
                                    {
                                        var inventorySku =
                                            inventory.Items.Where(x => x.Key == sku).FirstOrDefault().Value as
                                            InventoryItem_V01;
                                        if (inventorySku != null)
                                        {
                                            var whinventory =
                                                inventorySku.Warehouses.Where(
                                                    x => x.Key == deliveryOption.WarehouseCode.ToString())
                                                            .FirstOrDefault()
                                                            .Value as WarehouseInventory_V02;
                                            if (whinventory != null)
                                            {
                                                if (whinventory.BlockedReason != null)
                                                {
                                                var blockDetails = whinventory.BlockedReason.Split(',');
                                                if (blockDetails.Any())
                                                {
                                                    var globalResourceObject =
                                                        HttpContext.GetGlobalResourceObject("BackOrderResources",
                                                                                            "ETANotAvailableResource",
                                                                                                culture);
                                                    if (globalResourceObject != null && detail.Status != "Available")
                                                    {
                                                        etaNotAvailable = globalResourceObject.ToString();
                                                    }
                                                        detail.Comments = GetBackOrderCommets(blockDetails[0], culture);

                                                    if (blockDetails.Count() > 1)
                                                    {
                                                        detail.AvailabilityDate = blockDetails[1].Replace("ETA", "");
                                                    }
                                                    else
                                                    {
                                                        detail.AvailabilityDate = etaNotAvailable;
                                                    }
                                                }
                                                else
                                                {
                                                    detail.AvailabilityDate = "";
                                                    detail.Comments = "";
                                                }
                                            }
                                                else
                                                {
                                                    detail.AvailabilityDate = "";
                                                    detail.Comments = "";
                                        }
                                            }
                                        }
                                        else
                                        {
                                            detail.AvailabilityDate = etaNotAvailable;
                                            detail.Comments = "";
                                        }

                                    }
                                    else
                                    {
                                        detail.AvailabilityDate = etaNotAvailable;
                                        detail.Comments = "";
                                    }
                                    //json.AppendFormat(",hastooltip:{0}", tooltipmessage.Length > 1);
                                    response.InventoryDetails.Add(detail);
                                }
                        }
                    }
                }
            }
            response.InventoryDetails = response.InventoryDetails.OrderByDescending(x => x.DeliveryType).ToList();
            return response;
        }


        private static string GetBackOrderCommets(string option, CultureInfo culture)
        {
            object globalResourceObject;
            switch (option)
            {
                case "Product Availability":
                case "Quality":
                case "Delivery Delay":
                case "Planning":
                case "Materials":
                case "Inventory":
                case "Expiration":
                case "R&D":
                case "Manufacturing":
                case "Forecast":
                case "Pre-Launch":
                case "Auto Block":
                case "Other":
                   
                case "Auto Blocked":
                case "Regulatory":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "ProductNotAvailableResource", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "Discontinued":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "ProductDiscontinuedResource", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "Available":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "ProductAvailableResource", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "AllowBackOrder":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "BackOrderAvailableResource", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "NA":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "NonApplicableResource", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "Shipping":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "ShippingStatus", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;
                case "Pickup":
                    globalResourceObject = HttpContext.GetGlobalResourceObject("BackOrderResources", "PickUpStatus", culture);
                    if (globalResourceObject != null)
                        return globalResourceObject.ToString();
                    break;

                default :
                    return "";
                    
                    
            }
            return null;
        }


        //retrieve full catalog to Display table for Inventory page
        public static Task<BackOrderDetailsViewModel> GetBackOrderDetailsFullWh(string locale)
        {
            var response = Task<BackOrderDetailsViewModel>.Factory.StartNew(() => CompleteWH(locale));
            return response;
        }

        public static BackOrderDetailsViewModel CompleteWH(string locale)
        {
            var culture = CultureInfo.GetCultureInfo(locale);
            var cacheKey = string.Format("BackOrderDetailsViewModelTable_{0}", locale);
            var response = GetBackOrderDetailsViewModelFromCache(locale, cacheKey);
            if (response == null)
            {
                response = GetBackOrderDetailsViewModel(locale);
                if (response == null)
                    return null;

                foreach (var item in response.FullCatalog)
                {
                    foreach (var wh in response.WhDetails)
                    {
                        var hasWh = item.InventoryDetails.Where(x => x.Location == wh.Name && x.DeliveryType==wh.Option);
                        if (!hasWh.Any())
                        {
                            BackOrderLocationViewModel detail = new BackOrderLocationViewModel();
                            detail.Location = wh.Name;

                            detail.AvailabilityDate = "";
                            detail.Comments = GetBackOrderCommets("NA", culture);
                            detail.ToolTipComments = "";
                            detail.Type = GetBackOrderCommets(wh.Option, culture);
                            detail.DeliveryType = wh.Option;
                            detail.Status = GetBackOrderCommets("NA", culture);
                            detail.WHCode = wh.WHCode;
                            item.InventoryDetails.Add(detail);

                        }
                    }
                    item.InventoryDetails = item.InventoryDetails.OrderByDescending(x => x.Type).ToList();
                }
                
                SaveBackOrderDetailsViewModelToCache(string.Format("BackOrderDetailsViewModelTable_{0}", locale), response);
            }
            
            return response;
        }    

        public static Task<BackOrderDetailsViewModel> GetBackOrderDetails(string locale)
        {
            var response =  Task<BackOrderDetailsViewModel>.Factory.StartNew( () => GetBackOrderDetailsViewModel(locale));

            return response;
        }

        private static BackOrderDetailsViewModel GetBackOrderDetailsViewModel(string locale)
        {
            var doConfig = HLConfigManager.CurrentPlatformConfigs[locale].DOConfiguration;
            if (doConfig.DisplayBackOrderEnhancements)
            {
                var response = new BackOrderDetailsViewModel();

                response = new BackOrderDetailsViewModel();
                response.FullCatalog = new List<BackOrderSKUDetailViewModel>();
                response.WhDetails = GetwareHouseDetailsforBackOrder(locale);
                var fullCatalog = GetCatalog(locale.Substring(3)) as Catalog;
                var productsList = fullCatalog.Items.Where(x => x.Value.ProductType == ProductType.Product)
                                                                .Where(y => y.Value.IsInventory).OrderBy(k => k.Value.ProductType).ToList();
                foreach (var item in productsList)
                {
                    {
                        var result = GetSKUBackOrderDetails(item.Key, locale);
                        if (result.InventoryDetails.Count > 0)
                        {
                            if (doConfig.AllowShipping)
                            {

                                //filtering the skus sto display only when any of the shipping wh is unavailable
                                var shiippingWH = result.InventoryDetails.Where(x => x.DeliveryType == "Shipping");
                                
                                if (shiippingWH.Any(x => x.Status == "Unavailable"))
                                    response.FullCatalog.Add(result);
                            }
                            else
                            {
                                //filtering for pick up
                                var pickUpWH = result.InventoryDetails.Where(x => x.DeliveryType == "Pickup");
                                if (pickUpWH.Any(x => x.Status == "Unavailable"))
                                    response.FullCatalog.Add(result);
                            }
                        }
                    }
                }

                response.HasShipping = doConfig.AllowShipping;
                return response;
            }
            return null;
        }

        private static BackOrderDetailsViewModel GetBackOrderDetailsViewModelFromCache(string locale,string cacheKey)
        {
            var result = HttpRuntime.Cache[cacheKey] as BackOrderDetailsViewModel;
            return result;
        }

        private static void SaveBackOrderDetailsViewModelToCache(string cacheKey, BackOrderDetailsViewModel results)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     results,
                                     null,
                                     Cache.NoAbsoluteExpiration,
                                     ProductInfoBySearchTermSlidingCache,
                                     CacheItemPriority.Normal,
                                     null);
        }

        public static IShippingProvider GetShippingProvider(string countryCode)
        {
            return ShippingProvider.GetShippingProvider(countryCode);
        }

        public static bool IsBlocked(SKU_V01 sku, string warehouse)
        {
            var isBlocked = false;
            var wareHouse = warehouse;

            try
            {
                var inventory = sku.CatalogItem.InventoryList[wareHouse] as WarehouseInventory_V01;
                if (inventory != null)
                {
                    isBlocked = inventory.IsBlocked;
                }
            }
            catch
            { }

            return isBlocked;
        }

        
        #endregion


        #region DateExpiriSKU

        public static WarehouseInventory_V02 GetSkuExpiration(string locale, string sku, string whCode)
        {
            //validate Entry
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            var inventory = GetInventoryCatalogForBackOrder(locale);

            if (inventory != null && inventory.Items != null)
            {
                var inventorySku = inventory.Items.Where(x => x.Key == sku).FirstOrDefault().Value as
                                            InventoryItem_V01;
                if (inventorySku != null)
                {
                    var whinventory = inventorySku.Warehouses.Where(
                                                   x => x.Key == whCode)
                                                           .FirstOrDefault()
                                                           .Value as WarehouseInventory_V02;

                    if (whinventory != null && whinventory.BlockedReason != null)
                    {
                        var blockDetails = whinventory.BlockedReason.Split(',');
                        if (blockDetails.Any() && blockDetails[0] != null && blockDetails[0] == "Expiration")
                        {
                            return whinventory;
                        }
                    }
                }
            }
            return null;
        }
        
        #endregion

        public static void CloseShoppingCart(int shoppingCartId, string distributorId, string orderNumber, DateTime orderDate)
        {
            var proxy = new CatalogInterfaceClient();
            proxy.Endpoint.Address = new EndpointAddress(Settings.GetRequiredAppSetting("IACatalogUrl"));
            var request = new CloseShoppingCartRequest_V01()
            {
                ShoppingCartID = shoppingCartId,
                Distributor = distributorId,
                OrderNumber = orderNumber,
                OrderDate = orderDate
            };
            try
            {
                var response = proxy.CloseShoppingCart(new CloseShoppingCartRequest1(request)).CloseShoppingCartResult;
                if (response == null ||
                    response.Status != ServiceResponseStatusType.Success)
                {
                    LoggerHelper.Info(
                        string.Format("DeleteShoppingCartResponse indicates an error for OrderNumber: {0}, Status:{1}",
                                      orderNumber, response == null ? "null" : response.Status.ToString()));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format(
                    "Failed to Close Shoppingcart : orderNumber: {0}, exception: {1} ", orderNumber, ex.Message));
            }
        }

        #region Size Chart

        public static SizeChart_V01 GetSizeChartByProductID(string locale, int productID)
        {
            SizeChart_V01 sizeChart = null;
            List<SizeChart_V01> allSizeCharts = new List<SizeChart_V01>();

            // gets cache key
            string cacheKey = getSizeChartsCacheKey(locale);

            // tries to get object from cache
            allSizeCharts = HttpRuntime.Cache[cacheKey] as List<SizeChart_V01>;

            if (null == allSizeCharts || allSizeCharts.Count() == 0)
            {
                try
                {
                    allSizeCharts = getSizeChartsFromService(locale);
                    
                    // saves to cache if successful
                    if (null != allSizeCharts)
                    {
                        SaveSizeChartsToCache(cacheKey, allSizeCharts);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            
            if(allSizeCharts.Count() > 0)
            {
                sizeChart = allSizeCharts.Find(s => s.ProductID == productID);
            }

            return sizeChart;
        }

        private static List<SizeChart_V01> getSizeChartsFromService(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }
            else
            {
                CatalogInterfaceClient proxy = null;

                using (proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response =
                            proxy.GetSizeCharts(new GetSizeChartsRequest1(new GetSizeChartsRequest_V01(locale))).GetSizeChartsResult as GetSizeChartsResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.SizeCharts == null)
                        {
                            throw new ApplicationException(
                                "CatalogProvider.GetSizeCharts error. GetSizeCharts indicates error. Locale : " +
                                locale);
                        }

                        return response.SizeCharts;
        }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("getSizeChartsFromService error, locale: {0} {1}", locale, ex));
                    }
                }
            }

            return null;
        }

        private static string getSizeChartsCacheKey(string locale)
        {
            return SIZE_CHART_CACHE_PREFIX + locale;
        }

        private static void SaveSizeChartsToCache(string cacheKey, List<SizeChart_V01> sizeCharts)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     sizeCharts,
                                     null,
                                     DateTime.Now.AddMinutes(PRODUCTINFO_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);            
        }

        #endregion

        public static CatalogItemList GetDiscontinuedProductDetailFromCahce(List<string> skutobeQueried)
        {
            CatalogItemList result = new CatalogItemList();
            if (skutobeQueried == null || skutobeQueried.Count == 0)
                return new CatalogItemList();

            var locale = CultureInfo.CurrentCulture.Name;

            // gets cache key
            string cacheKey = GetDiscontinuedCacheKey(locale);
            result = HttpRuntime.Cache[cacheKey] as CatalogItemList;

            if (null == result)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = LoadCatalogItemListFromService(skutobeQueried);
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveCatalogItemList(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static void SaveCatalogItemList(string cacheKey, CatalogItemList result)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                    result,
                                    null,
                                    DateTime.Now.AddMinutes(CATALOG_CACHE_MINUTES),
                                    Cache.NoSlidingExpiration,
                                    CacheItemPriority.Normal,
                                    null);
        }

        private static CatalogItemList LoadCatalogItemListFromService(List<string> skutobeQueried)
        {
            return GetDiscontinuedProductDetail(skutobeQueried);
        }

        private static string GetDiscontinuedCacheKey(string locale)
        {
            return ALL_PRODUCTS_DISC_PREFIX + locale;
        }

        public static List<DiscontinuedSkuItemResponseViewModel> GetMobilePromoDiscotinued(List<ShoppingCartItem_V01> shoppingcartItems)
        {
            FreeSKUCollection freeSkuList = new FreeSKUCollection();
            var promoCollection = ChinaPromotionProvider.LoadPromoConfig();
            if (promoCollection != null && promoCollection.Any())
            {
                foreach (var item in promoCollection)
                    freeSkuList.AddRange(item.FreeSKUList);
            }
            List<string> freeSkuListOnly = freeSkuList.Select(item => item.SKU.Trim()).ToList();
            List<DiscontinuedSkuItemResponseViewModel> invalidPromo = (from items in shoppingcartItems where items.IsPromo where !freeSkuListOnly.Contains(items.SKU) select new DiscontinuedSkuItemResponseViewModel { Sku = items.SKU, ProductName = string.Empty }).ToList();
            if (invalidPromo.Any())
                return invalidPromo;
            return new List<DiscontinuedSkuItemResponseViewModel>();
        }
    }
}

