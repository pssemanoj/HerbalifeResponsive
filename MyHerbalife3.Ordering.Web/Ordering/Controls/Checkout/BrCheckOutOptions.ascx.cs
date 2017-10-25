using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

    public partial class BrCheckOutOptions : CheckOutOptions
    {
        //[Publishes(MyHLEventTypes.QuoteRetrieved)]
        //new public event EventHandler OnQuoteRetrieved;

        [Publishes(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        new public event EventHandler OnCheckOutOptionsNotPopulated;

        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnBRQuoteRetrieved;

        [Publishes(MyHLEventTypes.WarehouseChanged)]
        public event EventHandler BrOnWarehouseChanged;

        public BrCheckOutOptions()
        {
            DisplayHoursOfOperationForPickup = true;
        }

        protected override void ConfigureMenu()
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier && DeliveryType.Items.FindByValue("PickupFromCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
            }
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowShipToCourier && DeliveryType.Items.FindByValue("ShipToCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("ShipToCourier"));
            }
        }

        protected override void LoadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(this.DistributorID, this.Locale);
            _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                new ShippingAddress_V01
                {
                    Address = new Address_V01
                    {
                        StateProvinceTerritory = getDSMailingAddressState(),
                        Country = CountryCode,
                    }
                }
                );

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                _pickupRrefList = ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(DistributorID, CountryCode);
            }
            else
            {
                _pickupRrefList = ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(DistributorID, CountryCode);
            }
        }

        protected override void RenderEditableView()
        {
            base.RenderEditableView();

            trEditableRGNumber.Visible = true;
            trCourierName.Visible = true;
            trStaticRGNumber.Visible = false;
            trStaticCourierName.Visible = false;
        }

        protected override void RenderReadOnlyView()
        {
            base.RenderReadOnlyView();

            trStaticRGNumber.Visible = true;
            trStaticCourierName.Visible = this.ShoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.ShipToCourier;
            trEditableRGNumber.Visible = false;
            trCourierName.Visible = false;
        }

        #region CheckOutOptionsControlMethods

        protected override void BindDataForEditableView()
        {
            base.BindDataForEditableView();
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                {
                    trEditableRGNumber.Visible = false;
                    setAddressByNickName(ShoppingCart.DeliveryInfo.Address);
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                    {
                        txtCourierName.Text = ShoppingCart.CourierName;
                    }
                }
                else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    trEditableRGNumber.Visible = true;
                    if (ShoppingCart.ShippingAddressID == 0)
                    {
                        pDSAddress.InnerHtml = "";
                    }
                    else
                    {
                        if (ShoppingCart.DeliveryInfo != null && ShoppingCart.ShippingAddressID != 0)
                        {
                            DeliveryOption deliveryOption = getShippingAddressByID(ShoppingCart.ShippingAddressID);
                            if (deliveryOption != null)
                            {
                                pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                            }
                        }
                        if (_shippingAddresses.Count == 1 && ShoppingCart.ShippingAddressID != _shippingAddresses[0].Id)
                        {
                            DropdownDSAddressNickName.Attributes.Add("style", "display:none");
                            disableDSAddressLinkDelete(true);
                            pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(_shippingAddresses[0], DeliveryOptionType.Shipping, string.Empty, true);

                            this.ShoppingCart.ShippingAddressID = _shippingAddresses[0].ID;
                        }
                        else
                        {
                            disableDSAddressLinkDelete(false);
                        }
                    }
                    if (_pickupLocations != null && _pickupLocations.Count == 1)
                    {
                        this.DropdownNickName.Items[0].Selected = true;
                    }
                }
                else
                {
                    var courierLocation = (from p in _pickupRrefList where p.ID == ShoppingCart.DeliveryInfo.Id select p).FirstOrDefault();
                    if (courierLocation != null)
                    {
                        var description = courierLocation.PickupLocationNickname;
                        var listItem = DropdownNickName.Items.FindByText(description);
                        if (listItem != null)
                        {
                            listItem.Selected = true;
                        }
                    }
                }
            }
        }

        protected override void BindDataForReadOnlyView()
        {
            base.BindDataForReadOnlyView();
            if (ShoppingCart.DeliveryInfo != null)
            {
                lblRGNumberValue.Text = ShoppingCart.DeliveryInfo.RGNumber;
                lbCourierName.Text = ShoppingCart.CourierName;
                switch (ShoppingCart.DeliveryInfo.Option)
                {
                    case DeliveryOptionType.Shipping:
                        lblSelectedDeliveryType.Text = (string)GetLocalResourceObject("ListItemResource1.Text");
                        break;
                    case DeliveryOptionType.ShipToCourier:
                        lblSelectedDeliveryType.Text = (string)GetLocalResourceObject("ListItemResource5.Text");
                        break;
                    case DeliveryOptionType.Pickup:
                        lblSelectedDeliveryType.Text = (string)GetLocalResourceObject("ListItemResource2.Text");
                        break;
                    case DeliveryOptionType.PickupFromCourier:
                        lblSelectedDeliveryType.Text = (string)GetLocalResourceObject("ListItemResource6.Text");
                        var courierLocation = (from p in _pickupRrefList where p.ID == ShoppingCart.DeliveryInfo.Id select p).FirstOrDefault();
                        if (courierLocation != null)
                        {
                            lblNickName.Text = courierLocation.PickupLocationNickname;
                        }
                        else
                        {
                            lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias) ?
                                ShoppingCart.DeliveryInfo.Address.DisplayName : ShoppingCart.DeliveryInfo.Address.Alias);
                        }
                        break;
                }
                lblNickName.Text = ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier
                    ? (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias) ? ShoppingCart.DeliveryInfo.Address.DisplayName : ShoppingCart.DeliveryInfo.Address.Alias)
                    : ShoppingCart.DeliveryInfo.Description;

                pAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(
                    new DeliveryOption(ShoppingCart.DeliveryInfo.Address), ShoppingCart.DeliveryInfo.Option,
                    ShoppingCart.DeliveryInfo.Description, true);
            }
        }

        protected override void showShiptoOrPickup(bool isStatic)
        {
            DeliveryOptionType selectedOption;
            if (Session["deliveryOptionSelected"] != null)
            {
                selectedOption = (DeliveryOptionType)Session["deliveryOptionSelected"];
                Session.Remove("deliveryOptionSelected");
                DeliveryType.SelectedValue = selectedOption.ToString();
                if(ShoppingCart.DeliveryInfo != null)
                    ShoppingCart.DeliveryInfo.Option = selectedOption;
            }
            else
            {
                selectedOption = isStatic == false ? getDeliveryOptionTypeFromDropdown(DeliveryType) : (ShoppingCart.DeliveryInfo == null ? DeliveryOptionType.Shipping : ShoppingCart.DeliveryInfo.Option);
            }
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

            pInformation.InnerHtml = string.Empty;
            if (selectedOption == DeliveryOptionType.Shipping)
            {
                lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = (string)GetLocalResourceObject("NickName");
                divDeliveryMethodShipping.Visible = true;
                divDeliveryMethodPickup.Visible = false;
                divDeliveryMethodShipToCourier.Visible = false;
                LoadShippingInstructions(IsStatic);
            }
            else if (selectedOption == DeliveryOptionType.ShipToCourier)
            {
                lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = (string)GetLocalResourceObject("NickName");
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = false;
                divDeliveryMethodShipToCourier.Visible = true;
                LoadShippingInstructions(IsStatic);
            }
            else
            {
                lbShiptoOrPickup.Text = (string)GetLocalResourceObject("PickUpFrom");
                NickNameReadonly.Text = (string)GetLocalResourceObject("Location");
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = true;
                divDeliveryMethodShipToCourier.Visible = false;
                trPickupDetails.Visible = (selectedOption == DeliveryOptionType.Pickup || !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                trPickupFromCourierDetails.Visible = (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                LoadPickUpInstructions(IsStatic);
                loadDistributorAddress(IsStatic, selectedOption);
                pInformation.InnerHtml = this.DisplayHoursOfOperationForPickup &&
                    ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                    ? ShoppingCart.DeliveryInfo.AdditionalInformation
                    : string.Empty;
            }
        }

        private DeliveryOption getShippingAddressByID(int shippingAddressID)
        {
            return _shippingAddresses.Find(s => s.ID == shippingAddressID);
        }

        private void loadDistributorAddress(bool IsStatic, DeliveryOptionType deliveryType)
        {
            if (deliveryType == DeliveryOptionType.Pickup)
            {
                if (IsStatic == false)
                {
                    DropdownDSAddressNickName.Items.Clear();

                    string textSelect = (string)GetLocalResourceObject("TextSelect");
                    bool noAddress = _shippingAddresses.Count() == 0;

                    dvDSAddressEdit.Visible = true;
                    divDeliveryMethodShipToCourier.Visible = false;
                    divAddDSAddressLink.Visible = noAddress == true;
                    if (!noAddress)
                    {
                        populateShipping(DropdownDSAddressNickName);
                        disableDSAddressLinkDelete(DropdownDSAddressNickName.Items.Count == 1);

                        if (this.ShoppingCart.ShippingAddressID != 0)
                        {
                            DeliveryOption deliveryOption = getShippingAddressByID(ShoppingCart.ShippingAddressID);
                            if (deliveryOption != null)
                            {
                                ListItem selected = DropdownDSAddressNickName.Items.FindByValue(deliveryOption == null ? string.Empty : ShoppingCart.ShippingAddressID.ToString());
                                if (selected != null)
                                {
                                    DropdownDSAddressNickName.ClearSelection();
                                    selected.Selected = true;
                                    pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                                    //setAddressByNickName(deliveryOption, pDSAddress, DropdownDSAddressNickName);
                                }
                            }
                        }
                        if (_shippingAddresses.Count == 1 && this.ShoppingCart.ShippingAddressID == 0)
                        {
                            if (_shippingAddresses[0].ID != this.ShoppingCart.ShippingAddressID)
                            {
                                DropdownDSAddressNickName.ClearSelection();
                                DropdownDSAddressNickName.Items[0].Selected = true;
                                DropdownDSAddressNickName.Visible = false;

                                pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(_shippingAddresses[0], DeliveryOptionType.Shipping, string.Empty, true);
                                DropdownDSAddressNickName.Attributes.Add("style", "display:none");
                                this.ShoppingCart.ShippingAddressID = _shippingAddresses[0].ID;

                                updateShippingInfo(_shippingAddresses[0].ID, ShoppingCart.DeliveryInfo == null ? 0 :
                                        ShoppingCart.DeliveryInfo.Id,
                                        DeliveryOptionType.Pickup);
                                DropdownDSAddressNickName.Attributes.Remove("style");
                            }
                        }
                        if (this.ShoppingCart.ShippingAddressID == 0)
                        {
                            if (DropdownDSAddressNickName.Items.FindByValue("0") == null)
                            {
                                DropdownDSAddressNickName.Items.Insert(0, new ListItem(textSelect, "0"));
                            }
                            DropdownDSAddressNickName.ClearSelection();
                            DropdownDSAddressNickName.Items[0].Selected = true;
                        }
                    }
                    else
                    {
                        DropdownDSAddressNickName.Visible = false;
                        divDSAddressLink.Visible = false;
                    }
                }
                else
                {
                    dvDSAddressEdit.Visible = false;
                    dvDSAddressReadOnly.Visible = true;
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.ShippingAddressID != 0)
                    {
                        DeliveryOption deliveryOption = getShippingAddressByID(ShoppingCart.ShippingAddressID);
                        if (deliveryOption != null)
                        {
                            //setAddressByNickName(deliveryOption, pDSAddressReadOnly, DropdownDSAddressNickName);
                            pDSAddressReadOnly.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                        }
                    }
                }
            }
        }

        protected override void LoadPickUpInstructions(bool IsStatic)
        {
            DeliveryOptionType selectedOption = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Option : getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (selectedOption == DeliveryOptionType.Pickup || selectedOption == DeliveryOptionType.PickupFromCourier)
            {
                if (selectedOption == DeliveryOptionType.Pickup && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                if (selectedOption == DeliveryOptionType.PickupFromCourier && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                TextShippingMethod.Text = (string)GetLocalResourceObject("PickUpBy");
                this.divDeliveryTime.Visible = false;

                if (!IsStatic)
                {
                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate))
                    {
                        pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode);
                        pickupdateTextBox.MaxDate = pickupdateTextBox.MinDate + new TimeSpan(14, 0, 0, 0);
                        pickupdateTextBox.SelectedDate = pickupdateTextBox.MinDate;
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                            {
                                DateTime dtShoppingCartDate;
                                bool success = DateTime.TryParse(ShoppingCart.DeliveryInfo.Instruction, out dtShoppingCartDate);
                                if (success)
                                {
                                    pickupdateTextBox.SelectedDate = dtShoppingCartDate;
                                }
                            }
                        }
                        trEditableDatePicker.Visible = true;
                    }
                    else
                    {
                        trEditableDatePicker.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                    {
                        this.txtPickupName.Attributes.Add("maxlength", "36");
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
                            this.txtPickupPhone.Attributes.Add("maxlength", HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneMaxLen.ToString());
                        }
                        else
                        {
                            txtPhoneNumber_MaskedEditExtender.Enabled = true;
                            txtPhoneNumber_MaskedEditExtender.Mask = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneMask;
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

                    // RG Number field
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber)
                    {
                        trEditableRGNumber.Visible = true;
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.RGNumber))
                            {
                                txtRGNumber.Text = ShoppingCart.DeliveryInfo.RGNumber;
                            }
                        }
                    }
                    else
                    {
                        trEditableRGNumber.Visible = false;
                    }
                }
                else // if (!IsStatic)
                {
                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate))
                    {
                        if (!APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                        {
                            lbPickupdate.Text = ShoppingCart.DeliveryInfo != null ?
                                (ShoppingCart.DeliveryInfo.Instruction != null ? DateTime.Parse(ShoppingCart.DeliveryInfo.Instruction).ToShortDateString() : string.Empty)
                                : String.Empty;
                        }
                    }
                    else
                    {
                        trStaticDatePicker.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                    {
                        lbPickupname.Text = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Address.Recipient : String.Empty;
                    }
                    else
                    {
                        trStaticPickupName.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone)
                    {
                        lblPickUpPhone.Text = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Address.Phone : String.Empty;
                    }
                    else
                    {
                        trStaticPickupPhone.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                    {
                        if (!APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                        {
                            txtPickupTime.Text = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Instruction : String.Empty;
                        }
                    }
                    else
                    {
                        trStaticPickUpTimeDropDown.Visible = false;
                    }

                    // RG Number ===============
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber)
                    {
                        trEditableRGNumber.Visible = false;
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            lblRGNumberValue.Text = ShoppingCart.DeliveryInfo.RGNumber;
                        }
                    }
                    else
                    {
                        trStaticRGNumber.Visible = false;
                    }
                    // End RG Number ===============

                    // Delivery estimate in Post Office option
                    if (this.CountryCode == "BR" && selectedOption == DeliveryOptionType.PickupFromCourier)
                    {
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeEstimated)
                        {
                            var estimated = ProductsBase.GetShippingProvider().GetDeliveryEstimate(ShoppingCart.DeliveryInfo, this.Locale);
                            if (estimated != null)
                            {
                                this.hlDeliveryTime.Text =
                                    string.Format(GetLocalResourceObject("hlDeliveryTimeResource.Text") as string,
                                                  estimated.Value);
                                this.hlDeliveryTime.NavigateUrl =
                                    string.Format(@"/content/{0}/html/ordering/deliveryEstimator.html", this.Locale);
                                this.divDeliveryTime.Visible = true;
                            }
                            else
                            {
                                this.divDeliveryTime.Visible = false;
                            }
                        }
                    }
                }
            }
        }

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
                lnAddAddress.Visible = ProductsBase.GetShippingProvider().NeedEnterAddress(this.DistributorID, this.Locale) && ShoppingCart.DeliveryInfo == null;
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
                            LinkEdit.Visible = (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier);
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
                    lnAddAddress.Text = (string)GetLocalResourceObject("AddShippingAddress");
                }
                else
                {
                    lnAddAddress.Text = String.Empty;
                }
                divLinks.Visible = false;
                if (deliveryType == DeliveryOptionType.Unknown && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup)
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
                LinkEdit.Visible = true;
                if (_shippingAddresses.Count == 1 && (this.Page as ProductsBase).CantDeleteFinalAddress)
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
            if (deliveryType == DeliveryOptionType.ShipToCourier)
            {
                divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = true;
                LinkEdit.Visible = true;
            }
        }

        private ShippingAddress_V01 getDefaultAddress()
        {
            return new ShippingAddress_V01
            {
                Address = new Address_V01
                {
                    Country = "BR",
                }
            };
        }

        protected override void LoadShippingInstructions(bool IsStatic)
        {
            DeliveryOptionType selectedOption = ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Option : getDeliveryOptionTypeFromDropdown(DeliveryType);

            this.divDeliveryTime.Visible = false;
            //this.divCourierInfo.Visible = false;
            if (selectedOption == DeliveryOptionType.ShipToCourier)
            {
                //divDeliveryMethodShipToCourier.Visible = true;
                //trEditableShippingTime.Visible = false;
                //trEditableShippingMethod.Visible = false;
                //trEditableFreeFormShippingInstruction.Visible = false;
                //divDeliveryMethodShipping.Visible = false;
                if (!IsStatic)
                {
                    trCourierName.Visible = true;
                    trStaticCourierName.Visible = false;
                }
                else
                {
                    trStaticCourierName.Visible = true;
                    trCourierName.Visible = false;
                }
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null && !IsStatic)
                {
                    List<DeliveryOption> lstDeliveryOption = ProductsBase.GetShippingProvider().GetDeliveryOptions(DeliveryOptionType.ShipToCourier, getDefaultAddress());
                    if (lstDeliveryOption.Count > 0)
                    {
                        DeliveryOption option = lstDeliveryOption.Find(p => p.State.Trim() == ShoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory);

                        if (option == null)
                        {
                            option = lstDeliveryOption.Find(p => p.Address.StateProvinceTerritory == "SP");
                        }
                        if (option != null)
                        {
                            lbShippingMethod.Text = option.Description;
                            if (ShoppingCart.DeliveryInfo.WarehouseCode != option.WarehouseCode ||
                                ShoppingCart.DeliveryInfo.FreightCode != option.FreightCode)
                            {
                                ShoppingCart.DeliveryInfo.WarehouseCode = option.WarehouseCode;
                                ShoppingCart.DeliveryInfo.FreightCode = option.FreightCode;
                                ShoppingCart.Calculate();
                                ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                            }
                        }
                    }
                }
            }
            else if (selectedOption == DeliveryOptionType.Shipping)
            {
                base.LoadShippingInstructions(IsStatic);
            }
        }

        public override bool CheckNoDeliveryType(DropDownList DeliveryType)
        {
            if (null != DeliveryType && null != ShoppingCart &&
                    null != ShoppingCart.DeliveryInfo && (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier))
            {
                return true;
            }
            return false;
        }

        protected override DeliveryOption getSelectedAddress(int id, DeliveryOptionType optionType, DropDownList deliveryTypeDropdown)
        {
            DeliveryOption address = base.getSelectedAddress(id, optionType == DeliveryOptionType.ShipToCourier ? DeliveryOptionType.Shipping : optionType, deliveryTypeDropdown);
            if (optionType == DeliveryOptionType.Pickup)
            {
                if (deliveryTypeDropdown == DropdownDSAddressNickName)
                {
                    address = base.getSelectedAddress(id, DeliveryOptionType.Shipping, DropdownDSAddressNickName);
                }
            }
            return address;
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
                        ListItem item = deliveryType.Items.FindByValue("Shipping");
                        if (item != null)
                            item.Selected = true;
                        populateShipping();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        deliveryType.ClearSelection();
                        ListItem item = deliveryType.Items.FindByValue("Pickup");
                        if (item != null)
                            item.Selected = true;
                        populatePickup();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                    {
                        deliveryType.ClearSelection();
                        ListItem item = deliveryType.Items.FindByValue("ShipToCourier");
                        if (item != null)
                            item.Selected = true;
                        populateShipping();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        deliveryType.ClearSelection();
                        ListItem item = deliveryType.Items.FindByValue("PickupFromCourier");
                        if (item != null)
                            item.Selected = true;
                        PopulatePickupPreference();
                    }
                }
            }
            else
            {
                if (hasShippingAddresses)
                {
                    deliveryType.ClearSelection();
                    ListItem item = deliveryType.Items.FindByValue("Shipping");
                    if (item != null)
                        item.Selected = true;
                    populateShipping();
                }
                else
                {
                    string textSelect = (string)GetLocalResourceObject("TextSelect");
                    deliveryType.Items.Insert(0, new ListItem(textSelect, "0"));
                    deliveryType.Items[0].Selected = true;
                }
            }
        }

        #endregion CheckOutOptionsControlMethods

        #region EventHandlerDeliveryOptions

        protected void OnRGNumber_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                ShoppingCart.DeliveryInfo.RGNumber = txtRGNumber.Text;
            }
        }

        protected void OnCourierName_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart != null)
            {
                ShoppingCart.CourierName = txtCourierName.Text;
            }
        }

        new protected void OnNickName_Databind(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
                ShippingInfo deliveryInfo = ShoppingCart.DeliveryInfo;
                string textSelect = (string)GetLocalResourceObject("TextSelect");
                if (deliveryInfo != null)
                {
                    DeliveryOptionType deliveryOptionTypeFromDowndown = getDeliveryOptionTypeFromDropdown(DeliveryType);

                    if (CheckNoDeliveryType(DeliveryType) && deliveryOptionTypeFromDowndown == DeliveryOptionType.Unknown)
                    {
                        deliveryOptionTypeFromDowndown = DeliveryOptionType.Shipping;
                    }

                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
                        {
                            DeliveryOption deliveryOption = GetSelectedAddress(deliveryInfo.Address.ID, DeliveryOptionType.Shipping);
                            ListItem selected = ddl.Items.FindByValue(deliveryOption == null ? string.Empty : deliveryInfo.Address.ID.ToString());
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
                    if (deliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.ShipToCourier)
                        {
                            DeliveryOption deliveryOption = GetSelectedAddress(deliveryInfo.Address.ID, DeliveryOptionType.Shipping);
                            ListItem selected = ddl.Items.FindByValue(deliveryOption == null ? string.Empty : deliveryInfo.Address.ID.ToString());
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
                            if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Pickup)
                            {
                                var varPref = this._pickupLocations.Where(f => f.Id == deliveryInfo.Id);
                                if (varPref.Count() > 0)
                                {
                                    ListItem selected = ddl.Items.FindByValue(deliveryInfo.Id.ToString());
                                    if (selected != null)
                                    {
                                        ddl.ClearSelection();
                                        selected.Selected = true;
                                        DisableDeleteLink(varPref.First().IsPrimary);
                                    }
                                    else
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

        private string GetDefaultWarehouseFreightCode(List<DeliveryOption> lstDeliveryOption)
        {
            string defaultCode = string.Empty;
            if (null != lstDeliveryOption && lstDeliveryOption.Count > 0)
            {
                var varOption = lstDeliveryOption.Where(l => l.IsDefault == true);
                if (varOption.Count() > 0)
                {
                    defaultCode = varOption.First().FreightCode;
                }
                else
                {
                    defaultCode = lstDeliveryOption[0].FreightCode;
                }
            }

            return defaultCode;
        }

        private void SetWarehouseFreightCode(string freightCode)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                List<DeliveryOption> lstDeliveryOption =
                    ProductsBase.GetShippingProvider().GetDeliveryOptionsListForShipping(this.CountryCode, this.Locale, ShoppingCart.DeliveryInfo.Address);
                DeliveryOption selectedOption = lstDeliveryOption.Find(p => p.FreightCode.Trim().ToUpper() == freightCode.ToUpper().Trim());
                if (selectedOption != null)
                {
                    ShoppingCart.DeliveryInfo.WarehouseCode = selectedOption.WarehouseCode;
                    ShoppingCart.FreightCode = selectedOption.FreightCode;
                    ShoppingCart.DeliveryInfo.Name = selectedOption.Name;
                    ShoppingCart.DeliveryInfo.Id = selectedOption.Id;
                    ShoppingCart.DeliveryInfo.FreightCode = selectedOption.FreightCode;
                    ShoppingCart.OrderSubType = selectedOption.WarehouseCode;
                    ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                    ShoppingCart.Calculate();
                    OnBRQuoteRetrieved(this, null);
                }
            }
        }

        protected void ddlBRShippingMethod_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (ddlShippingMethod.SelectedValue != null)
                {
                    SetWarehouseFreightCode(ddlShippingMethod.SelectedValue);
                }
            }
            blErrors.Items.Clear();
        }

        new protected void ddlShippingMethod_OnDataBound(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;

            List<DeliveryOption> lstDeliveryOption = ProductsBase.GetShippingProvider().GetDeliveryOptionsListForShipping(CountryCode, Locale, ShoppingCart.DeliveryInfo.Address);
            if (ShoppingCart != null && ddl != null && ShoppingCart.DeliveryInfo != null)
            {
                if (ddl.Items.Count > 0)
                {
                    if (String.IsNullOrEmpty(ShoppingCart.FreightCode) && String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.FreightCode))
                    {
                        //If both are empty, get the default or first from the drop down and make it the freight code
                        if (lstDeliveryOption.Count > 0)
                        {
                            string defaultCode = GetDefaultWarehouseFreightCode(lstDeliveryOption);
                            ListItem liFreightCode = ddl.Items.FindByValue(defaultCode);
                            if (liFreightCode != null)
                            {
                                ddl.ClearSelection();
                                SetWarehouseFreightCode(defaultCode);
                                liFreightCode.Selected = true;
                            }
                            else
                            {
                                SetWarehouseFreightCode(ddl.SelectedItem.Value);
                            }
                        }
                    }
                    else if (!string.Equals(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.FreightCode))
                    {
                        //if both are not equal, make these invalid and get the default or first from the drop down and make it the freight code
                        if (lstDeliveryOption.Count > 0)
                        {
                            string defaultCode = GetDefaultWarehouseFreightCode(lstDeliveryOption);
                            ListItem liFreightCode = ddl.Items.FindByValue(defaultCode);
                            if (liFreightCode != null)
                            {
                                ddl.ClearSelection();
                                SetWarehouseFreightCode(defaultCode);
                                liFreightCode.Selected = true;
                            }
                            else
                            {
                                SetWarehouseFreightCode(ddl.SelectedItem.Value);
                            }
                        }
                    }
                    else if (string.Equals(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.FreightCode))
                    {
                        //search for this in the drop down, if exists select that, if not get the default or first from the drop down and make it the freight code
                        ListItem liFreightCode = ddl.Items.FindByValue(ShoppingCart.FreightCode);
                        if (liFreightCode != null)
                        {
                            ddl.ClearSelection();
                            SetWarehouseFreightCode(ShoppingCart.FreightCode);
                            liFreightCode.Selected = true;
                        }
                        //else
                        //{
                        //    if (!SessionInfo.IsEventTicketMode)
                        //    {
                        //        if (lstDeliveryOption.Count > 0)
                        //        {
                        //            string defaultCode = GetDefaultWarehouseFreightCode(lstDeliveryOption);
                        //            ListItem liFreightCodeDefault = ddl.Items.FindByValue(defaultCode);
                        //            if (liFreightCodeDefault != null)
                        //            {
                        //                ddl.ClearSelection();
                        //                SetWarehouseFreightCode(defaultCode);
                        //                liFreightCodeDefault.Selected = true;
                        //            }
                        //            else
                        //            {
                        //                SetWarehouseFreightCode(ddl.SelectedItem.Value);
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    if (ddl.Items.Count == 1)
                    {
                        trEditableShippingMethod.Visible = false;
                        trStaticShippingMethod.Visible = true;
                        lbShippingMethod.Text = ddl.SelectedItem == null ? string.Empty : ddl.SelectedItem.Text;
                    }
                    else
                    {
                        trEditableShippingMethod.Visible = true;
                        trStaticShippingMethod.Visible = false;
                    }
                }
            }
        }

        protected void OnDSAddressNickNameChanged(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;
            if (ShoppingCart != null && ddl.SelectedItem != null && ddl.SelectedItem.Value != "0")
            {
                int shippingAddressId = int.Parse(ddl.SelectedValue);
                DeliveryOption deliveryOption = getShippingAddressByID(shippingAddressId);
                if (deliveryOption != null)
                {
                    setAddressByNickName(deliveryOption, pDSAddress, DeliveryType);

                    if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
                    {
                        ShippingInfo deliveryInfo = ShoppingCart.DeliveryInfo;
                        string previousWarehouse = deliveryInfo.WarehouseCode;

                        pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, ShoppingCart != null && ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Description : string.Empty, true);
                        int deliveryOptionId = ShoppingCart.DeliveryInfo.Id; // pick up location ID
                        ShoppingCart.UpdateShippingInfo(shippingAddressId, deliveryOptionId, DeliveryOptionType.Pickup);

                        if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.WarehouseCode != previousWarehouse)
                        {
                            BrOnWarehouseChanged(this, null);
                        }
                        ShoppingCart.ShippingAddressID = shippingAddressId;
                    }
                }
            }
        }

        new protected void OnNickNameChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            DropDownList ddl = sender as DropDownList;
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                bool shippingOrShipToCourier = (deliveryOptionType == DeliveryOptionType.Shipping || deliveryOptionType == DeliveryOptionType.ShipToCourier);

                ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
                int deliveryOptionId = 0, shippingAddressId = 0;
                if (shippingInfo != null)
                {
                    shippingAddressId = shippingOrShipToCourier ? int.Parse(DropdownNickName.SelectedValue) : shippingInfo.Address.ID;
                    deliveryOptionId = shippingOrShipToCourier ? shippingInfo.Id : int.Parse(DropdownNickName.SelectedValue);
                }
                else
                {
                    shippingAddressId = shippingOrShipToCourier ? int.Parse(DropdownNickName.SelectedValue) : 0;
                    deliveryOptionId = shippingOrShipToCourier ? 0 : int.Parse(DropdownNickName.SelectedValue);
                }
                updateShippingInfo(shippingAddressId,
                        deliveryOptionId,
                        deliveryOptionType);
                shippingInfo = ShoppingCart.DeliveryInfo;
                if (shippingInfo != null)
                    pAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(new DeliveryOption(shippingInfo.Address), deliveryOptionType, ShoppingCart != null && ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.Description : string.Empty, true);
                else
                    pAddress.InnerHtml = "";

                if (shippingOrShipToCourier)
                {
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    LoadPickUpInstructions(IsStatic);
                    pInformation.InnerHtml = this.DisplayHoursOfOperationForPickup
                        ? ShoppingCart.DeliveryInfo.AdditionalInformation
                        : string.Empty;
                }
                lbShiptoOrPickup.Visible = true;
                pAddress.Visible = true;
                this.cntrlConfirmAddress.IsConfirmed = false;
            }
        }

        new protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            lblNickName.Visible = false;
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                //divDeliveryMethodShipping.Visible = deliveryType == DeliveryOptionType.Shipping;
                //divDeliveryMethodPickup.Visible = false;
                dvDSAddressEdit.Visible = false;

                //divDeliveryMethodShipToCourier.Visible = deliveryType == DeliveryOptionType.ShipToCourier;
                populateShipping();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping ? ShoppingCart.DeliveryInfo.Address : null));
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                dvDSAddressEdit.Visible = false;
                PopulatePickupPreference();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? ShoppingCart.DeliveryInfo.Address : null));
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = true;
                trPickupDetails.Visible = !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails;
                trPickupFromCourierDetails.Visible = HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails;
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }
            }
            else
            {
                populatePickup();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup ? ShoppingCart.DeliveryInfo.Address : null));
                trPickupDetails.Visible = true;
                trPickupFromCourierDetails.Visible = false;
                dvDSAddressEdit.Visible = true;
            }
            showShiptoOrPickup(IsStatic);
            if (null != DropdownNickName.SelectedItem)
            {
                lbShiptoOrPickup.Visible = DropdownNickName.SelectedItem.Value != "0";
            }
            this.cntrlConfirmAddress.IsConfirmed = false;
        }

        protected void EditDSAddressClicked(object sender, EventArgs e)
        {
            if (DropdownDSAddressNickName.SelectedItem != null)
            {
                DeliveryOption deliveryOption = getSelectedAddress(int.Parse(DropdownDSAddressNickName.SelectedValue), (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), DeliveryType.SelectedValue), DropdownDSAddressNickName);
                if (deliveryOption != null)
                {
                    Session["OQVOldaddress"] = deliveryOption;
                    AjaxControlToolkit.ModalPopupExtender mpAddAddress = null;
                    mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Edit, new ShippingAddressEventArgs(DistributorID, deliveryOption, false, ProductsBase.DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
            }
        }

        protected void DeleteDSAddressClicked(object sender, EventArgs e)
        {
            if (DropdownDSAddressNickName.SelectedItem != null)
            {
                DeliveryOption deliveryOption = getSelectedAddress(int.Parse(DropdownDSAddressNickName.SelectedValue), (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), DeliveryType.SelectedValue), DropdownDSAddressNickName);
                if (deliveryOption != null)
                {
                    AjaxControlToolkit.ModalPopupExtender mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");

                    if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup || getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
                    {
                        ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete, new ShippingAddressEventArgs(DistributorID, deliveryOption, false, ProductsBase.DisableSaveAddressCheckbox));
                    }
                    mpAddAddress.Show();
                }
            }
        }

        protected void AddDSAddressClicked(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup || getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
            {
                AjaxControlToolkit.ModalPopupExtender mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add, new ShippingAddressEventArgs(DistributorID, null, false, ProductsBase.DisableSaveAddressCheckbox));
                mpAddAddress.Show();
            }
        }

        #endregion EventHandlerDeliveryOptions

        #region SubscriptionEvents

        protected override void onProceedingToCheckout()
        {
            List<string> errors = new List<string>();

            try
            {
                #region Validations

                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDeliveryType"));
                }
                else if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    if (DropdownNickName.SelectedValue == "0")
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingNickNameNotPopulated"));
                    }

                    if (!APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale) && !this.ProductsBase.SessionInfo.IsEventTicketMode == true)
                    {
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                        {
                            if (ddlShippingMethod.Items.Count > 0)
                            {
                                if (ddlShippingMethod.SelectedValue == "0")
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoShippingMethodSelected"));
                                }
                            }
                        }

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveTime)
                        {
                            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsTimeMandatory)
                            {
                                if (ddlShippingTime.Items[0].Selected)
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoShippingTimeSelected"));
                                }
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null && ddlShippingTime.SelectedItem != null)
                                    ShoppingCart.DeliveryInfo.Instruction = ddlShippingTime.SelectedItem.Text;
                            }
                        }
                    }
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasFreeFormShippingInstruction)
                    {
                        if (!APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
                        {
                            if (ShoppingCart.DeliveryInfo != null && SessionInfo.IsEventTicketMode == false)
                            {
                                if (string.IsNullOrEmpty(txtShippingInstruction.Text))
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingInstructionNoFilled"));
                                }
                                else
                                {
                                    ShoppingCart.DeliveryInfo.Instruction = txtShippingInstruction.Text;
                                }
                            }
                        }
                    }
                }
                else if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.ShipToCourier)
                {
                    if (DropdownNickName.SelectedValue == "0")
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingNickNameNotPopulated"));
                    }
                    if (String.IsNullOrEmpty(txtCourierName.Text))
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCourierUpNameEntered"));
                    }
                    else
                    {
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            ShoppingCart.DeliveryInfo.Instruction = ShoppingCart.CourierName = txtCourierName.Text;
                        }
                    }

                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                    {
                        if (ShoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory != getDSMailingAddressState())
                        {
                            errors.Add((string)GetLocalResourceObject("lbPickupErrorMessage.Text"));
                        }
                        else if (ShoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory == "AM")
                        {
                            errors.Add((string)GetLocalResourceObject("lbPickupErrorMessage.Text"));
                        }
                    }
                }
                else // pickup  & pickupFromCourier
                {
                    if (DropdownNickName.SelectedValue == "0")
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated"));
                    }

                    if (!APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
                    {
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                        {
                            if (String.IsNullOrEmpty(txtPickupName.Text))
                            {
                                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SevenElevenCountry)
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpNameEnteredFor7-11"));
                                }
                                else
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpNameEntered"));
                                }
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null)
                                {
                                    ShoppingCart.DeliveryInfo.Address.Recipient = txtPickupName.Text.Trim();
                                }
                            }
                        }

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone)
                        {
                            string phone = txtPickupPhone.Text.Replace("_", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Replace("-", String.Empty).Trim();
                            if (String.IsNullOrEmpty(phone))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpPhoneEntered"));
                            }
                            else if (!System.Text.RegularExpressions.Regex.IsMatch(phone.ToString(), HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneRegExp))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null)
                                {
                                    ShoppingCart.DeliveryInfo.Address.Phone = txtPickupPhone.Text;
                                }
                            }
                        }

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate)
                        {
                            if (!pickupdateTextBox.SelectedDate.HasValue)
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpDateSelected"));
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null)
                                {
                                    ShoppingCart.DeliveryInfo.Instruction = Convert.ToDateTime(pickupdateTextBox.SelectedDate).ToString("d", CultureInfo.CurrentCulture);
                                    ShoppingCart.DeliveryInfo.PickupDate = pickupdateTextBox.SelectedDate.Value;
                                }
                            }
                        }

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                        {
                            if (ddlPickupTime.Items[0].Selected)
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpTimeSelected"));
                            }
                        }

                        // Pickup Instructions have RG Number
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber)
                        {
                            string rgNumber = txtRGNumber.Text;
                            if (String.IsNullOrEmpty(rgNumber))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoRGNumberSelected"));
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null)
                                {
                                    ShoppingCart.DeliveryInfo.RGNumber = rgNumber;
                                }
                            }
                        } // if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber)

                        if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup)
                        {
                            if (DropdownDSAddressNickName.SelectedValue == "0")
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoDistributorAddressEntered"));
                            }
                            else
                            {
                                //this.OnDSAddressNickNameChanged(DropdownDSAddressNickName, null);
                                //For Pick up the ship to state has to be the same as the one DS has in Country of Mailing in HMS.
                                //If the shipping address has a different state then DS will not be allowed to proceed a
                                //nd will have to change the shipping option to Shipping or change the address.
                                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.ShippingAddressID != 0)
                                {
                                    DeliveryOption deliveryOption = getShippingAddressByID(ShoppingCart.ShippingAddressID);
                                    if (deliveryOption != null)
                                    {
                                        if (deliveryOption.Address.StateProvinceTerritory != getDSMailingAddressState())
                                        {
                                            errors.Add((string)GetLocalResourceObject("lbPickupErrorMessage.Text"));
                                        }
                                    }
                                }
                                else
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                         "NoDistributorAddressEntered"));
                                }
                            }
                        }
                    }
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail && String.IsNullOrEmpty(txtEmail.Text.Trim()))
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoEmailAddress"));
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail && !isValidEmail(txtEmail.Text.Trim()))
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidEmailAddress"));
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.RequiresAddressConfirmation &&
                   !this.SessionInfo.IsEventTicketMode && !APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                {
                    if (!cntrlConfirmAddress.IsConfirmed)
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddressConfirmationRequired"));
                    }
                    cntrlConfirmAddress.SetConfirmAddressContentStyle();
                    this.SessionInfo.ConfirmedAddress = cntrlConfirmAddress.IsConfirmed;
                }

                #endregion Validations

                if (errors.Count > 0)
                {
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                    // Scroll to top.
                    var script = "setTimeout('window.scrollTo(0,0)', 100);";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script, true);
                    OnCheckOutOptionsNotPopulated(this, null);
                }
                else
                {
                    //Save Shipping/Pickup/Email Data before moving to check out

                    ShoppingCart.EmailAddress = txtEmail.Text.Trim();
                    SessionInfo.ChangedEmail = ShoppingCart.EmailAddress;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("BrCheckOutOptions - onProceedingToCheckout error : " + ex);
            }
            
        }

        private void reloadDistributorAddress()
        {
            DropdownDSAddressNickName.Items.Clear();
            loadDistributorAddress(false, DeliveryOptionType.Pickup);
        }

        public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                DeliveryOptionType selectedOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
                if (selectedOption == DeliveryOptionType.Shipping)
                {
                    base.onShippingAddressCreated(args);
                }
                else if (selectedOption == DeliveryOptionType.ShipToCourier)
                {
                    base.onShippingAddressCreated(args);
                    this.ShoppingCart.DeliveryInfo.Option = DeliveryOptionType.ShipToCourier;
                    populateDropdown();
                }
                else if (selectedOption == DeliveryOptionType.Pickup)
                {
                    this.reloadShipping();
                    //reloadDistributorAddress();
                    populateShipping(DropdownDSAddressNickName);
                    ListItem selected = DropdownDSAddressNickName.Items.FindByValue(args.ShippingAddress.ID.ToString());
                    if (selected != null)
                    {
                        DropdownDSAddressNickName.ClearSelection();
                        selected.Selected = true;
                        DeliveryOption deliveryOption = this.getShippingAddressByID(args.ShippingAddress.ID);
                        if (deliveryOption != null)
                        {
                            pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                            divDSAddress.Visible = true;
                            if (this.ShoppingCart != null && ShoppingCart.ShippingAddressID != args.ShippingAddress.ID)
                            {
                                ShoppingCart.ShippingAddressID = args.ShippingAddress.ID;
                                ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                            }
                        }

                        divDSAddressLink.Visible = true;
                        disableDSAddressLinkDelete(DropdownDSAddressNickName.Items.Count == 1);
                    }
                }
            }
        }

        private void disableDSAddressLinkDelete(bool disable)
        {
            this.DSAddressLinkDelete.Enabled = !disable;
        }

        public override void onShippingAddressDeleted(ShippingAddressEventArgs args)
        {
            DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryOptionType == DeliveryOptionType.Shipping || deliveryOptionType == DeliveryOptionType.ShipToCourier)
            {
               if (args != null)
                {
                    shippingAddressDeleted(args.ShippingAddress);
                    ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
                }
            
                if (DropdownNickName.SelectedIndex >= 0)
                {
                    int id = 0;
                    if (int.TryParse(DropdownNickName.SelectedValue, out id))
                    {
                        var deliveryOption = GetSelectedAddress(id,
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
             LoadShippingInstructions(IsStatic);
        
            }
            else if (deliveryOptionType == DeliveryOptionType.Pickup)
            {
                this.reloadShipping();
                //reloadDistributorAddress();
                populateShipping(DropdownDSAddressNickName);
                if (_shippingAddresses != null && _shippingAddresses.Count != 0)
                {
                    //After delete pass the primary shipping ID to update shipping info..
                    if (_shippingAddresses.Count > 0)
                    {
                        ListItem item = null;
                        DropdownDSAddressNickName.ClearSelection();
                        DeliveryOption primaryDeliveryOption = _shippingAddresses.Find(s => s.IsPrimary);
                        if (primaryDeliveryOption != null)
                        {
                            item = DropdownDSAddressNickName.Items.FindByValue(primaryDeliveryOption.ID.ToString());
                        }
                        else
                        {
                            if (DropdownDSAddressNickName.Items.Count > 0)
                            {
                                item = DropdownDSAddressNickName.Items[0];
                            }
                        }
                        if (item != null)
                        {
                            item.Selected = true;
                            int id = int.Parse(item.Value);
                            DeliveryOption deliveryOption = this.getShippingAddressByID(id);
                            if (deliveryOption != null)
                            {
                                pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                                if (this.ShoppingCart != null && ShoppingCart.ShippingAddressID != id)
                                {
                                    ShoppingCart.ShippingAddressID = id;
                                    ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                                }
                            }
                        }
                    }
                    else
                    {
                        ProductsBase.ClearCart();
                    }
                }
                else
                {
                    // no shipping address left
                    DropdownDSAddressNickName.ClearSelection();
                    DropdownDSAddressNickName.Attributes.Add("style", "display:none");
                    pDSAddress.InnerHtml = "";
                    divAddDSAddressLink.Visible = true;
                }
                showHideAddressLink();
            }

            ucShippingInfoControl.Hide();
        }

        new protected void DeleteClicked(object sender, EventArgs e)
        {
            var deliveryOptionSelected = getDeliveryOptionTypeFromDropdown(DeliveryType);
            Session["deliveryOptionSelected"] = deliveryOptionSelected;
            if (DropdownNickName.SelectedItem != null)
            {
                var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                var deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue), (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), DeliveryType.SelectedValue));
                var mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                if (deliveryOptionType == DeliveryOptionType.Shipping || deliveryOptionType == DeliveryOptionType.ShipToCourier)
                {
                    if (deliveryOption != null)
                    {
                        ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete, new ShippingAddressEventArgs(DistributorID, deliveryOption, false, ProductsBase.DisableSaveAddressCheckbox));
                        mpAddAddress.Show();
                    }
                }
                else
                {
                    ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
                    if (shippingInfo != null)
                    {
                        List<PickupLocationPreference_V01> pickupLocationPreferences = (this.Page as ProductsBase).GetShippingProvider().GetPickupLocationsPreferences((this.Page as ProductsBase).DistributorID, (this.Page as ProductsBase).CountryCode);
                        if (int.Parse(this.DropdownNickName.SelectedValue) != 0)
                        {
                            PickupLocationPreference_V01 pref = pickupLocationPreferences.Find(p => p.ID == int.Parse(this.DropdownNickName.SelectedValue));
                            if (pref != null)
                            {
                                ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete, new DeliveryOptionEventArgs(pref.PickupLocationID, shippingInfo.Name));
                            }
                        }
                        mpAddAddress.Show();
                    }
                }
            }
        }

        new protected void EditClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                DeliveryOption deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue), (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), DeliveryType.SelectedValue));
                var deliveryOptionSelected = getDeliveryOptionTypeFromDropdown(DeliveryType);
                if (deliveryOption != null)
                {
                    Session["OQVOldaddress"] = deliveryOption;
                    Session["deliveryOptionSelected"] = deliveryOptionSelected;
                    AjaxControlToolkit.ModalPopupExtender mpAddAddress = null;
                    mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Edit, new ShippingAddressEventArgs(DistributorID, deliveryOption, false, ProductsBase.DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
            }
        }

        new protected void AddAddressClicked(object sender, EventArgs e)
        {
            AjaxControlToolkit.ModalPopupExtender mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
            ucShippingInfoControl.ShowPopupForShipping(CommandType.Add, new ShippingAddressEventArgs(DistributorID, null, false, ProductsBase.DisableSaveAddressCheckbox));
            mpAddAddress.Show();
        }

        new protected void AddClicked(object sender, EventArgs e)
        {
            var deliveryOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var mpAddAddress = (AjaxControlToolkit.ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
            Session["deliveryOptionSelected"] = deliveryOption;
            if (deliveryOption == DeliveryOptionType.Shipping || deliveryOption == DeliveryOptionType.ShipToCourier)
            {
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add, new ShippingAddressEventArgs(DistributorID, null, false, ProductsBase.DisableSaveAddressCheckbox));
            }
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
            mpAddAddress.Show();
        }

        public override void onShippingAddressUpdated(ShippingAddressEventArgs args)
        {
            DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryOptionType == DeliveryOptionType.Pickup)
            {
                this.reloadShipping();
                //reloadDistributorAddress();
                ListItem selected = DropdownDSAddressNickName.Items.FindByValue(args.ShippingAddress.ID.ToString());
                if (selected != null)
                {
                    DropdownDSAddressNickName.ClearSelection();
                    selected.Selected = true;
                    DeliveryOption deliveryOption = this.getShippingAddressByID(args.ShippingAddress.ID);
                    if (deliveryOption != null)
                    {
                        pDSAddress.InnerHtml = ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption, DeliveryOptionType.Shipping, string.Empty, true);
                    }
                }
                showHideAddressLink();
                //divDSAddress.Visible = false;
                //divDSAddressLink.Visible = true;
            }
            else if (deliveryOptionType == DeliveryOptionType.Shipping || deliveryOptionType == DeliveryOptionType.ShipToCourier)
            {
                base.onShippingAddressUpdated(args);
                if (deliveryOptionType == DeliveryOptionType.ShipToCourier)
                {
                    this.ShoppingCart.PassDSFraudValidation = true;
                    this.ShoppingCart.DSFraudValidationError = string.Empty;
                }
            }
        }

        protected void PopulatePickupPreference()
        {
            DropdownNickName.Items.Clear();
            if (_pickupRrefList != null && _pickupRrefList.Count > 0)
            {
                DropdownNickName.DataSource = from p in _pickupRrefList
                                              select new
                                              {
                                                  DisplayName = p.PickupLocationNickname,
                                                  ID = p.ID
                                              };
                DropdownNickName.DataBind();
            }
            else
            {
                DropdownNickName.Visible = false;
                divshipToOrPickup.Visible = false;
            }
        }

        #endregion SubscriptionEvents
    }
}