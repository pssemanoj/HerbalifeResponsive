using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;

using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Inventory.it_IT
{
    public class InventoryRules : MyHerbalifeRule, IInventoryRule
    {
        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
            MyHLShoppingCart cart = shoppingCart as MyHLShoppingCart;
            bool isPickup = (cart != null && cart.DeliveryInfo != null &&
                             cart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup);
            //Dictionary<string, SKU_V01> allSKU = HL.MyHerbalife.Providers.Ordering.CatalogProvider.GetAllSKU(locale);
            foreach (SKU_V01 sku in itemList)
            {
                if (sku.CatalogItem == null)
                    continue;
                if (sku.CatalogItem.InventoryList == null)
                    continue;

                WarehouseInventory_V01 warehouse = sku.CatalogItem.InventoryList.ContainsKey("I2") ? sku.CatalogItem.InventoryList["I2"] as WarehouseInventory_V01 : null;
                if (warehouse != null)
                {
                    // Hard fast rule for IT - no A or L types backordered
                    if (sku.CatalogItem.ProductType == ProductType.Product && !warehouse.IsBlocked)
                    {
                        if (warehouse.QuantityAvailable <= 0)
                        {
                            warehouse.IsBackOrder = !isPickup;
                        }
                    }
                }
            }
        }

        public void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item)
        {
        }

        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {

        }
    }
}