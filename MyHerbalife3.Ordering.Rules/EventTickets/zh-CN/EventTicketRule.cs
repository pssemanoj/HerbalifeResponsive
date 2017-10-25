using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.EventTickets.zh_CN
{
    public class EventTicketRule : MyHerbalifeRule, IShoppingCartRule
    {
        private IEventTicketProviderLoader _iEventTicketProviderLoader;
       // private const String EligiblecustType = "SP";
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "ETO Rules";
            defaultResult.Result = RulesResult.Unknown;
            if (null != cart)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }
            return result;
        }
        protected virtual ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                   ShoppingCartRuleResult result,
                                                   ShoppingCartRuleReason reason)
        {
            //Do the rules
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }

            else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (_iEventTicketProviderLoader == null)
                    _iEventTicketProviderLoader = new EventTicketProviderLoader();
                return CartItemBeingAddedRuleHandler(cart, result);
            }
            else if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                return CartRetrievedRuleHandler(cart, result);
            }

            return result;
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result)
        {
            try
            {
                var eTOresp = Providers.China.OrderProvider.GetEventEligibility(cart.DistributorID);
                var sessioninfo = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");
                if (sessioninfo != null && distributorOrderingProfile != null)
                {
                    if (sessioninfo.IsEventTicketMode)
                    {

                        if (!sessioninfo.IsReplacedPcOrder)
                        {

                            if (cart.CurrentItems != null && cart.CurrentItems.Any())
                            {
                                var eventTicketList = GetEventTicketList(cart);
                                if (eTOresp.IsEligible && eventTicketList != null)
                                {
                                    Dictionary<int, SKU_V01> eligibleEventProductList = null;
                                    ValidateMaxQtyDependsOnSpouseName(eventTicketList, cart, ref result,
                                                                      out eligibleEventProductList);
                                    foreach (var skuV01 in eventTicketList)
                                    {
                                        cart.CurrentItems.Remove(cart.CurrentItems.Find(x => x.SKU == skuV01.Value.SKU));
                                    }
                                    foreach (var skuV01 in eligibleEventProductList)
                                    {
                                        if (skuV01.Key > 0)
                                        {
                                            cart.CurrentItems.Add(new ShoppingCartItem_V01
                                            {
                                                SKU = skuV01.Value.SKU,
                                                Quantity = skuV01.Key,
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    cart.CurrentItems.Clear();
                                    result.Result = RulesResult.Feedback;
                                    var msg = GetRulesResourceString("YouAreNotEligibleForEvent");
                                    result.AddMessage(msg);
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            cart.CurrentItems.Clear();
                            result.Result = RulesResult.Feedback;
                            switch (eTOresp.Remark)
                            {
                                case "Invalid":
                                    var msg = GetRulesResourceString("NoValidEvent");
                                    result.AddMessage(msg);
                                    break;
                                case "StartDateGT":
                                    var msg1 = GetRulesResourceString("YouAreEligibleForEventPeriod");
                                    result.AddMessage(msg1);
                                    break;
                                case "InvalidEventPeriod":
                                case "InvalidEventPeriodNoSKU":
                                    var msg2 = GetRulesResourceString("InvalidEventPeriod");
                                    result.AddMessage(msg2);
                                    break;
                                case "SKUOutOfStock":
                                    var msg3 = GetRulesResourceString("SKUOutOfStock");
                                    result.AddMessage(msg3);
                                    break;
                            }
                            //var msg = GetRulesResourceString("YouAreNotEligibleForEvent");

                            return result;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Event Ticket Rules.Process Event,  DS:{0} locale:{1} ERR:{2}", cart.DistributorID, Locale, ex));
                cart.CurrentItems.Clear();
                result.Result = RulesResult.Failure;
                var msg = GetRulesResourceString("");
                result.AddMessage(msg);
            }
            return result;
        }

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart,
                                                                ShoppingCartRuleResult result)
        {
            var sessioninfo = SessionInfo.GetSessionInfo(cart.DistributorID, cart.Locale);
            var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");
            if (sessioninfo != null && distributorOrderingProfile != null)
            {
                if (sessioninfo.IsEventTicketMode)
                {

                    if (sessioninfo.IsReplacedPcOrder)
                    {
                        var shoppingCart = cart as MyHLShoppingCart;
                        var items = new List<string>();
                        if (cart.CartItems != null)
                        {
                            items.AddRange(cart.CartItems.Select(item => item.SKU));
                        }
                        if (items.Any())
                        {
                            if (shoppingCart != null) shoppingCart.DeleteItemsFromCart(items, true);
                        }

                    }
                }
            }
            return result;
        }


        private Dictionary<int, SKU_V01> GetEventTicketList(ShoppingCart_V01 cart)
        {

            var eventItemList = new Dictionary<int, SKU_V01>();
            if (Helper.Instance.HasData(cart.CurrentItems))
            {

                foreach (var currentitem in cart.CurrentItems)
                {
                    var AllSKUS = CatalogProvider.GetAllSKU(cart.Locale);
                    SKU_V01 sku_v01 = null;
                    AllSKUS.TryGetValue(currentitem.SKU, out sku_v01);
                    if (sku_v01 == null) continue;
                    if (sku_v01.Product.TypeOfProduct == ProductType.EventTicket)
                    {
                        var result = cart.CartItems.FirstOrDefault(x => x.SKU == sku_v01.SKU);
                        if (result != null)
                        {
                            eventItemList.Add(currentitem.Quantity + result.Quantity, sku_v01);
                            var shoppingCart = cart as MyHLShoppingCart;
                            var listitem = new List<string> { sku_v01.SKU };
                            if (shoppingCart != null)
                            {
                                shoppingCart.DeleteItemsFromCart(listitem, true);
                            }

                        }
                        else
                        {
                            eventItemList.Add(currentitem.Quantity, sku_v01);
                        }
                    }
                }
            }
            return eventItemList;


        }

        void ValidateMaxQtyDependsOnSpouseName(Dictionary<int, SKU_V01> eventProductList, ShoppingCart_V01 cart, ref ShoppingCartRuleResult result, out Dictionary<int, SKU_V01> eligibleEventProductList)
        {
            int maxAllowed = 0;
            string[] words = null;
            int Quantity = 0;
            string eligibleSKu = string.Empty;


            var allreadypurchasedEventTicktList = _iEventTicketProviderLoader.LoadSkuPurchasedCount(eventProductList, cart.DistributorID, Locale.Substring(3));
            var user = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, this.Country);
            var rsp = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetEventEligibility(cart.DistributorID);
            if (rsp != null && rsp.IsEligible)
            {
                words = rsp.Remark.Split('|');

                Quantity = Convert.ToInt32(words[words.Length - 1]);
                eligibleSKu = words[words.Length - 2];

            }
            var MaxEventTicketPerUser = Convert.ToInt32(HL.Common.Configuration.Settings.GetRequiredAppSetting("MaxEventTicketPerUser"));
            if (Quantity > 0 && Enumerable.Range(1, MaxEventTicketPerUser).Contains(Quantity))
            {
                maxAllowed = Quantity;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(user.SpouseLocalName))
                    maxAllowed = 1;
                else
                    maxAllowed = 2;
            }
            var CheckEventTicket = eventProductList.Any(p => p.Value.SKU == eligibleSKu);

            if (CheckEventTicket)
            {

                eligibleEventProductList = _iEventTicketProviderLoader.ValidateEventList(eventProductList, allreadypurchasedEventTicktList, maxAllowed, ref result);
            }
            else
            {
                maxAllowed = 0;
                eligibleEventProductList = new Dictionary<int, SKU_V01>();
                result.Result = RulesResult.Failure;
                var msg = GetRulesResourceString("YouAreNotEligibleForEvent");
                result.AddMessage(msg);
              
            }


        }

        protected string GetRulesResourceString(string key, string defaultValue = null)
        {
            var clsKey = string.Format("{0}_Rules", HLConfigManager.Platform);
            var value = HttpContext.GetGlobalResourceObject(clsKey, key);

            if (value == null || !(value is string))
            {
                HL.Common.Logging.LoggerHelper.Warn(string.Format("Missing {0} resource object. Key: {1}", clsKey, key));
                return string.Empty;
            }

            return value as string;
        }
    }
}
