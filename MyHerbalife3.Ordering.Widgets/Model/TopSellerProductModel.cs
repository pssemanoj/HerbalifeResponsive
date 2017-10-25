namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class TopSellerProductModel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CurrencySymbol { get; set; }
        public string DisplayPrice { get; set; }
    }
}
