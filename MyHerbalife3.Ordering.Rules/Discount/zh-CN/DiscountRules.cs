using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Rules.Promotional.zh_CN;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.Discount.zh_CN
{
    public class DiscountRules : MyHerbalifeRule, IDiscountRule
    {
        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason)
        {
            if (reason == ShoppingCartRuleReason.CartCalculated)
            {
                if (cart != null && (cart as MyHLShoppingCart).Totals != null)
                {
                    //call promotionalRules to calculate discount amount
                    (new PromotionalRules()).ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);
                }
            }
        }

        public string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount)
        {
            return null;
        }
    }
}