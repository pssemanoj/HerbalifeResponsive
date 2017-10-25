using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.es_CO
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule, IOrderManagementRule
    {
        private const string RuleName = "CartIntegrity Rules";

        public void PerformOrderManagementRules(ShoppingCart_V02 cart, Order_V01 order, string locale, OrderManagementRuleReason reason)
        {
            if (reason == OrderManagementRuleReason.OrderBeingSubmitted && cart != null)
            {
                var myHLCart = cart as MyHLShoppingCart;
                validateAndUpdateWH(ref myHLCart);
            }
        }

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
            if (reason == ShoppingCartRuleReason.CartCreated && cart != null)
            {
                var myHLCart = cart as MyHLShoppingCart;
                validateAndUpdateWH(ref myHLCart);
            }
            return result;
        }

        private void validateAndUpdateWH(ref MyHLShoppingCart hlCart)
        {
            if (hlCart != null && hlCart.DeliveryInfo != null && !string.IsNullOrEmpty(hlCart.FreightCode) &&
                hlCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
            {
                var shippingProvider = ShippingProvider.GetShippingProvider(Country);
                string warehouse = shippingProvider.GetWarehouseFromShippingMethod(hlCart.FreightCode, hlCart.DeliveryInfo.Address);
                if (!string.IsNullOrEmpty(warehouse))
                {
                    hlCart.DeliveryInfo.WarehouseCode = warehouse;
                }
            }
        }
    }
}
