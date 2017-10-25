using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.Inventory.iKiosk_Global
{
    public class InventoryRules : MyHerbalifeRule, IShoppingCartRule
    {
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "iKiosk Inventory Rules";
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

        private bool isNonInventoryItem(string sku)
        {
            bool IsNonInventory = false;

            var nonInventorySKUs = new List<string>()
                    {
                        HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                        HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                        HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                    };

            nonInventorySKUs.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            IsNonInventory = nonInventorySKUs.Contains(sku);

            return IsNonInventory;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                bool bEventTicket = isEventTicket(cart.DistributorID, Locale);
                var thisCart = cart as MyHLShoppingCart;
                if (null != thisCart)
                {
                    //Kiosk items must have inventory
                    string sku = thisCart.CurrentItems[0].SKU;
                    var catItem = CatalogProvider.GetCatalogItem(sku, Country);
                    WarehouseInventory warehouseInventory = null;

                    var isSplitted = false;
                    
                    if (thisCart.DeliveryInfo != null && thisCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping &&
                            HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Contains(thisCart.DeliveryInfo.WarehouseCode))
                    {
                        //split order scenario
                        ShoppingCartProvider.CheckInventory(catItem, thisCart.CurrentItems[0].Quantity, thisCart.DeliveryInfo.WarehouseCode, thisCart.DeliveryInfo.FreightCode, ref isSplitted);
                    }


                    if (!isSplitted && null != thisCart.DeliveryInfo && !string.IsNullOrEmpty(thisCart.DeliveryInfo.WarehouseCode) && !bEventTicket && !isNonInventoryItem(sku))
                    {
                        catItem.InventoryList.TryGetValue(thisCart.DeliveryInfo.WarehouseCode, out warehouseInventory);
                        if ((warehouseInventory != null && (warehouseInventory as WarehouseInventory_V01).QuantityAvailable <= 0) || warehouseInventory == null)
                        {
                            Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "BackOrderItem").ToString(), sku));
                            Result.Result = RulesResult.Failure;
                            cart.RuleResults.Add(Result);
                        }
                        else
                        {
                            int availQuantity = InventoryHelper.CheckInventory(thisCart, thisCart.CurrentItems[0].Quantity, catItem, thisCart.DeliveryInfo.WarehouseCode);
                            if (availQuantity - thisCart.CurrentItems[0].Quantity < 0)
                            {
                                Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OutOfInventory").ToString(), sku));
                                Result.Result = RulesResult.Failure;
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }
                }
            }

            return Result;
        }

        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse)
        {

        }
    }
}