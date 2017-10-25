using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using HL.Common.Configuration;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class OrderQuickView : OrderQuickViewBase
    {
        [Publishes(MyHLEventTypes.ShowAllInventory)]
        public event EventHandler OnShowAllInventory;

        [Publishes(MyHLEventTypes.ShowAvailableInventory)]
        public event EventHandler OnShowAvailableInventory;

        [Publishes(MyHLEventTypes.PickUpNicknameNotSelected)]
        public event EventHandler PickUpNicknameNotSelected;

        [Publishes(MyHLEventTypes.PickUpNicknameNotSelectedInMiniCart)]
        public event EventHandler PickUpNicknameNotSelectedInMiniCart;

        protected virtual void loadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);

            // If pick up is available.
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup)
            {
                _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                       ShippingProvider.GetDefaultAddress());
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                                                                       ShippingProvider.GetDefaultAddress());
            }

            // Hide shipping method option is this flag is true.
            if (!HLConfigManager.Configurations.DOConfiguration.AllowShipping &&
                DeliveryType.Items.FindByValue("Shipping") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Shipping"));
            }

            //user story 201090 HAP ordering
            //HAP Price List: Shipping Option only for HAP Order
            if (SessionInfo.IsHAPMode && HLConfigManager.Configurations.DOConfiguration.AllowHAP && DeliveryType.Items.FindByValue("Shipping") != null)
            {
                _pickupLocations = null;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // bug 157300 reported, at time get error, ShoppingCart is null at this stage, but refereshing the page the issue gone
            // this happened for China GDO in ETO mode. And since the same method is being executed in PageLoad, so let the PageLoad stage handle this function.
            if (!IsChina)
            {
                if (APFDueProvider.ShouldHideOrderQuickView(ShoppingCart))
                {
                    Visible = false;
                    return;
                }
            }

            if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
            {
                Visible = HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket;
            }
            else
            {
                Visible = true;
            }
            //divLinkAdd = new System.Web.UI.HtmlControls.HtmlGenericControl();
            //divLinkEdit = new System.Web.UI.HtmlControls.HtmlGenericControl();
            //divLinkDelete = new System.Web.UI.HtmlControls.HtmlGenericControl();

        }

        protected void SetInventoryViewForPickUp()
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HideWireForSpecialWhLocations
                    && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialWhlocations.Contains(ShoppingCart.DeliveryInfo.WarehouseCode))
            {
                rbShowOnlyAvail.Checked = true;
                rbShowAll.Checked = false;
                rbShowAll.Visible = false;
                lblShowAll.Visible = false;
            }
            else
            {
                //Default Behaviour
                //Don't make any changes to rbShowAll/rbShowOnlyAvail checked value, as it will be handled nicely at the setInventoryView function.
                rbShowAll.Visible = true;
                rbShowOnlyAvail.Enabled = true;
                lblShowAll.Visible = true;
            }

            RBListInventoryView_OnSelectedIndexChanged(rbShowOnlyAvail, null);
        }

        protected void setInventoryView()
        {
            var sessionInfo = ProductsBase.SessionInfo;
            if (!IsPostBack)
            {
                pnlInventoryView.Visible = !HideInventoryOption;
                if (!HideInventoryOption)
                {
                    if (sessionInfo != null)
                    {
                        if (sessionInfo.IsEventTicketMode)
                            pnlInventoryView.Visible = false;

                        DistributorOrderingProfile.ShowAllInventory = sessionInfo.ShowAllInventory; //Resume the previous selected option.
                        rbShowAll.Checked = DistributorOrderingProfile.ShowAllInventory;
                        rbShowOnlyAvail.Checked = !rbShowAll.Checked;

                        SetInventoryViewForPickUp();
                    }
                }
            }
        }

        protected void adjustInventoryOption()
        {
            if (!HideInventoryOption)
            {
                rbShowOnlyAvail.Enabled = hasShippingAddresses || ShoppingCart.DeliveryInfo != null;
                if (rbShowOnlyAvail.Enabled)
                    rbShowOnlyAvail.Enabled = (null != DropdownNickName.SelectedItem &&
                                               DropdownNickName.SelectedItem.Value != "0");
                if (rbShowOnlyAvail.Enabled == false)
                {
                    bool wasShowAllInventory = ProductsBase.SessionInfo.ShowAllInventory;
                    rbShowOnlyAvail.Checked = false;
                    rbShowAll.Checked = true;
                    ProductsBase.SessionInfo.ShowAllInventory = true;
                    if (!wasShowAllInventory)
                        OnShowAllInventory(this, null);
                }
            }
        }

        protected override void showShiptoOrPickup(bool isStatic)
        {
            var selectedOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (IsStatic)
            {
                if (ShoppingCart != null)
                {
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        selectedOption = ShoppingCart.DeliveryInfo.Option;
                    }
                }
            }

            if (selectedOption == DeliveryOptionType.Shipping)
            {
                lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = NickName.Text = (string)GetLocalResourceObject("NickName");
            }
            else
            {
                if (CheckNoDeliveryType(DeliveryType))
                {
                    lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                    NickNameReadonly.Text = NickName.Text = (string)GetLocalResourceObject("NickName");
                }
                else
                {
                    lbShiptoOrPickupReadonly.Text =
                        lbShiptoOrPickup.Text = (string)GetLocalResourceObject("PickUpForm");
                    NickNameReadonly.Text = NickName.Text = (string)GetLocalResourceObject("Location");
                }
            }
        }

        protected void showTitle()
        {
            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                ShippingPickupOptions.Visible = false;
            }
            else if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
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
                ShippingPickupOptions.Text = (string)GetLocalResourceObject("TitleShippingPickup");
            }

            if (IsChina && SessionInfo.IsEventTicketMode)
            {
                ShippingPickupOptions.Text = (string)GetLocalResourceObject("ListItemResource2.Text");
            }
        }

        protected void showStatic()
        {
            pnlDeliveryOptionSelection.Visible = false;
            pnlReadonlyDeliveryOptionSelection.Visible = true;
            showShippingMethodReadonly();
            loadAddressReadonly();
        }

        protected void showShippingMethodReadonly()
        {
            var shippingInfo = ShoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                DeliveryTypeReadonly.Text =
                    (string)GetLocalResourceObject("DeliveryOptionType_" + shippingInfo.Option.ToString());
                if (shippingInfo.Option == DeliveryOptionType.Shipping)
                {
                    DropdownNickNameReadonly.Text = shippingInfo.Address.Alias;
                }
                else if (shippingInfo.Option == DeliveryOptionType.Pickup)
                {
                    DropdownNickNameReadonly.Text = shippingInfo.Description;
                }
            }
        }

        protected void showEdit()
        {
            pnlDeliveryOptionSelection.Visible = true;
            pnlReadonlyDeliveryOptionSelection.Visible = false;
            if (!IsPostBack)
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
                    (ProductsBase.SessionInfo.IsEventTicketMode &&
                     !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupAllowForEventTicket)
                     || ProductsBase.SessionInfo.IsHAPMode)
                {
                    DeliveryType.Items.Remove("Pickup");
                    divDeliveryOptionSelection.Visible = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryOptionHaveDropDown;
                }
                populateDropdown();
            }
            loadAddressEditMode();
          
        }

        protected override void setLinksVisiblity()
        {
            if (CheckNoDeliveryType(DeliveryType))
            {
                divLinks.Visible = true;
            }
            else
            {
                divLinks.Visible = (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping);
            }
       
        }

        protected override void populateDropdown()
        {
            lblNickName.Visible = false;
            if (ShoppingCart != null)
            {
                DeliveryType.SelectedIndex = -1;
                DropdownNickName.ClearSelection();
                populateDropdown(DeliveryType);
            }
        }

        protected override void showHideAddressLink()
        {
            lnAddAddress.Visible = ProductsBase.GetShippingProvider().NeedEnterAddress(DistributorID, Locale) &&
                                   ShoppingCart.DeliveryInfo == null &&
                                   getDeliveryOptionTypeFromDropdown(DeliveryType) != DeliveryOptionType.Pickup;
            if (_shippingAddresses.Count == 0 &&
                getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                lnAddAddress.Visible = true;
            if (lnAddAddress.Visible == false)
            {
                if (IsStatic == false)
                {
                    pnlDeliveryOptionSelection.Visible = !lnAddAddress.Visible;
                    divNicknameInfoAndLink.Visible = !lnAddAddress.Visible;
                    if (CheckNoDeliveryType(DeliveryType))
                    {
                        divLinks.Visible = true;
                    }
                    else
                    {
                        divLinks.Visible = (getDeliveryOptionTypeFromDropdown(DeliveryType) ==
                                            DeliveryOptionType.Shipping);
                    }
                }
                else
                {
                    divLinks.Visible = false;
                }
            }
            else
            {
                pnlReadonlyDeliveryOptionSelection.Visible = false;
                divNicknameInfoAndLink.Visible = false;
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown ||
                    getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    lnAddAddress.Text = (string)GetLocalResourceObject("AddShippingAddress");
                }
                else
                {
                    lnAddAddress.Text = String.Empty;
                }
                divLinks.Visible = false;
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown &&
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup
                    && ProductsBase.SessionInfo.IsEventTicketMode == false 
                    && !ProductsBase.SessionInfo.IsHAPMode)
                {
                    lnAddAddress.Visible = false;
                }
                else
                {
                    lnAddAddress.Visible = true;
                }
            }

            if (_shippingAddresses.Count == 1 && (Page as ProductsBase).CantDeleteFinalAddress)
            {
                disableDeleteLink(true);
            }
            else if (_shippingAddresses.Count > 1)
            {
                disableDeleteLink(false);
            }

            if (_shippingAddresses.Count == 1 && ShoppingCart.CustomerOrderDetail != null)
            {
                disableDeleteLink(true);
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
            {
                var restrictedAddress = _shippingAddresses.Where(x => x.HasAddressRestriction ?? false);
                _shippingAddresses = restrictedAddress.ToList();
                divLinkEdit.Visible = divLinkDelete.Visible = false;
                divLinkAdd.Visible =  !(_shippingAddresses.Count >= HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestrictionLimit);
            }
          
        }

        protected virtual void loadAddressEditMode()
        {
            int id = 0;
            bool isVenezula = DropdownNickName.SelectedValue == "0" && ShoppingCart != null && ShoppingCart.Locale == "es-VE" ? false : true;
            if (DropdownNickName.SelectedIndex >= 0 && isVenezula)
            {
                if (int.TryParse(DropdownNickName.SelectedValue, out id))
                {
                    var deliveryOption = getSelectedAddress(id,
                                                            (DeliveryOptionType)
                                                            Enum.Parse(typeof(DeliveryOptionType),
                                                                       DeliveryType.SelectedValue));
                    shouldShowMiriMessage(deliveryOption, (DeliveryOptionType)
                                                            Enum.Parse(typeof(DeliveryOptionType),
                                                                       DeliveryType.SelectedValue));
                    if (deliveryOption != null)
                    {
                        var option = getDeliveryOptionTypeFromDropdown(DeliveryType);
                        if (option == DeliveryOptionType.Unknown && CheckNoDeliveryType(DeliveryType))
                        {
                            option = DeliveryOptionType.Shipping;
                        }

                        pAddress.InnerHtml = ProductsBase.GetShippingProvider()
                                                         .FormatShippingAddress(deliveryOption, option,
                                                                                ShoppingCart != null &&
                                                                                ShoppingCart.DeliveryInfo != null
                                                                                    ? ShoppingCart.DeliveryInfo
                                                                                                  .Description
                                                                                    : string.Empty, true);
                    }
                }
            }
        }

        protected void loadAddressReadonly()
        {
            if (ShoppingCart != null)
            {
                var deliveryInfo = ShoppingCart.DeliveryInfo;
                if (deliveryInfo != null)
                {
                    pAddreaddReadOnly.InnerHtml =
                        ProductsBase.GetShippingProvider()
                                    .FormatShippingAddress(deliveryInfo.Address, deliveryInfo.Option,
                                                           deliveryInfo.Description, true);
                }
            }
        }

        protected void shouldShowMiriMessage(DeliveryOption Deliveryoption,DeliveryOptionType deliverytype)
        {
            if (Deliveryoption != null)
            {
                if (deliverytype == DeliveryOptionType.Pickup)
                {
                    var ShippingID = Settings.GetRequiredAppSetting("ShippingIDForMIriMessage").Split(',');
                    if (ShippingID.Contains(Deliveryoption.Id.ToString()) && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupHaveMessage)
                    {
                        lblMiriPickUpMessaage.Visible = true;
                    }
                    else
                    {
                        lblMiriPickUpMessaage.Visible = false;
                    }
                }
                else
                {
                    lblMiriPickUpMessaage.Visible = false;
                }
            }
            else
            {
                lblMiriPickUpMessaage.Visible = false;
            }

        }
        protected override void populateShipping()
        {
            populateShipping(DropdownNickName);
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
        }

        protected override void populatePickup()
        {
            if (_pickupLocations != null)
            {
                populatePickup(DropdownNickName, lblNickName);
            }
        }

        protected override void reload()
        {
            base.reload();
            //adjustInventoryOption();
        }

        protected void disableDeleteLink(bool disable)
        {
            LinkDelete.Enabled = !disable;
        }

        protected DeliveryOption getSelectedAddress(int id, DeliveryOptionType optionType)
        {
            return getSelectedAddress(id, optionType, DeliveryType);
        }

        /// <summary>
        ///     update Shipping info when changed
        /// </summary>
        /// <param name="shippingAddressId"></param>
        /// <param name="deliveryOptionId"></param>
        /// <param name="option"></param>
        protected override void updateShippingInfo(int shippingAddressId,
                                                   int deliveryOptionId,
                                                   DeliveryOptionType option)
        {
            base.updateShippingInfo(shippingAddressId, deliveryOptionId, option);
            //adjustInventoryOption();
        }

        protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                Session["messagesToShow"] = null;
                Session["showedMessages"] = null;
            }
            lblNickName.Visible = false;
            handleDeliveryTypeChanged(DeliveryType, DropdownNickName);
            setLinksVisiblity();
            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);

            if (null != DropdownNickName.SelectedItem)
            {
                if (DropdownNickName.SelectedItem.Value == "0")
                {
                    lbShiptoOrPickup.Visible = false;
                }
                else
                {
                    lbShiptoOrPickup.Visible = true;
                }
                //this.adjustInventoryOption();
                SetInventoryViewForPickUp();
            }
        }

        protected bool CheckDSCantBuyStatus()
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
            ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                       new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                    ProductsBase
                                                                                        .DisableSaveAddressCheckbox));
        }

        protected void AddClicked(object sender, EventArgs e)
        {
            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryOptionType != DeliveryOptionType.PickupFromCourier)
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
                var deliveryOption = getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                        (DeliveryOptionType)
                                                        Enum.Parse(typeof(DeliveryOptionType),
                                                                   DeliveryType.SelectedValue));
                if (deliveryOption != null)
                {
                    Session["OQVOldaddress"] = new DeliveryOption(deliveryOption);
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
                var deliveryOption = getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                        (DeliveryOptionType)
                                                        Enum.Parse(typeof(DeliveryOptionType),
                                                                   DeliveryType.SelectedValue));
                if (deliveryOption != null)
                {
                    var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    if (deliveryOptionType != DeliveryOptionType.PickupFromCourier)
                    {
                        ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete,
                                                                   new ShippingAddressEventArgs(DistributorID,
                                                                                                deliveryOption, false,
                                                                                                ProductsBase
                                                                                                    .DisableSaveAddressCheckbox));
                    }
                    else
                    {
                        var shippingInfo = ShoppingCart.DeliveryInfo;
                        if (shippingInfo != null)
                        {
                            var mpAddAddress =
                                (ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                            var pickupLocationPreferences =
                                (Page as ProductsBase).GetShippingProvider()
                                                      .GetPickupLocationsPreferences(
                                                          (Page as ProductsBase).DistributorID,
                                                          (Page as ProductsBase).CountryCode);
                            if (int.Parse(DropdownNickName.SelectedValue) != 0)
                            {
                                var pref =
                                    pickupLocationPreferences.Find(
                                        p => p.ID == int.Parse(DropdownNickName.SelectedValue));
                                if (pref != null)
                                {
                                    ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete,
                                                                             new DeliveryOptionEventArgs(
                                                                                 pref.PickupLocationID,
                                                                                 shippingInfo.Name));
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void setAddressByNickName(ShippingAddress_V01 address)
        {
            if (setAddressByNickName(address, pAddress, DeliveryType))
            {
                divLinks.Visible = true;
                showShiptoOrPickup(false);
            }
        }

        protected void OnNickNameChanged(object sender, EventArgs e)
        {
            bool isVenezula = DropdownNickName.SelectedValue == "0" && ShoppingCart != null && ShoppingCart.Locale == "es-VE" ? false : true;
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0" && isVenezula)
            {
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);
                SetInventoryViewForPickUp();
                lbShiptoOrPickup.Visible = true;
                if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
                {
                    ExpireDatePopUp1.ShowPopUp();
                }
                SessionInfo.IsVenuzulaShipping = false;
            }
            else
            {
                pAddress.InnerHtml = "";
                SessionInfo.IsVenuzulaShipping = true;
            }
        }

        protected override void setPickupInfo(DeliveryOption deliveryOption)
        {
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                var shippingInfo = setPickupInfo(deliveryOption, deliveryOptionType);

                if (pAddress != null && string.IsNullOrEmpty(pAddress.InnerHtml))
                    setAddressByNickName(shippingInfo == null ? null : shippingInfo.Address);
            }
        }

        protected void OnNickName_Databind(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
                var sessionInf = SessionInfo.GetSessionInfo(ShoppingCart.DistributorID, ShoppingCart.Locale);
                var deliveryInfo = ShoppingCart.DeliveryInfo;
                var textSelect = (string)GetLocalResourceObject("TextSelect");
                if (deliveryInfo != null)
                {
                    var deliveryOptionTypeFromDowndown = getDeliveryOptionTypeFromDropdown(DeliveryType);

                    if (CheckNoDeliveryType(DeliveryType) &&
                        deliveryOptionTypeFromDowndown == DeliveryOptionType.Unknown)
                    {
                        deliveryOptionTypeFromDowndown = DeliveryOptionType.Shipping;
                    }

                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
                        {
                            var deliveryOption = getSelectedAddress(deliveryInfo.Address.ID,
                                                                    DeliveryOptionType.Shipping);
                            var selected =
                                ddl.Items.FindByValue(deliveryOption == null
                                                          ? string.Empty
                                                          : deliveryInfo.Address.ID.ToString());
                            if (selected != null)
                            {
                                if (ShoppingCart.Locale == "es-VE")
                                {
                                    ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                                    if (!SessionInfo.IsVenuzulaShippingNew)
                                    {
                                        ddl.ClearSelection();
                                        ddl.Items[0].Selected = true;
                                    }
                                    else
                                    {
                                        selected.Selected = true;
                                    }
                                    sessionInf.IsVenuzulaShipping = true;
                                }
                                else
                                {
                                    selected.Selected = true;
                                    sessionInf.IsVenuzulaShipping = false;
                                }
                                //disableDeleteLink(deliveryOption.IsPrimary);
                            }
                            else
                            {
                                if (CheckNoDeliveryType(DeliveryType))
                                {
                                    if (null != ddl.Items && ddl.Items.Count > 0 && ddl.Items.Count == 1)
                                    {
                                        ddl.Items[0].Selected = true;
                                    }
                                }
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
                            var varPref = _pickupLocations.Where(f => f.Id == deliveryInfo.Id);
                            if (varPref.Count() > 0)
                            {
                                var selected = ddl.Items.FindByValue(deliveryInfo.Id.ToString());
                                if (selected != null)
                                {
                                    selected.Selected = true;
                                    disableDeleteLink(varPref.First().IsPrimary);
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
                    var option = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    if (option == DeliveryOptionType.Shipping)
                    {
                        bool isShowAddress = true;
                        int id = 0;
                        if (int.TryParse(DropdownNickName.SelectedValue, out id))
                        {
                            var deliveryOption = getSelectedAddress(id,
                                                                    (DeliveryOptionType)
                                                                    Enum.Parse(typeof(DeliveryOptionType),
                                                                               DeliveryType.SelectedValue));
                            if (deliveryOption != null)
                            {
                                var selected = ddl.Items.FindByValue(deliveryOption.Id.ToString());
                                if (selected != null)
                                {
                                    if (ShoppingCart.Locale == "es-VE")
                                    {
                                        ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                                        if (!SessionInfo.IsVenuzulaShippingNew)
                                        {
                                            ddl.ClearSelection();
                                            ddl.Items[0].Selected = true;
                                            isShowAddress = false;
                                        }
                                        else
                                        {
                                            selected.Selected = true;
                                        }
                                        sessionInf.IsVenuzulaShipping = true;
                                    }
                                    else
                                    {
                                        selected.Selected = true;
                                        updateShippingInfo(deliveryOption.ID, ProductsBase.DeliveryOptionID,
                                                           DeliveryOptionType.Shipping);
                                        sessionInf.IsVenuzulaShipping = false;
                                    }
                                }
                                if (isShowAddress)
                                {
                                    pAddress.InnerHtml =
                                        ProductsBase.GetShippingProvider()
                                                    .FormatShippingAddress(deliveryOption, option,
                                                                           ShoppingCart != null &&
                                                                           ShoppingCart.DeliveryInfo != null
                                                                               ? ShoppingCart.DeliveryInfo.Description
                                                                               : string.Empty, true);
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
        }

        protected void RBListInventoryView_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var sessionInfo = ProductsBase.SessionInfo;
            //sessionInfo.ShowAllInventory = (sender as RadioButtonList).SelectedIndex == 0;
            DistributorOrderingProfile.ShowAllInventory = sessionInfo.ShowAllInventory = rbShowAll.Checked;
            ProductsBase.SessionInfo = sessionInfo;

            if (sessionInfo.ShowAllInventory)
            {
                OnShowAllInventory(this, null);
            }
            else
            {
                OnShowAvailableInventory(this, null);
            }
        }

        public override void onShippingAddressUpdated(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                shippingAddressChanged(args.ShippingAddress);
                ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressChanged)]
        public void OnShippingAddressChanged(object sender, EventArgs e)
        {
            onShippingAddressUpdated(e as ShippingAddressEventArgs);
            if ((e as ShippingAddressEventArgs) != null)
            {
                upOrderQuickView.Update();
            }
        }

        public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                SessionInfo.IsVenuzulaShippingNew = true;
                shippingAddressCreated(args.ShippingAddress);
                reload();
                lnAddAddress.Visible = false;

                if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.APFConfiguration.DeliveryAllowed))
                {
                    if (this.ShoppingCart != null
                        && this.ShoppingCart.DeliveryInfo != null
                        && this.ShoppingCart.DeliveryInfo.Address != null)
                    {
                        this.ShoppingCart.ShippingAddressID = this.ShoppingCart.DeliveryInfo.Address.ID;
                    }
                }
            }
            if (null != DropdownNickName.SelectedItem)
            {
                lbShiptoOrPickup.Visible = DropdownNickName.SelectedItem.Value != "0";
            }
            ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressCreated)]
        public void OnShippingAddressCreated(object sender, EventArgs e)
        {
            onShippingAddressCreated(e as ShippingAddressEventArgs);
            if ((e as ShippingAddressEventArgs) != null)
            {
                upOrderQuickView.Update();
            }
        }

        public override void onShippingAddressDeleted(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    shippingAddressDeleted(args.ShippingAddress);
                }

                int id = 0; ////// ?????????
                if (DropdownNickName.SelectedIndex >= 0)
                {
                    if (int.TryParse(DropdownNickName.SelectedValue, out id))
                    {
                        var deliveryOption = getSelectedAddress(id,
                                                                (DeliveryOptionType)
                                                                Enum.Parse(typeof(DeliveryOptionType),
                                                                           DeliveryType.SelectedValue));
                        if (deliveryOption != null)
                        {
                            updateShippingInfo(deliveryOption.ID, ProductsBase.DeliveryOptionID,
                                               DeliveryOptionType.Shipping);
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
                ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressDeleted)]
        public void OnShippingAddressDeleted(object sender, EventArgs e)
        {
            onShippingAddressDeleted(e as ShippingAddressEventArgs);
            upOrderQuickView.Update();
        }

        [SubscribesTo(MyHLEventTypes.ItemRemovedAPFLeft)]
        public void OnItemRemovedAPFLeft(object sender, EventArgs e)
        {
            pnlShippingMethod.Visible = false;
            ppShippingInfoControl.Update();
        }

        private string CourierTypeSelected
        {
            get { return ProductsBase.GetShippingProvider().GetCourierTypeBySelection(DeliveryType.SelectedValue ?? "PickupFromCourier"); }
        }

        [SubscribesTo(MyHLEventTypes.ShippingInfoNotFilled)]
        public void OnShippingInfoNotFilled(object sender, EventArgs e)
        {
            if (ProductsBase.NeedEnterAddress())
            {
                // If shipping is allowed.
                if (HLConfigManager.Configurations.DOConfiguration.AllowShipping)
                {
                    AddAddressClicked(lnAddAddress, e);
                }
                else if (HLConfigManager.Configurations.DOConfiguration.IsChina && (_pickupRrefList == null || _pickupRrefList.Count == 0))
                {
                    //var message = PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated");
                    //(ProductsBase.Master as OrderingMaster).Status.AddMessage(message);
                    ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(CourierTypeSelected));
                }
                else
                {
                    var message = PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated");
                    (ProductsBase.Master as OrderingMaster).Status.AddMessage(message);
                }
            }
            else
            {
                var eventTicketMode = false;
                if (SessionInfo !=null)
                {
                    eventTicketMode = SessionInfo.IsEventTicketMode;
                }
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && (_pickupRrefList == null || _pickupRrefList.Count == 0) && !eventTicketMode)
                {

                    ucShippingInfoControl.ShowPopupForPickup(CommandType.Add,
                                                       new DeliveryOptionEventArgs(true));
                }
                else
                {
                    var currentOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    string message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                               "ShippingNickNameNotPopulated");
                    if (currentOption == DeliveryOptionType.Pickup)
                    {
                        message = PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated");
                    }

                    (ProductsBase.Master as OrderingMaster).Status.AddMessage(message);
                }
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
            if (APFDueProvider.ShouldHideOrderQuickView(ShoppingCart))
            {
                return;
            }

            if (DropdownNickName != null)
            {
                if (DropdownNickName.Items.Count == 0)
                {
                    if (DeliveryType.SelectedValue == "Shipping")
                    {
                        if (ProductsBase.ShoppingCart.OrderCategory == OrderCategoryType.ETO &&
                            !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
                        {
                            return;
                        }
                        PickUpNicknameNotSelectedInMiniCart(this, new DeliveryOptionEventArgs(1, string.Empty));
                    }
                }
                else if (DropdownNickName.Items.Count > 0)
                {
                    if (DropdownNickName.SelectedItem != null)
                    {
                        if (DropdownNickName.SelectedItem.Value == "0")
                        {
                            if (DeliveryType.SelectedValue == "Pickup")
                            {
                                PickUpNicknameNotSelectedInMiniCart(this, new DeliveryOptionEventArgs(2, string.Empty));
                            }
                            else if (DeliveryType.SelectedValue == "PickupFromCourier")
                            {
                                PickUpNicknameNotSelectedInMiniCart(this, new DeliveryOptionEventArgs(3, string.Empty));
                            }
                            else if (ShoppingCart.Locale =="es-VE" && DeliveryType.SelectedValue == "Shipping")
                            {
                                PickUpNicknameNotSelectedInMiniCart(this, new DeliveryOptionEventArgs(1, string.Empty));
                            }
                        }
                    }
                }
            }
            //}
        }

        [SubscribesTo(MyHLEventTypes.OnSaveCart)]
        public void OnSaveCart(object sender, EventArgs e)
        {
            CanSaveCart(ref DeliveryType, ref DropdownNickName);
        }

        #region page load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (APFDueProvider.ShouldHideOrderQuickView(ShoppingCart))
            {
                return;
            }

            if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode &&
                !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
            {
                return;
            }

            loadData();

            // Displaying the only allowed delivery type if specified for ETO
            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO || SessionInfo.IsEventTicketMode)
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedETO))
                {
                    DeliveryType.Items.Cast<ListItem>()
                                .Where(i => i.Value != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedETO)
                                .ToList()
                                .ForEach(DeliveryType.Items.Remove);
                }
            }

            if (ShoppingCart.OrderCategory == OrderCategoryType.HSO || SessionInfo.IsHAPMode)
            {
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedHAP))
                {
                    DeliveryType.Items.Cast<ListItem>()
                                .Where(i => i.Value != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedHAP)
                                .ToList()
                                .ForEach(DeliveryType.Items.Remove);
                }
            }

            // Displaying the only allowed delivery type if specified
            if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.APFConfiguration.DeliveryAllowed))
            {
                var deliveryOptionsToRemove = DeliveryType.Items.Cast<ListItem>()
                            .Where(i => i.Value != HLConfigManager.Configurations.APFConfiguration.DeliveryAllowed)
                            .ToList();

                if (deliveryOptionsToRemove.Any())
                {
                    var selected = 0;
                    if (this.ShoppingCart != null && this.ShoppingCart.DeliveryInfo != null)
                    {
                        selected = HLConfigManager.Configurations.APFConfiguration.DeliveryAllowed == "Shipping"
                                       ? this.ShoppingCart.ShippingAddressID
                                       : this.ShoppingCart.DeliveryInfo.Id;
                    }

                    deliveryOptionsToRemove.ForEach(DeliveryType.Items.Remove);
                    OnDeliveryTypeChanged(sender, e);

                    if (DropdownNickName.Items.FindByValue(selected.ToString(CultureInfo.InvariantCulture)) != null &&
                       selected > 0)
                    {
                        DropdownNickName.SelectedValue = selected.ToString(CultureInfo.InvariantCulture);
                        OnNickNameChanged(sender, e);
                    }
                }
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
            if (IsPostBack)
            {
                ViewState["DeliveryOption"] = getDeliveryOptionTypeFromDropdown(DeliveryType);
            }
            SessionInfo.IsVenuzulaShippingNew = false;
        }

        #endregion page load
    }
}