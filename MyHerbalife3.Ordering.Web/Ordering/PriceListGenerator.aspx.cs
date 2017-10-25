// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceListGenerator.aspx.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Used to provide the code behind functionality for the price list generator feature.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Security;
using HL.Common.Logging;
using MyHerbalife3.Shared.Infrastructure;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Web.Script.Services;
    using System.Web.Services;
    using System.Web.UI.WebControls;
    using Providers;
    using Providers.ResultViews;
    using Providers.Shipping;
    using Controls;
    using Telerik.Web.UI;
    using MyHerbalife3.Ordering.Web.MasterPages;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

    /// <summary>
    /// Used to provide the code behind functionality for the price list generator feature.
    /// </summary>
    [ScriptService]
    public partial class PriceListGenerator : ProductsBase
    {
        /// <summary>
        /// The _is to export
        /// </summary>
        private bool _isToExport;

        /// <summary>
        /// Gets or sets the price list view.
        /// </summary>
        /// <value>
        /// The price list view.
        /// </value>
        public static List<PriceListItemView> PriceListView { get; set; }

        /// <summary>
        /// Gets or sets the name of the current category.
        /// </summary>
        /// <value>
        /// The name of the current category.
        /// </value>
        public static string CurrentCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the name of the current sub category.
        /// </summary>
        /// <value>
        /// The name of the current sub category.
        /// </value>
        public static string CurrentSubCategoryName { get; set; }

        public static Dictionary<string, decimal> ProductTax { get; set; }

        public static bool IsMultipleRate { get; set; }

        /// <summary>
        /// Getting the price list items.
        /// </summary>
        /// <param name="salesTax">The sales tax.</param>
        /// <param name="shippingAndHandling">The shipping and handling.</param>
        /// <param name="distributorDiscount">The distributor discount.</param>
        /// <param name="customerDiscount">The customer discount.</param>
        /// <returns>
        /// Price list items
        /// </returns>
        [WebMethod]
        [ScriptMethod]
        public static IEnumerable<PriceListItemView> GetData(
            decimal salesTax, 
            decimal shippingAndHandling,
            decimal distributorDiscount,
            decimal customerDiscount)
        {
            try
            {
                // Calculations.
                CalculatePrices(distributorDiscount, customerDiscount, salesTax, shippingAndHandling);

                return PriceListView;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception(ProviderPolicies.SYSTEM_EXCEPTION, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the sales tax.
        /// </summary>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="streetAddress">The street address.</param>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod]
        public static string GetSalesTax(string city, string state, string streetAddress, string zipCode)
        {
            try
            {
                ProductTax = new Dictionary<string, decimal>();
                var locale = CultureInfo.CurrentCulture.Name;

                string errorMessage;
                var address = new ServiceProvider.ShippingSvc.Address_V01()
                {
                    City = city,
                    Line1 = streetAddress,
                    StateProvinceTerritory = locale.Substring(3).Equals("CA") && state.IndexOf(" -", System.StringComparison.Ordinal) > 0 ?
                        state.Substring(0, state.IndexOf(" -", System.StringComparison.Ordinal)) : state,
                    PostalCode = zipCode,
                    Country = locale.Substring(3)
                };

                string validateCity;
                string validatePostalCode;

                var user = Membership.GetUser();

                // Getting the sales tax from provider.
                ProductTax = OrderProvider.GetAllTaxRateFromVertex(
                    user.UserName,
                    System.Threading.Thread.CurrentThread.CurrentCulture.ToString(),
                    address,
                    out errorMessage, out validateCity, out validatePostalCode);

                CheckIsMultiple();
                return string.IsNullOrEmpty(errorMessage)
                           ? string.Concat((IsMultipleRate ? "Multiple" : ProductTax.First().Value.ToString("00.00")), "@",
                                           validateCity, "@", validatePostalCode)
                           : errorMessage;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception(ProviderPolicies.SYSTEM_EXCEPTION, ex);
                throw;
            }
        }

        private static void CheckIsMultiple()
        {
            var taxRate = (from i in PriceListView
                           from t in ProductTax
                           where i.Sku == t.Key
                           select t.Value).ToList();
            var multiple = taxRate.Distinct().ToList();
            IsMultipleRate = (multiple.Count > 1);
        }

        /// <summary>
        /// Calculate customer price and distributor cost.
        /// </summary>
        /// <param name="distributorDiscount">The distributor discount.</param>
        /// <param name="customerDiscount">The customer discount.</param>
        /// <param name="salesTax">The sales tax.</param>
        /// <param name="shippingAndHandling">The shipping and handling.</param>
        private static void CalculatePrices(decimal distributorDiscount, decimal customerDiscount, decimal salesTax, decimal shippingAndHandling)
        {            
            PriceListView.ForEach(
                item =>
                    {
                        var tax = item.GetTaxRate(salesTax, ProductTax);
                        item.CustomerPrice =
                            getAmountString(
                                item.GetCustomerPrice(customerDiscount, tax, shippingAndHandling));
                        item.CustomerRetailPrice =
                            getAmountString(
                                item.GetCustomerRetailPrice(tax, shippingAndHandling));
                        item.DistributorLoadedCost =
                            getAmountString(
                                item.GetDistributorLoadedCost(distributorDiscount, tax, shippingAndHandling));
                        item.DistributorDiscountCost = getAmountString(item.GetDistributorDiscountCost(distributorDiscount));
                        item.TaxRate = string.Format("{0} %", string.Format(GetPriceFormat(tax*100), tax*100));
                    });

            var multiple = PriceListView != null ? PriceListView.Select(i => i.TaxRate).Distinct().ToList() : new List<string>();
            IsMultipleRate = (multiple.Count > 1);
        }

        /// <summary>
        /// Page load method.
        /// </summary>
        /// <param name="sender">Parameter sender.</param>
        /// <param name="e">Event arguments.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initializing distributor cost list items.
                ddlDistributorCost.Items.AddRange(new BindingList<RadComboBoxItem>()
                    {
                        new RadComboBoxItem("50"),
                        new RadComboBoxItem("42"),
                        new RadComboBoxItem("35"),  
                        new RadComboBoxItem("25") 
                    });

                // Default ds discount selection.
                foreach (RadComboBoxItem item in ddlDistributorCost.Items)
                {
                    item.Selected = item.Text ==
                                    this.DistributorDiscount.ToString(CultureInfo.InvariantCulture);
                }

                // Set defaults for tax and s&h
                if (HLConfigManager.Configurations.DOConfiguration.PLGDefaultTax != 0)
                {
                    double defTax = 0;
                    if (double.TryParse(HLConfigManager.Configurations.DOConfiguration.PLGDefaultTax.ToString(), out defTax))
                    {
                        txtSalesTax.Value = defTax;
                    }
                }
                if (HLConfigManager.Configurations.DOConfiguration.PLGDefaultSH != 0)
                {
                    double defSH = 0;
                    if (double.TryParse(HLConfigManager.Configurations.DOConfiguration.PLGDefaultSH.ToString(), out defSH))
                    {
                        txtShippingAndHandling.Value = defSH;
                    }
                }

                // Set defaults for tax and s&h
                if (HLConfigManager.Configurations.DOConfiguration.PLGDefaultTax != 0)
                {
                    double defTax = 0;
                    if (double.TryParse(HLConfigManager.Configurations.DOConfiguration.PLGDefaultTax.ToString(), out defTax))
                    {
                        txtSalesTax.Value = defTax;
                    }
                }
                if (HLConfigManager.Configurations.DOConfiguration.PLGDefaultSH != 0)
                {
                    double defSH = 0;
                    if (double.TryParse(HLConfigManager.Configurations.DOConfiguration.PLGDefaultSH.ToString(), out defSH))
                    {
                        txtShippingAndHandling.Value = defSH;
                    }
                }

                // if Lookup should be displayed
                if (HLConfigManager.Configurations.DOConfiguration.DisplayLookUpPriceListGenerator)
                {
                var provider = ShippingProvider.GetShippingProvider(this.CountryCode);
                if (provider != null)
                {
                    var lookupResults = provider.GetStatesForCountry(this.CountryCode);
                    if (lookupResults.Any())
                    {
                        ddlTaxState.Items.AddRange(lookupResults.Select(i => new ListItem(i, i)).ToArray());
                    }
                }

                // Default option for tax inputs.
                ddlTaxState.Items.Insert(0,
                                         new ListItem(
                                             this.GetLocalResourceString("DefaultStateOption") as string,
                                             string.Empty));
                txtTaxCity.Text = this.GetLocalResourceString("DefaultCityText") as string;
                txtTaxZipCode.Text = this.GetLocalResourceString("DefaultZipCodeText") as string;
                txtStreetAddress.Text = this.GetLocalResourceString("DefaultStreetAddressText") as string;
                    txtTaxZipCode.MaxLength = CountryCode.Equals("CA") ? 7 : 5;
                }
                else
                {
                    btnShowFindTax.Visible = false;
                    SHLookup.Visible = false;
                }
                // Initializing price list view.
                Session["PriceListViewToPrint"] = PriceListView = GetPriceListItems();          
           
                // Default file name.
                this.ProductListGrid.ExportSettings.FileName = this.GetLocalResourceString("DefaultFileName") as string;

            }

            if (Request.UrlReferrer == null || !Request.UrlReferrer.ToString().ToLower().Contains("pricelistgenerator"))
            {
                ProductTax = new Dictionary<string, decimal>();
            }

            // Is not exporting by default.
            this.ProductListGrid.ClientSettings.Scrolling.AllowScroll =
            this.ProductListGrid.ClientSettings.Scrolling.UseStaticHeaders = true;

            // By default, is not exporting.
            this._isToExport = false;

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-plg");
        }

        /// <summary>
        /// Gets the price list items.
        /// </summary>
        /// <returns></returns>
        private List<PriceListItemView> GetPriceListItems()
        {
            try
            {
                // Getting products from all categories.
                var priceListItems = this.ProductInfoCatalog.AllCategories.Select(i => i.Value)
                                         .Aggregate(new List<PriceListItemView>(),
                                                    (current, category) =>
                                                    InitializeProductListItems(category, category, current));

                // Disitnct function.
                priceListItems = priceListItems.GroupBy(item => item.Sku).Select(grp => grp.First()).ToList();

                // Order function.
                priceListItems = priceListItems.OrderBy(item => item.Category).ThenBy(item => item.SubCategory).ThenBy(item => item.Description).ToList();

                return priceListItems;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception(ProviderPolicies.SYSTEM_EXCEPTION, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <param name="rootCategory">The root category.</param>
        /// <param name="category">The category.</param>
        /// <param name="allProducts">All products.</param>
        /// <returns>List of price list items.</returns>
        private List<PriceListItemView> InitializeProductListItems(Category_V02 rootCategory, Category_V02 category,
                                                                   List<PriceListItemView> allProducts)
        {
            if (category.SubCategories != null)
            {
                allProducts = category.SubCategories.Aggregate(allProducts,
                                                               (current, sub) =>
                                                               InitializeProductListItems(rootCategory, sub, current));
            }

            if (category.Products != null)
            {
                allProducts.AddRange(category.Products
                                             .Where(
                                                 product =>
                                                 product.SKUs != null && product.SKUs.Count > 0 &&
                                                 product.SKUs.Any(sku => sku.IsDisplayable))
                                             .SelectMany(
                                                 product => product.SKUs
                                                     .SelectMany(productSku => this.AllSKUS.
                                                         Where(sku => productSku.IsDisplayable
                                                             && productSku.SKU == sku.Key
                                                             && sku.Value.CatalogItem.ProductType == ProductType.Product)
                                                         .Select(sku => new PriceListItemView(
                                                                sku.Value.CatalogItem.EarnBase, 
                                                                sku.Value.CatalogItem.ListPrice)
                                                             {
                                                                 Sku = sku.Key,
                                                                 ProductName = FormatDescription(sku.Value.Product.DisplayName),
                                                                 ShortDescription = FormatDescription(sku.Value.Description),
                                                                 Description = FormatDescription(string.Concat(sku.Value.Product.DisplayName, " ", sku.Value.Description)),
                                                                 Category = category.ParentCategory != null ? 
                                                                    category.ParentCategory.DisplayName :
                                                                    category.DisplayName,
                                                                 SubCategory = category.DisplayName,
                                                                 VolumePoints = sku.Value.CatalogItem.VolumePoints,
                                                                 EarnBase = getAmountString(sku.Value.CatalogItem.EarnBase),
                                                                 RetailPrice = getAmountString(sku.Value.CatalogItem.ListPrice),
                                                                 IsTaxExempt = sku.Value.CatalogItem.IsTaxExempt
                                                             }))));
            }

            return allProducts;
        }

        /// <summary>
        /// Formats the description.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string FormatDescription(string name)
        {
            name = name.Replace("<p>", string.Empty);
            name = name.Replace("</p>", string.Empty);
            return name;
        }

        /// <summary>
        /// Exports to PDF.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ExportToPdf(object sender, EventArgs e)
        {
            CurrentCategoryName = CurrentSubCategoryName = string.Empty;

            this._isToExport = true;
            
            // When exporting, avoiding a blank document, because the scrolling settings.
            this.ProductListGrid.ClientSettings.Scrolling.AllowScroll =
            this.ProductListGrid.ClientSettings.Scrolling.UseStaticHeaders = false;
        
            // Binding from server.
            this.ProductListGrid.DataSource = PriceListView;
            this.ProductListGrid.Rebind();

            this.ProductListGrid.MasterTableView.GetColumn("ProductName").
                 HeaderStyle.Width = cbCustomerPrice.Checked ? Unit.Pixel(496) : Unit.Pixel(236);

            // Exporting to pdf.
            this.ProductListGrid.MasterTableView.ExportToPdf();
        }

        /// <summary>
        /// Formats the grid item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void ItemCreated(object sender, GridItemEventArgs e)
        {
            if (!this._isToExport)
            {
                return;
            }            

            var item = e.Item;
            var priceListItemView = e.Item.DataItem as PriceListItemView;

            /* The stlyes are being applied per row. */

            // Fornt style.
            item.Style["color"] = "#555";
            item.Style["font-size"] = "11pt";
            item.Style["font-family"] = "Helvetica";
            item.Style["font-size-adjust"] = "none";
            item.Style["font-stretch"] = "normal";

            // Alignment.
            item.Style["vertical-align"] = "middle";
            item.Style["text-align"] = "left";

            switch (item.ItemType) 
            {
                case GridItemType.Header:
                    item.Style["color"] = "black";
                    item.Style["background-color"] = "#ffffff";
                    item.Style["font-family"] = "Helvetica";
                    item.Style["font-size"] = "9pt";
                    break;

                case GridItemType.AlternatingItem:
                    item.Style["background-color"] = "#f1f1f1";
                    break;
            }

            if (priceListItemView != null)
            {
                if (string.IsNullOrEmpty(CurrentCategoryName) ||
                    CurrentCategoryName != priceListItemView.Category)
                {
                    if (!priceListItemView.Sku.Contains("#CartNameStart#"))
                    {
                        priceListItemView.Sku =
                            string.Concat("#CartNameStart#",
                                          priceListItemView.Category, "#CatNameEnd#",
                                          priceListItemView.SubCategory, "#SubCatNameEnd#",
                                          priceListItemView.Sku);
                    }
                }
                else if (CurrentSubCategoryName != priceListItemView.SubCategory)
                {
                    if (!priceListItemView.Sku.Contains("#SubCatNameStart#"))
                    {
                        priceListItemView.Sku =
                            string.Concat("#SubCatNameStart#",
                                          priceListItemView.SubCategory, "#SubCatNameEnd#",
                                          priceListItemView.Sku);
                    }
                }

                CurrentCategoryName = priceListItemView.Category;
                CurrentSubCategoryName = priceListItemView.SubCategory;
            }
        }
        
        /// <summary>
        /// Exportings the PDF.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void ExportingPdf(object sender, GridPdfExportingArgs e)
        {
            if (!this._isToExport)
            {
                return;
            }

            var distributorName = string.Format(
                "<img style ='{0}' src='/Content/Global/css/img/logo_HID_300dpi.jpg' /><div style='{1}'>{2}</div>",
                PriceListGeneratorPdfStyle.LogoImageStyle,
                PriceListGeneratorPdfStyle.DistributorNameStyle,
                DistributorName);

            if (!this.cbCustomerPrice.Checked)
            {
                // Tax information.
                if (txtSalesTax.Text.Equals(txtSalesTaxCalculated.Value) &&
                    !string.IsNullOrEmpty(ddlTaxState.SelectedValue))
                {
                    distributorName += string.Format(
                        "<p style=\"{3}\">{0}, {1} {2}</p>",
                        txtTaxCity.Text,
                        ddlTaxState.Text,
                        txtTaxZipCode.Text,
                        PriceListGeneratorPdfStyle.TaxInformationStyle);
                }
            }

            // Inserting the header.
            e.RawHTML = e.RawHTML.Replace("<table", String.Format("{0}<table", distributorName));

            // Cleaning some styles;
            e.RawHTML = e.RawHTML.Replace("border=\"0\"", string.Empty);
            e.RawHTML = e.RawHTML.Replace("<th s", "<th style='border-bottom: 2px solid #000000' s");

            // Setting up categories and subcategories html mark up.
            e.RawHTML = e.RawHTML.Replace(
                "<td>#CartNameStart#",
                string.Format("</tr><tr><td colspan='9'><h2 style='{0}'>", 
                    PriceListGeneratorPdfStyle.CategoryNameStyle));

            e.RawHTML = e.RawHTML.Replace("#CatNameEnd#", string.Format("</h2><h3 style='{0}'>", PriceListGeneratorPdfStyle.SubCategoryNameStyle));
            e.RawHTML = e.RawHTML.Replace(
                "<td>#SubCatNameStart#",
                string.Format("</tr><tr><td colspan='9'><h3 style='{0}'>",
                    PriceListGeneratorPdfStyle.SubCategoryNameStyle));
            e.RawHTML = e.RawHTML.Replace("#SubCatNameEnd#", "</h3></td></tr><tr><td>");

            // Removing unused tags for Web UI.
            var toFix = PriceListView.Where(item => item.Sku.Contains("#SubCatNameEnd#")).ToList();
            if (toFix.Any())
            {
                toFix.ForEach(item => item.Sku =
                                      item.Sku.Substring(item.Sku.IndexOf("#SubCatNameEnd#") + "#SubCatNameEnd#".Length));
            }
        }
    }
}