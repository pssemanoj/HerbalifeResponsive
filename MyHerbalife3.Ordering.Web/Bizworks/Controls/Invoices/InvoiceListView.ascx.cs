using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.SharedProviders.Bizworks;
using MyHerbalife3.Ordering.SharedProviders.Invoices;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.ViewModel.Models;
using Telerik.Web.UI;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web.Script.Serialization;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class InvoiceListView : MyHerbalife3.Shared.UI.UserControlBase
    {

        protected enum SearchTypes
        {
            ByDateOrAmount,
            ByOthers
        }

        protected enum SearchFilterTypes
        {
            FirstName,
            LastName,
            StreetAddress,
            City,
            State,
            ZipCode,
            SKU,
            Description,
            TotalVolumePoints,
            InvoiceTotal,
            None
        }

        public string DistributorID
        {
            get { return ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.Id; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.goSearchByDate.Click += goSearchByDate_Click;
            this.goSearchBy.Click += goSearchBy_Click;
            this.createInvoice.Click += createInvoice_Click;
            rgdInvoices.NeedDataSource += rgdInvoices_NeedDataSource;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           
            rgdInvoices.PagerStyle.PagerTextFormat = (string)GetLocalResourceObject("PageTextFormat");
            rgdInvoices.PagerStyle.PageSizeLabelText = (string)GetLocalResourceObject("PageSizeText");

            // Control the create order visibility.
            rgdInvoices.MasterTableView.Columns.FindByUniqueName("CreateOrderColumn").Visible = HLConfigManager.Configurations.DOConfiguration.AllowCreateOrderFromInvoice;
        }

        protected void createInvoice_Click(object sender, EventArgs e)
        {
            Session["InvoiceSKUs"] = null;
            Response.Redirect("~/Bizworks/CreateInvoice.aspx");
        }

        protected void rgdInvoices_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            if (Session["IsExporting"] == null)
            {
                var searchType = SearchTypes.ByDateOrAmount;
                try
                {
                    if (Session["InvoiceContactID"] != null)
                    {
                        var invoiceContactID = (int)Session["InvoiceContactID"];
                        var assignedIds = new List<int>();
                        var contact = ContactsDataProvider.GetContactDetail(invoiceContactID, out assignedIds);

                        if (null != contact)
                        {
                            var contactSearchFilter = CreateContactSearchFilter(contact);
                            rgdInvoices.DataSource = searchInvoices(contactSearchFilter) ?? new List<Invoice>();
                            rgdInvoices.MasterTableView.NoMasterRecordsText = (string)GetLocalResourceObject("NoSavedInvoices.Text");
                        }

                        Session["InvoiceContactID"] = null;
                    }
                    else if (Session["InvoiceSearchType"] != null)
                    {
                        searchType = (SearchTypes)Session["InvoiceSearchType"];
                        var searchFilterType = convertFilterType(this.ddlSearchBy.SelectedValue);
                        rgdInvoices.DataSource = searchInvoices(searchType, searchFilterType);
                    }
                    else
                    {
                        searchType = SearchTypes.ByOthers;
                        var searchFilterType = convertFilterType(this.ddlSearchBy.SelectedValue);
                        rgdInvoices.DataSource = searchInvoices(searchType, searchFilterType) ?? new List<Invoice>();
                        rgdInvoices.MasterTableView.NoMasterRecordsText = (string)GetLocalResourceObject("NoSavedInvoices.Text");
                    }
                }
                catch (Exception ex)
                {
                    MyHLWebUtil.LogExceptionWithContext(ex);
                }
            }
            else
            {
                Session["IsExporting"] = null;
            }
        }

        /// <summary>
        /// On invoices grid command.
        /// </summary>
        /// <param name="sender">Sender parameter.</param>
        /// <param name="e">Arguments parameter.</param>
        protected void InvoicesItemCommand(object sender, GridCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "CreateOrder":
                    if (e.Item as GridDataItem != null)
                    {
                        var lblID = e.Item.FindControl("lblID") as Label;
                        if (lblID != null)
                        {
                            long key = 0;
                            if (long.TryParse(lblID.Text, out key))
                            {
                                Session["IsCopingFromInvoice"] = "Y";
                                Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?invoiceId={0}", key));
                            }
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        protected void rgdInvoices_ItemDataBound(object sender, GridItemEventArgs e)
        {
            try
            {
                if (e.Item is GridPagerItem)
                {
                    var pageSizeComboBox = e.Item.FindControl("PageSizeComboBox") as RadComboBox;

                    if (pageSizeComboBox != null)
                    {
                        var item = pageSizeComboBox.Items.FindItemByText("25");

                        if (item != null)
                        {
                            pageSizeComboBox.Items.Remove(item);
                        }

                        pageSizeComboBox.Items[1].Text = "25";
                        pageSizeComboBox.Items[1].Value = "25";

                        if (rgdInvoices.PageSize == 25)
                        {
                            pageSizeComboBox.SelectedIndex = 1;
                        }
                    }
                }

                if (e.Item is GridDataItem)
                {
                    var invoice = (Invoice)e.Item.DataItem;
                    var values = string.Empty;

                    if (!string.IsNullOrWhiteSpace(invoice.SkuJson))
                    {
                        JavaScriptSerializer serial = new JavaScriptSerializer();
                        List<InvoiceSKU> skuList = serial.Deserialize<List<InvoiceSKU>>(invoice.SkuJson);

                        foreach (var sku in skuList)
                        {
                            values += "SKU# " + sku.SKU + " Qty= " + sku.Quantity + " ";
                        }
                    }
                    e.Item.Cells[10].Text = values;
                }
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
        }

        protected void rgdInvoices_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            //Apply custom sorting
            var sortExpr = new GridSortExpression();
            switch (e.OldSortOrder)
            {
                case GridSortOrder.None:
                    sortExpr.FieldName = e.SortExpression;
                    sortExpr.SortOrder = GridSortOrder.Descending;

                    e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                    break;
                case GridSortOrder.Ascending:
                    sortExpr.FieldName = e.SortExpression;
                    sortExpr.SortOrder = rgdInvoices.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                    e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                    break;
                case GridSortOrder.Descending:
                    sortExpr.FieldName = e.SortExpression;
                    sortExpr.SortOrder = GridSortOrder.Ascending;

                    e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                    break;
            }

            e.Canceled = true;
            rgdInvoices.Rebind();
        }

        protected void goSearchBy_Click(object sender, EventArgs e)
        {
            SearchFilterTypes searchFilterType = convertFilterType(this.ddlSearchBy.SelectedValue);
            rgdInvoices.DataSource = searchInvoices(SearchTypes.ByOthers, searchFilterType) ?? new List<Invoice>();
            rgdInvoices.MasterTableView.NoMasterRecordsText = (string)GetLocalResourceObject("ltNoRecords.Text");
            rgdInvoices.DataBind();
            Session["InvoiceSearchType"] = searchFilterType;
        }

        protected void goSearchByDate_Click(object sender, EventArgs e)
        {
            var searchFilterType = convertFilterType(this.ddlSearchBy.SelectedValue);
            rgdInvoices.DataSource = searchInvoices(SearchTypes.ByDateOrAmount, searchFilterType) ?? new List<Invoice>(); ;
            rgdInvoices.MasterTableView.NoMasterRecordsText = (string)GetLocalResourceObject("ltNoRecords.Text");
            rgdInvoices.DataBind();
            Session["InvoiceSearchType"] = searchFilterType;
        }

        protected void OnExportToExcel(object sender, EventArgs e)
        {
            if (rgdInvoices.MasterTableView.GetItems(GridItemType.Item).Count() > 0)
            {
                ApplyStyleSheetsForExport(this.rgdInvoices.MasterTableView);

                rgdInvoices.MasterTableView.AllowPaging = false;
                rgdInvoices.ExportSettings.ExportOnlyData = false;
                rgdInvoices.ExportSettings.OpenInNewWindow = true;
                rgdInvoices.ExportSettings.IgnorePaging = true;
                rgdInvoices.Rebind(); 
                rgdInvoices.ExportSettings.FileName = "MyInvoices";
                rgdInvoices.Page.Response.ClearHeaders();
                rgdInvoices.MasterTableView.Columns[2].Visible = false;
                rgdInvoices.MasterTableView.Columns[3].Visible = true;
                rgdInvoices.MasterTableView.Columns[8].Visible = true;
                rgdInvoices.MasterTableView.ExportToCSV();
            }
        }

        private void ApplyStyleSheetsForExport(GridTableView gridTableView)
        {
            if (null != gridTableView.GetItems(GridItemType.Header) && gridTableView.GetItems(GridItemType.Header).Count() > 0)
            {
                GridItem headerItem = gridTableView.GetItems(GridItemType.Header)[0];

                headerItem.Style["font-size"] = "8pt";
                headerItem.Style["color"] = "black";
                headerItem.Style["vertical-align"] = "middle";
                headerItem.Style["width"] = "80px";

                foreach (TableCell tableCell in headerItem.Cells)
                {
                    tableCell.Wrap = true;
                    tableCell.Style["text-align"] = "center";
                    tableCell.Style["font-weight"] = "bold";
                    tableCell.Style["font-size"] = "12pt";
                    tableCell.Style["width"] = "75px";
                }

                GridItem[] items = gridTableView.GetItems(GridItemType.Item);
                GridItem[] alternatingItems = gridTableView.GetItems(GridItemType.AlternatingItem);

                ApplyGridItemStyle(items);
                ApplyGridItemStyle(alternatingItems);
            }
        }

        private void ApplyGridItemStyle(GridItem[] items)
        {
            foreach (var item in items)
            {
                //Default color for headeritem/dataitem
                item.Style["color"] = "black";
                item.Style["font-family"] = "Verdana";
                item.Style["text-align"] = "left";
                item.Style["vertical-align"] = "top";
                item.Style["font-size"] = "11px";
                item.Style["width"] = "75px";

                foreach (TableCell cell in item.Cells)
                {
                    if (cell.Controls != null && cell.Controls.Count > 1 && cell.Controls[1] is LinkButton)
                    {
                        cell.Text = (cell.Controls[1] as LinkButton).Text;
                    }

                    if (cell.Controls != null && cell.Controls.Count > 1 && cell.Controls[1] is HyperLink)
                    {
                        cell.Text = (cell.Controls[1] as HyperLink).Text;
                    }

                    cell.Width = 40;
                    cell.Wrap = true;
                    cell.BorderWidth = Unit.Pixel(1);
                    cell.BorderColor = Color.Black;
                    cell.Style[HtmlTextWriterStyle.MarginLeft] = "2px";
                }
            }
        }

        private SearchFilterTypes convertFilterType(string value)
        {
            SearchFilterTypes searchFilterType;

            switch (value.Trim().ToLower())
            {
                case "firstname":
                    searchFilterType = SearchFilterTypes.FirstName;
                    break;

                case "lastname":
                    searchFilterType = SearchFilterTypes.LastName;
                    break;

                case "streetaddress":
                    searchFilterType = SearchFilterTypes.StreetAddress;
                    break;

                case "city":
                    searchFilterType = SearchFilterTypes.City;
                    break;

                case "state":
                    searchFilterType = SearchFilterTypes.State;
                    break;

                case "zipcode":
                    searchFilterType = SearchFilterTypes.ZipCode;
                    break;

                case "sku":
                    searchFilterType = SearchFilterTypes.SKU;
                    break;

                case "description":
                    searchFilterType = SearchFilterTypes.Description;
                    break;

                case "totalvolumepoints":
                    searchFilterType = SearchFilterTypes.TotalVolumePoints;
                    break;

                case "invoicetotal":
                    searchFilterType = SearchFilterTypes.InvoiceTotal;
                    break;

                default:
                    searchFilterType = SearchFilterTypes.FirstName;
                    break;
            }

            return searchFilterType;
        }

        private InvoiceSearchFilter CreateContactSearchFilter(Contact_V01 invoiceContact)
        {
            var searchFilter = new InvoiceSearchFilter();
            try
            {
                if (null != invoiceContact)
                {
                    searchFilter.SearchByDateOrAmount = false;
                    if (null != invoiceContact.EnglishName)
                    {
                        if (!string.IsNullOrEmpty(invoiceContact.EnglishName.First))
                        {
                            searchFilter.FirstName = invoiceContact.EnglishName.First;
                        }

                        if (!string.IsNullOrEmpty(invoiceContact.EnglishName.Last))
                        {
                            searchFilter.LastName = invoiceContact.EnglishName.Last;
                        }
                    }

                    if (null != invoiceContact.PrimaryAddress)
                    {
                        if (!string.IsNullOrEmpty(invoiceContact.PrimaryAddress.Line1))
                        {
                            searchFilter.StreetAddress = invoiceContact.PrimaryAddress.Line1;
                        }
                        if (!string.IsNullOrEmpty(invoiceContact.PrimaryAddress.City))
                        {
                            searchFilter.City = invoiceContact.PrimaryAddress.City;
                        }
                        if (!string.IsNullOrEmpty(invoiceContact.PrimaryAddress.StateProvinceTerritory))
                        {
                            searchFilter.State = invoiceContact.PrimaryAddress.StateProvinceTerritory;
                        }
                        if (!string.IsNullOrEmpty(invoiceContact.PrimaryAddress.PostalCode))
                        {
                            searchFilter.ZipCode = invoiceContact.PrimaryAddress.PostalCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
            return searchFilter;
        }

        private List<Invoice> searchInvoices(InvoiceSearchFilter searchFilter)
        {
            var invoices = InvoiceProvider.SearchInvoice(searchFilter, DistributorID);
            return invoices;
        }

        private List<Invoice> searchInvoices(SearchTypes searchType, SearchFilterTypes filterType)
        {
            //begin - shan - mar 14, 2012 - set default value to be error value (-999)
            //based on that differentiate whether zero has been entered or fields has been left empty
            //while searching based on the amount
            //decimal startAmount = 0;
            //decimal.TryParse(txtStartAmount.Text.Trim(), out startAmount);
            //decimal endAmount = 0;
            //decimal.TryParse(txtEndAmount.Text.Trim(), out endAmount);
            var invoices = new List<Invoice>();
            decimal startAmount = -999;
            decimal endAmount = -999;

            startAmount = decimal.TryParse(txtStartAmount.Text.Trim(), out startAmount) ? startAmount : -999;
            endAmount = decimal.TryParse(txtEndAmount.Text.Trim(), out endAmount) ? endAmount : -999;

            var searchValue = txtSearchBy.Text.Trim();
            if (searchType == SearchTypes.ByDateOrAmount)
            {
                invoices = InvoiceProvider.SearchInvoice(new InvoiceSearchFilter()
                {
                    SearchByDateOrAmount = true,
                    FromDate = FromDate.SelectedDate,
                    ToDate = ToDate.SelectedDate,
                    StartAmount = (0 > startAmount) ? (decimal?)null : startAmount,
                    EndAmount = (0 > endAmount) ? (decimal?)null : endAmount,
                }, DistributorID);
            }
            else
            {
                var searchFilter = new InvoiceSearchFilter();
                searchFilter.SearchByDateOrAmount = false;
                searchFilter.FirstName = (filterType == SearchFilterTypes.FirstName) ? searchValue : string.Empty;
                searchFilter.LastName = (filterType == SearchFilterTypes.LastName) ? searchValue : string.Empty;
                searchFilter.StreetAddress = (filterType == SearchFilterTypes.StreetAddress) ? searchValue : string.Empty;
                searchFilter.City = (filterType == SearchFilterTypes.City) ? searchValue : string.Empty;
                searchFilter.State = (filterType == SearchFilterTypes.State) ? searchValue : string.Empty;
                searchFilter.ZipCode = (filterType == SearchFilterTypes.ZipCode) ? searchValue : string.Empty;
                searchFilter.SKU = (filterType == SearchFilterTypes.SKU) ? searchValue : string.Empty;
                searchFilter.Description = (filterType == SearchFilterTypes.Description) ? searchValue : string.Empty;

                //begin - shan - mar 14, 2012 - assign null based on error value
                //to differentiate whether to search by 0 or empty field
                //based on this, while filtering check for null or 0				
                decimal parseValue = -999;

                parseValue = decimal.TryParse(searchValue, out parseValue) ? parseValue : -999;
                {
                    searchFilter.TotalVolumePoints = (filterType == SearchFilterTypes.TotalVolumePoints) ?
                        (0 < parseValue ? parseValue : (decimal?)null) : (decimal?)null;
                    searchFilter.InvoiceTotal = (filterType == SearchFilterTypes.InvoiceTotal) ?
                        (0 < parseValue ? parseValue : (decimal?)null) : (decimal?)null;
                }

                invoices = InvoiceProvider.SearchInvoice(searchFilter, DistributorID);

                //shan - mar 15, 2012 - to set secondary sorting to be of invoice #
                //sort the invoices to be ordered by invoice # descendingg
                //while applying sorting from grid that should get filter as the primary sorting
                if (null != invoices && invoices.Count > 0)
                {
                    invoices = invoices.OrderByDescending(inv => inv.DistributorInvoiceNumber).ToList();
                }
            }

            return invoices;
        }

        protected string LocalizeStatus(string status)
        {
            return Resources.InvoiceStatusTypes.ResourceManager.GetString(status) ??
                status;
        }

        protected string LocalizeType(string type)
        {
            return Resources.InvoiceTypes.ResourceManager.GetString(type) ??
                type;
        }
    }
}
