using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.UI;
using HL.Common.Configuration;
using MyHerbalife3.Core.DistributorProvider.Providers;
using MyHerbalife3.Ordering.Configuration.NPSConfiguration;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using HL.Common.Logging;
using WebUtilities = HL.Common.Utilities.WebUtilities;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using DateUtils = HL.Common.Utilities.DateUtils;
using Message = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Message;
using PhoneNumber_V03 = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03;
using MyHerbalife3.Ordering.ServiceProvider.PaymentGatewayBridgeSvc;
using Newtonsoft.Json;
using ServiceResponseStatusType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using PCLearningPointResponse_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCLearningPointResponse_V01;
using PCLearningPointRequest_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCLearningPointRequest_V01;
using ETOLearningPointRequest_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ETOLearningPointRequest_V01;
using ETOLearningPointResponse_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ETOLearningPointResponse_V01;
using System.Net.Http;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Model.Invoice;

namespace MyHerbalife3.Ordering.Providers
{
    public enum SubmitOrderStatus
    {
        Unknown = 0,
        OrderBeingSubmitted = 1,
        OrderSubmitted = 2,
        OrderSubmitFailed = 3,
        OrderSubmittedPending = 4,
        OrderSubmittedProcessing = 5,
        OrderSubmittedInReview = 6,
    }

    public class DupeOrderInfo
    {
        public DateTime OrderDate { get; set; }
        public string OrderNumber { get; set; }
        public bool IsDuplicdate { get; set; }
    }

    public partial class AsyncSubmitOrderProvider : ICallbackEventHandler
    {
        private string _countryCode;
        private string _orderID = string.Empty;
        private SessionInfo _sessionInfo;
        private MyHLShoppingCart _shoppingCart;
        private bool _submitOrderSuccess;

        string ICallbackEventHandler.GetCallbackResult()
        {
            return "";
        }

        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
        }

        private void SubmitOrderCallback(IAsyncResult ar)
        {
            var aResult = (AsyncResult)ar;
            var doDelegate = (DoSubmitOrderDelegate)aResult.AsyncDelegate;
            doDelegate.EndInvoke(ar);
            if ((ar.AsyncState as SessionInfo) != null)
            {
                (ar.AsyncState as SessionInfo).OrderNumber = _orderID;
                if (_submitOrderSuccess)
                {
                    (ar.AsyncState as SessionInfo).OrderStatus = SubmitOrderStatus.OrderSubmitted;
                }
                else
                {
                    (ar.AsyncState as SessionInfo).OrderStatus = SubmitOrderStatus.OrderSubmitFailed;
                }
            }
        }

        public IAsyncResult AsyncSubmitOrder(PaymentGatewayResponse response, string countryCode, SessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            DoSubmitOrderDelegate doDelegate = DoSubmitOrder;

            return doDelegate.BeginInvoke(response, countryCode, sessionInfo.OrderMonthShortString, sessionInfo.ShoppingCart, SubmitOrderCallback, sessionInfo);
        }

        /// <summary>
        ///     This is an async version of SubmitOrder used for Payment Gateways
        /// </summary>
        /// <param name="response"></param>
        /// <param name="countryCode"></param>
        /// <param name="orderMonthShortString"></param>
        /// <param name="shoppingCart"></param>
        /// <returns></returns>
        public bool DoSubmitOrder(PaymentGatewayResponse response, string countryCode, string orderMonthShortString, MyHLShoppingCart shoppingCart)
        {
            string error;
            _shoppingCart = shoppingCart;
            _countryCode = countryCode;
            _orderID = response.OrderNumber;

            var holder = new SerializedOrderHolder();
            try
            {
                _submitOrderSuccess = OrderProvider.deSerializeAndSubmitOrder(response, out error, out holder);
                if (_submitOrderSuccess)
                {
                    // Trying to update the order number and date to the shopping cart.
                    Order_V01 orderV01 = holder.Order as Order_V01;
                    ShoppingCartProvider.UpdateShoppingCart(_shoppingCart, null, _orderID, orderV01.ReceivedDate);
                    if (null != _sessionInfo)
                    {
                        _sessionInfo.Payments = orderV01.Payments;
                    }
                    if (!string.IsNullOrEmpty(holder.Email))
                    {
                        EmailHelper.SendEmail(_shoppingCart, holder.Order as Order_V01);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("DoSubmitOrder DoSubmitOrder error : {0}", ex.Message));
            }

            return _submitOrderSuccess;
        }

        private delegate bool DoSubmitOrderDelegate(PaymentGatewayResponse response, string countryCode, string orderMonthShortString, MyHLShoppingCart shoppingCart);
    }

    /// <summary>
    ///     OrderProvider
    /// </summary>
    public static partial class OrderProvider
    {
        private static ISimpleCache _cache = CacheFactory.Create();
        private static ILocalizationManager _localizationManager;
        public static IDistributorOrderingProfileProviderLoader DistributorOrderingProfileProviderLoader
        {
            get; set;
        }

        public static ICatalogProviderLoader CatalogProviderLoader
        {
            get; set;
        }

        public static IEmailHelperLoader EmailHelperLoader
        {
            get; set;
        }

        public const string InvoiceStatusCache = "InvoiceStatus_";
        //public static readonly int InvoiceStatusCacheMin = Settings.GetRequiredAppSetting<int>("OnlineInvoiceCacheExpireMinutes");
        public const string MLM_DS_CACHE_PREFIX = "MLM_DS_";

        public static int MLM_DS_CACHE_MINUTES = Settings.GetRequiredAppSetting<int>("MLMDSCacheExpirationMinutes");

        //public static int OnlineInvoiceCacheMinutes =Settings.GetRequiredAppSetting<int>("OnlineInvoiceCacheExpireMinutes");
        public static int NUMBER_OF_DAYS_FOR_MLM =
            Settings.GetRequiredAppSetting<int>("NoOfDaysToCheckForMLMDistributor");

        public static IEnumerable<string> EtoSkuList = Settings.GetRequiredAppSetting("ETOSkuList", String.Empty).Split('|');

        public static int GetAccumulatedPCLearningPoint(string distributorId, string learningPointType)
        {
            try
            {
                var result = LoadPCLearningPointFromService(distributorId);

                if (result != null && result.PCLearningPoint != null)
                {
                    if (learningPointType == "pcpoint")
                        return result.PCLearningPoint.PCLearningPoints;
                    else
                        return result.PCLearningPoint.ChangeRate;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                return 0;
            }

            return 0;
        }

        public static PCLearningPointResponse_V01 LoadPCLearningPointFromService(string distributorID)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {

                PCLearningPointRequest_V01 req = new PCLearningPointRequest_V01
                {
                    DistributorID = distributorID
                };

                PCLearningPointResponse_V01 resp = proxy.GetPCLearningPoint(new ServiceProvider.OrderChinaSvc.GetPCLearningPointRequest(req)).GetPCLearningPointResult as PCLearningPointResponse_V01;

                return resp;
            }
        }

        public static int DeductPCLearningPoint(string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    PCLearningPointRequest_V01 req = new PCLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = orderNumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = ""
                    };

                    PCLearningPointResponse_V01 resp = proxy.DeductPCLearingPoint(new ServiceProvider.OrderChinaSvc.DeductPCLearingPointRequest(req)).DeductPCLearingPointResult as PCLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success && resp.PCLearningPoint != null)
                        return resp.PCLearningPoint.PCLearningPoints;
                    else
                        return 0;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return 0;
                }
            }
        }

        public static bool LockPCLearningPoint(string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    PCLearningPointRequest_V01 req = new PCLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = orderNumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = ""
                    };

                    PCLearningPointResponse_V01 resp = proxy.LockPCLearningPoint(new ServiceProvider.OrderChinaSvc.LockPCLearningPointRequest(req)).LockPCLearningPointResult as PCLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        return true;
                    else
                        return false;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return false;
                }

            }
        }

        public static bool RollbackPCLearningPoint(string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    PCLearningPointRequest_V01 req = new PCLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = orderNumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = ""
                    };

                    PCLearningPointResponse_V01 resp = proxy.RollbackPCLearningPoint(new ServiceProvider.OrderChinaSvc.RollbackPCLearningPointRequest(req)).RollbackPCLearningPointResult as PCLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        return true;
                    else
                        return false;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return false;
                }
            }
        }
        // User Story 266925 changes
        public static int GetAccumulatedETOLearningPoint(string sku, string distributorId, string learningPointType)
        {
            try
            {
                var result = LoadETOLearningPointFromService(sku, distributorId);

                if (result != null && result.ETOLearningPoint != null)
                {
                    if (learningPointType == "etopoint")
                        return result.ETOLearningPoint.ETOLearningPoints;
                    else
                        return result.ETOLearningPoint.ChangeRate;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                return 0;
            }

            return 0;
        }


        public static double GetDueConvertibleFee(string etoSku, string distributorId)
        {
            var result = 0;
            try
            {
                var response = LoadETOLearningPointFromService(etoSku, distributorId);
                if(response != null)
                {
                    if (response.Status != ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        LoggerHelper.Error(string.Format("LoadETOLearningPointFromService is error : status:{0}", response.Status));
                    }
                    else
                    {
                        if(response.ETOLearningPoint != null)
                        {
                            var userConvertibleFee = response.ETOLearningPoint.ConvertibleFee;
                            var userMaxFee = response.ETOLearningPoint.MOneMax;
                            var orderConvertibleFee = response.ETOLearningPoint.MTotalMax - response.ETOLearningPoint.CurrentOrderTotalAmount;
                            return userMaxFee < userConvertibleFee ?
                                        userMaxFee < orderConvertibleFee ?
                                            userMaxFee
                                            : orderConvertibleFee
                                        : userConvertibleFee < orderConvertibleFee ?
                                            userConvertibleFee
                                        : orderConvertibleFee;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
            }
            return result;
        }

        private static ETOLearningPointResponse_V01 LoadETOLearningPointFromService(string etosku, string distributorID)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {

                ETOLearningPointRequest_V01 req = new ETOLearningPointRequest_V01
                {
                    DistributorID = distributorID,
                    Sku = etosku
                };

                ETOLearningPointResponse_V01 resp = proxy.GetEduChangeFeeForTicket(req) as ETOLearningPointResponse_V01;

                return resp;
            }
        }

        public static int DeductETOLearningPoint(string sku, string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {

            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    var onumber = orderNumber.StartsWith("888-") ? orderNumber : "888-" + orderNumber;
                    ETOLearningPointRequest_V01 req = new ETOLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = onumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = "",
                        Sku = sku
                    };

                    ETOLearningPointResponse_V01 resp = proxy.DeductEduChangeFeeForTicket(req) as ETOLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success && resp.ETOLearningPoint != null)
                        return resp.ETOLearningPoint.ETOLearningPoints;
                    else
                        return 0;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return 0;
                }
            }
        }

        public static bool LockETOLearningPoint(IEnumerable<string> orderItemSkus, string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {
            if (!EtoSkuList.Any())
                return false;

            var findEto = from itemsku in orderItemSkus
                          join sku in EtoSkuList
                          on itemsku equals sku
                          select sku;
            
            if(findEto.Any())
            {
                return LockETOLearningPoint(findEto.First(), distributorId, orderNumber, orderMonth, point, platform);
            }

            return false;
        }

        public static bool LockETOLearningPoint(string sku, string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {

            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    ETOLearningPointRequest_V01 req = new ETOLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = "888-" + orderNumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = "",
                        Sku = sku
                    };

                    ETOLearningPointResponse_V01 resp = proxy.LockEduChangeFeeForTicket(req) as ETOLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        return true;
                    else
                        return false;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return false;
                }

            }
        }

        public static bool RollbackETOLearningPoint(string distributorId, string orderNumber, string orderMonth, int point, string platform)
        {
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                try
                {
                    ETOLearningPointRequest_V01 req = new ETOLearningPointRequest_V01
                    {
                        DistributorID = distributorId,
                        OrderNumber = orderNumber,
                        Point = point,
                        Platform = platform,
                        VolumeMonth = orderMonth,
                        Opt = ""
                    };

                    ETOLearningPointResponse_V01 resp = proxy.RollbackLockEduChangeFeeForTicket(req) as ETOLearningPointResponse_V01;

                    if (resp.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                        return true;
                    else
                        return false;

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("{0}, \n {1}", ex.Message, ex.StackTrace));
                    return false;
                }
            }
        }
        // End User Story 266925 changes
        public static bool RenewHap(string distributorId)
        {
            if (string.IsNullOrEmpty(distributorId))
                return false;

            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new RenewHapRequest_V01();
                request.MemberId = distributorId;
                request.HapExtendFlag = "Y";

                var response = proxy.RenewHap(new RenewHapRequest1(request)).RenewHapResult as RenewHapResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return response.Success;
                }
            }
            catch (Exception ex)
            {
                ex = new ApplicationException("OrderProvider: Error GetOrderDetailsFromFusion.", ex);
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }

            return false;
        }

        public static Order_V02 GetOrderDetailsFromFusion(string orderId, string locale)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new GetOrderDetailRequest_V01();
                request.OrderNumber = orderId;
                request.Locale = locale;

                var response = proxy.GetOrderDetail(new GetOrderDetailRequest1(request)).GetOrderDetailResult as GetOrderDetailResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return response.Order;
                }
            }
            catch (Exception ex)
            {
                ex = new ApplicationException("OrderProvider: Error GetOrderDetailsFromFusion.", ex);
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            return null;
        }

        public static ServiceProvider.OrderChinaSvc.UpdateOrderInvoiceResponse UpdateOrderInvoice(string distributorId, ServiceProvider.OrderChinaSvc.UpdateOrderInvoiceRequest_V01 request)
        {
            try
            {

                var _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                ServiceProvider.OrderChinaSvc.UpdateOrderInvoiceResponse rsp =
                    _chinaOrderProxy.UpdateOrderInvoice(new ServiceProvider.OrderChinaSvc.UpdateOrderInvoiceRequest1(request)).UpdateOrderInvoiceResult as ServiceProvider.OrderChinaSvc.UpdateOrderInvoiceResponse;

                rsp.Status = ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success;
                return rsp;

            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error(
                        string.Format(
                            "OrderProvider.UpdateOrderInvoice() -> DistributorId{0}, error:{1}, stackTrace:{2}",
                            distributorId, ex.Message, ex.StackTrace));

                return null;
            }

        }

        public static void RemoveOrdersCache(string distributorId)
        {
            var cacheKey = "OrdersCache_" + distributorId;

            HttpRuntime.Cache.Remove(cacheKey);
        }

        public static List<ServiceProvider.OrderChinaSvc.OnlineOrder> GetOrdersWithDetail(string distributorId, int customerProfileID, string countryCode, DateTime startOrderDate, DateTime endOrderDate, OrderStatusFilterType statusFilter, string filterExpressions, string sortExpressions, bool isNonGDOOrder = false, bool isPreOrdering = false, string orderNumber = null)
        {
            //var cacheKey = "OrdersCacheInvoice_" + distributorId + startOrderDate.ToString("yyyyMMdd") + "_" + endOrderDate.ToString("yyyMMdd");

            //var result = HttpRuntime.Cache[cacheKey];

            //if (null != result)
            //{
            //    return ((GetOrdersResponse_V01)result).Orders as List<Order>;
            //}

            try
            {

                var ordStsList = new List<ServiceProvider.OrderChinaSvc.OrderStatusType>();
                switch (statusFilter)
                {
                    case OrderStatusFilterType.Cancel_Order:
                        ordStsList.Add(ServiceProvider.OrderChinaSvc.OrderStatusType.Cancel_Order);
                        break;

                    case OrderStatusFilterType.Complete:
                        ordStsList.Add(ServiceProvider.OrderChinaSvc.OrderStatusType.Complete);
                        break;

                    case OrderStatusFilterType.In_Progress:
                        ordStsList.Add(ServiceProvider.OrderChinaSvc.OrderStatusType.In_Progress);
                        break;

                    case OrderStatusFilterType.NTS_Printed:
                        ordStsList.Add(ServiceProvider.OrderChinaSvc.OrderStatusType.NTS_Printed);
                        break;

                    case OrderStatusFilterType.To_Be_Assign:
                        ordStsList.Add(ServiceProvider.OrderChinaSvc.OrderStatusType.To_Be_Assign);
                        break;

                    case OrderStatusFilterType.Payment_Failed:
                    case OrderStatusFilterType.Payment_Pending:
                        ordStsList = null;
                        break;
                }

                var req = new ServiceProvider.OrderChinaSvc.GetOrdersRequest_V02
                {
                    CountryCode = countryCode,
                    CustomerProfileID = customerProfileID,
                    OrderFilter = new ServiceProvider.OrderChinaSvc.OrdersFilter
                    {
                        DistributorId = distributorId,
                        StartDate = startOrderDate,
                        EndDate = endOrderDate,
                        OrderStatusList = ordStsList,
                        IsNonGDOOrders = isNonGDOOrder,
                        OrderNumber = orderNumber,
                    },
                    OrderingType = isPreOrdering ? ServiceProvider.OrderChinaSvc.OrderingType.PreOrder : ServiceProvider.OrderChinaSvc.OrderingType.RSO,
                };

                var _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                var rsp = _chinaOrderProxy.GetOrdersWithDetail(new ServiceProvider.OrderChinaSvc.GetOrdersWithDetailRequest(req)).GetOrdersWithDetailResult as ServiceProvider.OrderChinaSvc.GetOrdersResponse_V01;

                //HttpRuntime.Cache.Insert(cacheKey, rsp, null, DateTime.Now.AddMinutes(OnlineInvoiceCacheMinutes),
                //                         Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);

                return rsp.Orders;

            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error(
                        string.Format(
                            "OrderProvider.GetOrdersWithDetail() -> DistributorId{0}, error:{1}, stackTrace:{2}",
                            distributorId, ex.Message, ex.StackTrace));
                return null;
            }

        }

        static string getHapOrderCacheKey(string distributorId, string countryCode)
        {
            return string.Format("HAPORDERS_{0}_{1}", distributorId, countryCode);
        }

        public static List<Order_V02> AddHapOrders(string distributorId, string countryCode, MyHLShoppingCart cart, Order_V01 newOrder)
        {
            var hapOrders = GetHapOrders(distributorId, countryCode);
            if (hapOrders != null)
            {
                if (cart.HAPAction == "CREATE")
                {
                    if (!hapOrders.Exists(o => o.OrderID == newOrder.OrderID))
                        hapOrders.Add(CreateHAPOrderFromV01(newOrder, cart, distributorId));
                }
                else if (cart.HAPAction == "UPDATE")
                {
                    var orderEdited = hapOrders.Find(o => o.OrderID == newOrder.OrderID);
                    if (orderEdited != null && orderEdited.Pricing != null)
                    {
                        (orderEdited.Pricing as OrderTotals_V01).VolumePoints = (cart.Totals as OrderTotals_V01).VolumePoints;
                        (orderEdited.Pricing as OrderTotals_V01).AmountDue = (cart.Totals as OrderTotals_V01).AmountDue;
                    }
                }
            }
            return hapOrders;
        }
        public static List<Order_V02> RemoveHapOrders(string distributorId, string countryCode, string orderToRemove)
        {
            var hapOrders = GetHapOrders(distributorId, countryCode);
            if (hapOrders != null)
            {
                hapOrders.RemoveAll(r => r.OrderID == orderToRemove);
            }
            return hapOrders;
        }
        public static List<Order_V02> GetHapOrders(string distributorId, string countryCode)
        {
            return SimpleCache.Retrieve(_ => OrdersProvider.GetOrders(distributorId, "GDO-" + countryCode, null, null, true, ""),
                                      getHapOrderCacheKey(distributorId, countryCode), new TimeSpan(0, 20, 0));
        }
        public static List<Order_V02> GetActiveHAPOrders(string distributorId, string locale, string countryCode)
        {
            var _activeHapOrders = new List<Order_V02>();

            var oHAPOrders = GetHapOrders(distributorId, countryCode);

            if (oHAPOrders != null)
            {
                foreach (Order_V02 orderHeader in oHAPOrders)
                {
                    if (!orderHeader.HapOrderStatus.Contains("CANCELLED"))
                    {
                        var orderDetail = orderHeader.OrderItems == null ? GetOrderDetailsFromFusion(orderHeader.OrderID, locale) : orderHeader;

                        if (orderDetail != null && orderDetail.CountryOfProcessing != null && orderDetail.CountryOfProcessing.Contains(countryCode))
                        {
                            _activeHapOrders.Add(orderDetail);
                        }
                    }
                }

                return _activeHapOrders;
            }

            LoggerHelper.Error(
                    string.Format("GetActiveHAPOrders returned null"));
            return null;

        }

        public static ServiceProvider.OrderSvc.OnlineOrder CreateOrder(string orderNumber,
                                                                                MyHLShoppingCart shoppingCart,
                                                                                PaymentCollection payments)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {

                return China.OrderProvider.CreateOrder(orderNumber, shoppingCart, payments);
            }
            return null;
        }

        private static HandlingIncludeInvoice invoiceOptionConversion(InvoiceHandlingType from)
        {
            switch (from)
            {
                case InvoiceHandlingType.RecycleInvoice:
                    return HandlingIncludeInvoice.RECYCLEINVOICE;
                case InvoiceHandlingType.SendToCustomer:
                    return HandlingIncludeInvoice.TODISTRIBUTOR;
                case InvoiceHandlingType.SendToDistributor:
                    return HandlingIncludeInvoice.TODISTRIBUTOR;
                case InvoiceHandlingType.WithPackage:
                    return HandlingIncludeInvoice.WITHPACKAGE;
                default:
                    break;
            }
            return HandlingIncludeInvoice.TODISTRIBUTOR;
        }

        public static void SetAPFDeliveryOption(MyHLShoppingCart cart)
        {
            // this method may get called from workder thread
            var config = HLConfigManager.CurrentPlatformConfigs[cart.Locale].APFConfiguration;
            if (null != cart.DeliveryInfo)
            {
                cart.DeliveryInfo.WarehouseCode = string.IsNullOrEmpty(config.APFwarehouse)
                                                      ? cart.DeliveryInfo.WarehouseCode
                                                      : config.APFwarehouse;
                cart.DeliveryInfo.FreightCode = config.APFFreightCode;
            }
        }

        public static OrderItem CreateOrderItem(DistributorShoppingCartItem item)
        {
            var oitem = new OrderItem_V01();
            oitem.SKU = item.SKU;
            oitem.Quantity = item.Quantity;
            return oitem;
        }

        public static ServiceProvider.OrderSvc.Order PopulateLineItems(string countryCode,
                                                                    ServiceProvider.OrderSvc.Order _order,
                                                                    MyHLShoppingCart shoppingCart)
        {
            Order_V01 order = _order as Order_V01;
            //DistributorShoppingCartInfo OrderLines = getCartItems();
            if (shoppingCart != null && shoppingCart.ShoppingCartItems != null)
            {
                order.OrderItems = new OrderItems();
                var sortedItems = new List<OrderItem>();

                foreach (DistributorShoppingCartItem item in shoppingCart.ShoppingCartItems)
                {
                    var oitem = HLConfigManager.Configurations.DOConfiguration.IsChina
                                    ? China.OrderProvider.CreateOrderItem(item)
                                    : CreateOrderItem(item);
                    sortedItems.Add(oitem);
                }
                order.OrderItems = new OrderItems();
                order.OrderItems.AddRange(sortedItems.OrderBy(s => s.SKU).ToList());
                order = ShoppingCartProvider.AddLinkedSKU(_order, shoppingCart.Locale, countryCode,
                                                          shoppingCart.DeliveryInfo.WarehouseCode) as Order_V01;
            }
            return order;
        }

        public static ShippingInfo_V01 CreateShippingInfo(string CountryCode, MyHLShoppingCart shoppingCart)
        {
            var shippingInfo = new ShippingInfo_V01
            {
                Address = new ServiceProvider.OrderSvc.Address_V01()
                {
                    Line1 = shoppingCart.DeliveryInfo.Address.Address.Line1,
                    Line2 = shoppingCart.DeliveryInfo.Address.Address.Line2,
                    Line3 = shoppingCart.DeliveryInfo.Address.Address.Line3,
                    Line4 = shoppingCart.DeliveryInfo.Address.Address.Line4,
                    City = shoppingCart.DeliveryInfo.Address.Address.City,
                    CountyDistrict = shoppingCart.DeliveryInfo.Address.Address.CountyDistrict,
                    StateProvinceTerritory = shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory,
                    Country = shoppingCart.DeliveryInfo.Address.Address.Country,
                    PostalCode = shoppingCart.DeliveryInfo.Address.Address.PostalCode
                }
            };

            var provider = ShippingProvider.GetShippingProvider(null);
            if (provider != null)
            {
                provider.GetDistributorShippingInfoForHMS(shoppingCart, shippingInfo);
            }

            shippingInfo.WarehouseCode = shoppingCart.DeliveryInfo.WarehouseCode;
            shippingInfo.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
            shippingInfo.FreightVariant = shoppingCart.DeliveryInfo.AddressType;
            shippingInfo.DeliveryNickName = shoppingCart.DeliveryInfo.DeliveryNickName;

            var checkoutConfig =
                HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].CheckoutConfiguration;

            if (!checkoutConfig.ModifyRecipientName)
            {
                shippingInfo.Recipient = shoppingCart.DeliveryInfo.Address.Recipient;
            }
            else
            {
                var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
                if (shippingProvider != null)
                {
                    shippingInfo.Recipient = shippingProvider.GetRecipientName(shoppingCart.DeliveryInfo);
                    shippingInfo.Address.Line4 = String.Empty;
                }
            }

            if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var dashes = (from c in shoppingCart.DeliveryInfo.Address.Phone
                              where c == '-'
                              select c).Count();
                switch (dashes)
                {
                    case 0:
                        shippingInfo.Phone = string.Format("-{0}-", shoppingCart.DeliveryInfo.Address.Phone);
                        break;
                    case 1:
                        // Takes by default area code-phone number
                        shippingInfo.Phone = string.Format("{0}-", shoppingCart.DeliveryInfo.Address.Phone);
                        break;
                    default:
                        shippingInfo.Phone = shoppingCart.DeliveryInfo.Address.Phone;
                        break;
                }
            }
            else
            {
                shippingInfo.Phone = shoppingCart.SMSNotification;
            }

            return shippingInfo;
        }

        public static HandlingInfo_V01 CreateHandlingInfo(string CountryCode,
                                                          string InvoiceOption,
                                                          MyHLShoppingCart ShoppingCart,
                                                          ShippingInfo_V01 shippingInfo)
        {
            string invOpt = InvoiceOption == null ? string.Empty : InvoiceOption;
            if (String.IsNullOrEmpty(invOpt) && !String.IsNullOrEmpty(ShoppingCart.InvoiceOption))
            {
                invOpt = ShoppingCart.InvoiceOption;
            }

            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);

            var handlingInfo = new HandlingInfo_V01();
            if (!String.IsNullOrEmpty(invOpt))
            {
                handlingInfo.IncludeInvoice = (InvoiceHandlingType)Enum.Parse(typeof(InvoiceHandlingType), invOpt);
            }
            else
            {
                //Call the rule and get the default option
                var catItems = CatalogProvider.GetCatalogItems((from c in ShoppingCart.CartItems
                                                                select c.SKU.Trim()).ToList<string>(),
                                                               CountryCode);
                var list = new List<CatalogItem_V01>();
                list.AddRange(catItems.Select(c => c.Value as CatalogItem_V01).ToList());
                var invoiceOptions = new List<InvoiceHandlingType>();
                if (shippingProvider != null)
                {
                    invoiceOptions = shippingProvider.GetInvoiceOptions(ShoppingCart.DeliveryInfo.Address, list,
                                                                        ShoppingCart);
                }
                if (invoiceOptions != null && invoiceOptions.Count > 0)
                {
                    handlingInfo.IncludeInvoice = invoiceOptions.First();

                    if (string.IsNullOrEmpty(ShoppingCart.InvoiceOption))
                    {
                        ShoppingCart.InvoiceOption = invoiceOptions.First().ToString();
                    }
                }
            }

            if (ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
            {
                handlingInfo.PickupName = shippingInfo.Recipient;
                handlingInfo.ShippingInstructions = ShoppingCart.DeliveryInfo.Instruction;

                // For defect 40573
                // Format the pickupname for GR
                // Remove this according 40857.
                //if (ShoppingCart.CountryCode.Equals("GR") && ShoppingCart.DeliveryInfo.PickupDate.HasValue)
                //{
                //    handlingInfo.PickupName = string.Format("{0}, {1}", shippingInfo.Recipient,
                //        ShoppingCart.DeliveryInfo.PickupDate.Value.ToString("dd/MM/yy"));
                //}

                if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                    && ShoppingCart.CountryCode.Equals("MX")
                    && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasSpecialEventWareHouse
                    && ShoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
                {
                    var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    if (distributorProfileModel != null)
                    {
                        _localizationManager = new LocalizationManager();
                        var teamLevelName = _localizationManager.GetGlobalString("HrblUI", distributorProfileModel.Value.TeamLevelNameResourceKey);
                        var calculatedType = distributorProfileModel.Value.IsChairmanClubMember
                                            || distributorProfileModel.Value.IsFounderCircleMember
                            ? teamLevelName
                            : distributorProfileModel.Value.SubTypeCode;
                        handlingInfo.PickupName = string.Format("{0} {1}, {2}", distributorProfileModel.Value.FirstName, distributorProfileModel.Value.LastName, calculatedType);
                    }
                }
            }
            else if (ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier)// && HLConfigManager.Configurations.CheckoutConfiguration.IncludePUNameForPUFromCourier)
            {
                handlingInfo.PickupName = shippingInfo.Recipient ?? string.Empty;
                handlingInfo.ShippingInstructions = ShoppingCart.DeliveryInfo.Instruction;

            }
            else
            {
                handlingInfo.PickupName = string.Empty; // 37142, if shipping, set pickup name empty
                handlingInfo.ShippingInstructions = ShoppingCart.DeliveryInfo.Instruction;
            }

            var checkoutConfig =
                HLConfigManager.CurrentPlatformConfigs[ShoppingCart.Locale].CheckoutConfiguration;
            if (checkoutConfig.GetShippingInstructionsFromProvider)
            {
                if (shippingProvider != null)
                {
                    handlingInfo.ShippingInstructions = shippingProvider.GetShippingInstructionsForDS(ShoppingCart,
                                                                                                      ShoppingCart
                                                                                                          .DistributorID,
                                                                                                      ShoppingCart
                                                                                                          .Locale);
                }
            }
            return handlingInfo;
        }

        public static bool IsRegisteredCardsOnly(MyHLShoppingCart myShoppingCart)
        {
            var currentLocale = Thread.CurrentThread.CurrentCulture.Name;
            if (!currentLocale.Substring(3).Equals("MX") || !HLConfigManager.Configurations.DOConfiguration.IsEventInProgress)
            {
                return HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry;
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress)
            {
                var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var isMexicanMember = (user != null && user.Value.ProcessingCountryCode == "MX"
                                       && (currentLocale == "es-MX" || currentLocale == "en-MX"));
                var isApfOnly = APFDueProvider.containsOnlyAPFSku(myShoppingCart.ShoppingCartItems);
                if ((HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry == false
                    && isMexicanMember) || isApfOnly)
                {
                    return true;
                }
                if (!isMexicanMember
                    && myShoppingCart != null
                    && myShoppingCart.DeliveryInfo != null
                    && myShoppingCart.DeliveryInfo.WarehouseCode != "96"
                    && myShoppingCart.DeliveryInfo.WarehouseCode != "M0")
                {
                    return true;
                }
            }

            return HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry;
        }

        private static string LogObjectTo(object obj, string nameObj)
        {
            var log = new StringBuilder();
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
            return log.ToString();
        }

        private static string OrderSerialization(ServiceProvider.SubmitOrderBTSvc.Order order, string cartID)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Distributor : {0}, ReceiveDate : {1}, Cart ID : {2}", order.DistributorID,
                            order.ReceivedDate.ToString(), cartID);
            sb.AppendLine();
            sb.Append(LogObjectTo(order, "Order Details************"));
            sb.Append(LogObjectTo(order.Pricing, "Pricing"));
            sb.Append(LogObjectTo(order.Shipment, "Shipment"));
            sb.Append(LogObjectTo(order.Shipment.Address, "Shipment Adress"));
            if (order.Payments != null)
            {
                foreach (ServiceProvider.SubmitOrderBTSvc.Payment payment in order.Payments)
                {
                    sb.Append(LogObjectTo(payment, "Payment"));
                    if (payment.Address != null)
                    {
                        sb.Append(LogObjectTo(payment.Address, "Payment Address"));
                    }
                }
            }
            sb.Append(LogObjectTo(order.Handling, "Order Handling"));
            sb.Append(LogObjectTo(order.Handling.IncludeInvoice, "Order Handling IncludeInvoice"));
            if (order.Messages != null)
            {
                foreach (Message mensaje in order.Messages)
                {
                    sb.Append(LogObjectTo(mensaje, "Order Message"));
                }
            }
            sb.AppendLine("End of order.");

            return sb.ToString();
        }

        public static decimal GetConvertedAmount(decimal amountDue, string CountryCode)
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                try
                {
                    decimal rate = 0.0M;
                    var request = new CurrencyConversionRateRequest_V01();
                    request.FromCurrency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                    request.ToCurrency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyTo;
                    request.RequestDate = DateUtils.GetCurrentLocalTime(CountryCode);
                    var response =
                        proxy.GetCurrencyConversion(new GetCurrencyConversionRequest(request)).GetCurrencyConversionResult as CurrencyConversionRateResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        rate = response.ConversionRate;
                        if (rate != 0.0M)
                        {
                            amountDue = Math.Round(amountDue * rate, 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            amountDue = 0.0M;
                            Exception ex1 = new ApplicationException("OrderProvider: Conversion Rate Not Received");
                            WebUtilities.LogServiceExceptionWithContext(ex1, proxy);
                        }
                        LoggerHelper.Write("Error getting currency conversion: " + CountryCode + ", Rate: " + response.ConversionRate, "GetConvertedAmount");
                    }
                    else
                    {
                        amountDue = 0.0M;
                    }
                }
                catch (Exception ex)
                {
                    ex = new ApplicationException("OrderProvider: Error getting currency conversion.", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                    amountDue = 0.0M;
                    LoggerHelper.Write("Error getting currency conversion: " + CountryCode, "GetConvertedAmount");

                }
            }

            return amountDue;
        }

        public static decimal GetOriginalConvertedAmount(decimal amountDue, string CountryCode)
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                try
                {
                    var rate = 0.0M;
                    var request = new CurrencyConversionRateRequest_V01();
                    request.FromCurrency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyTo;
                    request.ToCurrency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                    request.RequestDate = DateUtils.GetCurrentLocalTime(CountryCode);
                    var response =
                        proxy.GetCurrencyConversion(new GetCurrencyConversionRequest(request)).GetCurrencyConversionResult as CurrencyConversionRateResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        rate = response.ConversionRate;
                        if (rate != 0.0M)
                        {
                            amountDue = Math.Round(amountDue * rate, 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            amountDue = 0.0M;
                            Exception ex1 = new ApplicationException("OrderProvider: Conversion Rate Not Received");
                            WebUtilities.LogServiceExceptionWithContext(ex1, proxy);
                        }
                    }
                    else
                    {
                        amountDue = 0.0M;
                    }
                }
                catch (Exception ex)
                {
                    ex = new ApplicationException("OrderProvider: Error getting currency conversion.", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                    amountDue = 0.0M;
                }
            }

            return amountDue;
        }

        /// <summary>
        ///     Checks to see if an order is a duplicate of the most recent order
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The Date of the dupe</returns>
        public static DupeOrderInfo CheckForRecentDupeOrder(Order_V01 order, int shoppingCartID)
        {
            string existingOrderId = order.OrderID;
            var dupeOrderInfo = new DupeOrderInfo { OrderDate = DateTime.MinValue, OrderNumber = string.Empty };
            var proxy = ServiceClientProvider.GetOrderServiceProxy();

            OrderDupeCheckRequest request = new OrderDupeCheckRequest_V01() { Order = order };
            request.Order.OrderID = shoppingCartID.ToString(CultureInfo.InvariantCulture);
            OrderDupeCheckResponse response = null;
            try
            {
                response = proxy.FindDuplicate(new FindDuplicateRequest(request)).FindDuplicateResult;
                if (response.Status == ServiceResponseStatusType.Success)
                {
                    dupeOrderInfo.OrderDate = (response as OrderDupeCheckResponse_V01).CreatedDate;
                    dupeOrderInfo.OrderNumber = (response as OrderDupeCheckResponse_V01).OrderID;
                    dupeOrderInfo.IsDuplicdate = (response as OrderDupeCheckResponse_V01).IsDuplicate;
                }
            }
            catch (Exception ex)
            {
                ex = new ApplicationException("OrderProvider: Error checking for recent duplicate order.", ex);
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }

            order.OrderID = existingOrderId;
            return dupeOrderInfo;
        }


        public static string SubmitHAPOrder(Order_V02 order, string CountryCode, string distributorID)
        {
            var btOrder = OrderProvider.CreateOrderFromHAPOrder(order, CountryCode, distributorID) as ServiceProvider.SubmitOrderBTSvc.Order;

            List<FailedCardInfo> failedCards = null;
            string error = string.Empty;

            if (btOrder == null)
            {
                return PlatformResources.GetGlobalResourceString("ErrorMessage", "ErrorCancelHAPOrder");
            }

            string orderID = OrderProvider.ImportOrder(btOrder, out error, out failedCards, 1);
            if (string.IsNullOrEmpty(error))
            {
                RemoveHapOrders(distributorID, CountryCode, orderID);
            }
            return error;
        }

        public static Order_V02 CreateHAPOrderFromV01(Order_V01 order, MyHLShoppingCart cart, string distributorID)
        {
            var orderV02 = new Order_V02();
            orderV02.DistributorID = order.DistributorID;
            orderV02.CountryOfProcessing = order.CountryOfProcessing;
            orderV02.OrderID = order.OrderID;
            orderV02.ReferenceID = order.ReferenceID;
            orderV02.InputMethod = order.InputMethod;
            orderV02.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.HSO;
            orderV02.HapOrderProgramType = cart.HAPType;
            switch (cart.HAPScheduleDay)
            {
                case 4:
                    orderV02.HapOrderSchedule = "A";
                    break;
                case 11:
                    orderV02.HapOrderSchedule = "B";
                    break;
                case 18:
                    orderV02.HapOrderSchedule = "C";
                    break;
            }
            orderV02.HapOrderStatus = "ACTIVE";
            orderV02.IsHapOrder = true;
            orderV02.ReceivedDate = order.ReceivedDate;
            orderV02.OrderMonth = order.OrderMonth;
            orderV02.DiscountPercentage = order.DiscountPercentage;
            orderV02.CustomerID = string.Empty;
            orderV02.QualifyingSupervisorID = order.DistributorID;
            orderV02.Shipment = order.Shipment;
            orderV02.Pricing = order.Pricing;
            orderV02.OrderItems = order.OrderItems;
            orderV02.Handling = order.Handling;
            orderV02.Payments = order.Payments;
            orderV02.Messages = order.Messages;
            orderV02.WebClientAuthenticationKey = order.WebClientAuthenticationKey;
            return orderV02;
        }

        public static ServiceProvider.SubmitOrderBTSvc.Order CreateOrderFromHAPOrder(Order_V02 order, string locale, string distributorID)
        {
            if (string.IsNullOrEmpty(order.OrderID))
                return null;

            var btOrder = new ServiceProvider.SubmitOrderBTSvc.Order();
            var country = locale.Substring(3, 2);
            var pricing = order.Pricing as OrderTotals_V01;

            btOrder.DistributorID = distributorID;
            btOrder.CountryOfProcessing = country;

            btOrder.OrderID = order.OrderID;
            btOrder.ReferenceID = string.Format("HAPC-{0}", Guid.NewGuid());
            btOrder.InputMethod = "IE";

            btOrder.OrderCategory = "HSO";
            switch (order.HapOrderSchedule)
            {
                case "A":
                    btOrder.HapScheduleCode = "A";
                    btOrder.HapSchedule = "4th";
                    break;
                case "B":
                    btOrder.HapScheduleCode = "B";
                    btOrder.HapSchedule = "11th";
                    break;
                case "C":
                    btOrder.HapScheduleCode = "C";
                    btOrder.HapSchedule = "18th";
                    break;
            }

            btOrder.OrderSubType = btOrder.HapTypeCode = order.HapOrderProgramType;
            btOrder.HapType = btOrder.HapTypeCode == "01" ? "Personal" : "Resale";
            btOrder.HapAction = "CANCEL";
            btOrder.IsHap = "true";

            btOrder.ReceivedDate = order.ReceivedDate;
            btOrder.OrderMonth = order.OrderMonth != null && order.OrderMonth.Length >= 7
                                     ? order.OrderMonth.Substring(2, 2) + order.OrderMonth.Substring(5, 2)
                                     : DateTime.Today.ToString("yyMM");
            btOrder.DiscountPercentage = order.DiscountPercentage;
            btOrder.CustomerID = string.Empty;
            btOrder.QualifyingSupervisorID = order.DistributorID;

            btOrder.Website = "GDO";
            btOrder.Shipment = setShipment(order);

            btOrder.Pricing = setPricing(pricing);

            var btItems = new List<ServiceProvider.SubmitOrderBTSvc.Item>();
            //Retrieving a list of child skus in the order
            List<string> childSKUsInOrder = new List<string>();
            var sKUsInOrder = from o in order.OrderItems
                              select o.SKU;
            childSKUsInOrder = retrieveChildSKUsInOrder(sKUsInOrder.ToList<string>());

            foreach (OrderItem item in order.OrderItems)
            {
                var newItem = CreateLineItem(pricing, item, btOrder.Shipment.WarehouseCode, item.Quantity,
                                             childSKUsInOrder, 0M);
                btItems.Add(newItem);
            }
            btOrder.OrderItems = btItems.ToArray();

            var handling = new Handling();
            var handlingV01 = order.Handling as HandlingInfo_V01;
            handling.IncludeInvoice = invoiceOptionConversion(handlingV01.IncludeInvoice);
            handling.PickupName = handlingV01.PickupName;
            handling.ShippingInstructions = "Member cancelled order online";
            btOrder.Handling = handling;

            btOrder.Payments = setPayments(order.Payments, country, locale, null, false);

            if (btOrder.Payments == null)
            {
                LoggerHelper.Error(
                    string.Format(
                        "CreateOrderFromHAPOrder DS:{0} OrderId{1} - No Payment info retrieved from HMS, cannot cancel the order",
                        distributorID, order.OrderID));
                return null;
            }

            foreach (var payment in btOrder.Payments)
            {
                if (payment.Amount == 0)
                    payment.Amount = pricing != null ? pricing.AmountDue : 1;
            }

            order.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.HSO;
            var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(order.DistributorID, country);
            if (DistributorType == MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.Scheme.Member)
            {
                order.OrderIntention = OrderIntention.PersonalConsumption;
            }
            else
            {
                if (!string.IsNullOrEmpty(btOrder.HapType))
                {
                    if (btOrder.HapType == "Personal")
                    {
                        order.OrderIntention = OrderIntention.PersonalConsumption;
                    }
                    else if (btOrder.HapType == "RetailOrder")
                    {
                        order.OrderIntention = OrderIntention.RetailOrder;
                    }
                }
            }

            btOrder.Messages = new List<Message>().ToArray();
            getParametersByCountry(country, btOrder, locale, order);
            btOrder.WebClientAuthenticationKey =
                WebClientAuthenticationProvider.GetWebClientAuthCode(btOrder.CountryOfProcessing);

            btOrder.EnableDeferredProcessing = HLConfigManager.Configurations.DOConfiguration.HasDeferredProcessing
                                                   ? "true"
                                                   : "false";
            btOrder.ServerName = Environment.MachineName;
            btOrder.Locale = locale;
            var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorID, country);
            if (distributorOrderingProfile != null)
            {
                DistributorOrderingProfileProvider.GetHAPSettings(distributorOrderingProfile);
                btOrder.HapExpiryDate = distributorOrderingProfile.HAPExpiryDateSpecified &&
                                        distributorOrderingProfile.HAPExpiryDate != null
                                            ? ((DateTime)distributorOrderingProfile.HAPExpiryDate).ToString(
                                                "MM/dd/yyyy")
                                            : string.Empty;
            }

            DistributorOrderConfirmation orderForMail = null;
            try
            {
                //Get ShoppingCart to calculate the Items pricing details
                var cartInfo = ShoppingCartProvider.GetShoppingCartFromHAPOrder(distributorID, locale, order.OrderID);

                orderForMail = EmailHelper.GetEmailFromOrder_V02(order, cartInfo, locale);

                if (orderForMail != null)
                {
                    btOrder.SendEmail = "true";

                    btOrder.Email = orderForMail.Distributor.Contact.Email;

                    // Email Info
                    var btOrderEmailInfo = new EmailInfo
                    {
                        DeliveryTimeEstimated = orderForMail.DeliveryTimeEstimated,
                        FirstName = orderForMail.Distributor.FirstName,
                        HffMessage = orderForMail.HFFMessage,
                        InvoiceOption = orderForMail.InvoiceOption,
                        LastName = orderForMail.Distributor.LastName,
                        MiddleName = orderForMail.Distributor.MiddleName,
                        PaymentOption = orderForMail.paymentOption,
                        PaymentType =
                                orderForMail.Payments[0].CardType.ToUpper() == "WIRE" ||
                                orderForMail.Payments[0].CardType.ToUpper() == "DEMANDDRAFT"
                                    ? "Wire"
                                    : "CreditCard",
                        PickUpLocation = orderForMail.PickupLocation,
                        PurchaseType = orderForMail.PurchaseType,
                        ShippingMethod = orderForMail.Shipment.ShippingMethod,
                        SpecialInstructions = orderForMail.SpecialInstructions
                    };

                    btOrder.EmailInfo = btOrderEmailInfo;

                    // Pricing
                    if (order.CountryOfProcessing != "CN") // don't override for China
                    {
                        btOrder.Pricing.DiscountedItemsTotal =
                            orderForMail.TotalDiscountRetail.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.LocalTaxCharge =
                            orderForMail.LocalTaxCharge.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.LogisticsCharge =
                            orderForMail.Logistics.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.IpiTax = orderForMail.IPI.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.IcmsTax = orderForMail.ICMS.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.TotalMarketingFund =
                            orderForMail.MarketingFund.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.TaxedNet = orderForMail.TaxedNet.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.TotalProductRetail =
                            orderForMail.TotalProductRetail.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.TotalCollateralRetail =
                            orderForMail.TotalCollateralRetail.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.TotalPromotionalRetail =
                            orderForMail.TotalPromotionalRetail.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.ItemsTotal = orderForMail.TotalRetail.ToString(CultureInfo.InvariantCulture);
                        btOrder.Pricing.VolumePointsRate = orderForMail.VolumePointsRate;
                    }
                    GetMessagesForEmail_V02(country, order, btOrder, orderForMail);
                    if (pricing != null)
                    {
                        btOrder.PricingServerName = pricing.PricingServerName;
                    }

                    // Payment
                    foreach (var payment in btOrder.Payments)
                    {
                        var paymentFromMail = orderForMail.Payments.ToList().Find(oi => oi.Amount == payment.Amount);
                        if (paymentFromMail != null)
                        {
                            payment.BankName = paymentFromMail.BankName;
                        }
                        if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDueOnImport &&
                            btOrder.Payments.Count() == 1)
                        {
                            if (pricing != null)
                            {
                                payment.Amount = pricing.AmountDue;
                                payment.Currency =
                                    HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                            }
                        }
                    }

                    //OrderItems
                    foreach (var orderItem in btOrder.OrderItems)
                    {
                        var itemFromMail = orderForMail.Items.ToList().Find(oi => oi.SkuId == orderItem.SKU);
                        if (itemFromMail != null)
                        {
                            orderItem.EarnBase = itemFromMail.EarnBase;
                            orderItem.ItemDescription = itemFromMail.ItemDescription;
                            orderItem.LineTotal = itemFromMail.LineTotal;
                            orderItem.UnitPrice = itemFromMail.UnitPrice;
                            orderItem.VolumePoints = itemFromMail.VolumePoints;
                            orderItem.DistributorCost = itemFromMail.DistributorCost;
                            orderItem.Flavor = itemFromMail.Flavor;
                            orderItem.PriceWithCharges = itemFromMail.PriceWithCharges;
                            var catlogItem = CatalogProvider.GetCatalogItem(itemFromMail.SkuId, country);
                            orderItem.ProductType = Enum.GetName(typeof(ServiceProvider.CatalogSvc.ProductType), catlogItem.ProductType);
                            orderItem.UnitVolume = catlogItem.VolumePoints.ToString();
                        }
                    }

                    if (HLConfigManager.Configurations.DOConfiguration.HasMLMCheck)
                    {
                        var messages = btOrder.Messages.ToList();
                        var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                        messages.Add(new Message
                        {
                            MessageType = "TrainingExtendFlag",
                            MessageValue = sessionInfo.TrainingBreached.ToString()
                        });
                        btOrder.Messages = messages.ToArray();
                    }
                }

                cartInfo.CloseCart(true);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CreateOrder GetEmailFromOrder Failed. Exception error : {0}", ex.Message));
            }

            return btOrder;
        }



        public static object CreateOrder(Order_V01 order, MyHLShoppingCart shoppingCart, string countryCode)
        {
            return CreateOrder(order, shoppingCart, countryCode, null);
        }

        /// <summary>Create the BT Order object for this order</summary>
        /// <param name="order"></param>
        /// <param name="shoppingCart"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public static object CreateOrder(Order_V01 order,
                                         MyHLShoppingCart shoppingCart,
                                         string countryCode,
                                         ThreeDSecuredCreditCard threeDSecuredCreditCard, string source = null)
        {
            var orderTotal = shoppingCart.Totals as OrderTotals_V01;
            string cartID = shoppingCart.ShoppingCartID.ToString();

            if (order == null || orderTotal == null)
            {
                return null;
            }
            var btOrder = new ServiceProvider.SubmitOrderBTSvc.Order();
            var locale = Thread.CurrentThread.CurrentCulture.ToString();

            var currentSession = SessionInfo.GetSessionInfo(order.DistributorID, locale);
            if (currentSession != null && HLConfigManager.Configurations.DOConfiguration.IsChina && currentSession.IsReplacedPcOrder && currentSession.ReplacedPcDistributorOrderingProfile != null)
            {
                btOrder.DistributorID = currentSession.ReplacedPcDistributorOrderingProfile.Id;
            }
            else
            {
                btOrder.DistributorID = order.DistributorID;
            }

            btOrder.CountryOfProcessing = order.CountryOfProcessing;

            btOrder.OrderID = !String.IsNullOrEmpty(order.OrderID) ? order.OrderID : string.Empty;
            btOrder.ReferenceID = string.Concat(cartID, "-", Guid.NewGuid().ToString().Replace("-", string.Empty));
            btOrder.InputMethod = "IE";

            btOrder.OrderCategory = Enum.GetName(typeof(ServiceProvider.OrderSvc.OrderCategoryType), order.OrderCategory);
            if (btOrder.OrderCategory == "HSO")
            {
                switch (shoppingCart.HAPScheduleDay)
                {
                    case 4:
                        btOrder.HapScheduleCode = "A";
                        btOrder.HapSchedule = "4th";
                        break;
                    case 11:
                        btOrder.HapScheduleCode = "B";
                        btOrder.HapSchedule = "11th";
                        break;
                    case 18:
                        btOrder.HapScheduleCode = "C";
                        btOrder.HapSchedule = "18th";
                        break;
                }
                btOrder.OrderSubType = btOrder.HapTypeCode = shoppingCart.HAPType;
                btOrder.HapType = btOrder.HapTypeCode == "01" ? "Personal" : "Resale";
                shoppingCart.HAPAction = btOrder.HapAction = string.IsNullOrEmpty(order.OrderID) ? "CREATE" : "UPDATE";
                btOrder.HapExpiryDate = shoppingCart.HAPExpiryDate.ToString("MM/dd/yyyy");
                btOrder.IsHap = "true";
            }

            btOrder.ReceivedDate = order.ReceivedDate;
            btOrder.OrderMonth = order.OrderMonth;
            btOrder.CountryOfProcessing = order.CountryOfProcessing;
            btOrder.DiscountPercentage = order.DiscountPercentage;
            btOrder.CustomerID = string.Empty;
            btOrder.QualifyingSupervisorID = order.DistributorID;

            if (DistributorOrderingProfileProviderLoader == null)
                DistributorOrderingProfileProviderLoader = new DistributorOrderingProfileProviderLoader();

            btOrder.Shipment = setShipment(order);
            var provider = ShippingProvider.GetShippingProvider(null);
            if (provider != null)
            {
                provider.GetDistributorShippingInfoForHMS(shoppingCart, btOrder.Shipment);
            }

            try
            {
                if (btOrder.Shipment.WarehouseCode != ((ShippingInfo_V01)order.Shipment).WarehouseCode)
                {
                    ((ShippingInfo_V01)order.Shipment).WarehouseCode = btOrder.Shipment.WarehouseCode;
                    if (shoppingCart != null && shoppingCart.DeliveryInfo != null)
                    {
                        shoppingCart.DeliveryInfo.WarehouseCode = btOrder.Shipment.WarehouseCode;
                    }
                }
            }
            catch
            {

            }

            if (order.CountryOfProcessing == "CN")
            {
                orderTotal = shoppingCart.Totals as OrderTotals_V02; // in case order was re-priced
            }
            else
            {
                orderTotal = shoppingCart.Totals as OrderTotals_V01; // in case order was re-priced    
            }
            order.Pricing = orderTotal;
            btOrder.Pricing = setPricing(orderTotal);
            if (order.CountryOfProcessing == "HR")
            {
                var charge = getFee("OTHER", orderTotal.ChargeList);
                if (charge != null)
                {
                    btOrder.Pricing.TaxAmount += charge.Amount;
                }
            }
            btOrder.PackageInfo = createPackageInfo(order, shoppingCart);

            btOrder.OrderItems = convertLineItems(order, orderTotal, shoppingCart);
            if (order.CountryOfProcessing == "CN")
            {
                updateSKU(btOrder.OrderItems, order.CountryOfProcessing);
                //  btOrder.CustomerID = 
            }

            var handling = new Handling();
            var handlingV01 = order.Handling as HandlingInfo_V01;
            handling.IncludeInvoice = invoiceOptionConversion(handlingV01.IncludeInvoice);
            handling.PickupName = handlingV01.PickupName;
            handling.ShippingInstructions = handlingV01.ShippingInstructions;
            btOrder.Handling = handling;

            if (!(shoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO &&
                  HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket &&
                  shoppingCart.ShoppingCartItems != null && shoppingCart.ShoppingCartItems.Any() &&
                  shoppingCart.ShoppingCartItems.Sum(i => i.CatalogItem.ListPrice) == 0))
            {
                btOrder.Payments = setPayments(order.Payments, countryCode, shoppingCart.Locale, threeDSecuredCreditCard,
                                               false);
            }

            // MPC Fraud
            if (HLConfigManager.Configurations.CheckoutConfiguration.HoldPickupOrder)
            {
                if (btOrder.Payments != null && btOrder.Payments.Count() > 0)
                {
                    // TODO: Get MPC Frauf from Nuget
                    //MyHerbalife3.Core.DistributorProvider.DistributorLoader distributorLoader = new MyHerbalife3.Core.DistributorProvider.DistributorLoader();
                    //var distributorProfile = distributorLoader.Load(shoppingCart.DistributorID, countryCode);
                    var distributorProfile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID, countryCode);
                    if (distributorProfile != null)
                    {
                        shoppingCart.HoldCheckoutOrder = distributorProfile.IsMPCFraud.HasValue ? distributorProfile.IsMPCFraud.Value : false;
                        if (shoppingCart.HoldCheckoutOrder)
                        {
                            shoppingCart.FraudControlSessonId = string.Empty;
                            foreach (var p in btOrder.Payments)
                            {
                                p.UserInformation = "M";
                            }
                        }
                    }
                }
            }

            // Kount
            var distributorOrderingProfile = DistributorOrderingProfileProviderLoader.GetProfile(order.DistributorID, order.CountryOfProcessing);
            if (distributorOrderingProfile != null)
            {
                btOrder.ApplicationDate = distributorOrderingProfile.ApplicationDate.ToString();
                btOrder.SponsorId = distributorOrderingProfile.SponsorID;
            }
            btOrder.SessionId = shoppingCart.FraudControlSessonId;
            btOrder.Website = "GDO";

            //btOrder.Messages = getMessages(countryCode, ShoppingCart, order);
            getParametersByCountry(countryCode, btOrder, shoppingCart, order, source);
            btOrder.WebClientAuthenticationKey =
                WebClientAuthenticationProvider.GetWebClientAuthCode(btOrder.CountryOfProcessing);

            btOrder.EnableDeferredProcessing = HLConfigManager.Configurations.DOConfiguration.HasDeferredProcessing
                                                   ? "true"
                                                   : "false";
            btOrder.ServerName = Environment.MachineName;
            btOrder.Email = shoppingCart.EmailAddress;
            btOrder.Locale = shoppingCart.Locale;

            DistributorOrderConfirmation orderForMail = null;
            btOrder.SendEmail =
                     HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrder.ToString().ToLower();
            //if (btOrder.SendEmail == "true" || (HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrderForWire && btOrder.Payments.Any(x => x.PaymentCode == "WR")))
            {
                if (EmailHelperLoader == null)
                    EmailHelperLoader = new EmailHelperLoader();

                try
                {
                    orderForMail = EmailHelperLoader.GetEmailFromOrder(
                    order,
                    shoppingCart,
                    shoppingCart.Locale,
                    shoppingCart.DeliveryInfo.Address.Recipient,
                    shoppingCart.DeliveryInfo);

                    if (orderForMail != null)
                    {

                        if (btOrder.SendEmail == "false"
                            && HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrderForWire
                            && orderForMail.Payments[0] != null
                            && (orderForMail.Payments[0].CardType.ToUpper() == "WIRE"
                                || orderForMail.Payments[0].CardType.ToUpper() == "DEMANDDRAFT"))
                        {
                            btOrder.SendEmail =
                                    HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrderForWire.ToString
                                        ()
                                               .ToLower();
                        }

                        btOrder.Email = orderForMail.Distributor.Contact.Email;

                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            btOrder.SendEmail = (!string.IsNullOrEmpty(orderForMail.Distributor.Contact.Email) ||
                                                 !string.IsNullOrEmpty(shoppingCart.EmailAddress))
                                ? "true"
                                : "false";
                        }

                        // Email Info
                        var btOrderEmailInfo = new EmailInfo
                        {
                            DeliveryTimeEstimated = orderForMail.DeliveryTimeEstimated,
                            FirstName = orderForMail.Distributor.FirstName,
                            HffMessage = orderForMail.HFFMessage,
                            InvoiceOption = orderForMail.InvoiceOption,
                            LastName = orderForMail.Distributor.LastName,
                            MiddleName = orderForMail.Distributor.MiddleName,
                            PaymentOption = orderForMail.paymentOption,
                            PaymentType =
                                    orderForMail.Payments == null ||
                                    orderForMail.Payments[0].CardType.ToUpper() == "WIRE" ||
                                    orderForMail.Payments[0].CardType.ToUpper() == "DEMANDDRAFT"
                                        ? "Wire"
                                        : "CreditCard",
                            PickUpLocation = orderForMail.PickupLocation,
                            PurchaseType = orderForMail.PurchaseType,
                            ShippingMethod = orderForMail.Shipment.ShippingMethod,
                            SpecialInstructions = orderForMail.SpecialInstructions
                        };

                        btOrder.EmailInfo = btOrderEmailInfo;

                        // Pricing
                        if (order.CountryOfProcessing != "CN") // don't override for China
                        {
                            btOrder.Pricing.DiscountedItemsTotal =
                                orderForMail.TotalDiscountRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.LocalTaxCharge =
                                orderForMail.LocalTaxCharge.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.LogisticsCharge =
                                orderForMail.Logistics.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.IpiTax = orderForMail.IPI.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.IcmsTax = orderForMail.ICMS.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalMarketingFund =
                                orderForMail.MarketingFund.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TaxedNet = orderForMail.TaxedNet.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalProductRetail =
                                orderForMail.TotalProductRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalCollateralRetail =
                                orderForMail.TotalCollateralRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalPromotionalRetail =
                                orderForMail.TotalPromotionalRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.ItemsTotal = orderForMail.TotalRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.VolumePointsRate = orderForMail.VolumePointsRate;
                        }
                        GetMessagesForEmail(countryCode, order, shoppingCart, btOrder, orderForMail);
                        if (shoppingCart.Totals != null)
                        {
                            btOrder.PricingServerName = (shoppingCart.Totals as OrderTotals_V01).PricingServerName;
                        }

                        // Payment
                        if (btOrder.Payments != null)
                        {
                            foreach (var payment in btOrder.Payments)
                            {
                                var paymentFromMail = orderForMail.Payments.ToList().Find(oi => oi.Amount == payment.Amount);
                                if (paymentFromMail != null)
                                {
                                    payment.BankName = paymentFromMail.BankName;
                                }
                                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDueOnImport &&
                                    btOrder.Payments.Count() == 1)
                                {
                                    //var amount = GetOriginalConvertedAmount(payment.Amount, countryCode);
                                    //payment.Amount = amount;
                                    //payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                                    var totals = shoppingCart.Totals as OrderTotals_V01;
                                    if (totals != null)
                                    {
                                        payment.Amount = totals.AmountDue;
                                        payment.Currency =
                                            HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                                    }
                                }
                            }
                        }

                        //OrderItems
                        foreach (var orderItem in btOrder.OrderItems)
                        {
                            var itemFromMail = orderForMail.Items.ToList().Find(oi => oi.SkuId == orderItem.SKU);
                            if (itemFromMail != null)
                            {
                                orderItem.EarnBase = itemFromMail.EarnBase;
                                orderItem.ItemDescription = itemFromMail.ItemDescription;
                                orderItem.LineTotal = itemFromMail.LineTotal;
                                orderItem.UnitPrice = itemFromMail.UnitPrice;
                                orderItem.VolumePoints = itemFromMail.VolumePoints;
                                orderItem.DistributorCost = itemFromMail.DistributorCost;
                                orderItem.Flavor = itemFromMail.Flavor;
                                orderItem.PriceWithCharges = itemFromMail.PriceWithCharges;
                                var catlogItem = CatalogProvider.GetCatalogItem(itemFromMail.SkuId, countryCode);
                                orderItem.ProductType = Enum.GetName(typeof(ServiceProvider.CatalogSvc.ProductType), catlogItem.ProductType);
                                orderItem.UnitVolume = catlogItem.VolumePoints.ToString();
                            }
                        }
                        var CountryRequiringOrderHold = Settings.GetRequiredAppSetting("CountryRequiringOrderHold", string.Empty).Split(',');
                        if (CountryRequiringOrderHold.Contains(btOrder.CountryOfProcessing))
                        {
                            btOrder.PaymentStatus = "Processing";
                        }
                        if (HLConfigManager.Configurations.DOConfiguration.HasMLMCheck)
                        {
                            var messages = btOrder.Messages.ToList();
                            var sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                            messages.Add(new Message
                            {
                                MessageType = "TrainingExtendFlag",
                                MessageValue = sessionInfo.TrainingBreached.ToString()
                            });
                            btOrder.Messages = messages.ToArray();
                        }


                    }

                    if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.HSO)
                    {
                        btOrder.OrderSubType = btOrder.HapTypeCode;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("CreateOrder GetEmailFromOrder Failed. Exception error : {0}", ex.Message));
                }
                btOrder.SendEmail =
                     HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrder.ToString().ToLower();
                if (btOrder.SendEmail == "false" && (HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrderForWire && btOrder.Payments.Any(x => x.PaymentCode == "WR")))
                {
                    btOrder.SendEmail = "true";
                }
            }

            return btOrder;
        }


        private static void GetMessagesForEmail(string countryCode, Order_V01 order, MyHLShoppingCart shoppingCart,
                                                ServiceProvider.SubmitOrderBTSvc.Order btOrder,
                                                DistributorOrderConfirmation orderForMail)
        {
            var msggsss = btOrder.Messages.ToList();
            DateTime dt;
            string txtOM;
            string orderDateText = "20" + order.OrderMonth + "01";
            bool chkParsing = DateTime.TryParseExact(orderDateText, "yyyyMMdd", CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out dt);
            txtOM = chkParsing ? calculateOrderMonthstring(dt) : orderForMail.OrderMonth;
            msggsss.Add(new Message { MessageType = "Scheme", MessageValue = orderForMail.Scheme });

            if (shoppingCart != null && shoppingCart.DsType != null && shoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (distributorProfileModel != null && distributorProfileModel.Value != null && !string.IsNullOrEmpty(distributorProfileModel.Value.SubTypeCode))
                {
                    string levelType = HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                        string.Format("DisplayLevel_{0}", distributorProfileModel.Value.SubTypeCode)).ToString();

                    msggsss.Add(new Message { MessageType = "YourLevel", MessageValue = !string.IsNullOrEmpty(levelType) ? levelType : string.Empty });
                }
            }

            var lstShoppingCartItems = shoppingCart.ShoppingCartItems;
            var total = GetTotalEarnBase(lstShoppingCartItems);
            msggsss.Add(new Message { MessageType = "totalProductEarnBase", MessageValue = total.ToString() });

            msggsss.Add(new Message { MessageType = "orderMonth", MessageValue = txtOM });
            msggsss.Add(new Message
            {
                MessageType = "subTotal",
                MessageValue = (shoppingCart.Totals as OrderTotals_V01).TaxableAmountTotal.ToString()
            });
            if (null != btOrder.Pricing.FirstDonationAmount &&
                !string.IsNullOrEmpty(btOrder.Pricing.FirstDonationAmount))
            {
                msggsss.Add(new Message
                {
                    MessageType = "firstDonationAmount",
                    MessageValue = btOrder.Pricing.FirstDonationAmount
                });
            }

            if (!string.IsNullOrEmpty(shoppingCart.EmailValues.CurrentMonthVolume))
            {
                msggsss.Add(new Message
                {
                    MessageType = "orderMonthVolume",
                    MessageValue = shoppingCart.EmailValues.CurrentMonthVolume
                });
            }
            if (!string.IsNullOrEmpty(shoppingCart.EmailValues.RemainingVolume))
            {
                msggsss.Add(new Message
                {
                    MessageType = "remainingVal",
                    MessageValue = shoppingCart.EmailValues.RemainingVolume
                });
            }
            if (!string.IsNullOrWhiteSpace(shoppingCart.EmailValues.DistributorSubTotalFormatted))
            {
                msggsss.Add(new Message
                {
                    MessageType = "distributorSubTotal",
                    MessageValue = shoppingCart.EmailValues.DistributorSubTotalFormatted
                });
            }
            //var distributorProfile = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            //if (distributorProfile != null && distributorProfile.Value != null)
            //{
            //    if (distributorProfile.Value.ProcessingCountryCode == "US")
            //    {
            //        msggsss.Add(new Message
            //        {
            //            MessageType = "IsForeignPPV",
            //            MessageValue = "true"
            //        });
            //    }
            //}
            if (string.IsNullOrWhiteSpace(GetForeignPPVCountryCode(shoppingCart.DistributorID,shoppingCart.CountryCode)) && IsEligibleReceiptModelSkus(shoppingCart).Count() >0 && 
                shoppingCart.OrderCategory== ServiceProvider.CatalogSvc.OrderCategoryType.RSO )
            {
                msggsss.Add(new Message
                {
                    MessageType = "DisplayCreateReceipt",
                    MessageValue = "true"
                });
            }

            // added for China DO
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasDiscountAmount)
            {
                OrderTotals_V02 totals_V02 = shoppingCart.Totals as OrderTotals_V02;
                if (totals_V02 != null)
                {
                    msggsss.Add(new Message
                    {
                        MessageType = "promotionAmount",
                        MessageValue = totals_V02.DiscountAmount.ToString()
                    });
                }
            }
            if (!string.IsNullOrEmpty(btOrder.Pricing.FreightAmount.ToString()))
            {
                msggsss.Add(new Message
                {
                    MessageType = "freightAmount",
                    MessageValue = btOrder.Pricing.FreightAmount.ToString()
                });
            }
            if ("BG" == countryCode)
            {
                switch (shoppingCart.SelectedDSSubType)
                {
                    case "PC":
                        msggsss.Add(new Message { MessageType = "categorySubtype", MessageValue = "Лична консумация" });
                        break;
                    case "CO":
                        msggsss.Add(new Message { MessageType = "categorySubtype", MessageValue = "За препродажба" });
                        break;
                }
            }
            if ("CH" == countryCode && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null &&
                shoppingCart.DeliveryInfo.Address.Address != null && !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.Line4))
            {
                msggsss.Add(new Message { MessageType = "categorySubtype", MessageValue = shoppingCart.DeliveryInfo.Address.Address.Line4 });
            }
            if ("BE" == countryCode)
            {
                if (btOrder.Payments != null &&
                    btOrder.Payments[0] != null &&
                    btOrder.Payments[0].PaymentCode == "IO")
                {
                    msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = "MisterCash" });
                }
            }
            if (countryCode == "RS")
            {
                //add payment  codes info for email confirmation

                StringBuilder text = new StringBuilder();
                List<string> paymentGatewayLog = OrderProvider.GetPaymentGatewayLog(btOrder.OrderID, ServiceProvider.OrderSvc.PaymentGatewayLogEntryType.Response);
                if (null != paymentGatewayLog)
                {
                    string theOne = paymentGatewayLog.Find(i => i.Contains("QueryString: Agency:=NestPay"));
                    if (!string.IsNullOrEmpty(theOne))
                    {
                        NameValueCollection theResponse = GetRequestVariables(theOne);

                        // order ID
                        if (!string.IsNullOrEmpty(theResponse["oid"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "OrderIdForMail")
                                           .ToString()) + " " + theResponse["oid"] + "|");
                        }

                        // Authorization Code
                        if (!string.IsNullOrEmpty(theResponse["AuthCode"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "AuthCode")
                                           .ToString()) + " " + theResponse["AuthCode"] + "|");
                        }

                        // Payment Status
                        if (!string.IsNullOrEmpty(theResponse["Response"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "PaymentStatus")
                                           .ToString()) + " " + theResponse["Response"] + "|");
                        }

                        // Transaction Status Code
                        if (!string.IsNullOrEmpty(theResponse["ProcReturnCode"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "TransactionStatusCode")
                                           .ToString()) + " " + theResponse["ProcReturnCode"] + "|");
                        }

                        // Transaction ID
                        if (!string.IsNullOrEmpty(theResponse["TransId"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "TransactionId")
                                           .ToString()) + " " + theResponse["TransId"] + "|");
                        }

                        // Transaction Date
                        if (!string.IsNullOrEmpty(theResponse["EXTRA.TRXDATE"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "TransactionDate")
                                           .ToString()) + " " + theResponse["EXTRA.TRXDATE"] + "|");
                        }

                        // Status code for the 3D transaction
                        if (!string.IsNullOrEmpty(theResponse["mdStatus"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "Transaction3DStatus")
                                           .ToString()) + " " + theResponse["mdStatus"] + "|");
                        }
                    }
                    else
                    {
                        theOne = paymentGatewayLog.Find(i => i.Contains("result=CAPTURED"));
                        if (!string.IsNullOrEmpty(theOne))
                        {
                            NameValueCollection theResponse = GetRequestVariables(theOne);

                            // Authorization Code
                            if (!string.IsNullOrEmpty(theResponse["auth"]))
                            {

                                text.Append(string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "AutorizationCode")
                                               .ToString()) + " " + theResponse["auth"] + "|");
                            }

                            // Authorization Code
                            if (!string.IsNullOrEmpty(theResponse["paymentid"]))
                            {

                                text.Append(string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "PaymentCode")
                                               .ToString()) + " " + theResponse["paymentid"] + "|");
                            }

                            // Authorization Code
                            if (!string.IsNullOrEmpty(theResponse["tranid"]))
                            {
                                text.Append(string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "TransactionCode")
                                               .ToString()) + " " + theResponse["tranid"] + "|");
                            }
                        }
                    }
                    msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = text.ToString() });
                }
            }
            if ("UY" == countryCode)
            {
                //add details for the tax splitted 14 and 22 %
                //GetLocalTax = 14.3%
                string value =
                    GetLocalTax(shoppingCart.Totals as OrderTotals_V01)
                        .Amount.ToString("F", CultureInfo.InvariantCulture) + "|";

                //GetTaxAmount= 22%
                value += GetTaxAmount(shoppingCart.Totals as OrderTotals_V01)
                    .ToString("F", CultureInfo.InvariantCulture);

                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value });
            }

            if ("US" == countryCode || "PR" == countryCode || "CA" == countryCode)
            {
                string value = string.Empty;
                if (null != shoppingCart.DeliveryInfo &&
                    ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier == shoppingCart.DeliveryInfo.Option)
                {
                    value = shoppingCart.DeliveryInfo != null
                                                ? shoppingCart.DeliveryInfo.AdditionalInformation
                                                : string.Empty;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value });
                }
                if (btOrder.Handling.ShippingInstructions != null)
                    btOrder.EmailInfo.SpecialInstructions = btOrder.Handling.ShippingInstructions;

            }
            if ("CH" == countryCode)
            {
                var orderTotals_V01 = shoppingCart.Totals as OrderTotals_V01;

                decimal value;

                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                {
                    decimal AmountDue = 0;
                    if (order.Payments != null)
                    {
                        AmountDue = order.Payments.Sum(i => i.Amount);
                    }
                    value = AmountDue;
                }
                else
                {
                    value = orderTotals_V01.AmountDue;
                }

                msggsss.Add(new Message { MessageType = "promotionAmount", MessageValue = value.ToString(CultureInfo.InvariantCulture) });
            }
            if ("DO" == countryCode)
            {
                var orderTotals_V01 = shoppingCart.Totals as OrderTotals_V01;
                var value = OrderProvider.GetConvertedAmount(orderTotals_V01.AmountDue, "DO");
                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value.ToString() });
            }
            if ("HR" == countryCode)
            {
                var orderTotals_V01 = shoppingCart.Totals as OrderTotals_V01;
                Charge_V01 otherCharges =
                        orderTotals_V01.ChargeList.Find(
                            delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.OTHER; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.OTHER, (decimal)0.0);

                if (otherCharges != null)
                {
                    msggsss.Add(new Message { MessageType = "otherCharges", MessageValue = otherCharges.Amount.ToString() });
                }
            }
            //IN tax bifurcation
            if ("IN" == countryCode)
            {
                var orderTotals_V01 = shoppingCart.Totals as OrderTotals_V01;
                msggsss.Add(new Message { MessageType = "otherCharges", MessageValue = orderTotals_V01.VatTax != null ? orderTotals_V01.VatTax.ToString() : "0" });
                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = orderTotals_V01.ServiceTax != null ? orderTotals_V01.ServiceTax.ToString() : "0" });
                // Including taxes
                // Taxes will be paases to email in promotionAmount property in this order: SwachhBharatCess, KrishiKalyanCess, AdditionalTaxCess, CstTax
                var localTax = string.Format("{0}|", orderTotals_V01.SwachhBharatCess != null ? orderTotals_V01.SwachhBharatCess.Value.ToString() : "0");
                if (HL.Common.Configuration.Settings.GetRequiredAppSetting("KKCEnabled", false))
                {
                    localTax += orderTotals_V01.KrishiKalyanCess != null ? orderTotals_V01.KrishiKalyanCess.Value.ToString() : "0";
                }
                localTax = string.Format("{0}|{1}", localTax, orderTotals_V01.AdditionalTaxCess.HasValue ? orderTotals_V01.AdditionalTaxCess.Value.ToString() : string.Empty);
                localTax = string.Format("{0}|{1}", localTax, orderTotals_V01.CstTax.HasValue ? orderTotals_V01.CstTax.Value.ToString() : string.Empty);
                msggsss.Add(new Message { MessageType = "promotionAmount", MessageValue = localTax });
            }

            if (countryCode == "PH")
            {
                var weight = ShoppingCartProvider.GetWeight(shoppingCart);
                if (!string.IsNullOrWhiteSpace(weight))
                {
                    msggsss.Add(new Message
                    {
                        MessageType = "aditionalInformation",
                        MessageValue = ShoppingCartProvider.GetWeight(shoppingCart)
                    });
                }
            }

            if (countryCode == "AR")
            {
                
                if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null && shoppingCart.DeliveryInfo.Address.Address != null)
                {
                    int stateCode = 0;
                    if (int.TryParse(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory, out stateCode) && stateCode > 0)
                    {
                        var shippingProvider = new ShippingProvider_AR();
                        // Get Shipping State name from StateCode
                        var stateValue = shippingProvider.GetStateNameFromStateCode(stateCode.ToString());
                        msggsss.Add(new Message
                        {
                            MessageType = "aditionalInformation",
                            MessageValue = stateValue
                        });
                    }
                    else
                    {
                        msggsss.Add(new Message
                        {
                            MessageType = "aditionalInformation",
                            MessageValue = shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory
                        });
                    }
                }
            }

            btOrder.Messages = msggsss.ToArray();
        }


        private static void GetMessagesForEmailForMobile(string countryCode, Order_V01 order, MyHLShoppingCart shoppingCart,
                                                MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Order btOrder,
                                                DistributorOrderConfirmation orderForMail)
        {
            var msggsss = btOrder.Messages.ToList();
            var pricing = order.Pricing as OrderTotals_V01;
            DateTime dt;
            string txtOM;
            string orderDateText = "20" + order.OrderMonth + "01";
            bool chkParsing = DateTime.TryParseExact(orderDateText, "yyyyMMdd", CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out dt);
            txtOM = chkParsing ? calculateOrderMonthstring(dt) : orderForMail.OrderMonth;

            var lstShoppingCartItems = shoppingCart.ShoppingCartItems;

            msggsss.Add(new Message { MessageType = "totalProductEarnBase", MessageValue = orderForMail.TotalProductEarnBase.ToString() });

            msggsss.Add(new Message { MessageType = "orderMonth", MessageValue = txtOM });
            msggsss.Add(new Message
            {
                MessageType = "subTotal",
                MessageValue = pricing.TaxableAmountTotal.ToString()
            });


            if (!string.IsNullOrEmpty(pricing.VolumePoints.ToString()))
            {
                msggsss.Add(new Message
                {
                    MessageType = "orderMonthVolume",
                    MessageValue = pricing.VolumePoints.ToString()
                });
            }
            if (!string.IsNullOrEmpty(orderForMail.RemainingValue))
            {
                msggsss.Add(new Message
                {
                    MessageType = "remainingVal",
                    MessageValue = orderForMail.RemainingValue
                });
            }
            if (!string.IsNullOrWhiteSpace(orderForMail.SubTotal.ToString()))
            {
                msggsss.Add(new Message
                {
                    MessageType = "distributorSubTotal",
                    MessageValue = orderForMail.SubTotal.ToString()
                });
            }

            if (!string.IsNullOrEmpty(btOrder.Pricing.FreightAmount.ToString()))
            {
                msggsss.Add(new Message
                {
                    MessageType = "freightAmount",
                    MessageValue = btOrder.Pricing.FreightAmount.ToString()
                });
            }

            btOrder.Messages = msggsss.ToArray();
        }

        private static void GetMessagesForEmail_V02(string countryCode, Order_V02 order, ServiceProvider.SubmitOrderBTSvc.Order btOrder, DistributorOrderConfirmation orderForMail)
        {
            var msggsss = btOrder.Messages.ToList();
            var pricing = order.Pricing as OrderTotals_V01;
            var shipping = order.Shipment as ShippingInfo_V01;
            DateTime dt;
            string txtOM;
            string orderDateText = "20" + order.OrderMonth + "01";
            bool chkParsing = DateTime.TryParseExact(orderDateText, "yyyyMMdd", CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out dt);
            txtOM = chkParsing ? calculateOrderMonthstring(dt) : orderForMail.OrderMonth;
            msggsss.Add(new Message { MessageType = "Scheme", MessageValue = orderForMail.Scheme });

            var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(order.DistributorID, order.CountryOfProcessing);
            if (DistributorType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (distributorProfileModel != null && distributorProfileModel.Value != null && !string.IsNullOrEmpty(distributorProfileModel.Value.SubTypeCode))
                {
                    string levelType = HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                        string.Format("DisplayLevel_{0}", distributorProfileModel.Value.SubTypeCode)).ToString();

                    msggsss.Add(new Message { MessageType = "YourLevel", MessageValue = !string.IsNullOrEmpty(levelType) ? levelType : string.Empty });
                }
            }

            msggsss.Add(new Message
            {
                MessageType = "totalProductEarnBase",
                MessageValue = orderForMail.TotalProductEarnBase.ToString()
            });
            msggsss.Add(new Message { MessageType = "orderMonth", MessageValue = txtOM });
            msggsss.Add(new Message
            {
                MessageType = "subTotal",
                MessageValue = pricing.TaxableAmountTotal.ToString()
            });
            if (null != btOrder.Pricing.FirstDonationAmount &&
                !string.IsNullOrEmpty(btOrder.Pricing.FirstDonationAmount))
            {
                msggsss.Add(new Message
                {
                    MessageType = "firstDonationAmount",
                    MessageValue = btOrder.Pricing.FirstDonationAmount
                });
            }

            // TODO: Get values from Order_V02 object
            //if (!string.IsNullOrEmpty(shoppingCart.EmailValues.CurrentMonthVolume))
            //{
            //    msggsss.Add(new Message
            //    {
            //        MessageType = "orderMonthVolume",
            //        MessageValue = shoppingCart.EmailValues.CurrentMonthVolume
            //    });
            //}
            //if (!string.IsNullOrEmpty(shoppingCart.EmailValues.RemainingVolume))
            //{
            //    msggsss.Add(new Message
            //    {
            //        MessageType = "remainingVal",
            //        MessageValue = shoppingCart.EmailValues.RemainingVolume
            //    });
            //}
            //if (!string.IsNullOrWhiteSpace(shoppingCart.EmailValues.DistributorSubTotalFormatted))
            //{
            //    msggsss.Add(new Message
            //    {
            //        MessageType = "distributorSubTotal",
            //        MessageValue = shoppingCart.EmailValues.DistributorSubTotalFormatted
            //    });
            //}

            // added for China DO
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasDiscountAmount)
            {
                OrderTotals_V02 totals_V02 = order.Pricing as OrderTotals_V02;
                if (totals_V02 != null)
                {
                    msggsss.Add(new Message
                    {
                        MessageType = "promotionAmount",
                        MessageValue = totals_V02.DiscountAmount.ToString()
                    });
                }
            }
            if (!string.IsNullOrEmpty(btOrder.Pricing.FreightAmount.ToString()))
            {
                msggsss.Add(new Message
                {
                    MessageType = "freightAmount",
                    MessageValue = btOrder.Pricing.FreightAmount.ToString()
                });
            }

            if (countryCode == "RS")
            {
                //add payment  codes info for email confirmation

                StringBuilder text = new StringBuilder();
                List<string> paymentGatewayLog = OrderProvider.GetPaymentGatewayLog(btOrder.OrderID, PaymentGatewayLogEntryType.Response);
                if (null != paymentGatewayLog)
                {
                    string theOne = paymentGatewayLog.Find(i => i.Contains("result=CAPTURED"));
                    if (!string.IsNullOrEmpty(theOne))
                    {
                        NameValueCollection theResponse = GetRequestVariables(theOne);

                        // Authorization Code
                        if (!string.IsNullOrEmpty(theResponse["auth"]))
                        {

                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "AutorizationCode")
                                           .ToString()) + " " + theResponse["auth"] + "|");
                        }

                        // Authorization Code
                        if (!string.IsNullOrEmpty(theResponse["paymentid"]))
                        {

                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "PaymentCode")
                                           .ToString()) + " " + theResponse["paymentid"] + "|");
                        }

                        // Authorization Code
                        if (!string.IsNullOrEmpty(theResponse["tranid"]))
                        {
                            text.Append(string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "TransactionCode")
                                           .ToString()) + " " + theResponse["tranid"] + "|");
                        }
                    }
                    msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = text.ToString() });
                }
            }
            if ("UY" == countryCode)
            {
                //add details for the tax splitted 14 and 22 %
                //GetLocalTax = 14.3%
                string value =
                    GetLocalTax(pricing)
                        .Amount.ToString("F", CultureInfo.InvariantCulture) + "|";

                //GetTaxAmount= 22%
                value += GetTaxAmount(pricing)
                    .ToString("F", CultureInfo.InvariantCulture);

                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value });
            }

            if ("US" == countryCode || "PR" == countryCode || "CA" == countryCode)
            {
                string value = string.Empty;

                // TODO: Get values from Order_V02 object
                //if (null != shoppingCart.DeliveryInfo &&
                //    DeliveryOptionType.PickupFromCourier == shoppingCart.DeliveryInfo.Option)
                //{
                //    value = shoppingCart.DeliveryInfo != null
                //                                ? shoppingCart.DeliveryInfo.AdditionalInformation
                //                                : string.Empty;
                //}
                if (!string.IsNullOrEmpty(value))
                {
                    msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value });
                }
                if (btOrder.Handling.ShippingInstructions != null)
                    btOrder.EmailInfo.SpecialInstructions = btOrder.Handling.ShippingInstructions;

            }
            if ("CH" == countryCode)
            {
                decimal value;

                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                {
                    decimal AmountDue = 0;
                    if (order.Payments != null)
                    {
                        AmountDue = order.Payments.Sum(i => i.Amount);
                    }
                    value = AmountDue;
                }
                else
                {
                    value = pricing.AmountDue;
                }

                msggsss.Add(new Message { MessageType = "promotionAmount", MessageValue = value.ToString(CultureInfo.InvariantCulture) });
            }
            if ("DO" == countryCode)
            {
                var value = OrderProvider.GetConvertedAmount(pricing.AmountDue, "DO");
                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = value.ToString() });
            }
            if ("HR" == countryCode)
            {
                Charge_V01 otherCharges =
                        pricing.ChargeList.Find(
                            delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.OTHER; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.OTHER, (decimal)0.0);

                if (otherCharges != null)
                {
                    msggsss.Add(new Message { MessageType = "otherCharges", MessageValue = otherCharges.Amount.ToString() });
                }
            }
            //IN tax bifurcation
            if ("IN" == countryCode)
            {
                msggsss.Add(new Message { MessageType = "otherCharges", MessageValue = pricing.VatTax != null ? pricing.VatTax.ToString() : "0" });
                msggsss.Add(new Message { MessageType = "aditionalInformation", MessageValue = pricing.ServiceTax != null ? pricing.ServiceTax.ToString() : "0" });
                // Including taxes
                // Taxes will be paases to email in promotionAmount property in this order: SwachhBharatCess, KrishiKalyanCess, AdditionalTaxCess, CstTax
                var localTax = string.Format("{0}|", pricing.SwachhBharatCess != null ? pricing.SwachhBharatCess.Value.ToString() : "0");
                if (HL.Common.Configuration.Settings.GetRequiredAppSetting("KKCEnabled", false))
                {
                    localTax += pricing.KrishiKalyanCess != null ? pricing.KrishiKalyanCess.Value.ToString() : "0";
                }
                localTax = string.Format("{0}|{1}", localTax, pricing.AdditionalTaxCess.HasValue ? pricing.AdditionalTaxCess.Value.ToString() : string.Empty);
                localTax = string.Format("{0}|{1}", localTax, pricing.CstTax.HasValue ? pricing.CstTax.Value.ToString() : string.Empty);
                msggsss.Add(new Message { MessageType = "promotionAmount", MessageValue = localTax });
            }

            btOrder.Messages = msggsss.ToArray();
        }

        public static decimal GetTotalEarnBase(List<DistributorShoppingCartItem> items)
        {
            // If total earn base should only consider product type P.
            var countryCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var config = (new NPSConfigurationProvider()).GetNPSConfigSection(countryCode);

            if (config != null && config.TotalEarnBasePrdctTypeOnly)
            {
                var startDate = config.TotalEarnBasePrdctTypeOnlyDate;
                if (!string.IsNullOrEmpty(startDate))
                {
                    const string format = "yyyy-MM-dd";

                    DateTime startDateTime;

                    if (DateTime.TryParseExact(startDate, format, CultureInfo.InvariantCulture,
                                               DateTimeStyles.None, out startDateTime))
                    {
                        if (DateUtils.GetCurrentLocalTime(countryCode).Date >= startDateTime.Date)
                        {
                            return
                                items.Where(
                                    shoppingCartItem => shoppingCartItem.CatalogItem != null && shoppingCartItem.CatalogItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                                     .Sum(shoppingCartItem => shoppingCartItem.EarnBase);
                        }
                    }
                }
            }

            return items.Sum(shoppingCartItem => shoppingCartItem.EarnBase);
        }

        private static NameValueCollection GetRequestVariables(string requestData)
        {
            NameValueCollection result = new NameValueCollection();
            List<string> items = new List<string>(requestData.Split(new char[] { '&' }));
            foreach (string item in items)
            {
                string[] elements = item.Split(new char[] { '=' });
                if (elements.Length == 2)
                {
                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the order monthstring.
        /// </summary>
        /// <param name="ordermonth">The ordermonth.</param>
        /// <returns></returns>
        private static string calculateOrderMonthstring(DateTime ordermonth)
        {
            var config = HLConfigManager.Configurations.DOConfiguration;
            //Calculate the original value
            var omString = HLConfigManager.Configurations.DOConfiguration.OrderMonthFormatLocalProvider
                               ? ordermonth.ToString(config.OrderMonthFormat,
                                                     Thread.CurrentThread.CurrentCulture.DateTimeFormat)
                               : ordermonth.ToString(config.OrderMonthFormat, CultureInfo.InvariantCulture);

            if (HLConfigManager.Configurations.DOConfiguration.UseGregorianCalendar)
            {
                var gregorianYear = DateTime.Now.Year;
                if (!omString.Contains(gregorianYear.ToString()))
                {
                    var lenght = omString.Length - 4;
                    string monthname;

                    double Num;
                    //validate if the format is MMMM yyyy
                    bool isNum = double.TryParse(omString.Substring(lenght), out Num);
                    if (isNum)
                    {
                        monthname = omString.Substring(0, lenght) + gregorianYear;
                    }
                    //format yyyy MMMM
                    else
                    {
                        monthname = gregorianYear + omString.Substring(4);
                    }

                    omString = monthname;
                    //omString = omString.Substring(0, lenght)+ gregorianYear;
                }
            }
            return omString;
        }

        /// <summary>Serialize a BTOrder to a string</summary>
        /// <param name="btOrderObject"></param>
        /// <returns></returns>
        public static string SerializeOrder(object btOrderObject, ServiceProvider.OrderSvc.Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken)
        {
            SerializedOrderHolder holder = GetSerializedOrderHolder(btOrderObject, _order, shoppingCart, authenticationToken);
            return OrderSerializer.SerializeOrder(holder);
        }


        public static ServiceProvider.OrderSvc.Payment GetBasePayment(string transcationType)
        {
            var cc = new CreditCard();
            cc.IssuerAssociation = CreditCard.GetCardType("VI");
            cc.AccountNumber = PaymentInfoProvider.VisaCardNumber;
            cc.CVV = "123";
            cc.Expiration = new DateTime(2020, 12, 31);
            cc.NameOnCard = "Test Card";
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayPayCode))
            {
                cc.IssuerAssociation = CreditCard.GetCardType(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayPayCode);
            }

            var cp = new CreditPayment_V01();
            cp.AuthorizationMethod = AuthorizationMethodType.PaymentGateway;
            cp.TransactionType = HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayAlias;
            cp.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
            var options = new PaymentOptions_V01();
            options.NumberOfInstallments = 1;
            cp.PaymentOptions = options;
            cp.TransactionType = transcationType;
            cp.AuthorizationCode = "654321";
            cp.TransactionID = Guid.NewGuid().ToString();
            cp.Card = cc;
            cp.Address = new ServiceProvider.OrderSvc.Address_V01();
            return cp;
        }

        /// <summary>Serialize a BTOrder to a string</summary>
        /// <param name="btOrderObject"></param>
        /// <returns></returns>
        public static SerializedOrderHolder GetSerializedOrderHolder(object btOrderObject, ServiceProvider.OrderSvc.Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken)
        {
            //var order = _order as Order_V01;
            (_order as Order_V01).Pricing = shoppingCart.Totals;
            var holder = new SerializedOrderHolder(btOrderObject as ServiceProvider.SubmitOrderBTSvc.Order, _order);
            holder.Email = shoppingCart.EmailAddress;
            holder.DistributorId = shoppingCart.DistributorID;
            holder.Token = authenticationToken;
            holder.Locale = shoppingCart.Locale;
            holder.ShoppingCartId = shoppingCart.ShoppingCartID;
            if (_order as ServiceProvider.OrderSvc.OnlineOrder != null)
            {
                holder.OrderHeaderId = (_order as ServiceProvider.OrderSvc.OnlineOrder).OrderHeaderID;
            }
            return holder;
        }

        /// <summary>
        /// deSerializeAndSubmitOrder
        /// </summary>
        /// <param name="response"></param>
        /// <param name="error"></param>
        /// <param name="holder"></param>
        /// <returns></returns>
        public static bool deSerializeAndSubmitOrder(PaymentGatewayResponse response, out string error, out SerializedOrderHolder holder)
        {
            error = string.Empty;
            var failedCards = new List<FailedCardInfo>();
            try
            {
                string orderId = string.Empty;
                string orderNumber = (response.OrderNumber.Length > 10) ? response.OrderNumber.Substring(0, 10) : response.OrderNumber;
                if (null != response.PGHOrderHolder)
                {
                    holder = response.PGHOrderHolder;
                }
                else
                {
                    holder = GetPaymentGatewayOrder(orderNumber);
                }
                //This check is for multiple out-of-band posts that sometimes happen
                if (response.Status == PaymentGatewayRecordStatusType.OrderSubmitted)
                {
                    PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Error, orderNumber, holder.DistributorId, string.Empty, PaymentGatewayRecordStatusType.OrderSubmitted, "Gateway already posted this order and it was submitted");
                }
                else
                {
                    holder.Order.OrderID = holder.BTOrder.OrderID = response.OrderNumber;
                    //PGH already takes care of these
                    if (null == response as PGHPaymentGatewayResponse)
                    {
                        if (holder.Locale.Trim().ToUpper() == "ZH-CN" && response != null)
                        {
                            //if (holder != null && holder.BTOrder != null && holder.BTOrder.Payments != null &&
                            //    holder.BTOrder.Payments.Any())
                            //{
                            //    var gatewayAmount = string.Empty;
                            //    if (response as CN_99BillPaymentGatewayResponse != null)
                            //    {
                            //        gatewayAmount = ((CN_99BillPaymentGatewayResponse)response).GatewayAmount;
                            //    }
                            //    else if (response as CN_99BillCNPResponse != null)
                            //    {
                            //        gatewayAmount = ((CN_99BillCNPResponse) response).GatewayAmount;
                            //    }
                            //    if (gatewayAmount != holder.BTOrder.Payments.FirstOrDefault().Amount.ToString())
                            //    {
                            //        var logs = GetPaymentGatewayLog(orderNumber, PaymentGatewayLogEntryType.Request);
                            //        if (logs != null && logs.Any())
                            //        {
                            //            var sortedList =
                            //                logs.Where(log => log.Contains("SerializedOrderHolder")).ToList();
                            //            if (sortedList != null && sortedList.Any())
                            //            {
                            //                var tempPaymentRecord = sortedList.LastOrDefault();
                            //                holder = OrderSerializer.DeSerializeOrder(tempPaymentRecord);
                            //                holder.Order.OrderID = holder.BTOrder.OrderID = response.OrderNumber;
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        //For gateways that return card info
                        response.GetPaymentInfo(holder);
                    }
                    if (response.Status == PaymentGatewayRecordStatusType.ApprovalPending && HLConfigManager.Configurations.PaymentsConfiguration.CanSubmitPending)
                    {
                        holder.BTOrder.PaymentStatus = "Processing";
                        if (!string.IsNullOrEmpty(holder.Order.OrderID) && holder.Order.OrderID.ToLower().Contains("-ssl-pend"))
                        {
                            holder.Order.OrderID = holder.BTOrder.OrderID = holder.Order.OrderID.Split('-')[0];
                        }
                    }
                    if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayTransactionTime)
                    {
                        holder.BTOrder.ReceivedDate = DateTime.Parse(DateUtils.GetCurrentLocalTime(holder.Locale).ToString());
                    }
                    try
                    {
                        orderId = ImportOrder(holder.BTOrder, out error, out failedCards, holder.ShoppingCartId);

                        if (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(orderId))
                        {
                            PaymentGatewayInvoker.LogMessageWithInfo(PaymentGatewayLogEntryType.OrderSubmitted, orderId, holder.DistributorId, string.Empty, PaymentGatewayRecordStatusType.OrderSubmitted);
                        }
                        else
                        {
                            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Error, orderId, holder.DistributorId, string.Empty, PaymentGatewayRecordStatusType.InError, error);
                        }
                    }
                    catch (Exception ex)
                    {
                        PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Error, orderId, holder.DistributorId, string.Empty, PaymentGatewayRecordStatusType.InError, string.Format("BT Error: {0}\r\nException: {1}", error, ex.Message));
                    }
                }

                return (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(orderId));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("deSerializeAndSubmitOrder error : {0}", ex.Message));
            }
            holder = null;
            return false;
        }

        /// <summary>Retrieve and deserialize a PaymentGatewayRecord </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        public static SerializedOrderHolder GetPaymentGatewayOrder(string orderNumber)
        {
            string serializedOrder = GetPaymentGatewayRecord(orderNumber);
            if (serializedOrder != null && !serializedOrder.Contains("</d2p1:volumePointsField><d2p1:warehouseCodeField>") && Settings.GetRequiredAppSetting("EnsureWarehousecode") == "true")
            {
                serializedOrder = serializedOrder.Replace("</d2p1:volumePointsField>", "</d2p1:volumePointsField><d2p1:warehouseCodeField></d2p1:warehouseCodeField>");
            }
            return serializedOrder != null ? OrderSerializer.DeSerializeOrder(serializedOrder) : null;
        }

        public static void UpdateTWSKUOrderedQuantity(object btOrderObject)
        {
            try
            {
                var btOrder = btOrderObject as ServiceProvider.SubmitOrderBTSvc.Order;
                if (btOrder.Locale == "zh-TW")
                {
                    SKUQuantityRestrictRequest_V01 req = new SKUQuantityRestrictRequest_V01();
                    req.CountryCode = btOrder.CountryOfProcessing;
                    req.Flag = "R";
                    var list = OrderProvider.GetOrUpdateSKUsMaxQuantity(req);
                    if (list != null && list.Count > 0 && btOrder.OrderItems != null && btOrder.OrderItems.Count() > 0)
                    {
                        var result = from sku in list
                                     join skuordered in btOrder.OrderItems
                                     on sku.SKU equals skuordered.SKU
                                     where sku.WarehouseCode.Trim() == skuordered.WarehouseCode.Trim()
                                     select new { sku.SKU, skuordered.Quantity, skuordered.WarehouseCode };
                        if (result != null && result.Count() > 0)
                        {
                            foreach (var sku in result)
                            {
                                req.Flag = "U";
                                req.SKU = sku.SKU;
                                req.QuantityOrdered = sku.Quantity;
                                req.WarehouseCode = sku.WarehouseCode;
                                OrderProvider.GetOrUpdateSKUsMaxQuantity(req);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("UpdateTWSKUOrderedQuantity error : {0}", ex.Message));
            }
        }

        /// <summary>
        /// ImportOrder
        /// </summary>
        /// <param name="btOrderObject"></param>
        /// <param name="error"></param>
        /// <param name="failedCards"></param>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public static string ImportOrder(object btOrderObject, out string error, out List<FailedCardInfo> failedCards, int cartId)
        {
            failedCards = new List<FailedCardInfo>();
            error = string.Empty;
            var btOrder = btOrderObject as ServiceProvider.SubmitOrderBTSvc.Order;
            string distributorId = btOrder.DistributorID;
            bool isPendingFControl = false;

            if (string.IsNullOrEmpty(btOrder.ReferenceID))
                btOrder.ReferenceID = string.Concat(cartId, "-", Guid.NewGuid().ToString().Replace("-", string.Empty));

            var proxy = ServiceClientProvider.GetSubmitOrderProxy();
            try
            {
                LoggerTempWireup.WriteInfo(OrderSerialization(btOrder, btOrder.ReferenceID), "Checkout");
                var response = proxy.ProcessRequest(new ProcessRequestRequest() { Order = btOrder }).Response;
                if (response != null && !string.IsNullOrEmpty(response.OrderID))
                {
                    UpdateTWSKUOrderedQuantity(btOrderObject);
                }
                var currentSession = SessionInfo.GetSessionInfo(btOrder.DistributorID, btOrder.Locale);
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && currentSession != null)
                {
                    currentSession.OrderQueryStatus99BillInprocess = false;
                }
                string status = response.Status.ToUpper(CultureInfo.InvariantCulture);
                if (status.Equals("SUCCESS") || status.Equals("DUPLICATE-TOTAL-MATCH"))
                {
                    btOrder.OrderID = response.OrderID;
                    if (btOrder.CountryOfProcessing == "US" && !string.IsNullOrEmpty(response.PayNearMeId) && currentSession != null)
                    {
                        currentSession.LocalPaymentId = response.PayNearMeId;
                        currentSession.TrackingUrl = response.PayNearMeTrackingUrl;
                    }

                    //Kount Review
                    if (!string.IsNullOrEmpty(response.PaymentStatus) && response.PaymentStatus.Trim().ToUpper(CultureInfo.InvariantCulture) == "KOUNTREVIEW")
                    {
                        error = "KOUNTREVIEW";
                    }

                    //Deferred Processing
                    if (!string.IsNullOrEmpty(response.PaymentStatus) && response.PaymentStatus.Trim().ToUpper(CultureInfo.InvariantCulture) == "PROCESSING")
                    {
                        error = "PROCESSING";
                    }
                    if (btOrder.CountryOfProcessing == "CN")
                    {
                        btOrder.PaymentStatus = response.PaymentStatus;

                        var payment = btOrder.Payments.FirstOrDefault();

                        //Pay by Phone
                        if (payment != null && payment.PaymentCode == "DB")
                        {
                            DistributorOrderingProfile dop = DistributorOrderingProfileProvider.GetProfile(btOrder.DistributorID,
                                                                                           CultureInfo.CurrentCulture
                                                                                                      .Name.Substring(3));
                            bool isLockedeach = true;
                            bool isLocked = true;
                            string lockfailed = string.Empty;

                            if ((dop != null && dop.IsPC) || btOrder.OrderCategory == "ETO")
                            {
                                var PCLearningOffSet =
                                    btOrder.Messages.ToList()
                                           .Find(oi => oi.MessageType.ToLower() == "pcLearningPointOffSet".ToLower());
                                decimal point;
                                decimal.TryParse(PCLearningOffSet.MessageValue.ToString(), out point);
                                if (point > 0 && btOrder.OrderCategory != "ETO")
                                {
                                    isLockedeach = OrderProvider.LockPCLearningPoint(
                                        btOrder.DistributorID,
                                        btOrder.OrderID,
                                        btOrder.OrderMonth,
                                        Convert.ToInt32(Math.Truncate(point)),
                                        HLConfigManager.Platform);

                                    if (!isLockedeach)
                                    {
                                        lockfailed = "PC Learning Point";
                                        isLocked = false;
                                    }
                                }
                                else if (point > 0)
                                {
                                    var etoOrder = btOrder.OrderItems.FirstOrDefault();
                                    if (etoOrder != null)
                                    {

                                        isLockedeach = LockETOLearningPoint(
                                            etoOrder.SKU,
                                            btOrder.DistributorID,
                                            btOrder.OrderID,
                                            btOrder.OrderMonth,
                                            Convert.ToInt32(Math.Truncate(point)),
                                            HLConfigManager.Platform);
                                    }
                                    else
                                    {
                                        isLockedeach = false;
                                    }
                                    if (!isLockedeach)
                                    {
                                        lockfailed = "ETO Learning Point";
                                        isLocked = false;
                                    }
                                }
                            }
                            var shoppingcart = ShoppingCartProvider.GetShoppingCart(btOrder.DistributorID, btOrder.Locale, true, false);
                            if (shoppingcart.HastakenSrPromotion)
                            {
                                isLockedeach = ChinaPromotionProvider.LockSRPromotion(shoppingcart);
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", SR Promotion";
                                    isLocked = false;
                                }
                            }
 
                            if (shoppingcart.HastakenSrPromotionGrowing)
                            {
                                isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(shoppingcart);
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", SR Query Growing";
                                    isLocked = false;
                                }
                            }
                            if (shoppingcart.HastakenSrPromotionExcelnt)
                            {
                                isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(shoppingcart);
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", SR Query Excellent";
                                    isLocked = false;
                                }
                            }
                            if (shoppingcart.HastakenBadgePromotion)
                            {
                                isLockedeach = ChinaPromotionProvider.LockBadgePromotion(shoppingcart);
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", Badge promotion";
                                    isLocked = false;
                                }
                            }
                            if (shoppingcart.HastakenNewSrpromotion)
                            {
                                isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(shoppingcart, "");
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", NewSrPromotion";
                                    isLocked = false;
                                }
                            }

                            if (shoppingcart.HasBrochurePromotion)
                            {
                                isLockedeach= ChinaPromotionProvider.LockBrochurePromotion(shoppingcart);
                                if (!isLockedeach)
                                {
                                    lockfailed = lockfailed + ", Brochure Promotion";
                                    isLocked = false;
                                }
                            }
                               
                            if (!isLocked)
                            {
                                error = lockfailed.TrimStart(',') + " locking fails.";
                            }
                        }
                    }
                }
                else
                {
                    if (response.ResponsePayments != null)
                    {
                        if (response.ResponsePayments.Length > 0)
                        {
                            for (int count = 0; count < response.ResponsePayments.Length; count++)
                            {
                                if (string.IsNullOrEmpty(response.ResponsePayments[count].Status))
                                {
                                    continue;
                                }
                                if (
                                    response.ResponsePayments[count].Status.Trim().ToUpper(CultureInfo.InvariantCulture) == "KOUNT")
                                {
                                    status = "KOUNTDECLINED";
                                }
                                if (
                                    response.ResponsePayments[count].Status.Trim().ToUpper(CultureInfo.InvariantCulture) == "DECLINED")
                                {
                                    var info = new FailedCardInfo();
                                    info.Amount = response.ResponsePayments[count].Amount;
                                    info.CardNumber = response.ResponsePayments[count].AccountNumber;
                                    info.CardType = response.ResponsePayments[count].Paycode;
                                    failedCards.Add(info);
                                }

                                if (response.CountryOfProcessing == "BR")
                                {
                                    if (
                                        response.ResponsePayments[count].Status.Trim().ToUpper(CultureInfo.InvariantCulture) == "DECLINED-FCONTROL")
                                    {
                                        var info = new FailedCardInfo();
                                        info.Amount = response.ResponsePayments[count].Amount;
                                        info.CardNumber = response.ResponsePayments[count].AccountNumber;
                                        info.CardType = response.ResponsePayments[count].Paycode;
                                        failedCards.Add(info);
                                        status = "DECLINED-FCONTROL";
                                    }
                                    if (
                                        response.ResponsePayments[count].Status.Trim().ToUpper(CultureInfo.InvariantCulture).StartsWith("PENDING-FCONTROL"))
                                    {
                                        btOrder.OrderID = response.ResponsePayments[count].Status.Substring(17);
                                        isPendingFControl = true;
                                    }
                                }
                            }
                        }
                    }
                    error =
                        string.Format("[{3}] DistributorID :{0}, CartID :{1}, error: {2}", distributorId, cartId.ToString(), response.Message, status).ToUpper(CultureInfo.InvariantCulture);
                    LoggerHelper.Error(error);
                    if (isPendingFControl)
                    {
                        error = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                string msg =
                    error =
                    string.Format("TIMEOUT:DistributorID :{0}, CartID :{1}, error: {2}", distributorId,
                                  cartId.ToString(), ex);
                LoggerHelper.Error(msg);
            }
            finally
            {
                //bugs from cart widget, after placing an order the cache is not refreshed 
                ClearMyHL3ShoppingCartCache(distributorId, btOrder.Locale);
            }

            return isPendingFControl ? string.Format("PENDING-FCONTROL-{0}", btOrder.OrderID) : btOrder.OrderID;
        }

        private static void ClearMyHL3ShoppingCartCache(string memberId, string locale)
        {
            try
            {
                var cartWidgetSource = new CartWidgetSource();
                cartWidgetSource.ExpireShoppingCartCache(memberId, locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                       string.Format("Error occurred ClearMyHL3ShoppingCartCache. Id is {0}-{1}.\r\n{2}", memberId, locale,
                                     ex.Message));
            }
        }

        public static decimal getOtherCharges(ChargeList chargeList)
        {
            return chargeList != null
                       ? chargeList.Sum(c => (c as Charge_V01).Amount + (c as Charge_V01).TaxAmount)
                       : 0;
        }

        public static decimal getKRDistributorPrice(ItemTotal_V01 lineItem, string sku)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var user = member.Value;

            DistributorOrderingProfile dop = DistributorOrderingProfileProvider.GetProfile(user.Id,
                                                                                           CultureInfo.CurrentCulture
                                                                                                      .Name.Substring(3));
            decimal discount = dop.StaticDiscount;

            var prodType = ServiceProvider.CatalogSvc.ProductType.Product;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            var catalogItem = CatalogProvider.GetCatalogItem(sku, locale.Substring(3));
            if (catalogItem != null)
            {
                prodType = catalogItem.ProductType;
            }
            int marketingFundPercent = 93;
            var roundMode = MidpointRounding.AwayFromZero;

            var discountedPrice = lineItem.DiscountedPrice;

            var marketingFund = Math.Round(lineItem.LinePrice * marketingFundPercent / 10000.0M, roundMode);


            var taxableAmount = discountedPrice + marketingFund;

            var tax = Math.Round((taxableAmount * 10) / 100.0M, roundMode);

            var total = discountedPrice + marketingFund + tax;

            if (prodType == ServiceProvider.CatalogSvc.ProductType.Product || prodType == ServiceProvider.CatalogSvc.ProductType.PromoAccessory)
            {
                var surcharge = Math.Round((lineItem.LinePrice / 100) * 2.2M, roundMode);
                var surchargeTax = Math.Round((surcharge * 10) / 100.0M, roundMode);
                total += surcharge + surchargeTax;
            }

            return total;
        }

        public static decimal getBODistributorPrice(ItemTotal_V01 lineItem, string sku)
        {
            return lineItem.RetailPrice * 1.19050M;
        }

        public static decimal getPriceWithAllCharges(OrderTotals_V01 totals, string sku, int quantity)
        {
            var lineItem = totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
            if (lineItem != null)
            {
                string locale = Thread.CurrentThread.CurrentCulture.ToString();
                switch (locale)
                {
                    case "ko-KR":
                        return getKRDistributorPrice(lineItem, sku);
                    case "es-BO":
                        {
                            //return getBODistributorPrice(lineItem, sku);
                            SKU_V01 sku01;
                            if (CatalogProvider.GetAllSKU("es-BO").TryGetValue(sku, out sku01))
                            {
                                return sku01.CatalogItem.ListPrice * 1.19050M * quantity;
                            }
                        }
                        break;
                    case "sr-RS":
                        return lineItem.DiscountedPrice + (lineItem.RetailPrice * 4.94M / 100.0M);
                    case "mk-MK":
                        return lineItem.DiscountedPrice + (lineItem.RetailPrice * 15.9M / 100.0M);
                    case "sl-SI":
                        return lineItem.DiscountedPrice + (lineItem.RetailPrice * 3.94M / 100.0M);
                    case "es-MX":
                        var exciseTax =
                            lineItem.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.EXCISE_TAX) ??
                            new Charge_V01(ChargeTypes.EXCISE_TAX, (decimal)0.0);
                        return lineItem.DiscountedPrice + ((Charge_V01)exciseTax).Amount;
                    default:
                        return lineItem.DiscountedPrice + lineItem.LineTax + getOtherCharges(lineItem.ChargeList);
                }
            }
            return 0;
        }

        public static decimal getPriceWithAllCharges(OrderTotals_V01 totals)
        {
            decimal runningTotal = 0;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (null != totals && null != totals.ItemTotalsList)
            {
                foreach (ItemTotal_V01 lineItem in totals.ItemTotalsList)
                {
                    switch (locale)
                    {
                        case "ko-KR":
                            runningTotal += getKRDistributorPrice(lineItem, lineItem.SKU);
                            break;
                        case "es-BO":
                            runningTotal = totals.DiscountedItemsTotal;
                            break;
                        case "sr-RS":
                            {
                                var freight =
                                    ((ItemTotal_V01)lineItem).ChargeList.Find(
                                        c => ((Charge_V01)c).ChargeType == ChargeTypes.FREIGHT) ??
                                    new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                                var packing =
                                    ((ItemTotal_V01)lineItem).ChargeList.Find(
                                        c => ((Charge_V01)c).ChargeType == ChargeTypes.PH) ??
                                    new Charge_V01(ChargeTypes.PH, (decimal)0.0);
                                runningTotal += lineItem.DiscountedPrice + ((Charge_V01)packing).Amount +
                                                ((Charge_V01)freight).Amount;
                            }
                            break;
                        case "hr-BA":
                            {
                                var freight =
                                    ((ItemTotal_V01)lineItem).ChargeList.Find(
                                        c => ((Charge_V01)c).ChargeType == ChargeTypes.FREIGHT) ??
                                    new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                                runningTotal += lineItem.DiscountedPrice +
                                                ((Charge_V01)freight).Amount;
                            }
                            break;
                        case "es-MX":
                            {
                                var exciseTax =
                                    totals.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.EXCISE_TAX) ??
                                    new Charge_V01(ChargeTypes.EXCISE_TAX, (decimal)0.0);
                                runningTotal = totals.DiscountedItemsTotal + ((Charge_V01)exciseTax).Amount;
                            }
                            break;
                        case "en-MX":
                            {
                                var exciseTax =
                                    totals.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.EXCISE_TAX) ??
                                    new Charge_V01(ChargeTypes.EXCISE_TAX, (decimal)0.0);
                                runningTotal = totals.DiscountedItemsTotal + ((Charge_V01)exciseTax).Amount;
                            }
                            break;
                        default:
                            runningTotal += lineItem.DiscountedPrice + lineItem.LineTax +
                                            getOtherCharges(lineItem.ChargeList);
                            break;
                    }
                }
            }

            if (null != totals && null != totals.ChargeList)
            {
                if (locale == "mk-MK" || locale == "sl-SI" || locale == "hr-BA")
                {
                    var packing = totals.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.PH) ??
                                  new Charge_V01(ChargeTypes.PH, (decimal)0.0);
                    var freight = totals.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.FREIGHT) ??
                                  new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                    var others = totals.ChargeList.Find(c => ((Charge_V01)c).ChargeType == ChargeTypes.OTHER) ??
                                 new Charge_V01(ChargeTypes.OTHER, (decimal)0.0);
                    runningTotal = totals.DiscountedItemsTotal + ((Charge_V01)packing).Amount +
                                   ((Charge_V01)freight).Amount + ((Charge_V01)others).Amount;
                }
            }

            return runningTotal;
        }

        public static decimal GetDistributorSubTotal(OrderTotals_V01 totals)
        {
            if (totals == null)
                return 0.0M;

            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            switch (locale)
            {
                case "sr-RS":
                case "sl-SI":
                case "mk-MK":
                case "es-MX":
                case "en-MX":
                case "hr-BA":
                    return getPriceWithAllCharges(totals);
                //case "hr-HR":
                //    {
                //        return totals.ItemsTotal * (1 - totals.DiscountPercentage / 100);
                //    }
                default:
                    return totals.DiscountedItemsTotal;
            }
        }

        public static decimal GetTaxAmount(OrderTotals_V01 totals)
        {
            if (totals == null)
                return 0.0M;

            var locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (locale == "es-UY")
            {
                return totals.TaxableAmountTotal * 0.22m;
            }
            return totals.TaxAmount;
        }

        public static bool HasLocalTax(OrderTotals_V01 totals)
        {
            if (totals == null)
                return false;

            var locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (locale == "es-UY")
            {
                return true;
            }

            if (totals.ChargeList == null)
                return false;

            return totals.ChargeList.Any(c => ((Charge_V01)c).ChargeType == ChargeTypes.LOCALTAX);
        }


        public static Charge_V01 GetLocalTax(OrderTotals_V01 totals)
        {
            if (totals != null)
            {
                var locale = Thread.CurrentThread.CurrentCulture.ToString();
                if (locale == "es-UY")
                {
                    var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var user = member.Value;
                    var tins = DistributorOrderingProfileProvider.GetTinList(user.Id, true);
                    var hasUYTX = false;
                    if (tins != null)
                    {
                        hasUYTX = tins.Any(t => t.IDType.Key == "UYTX");
                    }
                    var dsProfile = DistributorOrderingProfileProvider.GetProfile(user.Id, "UY");
                    if ((dsProfile != null && dsProfile.StaticDiscount >= 50) || hasUYTX)
                    {
                        var catalogItems = CatalogProvider.GetCatalogItems(totals.ItemTotalsList.Cast<ItemTotal_V01>().Select(l => l.SKU).ToList(), "UY");
                        var perceptionedTax = (from i in totals.ItemTotalsList
                                               from c in catalogItems
                                               where c.Value.SKU == ((ItemTotal_V01)i).SKU && c.Value.ProductType == ServiceProvider.CatalogSvc.ProductType.Product
                                               select i).Cast<ItemTotal_V01>().Sum(lineItem => lineItem.EarnBase * ((100 - totals.DiscountPercentage) / 100) * 0.143m);
                        return new Charge_V01(ChargeTypes.LOCALTAX, perceptionedTax);
                    }
                }
                else
                {
                    return
                        totals.ChargeList.Find(p => ((Charge_V01)p).ChargeType == ChargeTypes.LOCALTAX) as Charge_V01 ??
                        new Charge_V01(ChargeTypes.LOCALTAX, (decimal)0.0);
                }
            }
            return new Charge_V01(ChargeTypes.LOCALTAX, (decimal)0.0);
        }

        public static bool HasVATDiscount(OrderTotals_V01 totals)
        {
            if (totals == null)
                return false;

            var locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (locale == "es-UY")
            {
                decimal maxAmountForVATDiscount = 0;

                // Decide where the limit amount is comming from
                if (decimal.TryParse(HLConfigManager.Configurations.PaymentsConfiguration.MaxAmountForVATDiscount, out maxAmountForVATDiscount) && maxAmountForVATDiscount == 0)
                {
                    // Get value from service
                    var exciseTaxLimitList = GetExciseTax("UY", DateTime.Now);
                    var exciseTaxLimit = exciseTaxLimitList.FirstOrDefault();
                    if (exciseTaxLimit != null)
                    {
                        maxAmountForVATDiscount = exciseTaxLimit.BaseTax;
                    }
                }

                if (maxAmountForVATDiscount != 0 && totals.AmountDue <= maxAmountForVATDiscount)
                {
                    var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var user = member.Value;
                    var tins = DistributorOrderingProfileProvider.GetTinList(user.Id, true);
                    if (tins != null)
                    {
                        return tins.All(t => t.IDType.Key != "UYTX");
                    }
                }
            }
            return false;
        }

        #region Excise tax

        private const string ExciseTaxCachePrefix = "Excise_Tax";
        private static int ExciseTaxCacheMinutes = Settings.GetRequiredAppSetting<int>("ExciseTaxCacheMinutes");

        public static List<ExciseTaxInfo> GetExciseTax(string countryCode, DateTime date)
        {
            var cacheKey = string.Format("{0}_{1}", ExciseTaxCachePrefix, countryCode);
            var exciseTax = HttpRuntime.Cache[cacheKey] as List<ExciseTaxInfo>;

            if (exciseTax == null)
            {
                exciseTax = new List<ExciseTaxInfo>();
                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    try
                    {
                        var request = new GetExciseTaxRequest_V01()
                        {
                            CountryCode = countryCode,
                            CurrentDate = DateTime.Now
                        };
                        var response = proxy.GetExciseTax(new GetExciseTaxRequest1(request)).GetExciseTaxResult as GetExciseTaxResponse_V01;

                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.ExciseTaxList == null)
                        {
                            throw new ApplicationException(
                                "OrderProvider.GetExciseTax error. GetExciseTaxResponse indicates error.");
                        }
                        exciseTax = response.ExciseTaxList;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("GetExciseTax error, country code: {0}: {1}", countryCode, ex));
                    }
                }

                if (exciseTax.Any())
                {
                    HttpRuntime.Cache.Insert(cacheKey, exciseTax, null,
                                             DateTime.Now.AddMinutes(ExciseTaxCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                             null);
                }

            }
            return exciseTax;
        }

        #endregion

        private static void getParametersByCountry(string countryCode,
                                                   ServiceProvider.SubmitOrderBTSvc.Order orderWS,
                                                   string locale,
                                                   Order_V01 order)
        {
            var messages = new List<Message>();
            if (null != order.Messages && order.Messages.Count > 0)
            {
                var contributorMessage =
                    order.Messages.Find(m => m.MessageType == "ContributorClass");
                if (contributorMessage != null)
                {
                    orderWS.ContributorClass = contributorMessage.MessageValue;
                }
                messages.AddRange(order.Messages.Select(x => new Message { MessageType = x.MessageType, MessageValue = x.MessageValue }));
            }

            orderWS.PaymentClient = "GlobaleCom";
            orderWS.DiscountType = "SLIDING-0";
            // orderWS.OrderSubType = (order.PurchasingLimits as PurchasingLimits_V01) != null ? (order.PurchasingLimits as PurchasingLimits_V01).PurchaseSubType :  string.Empty;

            if (order != null && order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.HSO)
            {
                Message msg4 = new Message();
                msg4.MessageType = "GDOIntention";
                if (order.OrderIntention == OrderIntention.PersonalConsumption)
                {
                    msg4.MessageValue = "PC";
                }
                else if (order.OrderIntention == OrderIntention.RetailOrder)
                {
                    msg4.MessageValue = "RO";
                }
                messages.Add(msg4);
            }

            orderWS.Messages = messages.ToArray();
        }
        private static void getParametersByCountry(string countryCode,
                                                   ServiceProvider.SubmitOrderBTSvc.Order orderWS,
                                                   MyHLShoppingCart shoppingCart,
                                                   Order_V01 order, string source = null)
        {
            if (null != order.Messages && order.Messages.Count > 0)
            {
                // it is set by taxation rules
                var contributorMessage =
                    order.Messages.Find(m => m.MessageType == "ContributorClass");
                if (contributorMessage != null)
                {
                    orderWS.ContributorClass = contributorMessage.MessageValue;
                }
            }
            var paymentConfig =
                HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].PaymentsConfiguration;
            if (!String.IsNullOrEmpty(paymentConfig.MerchantAccountName))
            {
                if (orderWS.Payments != null && orderWS.Payments.Count() > 0)
                {
                    Array.ForEach(orderWS.Payments.ToArray(),
                                  p => p.MerchantAccountName = paymentConfig.MerchantAccountName);
                }
            }
            var messages = new List<Message>();
            orderWS.PaymentClient = "GlobaleCom";
            orderWS.DiscountType = "SLIDING-0";
            orderWS.Locale = shoppingCart.Locale;
            //orderWS.Platform = "MyHL";
            var sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);

            if (null != order.PurchasingLimits as PurchasingLimits_V01 && order.OrderCategory != ServiceProvider.OrderSvc.OrderCategoryType.HSO)
            {
                if (!string.IsNullOrEmpty((order.PurchasingLimits as PurchasingLimits_V01).PurchaseSubType))
                {
                    orderWS.OrderSubType = (order.PurchasingLimits as PurchasingLimits_V01).PurchaseSubType;
                }
                else
                {
                    orderWS.OrderSubType = shoppingCart.OrderSubType;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(shoppingCart.OrderSubType))
                {
                    orderWS.OrderSubType = shoppingCart.OrderSubType;
                }
            }

            var doConfig = HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].DOConfiguration;
            if (doConfig.HasPayeeID)
            {
                setPayeeID(countryCode, orderWS, shoppingCart);
            }

            if (countryCode == "KR")
            {
                orderWS.PaymentClient = "KSeCom";
                orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
                orderWS.SMSTrigger = (null != order.Payments[0] as WirePayment_V01)
                                         ? "WIRE TRANSFER"
                                         : "ORDER COMPLETION";
            }

            else if (countryCode == "IN")
            {
                orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
                WirePayment_V01 wrPayment = order.Payments != null ? order.Payments[0] as WirePayment_V01 : null;
                if (wrPayment != null && wrPayment.PaymentCode == "W2")
                {
                    orderWS.SMSTrigger = "ICICI REQUEST";
                }
            }

            else if (countryCode == "HK")
            {
                orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;

                if (Thread.CurrentThread.CurrentCulture.ToString() == "en-HK")
                {
                    orderWS.SMSTrigger = (null != order.Payments[0] as CreditPayment_V01)
                                             ? "ORDER COMPLETION"
                                             : "ORDER EXCEPTION";
                }
                else
                {
                    orderWS.SMSTrigger = (null != order.Payments[0] as CreditPayment_V01)
                                             ? "ORDER COMPLETION LOCAL"
                                             : "ORDER EXCEPTION LOCAL";
                }

                if (APFDueProvider.containsOnlyAPFSku(shoppingCart.CartItems))
                {
                    orderWS.SMSTrigger = "APF ORDER";
                }
            }
            //This change should be corrected to not have hardcode...
            else if (countryCode == "TH")
            {
                orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
                orderWS.SMSTrigger = (null != order.Payments[0] as WirePayment_V01)
                                         ? "ORDER EXCEPTION"
                                         : "ORDER COMPLETION";
            }
            else if (countryCode == "MX")
            {
                if (orderWS.Payments != null && orderWS.Payments.Any())
                {
                    Array.ForEach(orderWS.Payments.ToArray(), p => p.Agency = "Banamex");
                }
            }
            else if (countryCode == "MY")
            {
                if (orderWS.Payments != null && orderWS.Payments.Any())
                {
                    Array.ForEach(orderWS.Payments.ToArray(), p => p.Agency = "CyberSource");
                }
            }
            else if (countryCode == "GB")
            {
                var creditPayment = order.Payments[0] as CreditPayment_V01;
                if (null != creditPayment && creditPayment.Card.IssuerAssociation == IssuerAssociationType.Maestro)
                {
                    if (orderWS.Payments != null && orderWS.Payments.Count() > 0)
                    {
                        orderWS.Payments[0].IssueNumber = creditPayment.Card.IssuingBankID;
                    }
                }
            }
            else if (countryCode == "CN")
            {
                sessionInfo.IsNotFirstOrder = true;

                if (source != null && source == "Mobile")
                {
                    var msg = new Message();
                    msg.MessageValue = "True";
                    msg.MessageType = "Mobile";
                    messages.Add(msg);
                }
                var creditPayment = order.Payments != null ? order.Payments[0] as CreditPayment_V01 : null;
                if (null != creditPayment && !string.IsNullOrEmpty(creditPayment.AuthorizationMerchantAccount))
                {
                    orderWS.Comments = " 付款交易编号：" + creditPayment.AuthorizationMerchantAccount;
                }
                string prefix = Settings.GetRequiredAppSetting("OrderNumberPrefix");
                orderWS.ProcessingLocation = prefix;
                //orderWS.InputMethod = "CI";
                DistributorOrderingProfile dop = null;
                var locale = Thread.CurrentThread.CurrentCulture.ToString();
                var currentSession = SessionInfo.GetSessionInfo(order.DistributorID, locale);
                if (currentSession.IsReplacedPcOrder &&
                    currentSession.ReplacedPcDistributorOrderingProfile != null)
                {
                    dop = currentSession.ReplacedPcDistributorOrderingProfile;
                }
                else
                {
                    dop = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                    shoppingCart.CountryCode);
                }

                orderWS.CustomerID = dop.CNCustomorProfileID.ToString();
                if (APFDueProvider.IsAPFSkuPresent(shoppingCart.CartItems))
                {
                    var msg = new Message();
                    msg.MessageValue = "True";
                    msg.MessageType = "HasAPF";
                    messages.Add(msg);
                }
                if (!string.IsNullOrEmpty(shoppingCart.DeliveryInfo.RGNumber) &&
                    (shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup ||
                    shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier))
                {
                    var msg = new Message();
                    string[] parts = shoppingCart.DeliveryInfo.RGNumber.Split('|');
                    if (!string.IsNullOrEmpty(parts[1]))
                    {
                        msg.MessageValue = parts.Length == 2 ? parts[1] : shoppingCart.DeliveryInfo.RGNumber;
                        msg.MessageType = "CNID";
                        messages.Add(msg);
                    }

                }
                if ((shoppingCart.CartItems == null || shoppingCart.CartItems.Count == 0) && shoppingCart.Totals != null)
                {
                    OrderTotals_V02 totals = shoppingCart.Totals as OrderTotals_V02;
                    if (totals != null)
                    {
                        if (totals.Donation > decimal.Zero)
                        {
                            var msg = new Message();
                            msg.MessageValue = "True";
                            msg.MessageType = "IsOnlyDonation";
                            messages.Add(msg);
                            if (orderWS.OrderItems == null || orderWS.OrderItems.Count() == 0)
                            {
                                var btItems = new ServiceProvider.SubmitOrderBTSvc.Item[1];
                                btItems[0] = new ServiceProvider.SubmitOrderBTSvc.Item();
                                //changing the below line casuing exception when decimal is passed in the donation field
                                //btItems[0].Quantity = int.Parse(totals.Donation.ToString());
                                if (totals.Donation < 1)
                                    btItems[0].Quantity = 1;
                                else
                                    btItems[0].Quantity = (int)Math.Round(totals.Donation);
                                btItems[0].SKU = "9999";
                                orderWS.OrderItems = btItems;
                            }
                        }
                        if (totals.IsFreeFreight)
                        {
                            var msg = new Message();
                            msg.MessageValue = "True";
                            msg.MessageType = "FreeFreightOrder";
                            messages.Add(msg);
                        }

                    }
                }
                if (sessionInfo.surveyDetails != null)
                {
                    var msg = new Message()
                    {
                        MessageType = "ChinaSurveyId",
                        MessageValue = Convert.ToString(sessionInfo.surveyDetails.SurveyId)
                    };
                    messages.Add(msg);
                    HttpRuntime.Cache.Remove(string.Format("GetCustomerSurveyCacheKey_KEY_CN_{0}", shoppingCart.DistributorID));
                }

                orderWS.OrderSubType = dop.IsPC ? "PC" : string.Empty;
                orderWS.SMSNumber = !string.IsNullOrEmpty(shoppingCart.SMSNotification)
                                        ? shoppingCart.SMSNotification + ";"
                                        : string.Empty;
                if (orderWS.Shipment != null && orderWS.Shipment.ShippingMethodID == "0")
                    orderWS.Shipment.ShippingMethodID = "3";
                var items = orderWS.OrderItems.ToList().Select(i => i.Flavor);
                if (items.Any(x => x != null))
                {
                    var catalogItems =
                        CatalogProvider.GetCatalogItems(new List<string>(items), countryCode)
                                       .ToList();
                    var isVirtualOrder =
                        orderWS.OrderItems.Select(item => catalogItems.Find(c => c.Key == item.Flavor))
                               .All(cItem => !cItem.Value.IsInventory);
                    var virtualMsg = new Message()
                    {
                        MessageType = "CNIsVirtualOrder",
                        MessageValue = isVirtualOrder.ToString()
                    };
                    messages.Add(virtualMsg);

                }

                var PCLearningMsg = new Message()
                {
                    MessageType = "pcLearningPointOffSet",
                    MessageValue = shoppingCart.pcLearningPointOffSet.ToString()
                };
                messages.Add(PCLearningMsg);
                if (!string.IsNullOrWhiteSpace(shoppingCart.GreetingMsg))
                {
                    var GreetingMsg = new Message()
                    {
                        MessageType = "GreetingMsg",
                        MessageValue = shoppingCart.GreetingMsg
                    };
                    messages.Add(GreetingMsg);
                }
                if (shoppingCart.HasBrochurePromotion)
                {
                    var HasBrochurePromotion = new Message()
                    {
                        MessageType = "HasBrochurePromotion",
                        MessageValue = shoppingCart.HasBrochurePromotion.ToString()
                    };
                    messages.Add(HasBrochurePromotion);
                }
                if (shoppingCart.HastakenSrPromotion)
                {
                    var SRpromoMsg = new Message()
                    {
                        MessageType = "GetSRPromotion",
                        MessageValue = shoppingCart.HastakenSrPromotion.ToString()
                    };
                    messages.Add(SRpromoMsg);
                }
                if (shoppingCart.HastakenSrPromotionGrowing)
                {
                    var SrPromotionGrowingMsg = new Message()
                    {
                        MessageType = "SrPromotionGrowing",
                        MessageValue = shoppingCart.HastakenSrPromotionGrowing.ToString()
                    };
                    messages.Add(SrPromotionGrowingMsg);
                }
                if (shoppingCart.HastakenSrPromotionExcelnt)
                {
                    var SrPromotionExcelnMsg = new Message()
                    {
                        MessageType = "SrPromotionExcelnt",
                        MessageValue = shoppingCart.HastakenSrPromotionExcelnt.ToString()
                    };
                    messages.Add(SrPromotionExcelnMsg);
                }
                if (shoppingCart.HastakenBadgePromotion)
                {
                    var ChinaBadgePromotionMsg = new Message()
                    {
                        MessageType = "ChinaBadgePromotion",
                        MessageValue = shoppingCart.HastakenBadgePromotion.ToString()
                    };
                    messages.Add(ChinaBadgePromotionMsg);
                }
                if (shoppingCart.HastakenNewSrpromotion)
                {
                    var newsrMsg = new Message
                    {
                        MessageType = "NewSRPromotion",
                        MessageValue = "true"
                    };

                    messages.Add(newsrMsg);
                }
                //        HttpRuntime.Cache.Remove(string.Format("GetSRPromoDetail_{0}", shoppingCart.DistributorID));
                var PCLearningChangeRateMsg = new Message()
                {
                    MessageType = "PCLearningChangeRate",
                    MessageValue = order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.ETO ? shoppingCart.ETOChangeRate.ToString() : shoppingCart.ChangeRate.ToString()
                };
                messages.Add(PCLearningChangeRateMsg);

                string dsID;
                // Code changes for mobile API && ChinaDO
                dsID = string.Format("DSFirstOrder_{0}", !string.IsNullOrEmpty(shoppingCart.SrPlacingForPcOriginalMemberId) ? shoppingCart.SrPlacingForPcOriginalMemberId : shoppingCart.DistributorID);
                var purchasingLimitKey = string.Format("PL_{0}_{1}", !string.IsNullOrEmpty(shoppingCart.SrPlacingForPcOriginalMemberId) ? shoppingCart.SrPlacingForPcOriginalMemberId : shoppingCart.DistributorID, locale.Substring(3));
                HttpRuntime.Cache.Remove(dsID);
                //HttpRuntime.Cache.Remove(purchasingLimitKey);
                _cache.Expire(typeof(MyHerbalife3.Ordering.Providers.OrderingProfile.PurchasingLimitManager), purchasingLimitKey);
                var OrderTotals = shoppingCart != null ? shoppingCart.Totals != null ? shoppingCart.Totals as OrderTotals_V02 : null : null;
                if (OrderTotals != null && OrderTotals.Donation > 0 && !string.IsNullOrEmpty(shoppingCart.BehalfOfMemberId) && (shoppingCart.BehalfOfAmount > 0.0m || shoppingCart.BehalfOfSelfAmount > 0.0m))
                {
                    var preOrderingMsg = new Message
                    {
                        MessageType = "BehalfOfDonation",
                        MessageValue = "true|" + shoppingCart.BehalfDonationName + "|" + shoppingCart.BehalfOfContactNumber + "|" + shoppingCart.BehalfOfMemberId + "|" + shoppingCart.BehalfOfSelfAmount + "|" + shoppingCart.BehalfOfAmount
                    };
                    messages.Add(preOrderingMsg);
                }

                if (CatalogProviderLoader == null)
                    CatalogProviderLoader = new CatalogProviderLoader();

                if (CatalogProviderLoader.IsPreordering(shoppingCart.CartItems, shoppingCart.DeliveryInfo.WarehouseCode))
                {
                    var preOrderingMsg = new Message
                    {
                        MessageType = "SubmitPreOrder",
                        MessageValue = "false"
                    };

                    messages.Add(preOrderingMsg);
                }
                string OrderDetailsCache = string.Format("InvoiceDetails_{0}", shoppingCart.DistributorID);
                HttpRuntime.Cache.Remove(OrderDetailsCache);
            }
            else if (countryCode == "BR")
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();

                //SessionInfo sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (null != sessionInfo)
                {
                    if (!string.IsNullOrEmpty(sessionInfo.BRPF))
                    {
                        var msg3 = new Message();
                        msg3.MessageValue = sessionInfo.BRPF;
                        msg3.MessageType = "BRPF";
                        messages.Add(msg3);
                    }
                    DistributorOrderingProfile dop =
                        DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                                                                      shoppingCart.CountryCode);
                    if (dop.BirthDate != DateTime.MinValue)
                    {
                        var msg4 = new Message();
                        msg4.MessageType = "DateOfBirth";
                        msg4.MessageValue = dop.BirthDate.ToString(new CultureInfo("en-US")) ?? string.Empty;
                        messages.Add(msg4);
                    }
                    var msg5 = new Message();
                    msg5.MessageType = "DSLevel";
                    msg5.MessageValue = distributorProfileModel != null
                                            ? distributorProfileModel.Value.SubTypeCode
                                            : "DS";
                    messages.Add(msg5);
                    if (!string.IsNullOrEmpty(shoppingCart.EmailAddress))
                    {
                        var msg6 = new Message();
                        msg6.MessageType = "Email";
                        msg6.MessageValue = shoppingCart.EmailAddress;
                        messages.Add(msg6);
                    }
                }

                var headerIP = HttpContext.Current.Request.Headers["True-Client-IP"] ?? string.Empty;
                IPAddress akamaiIpAddress = IPAddress.None;
                var ipExists = IPAddress.TryParse(headerIP, out akamaiIpAddress);
                if (ipExists)
                {
                    var dsIPAddressMessag = new Message
                    {
                        MessageType = "DSIP",
                        MessageValue = Convert.ToString(akamaiIpAddress)
                    };
                    messages.Add(dsIPAddressMessag);
                }
                orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
            }
            else if (countryCode == "GR")
            {
                // If HFF standalone order validate the order type
                if (ShoppingCartProvider.IsStandaloneHFF(shoppingCart.CartItems))
                {
                    orderWS.OrderCategory = HLConfigManager.Configurations.DOConfiguration.HFFModalOrderType;
                }
            }
            else if (countryCode == "VE")
            {
                if (null != sessionInfo)
                {
                    if (!string.IsNullOrEmpty(sessionInfo.NationaId))
                    {
                        Message msg3 = new Message();
                        msg3.MessageType = "NationalID";
                        msg3.MessageValue = sessionInfo.NationaId;
                        messages.Add(msg3);
                    }
                }
            }
            else if (countryCode == "RU")
            {
                //Validate if is postmat shipment and add the messages postmatid and postmat Y
                if (null != shoppingCart.DeliveryInfo &&
                    ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier == shoppingCart.DeliveryInfo.Option)
                {
                    var pickupLocations =
                        ShippingProvider.GetShippingProvider("RU")
                                        .GetPickupLocationsPreferences(orderWS.DistributorID, "RU");

                    if (null != pickupLocations && pickupLocations.Count > 0)
                    {
                        var pickupLocation = pickupLocations.Find(p => p.ID == shoppingCart.DeliveryInfo.Id);
                        if (null != pickupLocation)
                        {
                            var id = string.Format("{0}",
                                                   getPostmatIDformat(pickupLocation.PickupLocationID,
                                                                      shoppingCart.DeliveryInfo.Address));
                            var msg1 = new Message { MessageType = "PostamatID", MessageValue = id };
                            messages.Add(msg1);
                        }
                    }
                }
            }
            else if (countryCode == "SI")
            {
                orderWS.Handling.IncludeInvoice = HandlingIncludeInvoice.WITHPACKAGE;
            }
            else if (countryCode == "US")
            {
                var pnm = order.Payments[0] as LocalPayment_V01;
                if (pnm != null && pnm.PaymentCode == "PN")
                {
                    orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
                }
            }
            else if (countryCode == "SI")
            {
                orderWS.Handling.IncludeInvoice = HandlingIncludeInvoice.WITHPACKAGE;
            }
            else if (countryCode == "PF")
            {
                foreach (ServiceProvider.SubmitOrderBTSvc.Payment p in orderWS.Payments)
                {
                    p.Currency = "FPX";
                }
            }
            else if (countryCode == "TW")
            {
                var pnm = order.Payments[0] as WirePayment_V01;
                if (pnm != null && pnm.PaymentCode == "W1")
                {
                    orderWS.SMSNumber = shoppingCart.SMSNotification ?? string.Empty;
                    orderWS.SMSTrigger = "VA PAYMENT REQUEST";
                }
            }
            if (shoppingCart.IsResponsive)
            {
                var msg = new Message { MessageValue = "True", MessageType = "Responsive" };
                messages.Add(msg);

                if (countryCode != "CN")
                {
                    orderWS.Platform = OrderPlatform.MOBILE;
                    orderWS.InputMethod = "MO";
                }
            }

            // for email
            //if (shoppingCart.HoldCheckoutOrder)
            //{
            //    var msg = new Message { MessageValue = "fraud", MessageType = "aditionalInformation" };
            //    messages.Add(msg);
            //}

            if (null != sessionInfo && null != sessionInfo.ThreeDSecuredCardInfo)
            {
                var msg1 = new Message { MessageType = "Is3DSecuredCreditCard", MessageValue = "True" };
                messages.Add(msg1);
            }
            bool disableTokenization = Settings.GetRequiredAppSetting<bool>("TokenizationDisabled", false);
            var msg2 = new Message { MessageType = "IsTokenized", MessageValue = (!disableTokenization).ToString() };
            messages.Add(msg2);
            //if (order != null && order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.HSO)
            //{
            //    Message msg4 = new Message();
            //    msg4.MessageType = "GDOIntention";
            //    if (order.OrderIntention == OrderIntention.PersonalConsumption)
            //    {
            //        msg4.MessageValue = "PC";
            //    }
            //    else if (order.OrderIntention == OrderIntention.RetailOrder)
            //    {
            //        msg4.MessageValue = "RO";
            //    }

            //    messages.Add(msg4);
            //}

            orderWS.Messages = messages.ToArray();
            var customerOrderDetail = shoppingCart.CustomerOrderDetail;
            if (customerOrderDetail != null)
            {
                var customer = new Customer();
                customer.Email = customerOrderDetail.EmailAddress;
                customer.FirstName = customerOrderDetail.FirstName;
                customer.LastName = customerOrderDetail.LastName;
                customer.Telephone = customerOrderDetail.Telephone;
                customer.ContactPreference = customerOrderDetail.ContactPreference;
                orderWS.Customer = customer;
            }
        }

        private static string getPostmatIDformat(int id, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01 address)
        {
            string postamatID = "";
            var couriers =
                ShippingProvider.GetShippingProvider("RU")
                                .GetDeliveryOptions(ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier, address);

            if (couriers != null && couriers.Count() > 0)
            {
                var courier = couriers.Find(c => c.Id == id);
                if (courier != null)
                {
                    //CourierType stores the Fusion alphanumeric ID. RU only
                    //CourierType starts with number = PickPoint
                    //CourierType starts with char = QIWI Post
                    postamatID = courier.CourierType.ToString();
                }
            }

            return postamatID;
        }

        private static void setPayeeID(string countryCode, ServiceProvider.SubmitOrderBTSvc.Order orderWS, MyHLShoppingCart shoppingCart)
        {
            switch (countryCode)
            {
                case "KR":
                    string primaryPhone = "";
                    PhoneNumber_V03 primaryPhoneObj = getDSPrimaryPhone(shoppingCart.DistributorID);

                    if (primaryPhoneObj != null)
                    {
                        primaryPhone = primaryPhoneObj.AreaCode + primaryPhoneObj.Number;
                    }

                    if (!string.IsNullOrEmpty(primaryPhone))
                    {
                        foreach (ServiceProvider.SubmitOrderBTSvc.Payment p in orderWS.Payments)
                        {
                            if (p.PaymentCode == "W3" || p.PaymentCode == "W4" || p.PaymentCode == "W1")
                            {
                                //p.PayeeID = primaryPhone;
                                orderWS.Shipment.Address.Line4 = primaryPhone;
                                break;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private static PhoneNumber_V03 getDSPrimaryPhone(string distributorID)
        {
            PhoneNumber_V03 primaryPhone = null;

            var details = new Core.DistributorProvider.DistributorSvc.DistributorDetailType();
            var distributor = DistributorProvider.GetDetailedDistributor(distributorID, details);

            if (null != distributor.Phones)
            {
                foreach (var ph in distributor.Phones)
                {
                    if (ph.Value != null)
                    {
                        var phoneVal = (Core.DistributorProvider.DistributorSvc.PhoneNumber_V03)ph.Value;
                        if (phoneVal.IsPrimary)
                        {
                            primaryPhone = new PhoneNumber_V03
                            {
                                AreaCode = phoneVal.AreaCode,
                                CountryPrefix = phoneVal.CountryPrefix,
                                Extention = phoneVal.Extention,
                                IsPrimary = phoneVal.IsPrimary,
                                Number = phoneVal.Number
                            };
                            //primaryPhone.PhoneType = phoneVal.PhoneType;
                            break;
                        }
                    }
                }
            }

            return primaryPhone;
        }

        private static Message[] getMessages(string countryCode, MyHLShoppingCart shoppingCart, Order_V01 order)
        {
            string paymentClient = "GlobaleCom";

            var messages = new List<Message>();
            if (null != order.Messages && order.Messages.Count > 0)
            {
                foreach (var message in order.Messages)
                {
                    var btMessage = new Message();
                    btMessage.MessageType = message.MessageType;
                    btMessage.MessageValue = message.MessageValue;
                    messages.Add(btMessage);
                }
            }
            var msg1 = new Message();
            msg1.MessageType = "PaymentClient";
            msg1.MessageValue = paymentClient;
            messages.Add(msg1);

            var msg2 = new Message();
            msg2.MessageType = "DiscountType";
            msg2.MessageValue = "SLIDING-0";
            messages.Add(msg2);

            var paymentConfig =
                HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].PaymentsConfiguration;
            if (!String.IsNullOrEmpty(paymentConfig.MerchantAccountName))
            {
                var msgMerchantName = new Message();
                msgMerchantName.MessageType = "MerchantAccountName";
                msgMerchantName.MessageValue = paymentConfig.MerchantAccountName;
                messages.Add(msgMerchantName);
            }

            if (countryCode == "MX")
            {
                var msg3 = new Message();
                msg3.MessageType = "Agency";
                msg3.MessageValue = "Banamex";
                messages.Add(msg3);
            }
            else if (countryCode == "MY")
            {
                var msg3 = new Message();
                msg3.MessageType = "Agency";
                msg3.MessageValue = "CyberSource";
                messages.Add(msg3);
            }
            else if (countryCode == "GB")
            {
                var creditPayment = order.Payments[0] as CreditPayment_V01;
                if (null != creditPayment && creditPayment.Card.IssuerAssociation == IssuerAssociationType.Maestro)
                {
                    if (!string.IsNullOrEmpty(creditPayment.Card.IssuingBankID))
                    {
                        var msg4 = new Message();
                        msg4.MessageType = "IssueNumber";
                        msg4.MessageValue = creditPayment.Card.IssuingBankID;
                        messages.Add(msg4);
                    }
                }
            }
            else if (countryCode == "IT" || countryCode == "FR")
            {
                if (null != order.PurchasingLimits as PurchasingLimits_V01)
                {
                    var msg4 = new Message();
                    msg4.MessageType = "OrderSubType";
                    msg4.MessageValue = (order.PurchasingLimits as PurchasingLimits_V01).PurchaseSubType;
                    messages.Add(msg4);
                }
            }
            else if (countryCode == "CH")
            {
                var msg4 = new Message();
                msg4.MessageType = "Locale";
                msg4.MessageValue = shoppingCart.Locale;
                messages.Add(msg4);
            }
            else if (countryCode == "BE")
            {
                var msg4 = new Message();
                msg4.MessageType = "Locale";
                msg4.MessageValue = shoppingCart.Locale;
                messages.Add(msg4);
            }
            else if (countryCode == "KR")
            {
                var msg4 = new Message();
                msg4.MessageType = "SMSTrigger";
                msg4.MessageValue = (null != order.Payments[0] as WirePayment_V01)
                                        ? "WIRE TRANSFER"
                                        : "ORDER COMPLETION";
                messages.Add(msg4);
                var msg3 = new Message();
                msg3.MessageType = "SMSNumber";
                msg3.MessageValue = shoppingCart.SMSNotification ?? string.Empty;
                messages.Add(msg3);
            }
            else if (countryCode == "BR")
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var ds = distributorProfileModel.Value.SubTypeCode;

                var sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (null != sessionInfo)
                {
                    if (!string.IsNullOrEmpty(sessionInfo.BRPF))
                    {
                        var msg3 = new Message();
                        msg3.MessageType = "BRPF";
                        msg3.MessageValue = sessionInfo.BRPF;
                        messages.Add(msg3);
                    }
                    DistributorOrderingProfile dop =
                        DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                                                                      shoppingCart.CountryCode);
                    if (dop.BirthDate != DateTime.MinValue)
                    {
                        var msg4 = new Message();
                        msg4.MessageType = "DateOfBirth";
                        msg4.MessageValue = dop.BirthDate.ToString(new CultureInfo("en-US")) ?? string.Empty;
                        messages.Add(msg4);
                    }
                    var msg5 = new Message();
                    msg5.MessageType = "DSLevel";
                    msg5.MessageValue = distributorProfileModel.Value.SubTypeCode;
                    messages.Add(msg5);
                    if (!string.IsNullOrEmpty(shoppingCart.EmailAddress))
                    {
                        var msg6 = new Message();
                        msg6.MessageType = "Email";
                        msg6.MessageValue = shoppingCart.EmailAddress;
                        messages.Add(msg6);
                    }
                }
            }

            var customerOrderDetail = shoppingCart.CustomerOrderDetail;
            if (customerOrderDetail != null)
            {
                Message custMessage = null;
                if (!String.IsNullOrEmpty(customerOrderDetail.FirstName))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingFirstName";
                    custMessage.MessageValue = customerOrderDetail.FirstName;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.LastName))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingLastName";
                    custMessage.MessageValue = customerOrderDetail.LastName;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.Telephone))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingTelephone";
                    custMessage.MessageValue = customerOrderDetail.Telephone;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.EmailAddress))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingFirstEmail";
                    custMessage.MessageValue = customerOrderDetail.EmailAddress;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.ContactPreference))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "ContactPreference";
                    custMessage.MessageValue = customerOrderDetail.ContactPreference;
                    messages.Add(custMessage);
                }
            }

            if (customerOrderDetail != null)
            {
                Message custMessage = null;
                if (!String.IsNullOrEmpty(customerOrderDetail.FirstName))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingFirstName";
                    custMessage.MessageValue = customerOrderDetail.FirstName;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.LastName))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingLastName";
                    custMessage.MessageValue = customerOrderDetail.LastName;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.Telephone))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingTelephone";
                    custMessage.MessageValue = customerOrderDetail.Telephone;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.EmailAddress))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "CustomerFacingFirstEmail";
                    custMessage.MessageValue = customerOrderDetail.EmailAddress;
                    messages.Add(custMessage);
                }

                if (!String.IsNullOrEmpty(customerOrderDetail.ContactPreference))
                {
                    custMessage = new Message();
                    custMessage.MessageType = "ContactPreference";
                    custMessage.MessageValue = customerOrderDetail.ContactPreference;
                    messages.Add(custMessage);
                }
            }

            return messages.ToArray();
        }

        private static ServiceProvider.SubmitOrderBTSvc.Payment[] setPayments(PaymentCollection orderPayments, string countryCode, string locale,
                                             bool isKioskOrder)
        {
            return setPayments(orderPayments, countryCode, locale, null, isKioskOrder);
        }

        private static ServiceProvider.SubmitOrderBTSvc.Payment[] setPayments(PaymentCollection orderPayments, string countryCode, string locale,
                                             ThreeDSecuredCreditCard threeDSecuredCreditCard, bool isKioskOrder)
        {
            if (orderPayments != null && orderPayments.Count > 0)
            {
                var paymentConfig =
                    HLConfigManager.CurrentPlatformConfigs[locale].PaymentsConfiguration;
                int i = 0;
                var paymentDetail = new ServiceProvider.SubmitOrderBTSvc.Payment[orderPayments.Count];
                foreach (var p in orderPayments)
                {
                    var payment = new ServiceProvider.SubmitOrderBTSvc.Payment();

                    if (null != p as WirePayment_V01)
                    {
                        var wp = p as WirePayment_V01;
                        payment.LineID = wp.ReferenceID;
                        payment.Amount = wp.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = wp.Currency;
                        payment.TransactionType = "SALE";

                        if (locale == "zh-TW" && paymentConfig != null && paymentConfig.WirePaymentCodes[0] == "W1")
                        {
                            payment.PaymentCode = "W1";
                        }
                        else
                        {
                            payment.PaymentCode = wp.PaymentCode;
                        }

                        //payment.PaymentCode = wp.PaymentCode;
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = "";
                        payment.AccountNumber = PaymentInfoProvider.GenericAccountNumber;
                        payment.Expiration = new DateTime(2199, 12, 1);
                    }
                    else if (null != p as DirectDepositPayment_V01)
                    {
                        var wp = p as DirectDepositPayment_V01;
                        payment.LineID = wp.ReferenceID;
                        payment.Amount = wp.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = wp.Currency;
                        payment.TransactionType = "SALE";
                        payment.PaymentCode = wp.PaymentCode;
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = "";
                        payment.AccountNumber = PaymentInfoProvider.GenericAccountNumber;
                        payment.Expiration = new DateTime(2199, 12, 1);
                    }
                    else if (null != p as CashPayment_V01)
                    {
                        var cp = p as CashPayment_V01;
                        payment.LineID = cp.ReferenceID;
                        payment.Amount = cp.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = cp.Currency;
                        payment.TransactionType = "SALE";
                        payment.PaymentCode = cp.PaymentCode;
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = "";
                        payment.AccountNumber = PaymentInfoProvider.GenericAccountNumber;
                        payment.Expiration = new DateTime(2199, 12, 1);
                    }
                    else if (null != p as LocalPayment_V01)
                    {
                        var cp = p as LocalPayment_V01;
                        payment.LineID = cp.ReferenceID;
                        payment.Amount = cp.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = cp.Currency;
                        payment.TransactionType = "SALE";
                        payment.PaymentCode = cp.PaymentCode;
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = "";
                        payment.AccountNumber = PaymentInfoProvider.GenericAccountNumber;
                        payment.Expiration = new DateTime(2199, 12, 1);
                    }
                    else if (null != p as LegacyPayment_V01)
                    {
                        var cp = p as LegacyPayment_V01;
                        payment.LineID = cp.ReferenceID;
                        payment.Amount = cp.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = cp.Currency;
                        payment.TransactionType = "SALE";
                        payment.PaymentCode = cp.PaymentCode.Substring(0, 2);
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = "";
                        payment.AccountNumber = PaymentInfoProvider.GenericAccountNumber;
                        payment.Expiration = new DateTime(2199, 12, 1);
                    }
                    else
                    {
                        var orderPayment = p as CreditPayment_V01;
                        //We need to accomodate the Korean variants
                        if (null != p as KoreaISPPayment_V01 || null != p as KoreaMPIPayment_V01)
                        {
                            if (p is KoreaISPPayment_V01)
                            {
                                var ispPayment = p as KoreaISPPayment_V01;
                                payment.ISP_EncryptedData = HttpUtility.UrlEncode(ispPayment.KvpEncryptedData,
                                                                                  new UTF8Encoding());
                                payment.ISP_SessionKey = HttpUtility.UrlEncode(ispPayment.KvpSessionKey,
                                                                               new UTF8Encoding());
                                payment.SecurePaymentType = "ISP";

                                // This is for KSNet non-3D, switch ISP to MPI
                                if (ispPayment.KvpSessionKey.Trim() == "FA")
                                {
                                    payment.SecurePaymentType = "MPI";
                                    payment.MPI_XID = "FA";
                                    payment.MPI_CAVV = ispPayment.KvpEncryptedData;
                                    payment.ISP_EncryptedData = string.Empty;
                                    payment.ISP_SessionKey = string.Empty;
                                    payment.CVV = (null != orderPayment.Card && !string.IsNullOrEmpty(orderPayment.Card.CVV)) ? orderPayment.Card.CVV : string.Empty;
                                }
                            }
                            else
                            {
                                var mpiPayment = p as KoreaMPIPayment_V01;
                                payment.MPI_CAVV = mpiPayment.CAVV;
                                if (isKioskOrder)
                                {
                                    payment.PayeeID = payment.MPI_CAVV;
                                }
                                payment.MPI_ECI = mpiPayment.ECI;
                                payment.MPI_XID = mpiPayment.XID;
                                payment.SecurePaymentType = "MPI";
                                if (null != orderPayment.Card)
                                {
                                    if (!string.IsNullOrEmpty(orderPayment.Card.CVV))
                                    {
                                        payment.CVV = orderPayment.Card.CVV;
                                    }
                                }
                            }
                            orderPayment.AuthorizationCode = null;
                            payment.PaymentCode = "OT";
                        }
                        else
                        {
                            payment.PaymentCode = countryCode == "CN"
                                                      ? orderPayment.TransactionType
                                                      : CreditCard.CardTypeToHPSCardType(
                                                          orderPayment.Card.IssuerAssociation);
                            payment.CVV = orderPayment.Card.CVV;
                        }
                        payment.LineID = orderPayment.ReferenceID;
                        payment.Amount = orderPayment.Amount;
                        payment.PaymentDate = DateUtils.GetCurrentLocalTime(countryCode);
                        payment.Currency = orderPayment.Currency;
                        payment.TransactionType = "SALE";
                        payment.NumberOfInstallments = 1;
                        payment.AddressVerification = false;
                        payment.Expiration = orderPayment.Card.Expiration;
                        payment.Operator = "INTERNET";
                        payment.NameOnAccount = orderPayment.Card.NameOnCard;
                        payment.AccountNumber = orderPayment.Card.AccountNumber.Trim();
                        var options = orderPayment.PaymentOptions as PaymentOptions_V01;
                        if (null != options)
                        {
                            payment.NumberOfInstallments = options.NumberOfInstallments;
                        }
                        var JPOptions = orderPayment.PaymentOptions as JapanPaymentOptions_V01;
                        if (null != JPOptions)
                        {
                            payment.NRI_PaymentOptionType =
                                JapanPaymentOptions_V01.JapanPaymentOptionTypeToHPSOptionType(JPOptions.ChargeMode);
                            payment.NRI_BonusMonth = (JPOptions.FirstInstallmentMonth > 0)
                                                         ? JPOptions.FirstInstallmentMonth.ToString()
                                                         : string.Empty;
                            payment.NRI_FirstBonusMonth = (JPOptions.FirstBonusMonth > 0)
                                                              ? JPOptions.FirstBonusMonth.ToString()
                                                              : string.Empty;
                            payment.NRI_SecondBonusMonth = (JPOptions.SecondBonusMonth > 0)
                                                               ? JPOptions.SecondBonusMonth.ToString()
                                                               : string.Empty;
                            if (orderPayment.Card.IssuerAssociation == IssuerAssociationType.AmericanExpress)
                                payment.NRI_PaymentOptionType =
                                    JapanPaymentOptions_V01.JapanPaymentOptionTypeToHPSOptionType(
                                        JapanPayOptionType.LumpSum);
                        }
                        payment.AuthMerchant =
                            payment.SettMerchant =
                            payment.ClientReferenceNumber =
                            payment.AuthNumber =
                            payment.TransactionCode = string.Empty;



                        if (!string.IsNullOrEmpty(orderPayment.AuthorizationCode))
                        {
                            payment.AuthNumber = payment.ClientReferenceNumber = orderPayment.AuthorizationCode;
                        }
                        if (orderPayment.AuthorizationMethod == AuthorizationMethodType.PaymentGateway)
                        {
                            payment.TransactionCode = orderPayment.TransactionID;
                            //Payments.PaymentGatewayResponse gatewayResponse = Payments.PaymentGatewayResponse.Create(false);
                            //if (null != gatewayResponse)
                            //{
                            //    payment.TransactionCode = gatewayResponse.TransactionCode;
                            //}
                            //else
                            //{
                            //    payment.TransactionCode = Guid.NewGuid().ToString();
                            //}
                        }
                        if (paymentConfig.AddressRequiredForNewCard)
                        {
                            if (null != p.Address)
                            {
                                var address = new ServiceProvider.SubmitOrderBTSvc.Address();
                                address.Line1 = p.Address.Line1;
                                address.Line2 = p.Address.Line2;
                                address.Line3 = p.Address.Line3;
                                address.Line4 = p.Address.Line4;
                                address.City = p.Address.City;
                                address.StateProvinceTerritory = p.Address.StateProvinceTerritory;
                                address.PostalCode = p.Address.PostalCode;
                                address.Country = p.Address.Country;
                                payment.Address = address;
                                var provider = ShippingProvider.GetShippingProvider(null);
                                if (provider != null)
                                {
                                    provider.FormatAddressForHMS(payment.Address);
                                }
                            }
                        }
                        if (null != threeDSecuredCreditCard)
                        {
                            payment.MPI_CAVV = !string.IsNullOrEmpty(threeDSecuredCreditCard.PaRes)
                                                   ? threeDSecuredCreditCard.PaRes
                                                   : string.Empty;
                            payment.MPI_ECI = !string.IsNullOrEmpty(threeDSecuredCreditCard.Eci)
                                                  ? threeDSecuredCreditCard.Eci
                                                  : string.Empty;
                            payment.MPI_XID = !string.IsNullOrEmpty(threeDSecuredCreditCard.Xid)
                                                  ? threeDSecuredCreditCard.Xid
                                                  : string.Empty;
                            payment.ISP_EncryptedData = !string.IsNullOrEmpty(threeDSecuredCreditCard.RequestToken)
                                                            ? threeDSecuredCreditCard.RequestToken
                                                            : string.Empty;
                            payment.ISP_SessionKey = !string.IsNullOrEmpty(threeDSecuredCreditCard.RequestId)
                                                         ? threeDSecuredCreditCard.RequestId
                                                         : string.Empty;
                            payment.NRI_BonusMonth = !string.IsNullOrEmpty(threeDSecuredCreditCard.VeresEnrolled)
                                                         ? threeDSecuredCreditCard.VeresEnrolled
                                                         : string.Empty;
                            payment.NRI_FirstBonusMonth =
                                !string.IsNullOrEmpty(threeDSecuredCreditCard.CommerceIndicator)
                                    ? threeDSecuredCreditCard.CommerceIndicator
                                    : string.Empty;
                            payment.NRI_SecondBonusMonth =
                                !string.IsNullOrEmpty(threeDSecuredCreditCard.UcafCollectionIndicator)
                                    ? threeDSecuredCreditCard.UcafCollectionIndicator
                                    : string.Empty;
                            payment.NRI_PaymentOptionType =
                                !string.IsNullOrEmpty(threeDSecuredCreditCard.AuthenticationPath)
                                    ? threeDSecuredCreditCard.AuthenticationPath
                                    : string.Empty;
                        }
                    }
                    //if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDueOnImport)
                    //{
                    //    var amount = GetOriginalConvertedAmount(payment.Amount, countryCode);
                    //    payment.Amount = amount;
                    //    payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.ConvertCurrencyFrom;
                    //}
                    paymentDetail[i++] = payment;
                }
                return paymentDetail;
            }
            return null;
        }

        private static Shipment setShipment(Order_V01 order)
        {
            var shipment = new Shipment();
            var shippingInfo = order.Shipment as ShippingInfo_V01;

            shipment.ShippingMethodID = shippingInfo.ShippingMethodID;
            shipment.Recipient = string.IsNullOrEmpty(shippingInfo.Recipient) ? string.Empty : shippingInfo.Recipient;
            shipment.Phone = shippingInfo.Phone;
            shipment.Recipient = shipment.Recipient;

            var address = shippingInfo.Address as ServiceProvider.OrderSvc.Address_V01;
            shipment.Address = new ServiceProvider.SubmitOrderBTSvc.Address();
            shipment.Address.Line1 = address.Line1;
            shipment.Address.Line2 = address.Line2;
            shipment.Address.Line3 = address.Line3;
            shipment.Address.Line4 = address.Line4;
            shipment.Address.City = address.City;
            shipment.Address.CountyDistrict = address.CountyDistrict;
            shipment.Address.StateProvinceTerritory = address.StateProvinceTerritory;
            shipment.Address.PostalCode = address.PostalCode;
            shipment.Address.Country = address.Country;
            shipment.WarehouseCode = shippingInfo.WarehouseCode;
            shipment.DeliveryTypeCode = shippingInfo.FreightVariant; // set address type for China
            shipment.StoreId = shippingInfo.WarehouseCode;
            var provider = ShippingProvider.GetShippingProvider(null);
            if (provider != null)
            {
                provider.FormatAddressForHMS(shipment.Address);
            }

            return shipment;
        }

        private static ItemTotal_V01 getItemTotal(ItemTotalsList itemTotalsList, string sku)
        {
            if (itemTotalsList != null)
            {
                return itemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
            }
            return null;
        }

        private static Item[] convertLineItems(Order_V01 order, OrderTotals_V01 orderTotals, MyHLShoppingCart shoppingCart)
        {
            var btItems = new List<Item>();
            var shippingInfo = order.Shipment as ShippingInfo_V01;
            var isSplitOrder = !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit) &&
                               shoppingCart.IsSplit;
            var alternativeWhs = isSplitOrder
                                     ? HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit.Split(',')
                                                      .ToList()
                                     : null;

            //Retrieving a list of child skus in the order
            List<string> childSKUsInOrder = new List<string>();
            var sKUsInOrder = from o in order.OrderItems
                              select o.SKU;
            childSKUsInOrder = retrieveChildSKUsInOrder(sKUsInOrder.ToList<string>());

            if (isSplitOrder)
            {
                foreach (OrderItem item in order.OrderItems)
                {
                    var catlogItem = CatalogProvider.GetCatalogItem(item.SKU, order.CountryOfProcessing);
                    var whCodeAndQuantity = CatalogProvider.GetWhCodeAndQuantity(item.SKU, order.CountryOfProcessing,
                                                                                 shippingInfo.WarehouseCode,
                                                                                 item.Quantity, alternativeWhs);

                    // Checking quantity
                    if (whCodeAndQuantity == null || !whCodeAndQuantity.Any() ||
                        whCodeAndQuantity.Select(whQty => whQty.Value).Sum() != item.Quantity)
                    {
                        var newItem = CreateLineItem(orderTotals, item, shippingInfo.WarehouseCode, item.Quantity, childSKUsInOrder, catlogItem.VolumePoints);
                        btItems.Add(newItem);
                    }
                    else
                    {
                        btItems.AddRange(whCodeAndQuantity.Select(whQty => CreateLineItem(orderTotals, item, whQty.Key, whQty.Value, childSKUsInOrder, catlogItem.VolumePoints)));
                    }
                }
            }
            else
            {
                foreach (OrderItem item in order.OrderItems)
                {
                    var catlogItem = CatalogProvider.GetCatalogItem(item.SKU, order.CountryOfProcessing);
                    var newItem = CreateLineItem(orderTotals, item, shippingInfo.WarehouseCode, item.Quantity, childSKUsInOrder, catlogItem.VolumePoints);
                    btItems.Add(newItem);
                }
            }
            return btItems.ToArray();
        }

        private static Item CreateLineItem(OrderTotals_V01 orderTotals, OrderItem item, string warehouse, int quantity, List<string> childSKUsInOrder, decimal unitVolume)
        {
            var newItem = new ServiceProvider.SubmitOrderBTSvc.Item();
            newItem.Quantity = quantity;
            newItem.WarehouseCode = warehouse;
            newItem.SKU = item.SKU;
            newItem.IsLinkedSku = childSKUsInOrder.Contains(item.SKU).ToString();
            newItem.UnitVolume = unitVolume.ToString();

            var orderItem = (item as ServiceProvider.OrderSvc.OnlineOrderItem);
            if (orderItem != null)
            {
                newItem.VolumePoints = orderItem.VolumePoint / item.Quantity;
                newItem.ItemDescription = orderItem.Description;
                newItem.Flavor = orderItem.SKU;


                if (orderTotals != null)
                {
                    var itemTotal = getItemTotal(orderTotals.ItemTotalsList, item.SKU);
                    if (itemTotal != null)
                    {
                        newItem.UnitPrice = itemTotal.TaxableAmount;
                        newItem.DiscountAmount = itemTotal.Discount.ToString();
                        newItem.TaxBeforeDiscount = decimal.Round(itemTotal.LineTax, 2).ToString();
                        newItem.TaxAfterDiscount = decimal.Round(itemTotal.AfterDiscountTax, 2).ToString();
                        newItem.LineTotal = itemTotal.LinePrice;
                    }
                }
            }
            return newItem;
        }

        public static List<string> retrieveChildSKUsInOrder(List<string> skusInOrder)
        {
            //var skusInOrder = from o in order.OrderItems
            //                       select new { o.SKU };
            var locale = Thread.CurrentThread.CurrentCulture.ToString();
            var allSKU = CatalogProvider.GetAllSKU(locale);
            List<string> listChildSKUs = new List<string>();

            if (allSKU != null)
            {
                foreach (string orderSKU in skusInOrder)
                {
                    SKU_V01 skuInOrder;
                    if (allSKU.TryGetValue(orderSKU, out skuInOrder))
                    {
                        if (skuInOrder.SubSKUs == null)
                            continue;
                        foreach (SKU_V01 linkedSKU in skuInOrder.SubSKUs)
                        {
                            if (linkedSKU.CatalogItem == null)
                            {
                                //invalid child sku, skip it
                                continue;
                            }
                            else
                            {
                                if (listChildSKUs != null)
                                {
                                    //only add Child SKUs that haven't been already added
                                    if (!listChildSKUs.Contains(linkedSKU.SKU))
                                    {
                                        listChildSKUs.Add(linkedSKU.SKU);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return listChildSKUs;
        }

        private static void updateSKU(Item[] items, string countryCode)
        {
            if (CatalogProviderLoader == null)
                CatalogProviderLoader = new CatalogProviderLoader();

            var catalogItems = CatalogProviderLoader.GetCatalogItems(new List<string>(items.ToList().Select(i => i.SKU)), countryCode).ToList();

            foreach (var item in items)
            {
                var cItem = catalogItems.Find(c => c.Key == item.SKU);
                item.SKU = cItem.Value.StockingSKU; // this is product ID
            }
        }

        private static Charge_V01 getFee(string type, ChargeList chargeList)
        {
            if (chargeList != null)
            {
                foreach (Charge_V01 charge in chargeList)
                {
                    if (charge.Type == type)
                    {
                        return charge;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// createPackageInfo - China
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private static OrderPackageInfo createPackageInfo(Order_V01 order, MyHLShoppingCart shoppingCart)
        {
            OrderPackageInfo orderPackageInfo = new OrderPackageInfo();
            ServiceProvider.OrderSvc.OnlineOrder onlineOrder = order as
                                                                        ServiceProvider.OrderSvc.OnlineOrder;
            if (onlineOrder != null && onlineOrder.Pricing != null)
            {
                OrderTotals_V02 totalsV02 = order.Pricing as OrderTotals_V02;
                if (totalsV02 != null && totalsV02.OrderFreight != null)
                {
                    if (totalsV02.OrderFreight.Packages != null)
                    {
                        int i = 0;
                        orderPackageInfo.Package = new ServiceProvider.SubmitOrderBTSvc.Package[totalsV02.OrderFreight.Packages.Count];
                        foreach (var oFreight in totalsV02.OrderFreight.Packages)
                        {
                            var SRPromo2Sku = Settings.GetRequiredAppSetting("ChinaSRPromoPhase2", string.Empty).Split('|');
                            var itemsInBoth = shoppingCart.CartItems.Select(c => c.SKU)
                                                .Intersect(SRPromo2Sku, StringComparer.OrdinalIgnoreCase);
                            if ((shoppingCart.HastakenSrPromotionExcelnt) || (shoppingCart.HastakenSrPromotionGrowing))
                            {
                                orderPackageInfo.Package[i++] = new ServiceProvider.SubmitOrderBTSvc.Package
                                {
                                    PackageType = oFreight.Packagetype,
                                    Unit = oFreight.Packagetype == "B" ? (oFreight.Unit + 1).ToString() : oFreight.Unit.ToString(),
                                    Volume = oFreight.Volume.ToString()
                                };
                            }
                            else
                            {
                                orderPackageInfo.Package[i++] = new ServiceProvider.SubmitOrderBTSvc.Package
                                {
                                    PackageType = oFreight.Packagetype,
                                    Unit = oFreight.Unit.ToString(),
                                    Volume = oFreight.Volume.ToString()
                                };
                            }
                        }
                    }
                    OrderFreight orderFreight = totalsV02.OrderFreight;
                    orderPackageInfo.ActualWeight = (orderFreight.PhysicalWeight).ToString();  //orderFreight.ActualFreight.ToString();
                    orderPackageInfo.ChargeableWeight = orderFreight.BeforeWeight.ToString();
                    orderPackageInfo.PackageAmount = orderFreight.MaterialFee.ToString();
                    orderPackageInfo.PackageWeight = orderFreight.PackageWeight.ToString();
                    orderPackageInfo.ProductWeight = orderFreight.Weight.ToString();
                    orderPackageInfo.VolumeWeight = orderFreight.VolumeWeight.ToString();
                    orderPackageInfo.TotalVolume = orderFreight.Packages.Sum(oi => oi.Volume).ToString();
                }
            }
            return orderPackageInfo;
        }


        private static Pricing setPricing(OrderTotals_V01 orderTotal)
        {
            var pricing = new Pricing();

            pricing.AmountDue = decimal.Parse(orderTotal.AmountDue.ToString());
            var charge = getFee("FREIGHT", orderTotal.ChargeList);
            if (charge != null)
            {
                pricing.FreightAmount = decimal.Parse(charge.Amount.ToString());
                pricing.FreightTaxAmount = decimal.Parse(charge.TaxAmount.ToString());
            }

            charge = getFee("PH", orderTotal.ChargeList);
            if (charge != null)
            {
                pricing.PHAmount = decimal.Parse(charge.Amount.ToString());
                pricing.PHTaxAmount = decimal.Parse(charge.TaxAmount.ToString());
            }
            pricing.TaxAmount = decimal.Parse(orderTotal.TaxAmount.ToString());
            pricing.MiscAmount = decimal.Parse(orderTotal.MiscAmount.ToString());

            pricing.VolumePoints = decimal.Parse(orderTotal.VolumePoints.ToString());

            // additional info
            if ((orderTotal as OrderTotals_V02) != null)
            {
                OrderTotals_V02 orderTotalsV02 = orderTotal as OrderTotals_V02;
                pricing.TotalDiscount = orderTotalsV02.MiscAmount.ToString();
                var freightCharge = getFee("FREIGHT", orderTotal.ChargeList);
                pricing.MiscAmount = freightCharge == null ? decimal.Zero : freightCharge.Amount;
                pricing.TotalTaxBeforeDiscount = orderTotalsV02.TaxBeforeDiscountAmount.ToString(); // orderTotalsV02.TaxAmount.ToString();
                pricing.TotalTaxAfterDiscount = orderTotalsV02.TaxAfterDiscountAmount.ToString();
                pricing.PromotionAmount = string.Empty;
                pricing.AOPAmount = orderTotalsV02.PromotionRetailAmount.ToString();
                pricing.IcmsTax = orderTotalsV02.LiteratureRetailAmount.ToString();
                pricing.IpiTax = orderTotalsV02.ProductTaxTotal.ToString();
                pricing.TotalCollateralRetail = orderTotalsV02.TaxableAmountTotal.ToString();
                pricing.TotalPromotionalRetail = orderTotalsV02.TaxableAmountTotal.ToString();
                pricing.TotalProductRetail = orderTotalsV02.ProductRetailAmount.ToString();
                pricing.FirstDonationAmount = orderTotalsV02.Donation.ToString();
                pricing.ItemsTotal = orderTotalsV02.ItemsTotal.ToString();
                //pricing.SecondDonationAmount = orderTotalsV02.Donation2;
            }
            return pricing;
        }

        /// <summary>
        ///     This method is used for the calculating tax on the Invoice page.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        public static decimal CalculateTaxForInvoice(Invoice invoice)
        {
            try
            {
                if (null != invoice && null != invoice.InvoiceSkus && invoice.InvoiceSkus.Count > 0)
                {
                    var countryCode = Thread.CurrentThread.CurrentCulture.Name.Substring(3);
                    var locale = Thread.CurrentThread.CurrentCulture.ToString();
                    var orderMonthDate = DateUtils.GetCurrentLocalTime(countryCode);
                    var backEndOrder = new Order_V01();
                    var items = new OrderItems();
                    items.AddRange(from item in invoice.InvoiceSkus
                                   select
                                       (OrderItem)
                                           new OrderItem_V01(item.SKU.Trim(), item.SKU.Trim(), (int)item.Quantity));
                    backEndOrder.OrderItems = items;
                    backEndOrder.DistributorID = invoice.DistributorID;
                    var shippingAddress = new ServiceProvider.OrderSvc.Address_V01
                    {
                        Line1 = invoice.Address1,
                        Line2 = invoice.Address2,
                        Line3 = string.Empty,
                        Line4 = string.Empty,
                        PostalCode = invoice.PostalCode,
                        StateProvinceTerritory = invoice.State,
                        City = invoice.City,
                        Country = invoice.Country,
                        CountyDistrict = invoice.Country
                    };

                    var warehouseCode = GetWarehouseCodeForUS(locale, shippingAddress);
                    if (string.IsNullOrEmpty(warehouseCode))
                    {
                        return 0;
                    }

                    var shipping = new ShippingInfo_V01
                    {
                        FreightVariant = string.Empty,
                        ShippingMethodID = "FED", // Hard Coding the Shipping method to "FED" as per ATG
                        WarehouseCode = warehouseCode,
                        Address = shippingAddress,
                        Recipient = invoice.FirstName,
                        Phone = invoice.PhoneNumber
                    };
                    backEndOrder.Shipment = shipping;
                    backEndOrder.InputMethod = InputMethodType.Internet;
                    backEndOrder.ReceivedDate = DateUtils.GetCurrentLocalTime(countryCode);
                    backEndOrder.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                    backEndOrder.OrderMonth = orderMonthDate.ToString("yyMM");
                    backEndOrder.UseSlidingScale = true;
                    backEndOrder.DiscountPercentage = 0;
                    backEndOrder.CountryOfProcessing = countryCode;
                    string errorCode;
                    var orderTotals = ShoppingCartProvider.GetQuote(backEndOrder, QuotePartType.Tax, true, out errorCode);
                    if (null != orderTotals)
                        return orderTotals.TaxAmount;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("Error in CalculateTaxForInvoice " + ex);
            }
            return 0;
        }

        public static CatalogItem_V01 GetInvoiceSkuDetails(string skuValue)
        {
            var allSKUs = CatalogProvider.GetAllSKU("en-US");
            if (allSKUs != null)
            {
                SKU_V01 sku;
                if (allSKUs.TryGetValue(skuValue, out sku))
                {
                    return sku.CatalogItem;
                }
            }
            return null;
        }

        //begin - shan - mar 14, 2012 - to get the product display name
        //to make consistent with how it is displaying in prdn currently
        //for ATGDecom - CreateInvoice page
        public static string GetProductDescription(string skuValue)
        {
            var allSKUs = CatalogProvider.GetAllSKU("en-US");
            if (null != allSKUs)
            {
                SKU_V01 sku;
                if (allSKUs.TryGetValue(skuValue, out sku))
                {
                    return string.Format("{0} {1}",
                                         (null != sku.Product ? sku.Product.DisplayName ?? string.Empty : string.Empty),
                                         (sku.Description ?? string.Empty));
                }
            }
            return null;
        }

        //end

        private static string GetWarehouseCodeForUS(string locale, ServiceProvider.OrderSvc.Address_V01 addressV01)
        {
            var shippingProvider = new ShippingProvider_US();
            var deliveryOptions = shippingProvider.GetDeliveryOptions(locale);
            if (null != deliveryOptions && deliveryOptions.Count > 0)
            {
                var state = shippingProvider.GetStateNameFromStateCode(addressV01.StateProvinceTerritory);
                var options =
                    deliveryOptions.Where(
                        d => d.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping && !string.IsNullOrEmpty(d.State) &&
                             d.State.Trim() == state);
                if (null != options && options.Count() > 0)
                {
                    return options.First().WarehouseCode;
                }
                return string.Empty;
            }
            return string.Empty;
        }

        public static GenerateOrderNumberResponse_V01 GenerateOrderNumber(GenerateOrderNumberRequest_V01 request)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            var response =
                proxy.GenerateOrderNumber(new GenerateOrderNumberRequest1(request)).GenerateOrderNumberResult as GenerateOrderNumberResponse_V01;

            return response;
        }

        /// <summary>Create a new Payment gateway Record</summary>
        /// <returns></returns>
        public static int InsertPaymentGatewayRecord(string orderNumber,
                                                     string distributorId,
                                                     string gatewayName,
                                                     string serializedOrder,
                                                     string locale)
        {
            OrderServiceClient proxy = null;
            try
            {
                proxy = ServiceClientProvider.GetOrderServiceProxy();
                var request = new InsertPaymentGatewayRecordRequest_V01();
                request.ServerName = Environment.MachineName;
                request.DistributorID = distributorId;
                request.GatewayName = gatewayName;
                request.Order = serializedOrder;
                request.OrderNumber = orderNumber;
                request.Locale = locale;
                var response =
                    proxy.InsertPaymentGatewayRecord(new InsertPaymentGatewayRecordRequest1(request)).InsertPaymentGatewayRecordResult as InsertPaymentGatewayRecordResponse_V01;

                if (response.Status == ServiceResponseStatusType.ServerFault || response.Id == 0)
                {
                    throw new Exception();
                }

                return response.Id;
            }
            catch (Exception ex)
            {
                ex =
                    new ApplicationException(
                        string.Format("OrderProvider.InsertPaymentGatewayRecord failed for Order Number {0}",
                                      orderNumber, ex));
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                throw ex;
            }
        }

        public static PaymentGatewayRecordStatusType GetPaymentGatewayRecordStatus(string orderNumber)
        {
            var response = new GetPaymentGatewayRecordResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetPaymentGatewayRecordRequest_V01();
                    request.OrderNumber = orderNumber;
                    response = proxy.GetPaymentGatewayRecord(new GetPaymentGatewayRecordRequest1(request)).GetPaymentGatewayRecordResult as GetPaymentGatewayRecordResponse_V01;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.GetPaymentGatewayRecordStatus failed for Order Number {0}",
                                          orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return response.TransactionStatus;
        }


        public static int InsertPaymentGatewayNotification(string orderNumber, string bankName)
        {
            var response = new InsertPaymentGatewayNotificationResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new InsertPaymentGatewayNotificationRequest_V01
                    {
                        OrderNumber = orderNumber,
                        BankName = bankName,
                        ServerName = Environment.MachineName
                    };
                    response = proxy.InsertPaymentGatewayNotification(new InsertPaymentGatewayNotificationRequest1(request)).InsertPaymentGatewayNotificationResult as InsertPaymentGatewayNotificationResponse_V01;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.InsertPaymentGatewayNotification failed for Order Number {0}",
                                          orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return response.Id;
        }


        public static int GetPaymentGatewayNotification(string orderNumber)
        {
            var response = new GetPaymentGatewayNotificationResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetPaymentGatewayNotificationRequest_V01 { OrderNumber = orderNumber };
                    response = proxy.GetPaymentGatewayNotification(new GetPaymentGatewayNotificationRequest1(request)).GetPaymentGatewayNotificationResult as GetPaymentGatewayNotificationResponse_V01;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.GetPaymentGatewayNotification failed for Order Number {0}",
                                          orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return response.IsProcessing;
        }

        public static int UpdatePaymentGatewayNotification(string orderNumber, int orderStatus)
        {
            var response = new GetPaymentGatewayNotificationResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetPaymentGatewayNotificationRequest_V01 { OrderNumber = orderNumber, OrderStatus = orderStatus };
                    response = proxy.UpdatePaymentGatewayNotification(new UpdatePaymentGatewayNotificationRequest(request)).UpdatePaymentGatewayNotificationResult as GetPaymentGatewayNotificationResponse_V01;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.UpdatePaymentGatewayNotification failed for Order Number {0}",
                                          orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return response.IsProcessing;
        }

        /// <summary>Get an existing PaymentGateway Record</summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        public static string GetPaymentGatewayRecord(string orderNumber)
        {
            var response = new GetPaymentGatewayRecordResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetPaymentGatewayRecordRequest_V01();
                    request.OrderNumber = orderNumber;
                    response = proxy.GetPaymentGatewayRecord(new GetPaymentGatewayRecordRequest1(request)).GetPaymentGatewayRecordResult as GetPaymentGatewayRecordResponse_V01;
                }
                catch (Exception ex)
                {
                    ex = new ApplicationException(string.Format("OrderProvider.GetPaymentGatewayRecord failed for Order Number {0}", orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }


            return response.Record;
        }

        /// <summary>Get an existing PaymentGateway Record</summary>
        /// <param name="OrderNumber"></param>
        /// <returns></returns>
        public static List<string> GetPaymentGatewayLog(string orderNumber, PaymentGatewayLogEntryType entryType)
        {
            var response = new GetPaymentGatewayLogResponse_V01();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetPaymentGatewayLogRequest_V01();
                    request.OrderNumber = orderNumber;
                    request.EntryType = entryType;
                    response = proxy.GetPaymentGatewayLog(new GetPaymentGatewayLogRequest1(request)).GetPaymentGatewayLogResult as GetPaymentGatewayLogResponse_V01;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.GetPaymentGatewayLog failed for Order Number {0}", orderNumber,
                                          ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return response.Records;
        }

        /// <summary>Update the PaymentGateway Logs for an existing PaymentGateway Record</summary>
        /// <param name="orderNumber"></param>
        /// <param name="data"></param>
        /// <param name="entryType"></param>
        /// <param name="status"></param>
        public static void UpdatePaymentGatewayRecord(string orderNumber,
                                                      string data,
                                                      PaymentGatewayLogEntryType entryType,
                                                      PaymentGatewayRecordStatusType status)
        {
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new UpdatePaymentGatewayRecordRequest_V01();
                    request.ServerName = Environment.MachineName;
                    request.LogEntry = data;
                    request.LogEntryType = entryType;
                    request.OrderNumber = orderNumber;
                    request.Status = status;
                    var response = proxy.UpdatePaymentGatewayRecord(new UpdatePaymentGatewayRecordRequest1(request)).UpdatePaymentGatewayRecordResult;
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            string.Format("OrderProvider.UpdatePaymentGatewayRecord failed for Order Number {0}",
                                          orderNumber, ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
        }

        public static string SendBPagServiceRequest(string version,
                                                    string action,
                                                    string merchant,
                                                    string user,
                                                    string password,
                                                    string probeXml)
        {
            string probeResponse = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetBPagPaymentServiceRequest_V01();
                    request.ServiceVersion = version;
                    request.Action = action;
                    request.Merchant = merchant;
                    request.User = user;
                    request.Password = password;
                    request.Data = probeXml;
                    var response =
                        proxy.SendBPagPaymentService(new SendBPagPaymentServiceRequest(request)).SendBPagPaymentServiceResult as GetBPagPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        probeResponse = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendBPagServiceRequest: Error calling BPag Payment Gateway Service.", ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return probeResponse;
        }

        public static string SendBrasPagEncryptServiceRequest(string version,
                                                              string merchant,
                                                              string data)
        {
            string probeResponse = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetBrasPagPaymentServiceRequest_V01();
                    request.ServiceVersion = version;
                    request.MerchantId = merchant;
                    request.Data = data;
                    var response =
                        proxy.SendBrasPagPaymentService(new SendBrasPagPaymentServiceRequest(request)).SendBrasPagPaymentServiceResult as GetBrasPagPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        probeResponse = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendBPagServiceRequest: Error calling BrasPag Encrypt Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return probeResponse;
        }

        public static string SendBrasPagDecryptPaymentService(string version,
                                                              string merchant,
                                                              string data)
        {
            string responseDecrypt = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetBrasPagDecryptPaymentServiceRequest_V01();
                    request.Version = version;
                    request.MerchantId = merchant;
                    request.Data = data;
                    var response =
                        proxy.SendBrasPagDecryptPaymentService(new SendBrasPagDecryptPaymentServiceRequest(request)).SendBrasPagDecryptPaymentServiceResult as GetBrasPagDecryptPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        responseDecrypt = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendBrasPagDecryptPaymentService: Error calling BrasPag Decrypt Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseDecrypt;
        }


        public static string SendBanCardServiceRequest(string data)
        {
            string responseId = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetBanCardPaymentServiceRequest_V01();
                    request.Data = data;
                    var response =
                        proxy.SendBanCardPaymentService(new SendBanCardPaymentServiceRequest(request)).SendBanCardPaymentServiceResult as GetBanCardPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        responseId = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendBanCardServiceRequest: Error calling BanCard Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseId;
        }


        public static string SendOcaTransactionIdServiceRequest(string data)
        {
            string responseId = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetOcaPaymentServiceRequest_V01();
                    request.Data = data;
                    var response =
                        proxy.SendOcaTransactionIdPaymentService(new SendOcaTransactionIdPaymentServiceRequest(request)).SendOcaTransactionIdPaymentServiceResult as GetOcaPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        responseId = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendOcaTransactionIdServiceRequest: Error calling Oca SendOcaTransactionIdService Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseId;
        }


        public static string SendOcaConfirmationServiceRequest(string data)
        {
            string responseId = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetOcaPaymentServiceRequest_V01();
                    request.Data = data;
                    var response =
                        proxy.SendOcaConfirmationPaymentService(new SendOcaConfirmationPaymentServiceRequest(request)).SendOcaConfirmationPaymentServiceResult as GetOcaPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        responseId = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendOcaTransactionIdServiceRequest: Error calling Oca SendOcaConfirmationPaymentService Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseId;
        }

        public static string SendBancaIntesaPaymentServiceRequest(string data)
        {
            string responseId = string.Empty;
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetBancaIntesaPaymentServiceRequest_V01();
                    request.Data = data;
                    var response =
                        proxy.SendBancaIntesaPaymentService(new SendBancaIntesaPaymentServiceRequest(request)).SendBancaIntesaPaymentServiceResult as GetBancaIntesaPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        responseId = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendBancaIntesaPaymentService: Error calling BancaIntesa SendBancaIntesaPaymentService Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseId;
        }

        public static string SendTutunskaPaymentServiceRequest(string data)
        {
            string responseId = string.Empty;
            using (var proxy = ServiceClientProvider.GetPaymentGatewayBridgeProxy())
            {
                try
                {
                    var request = new GetTutunskaPaymentServiceRequest_V01();
                    request.Data = data;
                    var response =
                        proxy.SendTutunskaPaymentService(new SendTutunskaPaymentServiceRequest(request)).SendTutunskaPaymentServiceResult as GetTutunskaPaymentServiceResponse_V01;
                    if (response != null && response.Status == ServiceProvider.PaymentGatewayBridgeSvc.ServiceResponseStatusType.Success)
                    {
                        responseId = response.Response;
                    }
                }
                catch (Exception ex)
                {
                    ex =
                        new ApplicationException(
                            "OrderProvider.SendTutunskaPaymentService: Error calling Tutuska Macedonia SendTutunskaPaymentService Payment Gateway Service.",
                            ex);
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return responseId;
        }



        public static decimal GetTaxRateFromVertex(string distributorID, string locale, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 address,
                                                   out string errorMessage, out string validateCity,
                                                   out string validateZipCode)
        {
            var rate = decimal.Zero;
            errorMessage = string.Empty;
            var countryCode = locale.Substring(3, 2);

            string errorCode;
            ServiceProvider.AddressValidationSvc.Address avsAddress;
            if (
                !ShippingProvider.GetShippingProvider(countryCode)
                                 .ValidateAddress(new MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V02 { Address = address }, out errorCode,
                                                  out avsAddress))
            {
                errorMessage = "InvalidAddress";
            }

            address.City = validateCity = avsAddress != null ? avsAddress.City : address.City;
            address.PostalCode = validateZipCode = avsAddress != null
                                                       ? avsAddress.PostalCode.IndexOf("-",
                                                                                       System.StringComparison.Ordinal) >
                                                         1
                                                             ? avsAddress.PostalCode.Substring(0,
                                                                                               avsAddress.PostalCode
                                                                                                         .IndexOf("-",
                                                                                                                  System
                                                                                                                      .StringComparison
                                                                                                                      .Ordinal))
                                                             : avsAddress.PostalCode
                                                       : address.PostalCode;

            var freightWarehouse =
                ShippingProvider.GetShippingProvider(countryCode).GetFreightCodeAndWarehouseForTaxRate(address);
            if (freightWarehouse == null)
            {
                return rate;
            }

            try
            {
                var shoppingCart = new MyHLShoppingCart
                {
                    CartItems =
                            new ShoppingCartItemList()
                                {
                                    new ShoppingCartItem_V01
                                        {
                                            Quantity = 1,
                                            SKU = HLConfigManager.Configurations.DOConfiguration.TaxRateSKU
                                        }
                                },
                    ShoppingCartItems =
                            new List<DistributorShoppingCartItem>()
                                {
                                    new DistributorShoppingCartItem
                                        {
                                            Quantity = 1,
                                            SKU = HLConfigManager.Configurations.DOConfiguration.TaxRateSKU
                                        }
                                },
                    CountryCode = countryCode,
                    OrderCategory = ServiceProvider.CatalogSvc.OrderCategoryType.RSO,
                    DeliveryInfo = new Providers.Shipping.ShippingInfo
                    {
                        FreightCode = freightWarehouse[0],
                        WarehouseCode = freightWarehouse[1],
                        Address = new ServiceProvider.ShippingSvc.ShippingAddress_V01 { Address = address }
                    },
                    Locale = locale,
                    DistributorID = distributorID,
                };

                var order = OrderCreationHelper.CreateOrderObject(shoppingCart) as Order_V01;
                order = OrderCreationHelper.FillOrderInfo(order, shoppingCart) as Order_V01;

                var request = new GetTaxRateFromVertexRequest_V01 { Order = order };
                GetTaxRateFromVertexResponse_V01 result = null;
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                result = proxy.GetTaxRateFromVertex(new GetTaxRateFromVertexRequest1(request)).GetTaxRateFromVertexResult as GetTaxRateFromVertexResponse_V01;

                if (result.Status == ServiceResponseStatusType.Success)
                {
                    return result.TaxRate;
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format("GetTaxRateFromVertex, distributor:{0} locale:{1}, error message:{2}",
                                      distributorID, locale, order.Messages));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetTaxRateFromVertex, distributor:{0} locale:{1}, error message:{2}",
                                                 distributorID, locale, ex.ToString()));
            }

            return rate;
        }

        public static Dictionary<string, decimal> GetAllTaxRateFromVertex(string distributorId, string locale,
                                                                          MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 address, out string errorMessage,
                                                                          out string validateCity,
                                                                          out string validateZipCode)
        {
            var productTax = new Dictionary<string, decimal>();
            errorMessage = string.Empty;
            var countryCode = locale.Substring(3, 2);

            string errorCode;
            ServiceProvider.AddressValidationSvc.Address avsAddress;
            if (
                !ShippingProvider.GetShippingProvider(countryCode)
                                 .ValidateAddress(new MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V02 { Address = address }, out errorCode,
                                                  out avsAddress))
            {
                errorMessage = "InvalidAddress";
            }

            address.City = validateCity = avsAddress != null ? avsAddress.City : address.City;
            address.PostalCode = validateZipCode = avsAddress != null
                                                       ? avsAddress.PostalCode.IndexOf("-",
                                                                                       System.StringComparison.Ordinal) >
                                                         1
                                                             ? avsAddress.PostalCode.Substring(0,
                                                                                               avsAddress.PostalCode
                                                                                                         .IndexOf("-",
                                                                                                                  System
                                                                                                                      .StringComparison
                                                                                                                      .Ordinal))
                                                             : avsAddress.PostalCode
                                                       : address.PostalCode;

            var freightWarehouse =
                ShippingProvider.GetShippingProvider(countryCode).GetFreightCodeAndWarehouseForTaxRate(address);
            if (freightWarehouse == null)
            {
                return productTax;
            }

            try
            {
                var shoppingCart = new MyHLShoppingCart
                {
                    CartItems =
                            new ShoppingCartItemList()
                                {
                                    new ShoppingCartItem_V01
                                        {
                                            Quantity = 1,
                                            SKU = HLConfigManager.Configurations.DOConfiguration.TaxRateSKU
                                        }
                                },
                    ShoppingCartItems =
                            new List<DistributorShoppingCartItem>()
                                {
                                    new DistributorShoppingCartItem
                                        {
                                            Quantity = 1,
                                            SKU = HLConfigManager.Configurations.DOConfiguration.TaxRateSKU
                                        }
                                },
                    CountryCode = countryCode,
                    OrderCategory = ServiceProvider.CatalogSvc.OrderCategoryType.RSO,
                    DeliveryInfo = new Providers.Shipping.ShippingInfo
                    {
                        FreightCode = freightWarehouse[0],
                        WarehouseCode = freightWarehouse[1],
                        Address = new ServiceProvider.ShippingSvc.ShippingAddress_V01 { Address = address }
                    },
                    Locale = locale,
                    DistributorID = distributorId,
                };

                var order = OrderCreationHelper.CreateOrderObject(shoppingCart) as Order_V01;
                order = OrderCreationHelper.FillOrderInfo(order, shoppingCart) as Order_V01;

                var request = new GetTaxRateFromVertexRequest_V02 { Order = order };
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                var result = proxy.GetTaxRateFromVertex(new GetTaxRateFromVertexRequest1(request)).GetTaxRateFromVertexResult as GetTaxRateFromVertexResponse_V02;

                if (result != null &&
                    result.Status == ServiceResponseStatusType.Success)
                {
                    productTax = result.ProductTax;
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format("GetTaxRateFromVertex, distributor:{0} locale:{1}, error message:{2}",
                                      distributorId, locale, order.Messages));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetTaxRateFromVertex, distributor:{0} locale:{1}, error message:{2}",
                                                 distributorId, locale, ex.ToString()));
            }

            return productTax;
        }

        public static string GetShipToTaxAreaId(ServiceProvider.OrderSvc.Address address)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var response = proxy.GetTaxAreaId(new GetTaxAreaIdRequest1(new GetTaxAreaIdRequest_V01 { Address = address })).GetTaxAreaIdResult;
                if (null != response && response.Status == ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as GetTaxAreaIdResponse_V01;
                    if (null != responseV01)
                    {
                        return responseV01.TaxAreaId;
                    }
                }
                LoggerHelper.Error("OrderProvider: Error GetShipToTaxAreaId null");
                return string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("OrderProvider: Error GetShipToTaxAreaId, error message:{0}",
                    ex.Message));
                return string.Empty;
            }
            finally
            {
                proxy.Close();
            }
        }

        #region IPhoneOrder

        public static string ImportOrder(ServiceProvider.OrderSvc.Order _order,
                                         MyHLShoppingCart cart,
                                         string paymentClientName,
                                         string locale,
                                         out string error,
                                         out List<FailedCardInfo> failedCards,
                                         string emailAddress, string smsNumber, out string pnmId,
                                         out string pnmTrackingUrl)
        {
            Order_V01 order = _order as Order_V01;
            var theOrder = PopulateLineItems(order.CountryOfProcessing, _order, cart) as Order_V01;
            theOrder.DiscountPercentage = (cart.Totals as OrderTotals_V01).DiscountPercentage;

            failedCards = new List<FailedCardInfo>();
            error = pnmId = pnmTrackingUrl = string.Empty;

            if (theOrder == null || (theOrder.Pricing as OrderTotals_V01) == null)
            {
                return string.Empty;
            }
            var btOrder = new ServiceProvider.SubmitOrderBTSvc.Order();
            btOrder.DistributorID = theOrder.DistributorID.ToUpper();
            btOrder.CountryOfProcessing = theOrder.CountryOfProcessing;
            btOrder.Platform = (HLConfigManager.Platform == "iKiosk" ? OrderPlatform.IKIOSK : OrderPlatform.MOBILE);
            //Need to unify this enum
            btOrder.PaymentClient = paymentClientName;
            btOrder.OrderID = !String.IsNullOrEmpty(theOrder.OrderID) ? theOrder.OrderID : string.Empty;
            btOrder.ReferenceID = Guid.NewGuid().ToString().Replace("-", string.Empty);
            btOrder.InputMethod = (HLConfigManager.Platform == "iKiosk" ? "IK" : "MO"); //Need to unify this enum
            btOrder.OrderCategory = Enum.GetName(typeof(ServiceProvider.OrderSvc.OrderCategoryType), theOrder.OrderCategory);
            btOrder.ReceivedDate = theOrder.ReceivedDate;
            btOrder.OrderMonth = theOrder.OrderMonth;
            btOrder.CountryOfProcessing = theOrder.CountryOfProcessing;
            btOrder.DiscountPercentage = theOrder.DiscountPercentage;
            btOrder.CustomerID = string.Empty;
            btOrder.QualifyingSupervisorID = theOrder.DistributorID;
            btOrder.Pricing = setPricing(theOrder.Pricing as OrderTotals_V01);
            btOrder.OrderItems = convertLineItems(theOrder, theOrder.Pricing as OrderTotals_V01, cart);
            if (order.CountryOfProcessing == "CN")
                updateSKU(btOrder.OrderItems, order.CountryOfProcessing);
            btOrder.Shipment = setShipment(theOrder);

            var handling = new Handling();
            var handlingV01 = theOrder.Handling as HandlingInfo_V01;
            handling.IncludeInvoice = invoiceOptionConversion(handlingV01.IncludeInvoice);
            handling.PickupName = handlingV01.PickupName;
            handling.ShippingInstructions = handlingV01.ShippingInstructions;
            btOrder.Handling = handling;
            btOrder.Messages = new List<Message>().ToArray();
            btOrder.DiscountType = "SLIDING-0";
            btOrder.Locale = locale;
            btOrder.SMSNumber = smsNumber;
            btOrder.Email = emailAddress;
            btOrder.Payments = setPayments(theOrder.Payments, theOrder.CountryOfProcessing, locale, true);

            //For TW New Payment Type,Kept it generic to make changes for other countries.
            getParametersByCountryForMobile(theOrder.CountryOfProcessing, btOrder, cart, order, smsNumber);

            #region Mobile Exact Target

            //New Code to send email using Exact Target
            if (Settings.GetRequiredAppSetting("MobileExactTargetCountry").Contains(theOrder.CountryOfProcessing) && HLConfigManager.CurrentPlatformConfigs[cart.Locale].DOConfiguration.SendEmailUsingSubmitOrder)
            {
                btOrder.SendEmail = HLConfigManager.CurrentPlatformConfigs[cart.Locale].DOConfiguration.SendEmailUsingSubmitOrder.ToString().ToLower();

                if (theOrder.Payments != null)
                {
                    foreach (var payment in theOrder.Payments)
                    {
                        if (null != payment as WirePayment_V01)
                        {
                            btOrder.PaymentType = "Wire";
                        }
                        else if (null != payment as CreditPayment_V01)
                        {
                            btOrder.PaymentType = "CreditCard";
                        }
                    }
                }
                else
                {
                    btOrder.PaymentType = "Wire";
                }

                DistributorOrderConfirmation orderForMail = null;

                try
                {

                    orderForMail = EmailHelper.GetEmailFromOrderForMobile(
                                        order,
                                        cart,
                                        cart.Locale,
                                        cart.DeliveryInfo.Address.Recipient,
                                        cart.DeliveryInfo);

                    if (orderForMail != null)
                    {
                        // Email Info
                        var btOrderEmailInfo = new EmailInfo
                        {
                            DeliveryTimeEstimated = orderForMail.DeliveryTimeEstimated,
                            FirstName = orderForMail.Distributor.FirstName,
                            HffMessage = orderForMail.HFFMessage,
                            InvoiceOption = orderForMail.InvoiceOption,
                            LastName = orderForMail.Distributor.LastName,
                            MiddleName = orderForMail.Distributor.MiddleName,
                            PaymentOption = orderForMail.paymentOption,
                            PaymentType =
                                orderForMail.Payments != null ||
                                orderForMail.Payments[0].CardType.ToUpper() == "WIRE" ||
                                orderForMail.Payments[0].CardType.ToUpper() == "DEMANDDRAFT"
                                    ? "Wire"
                                    : "CreditCard",
                            PickUpLocation = orderForMail.PickupLocation,
                            PurchaseType = orderForMail.PurchaseType,
                            ShippingMethod = orderForMail.Shipment.ShippingMethod,
                            SpecialInstructions = orderForMail.SpecialInstructions
                        };


                        btOrderEmailInfo.PaymentType = btOrder.PaymentType;

                        btOrder.EmailInfo = btOrderEmailInfo;

                        // Pricing
                        if (order.CountryOfProcessing != "CN") // don't override for China
                        {
                            btOrder.Pricing.DiscountedItemsTotal =
                                orderForMail.TotalDiscountRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.LocalTaxCharge =
                                orderForMail.LocalTaxCharge.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.LogisticsCharge = orderForMail.Logistics.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.IpiTax = orderForMail.IPI.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.IcmsTax = orderForMail.ICMS.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalMarketingFund =
                                orderForMail.MarketingFund.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TaxedNet = orderForMail.TaxedNet.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalProductRetail =
                                orderForMail.TotalProductRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalCollateralRetail =
                                orderForMail.TotalCollateralRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.TotalPromotionalRetail =
                                orderForMail.TotalPromotionalRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.ItemsTotal = orderForMail.TotalRetail.ToString(CultureInfo.InvariantCulture);
                            btOrder.Pricing.VolumePointsRate = orderForMail.VolumePointsRate;
                        }

                        GetMessagesForEmailForMobile(theOrder.CountryOfProcessing, order, cart, btOrder, orderForMail);

                        foreach (var orderItem in btOrder.OrderItems)
                        {
                            var itemFromMail = orderForMail.Items.ToList().Find(oi => oi.SkuId == orderItem.SKU);
                            if (itemFromMail != null)
                            {
                                orderItem.EarnBase = itemFromMail.EarnBase;
                                orderItem.ItemDescription = itemFromMail.ItemDescription;
                                orderItem.LineTotal = itemFromMail.LineTotal;
                                orderItem.UnitPrice = itemFromMail.UnitPrice;
                                orderItem.VolumePoints = itemFromMail.VolumePoints;
                                orderItem.DistributorCost = itemFromMail.DistributorCost;
                                orderItem.Flavor = itemFromMail.Flavor;
                                orderItem.PriceWithCharges = itemFromMail.PriceWithCharges;
                                var catlogItem = CatalogProvider.GetCatalogItem(itemFromMail.SkuId, theOrder.CountryOfProcessing);
                                orderItem.ItemDescription = catlogItem.Description;
                                orderItem.ProductType = Enum.GetName(typeof(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType), catlogItem.ProductType);
                                orderItem.UnitVolume = catlogItem.VolumePoints.ToString();
                            }
                        }

                        //Item Description should come straight from ShoppingCart Items
                        foreach (var orderItem in btOrder.OrderItems)
                        {
                            //var itemFromMail = orderForMail.Items.ToList().Find(oi => oi.SkuId == orderItem.SKU);
                            var itemFromDescription = cart.ShoppingCartItems.ToList().Find(oi => oi.SKU == orderItem.SKU);
                            if (itemFromDescription != null)
                            {

                                string catalogDesc = orderItem.ItemDescription;
                                orderItem.ItemDescription = itemFromDescription.Description + catalogDesc;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("CreateOrder GetEmailFromOrder Failed. Exception error : {0}", ex.Message));
                }

            }

            #endregion Mobile Exact Target

            var paymentConfig = HLConfigManager.CurrentPlatformConfigs[locale].PaymentsConfiguration;
            if (!String.IsNullOrEmpty(paymentConfig.MerchantAccountName))
            {
                if (btOrder.Payments != null && btOrder.Payments.Count() > 0)
                {
                    Array.ForEach(btOrder.Payments.ToArray(),
                                  p => p.MerchantAccountName = paymentConfig.MerchantAccountName);
                }
            }
            if (null != theOrder.PurchasingLimits as PurchasingLimits_V01)
            {
                btOrder.OrderSubType = (theOrder.PurchasingLimits as PurchasingLimits_V01).PurchaseSubType;
            }
            btOrder.WebClientAuthenticationKey =
                WebClientAuthenticationProvider.GetWebClientAuthCode(btOrder.CountryOfProcessing);

            // populate contributor class

            if (null != order.Messages && order.Messages.Count > 0)
            {
                // it is set by taxation rules
                ServiceProvider.OrderSvc.Message contributorMessage =
                    order.Messages.Find(m => m.MessageType == "ContributorClass");
                if (contributorMessage != null)
                {
                    btOrder.ContributorClass = contributorMessage.MessageValue;
                }
            }

            var proxy = ServiceClientProvider.GetSubmitOrderProxy();
            try
            {
                LoggerTempWireup.WriteInfo(OrderSerialization(btOrder, btOrder.ReferenceID), "Checkout");
                var LocalesOrderIntentionValidation = Settings.GetRequiredAppSetting("LocalesOrderIntentionValidation", "").Split(',').ToList();
                var DSType = DistributorOrderingProfileProvider.CheckDsLevelType(theOrder.DistributorID, locale.Substring(3, 2));
                if (LocalesOrderIntentionValidation.Any(x => x == locale) && DSType == ServiceProvider.DistributorSvc.Scheme.Member)
                {
                    if (btOrder.Messages == null)
                    {
                        btOrder.Messages = new List<Message>().ToArray();

                    }
                    var messages = btOrder.Messages.ToList();
                    var message = new Message()
                    {
                        MessageType = "GDOIntention",
                        MessageValue = "PC"
                    };

                    var GreetingMsg = new Message()
                    {
                        MessageType = "GreetingMsg",
                        MessageValue = cart.GreetingMsg
                    };
                    messages.Add(GreetingMsg);
                    btOrder.Messages = messages.ToArray();

                }
                var response = proxy.ProcessRequest(new ProcessRequestRequest(btOrder)).Response;
                if (response != null && response.OrderID != null)
                {
                    UpdateTWSKUOrderedQuantity(btOrder);
                }
                string status = response.Status.ToUpper();
                if (status.Equals("SUCCESS") || status.Equals("DUPLICATE-TOTAL-MATCH"))
                {
                    btOrder.OrderID = response.OrderID;
                    pnmId = response.PayNearMeId;
                    pnmTrackingUrl = response.PayNearMeTrackingUrl;
                    //Kount Review
                    if (!string.IsNullOrEmpty(response.PaymentStatus) && response.PaymentStatus.Trim().ToUpper(CultureInfo.InvariantCulture) == "KOUNTREVIEW")
                    {
                        error = "KOUNTREVIEW";
                    }

                    //Deferred Processing
                    if (!string.IsNullOrEmpty(response.PaymentStatus) && response.PaymentStatus.Trim().ToUpper(CultureInfo.InvariantCulture) == "PROCESSING")
                    {
                        error = "PROCESSING";
                    }
                    LoggerHelper.Error(error);
                }
                else
                {
                    error =
                        string.Format("[{3}] DistributorID :{0}, CartID :{1}, error: {2}", theOrder.DistributorID,
                                      string.Empty, response.Message, status).ToUpper();
                    LoggerHelper.Error(error);
                    btOrder.OrderID = response.OrderID;
                    if (response.ResponsePayments != null)
                    {
                        if (response.ResponsePayments.Length > 0)
                        {
                            for (int count = 0; count < response.ResponsePayments.Length; count++)
                            {
                                if (string.IsNullOrEmpty(response.ResponsePayments[count].Status))
                                {
                                    continue;
                                }
                                if (response.ResponsePayments[count].Status.Trim().ToUpper(CultureInfo.InvariantCulture) == "KOUNT")
                                {
                                    status = "KOUNTDECLINED";
                                    error = "KOUNTDECLINED";
                                    var info = new FailedCardInfo();
                                    info.Amount = response.ResponsePayments[count].Amount;
                                    info.CardNumber = response.ResponsePayments[count].AccountNumber;
                                    info.CardType = response.ResponsePayments[count].Paycode;
                                    failedCards.Add(info);
                                }
                                if (response.ResponsePayments[count].Status.Trim().ToUpper() == "DECLINED")
                                {
                                    var info = new FailedCardInfo();
                                    info.Amount = response.ResponsePayments[count].Amount;
                                    info.CardNumber = response.ResponsePayments[count].AccountNumber;
                                    info.CardType = response.ResponsePayments[count].Paycode;
                                    failedCards.Add(info);
                                }
                            }
                        }
                    }

                    if (response.Status == "ServerFault" || response.OrderID.Contains("Exception"))
                    {
                        btOrder.OrderID = string.Empty;
                        error = "ServerFault";
                    }
                }
            }
            catch (Exception ex)
            {
                string msg =
                    error =
                    string.Format("TIMEOUT:DistributorID :{0}, CartID :{1}, error: {2}", theOrder.DistributorID,
                                  string.Empty, ex);
                LoggerHelper.Error(msg);
            }
            return btOrder.OrderID;
        }

        #endregion IPhoneOrder

        private static void getParametersByCountryForMobile(string countryCode,
                                                   ServiceProvider.SubmitOrderBTSvc.Order orderWS,
                                                   MyHLShoppingCart shoppingCart,
                                                   Order_V01 order, string smsPhone)
        {

            //Populate order items with missing parameters.This is necessary as Order Import Fails.
            DistributorOrderConfirmation orderForMail = null;

            try
            {
                orderForMail = EmailHelper.GetEmailFromOrderForMobile(
                    order,
                    shoppingCart,
                    shoppingCart.Locale,
                    shoppingCart.DeliveryInfo.Address.Recipient,
                    shoppingCart.DeliveryInfo);

                //OrderItems
                foreach (var orderItem in orderWS.OrderItems)
                {
                    var itemFromMail = orderForMail.Items.ToList().Find(oi => oi.SkuId == orderItem.SKU);
                    if (itemFromMail != null)
                    {
                        orderItem.EarnBase = itemFromMail.EarnBase;
                        orderItem.ItemDescription = itemFromMail.ItemDescription;
                        orderItem.LineTotal = itemFromMail.LineTotal;
                        orderItem.UnitPrice = itemFromMail.UnitPrice;
                        orderItem.VolumePoints = itemFromMail.VolumePoints;
                        orderItem.DistributorCost = itemFromMail.DistributorCost;
                        orderItem.Flavor = itemFromMail.Flavor;
                        orderItem.PriceWithCharges = itemFromMail.PriceWithCharges;
                        var catlogItem = CatalogProvider.GetCatalogItem(itemFromMail.SkuId, countryCode);
                        orderItem.ProductType = Enum.GetName(typeof(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType), catlogItem.ProductType);
                        //  orderItem.UnitVolume = itemFromMail.VolumePoints.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CreateOrder GetEmailFromOrder Failed. Exception error : {0}", ex.Message));
            }

            if (countryCode == "TW")
            {
                var virtualpayment = order.Payments[0] as WirePayment_V01;
                var paymentConfig =
                    HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].PaymentsConfiguration;

                if (virtualpayment != null && paymentConfig.WirePaymentCodes[0] == "W1")
                {
                    orderWS.SMSNumber = smsPhone;
                    orderWS.SMSTrigger = "VA PAYMENT REQUEST";
                }
            }
            else if (countryCode == "MX")
            {
                if (orderWS.Payments != null && orderWS.Payments.Any())
                {
                    Array.ForEach(orderWS.Payments.ToArray(), p => p.Agency = "Banamex");
                }
            }

            // MPC Fraud
            if (HLConfigManager.Configurations.CheckoutConfiguration.HoldPickupOrder)
            {
                if (orderWS.Payments != null && orderWS.Payments.Count() > 0)
                {
                    //// TODO: Get MPC Fraud from nuget
                    //MyHerbalife3.Core.DistributorProvider.DistributorLoader distributorLoader = new MyHerbalife3.Core.DistributorProvider.DistributorLoader();
                    //var distributorProfile = distributorLoader.Load(shoppingCart.DistributorID, countryCode);
                    var distributorProfile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID, countryCode);
                    if (distributorProfile != null)
                    {
                        shoppingCart.HoldCheckoutOrder = distributorProfile.IsMPCFraud.HasValue ? distributorProfile.IsMPCFraud.Value : false;
                        if (shoppingCart.HoldCheckoutOrder)
                        {
                            shoppingCart.FraudControlSessonId = string.Empty;
                            foreach (var p in orderWS.Payments)
                            {
                                p.UserInformation = "M";
                            }
                        }
                    }
                }
            }
            //KOUNT
            if (HLConfigManager.Configurations.CheckoutConfiguration.KountEnabled)
            {
                //1.KOUNT check won't be performed unless we have valid data passed in from Mobile Services
                if (!string.IsNullOrEmpty(shoppingCart.FraudControlSessonId))
                {
                    //2.KOUNT check will only be passed if it is enabled for the Country and Is Subject to Fraud check
                    var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var dsSubTypeCode = distributorProfileModel.Value.SubTypeCode.Trim();
                    var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(order.DistributorID, order.CountryOfProcessing);
                    if (MyHerbalife3.Ordering.Providers.FraudControl.FraudControlProvider.IsSubjectToFraudCheck(shoppingCart.Locale, shoppingCart, dsSubTypeCode, distributorOrderingProfile.ApplicationDate, MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers.HlCountryConfigurationProvider.GetCountryConfiguration(shoppingCart.Locale)))
                    {
                        // Kount
                        if (distributorOrderingProfile != null)
                        {
                            orderWS.ApplicationDate = distributorOrderingProfile.ApplicationDate.ToString();
                            orderWS.SponsorId = distributorOrderingProfile.SponsorID;
                        }
                        orderWS.SessionId = shoppingCart.FraudControlSessonId;
                        orderWS.Website = "MOBILE";
                    }
                }
            }

            //InputMethod - Only US/IT requests will reach here.
            if (HLConfigManager.Platform == "MyHLMobile")
            {
                orderWS.InputMethod = "IE";
            }

            //IN - Factura
            if (countryCode == "IN" && HLConfigManager.Platform == "iKiosk" && orderWS.Shipment.ShippingMethodID == "PU" && orderWS.Shipment.WarehouseCode == "SJ")
            {
                orderWS.Handling.ShippingInstructions = string.Format("{0} {1}", "297736762", "GUGM01");
                orderWS.Shipment.StoreName = "GUGM01";
            }

        }



        public static void ValidateInstructions(Object btOrder, Order_V01 order, MyHLShoppingCart shoppingCart)
        {
            if (order != null && order.Handling != null && shoppingCart != null)
            {
                var orderHandling = order.Handling as HandlingInfo_V01;
                if (orderHandling != null && string.IsNullOrEmpty(orderHandling.ShippingInstructions))
                {
                    var checkoutConfig =
                        HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].CheckoutConfiguration;
                    if (checkoutConfig.GetShippingInstructionsFromProvider)
                    {
                        var shippingProvider = ShippingProvider.GetShippingProvider(shoppingCart.CountryCode);
                        if (shippingProvider != null)
                        {
                            order.Handling = OrderProvider.CreateHandlingInfo(shoppingCart.CountryCode,
                                                                              shoppingCart.InvoiceOption, shoppingCart,
                                                                              order.Shipment as ShippingInfo_V01);
                            var handling = new Handling();
                            handling.IncludeInvoice = invoiceOptionConversion(orderHandling.IncludeInvoice);
                            handling.PickupName = orderHandling.PickupName;
                            handling.ShippingInstructions = orderHandling.ShippingInstructions;
                            var btOrderWs = btOrder as ServiceProvider.SubmitOrderBTSvc.Order;
                            btOrderWs.Handling = handling;
                        }
                    }
                }
            }
        }

        public static void SubmitOrder(string orderId, string distributorId, string locale,
                                       MyHLShoppingCart shoppingCart, List<ServiceProvider.OrderSvc.Payment> payments)
        {
            var countryCode = locale.Substring(3, 2);
            var order = new Order_V01
            {
                DistributorID = distributorId,
                CountryOfProcessing = countryCode,
                OrderID = orderId,
                ReceivedDate = DateUtils.GetCurrentLocalTime(countryCode),
                OrderMonth = GetOrderMonthShortString(distributorId, locale, countryCode),
                OrderCategory =
                        (ServiceProvider.OrderSvc.OrderCategoryType)
                        Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), shoppingCart.OrderCategory.ToString())
            };
            if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.ETO)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType))
                {
                    order.OrderCategory =
                        (ServiceProvider.OrderSvc.OrderCategoryType)
                        Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                                   HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType);
                }
            }

            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, string.Empty))
            {
                OrderProvider.SetAPFDeliveryOption(shoppingCart);
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }
            order.Shipment = OrderProvider.CreateShippingInfo(countryCode, shoppingCart);
            order.Handling = OrderProvider.CreateHandlingInfo(countryCode, shoppingCart.InvoiceOption, shoppingCart,
                                                              order.Shipment as ShippingInfo_V01);

            // Populate payment information.
            SessionInfo currentSession = SessionInfo.GetSessionInfo(distributorId, locale);
            if (payments != null && payments.Count > 0)
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in payments select p).ToArray());
            }
            else if (currentSession.Payments != null && currentSession.Payments.Count > 0)
            // this is for 3D Secured orders; we put Payments in session before popup 3D bank page
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in currentSession.Payments select p).ToArray());
            }
            //else
            //{
            //    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo"));
            //    blErrors.DataSource = _errors;
            //    blErrors.DataBind();
            //    return;
            //}

            //prepareOrderAndSubmit(order, countryCode);
        }

        private static string GetOrderMonthShortString(string distributorId, string locale, string countryCode)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorId, locale);
            var orderMonth = new OrderMonth(countryCode);
            if (sessionInfo != null)
            {
                if (!string.IsNullOrEmpty(sessionInfo.OrderMonthShortString))
                {
                    return sessionInfo.OrderMonthShortString;
                }
                else
                {
                    return orderMonth.OrderMonthShortString;
                }
            }
            else
            {
                return orderMonth.OrderMonthShortString;
            }
        }

        public static bool HasDsClickedIAgreeBefore(string DistributorId)
        {
            bool result = false;
            var MLMDistributor = GetMLMOverrideDSCache(DistributorId);
            if (MLMDistributor != null &&
                MLMDistributor.Any(p => p.DistributorID.Trim().ToUpper() == DistributorId.ToUpper()))
            {
                //Validation For No of days
                if (
                    MLMDistributor.Any(
                        mlmOverrideV01 => DateTime.Now <= mlmOverrideV01.AgreementDate.AddDays(NUMBER_OF_DAYS_FOR_MLM)))
                {
                    result = true;
                }

            }
            return result;
        }

        private static List<MLMOverride_V01> GetMLMOverrideDSCache(string DistributorId)
        {
            List<MLMOverride_V01> result = null;

            if (string.IsNullOrEmpty(DistributorId))
            {
                return result;
            }

            // gets cache key
            string cacheKey = MLM_DS_CACHE_PREFIX + DistributorId;

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as List<MLMOverride_V01>;

            if (result == null)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = LoadMLMOverrideDistributorFromService(DistributorId);
                    // saves to cache is successful
                    if (null != result)
                    {
                        SaveMLMOverrideDistributorToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, MyHerbalife3.Shared.Infrastructure.ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static List<MLMOverride_V01> LoadMLMOverrideDistributorFromService(string DistributorId)
        {
            if (string.IsNullOrEmpty(DistributorId))
            {
                return null;
            }
            else
            {
                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    try
                    {
                        GetMLMOverrideRequest_V01 request_V01 = new GetMLMOverrideRequest_V01();
                        request_V01.DistributorID = DistributorId;
                        var response =
                            proxy.GetMLMOverrideStatus(new GetMLMOverrideStatusRequest(request_V01)).GetMLMOverrideStatusResult as GetMLMOverrideResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success ||
                            response.MLMOverrideList == null)
                            throw new ApplicationException(
                                "OrderProvider.LoadMLMOverrideDistributorFromService error. GetExemptedDistributorResponse indicates error.");
                        return response.MLMOverrideList;
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("LoadMLMOverrideDistributorFromService error, country Code: {0} {1}",
                                          DistributorId, ex));
                    }
                }
            }
            return null;
        }

        private static void SaveMLMOverrideDistributorToCache(string cacheKey,
                                                              List<MLMOverride_V01> MLMOverrideDistributor)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     MLMOverrideDistributor,
                                     null,
                                     DateTime.Now.AddMinutes(MLM_DS_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable,
                                     null);
        }

        public static int InsertMLMOverrideRecordForDS(string dsid, string countrycode)
        {
            var response = new InsertMLMOverrideRecordResponse_V01();

            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    InsertMLMOverrideRecordRequest_V01 request_V01 = new InsertMLMOverrideRecordRequest_V01();

                    MLMOverride_V01 dsOverride = new MLMOverride_V01();
                    dsOverride.DistributorID = dsid;
                    dsOverride.Country = countrycode;
                    dsOverride.AgreementDate = DateTime.Now;
                    dsOverride.CreatedOn = DateTime.Now;
                    dsOverride.CreatedBy = Environment.MachineName;
                    dsOverride.UpdatedOn = DateTime.Now;
                    dsOverride.UpdatedBy = Environment.MachineName;

                    request_V01.MLMOverride = dsOverride;
                    response =
                        proxy.InsertOverrideRecord(new InsertOverrideRecordRequest(request_V01)).InsertOverrideRecordResult as InsertMLMOverrideRecordResponse_V01;

                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success)
                        throw new ApplicationException(
                            "OrderProvider.InsertMLMOverrideRecordForDS error. InsertMLMOverrideRecordForDS indicates error.");
                    return 0;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("InsertMLMOverrideRecordForDS error, DistributorId: {0} {1}", dsid, ex));
                }
            }

            return response.RecordID;
        }

        public static bool HasZeroPriceEventTickets(string DistributorID, string Locale)
        {
            bool result = false;
            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            if (sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO &&
                HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket &&
                sessionInfo.ShoppingCart.ShoppingCartItems != null && sessionInfo.ShoppingCart.ShoppingCartItems.Any() &&
                sessionInfo.ShoppingCart.ShoppingCartItems.Sum(i => i.CatalogItem.ListPrice) == 0)
            {
                result = true;
            }

            return result;
        }

        public static void GetOrderByReferenceId(string refId, ref string orderId, ref bool isDuplicate)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new OrderDupeCheckRequest_V02 { ReferenceId = refId };
                var response = proxy.GetOrderByReferenceId(new GetOrderByReferenceIdRequest(request)).GetOrderByReferenceIdResult as OrderDupeCheckResponse_V02;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    orderId = response.OrderID;
                    isDuplicate = response.IsDuplicate;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format(
                    "OrderProvider: Error getting GetOrderByReferenceId, error message:{0}", ex.Message));
            }
        }

        public static bool ValidateOrders(ServiceProvider.SubmitOrderBTSvc.Order previousOrder, ServiceProvider.SubmitOrderBTSvc.Order newOrder)
        {
            // do checking for the previous order.order.orderItems and new order.orderItems this 2 should not be null
            //Bug fix for 225894
            if (previousOrder == null || newOrder == null || previousOrder.OrderItems == null ||
                newOrder.OrderItems == null)
            {
                return false;
            }
            bool paymentcode = previousOrder.Payments[0].PaymentCode == newOrder.Payments[0].PaymentCode;
            bool paymentamont = previousOrder.Payments[0].Amount == newOrder.Payments[0].Amount;
            if (paymentcode && paymentamont)
            {
                if (previousOrder.OrderItems.Count() == newOrder.OrderItems.Count())
                {
                    foreach (var previousOrderitem in previousOrder.OrderItems)
                    {
                        var exists = newOrder.OrderItems.Any(x => x.Flavor.Equals(previousOrderitem.Flavor));
                        if (exists)
                        {
                            var quentity = newOrder.OrderItems.Where(x => x.Flavor.Equals(previousOrderitem.Flavor)) as Item;
                            if (quentity.Quantity != previousOrderitem.Quantity)
                            {
                                return false;
                            }

                        }
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
                return false;
            return true;
        }

        public static bool SubmitPaymentGatewayOrder(string orderNumber, string gatewayResponse)
        {
            var isSubmitted = false;
            try
            {
                var holder = GetPaymentGatewayOrder(orderNumber);
                holder.Order.OrderID = holder.BTOrder.OrderID = orderNumber;
                if (holder != null && holder.BTOrder != null && holder.BTOrder.Payments != null &&
                    holder.BTOrder.Payments.Any())
                {
                    var logs = GetPaymentGatewayLog(orderNumber, PaymentGatewayLogEntryType.Request);
                    if (logs != null && logs.Any())
                    {
                        var sortedList =
                            logs.Where(log => log.Contains("SerializedOrderHolder")).ToList();
                        if (sortedList != null && sortedList.Any())
                        {
                            var tempPaymentRecord = sortedList.LastOrDefault();
                            holder = OrderSerializer.DeSerializeOrder(tempPaymentRecord);
                            holder.Order.OrderID = holder.BTOrder.OrderID = orderNumber;
                        }
                    }
                    //var error = string.Empty;
                    //var failedCards = new List<FailedCardInfo>();
                    //var orderId = Providers.OrderProvider.ImportOrder(holder.BTOrder, out error, out failedCards, holder.ShoppingCartId);
                    var orderToSubmit = holder.BTOrder;
                    orderToSubmit.OrderID = orderNumber;
                    orderToSubmit.SendEmail = "true";
                    //orderToSubmit.IsResubmitted = "1";
                    var approvalNumber = string.Empty;
                    if (orderToSubmit.Payments.Any() && !string.IsNullOrEmpty(approvalNumber))
                    {
                        orderToSubmit.Payments[0].AuthNumber = approvalNumber;
                        orderToSubmit.Payments[0].TransactionCode = approvalNumber;
                    }
                    if (orderToSubmit.Payments[0].PaymentCode == "10" || orderToSubmit.Payments[0].PaymentCode == "12")
                        orderToSubmit.Payments[0].PaymentCode = "DC";

                    if (orderToSubmit.Payments.Any())
                    {
                        foreach (var payment in orderToSubmit.Payments)
                        {
                            payment.AccountNumber = CryptographyProvider.Encrypt(payment.AccountNumber);
                        }
                    }

                    var proxy = ServiceClientProvider.GetSubmitOrderProxy();
                    var response = proxy.ProcessRequest(new ProcessRequestRequest(holder.BTOrder)).Response;
                    if (response != null && response.OrderID != null)
                    {
                        UpdateTWSKUOrderedQuantity(holder.BTOrder);
                    }
                    var status = response.Status.ToUpper(CultureInfo.InvariantCulture);
                    if (status.Equals("SUCCESS"))
                    {
                        LoggerHelper.Info(
                            string.Format(
                                "Successfully Placed Order. OrderId - {0}", orderNumber));

                        // Update Status in Payment Gateway Record
                        UpdatePaymentGatewayRecord(orderNumber, gatewayResponse, PaymentGatewayLogEntryType.OrderSubmitted, PaymentGatewayRecordStatusType.OrderSubmitted);
                        CatalogProvider.CloseShoppingCart(holder.ShoppingCartId, holder.DistributorId, orderNumber, orderToSubmit.Payments[0].PaymentDate);
                        return true;
                    }
                    else
                    {
                        LoggerHelper.Info(string.Format(
                            "OrderPlacement Failed. OrderId - {0}", orderNumber));
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Unable To Deserialize Order. OrderId - {0}, Error Msg: {1}", orderNumber, ex.Message));
                return false;
            }
            return isSubmitted;
        }

        public static ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceResponse_V01 CreateExternalInvoice(ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceRequest_V01 request)
        {
            try
            {
                var _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceResponse_V01 rsp =
                    (ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceResponse_V01)_chinaOrderProxy.ApplyForExternalOnlineInvoice(new ServiceProvider.OrderChinaSvc.ApplyForExternalOnlineInvoiceRequest(request)).ApplyForExternalOnlineInvoiceResult;
                rsp.Status = ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success;
                return rsp;
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(ex.StackTrace, "a");
                return null;
            }
        }

        public static List<ServiceProvider.OrderChinaSvc.InvoiceInfoObject> GetOrderStatus(string[] orderNumbers, string distributorId)
        {
            try
            {
                if (orderNumbers == null || orderNumbers.Length == 0)
                    return new List<ServiceProvider.OrderChinaSvc.InvoiceInfoObject>();

                //string cacheKey = InvoiceStatusCache + distributorId;
                //var cacheResult= HttpRuntime.Cache[cacheKey] as List<InvoiceInfoObject>;

                //if (cacheResult != null)
                //    return cacheResult;

                ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceRequest_V02 requestV02 = new ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceRequest_V02
                {
                    InvoiceNumberStrings = orderNumbers.ToList()
                };
                var chinaProxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                var rsp = chinaProxy.ApplyForRegisteredNo(new ServiceProvider.OrderChinaSvc.ApplyForRegisteredNoRequest(requestV02)).ApplyForRegisteredNoResult;

                if (rsp != null)
                {
                    var response02 = rsp as ServiceProvider.OrderChinaSvc.ExternalOnlineInvoiceResponse_V02;
                    if (response02 != null)
                    {
                        //HttpRuntime.Cache.Insert(cacheKey, rsp, null, DateTime.Now.AddMinutes(InvoiceStatusCacheMin),
                        //    Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        return response02.InvoiceInfo;
                    }
                }
                return new List<ServiceProvider.OrderChinaSvc.InvoiceInfoObject>();
            }
            catch (Exception ex)
            {
                string.Format(
                    "There is error on GetOrderStatus applying for register number:distributor={0} ,stackt race= {1}",
                    distributorId, ex.StackTrace);
                return new List<ServiceProvider.OrderChinaSvc.InvoiceInfoObject>();
            }
        }

        public static List<ServiceProvider.OrderChinaSvc.RdcConfigObject> GetRdcConfig(int rdcId)
        {
            try
            {
                if (rdcId == 0)
                    return new List<ServiceProvider.OrderChinaSvc.RdcConfigObject>();

                ServiceProvider.OrderChinaSvc.RdcConfigRequestV01 request = new ServiceProvider.OrderChinaSvc.RdcConfigRequestV01();
                request.RdcId = rdcId;

                var chinaProxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                var resp = chinaProxy.GetRdcConfig(new ServiceProvider.OrderChinaSvc.GetRdcConfigRequest(request)).GetRdcConfigResult as ServiceProvider.OrderChinaSvc.RdcConfigResponseV01;

                if (resp != null)
                {
                    return resp.RdcConfigObjects;
                }
                return new List<ServiceProvider.OrderChinaSvc.RdcConfigObject>();
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(string.Format("There is a Error While retrieving the Rdc Config RdcId:{0}, Stack Trace{1}", rdcId, ex.StackTrace), "Error");
                return new List<ServiceProvider.OrderChinaSvc.RdcConfigObject>();
            }
        }

        public static bool IsValidToSubmit(Order_V01 order, MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart.CountryCode == "AR")
            {
                if (order.Messages == null || !order.Messages.Any(m => m.MessageType == "ContributorClass"))
                    return false;

                var cc = order.Messages.FirstOrDefault(m => m.MessageType == "ContributorClass").MessageValue;
                if (!cc.StartsWith("Y") && !cc.StartsWith("N") && !cc.StartsWith("X") && !cc.StartsWith("M") && !cc.StartsWith("I") && !cc.StartsWith("Z"))
                    return false;
            }
            return true;
        }
        public static List<SKUQuantityRestrictInfo> GetOrUpdateSKUsMaxQuantity(SKUQuantityRestrictRequest_V01 req)
        {
            try
            {
                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    SKUQuantityRestrictResponse_V01 resp = proxy.GetSKUQuantityRestrict(req) as SKUQuantityRestrictResponse_V01;
                    if (resp.Status == ServiceResponseStatusType.Success && resp.SKUQuantityRestrictList != null && resp.SKUQuantityRestrictList.Count > 0)
                        return resp.SKUQuantityRestrictList;
                    else
                        return new List<SKUQuantityRestrictInfo>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(string.Format("There is an Error While retrieving SKU Quantity for :{0}, Stack Trace{1}", req.CountryCode, ex.StackTrace), "Error");
                return new List<SKUQuantityRestrictInfo>();
            }
        }
        public static void CreateReceipt()
        {
            var error = string.Empty;
            if (HttpContext.Current.Session["ReceiptModel"] != null)
            {
                string DSid = string.Empty, Locale =string.Empty;
                try
                {
                    var QRCode = HttpContext.Current.Session["ReceiptModel"] as LegacyReceiptModel;
                    DSid = QRCode.MemberId;
                    Locale = QRCode.Locale;
                    string stickyURL = Settings.GetRequiredAppSetting("IAStickyShopBaseUrl");
                    var CountryCode = GetForeignPPVCountryCode(DSid, Locale.Substring(3));
                    string URL = string.Empty;
                    if (!string.IsNullOrWhiteSpace(CountryCode))
                    {
                        URL = CountryCode == "US"
                            ? stickyURL + "/en-US/Shop/Receipts/Api/LegacyReceipt/V1/"
                            : stickyURL + "/es-PR/Shop/Receipts/Api/LegacyReceipt/V1/";
                    }
                    else
                       URL = stickyURL + "/en-US/Shop/Receipts/Api/LegacyReceipt/V1/";
                    string _endpoint = URL;

                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var json = JsonConvert.SerializeObject(QRCode);
                        var stringContent = new System.Net.Http.StringContent(json,
                            UnicodeEncoding.UTF8,
                            "application/json");
                        LoggerHelper.Info(
                            string.Format(
                                "Foreign PPV Request for DS : {0}, Locale : {1}, DS CountryCode : {2}, JasonRequest: {3}",
                                DSid, Locale, CountryCode, json));
                        var response = httpClient.PostAsync(_endpoint, stringContent).Result;
                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            // return result;
                            if (!string.IsNullOrWhiteSpace(CountryCode))
                            {
                                URL = CountryCode == "US"
                                    ? stickyURL + "/en-US/Shop/Receipts/Invoice/Edit/DS/"
                                    : stickyURL + "/es-PR/Shop/Receipts/Invoice/Edit/DS/";
                            }
                            HttpContext.Current.Response.Redirect(URL + result, true);
                           

                        }

                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("CreateLegacyReceipt for Locale {0}, DS {1}, error : {2}", Locale,DSid, ex.Message));
                }
                HttpContext.Current.Session.Remove("ReceiptModel");
            }
        }

        public static LegacyReceiptModel GetReceiptModel(MyHLShoppingCart cart)
        {
            var QRCode = new LegacyReceiptModel
            {
                MemberId = cart.DistributorID,
                Locale =cart.Locale,
                SkuDetailsList = IsEligibleReceiptModelSkus(cart),
                Address = new InvoiceAddressModel
                {
                    Address1= cart.DeliveryInfo.Address.Address.Line1,
                    Address2= cart.DeliveryInfo.Address.Address.Line2,
                    City = cart.DeliveryInfo.Address.Address.City,
                    Country = cart.DeliveryInfo.Address.Address.Country,
                    County = cart.DeliveryInfo.Address.Address.CountyDistrict,
                    PostalCode = cart.DeliveryInfo.Address.Address.PostalCode,
                    State= cart.DeliveryInfo.Address.Address.StateProvinceTerritory
                }

            };
            return QRCode;
        }
        public static List<SkuDetails> IsEligibleReceiptModelSkus(MyHLShoppingCart cart)
        {
            var notapplicableSkus = new List<string> { HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku };
            notapplicableSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
            notapplicableSkus.Add(HLConfigManager.Configurations.APFConfiguration.DistributorSku);
            notapplicableSkus.Add(HLConfigManager.Configurations.APFConfiguration.SupervisorSku);
         //   var skulist = cart.CartItems.Select(x => x.SKU).ToList().Except(notapplicableSkus);
            var skulist = (from c in cart.CartItems
                where !notapplicableSkus.ToArray().Contains(c.SKU)
                select new SkuDetails
                {
                    SkuNumber = c.SKU,
                    SkuQuantity = c.Quantity
                }).ToList();
            skulist = skulist ?? new List<SkuDetails>();
            return skulist;
         
        }

       

        public static string GetForeignPPVCountryCode(string DistributorID, string CountryCode)
        {
           
                var mailingAddress =
                    DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing,
                        DistributorID, CountryCode);
                var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser());
                if ((mailingAddress != null && mailingAddress.Country != null &&
                     (mailingAddress.Country == "US" || mailingAddress.Country == "PR")))
                {
                    return mailingAddress.Country;
                }
                if ((member != null && (member.Value != null) &&
                     (member.Value.ProcessingCountryCode == "US" || member.Value.ProcessingCountryCode == "PR")))
                {
                    return member.Value.ProcessingCountryCode;
                }
                if ((member != null && (member.Value != null) && 
                    member.Value.ResidenceCountryCode == "US" || member.Value.ResidenceCountryCode == "PR"))
                {
                    return member.Value.ResidenceCountryCode;
                }
               
            
            return string.Empty;
        }
    }

    public class CryptographyProvider
    {
        #region Constants and Fields

        private const string HASHALGORITHM = "SHA1";

        private const string INITVECTOR = "@1B2c3D4e5F6g7H8";

        private const int KEYSIZE = 128;

        private const string PASSPHRASE = "herbGlobalOrdering@4321!";

        private const int PASSWORDITERATIONS = 7;

        private const string SALTVALUE = "herbSalt@1Value";

        #endregion

        #region Public Methods and Operators

        public static string Decrypt(string stingToDecrypt)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(INITVECTOR);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SALTVALUE);

            byte[] cipherTextBytes = Convert.FromBase64String(stingToDecrypt);

            PasswordDeriveBytes password = new PasswordDeriveBytes(
                PASSPHRASE, saltValueBytes, HASHALGORITHM, PASSWORDITERATIONS);

            byte[] keyBytes = password.GetBytes(KEYSIZE / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();

            symmetricKey.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

            return plainText;
        }

        public static string Encrypt(string stingToEncrypt)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(INITVECTOR);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SALTVALUE);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(stingToEncrypt);

            PasswordDeriveBytes password = new PasswordDeriveBytes(
                PASSPHRASE, saltValueBytes, HASHALGORITHM, PASSWORDITERATIONS);

            byte[] keyBytes = password.GetBytes(KEYSIZE / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();

            symmetricKey.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            MemoryStream memoryStream = new MemoryStream();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            cryptoStream.FlushFinalBlock();

            byte[] cipherTextBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string cipherText = Convert.ToBase64String(cipherTextBytes);

            return cipherText;
        }

        #endregion
    }    

    //public class FailedCardInfo
    //        {
    //    public string CardNumber { get; set; }

    //    public string CardType { get; set; }

    //    public decimal Amount { get; set; }
    //        }

    //[XmlRoot]
    //public class SerializedOrderHolder2
    //{
    //    public SerializedOrderHolder2()
    //    {
    //    }

    //    public SerializedOrderHolder2(Order btOrder, HL.Order.ValueObjects.Order order)
    //    {
    //        BTOrder = btOrder;
    //        Order = order;
    //    }

    //    [XmlElement]
    //    public Order BTOrder { get; set; }

    //    [XmlElement]
    //    public HL.Order.ValueObjects.Order Order { get; set; }

    //    [XmlElement]
    //    public string DistributorId { get; set; }

    //    [XmlElement]
    //    public Guid Token { get; set; }

    //    [XmlElement]
    //    public string Locale { get; set; }

    //    [XmlElement]
    //    public string Email { get; set; }

    //    [XmlElement]
    //    public int ShoppingCartId { get; set; }


    //    [XmlElement]
    //    public int OrderHeaderId { get; set; }

    //    public static SerializedOrderHolder FromString(string data)
    //    {
    //        return OrderSerializer.DeSerializeOrder(data);
    //    }

    //    public string ToSafeString()
    //    {
    //        return Convert.ToBase64String(new UTF8Encoding().GetBytes(OrderSerializer.SerializeOrder(this)));
    //    }

    //    public static SerializedOrderHolder FromSafeString(string encoded)
    //    {
    //        return OrderSerializer.DeSerializeOrder(new UTF8Encoding().GetString(Convert.FromBase64String(encoded)));
    //    }

    //}
  
}


