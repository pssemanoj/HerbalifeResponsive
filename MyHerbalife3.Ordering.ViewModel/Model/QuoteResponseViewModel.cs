#region

using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class QuoteResponseViewModel : MobileResponseViewModel
    {
        public OrderViewModel Order { get; set; }
        public DualOrderMonthViewModel DualOrderMonth { get; set; }
    }
}