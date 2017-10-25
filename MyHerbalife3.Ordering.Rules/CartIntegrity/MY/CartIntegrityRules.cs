using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.CartIntegrity.MY
{
    public class CartIntegrityRules : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity_ExpireRules";
        
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            if (cart.RuleResults != null &&  cart.RuleResults.Any(x => x.RuleName == RuleName))
            {
                defaultResult = cart.RuleResults.FirstOrDefault(x => x.RuleName == RuleName);
            }
            else
            {
                defaultResult.RuleName = RuleName;
                defaultResult.Result = RulesResult.Unknown;
            }
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result)
        {
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                switch (reason)
                {
                    case ShoppingCartRuleReason.CartItemsBeingAdded:
                        {
                            GetMessagesForAddToCart(cart);
                        }
                        break;
                        case ShoppingCartRuleReason.CartRetrieved:
                        {
                            GetMessagesForEntireCart(cart, result);
                        }
                        break;
                        case ShoppingCartRuleReason.CartWarehouseCodeChanged:
                        {
                            Session["messagesToShow"] = null;
                            Session["showedMessages"] = null;
                            GetMessagesForEntireCart(cart, result);
                        }
                        break;
                    case ShoppingCartRuleReason.CartCreated:
                        {
                            Session["messagesToShow"] = null;
                            Session["showedMessages"] = null;
                            GetMessagesForEntireCart(cart, result);
                        }
                        break;
                        case ShoppingCartRuleReason.CartBeingClosed:
                        {
                            Session["messagesToShow"] = null;
                            Session["showedMessages"] = null;
                        }
                        break;
                        case ShoppingCartRuleReason.CartItemsBeingRemoved:
                        {
                            List<string> skusShowed = new List<string>();
                            skusShowed = (List<string>)Session["showedMessages"];
                            if (skusShowed != null && cart.CurrentItems != null)
                            {
                                foreach (var item in cart.CurrentItems)
                                {
                                    skusShowed.Remove(item.SKU);
                                }
                            }

                            Session["showedMessages"] = skusShowed;
                            Session["messagesToShow"] = null;
                        }
                        break;
                }
            }

            return result;
        }

        private void GetMessagesForAddToCart(ShoppingCart_V02 cart)
        {
            if (cart != null)
            {
                var myHLCart = cart as MyHLShoppingCart;
                if (myHLCart != null && cart.CurrentItems != null)
                {
                    var currentSKU = cart.CurrentItems[0].SKU;
                    if (!string.IsNullOrEmpty(currentSKU) && !string.IsNullOrWhiteSpace(currentSKU))
                    {
                        List<string> skusShowed = new List<string>();
                        skusShowed = (List<string>) Session["showedMessages"];
                        if (skusShowed != null)
                            skusShowed.Remove(currentSKU);
                        Session["showedMessages"] = skusShowed;
                        var cart2 = cart as MyHLShoppingCart;
                        //get the details from catalog provider
                        var details = CatalogProvider.GetSkuExpiration(cart.Locale, currentSKU, cart2.DeliveryInfo.WarehouseCode);
                        if (details != null)
                        {
                            var message = HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                "ShortExpirationDate") as string;
                            if (message != null)
                            {
                                var eta = details.BlockedReason.Split(',');
                                var fullmessage = string.Format(message, currentSKU,
                                                                eta[1].Replace("ETA", "").Trim());
                                List<string> messages = new List<string>();
                                var sesionValues = Session["messagesToShow"];
                                messages = (sesionValues != null ? (List<string>) sesionValues : new List<string>());
                                if (!messages.Any(k => k == fullmessage))
                                {
                                    messages.Add(string.Format("-SKU:{0}", currentSKU));
                                    messages.Add(fullmessage);
                                    Session["messagesToShow"] = messages;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetMessagesForEntireCart(ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {
            if (cart != null)
            {
                var myHLCart = cart as MyHLShoppingCart;
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
                                var message = HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "ShortExpirationDate") as string;
                                if (message != null)
                                {
                                    result.Result = RulesResult.Errors;
                                    var eta = details.BlockedReason.Split(',');
                                    var fullmessage = string.Format(message, item.SKU,
                                                                    eta[1].Replace("ETA", "").Trim());

                                    List<string> messages = new List<string>();
                                    var sesionValues = Session["messagesToShow"];
                                    messages = (sesionValues != null ? (List<string>) sesionValues : new List<string>());
                                    if (!messages.Any(k => k == fullmessage))
                                    {
                                        messages.Add(string.Format("-SKU:{0}", item.SKU));
                                        messages.Add(fullmessage);
                                        Session["messagesToShow"] = messages;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}