using System.Collections.Generic;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.es_VE
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        //private const decimal MaxVolPoints = 1000.00m;

        //private static bool VolumePointCheckRequired =
        //    Settings.GetRequiredAppSetting<bool>("VolumePointCheckRequired",false);
        
        public bool CanPurchase(string distributorID, string countryCode)
        {
            //bool canPurchase = false;
           var codes = new List<string>(CountryType.VE.HmsCountryCodes);

            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true); //HMS made up the country code
                //VRIF TinCode required to purchase
                foreach (TaxIdentification t in tins)
                {
                    if (t.IDType.Key == "VRIF")
                    {
                        return true;
                    }
                }
            }
            //foreign DS, Not in whitelist, can't purchase
            return CatalogProvider.IsDistributorExempted(distributorID);
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
        //    //1000 volume points check based on VolumePointCheckRequired

        //    if (VolumePointCheckRequired)
        //    {

        //        if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
        //        {
        //            try
        //            {
        //                var cartVolume = 0m;
        //                var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
        //                var myCart = cart as MyHLShoppingCart;
        //                if (myCart != null && !string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
        //                {
        //                    cartVolume = myCart.VolumeInCart;
        //                }
        //                var newVolumePoints = currentItem.VolumePoints*cart.CurrentItems[0].Quantity;

        //                if (cartVolume + newVolumePoints > MaxVolPoints)
        //                {
        //                    Result.AddMessage(
        //                        string.Format(
        //                            HttpContext.GetGlobalResourceObject(
        //                                string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds")
        //                                       .ToString(), cart.CurrentItems[0].SKU));
        //                    Result.Result = RulesResult.Failure;
        //                    cart.RuleResults.Add(Result);
        //                    return Result;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                LoggerHelper.Error(
        //                    string.Format(
        //                        "Error while performing Add to Cart Rule for Venezuala distributor: {0}, Cart Id:{1}, \r\n{2}",
        //                        cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
        //            }
        //        }
        //    }

            return Result;
        }
    }
}