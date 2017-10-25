using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Inventory
{
    public static class InventoryHelper
    {
        public static int CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, CatalogItem catalogItem, string warehouse)
        {
            string errorMessage = string.Empty;
            int newQuantity = quantity;

            if ( shoppingCart.RuleResults == null )
                shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            var freightCode = string.Empty;
            var isSplit = false;
            var myhlCart = shoppingCart as MyHLShoppingCart;
            if (myhlCart != null && myhlCart.DeliveryInfo != null)
            {
                freightCode = myhlCart.DeliveryInfo.FreightCode;
            }
            int availQuantity = ShoppingCartProvider.CheckInventory(catalogItem as CatalogItem_V01, newQuantity, warehouse, freightCode, ref isSplit);
            if (availQuantity == 0)
            {
                var resulst = new ShoppingCartRuleResult();
                errorMessage = (HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                "OutOfInventory") ?? string.Empty) as string;
                errorMessage = string.Format(errorMessage, catalogItem.SKU);
                resulst.AddMessage(errorMessage);
                resulst.RuleName = "Inventory Rules";
                resulst.Result = RulesResult.Failure;
                shoppingCart.RuleResults.Add(resulst);

            }
            else if (availQuantity != newQuantity)
            {
                var resulst = new ShoppingCartRuleResult();
                errorMessage = (HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                "LessInventory") ?? string.Empty) as string;
                errorMessage = string.Format(errorMessage, catalogItem.SKU, availQuantity);
                resulst.AddMessage(errorMessage);
                resulst.RuleName = "Inventory Rules";
                resulst.Result = RulesResult.Failure;
                shoppingCart.RuleResults.Add(resulst);
                HLRulesManager.Manager.PerformBackorderRules(shoppingCart, catalogItem);
            }
            else
            {
                availQuantity = quantity;
            }
            return availQuantity;
        }
    }
}
