#region

using System;
using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PreferredCustomerViewModel
    {
        public string CustomerId { get; set; }
        public string MemberId { get; set; }
        public string Name { get; set; }
    }

    public class PreferredCustomerRequestViewModel : BaseRequestViewModel
    {
        public string MemberId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class PreferredCustomerResponseViewModel : MobileResponseViewModel
    {
        public List<PreferredCustomerViewModel> Customers { get; set; }
    }
}