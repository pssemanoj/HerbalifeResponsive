using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileOrderTrackingProvider
    {
        List<ExpressTrackingInfoViewModel> Get(OrderTrackingRequestViewModel request);
    }
}
