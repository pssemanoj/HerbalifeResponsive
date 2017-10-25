using System;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class MyOrdersViewModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public decimal Volume { get; set; }
        public decimal Total { get; set; }
    }
}