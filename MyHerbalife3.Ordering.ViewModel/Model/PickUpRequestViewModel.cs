using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PickUpRequestViewModel
    {
        public string Locale { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string MemberId { get; set; }
        public string District { get; set; }
    }


    public class ModifyPickUpRequestViewModel
    {
        public string Locale { get; set; }
        public PickupViewModel PickUpModel { get; set; }
        public string MemberId { get; set; }

    }

}
