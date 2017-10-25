namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetInvoiceById
    {
        public int Id { get; set; }
        public string MemberId { get; set; }
        public string Locale { get; set; }
    }
}