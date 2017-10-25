using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.Inventory.zh_CN
{
    public class InventoryRules : MyHerbalifeRule, IInventoryRule
    {
        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
            MyHLShoppingCart cart = shoppingCart as MyHLShoppingCart;
            string warehouse = (cart == null || cart.DeliveryInfo == null || (cart.DeliveryInfo != null  && string.IsNullOrEmpty(cart.DeliveryInfo.WarehouseCode))) ? HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse : cart.DeliveryInfo.WarehouseCode;
            // catalog has been reloaded
            try
            {
                if (itemList != null && !itemList.Exists(i => i.CatalogItem != null && i.CatalogItem.InventoryList != null))
                {
                    LoggerHelper.Info(string.Format("CN Inventory {0}", warehouse));
                    Providers.China.CatalogProvider.ClearCache(warehouse);
                }
                Providers.China.CatalogProvider.LoadInventory(warehouse);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error in zh-CN ProcessCatalogItemsForInventory {0} : store {1}", ex.ToString(), warehouse));
            }
        }

        public void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item)
        {
        }

        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {
            availQuantity = InventoryHelper.CheckInventory(shoppingCart, quantity, sku.CatalogItem, warehouse);
        }

    }
}