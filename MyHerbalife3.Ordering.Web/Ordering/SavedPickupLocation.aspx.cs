using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
    public partial class SavedPickupLocation : ProductsBase
    {
        //TODO: revisit the use of this var
        //AjaxControlToolkit.ModalPopupExtender mpAddAddress = null;

        private int endRecordIndex;
        private AddressBase pickupLocationBase;
        private List<DeliveryOption> pickupLocations;

        private String sortDirection;
        private String sortExpression;

        private int startRecordIndex;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.HasPickupPreference == false)
            {
                return;
            }

            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            }

            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;

            loadShippingAddress();
        }

        private string getAddressControlPath(ref bool isXML)
        {
            isXML = true;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress;
        }

        protected AddressBase createAddress(string controlPath, bool isXML)
        {
            try
            {
                if (!isXML)
                {
                    pickupLocationBase = LoadControl(controlPath) as AddressBase;
                }
                else
                {
                    pickupLocationBase = new AddressControl();
                    pickupLocationBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Exception("createAddress error", ex);
            }
            return pickupLocationBase;
        }

        protected void gvSavedPickupLocation_DataBound(object sender, EventArgs e)
        {
            endRecordIndex = (gvSavedPickupLocation.PageIndex + 1)*gvSavedPickupLocation.PageSize;

            int totalRecords = 0;

            if (pickupLocations != null)
                totalRecords = pickupLocations.Count();

            if (endRecordIndex > totalRecords)
                endRecordIndex = totalRecords;

            if (totalRecords == 0)
                startRecordIndex = 0;
            else
                startRecordIndex = gvSavedPickupLocation.PageIndex*gvSavedPickupLocation.PageSize + 1;

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
            var headerRow = gvSavedPickupLocation.HeaderRow;
            if (headerRow != null)
            {
                headerRow.Cells[4].Controls.Add(lbl);
            }
        }

        protected void gvSavedPickupLocation_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var gridView = (GridView) sender;

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
                bool isXML = true;
                string controlPath = getAddressControlPath(ref isXML);
                pickupLocationBase = createAddress(controlPath, isXML);

                int addressId = Convert.ToInt32(gvSavedPickupLocation.DataKeys[e.Row.RowIndex]["Id"]);

                var q = (from m in pickupLocations.Where(s => s.Id == addressId)
                         select m).First();

                pickupLocationBase.DataContext = q;

                if (getSelectedAddress(addressId).IsPrimary)
                {
                    e.Row.CssClass = "gdo-row-selected gdo-body-text";
                    e.Row.FindControl("Primary").Visible = true;
                }

                e.Row.Cells[3].Controls.Add((Control) pickupLocationBase);

                // Retrieve the LinkButton control from the last column.
                var deleteButton = (LinkButton) e.Row.Cells[4].Controls[3];

                // Set the LinkButton's CommandArgument property with the
                // row's index.
                deleteButton.CommandArgument = e.Row.RowIndex.ToString();

                var trigger1 = new AsyncPostBackTrigger();
                trigger1.ControlID = deleteButton.UniqueID;
                UpdatePanel2.Triggers.Add(trigger1);
            }
        }

        protected void btnAddPickupLocation_Click(object sender, EventArgs e)
        {
            Session["AddClickedFromPickupPref"] = true;
            ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(true));
            //mpAddAddress.Show();
        }

        protected void gvSavedPickupLocation_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteCommand") return;

            int index = Convert.ToInt32(e.CommandArgument);
            int rowIndex;
            //if (gvSavedPickupLocation.PageIndex == 0)
            //{
            rowIndex = index;
            //}
            //else
            //{
            //    rowIndex = index - gvSavedPickupLocation.PageIndex * gvSavedPickupLocation.PageSize;
            //}

            // Retrieve the row that contains the button clicked 
            // by the user from the Rows collection.
            var row = gvSavedPickupLocation.Rows[rowIndex];
            int addressId = Convert.ToInt32(gvSavedPickupLocation.DataKeys[row.RowIndex]["Id"]);

            var deliveryOption = getSelectedAddress(addressId);

            string distributorId = (Page as ProductsBase).DistributorID;
            if (deliveryOption != null)
            {
                switch (e.CommandName)
                {
                    case "DeleteCommand":
                        ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete,
                                                                 new DeliveryOptionEventArgs(deliveryOption.Id,
                                                                                             string.Empty));
                        break;
                    default:
                        return;
                }
            }
        }

        private DeliveryOption getSelectedAddress(int addressID)
        {
            try
            {
                var addresses = pickupLocations.Where(s => s.Id == addressID);
                if (addresses.Count() == 0)
                {
                    addresses = pickupLocations.Where(s => s.IsPrimary);
                }
                return addresses.Count() == 0 ? null : addresses.First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void gvSavedPickupLocation_RowEditing(object sender, GridViewEditEventArgs e)
        {
            //AjaxControlToolkit.ModalPopupExtender mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucAddEditLocation.FindControl("popup_AddEditShippingControl");
            //OnShippingAddressBeingChanged(this, null);
            ////ucAddEditLocation.RenderCommandCentricView(new ShippingAddressCommand(ShippingAddressCommandType.ADD));
            //mpAddAddress.Show();
        }

        protected void gvSavedPickupLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //AjaxControlToolkit.ModalPopupExtender mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucAddEditLocation.FindControl("popup_AddEditShippingControl");
            //OnShippingAddressBeingChanged(this, null);
            //mpAddAddress.Show();
        }

        protected void gvSavedPickupLocation_RowCreated(object sender, GridViewRowEventArgs e)
        {
            //cell.Controls.Add((Control)addressBase);
            //e.Row.Cells[3].Controls.Add((Control)addressBase);
        }

        protected void gvSavedPickupLocation_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSavedPickupLocation.PageIndex = e.NewPageIndex;
            gvSavedPickupLocation.DataBind();
        }

        protected void gvSavedPickupLocation_Sorting(object sender, GridViewSortEventArgs e)
        {
            // get values from viewstate   
            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;
            // on first time header clicked ( including different header), sort ASCENDING    
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
            gvSavedPickupLocation_DataBind(sortExpression, sortDirection);
        }

        private void gvSavedPickupLocation_DataBind(String sortExpression, String sortDirection)
        {
            // save state for next time   
            ViewState["_GridView1LastSortDirection_"] = sortDirection;
            ViewState["_GridView1LastSortExpression_"] = sortExpression;

            IEnumerable<DeliveryOption> q;
            List<DeliveryOption> list = null;

            try
            {
                if (pickupLocations != null)
                {
                    if (sortDirection == "Descending")
                    {
                        q = from m in pickupLocations
                            orderby m.IsPrimary descending, OrderBy(sortExpression, m) descending
                            select m;
                    }
                    else
                    {
                        if (sortExpression == "Alias")
                        {
                            q = from m in pickupLocations
                                orderby m.IsPrimary descending,
                                    (string.IsNullOrEmpty(m.Alias) ? "zzzzz" : m.Alias) ascending,
                                    OrderBy(sortExpression, m)
                                select m;
                        }
                        else
                        {
                            q = from m in pickupLocations
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

            gvSavedPickupLocation.DataSource = list;
            gvSavedPickupLocation.DataBind();
        }

        private object OrderBy(String sortKey, DeliveryOption pickupLocation)
        {
            switch (sortKey)
            {
                case "Alias":
                    return pickupLocation.Alias;
                case "Description":
                    return pickupLocation.Description;
                default:
                    break;
            }

            return pickupLocation.Description;
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
            //loadShippingAddress();
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
            //loadShippingAddress();
        }

        private void loadShippingAddress()
        {
            try
            {
                pickupLocations = new List<DeliveryOption>();
                var pickupLocationPreferences =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode)
                                          .Where(s => s.ID > 0)
                                          .ToList();
                ;
                if (pickupLocationPreferences.Count > 0)
                {
                    foreach (PickupLocationPreference_V01 pref in pickupLocationPreferences)
                    {
                        int pickupLocationID = pref.PickupLocationID;
                        var shippingInfo =
                            ShippingProvider.GetShippingProvider(CountryCode)
                                            .GetShippingInfoFromID(DistributorID, Locale, DeliveryOptionType.Pickup,
                                                                   pickupLocationID, 0);
                        if (null != shippingInfo)
                        {
                            var deliveryOption = new DeliveryOption(shippingInfo.WarehouseCode, shippingInfo.FreightCode,
                                                                    DeliveryOptionType.Pickup);
                            if (shippingInfo.Address != null)
                                deliveryOption.Address = shippingInfo.Address.Address;
                            deliveryOption.Alias = pref.PickupLocationNickname;
                            deliveryOption.Id = pref.PickupLocationID;
                            deliveryOption.Description = shippingInfo.Description;
                            deliveryOption.IsPrimary = pref.IsPrimary;
                            pickupLocations.Add(deliveryOption);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

            gvSavedPickupLocation.DataSource = pickupLocations;
            gvSavedPickupLocation.DataBind();

            if (sortExpression == null)
            {
                sortExpression = "Alias";
            }

            if (sortDirection == null)
            {
                sortDirection = "Ascending";
            }
            gvSavedPickupLocation_DataBind(sortExpression, sortDirection);

            if (pickupLocations != null && pickupLocations.Count > 0)
            {
                tblSavedPickupLocation.Visible = true;
            }
            else
            {
                tblSavedPickupLocation.Visible = false;
            }
        }

        public ShippingAddress_V02 CreateAddress()
        {
            return new ShippingAddress_V02
                {
                    Address = new Address_V01
                        {
                            Country = CountryCode,
                        }
                };
        }
    }
}