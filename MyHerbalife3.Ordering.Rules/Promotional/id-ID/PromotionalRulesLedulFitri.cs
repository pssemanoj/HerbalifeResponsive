using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Catalog.ValueObjects;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;


namespace MyHerbalife3.Ordering.Rules.Promotional.id_ID
{
    public class PromotionalRulesLedulFitri : PromoRuleBase, IPromoRule
    {
        private readonly List<string> AllowedSKUToAddPromoSKU_K214 = new List<string> { "K426", "K427", "K428", "K429", "K430", "K431" };
        private readonly List<string> AllowedSKUToAddPromoSKU_K215 = new List<string> { "K432", "K433", "K434" };
        //private readonly List<string> AllowedSKUToAddPromoSKU_K214 = new List<string> { "0230", "0231", "0232", "3115", "0104" };
        //private readonly List<string> AllowedSKUToAddPromoSKU_K215 = new List<string> { "0233", "0111", "0130" };
        private string promoSkuK214 = "K435";
        private string promoSkuK215 = "K436";

        public PromotionalRulesLedulFitri()
        {
            MandatoryPromotionalStartDate = new DateTime(2014, 06, 07);
            MandatoryPromotionalEndDate = new DateTime(2020, 12, 31);
        }

        #region IShoppingCartRule implementation

        /// <summary>
        /// Performs shopping cart rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            if (cart.CurrentItems == null || cart.CurrentItems.Count == 0)
            {
                return result;
            }
            bool bIsPromoSKu = (cart.CurrentItems[0].SKU == promoSkuK214 || cart.CurrentItems[0].SKU == promoSkuK215);
            switch (reason)
            {
                case ShoppingCartRuleReason.CartItemsBeingAdded:
                    // Avoid the DS adds the promo sku to the cart
                    if (bIsPromoSKu)
                    {
                        cart.CurrentItems.Clear();
                    }
                    break;
                case ShoppingCartRuleReason.CartItemsBeingRemoved:
                    hlCart.IsPromoDiscarted = bIsPromoSKu ? true : hlCart.IsPromoDiscarted;
                    break;
                case ShoppingCartRuleReason.CartItemsAdded:
                case ShoppingCartRuleReason.CartItemsRemoved:
                    hlCart.IsPromoDiscarted = (reason == ShoppingCartRuleReason.CartItemsAdded) ? false : hlCart.IsPromoDiscarted;
                    if (!hlCart.IsPromoDiscarted)
                    {
                        result = CheckPromoIncart(hlCart, result, AllowedSKUToAddPromoSKU_K214, promoSkuK214);
                        result = CheckPromoIncart(hlCart, result, AllowedSKUToAddPromoSKU_K215, promoSkuK215);
                    }
                    break;
            }
            return result;
        }

        #endregion

        #region IPromoRule implementation

        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };

            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            // If it's a saved cart or a copy from order, check the promo in cart
            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (!hlCart.IsPromoDiscarted && (hlCart.IsSavedCart || hlCart.IsFromCopy))
                {
                    result.Add(CheckPromoIncart(hlCart, defaultResult, AllowedSKUToAddPromoSKU_K214, promoSkuK214));
                    result.Add(CheckPromoIncart(hlCart, defaultResult, AllowedSKUToAddPromoSKU_K215, promoSkuK215));
                }
            }
            return result;
        }

        #endregion
    }
}
