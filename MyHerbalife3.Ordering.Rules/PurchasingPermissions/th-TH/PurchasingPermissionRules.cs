using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Logging;
using CountryType = HL.Common.ValueObjects.CountryType;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.th_TH
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private string dummyTin = "TH00000000000";
        private bool IsWithOutTinCode = false;

        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = false;
            List<string> codes = new List<string>(CountryType.TH.HmsCountryCodes);
            codes.Add(CountryType.TH.Key);

            bool isCOPThai = codes.Contains(DistributorProfileModel.ProcessingCountryCode);

            var tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
            TaxIdentification tid = null;

            tid = tins == null ? null : tins.Find(t => t.IDType.Key == "THID");
            IsWithOutTinCode = tid == null;
            //COP = Thai, Tin = THID – Can place any item without any limit.
            if (isCOPThai && !IsWithOutTinCode)
            {
                canPurchase = true;
            }
            //Foreign DS 
            else
            {
                //with Thai Tin Code, Can place all items without any limitation.
                //COP Not Thai, Tin =Dummy tin Code – Can place any item (P,L, A and Volume limitation is set 1050 VP per order.)
                if (tins != null && tins.Find(t => t.ID == dummyTin) != null) //have dummyTIN
                {
                    IsWithOutTinCode = false;
                    canPurchase = true;
                }
                else if (!IsWithOutTinCode && codes.Contains(tid.ID))
                {
                    canPurchase = true;
                }
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
                CatalogItem_V01 currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);

                try
                {
                    List<string> codes = new List<string>(CountryType.TH.HmsCountryCodes);
                    codes.Add(CountryType.TH.Key);
                    bool isCOPThai = codes.Contains(DistributorProfileModel.ProcessingCountryCode);
                    TaxIdentification tid = null;

                    var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
                    tid = tins.Find(t => t.IDType.Key == "THID");
                    //Simulate the dummy tin with an foreign DS
                    //TaxIdentification ti = new TaxIdentification() { CountryCode = "TH", ID = "dummy", IDType = new TaxIdentificationType(dummyTin) };
                    //ods.Value.TinList.Add(ti);

                    if (CanPurchase(cart.DistributorID,Country))
                    {
                        //Additional Check to allow DS COP = Thai Tin = No TIN, Can place only L and A items
                        if (IsWithOutTinCode)
                        {
                            if (currentItem.ProductType == ProductType.Product)
                            {
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }
                    else
                    {
                        if (IsWithOutTinCode)
                        {
                            if (currentItem.ProductType == ProductType.Product)
                            {
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Error while performing Add to Cart Rule for Singapore distributor: {0}, Cart Id:{1}, \r\n{2}", cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
                }
            }
            return Result;
        }
    }
}
