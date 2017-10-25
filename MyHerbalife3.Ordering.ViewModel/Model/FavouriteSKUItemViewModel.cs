namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class FavouriteSKUItemViewModel
    {
        public string MemberId { get; set; }
        public string Locale { get; set; }
        public string DistributorID { get; set; }
        public int FavouriteID { get; set; }
	    public string ProductSKU { get; set; }
        public int ProductID { get; set; }
    }
    public class FavouriteSKUItemResponseViewModel
    {
        public int FavouriteID { get; set; }
        public string ProductSKU { get; set; }
        public int ProductID { get; set; }
    }
}