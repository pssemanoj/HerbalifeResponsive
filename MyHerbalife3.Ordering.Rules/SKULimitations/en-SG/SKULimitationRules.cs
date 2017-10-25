using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.en_SG
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        // This is not a requirement!!!
        private const int ForeingDSLines = 12;

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private int getPTypeSKUCount(ShoppingCartItemList cartItems, ShoppingCartItem_V01 currentItem)
        {
            int count = 0;
            SKU_V01 skuV01;

            Dictionary<string, SKU_V01> allSKUs = CatalogProvider.GetAllSKU(this.Locale);
            foreach (ShoppingCartItem_V01 item in cartItems)
            {
                if (allSKUs.TryGetValue(item.SKU, out skuV01))
                {
                    if (skuV01.CatalogItem != null &&
                        skuV01.CatalogItem.ProductType == ProductType.Product)
                    {
                        count += item.Quantity;
                    }
                }
            }
            if (currentItem != null) // include current item being added
            {
                if (allSKUs.TryGetValue(currentItem.SKU, out skuV01))
                {
                    if (skuV01.CatalogItem != null &&
                        skuV01.CatalogItem.ProductType == ProductType.Product)
                    {
                        count += currentItem.Quantity;
                    }
                }
            }
            return count;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            //45479, SG:Foreign ds with SNID and foreign DS with dummy tinid are both able to add more than 12 p type products to cart.
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                TaxIdentification tid = null;
                if (  tins != null &&  (tid=tins.Find(t => t.IDType.Key == "SNID")) != null) // have national ID
                {
                    if (tid.ID == "S0000000S" || !DistributorProfileModel.ProcessingCountryCode.Equals(CountryType.SG.Key))
                        // dummy TIN and foreign DS
                    {
                        if (getPTypeSKUCount(cart.CartItems, cart.CurrentItems[0]) > ForeingDSLines)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "AnySKUQuantityExceeds")
                                               .ToString(), ForeingDSLines.ToString()));
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
