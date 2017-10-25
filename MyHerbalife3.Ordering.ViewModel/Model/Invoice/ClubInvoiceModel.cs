#region

using System;
using System.Collections.Generic;
using System.Globalization;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ClubInvoiceModel : ICloneable
    {
        public string ClubRecieptTotalVolumePoints { get; set; }
        public string ClubRecieptQuantity { get; set; }
        public string ClubRecieptProductName { get; set; }
        public string ClubRecieptDisplayTotalDue { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}