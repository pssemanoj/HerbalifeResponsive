// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyHlShoppingCartViewResult.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to pass a hl orders view result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    /// Used to pass a hl shopping cart view result.
    /// </summary>
    public class MyHlShoppingCartViewResult : MyHlResultViewResult
    {
        /// <summary>
        /// Result list.
        /// </summary>
        public List<MyHLShoppingCartView> ResultList { get; set; }

        /// <summary>
        /// Initializes a new instance of the MyHlOrdersViewResult class.
        /// </summary>
        public MyHlShoppingCartViewResult(int totalRows, List<MyHLShoppingCartView> resultList)
            : base(totalRows)
        {
            this.ResultList = resultList;
        }

        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}