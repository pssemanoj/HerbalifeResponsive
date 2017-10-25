using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.id_ID
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            return true;
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
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                try
                {
                    bool bCheckProductType = false;
                    {
                        var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);

                        if (tins.Count > 0)
                        {
                            if (tins.Find(t => t.IDType.Key == "IDID") == null) // DS has no national ID
                            {
                                bCheckProductType = true;
                            }
                        }
                        else
                        {
                            bCheckProductType = true;
                        }
                        if (bCheckProductType)
                        {
                            var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU,
                                                                             Country);
                            if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                            {
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add to Cart Rule for Indonesia distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}