using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;

namespace MyHerbalife3.Ordering.SharedProviders
{
    /// <summary>
    /// The coupon status.
    /// </summary>
    public enum CouponStatus
    {
        /// <summary>
        /// The active.
        /// </summary>
        Active,

        /// <summary>
        /// The deactivated.
        /// </summary>
        Deactivated,

        /// <summary>
        /// The expired.
        /// </summary>
        Expired,

        /// <summary>
        /// The scheduled.
        /// </summary>
        Scheduled
    }

    public static class DistributorWebsiteProvider
    {
        /// <summary>
        /// The get available discounts.
        /// </summary>
        /// <param name="distributorId">
        /// The distributor id.
        /// </param>
        /// <returns>
        /// List of customer discounts
        /// </returns>
        public static List<CustomerDiscountDefinition> GetAvailableDiscounts(string distributorId)
        {
            if (string.IsNullOrEmpty(distributorId))
            {
                return null;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new GetCustomerDiscountsByDistributor { DistributorID = distributorId };
                    var response = proxy.GetCustomerDiscounts(new GetCustomerDiscountsRequest1(request)).GetCustomerDiscountsResult as GetCustomerDiscountsResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Discounts.ToList();
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("GetAvailableDiscounts DS:{0} ERR:{1}", distributorId, ex));
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// The get coupon status.
        /// </summary>
        /// <param name="isActive">
        /// The is active.
        /// </param>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        /// <returns>
        /// Coupon Status
        /// </returns>
        public static CouponStatus GetCouponStatus(bool isActive, DateTime? startDate, DateTime? endDate)
        {
            if (!isActive)
            {
                return CouponStatus.Deactivated;
            }
            {
                if (startDate > DateTime.UtcNow)
                {
                    return CouponStatus.Scheduled;
                }

                if (endDate < DateTime.UtcNow)
                {
                    return CouponStatus.Expired;
                }
            }

            return CouponStatus.Active;
        }


        /// <summary>
        /// The get customer orders.
        /// </summary>
        /// <param name="distributorID">
        /// The distributor id.
        /// </param>
        /// <returns>
        /// </returns>
        public static List<CustomerOrder_V01> GetCustomerOrders(string distributorID)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new GetCustomerOrderRequest_V01();
                    var filter = new CustomerOrdersByDistributor { DistributorId = distributorID };
                    request.Filter = filter;
                    var response = proxy.GetOrders(new GetOrdersRequest(request)).GetOrdersResult as GetCustomerOrderResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Orders.ToList().Cast<CustomerOrder_V01>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);

                    // ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    LoggerHelper.Error(string.Format("GetCustomerOrders DS:{0} ERR:{1}", distributorID, ex));
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// The save customer order.
        /// </summary>
        /// <param name="customerOrder">
        /// The customer order.
        /// </param>
        /// <returns>
        /// The save customer order.
        /// </returns>
        public static bool SaveCustomerOrder(CustomerOrder_V01 customerOrder)
        {
            if (customerOrder == null)
            {
                return false;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new SaveCustomerOrderRequest_V01 { Order = customerOrder };
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
                    LoggerHelper.Error(string.Format("SaveCustomerOrder OrderID:{0} ERR:{1}", customerOrder.OrderID, ex));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// The save discount.
        /// </summary>
        /// <param name="coupon">
        /// The coupon.
        /// </param>
        /// <param name="distributorId">
        /// The distributor id.
        /// </param>
        /// <returns>
        /// The save discount.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static bool SaveDiscount(CustomerDiscountDefinition coupon, string distributorId)
        {
            if (coupon == null)
            {
                return false;
            }

            using (var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy())
            {
                try
                {
                    var request = new SaveCustomerDiscountRequest_V01
                    {
                        Discount = coupon,
                        DistributorID = distributorId
                    };
                    var response = proxy.SaveCustomerDiscount(new SaveCustomerDiscountRequest1(request));
                    if (response.SaveCustomerDiscountResult.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }

                    if (response.SaveCustomerDiscountResult.Status.ToString() == "Failure, ClientFault")
                    {
                        foreach (var parameterError in response.SaveCustomerDiscountResult.ParameterErrorList)
                        {
                            if (parameterError.Name.Equals("Discount Code"))
                            {
                                throw new ArgumentException("Duplicate coupon code");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType().Equals(typeof(ArgumentException)))
                    {
                        throw;
                    }
                    LoggerHelper.Error(string.Format("SaveDiscount DS ID:{0} ERR:{1}", distributorId, ex));
                    return false;
                }
            }

            return false;
        }

    }
}
