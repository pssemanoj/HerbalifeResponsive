using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.Inventory.pt_BR
{
    public class InventoryRules : MyHerbalifeRule, IInventoryRule
    {
        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
        }

        public void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null && cart.DeliveryInfo != null && item != null)
            {
                if (item.ProductType != ProductType.Product)
                {
                    if (cart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.ShipToCourier ||
                        cart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                    {
                        if (cart.RuleResults == null)
                            cart.RuleResults = new List<ShoppingCartRuleResult>();
                        var resulst = new ShoppingCartRuleResult();
                        resulst.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "NoBackOrdersAllowed") as
                            string);
                        resulst.Result = RulesResult.Failure;
                        cart.RuleResults.Add(resulst);
                        resulst.RuleName = "Back Order";
                    }
                }
            }
        }
        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {

        }
    }
}