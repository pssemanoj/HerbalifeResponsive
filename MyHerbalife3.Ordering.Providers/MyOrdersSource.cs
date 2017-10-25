#region

using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


#endregion

namespace MyHerbalife3.Ordering.Providers
{
    public class MyOrdersSource : IMyOrdersSource
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();

        public List<MyOrdersViewModel> GetMyOrders(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }
            var cacheKey = string.Format("inv_myorders_{0}", id);
            var result =
                _cache.Retrieve(_ => GetMyOrdersFromSource(id), cacheKey,
                    TimeSpan.FromMinutes(30));
            return result;
        }

        public IEnumerable<MyOrdersViewModel> SearchMyOrders(string filter, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }

            var orders = GetMyOrders(id);

            if (null == orders)
            {
                return null;
            }

            var result = string.IsNullOrEmpty(filter) ? orders : orders.Where(o => o.Id == filter);
            return result;
        }

        private List<MyOrdersViewModel> GetMyOrdersFromSource(string id)
        {
            var orders = OrdersProvider.GetOrders(id, string.Empty, null, null, false, "DESC");
            var myOrders= ConvertToMyorders(orders);
            if (null != myOrders && myOrders.Any())
            {
                return myOrders.OrderByDescending(o => o.Date).ToList();
            }
            return new List<MyOrdersViewModel>();
        }

        private static IEnumerable<MyOrdersViewModel> ConvertToMyorders(IEnumerable<Order_V02> orders)
        {
            return orders.Select(order => new MyOrdersViewModel
            {
                Date = order.ReceivedDate,
                Id = order.OrderID,
                Name = order.Shipment != null ? ((ShippingInfo_V01)order.Shipment).Recipient : string.Empty,
                Status =
                    !string.IsNullOrEmpty(order.OrderStatus) && order.OrderStatus.Contains("|")
                        ? order.OrderStatus.Split('|')[1]
                        : order.OrderStatus,
                Volume = order.Pricing != null ? (order.Pricing as OrderTotals_V01).VolumePoints : 0,
                Total = order.Pricing != null ? (order.Pricing as OrderTotals_V01).AmountDue : 0,
            });
        }
    }
}