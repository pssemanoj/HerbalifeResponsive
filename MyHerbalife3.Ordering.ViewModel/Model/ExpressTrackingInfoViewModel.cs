using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ExpressTrackingInfoViewModel
    {
        public string ExpressCode { get; set; }
        public string ExpressCompanyName { get; set; }

        public string ExpressNum { get; set; }

        public ExpressTrackingViewModel ExpressTracking { get; set; }

        public string OrderDeliveryType { get; set; }

        public string ReceivingName { get; set; }

        public string ReceivingPhone { get; set; }

        public int? TotalPackageUnits { get; set; }
    }

    public class ExpressTrackingViewModel
    {
        public string mailno { get; set; }
        public string result { get; set; }
        public string time { get; set; }
        public string remark { get; set; }
        public string status { get; set; }
        public string weight { get; set; }

        public List<steps> steps { get; set; }
    }

    public class steps
    {
        public string time { get; set; }
        public string address { get; set; }
        public string station { get; set; }
        public string station_phone { get; set; }
        public string status { get; set; }
        public string remark { get; set; }
        public string next { get; set; }
        public string next_name { get; set; }
    }
}
