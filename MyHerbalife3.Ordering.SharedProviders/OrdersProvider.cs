using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Caching;
using HL.Blocks.CircuitBreaker;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public class OrdersProvider
    {
        public const string CachePrefix = "OnlineCardTypes_";
        public const int CardTypesCacheMinutes = 1440;

        public static Order_V02 GetOrderDetail(string orderNumber)
        {
            Order_V02 order = null;
            var proxy = ServiceClientProvider.GetOrderServiceProxy();

                try
                {
                    using (new OperationContextScope(proxy.InnerChannel))
                    {
                        var req = new GetOrderDetailRequest_V01();
                        req.OrderNumber = orderNumber;
                        req.Locale = Thread.CurrentThread.CurrentCulture.Name;
                        var response = proxy.GetOrderDetail(new GetOrderDetailRequest1(req)).GetOrderDetailResult as GetOrderDetailResponse_V01;
                        if (response != null)
                        {
                            return response.Order;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
                finally
                {
                    ServiceClientFactory.Dispose(proxy);
                }

            return order;
        }

        private static List<Order_V02> sortList(string sortOrder, List<Order_V02> orderList)
        {
            var sortParts = sortOrder.Split(' ');
            if (sortParts.Length != 2)
                return orderList;
            string order = sortParts[1];
            string orderBy = sortParts[0];
            IEnumerable varSortList = null;
            switch (orderBy)
            {
                case "ReceivedDate":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => o.ReceivedDate)
                                      : orderList.OrderByDescending(o => o.ReceivedDate);
                    break;
                case "OrderMonth":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => o.OrderMonth)
                                      : orderList.OrderByDescending(o => o.OrderMonth);
                    break;
                case "OrderId":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => o.OrderID)
                                      : orderList.OrderByDescending(o => o.OrderID);
                    break;
                case "OrderStatus":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => o.OrderStatus)
                                      : orderList.OrderByDescending(o => o.OrderStatus);
                    break;
                case "PurchaserInfo.PurchaserName":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => o.PurchaserInfo.PurchaserName)
                                      : orderList.OrderByDescending(o => o.PurchaserInfo.PurchaserName);
                    break;
                case "Shipment.Recipient":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => ((ShippingInfo_V01) o.Shipment).Recipient)
                                      : orderList.OrderByDescending(o => ((ShippingInfo_V01) o.Shipment).Recipient);
                    break;
                case "Pricing.VolumePoints":
                    varSortList = order == "ASC"
                                      ? orderList.OrderBy(o => ((OrderTotals_V01) o.Pricing).VolumePoints)
                                      : orderList.OrderByDescending(o => ((OrderTotals_V01) o.Pricing).VolumePoints);
                    break;
            }
            return (List<Order_V02>) varSortList;
        }

        public static List<Order_V02> GetOrders(string distributorId,
                                                string distributorType,
                                                DateTime? startDate,
                                                DateTime? endDate,
                                                bool isHapOrder,
                                                string sortOrder)
        {
            GetOrdersResponse_V01 response;
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            
            try
            {
                //TODO: Remove scope? this is not being used... -Manuel Sauceda
                using (var scope = new OperationContextScope(proxy.InnerChannel))
                {
                    //var mhg = new MessageHeader<string>(authToken);
                    //var untyped = mhg.GetUntypedHeader("AuthToken", "ns");
                    //OperationContext.Current.OutgoingMessageHeaders.Add(untyped);

                    var sDate = (startDate == null) ? DateTime.Now.AddMonths(-3) : startDate.Value;
                    var eDate = (endDate == null) ? DateTime.Now : endDate.Value;

                    response = proxy.GetOrders(new GetOrdersRequest1(new GetOrdersRequest_V01
                        {
                            OrderFilter = new OrdersByDateRange
                                {
                                    DistributorId = distributorId,
                                    DistributorType = distributorType,
                                    StartDate = sDate,
                                    EndDate = eDate,
                                    IsHAPOrder = isHapOrder
                                }
                        })).GetOrdersResult as GetOrdersResponse_V01;

                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Orders;
                    }

                    string debugLine = " DistributorId=" + distributorId + "; DistributorType=" + distributorType +
                                       "; IsHAPOrder=" + isHapOrder;
                    throw new ApplicationException(
                        "OrderProvider.GetOrders() Error. Unsuccessful result from web service. Data: " + debugLine);
                }
            }
            catch (Exception ex)
            {
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            finally
            {
                ServiceClientFactory.Dispose(proxy);
            }

            return null;
        }

        public static List<PendingOrder> GetOrdersInProcessing(string distributorId, string locale)
        {
            var request = new GetPendingOrdersRequest_V01();
            var response = new GetPendingOrdersResponse_V01();
            var pendingOrders = new List<PendingOrder>();

            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    using (var scope = new OperationContextScope(proxy.InnerChannel))
                    {
                        request.DistributorId = distributorId;
                        request.CountryCode = locale;
                        response = proxy.GetOrders(new GetOrdersRequest1(request)).GetOrdersResult as GetPendingOrdersResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            pendingOrders = response.PendingOrders;
                        }
                        else
                        {
                            string debugLine = " DistributorId=" + distributorId +
                                               "; Errored while fetching pending payments orders";
                            throw new ApplicationException("OrdersProvider.GetOrders() Data: " + debugLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }

            return pendingOrders;
        }

        public static bool InMaintenance()
        {
            bool isMaintenance = false;

            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var response = proxy.InMaintenance(new InMaintenanceRequest());
                    if (response.InMaintenanceResult.Status == ServiceResponseStatusType.Success)
                    {
                        isMaintenance = response.InMaintenanceResult.InMaintenance;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
            }
            return isMaintenance;
        }

        #region HPSInfoService methods

        /// <summary>Get a list of valid online Card Types from HPS for a country</summary>
        /// <param name="IsoCountryCode"></param>
        /// <returns></returns>
        public static List<HPSCreditCardType> GetOnlineCreditCardTypes(string IsoCountryCode)
        {
            return GetFromCache(IsoCountryCode);
        }

        private static List<HPSCreditCardType> GetFromCache(string IsoCountryCode)
        {
            List<HPSCreditCardType> list = null;
            string cacheKey = GetCacheKey(IsoCountryCode);
            list = HttpRuntime.Cache[cacheKey] as List<HPSCreditCardType>;
            if (null == list || list.Count == 0)
            {
                list = LoadFromService(IsoCountryCode);
                HttpRuntime.Cache.Insert(cacheKey, list, null, DateTime.Now.AddMinutes(CardTypesCacheMinutes), Cache.NoSlidingExpiration);
            }

            return list;
        }

        private static List<HPSCreditCardType> LoadFromService(string isoCountryCode)
        {
            using (var orderProxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var req = new GetCardTypesForCountryRequest_V01()
                    {
                        CountryCode = isoCountryCode,
                        OnlineOnly = true,
                    };

                    var response = orderProxy.GetCardTypesForCountry(new GetCardTypesForCountryRequest1(req)).GetCardTypesForCountryResult as GetCardTypesForCountryResponse_V01;

                    if (response != null && response.HPSCreditCardTypes != null)
                    {
                        return response.HPSCreditCardTypes;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext<IOrderService>(ex, orderProxy);
                }
            }

            return null;
        }

        private static string GetCacheKey(string key)
        {
            return string.Concat(CachePrefix, key);
        }

        #endregion HPSInfoService methods

        public static BankSlipData GetBankSlipData(string orderNumber)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();

            try
            {
                var request = new GetBankSlipReprintDataRequest_V01
                    {
                        OrderNumber = orderNumber
                    };

                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetBankSlipReprintDataResponse_V01>();

                var response =
                    circuitBreaker.Execute(() => proxy.GetBankSlipReprintData(new GetBankSlipReprintDataRequest1(request))).GetBankSlipReprintDataResult as
                    GetBankSlipReprintDataResponse_V01;

                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return response.BankSlipData;
                }
            }
            catch (Exception ex)
            {
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            finally
            {
                if (null != proxy)
                {
                    if (proxy.State == CommunicationState.Faulted)
                    {
                        proxy.Abort();
                    }
                    else
                    {
                        proxy.Close();
                    }
                }
            }

            return null;
        }
    }
}