using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public partial class ChinaThridPartyPickupControl : UserControlBase
    {
        private HLGoogleMapper objHLGoogleMapper;
        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

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

        public List<DeliveryOption> PickupLocations { get; set; }

        public static string SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND = "SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND";
        public static string SESSION_KEY_PICKUPLOC_DESCRIPTION = "SESSION_KEY_PICKUPLOC_DESCRIPTION";
        public static string SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID = "SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                HideSelected();
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }
            divSubRegion.Visible = true;
            divNeighbourhood.Visible = true;
            divCourierInfo.Visible = false;
            //displayMap.Visible = false;

            RegisterScript();

      }


        #region EventHandlers

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                hfDiableSavedCheckbox.Value = args.DisableSaveAddressCheckbox.ToString();
            }

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

                OnPickupPreferenceDeleted(this, new DeliveryOptionEventArgs(WorkedUponDeliveryOptionId, "PickupFromCourier"));
                //popup_AddDeletePickupControl.Hide();
            }
            else
            {
                UpdateViewChanges();
            }
            string Errormsg = lblErrors.Text;
            if (lblErrors.Text.Equals(String.Empty))
            {
                if (PopupExtender != null)
                    PopupExtender.ClosePopup();
            }
            else
            {
                PopulatePickupInfoDataList();
                lblErrors.Text = Errormsg;
            }

      
        }

        protected void dnlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            dnlDistrict.Items.Clear();
            dnlCity.Items.Clear();
            PopulateCityDropDown();
       
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            dnlDistrict.Items.Clear();
            PopulateDistrictDropDown();
      
        }

        protected void dnlDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelected();
            PopulatePickupInfoDataList();
     
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

        private void PopulatePickupInfoDataList()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                ShippingAddress_V02 shippingAddress = null;

                shippingAddress = new ShippingAddress_V02
                {
                    Address = new Address_V01
                    {
                        Country = this.ProductsBase.CountryCode,
                        StateProvinceTerritory = dnlProvince.Text,
                        City = dnlCity.Text,
                        CountyDistrict = dnlDistrict.Text
                    }
                };

                List<DeliveryOption> pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                                 shippingAddress);
                if (pickupOptions != null)
                {
                    divLocations.Visible = true;
                    if (
                        !string.IsNullOrEmpty(
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration.CourierInfoUrl))
                    {
                        hlCourierInfo.NavigateUrl =
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration.CourierInfoUrl;
                        divCourierInfo.Visible = true;
                    }
                    else
                    {
                        divCourierInfo.Visible = false;
                    }

                    dlPickupInfo.DataSource =
                        (
                            from o in pickupOptions
                            where o.State.Trim() == dnlProvince.Text.Trim()
                            && o.Address.City == dnlCity.Text.Trim()
                            && o.Address.CountyDistrict == dnlDistrict.Text.Trim()
                            select new
                            {
                                ID = o.Id,
                                IsPickup = true,
                                IsWaldos = false,
                                BranchName = o.Description,
                                o.FreightCode,
                                Warehouse = o.WarehouseCode,
                                o.Address,
                                o.Information
                            }
                        );
                    dlPickupInfo.DataBind();
                    List<Address_V01> lstaddress = new List<Address_V01>();
                    lstaddress = (from o in pickupOptions
                                  where o.State.Trim() == dnlProvince.Text.Trim()
                                        && o.Address.City == dnlCity.Text.Trim()
                                        && o.Address.CountyDistrict == dnlDistrict.Text.Trim()
                                  select o.Address).ToList();
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasBaiduMap)
                    {
                        objHLGoogleMapper =
                            (HLGoogleMapper)LoadControl("../HLGoogleMapper.ascx");
                        PlaceHolder1.Controls.Add(objHLGoogleMapper);
                        if (lstaddress.Count > 0)
                        {

                            objHLGoogleMapper.DispalyAddressOnMap(lstaddress);

                        }
                        else
                        {
                            HideSelected();
                        }
                    }
                }
                if (pickupOptions.Count.Equals(0))
                    HideSelected();
                lblErrors.Text = string.Empty;
            }
        }
        #endregion EventHandlers

        # region Methods

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
            HideSelected();
            dnlProvince.Items.Clear();
            dnlCity.Items.Clear();
            dnlDistrict.Items.Clear();
            PopulateProvinceDropDown();
            dnlProvince.SelectedIndex = -1;
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
            trLocation.Visible = false;
            trPrimary.Visible = false;

            btnContinue.Enabled = true;

            if (pickUpLocationPreferences.Count == 1)
            {
                lblErrors.Text = GetLocalResourceObject("LastPickupAddress.Text") as string;
            }
            if (ShoppingCart.DeliveryInfo != null)
                ShoppingCart.DeliveryInfo = null;
        }

        public Object GetAddress(Address_V01 address)
        {
            var stringList = new List<string>();
            if (address != null)
            {
                stringList.Add(string.Format("{0}{1}{2}", address.City, address.CountyDistrict, address.Line1));
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
            string nickName = string.IsNullOrEmpty(txtNickname.Text.Trim()) ? pickupLocationId + "," + branchName : txtNickname.Text.Trim();

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

        private void PopulateProvinceDropDown()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                var provinces = (from p in provider.GetStatesForCountry(ProductsBase.CountryCode)
                                 select p.Split('-')[1]);

                dnlProvince.DataSource = provinces;

                dnlProvince.DataBind();
                dnlProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                dnlProvince.SelectedIndex = 0;
            }
            if (dnlProvince.SelectedIndex != 0 || dnlCity.SelectedIndex != 0 || dnlDistrict.SelectedIndex != 0)
            {
                HideSelected();
            }
        }

        private void PopulateCityDropDown()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null && dnlProvince.SelectedIndex != 0)
            {
                var shippingAddress = new ShippingAddress_V02
                {
                    Address =
                            new Address_V01
                            {
                                Country = ProductsBase.CountryCode,
                                StateProvinceTerritory = dnlProvince.Text
                            }
                };
                var pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                shippingAddress);
                var cities = (from p in pickupOptions
                              where p.Address.StateProvinceTerritory.Equals(dnlProvince.Text)
                              select p.Address.City).Distinct().OrderBy(s => s).ToList();
                dnlCity.DataSource = cities;
                dnlCity.DataBind();
                dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                dnlCity.SelectedIndex = 0;

            }
            if (dnlProvince.SelectedIndex != 0 || dnlCity.SelectedIndex != 0 || dnlDistrict.SelectedIndex != 0)
            {
                HideSelected();
            }

        }

        private void PopulateDistrictDropDown()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null && dnlProvince.SelectedIndex != 0 && dnlCity.SelectedIndex != 0)
            {
                var shippingAddress = new ShippingAddress_V02
                {
                    Address =
                            new Address_V01
                            {
                                Country = ProductsBase.CountryCode,
                                StateProvinceTerritory = dnlProvince.Text
                            }
                };
                var pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                shippingAddress);
                var districts = (from p in pickupOptions
                                 where
                                     p.State.Trim().Equals(dnlProvince.Text.Trim()) &&
                                     p.Address.City.Equals(dnlCity.Text)
                                 select p.Address.CountyDistrict).Distinct().OrderBy(s => s).ToList();
                dnlDistrict.DataSource = districts;
                dnlDistrict.DataBind();
                dnlDistrict.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                dnlDistrict.SelectedIndex = 0;

            }
            if (dnlProvince.SelectedIndex != 0 || dnlCity.SelectedIndex != 0 || dnlDistrict.SelectedIndex != 0)
            {
                HideSelected();
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
            lblErrors.Text = string.Empty;
        }
        protected void btnToggle_Click(object sender, EventArgs e)
        {
            //            string toggle = @"function toggle_visibility(id) {
            //             var e = document.getElementById(id);
            //             if (e.style.display == 'block')
            //                 e.style.display = 'none';
            //             else
            //                 e.style.display = 'block';
            //         }";
            //            ScriptManager.RegisterStartupScript(this, typeof(Page), "showhide", toggle, true);
        }


        #endregion

        private void RegisterScript()
        {
            string script = "function Submission() {" +
                                "$('#" + divAddPickUp.ClientID + "  :input').prop('disabled', true); " +
                                "$('#" + divDeletePickUp.ClientID + "  :input').prop('disabled', true); " +
                            "}";

            ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "Submission_ThirdPartyPickup", script, true);
        }
    }
}