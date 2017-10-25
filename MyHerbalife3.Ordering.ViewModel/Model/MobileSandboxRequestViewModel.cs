using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class MobileSandboxRequestViewModel
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string DistributorId { get; set; }
        public string Locale { get; set; }
        public string AppName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
