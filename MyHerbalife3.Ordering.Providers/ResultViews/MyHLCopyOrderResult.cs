// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyHLCopyOrderResult.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to pass a hl copy order result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    /// Used to pass a hl copy order result.
    /// </summary>
    public class MyHLCopyOrderResult
    {
        /// <summary>
        /// Shopping Cart ID.
        /// </summary>
        public int ShoppingCartID { get; set; }

        /// <summary>
        /// Copied.
        /// </summary>
        public bool Copied { get; set; }

        public bool IsShoppingCartNotEmpty { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MyHLCopyOrderResult()
        { }

        /// <summary>
        /// Initializes a new instance of the MyHLCopyOrderResult class.
        /// </summary>
        public MyHLCopyOrderResult(int shoppingCartID, bool copied, bool isShoppingCartNotEmpty)
        {
            this.ShoppingCartID = shoppingCartID;
            this.Copied = copied;
            this.IsShoppingCartNotEmpty = isShoppingCartNotEmpty;
        }
    }
}