namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PayPalPaymentViewModel : PaymentViewModel
    {
        public string BillingAgreementId { get; set; }

        public string PayPalId { get; set; }

        public string SolutionType { get; set; }
    }
}