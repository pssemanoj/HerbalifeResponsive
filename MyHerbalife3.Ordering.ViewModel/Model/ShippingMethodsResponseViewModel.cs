#region

using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ShippingMethodsResponseViewModel : MobileResponseViewModel
    {
        public List<CustomShippingMethodViewModel> ShippingMethods { get; set; }
    }
}