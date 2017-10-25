using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public partial class NAMAddDeletePickupControl : UserControlBase
    {
        #region Events
        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.ADD);
            RenderCommandCentricView(SourceCommand);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.DELETE);
            var args = e as DeliveryOptionEventArgs;
            WorkedUponDeliveryOptionId = args.DeliveryOptionId;
            Session["IDToDelete"] = args.DeliveryOptionId;
            PickupLocationDescription = args.Description;
            RenderCommandCentricView(SourceCommand);
        }
        #endregion

        #region Constants
        public static string SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND = "SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND";
        public static string SESSION_KEY_PICKUPLOC_DESCRIPTION = "SESSION_KEY_PICKUPLOC_DESCRIPTION";
        public static string SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID = "SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID";
        public const string PICKUPLOC_PREFERENCE_CACHE_PREFIX = "PickupLocationPreference_";
        #endregion

        #region Properties and fields
        public int WorkedUponDeliveryOptionId
        {
            get { return (int)Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID]; }
            set { Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID] = value; }
        }

        public string PickupLocationDescription
        {
            get { return (string)Session[SESSION_KEY_PICKUPLOC_DESCRIPTION]; }
            set { Session[SESSION_KEY_PICKUPLOC_DESCRIPTION] = value; }
        }

        public PickupCommand SourceCommand
        {
            get { return (PickupCommand)Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND]; }
            set { Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND] = value; }
        }

        /// <summary>
        /// The location list.
        /// </summary>
        public static List<DeliveryOption> PickupLocations { get; set; }

        private HLGoogleMapper Mapper;
        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            var distanceUnit = GetLocalResourceObject("Unit") as string;
            foreach (ListItem item in rblDistanceRadio.Items)
            {
                item.Text = string.Format(item.Text, distanceUnit);
            }

            if (!Page.IsPostBack)
            {
                HideSelected();
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }
        }

        private void PopulatePickupInfoDataList()
        {
            lblErrors.Text = string.Empty;
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                var shippingAddress = new ShippingAddress_V02
                {
                    Address = new Address_V01
                    {
                        Country = this.ProductsBase.CountryCode,
                    }
                };
                if (divZipLookup.Visible)
                {
                    shippingAddress.Address.PostalCode = txtPostalCode.Text;
                }
                else
                {
                    shippingAddress.Address.StateProvinceTerritory = dnlState.SelectedValue;
                    shippingAddress.Address.City = txtCity.Text;
                }

                PickupLocations = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
                if (PickupLocations != null)
                {
                    decimal radio = (CountryCode == "CA") ? 8M : 5M;
                    var distanceUnit = GetLocalResourceObject("Unit") as string ?? "miles";
                    var distance = rblDistanceRadio.SelectedItem.Text.Substring(0, rblDistanceRadio.SelectedItem.Text.IndexOf(distanceUnit)).Trim();
                    if (!decimal.TryParse(distance, out radio))
                    {
                        radio = (CountryCode == "CA") ? 8M : 5M;
                    }

                    var source =
                        (
                            from o in PickupLocations
                            orderby o.displayIndex ascending 
                            where o.Distance <= radio
                            select new
                            {
                                ID = o.Id,
                                Name = o.Name,
                                DisplayName = o.DisplayName,
                                CourierStoreId = o.CourierStoreId,
                                BranchName = o.CourierType,
                                o.Address,
                                AdditionalInformation = o.Information,
                                Phone = o.Phone,
                                HasDistance = !string.IsNullOrEmpty(o.DistanceUnit),
                                Distance = o.Distance,
                                Unit = o.DistanceUnit
                            }
                        ).ToList();

                    if (source.Count > 0)
                    {
                        grdPickupInfo.DataSource = source;
                        grdPickupInfo.DataBind();
                        DisplayMapLocations(PickupLocations);
                        divLocations.Visible = true;
                        lblLocations.Text = string.Format(
                            GetLocalResourceObject("lblLocationsResource.Text") as string, source.Count());
                        lblLocations.Visible = true;
                    }
                    else
                    {
                        grdPickupInfo.DataSource = null;
                        grdPickupInfo.DataBind();
                        divLocations.Visible = true;
                        lblErrors.Text = GetLocalResourceObject("NoLocations") as string;
                        lblLocations.Visible = false;
                    }
                }
                else
                {
                    divLocations.Visible = false;
                    lblErrors.Text = GetLocalResourceObject("LocationNotFound") as string;
                }
            }
        }

        private void DisplayMapLocations(List<DeliveryOption> locations)
        {
            if (locations.Count > 0)
            {
                phMap.Controls.Clear();
                Mapper = (HLGoogleMapper)LoadControl("../HLGoogleMapper.ascx");
                phMap.Controls.Add(Mapper);
                // if there are not geographic poinst then locate by the address
                if (locations.Any(p => string.IsNullOrEmpty(p.GeographicPoint)))
                {
                    var addressList = PickupLocations.Select(c => c.Address).ToList();
                    Mapper.DispalyAddressOnMap(addressList);
                }
                else
                {
                    var mappingLocations = (from loc in locations
                                            select
                                                new DeliveryOption()
                                                    {
                                                        CourierType = loc.CourierType,
                                                        CourierStoreId = loc.CourierStoreId,
                                                        Address = loc.Address,
                                                        DisplayName = loc.DisplayName,
                                                        GeographicPoint = loc.GeographicPoint
                                                    }).ToList();
                    mappingLocations.ForEach(x => x.DisplayName = x.CourierType + " # " + x.CourierStoreId + "<br/> " + x.Address.Line1 + " " + x.Address.Line2 + " " + x.Address.City + " " + x.Address.PostalCode);
                    Mapper.DispalyAddressOnMap(mappingLocations);
                }
            }            
        }

        private void PopulateStateDropDown()
        {
            dnlState.Items.Clear();
            var states = new List<string>();

            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                if (ProductsBase.CountryCode == "US")
                {
                    var providerUS = ProductsBase.GetShippingProvider() as ShippingProvider_US;
                    if (providerUS != null)
                    {
                        states = providerUS.GetStatesForCountryToDisplay(ProductsBase.CountryCode);
                    }
                }
                else
                {
                    states = provider.GetStatesForCountry(ProductsBase.CountryCode);   
                }
                if (states.Count > 0)
                {
                    var items = (from state in states
                                 select new ListItem { Text = state, Value = state.Substring(0, 2) }).ToArray();
                    dnlState.Items.AddRange(items);
                    dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
                    dnlState.SelectedIndex = 0;
                }
            }
        }

        public void RenderCommandCentricView(PickupCommand command)
        {
            SourceCommand = command;
            switch (SourceCommand.Mode)
            {
                case PickupCommandType.ADD:
                    RenderAddPickupView();
                    return;

                case PickupCommandType.DELETE:
                    RenderDeletePickupView();
                    return;
            }
        }

        private void RenderAddPickupView()
        {
            lblErrors.Text = string.Empty;
            lblHeader.Visible = true;
            btnCancel.Visible = true;
            btnContinue.Visible = true;
            divAddPickUp.Visible = true;
            divDeletePickUp.Visible = false;
            divLocations.Visible = false;
            btnContinue.Text = GetLocalResourceObject("btnContinueResource.Text") as string;
            HideSelected();
            dnlState.Items.Clear();
            txtCity.Text = string.Empty;
            txtPostalCode.Text = string.Empty;
            PopulateStateDropDown();
            dnlState.SelectedIndex = -1;
            if (Session["AddClickedFromPickupPref"] != null)
            {
                bool CmdFromOrderPref = false;
                bool.TryParse(Session["AddClickedFromPickupPref"].ToString(), out CmdFromOrderPref);
                if (CmdFromOrderPref)
                {
                    cbSaveThis.Checked = true;
                    cbSaveThis.Enabled = false;
                    Session["AddClickedFromPickupPref"] = null;
                }
            }
            divSaveOptions.Visible = false;
            rblDistanceRadio.SelectedIndex = 0;
        }

        private void RenderDeletePickupView()
        {
            lblHeader.Visible = false;
            divAddPickUp.Visible = false;
            divDeletePickUp.Visible = true;
            HideSelected();
            btnContinue.Text = GetLocalResourceObject("btnDeleteResource.Text") as string;

            var shipAddr = new ShippingAddress_V01();
            shipAddr.Address = new Address_V01();
            shipAddr.Address.Country = CountryCode;

            var deliveryType = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier
                                   ? DeliveryOptionType.PickupFromCourier
                                   : DeliveryOptionType.ShipToCourier;
            List<PickupLocationPreference_V01> pickUpLocationPreferences =
                ProductsBase.GetShippingProvider()
                            .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, deliveryType);
            PickupLocationPreference_V01 selectedPickupPreference = pickUpLocationPreferences.Find
                (p => p.PickupLocationID == WorkedUponDeliveryOptionId);

            lblName.Text = PickupLocationDescription;
            lblDeleteIsPrimaryText.Text = selectedPickupPreference.IsPrimary
                                              ? GetLocalResourceObject("PrimaryYes.Text") as string
                                              : GetLocalResourceObject("PrimaryNo.Text") as string;
            lblDeleteNicknameText.Text = selectedPickupPreference.PickupLocationNickname;
            divLocation.Visible = false;
            divPrimary.Visible = false;
            btnContinue.Enabled = true;
            if (pickUpLocationPreferences.Count == 1)
            {
                lblErrors.Text = GetLocalResourceObject("LastPickupAddress.Text") as string;
            }
        }

        private Object GetAddress(Address_V01 address)
        {
            var stringList = new List<string>();
            if (address != null)
            {
                stringList.Add(address.Line1);
                stringList.Add(string.Format("{0}, {1} {2}", address.City, address.StateProvinceTerritory, address.PostalCode));
            }
            return stringList;
        }

        private void UpdateViewChanges(int selected)
        {
            var pickupLocationId = GetSelected(selected);
            if (pickupLocationId < 0)
            {
                lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpLocation");
                return;
            }

            bool isPrimary = cbMakePrimary.Checked;
            bool isSession = !HLConfigManager.Configurations.CheckoutConfiguration.SavePickupFromCourierPreferences;
            string nickName = ViewState["DisplayName"] as string ?? string.Empty; 
            string branchName = ViewState["BranchName"] as string ?? string.Empty;

            int returnId = ProductsBase.GetShippingProvider()
                                       .SavePickupLocationsPreferences(DistributorID, isSession, pickupLocationId,
                                                                       nickName, nickName, CountryCode, isPrimary, branchName);

            if (returnId == -2) //duplicate nickname
            {
                lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateAddressNickname");
                return;
            }

            if (returnId == -3) //duplicate pickuplocation
            {
                lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicatePickupLocation");
                return;
            }

            OnPickupPreferenceCreated(this, new DeliveryOptionEventArgs(returnId, branchName));
        }

        private int GetSelected(int index)
        {
            var selectedRow = grdPickupInfo.Rows[index];
            int locationId = Convert.ToInt32(grdPickupInfo.DataKeys[selectedRow.RowIndex]["ID"]);
            ViewState["BranchName"] = grdPickupInfo.DataKeys[selectedRow.RowIndex]["BranchName"];
            ViewState["DisplayName"] = grdPickupInfo.DataKeys[selectedRow.RowIndex]["DisplayName"];
            return locationId;
        }

        private void ShowSelected()
        {
            lblLocations.Visible = true;
            //divLocations.Visible = true;
            lblNickname.Visible = true;
            txtNickname.Visible = true;
            cbMakePrimary.Visible = true;
            cbSaveThis.Visible = true;
            btnCancel.Visible = true;
            btnContinue.Visible = true;
        }

        private void HideSelected()
        {
            lblLocations.Visible = false;
            divLocations.Visible = false;
            lblNickname.Visible = false;
            txtNickname.Visible = false;
            cbMakePrimary.Visible = false;
            cbSaveThis.Visible = false;
        }

        private void ClosePopup()
        {
            if (PopupExtender != null)
                PopupExtender.ClosePopup();
        }

        private void HideMap(bool hide)
        {
            if (null != displayMap)
            {
                if (hide)
                {
                    displayMap.Attributes["style"] = "display: none";
                    dvFragment.Visible = true;

                }
                else
                {
                    displayMap.Attributes["style"] ="display: inline-block";
                    dvFragment.Visible = false;
                }
            }
        }

        private void HideLocations(bool hide)
        {
            if (null != dvLocations)
            {
                if (hide)
                {
                    dvFragment.Visible = false;
                    dvLocations.Attributes["style"] = "display: none";

                }
                else
                {
                    dvFragment.Visible = true;
                    dvLocations.Attributes["style"] = "display: inline-block";
                }
            }
        }
        
        #endregion

        #region EventHandlers

        protected void CancelChanges_Clicked(object sender, EventArgs e)
        {
            ClosePopup();
        }

        protected void ContinueChanges_Clicked(object sender, EventArgs e)
        {
            lblErrors.Text = "";
            if (SourceCommand.Mode == PickupCommandType.DELETE)
            {
                try
                {
                    if (WorkedUponDeliveryOptionId == 0)
                    {
                        int value = 0;
                        int.TryParse(Session["IDToDelete"].ToString(), out value);
                        WorkedUponDeliveryOptionId = value;
                    }
                }
                catch
                {
                    WorkedUponDeliveryOptionId = int.Parse(Session["IDToDelete"].ToString());
                }

                int returnId = (Page as ProductsBase).GetShippingProvider().DeletePickupLocationsPreferences
                    ((Page as ProductsBase).DistributorID,
                     WorkedUponDeliveryOptionId,
                     (Page as ProductsBase).CountryCode
                    );

                OnPickupPreferenceDeleted(this, new DeliveryOptionEventArgs(WorkedUponDeliveryOptionId, string.Empty));
                ClosePopup();
            }
            else
            {
                if (!string.IsNullOrEmpty(
                        HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ZipCodeLookupRegExp) &&
                    !Regex.IsMatch(txtPostalCode.Text,
                                   HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ZipCodeLookupRegExp))
                {
                    lblErrors.Text = GetLocalResourceObject("ZipCodeNotValid") as string;
                    return;
                }

                // Validate if zip code is blocked for courier
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.BlockedZipCodesForCourier))
                {
                    var blockedZip = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.BlockedZipCodesForCourier.ToLower();
                    var zipLookup = txtPostalCode.Text.Replace(" ", string.Empty).ToLower();
                    if (blockedZip.Contains(zipLookup))
                    {
                        divLocations.Visible = false;
                        lblErrors.Text = GetLocalResourceObject("LocationNotFound") as string;
                        return;
                    }
                }

                ShowSelected();
                PopulatePickupInfoDataList();
            }
        }

        protected void cbSaveThis_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbSaveThis.Checked)
            {
                cbMakePrimary.Enabled = false;
                cbMakePrimary.Checked = false;
            }
            else
            {
                cbMakePrimary.Enabled = true;
            }
            cbSaveThis.Focus();
        }

        protected void GrdLocation_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            lblErrors.Text = string.Empty;
            var index = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "SelectCommand":
                    UpdateViewChanges(index);
                    if (lblErrors.Text.Equals(String.Empty))
                    {
                        ClosePopup();
                    }
                    break;
            
                case "DisplayCommand":
                    HideMap(false);
                    HideLocations(true);
                    var selectedRow = grdPickupInfo.Rows[index];
                    int locationId = Convert.ToInt32(grdPickupInfo.DataKeys[selectedRow.RowIndex]["ID"]);
                    var location = PickupLocations.Where(c => c.Id == locationId).ToList();
                    DisplayMapLocations(location);
                    break;
            }
        }

        protected void lnkHideMap_OnClick(object sender, EventArgs e)
        {
            HideMap(true);
        }

        protected void lnkViewMap_OnClick(object sender, EventArgs e)
        {
            HideMap(false);
            HideLocations(true);
            DisplayMapLocations(PickupLocations);
        }

        protected void rblDistanceRadio_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePickupInfoDataList();
        }

        #endregion EventHandlers

        protected void lnkViewLocations_Click(object sender, EventArgs e)
        {
            HideLocations(false);
            HideMap(true);
        }
    }
}