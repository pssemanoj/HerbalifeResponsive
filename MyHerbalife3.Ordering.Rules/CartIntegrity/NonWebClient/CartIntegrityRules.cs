using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Configuration;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.NonWebClient
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        public const string RuleName = "CartIntegrity Rules";

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));

            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                string TodaysMagazineAllowedLoales = Settings.GetRequiredAppSetting("TodaysMagazineAllowedLocales","");
                if (cart.CurrentItems[0].SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku && !TodaysMagazineAllowedLoales.Contains(cart.Locale))
                {
                result = CheckForInvalidSKU(cart, result);
                result = CheckForInvalidQuantity(cart, result);
            }
            }

            else if (reason == ShoppingCartRuleReason.CartBeingClosed)
            {
                //for non-mobile clients, need to do all the usual cleanup done by web
                CleanupCart(cart as MyHLShoppingCart);
            }

            else if (reason == ShoppingCartRuleReason.CartItemsAdded)
            {
                if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
                {
                    GetMessagesForEntireCart(cart, result);
                }
            }

            return result;
        }

        #region CartIntegrity rule methods

        private void GetMessagesForEntireCart(ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {
            if (cart != null)
            {
                var myHLCart = cart as MyHLShoppingCart;
                List<string> messages = new List<string>();
                if (myHLCart != null && cart.CartItems != null)
                {
                    foreach (var item in cart.CartItems)
                    {
                        if (item != null && !string.IsNullOrEmpty(item.SKU) && !string.IsNullOrWhiteSpace(item.SKU))
                        {
                            //get the details from catalog provider
                            var details = CatalogProvider.GetSkuExpiration(cart.Locale, item.SKU,
                                                                           myHLCart.DeliveryInfo.WarehouseCode);
                            if (details != null)
                            {
                                string message = string.Empty;
                               
                                    result.Result = RulesResult.Feedback;
                                    var eta = details.BlockedReason.Split(',');
                                    
                                    if (eta.Count() > 1)
                                    {
                                        string skuExpirationDate = eta[1].Replace("ETA", "").Trim();
                                        string fullMessage = item.SKU + "," + skuExpirationDate;
                                        messages.Add(fullMessage);
                                        result.AddMessage(fullMessage);
                                        
                                    }
                                    else if (eta.Count() == 1)
                                    {
                                        string skuExpirationDate = eta[0].Replace("ETA", "").Trim();
                                        string fullMessage = item.SKU + ",";
                                        messages.Add(fullMessage);
                                        result.AddMessage(fullMessage);
                                        
                                    }
                                
                            }
                        }
                    }
                }
            }
        }

        private bool removeThisItem(string sku, ShoppingCart_V01 cart)
        {   
            var skuToExclude = new List<string>(new[]
                        {
                            HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                            HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                            HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                        });

            skuToExclude.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
            
            if (!skuToExclude.Any(s => s == sku))
            {
                return true;
            }
            return false;
        }

        private ShoppingCartRuleResult CheckForInvalidSKU(ShoppingCart_V01 shoppingCart,
                                                          ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {
                bool isValid = false;
                SKU_V01 testThisItem = null;
                var validSKUList = CatalogProvider.GetAllSKU(Locale);
                if (null != validSKUList)
                {
                    isValid = validSKUList.TryGetValue(cart.CurrentItems[0].SKU, out testThisItem);
                    if (isValid)
                    {
                        isValid = (null != testThisItem.CatalogItem);
                    }
                }

                if (!isValid)
                {
                    ruleResult.Result = RulesResult.Failure;
                    var errorMessage =
                           HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                               "SkuIsInvalid") ??
                           "SKU {0} is not available.";
                    ruleResult.AddMessage(string.Format(errorMessage.ToString(), cart.CurrentItems[0].SKU));
                }
            }
            return ruleResult;
        }

        private ShoppingCartRuleResult CheckForInvalidQuantity(ShoppingCart_V01 shoppingCart,
                                                               ShoppingCartRuleResult ruleResult)
        {
            var cart = shoppingCart as MyHLShoppingCart;
            if (cart != null)
            {                
                int maxQuantity = 0;

                if (cart.CurrentItems[0].SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                    || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(cart.CurrentItems[0].SKU)))
                {
                    maxQuantity = HLConfigManager.Configurations.DOConfiguration.HFFSkuMaxQuantity;
                }
                else
                {
                    maxQuantity = HLConfigManager.Configurations.ShoppingCartConfiguration.MaxQuantity;
                }
                bool isValid = true;
                isValid = (!(cart.CurrentItems[0].Quantity > maxQuantity));

                if (isValid)
                {
                    var item = cart.CartItems.Find(s => s.SKU == cart.CurrentItems[0].SKU);
                    if (null != item)
                    {
                        isValid = (!((cart.CurrentItems[0].Quantity + item.Quantity) > maxQuantity));
                    }
                }

                if (!isValid)
                {
                    ruleResult.Result = RulesResult.Failure;
                    var errorMessage =
                          HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                              "SkuQuantityInvalid") ??
                          "The quantity of SKU {0} is not valid.";
                    ruleResult.AddMessage(string.Format(errorMessage.ToString(), cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                }
            }
            return ruleResult;
        }

        private void CleanupCart(MyHLShoppingCart cart)
        {
            if (PurchasingLimitProvider.RequirePurchasingLimits(cart.DistributorID,cart.CountryCode))
            {
                PurchasingLimitProvider.ReconcileAfterPurchase(cart, cart.DistributorID, Country);
            }

            if (null != HttpContext.Current)
            {
                var currentSession = SessionInfo.GetSessionInfo(cart.DistributorID, Locale);
                if (currentSession != null)
                {
                    if (!String.IsNullOrEmpty(currentSession.OrderNumber))
                    {
                        currentSession.OrderNumber = String.Empty;
                        currentSession.OrderMonthShortString = string.Empty;
                        currentSession.OrderMonthString = string.Empty;
                        currentSession.ShippingMethodNameMX = String.Empty;
                        currentSession.ShippingMethodNameUSCA = String.Empty;
                        currentSession.ShoppingCart.CustomerOrderDetail = null;
                        // currentSession.CustomerPaymentSettlementApproved = false; Commented out for merge. Need to investigate
                        currentSession.CustomerOrderNumber = String.Empty;
                        currentSession.CustomerAddressID = 0;
                    }
                }
                //Clear the order month session... 
                HttpContext.Current.Session["OrderMonthDataSessionKey"] = null;
                SessionInfo.SetSessionInfo(cart.DistributorID, Locale, currentSession);
            }

            ShoppingCartProvider.UpdateInventory(cart, Country, Locale, true);
        }

        private void ResolveAPF(MyHLShoppingCart cart)
        {
            if (APFDueProvider.IsAPFSkuPresent(cart.CartItems))
            {
                int payedApf = APFDueProvider.APFQuantityInCart(cart);
                var currentDueDate = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, cart.CountryCode).ApfDueDate;
                var newDueDate = currentDueDate + new TimeSpan(payedApf*365, 0, 0, 0);
                DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, cart.CountryCode).ApfDueDate = newDueDate;

                //TODO : what to do
                //DistributorProvider.UpdateDistributor(ods);
                Session.Add("apfdue", newDueDate);
                APFDueProvider.UpdateAPFDuePaid(cart.DistributorID, newDueDate);
 
            }
        }

        #endregion
    }
}