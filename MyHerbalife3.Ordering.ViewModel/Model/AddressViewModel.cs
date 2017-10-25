#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class AddressViewModel
    {
        public Guid CloudId { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountyDistrict { get; set; }
        public string PostalCode { get; set; }
        public string StateProvinceTerritory { get; set; }
        public string NickName { get; set; }
        public bool IsPrimary { get; set; }
        public string PersonCloudId { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime LastUpdatedDate { get; set; }

        public string Type
        {
            get { return "GDO-Address"; }
        }

        public List<CustomShippingMethodViewModel> CustomShippingMethods { get; set; }
    }
}