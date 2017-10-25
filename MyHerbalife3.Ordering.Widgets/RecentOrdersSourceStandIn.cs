using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets
{
    public class RecentOrdersSourceStandIn : IRecentOrdersSource
    {
        public List<RecentOrderModel> GetRecentOrders(string id, string countryCode)
        {
            return new List<RecentOrderModel>
                {
                    new RecentOrderModel {Id = "Y10100100", OrderDate = DateTime.Now, Volume = 32.5},
                    new RecentOrderModel {Id = "Y10100101", OrderDate = DateTime.Now, Volume = 31.5},
                    new RecentOrderModel {Id = "Y10100102", OrderDate = DateTime.Now, Volume = 32.5},
                    new RecentOrderModel {Id = "Y10100103", OrderDate = DateTime.Now, Volume = 32.5}
                };
        }
    }
}