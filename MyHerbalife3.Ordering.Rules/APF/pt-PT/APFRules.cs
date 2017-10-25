using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Utilities;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Rules.APF.pt_PT
{
    public class APFRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "APF Rules";
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult { RuleName = "APF Rules", Result = RulesResult.Unknown, ApfRuleResponse = new ApfRuleResponse() };

            if (cart != null)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V01 cart, ShoppingCartRuleResult result, ShoppingCartRuleReason reason)
        {
            if (cart != null)
            {
                string level = "DS";
                if (DistributorProfileModel != null)
                {
                    level = DistributorProfileModel.TypeCode.ToUpper();
                }

                if (reason == ShoppingCartRuleReason.CartRetrieved)
                {
                    return CartRetrievedRuleHandler(cart, result, level);
                }
                else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
                {
                    return CartItemBeingAddedRuleHandler(cart, result, level);
                }
            }
            return result;
        }

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart, ShoppingCartRuleResult result, string level)
        {
            string key = string.Format("{0}_{1}_{2}", "JustEntered", cart.DistributorID, Locale);

            bool justEntered = (Session != null && ((null != Session[key]))) ? (bool)Session[key] : true;
            if (Session != null) Session[key] = null;

            string cacheKey = string.Empty; ////ShoppingCartProvider.GetCacheKey(distributor.Value.ID, Locale);
            bool reloadAPFs = (null != cart && null != cart.CartItems) &&
                              APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale) &&
                              !APFDueProvider.IsAPFSkuPresent(cart.CartItems);

            var myhlCart = cart as MyHLShoppingCart;
            bool shouldAddAPF = ShouldAddAPFPT(myhlCart);

            if (null == cart || null == cart.CartItems || cart.CartItems.Count == 0 || reloadAPFs ||
                APFDueProvider.IsAPFSkuPresent(cart.CartItems))
            {
                try
                {
                    if (null != cart)
                    {
                        if (APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale))
                        {
                            if (justEntered || !cart.APFEdited)
                            {
                                if (shouldAddAPF)
                                {
                                    DoApfDue(cart.DistributorID, cart, cacheKey, Locale, reloadAPFs, result, justEntered);
                                }
                                else
                                {
                                    myhlCart.DeleteItemsFromCart(null, true);   //remove items from the cart since TIN is not active or member only has mailing addresses in PT                      
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            if (APFDueProvider.IsAPFSkuPresent(cart.CartItems))
                            {
                                if (!shouldAddAPF)  //if APF shouldn't be present in cart
                                {
                                    myhlCart.DeleteItemsFromCart(null, true);   //remove items from the cart since TIN is not active                            
                                    return result;
                                }                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("APFRules.ProcessAPF DS:{0} locale:{2} ERR:{1}", cart.DistributorID,ex, Locale));
                }
            }
                        
            return result;
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result,
                                                                     string level)
        {
            var hlCart = cart as MyHLShoppingCart;
            if (null != hlCart)
            {
                foreach (ShoppingCartItem_V01 item in cart.CurrentItems)
                {
                    if (APFDueProvider.IsAPFSku(item.SKU))
                    {
                        bool canAddAPF = ShouldAddAPFPT(cart as MyHLShoppingCart);

                        if (!canAddAPF)
                        {
                            result.Result = RulesResult.Failure;
                            result.AddMessage(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CannotAddAPFSku")
                                as string);
                            var sku = APFDueProvider.GetAPFSku();
                            SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                        "CannotRemoveAPFSku");

                            cart.RuleResults.Add(result);
                            cart.APFEdited = false;
                            return result;
                        }
                    }
                }
                Global.APFRules globalRules = new Global.APFRules();
                hlCart.RuleResults = globalRules.ProcessCart(hlCart, ShoppingCartRuleReason.CartItemsBeingAdded);
            }
            return result;
        }

        private void SetApfRuleResponse(ShoppingCartRuleResult ruleResult, ApfAction action, string sku, string name, TypeOfApf apfType, string message)
        {
            ruleResult.ApfRuleResponse.Action = action;
            ruleResult.ApfRuleResponse.Sku = sku;
            ruleResult.ApfRuleResponse.Name = name;
            ruleResult.ApfRuleResponse.Type = apfType;
            ruleResult.ApfRuleResponse.Message = message;
        }

        private void DoApfDue(string distributorID,
                              ShoppingCart_V01 result,
                              string cacheKey,
                              string locale,
                              bool cartHasItems,
                              ShoppingCartRuleResult ruleResult,
                              bool justEntered)
        {
            var cart = result as MyHLShoppingCart;
            if (cart == null)
            {
                return;
            }

            try
            {
                string level;
                if (DistributorProfileModel != null)
                {
                    level = DistributorProfileModel.TypeCode.ToUpper();
                }
                else
                {
                    level = GetMemberLevelFromDistributorProfile(cart.DistributorID);
                }

                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID,
                                                                                               cart.CountryCode);
                if ((distributorOrderingProfile.HardCashOnly &&
                     !HLConfigManager.Configurations.PaymentsConfiguration.AllowWireForHardCash))
                {
                    return;
                }
                var apfItems = new List<ShoppingCartItem_V01>();
                if (cart.CartItems != null && cart.CartItems.Count > 0)
                {
                    //Stash off all non-APF items - to be re-added if appropriate
                    var nonApfItems =
                        (from c in cart.CartItems where APFDueProvider.IsAPFSku(c.SKU.Trim()) == false select c)
                            .ToList<ShoppingCartItem_V01>();
                    apfItems =
                        (from c in cart.CartItems where APFDueProvider.IsAPFSku(c.SKU.Trim()) select c)
                            .ToList<ShoppingCartItem_V01>();
                    if (nonApfItems.Count > 0 ||
                        HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                    {
                        // Clear the cart
                        cart.DeleteItemsFromCart(null, true);
                        //if (APFDueProvider.CanEditAPFOrder(distributorID, locale, level))
                        //{
                        //Global rule - they can always edit the cart ie add remove products at least
                        var list =
                            CatalogProvider.GetCatalogItems((from p in nonApfItems select p.SKU).ToList(), Country);
                        var products =
                            (from c in list where c.Value.ProductType == ProductType.Product select c.Value.SKU).ToList();
                        var nonproducts =
                            (from c in list where c.Value.ProductType != ProductType.Product select c.Value.SKU).ToList();
                        if (!HLConfigManager.Configurations.APFConfiguration.AllowNonProductItemsWithStandaloneAPF)
                        //We don't allow non product items alone on an apf order
                        {
                            if (products.Count == 0)
                            {
                                if (nonproducts.Count > 0)
                                {
                                    ruleResult.Result = RulesResult.Success;
                                    ruleResult.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "NonProductItemsRemovedForStandaloneAPF") as string);
                                    cart.RuleResults.Add(ruleResult);
                                }
                            }
                            else
                            {
                                cart.AddItemsToCart(nonApfItems, true);
                            }
                        }
                        else
                        {
                            if (!HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                            {
                                cart.AddItemsToCart(nonApfItems, true);
                            }
                        }
                    }
                }
                else if (null != cart && null != cart.RuleResults)
                {
                    var rules =
                        (from rule in cart.RuleResults
                         where rule.RuleName == RuleName && rule.Result == RulesResult.Failure
                         select rule);
                    if (null != rules && rules.Count() > 0)
                    {
                        cart.RuleResults.Remove(rules.First());
                    }
                }

                //Add the APF in
                var apfSku = new List<ShoppingCartItem_V01>();
                var sku = APFDueProvider.GetAPFSku();
                apfSku.Add(new ShoppingCartItem_V01(0, sku, 1, DateTime.Now));
                if (!cart.APFEdited)
                {
                    apfSku[0].Quantity = 1; //CalcQuantity(distributorOrderingProfile.ApfDueDate);
                    if (cart.CartItems.Exists(c => c.SKU == apfSku[0].SKU))
                    {
                        var apf =
                            (from a in cart.CartItems where a.SKU == apfSku[0].SKU select a).First();
                        cart.DeleteItemsFromCart(
                            (from a in cart.CartItems where a.SKU == apfSku[0].SKU select a.SKU).ToList(), true);
                    }
                    if (cart.CartItems.Count == 0) //This is now a Standalone APF
                    {
                        SetAPFDeliveryOption(cart);
                    }
                    cart.AddItemsToCart(apfSku, true);

                    if (justEntered)
                    {
                        ruleResult.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "APFDueAdded") as string);
                        ruleResult.Result = RulesResult.Success;

                        SetApfRuleResponse(ruleResult, ApfAction.None, sku, "ApfRule", TypeOfApf.CantDSRemoveAPF,
                            "APFDueAdded");

                        cart.RuleResults.Add(ruleResult);
                    }
                    else
                    {
                        foreach (ShoppingCartRuleResult r in cart.RuleResults)
                        {
                            if (r.RuleName == RuleName)
                            {
                                r.Messages.Clear();
                                r.AddMessage(string.Empty);
                            }
                        }
                    }
                }
                //else
                //{
                //    if (APFDueProvider.CanRemoveAPF(distributorID, locale, level))
                //    {
                //        cart.AddItemsToCart(apfSku, true);
                //    }
                //}
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("doAPFDue DS:{0} ERR:{1}", distributorID, ex));                
            }
        }

        private void SetAPFDeliveryOption(MyHLShoppingCart cart)
        {
            cart.CheckAPFShipping();
        }

        private static int CalcQuantity(DateTime apfDue)
        {
            var now = DateTime.Now;
            var ts = now.Subtract(apfDue);
            return ((ts.Days / 365) + 1);
        }

        private static string GetMemberLevelFromDistributorProfile(string distributorId)
        {
            return DistributorProfile(distributorId).TypeCode;
        }

        private bool ShouldAddAPFPT(MyHLShoppingCart cart)
        {
            //APF rules for PT
            bool shouldAddAPF = true;

            // Verify TIN is valid and non expired
            var tins = DistributorOrderingProfileProvider.GetTinList(cart.DistributorID, true);
            var ptTIN = tins.Find(p => p.IDType.Key == "POTX");
            var now = DateUtils.GetCurrentLocalTime("PT");
            bool hasForeignMailingAddress = false;
            
            var mailingAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, this.DistributorProfileModel.Id, "PT");
            if (mailingAddress != null)
            {
                if (!string.IsNullOrEmpty(mailingAddress.Country) && mailingAddress.Country != "PT")
                {
                    hasForeignMailingAddress = true;
                }
            }

            if (ptTIN != null)
            {
                if (ptTIN.IDType.ExpirationDate < now)
                {
                    shouldAddAPF = false;   //if TIN is not active, APF shouldn't be added
                }
            }
            else  //no POTX TIN found
            {
                if (!hasForeignMailingAddress)
                {
                    shouldAddAPF = false;   //Adding APF is not allowed since mailing address is still PT and member doens't have TIN                                            
                }
            }

            return shouldAddAPF;
        }
    }
}
