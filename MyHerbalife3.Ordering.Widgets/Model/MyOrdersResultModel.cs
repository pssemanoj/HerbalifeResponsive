using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class MyOrdersResultModel
    {
        public IEnumerable<MyOrdersViewModel> Items { get; set; }
        public int TotalCount { get; set; }
    }
}