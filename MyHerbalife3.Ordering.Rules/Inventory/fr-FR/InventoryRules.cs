using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.Inventory.fr_FR
{
    public class InventoryRules : MyHerbalifeRule, IInventoryRule
    {
        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            bool isPickup = (cart != null && cart.DeliveryInfo != null &&
                             cart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup);
            //Dictionary<string, SKU_V01> allSKU = HL.MyHerbalife.Providers.Ordering.CatalogProvider.GetAllSKU(locale);
            //No BackOrder for French Territories
            foreach (SKU_V01 sku in itemList)
            {
                if (null != sku.CatalogItem && null != sku.CatalogItem.InventoryList)
                {
                    var warehouseF8 = sku.CatalogItem.InventoryList["F8"] as WarehouseInventory_V01;
                    if (null != warehouseF8)
                    {
                        warehouseF8.IsBackOrder = false;
                    }
                    var warehouseG4 = sku.CatalogItem.InventoryList["G4"] as WarehouseInventory_V01;
                    if (null != warehouseG4)
                    {
                        warehouseG4.IsBackOrder = false;
                    }
                }
            }
        }

        public void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            string warehouse = cart.DeliveryInfo == null
                                   ? HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                                   : cart.DeliveryInfo.WarehouseCode;
            if (warehouse == "F8" || warehouse == "G4")
            {
                if (cart.RuleResults == null)
                    cart.RuleResults = new List<ShoppingCartRuleResult>();
                var resulst = new ShoppingCartRuleResult();
                resulst.AddMessage(
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                        "NoBackOrdersAllowed") as string);
                resulst.Result = RulesResult.Failure;
                cart.RuleResults.Add(resulst);
                resulst.RuleName = "Back Order";
            }
        }

        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {
            
        }

    }
}