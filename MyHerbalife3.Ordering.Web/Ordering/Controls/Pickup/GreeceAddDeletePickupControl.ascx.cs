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
    public partial class GreeceAddDeletePickupControl : UserControlBase
    {
        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

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
                HideSelected();
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }
            divSubRegion.Visible = this.ProductsBase.CountryCode.Equals("IN") || this.ProductsBase.CountryCode.Equals("BR") || this.ProductsBase.CountryCode.Equals("CN") || this.ProductsBase.CountryCode.Equals("RU") || this.ProductsBase.CountryCode.Equals("PH") || this.ProductsBase.CountryCode.Equals("CO") || this.ProductsBase.CountryCode.Equals("KZ") || this.ProductsBase.CountryCode.Equals("UA"); 
            divNeighbourhood.Visible = this.ProductsBase.CountryCode.Equals("BR") || this.ProductsBase.CountryCode.Equals("CN") || this.ProductsBase.CountryCode.Equals("RU");            
            divCourierInfo.Visible = false;
        }

        private void PopulatePickupInfoDataList()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                ShippingAddress_V02 shippingAddress = null;

                if (divNeighbourhood.Visible)
                {
                    shippingAddress = new ShippingAddress_V02
                    {
                        Address = new Address_V01
                        {
                            Country = this.ProductsBase.CountryCode,
                            StateProvinceTerritory = dnlRegion.Text,
                            City = dnlSubRegion.Text,
                            Line3 = dnlNeighbourhood.Text
                        }
                    };
                }
                else if (divSubRegion.Visible)
                {
                    shippingAddress = new ShippingAddress_V02
                        {
                            Address =
                                new Address_V01
                                    {
                                        Country = ProductsBase.CountryCode,
                                        StateProvinceTerritory = dnlRegion.Text,
                                        City = dnlSubRegion.Text
                                    }
                        };
                }
                else
                {
                    shippingAddress = new ShippingAddress_V02
                        {
                            Address =
                                new Address_V01
                                    {
                                        Country = ProductsBase.CountryCode,
                                        StateProvinceTerritory = dnlRegion.Text
                                    }
                        };
                }

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

                    if (ProductsBase.CountryCode.Equals("IN") || ProductsBase.CountryCode.Equals("KZ"))
                    {
                        dlPickupInfo.DataSource =
                            (
                                from o in pickupOptions
                                where o.State == dnlRegion.Text
                                select new
                                    {
                                        ID = o.Id,
                                        IsPickup = true,
                                        IsWaldos = false,
                                        BranchName = o.Description,
                                        o.FreightCode,
                                        Warehouse = o.WarehouseCode,
                                        o.Address,
                                        Information = string.Format("{0} {1}", o.Id, o.Name),
                                        AditionalInformation=string.Empty
                                    }
                            );
                    }
                    else if (ProductsBase.CountryCode.Equals("RU"))
                    {
                        var datasource =
                            (from o in pickupOptions
                            where o.State == dnlRegion.Text
                                  && o.Address.City == dnlSubRegion.Text
                            select new
                                {
                                    ID = o.Id,
                                    IsPickup = true,
                                    IsWaldos = false,
                                    BranchName = o.Description,
                                    o.FreightCode,
                                    Warehouse = o.WarehouseCode,
                                    o.Address,
                                    Information = string.Empty,
                                    AditionalInformation= o.Information
                                });
                        if(dnlNeighbourhood.SelectedIndex!=0)
                            datasource = (from o in pickupOptions
                                          where o.State == dnlRegion.Text
                                                && o.Address.City == dnlSubRegion.Text
                                                && o.Address.CountyDistrict== dnlNeighbourhood.Text
                                          select new
                                          {
                                              ID = o.Id,
                                              IsPickup = true,
                                              IsWaldos = false,
                                              BranchName = o.Description,
                                              o.FreightCode,
                                              Warehouse = o.WarehouseCode,
                                              o.Address,
                                              Information = string.Empty,
                                              AditionalInformation = o.Information
                                          });
                        dlPickupInfo.DataSource = datasource;
                            
                    }
                    else if (ProductsBase.CountryCode.Equals("PH"))
                    {
                        var datasource =
                            (from o in pickupOptions
                             where o.State == dnlRegion.Text
                                   && o.Address.City == dnlSubRegion.Text
                                 select new
                                {
                                    ID = o.Id,
                                    IsPickup = true,
                                    IsWaldos = false,
                                    BranchName = o.Description,
                                    o.FreightCode,
                                    Warehouse = o.WarehouseCode,
                                    o.Address,
                                    Information = string.Empty,
                                    AditionalInformation= o.Information
                                });
                        dlPickupInfo.DataSource = datasource;
                    }
                    else if (ProductsBase.CountryCode.Equals("UA"))
                    {
                        string branchNameDesc = GetLocalResourceObject("BranchNameDescription") as string;
                        string additionalInfoDesc = GetLocalResourceObject("AdditionalInfoDescription") as string;

                        dlPickupInfo.DataSource =
                            (
                                from o in pickupOptions
                                where o.State == dnlRegion.Text
                                      && o.Address.City == dnlSubRegion.Text
                                orderby o.Id ascending
                                select new
                                {
                                    ID = o.Id,
                                    IsPickup = true,
                                    IsWaldos = false,
                                    BranchName = string.Format("{0}   <font style='font-weight: normal;color: black;'>{1}</font>", o.Id, branchNameDesc),
                                    o.FreightCode,
                                    Warehouse = o.WarehouseCode,
                                    o.Address,
                                    Information = string.Empty,
                                    AditionalInformation = string.Format("<strong>{0}</strong><br>{1}", additionalInfoDesc, o.Information)
                                }
                            );
                    }
                    else
                    {
                        dlPickupInfo.DataSource =
                            (
                                from o in pickupOptions
                                where o.State == dnlRegion.Text
                                select new
                                    {
                                        ID = o.Id,
                                        IsPickup = true,
                                        IsWaldos = false,
                                        BranchName = o.Description,
                                        o.FreightCode,
                                        Warehouse = o.WarehouseCode,
                                        o.Address,
                                        o.Information,
                                        AditionalInformation = string.Empty
                                    }
                            );
                    }

                    dlPickupInfo.DataBind();
                }

                if (pickupOptions.Count.Equals(0))
                    divLocations.Visible = false;
            }
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

        protected void dnlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (divNeighbourhood.Visible)
            {
                dnlNeighbourhood.Items.Clear();
            }
            if (divSubRegion.Visible)
            {
                PopulateSubRegionDropDown();
            }
            else
            {
                PopulatePickupInfoDataList();
                ShowSelected();
            }
        }

        protected void dnlSubRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (divNeighbourhood.Visible)
            {
                PopulateNeighbourhoodDropDown();
            }
            else
            {
                PopulatePickupInfoDataList();
                ShowSelected();
            }
        }

        protected void dnlNeighbourhood_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePickupInfoDataList();
            ShowSelected();
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
            dnlRegion.Items.Clear();
            dnlSubRegion.Items.Clear();
            dnlNeighbourhood.Items.Clear();
            PopulateRegionDropDown();
            dnlRegion.SelectedIndex = -1;
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
        }

        private string getDeleteAddressControlPath(ref bool isXML)
        {
            isXML = true;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress;
        }

        public Object GetAddress(Address_V01 address)
        {
            var stringList = new List<string>();
            if (address != null)
            {
                switch (ProductsBase.CountryCode)
                {
                    case "GR":
                        stringList.Add(address.Line1);
                        if (!string.IsNullOrEmpty(address.Line2))
                        {
                            stringList.Add(address.Line2);
                        }
                        stringList.Add(string.Format("{0},{1}", address.StateProvinceTerritory, address.PostalCode));
                        break;
                    case "PT":
                        stringList.Add(address.Line1);
                        if (!string.IsNullOrEmpty(address.Line2))
                        {
                            stringList.Add(address.Line2);
                        }
                        stringList.Add(string.Format("{0} {1}", address.City, address.PostalCode));
                        break;
                    case "IN":
                        stringList.Add(address.Line1);
                        stringList.Add(address.Line2);
                        if (!string.IsNullOrEmpty(address.Line3)) stringList.Add(address.Line3);
                        if (!string.IsNullOrEmpty(address.Line4)) stringList.Add(address.Line4);
                        stringList.Add(string.Format("{0}, {1}", address.City, address.PostalCode));
                        break;
                    case "BR":
                        stringList.Add(address.Line1);
                        if (!string.IsNullOrEmpty(address.Line2)) stringList.Add(address.Line2);
                        stringList.Add(address.Line3);
                        stringList.Add(string.Format("{0}, {1}", address.City, address.StateProvinceTerritory));
                        stringList.Add(address.PostalCode);
                        break;
                    case "RU":
                        stringList.Add(address.CountyDistrict);
                        stringList.Add(address.Line2);
                        stringList.Add(address.Line1);
                        stringList.Add(address.PostalCode);
                        stringList.Add(address.City);
                        stringList.Add(address.StateProvinceTerritory);
                        break;
                    case "PH":
                        stringList.Add(string.Format("{0} {1}",address.Line2,address.Line1));
                        stringList.Add(string.Format("{0}, {1}", address.City, address.PostalCode));
                        stringList.Add(address.StateProvinceTerritory);
                        break;
                    case "CO":
                        stringList.Add(string.Format("{0} {1}",address.Line2,address.Line1));
                        stringList.Add(string.Format("{0}, {1}", address.City, address.PostalCode));
                        stringList.Add(address.StateProvinceTerritory);
                        break;
                    case "NO":
                        stringList.Add(string.Format("{0} {1}", address.Line2, address.Line1));
                        stringList.Add(string.Format("{0}, {1}", address.CountyDistrict, address.City));
                        stringList.Add(string.Format("{0}, {1}", address.StateProvinceTerritory, address.PostalCode));
                        break;
                    case "SE":
                        stringList.Add(string.Format("{0}, {1}", address.CountyDistrict, address.City));
                        stringList.Add(string.Format("{0}, {1}",address.StateProvinceTerritory, address.PostalCode));
                        break;
                    case "FI":
                        stringList.Add(string.Format("{0}, {1}", address.CountyDistrict, address.City));
                        stringList.Add(string.Format("{0}, {1}", address.StateProvinceTerritory, address.PostalCode));
                        break;
                    case "KZ":
                        stringList.Add(address.Line1);
                        stringList.Add(address.Line2);
                        if (!string.IsNullOrEmpty(address.Line3)) stringList.Add(address.Line3);
                        if (!string.IsNullOrEmpty(address.Line4)) stringList.Add(address.Line4);
                        stringList.Add(string.Format("{0}, {1}", address.City, address.PostalCode));
                        break;
                    case "UA":
                        stringList.Add(string.Format("{0} {1}", address.City, address.PostalCode));
                        stringList.Add(address.Line1);
                        break;

                }
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

        private void PopulateRegionDropDown()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null)
            {
                if (ProductsBase.CountryCode.Equals("IN") )
                {
                    dnlRegion.DataSource = provider.GetStatesForCountry(ProductsBase.Locale);
                }
                else if (ProductsBase.CountryCode.Equals("RU") || ProductsBase.CountryCode.Equals("KZ") || ProductsBase.CountryCode.Equals("UA"))
                {
                    ShippingAddress_V01 shippingAddress = new ShippingAddress_V01();
                    PickupLocations =PickupLocations ?? provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
                    var regions = PickupLocations.Select(x => x.State).Distinct().OrderBy(a=>a);
                    dnlRegion.DataSource = regions;
                    dnlRegion.DataBind();
                    dnlRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlRegion.SelectedIndex = 0;
                }
                else if (ProductsBase.CountryCode.Equals("PH"))
                {
                    dnlRegion.DataSource = provider.GetStatesForCountry(ProductsBase.Locale);
                }
                else
                {
                    dnlRegion.DataSource = provider.GetStatesForCountry(ProductsBase.CountryCode);
                }
                dnlRegion.DataBind();
                dnlRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                dnlRegion.SelectedIndex = 0;
            }
        }

        private void PopulateSubRegionDropDown()
        {
            var provider = ProductsBase.GetShippingProvider();
            if (provider != null && dnlRegion.SelectedIndex != 0)
            {
                if (ProductsBase.CountryCode.Equals("IN"))
                {
                    var shippingAddress = new ShippingAddress_V02
                        {
                            Address =
                                new Address_V01
                                    {
                                        Country = ProductsBase.CountryCode,
                                        StateProvinceTerritory = dnlRegion.Text
                                    }
                        };
                    var pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                    shippingAddress);
                    var cities = (from p in pickupOptions
                                  where p.Address.StateProvinceTerritory.Equals(dnlRegion.Text)
                                  select p.Address.City).Distinct().OrderBy(s => s).ToList();
                    dnlSubRegion.DataSource = cities;
                    dnlSubRegion.DataBind();
                    dnlSubRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlSubRegion.SelectedIndex = 0;
                }
                else if (this.ProductsBase.CountryCode.Equals("BR"))
                {
                    dnlSubRegion.DataSource = provider.GetCitiesForState(this.CountryCode, dnlRegion.Text);
                    dnlSubRegion.DataBind();
                    dnlSubRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlSubRegion.SelectedIndex = 0;
                }
                else if (ProductsBase.CountryCode.Equals("RU") || ProductsBase.CountryCode.Equals("KZ") || ProductsBase.CountryCode.Equals("UA"))
                {
                    ShippingAddress_V01 shippingAddress = new ShippingAddress_V01();
                    PickupLocations =PickupLocations ?? provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
                    var regionSelected = dnlRegion.SelectedItem.Text;
                    var cities = PickupLocations.Where(y => y.State == regionSelected).Select(x => x.Address.City).Distinct().OrderBy(a=>a);
                    dnlSubRegion.DataSource = cities;
                    dnlSubRegion.DataBind();
                    dnlSubRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlSubRegion.SelectedIndex = 0;
                }
                else if (ProductsBase.CountryCode.Equals("PH"))
                {
                    var shippingAddress = new ShippingAddress_V02
                    {
                        Address =
                            new Address_V01
                            {
                                Country = ProductsBase.CountryCode,
                                StateProvinceTerritory = dnlRegion.Text
                            }
                    };
                    var pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                    shippingAddress);
                    var cities = (from p in pickupOptions
                                  where p.Address.StateProvinceTerritory.Equals(dnlRegion.Text)
                                  select p.Address.City).Distinct().OrderBy(s => s).ToList();
                    dnlSubRegion.DataSource = cities;
                    dnlSubRegion.DataBind();
                    dnlSubRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlSubRegion.SelectedIndex = 0;
                }
                else if (ProductsBase.CountryCode.Equals("CO"))
                {
                    var shippingAddress = new ShippingAddress_V02
                    {
                        Address =
                            new Address_V01
                            {
                                Country = ProductsBase.CountryCode,
                                StateProvinceTerritory = dnlRegion.Text
                            }
                    };
                    var pickupOptions = provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                    shippingAddress);
                    var cities = (from p in pickupOptions
                                  where p.Address.StateProvinceTerritory.Equals(dnlRegion.Text)
                                  select p.Address.City).Distinct().OrderBy(s => s).ToList();
                    dnlSubRegion.DataSource = cities;
                    dnlSubRegion.DataBind();
                    dnlSubRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlSubRegion.SelectedIndex = 0;
                }
        }
        }

        private void PopulateNeighbourhoodDropDown()
        {
            if (this.ProductsBase.CountryCode.Equals("BR"))
            {
                var provider = new ShippingProvider_BR();
                if (dnlRegion.SelectedIndex != 0 && dnlSubRegion.SelectedIndex != 0)
                {
                    dnlNeighbourhood.DataSource = provider.GetNeighbourhoodForCity(this.CountryCode, dnlRegion.Text, dnlSubRegion.Text);
                    dnlNeighbourhood.DataBind();
                    dnlNeighbourhood.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlNeighbourhood.SelectedIndex = 0;
                }
            }
            else if (ProductsBase.CountryCode.Equals("RU"))
            {
                var provider = ProductsBase.GetShippingProvider();
                if (provider != null && dnlSubRegion.SelectedIndex != 0)
                {
                    ShippingAddress_V01 shippingAddress = new ShippingAddress_V01();
                    PickupLocations = PickupLocations ??
                                      provider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, shippingAddress);
                    
                    var regionSelected = dnlRegion.SelectedItem.Text;
                    var citySelected = dnlSubRegion.SelectedItem.Text;

                    var metroStations = PickupLocations.Where(y => y.Address.City == citySelected && y.Address.StateProvinceTerritory == regionSelected).Select(x => x.Address.CountyDistrict).Distinct();
                    
                    dnlNeighbourhood.DataSource = metroStations;
                    dnlNeighbourhood.DataBind();
                    dnlNeighbourhood.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, "0"));
                    dnlNeighbourhood.SelectedIndex = 0;
                    //Metrostation (dnlNeightbourhood is not Mandatory)
                    PopulatePickupInfoDataList();
                    ShowSelected();

                }
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

        protected string GetImage()
        {
            if (CountryCode.Equals("UA"))
            {
                return "/Content/uk-UA/img/logo_site.png";
            }
            else
            {
                return "/Content/Global/Products/img/order_icon_HL.png";
            }
        }

        #endregion
    }
}