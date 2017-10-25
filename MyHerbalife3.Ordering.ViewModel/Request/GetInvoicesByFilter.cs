using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetInvoicesByFilter
    {
        public InvoiceFilterModel InvoiceFilterModel { get; set; }
        public string MemberId { get; set; }
        public string Locale { get; set; }
    }
}