using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public static class CustomerOrderingProvider
    {
        /// <summary>
        /// The get customer order by order id.
        /// </summary>
        /// <param name="orderId">
        /// The order id.
        /// </param>
        /// <returns>
        /// Customer Order Object
        /// </returns>
        public static CustomerOrder_V01 GetCustomerOrderByOrderID(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return null;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new GetCustomerOrderRequest_V01();
                    var filter = new CustomerOrderById { CustomerOrderId = orderId };
                    request.Filter = filter;
                    var response = proxy.GetOrders(new GetOrdersRequest(request)).GetOrdersResult as GetCustomerOrderResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Orders.ToList().Cast<CustomerOrder_V01>().ToList().FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("GetCustomerOrderByOrderID OrderID:{0} ERR:{1}", orderId, ex));
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// The update customer order status.
        /// </summary>
        /// <param name="customerOrderID">
        /// The customer order id.
        /// </param>
        /// <param name="previousStatus">
        /// The previous status.
        /// </param>
        /// <param name="newStatus">
        /// The new status.
        /// </param>
        /// <returns>
        /// The update customer order status.
        /// </returns>
        public static bool UpdateCustomerOrderStatus(
            string customerOrderID, CustomerOrderStatusType previousStatus, CustomerOrderStatusType newStatus)
        {
            if (string.IsNullOrEmpty(customerOrderID))
            {
                return false;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new SaveCustomerOrderStatusRequest_V01
                    {
                        OrderId = customerOrderID,
                        FromStatus = previousStatus,
                        ToStatus = newStatus
                    };

                    var response = proxy.SaveOrder(new SaveOrderRequest(request)).SaveOrderResult as SaveCustomerOrderStatusResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("UpdateCustomerOrderStatus OrderID:{0} ERR:{1}", customerOrderID, ex));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// The update customer order tags.
        /// </summary>
        /// <param name="customerOrderID">
        /// The customer order id.
        /// </param>
        /// <param name="orderTag">
        /// The order tag.
        /// </param>
        /// <returns>
        /// The update customer order tags.
        /// </returns>
        public static bool UpdateCustomerOrderTags(string customerOrderID, CustomerOrderStatusTag orderTag)
        {
            if (string.IsNullOrEmpty(customerOrderID))
            {
                return false;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new SavePartialCustomerOrderRequest
                    {
                        OrderId = customerOrderID,
                        Updates =
                        new[]
                                    {
                                        new CustomerOrderPropertyValue
                                            {
                                                Property = CustomerOrderProperty.CustomerOrderStatusTag, 
                                                Value =
                                                    (int)
                                                    Enum.Parse(
                                                        typeof(CustomerOrderStatusTag), 
                                                        Enum.GetName(typeof(CustomerOrderStatusTag), orderTag))
                                            }
                                    }.ToList()
                    };

                    var response = proxy.SaveOrder(new SaveOrderRequest(request)).SaveOrderResult as SaveCustomerOrderResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("UpdateCustomerOrderTags OrderID:{0} ERR:{1}", customerOrderID, ex));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// The update customer order distributor order id.
        /// </summary>
        /// <param name="customerOrderID">
        /// The customer order id.
        /// </param>
        /// <param name="distributorOrderID">
        /// The distributor order id.
        /// </param>
        /// <returns>
        /// The update customer order distributor order id.
        /// </returns>
        public static bool UpdateCustomerOrderDistributorOrderID(string customerOrderID, string distributorOrderID)
        {
            if (string.IsNullOrEmpty(customerOrderID) || string.IsNullOrEmpty(distributorOrderID))
            {
                return false;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new SavePartialCustomerOrderRequest
                    {
                        OrderId = customerOrderID,
                        Updates =
                            new[]
                                    {
                                        new CustomerOrderPropertyValue
                                            {
                                                Property = CustomerOrderProperty.DistributorOrderId, 
                                                Value = distributorOrderID
                                            }
                                    }.ToList()
                    };

                    var response = proxy.SaveOrder(new SaveOrderRequest(request)).SaveOrderResult as SaveCustomerOrderResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("UpdateCustomerOrderTags OrderID:{0} ERR:{1}", customerOrderID, ex));
                    return false;
                }
            }

            return false;
        }
    }
}
