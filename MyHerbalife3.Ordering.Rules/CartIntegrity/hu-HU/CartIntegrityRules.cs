using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.hu_HU
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const int RequiredVP = 500;

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
                reason == ShoppingCartRuleReason.CartCalculated || reason == ShoppingCartRuleReason.CartCreated)
            {
                if (cart != null)
                {
                    var myHLCart = cart as MyHLShoppingCart;
                    if (myHLCart != null)
                    {
                        var maxQty = GetFTMMaxQuantity(cart);
                        myHLCart.TodaysMagaZineQuantity = maxQty;

                        var tmSku = (from i in myHLCart.ShoppingCartItems
                                     where i.SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku
                                     select i).FirstOrDefault();
                        if (tmSku != null)
                        {
                            if (tmSku.Quantity > maxQty)
                            {
                                myHLCart.DeleteTodayMagazine(tmSku.SKU);
                                myHLCart.AddTodayMagazine(maxQty, tmSku.SKU);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private int GetFTMMaxQuantity(ShoppingCart_V02 cart)
        {
            var maxQty = HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax;
            if (cart == null)
            {
                return maxQty;
            }

            var myHLCart = cart as MyHLShoppingCart;
            if (myHLCart == null)
            {
                return maxQty;
            }

            maxQty += Convert.ToInt32(Math.Truncate(myHLCart.VolumeInCart / RequiredVP));
            return maxQty;
        }
    }
}