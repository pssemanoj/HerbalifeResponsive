namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class LocalPaymentViewModel : PaymentViewModel
    {
        public string PaymentCode { get; set; }
        public string PaymentId { get; set; }
        public string OrderTrackingUrl { get; set; }
    }
}