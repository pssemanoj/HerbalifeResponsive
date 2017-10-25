using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.APF.hu_HU
{
    public class APFRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string AlternAPFSku = "9910";

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult { RuleName = "APF Rules", Result = RulesResult.Unknown };

            if (cart != null)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V01 cart, ShoppingCartRuleResult result, ShoppingCartRuleReason reason)
        {
            if (cart != null)
            {
                string level = "DS";
                if (DistributorProfileModel != null)
                {
                    level = DistributorProfileModel.TypeCode.ToUpper();
                }

                if (reason == ShoppingCartRuleReason.CartRetrieved)
                {
                    return CartRetrievedRuleHandler(cart, result, level);
                }
            }
            return result;
        }

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart, ShoppingCartRuleResult result, string level)
        {
            if (cart != null)
            {
                var myhlCart = cart as MyHLShoppingCart;

                if (APFDueProvider.IsAPFSkuPresent(cart.CartItems) && myhlCart != null)
                {
                    // Change the APF sku according tins
                    var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                    var huvt = tins.Find(p => p.IDType.Key == "HUVT");
                    var hupt = tins.Find(p => p.IDType.Key == "HUPT");
                    if (huvt != null && hupt != null)
                    {
                        myhlCart.DeleteItemsFromCart(null, true);
                        myhlCart.AddItemsToCart(new List<ShoppingCartItem_V01>(new[] { new ShoppingCartItem_V01(0, AlternAPFSku, 1, DateTime.Now) }), true);
                        return result;
                    }
                    //Prevent to have a cart with skus Bug 148170
                    if (cart.CartItems.Count > 1)
                    {
                        //get the skus != apf
                        var nonApfItems =
                       (from c in cart.CartItems where APFDueProvider.IsAPFSku(c.SKU.Trim()) == false select c)
                           .ToList<ShoppingCartItem_V01>();
                        if (nonApfItems.Count > 0 ||
                            HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                        {
                            var skuToBeRemoved=nonApfItems.Select(x => x.SKU).ToList();
                            myhlCart.DeleteItemsFromCart(skuToBeRemoved);
                        }
                    }
                }
            }
            return result;
        }
    }
}