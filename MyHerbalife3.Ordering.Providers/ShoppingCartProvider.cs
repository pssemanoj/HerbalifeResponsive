using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.DataContract.Interfaces;
using HL.Common.Utilities;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using ServiceResponseStatusType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ServiceResponseStatusType;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Providers
{
    [DataObject]
    public static partial class ShoppingCartProvider
    {
        #region Consts

        public const string ShoppingCartCachePrefix = "SHOPPINGCART_";

        public const string CartsCachePrefix = "CARTS_";
        public const string InternetCartsCachePrefix = "InternetCARTS_";

        private const string DualMonthCacheKey = "DUAL_MONTH_KEY_";
        public const int DualMonthCacheMinutes = 60;

        private const string PromoCachePrefix = "PROMO_";

        #endregion Consts


        public static ICatalogProviderLoader CatalogProviderLoader { get; set; }
        public static ICatalogInterface CatalogProxyInterface { get; set; }

        private class WarehouseQuantity
        {
            public string Warehouse { get; set; }
            public int Quantity { get; set; }
            public bool IsBackOrder { get; set; }
        }

        #region Public Methods

        public static ShoppingCart GetShoppingCartFromV02(string memberId, string locale, int shoppingCartId)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new GetShoppingCartRequest_V02() { DistributorID = memberId, Locale = locale, OrderCategory = OrderCategoryType.RSO };
                    request.ShoppingCartID = shoppingCartId;
                    var response = proxy.GetShoppingCart(new GetShoppingCartRequest1(request)).GetShoppingCartResult as GetShoppingCartResponse_V02;
                    if (null != response && null != response.ShoppingCart)
                    {
                        return response.ShoppingCart;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("General", ex);
                }
            }
            return null;
        }

        public static List<ShoppingCartItem_V01> GetDiscontinuededSku(int shoppingCartId, string DistributorID,
                                    string locale, int newShoppingCartId, SKU_V01ItemCollection SkuitemList = null)
        {
            MyHLShoppingCart originalUnfilteredCopyCart=new MyHLShoppingCart();
            if (CatalogProxyInterface != null)
                originalUnfilteredCopyCart.CatalogProxy = CatalogProxyInterface;
            
            originalUnfilteredCopyCart = GetUnfilteredShoppingCartForCopyFromServiceint(shoppingCartId,
                DistributorID, locale, 0, SkuitemList);
            
            if (CatalogProviderLoader != null)
                originalUnfilteredCopyCart.CatalogProviderLoader = CatalogProviderLoader;

            
            var itemList = new List<DistributorShoppingCartItem>();
            var cartItems = originalUnfilteredCopyCart.CartItems;
            if (cartItems != null && originalUnfilteredCopyCart.CartItems.Count > 0)
            {
                DistributorShoppingCartItem item = null;
                itemList.AddRange(from c in cartItems
                                  orderby c.Updated ascending
                                  where (item = originalUnfilteredCopyCart.CreateShoppingCartItem(c)) != null
                                  select item);
                originalUnfilteredCopyCart.ShoppingCartItems = itemList;
                // if any ShoppingCartItem_V01 does not have matching DistributorShoppingCartItem, remove it
                var skuToRemove = (from c in cartItems
                                   where originalUnfilteredCopyCart.ShoppingCartItems.Find(s => s.SKU == c.SKU) == null
                                   select c).ToList();

                // checking for virtual Sku
                var specialSKUs = GetSpecialSkulist();



                var specialSkuRemoved = (from c in cartItems
                                         where specialSKUs.Find(s => s.ToString() == c.SKU) != null
                                         select c).ToList();

                skuToRemove.AddRange(specialSkuRemoved);

                return skuToRemove;
            }
            return new List<ShoppingCartItem_V01>();
        }

        public static List<ShoppingCartItem_V01> GetDiscontinuededSku(MyHLShoppingCart originalUnfilteredCopyCart)
        {
            var itemList = new List<DistributorShoppingCartItem>();
            
            if (CatalogProviderLoader != null)
                originalUnfilteredCopyCart.CatalogProviderLoader = CatalogProviderLoader;
            
            var cartItems = originalUnfilteredCopyCart.CartItems;
            if (cartItems != null && originalUnfilteredCopyCart.CartItems.Count > 0)
            {
                DistributorShoppingCartItem item = null;
                itemList.AddRange(from c in cartItems
                                  orderby c.Updated ascending
                                  where (item = originalUnfilteredCopyCart.CreateShoppingCartItem(c)) != null
                                  select item);
                originalUnfilteredCopyCart.ShoppingCartItems = itemList;
                // if any ShoppingCartItem_V01 does not have matching DistributorShoppingCartItem, remove it
                var skuToRemove = (from c in cartItems
                                   where originalUnfilteredCopyCart.ShoppingCartItems.Find(s => s.SKU == c.SKU) == null
                                   select c).ToList();
                
                // checking for virtual Sku
                var specialSKUs = GetSpecialSkulist();

                

                var specialSkuRemoved = (from c in cartItems
                                        where specialSKUs.Find(s => s.ToString() == c.SKU) != null
                                        select c).ToList();
                foreach (var sku in specialSkuRemoved)
                {
                    if (!skuToRemove.Exists(x=>x.SKU==sku.SKU))
                    {
                        skuToRemove.AddRange(specialSkuRemoved);
                    }
                }


                return skuToRemove;
            }
            return new List<ShoppingCartItem_V01>();
        }

        public static List<string> GetSpecialSkulist()
        {
            List<string> specialSkUs = new List<string>();
            specialSkUs.Add(HLConfigManager.Configurations.APFConfiguration.SupervisorSku);
            specialSkUs.Add(HLConfigManager.Configurations.APFConfiguration.DistributorSku);
            specialSkUs.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku);
            specialSkUs.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku);
            specialSkUs.Add(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku);

           if (!string.IsNullOrEmpty(HLConfigManager.Configurations.APFConfiguration.AlternativeSku))
           {
               specialSkUs.Add(HLConfigManager.Configurations.APFConfiguration.AlternativeSku);
           }
            return specialSkUs;
        }
        public static MyHLShoppingCart GetShoppingCart2(string distributorID, string locale)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo != null)
            {
                if (sessionInfo.ShoppingCart != null)
                {
                     result = sessionInfo.ShoppingCart;
                }
                else
                {
                    MyHLShoppingCart theCart = null;
                    using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                    {
                        try
                        {
                            var request = new GetShoppingCartRequest_V03(){ DistributorID = distributorID, Locale = locale, OrderCategory = OrderCategoryType.RSO };
                            var response =
                                proxy.GetShoppingCart(new GetShoppingCartRequest1(request)).GetShoppingCartResult as GetShoppingCartResponse_V03;
                            if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                            {
                                theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                                //sessionInfo.ShoppingCart = theCart;

                                //theCart.RuleResults = new List<ShoppingCartRuleResult>();
                                //theCart.GetShoppingCartForDisplay(true);
                                //theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID,
                                //                         theCart.DeliveryOption, theCart.OrderCategory,
                                //                         theCart.FreightCode, theCart.IsSavedCart);
                                //theCart.Calculate();
                                //List<ShoppingCartRuleResult> ruleResults = HLRulesManager.Manager.ProcessCart(theCart,
                                //                                                     ShoppingCartRuleReason
                                //                                                         .CartCreated);
                                //if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                                //{
                                //    ruleResults = HLRulesManager.Manager.ProcessCart(ShoppingCartRuleReason.CartCreated, theCart);
                                //}
                                //theCart.RuleResults = ruleResults.Where(r => r.Result == RulesResult.Failure).ToList();
                                return theCart;
                            }
                        }
                        catch (Exception ex)
                        {
                            WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                            LoggerHelper.Error(
                                string.Format("GetShoppingCart2 CardistributorIDtId:{0} ERR:{1}", distributorID, ex));
                            return null;
                        }
                    }
                }
            }
            return result;
        }

        public static MyHLShoppingCart GetShoppingCart(string distributorID, string locale, int shoppingCartID)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo != null)
            {
                if (sessionInfo.ShoppingCart != null)
                {
                    if (sessionInfo.ShoppingCart.ShoppingCartID != shoppingCartID)
                    {
                        result = GetShoppingCartFromService(shoppingCartID, distributorID, locale);
                        if (result != null)
                        {
                            sessionInfo.CustomerOrderNumber = null;
                            sessionInfo.ShoppingCart = result;
                        }
                    }
                    else
                    {
                        result = sessionInfo.ShoppingCart;
                    }
                }
                else
                {
                    result = GetShoppingCartFromService(shoppingCartID, distributorID, locale);
                    if (result != null)
                    {
                        sessionInfo.ShoppingCart = result;
                    }
                }
            }
            return result;
        }

        //Method to get the OrderDetails From GDO sql tables for Invoice
        public static MyHLShoppingCart GetShoppingCart(string distributorID, string locale, string OrderId)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }
            List<string> orders= new List<string>();
            orders.Add(OrderId);
            var results = GetInternetShoppingCartsFromService(distributorID, locale,0 , 10, orders);
            if (results != null)
                result = results.FirstOrDefault();
            return result;
        }

      
        public static MyHLShoppingCart GetShoppingCartForCopy(string distributorID,
                                                              int shoppingCartID,
                                                              string locale,
                                                              int newShoppingCartID, SKU_V01ItemCollection SkuitemList = null)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo != null)
            {
                result = GetShoppingCartForCopyFromService(shoppingCartID, distributorID, locale, newShoppingCartID, SkuitemList);
                if (result != null)
                {
                    sessionInfo.ShoppingCart = result;
                }
            }
            return result;
        }

        public static MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool noupdateDeliveryInfoToDB = false)
        {
            return GetShoppingCart(distributorID, locale, false, noupdateDeliveryInfoToDB);
        }


        public static MyHLShoppingCart GetShoppingCart(OrderViewModel order,string distributorID, string locale, bool suppressRules, bool noupdateDeliveryInfoToDB = false)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }

            //string cacheKey = GetShoppingCartCacheKey(distributorID, locale);
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            string category = (sessionInfo == null) ? "RSO" : sessionInfo.IsEventTicketMode ? "ETO" : sessionInfo.IsHAPMode ? "HSO" : "RSO";

            if (null != sessionInfo && string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
            {
                result = (sessionInfo == null) ? null : sessionInfo.ShoppingCart;
            }
            else
            {
                // Retrieve the cart from session only if there is an order number for it
                if (sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.CustomerOrderDetail != null
                    && sessionInfo.ShoppingCart.CustomerOrderDetail.CustomerOrderID.ToLower() == sessionInfo.CustomerOrderNumber.ToLower()
                    && !string.IsNullOrEmpty(sessionInfo.OrderNumber))
                {
                    result = (sessionInfo == null) ? null : sessionInfo.ShoppingCart;
                }
            }

            var custOrder = sessionInfo != null ? sessionInfo.CustomerOrderNumber : null;

            if (null == result)
            {
                result = GetShoppingCartFromService(order,distributorID, locale, custOrder, noUpdateDeliveryInfoToDB: noupdateDeliveryInfoToDB);
                if (result != null)
                {
                    if (sessionInfo != null) sessionInfo.ShoppingCart = result;

                }
            }
            else
            {
                if (result.OrderCategory.ToString() != category)
                {
                    result = GetShoppingCartFromService(distributorID, locale, sessionInfo.CustomerOrderNumber);
                    if (result != null)
                    {
                        sessionInfo.ShoppingCart = result;
                    }
                }
            }
            if (sessionInfo != null && sessionInfo.ShoppingCart != null)
            {
                if (sessionInfo.IsHAPMode && sessionInfo.CreateHapOrder)
                {
                    if (sessionInfo.ShoppingCart.CartItems.Count > 0)
                    {
                        DeleteShoppingCart(sessionInfo.ShoppingCart, sessionInfo.ShoppingCart.CartItems.Select(c => c.SKU).ToList());
                        sessionInfo.ShoppingCart.CartItems.Clear();
                        sessionInfo.ShoppingCart.ShoppingCartItems.Clear();
                    }
                    sessionInfo.CreateHapOrder = false;
                }

            }

            //ExceptionFix: Adding TryCatch, Exception message when result is null.
            // If result == null at this point, service is wrong, log it pending.
            try
            {
                if (result == null)
                {
                    throw new Exception("Method Name: GetShoppingCart,  distributorID:" + distributorID + ", locale:" + locale + ", Exception: result is null, service is wrong.");
                }
                else
                {
                    if (sessionInfo == null || !string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                    {
                        if (result != null)
                        {
                            if (!suppressRules)
                            {
                                var results = HLRulesManager.Manager.ProcessCart(
                                    result, ShoppingCartRuleReason.CartRetrieved);
                            }
                        }
                    }
                    else
                    {
                        if (!suppressRules)
                        {
                            HLRulesManager.Manager.ProcessCart(
                                result, ShoppingCartRuleReason.CartRetrieved);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("{0}", ex.Message));
            }
            return result;
        }

        public static MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool suppressRules, bool noupdateDeliveryInfoToDB = false)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(distributorID))
            {
                return result;
            }

            //string cacheKey = GetShoppingCartCacheKey(distributorID, locale);
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            string category = (sessionInfo == null) ? "RSO" : sessionInfo.IsEventTicketMode ? "ETO" : sessionInfo.IsHAPMode ? "HSO" : "RSO";

            if (null != sessionInfo && string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
            {
                result = (sessionInfo == null) ? null : sessionInfo.ShoppingCart;
            }
            else
            {
                // Retrieve the cart from session only if there is an order number for it
                if (sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.CustomerOrderDetail != null 
                    && sessionInfo.ShoppingCart.CustomerOrderDetail.CustomerOrderID.ToLower() == sessionInfo.CustomerOrderNumber.ToLower()
                    && !string.IsNullOrEmpty(sessionInfo.OrderNumber))
                {
                    result = (sessionInfo == null) ? null : sessionInfo.ShoppingCart;
                }
            }

            var custOrder = sessionInfo != null ? sessionInfo.CustomerOrderNumber : null;

            if (null == result)
            {
                result = GetShoppingCartFromService(distributorID, locale, custOrder, noUpdateDeliveryInfoToDB: noupdateDeliveryInfoToDB);
                if (result != null)
                {
                    if (sessionInfo != null) sessionInfo.ShoppingCart = result;
                }
            }
            else
            {
                if (result.OrderCategory.ToString() != category)
                {
                    result = GetShoppingCartFromService(distributorID, locale, sessionInfo.CustomerOrderNumber);
                    if (result != null)
                    {
                        sessionInfo.ShoppingCart = result;
                    }
                }
            }
            if (sessionInfo != null && sessionInfo.ShoppingCart != null)
            {
                if (sessionInfo.IsHAPMode && sessionInfo.CreateHapOrder)
                {
                    if (sessionInfo.ShoppingCart.CartItems.Count>0)
                    {
                        DeleteShoppingCart(sessionInfo.ShoppingCart, sessionInfo.ShoppingCart.CartItems.Select(c => c.SKU).ToList());
                        sessionInfo.ShoppingCart.CartItems.Clear();
                        sessionInfo.ShoppingCart.ShoppingCartItems.Clear();
                    }
                    sessionInfo.CreateHapOrder = false;
                }
                
            }

            //ExceptionFix: Adding TryCatch, Exception message when result is null.
            // If result == null at this point, service is wrong, log it pending.
            try
            {
                if (result == null)
                {
                    throw new Exception("Method Name: GetShoppingCart, distributorID:" + distributorID + ", locale:" + locale+", Exception: result is null, service is wrong.");
                }
                else
                {
                    if (sessionInfo == null || !string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
            {
                if (result != null)
                {
                    if (!suppressRules)
                    {
                        var results = HLRulesManager.Manager.ProcessCart(
                            result, ShoppingCartRuleReason.CartRetrieved);
                    }
                }
            }
            else
            {
                if (!suppressRules)
                {
                    HLRulesManager.Manager.ProcessCart(
                        result, ShoppingCartRuleReason.CartRetrieved);
                }
            }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("{0}", ex.Message));
            }
            return result;
        }

        public static void ResetInternetShoppingCartsCache(string distributorID, string locale)
        {
            HttpRuntime.Cache[GetInternetCartsCacheKey(distributorID, locale)] = 0;
        }

        public static List<MyHLShoppingCart> GetInternetShoppingCarts(string distributorID,
                                                                      string locale,
                                                                      int index,
                                                                      int maxLength,
                                                                      bool indexChanged,
                                                                      bool byOrderCategory = false)
        {
            string cacheKey = GetInternetCartsCacheKey(distributorID, locale);
            var carts = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCart> ?? new List<MyHLShoppingCart>();
            if (carts.Count == 0 || indexChanged)
            {
                var carts_ = GetInternetShoppingCartsFromService(distributorID, locale, index, maxLength, byCategory: byOrderCategory);
                if (carts_ != null)
                {
                    carts.AddRange(carts_);
                }
                HttpRuntime.Cache[cacheKey] = carts;
            }

            return carts;
        }

        private static int getShoppingCartID(ShippingAddress_V02 addr)
        {
            int shoppingCartID = 0;
            if (!string.IsNullOrEmpty(addr.AltPhone))
            {
                int.TryParse(addr.AltPhone, out shoppingCartID);
            }
            return shoppingCartID;
        }

        public static ShippingInfo GetShippingInfoForCopyOrder(string distributorID,
                                                               string locale,
                                                               int shoppingCartID,
                                                               ServiceProvider.CatalogSvc.DeliveryOptionType option)
        {
            var shippingInfo = new ShippingInfo();
            shippingInfo.Option = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), option.ToString());
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo.OrderShippingAddresses == null)
            {
                // Refresh order shipping addresses
                var provider = new ShippingProviderBase();
                provider.ReloadOrderShippingAddressFromService(distributorID, locale);
            }
            if (sessionInfo.OrderShippingAddresses != null)
            {
                var OrderShippingAddresseses = sessionInfo.OrderShippingAddresses;
                var addresses = from o in OrderShippingAddresseses
                                where getShoppingCartID(o) == shoppingCartID
                                select o;
                if (addresses.Count() > 0)
                {
                    var addr = addresses.First();
                    shippingInfo.Address = new ShippingAddress_V01
                        {
                            Recipient = addr.Recipient,
                            Phone = addr.Phone,
                            Address = addr.Address,
                            Alias = string.IsNullOrEmpty(addr.Alias) ? string.Empty : addr.Alias
                        };
                }
            }
            return shippingInfo;
        }

        public static List<MyHLShoppingCart> GetInternetShoppingCartsFromService(string distributorId,
                                                                                 string locale,
                                                                                 int index,
                                                                                 int maxLength,
                                                                                 List<string> orderNumbers = null,
                                                                                 bool byCategory = false)
        {
            var SKUsToRemove = new List<string>(new[]
                {
                    HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                    HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                    HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                });

            SKUsToRemove.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            if (string.IsNullOrEmpty(distributorId))
            {
                return null;
            }
            else
            {
                var theCarts = new List<MyHLShoppingCart>();
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new GetInternetShoppingCartsRequest_V01();

                        if (byCategory)
                        {
                            var sessionInfo = SessionInfo.GetSessionInfo(distributorId, locale);
                            bool isEventTicketMode = sessionInfo == null ? false : sessionInfo.IsEventTicketMode;
                            bool isHAPMode = sessionInfo == null ? false : sessionInfo.IsHAPMode;

                            request.ShoppingCartFilterByDateRange = new ShoppingCartFilter_V01
                            {
                                Locale = locale,
                                DistributorId = distributorId,
                                Index = index,
                                MaxLength = maxLength,
                                DiscardEmptyOrderNumbers = true,
                                OrderCategory = isEventTicketMode ? OrderCategoryType.ETO : 
                                                isHAPMode ? OrderCategoryType.HSO : 
                                                OrderCategoryType.RSO
                            };
                        }
                        else
                        {
                            request.ShoppingCartFilterByDateRange = new ShoppingCartFilter
                            {
                                Locale = locale,
                                DistributorId = distributorId,
                                Index = index,
                                MaxLength = maxLength,
                                DiscardEmptyOrderNumbers = true
                            };
                        }

                        if (orderNumbers != null)
                        {
                            request.ShoppingCartFilterByDateRange.OrderNumberToBeFetched=  orderNumbers;
                        }
                        var response = proxy.GetInternetShoppingCarts(new GetInternetShoppingCartsRequest1(request)).GetInternetShoppingCartsResult as GetInternetShoppingCartsResponse_V01;
                        if (response != null && response.ShoppingCarts != null && response.ShoppingCarts.Count > 0)
                        {
                            response.ShoppingCarts.ForEach(
                                cart => cart.Locale = cart.Locale != null ? cart.Locale.Trim() : string.Empty);
                            foreach (var cart in response.ShoppingCarts)
                            {
                                if (APFDueProvider.containsOnlyAPFSku(cart.CartItems))
                                {
                                    continue;
                                }
                                if (cart.OrderDate < DateTime.Now.AddMonths(-6))
                                    continue;

                                //cart.OrderDate = DateUtils.ConvertToLocalDateTime(cart.OrderDate, locale.Substring(3));

                                cart.CartItems.ForEach(item => item.SKU = item.SKU.Trim());

                                Array.ForEach(SKUsToRemove.ToArray(),
                                              a => cart.CartItems.Remove(cart.CartItems.Find(x => x.SKU == a)));

                                if (cart.CartItems == null || cart.CartItems.Count == 0)
                                {
                                    continue;
                                }

                                cart.FreightCode = cart.FreightCode != null ? cart.FreightCode.Trim() : cart.FreightCode;
                                cart.OrderSubType = cart.OrderSubType != null
                                                        ? cart.OrderSubType.Trim()
                                                        : cart.OrderSubType;

                                var myHLCart = new MyHLShoppingCart(cart);
                                myHLCart.OrderNumber = cart.OrderNumber;
                                myHLCart.OrderDate = cart.OrderDate;

                                myHLCart.GetShippingInfoForCopyOrder(cart.DistributorID, cart.Locale,
                                                                     cart.ShoppingCartID, cart.DeliveryOption, true);
                                if (myHLCart.DeliveryInfo != null)
                                {
                                    if (myHLCart.DeliveryInfo.Address == null)
                                    {
                                        int newShippingAddressID, newDeliveryOptionID;
                                        ServiceProvider.CatalogSvc.DeliveryOptionType newDeliveryOptionType;
                                        myHLCart.MatchShippingInfo(cart.ShoppingCartID, cart.DeliveryOption,
                                                                   cart.ShippingAddressID, cart.DeliveryOptionID,
                                                                   out newDeliveryOptionType, out newShippingAddressID,
                                                                   out newDeliveryOptionID, false);

                                        // Loading the address from the .normal tables.
                                        myHLCart.LoadShippingInfo(myHLCart.DeliveryOptionID = newDeliveryOptionID,
                                                                  myHLCart.ShippingAddressID = newShippingAddressID,
                                                                  myHLCart.DeliveryOption = newDeliveryOptionType,
                                                                  myHLCart.OrderCategory, myHLCart.FreightCode);
                                    }
                                }

                                // Items.
                                myHLCart.GetShoppingCartForDisplay(true, true);

                                //if (myHLCart.DeliveryInfo != null)
                                //{
                                //    MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo shippingInfo =
                                //        myHLCart.DeliveryInfo;
                                //    // if there is freight code in database and is not APF
                                //    if (!string.IsNullOrEmpty(myHLCart.FreightCode) &&
                                //        myHLCart.FreightCode !=
                                //        HLConfigManager.Configurations.APFConfiguration.APFFreightCode)
                                //    {
                                //        myHLCart.DeliveryInfo.FreightCode = myHLCart.FreightCode;
                                //    }
                                //}

                                //if (HLConfigManager.Configurations.DOConfiguration.EnforcesPurchaseLimits &&
                                //    !string.IsNullOrEmpty(myHLCart.OrderSubType))
                                //{
                                //    PurchasingLimitProvider.GetPurchasingLimits(myHLCart.DistributorID,
                                //                                                myHLCart.OrderSubType);
                                //}
                                //if (
                                //    HLConfigManager.Configurations.CheckoutConfiguration.
                                //        GetShippingInstructionsFromProvider)
                                //{
                                //    IShippingProvider provider =
                                //        ShippingProvider.GetShippingProvider(myHLCart.CountryCode);
                                //    if (provider != null && myHLCart.DeliveryInfo != null)
                                //    {
                                //        myHLCart.DeliveryInfo.Instruction =
                                //            provider.GetShippingInstructionsForDS(myHLCart, myHLCart.DistributorID,
                                //                                                  myHLCart.Locale);
                                //    }
                                //}
                                //myHLCart.Calculate();

                                //List<ShoppingCartRuleResult> results = HLRulesManager.Manager.ProcessCart(myHLCart,
                                //                                                                          ShoppingCartRuleReason
                                //                                                                              .
                                //                                                                              CartCreated);

                                theCarts.Add(myHLCart);
                            }
                            // }

                            return theCarts;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetIntenernetShoppingCartsFromService DS:{0} locale:{2} ERR:{1}",
                                          distributorId, ex, locale));
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     get a list of drafts
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        public static List<MyHLShoppingCart> GetCarts(string distributorID,
                                                      string locale,
                                                      bool reLoad = false,
                                                      bool updateSession = true)
        {
            var carts = new List<MyHLShoppingCart>();
            if (string.IsNullOrEmpty(distributorID))
            {
                return carts;
            }

            string cacheKey = GetCartsCacheKey(distributorID, locale);
            carts = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCart>;

            if (null == carts || reLoad)
            {
                GetShoppingCartFromService(distributorID, locale, string.Empty, false);
                carts = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCart>;
            }

            return carts;
        }

        /// <summary>
        ///     get a list of drafts
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="locale"></param>
        /// <param name="shoppingCartID"></param>
        /// <returns></returns>
        public static bool DeleteCartFromCache(string distributorID, string locale, int shoppingCartID)
        {
            string cacheKey = GetCartsCacheKey(distributorID, locale);
            var carts = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCart>;

            if (null != carts)
            {
                carts = carts.TakeWhile(sh => !sh.ShoppingCartID.Equals(shoppingCartID)).ToList();
                HttpRuntime.Cache[cacheKey] = carts;
                return true;
            }

            return false;
        }

        //public static void DeleteItemsFromCache(string distributorID, string locale)
        //{
        //    ClearShoppingCartFromCache(GetShoppingCartCacheKey(distributorID, locale));
        //}
        public static void DeleteItemsFromSession(string distributorID, string locale)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo != null)
            {
                sessionInfo.ShoppingCart = null;
            }
        }

        private static bool isBlocked(WarehouseInventoryList invList, string warehouse)
        {
            if (invList.ContainsKey(warehouse) && invList[warehouse] is WarehouseInventory_V01)
            {
                var warehouseInv = invList[warehouse] as WarehouseInventory_V01;
                return warehouseInv.IsBlocked;
            }
            return false;
        }

        public static Order AddLinkedSKU(Order _order, string locale, string countryCode, string warehouse)
        {
            //This method stays
            Order_V01 order = _order as Order_V01;
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.SupportLinkedSKU)
            {
                var allSKU = CatalogProvider.GetAllSKU(locale, warehouse);
                if (allSKU != null)
                {
                    var lstOrderItem = new List<OrderItem>();
                    var allItems = from o in order.OrderItems
                                   select new { o.SKU, o.Quantity };
                    // added linked sku for calulation
                    foreach (var item in allItems)
                    {
                        SKU_V01 sku;
                        if (allSKU.TryGetValue(item.SKU, out sku))
                        {
                            if (sku.SubSKUs == null)
                                continue;
                            foreach (SKU_V01 linkedSKU in sku.SubSKUs)
                            {
                                if (linkedSKU.CatalogItem == null)
                                {
                                    LoggerHelper.Error(string.Format("Error while trying to add linked SKU to the cart.\nSKU {0} linked to Parent {1} is not properly setup in TeamSite or HMS. Linked SKU {0} won't get added to the cart.", linkedSKU.SKU, sku.SKU));
                                    continue;
                                }

                                //Execute Inventory rules for Child SKUs before checking inventory
                                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                                if (null != member && null != member.Value && !string.IsNullOrEmpty(member.Value.Id))
                                {
                                    var sessionInfo = SessionInfo.GetSessionInfo(member.Value.Id, locale);
                                    HLRulesManager.Manager.ProcessCatalogItemsForInventory(locale, sessionInfo.ShoppingCart,
                                                                                       new List<SKU_V01> { linkedSKU });
                                }

                                //validation to avoid adding Child SKUs that don't have available inventory
                                if (!linkedSKU.CatalogItem.IsInventory || CheckInventory(linkedSKU.CatalogItem, item.Quantity, warehouse) > 0)
                                {
                                    // Checking item in order items
                                    var itemInOrder = order.OrderItems.Find(i => i.SKU == linkedSKU.SKU) as OrderItem_V01;
                                    if (itemInOrder != null)
                                    {
                                        itemInOrder.Quantity += item.Quantity;
                                    }
                                    else
                                    {
                                        var itemInList =
                                            lstOrderItem.Find(l => l.SKU == linkedSKU.SKU) as OrderItem_V01;
                                        if (itemInList != null)
                                        {
                                            itemInList.Quantity += item.Quantity;
                                        }
                                        else
                                        {
                                            lstOrderItem.Add(new OrderItem_V01() { SKU = linkedSKU.SKU, Quantity = item.Quantity });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    order.OrderItems.AddRange(lstOrderItem.OrderBy(i => i.SKU));
                }
            }
            return order;
        }

        //Overloaded method to take the address..
        // iPHONE
        // TODO
        //public static OrderTotals_V01 CalculatePricing(List<ShoppingCartItem_V01> items, ShoppingCartRequest request, bool HMSCalc)
        //{
        //    //This method stays
        //    // TODO -- Get Cart and calc from there
        //    OrderTotals_V01 total = new HL.Order.ValueObjects.OrderTotals_V01(); ;
        //    OrderTotals total2 = total;
        //    try
        //    {
        //        Order_V01 order = OrderCreationHelper.CreateOrderObject(items);
        //        return GetQuote(OrderCreationHelper.FillOrderInfoRequest(order, request), QuotePartType.Complete, HMSCalc);
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerHelper.WriteInfo(string.Format("Calculate error: DS {0}, countryCode{2}, warehouse{3}, shippingmethod:{4}, error: {1}", request.DistributorID, ex.ToString(), request.ISOCountryCode, request.WarehouseCode, request.ShippingmethodID), "Shoppingcart");
        //    }
        //    return total;
        //}

        public static int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse)
        {
            var isSplitted = false;
            return CheckInventory(cItem, quantity, warehouse, string.Empty, ref isSplitted);
        }

        public static int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse, string freightCode, ref bool isSplitted)
        {
            isSplitted = false;
            var isBackOrder = false;
            var isBlocked = false;
            if (HLConfigManager.Configurations.DOConfiguration.IgnoreInventory)
            {
                return quantity;
            }

            if (cItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product && !cItem.IsFlexKit &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit) &&
                HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Contains(warehouse) &&
                (string.IsNullOrEmpty(freightCode) || string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.FreightCodesForSplit) ||
                HLConfigManager.Configurations.DOConfiguration.FreightCodesForSplit.Contains(freightCode)))
            {
                //var alternativeWhs = new List<string> { warehouse };
                var alternativeWhs = new List<string>();
                alternativeWhs.AddRange(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Split(',').Where(w => w != warehouse));  //all WHs not including the one being passed
                
                var result = new List<WarehouseQuantity>();
                isBackOrder = false;

                // add primary WH by default to list of WHs regardless of Split flag
                var primaryWHAvailableQty = GetQuantityAvailableInWarehouse(cItem, quantity, warehouse, ref isBackOrder, ref isBlocked);                
                result.Add(new WarehouseQuantity()
                {
                    Warehouse = warehouse,
                    Quantity = isBackOrder || (!isBlocked && primaryWHAvailableQty < quantity) ? quantity : primaryWHAvailableQty,
                    IsBackOrder = isBackOrder || (!isBlocked && primaryWHAvailableQty < quantity)
                });

                foreach (var wh in alternativeWhs)
                {
                    var whAvailable = cItem.InventoryList != null ? cItem.InventoryList[wh] as WarehouseInventory_V01 : null;                    
                    if (wh != null && whAvailable.IsSplitAllowed) //add only alternate WHs where split is allowed
                    {
                        isBackOrder = false;
                        var whAvailableQty = GetQuantityAvailableInWarehouse(cItem, quantity, wh, ref isBackOrder, ref isBlocked);
                        result.Add(new WarehouseQuantity()
                            {
                                Warehouse = wh,
                                Quantity = isBackOrder || (!isBlocked && whAvailableQty < quantity) ? quantity : whAvailableQty,
                                IsBackOrder = isBackOrder || (!isBlocked && whAvailableQty < quantity)
                            });
                    }
                }

                var availableWh = result.FirstOrDefault(w => w.Quantity >= quantity && !w.IsBackOrder);
                if (availableWh != null)
                {
                    isSplitted = (availableWh.Warehouse != warehouse);
                    return quantity;
                }

                availableWh = result.FirstOrDefault(w => w.Quantity >= quantity && w.IsBackOrder);
                if (availableWh != null)
                {
                    isSplitted = (availableWh.Warehouse != warehouse);
                    return quantity;
                }

                availableWh = result.FirstOrDefault(w => w.Warehouse == warehouse);
                if (availableWh != null)
                {
                    isSplitted = false;
                    return availableWh.Quantity;
                }

                return 0;
            }
            else
            {
                return GetQuantityAvailableInWarehouse(cItem, quantity, warehouse, ref isBackOrder, ref isBlocked);
            }
        }

        public static int GetQuantityAvailableInWarehouse(CatalogItem_V01 cItem, int quantity, string warehouse, ref bool isBackOrder, ref bool isBlocked)
        {
            isBackOrder = false;
            isBlocked = false;
            if (HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList != null &&
                HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Count > 0)
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.SpecialSKUList.Exists(s => s == cItem.SKU))
                {
                    // Check if the sku is blocked
                    if (cItem != null && cItem.InventoryList != null &&
                        cItem.InventoryList.ContainsKey(warehouse) &&
                        cItem.InventoryList[warehouse] is WarehouseInventory_V01)
                    {
                        var warehouseInv = cItem.InventoryList[warehouse] as WarehouseInventory_V01;
                        return warehouseInv.IsBlocked ? 0 : quantity;
                    }

                    return quantity;
                }
            }
            var iList = cItem == null ? null : cItem.InventoryList;
            if (iList != null)
            {
                WarehouseInventory inventory = null;
                WarehouseInventory_V01 inventory_v01 = null;
                if (iList.TryGetValue(warehouse, out inventory))
                {
                    inventory_v01 = inventory as WarehouseInventory_V01;
                    if (inventory_v01.QuantityAvailable <= 0)
                    {
                        if (inventory_v01.IsBackOrder)
                        {
                            isBackOrder = true;
                            return quantity;
                        }
                        if (cItem.IsEventTicket)
                        {
                            return quantity;
                        }
                        if (cItem.SKU.Trim() == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku || 
                            HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s=> s.Equals(cItem.SKU.Trim())))
                        {
                            return quantity;
                        }
                        return 0;
                    }
                    else if (inventory_v01.QuantityAvailable - quantity < 0)
                    {
                        if (inventory_v01.IsBackOrder)
                        {
                            return quantity;
                        }
                        else
                        {
                            return inventory_v01.QuantityAvailable;
                        }
                    }
                    isBlocked = inventory_v01.IsBlocked;
                }
                else
                {
                    if (cItem.SKU.Trim() == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku ||
                            HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(cItem.SKU.Trim())))
                    {
                        return quantity;
                    }
                    else return 0;
                }
            }
            return quantity;
        }

        private static bool productTypeOk(ServiceProvider.CatalogSvc.ProductType type)
        {
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.allowBackorderForPromoTypeOnly)
            {
                return (type == ServiceProvider.CatalogSvc.ProductType.PromoAccessory);
            }
            else if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderInventorySKUOnly)
            {
                bool isOk = (type == ServiceProvider.CatalogSvc.ProductType.Product);
                if (!isOk && HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPromoType)
                {
                    isOk = (type == ServiceProvider.CatalogSvc.ProductType.PromoAccessory);
                }
                return isOk;
            }
            else
            {
                return true;
            }
        }

        public static int CheckForBackOrderableOverage(SKU_V01 sku,
                                                       int quantity,
                                                       string warehouse,
                                                       ServiceProvider.CatalogSvc.DeliveryOptionType type)
        {
            //string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(3);
            //MyHerbalife3.Ordering.Providers.Interfaces.IShippingProvider provider = MyHerbalife3.Ordering.Providers.Shipping.ShippingProvider.GetShippingProvider(countryCode);
            //List<DeliveryOption> options = provider.GetDeliveryOptions(Common.ValueObjects.DeliveryOptionType.Shipping, provider.GetDefaultAddress());
            //List<string> shippingWarehouseCodes = (from p in options where p.Option == Common.ValueObjects.DeliveryOptionType.Shipping select p.WarehouseCode).ToList();
            var catalogItem = sku.CatalogItem; // CatalogProvider.GetCatalogItem(sku, countryCode);
            if (catalogItem == null)
            {
                return 0;
            }
            if (catalogItem.IsEventTicket)
            {
                return 0;
            }
            if (!catalogItem.IsInventory)
            {
                return 0;
            }
            if (type == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping && sku.ProductAvailability != ProductAvailabilityType.Unavailable)
            {
                if (!productTypeOk(catalogItem.ProductType))
                    return 0;
                var iList = catalogItem.InventoryList;
                if (iList != null)
                {
                    WarehouseInventory inventory = null;
                    WarehouseInventory_V01 inventory_v01 = null;
                    if (iList.TryGetValue(warehouse, out inventory))
                    {
                        inventory_v01 = inventory as WarehouseInventory_V01;
                        if (inventory_v01.QuantityAvailable > 0)
                        {
                            if (inventory_v01.QuantityAvailable < quantity)
                            {
                                //if (shippingWarehouseCodes.Contains(inventory_v01.WarehouseCode))
                                //{
                                //    //22863, Review cart needs to display as green
                                //    //inventory_v01.IsBackOrder = true;
                                //    return quantity - inventory_v01.QuantityAvailable;
                                //}

                                // Check if can be split and cover
                                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit) &&
                                    HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Contains(warehouse))
                                {
                                    var whAvailableQty = 0;
                                    var alternativeWhs = new List<string>();
                                    alternativeWhs.AddRange(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Split(',').Where(w => w != warehouse));
                                    foreach (var wh in alternativeWhs)
                                    {
                                        var isBackOrder = false;
                                        var isBlocked = false;
                                        whAvailableQty = GetQuantityAvailableInWarehouse(catalogItem, quantity, wh, ref isBackOrder, ref isBlocked);
                                        if (whAvailableQty >= quantity)
                                        {
                                            break;
                                        }
                                    }

                                    return (whAvailableQty >= quantity) ? 0 : (quantity - whAvailableQty);
                                }
                                else
                                {
                                    return quantity - inventory_v01.QuantityAvailable;
                                }
                            }
                        }
                    }
                }
            }
            else if (type == ServiceProvider.CatalogSvc.DeliveryOptionType.Pickup)
            {
                if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupAllTypes)
                {
                    var iList = catalogItem.InventoryList;
                    if (iList != null)
                    {
                        WarehouseInventory inventory = null;
                        WarehouseInventory_V01 inventory_v01 = null;
                        if (iList.TryGetValue(warehouse, out inventory))
                        {
                            inventory_v01 = inventory as WarehouseInventory_V01;
                            if (inventory_v01.QuantityAvailable > 0)
                            {
                                if (inventory_v01.QuantityAvailable < quantity)
                                {
                                    return quantity - inventory_v01.QuantityAvailable;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        ///     Gets an specific shopping cart from service. In this case, the rules are applyed.
        /// </summary>
        /// <param name="shoppingCartId">The shopping cart ID.</param>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="locale">The locale for the cart.</param>
        /// <returns>The shopping cart.</returns>
        public static MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                                  string distributorID,
                                                                  string locale)
        {
            return GetShoppingCartFromService(shoppingCartId, distributorID, locale, false);
        }

        public static string GetDSFraudResxKey(DRFraudStatusType fraudStatus)
        {
            if (fraudStatus == DRFraudStatusType.DistributorIsBlocked)
            {
                return "BlockedDS";
            }
            else if (fraudStatus == DRFraudStatusType.PostalCodeIsBlocked)
            {
                return "BlockedZip";
            }
            return string.Empty;
        }

        public static void CheckDSFraud(MyHLShoppingCart ShoppingCart)
        {
            if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
            {
                if (ShoppingCart.DeliveryInfo != null &&
                    null != ShoppingCart.DeliveryInfo.Address &&
                    null != ShoppingCart.DeliveryInfo.Address.Address)
                {
                    ShoppingCart.DSFraudValidationError = ShoppingCartProvider.GetDSFraudResxKey(DistributorOrderingProfileProvider.CheckForDRFraud(
                        ShoppingCart.DistributorID, ShoppingCart.CountryCode,
                        ShoppingCart.DeliveryInfo.Address.Address.PostalCode));

                    ShoppingCart.PassDSFraudValidation = string.IsNullOrEmpty(ShoppingCart.DSFraudValidationError);
                }
            }
        }

        /// <summary>
        ///     Gets a specific shopping cart from service.
        /// </summary>
        /// <param name="shoppingCartId">The shopping cart ID.</param>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="locale">The locale for the cart.</param>
        /// <param name="ignoreRules">To indicate if the rules must be ignored.</param>
        /// <returns>The shopping cart.</returns>
        public static MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                                  string distributorID,
                                                                  string locale,
                                                                  bool ignoreRules)
        {
            if (shoppingCartId == 0)
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new GetShoppingCartRequest_V03() { DistributorID = distributorID, Locale = locale, OrderCategory = OrderCategoryType.RSO };
                        request.ShoppingCartID = shoppingCartId;
                        var response =
                            proxy.GetShoppingCart(new GetShoppingCartRequest1(request)).GetShoppingCartResult as GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                            getCarts(response.ShoppingCartList, distributorID, locale);

                            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                            sessionInfo.ShoppingCart = theCart;

                            theCart.RuleResults = new List<ShoppingCartRuleResult>();
                            theCart.GetShoppingCartForDisplay(true);
                            try
                            {
                                //theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID, theCart.DeliveryOption);
                                theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID,
                                                         theCart.DeliveryOption, theCart.OrderCategory,
                                                         theCart.FreightCode, theCart.IsSavedCart);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        theCart.DistributorID, theCart.ShoppingCartID, theCart.DeliveryOptionID,
                                        theCart.ShippingAddressID, theCart.DeliveryOption.ToString(), ex.Message));
                            }
                            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            {
                                if (PurchasingLimitProvider.RequirePurchasingLimits(theCart.DistributorID, theCart.CountryCode))
                                {
                                    PurchasingLimitProvider.GetPurchasingLimits(theCart.DistributorID, theCart.OrderSubType ?? string.Empty);
                                }
                            }
                            if (null != response && null != response.ShoppingCartList &&
                                response.ShoppingCartList.Count > 0)
                            {
                                if (
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                   .ShippingCodesAreConsolidated)
                                {
                                    var provider =
                                        ShippingProvider.GetShippingProvider(theCart.CountryCode);
                                    if (provider != null && theCart.DeliveryInfo != null)
                                    {
                                        theCart.DeliveryInfo.Instruction =
                                            provider.GetShippingInstructionsForShippingInfo(
                                                response.ShoppingCartList.First(), theCart, theCart.DistributorID,
                                                theCart.Locale);
                                    }
                                }
                            }
                            theCart.Calculate();
                            //Check for DR fraud..
                            ShoppingCartProvider.CheckDSFraud(theCart);
                            //if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            //{
                            //    if (theCart.DeliveryInfo != null &&
                            //        null != theCart.DeliveryInfo.Address &&
                            //        null != theCart.DeliveryInfo.Address.Address)
                            //    {
                            //        DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                            //                                                                             theCart
                            //                                                                                 .CountryCode,
                            //                                                                             theCart
                            //                                                                                 .DeliveryInfo
                            //                                                                                 .Address
                            //                                                                                 .Address
                            //                                                                                 .PostalCode);
                            //        theCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                            //        theCart.PassDSFraudValidation = string.IsNullOrEmpty(theCart.DSFraudValidationError);
                            //    }
                            //}

                            if (!ignoreRules)
                            {
                                var results = HLRulesManager.Manager.ProcessCart(theCart,
                                                                                 ShoppingCartRuleReason
                                                                                     .CartCreated);
                                //if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                                //{
                                //    results = HLRulesManager.Manager.ProcessCart(ShoppingCartRuleReason.CartCreated, theCart);
                                //}
                                }
                            }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                        LoggerHelper.Error(
                            string.Format("getShoppingCartFromService CartId:{0} ERR:{1}", shoppingCartId, ex));
                        return null;
                    }
                }
            }
        }

        public static MyHLShoppingCart GetBasicShoppingCartFromService(int shoppingCartId,
                                                                       string distributorID,
                                                                       string locale)
        {
            if (shoppingCartId == 0)
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new GetShoppingCartRequest_V03() { DistributorID = distributorID, Locale = locale, OrderCategory = OrderCategoryType.RSO };
                        request.ShoppingCartID = shoppingCartId;
                        var response =
                            proxy.GetShoppingCart(new GetShoppingCartRequest1(request)).GetShoppingCartResult as GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                            getCarts(response.ShoppingCartList, distributorID, locale);

                            theCart.RuleResults = new List<ShoppingCartRuleResult>();
                            theCart.GetShoppingCartForDisplay(false);
                            try
                            {
                                theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID,
                                                         theCart.DeliveryOption, theCart.OrderCategory,
                                                         theCart.FreightCode);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        theCart.DistributorID, theCart.ShoppingCartID, theCart.DeliveryOptionID,
                                        theCart.ShippingAddressID, theCart.DeliveryOption.ToString(), ex.Message));
                            }
                        }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                        LoggerHelper.Error(
                            string.Format("getBasicShoppingCartFromService CartId:{0} ERR:{1}", shoppingCartId,
                                          ex));
                        return null;
                    }
                }
            }
        }
        public static MyHLShoppingCart GetUnfilteredShoppingCartForCopyFromServiceint (int shoppingCartId,
                                                                         string distributorId,
                                                                         string locale,
                                                                         int newShoppingCartId, SKU_V01ItemCollection SkuitemList)
        {
            if (CatalogProxyInterface != null)
            {
                var request = new CopyShoppingCartRequest_V01() { DistributorID = distributorId, Locale = locale.Trim(), ShoppingCartID = shoppingCartId, NewShoppingCartID = newShoppingCartId };
                var response = CatalogProxyInterface.CopyShoppingCart(new CopyShoppingCartRequest(request)).CopyShoppingCartResult as GetShoppingCartResponse_V03;
                if (response != null) return new MyHLShoppingCart(response.ShoppingCartList.First());
            }

            if (shoppingCartId == 0)
            {
                return new MyHLShoppingCart();
            }
            else
            {
                MyHLShoppingCart theCart = new MyHLShoppingCart();
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new CopyShoppingCartRequest_V01() { DistributorID = distributorId, Locale = locale.Trim(), ShoppingCartID = shoppingCartId, NewShoppingCartID = newShoppingCartId };
                        var response = proxy.CopyShoppingCart(new CopyShoppingCartRequest(request)).CopyShoppingCartResult as GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                            theCart.IsFromCopy = true;

                        }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error("Error when loading cart from service, distributorId:" + distributorId +
                                           "error:" + ex.StackTrace);
                        return new MyHLShoppingCart();
                    }
                }
            }
        }

        public static MyHLShoppingCart GetShoppingCartForCopyFromService(int shoppingCartId,
                                                                         string distributorID,
                                                                         string locale,
                                                                         int newShoppingCartID, SKU_V01ItemCollection SkuitemList)
        {
            if (shoppingCartId == 0)
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new CopyShoppingCartRequest_V01() { DistributorID = distributorID, Locale = locale.Trim(), ShoppingCartID = shoppingCartId, NewShoppingCartID = newShoppingCartID };
                        var response =
                            proxy.CopyShoppingCart(new CopyShoppingCartRequest(request)).CopyShoppingCartResult as GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                            theCart.IsFromCopy = true;
                            //getCarts(response.ShoppingCartList, distributorID, locale);

                            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                            sessionInfo.ShoppingCart = theCart;

                            theCart.RuleResults = new List<ShoppingCartRuleResult>();
                            var SKUsToRemove = new List<string>(new[]
                                {
                                    HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                                    HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                                    HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                                });
                            SKUsToRemove.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

                            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                            {
                            foreach (var cartitem in theCart.CartItems)
                            {
                                SKU_V01 sku;
                                if (SkuitemList != null && SkuitemList.TryGetValue(cartitem.SKU, out sku))
                                {
                                    if (!sku.IsPurchasable)
                                    {
                                        SKUsToRemove.Add(sku.SKU);
                                    }
                                }
                            }
                            }
                            Array.ForEach(SKUsToRemove.ToArray(),
                                          a => theCart.CartItems.Remove(theCart.CartItems.Find(x => x.SKU == a)));

                            theCart.GetShoppingCartForDisplay(true);
                            try
                            {
                                // Load order shipping addresses.
                                var provider = ShippingProvider.GetShippingProvider(theCart.CountryCode);
                                provider.ReloadOrderShippingAddressFromService(distributorID, locale);

                                var deliveryOption = theCart.DeliveryOption;

                                // Load address from the Order shipping address tabe.
                                //theCart.LoadOrderShippingInfo(
                                //    theCart.DeliveryOption == DeliveryOptionType.Shipping || theCart.DeliveryOption == DeliveryOptionType.Unknown
                                //    ? theCart.ShippingAddressID
                                //    : theCart.DeliveryOptionID, theCart.FreightCode);

                                // Matching shipping information.
                                int newShippingAddressID, newDeliveryOptionID;
                                ServiceProvider.CatalogSvc.DeliveryOptionType newDeliveryOptionType;
                                theCart.MatchShippingInfo(shoppingCartId, deliveryOption, theCart.ShippingAddressID,
                                                          theCart.DeliveryOptionID, out newDeliveryOptionType,
                                                          out newShippingAddressID, out newDeliveryOptionID);

                                // Loading the address from the .normal tables.
                                theCart.LoadShippingInfo(theCart.DeliveryOptionID = newDeliveryOptionID,
                                                         theCart.ShippingAddressID = newShippingAddressID,
                                                         theCart.DeliveryOption = newDeliveryOptionType,
                                                         theCart.OrderCategory, theCart.FreightCode);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        theCart.DistributorID, theCart.ShoppingCartID, theCart.DeliveryOptionID,
                                        theCart.ShippingAddressID, theCart.DeliveryOption.ToString(), ex.Message));
                            }

                            theCart.OrderSubType = theCart.OrderSubType != null ? theCart.OrderSubType.Trim() : theCart.OrderSubType;
                            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            {
                                if (PurchasingLimitProvider.RequirePurchasingLimits(theCart.DistributorID, theCart.CountryCode))
                                {
                                    PurchasingLimitProvider.GetPurchasingLimits(theCart.DistributorID, theCart.OrderSubType ?? string.Empty);
                                }
                            }
                            if (null != response && null != response.ShoppingCartList &&
                                response.ShoppingCartList.Count > 0)
                            {
                                if (
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                   .ShippingCodesAreConsolidated)
                                {
                                    var provider =
                                        ShippingProvider.GetShippingProvider(theCart.CountryCode);
                                    if (provider != null && theCart.DeliveryInfo != null)
                                    {
                                        theCart.DeliveryInfo.Instruction =
                                            provider.GetShippingInstructionsForShippingInfo(
                                                response.ShoppingCartList.First(), theCart, theCart.DistributorID,
                                                theCart.Locale);
                                    }
                                }
                            }
                            theCart.Calculate();
                            //Check for DR fraud..
                            ShoppingCartProvider.CheckDSFraud(theCart);
                            //if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            //{
                            //    if (theCart.DeliveryInfo != null &&
                            //        null != theCart.DeliveryInfo.Address &&
                            //        null != theCart.DeliveryInfo.Address.Address)
                            //    {
                            //        DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                            //                                                                             theCart
                            //                                                                                 .CountryCode,
                            //                                                                             theCart
                            //                                                                                 .DeliveryInfo
                            //                                                                                 .Address
                            //                                                                                 .Address
                            //                                                                                 .PostalCode);
                            //        theCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                            //        theCart.PassDSFraudValidation = string.IsNullOrEmpty(theCart.DSFraudValidationError);
                            //    }
                            //}
                            var results = HLRulesManager.Manager.ProcessCart(theCart,
                                                                             ShoppingCartRuleReason
                                                                                 .CartCreated);
                            //if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            //{
                            //    results = HLRulesManager.Manager.ProcessCart(ShoppingCartRuleReason.CartCreated, theCart);
                            //}
                            
                        }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        //WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                        LoggerHelper.Error(
                            string.Format("getShoppingCartFromService CartId:{0} ERR:{1}", shoppingCartId, ex));
                        return null;
                    }
                }
            }
        }

        public static MyHLShoppingCart GetShoppingCartFromService(string distributorID,
                                                                  string locale,
                                                                  string customerOrderNumber,
                                                                  bool updateSession = true, bool noUpdateDeliveryInfoToDB = false)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                        bool isEventTicketMode = sessionInfo == null ? false : sessionInfo.IsEventTicketMode;
                        bool isHAPMode = sessionInfo == null ? false : sessionInfo.IsHAPMode;
                        var response = proxy.GetShoppingCart(new GetShoppingCartRequest1(new GetShoppingCartRequest_V03()
                        {
                            DistributorID = distributorID,
                            Locale = locale,
                            OrderCategory = isEventTicketMode
                                            ? OrderCategoryType.ETO
                                            : isHAPMode ? OrderCategoryType.HSO : OrderCategoryType.RSO,
                            CustomerOrderID = customerOrderNumber
                        })).GetShoppingCartResult as
                            GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                            {
                                var allSKU = CatalogProvider.GetAllSKU(locale);
                                SKU_V01 PrmoSku;
                                var SKUsToRemove = new List<string>();
                                foreach (var cartlist in response.ShoppingCartList)
                                {
                                    foreach (var cartitem in cartlist.CartItems)
                                    {
                                        allSKU.TryGetValue(cartitem.SKU, out PrmoSku);
                                        if (PrmoSku != null)
                                        {
                                            if (!PrmoSku.IsPurchasable)
                                            {
                                                SKUsToRemove.Add(PrmoSku.SKU.Trim());
                                            }
                                        }
                                    }
                                    Array.ForEach(SKUsToRemove.ToArray(),
                                           a => cartlist.CartItems.Remove(cartlist.CartItems.Find(x => x.SKU == a)));

                                }
                            }
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());
                            if (string.IsNullOrEmpty(customerOrderNumber))
                            {
                                getCarts(response.ShoppingCartList, distributorID, locale);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(customerOrderNumber))
                            {
                                return null;
                            }
                            theCart = createShoppingCart(distributorID, locale);
                        }
                        if (theCart != null)
                        {
                            if (updateSession && null != sessionInfo)
                            {
                                sessionInfo.ShoppingCart = theCart;
                            }

                            theCart.RuleResults = new List<ShoppingCartRuleResult>();
                            theCart.GetShoppingCartForDisplay(true);
                            if (!string.IsNullOrEmpty(theCart.FreightCode))
                            {
                                theCart.FreightCode = theCart.FreightCode.Trim();
                            }
                            if (!string.IsNullOrEmpty(theCart.OrderSubType))
                            {
                                theCart.OrderSubType = theCart.OrderSubType.Trim();
                            }
                            try
                            {
                                //theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID, theCart.DeliveryOption);
                                theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID,
                                                         theCart.DeliveryOption, theCart.OrderCategory,
                                                         theCart.FreightCode, noUpdateDeliveryInfoToDB);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        theCart.DistributorID, theCart.ShoppingCartID, theCart.DeliveryOptionID,
                                        theCart.ShippingAddressID, theCart.DeliveryOption.ToString(), ex.Message));
                            }

                            if (theCart.DeliveryInfo != null)
                            {
                                var shippingInfo = theCart.DeliveryInfo;
                                //ETO doesn't allow pickup if it's in configuration
                                if (isEventTicketMode)
                                {
                                    if (shippingInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup &&
                                        !HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                        .PickupAllowForEventTicket)
                                    {
                                        theCart.DeliveryInfo = null;
                                    }
                                }
                                // if there is freight code in database and is not APF
                                if (!string.IsNullOrEmpty(theCart.FreightCode) &&
                                    theCart.FreightCode !=
                                    HLConfigManager.Configurations.APFConfiguration.APFFreightCode)
                                {
                                    theCart.DeliveryInfo.FreightCode = theCart.FreightCode;
                                }
                            }

                            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            {
                                if (PurchasingLimitProvider.RequirePurchasingLimits(theCart.DistributorID, theCart.CountryCode))
                                {
                                    PurchasingLimitProvider.GetPurchasingLimits(theCart.DistributorID, theCart.OrderSubType ?? string.Empty);
                                }
                            }
                            if (null != response && null != response.ShoppingCartList &&
                                response.ShoppingCartList.Count > 0)
                            {
                                if (
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                   .ShippingCodesAreConsolidated)
                                {
                                    var provider =
                                        ShippingProvider.GetShippingProvider(theCart.CountryCode);
                                    if (provider != null && theCart.DeliveryInfo != null)
                                    {
                                        theCart.DeliveryInfo.Instruction =
                                            provider.GetShippingInstructionsForShippingInfo(
                                                response.ShoppingCartList.First(), theCart, theCart.DistributorID,
                                                theCart.Locale);
                                    }
                                }
                            }
                            theCart.Calculate();

                            //Check for DR fraud..
                            ShoppingCartProvider.CheckDSFraud(theCart);
                            //if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            //{
                            //    if (theCart.DeliveryInfo != null &&
                            //        null != theCart.DeliveryInfo.Address &&
                            //        null != theCart.DeliveryInfo.Address.Address)
                            //    {
                            //        DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                            //                                                                             theCart
                            //                                                                                 .CountryCode,
                            //                                                                             theCart
                            //                                                                                 .DeliveryInfo
                            //                                                                                 .Address
                            //                                                                                 .Address
                            //                                                                                 .PostalCode);
                            //        theCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                            //        theCart.PassDSFraudValidation = string.IsNullOrEmpty(theCart.DSFraudValidationError);
                            //    }
                            //}

                            var results = HLRulesManager.Manager.ProcessCart(theCart,
                                                                             ShoppingCartRuleReason
                                                                                 .CartCreated);
                            //if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            //{
                            //    results = HLRulesManager.Manager.ProcessCart(ShoppingCartRuleReason.CartCreated, theCart);
                            //}

                        }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        //WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                        LoggerHelper.Error(
                            string.Format("getShoppingCartFromService DS:{0} locale:{2} ERR:{1}", distributorID,
                                          ex, locale));
                        return null;
                    }
                }
            }
        }

        public static MyHLShoppingCart GetShoppingCartFromService(OrderViewModel order, string distributorID,
                                                                  string locale,
                                                                  string customerOrderNumber,
                                                                  bool updateSession = true, bool noUpdateDeliveryInfoToDB = false)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                        bool isEventTicketMode = sessionInfo == null ? false : sessionInfo.IsEventTicketMode;
                        bool isHAPMode = sessionInfo == null ? false : sessionInfo.IsHAPMode;
                        var response =
                            proxy.GetShoppingCart(new GetShoppingCartRequest1(new GetShoppingCartRequest_V03()
                            {
                                DistributorID = distributorID,
                                Locale = locale,
                                OrderCategory = isEventTicketMode
                                                                                     ? OrderCategoryType.ETO
                                                                                     : isHAPMode ? OrderCategoryType.HSO : OrderCategoryType.RSO,
                                CustomerOrderID = customerOrderNumber
                            })).GetShoppingCartResult as GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            theCart = new MyHLShoppingCart(response.ShoppingCartList.First());

                            if(!string.IsNullOrEmpty(order.Shipping.ShippingMethodId))
                            {
                                theCart.FreightCode = order.Shipping.ShippingMethodId;
                            }
                            
                            if (string.IsNullOrEmpty(customerOrderNumber))
                            {
                                getCarts(response.ShoppingCartList, distributorID, locale);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(customerOrderNumber))
                            {
                                return null;
                            }
                            theCart = createShoppingCart(distributorID, locale);
                        }
                        if (theCart != null)
                        {
                            if (updateSession && null != sessionInfo)
                            {
                                sessionInfo.ShoppingCart = theCart;
                            }

                            theCart.RuleResults = new List<ShoppingCartRuleResult>();
                            theCart.GetShoppingCartForDisplay(true);
                            if (!string.IsNullOrEmpty(theCart.FreightCode))
                            {
                                theCart.FreightCode = theCart.FreightCode.Trim();
                            }
                            if (!string.IsNullOrEmpty(theCart.OrderSubType))
                            {
                                theCart.OrderSubType = theCart.OrderSubType.Trim();
                            }
                            try
                            {
                                //theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID, theCart.DeliveryOption);
                                theCart.LoadShippingInfo(theCart.DeliveryOptionID, theCart.ShippingAddressID,
                                                         theCart.DeliveryOption, theCart.OrderCategory,
                                                         theCart.FreightCode, noUpdateDeliveryInfoToDB);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        theCart.DistributorID, theCart.ShoppingCartID, theCart.DeliveryOptionID,
                                        theCart.ShippingAddressID, theCart.DeliveryOption.ToString(), ex.Message));
                            }

                            if (theCart.DeliveryInfo != null)
                            {
                                var shippingInfo = theCart.DeliveryInfo;
                                //ETO doesn't allow pickup if it's in configuration
                                if (isEventTicketMode)
                                {
                                    if (shippingInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup &&
                                        !HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                        .PickupAllowForEventTicket)
                                    {
                                        theCart.DeliveryInfo = null;
                                    }
                                }
                                // if there is freight code in database and is not APF
                                if (!string.IsNullOrEmpty(theCart.FreightCode) &&
                                    theCart.FreightCode !=
                                    HLConfigManager.Configurations.APFConfiguration.APFFreightCode)
                                {
                                    theCart.DeliveryInfo.FreightCode = theCart.FreightCode;
                                }
                            }

                            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            {
                                if (PurchasingLimitProvider.RequirePurchasingLimits(theCart.DistributorID, theCart.CountryCode))
                                {
                                    PurchasingLimitProvider.GetPurchasingLimits(theCart.DistributorID, theCart.OrderSubType ?? string.Empty);
                                }
                            }
                            if (null != response && null != response.ShoppingCartList &&
                                response.ShoppingCartList.Count > 0)
                            {
                                if (
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                   .ShippingCodesAreConsolidated)
                                {
                                    var provider =
                                        ShippingProvider.GetShippingProvider(theCart.CountryCode);
                                    if (provider != null && theCart.DeliveryInfo != null)
                                    {
                                        theCart.DeliveryInfo.Instruction =
                                            provider.GetShippingInstructionsForShippingInfo(
                                                response.ShoppingCartList.First(), theCart, theCart.DistributorID,
                                                theCart.Locale);
                                    }
                                }
                            }
                            theCart.Calculate();

                            //Check for DR fraud..
                            ShoppingCartProvider.CheckDSFraud(theCart);
                            //if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            //{
                            //    if (theCart.DeliveryInfo != null &&
                            //        null != theCart.DeliveryInfo.Address &&
                            //        null != theCart.DeliveryInfo.Address.Address)
                            //    {
                            //        DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                            //                                                                             theCart
                            //                                                                                 .CountryCode,
                            //                                                                             theCart
                            //                                                                                 .DeliveryInfo
                            //                                                                                 .Address
                            //                                                                                 .Address
                            //                                                                                 .PostalCode);
                            //        theCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                            //        theCart.PassDSFraudValidation = string.IsNullOrEmpty(theCart.DSFraudValidationError);
                            //    }
                            //}

                            var results = HLRulesManager.Manager.ProcessCart(theCart,
                                                                             ShoppingCartRuleReason
                                                                                 .CartCreated);
                            //if (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                            //{
                            //    results = HLRulesManager.Manager.ProcessCart(ShoppingCartRuleReason.CartCreated, theCart);
                            //}

                        }
                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        //WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                        LoggerHelper.Error(
                            string.Format("getShoppingCartFromService DS:{0} locale:{2} ERR:{1}", distributorID,
                                          ex, locale));
                        return null;
                    }
                }
            }
        }

        //public static List<MyHLShoppingCart> GetDraftsFromService(string distributorID, string locale)
        //{
        //    List<MyHLShoppingCart> theDrafts = new List<MyHLShoppingCart>();
        //    if (string.IsNullOrEmpty(distributorID))
        //    {
        //        return theDrafts;
        //    }
        //    else
        //    {
        //        using (CatalogSVC.CatalogInterfaceClient proxy = new MyHerbalife3.Ordering.Providers.CatalogSVC.CatalogInterfaceClient())
        //        {
        //            proxy.Endpoint.Address = new System.ServiceModel.EndpointAddress(HL.Common.Configuration.Settings.GetRequiredAppSetting(CatalogServiceSettingKey));
        //            try
        //            {
        //                GetShoppingCartResponse_V03 response = proxy.GetShoppingCart(new GetShoppingCartRequest_V03(distributorID, locale, OrderCategoryType.RSO, string.Empty)) as GetShoppingCartResponse_V03;
        //                if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
        //                {
        //                    theDrafts = (from d in response.ShoppingCartList
        //                                 select new MyHLShoppingCart(d)).ToList();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                WebUtilities.LogServiceExceptionWithContext<CatalogSVC.ICatalogInterface>(ex, proxy);
        //                LoggerHelper.Error(string.Format("GetDraftsFromService DS:{0} locale:{2} ERR:{1}", distributorID, ex.ToString(), locale), "Shoppingcart");
        //                return theDrafts;
        //            }
        //        }
        //        return theDrafts;
        //    }
        //}

        public static MyHLShoppingCart createShoppingCart(string distributorID, string locale)
        {
            MyHLShoppingCart myHLShoppingCart = null;
            try
            {
                string countryCode = locale.Substring(3, 2);
                var shippingProvider =
                    ShippingProvider.GetShippingProvider(countryCode);
                DeliveryOption primaryShippingAddress = null;
                var shippingAddresses = shippingProvider.GetShippingAddresses(distributorID, locale);
                if (null != shippingAddresses)
                {
                var varPrimaryAddress = shippingAddresses.Where(s => s.IsPrimary);
                if (varPrimaryAddress.Count() > 0)
                {
                    primaryShippingAddress = varPrimaryAddress.First();
                }
                }
                else
                {
                    var shippingAddress = shippingProvider.GetShippingAddressesFromService(distributorID, locale);
                    var varPrimaryAddress = shippingAddress.Where(s => s.IsPrimary);
                    if (varPrimaryAddress.Count() > 0)
                    {
                        primaryShippingAddress = varPrimaryAddress.First();
                }
                }
                var shippingInfo =
                    new ShippingInfo(ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping,
                                     primaryShippingAddress);
                var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                myHLShoppingCart = InsertShoppingCart(distributorID, locale,
                                                      sessionInfo.IsEventTicketMode
                                                          ? OrderCategoryType.ETO
                                                          : sessionInfo.IsHAPMode ? OrderCategoryType.HSO : OrderCategoryType.RSO,
                                                      shippingInfo, false, string.Empty);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Error in ShoppingCartProvider createShoppingCart. error message:  {0}; \r\n Stack Info: {1}",
                        ex.GetBaseException().Message, ex.GetBaseException().StackTrace));
            }
            return myHLShoppingCart;
        }

        public static bool CartExists(string distributorID, string locale, string cartName)
        {
            var carts = GetCarts(distributorID, locale);
            if (carts != null && carts.Exists(d => d.CartName.ToUpper() == cartName.ToUpper()))
            {
                return true;
            }
            return false;
        }

        public static MyHLShoppingCart InsertShoppingCart(string distributorID,
                                                          string locale,
                                                          OrderCategoryType orderCategory,
                                                          ShippingInfo shippingInfo,
                                                          bool isDraft,
                                                          string cartName)
        {
            if (isDraft)
            {
                var carts = GetCarts(distributorID, locale);
                if (carts != null && carts.Exists(d => (d.CartName ?? "").ToUpper() == cartName.ToUpper()))
                {
                    return null;
                }

                var newCart = InsertShoppingCart(distributorID, locale, orderCategory, shippingInfo, null,
                                                 isDraft, cartName);
                if (newCart != null)
                {
                    carts = carts ?? new List<MyHLShoppingCart>();
                    var cart = carts.Find(d => d.ShoppingCartID == newCart.ShoppingCartID);
                    if (cart != null)
                    {
                        carts.Remove(cart);
                    }
                    carts.Add(newCart);
                    HttpRuntime.Cache.Insert(GetCartsCacheKey(distributorID, locale), carts, null,
                                             DateTime.Now.AddMinutes(60), Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal, null);
                }
                return newCart;
            }
            else
            {
                return InsertShoppingCart(distributorID, locale, orderCategory, shippingInfo, null, isDraft, cartName);
            }
        }

        public static MyHLShoppingCart InsertShoppingCart(string distributorID,
                                                          string locale,
                                                          OrderCategoryType orderCategory,
                                                          ShippingInfo shippingInfo,
                                                          CustomerOrderDetail customerOrderDetail,
                                                          bool isDraft,
                                                          string draftName)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                distributorID = distributorID.ToUpper();
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response = proxy.InsertShoppingCart(new InsertShoppingCartRequest1(new InsertShoppingCartRequest_V03()
                        {
                            DistributorID = distributorID,
                            Locale = locale,
                            DeliveryOptionID = shippingInfo != null ? shippingInfo.Id : 0,
                            OrderCategory = orderCategory,
                            DeliveryOption = shippingInfo != null
                            ? (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), shippingInfo.Option.ToString())
                            : MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown,
                            ShippingAddressID = (shippingInfo != null && shippingInfo.Address != null) ? shippingInfo.Address.ID : 0,
                            CustomerOrderDetail = customerOrderDetail,
                            IsDraft = isDraft,
                            DraftName = draftName,
                            CreationDateTime = DateUtils.ConvertToLocalDateTime(DateTime.Now, locale.Substring(3))
                        })).InsertShoppingCartResult as InsertShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCart &&
                            response.Status == ServiceResponseStatusType.Success)
                        {
                            return new MyHLShoppingCart(response.ShoppingCart);
                        }
                    }
                    catch (Exception ex)
                    {
                        //WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        LoggerHelper.Error(
                            string.Format("InsertShoppingCart error: DS {0}, locale{1},shippingInfoId: {2}, OrderCategory: {3}, ShippingInfoOption: {4}, ShippingInfoAddress: {5}, error: {6}", distributorID,
                                          locale,orderCategory,orderCategory, shippingInfo.Option,shippingInfo.Address, ex));
                    }
                }
            }
            return null;
        }

        public static bool InsertShoppingCartItem(string distributorID, ShoppingCartItem_V01 item, int shoppingCartID)
        {
            if (shoppingCartID == 0 || item == null || item.SKU.Trim() == string.Empty)
            {
                return false;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response =
                            proxy.InsertShoppingCartItem(new InsertShoppingCartItemRequest1(new InsertShoppingCartItemRequest_V01() { Item = item, ShoppingCartID = shoppingCartID })).InsertShoppingCartItemResult as
                            InsertShoppingCartItemResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                        LoggerHelper.Error(
                            string.Format("InsertShoppingCartItem error: DS {0}, shoppingCartID{1}, error: {2}",
                                          distributorID, shoppingCartID.ToString(), ex));
                    }
                }
            }
            return false;
        }

        public static bool InsertShoppingCartItemList(string distributorID, List<ShoppingCartItem_V01> items, int shoppingCartId)
        {
            if (shoppingCartId == 0 || items == null)
            {
                return false;
            }
            else
            {
                if (items.Any())
                {
                    using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                    {
                        try
                        {
                            var response = proxy.InsertShoppingCartItem(new InsertShoppingCartItemRequest1(new InsertShoppingCartItemRequest_V02() { Items = items, ShoppingCartID = shoppingCartId })).InsertShoppingCartItemResult
                                as InsertShoppingCartItemResponse_V02;
                            if (response != null && response.Status == ServiceResponseStatusType.Success)
                            {
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {

                            LoggerHelper.Error(
                                string.Format("InsertShoppingCartItem error: DS {0}, shoppingCartID{1}, error: {2}",
                                              distributorID, shoppingCartId.ToString(), ex));
                        }
                    }
                }
            }
            return false;
        }


        private static bool validateDeliveryInfo(MyHLShoppingCart shoppingCart)
        {
            if (null != shoppingCart.DeliveryInfo)
            {
                var shippingInfo = shoppingCart.DeliveryInfo;
            }
            return false;
        }

        public static MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart == null || string.IsNullOrEmpty(shoppingCart.DistributorID) || string.IsNullOrEmpty(shoppingCart.Locale))
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    UpdateShoppingCartResponse_V02 response = null;
                    try
                    {
                        string orderSubType = (string.IsNullOrEmpty(shoppingCart.OrderSubType))
                                                  ? string.Empty
                                                  : shoppingCart.OrderSubType;
                        if (null == shoppingCart.DeliveryInfo)
                        {
                            response = proxy.UpdateShoppingCart(new UpdateShoppingCartRequest1(new UpdateShoppingCartRequest_V02()
                            {
                                ShoppingCartID = shoppingCart.ShoppingCartID,
                                DeliveryOptionID = 0,
                                OrderCategory = shoppingCart.OrderCategory,
                                DeliveryOption = ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown,
                                ShippingAddressID = 0,
                                FreightCode = string.Empty,
                                OrderSubType = orderSubType,
                                IsSavedCart = null,
                                CartName = shoppingCart.CartName,
                                OrderData = string.Empty,
                                OrderNumber = string.Empty
                            })).UpdateShoppingCartResult 
                                as UpdateShoppingCartResponse_V02;
                        }
                        else
                        {
                            response =
                                proxy.UpdateShoppingCart(new UpdateShoppingCartRequest1(new UpdateShoppingCartRequest_V02()
                                {
                                    ShoppingCartID = shoppingCart.ShoppingCartID,
                                    DeliveryOptionID = shoppingCart.DeliveryInfo.Id,
                                    OrderCategory = shoppingCart.OrderCategory,
                                    DeliveryOption = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), shoppingCart.DeliveryInfo.Option.ToString()),
                                    ShippingAddressID = (shoppingCart.DeliveryInfo
                                                                                                        .Address == null
                                                                                                ? 0
                                                                                                : shoppingCart
                                                                                                      .DeliveryInfo
                                                                                                      .Address.ID),
                                    FreightCode = shoppingCart.DeliveryInfo
                                                                                                       .FreightCode,
                                    OrderSubType = orderSubType,
                                    IsSavedCart = null,
                                    CartName = shoppingCart.CartName,
                                    OrderData = string.Empty,
                                    OrderNumber = string.Empty
                                })).UpdateShoppingCartResult
                                as UpdateShoppingCartResponse_V02;
                        }
                        if (null != response && response.Status == ServiceResponseStatusType.Success)
                        {
                            return shoppingCart;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("UpdateShoppingCart error: DS {0}, locale{1}, error: {2} : {3}",
                                          shoppingCart.DistributorID, shoppingCart.Locale, ex, ex.StackTrace));
                    }
                }
            }
            return null;
        }

        public static MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart,
                                                          string orderXML,
                                                          string orderNumber,
                                                          DateTime orderDate)
        {
            if (shoppingCart == null || null == shoppingCart.DeliveryInfo)
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    UpdateShoppingCartResponse_V02 response = null;
                    try
                    {
                        string orderSubType = (string.IsNullOrEmpty(shoppingCart.OrderSubType))
                                                  ? string.Empty
                                                  : shoppingCart.OrderSubType.Trim();
                        response =
                            proxy.UpdateShoppingCart(new UpdateShoppingCartRequest1(new UpdateShoppingCartRequest_V02()
                            {
                                ShoppingCartID = shoppingCart.ShoppingCartID,
                                DeliveryOptionID = shoppingCart.DeliveryInfo.Id,
                                OrderCategory = shoppingCart.OrderCategory,
                                DeliveryOption = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), shoppingCart.DeliveryInfo.Option.ToString()),
                                ShippingAddressID = shoppingCart.DeliveryInfo.Address.ID,
                                FreightCode = shoppingCart.DeliveryInfo
                                                                                                   .FreightCode,
                                OrderSubType = orderSubType,
                                IsSavedCart = null,
                                CartName = shoppingCart.CartName,
                                OrderData = string.Empty,
                                OrderNumber = orderNumber,
                                OrderDate = orderDate,
                                DistributorID = shoppingCart.DistributorID
                            })).UpdateShoppingCartResult as
                            UpdateShoppingCartResponse_V02;
                        if (shoppingCart.DeliveryInfo.Address != null &&
                            shoppingCart.DeliveryInfo.Address.ID < 0)
                        {
                            new ShippingProviderBase().SaveOrderShippingAddressToDB(shoppingCart.DistributorID,
                                                                                    shoppingCart.Locale,
                                                                                    new ShippingAddress_V02
                                                                                        {
                                                                                            ID =
                                                                                                shoppingCart
                                                                                        .DeliveryInfo.Address.ID,
                                                                                            Recipient =
                                                                                                shoppingCart
                                                                                        .DeliveryInfo.Address.Recipient,
                                                                                            Phone =
                                                                                                shoppingCart
                                                                                        .DeliveryInfo.Address.Phone,
                                                                                            IsPrimary =
                                                                                                shoppingCart
                                                                                        .DeliveryInfo.Address.IsPrimary,
                                                                                            Alias = string.Empty,
                                                                                            Created = DateTime.Now,
                                                                                            Address =
                                                                                                shoppingCart
                                                                                        .DeliveryInfo.Address.Address,
                                                                                        },
                                                                                    false,
                                                                                    false,
                                                                                    shoppingCart.ShoppingCartID);
                        }
                        if (null != response && response.Status == ServiceResponseStatusType.Success)
                        {
                            return shoppingCart;
                        }
                    }
                    catch (Exception ex)
                    {
      
                        LoggerHelper.Error(
                            string.Format("InsertShoppingCart error: DS {0}, locale{1}, error: {2}",
                                          shoppingCart.DistributorID, shoppingCart.Locale, ex));
                    }
                }
            }
            return null;
        }

        public static string getTodayMagazineDeletedSessionKey(string distributorID, string localc, int shoppingCartID)
        {
            return string.Format("{0}_{1}_{2}_{3}", distributorID, localc, shoppingCartID, "TodayMagazineDeleted");
        }

        public static bool TodayMagazineDeleted(string distributorID, string localc, int shoppingCartID)
        {
            string sessionKey = getTodayMagazineDeletedSessionKey(distributorID, localc, shoppingCartID);
            if (null == HttpContext.Current.Session[sessionKey])
            {
                return false;
            }
            return (bool)(HttpContext.Current.Session[sessionKey]);
        }

        public static bool DeleteOldShoppingCartForCustomerOrder(string distributorID, string customerOrderID)
        {
            if (string.IsNullOrEmpty(distributorID) || String.IsNullOrEmpty(customerOrderID))
            {
                return false;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response = proxy.DeleteShoppingCart(new DeleteShoppingCartRequest1(new DeleteShoppingCartRequest_V01()
                        {
                            ShoppingCartID = 0,
                            Distributor = distributorID,
                            SKUList = null,
                            CustomerOrderNumber = customerOrderID
                        })).DeleteShoppingCartResult as DeleteShoppingCartResponse_V01;
                        // Check response status.
                        if (response == null || response.Status != ServiceResponseStatusType.Success)
                        {
                            throw new ApplicationException("DeleteShoppingCartResponse indicates an error. Status: " +
                                                           (response == null ? "null" : response.Status.ToString()));
                        }
                        if (response.ReturnCode != 0)
                        {
                            return false;
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "DeleteOldShoppingCartForCustomerOrder error: DS {0}, CustomerOrderNumber{1}, error: {2}",
                                distributorID, customerOrderID, ex));
                    }
                } // end using
            }
            return false;
        }

        public static bool DeleteShoppingCart(MyHLShoppingCart shoppingCart, List<string> skus)
        {
            if (string.IsNullOrEmpty(shoppingCart.DistributorID) || shoppingCart.ShoppingCartID == 0)
            {
                return false;
            }
            else
            {
                //if (skus != null && skus.Count == 0)
                //{
                //    return false;
                //}
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response = proxy.DeleteShoppingCart(new DeleteShoppingCartRequest1(new DeleteShoppingCartRequest_V01()
                        {
                            ShoppingCartID = shoppingCart.ShoppingCartID,
                            Distributor = shoppingCart.DistributorID,
                            SKUList = skus
                        })).DeleteShoppingCartResult as DeleteShoppingCartResponse_V01;

                        // Check response status.
                        if (response == null || response.Status != ServiceResponseStatusType.Success)
                        {
                            throw new ApplicationException("DeleteShoppingCartResponse indicates an error. Status: " +
                                                           (response == null ? "null" : response.Status.ToString()));
                        }
                        if (response.ReturnCode != 0)
                        {
                            return false;
                        }
                        if (skus == null)
                        {
                            DeleteItemsFromSession(shoppingCart.DistributorID, shoppingCart.Locale);
                        }
                        //foreach (string item in skus)
                        //{
                        //    ShoppingCartItem_V01 sciItem = (from sci in shoppingCart.ShoppingCartItems where sci.SKU == item select sci).First();
                        //    if (null != sciItem && null != shoppingCart.DeliveryInfo && !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode))
                        //    {
                        //        IncrementInventory(sciItem.SKU, sciItem.Quantity, shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart.CountryCode);
                        //    }
                        //}
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("DeleteShoppingCart error: DS {0}, shoppingCartID{1}, error: {2}",
                                          shoppingCart.DistributorID, shoppingCart.ShoppingCartID.ToString(),
                                          ex));
                    }
                } // end using
            }
            return false;
        }

        public static List<DualMonthPair> GetDualOrderMonthDates(DateTime fromDate,
                                                                 DateTime toDate,
                                                                 string isoCountryCode)
        {
            string cacheKey = string.Format("{0}{1}", DualMonthCacheKey, isoCountryCode);
            var results = HttpRuntime.Cache[cacheKey] as List<DualMonthPair>;
            if (results == null)
            {
                results = GetDualOrderMonthDatesFromService(fromDate, toDate, isoCountryCode);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                                             results,
                                             null,
                                             DateTime.Now.AddMinutes(DualMonthCacheMinutes),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);
                }
            }
            return results;
        }

        public static DateTime GetEOMDate(DateTime date, string isoCountryCode)
        {
            string cacheKey = string.Format("EOM_{0}", isoCountryCode);
            var monthPairList = HttpRuntime.Cache[cacheKey] as List<DualMonthPair>;
            if (monthPairList == null)
            {
                monthPairList = GetEOMDateFromService(date, isoCountryCode);
                if (monthPairList != null)
                {
                    var first = monthPairList.FirstOrDefault();
                    monthPairList = new List<DualMonthPair>() { first };
                    HttpRuntime.Cache.Insert(cacheKey,
                                             monthPairList,
                                             null,
                                             DateTime.Now.AddMinutes(DualMonthCacheMinutes),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);
                }
            }

            if (monthPairList != null && monthPairList.Count > 0 && monthPairList.FirstOrDefault() != null)
            {
                return monthPairList.FirstOrDefault().MonthEndDate;
            }
            else
            {
                var eom = DateTimeUtils.GetLastDayOfMonth(date);
                eom = new DateTime(eom.Year, eom.Month, eom.Day, 23, 59, 59);
                return eom;
            }
        }

        public static List<DualMonthPair> GetDualOrderMonth(string orderMonthValue, string countryCode)
        {
            var orderMonth = DateTime.ParseExact(orderMonthValue, "yyyyMMdd", CultureInfo.InvariantCulture);
            var fromDate = DateTimeUtils.GetFirstDayOfMonth(orderMonth);
            var from = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
            var toDate = DateTimeUtils.GetLastDayOfMonth(orderMonth);
            var to = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);

            List<DualMonthPair> orderMonthList = null;

            List<DualMonthPair> dualMonth;
            if ((dualMonth = GetDualOrderMonthDatesFromService(from, to, countryCode)) != null)
            {
                var oMonth = new DateTime(orderMonth.Year, orderMonth.Month, orderMonth.Day);
                orderMonthList = (from l in dualMonth
                                  where oMonth <= l.MonthEndDate
                                  select l).ToList();

                if (orderMonthList.Count > 0)
                {
                    return orderMonthList;
                }

                //if (dualMonth.Count == 0 )
                //{
                //    return FakeDualMonth();
                //}
                return dualMonth;
            }
            return null;
        }

        private static List<DualMonthPair> FakeDualMonth()
        {
            var dualMonths = new List<DualMonthPair>();
            var dualMonthPair = new DualMonthPair() { OrderMonth = "200912", MonthEndDate = DateTime.Now };
            var dualMonthPair1 = new DualMonthPair() { OrderMonth = "200911", MonthEndDate = new DateTime(2009, 12, 20) };
            dualMonths.Add(dualMonthPair);
            dualMonths.Add(dualMonthPair1);
            return dualMonths;
        }

        public static List<DualMonthPair> GetDualOrderMonthDatesFromService(DateTime fromDate,
                                                                            DateTime toDate,
                                                                            string isoCountryCode)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetDualMonthDates(new GetDualMonthDatesRequest1(new GetDualMonthDatesRequest_V01() { CountryCode = isoCountryCode, FromDate = fromDate, ToDate = toDate })).GetDualMonthDatesResult as
                        GetDualMonthDatesResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.DualMonth;
                    }
                }
                catch (Exception ex)
                {
                    //ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return null;
        }

        private static List<DualMonthPair> GetEOMDateFromService(DateTime date, string isoCountryCode)
        {
            var result = new List<DualMonthPair>();
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var response =
                        proxy.GetEOMDate(new GetEOMDateRequest1(new GetEOMDateRequest_V01() { CountryCode = isoCountryCode, Date = date })).GetEOMDateResult as
                        GetEOMDateResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        result = response.DualMonth;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return result;
        }

        public static List<ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                               ShoppingCartItem_V01 item,
                                                               ShoppingCartRuleReason reason)
        {
            cart.CurrentItems = new ShoppingCartItemList();
            cart.CurrentItems.Add(item);
            //cart.ItemsBeingAdded = null;

            //ShoppingCart_V01 shoppingCart01 = cart.ConvertCart();
            var Results = HLRulesManager.Manager.ProcessCart(cart, reason);
            if(HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var shoppingCartRuleResults = new List<ShoppingCartRuleResult>();
                if (Results.Any(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback))
                {
                    shoppingCartRuleResults.AddRange(
                        Results.Where(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback));
                }

            }

            //Modify Results to override purchasing permissions

            if (CatalogProvider.IsDistributorExempted(cart.DistributorID))
            {
                var rules = (from rule in Results
                             where rule.RuleName == "PurchasingPermission Rules"
                             select rule);
                if (null != rules && rules.Count() > 0)
                {
                    Results.Remove(rules.FirstOrDefault());
                } 
            }

            cart.RuleResults = Results;
            cart.CurrentItems.Remove(item);

            return Results;
        }

        public static List<ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                              List<ShoppingCartItem_V01> items,
                                                              ShoppingCartRuleReason reason)
        {
            if (cart.OrderCategory == OrderCategoryType.HSO)
            {
                return cart.RuleResults;
            }
            cart.ItemsBeingAdded = new ShoppingCartItemList();
            cart.ItemsBeingAdded.AddRange(items);

            var Results = HLRulesManager.Manager.ProcessCart(reason, cart);
            cart.RuleResults = Results;
            if (cart.CurrentItems!=null) 
            cart.CurrentItems.Clear();

            return Results;
        }
        //OverlLoaded Method to take the request as the input..
        //public static Order_V01 FillOrderInfoRequest(MyHLShoppingCart cart)
        //{
        //    var order = new Order_V01();

        //    order.CountryOfProcessing = cart.CountryCode;
        //    order.DistributorID = cart.DistributorID;
        //    var shipping = new ShippingInfo_V01();
        //    shipping.FreightVariant = cart.DeliveryInfo.FreightVariant;
        //    shipping.ShippingMethodID = cart.DeliveryInfo.FreightCode;
        //    shipping.WarehouseCode = cart.DeliveryInfo.WarehouseCode;
        //    shipping.Address = cart.DeliveryInfo.Address == null
        //                           ? OrderCreationHelper.CreateDefaultAddress()
        //                           : cart.DeliveryInfo.Address.Address;
        //    order.Shipment = shipping;
        //    order.InputMethod = InputMethodType.Internet;
        //    order.ReceivedDate = DateUtils.GetCurrentLocalTime(cart.CountryCode);

        //    // TODO : Order category
        //    //order.OrderCategory = cart.OrderCategory;
        //    if (order.OrderCategory == OrderCategoryType.ETO)
        //    {
        //        string ordertype = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType;
        //        if (!string.IsNullOrEmpty(ordertype))
        //        {
        //            order.OrderCategory =
        //                (OrderCategoryType) Enum.Parse(OrderCategoryType.None.GetType(), ordertype, true);
        //        }
        //    }

        //    order.OrderMonth = DateTime.Now.ToString("yyMM");

        //    DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, cart.CountryCode);
        //    order.UseSlidingScale = HLConfigManager.Configurations.CheckoutConfiguration.UseSlidingScale;
        //    order.DiscountPercentage = order.UseSlidingScale
        //                                   ? 0
        //                                   :  distributorOrderingProfile.StaticDiscount * 100;

        //    return order;
        //}

        /// <summary>
        ///     Gets the last empty not saved cart or creates a new one.
        /// </summary>
        /// <param name="distributorID">Distributor ID.</param>
        /// <param name="locale">Locale.</param>
        /// <returns>The empty cart.</returns>
        public static MyHLShoppingCart GetNewUnsavedShoppingCart(string distributorID, string locale)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                MyHLShoppingCart theCart = null;
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                        bool isEventTicketMode = sessionInfo == null ? false : sessionInfo.IsEventTicketMode;
                        bool isHAPMode = sessionInfo == null ? false : sessionInfo.IsHAPMode;
                        var response =
                            proxy.GetShoppingCart(new GetShoppingCartRequest1(new GetShoppingCartRequest_V03()
                            {
                                DistributorID = distributorID,
                                Locale = locale,
                                OrderCategory = isEventTicketMode
                                                                                     ? OrderCategoryType.ETO
                                                                                     : isHAPMode ? OrderCategoryType.HSO : OrderCategoryType.RSO,
                                CustomerOrderID = string.Empty
                            })).GetShoppingCartResult as
                            GetShoppingCartResponse_V03;
                        if (null != response && null != response.ShoppingCartList && response.ShoppingCartList.Count > 0)
                        {
                            var emptyCarts = from s in response.ShoppingCartList
                                             where !s.IsDraft && (s.CartItems == null || s.CartItems.Count == 0)
                                             select s;
                            if (emptyCarts != null && emptyCarts.Count() > 0)
                            {
                                theCart = new MyHLShoppingCart(emptyCarts.First());
                            }
                        }
                        if (theCart == null)
                        {
                            theCart = createShoppingCart(distributorID, locale);
                        }

                        return theCart;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetNewUnsavedShoppingCart DS:{0} locale:{2} ERR:{1}", distributorID,
                                          ex, locale));
                        return null;
                    }
                }
            }
        }

        /// <summary>
        ///     validateCartItems
        /// </summary>
        /// <param name="cartItems"></param>
        /// <param name="locale"></param>
        public static string ValidateCartItem(string sku,
                                              int quantity,
                                              out int newQuantity,
                                              string locale,
                                              string warehouseCode)
        {
            string error = string.Empty;
            newQuantity = quantity;
            int maxQuantity = 0;

            if (sku == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(sku)))
            {
                maxQuantity = HLConfigManager.Configurations.DOConfiguration.HFFSkuMaxQuantity;
            }
            else
            {
                maxQuantity = HLConfigManager.Configurations.ShoppingCartConfiguration.MaxQuantity;
            }

            var allSKUs = CatalogProvider.GetAllSKU(locale, warehouseCode);
            SKU_V01 mySKU;

            int maxQty = maxQuantity;
            if (allSKUs.TryGetValue(sku, out mySKU))
            {
                maxQty = (mySKU.MaxOrderQuantity > 0 && mySKU.MaxOrderQuantity < maxQuantity)
                             ? mySKU.MaxOrderQuantity
                             : maxQuantity;
                if (quantity > maxQty)
                {
                    newQuantity = maxQty;
                    error =
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "MaxQuantity").ToString();
                    return string.Format(error, sku, maxQuantity);
                }
                if (!string.IsNullOrEmpty(warehouseCode))
                {
                    var productAvailabilityType = CatalogProvider.GetProductAvailability(mySKU,
                                                                                         warehouseCode);
                    if (productAvailabilityType == ProductAvailabilityType.Unavailable)
                    {
                        newQuantity = 0;
                    }
                }
                if (mySKU.CatalogItem == null)
                {
                    newQuantity = 0;
                }
            }
            else
            {
                newQuantity = 0;
            }
            if (newQuantity == 0)
            {
                return
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "InvalidSKU").ToString(), sku);
            }
            return error;
        }

        /// <summary>
        ///     Giving an invoice number , create a shopping cart
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="locale"></param>
        /// <param name="invoiceID"></param>
        /// <returns></returns>
        public static MyHLShoppingCart GetShoppingCartFromInvoice(string distributorID, string locale, long invoiceID)
        {
            if (string.IsNullOrEmpty(distributorID) || string.IsNullOrEmpty(locale) || invoiceID <= 0)
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new CopyInvoiceRequest_V01();
                        request.DistributorID = distributorID;
                        request.InvoiceNumber = invoiceID;
                        request.Locale = locale;

                        var specialSKUsList = new List<string>(new[]
                        {
                            HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                            HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                            HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                        });

                        specialSKUsList.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
                        request.SpecialSKUs = specialSKUsList;

                        var response = proxy.CopyInvoice(new CopyInvoiceRequest1(request)).CopyInvoiceResult as CopyInvoiceResponse_V01;

                        // Check response status.
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            var shoppingCart = response.ShoppingCart as ShoppingCart_V03;
                            if (shoppingCart == null)
                            {
                                throw new Exception("copy invoice, shopping cart is null.");
                            }

                            var newCart = new MyHLShoppingCart(shoppingCart);
                            //getCarts(response.ShoppingCartList, distributorID, locale);

                            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);

                            // Saving current chopping cart id, if has items and has not been saved.
                            if (sessionInfo.ShoppingCart != null
                                && sessionInfo.ShoppingCart.CartItems != null
                                && sessionInfo.ShoppingCart.CartItems.Count > 0
                                && string.IsNullOrEmpty(sessionInfo.ShoppingCart.CartName)
                                && !sessionInfo.ShoppingCart.IsSavedCart)
                            {
                                newCart.CartName = sessionInfo.ShoppingCart.ShoppingCartID.ToString();
                            }

                            // Saving in session new cart.
                            sessionInfo.ShoppingCart = newCart;

                            newCart.CopyInvoiceStatus = response.CopyStatus;
                            newCart.CopyInvoiceAddress = ObjectMappingHelper.Instance.GetToShipping(response.Address);
                            newCart.CopyInvoiceName = response.Name;
                            newCart.CopyInvoicePhone = response.Phone;
                            newCart.IsFromInvoice = true;

                            // if address is not in cache, add it
                            if (newCart.CopyInvoiceStatus == CopyInvoiceStatus.success)
                            {
                                var shippingAddresses =
                                    ShippingProvider.GetShippingProvider(locale.Substring(3, 2))
                                                    .GetShippingAddresses(distributorID, locale);
                                if (shippingAddresses != null)
                                {
                                    if (newCart.CopyInvoiceAddress != null)
                                    {
                                        var newShippingAddress = new ShippingAddress_V02
                                            {
                                                ID = newCart.ShippingAddressID,
                                                Address = newCart.CopyInvoiceAddress,
                                                Recipient =
                                                    newCart.CopyInvoiceName != null
                                                        ? string.Format("{0} {1}", newCart.CopyInvoiceName.First,
                                                                        newCart.CopyInvoiceName.Last)
                                                        : null,
                                                FirstName =
                                                    newCart.CopyInvoiceName != null
                                                        ? newCart.CopyInvoiceName.First
                                                        : null,
                                                MiddleName =
                                                    newCart.CopyInvoiceName != null
                                                        ? newCart.CopyInvoiceName.Middle
                                                        : null,
                                                LastName =
                                                    newCart.CopyInvoiceName != null
                                                        ? newCart.CopyInvoiceName.Last
                                                        : null,
                                                Phone = response.Phone
                                            };
                                        ShippingProvider.GetShippingProvider(locale.Substring(3, 2))
                                                        .SaveShippingAddress(distributorID, locale, newShippingAddress,
                                                                             true, true, false);
                                        var address =
                                            shippingAddresses.Find(s => s.Id == newCart.ShippingAddressID);
                                        if (address == null) // not in the list
                                        {
                                            shippingAddresses.Add(new DeliveryOption(newShippingAddress));
                                        }
                                    }
                                }
                            }

                            newCart.RuleResults = new List<ShoppingCartRuleResult>();
                            newCart.GetShoppingCartForDisplay(true, false, true);
                            try
                            {
                                if (newCart.CopyInvoiceStatus == CopyInvoiceStatus.success)
                                {
                                    newCart.LoadShippingInfo(newCart.DeliveryOptionID, newCart.ShippingAddressID,
                                                             newCart.DeliveryOption, newCart.OrderCategory, string.Empty,
                                                             false);
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info for invoice, DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        newCart.DistributorID, newCart.ShoppingCartID, newCart.DeliveryOptionID,
                                        newCart.ShippingAddressID, newCart.DeliveryOption.ToString(), ex.Message));
                            }

                            newCart.Calculate();
                            //Check for DR fraud..
                            if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            {
                                if (newCart.DeliveryInfo != null &&
                                    null != newCart.DeliveryInfo.Address &&
                                    null != newCart.DeliveryInfo.Address.Address)
                                {
                                    DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                                                                                                         newCart
                                                                                                             .CountryCode,
                                                                                                         newCart
                                                                                                             .DeliveryInfo
                                                                                                             .Address
                                                                                                             .Address
                                                                                                             .PostalCode);
                                    newCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                                    newCart.PassDSFraudValidation = string.IsNullOrEmpty(newCart.DSFraudValidationError);
                                }
                            }
                            return newCart;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetShoppingCartFromInvoice fail {0} {1} {2} ", distributorID, locale,
                                          ex.Message));
                    }
                }
            }
            return null;
        }


        public static MyHLShoppingCart GetShoppingCartFromMemberInvoice(string distributorID, string locale, int invoiceID)
        {
            if (string.IsNullOrEmpty(distributorID) || string.IsNullOrEmpty(locale) || invoiceID <= 0)
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var request = new CopyMemberInvoiceRequest_V01();
                        request.DistributorID = distributorID;
                        request.InvoiceId = invoiceID;
                        request.Locale = locale;

                        var specialSKUsList = new List<string>(new[]
                        {
                            HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                            HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                            HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                            HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                        });

                        specialSKUsList.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
                        request.SpecialSKUs = specialSKUsList;

                        var response = proxy.CopyMemberInvoice(new CopyMemberInvoiceRequest1(request)).CopyMemberInvoiceResult as CopyMemberInvoiceResponse_V01;

                        // Check response status.
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            var shoppingCart = response.ShoppingCart as ShoppingCart_V03;
                            if (shoppingCart == null)
                            {
                                throw new Exception("copy invoice, shopping cart is null.");
                            }

                            var newCart = new MyHLShoppingCart(shoppingCart);
                            //getCarts(response.ShoppingCartList, distributorID, locale);

                            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);

                            // Saving current chopping cart id, if has items and has not been saved.
                            if (sessionInfo.ShoppingCart != null
                                && sessionInfo.ShoppingCart.CartItems != null
                                && sessionInfo.ShoppingCart.CartItems.Count > 0
                                && string.IsNullOrEmpty(sessionInfo.ShoppingCart.CartName)
                                && !sessionInfo.ShoppingCart.IsSavedCart)
                            {
                                newCart.CartName = sessionInfo.ShoppingCart.ShoppingCartID.ToString();
                            }

                            // Saving in session new cart.
                            sessionInfo.ShoppingCart = newCart;

                            newCart.CopyMemberInvoiceStatus = response.CopyStatus;
                            CopyInvoiceStatus copyInvoiceStatus;
                            CopyInvoiceStatus.TryParse(response.CopyStatus.ToString(),true,out copyInvoiceStatus);
                            newCart.CopyInvoiceStatus = copyInvoiceStatus;
                            newCart.CopyInvoiceAddress = ObjectMappingHelper.Instance.GetToShipping(response.Address);
                            newCart.CopyInvoiceName = response.Name;
                            newCart.CopyInvoicePhone = response.Phone;
                            newCart.IsFromInvoice = true;

                            // if address is not in cache, add it
                            if (newCart.CopyInvoiceStatus == CopyInvoiceStatus.success)
                            {
                                var shippingAddresses =
                                    ShippingProvider.GetShippingProvider(locale.Substring(3, 2))
                                                    .GetShippingAddresses(distributorID, locale);
                                if (shippingAddresses != null)
                                {
                                    if (newCart.CopyInvoiceAddress != null)
                                    {
                                        var newShippingAddress = new ShippingAddress_V02
                                        {
                                            ID = newCart.ShippingAddressID,
                                            Address = newCart.CopyInvoiceAddress,
                                            Recipient =
                                                newCart.CopyInvoiceName != null
                                                    ? string.Format("{0} {1}", newCart.CopyInvoiceName.First,
                                                                    newCart.CopyInvoiceName.Last)
                                                    : null,
                                            FirstName =
                                                newCart.CopyInvoiceName != null
                                                    ? newCart.CopyInvoiceName.First
                                                    : null,
                                            MiddleName =
                                                newCart.CopyInvoiceName != null
                                                    ? newCart.CopyInvoiceName.Middle
                                                    : null,
                                            LastName =
                                                newCart.CopyInvoiceName != null
                                                    ? newCart.CopyInvoiceName.Last
                                                    : null,
                                            Phone = response.Phone
                                        };
                                        ShippingProvider.GetShippingProvider(locale.Substring(3, 2))
                                                        .SaveShippingAddress(distributorID, locale, newShippingAddress,
                                                                             true, true, false);
                                        var address =
                                            shippingAddresses.Find(s => s.Id == newCart.ShippingAddressID);
                                        if (address == null) // not in the list
                                        {
                                            shippingAddresses.Add(new DeliveryOption(newShippingAddress));
                                        }
                                    }
                                }
                            }

                            newCart.RuleResults = new List<ShoppingCartRuleResult>();
                            newCart.GetShoppingCartForDisplay(true, false, true);
                            try
                            {
                                if (newCart.CopyInvoiceStatus == CopyInvoiceStatus.success)
                                {
                                    newCart.LoadShippingInfo(newCart.DeliveryOptionID, newCart.ShippingAddressID,
                                                             newCart.DeliveryOption, newCart.OrderCategory, string.Empty,
                                                             false);
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "Couldn't load Shipping Info for invoice, DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                        newCart.DistributorID, newCart.ShoppingCartID, newCart.DeliveryOptionID,
                                        newCart.ShippingAddressID, newCart.DeliveryOption.ToString(), ex.Message));
                            }

                            newCart.Calculate();
                            //Check for DR fraud..
                            if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            {
                                if (newCart.DeliveryInfo != null &&
                                    null != newCart.DeliveryInfo.Address &&
                                    null != newCart.DeliveryInfo.Address.Address)
                                {
                                    DRFraudStatusType fraudStatusType = DistributorOrderingProfileProvider.CheckForDRFraud(distributorID,
                                                                                                         newCart
                                                                                                             .CountryCode,
                                                                                                         newCart
                                                                                                             .DeliveryInfo
                                                                                                             .Address
                                                                                                             .Address
                                                                                                             .PostalCode);
                                    newCart.DSFraudValidationError = GetDSFraudResxKey(fraudStatusType);
                                    newCart.PassDSFraudValidation = string.IsNullOrEmpty(newCart.DSFraudValidationError);
                                }
                            }
                            return newCart;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetShoppingCartFromInvoice fail {0} {1} {2} ", distributorID, locale,
                                          ex.Message));
                    }
                }
            }
            return null;
        }

        #endregion Public Methods

        #region Private Methods

        private static string LogObjectTo(object obj, string nameObj)
        {
            var log = new StringBuilder();
            if (obj != null)
            {
                log.AppendLine(nameObj);
                foreach (PropertyInfo propiedad in obj.GetType().GetProperties())
                {
                    if (propiedad.PropertyType.Namespace == "System")
                    {
                        var valorObject = propiedad.GetValue(obj, null);
                        string valor = valorObject == null ? "null" : valorObject.ToString();
                        log.AppendLine(propiedad.Name + " = " + valor);
                    }
                }
            }
            return log.ToString();
        }

        private static string OrderSerialization(QuoteRequest_V01 orderRequest)
        {
            var sb = new StringBuilder();
            var order = orderRequest.Order as Order_V01;
            if (order != null)
            {
                sb.AppendFormat("Distributor : {0}, HMS Calc : {1} ", order.DistributorID,
                                orderRequest.GetHmsAuthoritativeTotals);
                sb.AppendLine();
                sb.Append(LogObjectTo(order, "Order Details************"));
                sb.Append(LogObjectTo(order.Pricing, "Pricing"));
                sb.Append(LogObjectTo(order.Shipment, "Shipment"));
                sb.Append(LogObjectTo((order.Shipment as ShippingInfo_V01).Address, "Shipment Adress"));
                if (order.Payments != null)
                {
                    foreach (Payment payment in order.Payments)
                    {
                        sb.Append(LogObjectTo(payment, "Payment"));
                        sb.Append(LogObjectTo(payment.Address, "Payment Address"));
                    }
                }
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        sb.Append(LogObjectTo(item, "Order Item"));
                    }
                }
                if (order.PurchasingLimits != null)
                {
                    sb.Append(LogObjectTo(order.PurchasingLimits as PurchasingLimits_V01, "Purchasing Limits"));
                }

                sb.Append(LogObjectTo(order.Handling, "Order Handling"));
                sb.Append(LogObjectTo(order.Handling, "Order Handling IncludeInvoice"));
                if (order.Messages != null)
                {
                    foreach (Message mensaje in order.Messages)
                    {
                        sb.Append(LogObjectTo(mensaje, "Order Message"));
                    }
                }
                sb.AppendLine("End of order.");
            }

            return sb.ToString();
        }

        private static string OrderTotalSerialization(OrderTotals_V01 orderTotal, string distributorID, bool HMSCalc)
        {
            var sb = new StringBuilder();
            if (orderTotal != null)
            {
                sb.AppendFormat("Distributor : {0}, HMS calc: {1} ", distributorID, HMSCalc);
                sb.AppendLine();
                sb.Append(LogObjectTo(orderTotal, "Order orderTotals************"));
                if (orderTotal.ItemTotalsList != null)
                {
                    foreach (ItemTotal_V01 itemTotal in orderTotal.ItemTotalsList)
                    {
                        sb.Append(LogObjectTo(itemTotal, "Item Total"));
                    }
                }
                if (orderTotal.ChargeList != null)
                {
                    foreach (Charge_V01 charge in orderTotal.ChargeList)
                    {
                        sb.Append(LogObjectTo(charge, "Charge"));
                    }
                }
                sb.AppendLine("End of Order orderTotals.");
            }
            return sb.ToString();
        }

        private static List<ShoppingCart_V03> getCarts(List<ShoppingCart_V03> cartList,
                                                       string distributorID,
                                                       string locale)
        {
            var drafts = cartList.Where(l => l.IsDraft).ToList();

            if (drafts != null)
            {
                var carts = (from c in drafts
                             select new MyHLShoppingCart(c)).ToList();
                HttpRuntime.Cache.Insert(GetCartsCacheKey(distributorID, locale), carts, null,
                                         DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal, null);
            }

            return cartList;
        }

        public static OrderTotals_V01 GetQuote(Order _order, QuotePartType quoteType, bool HMSCalc, out string errorCode)
        {
            var caledOrder = _order as Order_V01;
            var sessionInfo = SessionInfo.GetSessionInfo(caledOrder.DistributorID, CultureInfo.CurrentCulture.Name);
            if (sessionInfo.DsType == null)
            {
                string country = CultureInfo.CurrentCulture.Name.Substring(3).ToUpper();
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(caledOrder.DistributorID, country);
                sessionInfo.DsType = DistributorType;

            }
            if (sessionInfo.DsType == MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.Scheme.Member)
            {
                caledOrder.OrderIntention = OrderIntention.PersonalConsumption;
            }
            else
            {
                if (sessionInfo != null && sessionInfo.HAPOrderType != null)
                {
                    if (sessionInfo.HAPOrderType == "Personal")
                    {
                        caledOrder.OrderIntention = OrderIntention.PersonalConsumption;
                    }
                    else if (sessionInfo.HAPOrderType == "RetailOrder")
                    {
                        caledOrder.OrderIntention = OrderIntention.RetailOrder;
                    }
                }
            }

            var LocalesOrderIntentionValidation = Settings.GetRequiredAppSetting("LocalesOrderIntentionValidation","").Split(',').ToList();
            string countryLocale = CultureInfo.CurrentCulture.Name.Substring(3).ToUpper();
            var DSType = DistributorOrderingProfileProvider.CheckDsLevelType(caledOrder.DistributorID, countryLocale);
            if (LocalesOrderIntentionValidation.Any(x => x == CultureInfo.CurrentCulture.Name) && DSType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                if (caledOrder.Messages == null)
                {
                    caledOrder.Messages = new MessageCollection();

                }
                MessageCollection messages = caledOrder.Messages;
                var message = new Message()
                {
                    MessageType = "GDOIntention",
                    MessageValue = "PC"
                };
                messages.Add(message);
                caledOrder.Messages = messages;

            }

            var request = new QuoteRequest_V01() { Order = caledOrder };
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            errorCode = null;            
            DistributorOrderingProfile orderingProfile = null;
            if (null != member && null != member.Value && !string.IsNullOrEmpty(member.Value.Id))
            {
                orderingProfile = GetDistributorOrderingProfile(member.Value.Id, caledOrder.CountryOfProcessing);
            }
            if(!string.IsNullOrEmpty(Settings.GetRequiredAppSetting("CountriesOverridingHMSCalc", string.Empty)) &&
                Settings.GetRequiredAppSetting("CountriesOverridingHMSCalc",string.Empty).ToUpper().Contains(caledOrder.CountryOfProcessing) && HMSCalc)
             {
                 HMSCalc = false;
                 if (caledOrder.DiscountPercentage == 0 && null != orderingProfile && orderingProfile.StaticDiscount > 0)
                 {
                     caledOrder.DiscountPercentage = orderingProfile.StaticDiscount;
                 }
             }
            if(sessionInfo != null) 
            sessionInfo.HmsPricing = HMSCalc;
            if (IsTaxAreaIdCountry(caledOrder.CountryOfProcessing) && sessionInfo != null)
            {
                var postalCode = GetPostalCode(caledOrder);
                var taxAreaId = sessionInfo.GetTaxAreaId(postalCode);
                request.ShipToTaxAreaId = taxAreaId;
            }
            request.GetHmsAuthoritativeTotals = HMSCalc;
            
            request.LevelGroup = member != null && member.Value != null ? member.Value.TypeCode : string.Empty;
            caledOrder.DistributorID = caledOrder.DistributorID.ToUpper();
            request.MonthlyVolumes = null != orderingProfile ? ConvertToMonthlyVolumes(orderingProfile.DistributorVolumes): null;
            QuoteResponse result = null;
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                result = proxy.Quote(new QuoteRequest1(request)).QuoteResult as QuoteResponse;
                if (result.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success && result.Totals != null)
                {
                    LoggerHelper.Write(OrderTotalSerialization(result.Totals as OrderTotals_V01, _order.DistributorID, HMSCalc), "Ordertotal");

                    if (IsTaxAreaIdCountry(caledOrder.CountryOfProcessing) && !string.IsNullOrEmpty(result.ShipToTaxAreaId) && sessionInfo != null)
                    {
                        var postalCode = GetPostalCode(caledOrder);
                        sessionInfo.SetTaxAreadId(postalCode, result.ShipToTaxAreaId);
                    }

                    if (sessionInfo != null)
                    {
                        if (HMSCalc && null != sessionInfo.ShoppingCart)
                        {
                            sessionInfo.ShoppingCart.Totals = result.Totals;
                        }

                        if (!string.IsNullOrEmpty(result.Message))
                        {
                            //Validate if ZipCode got truncated when Pricing and replace value in Session Info
                            if (result.Message.Contains(" -Zip") && sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.DeliveryInfo != null &&
                               sessionInfo.ShoppingCart.DeliveryInfo.Address != null && sessionInfo.ShoppingCart.DeliveryInfo.Address.Address != null &&
                               !string.IsNullOrEmpty(sessionInfo.ShoppingCart.DeliveryInfo.Address.Address.PostalCode) &&
                               sessionInfo.ShoppingCart.DeliveryInfo.Address.Address.PostalCode.Contains('-'))
                            {
                                sessionInfo.ShoppingCart.DeliveryInfo.Address.Address.PostalCode = sessionInfo.ShoppingCart.DeliveryInfo.Address.Address.PostalCode.Substring(0, sessionInfo.ShoppingCart.DeliveryInfo.Address.Address.PostalCode.IndexOf('-'));
                            }
                        }
                    }

                    return result.Totals as OrderTotals_V01;
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Order Total error, distributor:{0} receive date:{1}, use HMS:{3},  error message:{2}",
                            caledOrder.DistributorID, caledOrder.ReceivedDate.ToString(), caledOrder.Messages, HMSCalc));
                    if (result.ParameterErrorList.Count > 0)
                    {
                        foreach(var error in result.ParameterErrorList)
                        {
                            if (error.Value.Contains("ORA-01403"))
                            {
                                errorCode = "ORA-01403";
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Order Total error, distributor:{0} receive date:{1}, use HMS:{3},  error message:{2}",
                        caledOrder.DistributorID, caledOrder.ReceivedDate.ToString(), ex, HMSCalc));
            }
            return null;
        }

        private static string GetPostalCode(Order_V01 order)
        {
            if (null != order.Shipment)
            {
                var shippingInfo = order.Shipment as ShippingInfo_V01;
                if (null != shippingInfo && null != shippingInfo.Address)
                {
                    return shippingInfo.Address.PostalCode;
                }
            }
            return string.Empty;
        }

        private static bool IsTaxAreaIdCountry(string countryCode)
        {
            if (!string.IsNullOrEmpty(Settings.GetRequiredAppSetting("TaxAreaIdCountries", string.Empty)) &&
                Settings.GetRequiredAppSetting("TaxAreaIdCountries", string.Empty)
                    .ToUpper()
                    .Contains(countryCode.ToUpper()))
            {
                return true;
            }
            return false;
        }

        private static DistributorOrderingProfile GetDistributorOrderingProfile(string id, string countryOfProcessing)
        {
            var distributorOrderingProfileFactory = new DistributorOrderingProfileFactory();
            var orderingProfile = distributorOrderingProfileFactory.GetDistributorOrderingProfile(id, countryOfProcessing);
            if (null == orderingProfile)
            {
                return null;
            }
            return orderingProfile;
        }
        private static List<MonthlyVolume> GetMonthlyVolumes(string id, string countryOfProcessing)
        {
            var distributorOrderingProfileFactory = new DistributorOrderingProfileFactory();
            var orderingProfile = distributorOrderingProfileFactory.GetDistributorOrderingProfile(id, countryOfProcessing);
            if (null == orderingProfile)
            {
                return null;
            }
            return ConvertToMonthlyVolumes(orderingProfile.DistributorVolumes);
        }

        private static List<MonthlyVolume> ConvertToMonthlyVolumes(IEnumerable<ServiceProvider.DistributorSvc.DistributorVolume_V01> distributorVolumes)
        {
            if (null == distributorVolumes)
            {
                return null;
            }

            return distributorVolumes.Select(item => new MonthlyVolume
            {
                DownlineVolume = item.DownlineVolume,
                GroupVolume = item.GroupVolume,
                PersonallyPurchasedVolume = item.PersonallyPurchasedVolume,
                RoyaltyOrganizationalVolumeCurrent = item.RoyaltyOrganizationalVolumeCurrent,
                RoyaltyPointsCurrent = item.RoyaltyPointsCurrent,
                TotalVolume = item.TotalVolume,
                Volume = item.Volume,
                VolumeDate = item.VolumeDate,
                VolumeMonth = item.VolumeMonth
            }).ToList();
        }
    
        public static string GetShoppingCartCacheKey(string distributor, string locale)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributor, locale);
            return string.Format("{0}|{1}|{2}|{3}", ShoppingCartCachePrefix, locale, distributor,
                                 sessionInfo == null ? "RSO" : sessionInfo.IsEventTicketMode ? "ETO" : sessionInfo.IsHAPMode ? "HAP" : "RSO");
        }

        public static string GetCartsCacheKey(string distributor, string locale)
        {
            return string.Format("{0}{1}_{2}", CartsCachePrefix, distributor, locale);
        }

        public static string GetInternetCartsCacheKey(string distributor, string locale)
        {
            return string.Format("{0}{1}_{2}", InternetCartsCachePrefix, distributor, locale);
        }

        public static int DecrementInventory(SKU_V01 sku, int quantity, string warehouse, string country)
        {
            int quantityDecremented = 0;
            var item = sku.CatalogItem;
            var iList = item == null ? null : item.InventoryList;
            if (iList != null)
            {
                WarehouseInventory inventory = null;
                WarehouseInventory_V01 inventory_v01 = null;
                if (iList.TryGetValue(warehouse, out inventory))
                {
                    try
                    {
                        inventory_v01 = inventory as WarehouseInventory_V01;
                        int availableQuantity = 0;
                        if (null != inventory)
                        {
                            var locker = new ReaderWriterLockSlim();
                            locker.EnterWriteLock();
                            try
                            {
                                availableQuantity = inventory_v01.QuantityAvailable;
                                inventory_v01.QuantityAvailable -= quantity;
                                quantityDecremented = availableQuantity - inventory_v01.QuantityAvailable;
                                var catItem = CatalogProvider.GetCatalogItem(sku.SKU, country);
                                if (catItem != null)
                                {
                                    if (catItem.InventoryList.TryGetValue(warehouse, out inventory))
                                    {
                                        (inventory as WarehouseInventory_V01).QuantityAvailable =
                                            inventory_v01.QuantityAvailable;
                                    }
                                }
                            }
                            finally
                            {
                                locker.ExitWriteLock();
                            }
                        }
                    }
                    catch (Exception lockException)
                    {
                        LoggerHelper.Error(lockException.Message);
                    }
                }
            }

            return quantityDecremented;
        }

        public static void IncrementInventory(SKU_V01 sku, int quantity, string warehouse, string country)
        {
            var item = sku.CatalogItem;
            var iList = item == null ? null : item.InventoryList;
            if (iList != null)
            {
                WarehouseInventory inventory = null;
                WarehouseInventory_V01 inventory_v01 = null;
                if (iList.TryGetValue(warehouse, out inventory))
                {
                    try
                    {
                        inventory_v01 = inventory as WarehouseInventory_V01;
                        if (null != inventory)
                        {
                            var locker = new ReaderWriterLockSlim();
                            locker.EnterWriteLock();
                            try
                            {
                                inventory_v01.QuantityAvailable += quantity;
                            }
                            finally
                            {
                                locker.ExitWriteLock();
                            }
                        }
                    }
                    catch (Exception lockException)
                    {
                        LoggerHelper.Error(lockException.Message);
                    }
                }
            }
        }

        public static void UpdateInventory(MyHLShoppingCart shoppingCart, string country, string locale, bool decrease)
        {
            if (shoppingCart != null)
            {
                List<ShoppingCartItem_V01> items = shoppingCart.CartItems;

                if (shoppingCart.DeliveryInfo != null && !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode))
                {
                    string warehouse = shoppingCart.DeliveryInfo.WarehouseCode;

                    var allSKUs = CatalogProvider.GetAllSKU(locale, warehouse);
                    if (allSKUs != null)
                    {
                        foreach (ShoppingCartItem_V01 item in items)
                        {
                            SKU_V01 sku;
                            if (allSKUs.TryGetValue(item.SKU, out sku))
                            {
                                if (decrease)
                                    DecrementInventory(sku, item.Quantity, warehouse, country);
                                else
                                    IncrementInventory(sku, item.Quantity, warehouse, country);
                            }
                        }

                        foreach (ShoppingCartItem_V01 item in items)
                        {
                            SKU_V01 sku;
                            if (allSKUs.TryGetValue(item.SKU, out sku))
                            {
                                HLRulesManager.Manager.ProcessCatalogItemsForInventory(locale, shoppingCart,
                                                                                       new List<SKU_V01> { sku });
                                CatalogProvider.GetProductAvailability(sku, warehouse);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsStandaloneHFF(List<ShoppingCartItem_V01> cartItems)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AllowHFFModal || cartItems == null ||
                cartItems.Count == 0)
            {
                return false;
            }

            var hffSkus = new List<string> { HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku };
            hffSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            var sku = from h in hffSkus
                      from c in cartItems
                      where h == c.SKU.Trim()
                      select h;
            return (sku.Count() == cartItems.Count);
        }

        public static bool IsStandaloneHFF(List<DistributorShoppingCartItem> cartItems)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AllowHFFModal || cartItems == null ||
                cartItems.Count == 0)
            {
                return false;
            }

            var hffSkus = new List<string> { HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku };
            hffSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

            var sku = from h in hffSkus
                      from c in cartItems
                      where h == c.SKU.Trim()
                      select h;
            return (sku.Count() == cartItems.Count);
        }

        #endregion Private Methods

        #region Promo from service methods
        /// <summary>
        /// Gets promo information for an eligible distributor.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        public static EligibleDistributorInfo_V01 GetEligibleForPromo(string distributorId, string locale)
        {
            if (string.IsNullOrEmpty(distributorId) || string.IsNullOrEmpty(locale))
            {
                return null;
            }

            //string cacheKey = GetPromoCacheKey(locale);
            //List<EligibleDistributorInfo_V01> promos = HttpRuntime.Cache[cacheKey] as List<EligibleDistributorInfo_V01>;

            //if (promos == null || !promos.Any(p => p.DistributorId == distributorId && p.Locale == locale))
            //{
            //    var promosForDS = GetEligibleForPromoFromService(distributorId, locale);
            //    if (promos == null) promos = new List<EligibleDistributorInfo_V01>();
            //    if (promosForDS != null && promosForDS.Count > 0)
            //    {
            //        promos.AddRange(promosForDS);
            //    }
            //    HttpRuntime.Cache[cacheKey] = promos;
            //}

            var promos = GetEligibleForPromoFromService(distributorId, locale);
            if (promos != null && promos.Any(p => p.DistributorId.ToUpper().Trim() == distributorId.ToUpper().Trim() && p.Locale == locale && !p.IsDisable))
            {
                return promos.FirstOrDefault();
            }
            else
            {
            return null;
        }
        }

        /// <summary>
        /// Saves (closes) the promo for the distributor.
        /// </summary>
        /// <param name="promoInfo">Promotional info.</param>
        /// <param name="shoppingCartId">The shopping cart id.</param>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="updShoppingCartId">The shopping cart id which closes the promo.</param>
        /// <param name="updOrderNumber">The stored order number for the closed promo.</param>
        /// <returns></returns>
        public static EligibleDistributorInfo_V01 SaveEligibleForPromo(EligibleDistributorInfo_V01 promoInfo, int shoppingCartId, string orderNumber,
            ref int updShoppingCartId, ref string updOrderNumber)
        {
            updShoppingCartId = 0;
            updOrderNumber = string.Empty;
            if (promoInfo == null || string.IsNullOrEmpty(promoInfo.DistributorId) || string.IsNullOrEmpty(promoInfo.Locale) ||
                (shoppingCartId <= 0 && string.IsNullOrEmpty(orderNumber)))
            {
                return null;
            }

            //string cacheKey = GetPromoCacheKey(promoInfo.Locale);
            //List<EligibleDistributorInfo_V01> promos = HttpRuntime.Cache[cacheKey] as List<EligibleDistributorInfo_V01>;

            //if (promos == null || !promos.Any(p => p.DistributorId == promoInfo.DistributorId && p.Locale == promoInfo.Locale))
            //{
            //    var promosForDS = GetEligibleForPromoFromService(promoInfo.DistributorId, promoInfo.Locale);
            //    if (promos == null) promos = new List<EligibleDistributorInfo_V01>();
            //    if (promosForDS != null && promosForDS.Count > 0)
            //    {
            //        promos.AddRange(promosForDS);
            //    }
            //    HttpRuntime.Cache[cacheKey] = promos;
            //}

            var promos = GetEligibleForPromoFromService(promoInfo.DistributorId, promoInfo.Locale);
            if (promos != null)
            {
                var tempPromo = promos.FirstOrDefault(p => p.DistributorId == promoInfo.DistributorId &&
                        p.Locale == promoInfo.Locale && p.Sku == promoInfo.Sku);
                if (tempPromo != null)
                {
                    if (!tempPromo.IsDisable)
                    {
                        var updatedPromoInfo = SaveEligibleForPromoToFromService(promoInfo, shoppingCartId, orderNumber, ref updShoppingCartId, ref updOrderNumber);
                        tempPromo.IsDisable = updatedPromoInfo.IsDisable;
                        return updatedPromoInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the key used when storing the promo information in cache.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        private static string GetPromoCacheKey(string locale)
        {
            return string.Format("{0}{1}", PromoCachePrefix, locale);
        }

        /// <summary>
        /// Gets the promo info for an eligible distributor from service.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        private static List<EligibleDistributorInfo_V01> GetEligibleForPromoFromService(string distributorId, string locale)
        {
            if (string.IsNullOrEmpty(distributorId) || string.IsNullOrEmpty(locale))
            {
                return null;
            }

            List<EligibleDistributorInfo_V01> promoInfo = null;
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new GetEligibleForPromoRequest_V01 { DistributorId = distributorId, Locale = locale };

                    var response = proxy.GetEligibleForPromo(new GetEligibleForPromoRequest1(request)).GetEligibleForPromoResult as GetEligibleForPromoResponse_V01;
                    if (response != null && response.PromoInfo != null && response.PromoInfo.Count > 0)
                    {
                        promoInfo = response.PromoInfo;
                    }
                }
                catch (Exception ex)
                {
                    promoInfo = null;
                   // WebUtilities.LogServiceExceptionWithContext<CatalogSVC.ICatalogInterface>(ex, proxy);
                    LoggerHelper.Error(string.Format("GetEligibleForPromoFromService DistributorId:{0} Locale:{1} ERR:{2}", distributorId, locale, ex.ToString()));
                }
            }
            return promoInfo;
        }

        /// <summary>
        /// Closes the promo for the distributor from service.
        /// </summary>
        /// <param name="promoInfo">The promotional info.</param>
        /// <param name="shoppingCartId">The shopping cart id.</param>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="updShoppingCartId">The shopping cart id which closes the promo.</param>
        /// <param name="updOrderNumber">The stored order number.</param>
        /// <returns></returns>
        public static EligibleDistributorInfo_V01 SaveEligibleForPromoToFromService(EligibleDistributorInfo_V01 promoInfo, int shoppingCartId, string orderNumber, ref int updShoppingCartId, ref string updOrderNumber)
        {
            if (promoInfo == null || string.IsNullOrEmpty(promoInfo.DistributorId) || string.IsNullOrEmpty(promoInfo.Locale) ||
                (shoppingCartId <= 0 && string.IsNullOrEmpty(orderNumber)))
            {
                return null;
            }

            EligibleDistributorInfo_V01 updatedPromo = null;
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new UpdateEligibleForPromoRequest_V01
                    {
                        PromoInfo = promoInfo,
                        ShoppingCartId = shoppingCartId,
                        OrderNumber = orderNumber,
                        Platform = HLConfigManager.Platform
                    };

                    var response = proxy.UpdateEligibleForPromo(new UpdateEligibleForPromoRequest1(request)).UpdateEligibleForPromoResult as UpdateEligibleForPromoResponse_V01;
                    if (response != null && response.PromoInfo != null)
                    {
                        updatedPromo = response.PromoInfo;
                        updShoppingCartId = response.ShoppingCartId;
                        updOrderNumber = response.OrderNumber;
                    }
                }
                catch (Exception ex)
                {
                    updatedPromo = null;
                    WebUtilities.LogServiceExceptionWithContext<ServiceProvider.CatalogSvc.ICatalogInterface>(ex, proxy);
                    LoggerHelper.Error(string.Format("SaveEligibleForPromoToFromService DistributorId:{0} Locale:{1} ERR:{2}", promoInfo.DistributorId, promoInfo.Locale, ex.ToString()));
                }
            }
            return updatedPromo;
        }

        #endregion

        #region Enabling payment per country rule 

        public static bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart)
        {
            switch(shoppingCart.CountryCode)
            {
                case "US":
                    if (shoppingCart.OrderCategory == OrderCategoryType.HSO)
                    {
                        return paymentType == "CreditCard";
                    }
                    else if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup && shoppingCart.DeliveryInfo.WarehouseCode == "95")
                    {
                        return paymentType == "CreditCard";
                    }
                    break;
                case "CA":
                    if (shoppingCart.OrderCategory == OrderCategoryType.HSO)
                    {
                        return paymentType == "CreditCard";
                    }
                    break;
                case "MX":
                    if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress 
                        && Thread.CurrentThread.CurrentCulture.Name.Substring(3).Equals("MX")
                        && paymentType == "WireTransfer")
                    {
                        var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                        var isApfOnly = APFDueProvider.containsOnlyAPFSku(shoppingCart.ShoppingCartItems);
                        if (shoppingCart.DeliveryInfo != null
                            && null != user
                            && user.Value.ProcessingCountryCode != "MX"
                            && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasSpecialEventWareHouse
                            && (shoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse 
                                || shoppingCart.DeliveryInfo.WarehouseCode == "M0")
                            && !isApfOnly)
                        {
                            //For non-mexican distributors wire payments should be disabled
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }

        public static bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart, bool isHidden)
        {
            if(shoppingCart.CountryCode == "UY")
            {
                var _paymentConfig = HLConfigManager.Configurations.PaymentsConfiguration;
                if (paymentType.Equals("CreditCard") ||
                    (paymentType.Equals("PaymentGateway") && _paymentConfig.HasPaymentGateway && _paymentConfig.PaymentGatewayPaymentMethods.Equals("CreditCard")))
                {
                    var countryConfig = Configuration.ConfigurationManagement.Providers.HlCountryConfigurationProvider.GetCountryConfiguration(shoppingCart.Locale);
                    if (countryConfig != null && countryConfig.CreditCardConfiguration != null)
                    {
                        if (countryConfig.CreditCardConfiguration.DisableCreditCard)
                            return false;

                        if (countryConfig.CreditCardConfiguration.DisableCreditCardByAmount)
                        {
                            var tinCodes = countryConfig.CreditCardConfiguration.TinCodeList != null ? countryConfig.CreditCardConfiguration.TinCodeList : new List<string>();

                            // Validate, if has valid TIN Code dont hide the Credit Card option
                            var currentTinList = DistributorOrderingProfileProvider.GetTinList(shoppingCart.DistributorID, true);
                            foreach (string tinCode in tinCodes)
                            {
                                if (currentTinList.Exists(t => t.CountryCode == shoppingCart.CountryCode && t.IDType.Key == tinCode))
                                    return true;
                            }

                            // if is not a valid TIN Code validate amount BaseTax
                            var exciseTaxLimitList = OrderProvider.GetExciseTax(shoppingCart.CountryCode, DateTime.Now);
                            var exciseTaxLimit = exciseTaxLimitList.FirstOrDefault();
                            if (exciseTaxLimit != null)
                            {
                                return exciseTaxLimit.BaseTax <= (shoppingCart.Totals as OrderTotals_V01).AmountDue;
                            }
                        }
                    }
                }
            }
            else if (shoppingCart.CountryCode == "AT" && HLConfigManager.Configurations.PaymentsConfiguration.HasPaymentGateway)
            {
                DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID, shoppingCart.CountryCode);
                //user story 155153 force to show sofort for HardCash Bug 217741
                if (distributorOrderingProfile.HardCashOnly)
                {
                    if (paymentType.Equals("PaymentGateway"))
                    {
                        return true;
                    }
                }
            }
            else if (shoppingCart.CountryCode == "MY")
            {
                if (paymentType.Equals("PaymentGateway"))
                {
                    return HLConfigManager.Configurations.PaymentsConfiguration.HasPaymentGateway;
                }
            }
            return isHidden;
        }

        #endregion

        /// <summary>
        /// Defines if the order should be splitted.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <returns></returns>
        public static void CheckSplitOrder(MyHLShoppingCart shoppingCart)
        {
            var isSplitOrder = false;
            if (shoppingCart != null)
            {
                var sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (sessionInfo != null)
                {
                    if (!sessionInfo.IsEventTicketMode && !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit))
                    {
                        if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping &&
                            HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Contains(shoppingCart.DeliveryInfo.WarehouseCode))
                        {
                            foreach (var sku in shoppingCart.CartItems)
                            {
                                SKU_V01 sku01;
                                var allSKUs = CatalogProvider.GetAllSKU(shoppingCart.Locale, shoppingCart.DeliveryInfo.WarehouseCode);
                                if (allSKUs.TryGetValue(sku.SKU, out sku01))
                                {
                                    //isSplitOrder = false;
                                    CheckInventory(sku01.CatalogItem, sku.Quantity, shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart.DeliveryInfo.FreightCode, ref isSplitOrder);
                                    if (isSplitOrder)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    shoppingCart.IsSplit = isSplitOrder;
                }
            }
        }

        private static ShippingAddress_V01 populateShippingAddress(ServiceProvider.ShippingSvc.Address_V01 addressSource, List<DeliveryOption> dsAddresses)
        {
            if (dsAddresses!=null && dsAddresses.Count()>0)
            {
                var addressInDB = dsAddresses.Find(a => a.Address.City == addressSource.City && a.Address.Line1 == addressSource.Line1 && a.Address.StateProvinceTerritory == addressSource.StateProvinceTerritory && a.Address.Country == addressSource.Country);
                if (addressInDB!=null)
                {
                    return addressInDB;
                }
            }
            return null;
        }

        private static ShippingAddress_V01 createTempShippingAddress(ShippingInfo_V01 addressSource, string distributorID, string locale)
        {
            ShippingAddress_V02 shippingAddressToSave = new ShippingAddress_V02()
            {
                ID = -1,
                Recipient = addressSource.Recipient,
                FirstName = string.Empty,
                LastName = string.Empty,
                MiddleName = string.Empty,
                Address = ObjectMappingHelper.Instance.GetToShipping(addressSource.Address as ServiceProvider.OrderSvc.Address_V01),
                Phone = addressSource.Phone,
                AltPhone = string.Empty,
                IsPrimary = false,
                Alias = string.Empty,
                Created = DateTime.Now
            };
            new ShippingProviderBase().SaveShippingAddress(distributorID, locale, shippingAddressToSave, true, false, false);
            return shippingAddressToSave;
        }

        public static MyHLShoppingCart GetShoppingCartFromHAPOrder(string distributorID, string locale, string hapOrderId)
        {
            MyHLShoppingCart result = null;

            if (string.IsNullOrEmpty(hapOrderId) || string.IsNullOrEmpty(distributorID))
            {
                return result;
            }
            string countryCode = locale.Substring(3, 2);
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (sessionInfo != null)
            {
                MyHLShoppingCart theCart = null;

                var orderDetail = OrderProvider.GetOrderDetailsFromFusion(hapOrderId, locale);

                if (orderDetail != null)
                {
                    // Validate if HapOrder loaded is saved for this DS and Country
                    if ( string.IsNullOrEmpty(orderDetail.CountryOfProcessing) || 
                        string.IsNullOrEmpty(orderDetail.HapOrderStatus) || orderDetail.HapOrderStatus.ToUpper().Contains("CANCELLED"))
                    {
                        //return result; // Error loading HAP Order
                        //get detail error.
                        var orders = OrderProvider.GetHapOrders(distributorID, locale.Substring(3,2));
                        orderDetail = orders.Find(o => o.OrderID == hapOrderId);
                    }

                    try
                    {
                        theCart = GetNewUnsavedShoppingCart(distributorID, locale);

                        // Update theCart with HapOrderDetails data
                        theCart.OrderNumber = hapOrderId;
                        theCart.OrderCategory = OrderCategoryType.HSO;
                        theCart.OrderSubType = theCart.HAPType = orderDetail.HapOrderProgramType;
                        theCart.HAPScheduleDay = orderDetail.HapOrderSchedule == "A" ? 4 : orderDetail.HapOrderSchedule == "B" ? 11 : 18;

                        // Set DeliveryInfo
                        try
                        {
                            ShippingInfo_V01 shippingInfo = orderDetail.Shipment as ShippingInfo_V01;
                            var shp = ShippingProvider.GetShippingProvider(countryCode);
                            var opt = shp.GetDeliveryOptions(locale);
                            var DSAddresses = shp.GetShippingAddresses(theCart.DistributorID, theCart.Locale);
                            var di = opt.FirstOrDefault(d => ObjectMappingHelper.Instance.GetToOrder(d.Address) == shippingInfo.Address);
                            if (di != null)
                            {
                                theCart.DeliveryInfo = new ShippingInfo(di);
                            }
                            else
                            {
                                theCart.DeliveryInfo = new ShippingInfo();
                                theCart.DeliveryInfo.WarehouseCode = shippingInfo.WarehouseCode;
                                theCart.DeliveryInfo.FreightCode = shippingInfo.ShippingMethodID;
                                var address = ObjectMappingHelper.Instance.GetToShipping(shippingInfo.Address as ServiceProvider.OrderSvc.Address_V01);
                                theCart.DeliveryInfo.Address = populateShippingAddress(address, DSAddresses);
                                if (theCart.DeliveryInfo.Address == null)
                                {
                                    theCart.DeliveryInfo.Address = createTempShippingAddress(shippingInfo, theCart.DistributorID, theCart.Locale);
                                }
                                theCart.DeliveryInfo.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                                if (orderDetail.PurchaserInfo != null)
                                    theCart.EmailAddress = (orderDetail.PurchaserInfo as Purchaser_V01).PurchaserEmail ?? theCart.EmailAddress;
                            }
                        }
                        catch
                        {
                            theCart.DeliveryInfo = new ShippingInfo();
                            theCart.DeliveryInfo.WarehouseCode = "03";
                            theCart.DeliveryInfo.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                            theCart.DeliveryInfo.Address = new ShippingAddress_V01();
                            theCart.DeliveryInfo.Address.Address = new ServiceProvider.ShippingSvc.Address_V01();
                        }

                        // Add Items
                        List<ShoppingCartItem_V01> cartItems = new List<ShoppingCartItem_V01>();
                        var allSKU = CatalogProvider.GetAllSKU(locale);

                        if (allSKU != null)
                        {
                            foreach (OrderItem oItem in orderDetail.OrderItems)
                            {
                                SKU_V01 sku;
                                if (allSKU.TryGetValue(oItem.SKU, out sku))
                                {
                                    cartItems.Add(new ShoppingCartItem_V01(0, sku.SKU, oItem.Quantity, DateTime.Now));
                                }
                            }
                        }

                        theCart.CartItems = new ShoppingCartItemList();
                        theCart.ShoppingCartItems = new List<DistributorShoppingCartItem>();

                        theCart.AddItemsToCart(cartItems, true);

                        return theCart;

                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetShoppingCartFromHAPOrder distributorId:{0}, hapOrderId:{1} ERR:{2}", distributorID, hapOrderId, ex));
                        return null;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the total weight for the shopping cart items.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <returns></returns>
        public static string GetWeight(MyHLShoppingCart shoppingCart)
        {
            double weight = 0.0;
            if (shoppingCart != null && shoppingCart.CartItems != null && shoppingCart.CartItems.Any())
            {
                var allSKU = CatalogProvider.GetAllSKU(shoppingCart.Locale);
                if (allSKU != null)
                {
                    foreach (var item in shoppingCart.CartItems)
                    {
                        SKU_V01 skuV01;
                        if (allSKU.TryGetValue(item.SKU, out skuV01))
                        {
                            weight += (item.Quantity * skuV01.CatalogItem.Weight);
                        }
                    }
                }
            }
            return (weight > 0) ? weight.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
        }
    }
}
