using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model.Invoice
{
    public class LegacyReceiptModel
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
        public InvoiceAddressModel Address { get; set; }
        public List<SkuDetails> SkuDetailsList { get; set; }

    }
    public class SkuDetails
    {
        public string SkuNumber { get; set; }
        public int SkuQuantity { get; set; }
    }
}
