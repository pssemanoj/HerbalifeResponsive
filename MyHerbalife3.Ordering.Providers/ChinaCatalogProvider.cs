using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Providers.China
{
    public static partial class CatalogProvider
    {
        public const string INVENTORY_CACHE_PREFIX = "INVENTORY_";
        public const string SLOWMOVING_CACHE_PREFIX = "GETSLOWMOVING_";

        public static int INVENTORY_CACHE_MINUTES =
            Settings.GetRequiredAppSetting("InventoryCacheExpireMinutes", 10);

        public const string ANNOUNCEMENTINFO_CACHEKEY = "AnnouncementInfo";

        public static int ANNOUNCEMENTINFO_CACHE_MINUTES =
            Settings.GetRequiredAppSetting("AnnouncementInfoCacheExpireMinutes", 30);

        public static void LoadInventory(string storeID)
        {
            string cachekey = string.Format("{0}{1}", INVENTORY_CACHE_PREFIX, storeID);
            Inventory_V01 inventory = HttpRuntime.Cache[cachekey] as Inventory_V01;
            if (Settings.GetRequiredAppSetting("LogCatalogCN", "false").ToLower() == "true")
            {
                var enumerator = HttpRuntime.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var key = (string) enumerator.Key;
                    var value = enumerator.Value;
                    if (value == null)
                    {
                        LogRequest(string.Format("Cache Key: {0} is null", key));
                    }
                }
            }
            if (inventory == null)
            {
                inventory = GetInventory(storeID);
            }
            GetInventoryList(inventory, storeID);
            if (Settings.GetRequiredAppSetting("LogCatalogCN", "false").ToLower() == "true")
            {
                LogRequest(string.Format("Store ID: {0}", storeID));
                LogRequest(string.Format("Inventory loaded: {0}", (inventory != null).ToString()));
            }
        }

        private static void GetInventoryList(Inventory_V01 inventory, string storeId)
        {
            if (inventory != null && inventory.Items != null)
            {
                var catalog = Providers.CatalogProvider.GetCatalog("CN");

                if(catalog != null && catalog.Items != null && catalog.Items.Count > 0)
                {
                    foreach (string sku in catalog.Items.Keys)
                    {
                        // Find inventory item with a matching SKU.
                        if (!inventory.Items.ContainsKey(sku))
                        {
                            continue;
                        }
                        var catalogItem = catalog.Items[sku] as CatalogItem_V01;
                        var inventoryItem = inventory.Items[sku] as InventoryItem_V01;
                        if (catalogItem != null && inventoryItem != null)
                        {
                            if (catalogItem.InventoryList == null)
                            {
                                catalogItem.InventoryList = new WarehouseInventoryList();
                            }
                            //Bug 231703:SPLUNK LOGS: CNDO - object ref not set  because no checking on warehouse object
                                if (inventoryItem.Warehouses != null)
                                catalogItem.InventoryList[storeId] = inventoryItem.Warehouses[storeId];
                        }
                    }
                }
            }
        }

        public static void ClearCache(string storeID)
        {
            string cachekey = string.Format("{0}{1}", INVENTORY_CACHE_PREFIX, storeID);
            HttpRuntime.Cache.Remove(cachekey);
            var catalog = Providers.CatalogProvider.GetCatalog("CN");
            if (catalog == null)
            {
                LoggerHelper.Info("Clear Cache : No catalog");
                return;
            }
            var productCatalog = Providers.CatalogProvider.GetProductInfoCatalog("zh-CN");
            if (productCatalog == null)
            {
                LoggerHelper.Info("Clear Cache : No productCatalog");
                return;
            }
            var varSKU = productCatalog.AllSKUs;

            LoggerHelper.Info(string.Format("CN ClearCache {0}", storeID));
            foreach (var sku in varSKU)
            {
                if (catalog.Items.ContainsKey(sku.Value.SKU))
                {
                    sku.Value.CatalogItem = catalog.Items[sku.Value.SKU] as CatalogItem_V01;
                }
            }
        }


        /// <summary>
        /// get inventory for china store
        /// </summary>
        /// <param name="storeID"></param>
        /// <returns></returns>
        public static Inventory_V01 GetInventory(string storeID)
        {
            string cachekey = string.Format("{0}{1}", INVENTORY_CACHE_PREFIX, storeID);

            CatalogInterfaceClient proxy = null;

            using (proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetInventory(new GetInventoryRequest1(new GetInventoryRequest_V01 {CountryCode = "CN", StoreID = storeID})).GetInventoryResult as
                        GetInventoryResponse_V01;

                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success ||
                        response.Inventory == null)
                    {
                        throw new ApplicationException(
                            "CatalogProvider.GetInventory error. GetInventoryResponse indicates error. storeID : " +
                            storeID);
                    }
                    HttpRuntime.Cache.Insert(cachekey,
                                             response.Inventory,
                                             null,
                                             DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.NotRemovable,
                                             null);

                    if (Settings.GetRequiredAppSetting("LogCatalogCN", "false").ToLower() == "true")
                    {
                        LogRequest(string.Format("Store ID: {0}", storeID));
                        LogRequest(string.Format("GetInventory service response: {0}",
                                                 OrderCreationHelper.Serialize(response.Inventory)));
                    }

                    return response.Inventory;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("ChinaGetInventory error, storeID: {0} {1}", storeID, ex));
                }
            }
            return null;
        }

        private static void onInventoryCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    {
                        var parts = key.Split('_');
                        var storeId = (parts.Length == 2) ? parts[1] : parts[0];
                        var result = GetInventory(storeId);
                        GetInventoryList(result, storeId);

                        if (result != null)
                        {
                            // if success replace cache with new resultset
                            HttpRuntime.Cache.Insert(key,
                                                     result,
                                                     null,
                                                     DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                                                     Cache.NoSlidingExpiration,
                                                     CacheItemPriority.NotRemovable,
                                                     onInventoryCacheRemove);
                        }
                        else
                        {
                            // if failure re-insert from cache
                            HttpRuntime.Cache.Insert(key,
                                                     (Inventory_V01) sender,
                                                     null,
                                                     DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                                                     Cache.NoSlidingExpiration,
                                                     CacheItemPriority.NotRemovable,
                                                     onInventoryCacheRemove);
                        }
                    }
                    break;
                case CacheItemRemovedReason.Removed:
                case CacheItemRemovedReason.DependencyChanged:
                default:
                    break;
            }
        }

        #region "AnnouncementInfo"


        public static AnnouncementInfoList GetAnnouncementInfo()
        {
            return GetAnnouncementInfoFromCache();
        }


        private static AnnouncementInfoList GetAnnouncementInfoFromCache()
        {
            string cacheKey = GetAnnouncementInfoCacheKey();
            var result = HttpRuntime.Cache[cacheKey] as AnnouncementInfoList;
            if (result == null)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = GetAnnouncementInfoFromService();
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveAnnouncementInfoToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            return result;
        }

        private static string GetAnnouncementInfoCacheKey()
        {
            return ANNOUNCEMENTINFO_CACHEKEY;
        }

        private static AnnouncementInfoList GetAnnouncementInfoFromService()
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var response =
                    proxy.GetAnnouncementInfo(new GetAnnouncementInfoRequest(new AnnouncementInfoRequest_V01())).GetAnnouncementInfoResult as AnnouncementInfoResponse_V01;

                // Check response for error.
                if (response == null || response.Status != ServiceResponseStatusType.Success)
                {
                    throw new ApplicationException("CatalogProvider.GetAnnouncementInfo error.");
                }
                return response.Announcements;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("China GetAnnouncementInfo error: {0} ", ex));
            }
            return null;
        }

        public static AnnouncementInfoList GetChinaAnnouncementInfo(DateTime? fromDate, DateTime? toDate, string locale)
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var request = new AnnouncementInfoRequest_V01
                    {
                        BeginDate = fromDate,
                        EndDate = toDate,
                        Locale = locale
                    };

                var response =
                    proxy.GetAnnouncementInfo(new GetAnnouncementInfoRequest(request)).GetAnnouncementInfoResult as AnnouncementInfoResponse_V01;

                // Check response for error.
                if (response == null || response.Status != ServiceResponseStatusType.Success)
                {
                    throw new ApplicationException("CatalogProvider.GetChinaAnnouncementInfo error.");
                }
                return response.Announcements;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("China GetChinaAnnouncementInfo error: {0} ", ex));
            }
            return null;
        }

        private static void SaveAnnouncementInfoToCache(string cacheKey, AnnouncementInfoList results)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     results,
                                     null,
                                     DateTime.Now.AddMinutes(ANNOUNCEMENTINFO_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);
        }

        #endregion

        private static void LogRequest(string logData)
        {
            Logger.SetLogWriter(new LogWriterFactory().Create(), false);
            var entry = new LogEntry {Message = logData};
            Logger.Write(entry, "SelectedShippingAddress");
        }

        public static GetSlowMovingSkuList GetSlowmovingskuDetail()
        {
            string cachekey = string.Format("{0}{1}", SLOWMOVING_CACHE_PREFIX, "CN");

            CatalogInterfaceClient proxy = null;

            using (proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetSlowMovingSkuInfo(new GetSlowMovingSkuInfoRequest(new GetSlowMovingSkuRequest_V01 {CountryCode = "CN"})).GetSlowMovingSkuInfoResult as
                        GetSlowMovingSkuResponse_V01;

                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success)
                    {
                        throw new ApplicationException(
                            "ChinaCatalogProvider.GetSlowMoving error. GetSlowMovingSkuResponse indicates error. ");
                    }
                    //HttpRuntime.Cache.Insert(cachekey,
                    //                         response.GetSlowMovingSkuList,
                    //                         null,
                    //                         DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                    //                         Cache.NoSlidingExpiration,
                    //                         CacheItemPriority.NotRemovable,
                    //                         null);


                    return response.GetSlowMovingSkuList;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("ChinaCatalogProvider Slowmoving error,{0}", ex));
                }
            }
            return null;
        }

        public static GetSlowMovingSkuList LoadSlowMovingSkuInfo()
        {
            var cacheKey = string.Format("{0}{1}", SLOWMOVING_CACHE_PREFIX, "CN");
            var slowmovingdetail =
                CacheFactory.Create().Retrieve(_ => GetSlowmovingskuDetail(), cacheKey,
                    TimeSpan.FromMinutes(ANNOUNCEMENTINFO_CACHE_MINUTES));
              return slowmovingdetail;
        }

        public static string GetSlowMovingPopUp(MyHLShoppingCart shoppingCart, Dictionary<string, SKU_V01> AllSKUS,
                                                out int count,out bool display)
        {
            count = 0;
            display = false;
            if (shoppingCart == null || shoppingCart.CurrentItems == null)
                return string.Empty;

            var SlowMovingskuList = LoadSlowMovingSkuInfo();
            var str = new StringBuilder();
            
            if (SlowMovingskuList != null && SlowMovingskuList.Any() && shoppingCart.CartItems.Any())
            {
            
                str.Append("<ul>");
                var quentity = (from slowmovingsku in SlowMovingskuList
                                from CurrentItems in shoppingCart.CurrentItems
                                where CurrentItems.SKU.Equals(slowmovingsku.SKU.Trim())
                                select CurrentItems.Quantity).Sum();

                if (AllSKUS != null)
                {
    
                    str.Append(string.Format("{0} {1} ",
                      HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingHeader"),
                                             "<br>"));
                    str.Append(
                        string.Format(
                         HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingTotalQuentityMessagePart1")
                           + "{0}" +
                            HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingTotalQuentityMessagePart2"), quentity.ToString()));
                    foreach (var skudetail in SlowMovingskuList)
                    {
                        if (shoppingCart.CurrentItems.Any(x => x.SKU.Trim().Equals(skudetail.SKU)))
                        {
                            var skudescription = new SKU_V01();
                            AllSKUS.TryGetValue(skudetail.SKU, out skudescription);
                            if (skudescription != null)
                            {
                                str.Append(
                                    string.Format(
                                        "<li>" + HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingSku")+ "{0} {1}" +
                                     HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingSkuDescription") + "{2}" +
                                     HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "SlowMovingSkuUnit") + "</li>", skudetail.SKU,
                                       skudescription.Product.DisplayName + skudescription.Description,
                                        Math.Round(skudescription.CatalogItem.ListPrice - skudetail.DiscountAmount,
                                                   0)));
                            }

                            count++;
                        }

                    }
                  
                    str.Append("</ul>");
                    display = true;
                }
            }
            return str.ToString();
        }
    }
}