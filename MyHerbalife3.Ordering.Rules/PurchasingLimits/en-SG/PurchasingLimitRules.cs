using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.en_SG
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        const decimal MaxVolPoints = 1100.00m;

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            Dictionary<int, PurchasingLimits_V01> currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            PurchasingLimits_V01 theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
            PurchaseLimitType limitsType = PurchaseLimitType.ProductCategory;
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);

            List<string> codes = new List<string>(CountryType.SG.HmsCountryCodes);
            codes.Add(CountryType.SG.Key);

            if (tins != null)
            {
                TaxIdentification tid = null;
                if ((tid = tins.Find(t => t.IDType.Key == "SNID")) != null)
                {
                    limitsType = PurchaseLimitType.None;
                }
                else
                {
                    // no SNID
                    if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
                    {
                        limitsType = PurchaseLimitType.None;
                    }
                }
            }

            // DS with Dummy TIN No "S0000000S", can purchase any category of products up to 1100 vp per order
            if (tins != null && tins.Find(t => t.ID == "S0000000S") != null)
            {
                limitsType = PurchaseLimitType.Volume;
                theLimits.RemainingVolume = MaxVolPoints;
            }

            if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId))
            {
                limitsType = PurchaseLimitType.Volume;
            }
            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);

            return currentLimits;
        }

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = true;
            List<string> codes = new List<string>(CountryType.SG.HmsCountryCodes);
            codes.Add(CountryType.SG.Key);
            
            //if (codes.Contains(ods.Value.ProcessingCountry.Key))
            //{
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            TaxIdentification tid = null;
            if ((tid = tins.Find(t => t.IDType.Key == "SNID")) != null)
            {
                if (tid.ID == "S0000000S")
                    isExempt = false;
            }
            //}

            return isExempt;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            decimal NewVolumePoints = 0m;
            decimal cartVolume = 0m;

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                if (currentItem == null)
                    return Result;

                List<string> codes = new List<string>(CountryType.SG.HmsCountryCodes);
                codes.Add(CountryType.SG.Key);

                var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                IPurchasingLimitManager manager = new PurchasingLimitManagerFactory().GetPurchasingLimitManager(cart.DistributorID);

                if (!codes.Contains(DistributorProfileModel.ProcessingCountryCode)) // foreign DS
                {
                    //Foreign DS without local National ID they cannot purchase "P" type products.
                    if (tins != null && tins.Find(t => t.IDType.Key == "SNID") == null)
                    {
                        if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    //Foreign DS with local National ID they can purchase P L and A type items.
                }
                if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(cart.DistributorID))
                {
                    return base.PerformRules(cart, reason, Result);
                }

                // DS with Dummy TIN No "S0000000S", can purchase any category of products up to 1100 vp per order
                if (tins != null && tins.Find(t => t.ID == "S0000000S") != null)
                {
                    MyHLShoppingCart myCart = cart as MyHLShoppingCart;
                    if (!string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                    {
                        cartVolume = myCart.VolumeInCart;
                    }

                    NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;

                    if (cartVolume + NewVolumePoints > MaxVolPoints)
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
                    else return Result;
                }
            }

            return Result;
        }

        public override bool PurchasingLimitsAreExceeded(string distributorId)
        {
            if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId))
            {
                return base.PurchasingLimitsAreExceeded(distributorId);
            }
            return false;
        }
    }
}