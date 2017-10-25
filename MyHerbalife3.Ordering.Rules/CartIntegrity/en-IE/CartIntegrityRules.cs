using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.en_IE
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private List<string> CZETOSKUs = new List<string>(){"C610", "C676", "C877"};
        private const string CZWarehouse = "4S";

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                // Checking standalone
                result = CheckForCZETOSKUs(cart, result);
            }
            else if (reason == ShoppingCartRuleReason.CartWarehouseCodeChanged)
            {
                // Valid skus in cart
                result = CheckForCZETOWarehouse(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CheckForCZETOSKUs(ShoppingCart_V01 shoppingCart,
                                                         ShoppingCartRuleResult ruleResult)
        {
            if (shoppingCart != null)
            {
                if (CZETOSKUs.Contains(shoppingCart.CurrentItems[0].SKU))
                {
                    var cart = shoppingCart as MyHLShoppingCart;
                    if (cart != null && cart.DeliveryInfo != null &&
                        cart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        ruleResult.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "InvalidSKU")
                                           .ToString(), shoppingCart.CurrentItems[0].SKU));
                        ruleResult.Result = RulesResult.Failure;                        
                    }
                    else if (shoppingCart.CartItems != null && shoppingCart.CartItems.Any(i => !CZETOSKUs.Contains(i.SKU)))
                    {
                        ruleResult.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SKUCZLimitation")
                                           .ToString(), shoppingCart.CurrentItems[0].SKU));
                        ruleResult.Result = RulesResult.Failure;
                    }
                }
                else
                {
                    if (shoppingCart.CartItems != null && shoppingCart.CartItems.Any(i => CZETOSKUs.Contains(i.SKU)))
                    {
                        ruleResult.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SKUCZLimitation")
                                           .ToString(), shoppingCart.CurrentItems[0].SKU));
                        ruleResult.Result = RulesResult.Failure;
                    }                    
                }
            }
            return ruleResult;
        }

        private ShoppingCartRuleResult CheckForCZETOWarehouse(ShoppingCart_V01 shoppingCart,
                                                         ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null && cart.OrderCategory == OrderCategoryType.RSO && cart.DeliveryInfo != null &&
                cart.DeliveryInfo.Option == DeliveryOptionType.Shipping && shoppingCart.CartItems != null &&
                shoppingCart.CartItems.Count > 0)
            {
                var czItemsToRemove = from item in shoppingCart.CartItems
                    where CZETOSKUs.Contains(item.SKU)
                    select item;
                if (czItemsToRemove.Any())
                {
                    var notValidSkus = czItemsToRemove.Select(s => s.SKU).ToList();
                    cart.DeleteItemsFromCart(notValidSkus, true);

                    ruleResult.Result = RulesResult.Failure;
                    ruleResult.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SKUCZLimitation")
                                       .ToString(), shoppingCart.CurrentItems[0].SKU));
                }
            }
            else if (cart != null && cart.OrderCategory == OrderCategoryType.RSO && cart.DeliveryInfo != null &&
                     cart.DeliveryInfo.Option == DeliveryOptionType.Pickup && cart.DeliveryInfo.WarehouseCode == CZWarehouse &&
                     shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
            {
                var itemsToRemove = from item in shoppingCart.CartItems
                                    where !CZETOSKUs.Contains(item.SKU)
                                    select item;
                if (itemsToRemove.Any())
                {
                    var notValidSkus = itemsToRemove.Select(s => s.SKU).ToList();
                    cart.DeleteItemsFromCart(notValidSkus, true);

                    ruleResult.Result = RulesResult.Failure;
                    ruleResult.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SKUCZLimitation")
                                       .ToString(), shoppingCart.CurrentItems[0].SKU));
                }
                
            }

            return ruleResult;
        }
    }
}
