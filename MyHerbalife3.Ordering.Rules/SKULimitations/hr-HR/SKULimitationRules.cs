using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.hr_HR
{
    public class SKULimitationRules : MyHerbalifeRule, IShoppingCartRule
    {
        private List<string> baseIsolatedSkus = new List<string>() {};
        private List<string> baseStandAloneSkus = new List<string>() {};

        private List<string> IsolatedSkus { get; set; }
        private List<string> StandaloneSkus { get; set; }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
                {
                    RuleName = "SkuLimitation Rules",
                    Result = RulesResult.Unknown
                };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded && cart.CartItems != null)
            {
                DefineRuleSkus();
                // Check for isolated skus
                var isolated = cart.CartItems.FirstOrDefault(i => IsolatedSkus.Contains(i.SKU));
                string error = string.Empty;
                if (isolated != null && !isolated.SKU.Equals(cart.CurrentItems[0].SKU))
                {
                    error = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "StandaloneSku").ToString(), isolated.SKU);
                    result.Result = RulesResult.Failure;
                    if (!cart.RuleResults.Exists(r => r.Messages != null ? r.Messages.Any(m => m.Equals(error)) : false))
                    {
                        result.AddMessage(error);
                        cart.RuleResults.Add(result);
                    }
                    return result;
                }
                if (cart.CartItems.Any() && cart.CartItems.Select(s => s.SKU).Except(IsolatedSkus.Select(i => i)).Count() > 0 && IsolatedSkus.Contains(cart.CurrentItems[0].SKU))
                {
                    error = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "StandaloneSku").ToString(), cart.CurrentItems[0].SKU);
                    result.Result = RulesResult.Failure;
                    if (!cart.RuleResults.Exists(r => r.Messages != null ? r.Messages.Any(m => m.Equals(error)) : false))
                    {
                        result.AddMessage(error);
                        cart.RuleResults.Add(result);
                    }
                    return result;                    
                }

                // Check for not combinable skus
                if ((cart.CartItems.Any(i => StandaloneSkus.Contains(i.SKU)) && !StandaloneSkus.Contains(cart.CurrentItems[0].SKU)) ||
                    (cart.CartItems.Any(i => !StandaloneSkus.Contains(i.SKU)) && StandaloneSkus.Contains(cart.CurrentItems[0].SKU)))
                {
                    error = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "StandaloneSkus").ToString();
                    result.Result = RulesResult.Failure;
                    if (!cart.RuleResults.Exists(r => r.Messages != null ? r.Messages.Any(m => m.Equals(error)) : false))
                    {
                        result.AddMessage(error);
                        cart.RuleResults.Add(result);
                    }
                    return result;
                }
            }
     
            return result;
        }

        private void DefineRuleSkus()
        {
            StandaloneSkus =
                string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.StandaloneSkus)
                    ? baseStandAloneSkus
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.StandaloneSkus.Split(',').ToList();
            IsolatedSkus =
                string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.IsolatedSkus)
                    ? baseIsolatedSkus
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.IsolatedSkus.Split(',').ToList();
        }
    }
}
