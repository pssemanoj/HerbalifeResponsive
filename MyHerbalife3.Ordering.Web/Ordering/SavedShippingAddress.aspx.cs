using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HL.Common.EventHandling;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class SavedShippingAddress : ProductsBase
    {
        //AjaxControlToolkit.ModalPopupExtender mpAddAddress;

        private int endRecordIndex;
        private AddressBase shippingAddressBase;
        private List<DeliveryOption> shippingAddresses;
        private ShippingInfoControlBase shippingInfoControlBase = null;
        //Used isMobile and isResposive to switch between GridViewControls
        private bool isMobile;
        private bool isResponsive;

        private String sortDirection;
        private String sortExpression;

        private int startRecordIndex;
        List<DeliveryOption> restrictedAddress = new List<DeliveryOption>();

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if ((HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) ||
                !(Master as OrderingMaster).IsMobile())
            {
                (Master as OrderingMaster).divLeftVisibility = true;
            }
            else
            {
                (Master as OrderingMaster).divLeftVisibility = false;
            }

            if (GlobalContext.CurrentExperience.ExperienceType == Shared.ViewModel.ValueObjects.ExperienceType.Green 
                && HLConfigManager.Configurations.DOConfiguration.ChangeOrderingLeftMenuMyHL3)
            {
                (Master as OrderingMaster).IsleftMenuVisible = true;
                (Master as OrderingMaster).IsleftOrderingMenuVisible = false;
            }
            shippingInfoControlBase = ucShippingInfoControl;
            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            }
            //(this.Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;

            loadShippingAddress();
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-no-right-nav");

            ValidateMobileResponsive();
        }

        protected AddressBase createAddress(string controlPath, bool isXML)
        {
            try
            {
                if (!isXML)
                {
                    shippingAddressBase = LoadControl(controlPath) as AddressBase;
                }
                else
                {
                    shippingAddressBase = new AddressControl();
                    shippingAddressBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return shippingAddressBase;
        }

        protected void gvSavedShippingAddress_DataBound(object sender, EventArgs e)
        {
            ValidateMobileResponsive();
            if (isMobile && isResponsive)
            {
                if (gvmobSavedShippingAddress == null) return;
            }
            else
            {
                if (gvSavedShippingAddress == null) return;
            }

            int pageIndex;
            int pageSize;

            if (isMobile && isResponsive)
            {
                pageIndex = gvmobSavedShippingAddress.PageIndex;
                pageSize = gvmobSavedShippingAddress.PageSize;
            }
            else
            {
                pageIndex = gvSavedShippingAddress.PageIndex;
                pageSize = gvSavedShippingAddress.PageSize;
            }

            endRecordIndex = (pageIndex + 1) * pageSize;

            int totalRecords = 0;

            if (shippingAddresses != null)
                totalRecords = shippingAddresses.Count();

            if (endRecordIndex > totalRecords)
                endRecordIndex = totalRecords;

            if (totalRecords == 0)
            {
                startRecordIndex = 0;
            }
            else
            {
                if (isMobile && isResponsive)
                {
                    startRecordIndex = gvmobSavedShippingAddress.PageIndex * gvmobSavedShippingAddress.PageSize + 1;
                }
                else
                {
                    startRecordIndex = gvSavedShippingAddress.PageIndex * gvSavedShippingAddress.PageSize + 1;
                }
            }

            string recordCountHeaderRow = "";

            try
            {
                if (startRecordIndex != endRecordIndex)
                {
                    recordCountHeaderRow = string.Format(base.GetLocalResourceString("ShowingResultsFromToFormat"),
                                                         startRecordIndex, endRecordIndex, totalRecords);
                    //Showing {0} - {1} of {2} Results
                }
                else
                {
                    recordCountHeaderRow = string.Format(base.GetLocalResourceString("ShowingPesultsFormat"),
                                                         startRecordIndex, totalRecords); //Showing {0} of {1} Results
                }
            }
            catch
            {
                //to aovid crashing the page because of invalid formatting provided in the resx file
            }

            // Get the header row.

            var lbl = new Label();

            lbl.Text = recordCountHeaderRow;
            GridViewRow headerRow;
            if (isMobile && isResponsive)
            {
                headerRow = gvmobSavedShippingAddress.HeaderRow;
                if (headerRow != null)
                {
                    headerRow.Cells[0].Controls.Add(lbl);
                }
            }
            else
            {
                headerRow = gvSavedShippingAddress.HeaderRow;
                if (headerRow != null)
                {
                    headerRow.Cells[5].Controls.Add(lbl);
                }
            }


        }

        protected void gvSavedShippingAddress_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            ValidateMobileResponsive();
            gvSavedShippingAddress_dataPagerGridView(sender, e);

            var gridView = (GridView)sender;

            if (e.Row.RowType == DataControlRowType.Header)
            {
                int cellIndex = -1;
                foreach (DataControlField field in gridView.Columns)
                {
                    e.Row.Cells[gridView.Columns.IndexOf(field)].CssClass = "headerstyle";

                    if (field.SortExpression == gridView.SortExpression)
                    {
                        cellIndex = gridView.Columns.IndexOf(field);
                    }

                    if (HLConfigManager.Configurations.DOConfiguration.DisableAddressSortingInOrderPreferences)
                    {
                        field.SortExpression = String.Empty;
                    }
                    if (!HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                    {
                       if( field.HeaderText =="Address Type")
                        {
                            field.HeaderText = string.Empty;
                        }
                    }
                }

                if (cellIndex > -1)
                {
                    //  this is a header row,
                    //  set the sort style
                    e.Row.Cells[cellIndex].CssClass =
                        gridView.SortDirection == SortDirection.Ascending
                            ? "sortascheaderstyle"
                            : "sortdescheaderstyle";
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int addressId;
                if (isMobile && isResponsive)
                {
                    addressId = Convert.ToInt32(gvmobSavedShippingAddress.DataKeys[e.Row.RowIndex]["Id"]);
                }
                else
                {
                    addressId = Convert.ToInt32(gvSavedShippingAddress.DataKeys[e.Row.RowIndex]["Id"]);
                }

                var q = (from m in shippingAddresses.Where(s => s.Id == addressId)
                         select m).First();

                var NickName = e.Row.FindControl("Nickname") as Label;
                if (NickName != null)
                {
                    NickName.Text = q.DisplayName.Replace("...", "");
                }
                if(q.HasAddressRestriction==true)
                {
                    var AddrssType = e.Row.FindControl("lblAddrssType") as Label;
                    if (AddrssType != null)
                    {
                        AddrssType.Text = GetLocalResourceObject("AddressTypeResource.Text") as string;
                    }
                }

                // Retrieve the LinkButton control from the last column.
                LinkButton editButton;
                LinkButton deleteButton;
                if (isMobile && isResponsive)
                {
                    editButton = (LinkButton)e.Row.Cells[0].Controls[11];
                    deleteButton = (LinkButton)e.Row.Cells[0].Controls[13];
                }
                else
                {
                    editButton = (LinkButton)e.Row.Cells[5].Controls[3];
                    deleteButton = (LinkButton)e.Row.Cells[5].Controls[5];
                }
                // defect 23993 disable delete for primary
                if (getSelectedAddress(addressId).IsPrimary)
                {
                    //e.Row.Style.Add("background-color", "#ffff66");
                    e.Row.CssClass = "gdo-row-selected gdo-body-text";
                    e.Row.FindControl("Primary").Visible = true;
                    //deleteButton.Enabled = false;
                    deleteButton.Enabled = true;
                }
                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                {
                    editButton.Visible = false;
                    deleteButton.Visible = false;
                }

                    var address = e.Row.FindControl("Address") as HtmlContainerControl;
                if (address != null)
                {
                    address.InnerHtml = GetShippingProvider()
                        .FormatShippingAddress(q, DeliveryOptionType.Shipping, string.Empty, false);
                }
                // Set the LinkButton's CommandArgument property with the
                // row's index.
                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                {
                    restrictedAddress = shippingAddresses.Where(x => x.HasAddressRestriction ?? false).ToList();

                }
                editButton.CommandArgument = e.Row.RowIndex.ToString();
                deleteButton.CommandArgument = e.Row.RowIndex.ToString();
                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction && (restrictedAddress.Count >= HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestrictionLimit))
                {
                    editButton.Visible = false;
                    btnAddShippingAddress.Visible = false;
                }
                else
                {
                    btnAddShippingAddress.Visible = true;
                }
            }
        }

        private static void gvSavedShippingAddress_dataPagerGridView(object sender, GridViewRowEventArgs e)
        {
            var dGgridView = (DataPagerGridView)sender;
            if (dGgridView.SortExpression.Length > 0)
            {
                int cellIndex = -1;
                foreach (DataControlField field in dGgridView.Columns)
                {
                    if (field.SortExpression == dGgridView.SortExpression)
                    {
                        cellIndex = dGgridView.Columns.IndexOf(field);
                        break;
                    }
                }
                if (cellIndex > -1)
                {
                    if (e.Row.RowType == DataControlRowType.Header)
                    {
                        //  this is a header row,set the sort style
                        e.Row.Cells[cellIndex].CssClass +=
                            (dGgridView.SortDirection == SortDirection.Ascending ? " sortasc" : " sortdesc");
                    }
                }
            }
        }

        protected void btnAddShippingAddress_Click(object sender, EventArgs e)
        {
            shippingInfoControlBase.ShowPopupForShipping(CommandType.Add,
                                                         new ShippingAddressEventArgs(DistributorID, null, false, true));
        }

        protected void gvSavedShippingAddress_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ValidateMobileResponsive();
            if (e.CommandName != "EditCommand" && e.CommandName != "DeleteCommand") return;

            int index = Convert.ToInt32(e.CommandArgument);
            int rowIndex;
            //if (gvSavedShippingAddress.PageIndex == 0)
            //{
            rowIndex = index;
            //}
            //else
            //{
            //    rowIndex = index - gvSavedShippingAddress.PageIndex * gvSavedShippingAddress.PageSize;
            //}

            // Retrieve the row that contains the button clicked 
            // by the user from the Rows collection.
            GridViewRow row;
            int addressId;

            if (isMobile && isResponsive)
            {
                row = gvmobSavedShippingAddress.Rows[rowIndex];
                addressId = Convert.ToInt32(gvmobSavedShippingAddress.DataKeys[row.RowIndex]["Id"]);
            }
            else
            {
                row = gvSavedShippingAddress.Rows[rowIndex];
                addressId = Convert.ToInt32(gvSavedShippingAddress.DataKeys[row.RowIndex]["Id"]);
            }

            DeliveryOption deliveryOption = getSelectedAddress(addressId);

            string distributorId = (Page as ProductsBase).DistributorID;
            if (deliveryOption != null)
            {
                switch (e.CommandName)
                {
                    case "EditCommand":
                        {
                            shippingInfoControlBase.ShowPopupForShipping(CommandType.Edit,
                                                                         new ShippingAddressEventArgs(distributorId,
                                                                                                      deliveryOption,
                                                                                                      false, true));
                            return;
                        }

                    case "DeleteCommand":
                        {
                            shippingInfoControlBase.ShowPopupForShipping(CommandType.Delete,
                                                                         new ShippingAddressEventArgs(distributorId,
                                                                                                      deliveryOption,
                                                                                                      false,
                                                                                                      DisableSaveAddressCheckbox));
                            return;
                        }

                    default:
                        return;
                }
            }
        }

        private DeliveryOption getSelectedAddress(int addressID)
        {
            try
            {
                var addresses = shippingAddresses.Where(s => s.Id == addressID);
                if (addresses.Count() == 0)
                {
                    addresses = shippingAddresses.Where(s => s.IsPrimary);
                }

                return addresses.Count() == 0 ? null : addresses.First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void gvSavedShippingAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void gvSavedShippingAddress_RowCreated(object sender, GridViewRowEventArgs e)
        {
            //cell.Controls.Add((Control)addressBase);
            //e.Row.Cells[3].Controls.Add((Control)addressBase);
        }

        protected void gvSavedShippingAddress_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ValidateMobileResponsive();
            if (isMobile && isResponsive)
            {
                gvmobSavedShippingAddress.PageIndex = e.NewPageIndex;
                gvmobSavedShippingAddress.DataBind();
            }
            else
            {
                gvSavedShippingAddress.PageIndex = e.NewPageIndex;
                gvSavedShippingAddress.DataBind();
            }
        }

        protected void gvSavedShippingAddress_Sorting(object sender, GridViewSortEventArgs e)
        {
            // get values from viewstate   
            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;

            // on first time header clicked ( including different header), sort ASCENDING    
            if (!HLConfigManager.Configurations.DOConfiguration.DisableAddressSortingInOrderPreferences)
            {
                if (e.SortExpression != sortExpression)
                {
                    sortExpression = e.SortExpression;
                    sortDirection = "Descending";
                }
                // on second time header clicked, toggle sort    
                else
                {
                    if (sortDirection == "Ascending")
                    {
                        sortExpression = e.SortExpression;
                        sortDirection = "Descending";
                    }
                    // Descending        
                    else
                    {
                        sortExpression = e.SortExpression;
                        sortDirection = "Ascending";
                    }
                }

                gvSavedShippingAddress.PageIndex = 0;
                gvSavedShippingAddress_DataBind(sortExpression, sortDirection);
            }
        }

        private void gvSavedShippingAddress_DataBind(String sortExpression, String sortDirection)
        {
            ValidateMobileResponsive();
            // save state for next time   
            ViewState["_GridView1LastSortDirection_"] = sortDirection;
            ViewState["_GridView1LastSortExpression_"] = sortExpression;

            IEnumerable<DeliveryOption> q;

            List<DeliveryOption> list = null;

            try
            {
                if (shippingAddresses != null)
                {
                    if (sortDirection == "Descending")
                    {
                        q = from m in shippingAddresses
                            orderby m.IsPrimary descending, OrderBy(sortExpression, m) descending
                            select m;
                    }
                    else
                    {
                        if (sortExpression == "Alias" || sortExpression == "DisplayName")
                        {
                            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                            {
                                q = from m in shippingAddresses
                                    orderby m.HasAddressRestriction descending, OrderBy(sortExpression, m) descending
                                    select m;

                            }
                            else
                            {
                                q = from m in shippingAddresses
                                    orderby m.IsPrimary descending,
                                        (string.IsNullOrEmpty(m.Alias) ? "zzzzzzzzzzzzzzzzzzzzzzzzz" : m.Alias) ascending,
                                        OrderBy(sortExpression, m)
                                    select m;
                            }
                        }
                        else
                        {
                            q = from m in shippingAddresses
                                orderby m.IsPrimary descending, OrderBy(sortExpression, m)
                                select m;
                        }
                    }
                    list = q.ToList();
                }
            }

            catch (Exception)
            {
                list = null;
            }
            if (isMobile && isResponsive)
            {
                gvmobSavedShippingAddress.DataSource = list;
                gvmobSavedShippingAddress.DataBind();
            }
            else
            {
                gvSavedShippingAddress.DataSource = list;
                gvSavedShippingAddress.DataBind();
            }
        }

        private object OrderBy(String sortKey, DeliveryOption shippingAddress)
        {
            switch (sortKey)
            {
                case "Alias":
                    return shippingAddress.DisplayName;
                case "DisplayName":
                    return shippingAddress.DisplayName;
                case "Recipient":
                    return shippingAddress.Recipient;
                default:
                    break;
            }

            return shippingAddress.Recipient;
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressCreated)]
        public void OnSShippingAddressCreated(object sender, EventArgs e)
        {
            //Response.Redirect(Request.Url.ToString());
            loadShippingAddress();
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction && (restrictedAddress.Count == HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestrictionLimit))
            {
                Response.Redirect(Request.RawUrl);
            }
            UpdatePanelAddress.Update();
            if (shippingAddresses.Count > 0)
            {
                if (null == ShoppingCart.DeliveryInfo)
                {
                    DeliveryOption primaryDeliveryOption = shippingAddresses.Find(s => s.IsPrimary);
                    if (null != primaryDeliveryOption)
                    {
                        ShoppingCart.UpdateShippingInfo(primaryDeliveryOption.ID, 0, DeliveryOptionType.Shipping);
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressDeleted)]
        public void OnShippingAddressDeleted(object sender, EventArgs e)
        {
            try
            {
                shippingAddresses =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetShippingAddresses((Page as ProductsBase).DistributorID,
                                                                (Page as ProductsBase).Locale)
                                          .Where(s => s.ID != -1)
                                          .ToList();
                var args = e as ShippingAddressEventArgs;
                if (args != null)
                {
                    if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                        ShoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping &&
                        args.ShippingAddress.ID == ShoppingCart.DeliveryInfo.Address.ID)
                    {
                        DeliveryOption primaryDeliveryOption = null;
                        //After delete pass the primary shipping ID to update shipping info..
                        if (null != shippingAddresses && shippingAddresses.Count > 0)
                        {
                            primaryDeliveryOption = shippingAddresses.Find(s => s.IsPrimary);
                            if (primaryDeliveryOption != null)
                            {
                                ShoppingCart.UpdateShippingInfo(primaryDeliveryOption.Id, 0, DeliveryOptionType.Shipping);
                            }
                        }
                        else // no addresses
                        {
                            ShoppingCart.DeliveryInfo = null;
                        }
                    }
                }
            }
            catch
            {
            }

            if (shippingAddresses == null || shippingAddresses.Count == 0)
            {
                ClearCart();
            }

            //Response.Redirect(Request.Url.ToString());
            loadShippingAddress();
            UpdatePanelAddress.Update();
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressChanged)]
        public void OnShippingAddressChanged(object sender, EventArgs e)
        {
            //Response.Redirect(Request.Url.ToString());
            loadShippingAddress();
            UpdatePanelAddress.Update();

            if (IsChina)
            {
                var arg = e as ShippingAddressEventArgs;

                if (arg != null && arg.ShippingAddress != null)
                {
                    if (ShoppingCart != null && ShoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping && ShoppingCart.ShippingAddressID == arg.ShippingAddress.ID)
                    {
                        //The main purpose is to reload the Warehouse. Incase user had change the state/province of the existing shipping address.
                        ShoppingCart.LoadShippingInfo(ShoppingCart.DeliveryOptionID, ShoppingCart.ShippingAddressID,
                                             ShoppingCart.DeliveryOption, ShoppingCart.OrderCategory,
                                             ShoppingCart.FreightCode, false);
                    }
                }
            }
        }

        private void loadShippingAddress()
        {
            ValidateMobileResponsive();
            try
            {
                shippingAddresses =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetShippingAddresses((Page as ProductsBase).DistributorID,
                                                                (Page as ProductsBase).Locale)
                                          .Where(s => s.ID > 0).OrderByDescending(x => x.HasAddressRestriction).ToList();
                                          
              
            }
            catch (Exception)
            {
                return;
            }

            if (sortExpression == null)
            {
                sortExpression = "Alias";
            }

            if (sortDirection == null)
            {
                sortDirection = "Ascending";
            }

            if (isMobile && isResponsive)
            {
                gvmobSavedShippingAddress.DataSource = shippingAddresses;
                gvmobSavedShippingAddress.DataBind();
                gvSavedShippingAddress_DataBind(sortExpression, sortDirection);
                if (shippingAddresses != null && shippingAddresses.Count > 0)
                {
                    divAddress.Visible = true;
                }
                else
                {
                    divAddress.Visible = false;
                }
            }
            else
            {
                //gvSavedShippingAddress.DataSource = shippingAddresses.Where(s => s.ID != -1);
                gvSavedShippingAddress.DataSource = shippingAddresses;

                //gvSavedShippingAddress.DataSource = shippingAddresses.Where(s => s.Id != -1);
                gvSavedShippingAddress.DataBind();

                gvSavedShippingAddress_DataBind(sortExpression, sortDirection);

                if (shippingAddresses != null && shippingAddresses.Count > 0)
                {
                    tblAddress.Visible = true;
                }
                else
                {
                    tblAddress.Visible = false;
                }
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
            {
                 restrictedAddress = shippingAddresses.Where(x => x.HasAddressRestriction ?? false).ToList();
               
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction && (restrictedAddress.Count >= HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestrictionLimit))
            {
                btnAddShippingAddress.Visible = false;
            }
            }
        private void ValidateMobileResponsive()
        {
            isMobile = (Master as OrderingMaster).IsMobile();
            isResponsive = HLConfigManager.Configurations.DOConfiguration.IsResponsive;
        }
    }
}