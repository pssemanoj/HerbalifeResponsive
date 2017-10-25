#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceEditModel
    {
        public int InvoiceId { get; set; }
        public string OrderId { get; set; }
        public int CopyInvoiceId { get; set; }
        public string Action { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}