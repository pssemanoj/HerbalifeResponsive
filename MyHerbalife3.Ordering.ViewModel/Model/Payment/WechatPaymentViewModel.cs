namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class WechatPaymentViewModel : PaymentViewModel
    {
        public string PaymentCode { get; set; }
        public string PrepayId { get; set; }
    }
}