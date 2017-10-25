using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface IMyOrdersSource
    {
        List<MyOrdersViewModel> GetMyOrders(string id);
        IEnumerable<MyOrdersViewModel> SearchMyOrders(string filter, string id);
    }
}