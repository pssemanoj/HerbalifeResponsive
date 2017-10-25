using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.ValueObjects;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.en_SG
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private bool IsWithOutTinCode;
        private bool IsLocalDS;

        public bool CanPurchase(string distributorID, string countryCode)
        {
            var canPurchase = false;
            var codes = new List<string>(CountryType.SG.HmsCountryCodes);
            codes.Add(CountryType.SG.Key);
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
            IsWithOutTinCode = (tins.Count == 0 || !tins.Any(t => t.IDType.Key.Equals("SNID")));
            IsLocalDS = codes.Contains(DistributorProfileModel.ProcessingCountryCode);

            if (IsLocalDS)
            {
                //Must have SNID TinCode to purchase
                canPurchase = !IsWithOutTinCode;
            }
            else
            {
                canPurchase = true;
            }
            return canPurchase;
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
                var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);

                try
                {
                    if (!CanPurchase(cart.DistributorID, Country))
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                        cart.RuleResults.Add(Result);
                    }
                    else
                    {
                        //Additional Check to allow Foreign DS without TinCode to purchase L and A products
                        if (!IsLocalDS && IsWithOutTinCode && currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "PurchaseLimitTypeProductCategory").ToString(), cart.CurrentItems[0].SKU));
                            cart.RuleResults.Add(Result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add to Cart Rule for Singapore distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}