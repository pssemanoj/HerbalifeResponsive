using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetCRMAddress
    {
        public string MemberId { get; set; }
        public string CountryCode { get; set; }
    }
}
