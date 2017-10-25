using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Rules.Inventory.Global
{
    public class InventoryRules : MyHerbalifeRule, IShoppingCartRule, IInventoryRule
    {
        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
            bool allowBackorder = HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder;
            if (!allowBackorder)
                return;
            var cart = shoppingCart as MyHLShoppingCart;
            bool isPickup = (cart != null && cart.DeliveryInfo != null &&
                             (cart.DeliveryInfo.Option == DeliveryOptionType.Pickup ||
                              cart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier || 
                              cart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier));
            bool isPickupFromCourier = (cart != null && cart.DeliveryInfo != null && cart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier);
            //if (isPickup)
            //{
            //    isPickup = !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickup;
            //}
            //Dictionary<string, SKU_V01> allSKU = CatalogProvider.GetAllSKU(locale);
            foreach (SKU_V01 sku in itemList)
            {
                if (sku.CatalogItem == null)
                    continue;

                if (sku.CatalogItem.InventoryList == null)
                    continue;

                foreach (WarehouseInventory_V01 warehouse in sku.CatalogItem.InventoryList.Values)
                {
                    //if (!allowBackorder)
                    //{
                    //    warehouse.IsBackOrder = false;
                    //    continue;
                    //}
                    if (sku.CatalogItem.IsEventTicket)
                    {
                        warehouse.IsBackOrder = false;
                        continue;
                    }
                    if (!warehouse.IsBlocked && warehouse.QuantityAvailable <= 0)
                    {
                        if (!isPickup) // shipping
                        {
                            if( HLConfigManager.Configurations.ShoppingCartConfiguration.allowBackorderForPromoTypeOnly)
                            {
                                warehouse.IsBackOrder = sku.CatalogItem.ProductType == ProductType.PromoAccessory;
                            }
                            else if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderInventorySKUOnly)
                            {
                                warehouse.IsBackOrder =
                                    sku.CatalogItem.ProductType == ProductType.Product;

                                if (!warehouse.IsBackOrder && HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPromoType)
                                {
                                    warehouse.IsBackOrder =
                                        sku.CatalogItem.ProductType == ProductType.PromoAccessory;
                                }
                            }
                            else
                            {
                                warehouse.IsBackOrder = true;
                            }
                        }
                        else
                        {
                            if(isPickupFromCourier && !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupFromCourier)
                            {
                                warehouse.IsBackOrder = false;
                                continue;
                            }
                            if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupAllTypes)
                            {
                                warehouse.IsBackOrder = true;
                            }
                            else if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickup)
                            {
                                warehouse.IsBackOrder =
                                    sku.CatalogItem.ProductType == ProductType.Product;
                            }
                            else
                            {
                                warehouse.IsBackOrder = false;
                            }
                        }
                    }
                }
            }
        }

        public void PerformBackorderRules(ShoppingCart_V02 shoppingCart, CatalogItem item)
        {
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "Inventory Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private bool isEventTicket(string distributorID, string locale)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (null != sessionInfo)
            {
                return sessionInfo.IsEventTicketMode;
            }
            return false;
        }

        //4	‘P’ type items will be allowed for Back order but ‘A’ and ‘L’ items, if out of stock at the warehouse will not be allowed to be backordered. 
        //a.	For items that are ‘Out of Stock’, and will be available for Back Order these items will be displayed in ‘Yellow’. (Green for Available and Red for Not Available is standard)
        //b.	When a Back Order item is added to the cart a message will be presented to alert the user that this item will be on Back Order.
        //c.	Blocks for items will over ride the item status.  If there is a block on the item then the item will not be available for purchase.
        //d.	Under NO circumstances Back Orders shall be allowed for shipping WH 25 and I1. Back orders will be allowed for product type “P” only (not “A” or “L”) for shipping WH I2.
        //e.	There will be an assumption that IBP’s will never be out of stock. Internet will not provide an exception list for ‘A’or ‘L’ items.
        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsAdded && HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder)
            {
                bool bEventTicket = isEventTicket(cart.DistributorID, Locale);
                var thisCart = cart as MyHLShoppingCart;
                if (null != thisCart)
                {
                    string warehouse = string.Empty;
                    if (thisCart.DeliveryInfo != null)
                    {
                        warehouse = thisCart.DeliveryInfo.WarehouseCode;
                    }
                    if (!string.IsNullOrEmpty(warehouse) && thisCart.CurrentItems != null)
                    {
                        var ALLSKUs = CatalogProvider.GetAllSKU(Locale, warehouse);
                        bool isAPFSkuPresent = APFDueProvider.IsAPFSkuPresent(thisCart.CurrentItems);
                        foreach (ShoppingCartItem_V01 cartItem in thisCart.CurrentItems)
                        {
                            SKU_V01 SKU_V01;
                            if (ALLSKUs.TryGetValue(cartItem.SKU, out SKU_V01))
                            {
                                if (SKU_V01.CatalogItem.InventoryList.Values.Where(
                                    i =>
                                    (i is WarehouseInventory_V01) &&
                                    (i as WarehouseInventory_V01).WarehouseCode == warehouse &&
                                    (i as WarehouseInventory_V01).IsBackOrder).Count() > 0)
                                {
                                    if (isAPFSkuPresent)
                                    {
                                        if (APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale))
                                        {
                                            Result.AddMessage(
                                                HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                    "NoBackOrdersOnAPFOrder") as string);
                                            Result.Result = RulesResult.Failure;
                                            cart.RuleResults.Add(Result);
                                            Result.RuleName = "Back Order";
                                            //return Result;
                                        }
                                    }

                                    var isSplitted = false;
                                    ShoppingCartProvider.CheckInventory(SKU_V01.CatalogItem, cartItem.Quantity, warehouse, thisCart.DeliveryInfo.FreightCode, ref isSplitted);
                                    if (!bEventTicket && !isSplitted &&
                                        HLConfigManager.Configurations.ShoppingCartConfiguration
                                                       .DisplayMessageForBackorder)
                                    {
                                        var errorMessage = HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                "BackOrderItem") ?? "The SKU {0} {1} added will be on back order.";
                                        Result.AddMessage(string.Format(errorMessage.ToString(), SKU_V01.SKU, SKU_V01.Description));
                                        
                                        Result.Result = RulesResult.Failure;
                                        cart.RuleResults.Add(Result);
                                        Result.RuleName = "Back Order";
                                        //return Result;
                                    }
                                }
                            }
                        }
                        return Result;
                    }
                }
            }
            return Result;
        }

        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {
            availQuantity = InventoryHelper.CheckInventory(shoppingCart, quantity, sku.CatalogItem, warehouse);
        }
    }
}