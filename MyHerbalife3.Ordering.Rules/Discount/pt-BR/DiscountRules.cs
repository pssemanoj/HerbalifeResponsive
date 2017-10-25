using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


namespace MyHerbalife3.Ordering.Rules.Discount.pt_BR
{
    public class DiscountRules : MyHerbalifeRule, IDiscountRule
    {
        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason)
        {
        }

        public string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount)
        {
            var myHLShoppingCart = cart as MyHLShoppingCart;
            if (myHLShoppingCart == null)
            {
                return string.Empty;
            }
            OrderTotals_V01 totals = myHLShoppingCart.Totals as OrderTotals_V01;
            decimal discount = (myHLShoppingCart.Totals != null && totals.VolumePoints > 0)
                                   ? totals.DiscountPercentage
                                   : dsDiscount;

            if (myHLShoppingCart.Totals != null && totals.VolumePoints > 4000)
            {
                return "1000-4000";
            }
            else if (discount.Equals(50))
            {
                return string.Empty;
            }
            else if (discount.Equals(42))
            {
                return "1000-4000";
            }
            else if (discount.Equals(35))
            {
                if (myHLShoppingCart.Totals != null)
                {
                    if (totals.VolumePoints < 1000)
                    {
                        return "500-999";
                    }
                    else
                    {
                        return "1000-4000";
                    }
                }
                else
                {
                    return "500-999";
                }
            }
            else if (discount.Equals(25))
            {
                if (myHLShoppingCart.Totals != null)
                {
                    if ( totals.VolumePoints >= 0 && totals.VolumePoints <= 499)
                    {
                        return "0-499";
                    }
                    else if (totals.VolumePoints >= 500 && totals.VolumePoints <= 999)
                    {
                        return "500-999";
                    }
                    else if (totals.VolumePoints > 999)
                    {
                        return "1000-4000";
                    }
                }
                else
                {
                    return "0-499";
                }
            }

            return string.Empty;
        }
    }
}