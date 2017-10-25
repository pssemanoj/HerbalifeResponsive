using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IInventoryRule
    {
        void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList);
        void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item);
        void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity);
    }
}
