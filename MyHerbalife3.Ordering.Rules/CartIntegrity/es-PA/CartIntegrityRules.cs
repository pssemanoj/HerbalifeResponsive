using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.es_PA
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const int URBVP = 501;

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartItemsRemoved ||
                reason == ShoppingCartRuleReason.CartCalculated || reason == ShoppingCartRuleReason.CartCreated ||
                reason == ShoppingCartRuleReason.CartWarehouseCodeChanged || reason== ShoppingCartRuleReason.CartFreightCodeChanging)
            {
                if (cart != null)
                {
                    var myHLCart = cart as MyHLShoppingCart;
                    if (myHLCart != null && myHLCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                    {
                        var shippingProvider = ShippingProvider.GetShippingProvider(Country);
                        //get URB locales GetFreightCodeAndWarehouseFromService
                        var FCandWH = shippingProvider.GetFreightCodeAndWarehouse(myHLCart.DeliveryInfo.Address);
                        if (FCandWH != null && FCandWH[0]== "URB") 
                        {
                            //User Story 229708
                            //a.Orders under 500.9999 VP = Remains as URB freight code
                            if (myHLCart.VolumeInCart < URBVP)
                            {
                                myHLCart.DeliveryInfo.FreightCode =
                                    myHLCart.FreightCode = "URB";
                            }
                            //b.Order above 501.0000 VP = System should be redirected and calculate PMS freight code instead of URB.
                            else if ((myHLCart.VolumeInCart >= URBVP))
                            {
                                myHLCart.DeliveryInfo.FreightCode =
                                    myHLCart.FreightCode = "PMS";
                            }
                        }
                    }
                }
            }
            return result;
        }

    }
}