using System.Collections.Generic;
using System.Web.Security;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using HL.Common.ValueObjects;

namespace MyHerbalife3.Ordering.Rules.Discount.es_VE
{
    public class DiscountRules : MyHerbalifeRule, IDiscountRule
    {
        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale)
        {
            MyHLShoppingCart shoppingCart = cart as MyHLShoppingCart;
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();

            decimal CartVolume = 0.0M;
            foreach (ShoppingCartItem_V01 CartItem in shoppingCart.CartItems)
            {
                var item = CatalogProvider.GetCatalogItem(CartItem.SKU, shoppingCart.CountryCode);
                if (item != null)
                {
                    CartVolume += (item.VolumePoints) * CartItem.Quantity;
                }
            }

            List<string> codes = new List<string>(CountryType.VE.HmsCountryCodes);

            if (order != null && shoppingCart != null && CartVolume >= 500
                 && member.Value.TypeCode != "SP" && codes.Contains(member.Value.ProcessingCountryCode))
            {
                order.UseSlidingScale = false;
                order.DiscountPercentage = 42;
            }
        }

        public string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount)
        {
            return null;
        }


        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason)
        {
        }
    }
}