namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class DirectDepositPaymentViewModel : PaymentViewModel
    {
        public string ReferenceNumber { get; set; }

        public string PaymentCode { get; set; }
        public string BankName { get; set; }
    }
}