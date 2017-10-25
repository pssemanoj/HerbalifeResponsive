using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class AddressListResponseViewModel : MobileResponseViewModel
    {
        public List<AddressViewModel> Address { get; set; }
    }

    public class SaveAddressResponseViewModel
    {
        public AddressViewModel Data { get; set; }
        public ErrorViewModel Error { get; set; }
    }

    public class AddressRequestViewModel
    {
        public AddressViewModel Data { get; set; }
    }

    public class SaveAddressRequestViewModel
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
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
        public DateTime LastUpdatedDate { get; set; }
    }

    public class GetAddressRequestViewModel
    {
        private string _memberId;
        public string MemberId
        {
            get
            {
                return _memberId;
            }
            set
            {
                _memberId = value.ToUpper();
            }
        }
        public string Locale { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }


}