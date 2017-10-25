using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using HL.Blocks.CircuitBreaker;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class RecentOrdersSource : IRecentOrdersSource
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();
        public List<RecentOrderModel> GetRecentOrders(string id, string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }

            var cacheKey = string.Format("MYHL3_RO_{0}_{1}", id, countryCode);
            var result = _cache.Retrieve(_ => GetRecentOrdersFromSource(id, countryCode), cacheKey, TimeSpan.FromMinutes(15));
            return result;
        }

        public List<RecentOrderModel> GetRecentOrdersFromSource(string id, string countryCode)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<RecentOrderModel>();
                var response =
                    circuitBreaker.Execute(() => proxy.GetLatestOrder(new GetLatestOrderRequest1(new GetLatestOrderRequest_V01
                    {
                        CountryCode = countryCode,
                        DistributorId = id
                    })).GetLatestOrderResult as GetLatestOrderResponse_V01);

                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return GetRecentOrdersModel(response.Orders.OfType<Order_V03>().ToList());
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex,
                                       "Errored out in MyRecentOrdersSource" + countryCode);
                if (null != proxy)
                {
                    proxy.Close();
                }
                throw;
            }
            finally
            {
                if (null != proxy)
                {
                    proxy.Close();
                }
            }
            LoggerHelper.Error("Errored out in MyRecentOrdersSource Catalog service" + countryCode);
            return null;
        }

        private static List<RecentOrderModel> GetRecentOrdersModel(IEnumerable<Order_V03> recentOrders)
        {
            var recentOrderList = recentOrders.Select(recentOrder => new RecentOrderModel
                {
                    Id = recentOrder.OrderID,
                    OrderDate = recentOrder.ReceivedDate,
                    Volume = recentOrder.VolumePoints
                }).ToList();
            return recentOrderList;
        }
    }
}