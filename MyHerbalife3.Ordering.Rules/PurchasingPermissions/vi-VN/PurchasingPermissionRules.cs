using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.vi_VN
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private const string ValidTin = "VNID";
       // private const string MandatoryNoteCode = "VNTRN";
        //private const string MandatoryNoteType = "MISC";

        public bool CanPurchase(string distributorID, string countryCode)
        {
            var canPurchase = false;
            var codes = new List<string>(CountryType.VN.HmsCountryCodes);
            codes.Add(CountryType.VN.Key);
            if (codes.Contains(DistributorProfileModel.ProcessingCountryCode))
            {
                return HasValidTin(distributorID);
                //var hasMandatoryNotes = HasMandatoryNotes(distributorID);
                //if (hasValidTin)
                //{
                    //if (DistributorProfileModel.TypeCode.ToUpper().Equals("SP"))
                    //{
                    //    canPurchase = HasMandatoryNotes(distributorID);
                    //}
                    //else
                    //{
                 //       canPurchase = true;
                    //}
               // }
            }
            return canPurchase;
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

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                try
                {
                    if (!CanPurchase(cart.DistributorID, Country))
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase") as string);
                        cart.RuleResults.Add(Result);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Error while performing Add to Cart Rule for Vietnamese distributor: {0}, Cart Id:{1}, \r\n{2}", cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
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
        //    DistributorOrderingProfile profile = DistributorOrderingProfileProvider.GetProfile(distributorID, Country);
        //    DistributorOrderingProfileProvider.GetDistributorNotes(profile, MandatoryNoteType, MandatoryNoteCode);
        //    return (profile.DistributorNotes != null && profile.DistributorNotes.Count > 0 && profile.DistributorNotes.Any(n => n.NoteCode == MandatoryNoteCode));
        //}
    }
}
