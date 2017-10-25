// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyHlResultViewResult.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to pass a hl items view result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    /// Used to pass a hl shopping cart view result.
    /// </summary>
    public abstract class MyHlResultViewResult
    {
        /// <summary>
        /// Total rows.
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Initializes a new instance of the MyHlResultViewResult class.
        /// </summary>
        public MyHlResultViewResult(int totalRows)
        {
            this.TotalRows = totalRows;
        }
    }
}