using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchaseRestriction;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.PF
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        private bool IsWHOrderForPF(ShoppingCart_V02 cart)
        {
            bool isWHOrderForPF = false;
            var myCart = cart as MyHLShoppingCart;

            if (myCart.ActualWarehouseCode == "PFT")
            {
                isWHOrderForPF = true;
            }
            return isWHOrderForPF;
        }

        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                             ShoppingCartRuleReason reason,
                                                             ShoppingCartRuleResult Result)
        {
            ///volume on orders placed against the French Polynesia warehouse in Oracle, only warehouses defined in “PFT” inventory org should be considered
            if (IsWHOrderForPF(cart))
                Result = base.PerformRules(cart, reason, Result);
            return Result;
        }
    }
}
