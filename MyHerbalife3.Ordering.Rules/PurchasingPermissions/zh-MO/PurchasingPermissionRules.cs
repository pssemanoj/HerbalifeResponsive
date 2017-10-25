using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.zh_MO
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            return true;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                try
                {
                    if (CanPurchase(cart.DistributorID, Country))
                    {
                        MyHLShoppingCart myShoppingCart = ShoppingCartProvider.GetShoppingCart(cart.DistributorID, cart.Locale,true);
                        if (myShoppingCart != null && myShoppingCart.DeliveryInfo !=null && myShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                        {
                            if (myShoppingCart.VolumeInCart > 250M)
                            {
                                myShoppingCart.DeliveryInfo.FreightCode =
                                myShoppingCart.FreightCode = "NOF";
                            }
                            else
                            {
                                myShoppingCart.DeliveryInfo.FreightCode =
                               myShoppingCart.FreightCode = "MCF";
                            }
                            //ShoppingCartProvider.UpdateShoppingCart(myShoppingCart);
                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Error while performing Add to Cart Rule for Macau distributor: {0}, Cart Id:{1}, \r\n{2}", cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
                }
            }
            return Result;
        }
    }
}
