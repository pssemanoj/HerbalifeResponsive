using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class MexicoOrderQuickView : OrderQuickViewBase
    {
        private const int Honors2016EventId = 2462;

        [Publishes(MyHLEventTypes.ShowAllInventory)]
        public event EventHandler OnShowAllInventory;

        [Publishes(MyHLEventTypes.ShowAvailableInventory)]
        public event EventHandler OnShowAvailableInventory;

        [Publishes(MyHLEventTypes.PickUpNicknameNotSelected)]
        public event EventHandler PickUpNicknameNotSelected;

        [Publishes(MyHLEventTypes.PickUpNicknameNotSelectedInMiniCart)]
        public event EventHandler PickUpNicknameNotSelectedInMiniCart;


        //[Publishes(MyHLEventTypes.QuoteRetrieved)]
        //public event EventHandler MXQuoteRetrieved;

        //[Publishes(MyHLEventTypes.WarehouseChanged)]
        //public event EventHandler MXOnWarehouseChanged;

        #region page load

        protected void Page_Load(object sender, EventArgs e)
        {
            _shippingAddresses =
                Providers.Shipping.ShippingProvider.GetShippingProvider(CountryCode)
                         .GetShippingAddresses(DistributorID, Locale);
            _pickupRrefList = ProductsBase.GetShippingProvider()
                                          .GetPickupLocationsPreferences(DistributorID, CountryCode);
            SetPredefinedPickUpOption();
            
            //User Story 176424 Message to inform "some addresses are updated, please add again your addresses" 
            var showMessage = HttpContext.Current.Session["showMessageOnPage"];
            if (showMessage != null && (string)showMessage == "True" && HLConfigManager.Configurations.DOConfiguration.ShowDeletedAddressesMessage)
            {
                (Master as OrderingMaster).ShowMessage((string)GetLocalResourceObject("Title"),
                                                   (string)GetLocalResourceObject("Message"));
                HttpContext.Current.Session["showMessageOnPage"] = null;
            }
            
            if (IsStatic)
            {
                showStatic();
            }
            else
            {
                showEdit();
            }
            setInventoryView();

            showHideAddressLink();
            showTitle();
            showShiptoOrPickup(IsStatic);
            setLinksVisiblity();
            if (pAddress != null && string.IsNullOrEmpty(pAddress.InnerHtml))
            {
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                {
                    setAddressByNickName(ShoppingCart.DeliveryInfo.Address);
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);


            if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
            {
                Visible = HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket;
            }
            else
            {
                Visible = true;
            }
        }

        #endregion

        #region private methods

        private void SetPredefinedPickUpOption()
        {
            var predefinedValue =
                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PredefinedPickUpLocationName;
            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasPredefinedPickUp
                && !_pickupRrefList.Any(p => p.PickupLocationNickname.Equals(predefinedValue)))
            {
                if (DistributorOrderingProfileProvider.IsEventQualified(Honors2016EventId, Locale))
                {
                    //Get the Id searching by zip code
                   var devileryOptionsForEvent = ProductsBase.GetShippingProvider()
                        .GetDeliveryOptions(DeliveryOptionType.Pickup, new ShippingAddress_V01()
                        {
                            Address = new Address_V01()
                            {
                                PostalCode = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PredefinedPickUpLocationZipCode.ToString()
                            }
                        });

                   var pickUpLocationId = devileryOptionsForEvent.SingleOrDefault(p => p.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse);
                    if (pickUpLocationId != null && pickUpLocationId.Id > 0)
                    {
                        ProductsBase.GetShippingProvider()
                        .SavePickupLocationsPreferences
                        (
                            DistributorID,
                            true,
                            pickUpLocationId.Id,
                            predefinedValue,
                            predefinedValue,
                            "MX",
                            false);
                    reload();
                    }                    
                }
            }
        }

        private void setInventoryView()
        {
            if (!IsPostBack)
            {
                pnlInventoryView.Visible = !HideInventoryOption;
                if (!HideInventoryOption)
                {
                    SessionInfo sessionInfo = ProductsBase.SessionInfo;
                    if (sessionInfo != null)
                    {
                        rbShowAll.Checked = sessionInfo.ShowAllInventory;
                        rbShowOnlyAvail.Checked = !rbShowAll.Checked;
                        if (sessionInfo.IsEventTicketMode)
                        {
                            pnlInventoryView.Visible = false;
                        }
                    }
                    else
                    {
                        rbShowOnlyAvail.Checked = true;
                    }
                }
            }
            // per Olga's request. defect -- 26546
            //if (!HideInventoryOption)
            //    RBListInventoryView.Items[1].Enabled = hasShippingAddresses;
        }

        protected override void showShiptoOrPickup(bool IsStatic)
        {
            DeliveryOptionType selectedOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (IsStatic)
            {
                selectedOption = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Option : selectedOption;
            }

            if (selectedOption == DeliveryOptionType.Shipping)
            {
                lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string) GetLocalResourceObject("ShipTo");
                NickName.Text = NickName.Text = (string) GetLocalResourceObject("NickName");
            }
            else
            {
                lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string) GetLocalResourceObject("PickUpForm");
                NickName.Text = NickName.Text = (string) GetLocalResourceObject("Location");
            }
        }

        private void showTitle()
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
                ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                ShippingPickupOptions.Text = (string) GetLocalResourceObject("TitleShipping");
            }
            else if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup 
                && !HLConfigManager.Configurations.DOConfiguration.AllowShipping)
            {
                ShippingPickupOptions.Text = (string)GetLocalResourceObject("TitlePickup");
            }
            else
            {
                ShippingPickupOptions.Text = (string) GetLocalResourceObject("TitleShippingPickup");
            }
        }

        private void showStatic()
        {
            pnlDeliveryOptionSelection.Visible = false;
            pnlReadonlyDeliveryOptionSelection.Visible = true;
            showShippingMethodReadonly();
            loadAddressReadonly();
        }

        private void showShippingMethodReadonly()
        {
            ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                DeliveryTypeReadonly.Text =
                    (string) GetLocalResourceObject("DeliveryOptionType_" + shippingInfo.Option.ToString());
                if (shippingInfo.Option == DeliveryOptionType.Shipping)
                {
                    if (!String.IsNullOrEmpty(shippingInfo.Address.Alias))
                    {
                        DropdownNickNameReadonly.Text = shippingInfo.Address.Alias;
                    }
                    else
                    {
                        DropdownNickNameReadonly.Text = shippingInfo.Address.Recipient;
                    }
                }
                else if (shippingInfo.Option == DeliveryOptionType.Pickup)
                {
                    DropdownNickNameReadonly.Text = shippingInfo.Description;
                }
            }
        }

        private void showEdit()
        {
            pnlDeliveryOptionSelection.Visible = true;
            pnlReadonlyDeliveryOptionSelection.Visible = false;
            if (!IsPostBack)
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
                    ProductsBase.SessionInfo.IsEventTicketMode)
                {
                    DeliveryType.Items.Remove("Pickup");
                    divDeliveryOptionSelection.Visible = false;
                }
                populateDropdown();
            }
            loadAddressEditMode();
        }

        private void deliveryOptionTypeSelection()
        {
            if (hasShippingAddresses)
            {
                ListItem item = DeliveryType.Items.FindByValue("Shipping");
                if (item != null)
                  item.Selected = true;
                populateShipping();
            }
            else if (hasPreferences)
            {
                ListItem item = DeliveryType.Items.FindByValue("Pickup");
                if (item != null)
                    item.Selected = true;
                populatePickup();
            }
            else
            {
                ListItem item = DeliveryType.Items.FindByValue("Shipping");
                if (item != null)
                    item.Selected = true;
                populateShipping();
            }
        }

        protected override void populateDropdown()
        {
            if (ShoppingCart != null)
            {
                DeliveryType.SelectedIndex = -1;
                if (ShoppingCart.DeliveryInfo != null)
                {
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        DeliveryType.Items[0].Selected = true;
                        populateShipping();
                    }
                    else
                    {
                        DeliveryType.Items[1].Selected = true;
                        populatePickup();
                    }
                }
                else
                {
                    deliveryOptionTypeSelection();
                }
            }
        }

        protected override void showHideAddressLink()
        {
            lnAddAddress.Visible = ProductsBase.GetShippingProvider().NeedEnterAddress(DistributorID, Locale) &&
                                   ShoppingCart.DeliveryInfo == null;
            if ((_shippingAddresses.Count == 0 &&
                 getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping) ||
                (_pickupRrefList.Count == 0 &&
                 getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup))
            {
                lnAddAddress.Visible = true;
            }
            if (lnAddAddress.Visible == false)
            {
                if (IsStatic == false)
                {
                    pnlDeliveryOptionSelection.Visible = !lnAddAddress.Visible;
                    divNicknameInfoAndLink.Visible = true;
                }
            }
            else
            {
                pnlReadonlyDeliveryOptionSelection.Visible = false;
                divNicknameInfoAndLink.Visible = false;
                lnAddAddress.Text = getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping
                                        ? (string) GetLocalResourceObject("AddShippingAddress")
                                        : (string) GetLocalResourceObject("SelectPickup");
            }
        }

        private void loadAddressEditMode()
        {
            int id = 0;
            if (DropdownNickName.SelectedIndex >= 0)
            {
                if (int.TryParse(DropdownNickName.SelectedValue, out id))
                {
                    DeliveryOption deliveryOption = getSelectedAddress(id,
                                                                       (DeliveryOptionType)
                                                                       Enum.Parse(typeof (DeliveryOptionType),
                                                                                  DeliveryType.SelectedValue));
                    if (deliveryOption != null)
                    {
                        pAddress.InnerHtml = ProductsBase.GetShippingProvider()
                                                         .FormatShippingAddress(deliveryOption,
                                                                                getDeliveryOptionTypeFromDropdown(
                                                                                    DeliveryType),
                                                                                ShoppingCart != null &&
                                                                                ShoppingCart.DeliveryInfo != null
                                                                                    ? ShoppingCart.DeliveryInfo
                                                                                                  .Description
                                                                                    : string.Empty, true);
                    }
                }
            }
        }

        private void loadAddressReadonly()
        {
            var addressBase = new AddressControl();
            string xmlFile;
            if (!string.IsNullOrEmpty(xmlFile = HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress))
            {
                addressBase.XMLFile = xmlFile;
                if (ShoppingCart.DeliveryInfo != null)
                {
                    addressBase.DataContext = ShoppingCart.DeliveryInfo.Address;
                }
                divAddreaddReadOnly.Controls.Add(addressBase);
            }
        }

        protected override void populateShipping()
        {
            populateShipping(DropdownNickName);
        }

        protected override void populatePickup()
        {
            DropdownNickName.Items.Clear();
            if (_pickupRrefList != null && _pickupRrefList.Count > 0)
            {
                DropdownNickName.DataSource = from p in _pickupRrefList
                                              select new
                                                  {
                                                      DisplayName = p.PickupLocationNickname,
                                                      p.ID
                                                  };
                DropdownNickName.DataBind();
            }
        }

        protected override void reload()
        {
            base.reload();
            setInventoryView();
        }

        private void disableDeleteLink(bool disable)
        {
            LinkDelete.Enabled = !disable;
        }

        protected override void setLinksVisiblity()
        {
            LinkEdit.Visible = (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping);
            if (CountryCode == "AR" && getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup)
            {
                LinkAdd.Text = (string) GetLocalResourceObject("LinkAddPickupResource1.Text");
            }
            else
            {
                LinkAdd.Text = (string)GetLocalResourceObject("LinkAddResource1.Text");
            }
        }

        private DeliveryOption getSelectedAddress(int id, DeliveryOptionType optionType)
        {
            if (optionType == DeliveryOptionType.Shipping)
            {
                var addresses = _shippingAddresses.Where(s => s.Id == id);
                if (addresses.Count() == 0)
                {
                    addresses = _shippingAddresses.Where(s => s.IsPrimary);
                }
                return addresses.Count() == 0 ? null : addresses.First();
            }
            else
            {
                // pickup
                if (_pickupRrefList != null && _pickupRrefList.Count > 0)
                {
                    var varPickupLocation = _pickupRrefList.Where(p => p.ID == id);
                    if (varPickupLocation.Count() > 0)
                    {
                        int pickupLocationID = varPickupLocation.First().PickupLocationID;
                        ShippingInfo shippingInfo =
                            Providers.Shipping.ShippingProvider.GetShippingProvider(CountryCode)
                                     .GetShippingInfoFromID(DistributorID,
                                                            Locale, optionType, pickupLocationID, 0);
                        if (null != shippingInfo)
                        {
                            var deliveryOption = new DeliveryOption(shippingInfo.WarehouseCode, shippingInfo.FreightCode,
                                                                    DeliveryOptionType.Pickup);
                            if (shippingInfo.Address != null)
                                deliveryOption.Address = shippingInfo.Address.Address;
                            deliveryOption.ID = shippingInfo.Id;
                            return deliveryOption;
                        }
                    }
                }
            }
            return null;
        }

        protected override void updateShippingInfo(int shippingAddressId, int deliveryOptionId,
                                                   DeliveryOptionType option)
        {
            if (ShoppingCart != null)
            {
                if (option == DeliveryOptionType.Pickup)
                {
                    var pref = _pickupRrefList.Find(f => f.ID == deliveryOptionId);
                    if (pref != null)
                    {
                        deliveryOptionId = pref.PickupLocationID;
                    }
                }
                base.updateShippingInfo(shippingAddressId, deliveryOptionId, option);

                setInventoryView();
            }
        }

        #endregion

        protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            handleDeliveryTypeChanged(DeliveryType, DropdownNickName);
        }

        //protected override bool hasNoPreference()
        //{
        //    IShippingProvider ShippingProvider = ProductsBase.GetShippingProvider();
        //    List<HL.Shipping.ValueObjects.PickupLocationPreference_V01> pickupRrefList =
        //        ShippingProvider.GetPickupLocationsPreferences(DistributorID, CountryCode);
        //    List<DeliveryOption> shippingAddresses =
        //        ShippingProvider.GetShippingAddresses(DistributorID, Locale);
        //    return (pickupRrefList == null || pickupRrefList.Count == 0) &&
        //            (shippingAddresses == null || shippingAddresses.Count == 0);
        //}

        private bool CheckDSCantBuyStatus()
        {
            if ((Page as ProductsBase).CantBuy || (Page as ProductsBase).Deleted)
            {
                errDRFraud.Visible = true;
                errDRFraud.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder");
                return false;
            }
            return true;
        }

        protected void AddAddressClicked(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
            }
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
        }

        [SubscribesTo(MyHLEventTypes.DeliveryOptionTypeChanged)]
        protected void OnDeliveryOptionTypeSelected(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionTypeEventArgs;
            if (args != null)
            {
                if (args.DeliveryOption != ServiceProvider.ShippingSvc.DeliveryOptionType.Unknown)
                {
                    DeliveryType.SelectedIndex = args.DeliveryOption == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping ? 0 : 1;
                    AddClicked(sender, e);
                }
            }
        }

        protected void AddClicked(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
            }
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
        }

        protected void EditClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                DeliveryOption deliveryOption = getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                   (DeliveryOptionType)
                                                                   Enum.Parse(typeof (DeliveryOptionType),
                                                                              DeliveryType.SelectedValue));
                if (deliveryOption != null)
                {
                    Session["OQVOldaddress"] = deliveryOption;
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Edit,
                                                               new ShippingAddressEventArgs(DistributorID,
                                                                                            deliveryOption, false,
                                                                                            ProductsBase
                                                                                                .DisableSaveAddressCheckbox));
                }
            }
        }

        protected void DeleteClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                DeliveryOption deliveryOption = getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                   (DeliveryOptionType)
                                                                   Enum.Parse(typeof (DeliveryOptionType),
                                                                              DeliveryType.SelectedValue));
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    if (deliveryOption != null)
                    {
                        ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete,
                                                                   new ShippingAddressEventArgs(DistributorID,
                                                                                                deliveryOption, false,
                                                                                                ProductsBase
                                                                                                    .DisableSaveAddressCheckbox));
                    }
                }
                else
                {
                    ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
                    if (shippingInfo != null)
                    {
                        List<PickupLocationPreference_V01> pickupLocationPreferences =
                            (Page as ProductsBase).GetShippingProvider()
                                                  .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                                 (Page as ProductsBase).CountryCode);
                        if (int.Parse(DropdownNickName.SelectedValue) != 0)
                        {
                            PickupLocationPreference_V01 pref =
                                pickupLocationPreferences.Find(p => p.ID == int.Parse(DropdownNickName.SelectedValue));
                            if (pref != null)
                            {
                                ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete,
                                                                         new DeliveryOptionEventArgs(
                                                                             pref.PickupLocationID, shippingInfo.Name));
                            }
                        }
                    }
                }
            }
        }

        protected override void setAddressByNickName(ShippingAddress_V01 address)
        {
            pAddress.Visible = address != null;
            setAddressByNickName(address, pAddress, DeliveryType);
        }

        protected void OnNickNameChanged(object sender, EventArgs e)
        {
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.ErrorNoDeliveryOption && ShoppingCart.DeliveryInfo == null)
            {
                string message = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDeliveryOption");
                (ProductsBase.Master as OrderingMaster).Status.AddMessage(message);
            }
        }

        protected void OnNickName_Databind(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
                ShippingInfo deliveryInfo = ShoppingCart.DeliveryInfo;
                var textSelect = (string) GetLocalResourceObject("TextSelect");
                if (deliveryInfo != null)
                {
                    DeliveryOptionType deliveryOptionTypeFromDowndown = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
                        {
                            if (null != deliveryInfo.Address)
                            {
                                DeliveryOption deliveryOption = getSelectedAddress(deliveryInfo.Address.ID,
                                                                                   DeliveryOptionType.Shipping);
                                ListItem selected =
                                    ddl.Items.FindByValue(deliveryOption == null
                                                              ? string.Empty
                                                              : deliveryInfo.Address.ID.ToString());
                                if (selected != null)
                                {
                                    selected.Selected = true;
                                    //disableDeleteLink(deliveryOption.IsPrimary);
                                }
                            }
                            else
                            {
                                LoggerHelper.Error(
                                    string.Format(
                                        "MX DeliveryInfo has null Address for Shipping addresss Id {0}, FreightCode {1}",
                                        deliveryInfo.Id, deliveryInfo.FreightCode));
                            }
                        }
                        else
                        {
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                        }
                    }
                    else
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Pickup)
                        {
                            var varPref = _pickupRrefList.Where(f => f.PickupLocationID == deliveryInfo.Id);
                            if (varPref.Count() > 0)
                            {
                                ListItem selected = ddl.Items.FindByValue(varPref.First().ID.ToString());
                                if (selected != null)
                                {
                                    selected.Selected = true;
                                    //disableDeleteLink(varPref.First().IsPrimary);
                                }
                            }
                        }
                        else
                        {
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                        }
                    }
                }
                else
                {
                    ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                    ddl.Items[0].Selected = true;
                }
            }
        }

        protected void RBListInventoryView_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SessionInfo sessionInfo = SessionInfo;
            if (sessionInfo != null)
            {
                //sessionInfo.ShowAllInventory = (sender as RadioButtonList).SelectedIndex == 0;
                DistributorOrderingProfile.ShowAllInventory = sessionInfo.ShowAllInventory = (sender as RadioButton) == rbShowAll;
                SessionInfo.SetSessionInfo(DistributorID, Locale, sessionInfo);
                if (sessionInfo.ShowAllInventory)
                {
                    OnShowAllInventory(this, null);
                }
                else
                {
                    OnShowAvailableInventory(this, null);
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public void OnPickupPreferenceCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId, DeliveryOptionType.Pickup);
            }
            reload();
            // ucShippingInfoControl.Hide();
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceDeleted)]
        public void OnPickupPreferenceDeleted(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                _pickupRrefList =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode);
                if (_pickupRrefList.Count > 0)
                {
                    PickupLocationPreference_V01 pref = _pickupRrefList.First();
                    updateShippingInfo(ProductsBase.ShippingAddresssID, pref.PickupLocationID, DeliveryOptionType.Pickup);
                }
                else
                {
                    ProductsBase.ClearCart();
                }
                populateDropdown();
            }
            //   ucShippingInfoControl.Hide();
            reload();
            if (_pickupRrefList.Count == 0)
            {
                setAddressByNickName(null);
                ProductsBase.ClearCart();
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressChanged)]
        public void OnShippingAddressChanged(object sender, EventArgs e)
        {
            var args = e as ShippingAddressEventArgs;
            if (args != null)
            {
                shippingAddressChanged(args.ShippingAddress);
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressCreated)]
        public void OnShippingAddressCreated(object sender, EventArgs e)
        {
            var args = e as ShippingAddressEventArgs;
            if (args != null)
            {
                shippingAddressCreated(args.ShippingAddress);
                reload();
                lnAddAddress.Visible = false;
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressDeleted)]
        public void OnShippingAddressDeleted(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                var args = e as ShippingAddressEventArgs;
                if (args != null)
                {
                    shippingAddressDeleted(args.ShippingAddress);
                }
            }
            int id = 0;
            if (DropdownNickName.SelectedIndex >= 0)
            {
                if (int.TryParse(DropdownNickName.SelectedValue, out id))
                {
                    DeliveryOption deliveryOption = getSelectedAddress(id,
                                                                       (DeliveryOptionType)
                                                                       Enum.Parse(typeof (DeliveryOptionType),
                                                                                  DeliveryType.SelectedValue));
                    if (deliveryOption != null)
                    {
                        updateShippingInfo(deliveryOption.ID, ProductsBase.DeliveryOptionID, DeliveryOptionType.Shipping);
                        pAddress.InnerHtml = ProductsBase.GetShippingProvider()
                                                         .FormatShippingAddress(deliveryOption,
                                                                                getDeliveryOptionTypeFromDropdown(
                                                                                    DeliveryType),
                                                                                ShoppingCart != null &&
                                                                                ShoppingCart.DeliveryInfo != null
                                                                                    ? ShoppingCart.DeliveryInfo
                                                                                                  .Description
                                                                                    : string.Empty, true);
                    }
                }
            }
            //ucShippingInfoControl.Hide();
        }

        [SubscribesTo(MyHLEventTypes.ShippingInfoNotFilled)]
        public void OnShippingInfoNotFilled(object sender, EventArgs e)
        {
            if (ProductsBase.NeedEnterAddress())
            {
                AddAddressClicked(lnAddAddress, e);
            }
            else
            {
                DeliveryOptionType currentOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
                string message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                           "ShippingNickNameNotPopulated");
                if (currentOption == DeliveryOptionType.Pickup)
                {
                    message = PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated");
                }

                (ProductsBase.Master as OrderingMaster).Status.AddMessage(message);
            }
        }

        [SubscribesTo(MyHLEventTypes.ProceedingToCheckout)]
        public void OnProceedingToCheckout(object sender, EventArgs e)
        {
            if (DropdownNickName != null)
            {
                if (DropdownNickName.Items.Count > 0)
                {
                    if (DropdownNickName.SelectedItem.Value == "0")
                    {
                        var args = new DeliveryOptionTypeEventArgs(getDeliveryOptionTypeFromDropdown(DeliveryType));
                        PickUpNicknameNotSelected(this, args);
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ProceedingToCheckoutFromMiniCart)]
        public void ProceedingToCheckoutFromMiniCart(object sender, EventArgs e)
        {
            bool isPickup = DeliveryType.SelectedItem.Value.ToLower() == "pickup";
            if (isPickup)
            {
                if (DropdownNickName != null)
                {
                    if (DropdownNickName.Items.Count > 0)
                    {
                        if (DropdownNickName.SelectedItem.Value == "0")
                        {
                            PickUpNicknameNotSelectedInMiniCart(this, null);
                        }
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.OnSaveCart)]
        public void OnSaveCart(object sender, EventArgs e)
        {
            CanSaveCart(ref DeliveryType, ref DropdownNickName);
        }
    }
}