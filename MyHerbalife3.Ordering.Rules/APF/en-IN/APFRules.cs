using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.APF.en_IN
{
    public class APFRules : MyHerbalifeRule, IShoppingCartRule
    {
      
    
        private const string RuleName = "APF Rules";

      

        #region IShoppingCartRule interface implementation

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            defaultResult.ApfRuleResponse = new ApfRuleResponse();

            if (null != cart && cart.OrderCategory != OrderCategoryType.HSO)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }

            return result;
        }

        #endregion



        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            if (null != cart && ((reason == ShoppingCartRuleReason.CartCreated) || (reason == ShoppingCartRuleReason.CartRetrieved)))
            {
                var myhlCart = cart as MyHLShoppingCart;

                var sku = APFDueProvider.GetAPFSku();
                myhlCart.DeleteItemsFromCart(
                               (from a in cart.CartItems where a.SKU == sku.Trim() select a.SKU).ToList(), true);

            }
            return result;
        }


    }
}
