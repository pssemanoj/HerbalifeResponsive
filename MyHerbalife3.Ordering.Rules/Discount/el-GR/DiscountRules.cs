using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.Discount.el_GR
{
    public class DiscountRules : MyHerbalifeRule, IDiscountRule
    {
        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason)
        {
            if (reason == ShoppingCartRuleReason.CartBeingCalculated)
            {
                MyHLShoppingCart shoppingCart = cart as MyHLShoppingCart;
                if (order != null && shoppingCart != null && shoppingCart.SelectedDSSubType == "RE")
                {
                    order.UseSlidingScale = false;
                    order.DiscountPercentage = 0;
                }
            }
        }

        public string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount)
        {
            return null;
        }
    }
}