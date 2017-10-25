using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Rules.SavedCartManagement.ko_KR
{
    class SavedCartManagementRules : MyHerbalifeRule, ISavedCartManagementRule
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
                        shoppingCart.FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                        if (shoppingCart.DeliveryInfo != null)
                        {
                            shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                        }
                    }
                }
            }
            return result;
        }
    }
}
