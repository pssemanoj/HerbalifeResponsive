namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetSavedCartsByFilter
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
        public string Filter { get; set; }
    }
}