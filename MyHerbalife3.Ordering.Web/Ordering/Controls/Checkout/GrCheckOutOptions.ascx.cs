using System;
using System.Linq;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Utilities;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using HL.Common.Configuration;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class GrCheckOutOptions : CheckOutOptions
    {
        #region Properties

        private string CourierTypeSelected
        {
            get { return ProductsBase.GetShippingProvider().GetCourierTypeBySelection(DeliveryType.SelectedValue ?? "PickupFromCourier"); }
        }

        #endregion

        #region ctor

        public GrCheckOutOptions()
        {

        }

        #endregion

        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnGrQuoteRetrieved;

        #region CheckOutOptionsControlMethods

        protected override void LoadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            if (ProductsBase.GetShippingProvider().HasAdditionalPickup())
            {
                _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                       ShippingProvider.GetDefaultAddress(), DeliveryType.SelectedValue);
            }
            else
            {
                _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                       ShippingProvider.GetDefaultAddress());
            }

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                {
                    _pickupRrefList = ProductsBase.GetShippingProvider()
                                                  .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                                 DeliveryOptionType.PickupFromCourier, null);
                }
                else
                {
                    _pickupRrefList = ProductsBase.GetShippingProvider()
                                                  .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                                 DeliveryOptionType.PickupFromCourier);
                }

                lblformatPickupPhone.Visible = true;
            }
        }

        protected override void ConfigureMenu()
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                if (DeliveryType.Items.FindByValue("PickupFromCourier") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
                if (DeliveryType.Items.FindByValue("PickupFromCourier1") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier1"));
            }
            else
            {
                if (!ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                {
                    if (DeliveryType.Items.FindByValue("PickupFromCourier1") != null)
                        DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier1"));
                }
                else
                {
                    // Validate if any specific courier type is disable
                    var courierType = ProductsBase.GetShippingProvider().GetCourierTypeBySelection("PickupFromCourier");
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisabledCourierType.Contains(courierType) &&
                        DeliveryType.Items.FindByValue("PickupFromCourier") != null)
                        DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
                    courierType = ProductsBase.GetShippingProvider().GetCourierTypeBySelection("PickupFromCourier1");
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisabledCourierType.Contains(courierType) &&
                        DeliveryType.Items.FindByValue("PickupFromCourier1") != null)
                        DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier1"));
                }
            }
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowShipToCourier &&
                DeliveryType.Items.FindByValue("ShipToCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("ShipToCourier"));
            }
            if (!ProductsBase.GetShippingProvider().HasAdditionalPickup())
            {
                if (DeliveryType.Items.FindByValue("Pickup1") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Pickup1"));
            }
             if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup &&
               DeliveryType.Items.FindByValue("Pickup") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Pickup"));
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
                        var item = deliveryType.Items.FindByValue("Shipping");
                        if (item != null)
                            item.Selected = true;
                        populateShipping();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        ListItem item = null;
                        deliveryType.ClearSelection();
                        item = deliveryType.Items.FindByValue("Pickup");

                        if (ProductsBase.GetShippingProvider().HasAdditionalPickup())
                        {
                            // Getting location for Pickup1 to validate the delivery option to select
                            var locations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                ShippingProvider.GetDefaultAddress(), "Pickup1");
                            if (ShoppingCart.DeliveryInfo != null)
                            {
                                if (locations.FirstOrDefault(l => l.Id == ShoppingCart.DeliveryInfo.Id) != null)
                                {
                                    item = deliveryType.Items.FindByValue("Pickup1");
                                }
                            }
                        }

                        if (item != null)
                            item.Selected = true;
                        populatePickup();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        deliveryType.ClearSelection();
                        ListItem item = null;

                        if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                        {
                            var pickupLocations =
                                ProductsBase.GetShippingProvider()
                                            .GetPickupLocationsPreferences(ShoppingCart.DistributorID,
                                                                           ShoppingCart.CountryCode, null);
                            var selectedLocation =
                                pickupLocations.FirstOrDefault(l => l.ID == ShoppingCart.DeliveryInfo.Id);

                            item = (selectedLocation != null &&
                                    !string.IsNullOrEmpty(selectedLocation.PickupLocationType) && selectedLocation.PickupLocationType == "Agency")
                                       ? deliveryType.Items.FindByValue("PickupFromCourier1")
                                       : deliveryType.Items.FindByValue("PickupFromCourier");
                        }
                        else
                        {
                            item = deliveryType.Items.FindByValue("PickupFromCourier");
                        }

                        if (item != null)
                            item.Selected = true;
                        LoadData();
                        populatePickupPreference();
                        //populateShipping();
                    }
                }
            }
            else
            {
                if (hasShippingAddresses)
                {
                    deliveryType.ClearSelection();
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

        protected override void populatePickup()
        {
            base.populatePickup();
            if (ProductsBase.GetShippingProvider().HasAdditionalPickup() && ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
            {
                var pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup, ShippingProvider.GetDefaultAddress(), "Pickup1");
                var selectedLocation = pickupLocations.FirstOrDefault(l => l.Id == ShoppingCart.DeliveryInfo.Id);
                if (selectedLocation != null)
                {
                    var listPickup = from d in pickupLocations
                                     select new
                                     {
                                         DisplayName = d.Description,
                                         ID = d.Id
                                     };

                    if (pickupLocations.Count == 1)
                    {
                        lblNickName.Visible = true;
                        lblNickName.Text = pickupLocations[0].Description;
                        setAddressByNickName(ShoppingCart.DeliveryInfo.Address, pAddress, DeliveryType);
                        DropdownNickName.Attributes.Add("style", "display:none");
                    }
                    else
                    {
                        lblNickName.Visible = false;
                        DropdownNickName.Attributes.Remove("style");
                    }

                    DropdownNickName.DataSource = listPickup;
                    DropdownNickName.DataBind();
                }
            }
        }

        protected override void BindDataForEditableView()
        {
            base.BindDataForEditableView();
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                if (_pickupRrefList.Count == 0)
                {
                    pAddress.InnerHtml = string.Empty;
                    pInformation.InnerHtml = string.Empty;
                }
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForCOP1)
            {
                var fragment = ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(DeliveryType.SelectedValue);
                if (!string.IsNullOrEmpty(fragment))
                {
                    ContentReader6.ContentPath = fragment;
                    ContentReader6.LoadContent();
                }
            }
        }

        protected override void BindDataForReadOnlyView()
        {
            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode)
                ShowTitle();
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
                        if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                        {
                            var pickupLocations =
                                ProductsBase.GetShippingProvider()
                                            .GetPickupLocationsPreferences(ShoppingCart.DistributorID,
                                                                           ShoppingCart.CountryCode, null);
                            var selectedLocation =
                                pickupLocations.FirstOrDefault(l => l.ID == ShoppingCart.DeliveryInfo.Id);

                            lblSelectedDeliveryType.Text = (selectedLocation != null && !string.IsNullOrEmpty(selectedLocation.PickupLocationType) && selectedLocation.PickupLocationType == "Agency")
                                                               ? (string)GetLocalResourceObject("DeliveryOptionType_PickupFromCourier1.Text")
                                                               : (string)GetLocalResourceObject("DeliveryOptionType_PickupFromCourier.Text");
                            if (selectedLocation != null)
                            {
                                lblNickName.Text = selectedLocation.PickupLocationNickname;
                            }
                            else
                            {
                                lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                                        ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                                        : ShoppingCart.DeliveryInfo.Address.Alias);
                            }
                        }
                        else
                        {
                            lblSelectedDeliveryType.Text =
                                (string) GetLocalResourceObject("DeliveryOptionType_PickupFromCourier.Text");
                            var courierLocation =
                                (from p in _pickupRrefList where p.ID == ShoppingCart.DeliveryInfo.Id select p)
                                    .FirstOrDefault();
                            if (courierLocation != null)
                            {
                                lblNickName.Text = courierLocation.PickupLocationNickname;
                            }
                            else
                            {
                                lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                                        ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                                        : ShoppingCart.DeliveryInfo.Address.Alias);
                            }
                        }
                        break;
                    case DeliveryOptionType.Shipping:
                        lblSelectedDeliveryType.Text =
                            (string) GetLocalResourceObject("DeliveryOptionType_Shipping.Text");
                        lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                                ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                                : ShoppingCart.DeliveryInfo.Address.Alias);
                        break;
                    case DeliveryOptionType.ShipToCourier:
                        lblSelectedDeliveryType.Text =
                            (string) GetLocalResourceObject("DeliveryOptionType_ShipToCourier.Text");
                        lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                                ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                                : ShoppingCart.DeliveryInfo.Address.Alias);
                        break;
                }

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
                    var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
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
            else if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
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
                if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                {

                    var couriers = ProductsBase.GetShippingProvider()
                                               .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                              DeliveryOptionType.PickupFromCourier,
                                                                              CourierTypeSelected);
                    DropdownNickName.DataSource = from p in couriers
                                                  select new
                                                      {
                                                          DisplayName = p.PickupLocationNickname,
                                                          p.ID
                                                      };
                }
                else
                {
                    DropdownNickName.DataSource = from p in _pickupRrefList
                                                  select new
                                                      {
                                                          DisplayName = p.PickupLocationNickname,
                                                          p.ID
                                                      };
                }
                DropdownNickName.Visible = true;
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

        protected override void RenderEditableView()
        {
            base.RenderEditableView();
            trEditableShippingMethodForPickup.Visible = true;
            trStaticShippingMethodForPickup.Visible = false;
        }

        protected override void RenderReadOnlyView()
        {
            base.RenderReadOnlyView();
            trEditableShippingMethodForPickup.Visible = false;
            trStaticShippingMethodForPickup.Visible = true;
        }
        #endregion

        #region DeliveryOptionMethods

        protected override void showHideAddressLink()
        {
            var deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            lnAddAddress.Visible = false;
            if (deliveryType == DeliveryOptionType.Unknown)
            {
                lnAddAddress.Visible = true;
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
               lnAddAddress.Visible = ProductsBase.GetShippingProvider().NeedEnterAddress(DistributorID, Locale) &&
                                       ShoppingCart.DeliveryInfo == null;
                Label3.Visible = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.MessageNotify;
                lblMessage.Visible = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowNotification;
            }
            else if (deliveryType == DeliveryOptionType.ShipToCourier || deliveryType == DeliveryOptionType.Shipping)
            {
                lnAddAddress.Visible = _shippingAddresses.Count == 0;
                
            }
            else if (deliveryType == DeliveryOptionType.Pickup)
            {
                Label3.Visible = true;
                lblMessage.Visible = false;
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
                this.LinkAdd.Text = GetLocalResourceObject("LinkAddResource1.Text") as string;
                divLinkEdit.Visible = divLinkDelete.Visible = divLinks.Visible = true;
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
                divLinkEdit.Visible = false;
                divLinkDelete.Visible = (_pickupRrefList.Count > 0 && DropdownNickName.SelectedValue != "0");
                divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = true;
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

            pInformation.InnerHtml = string.Empty;
            pInformation2.InnerHtml = string.Empty;
            pInformation2.Visible = false;
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
                    trPickupDetails.Visible = (selectedOption == DeliveryOptionType.Pickup || !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                    trPickupFromCourierDetails.Visible = (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                    LoadPickUpInstructions(IsStatic);
                    pInformation.InnerHtml = ProductsBase.GetShippingProvider().DisplayHoursOfOperation(selectedOption) &&
                                             ShoppingCart.DeliveryInfo != null 
                                                 ? ShoppingCart.DeliveryInfo.AdditionalInformation
                                                 : string.Empty;
                    if (selectedOption == DeliveryOptionType.PickupFromCourier)
                    {
                        string additionalInformationDesc = GetLocalResourceObject("AdditionalInformationDescription") as string;
                        if (!string.IsNullOrEmpty(additionalInformationDesc))
                        {
                            pInformation2.Visible = true;
                            pInformation2.InnerHtml = additionalInformationDesc;
                        }
                    }
                }
                if (selectedOption == DeliveryOptionType.PickupFromCourier && (CountryCode == "US" || CountryCode == "CA" || CountryCode == "PR" || CountryCode == "TH"))
                {
                    this.LinkAdd.Text = GetLocalResourceObject("ChangeFedexResource") as string;
                }
                if (selectedOption == DeliveryOptionType.PickupFromCourier && CountryCode == "TH")
                {
                    PickupName.Text = GetLocalResourceObject("7-11 PickUpName.Text") as string;
                    PickUpnameStatic.Text = GetLocalResourceObject("7-11 PickUpName.Text") as string;
                    PickupPhone.Text = GetLocalResourceObject("7-11 PickUpPhone.Text") as string;
                    PickUpPhoneStatic.Text = GetLocalResourceObject("7-11 PickUpPhone.Text") as string;
                    lblMobileNumberReadOnly.Text = GetLocalResourceObject("7-11SMSPhoneNumber.Text") as string;
                    lblMobileNumberNew.Text = GetLocalResourceObject("7-11SMSPhoneNumber.Text") as string;
                }
                else
                {
                    PickupName.Text = GetLocalResourceObject("PickupNameResource1.Text") as string;
                    PickUpnameStatic.Text = GetLocalResourceObject("PickupNameResource1.Text") as string;
                    PickupPhone.Text = GetLocalResourceObject("PickupPhoneResource1.Text") as string;
                    PickUpPhoneStatic.Text = GetLocalResourceObject("PickupPhoneResource1.Text") as string;
                    lblMobileNumberReadOnly.Text = GetLocalResourceObject("lblMobileNumber.text") as string;
                    lblMobileNumberNew.Text= GetLocalResourceObject("lblMobileNumber.text") as string;
                }
            }
        }

        #endregion

        #region DeliveryInstructionsMethods

        protected override void LoadPickUpInstructions(bool IsStatic)
        {
            var selectedOption = ShoppingCart.DeliveryInfo != null
                                     ? ShoppingCart.DeliveryInfo.Option
                                     : getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (selectedOption == DeliveryOptionType.Pickup || selectedOption == DeliveryOptionType.PickupFromCourier)
            {
                if (selectedOption == DeliveryOptionType.Pickup &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                if (selectedOption == DeliveryOptionType.PickupFromCourier &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveName &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplayPickupFromCourierInstructions)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                TextShippingMethod.Text = (string) GetLocalResourceObject("PickUpBy");
                divDeliveryTime.Visible = false;

                if (!IsStatic)
                {
                    if ((selectedOption == DeliveryOptionType.Pickup &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate))
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
                        trEditableDatePicker.Visible = true;
                    }
                    else
                    {
                        trEditableDatePicker.Visible = false;
                    }

                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveName))
                    {
                        txtPickupName.Attributes.Add("maxlength", "36");
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                            {
                                txtPickupName.Text = ShoppingCart.DeliveryInfo.Address.Recipient;
                            }
                        }
                        trEditablePickupName.Visible = true;
                    }
                    else
                    {
                        trEditablePickupName.Visible = false;
                    }

                    // Pick up Phone
                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone))
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
                        trEditablePickupPhone.Visible = true;
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
                                var li = ddlPickupTime.Items.FindByText(ShoppingCart.DeliveryInfo.Instruction);
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

                    // This option applies only for pickup from courier
                    if (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupMethodHaveDropDown)
                    {
                        tbShippingMethodForPickup.Visible = true;
                        PopulateShippingMethodsForPickupDropDown();
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Name))
                            {
                                ddlShippingMethodPickup.ClearSelection();
                                var li = ddlShippingMethod.Items.FindByValue(ShoppingCart.DeliveryInfo.Name);
                                if (li != null)
                                {
                                    li.Selected = true;
                                }
                                else if (ddlShippingMethodPickup.Items.Count > 0)
                                {
                                    ddlShippingMethodPickup.Items[0].Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        tbShippingMethodForPickup.Visible = false;
                    }
                }
                else // if (!IsStatic)
                {
                    if ((selectedOption == DeliveryOptionType.Pickup &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate))
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

                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveName))
                    {
                        lbPickupname.Text = ShoppingCart.DeliveryInfo != null
                                                ? ShoppingCart.DeliveryInfo.Address.Recipient
                                                : String.Empty;
                    }
                    else
                    {
                        trStaticPickupName.Visible = false;
                    }

                    if ((selectedOption == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone) ||
                        (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone))
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

                    // This option applies only for pickup from courier
                    if (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupMethodHaveDropDown)
                    {
                        tbShippingMethodForPickup.Visible = true;
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            var lstDeliveryOption =
                                ProductsBase.GetShippingProvider()
                                            .GetDeliveryOptionsListForPickup(CountryCode, Locale,
                                                                             ShoppingCart.DeliveryInfo.Address);
                            if (lstDeliveryOption.Count > 0)
                            {
                                var option =
                                    lstDeliveryOption.Find(p => p.FreightCode == ShoppingCart.FreightCode);
                                if (option != null)
                                {
                                    lbShippingMethodPickup.Text = option.Description;
                                }
                            }
                        }
                    }
                    else
                    {
                        tbShippingMethodForPickup.Visible = false;
                    }
                }
            }
        }

        private void PopulateShippingMethodsForPickupDropDown()
        {
            ddlShippingMethodPickup.Visible = false;
            if (ShoppingCart.DeliveryInfo != null)
            {
                ddlShippingMethodPickup.Items.Clear();

                var lstDeliveryOption = ProductsBase.GetShippingProvider()
                                                    .GetDeliveryOptionsListForPickup(CountryCode, Locale,
                                                                                     ShoppingCart.DeliveryInfo.Address);
                if (lstDeliveryOption != null && lstDeliveryOption.Count > 0)
                {
                    ddlShippingMethodPickup.DataSource = from l in lstDeliveryOption
                                                         select new
                                                             {
                                                                 FreightCode = l.FreightCode,
                                                                 Description = l.Description
                                                             };
                    ddlShippingMethodPickup.DataBind();
                    ddlShippingMethodPickup.SelectedIndex = 0;
                    //when return from checkout page the 
                    //ddl is refreshed but not the freightcode selected
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        if (ddlShippingMethodPickup.SelectedValue != null)
                        {
                            SetFreightCode(ddlShippingMethodPickup.SelectedValue);
                        }
                    }
                    
                    ddlShippingMethodPickup.Visible = true;
                }
            }
        }

        #endregion

        #region EventHandlerDeliveryOptions

        protected new void OnNickNameChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();

            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var ddl = sender as DropDownList;
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);

                lbShiptoOrPickup.Visible = true;

                if (deliveryOptionType == DeliveryOptionType.Shipping)
                {
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    LoadPickUpInstructions(IsStatic);
                    pInformation.InnerHtml = ProductsBase.GetShippingProvider().DisplayHoursOfOperation(deliveryOptionType) && ShoppingCart.DeliveryInfo != null
                                                 ? ShoppingCart.DeliveryInfo.AdditionalInformation
                                                 : string.Empty;
                }

                cntrlConfirmAddress.IsConfirmed = false;
                if (pickUpAddInfoHeader != null && deliveryOptionType != DeliveryOptionType.Shipping)
                {
                    pickUpAddInfoHeader.Visible = true;
                }
            }
            if (deliveryOptionType == DeliveryOptionType.PickupFromCourier)
            {
                divLinkEdit.Visible = false;
                divLinkDelete.Visible = (_pickupRrefList.Count > 0 && DropdownNickName.SelectedValue != "0");
            }
            divLinks.Visible = ((deliveryOptionType == DeliveryOptionType.Shipping ||
                                 deliveryOptionType == DeliveryOptionType.PickupFromCourier) &&
                                DropdownNickName.SelectedValue != "0");
        }

        protected new void OnNickName_Databind(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
                var deliveryInfo = ShoppingCart.DeliveryInfo;
                var textSelect = (string) GetLocalResourceObject("TextSelect");
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
                            var deliveryOption = GetSelectedAddress(deliveryInfo.Address.ID,
                                                                    DeliveryOptionType.Shipping);
                            var selected =
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
                                    var selected = ddl.Items.FindByValue(deliveryInfo.Id.ToString());
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
                                    var selected = ddl.Items.FindByValue(varPref.First().ID.ToString());
                                    if (selected != null)
                                    {
                                        ddl.ClearSelection();
                                        selected.Selected = true;
                                    }
                                }
                            }
                        }
                        else if (deliveryOptionTypeFromDowndown == DeliveryOptionType.PickupFromCourier)
                        {
                            int pickupLocationId = 0;
                            if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                            {
                                var pickupLocations = ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(ShoppingCart.DistributorID, ShoppingCart.CountryCode, null);
                                var selectedLocation =
                                    pickupLocations.FirstOrDefault(l => l.ID == ShoppingCart.DeliveryInfo.Id);

                                if (selectedLocation != null) pickupLocationId = selectedLocation.ID;
                            }
                            else
                            {
                                var varPref = _pickupRrefList.Where(f => f.ID == deliveryInfo.Id);
                                if (varPref.Count() > 0)
                                {
                                    pickupLocationId = varPref.First().ID;
                                }
                            }

                            ListItem selected = ddl.Items.FindByValue(pickupLocationId.ToString());
                            if (selected != null)
                            {
                                ddl.ClearSelection();
                                selected.Selected = true;
                            }
                            setAddressByNickName(
                                deliveryInfo.Address == null ? null : ShoppingCart.DeliveryInfo.Address,
                                pAddress, DeliveryType);
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
            LoadData();
            blErrors.Items.Clear();
            lblNickName.Visible = false;
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            var deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (pickUpAddInfoHeader != null)
            {
                pickUpAddInfoHeader.Visible = true;
            }
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                this.LinkAdd.Text = GetLocalResourceObject("LinkAddResource1.Text") as string;
                ShoppingCart.DeliveryInfo = null;
                showHideAddressLink();
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
                trPickupDetails.Visible = (deliveryType == DeliveryOptionType.Pickup || !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                trPickupFromCourierDetails.Visible = (deliveryType == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }
                if (CountryCode == "US" || CountryCode == "CA" || CountryCode == "PR" || CountryCode == "TH")
                {
                    this.LinkAdd.Text = GetLocalResourceObject("ChangeFedexResource") as string;
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
                trPickupDetails.Visible = true;
                trPickupFromCourierDetails.Visible = false;
                
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }
            }
            //Bug 174692: Hours of operation
            if (pickUpAddInfoHeader != null && (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.Pickup))
            {
                pickUpAddInfoHeader.Visible = false;
            }
            showShiptoOrPickup(IsStatic);
            if (null != DropdownNickName.SelectedItem)
            {
                lbShiptoOrPickup.Visible = DropdownNickName.SelectedItem.Value != "0";
            }
            cntrlConfirmAddress.IsConfirmed = false;
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForCOP1)
            {
                var fragment = ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(DeliveryType.SelectedValue);
                if (!string.IsNullOrEmpty(fragment))
                {
                    ContentReader6.ContentPath = fragment;
                    ContentReader6.LoadContent();
                }
            }
        }

        protected new void DeleteClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                string selectedValue = ("PickupFromCourier1" == DeliveryType.SelectedValue) ? "PickupFromCourier" : DeliveryType.SelectedValue;
                var deliveryOption =  GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                        (DeliveryOptionType)
                                                        Enum.Parse(typeof (DeliveryOptionType),
                                                                   selectedValue));

                DeliveryOptionType deliveryType =  getDeliveryOptionTypeFromDropdown(DeliveryType);
                if (deliveryType == DeliveryOptionType.Shipping)
                {
                    if (deliveryOption != null)
                    {
                        var mpAddAddress =
                            (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                        ucShippingInfoControl.ShowPopupForShipping(CommandType.Delete,
                                                                   new ShippingAddressEventArgs(DistributorID,
                                                                                                deliveryOption, false,
                                                                                                ProductsBase
                                                                                                    .DisableSaveAddressCheckbox));
                        mpAddAddress.Show();
                    }
                }
                else
                {
                    var shippingInfo = ShoppingCart.DeliveryInfo;
                    if (shippingInfo != null)
                    {
                        var mpAddAddress =
                            (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                        var pickupLocationPreferences = this.CountryCode == "CL" ?
                            (Page as ProductsBase).GetShippingProvider()
                                                  .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                                 (Page as ProductsBase).CountryCode, Locale, deliveryType, this.CourierTypeSelected)
                            :
                            (Page as ProductsBase).GetShippingProvider()
                                                 .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, deliveryType);

                        if (int.Parse(DropdownNickName.SelectedValue) != 0)
                        {
                            var pref =
                                pickupLocationPreferences.Find(p => p.ID == int.Parse(DropdownNickName.SelectedValue));
                            if (pref != null)
                            {
                                var args = new DeliveryOptionEventArgs(pref.PickupLocationID, shippingInfo.Name);
                                args.CourierType = CourierTypeSelected;
                                ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete, args);
                            }
                        }
                        mpAddAddress.Show();
                    }
                }
            }
        }

        protected new void AddAddressClicked(object sender, EventArgs e)
        {
            var deliveryOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var mpAddAddress = (ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
            if (deliveryOption == DeliveryOptionType.Shipping || deliveryOption == DeliveryOptionType.Unknown)
            {
                
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
            }
            else if (deliveryOption == DeliveryOptionType.PickupFromCourier)
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(CourierTypeSelected));
            }
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
            mpAddAddress.Show();
        }

        protected new void AddClicked(object sender, EventArgs e)
        {
            var deliveryOption = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var mpAddAddress = (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
            if (deliveryOption == DeliveryOptionType.Shipping || deliveryOption == DeliveryOptionType.Unknown)
            {
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
            }
            else if (deliveryOption == DeliveryOptionType.PickupFromCourier)
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(CourierTypeSelected));
            }
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
            mpAddAddress.Show();
        }

        #endregion

        #region SubscriptionEvents

        //public override void onShippingAddressDeleted(ShippingAddressEventArgs args)
        //{
        //    base.onShippingAddressDeleted(args);
        //}

        //public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        //{
        //    base.onShippingAddressCreated(args);
        //}

        public override void OnPickupPreferenceCreated(DeliveryOptionEventArgs args)
        {
            if (args != null)
            {
                _pickupRrefList =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode, Locale, DeliveryOptionType.PickupFromCourier, null);
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId,
                                   DeliveryOptionType.PickupFromCourier);
                populateDropdown();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                showShiptoOrPickup(IsStatic);
                //ppShippingInfoControl.Update();
            }
        }

        public override void OnPickupPreferenceDeleted(DeliveryOptionEventArgs args)
        {
            if (args != null)
            {

                _pickupRrefList =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode, Locale, DeliveryOptionType.PickupFromCourier, null);
                if (_pickupRrefList.Count > 0)
                {
                    if (string.IsNullOrEmpty(args.CourierType))
                    {
                        var pref = _pickupRrefList.First();
                        updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
                    }
                    else
                    {
                        if (_pickupRrefList.Where(p => p.Version == args.CourierType).Count() > 0)
                        {
                            var pref = _pickupRrefList.Where(p => p.Version == args.CourierType).First();
                            updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
                        }
                        else
                        {
                            DeliveryType.ClearSelection();
                            DeliveryType.SelectedIndex = -1;
                            ShoppingCart.DeliveryInfo = null;
                        }
                    }
                }
                else
                {
                    DeliveryType.ClearSelection();
                    DeliveryType.SelectedIndex = -1;
                    ShoppingCart.DeliveryInfo = null;
                }
                populateDropdown();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                showShiptoOrPickup(IsStatic);

                if (_pickupRrefList.Count == 0)
                {
                    setAddressByNickName(null);
                }
            }
        }

        #endregion SubscriptionEvents

        protected void ddlShippingMethodPickup_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (ddlShippingMethodPickup.SelectedValue != null)
                {
                    SetFreightCode(ddlShippingMethodPickup.SelectedValue);
                }
            }
        }

        protected void ddlShippingMethodPickup_OnDataBound(object sender, EventArgs e)
        {
            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null && ddlShippingMethodPickup != null)
            {
                if (ddlShippingMethodPickup.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.FreightCode))
                    {
                        SetFreightCode(ddlShippingMethodPickup.SelectedItem.Value);
                    }
                    if (ddlShippingMethodPickup.Items.Count == 1)
                    {
                        trEditableShippingMethod.Visible = false;
                        trStaticShippingMethod.Visible = true;
                        lbShippingMethod.Text = ddlShippingMethodPickup.SelectedItem == null ? string.Empty : ddlShippingMethodPickup.SelectedItem.Text;
                    }
                    else
                    {
                        trEditableShippingMethod.Visible = true;
                        trStaticShippingMethod.Visible = false;
                    }
                }
            }
        }

        private void SetFreightCode(string freightCode)
        {
            if (!string.IsNullOrEmpty(freightCode) && freightCode != "0")
            {
                if (freightCode != ShoppingCart.DeliveryInfo.FreightCode ||
                    freightCode != ShoppingCart.FreightCode)
                {
                    ShoppingCart.FreightCode = freightCode;
                    ShoppingCart.DeliveryInfo.FreightCode = freightCode;
                    ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                    ShoppingCart.Calculate();
                    OnGrQuoteRetrieved(this, null);
                }
            }
            else
            {
                ShoppingCart.FreightCode = string.Empty;
                ShoppingCart.DeliveryInfo.FreightCode = string.Empty;
            }
        }
    }
}