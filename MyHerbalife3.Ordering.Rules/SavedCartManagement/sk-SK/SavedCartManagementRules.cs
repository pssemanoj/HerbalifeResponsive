using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SavedCartManagement.sk_SK
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
                // Each time a saved cart or a copy from order is retrieved, set the freight code as default
                if (cart != null)
                {
                    var shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart != null && (shoppingCart.IsSavedCart || shoppingCart.IsFromCopy))
                    {
                        if (shoppingCart.DeliveryInfo != null)
                        {
                            //force shipping to have default freight code
                            if (shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                            {
                                shoppingCart.FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                                shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
