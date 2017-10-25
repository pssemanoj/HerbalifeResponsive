using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System.Threading;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public partial class MexicoAddDeletePickupControl : UserControlBase
    {
        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

        //[Publishes(MyHLEventTypes.CommandCancelled)]
        //public event EventHandler OnCommandCancelled;

        public int WorkedUponDeliveryOptionId
        {
            get { return (int) Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID]; }
            set { Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID] = value; }
        }

        public string PickupLocationDescription
        {
            get { return (string) Session[SESSION_KEY_PICKUPLOC_DESCRIPTION]; }
            set { Session[SESSION_KEY_PICKUPLOC_DESCRIPTION] = value; }
        }

        public PickupCommand SourceCommand
        {
            get { return (PickupCommand) Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND]; }
            set { Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND] = value; }
        }

        public List<DeliveryOption> PickupLocations { get; set; }
        public static string SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND = "SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND";
        public static string SESSION_KEY_PICKUPLOC_DESCRIPTION = "SESSION_KEY_PICKUPLOC_DESCRIPTION";
        public static string SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID = "SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                HidePostalCodeAndStateControls();
                HideSelected();
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }
            //this.Attributes.Add("onkeypress", "enterKey");
            //if (!IsPostBack)
            //{
            //    this.Attributes.Add("onkeypress", "enterKey");
            //    populatePickupInfoDataList();
            //}
        }

        private void PopulatePickupInfoDataList()
        {
            Providers.Interfaces.IShippingProvider provider = null;
            if (Thread.CurrentThread.CurrentCulture.Name == "es-MX")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
            else if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_AR;

            if (provider != null)
            {
                var shippingAddress = new ShippingAddress_V02();
                var address = new Address_V01();
                address.PostalCode = ViewState["PostalCode"].ToString();
                shippingAddress.Address = address;

                List<DeliveryOption> pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                 shippingAddress);
                if (pickupOptions != null)
                {
                    divLocations.Visible = true;
                    dlPickupInfo.DataSource =
                        (
                            from o in pickupOptions
                            orderby o.Description
                            select new
                                { 
                                    ID = o.Id,
                                    IsPickup = true,
                                    Description = new Func<string>(() =>
                                    {
                                        GetImage(o.Description.ToUpper());
                                        return o.Description.ToUpper().ToString();
                                    })(),
                                    BranchName = o.Name,
                                    o.FreightCode,
                                    Warehouse = o.WarehouseCode,
                                    o.Address
                                }
                        );

                    dlPickupInfo.DataBind();
                }

                if (pickupOptions.Count.Equals(0))
                    divLocations.Visible = false;
            }
        }

        protected string GetImage(string courrier)
        {
            if (courrier.Contains("BENAVIDES"))
            {
                return "/Content/es-MX/img/order_icon_benavides.png";
            }
            else if (courrier.Contains("WALDOS"))
            {
                return "/Content/Global/Products/img/order_icon_waldos.png";
            }
            else if (courrier.Contains("MODATELAS"))
            {
                return "/Content/es-MX/img/order_icon_modatelas.png";
            }
            else
            {
                return "/Content/Global/Products/img/order_icon_HL.png";
            }
        }

        #region EventHandlers

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            //if ((Page as ProductsBase).Locale != "es-MX")
            //    return;

            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                hfDiableSavedCheckbox.Value = args.DisableSaveAddressCheckbox.ToString();
            }

            lblErrors.Text = "";
            //Set the MODE & Load respective controlset
            SourceCommand = new PickupCommand(PickupCommandType.ADD);
            RenderCommandCentricView(SourceCommand);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            //if ((Page as ProductsBase).Locale != "es-MX")
            //    return;

            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.DELETE);
            var args = e as DeliveryOptionEventArgs;
            WorkedUponDeliveryOptionId = args.DeliveryOptionId;
            Session["IDToDelete"] = args.DeliveryOptionId;
            PickupLocationDescription = args.Description;

            RenderCommandCentricView(SourceCommand);
        }

        protected void PostalCode_TextChanged(Object sender, EventArgs e)
        {
            ViewState["PostalCode"] = txtPostalCode.Text.Trim();
            PopulatePickupInfoDataList();
            ShowSelected();

            //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
            if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
                cbSaveThis.Visible = false;
        }

        protected void CancelChanges_Clicked(object sender, EventArgs e)
        {
            if (PopupExtender != null)
                PopupExtender.ClosePopup();
        }

        public const string PICKUPLOC_PREFERENCE_CACHE_PREFIX = "PickupLocationPreference_";

        protected string getPickupLocationPreferenceKey(string distributorID, string country)
        {
            return string.Format("{0}_{1}_{2}", distributorID, country, PICKUPLOC_PREFERENCE_CACHE_PREFIX);
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

                string cacheKey = getPickupLocationPreferenceKey((Page as ProductsBase).DistributorID,
                                                                 (Page as ProductsBase).CountryCode);
                if (HttpRuntime.Cache[cacheKey] != null)
                {
                    HttpRuntime.Cache.Remove(cacheKey);
                }

                OnPickupPreferenceDeleted(this, new DeliveryOptionEventArgs(WorkedUponDeliveryOptionId, string.Empty));
                //popup_AddDeletePickupControl.Hide();
            }
            else
            {
                UpdateViewChanges();
            }

            if (lblErrors.Text.Equals(String.Empty))
            {
                if (PopupExtender != null)
                    PopupExtender.ClosePopup();
            }
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateMunicipalDropDown(dnlMunicipal);
            PopulateTownDropDown(dnlTown);
            divLocations.Visible = false;
            HideSelected();
        }

        protected void dnlMunicipal_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateTownDropDown(dnlTown);

            if (dnlTown.Items.Count > 1)
            {
                divLocations.Visible = false;
                HideSelected();
            }
            else
                DisplayLocations();
        }

        protected void dnlTown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlState.SelectedIndex <= 0 || dnlMunicipal.SelectedIndex <= 0 || (dnlTown.Items.Count > 1 && dnlTown.SelectedIndex <= 0))
            {
                lblErrors.Visible = true;
                lblErrors.Text = GetLocalResourceObject("InvalidDropDown") as string;
                return;
            }
            DisplayLocations();
        }

        protected void rbState_CheckedChanged(object sender, EventArgs e)
        {
            if (rbState.Checked)
            {
                lblErrors.Text = "";
                lblPostalCode.Visible = false;
                txtPostalCode.Visible = false;

                lblState.Visible = true;
                lblMunicipal.Visible = true;
                lblColonia.Visible = true;
                dnlState.Visible = true;
                dnlMunicipal.Visible = true;
                dnlTown.Visible = true;

                HideSelected();
                PopulateStateDropDown();
            }
        }

        protected void rbPostalCode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPostalCode.Checked)
            {
                lblErrors.Text = "";
                lblPostalCode.Visible = true;
                txtPostalCode.Visible = true;

                lblState.Visible = false;
                lblMunicipal.Visible = false;
                lblColonia.Visible = false;
                dnlState.Visible = false;
                dnlMunicipal.Visible = false;
                dnlTown.Visible = false;

                HideSelected();
            }
        }

        protected void cbSaveThis_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbSaveThis.Checked)
            {
                cbMakePrimary.Enabled = false;
                cbMakePrimary.Checked = false;
                //this.btnCancel.Focus();
            }
            else
            {
                cbMakePrimary.Enabled = true;
                //this.cbMakePrimary.Focus();
            }
            cbSaveThis.Focus();
        }

        #endregion

        # region Methods

        private void DisplayLocations()
        {
            var zip = string.Empty;
            Providers.Interfaces.IShippingProvider provider = null;
            if (Thread.CurrentThread.CurrentCulture.Name == "es-MX")
            {
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
                if (dnlMunicipal.SelectedIndex > 0 && dnlState.SelectedIndex > 0 && dnlTown.SelectedIndex > 0)
                {
                    zip = provider.LookupZipCode(dnlState.Text, dnlMunicipal.Text, dnlTown.Text);
                }
            }
            else if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
            { 
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_AR;
                if (dnlMunicipal.SelectedIndex > 0 && dnlState.SelectedIndex > 0)
                {
                    if (!dnlTown.Enabled)
                    {
                        var lookupResults = provider.GetAddressField(new AddressFieldForCountryRequest_V01()
                        {
                            AddressField = AddressPart.ZIPCODE,
                            Country = ProductsBase.CountryCode,
                            State = dnlState.Text,
                            City = dnlMunicipal.Text
                        });
                        zip = lookupResults != null ? lookupResults.FirstOrDefault() : null;
                    }
                    else if (dnlTown.SelectedIndex > 0)
                    {
                        var lookupResults = provider.GetZipsForCounty(ProductsBase.CountryCode, dnlState.Text, dnlMunicipal.Text, dnlTown.Text);
                        zip = lookupResults != null ? lookupResults.FirstOrDefault() : null;
                    }
                }
            }
            if (!string.IsNullOrEmpty(zip))
            {
                ViewState["PostalCode"] = zip;
                PopulatePickupInfoDataList();
                ShowSelected();
                //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
                if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
                    cbSaveThis.Visible = false;
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
            btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            HidePostalCodeAndStateControls();
            HideSelected();
            trstateRadio.Visible = true;
            rbPostalCode.Checked = false;
            rbState.Checked = false;
            txtPostalCode.Text = string.Empty;
            dnlState.SelectedIndex = -1;
            dnlMunicipal.SelectedIndex = -1;
            dnlTown.SelectedIndex = -1;
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

            //Bug: 28193:  First Pickup address should be primary and saved.
            List<PickupLocationPreference_V01> pickUpLocationPreferences = (Page as ProductsBase).GetShippingProvider().
                                                                                                  GetPickupLocationsPreferences
                (
                    (Page as ProductsBase).DistributorID,
                    CountryCode);

            if (null != pickUpLocationPreferences)
            {
                if (pickUpLocationPreferences.Count == 0)
                {
                    cbMakePrimary.Checked = true;
                    cbMakePrimary.Enabled = false;
                    cbSaveThis.Checked = true;
                    cbSaveThis.Enabled = false;
                }
                else
                {
                    cbMakePrimary.Checked = false;
                    cbMakePrimary.Enabled = true;
                    cbSaveThis.Checked = true;
                    cbSaveThis.Enabled = true;
                }
            }
            //else 
            //{
            //    this.cbMakePrimary.Checked = false; this.cbMakePrimary.Enabled = true;
            //    this.cbSaveThis.Checked = true; this.cbSaveThis.Enabled = true;
            //}

            //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
            if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
                cbSaveThis.Visible = false;
        }

        private void RenderDeletePickupView()
        {
            lblHeader.Visible = false;
            divAddPickUp.Visible = false;
            divDeletePickUp.Visible = true;
            HideSelected();
            btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;

            var shipAddr = new ShippingAddress_V01();
            shipAddr.Address = new Address_V01();
            shipAddr.Address.Country = CountryCode;

            //List<DeliveryOption> deliveryOptionList = (this.Page as ProductsBase).GetShippingProvider().
            //    GetDeliveryOptions(DeliveryOptionType.Pickup, shipAddr);

            //DeliveryOption pickupDeliveryOption = deliveryOptionList.Find(p => p.Id == this.WorkedUponDeliveryOptionId);
            //this.lblName.Text = pickupDeliveryOption.Description;


            //string controlPath = getDeleteAddressControlPath(ref isXML);
            //ordering.AddressBase addressBase = new ordering.AddressControl();
            //addressBase.XMLFile = controlPath;
            //this.colDeletePickUp.Controls.Add((Control)addressBase);

            //addressBase.DataContext = pickupDeliveryOption;

            List<PickupLocationPreference_V01> pickUpLocationPreferences = (Page as ProductsBase).GetShippingProvider().
                                                                                                  GetPickupLocationsPreferences
                (
                    (Page as ProductsBase).DistributorID,
                    CountryCode);
            PickupLocationPreference_V01 selectedPickupPreference = pickUpLocationPreferences.Find
                (p => p.PickupLocationID == WorkedUponDeliveryOptionId);

            lblName.Text = PickupLocationDescription;

            lblDeleteIsPrimaryText.Text = selectedPickupPreference.IsPrimary
                                              ? GetLocalResourceObject("PrimaryYes.Text") as string
                                              : GetLocalResourceObject("PrimaryNo.Text") as string;
            lblDeleteNicknameText.Text = selectedPickupPreference.PickupLocationNickname;
            trstateRadio.Visible = false;
            trLocation.Visible = false;
            trPrimary.Visible = false;

            //if (selectedPickupPreference.IsPrimary) //Eval UC:3.5.3.7 (deleting primary)
            //{
            //    this.lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "PrimaryPickupPreferenceDeleteNotAllowed");
            //    this.btnContinue.Enabled = false;
            //    return;
            //}
            //else //Eval UC:3.5.3.6 (deleting non-primary)
            //{
            btnContinue.Enabled = true;
            //}
            if (pickUpLocationPreferences.Count == 1)
            {
                lblErrors.Text = GetLocalResourceObject("LastPickupAddress.Text") as string;
            }
            //else
            //{
            //    this.btnContinue.Enabled = true;
            //}
        }

        public Object GetAddress(Address_V01 address)
        {
            var stringList = new List<string>();
            if (address != null)
            {
                stringList.Add(address.Line1);
                stringList.Add(address.Line2);
                stringList.Add(address.City + "," + address.StateProvinceTerritory);
            }
            return stringList;
        }

        private void UpdateViewChanges()
        {
            String pickupLocationId = GetSelected();
            if (pickupLocationId.Equals(""))
            {
                lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpLocation");
                return;
            }

            DataListItem chosenPickupItem = dlPickupInfo.SelectedItem;
            bool isPrimary = cbMakePrimary.Checked;
            bool isSession = cbSaveThis.Checked ? false : true;
            string branchName = ViewState["BranchName"] as string ?? string.Empty;
            string nickName = txtNickname.Text;

            int returnId = ProductsBase.GetShippingProvider().SavePickupLocationsPreferences
                (DistributorID,
                 isSession,
                 int.Parse(pickupLocationId),
                 nickName,
                 branchName,
                 CountryCode,
                 isPrimary);

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

        public string GetSelected()
        {
            foreach (var r in dlPickupInfo.Items)
            {
                var rb = (r as DataListItem).FindControl("rbSelected") as RadioButton;
                if (rb != null && rb.Checked)
                {
                    var lbID = (r as DataListItem).FindControl("lbID") as HiddenField;
                    if (lbID != null)
                    {
                        var branchName = (r as DataListItem).FindControl("lbBranchName") as Label;
                        if (branchName != null)
                        {
                            ViewState["BranchName"] = branchName.Text;
                        }
                        return lbID.Value;
                    }
                }
            }
            return "";
        }

        private void PopulateStateDropDown()
        {
            dnlMunicipal.Enabled = false;
            dnlTown.Enabled = false;
            Providers.Interfaces.IShippingProvider provider = null;
            if (Thread.CurrentThread.CurrentCulture.Name == "es-MX")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
            else if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_AR;
            if (provider != null)
            {
                var states = provider.GetStatesForCountry(this.CountryCode);
                if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
                {
                    var arStates = new List<string>();
                    if (states != null && states.Count > 0)
                    {
                        foreach (var province in states)
                        {
                            string[] state = province.Split('-');
                            arStates.Add(state[0]);
                        }
                        states = arStates;
                    }
                }

                dnlState.DataSource = states;
                dnlState.DataBind();
                dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
                dnlState.SelectedIndex = 0;
            }
        }

        private void PopulateMunicipalDropDown(DropDownList dnlMunicipal)
        {
            Providers.Interfaces.IShippingProvider provider = null;
            if (Thread.CurrentThread.CurrentCulture.Name == "es-MX")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
            else if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_AR;
            if (provider != null)
            {
                dnlMunicipal.DataSource = provider.GetCitiesForState(CountryCode,dnlState.SelectedValue);
                dnlMunicipal.DataBind();
                dnlMunicipal.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
                dnlMunicipal.Enabled = true;
                dnlMunicipal.SelectedIndex = 0;
            }
        }

        private void PopulateTownDropDown(DropDownList dnlTown)
        {
            Providers.Interfaces.IShippingProvider provider = null;
            if (Thread.CurrentThread.CurrentCulture.Name == "es-MX")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
            else if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
                provider = ProductsBase.GetShippingProvider() as ShippingProvider_AR;
            if (provider != null)
            {
                string stateSelected = dnlState.SelectedValue;
                var towns = (Thread.CurrentThread.CurrentCulture.Name == "es-MX") ?
                    provider.GetStreetsForCity(CountryCode, stateSelected, dnlMunicipal.SelectedValue) : 
                    provider.GetCountiesForCity(CountryCode, stateSelected, dnlMunicipal.SelectedValue);
                dnlTown.DataSource = towns;
                dnlTown.DataBind();
                dnlTown.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
                dnlTown.Enabled = true;
                dnlTown.SelectedIndex = 0;
            }

            if (Thread.CurrentThread.CurrentCulture.Name == "es-AR")
            {
                // If AR disable the neightborhood dropdown when no data
                dnlTown.Enabled = dnlTown.Items.Count > 1;
            }
        }

        private void ShowSelected()
        {
            lblLocations.Visible = true;
            dlPickupInfo.Visible = true;
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
            dlPickupInfo.Visible = false;
            lblNickname.Visible = false;
            txtNickname.Visible = false;
            cbMakePrimary.Visible = false;
            cbSaveThis.Visible = false;
        }

        private void HidePostalCodeAndStateControls()
        {
            lblPostalCode.Visible = false;
            txtPostalCode.Visible = false;

            lblState.Visible = false;
            lblMunicipal.Visible = false;
            lblColonia.Visible = false;
            dnlState.Visible = false;
            dnlMunicipal.Visible = false;
            dnlTown.Visible = false;
        }

        #endregion
    }
}