using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.vi_VN
{
    // NOTE : THIS IS NOT IS USE ANYMORE
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private const string ValidTin = "VNID";
       // private const string MandatoryNoteCode = "VNTRN";
        //private const string MandatoryNoteType = "MISC";
        private const decimal MaxVolPoints = 1100.00m;

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool isExempt = base.DistributorIsExemptFromPurchasingLimits(distributorId);
            if (!isExempt)
            {
                return false;
            }

            var codes = new List<string>(CountryType.VN.HmsCountryCodes);
            codes.Add(CountryType.VN.Key);
            var hasValidTin = HasValidTin(distributorId);
            //var hasMandatoryNotes = HasMandatoryNotes(distributorId);

            //isExempt = codes.Contains(DistributorProfileModel.ProcessingCountryCode) && hasValidTin && hasMandatoryNotes;
            isExempt = codes.Contains(DistributorProfileModel.ProcessingCountryCode) && hasValidTin;
            return isExempt;
        }

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];
            var limitsType = PurchaseLimitType.ProductCategory;
            
            var codes = new List<string>(CountryType.VN.HmsCountryCodes);
            codes.Add(CountryType.VN.Key);

            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                var hasValidTin = HasValidTin(distributorId);
                //var hasMandatoryNotes = HasMandatoryNotes(distributorId);

                //if (hasValidTin && !hasMandatoryNotes && DistributorProfileModel.TypeCode.ToUpper().Equals("DS"))
                if (hasValidTin && DistributorProfileModel.TypeCode.ToUpper().Equals("DS"))
                {
                    limitsType = PurchaseLimitType.Volume;
                    theLimits.RemainingVolume = MaxVolPoints;
                }
            }
            currentLimits.Values.AsQueryable().ToList().ForEach(pl => pl.PurchaseLimitType = limitsType);
            return currentLimits;
        }

        List<ShoppingCartRuleResult> IShoppingCartRule.ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingLimits Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(performRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult performRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (!DistributorIsExemptFromPurchasingLimits(cart.DistributorID))
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

                    var codes = new List<string>(CountryType.VN.HmsCountryCodes);
                    codes.Add(CountryType.VN.Key);
                    //if (codes.Contains(DistributorProfileModel.ProcessingCountryCode) && !HasMandatoryNotes(cart.DistributorID))
                    if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
                    {
                        Result.Result = RulesResult.Feedback;
                        Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "NoDistributorNotes").ToString());
                        cart.RuleResults.Add(Result);
                    }
                }
            }
            return Result;
        }

        private bool HasValidTin(string distributorID)
        {
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
            return (tins != null && tins.Count > 0) ? tins.Any(t => t.IDType.Key == ValidTin) : false;
        }

        //private bool HasMandatoryNotes(string distributorID)
        //{
        //    DistributorOrderingProfile profile = DistributorOrderingProfileProvider.GetProfile(distributorID,Country);
        //    DistributorOrderingProfileProvider.GetDistributorNotes(profile, MandatoryNoteType, MandatoryNoteCode);
        //    return (profile.DistributorNotes != null && profile.DistributorNotes.Count > 0 && profile.DistributorNotes.Any(n => n.NoteCode == MandatoryNoteCode));
        //}
    }
}
