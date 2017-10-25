using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrdersResponseViewModel : MobileResponseViewModel
    {
        public List<OrderViewModel> Orders { get; set; }
        public int RecordCount { get; set; }
        
    }
}