namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetMemberAddressById
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
        public string Country { get; set; }
    }
}