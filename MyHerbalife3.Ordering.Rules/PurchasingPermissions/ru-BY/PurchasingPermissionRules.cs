using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.ru_BY
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private const decimal MaxVolPoints = 7500.00m;

        public bool CanPurchase(string distributorID, string countryCode)
        {
        //    List<string> codes = new List<string>(CountryType.BY.HmsCountryCodes);
        //    codes.Add(CountryType.BY.Key);
        //    return codes.Contains(DistributorProfileModel.ProcessingCountryCode);
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
            //List<string> codes = new List<string>(CountryType.BY.HmsCountryCodes);
            //codes.Add(CountryType.BY.Key);
            //bool isCOPBY = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
            //if (!isCOPBY)
            //    return Result;
            //if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            //{
            //    //if (HasActiveAppTinCode(cart.DistributorID))
            //    //{
            //        try
            //        {
            //            var cartVolume = 0m;
            //            var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
            //            var myCart = cart as MyHLShoppingCart;
            //            if (myCart != null && !string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
            //            {
            //                cartVolume = myCart.VolumeInCart;
            //            }
            //            var newVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;

            //            if (cartVolume + newVolumePoints > MaxVolPoints)
            //            {
            //                Result.AddMessage(
            //                    string.Format(
            //                        HttpContext.GetGlobalResourceObject(
            //                            string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds")
            //                                   .ToString(), cart.CurrentItems[0].SKU));
            //                Result.Result = RulesResult.Failure;
            //                cart.RuleResults.Add(Result);
            //                return Result;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            LoggerHelper.Error(
            //                string.Format(
            //                    "Error while performing Add to Cart Rule for Belarus distributor: {0}, Cart Id:{1}, \r\n{2}",
            //                    cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
            //        }
            //    //}
            //}


            return Result;
        }

        private bool HasActiveAppTinCode(string distributorID)
        {           
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
            bool hasActiveAppTinCode = tins.Count == 2 && 
                        tins.Any(l => l.IDType != null && l.IDType.Key == "BYTX" & l.IDType.ExpirationDate > DateUtils.GetCurrentLocalTime("BY")) &&
                        tins.Any(l => l.IDType != null && l.IDType.Key == "BYBL" & l.IDType.ExpirationDate > DateUtils.GetCurrentLocalTime("BY")) &&
                        tins.Any(l => l.IDType != null && l.IDType.Key == "BYID" & l.IDType.ExpirationDate > DateUtils.GetCurrentLocalTime("BY")) &&
                        tins.Any(l => l.IDType != null && l.IDType.Key == "BYNA" & l.IDType.ExpirationDate > DateUtils.GetCurrentLocalTime("BY"));

            return hasActiveAppTinCode;
        }
    }
}
