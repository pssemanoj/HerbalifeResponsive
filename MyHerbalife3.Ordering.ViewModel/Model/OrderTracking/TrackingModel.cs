using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model.OrderTracking
{
    public class TrackingModel
    {
        public BestwayTrackingViewModel BestwayTracking { get; set; }

        public FedExTrackingViewModel FedExTracking { get; set; }

        public SFTrackingViewModel SFTracking { get; set; }

    }

    public class BestwayTrackingViewModel
    {
        public List<traceLogs> traceLogs { get; set; }
    }


    public class traceLogs
    {
        public string logisticProviderID { get; set; }
        public string mailNo { get; set; }

        public List<traces> traces { get; set; }

    }

    public class traces
    {
        public string acceptTime { get; set; }
        public string acceptAddress { get; set; }

        public string scanType { get; set; }
        public string remark { get; set; }

    }

    public class FedExTrackingViewModel
    {
        public tracking tracking { get; set; }
    }

    public class tracking
    {
        public detail detail { get; set; }
    }

    public class detail
    {
        public string tn { get; set; }
        public string ptn { get; set; }
        public string destination { get; set; }
        public string shipdate { get; set; }
        public string sentby { get; set; }
        public string deliveredto { get; set; }
        public string signedforby { get; set; }
        public string service { get; set; }
        public string deliverydate { get; set; }

        public string status { get; set; }

        public List<activity> activities { get; set; }

    }


    public class activity
    {
        public string datetime { get; set; }
        public string scan { get; set; }

        public string location { get; set; }
        public string details { get; set; }

    }

    public class SFTrackingViewModel
    {
        public Response Response { get; set; }

    }

    public class Response
    {
        public string service { get; set; }

        public string Head { get; set; }

        public Body Body { get; set; }

    }

    public class Body
    {
        public RouteResponse RouteResponse { get; set; }
    }

    public class RouteResponse
    {
        public string mailno { get; set; }

        public List<Route> Route { get; set; }
    }

    public class Route
    {
        public string remark { get; set; }
        public string accept_time { get; set; }
        public string accept_address { get; set; }
        public string opcode { get; set; }

    }
}
