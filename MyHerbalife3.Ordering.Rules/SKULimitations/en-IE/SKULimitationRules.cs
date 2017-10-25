using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.en_IE
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly List<string> _restrictedDeliverySkus =
            new List<string>(new[] {"0343", "0344", "2329", "2328"});

        private string _restrictedFreightCode = "TNT";

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
            if (reason == ShoppingCartRuleReason.CartFreightCodeChanging)
            {
                if (cart.CurrentItems[0].SKU == _restrictedFreightCode)
                {
                    if ((_restrictedDeliverySkus.AsQueryable()
                                                .Intersect(from c in cart.CartItems select c.SKU)
                                                .Count()) > 0)
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "RestrictedShippingMethodForSku").ToString());
                        cart.RuleResults.Add(Result);
                        return Result;
                    }
                }
            }

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (_restrictedDeliverySkus.Contains(cart.CurrentItems[0].SKU))
                {
                    if (cart.FreightCode == _restrictedFreightCode)
                    {
                        CatalogItem catItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                        if (catItem != null)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "RestrictedSkuForShippingMethod").ToString(), cart.CurrentItems[0].SKU));
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