namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class UpdateInvoiceModelResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorCodeKey { get; set; }
        public InvoiceModel InvoiceModel { get; set; }
        public bool IsAddressCorrected { get; set; }
    }
}