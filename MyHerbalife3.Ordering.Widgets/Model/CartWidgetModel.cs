namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class CartWidgetModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VolumePoints { get; set; }
        public string Sku { get; set; }
        public string DisplaySubtotal { get; set; }
    }
}
