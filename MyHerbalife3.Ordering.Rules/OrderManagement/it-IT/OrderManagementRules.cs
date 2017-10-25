using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Rules.OrderManagement.it_IT
{
    public class OrderManagementRules : MyHerbalifeRule, IOrderManagementRule
    {
        public void PerformOrderManagementRules(ShoppingCart_V02 cart, Order_V01 order, string locale, OrderManagementRuleReason reason)
        {
            if (reason == OrderManagementRuleReason.OrderFilled)
            {
                if (order.PurchasingLimits != null)
                {
                    var limits = (order.PurchasingLimits as PurchasingLimits_V01);
                    if (limits != null && string.IsNullOrEmpty(limits.PurchaseSubType))
                    {
                        var myHLCart = cart as MyHLShoppingCart;
                        if (myHLCart != null && string.IsNullOrEmpty(myHLCart.SelectedDSSubType))
                        {
                            limits.PurchaseSubType = myHLCart.SelectedDSSubType;
                        }
                        else
                        {
                            var  dsProfile  = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, order.CountryOfProcessing);
                            limits.PurchaseSubType = dsProfile.OrderSubType;
                        }
                    }
                }
            }
        }
    }
}
