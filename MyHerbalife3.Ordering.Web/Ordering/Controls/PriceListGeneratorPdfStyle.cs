// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceListGeneratorPdfStyle.aspx.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Used to provide the styles to the PDF price list generator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Price list generator Pdf Styles.
    /// </summary>
    public class PriceListGeneratorPdfStyle
    {
        /// <summary>
        /// The category name style.
        /// </summary>
        public static string CategoryNameStyle = "font-size: 22pt;color: white; background-color: #7A7A7A; font-weight: bold; width: 100%;font-family: Helvetica;text-transform:uppercase;padding: 10px;";

        /// <summary>
        /// The sub category name style
        /// </summary>
        public static string SubCategoryNameStyle = "font-family: Helvetica;font-size:20pt;color:#555555;";

        /// <summary>
        /// The distributor name style.
        /// </summary>
        public static string DistributorNameStyle = "float: left; font-family: Helvetica; text-align: right; font-weight: bold; font-size: 12pt";

        /// <summary>
        /// The tax information style.
        /// </summary>
        public static string TaxInformationStyle = "font-family: Helvetica; text-align: right; font-size: 10pt";

        /// <summary>
        /// The logo image style
        /// </summary>
        public static string LogoImageStyle = "width:250px; float: left;";
    }
}