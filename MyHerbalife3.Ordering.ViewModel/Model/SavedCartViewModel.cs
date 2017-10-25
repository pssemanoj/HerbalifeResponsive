#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class SavedCartViewModel
    {
        public int Id{ get; set; }
        public DateTime SavedDate { get; set; }
        public string Name { get; set; }
        public string ShipToName { get; set; }
        public string OrderMonth { get; set; }
        public decimal VolumePoints { get; set; }
    }
}