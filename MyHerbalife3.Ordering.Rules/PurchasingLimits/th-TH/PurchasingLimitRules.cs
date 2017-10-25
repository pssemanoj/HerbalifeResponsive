using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.th_TH
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule
    {
        const decimal MaxVolPoints = 1050.00m;
        private bool isDummyTin = false;
        /// <summary>
        /// The IShoppingCart Rule Interface implementation
        /// </summary>
        /// <param name="cart">The current Shopping Cart</param>
        /// <param name="reason">The Rule invoke Reason</param>
        /// <param name="Result">The Rule Results collection</param>
        /// <returns>The cumulative rule results - including the results of this iteration</returns>
        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (cart.CurrentItems == null || cart.CurrentItems.Count == 0)
                {
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);

                List<string> codes = new List<string>(CountryType.TH.HmsCountryCodes);
                decimal NewVolumePoints = 0m;
                decimal cartVolume = 0m;
                TaxIdentification tid = null;
                string dummyTin = "TH00000000000";

                bool isCOPThai = codes.Contains(DistributorProfileModel.ProcessingCountryCode);

                var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                isDummyTin = tins.Find(t => t.ID == dummyTin) != null;
                codes.Add(CountryType.TH.Key);
                tid = tins.Find(t => t.IDType.Key == "THID");

                //COP = Thai Tin = No TIN, Can place only L and A items
                //COP Not Thai, No Tin- Can place L & A  item
                if ((isCOPThai && tid == null) || (!isCOPThai && tid == null && !isDummyTin))
                {
                    if (currentItem.ProductType == ProductType.Product)
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase") as string);
                        cart.RuleResults.Add(Result);
                    }
                }
                // marketing plan 
                if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(cart.DistributorID))
                {
                    return base.PerformRules(cart, reason, Result);
                }
                if (isCOPThai) //COP Thai
                {
                    if (isDummyTin)//have dummyTIN
                    {
                        MyHLShoppingCart myCart = cart as MyHLShoppingCart;
                        if (!string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                        {
                            cartVolume = myCart.VolumeInCart;
                        }
                        NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;
                        if (cartVolume + NewVolumePoints > MaxVolPoints)
                        {
                            Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds").ToString(), cart.CurrentItems[0].SKU.ToString()));
                            Result.Result = RulesResult.Failure;
                            cart.RuleResults.Add(Result);
                        }
                    }

                }
                else
                {
                    //COP Not Thai, Tin =Dummy tin Code – Can place any item (P,L, A and Volume limitation is set 1050 VP per order.)
                    if (isDummyTin)//have dummyTIN
                    {
                        MyHLShoppingCart myCart = cart as MyHLShoppingCart;
                        if (!string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                        {
                            cartVolume = myCart.VolumeInCart;
                        }
                        NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;
                        if (cartVolume + NewVolumePoints > MaxVolPoints)
                        {
                            Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds").ToString(), cart.CurrentItems[0].SKU.ToString()));
                            Result.Result = RulesResult.Failure;
                            cart.RuleResults.Add(Result);
                        }
                    }

                }

            }
            return Result;
        }

    }
}
