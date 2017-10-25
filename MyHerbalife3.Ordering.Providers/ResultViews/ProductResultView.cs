// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductsResultView.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to pass a products details list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyHerbalife3.Ordering.Providers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Product result view class.
    /// </summary>
    [Serializable]
    public class ProductsResultView
    {
        /// <summary>
        /// Gets or sets a products list.
        /// </summary>
        public IEnumerable<string> ProductsList { get; set; }

        /// <summary>
        /// Gets or sets a products list.
        /// </summary>
        public IEnumerable<string> SkuList { get; set; }
    }
}