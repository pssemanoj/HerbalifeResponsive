namespace MyHerbalife3.Ordering.ViewModel.Request
{
    public class GetInvoiceCategoryByFilter
    {
        public string Filter { get; set; }

        public int RootCategoryId { get; set; }

        public string Locale { get; set; }

        public string Type { get; set; }
    }
}