#region

using System;
using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderSummaryRequestViewModel
    {
        public string MemberId { get; set; }

        public string Locale { get; set; }

        public int Limit { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class OrderSummaryResponseViewModel : MobileResponseViewModel
    {
        public List<OrderViewModel> Pending { get; set; }
        public List<OrderViewModel> Failed { get; set; }
        public List<OrderViewModel> Completed { get; set; }
    }
}