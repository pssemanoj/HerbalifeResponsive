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

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public partial class ChileAddDeletePickupControl : UserControlBase
    {
        private const string CityBoxType = "CityBox";
        private const string AgencyType = "Agency";

        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

        //[Publishes(MyHLEventTypes.CommandCancelled)]
        //public event EventHandler OnCommandCancelled;

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

        public List<DeliveryOption> PickupLocations
        {
            get
            {
                var shippingAddress = new ShippingAddress_V01 {Alias = CourierType};
                return
                    (this.Page as ProductsBase).GetShippingProvider()
                                               .GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
            }
            set { }
        }

        public string CourierType
        {
            get { return hfCourierType.Value; }
            set { hfCourierType.Value = value; } 
        }

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

        }

       #region EventHandlers

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            if ((Page as ProductsBase).Locale != "es-CL")
                return;

            CourierType = CityBoxType;
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                hfDiableSavedCheckbox.Value = args.DisableSaveAddressCheckbox.ToString();
                CourierType = args.CourierType;
            }

            lblErrors.Text = "";
            //Set the MODE & Load respective controlset
            SourceCommand = new PickupCommand(PickupCommandType.ADD);
            RenderCommandCentricView(SourceCommand);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            if ((Page as ProductsBase).Locale != "es-CL")
                return;

            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.DELETE);
            var args = e as DeliveryOptionEventArgs;
            WorkedUponDeliveryOptionId = args.DeliveryOptionId;
            Session["IDToDelete"] = args.DeliveryOptionId;
            PickupLocationDescription = args.Description;
            CourierType = args.CourierType;

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
            //var args = e as DeliveryOptionEventArgs;
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

                int returnId =
                    (Page as ProductsBase).GetShippingProvider()
                                          .DeletePickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                            WorkedUponDeliveryOptionId,
                                                                            (Page as ProductsBase).CountryCode,
                                                                            CourierType);

                string cacheKey = getPickupLocationPreferenceKey((Page as ProductsBase).DistributorID,
                                                                 (Page as ProductsBase).CountryCode);
                if (HttpRuntime.Cache[cacheKey] != null)
                {
                    HttpRuntime.Cache.Remove(cacheKey);
                }

                OnPickupPreferenceDeleted(this, new DeliveryOptionEventArgs(CourierType)); // tell quick view it is deleted
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
            var shippingAddress = new ShippingAddress_V01
                {
                    Address = new Address_V01 {Country = ProductsBase.CountryCode, StateProvinceTerritory = dnlState.Text}
                };
            PopulateCityboxDropDown(shippingAddress);
            HideSelected();
        }

        protected void dnlCitybox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCourierInfo();
            ShowSelected();
        }

        //protected void cbSaveThis_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (!cbSaveThis.Checked)
        //    {
        //        cbMakePrimary.Enabled = false;
        //        cbMakePrimary.Checked = false;
        //        //this.btnCancel.Focus();
        //    }
        //    else
        //    {
        //        cbMakePrimary.Enabled = true;
        //        //this.cbMakePrimary.Focus();
        //    }
        //    cbSaveThis.Focus();
        //}

        #endregion

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
            HideSelected();
            pAddress.InnerHtml = string.Empty;
            dnlState.Items.Clear();
            dnlCitybox.Items.Clear();
            LoadDataComuna();
            btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            dnlState.SelectedIndex = -1;
            dnlCitybox.SelectedIndex = -1;
            
            if(CourierType == AgencyType)
            {
                lblCitybox.Text = GetLocalResourceObject("lnAgecyResource1.Text") as string;
                lblState.Text = GetLocalResourceObject("lbStateAgencyResource1.Text") as string;                
            }
            else
            {
                lblCitybox.Text = GetLocalResourceObject("lnCityboxResource1.Text") as string;
                lblState.Text = GetLocalResourceObject("lbStateResource1.Text") as string;
            }

            if (Session["AddClickedFromPickupPref"] != null)
            {
                bool CmdFromOrderPref = false;
                bool.TryParse(Session["AddClickedFromPickupPref"].ToString(), out CmdFromOrderPref);
                if (CmdFromOrderPref)
                {
                    //cbSaveThis.Checked = true;
                    //cbSaveThis.Enabled = false;
                    Session["AddClickedFromPickupPref"] = null;
                }
            }

            //Bug: 28193:  First Pickup address should be primary and saved.
            //List<PickupLocationPreference_V02> pickUpLocationPreferences = (Page as ProductsBase).GetShippingProvider().
            //                                                                                      GetPickupLocationsPreferences
            //    (
            //        (Page as ProductsBase).DistributorID,
            //        CountryCode, CourierType);

            //if (null != pickUpLocationPreferences)
            //{
            //    if (pickUpLocationPreferences.Count == 0)
            //    {
            //        cbMakePrimary.Checked = true;
            //        cbMakePrimary.Enabled = false;
            //        cbSaveThis.Checked = true;
            //        cbSaveThis.Enabled = false;
            //    }
            //    else
            //    {
            //        cbMakePrimary.Checked = false;
            //        cbMakePrimary.Enabled = true;
            //        cbSaveThis.Checked = true;
            //        cbSaveThis.Enabled = true;
            //    }
            //}
            //else 
            //{
            //    this.cbMakePrimary.Checked = false; this.cbMakePrimary.Enabled = true;
            //    this.cbSaveThis.Checked = true; this.cbSaveThis.Enabled = true;
            //}

            //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
            //if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
            //    cbSaveThis.Visible = false;
        }

        private void RenderDeletePickupView()
        {
            lblHeader.Visible = false;
            divAddPickUp.Visible = false;
            divDeletePickUp.Visible = true;
            HideSelected();
            btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;
            lblCitybox.Text = CourierType == AgencyType
                      ? GetLocalResourceObject("lnAgecyResource1.Text") as string
                      : GetLocalResourceObject("lnCityboxResource1.Text") as string;

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

            List<PickupLocationPreference_V02> pickUpLocationPreferences = (Page as ProductsBase).GetShippingProvider().
                                                                                                  GetPickupLocationsPreferences
                (
                    (Page as ProductsBase).DistributorID, CountryCode, CourierType);
            PickupLocationPreference_V02 selectedPickupPreference = pickUpLocationPreferences.Find
                (p => p.PickupLocationID == WorkedUponDeliveryOptionId);

            lblName.Text = PickupLocationDescription;

            lblDeleteIsPrimaryText.Text = selectedPickupPreference.IsPrimary
                                              ? GetLocalResourceObject("PrimaryYes.Text") as string
                                              : GetLocalResourceObject("PrimaryNo.Text") as string;
            lblDeleteNicknameText.Text = selectedPickupPreference.PickupLocationNickname;
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
            if (this.SourceCommand.Mode == PickupCommandType.ADD)
            {
                bool isPrimary = false; // cbMakePrimary.Checked;
                bool isSession = false; //  !cbSaveThis.Checked;
                string branchName = CourierType;
                string nickName = this.dnlCitybox.SelectedItem.Text;
                String pickupLocationId = GetSelected();
                if (pickupLocationId.Equals(""))
                {
                    lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpLocation");
                    return;
                }
                int returnId = ProductsBase.GetShippingProvider()
                                           .SavePickupLocationsPreferences(DistributorID, isSession,
                                                                           int.Parse(pickupLocationId), nickName,
                                                                           branchName, CountryCode, isPrimary,
                                                                           CourierType);

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

            if (this.SourceCommand.Mode == PickupCommandType.DELETE)
            {
                int returnId =
                    (this.Page as ProductsBase).GetShippingProvider()
                                               .DeletePickupLocationsPreferences(
                                                   (this.Page as ProductsBase).DistributorID,
                                                   this.WorkedUponDeliveryOptionId,
                                                   (this.Page as ProductsBase).CountryCode, CourierType);

                OnPickupPreferenceDeleted(this,
                                          new DeliveryOptionEventArgs(this.WorkedUponDeliveryOptionId, string.Empty));
            }

            
           
        }

        public string GetSelected()
        {
            return dnlCitybox.SelectedValue;
        }

        private void PopulateCityboxDropDown(ShippingAddress_V01 address)
        {
            dnlCitybox.DataSource = PickupLocations.Where(x => x.Address.StateProvinceTerritory == address.Address.StateProvinceTerritory);
            dnlCitybox.DataTextField = CourierType.Equals(CityBoxType) ? "Alias" : "Name";
            dnlCitybox.DataValueField = "Id";
            dnlCitybox.DataBind();
            dnlCitybox.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
            dnlCitybox.SelectedIndex = 0;
        }

       private void ShowSelected()
        {
            //lblNickname.Visible = true;
            //txtNickname.Visible = true;
            //cbMakePrimary.Visible = true;
            //cbSaveThis.Visible = true;

            btnCancel.Visible = true;
            btnContinue.Visible = true;
        }

        private void HideSelected()
        {
            //lblNickname.Visible = false;
            //txtNickname.Visible = false;
            //cbMakePrimary.Visible = false;
            //cbSaveThis.Visible = false;

        }

        private void LoadDataComuna()
        {
            var k = PickupLocations.Select(x => x.Address.StateProvinceTerritory).ToList();
            dnlState.DataSource = k.Distinct().OrderBy(x=>x);
            dnlState.DataBind();
            dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, GetLocalResourceObject("Select.Text") as string));
            dnlState.SelectedIndex = 0;
        }

        private void ShowCourierInfo()
        {
            var provider = ProductsBase.GetShippingProvider() as ShippingProvider_CL;
            if (provider != null)
            {
                var shippingAddress = new ShippingAddress_V02
                    {
                        Address = new Address_V01 {Country = "CL", StateProvinceTerritory = dnlState.Text}
                    };


                if (null == PickupLocations)
                    PickupLocations = (this.Page as ProductsBase).
                    GetShippingProvider().GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
                if (PickupLocations != null)
                {
                    divLocations.Visible = true;
                    string text = this.dnlCitybox.SelectedItem.Text;

                    var selectedValue = PickupLocations.Select(x => x.Recipient == text).Count();

                    if (selectedValue != null)
                    {
                        DeliveryOption deliveryOption = CourierType.Equals(CityBoxType)
                                                            ? this.PickupLocations.Find(p => p.Alias == text)
                                                            : this.PickupLocations.Find(p => p.Name == text);

                        this.pAddress.InnerHtml = (this.Page as ProductsBase).GetShippingProvider().
                                                                              FormatPickupLocationAddress(
                                                                                  deliveryOption.Address);
                    }
                    else
                        this.pAddress.InnerHtml = string.Empty;
                }

                if (PickupLocations.Count.Equals(0))
                    divLocations.Visible = false;
            }

        }

        #endregion
    }
}