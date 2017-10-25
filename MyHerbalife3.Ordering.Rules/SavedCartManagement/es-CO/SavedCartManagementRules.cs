using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SavedCartManagement.es_CO
{
    public class SavedCartManagementRules : MyHerbalifeRule, ISavedCartManagementRule
    {
        public List<ShoppingCartRuleResult> ProcessSavedCartManagementRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Saved Cart Management Rules",
                Result = RulesResult.Unknown
            };

            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                // Each time a saved cart or a copy from order is retrieved, check if courier location is available
                if (cart != null)
                {
                    var shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart != null && (shoppingCart.IsSavedCart || shoppingCart.IsFromCopy))
                    {
                        var shippingProvider = ShippingProvider.GetShippingProvider(Country);
                        if (shoppingCart.DeliveryInfo == null &&
                            shoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.PickupFromCourier)
                        {

                            var theOrderShippingAddress =
                                shippingProvider.GetShippingInfoFromID(shoppingCart.DistributorID, Locale,
                                                                       ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier,
                                                                       shoppingCart.DeliveryOptionID, 0);
                            if (theOrderShippingAddress == null)
                            {
                                if (shoppingCart.SaveCopyResults == null)
                                    shoppingCart.SaveCopyResults = new List<ShoppingCartRuleResult>();
                                if (shoppingCart.SaveCopyResults.All(x => x.RuleName != "PickUpLocationRemoved"))
                                {
                                    var notAvailable = new ShoppingCartRuleResult {RuleName = "PickUpLocationRemoved"};
                                    shoppingCart.SaveCopyResults.Add(notAvailable);
                                }
                                defaultResult.Result = RulesResult.Success;
                            }
                        }
                        string warehouse = shippingProvider.GetWarehouseFromShippingMethod(shoppingCart.FreightCode, shoppingCart.DeliveryInfo.Address);
                        if (!string.IsNullOrEmpty(warehouse))
                        {
                            shoppingCart.DeliveryInfo.WarehouseCode = warehouse;
                        }
                    }

                }
            }
            return result;
        }
    }
}
