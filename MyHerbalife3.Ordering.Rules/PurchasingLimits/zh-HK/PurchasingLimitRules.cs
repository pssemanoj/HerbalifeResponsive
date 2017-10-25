using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.zh_HK
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                try
                {
                    bool IsWithOutHKID = true;
                    bool IsForeignDS = false;

                    var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                    bool IsProduct = currentItem != null && currentItem.ProductType == ProductType.Product;


                    var codes = new List<string>(CountryType.HK.HmsCountryCodes);
                    codes.Add(CountryType.HK.Key);

                    if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
                    {
                        //Local DS
                        var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                        if (tins.Count > 0)
                        {
                            var requiredTins = new List<string>(new[] { "HKID", "HKBL" });
                            var tin =
                                (from t in tins from r in requiredTins where t.IDType.Key == r select t).ToList();
                            if (tin != null && tin.Count > 0)
                            {
                                IsWithOutHKID = false;
                            }
                        }
                    }
                    else
                    {
                        //Foreign DS
                        IsForeignDS = true;
                    }

                    //Rule
                    if (IsWithOutHKID && !IsForeignDS && IsProduct)
                    {
                        //DS with COM as HK without HKID OR HKBL can purchase “L” and “A” type items only.
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase") as string);
                        cart.RuleResults.Add(Result);
                    }
                    if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(cart.DistributorID))
                    {
                        return base.PerformRules(cart, reason, Result);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add to Cart Rule for Hong Kong distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex));
                }
            }
            return Result;
        }
    }
}
