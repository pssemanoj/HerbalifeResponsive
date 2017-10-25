// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceListGeneratorPrintable.aspx.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Used to provide the code behind functionality for the price list generator printable feature.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    using ExpertPdf.HtmlToPdf;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Web.Security;
    using System.Web.UI;
    using System.IO;
    using System.Text;
    using System.Web.UI.WebControls;

    using MyHerbalife3.Ordering.Providers.ResultViews;

    /// <summary>
    /// Price list generator page to print.
    /// </summary>
    public partial class PriceListGeneratorPrintable : System.Web.UI.Page
    {
        /// <summary>
        /// View modes
        /// </summary>
        private enum ViewMode
        {
            Distributor,
            Customer,
            CustomerWithDiscount
        }

        /// <summary>
        /// Gets or sets the price list view.
        /// </summary>
        /// <value>
        /// The price list view.
        /// </value>
        private List<PriceListItemView> PriceListView
        {
            get
            {
                return Session["PriceListViewToPrint"] != null
                           ? Session["PriceListViewToPrint"] as List<PriceListItemView>
                           : null;
            }
        }

        /// <summary>
        /// Gets the tax geo information.
        /// </summary>
        /// <value>
        /// The tax geo information.
        /// </value>
        private string TaxGeoInformation
        {
            get { return Request.QueryString["TaxGeoInformation"] ?? null; }
        }

        /// <summary>
        /// Gets the sales tax.
        /// </summary>
        /// <value>
        /// The sales tax.
        /// </value>
        private string SalesTax
        {
            get { return Request.QueryString["SalesTax"] ?? null; }
        }

        /// <summary>
        /// Gets the shipping and handling tax.
        /// </summary>
        /// <value>
        /// The shipping and handling tax.
        /// </value>
        private string ShippingAndHandlingTax
        {
            get { return Request.QueryString["SHTax"] ?? null; }
        }

        /// <summary>
        /// Gets the distributor profile model.
        /// </summary>
        /// <value>
        /// The distributor profile model.
        /// </value>
        private DistributorProfileModel DistributorProfileModel
        {
            get
            {
                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                return membershipUser != null ? membershipUser.Value : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [customer view mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [customer view mode]; otherwise, <c>false</c>.
        /// </value>
        private ViewMode DocumentViewMode
        {
            get
            {
                var viewMode = ViewMode.Distributor;
                if (Request.QueryString["ViewMode"] != null)
                {
                    Enum.TryParse(Request.QueryString["ViewMode"], out viewMode);
                }

                return viewMode;
            }
        }

        private bool ShowRetailinCustomerMode
        {
            get
            {
                var customerMode = false;
                if (Request.QueryString["CustomerView"] != null)
                {
                    bool.TryParse(Request.QueryString["CustomerView"], out customerMode);
                }

                return customerMode;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [export when render].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [export when render]; otherwise, <c>false</c>.
        /// </value>
        private bool ExportWhenRender
        {
            get
            {
                var exportWhenRender = false;
                if (Request.QueryString["ExportWhenRender"] != null)
                {
                    bool.TryParse(Request.QueryString["ExportWhenRender"], out exportWhenRender);
                }

                return exportWhenRender;
            }
        }

        private string MultipleTax
        {
            get { return Request.QueryString["MultipleTax"] ?? null; }
        }

        private bool ExportXLS
        {
            get
            {
                bool exportXLS = false;
                if(Request.QueryString["ExportXLS"] != null)
                {
                    bool.TryParse(Request.QueryString["ExportXLS"], out exportXLS);
                }
                return exportXLS;
            }
        }


        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Grouping the categories with products.
            if (this.PriceListView != null)
            {
                var categoryDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, List<PriceListItemView>>>>();
                this.PriceListView.GroupBy(i => i.Category).Distinct().ToList().ForEach(i => categoryDictionary.Add(i.Key, new Dictionary<string, Dictionary<string, List<PriceListItemView>>>()));
                this.PriceListView.GroupBy(i => i.SubCategory).Distinct().ToList().ForEach(i =>
                {
                    var subCat = i.First();
                    categoryDictionary[subCat.Category].Add(i.Key, new Dictionary<string, List<PriceListItemView>>());
                });
                this.PriceListView.GroupBy(i => i.ProductName).Distinct().ToList().ForEach(i =>
                {
                    var productName = i.First();
                    categoryDictionary[productName.Category][productName.SubCategory].Add(i.Key, i.Select(item => item).ToList());
                });

                if (!string.IsNullOrEmpty(this.MultipleTax))
                {
                    var msg = this.GetLocalResourceObject("MultipleTaxMessage.Text") as string;
                    if (msg != null)
                    {
                        TaxInformation.Text = string.Format(msg, this.ShippingAndHandlingTax, DateTime.Now.ToShortDateString());
                    }
                }
                else if (!string.IsNullOrEmpty(this.SalesTax) && !string.IsNullOrEmpty(this.ShippingAndHandlingTax))
                {
                    var msg = this.GetLocalResourceObject("TaxInformation.Text") as string;
                    if (msg != null)
                    {
                        TaxInformation.Text = string.Format(msg, this.SalesTax, this.ShippingAndHandlingTax, DateTime.Now.ToShortDateString());
                    }
                }

                reCategory.DataSource = categoryDictionary;
                reCategory.DataBind();
            }

            // Header settings.
            lblTaxGeoInformation.Text = this.TaxGeoInformation;
            // Name now comes from Profile model.
            if (DistributorProfileModel != null)
            {
                lblDistributorName.Text = DistributorProfileModel.DistributorName();             
            }

            btnSaveXLS.Visible = ExportXLS;
            btnPrint.Visible = !ExportXLS;
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.HtmlTextWriter" /> object and calls on the child controls of the <see cref="T:System.Web.UI.Page" /> to render.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the page content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.ExportWhenRender)
            {
                // sb.AppendLine(@"@import url(" + AbsoluteCssPath1 + "container.css);");

                // Getting the HTML to export.
                var sb = new StringBuilder();
                form1.RenderControl(new HtmlTextWriter(new StringWriter(sb)));

                var pdfBytes = GenerateInvoicePdf(sb.ToString(), "tJ+GlIaGlIWFlIKahJSHhZqFhpqNjY2N");

                if (null != pdfBytes && pdfBytes.Length > 0)
                {
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.Charset = "";
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Pragma", "public");
                    Response.BinaryWrite(pdfBytes);
                    Response.Flush();

                    try
                    {
                        Response.End();
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                base.Render(writer);
            }
        }

        /// <summary>
        /// Generates the invoice PDF.
        /// </summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <returns></returns>
        public static byte[] GenerateInvoicePdf(string htmlContent, string licenseKey)
        {
            var pdfConverter = new PdfConverter
            {
                LicenseKey = licenseKey,
                InternetSecurityZone = InternetSecurityZone.LocalMachine
            };

            var pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(htmlContent);
            return pdfBytes;
        }

        /// <summary>
        /// Categories the created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void CategoryCreated(Object sender, RepeaterItemEventArgs e)
        {
            var category = e.Item.DataItem as KeyValuePair<string, Dictionary<string, Dictionary<string, List<PriceListItemView>>>>?;
            var reProductCategory = e.Item.FindControl("reProductCategory") as Repeater;
            
            if (reProductCategory != null && category != null)
            {
                reProductCategory.DataSource = category.Value.Value;
                reProductCategory.DataBind();
            }
        }

        /// <summary>
        /// Products the category created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void ProductCategoryCreated(Object sender, RepeaterItemEventArgs e)
        {
            var productCategory = e.Item.DataItem as KeyValuePair<string, Dictionary<string, List<PriceListItemView>>>?;
            var reProducts = e.Item.FindControl("reProducts") as Repeater;

            e.Item.FindControl("RetailPriceColumn").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("CustomerRetailPriceColumn").Visible = this.DocumentViewMode == ViewMode.CustomerWithDiscount;
            e.Item.FindControl("VolumePointsColumn").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("EarnBaseColumn").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("DiscountedRetailColumn").Visible = this.DocumentViewMode == ViewMode.Distributor;


            if (reProducts != null && productCategory != null)
            {
                reProducts.DataSource = productCategory.Value.Value;
                reProducts.DataBind();
            }
        }

        /// <summary>
        /// Productses the created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void ProductsCreated(Object sender, RepeaterItemEventArgs e)
        {
            var productCategory = e.Item.DataItem as KeyValuePair<string, List<PriceListItemView>>?;
            var reProducts = e.Item.FindControl("reProducts") as Repeater;

            if (reProducts != null && productCategory != null)
            {
                reProducts.DataSource = productCategory.Value.Value;
                reProducts.DataBind();
            }
        }

        /// <summary>
        /// Products the created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void ProductCreated(Object sender, RepeaterItemEventArgs e)
        {
            e.Item.FindControl("RetailPriceRow").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("CustomerRetailPriceRow").Visible = this.DocumentViewMode == ViewMode.CustomerWithDiscount;
            e.Item.FindControl("VolumePointsRow").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("EarnBaseRow").Visible = this.DocumentViewMode == ViewMode.Distributor;
            e.Item.FindControl("DiscountedRetailRow").Visible = this.DocumentViewMode == ViewMode.Distributor;
        }

        protected void btnSaveXLS_Click(object sender, EventArgs e)
        {
            imgLogo.Visible = false;
            this.EnableViewState = false;
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb, System.Globalization.CultureInfo.InvariantCulture);
            HtmlTextWriter w = new HtmlTextWriter(sw);
            divHTMLContent.RenderControl(w);

            Response.AppendHeader("content-disposition", "attachment;filename=" + this.GetLocalResourceObject("DefaultFileName") as string + ".xls");
            Response.Charset = "";
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());

            Response.Output.Write(@"<style> .textmode { mso-number-format:\@; } .headerXLS { color: windowtext !important; } </style>");
            Response.Output.Write(sb.ToString());
            Response.End();

            imgLogo.Visible = true;
        }
    }
}