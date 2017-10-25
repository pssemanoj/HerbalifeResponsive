using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.ru_KZ
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private const decimal MaxVolPoints = 7500.00m;

        public bool CanPurchase(string distributorID, string countryCode)
        {
            return true;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
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
                List<string> codes = new List<string>(CountryType.KZ.HmsCountryCodes);
                codes.Add(CountryType.KZ.Key);
                bool isCOPKZ = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
                if (!isCOPKZ)
                    return Result;

                if (!HasActiveAppTinCode(cart.DistributorID))
                {
                    try
                    {
                        var cartVolume = 0m;
                        var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                        var myCart = cart as MyHLShoppingCart;
                        if (myCart != null && !string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                        {
                            cartVolume = myCart.VolumeInCart;
                        }
                        var newVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;

                        if (cartVolume + newVolumePoints > MaxVolPoints)
                        {
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds")
                                               .ToString(), cart.CurrentItems[0].SKU));
                            Result.Result = RulesResult.Failure;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "Error while performing Add to Cart Rule for Belarus distributor: {0}, Cart Id:{1}, \r\n{2}",
                                cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
                    }
                }
            }


            return Result;
        }

        private bool HasActiveAppTinCode(string distributorID)
        {
            bool hasActiveAppTinCode = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);

            var now = DateUtils.GetCurrentLocalTime("KZ");
            int numberTins = 0;
            foreach (TaxIdentification taxId in tins)
            {
                if (taxId.IDType != null && ((taxId.IDType.Key == "KZTX" || taxId.IDType.Key == "KZBL") && taxId.IDType.ExpirationDate > now))
                {
                    numberTins++;

                    if (numberTins == 2)    //DS should have both tins and active
                    {
                        hasActiveAppTinCode = true;
                        break;
                    }
                }
            }

            return hasActiveAppTinCode;
        }
    }
}
