using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using Order = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrder;

namespace MyHerbalife3.Ordering.Providers.China
{
    public static partial class OrderProvider
    {
        /// <summary>
        /// IAChinaOrderUrl
        /// </summary>
        public const string CacheKey_CNSMS_OrderStatus = "CNSMS.OrderStatus";
        private const string PCCustomerIdByReferralIdCacheKey = "PCCustomerIdByReferralId_KEY_CN";
        private const string PCCustomerIdByReferralIdCacheMinutes = "PCCustomerIdByReferralIdCacheMinutes";
        private const string GetCustomerSurveyCacheKey = "GetCustomerSurveyCacheKey_KEY_CN";
        private const int GetCustomerSurveyCacheMinutes = 20;
        /// <summary>
        /// 待配送
        /// </summary>
        public const string OrderStatus_Code_ToBeAssign = "01";

        const string ThisClassName = "MyHerbalife3.Ordering.Providers.China.OrderProvider";

        private static void isFreeFreight(OnlineOrderItem item)
        {
            item.IsFreeFreight = HLConfigManager.Configurations.CheckoutConfiguration.FreeFreightSKUList.Contains(item.SKU);
        }
        public static Order CreateOrderObject(List<DistributorShoppingCartItem> cartItems)
        {
            var order = new Order();

            var sortedItems = new List<OnlineOrderItem>();

            sortedItems.AddRange(from item in cartItems
                                 where null != item
                                 select
                                     new OnlineOrderItem
                                         {
                                             Quantity = item.Quantity,
                                             SKU = item.SKU.Trim(),
                                             Description = item.Description,
                                             RetailPrice = item.RetailPrice,
                                             IsPromo = item.IsPromo,
                                         });
            Array.ForEach(sortedItems.ToArray(), a => isFreeFreight(a));
            order.OrderItems = new OrderItems();
            order.OrderItems.AddRange(sortedItems.OrderBy(i => i.SKU).ToList());

            return order;
        }

        public static Order CreateOrderObject(string orderNumber, MyHLShoppingCart shoppingCart,
                                              PaymentCollection payments)
        {
            var order = CreateOrderObject(shoppingCart.ShoppingCartItems);
            order = FillOrderInfo(order, shoppingCart);
            order.Pricing = shoppingCart.Totals;
            order.Handling = new HandlingInfo_V01 {PickupName = "name"};
            order.OrderID = orderNumber;
            order.ShoppingCartID = shoppingCart.ShoppingCartID;
            if (string.IsNullOrEmpty(orderNumber))
                order.OrderID = "ToBePaid";
            if (payments == null)
            {
                payments = new PaymentCollection();
                payments.Add(new CreditPayment_V01 {PaymentDate = DateTime.Now, AuthorizationCode = "CC"});
            }
            if ((order.Pricing as OrderTotals_V02).OrderFreight == null)
            {
                (order.Pricing as OrderTotals_V02).OrderFreight = new OrderFreight();
            }
            return order;
        }

        public static Order CreateOrder(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments)
        {
            var order = CreateOrderObject(orderNumber, shoppingCart, payments);
            //shoppingCart.OrderHeaderID = order.OrderHeaderID = InsertOrder(order);
            return order;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem CreateOrderItem(DistributorShoppingCartItem item)
        {

            var oitem = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
            oitem.Description = item.Description;
            oitem.SKU = item.SKU;
            oitem.RetailPrice = item.RetailPrice;
            oitem.VolumePoint = item.VolumePoints;
            oitem.Quantity = item.Quantity;
            return oitem;
        }

        public static int UpdateOrder(string orderNumber, int shoppingCartID, OnlineOrder order)
        {
            order.OrderID = orderNumber;
            order.ShoppingCartID = shoppingCartID;
            //order.OrderHeaderID = InsertOrder(order);
            return order.OrderHeaderID;
        }

        public static int InsertOrder(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OnlineOrder order)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.CreateOrderRequest_V01
                        {
                            DistributorID = order.DistributorID,
                            NeedInvoice = false,
                            Order = order,
                            OrderNumber = order.OrderID
                        };
                    var response =
                        proxy.CreateOrder(new ServiceProvider.OrderChinaSvc.CreateOrderRequest1(request)).CreateOrderResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.CreateOrderResponse_V01;
                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response.OrderHeaderID;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "InsertOrder failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return 0;
        }

        public static Order FillOrderInfo(Order order, MyHLShoppingCart shoppingCart)
        {
            order.DistributorID = shoppingCart.DistributorID;
            order.Shipment = OrderCreationHelper.GetShippingInfoFromCart(shoppingCart);
            (order.Shipment as ShippingInfo_V01).FreightVariant = shoppingCart.DeliveryInfo.AddressType;
                // address type 
            if (shoppingCart.DeliveryInfo.Option == MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup ||
                shoppingCart.DeliveryInfo.Option == MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier)
            {
                (order.Shipment as ShippingInfo_V01).Phone = shoppingCart.DeliveryInfo.Address.AltPhone;
            }
            order.Messages = order.Messages == null ? new MessageCollection() : order.Messages;
            Message IDNumber = new Message
                {
                    MessageType = "IDNumber",
                    MessageValue = shoppingCart.DeliveryInfo != null ? shoppingCart.DeliveryInfo.RGNumber : string.Empty
                };
            order.Messages.Add(IDNumber);
            if (shoppingCart.DeliveryInfo.AddressType.Contains("PUC"))
                (order.Shipment as ShippingInfo_V01).WarehouseCode = shoppingCart.DeliveryInfo.Id.ToString();
            order.InputMethod = InputMethodType.Internet;
            var recvdDate = DateUtils.GetCurrentLocalTime(shoppingCart.Locale.Substring(3, 2));
            order.ReceivedDate = recvdDate;
            order.OrderCategory =
                (OrderCategoryType) Enum.Parse(typeof (OrderCategoryType), shoppingCart.OrderCategory.ToString());

            var orderMonth = new OrderMonth(shoppingCart.CountryCode);
            order.OrderMonth = orderMonth.PhoenixOrderMonth;
            order.CountryOfProcessing = shoppingCart.CountryCode;

            return order;
        }
        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OnlineOrder> GetOrders(string distributorId,
                                            int profileId,
                                            string distributorType,
                                            DateTime? startDate,
                                            DateTime? endDate,
                                            string sortOrder)
        {
            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrdersResponse_V01 response;
            if (profileId == 0)
            {
                //need to get the profileID
            }
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var sDate = (startDate == null) ? DateTime.Now.AddMonths(-1) : startDate.Value; var eDate = (endDate == null) ? DateTime.Now.AddDays(2) : endDate.Value;
                    response = proxy.GetOrders(new ServiceProvider.OrderChinaSvc.GetOrdersRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrdersRequest_V01
                    {
                        CustomerProfileID = profileId,
                        OrderFilter = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrdersByDateRange
                        {
                            DistributorId = distributorId,
                            DistributorType = distributorType,
                            StartDate = sDate,
                            EndDate = eDate
                        }

                    })).GetOrdersResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrdersResponse_V01;

                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response.Orders;
                    }
                    else
                    {
                        string debugLine = " DistributorId=" + distributorId + "; DistributorType=" +
                                           distributorType;
                        throw new ApplicationException(
                            "OrderProvider.GetOrders() Error. Unsuccessful result from web service. Data: " +
                            debugLine);
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "GetOrders failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OnlineOrder GetOrderDetail(string distributorId, int orderHeaderId)
        {
            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderDetailResponse_V01 response;
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    response = proxy.GetOrderDetail(new ServiceProvider.OrderChinaSvc.GetOrderDetailRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderDetailRequest_V01()
                    {
                        DistributorID = distributorId,
                        OrderHeaderID = orderHeaderId
                    })).GetOrderDetailResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderDetailResponse_V01;
                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response.Order;
                    }
                    else
                    {
                        string debugLine = " DistributorId=" + distributorId + "; orderHeaderId=" +
                                           orderHeaderId;
                        throw new ApplicationException(
                            "OrderProvider.GetOrderDetail() Error. Unsuccessful result from web service. Data: " +
                            debugLine);
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "GetOrderDetail failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OnlineOrder GetPreOrderDetail(string distributorId, int orderHeaderId)
        {
            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPreOrderDetailResponse_V01 response;
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    response = proxy.GetPreOrderDetail(new ServiceProvider.OrderChinaSvc.GetPreOrderDetailRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPreOrderDetailRequest_V01()
                    {
                        DistributorID = distributorId,
                        OrderHeaderID = orderHeaderId
                    })).GetPreOrderDetailResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPreOrderDetailResponse_V01;
                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        return response.Order;
                    }
                    else
                    {
                        string debugLine = " DistributorId=" + distributorId + "; orderHeaderId=" +
                                           orderHeaderId;
                        throw new ApplicationException(
                            "OrderProvider.GetPreOrderDetail() Error. Unsuccessful result from web service. Data: " +
                            debugLine);
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "GetPreOrderDetail failed: China Order Provier", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return null;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPBPPaymentServiceResponse_V01 GetPBPPaymentServiceDetail(string distributorId, string orderNumber)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.GetPBPPaymentServiceDetail(new ServiceProvider.OrderChinaSvc.GetPBPPaymentServiceDetailRequest(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPBPPaymentServiceRequest_V01()
                    {
                        DistributorId = distributorId,
                        OrderNumber = orderNumber
                    })).GetPBPPaymentServiceDetailResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPBPPaymentServiceResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {

                    return response;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetPBPPaymentServiceDetail failed: China Order Provier DistributorId: {0}, orderNumber= {1}, exception: {2} ",
                        distributorId, orderNumber, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static void ProcessPBPPaymentService(string distributorId, string orderNumber)
        {
            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ProcessPBPPaymentServiceResponse_V01 response;
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                response = proxy.ProcessPBPPaymentService(new ServiceProvider.OrderChinaSvc.ProcessPBPPaymentServiceRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ProcessPBPPaymentServiceRequest_V01()
                {
                    DistributorId = distributorId,
                    OrderNumber = orderNumber
                })).ProcessPBPPaymentServiceResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ProcessPBPPaymentServiceResponse_V01;
                if (response != null)
                {
                    if (response.Status != MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        LoggerHelper.Error(string.Format("Pay By Phone: Payment method validation does not pass!. OrderNumber: {0}", orderNumber));
                    }
                }
                else
                {
                    LoggerHelper.Error(string.Format("ProcessPBPPaymentService Failed. OrderNumber: {0}", orderNumber));
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "ProcessPBPPaymentService failed: China Order Provier DistributorId: {0}, orderNumber= {1}, exception: {2} ",
                        distributorId, orderNumber, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
        }

        public static List<PendingOrder> GetPayByPhonePendingOrders(string distributorId, string locale)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            var pendingOrders = new List<PendingOrder>();
            try
            {
                var response =
                    proxy.GetPayByPhonePendingOrder(new GetPayByPhonePendingOrderRequest(new GetPendingOrdersRequest_V01
                        {
                            DistributorId = distributorId,
                            CountryCode = locale
                        })).GetPayByPhonePendingOrderResult as GetPendingOrdersResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    pendingOrders = response.PendingOrders;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetPayByPhonePendingOrders failed: China Order Provier DistributorId: {0}, exception: {1} ",
                        distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return pendingOrders;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetFeedbackResponse_V01 GetFeedBack(string distributorId)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.GetFeedBack(new ServiceProvider.OrderChinaSvc.GetFeedBackRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetFeedbackRequest_V01())).GetFeedBackResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetFeedbackResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetFeedBack failed: China Order Provier DistributorId: {0}, exception: {2} ", distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.UpdateFeedbackResponse_V01 UpdateFeedBack(string distributorId, string orderId, MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetFeedbackResult feedbackResult)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.UpdateFeedBack(new ServiceProvider.OrderChinaSvc.UpdateFeedBackRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.UpdateFeedbackRequest_V01()
                    {
                        DistributorId = distributorId,
                        OrderId = orderId,
                        SetFeedbackResult = feedbackResult
                    })).UpdateFeedBackResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.UpdateFeedbackResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetFeedBack failed: China Order Provier DistributorId: {0}, exception: {2} ", distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderStatus> GetListOfOrderStatus()
        {
            var ret = HttpRuntime.Cache[CacheKey_CNSMS_OrderStatus] as List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderStatus>;

            if (ret == null)
            {
                var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderStatusListRequest();
                var response = proxy.GetOrderStatusList(new ServiceProvider.OrderChinaSvc.GetOrderStatusListRequest1(request)).GetOrderStatusListResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderStatusListResponse_V01;


                if (IsResponseValid(response))
                {
                    var responseV01 = response as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetOrderStatusListResponse_V01;
                    if (responseV01 == null) return ret;
                    ret = responseV01.OrderStatusList;
                    HttpRuntime.Cache[CacheKey_CNSMS_OrderStatus] = ret;
                }
            }

            return ret;
        }

        static bool IsResponseValid(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseValue response)
        {
            return ((response != null) && (response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success));
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackResponse_V01 GetExpressTrackInfo(string orderId)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.GetExpressTrackInfo(new ServiceProvider.OrderChinaSvc.GetExpressTrackInfoRequest(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackRequest_V01()
                {
                    OrderNumber = orderId,
                })).GetExpressTrackInfoResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "ChinaOrderProvier.GetExpressTrackInfo failed: OrderNumber: {0}, exception: {1} ", orderId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static string GetExternalExpressTrackInfo(string expressCode, string expressNo)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();

            try
            {
                var response = proxy.GetExpressTrackInfo(new ServiceProvider.OrderChinaSvc.GetExpressTrackInfoRequest(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackRequest_V02()
                    {
                        ExpressCode = expressCode,
                        ExpressNum = expressNo,
                    })).GetExpressTrackInfoResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackResponse_V01;

                if (IsResponseValid(response))
                    return response.ExpressTracking;
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(
                        string.Format(
                            "ChinaOrderProvier.GetExternalExpressTrackInfo failed: expressCode: {0}, expressNo: {1}, exception: {2} ",
                            expressCode, expressNo, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCDistributorInfo> GetPCCustomerIdByReferralId(string distributorId)
        {
            string cacheKey = string.Format("{0}_{1}", PCCustomerIdByReferralIdCacheKey,distributorId);
            var results = HttpRuntime.Cache[cacheKey] as List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCDistributorInfo>;
            var pcCustomerIdByReferralIdCacheMinutes =
                        Settings.GetRequiredAppSetting("PCCustomerIdByReferralIdCacheMinutes", 20d);
            if (results == null)
            {
                results = GetPCCustomerIdByReferralIdFromService(distributorId);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                                             results,
                                             null,
                                             DateTime.Now.AddMinutes(pcCustomerIdByReferralIdCacheMinutes),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Low,
                                             null);
                }
            }
            return results;
        }

        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCDistributorInfo> GetPreferredCustomers(string distributorId, DateTime? from, DateTime? to)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.GetPCCustomerIdByReferralId(new ServiceProvider.OrderChinaSvc.GetPCCustomerIdByReferralIdRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPCCustomerIDByReferralIDRequest_V01
                {
                    DistributorId = distributorId,
                    From = from,
                    To = to
                })).GetPCCustomerIdByReferralIdResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPCCustomerIdByReferralIdResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response.PCDistributorInfo;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "ChinaOrderProvier.GetPreferredCustomers failed: DistributorId: {0}, exception: {1} ",
                        distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        private static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCDistributorInfo> GetPCCustomerIdByReferralIdFromService(string distributorId)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var response = proxy.GetPCCustomerIdByReferralId(new ServiceProvider.OrderChinaSvc.GetPCCustomerIdByReferralIdRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPCCustomerIDByReferralIDRequest_V01()
                {
                    DistributorId = distributorId,
                })).GetPCCustomerIdByReferralIdResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPCCustomerIdByReferralIdResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response.PCDistributorInfo;
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "ChinaOrderProvier.GetPCCustomerIdByReferralIdFromService failed: DistributorId: {0}, exception: {1} ", distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }
        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyQuestions> GetCustomerSurvey(string distributorId)
        {
            string cacheKey = string.Format("{0}_{1}", GetCustomerSurveyCacheKey,distributorId);
            var results = HttpRuntime.Cache[cacheKey] as List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyQuestions>;
            if (results == null)
            {
                results = GetCustomerSurveyFromService(distributorId);
                if (results != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                                             results,
                                             null,
                                             DateTime.Now.AddMinutes(GetCustomerSurveyCacheMinutes),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Low,
                                             null);
                }
            }
            return results;
        }
        private static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyQuestions> GetCustomerSurveyFromService(string distributorId)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetCustomerSurveyRequest_V01
                    {
                        DistributorID = distributorId
                    };

                var response = proxy.GetCustomerSurvey(new ServiceProvider.OrderChinaSvc.GetCustomerSurveyRequest1(request)).GetCustomerSurveyResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetCustomerSurveyResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    var currentSession = SessionInfo.GetSessionInfo(distributorId, "zh-CN");
                    if (currentSession != null)
                    {
                        currentSession.surveyDetails = response.SurveyDetail;
                    }
                    return response.SurveyQuestions;
                }
               
            }
            catch (Exception ex)
            {
               LoggerHelper.Error((string.Format(
                        "ChinaOrderProvier.GetPCCustomerIdByReferralIdFromService failed: DistributorId: {0}, exception: {1} ", distributorId, ex)));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }
        

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyCustomerSelectionsDetailResponse_V01 SubmitCustomerSurvey(string distributorId, int surveyID, List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SelectionDetail> selectiondetails)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyCustomerSelectionsDetailRequest_V01
                {
                    DistributorID = distributorId,
                    SurveyID = surveyID,
                    SelectionDetail=selectiondetails
                };

                var response = proxy.SaveSurveyCustomerSelectionsDetail(new ServiceProvider.OrderChinaSvc.SaveSurveyCustomerSelectionsDetailRequest(request)).SaveSurveyCustomerSelectionsDetailResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyCustomerSelectionsDetailResponse_V01;
                if (response == null || response.Status != MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                     LoggerHelper.Error(
                        string.Format("Unabale to Submit Customer Survey indicates an error. Status: " +
                                                         (response == null ? "null" : response.Message)));
                    return null;
                }
                else
                {
                    string cacheKey = string.Format("{0}_{1}", GetCustomerSurveyCacheKey, distributorId);
                    HttpRuntime.Cache.Remove(cacheKey);

                }
                return response;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                        string.Format(
                        "ChinaOrderProvier.GetPCCustomerIdByReferralIdFromService failed: DistributorId: {0}, exception: {1} ", distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }
        public static void AddFreeGift(string freeSKU, int skuQuantity,MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01ItemCollection AllSkus,string WareHouseCode,MyHLShoppingCart cart)
        {

            Dictionary<string, MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01> _AllSKUS = AllSkus;
            MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01 sku;

            if (_AllSKUS.TryGetValue(freeSKU, out sku))
            {
                var item = Providers.CatalogProvider.GetCatalogItem(freeSKU, "CN");
                if ((
                ShoppingCartProvider.CheckInventory(item, skuQuantity,
                                                        WareHouseCode) > 0 &&
                    (Providers.CatalogProvider.GetProductAvailability(sku,
                                                           WareHouseCode) == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType.Available)))
                {

                    cart.AddItemsToCart(new List<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01>
                    {
                        new MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01 {SKU = freeSKU, Quantity = skuQuantity,IsPromo = true}
                    });

                }
            }

            
            }

        public static bool AnalyzePaymentGatewayOrder(string orderNumber,
                                           out string strResponse)
        {
            strResponse = string.Empty;
            var bOrderApproved = false;
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceRequest_V01()
                {
                    OrderNumber = orderNumber
                };
                var response = proxy.Get99BillOrderStatus(new ServiceProvider.OrderChinaSvc.Get99BillOrderStatusRequest(request)).Get99BillOrderStatusResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceResponse_V01;
                if (response != null &&
                    response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success &&
                    response.IsCreditCardOrder && !string.IsNullOrEmpty(response.CreditCardResponse))
                {
                    if (CN_99BillPaymentGatewayInvoker.CNPQueryRespnse(response.CreditCardResponse, orderNumber))
                        bOrderApproved = true;
                    strResponse = response.CreditCardResponse;
                }
                else if (response != null &&
                         response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success &&
                         response.GatewayResponse != null &&
                         response.GatewayResponse.OrderDetail != null && response.GatewayResponse.OrderDetail.Any())
                {
                    if (response.GatewayResponse.OrderDetail.FirstOrDefault().PayResult == "10")
                        bOrderApproved = true;
                    strResponse = ToXml(response.GatewayResponse);
                }
            }
            catch (Exception ex)
            {
               LoggerHelper.Error(string.Format(
                    "AnalyzePaymentGatewayOrder Get99BillOrderStatus failed: China Order Provier orderNumber: {0}, exception: {1} ",
                    orderNumber, ex.Message));
            }
            finally
            {
                proxy.Close();
            }
            return bOrderApproved;
        }

        private static string ToXml(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Gateway99BillOrderQueryResponse request)
        {
            try
            {
                var ser = new XmlSerializer(typeof(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Gateway99BillOrderQueryResponse));
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                ser.Serialize(writer, request);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("ToXML failed serializing" + ex.Message);
                return null;
            }
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceResponse_V01 Get99BillOrderStatus(string orderNumber)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceRequest_V01()
                    {
                        OrderNumber = orderNumber
                    };
                var response = proxy.Get99BillOrderStatus(new ServiceProvider.OrderChinaSvc.Get99BillOrderStatusRequest(request)).Get99BillOrderStatusResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response;
                }
                else
                {
                    var msg = string.Format("Get99BillOrderStatus : China Order Provier orderNumber: {0}, responseMsg: {1}",
                        orderNumber, response != null ? response.Message : "response is NULL");
                    LoggerHelper.Error(msg);
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "Get99BillOrderStatus failed: China Order Provier orderNumber: {0}, exception: {1} ", orderNumber, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.EventTicketResponse_V01 GetEventEligibility(string distributorId)
        {
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var req = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.EventTicketRequest_V01
                {
                    DistributorId = distributorId,
                    Datetime = DateTime.Now,
                };

                var rsp = proxy.GetEventEligibility(new ServiceProvider.OrderChinaSvc.GetEventEligibilityRequest(req)).GetEventEligibilityResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.EventTicketResponse_V01;

                if (!Helper.Instance.ValidateResponse(rsp))
                    LoggerHelper.Error(string.Format("GetEventEligibility failed: Message: {0}", rsp.Message));
                else
                {
                    var etoLog = Settings.GetRequiredAppSetting("ETOLog",false);
                    if (etoLog)
                        LoggerHelper.Info(string.Format("GetEventEligibility ValidateResponse : Message: {0}",
                        rsp != null ? rsp.Remark  : string.Empty));
                }
                return rsp;
            }
            catch(Exception ex)
            {
                ex = new ApplicationException(string.Format("ChinaOrderProvier.GetEventEligibility failed: distributorId: {0}, exception: {1}", distributorId, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }

            return null;
        }

        const string Key_CN_EligibleForEvents_Prefix = "CN_EligibleForEvents";

        /// <summary>
        /// Is this member eligible for events? Result is being cached at Session.
        /// </summary>
        /// <param name="distributorId"></param>
        /// <returns></returns>
        public static bool IsEligibleForEvents(string distributorId)
        {
            var key = string.Format("{0}_{1}", Key_CN_EligibleForEvents_Prefix, distributorId);
            var chk1 = HttpContext.Current.Session[key];
            if ((chk1 != null) && (chk1 is bool)) return (bool)chk1;
            
            var rsp = GetEventEligibility(distributorId);
            if (Helper.Instance.ValidateResponse(rsp))
            {
                HttpContext.Current.Session[key] = rsp.IsEligible;
                return rsp.IsEligible;
            }

            return false;
        }
        //public static List<HlCnPromo.Promotion> GetEffectivePromotionList(string locale, DateTime? dateTime = null)
        //{
        //    HlCnPromo.PromotionResponse resp = null;
        //    HlCnPromo.PromotionRequest_V01 req = new HlCnPromo.PromotionRequest_V01 { Locale = locale, DateTime = dateTime ?? DateTime.Now };
            
        //    using (var svc = ServiceClientProvider.GetChinaOrderServiceProxy())
        //    {
        //        resp = svc.GetEffectivePromotionList(req);
        //    }

        //    if (!Helper.Instance.ValidateResponse(resp))
        //    {
        //        string msg = string.Format("{0}.GetEffectivePromotionList() Error. locale={0}", ThisClassName, locale);
        //        if (resp != null) msg += (" , error=" + resp.Message);

        //        LoggerHelper.Error(msg);
        //        return null;
        //    }

        //    HlCnPromo.PromotionResponse_V01 resp1 = resp as HlCnPromo.PromotionResponse_V01;
        //    if (resp1 == null)
        //    {
        //        string msg = string.Format("{0}.GetEffectivePromotionList() Error. locale={0} , error=PromotionResponse_V01 is null", ThisClassName, locale);

        //        LoggerHelper.Error(msg);
        //        return null;
        //    }

        //    return resp1.PromotionList;
        //}
        
        public static List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> GetSkuOrderedAndPurchased(string country, string distributorId, DateTime? eventStartDate, DateTime? eventEndDate, List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> skuList)
        {
            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchasedResponse_V01 rsp = null;
            int customerProfileId = 0;
            if (!string.IsNullOrWhiteSpace(distributorId))
            {
                var user = DistributorOrderingProfileProvider.GetProfile(distributorId, country);

                if (user != null)
                {
                    customerProfileId = user.CNCustomorProfileID;
                }  
            }


            MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchasedRequest_V01 req = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchasedRequest_V01
            {
                Country = country,
                DistributorId = distributorId,
                CustomerProfileId = customerProfileId,
                SkuList = skuList,
                StartDate = eventStartDate,
                EndDate = eventEndDate,
            };

            try
            {
                using (var svc = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    rsp = svc.GetSkuOrderedAndPurchased(new ServiceProvider.OrderChinaSvc.GetSkuOrderedAndPurchasedRequest(req)).GetSkuOrderedAndPurchasedResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchasedResponse_V01;
                }

                if (!Helper.Instance.ValidateResponse(rsp))
                {
                    string msg = string.Format("{0}.GetSkuOrderedAndPurchased() Error. distributorId={0} , eventStartDate={1} , eventEndDate={2}", ThisClassName, distributorId, eventStartDate, eventEndDate);
                    if (rsp != null) msg += (" , error=" + rsp.Message);

                    LoggerHelper.Error(msg);
                    return null;
                }
                return rsp.SkuOrderedAndPurchasedList;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message); 
            }
           

            return null;
        }
        public static MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPaymentDetailsResponse_V01 GetPaymentDetails(List<int> orderHeaderList)
        {
            if (!(orderHeaderList != null && orderHeaderList.Any()))
            {
                return null;
            }
            var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
            try
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPaymentDetailsRequest_V01()
                {
                    OrderHeaderList = orderHeaderList
                };
                var response = proxy.GetPaymentDetails(new ServiceProvider.OrderChinaSvc.GetPaymentDetailsRequest1(request)).GetPaymentDetailsResult as MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetPaymentDetailsResponse_V01;
                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                {
                    return response;
                }
                else
                {
                    var msg = string.Format("GetPaymentDetails : China Order Provier orderHeaderId: {0}, responseMsg: {1}",
                        orderHeaderList[0], response != null ? response.Message : "response is NULL");
                    LoggerHelper.Error(msg);
                }
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(string.Format(
                        "GetPaymentDetails failed: China Order Provier orderHeaderId: {0}, exception: {1} ", orderHeaderList[0], ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }
        public static string QRCodeSkuCheck(string SKU,out string error)
        {
            error = string.Empty; 
           string URL = GetAbsoluteUrl() + Settings.GetRequiredAppSetting("SkuCheckAPI").ToString().Trim();
            var _endpoint = string.Format(URL, SKU);
            //For debug uncomment the below code
            // var _endpoint = string.Format("http://cn.qa4d.ws.myherbalife.com/qrcode_alt/events/api/Events/V2/ticket/checksku/{0}", SKU);
            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(_endpoint).GetAwaiter().GetResult();
                if (response !=null && response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return result;
                }
                else
                {
                    error = string.Format("-1"+Convert.ToString((int)response.StatusCode)); 
                }
            }
            LoggerHelper.Error(string.Format("QR code Check EventTicket  error base url {0}, Message {1}", _endpoint, error));
            return string.Empty;
        }
        public static string EventsTicketCreationAPI(string member, string orderNumber, string orderDate,string eventId,List<MyHLShoppingCart.Ticket> TicketDetail,out string error)
        {
            error = string.Empty;
            var QRCode = new MyHLShoppingCart.QRCode
            {
                memberId = member,
                orderNumber = orderNumber,
                orderDate = orderDate,
                eventId = eventId,
                tickets=TicketDetail
            
            };
            string URL = GetAbsoluteUrl() + Settings.GetRequiredAppSetting("TicketCreationAPI").ToString().Trim();
            //For debug uncomment the below code
           //string URL = "http://cn.qa4d.ws.myherbalife.com/qrcode_alt/events/api/Events/V2/ticket/purchase";
            string _endpoint = URL;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = JsonConvert.SerializeObject(QRCode);
                var stringContent = new System.Net.Http.StringContent(json,
                     UnicodeEncoding.UTF8,
                     "application/json");
                var response = httpClient.PostAsync(_endpoint, stringContent).Result;
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return result;
                 
                }
                else
                {
                    error = string.Format("-2" + Convert.ToString((int)response.StatusCode));
                    LoggerHelper.Error(string.Format("QR code EventTicket creation error for DS {0}, baseURL{1}, Message{2}", member, _endpoint, error));
                    return string.Empty;
                }
            }
          
        }
        public static string EventsTicketDownLoadAPI(string code,string eventID, string locale)
        {
            string DownLoadURL = Settings.GetRequiredAppSetting("TicketDownLoadAPI").ToString().Trim();
            string path = string.Empty;
            if (!string.IsNullOrEmpty(code) && code == "0")
            {
                //string URL = "http://cn.qa4d.ws.myherbalife.com/qrcode_alt/events/OneTimeQRCode/SelectEvent?eventId={0}&locale={1}&errorCode=0";
                path = string.Format(DownLoadURL, eventID,locale,0);


            }
            else
            {
                path = string.Format(DownLoadURL, "",locale,code+"&");
            }
            string URL = GetAbsoluteUrl()+ path;
            LoggerHelper.Warn(string.Format("QR CODE download URL :{0}", URL));
            return URL;
        }
        public static string DownloadQRCode(string member, string orderNumber, string orderDate, string locale, List<MyHLShoppingCart.Ticket> TicketDetail)
        {
            try
            {
                #region Properties
                string code = string.Empty;
                string message = string.Empty;
                bool usesQRCode = false;
                string eventID = string.Empty;
                string error = string.Empty;
                List<string> CheckQRCode = new List<string>();
                List<string> errors = new List<string>();
                #endregion Properties
                //QR Code SKU-check 
                foreach (var item in TicketDetail)
                {
                    var result = QRCodeSkuCheck(item.ticketSKU, out error);
                    if(!string.IsNullOrEmpty(result))
                    CheckQRCode.Add(result);
                    if(!string.IsNullOrEmpty(error))
                    {
                        errors.Add(error);
                    }
                   
                }
                
                if (CheckQRCode != null && CheckQRCode.Count > 0)
                {
                    foreach (var result in CheckQRCode)
                    {
                        JObject jObject = JObject.Parse(result);
                        if ((string)jObject["eventId"] !=null)
                        {
                            eventID = (string)jObject["eventId"];
                            usesQRCode = jObject["usesQRCode"]==null?false:(bool)jObject["usesQRCode"];
                        }
                        else
                        {
                            error = "-101";
                            errors.Add(error);
                        }
                    }
                }
                if (errors.Count > 0)
                {
                    LoggerHelper.Error(string.Format("DownloadQRCode failed: China Order Provier orderNumber: {0},DistributorID:{1} errors:{2}", orderNumber, member, string.Join(", ", errors)));
                    return EventsTicketDownLoadAPI(errors[0], string.Empty, locale);
                }
                if (!string.IsNullOrEmpty(eventID) && usesQRCode)
                {
                    //call Events Ticket Creation API
                    string Result = EventsTicketCreationAPI(member, orderNumber, orderDate, eventID, TicketDetail, out error);
                    if (!string.IsNullOrEmpty(Result))
                    {
                        JObject jObject = JObject.Parse(Result);
                        code = (string)jObject["code"];
                        if (code != "0")
                        {
                            error = "-201";
                            message = (string)jObject["message"];
                            errors.Add(error);
                            LoggerHelper.Error(string.Format("QR code error for DS {0}, Message{1}", member, string.Join(", ", errors)));

                        }
                        else
                            error = "0";
                        message = (string)jObject["message"];

                    }

                   return EventsTicketDownLoadAPI(error, eventID, locale);
                }
                LoggerHelper.Error(string.Format("DownloadQRCode failed: China Order Provier orderNumber: {0},DistributorID:{1} eventid: {2}, usesQRcode: {3}", orderNumber, member, eventID, usesQRCode));
                return string.Empty;

            }
            catch(Exception ex)
            {
                LoggerHelper.Error(string.Format("DownloadQRCode failed: China Order Provier orderNumber: {0},DistributorID:{1} exception: {2} ", orderNumber, member, ex));

            
            }

             return string.Empty;
            
            }
       

        public static string GetAbsoluteUrl()
        {
            //Uri oldUri = new Uri(HttpContext.Current.Request.Url.Authority.ToString());
            //UriBuilder builder = new UriBuilder(oldUri);
            //string newUri = builder.Scheme.ToString();
            //string Scheme = HttpContext.Current.Request.Url.Scheme.ToString();
            //string Url = string.Join("://", Scheme, newUri);
            //return Url;
            return Settings.GetRequiredAppSetting("MyHerbalife.InternalUrl").ToString().Trim();
        }
    }
}
