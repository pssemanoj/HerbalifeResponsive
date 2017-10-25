#region

using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PickUpResponseViewModel
    {
        public PickupViewModel Data { get; set; }
        public ErrorViewModel Error { get; set; }
    }

    public class PickUpListResponseViewModel : MobileResponseViewModel
    {
        public List<PickupViewModel> Pickup { get; set; }
    }
}