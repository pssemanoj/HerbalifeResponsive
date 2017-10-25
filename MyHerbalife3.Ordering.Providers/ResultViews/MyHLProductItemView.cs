namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    ///     Used to pass product item data from server to client.
    /// </summary>
    public class MyHLProductItemView
    {
        /// <summary>
        ///     SKU.
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        ///     Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Quantity.
        /// </summary>
        public int Quantity { get; set; }

        public decimal RetailPrice { get; set; }
    }
}