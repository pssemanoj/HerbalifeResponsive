using System.Collections.Generic;
using System.Linq;
using System.Web;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.es_CL
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const int NTSLines = 25;
        private const int ForeingDSLines = 20;

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded && cart != null)
            {

                var included = from i in cart.CartItems
                               from n in cart.CurrentItems
                               where i.SKU == n.SKU
                               select i;
                if (included != null && included.Count() == 0)
                {
                    List<string> codes = new List<string>(CountryType.CL.HmsCountryCodes);
                    codes.Add(CountryType.CL.Key);
                    if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
                    {
                        if (cart.CartItems.Count >= NTSLines)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "AnySKUQuantityExceeds").ToString(), NTSLines.ToString()));
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    else
                    {
                        if (cart.CartItems.Count >= ForeingDSLines)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "AnySKUQuantityExceeds").ToString(), ForeingDSLines.ToString()));
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                }

            }
            return Result;
        }
    }
}