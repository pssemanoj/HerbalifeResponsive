using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface IRecentOrdersSource
    {
        List<RecentOrderModel> GetRecentOrders(string id, string countryCode);
    }
}