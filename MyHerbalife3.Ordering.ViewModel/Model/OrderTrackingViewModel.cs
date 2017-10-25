using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderTrackingRequestViewModel
    {
        public string MemberId { get; set; }

        public string Locale { get; set; }

        public string OrderId { get; set; }
    }

    public class OrderTrackingResponseViewModel : MobileResponseViewModel
    {
        public string MemberId { get; set; }

        public string Locale { get; set; }

        public string OrderId { get; set; }

        public List<ExpressTrackingInfoViewModel> ExpressInfo { get; set; }
    }

}
