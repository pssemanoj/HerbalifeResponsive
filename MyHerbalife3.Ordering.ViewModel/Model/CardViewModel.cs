using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class CardViewModel
    {
        public string StorablePan { get; set; }
        public string DistributorName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsCardBinded { get; set; }
        public string CNID { get; set; }
    }
}
