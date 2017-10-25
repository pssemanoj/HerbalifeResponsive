using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class PTCheckOutOptions : CheckOutOptions
    {
        #region Properties

        public new bool DisplayHoursOfOperationForPickup { get; set; }

        #endregion Properties

        #region ctor

        public PTCheckOutOptions()
        {
            DisplayHoursOfOperationForPickup = true;
        }

        #endregion ctor

        #region PTCheckOutOptionsControlMethods

        protected override void LoadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                   ShippingProvider.GetDefaultAddress());

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                _pickupRrefList = ProductsBase.GetShippingProvider()
                                              .GetPickupLocationsPreferences(DistributorID, CountryCode);
            }
        }

        protected override void ConfigureMenu()
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier &&
                DeliveryType.Items.FindByValue("PickupFromCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
            }
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowShipToCourier &&
                DeliveryType.Items.FindByValue("ShipToCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("ShipToCourier"));
            }
        }

        protected override void populateDropdown(DropDownList deliveryType)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (null != deliveryType && deliveryType.Items.Count == 3 && deliveryType.Items[0].Value == "0")
                {
                    deliveryType.Items.RemoveAt(0);
                }

                if (deliveryType != null && deliveryType.Items.Count > 0)
                {
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        deliveryType.ClearSelection();
                        deliveryType.Items[0].Selected = true;
                        populateShipping();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        deliveryType.ClearSelection();
                        deliveryType.Items[1].Selected = true;
                        populatePickup();
                    }
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        deliveryType.ClearSelection();
                        deliveryType.Items[2].Selected = true;
                        populatePickupPreference();
                        //populateShipping();
                    }
                }
            }
            else
            {
                if (hasShippingAddresses)
                {
                    deliveryType.Items[0].Selected = true;
                    populateShipping();
                }
                else
                {
                    var textSelect = (string) GetLocalResourceObject("TextSelect");
                    deliveryType.Items.Insert(0, new ListItem(textSelect, "0"));
                    deliveryType.Items[0].Selected = true;
                }
            }
        }

        protected override void BindDataForReadOnlyView()
        {
            showShiptoOrPickup(IsStatic);
            if (ShoppingCart.DeliveryInfo != null)
            {
                switch (ShoppingCart.DeliveryInfo.Option)
                {
                    case DeliveryOptionType.Pickup:
                        lblSelectedDeliveryType.Text = (string) GetLocalResourceObject("DeliveryOptionType_Pickup.Text");
                        lblNickName.Text = ShoppingCart.DeliveryInfo.Description;
                        break;
                    case DeliveryOptionType.PickupFromCourier:
                        lblSelectedDeliveryType.Text =
                            (string) GetLocalResourceObject("DeliveryOptionType_PickupFromCourier.Text");
                        lblNickName.Text = ShoppingCart.DeliveryInfo.Description;
                        break;
                    case DeliveryOptionType.Shipping:
                        lblSelectedDeliveryType.Text =
                            (string) GetLocalResourceObject("DeliveryOptionType_Shipping.Text");
                        lblNickName.Text = ShoppingCart.DeliveryInfo.Address.Alias;
                        break;
                    case DeliveryOptionType.ShipToCourier:
                        lblSelectedDeliveryType.Text =
                            (string) GetLocalResourceObject("DeliveryOptionType_ShipToCourier.Text");
                        lblNickName.Text = ShoppingCart.DeliveryInfo.Address.Alias;
                        break;
                }

                if (DisplayHoursOfOperationForPickup)
                    pInformation.InnerHtml = ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                                                 ? ShoppingCart.DeliveryInfo.AdditionalInformation
                                                 : string.Empty;
                pAddress.InnerHtml =
                    ProductsBase.GetShippingProvider().FormatShippingAddress(
                        new DeliveryOption(ShoppingCart.DeliveryInfo.Address), ShoppingCart.DeliveryInfo.Option,
                        ShoppingCart.DeliveryInfo.Description, true);
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail ||
                !String.IsNullOrEmpty(ShoppingCart.EmailAddress))
            {
                if (String.IsNullOrEmpty(ShoppingCart.EmailAddress))
                {
                    SessionInfo sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
                    if (!String.IsNullOrEmpty(sessionInfo.ChangedEmail))
                    {
                        ShoppingCart.EmailAddress = sessionInfo.ChangedEmail;
                    }
                }
                txtLongEmailAddress.Text = ShoppingCart.EmailAddress;
                lblShortEmail.Text = ShoppingCart.EmailAddress;
                if (!String.IsNullOrEmpty(ShoppingCart.EmailAddress))
                {
                    if (ShoppingCart.EmailAddress.Length > 55)
                    {
                        txtLongEmailAddress.Visible = true;
                        lblShortEmail.Visible = false;
                    }
                    else
                    {
                        txtLongEmailAddress.Visible = false;
                        lblShortEmail.Visible = true;
                    }
                }
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasAdditonalNumber)
            {
                if (!String.IsNullOrEmpty(ShoppingCart.SMSNotification))
                {
                    lblMobileNumberEntered.Text = ShoppingCart.SMSNotification;
                }
            }
        }

        protected void populatePickupPreference()
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
            else
            {
                DropdownNickName.Visible = false;
                divshipToOrPickup.Visible = false;
            }
        }

        public override bool CheckNoDeliveryType(DropDownList DeliveryType)
        {
            if (null != DeliveryType && null != ShoppingCart &&
                null != ShoppingCart.DeliveryInfo && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return true;
            }
            return false;
        }

        #endregion PTCheckOutOptionsControlMethods

        #region DeliveryOptionMethods

        protected override void showHideAddressLink()
        {
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            lnAddAddress.Visible = false;
            if (deliveryType == DeliveryOptionType.Unknown)
            {
                lnAddAddress.Visible = true;
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                lnAddAddress.Visible = hasNoPreference() && ShoppingCart.DeliveryInfo == null;
            }
            else if (deliveryType == DeliveryOptionType.ShipToCourier || deliveryType == DeliveryOptionType.Shipping)
            {
                lnAddAddress.Visible = _shippingAddresses.Count == 0;
            }
            if (lnAddAddress.Visible == false)
            {
                if (IsStatic == false)
                {
                    divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = !lnAddAddress.Visible;
                    if (CheckNoDeliveryType(DeliveryType))
                    {
                        divLinks.Visible = true;
                    }
                    else
                    {
                        divLinks.Visible = deliveryType == DeliveryOptionType.Shipping;
                        if (deliveryType == DeliveryOptionType.Pickup)
                        {
                            DropdownNickName.Visible = !lnAddAddress.Visible;
                        }
                    }

                    if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                    {
                        if (!IsStatic)
                        {
                            divLinks.Visible = true;
                            LinkEdit.Visible = (deliveryType == DeliveryOptionType.Shipping);
                        }
                    }
                }
                else
                {
                    divLinks.Visible = false;
                }
            }
            else
            {
                divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = false;
                if (deliveryType == DeliveryOptionType.Unknown || deliveryType == DeliveryOptionType.Shipping)
                {
                    lnAddAddress.Text = (string) GetLocalResourceObject("AddShippingAddress");
                }
                else
                {
                    lnAddAddress.Text = String.Empty;
                }
                divLinks.Visible = false;
                if (deliveryType == DeliveryOptionType.Unknown &&
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup)
                {
                    lnAddAddress.Visible = false;
                }
                else
                {
                    lnAddAddress.Visible = true;
                }
            }

            if (deliveryType == DeliveryOptionType.Shipping)
            {
                if (_shippingAddresses.Count == 1 && (Page as ProductsBase).CantDeleteFinalAddress)
                {
                    DisableDeleteLink(true);
                }
                else if (_shippingAddresses.Count > 1)
                {
                    DisableDeleteLink(false);
                }
            }
            if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = true;
                LinkEdit.Visible = false;
            }
            else
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                {
                    if (_pickupRrefList.Count == 1 && (Page as ProductsBase).CantDeleteFinalAddress)
                    {
                        DisableDeleteLink(true);
                    }
                    else if (_shippingAddresses.Count > 1)
                    {
                        DisableDeleteLink(false);
                    }
                }
            }
        }

        protected override void showShiptoOrPickup(bool isStatic)
        {
            DeliveryOptionType selectedOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
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

            if (selectedOption == DeliveryOptionType.Shipping || selectedOption == DeliveryOptionType.ShipToCourier)
            {
                lbShiptoOrPickup.Text = (string) GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = (string) GetLocalResourceObject("NickName");
                divDeliveryMethodShipping.Visible = true;
                divDeliveryMethodPickup.Visible = false;
                LoadShippingInstructions(IsStatic);
            }
            else
            {
                if (CheckNoDeliveryType(DeliveryType))
                {
                    lbShiptoOrPickup.Text = (string) GetLocalResourceObject("ShipTo");
                    NickNameReadonly.Text = (string) GetLocalResourceObject("NickName");
                    divDeliveryMethodShipping.Visible = true;
                    divDeliveryMethodPickup.Visible = false;
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    lbShiptoOrPickup.Text = (string) GetLocalResourceObject("PickUpFrom");
                    NickNameReadonly.Text = (string) GetLocalResourceObject("Location");
                    divDeliveryMethodShipping.Visible = false;
                    divDeliveryMethodPickup.Visible = true;
                    LoadPickUpInstructions(IsStatic);
                }
            }
        }

        #endregion DeliveryOptionMethods

        #region DeliveryInstructionsMethods

        protected override void LoadPickUpInstructions(bool IsStatic)
        {
            DeliveryOptionType selectedOption = ShoppingCart.DeliveryInfo != null
                                                    ? ShoppingCart.DeliveryInfo.Option
                                                    : getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (selectedOption == DeliveryOptionType.Pickup || selectedOption == DeliveryOptionType.PickupFromCourier)
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                TextShippingMethod.Text = (string) GetLocalResourceObject("PickUpBy");

                if (!IsStatic)
                {
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate)
                    {
                        pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode);
                        pickupdateTextBox.MaxDate = pickupdateTextBox.MinDate + new TimeSpan(14, 0, 0, 0);
                        pickupdateTextBox.SelectedDate = pickupdateTextBox.MinDate;
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                            {
                                DateTime dtShoppingCartDate;
                                bool success = DateTime.TryParse(ShoppingCart.DeliveryInfo.Instruction,
                                                                 out dtShoppingCartDate);
                                if (success)
                                {
                                    pickupdateTextBox.SelectedDate = dtShoppingCartDate;
                                }
                            }
                        }
                    }
                    else
                    {
                        trEditableDatePicker.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                    {
                        txtPickupName.Attributes.Add("maxlength", "36");
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                            {
                                txtPickupName.Text = ShoppingCart.DeliveryInfo.Address.Recipient;
                            }
                        }
                    }
                    else
                    {
                        trEditablePickupName.Visible = false;
                    }

                    // Pick up Phone
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone)
                    {
                        if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneHasPhoneMask)
                        {
                            txtPhoneNumber_MaskedEditExtender.Enabled = false;
                            txtPickupPhone.Attributes.Add("maxlength",
                                                          HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                                         .PickUpPhoneMaxLen.ToString());
                        }
                        else
                        {
                            txtPhoneNumber_MaskedEditExtender.Enabled = true;
                            txtPhoneNumber_MaskedEditExtender.Mask =
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneMask;
                        }
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))
                            {
                                txtPickupPhone.Text = ShoppingCart.DeliveryInfo.Address.Phone;
                            }
                        }
                    }
                    else
                    {
                        trEditablePickupPhone.Visible = false;
                    }

                    // Have Time Field
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                    {
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            PopulatePickUpInstructionsTimeDropDown();
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                            {
                                ListItem li = ddlPickupTime.Items.FindByText(ShoppingCart.DeliveryInfo.Instruction);
                                if (li != null)
                                {
                                    li.Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        trEditablePickUpTimeDropDown.Visible = false;
                    }
                }
                else // if (!IsStatic)
                {
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate)
                    {
                        if (!APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                        {
                            lbPickupdate.Text = ShoppingCart.DeliveryInfo != null
                                                    ? (ShoppingCart.DeliveryInfo.Instruction != null
                                                           ? DateTime.Parse(ShoppingCart.DeliveryInfo.Instruction)
                                                                     .ToShortDateString()
                                                           : string.Empty)
                                                    : String.Empty;
                        }
                    }
                    else
                    {
                        trStaticDatePicker.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                    {
                        lbPickupname.Text = ShoppingCart.DeliveryInfo != null
                                                ? ShoppingCart.DeliveryInfo.Address.Recipient
                                                : String.Empty;
                    }
                    else
                    {
                        trStaticPickupName.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone)
                    {
                        lblPickUpPhone.Text = ShoppingCart.DeliveryInfo != null
                                                  ? ShoppingCart.DeliveryInfo.Address.Phone
                                                  : String.Empty;
                    }
                    else
                    {
                        trStaticPickupPhone.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                    {
                        if (!APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                        {
                            txtPickupTime.Text = ShoppingCart.DeliveryInfo != null
                                                     ? ShoppingCart.DeliveryInfo.Instruction
                                                     : String.Empty;
                        }
                    }
                    else
                    {
                        trStaticPickUpTimeDropDown.Visible = false;
                    }
                }
            }
        }

        #endregion DeliveryInstructionsMethods

        #region EventHandlerDeliveryOptions

        protected new void OnNickName_Databind(object sender, EventArgs e)
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

                    if (CheckNoDeliveryType(DeliveryType) &&
                        deliveryOptionTypeFromDowndown == DeliveryOptionType.Unknown)
                    {
                        deliveryOptionTypeFromDowndown = DeliveryOptionType.Shipping;
                    }

                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
                        {
                            DeliveryOption deliveryOption = GetSelectedAddress(deliveryInfo.Address.ID,
                                                                               DeliveryOptionType.Shipping);
                            ListItem selected =
                                ddl.Items.FindByValue(deliveryOption == null
                                                          ? string.Empty
                                                          : deliveryInfo.Address.ID.ToString());
                            if (selected != null)
                            {
                                ddl.ClearSelection();
                                selected.Selected = true;
                            }
                            else
                            {
                                if (CheckNoDeliveryType(DeliveryType))
                                {
                                    if (null != ddl && ddl.Items.Count > 0 && ddl.Items.Count == 1)
                                    {
                                        ddl.ClearSelection();
                                        ddl.Items[0].Selected = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ddl.ClearSelection();
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                        }
                    }
                    else
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Pickup)
                        {
                            if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                            {
                                var varPref = _pickupLocations.Where(f => f.Id == deliveryInfo.Id);
                                if (varPref.Count() > 0)
                                {
                                    ListItem selected = ddl.Items.FindByValue(deliveryInfo.Id.ToString());
                                    if (selected != null)
                                    {
                                        ddl.ClearSelection();
                                        selected.Selected = true;
                                        DisableDeleteLink(varPref.First().IsPrimary);
                                    }
                                }
                            }
                            else
                            {
                                var varPref = _pickupRrefList.Where(f => f.PickupLocationID == deliveryInfo.Id);
                                if (varPref.Count() > 0)
                                {
                                    ListItem selected = ddl.Items.FindByValue(varPref.First().ID.ToString());
                                    if (selected != null)
                                    {
                                        selected.Selected = true;
                                    }
                                }
                            }
                        }
                        else if (deliveryOptionTypeFromDowndown == DeliveryOptionType.PickupFromCourier)
                        {
                            var varPref = _pickupRrefList.Where(f => f.ID == deliveryInfo.Id);
                            if (varPref.Count() > 0)
                            {
                                ListItem selected = ddl.Items.FindByValue(varPref.First().ID.ToString());
                                if (selected != null)
                                {
                                    selected.Selected = true;
                                }
                            }
                        }
                        else
                        {
                            ddl.ClearSelection();
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                        }
                    }
                }
                else
                {
                    ddl.ClearSelection();
                    ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                    ddl.Items[0].Selected = true;
                }
            }
        }

        protected new void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            lblNickName.Visible = false;
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                ShoppingCart.DeliveryInfo = null;
                populateShipping();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divDeliveryMethodShipping.Visible = true;
                divDeliveryMethodPickup.Visible = false;
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                }
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                populatePickupPreference();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = true;
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }
            }
            else
            {
                ShoppingCart.DeliveryInfo = null;
                populatePickup();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = true;
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }
            }
            showShiptoOrPickup(IsStatic);
            if (null != DropdownNickName.SelectedItem)
            {
                lbShiptoOrPickup.Visible = DropdownNickName.SelectedItem.Value != "0";
            }
        }

        protected new void DeleteClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                {
                    DeliveryOption deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                       (DeliveryOptionType)
                                                                       Enum.Parse(typeof (DeliveryOptionType),
                                                                                  DeliveryType.SelectedValue));
                    if (deliveryOption != null)
                    {
                        var mpAddAddress =
                            (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");

                        if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping ||
                            getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
                        {
                            ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete,
                                                                       new ShippingAddressEventArgs(DistributorID,
                                                                                                    deliveryOption,
                                                                                                    false,
                                                                                                    ProductsBase
                                                                                                        .DisableSaveAddressCheckbox));
                        }
                        mpAddAddress.Show();
                    }
                }
                else
                {
                    DeliveryOption deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                       (DeliveryOptionType)
                                                                       Enum.Parse(typeof (DeliveryOptionType),
                                                                                  DeliveryType.SelectedValue));
                    if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOption != null)
                        {
                            var mpAddAddress =
                                (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                            ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete,
                                                                       new ShippingAddressEventArgs(DistributorID,
                                                                                                    deliveryOption,
                                                                                                    false,
                                                                                                    ProductsBase
                                                                                                        .DisableSaveAddressCheckbox));
                            mpAddAddress.Show();
                        }
                    }
                    else
                    {
                        ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
                        if (shippingInfo != null)
                        {
                            var mpAddAddress =
                                (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                            List<PickupLocationPreference_V01> pickupLocationPreferences =
                                (Page as ProductsBase).GetShippingProvider()
                                                      .GetPickupLocationsPreferences(
                                                          (Page as ProductsBase).DistributorID,
                                                          (Page as ProductsBase).CountryCode);
                            if (int.Parse(DropdownNickName.SelectedValue) != 0)
                            {
                                PickupLocationPreference_V01 pref =
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
                            mpAddAddress.Show();
                        }
                    }
                }
            }
        }

        protected new void AddAddressClicked(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping ||
                getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
            {
                var mpAddAddress = (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
                mpAddAddress.Show();
            }
        }

        protected new void AddClicked(object sender, EventArgs e)
        {
            DeliveryOptionType deliveryOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var mpAddAddress = (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
            if (deliveryOption == DeliveryOptionType.Shipping || deliveryOption == DeliveryOptionType.Unknown)
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
            mpAddAddress.Show();
        }

        #endregion EventHandlerDeliveryOptions

        #region SubscriptionEvents

        public override void onShippingAddressDeleted(ShippingAddressEventArgs args)
        {
            base.onShippingAddressDeleted(args);
        }

        public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
            base.onShippingAddressCreated(args);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public new void OnPickupPreferenceCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId, DeliveryOptionType.Pickup);
            }
            populateDropdown();
            showHideAddressLink();
            setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
            showShiptoOrPickup(IsStatic);
            ppShippingInfoControl.Update();
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceDeleted)]
        public new void OnPickupPreferenceDeleted(object sender, EventArgs e)
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
                populateDropdown();
            }

            populateDropdown();
            showHideAddressLink();
            setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
            showShiptoOrPickup(IsStatic);

            if (_pickupRrefList.Count == 0)
            {
                setAddressByNickName(null);
            }
            ppShippingInfoControl.Update();
        }

        #endregion SubscriptionEvents
    }
}