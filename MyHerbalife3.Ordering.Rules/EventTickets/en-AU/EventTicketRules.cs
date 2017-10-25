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

namespace MyHerbalife3.Ordering.Rules.EventTickets.en_AU
{
    public class EventTicketRules : MyHerbalife3.Ordering.Rules.EventTicket.Global.EventTicketRules

    {
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                   ShoppingCartRuleResult result,
                                                   ShoppingCartRuleReason reason)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                MyHLShoppingCart shoppingCart = cart as MyHLShoppingCart;
                var items = shoppingCart.CartItems.SkipWhile(i => i.SKU.Equals(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku));
                if (!items.Any()) // standalone HFF
                {
                    return result;
                }
            }
            return base.PerformRules(cart, result, reason);
        }
    }
}
