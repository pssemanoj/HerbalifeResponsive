using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.en_PH
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase
    {
        private const decimal MaxVolPoints = 1050M;
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            decimal NewVolumePoints = 0m;
            decimal cartVolume = 0m;

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);

                List<string> codes = new List<string>(CountryType.PH.HmsCountryCodes);
                codes.Add(CountryType.PH.Key);

                if (!codes.Contains(DistributorProfileModel.ProcessingCountryCode))
                {
                    MyHLShoppingCart myCart = cart as MyHLShoppingCart;

                    if (!string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                    {
                        cartVolume = myCart.VolumeInCart;
                    }

                    NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;

                    if (cartVolume + NewVolumePoints > MaxVolPoints)
                    {
                        var errorMessage =
                          HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                              "VolumePointExceeds") ??
                          "Item SKU:{0} has not been added to the cart since by adding that into the cart, you exceeded your volume points  limit.";
                        Result.AddMessage(string.Format(errorMessage.ToString(), cart.CurrentItems[0].SKU.ToString()));
                        Result.Result = RulesResult.Failure;
                        cart.RuleResults.Add(Result);
                    }

                }

            }

            return Result;
        }
    }
}
