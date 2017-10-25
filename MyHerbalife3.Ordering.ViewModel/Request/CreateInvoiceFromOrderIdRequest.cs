namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class CreateInvoiceFromOrderIdRequest
    {
        public string OrderId { get; set; }
        public string Source { get; set; }
    }
}