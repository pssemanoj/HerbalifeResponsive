namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class WechatPrepayRequestModel
    {
        public WechatPrepayIdViewModel Prepay { get; set; }
        public OrderViewModel Order { get; set; }
    }

    public class WechatPrepayIdViewModel
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
        public string Ip { get; set; }
        public string Body { get; set; }
        public decimal TotalFee { get; set; }
    }
}