using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.ko_KR
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            var codes = new List<string>(CountryType.KR.HmsCountryCodes);
            codes.Add(CountryType.KR.Key);
            return (codes.Contains(this.DistributorProfileModel.ProcessingCountryCode));
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded ||
                reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
            {
                try
                {

                    if (!CanPurchase(cart.DistributorID, Country))
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "CountryOfProcessingOnly").ToString()));
                        cart.RuleResults.Add(Result);
                    }
                    else
                    {
                        string key = PaymentsConfiguration.GetPaymentInfoSessionKey(string.Empty, string.Empty);
                        var items = Session[key] as List<CreditPayment>;
                        if (null != items)
                        {
                            int maxCards = HLConfigManager.Configurations.PaymentsConfiguration.MaxCardsToDisplay;
                            if (items.Count(p => (!string.IsNullOrEmpty(p.AuthorizationCode))) == maxCards)
                            {
                                if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
                                {
                                    cart.CurrentItems.Clear();
                                }
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "MustDeleteACardToChangeItemsInCart").ToString()));
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add or Delete from Cart Rule for Korean distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}