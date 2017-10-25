using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.EventTickets.zh_CN
{
    public class EventTicketRules : MyHerbalife3.Ordering.Rules.EventTicket.Global.EventTicketRules
    {
        const string ThisClassName = "MyHerbalife3.Ordering.Rules.EventTickets.zh_CN.EventTicketRules";

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleResult result, ShoppingCartRuleReason reason)
        {
            try
            {
                if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
                {
                    StartLoggingTrace(cart);
                    LoggingTrace(string.Format("ShoppingCartRuleResult {0}", result));
                    LoggingTrace(string.Format("ShoppingCartRuleReason {0}", reason));

                    if (!CanProceed(result))
                    {
                        // result is in a state (already Failure?) where validation should't be bothered
                        LoggingTrace(string.Format("!CanProceed"));
                        return result; 
                    }

                    var eventProductList = GetEventProductList(cart);

                    if (!Helper.Instance.HasData(eventProductList.CurrentItemList))
                    {
                        LoggingTrace(string.Format("No ETO product involved."));
                        return result; 
                    }

                    if (!ValidateIsInEventMode(eventProductList, cart, result)) 
                    {
                        LoggingTrace(string.Format("!ValidateIsInEventMode"));
                        return result; 
                    }

                    if (!MyHerbalife3.Ordering.Providers.China.OrderProvider.IsEligibleForEvents(cart.DistributorID))
                    {
                        result.Result = RulesResult.Failure;
                        var msg = GetRulesResourceString("YouAreNotEligibleForEvent");
                        result.Messages.Add(msg);

                        LoggingTrace(string.Format("YouAreNotEligibleForEvent"));
                        return result;
                    }

                    if (!ValidateMaxQtyDependsOnSpouseName(eventProductList, cart, result)) 
                    {
                        LoggingTrace(string.Format("!ValidateMaxQtyDependsOnSpouseName"));
                        return result; 
                    }

                    MyHLShoppingCart shoppingCart = cart as MyHLShoppingCart;
                    var items = shoppingCart.CartItems.SkipWhile(i => i.SKU.Equals(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku));
                    if (!items.Any()) // standalone HFF
                    {
                        LoggingTrace(string.Format("standalone HFF"));
                        return result;
                    }
                }
                return base.PerformRules(cart, result, reason);
            }
            catch(Exception ex)
            {
                var errMsg = string.Format("{0}.PerformRules() error : {1}", ThisClassName, ex);
                LoggerHelper.Error(errMsg);

                result.Result = RulesResult.Failure; // only this can stop the event item from being processed
                result.Messages.Add("ETO encountered internal error");

                LoggingTrace(string.Format("internal error: {0}", errMsg));

                return result;
            }
            finally
            {
                CommitTrace(cart);
            }
        }

        /// <summary>
        /// False if RulesResult.Failure
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        bool CanProceed(ShoppingCartRuleResult result)
        {
            if (result == null) return true;

            switch(result.Result)
            {
                case RulesResult.Failure:
                    LoggingTrace(string.Format("CanProceed - detected RulesResult.Failure"));
                    return false;
            }

            return true;
        }

        #region SkuList
        Dictionary<string, SKU_V01> _SkuList = null;
        Dictionary<string, SKU_V01> SkuList
        {
            get
            {
                if (_SkuList == null) _SkuList = CatalogProvider.GetAllSKU(this.Locale);

                return _SkuList;
            }
        }
        #endregion

        CartItemWithDetailList GetEventProductList(ShoppingCart_V02 cart)
        {
            var ret = new CartItemWithDetailList();

            if (Helper.Instance.HasData(cart.CurrentItems))
            {
                foreach (var cartItem in cart.CurrentItems)
                {
                    if (!IsEventProduct(cartItem.SKU)) continue;
                    SKU_V01 sku_v01 = null;
                    SkuList.TryGetValue(cartItem.SKU, out sku_v01);
                    if (sku_v01 == null) continue;
                    var child = new CartItemWithDetail
                    {
                        CurrentItem = cartItem,
                        SKU = sku_v01
                    };
                    ret.Add(child);
                }
            }

            if (Helper.Instance.HasData(cart.CartItems))
            {
                foreach (var cartItem in cart.CartItems)
                {
                    if (!IsEventProduct(cartItem.SKU)) continue;

                    var child = ret.FirstOrDefault(x => x.SkuCode == cartItem.SKU);
                    if (child != null)
                    {
                        child.CartItem = cartItem;
                    }
                    else
                    {
                        child = new CartItemWithDetail
                        {
                            CartItem = cartItem,
                            SKU = SkuList[cartItem.SKU],
                        };
                        ret.Add(child);
                    }
                }
            }

            return ret;
        }

        bool IsEventProduct(string sku)
        {
            SKU_V01 sku_v01 = null;
            SkuList.TryGetValue(sku,out sku_v01);
            if (sku_v01 == null) return false;
            
            var p = sku_v01.Product;
            return p != null && (p.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket);
        }

        bool ValidateIsInEventMode(CartItemWithDetailList eventProductList, ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {
            if (isCheckEventTicket(cart)) return true;

            // not in event mode, build-up error info

            var itemToAddList = eventProductList.CurrentItemList;

            result.Result = RulesResult.Failure;

            var itemList = new StringBuilder();
            foreach(var item in itemToAddList)
            {
                if (itemList.Length > 0) itemList.Append(", ");
                itemList.Append(item.SKU.Product.DisplayName);
            }

            var msg = GetRulesResourceString("CanOnlyPurchaseInEventMode");
            result.Messages.Add(string.Format(msg, itemList));

            LoggingTrace(string.Format("CanOnlyPurchaseInEventMode"));
            return false;
        }

        #region MaxQtyDependsOnSpouseName

        public const int MaxQtyIfNoSpouseName = 1;
        public const int MaxQtyIfHasSpouseName = 2;

        /// <summary>
        /// Returns true if ordered quantity is within limit.
        /// </summary>
        /// <param name="eventProductList"></param>
        /// <param name="cart"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool ValidateMaxQtyDependsOnSpouseName(CartItemWithDetailList eventProductList, ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {
            bool ret = true;

            LoadSkuPurchasedCount(eventProductList, cart.DistributorID);

            foreach(var item in eventProductList)
            {
                if (item.TotalQty <= MaxQtyIfNoSpouseName) continue;

                RulesResult rslt = RulesResult.Unknown;
                string msg = null;
                int limit = 0;

                var user = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, this.Country);
                if (string.IsNullOrWhiteSpace(user.SpouseLocalName))
                {
                    #region no spouse name
                    limit = MaxQtyIfNoSpouseName;

                    if (item.TotalQty > limit) 
                    {
                        DeductEventProductQty(item, limit, cart);

                        rslt = (item.CurrentItem.Quantity <= 0) ? RulesResult.Failure : RulesResult.Feedback;
                                        
                        msg = GetRulesResourceString("EventProductLimitedToNAsNoSpouseName");
                        msg = string.Format(msg, item.SKU.Product.DisplayName, limit);
                    }
                    #endregion
                }
                else
                {
                    #region has spouse name
                    limit = MaxQtyIfHasSpouseName;
                    if (item.TotalQty > limit) 
                    {
                        DeductEventProductQty(item, limit, cart);

                        rslt = (item.CurrentItem.Quantity <= 0) ? RulesResult.Failure : RulesResult.Feedback;

                        msg = GetRulesResourceString("EventProductLimitedToNAsHasSpouseName");
                        msg = string.Format(msg, item.SKU.Product.DisplayName, limit);
                    }
                    #endregion
                }

                if (rslt != RulesResult.Unknown)
                {
                    ret = false;
                    result.Result = rslt;

                    if (item.QuantityAlreadyPurchased >= limit)
                    {
                        msg = GetRulesResourceString("EventProductAlreadyBeenPurchased");
                        msg = string.Format(msg, item.SKU.Product.DisplayName);
                    }

                    result.Messages.Add(msg);
                }

                LoggingTrace(string.Format("ValidateMaxQtyDependsOnSpouseName: {0}, {1}", rslt, msg));
            }

            return ret;
        }

        void DeductEventProductQty(CartItemWithDetail data, int toQty, ShoppingCart_V02 cart)
        {
            if (data.TotalQty <= toQty) return;

            var item = data.CurrentItem;
            if ((item != null) && (item.Quantity > 0))
            {
                var diff = data.TotalQty - toQty;
                item.Quantity -= diff;

                if (item.Quantity < 0) item.Quantity = 0;
            }
        }

        void LoadSkuPurchasedCount(CartItemWithDetailList eventProductList, string distributorId)
        {
            List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> skuList = new List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased>();
            foreach(var x in eventProductList)
            {
                var sku = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased
                {
                    SKU = x.ProductId,
                    Category = "ETO",
                };
                skuList.Add(sku);
            }

            var rslt = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetSkuOrderedAndPurchased(this.Country, distributorId, null, null, skuList);
            
            foreach(var r in rslt)
            {
                var mList = eventProductList.Where(x => x.ProductId == r.SKU);
                foreach(var m in mList)
                {
                    m.QuantityAlreadyPurchased = r.QuantityPurchased;
                }
            }
        }

        #endregion

        #region Logs Tracing

        StringBuilder SB = new StringBuilder();

        #region IsLoggingTraceEnabled
        bool? _IsLoggingTraceEnabled = null;
        bool IsLoggingTraceEnabled
        {
            get
            {
                if(_IsLoggingTraceEnabled == null) _IsLoggingTraceEnabled = HL.Common.Configuration.Settings.GetRequiredAppSetting("ETOLog", false);

                return _IsLoggingTraceEnabled.GetValueOrDefault(false);
            }
        }
        #endregion

        void LoggingTrace(string message)
        {
            if (!IsLoggingTraceEnabled) return;

            SB.AppendLine(message);
        }

        void CommitTrace(ShoppingCart_V02 cart)
        {
            if (!IsLoggingTraceEnabled || (SB.Length == 0)) return;
            
            LoggingTrace(string.Format("End"));
            LoggerHelper.Verbose(SB.ToString());

            SB.Clear();
        }

        void StartLoggingTrace(ShoppingCart_V02 cart)
        {
            if (!IsLoggingTraceEnabled) return;

            SB.Clear();

            LoggingTrace(string.Format("Start : {0}", ThisClassName));
            LoggingTrace(string.Format("DateTime.Now : {0}", DateTime.Now));

            LoggingTrace(string.Format("System.Web.HttpContext.Current.Server.MachineName : {0}", System.Web.HttpContext.Current.Server.MachineName));

            LoggingTrace(string.Format("cart.DistributorID : {0}", cart.DistributorID));
            LoggingTrace(string.Format("cart.Locale : {0}", cart.Locale));
            LoggingTrace(string.Format("this.Locale : {0}", this.Locale));
            LoggingTrace(string.Format("this.Country : {0}", this.Country));
        }

        #endregion
    }

    class CartItemWithDetail
    {
        public ShoppingCartItem_V01 CurrentItem { get; set; }
        public ShoppingCartItem_V01 CartItem { get; set; }
        public SKU_V01 SKU { get; set; }
        
        public int QuantityAlreadyPurchased { get; set; }

        public string SkuCode 
        { 
            get 
            {
                if (SKU == null) return null;

                return SKU.SKU.Trim();
            } 
        }

        public string ProductId
        {
            get
            {
                if ((SKU == null) || (SKU.CatalogItem == null)) return null;

                return SKU.CatalogItem.StockingSKU;
            }
        }

        public int TotalQty
        {
            get
            {
                int ret = 0;
                if (CurrentItem != null) ret += CurrentItem.Quantity;
                if (CartItem != null) ret += CartItem.Quantity;
                return ret += QuantityAlreadyPurchased;
            }
        }
    }

    class CartItemWithDetailList : List<CartItemWithDetail>
    {
        public List<CartItemWithDetail> CurrentItemList 
        { 
            get
            {
                return this.Where(x => x.CurrentItem != null).ToList();
            }
        }
    }
}
