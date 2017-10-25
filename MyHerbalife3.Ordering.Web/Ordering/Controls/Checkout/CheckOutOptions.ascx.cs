using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.UI.Helpers;
using Telerik.Web.UI;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.SharedProviders;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckOutOptions : OrderQuickViewBase
    {
        public static List<string> PickupTinCodeDisplayCountries =
            new List<string>(ConfigurationManager.AppSettings["PickupTinCodeDisplayCountries"].Split(new[] {','}));

        public CheckOutOptions()
        {
            DisplayHoursOfOperationForPickup = true;
        }

        public bool DisplayHoursOfOperationForPickup { get; set; }

        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnQuoteRetrieved;

        [Publishes(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        public event EventHandler OnCheckOutOptionsNotPopulated;

        [Publishes(MyHLEventTypes.NotifyTodaysMagazineAddressChanged)]
        public event EventHandler NotifyTodaysMagazineAddressChanged;

        [Publishes(MyHLEventTypes.NotifyTodaysMagazineRecalculate)]
        public event EventHandler NotifyTodaysMagazineRecalculate;

        public void ValidateInvoiceAddress()
        {
            CorrectAddress();
        }

        protected virtual void LoadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                   ShippingProvider.GetDefaultAddress());

            if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
            {
                _pickupRrefList = ProductsBase.GetShippingProvider()
                                              .GetPickupLocationsPreferences(DistributorID, CountryCode);
            }
        }

        protected virtual void ConfigureMenu()
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadData();
            ConfigureMenu();
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID &&
                trEditablePickUpTinCode != null)
            {
                trEditablePickUpTinCode.Visible = false;
                trStaticPickUpTinCode.Visible = false;
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone && !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickUpPhoneHasPhoneMask)
            {
                txtPhoneNumber_MaskedEditExtender.Enabled = false;
                txtPhoneNumber_MaskedEditExtender.CultureName = "en-US";
            }
            if ((!HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket &&
                 SessionInfo.IsEventTicketMode) ||
                (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowShippingMethodForAPFETO &&
                 APFDueProvider.ShouldHideOrderQuickView(ShoppingCart)))
            {
                DeliveryOptionsView.Visible = false;
                DeliveryOptionsInstructionsView.Visible = false;
            }

            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay &&
                !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup)
            {
                DeliveryOptionsInstructionsView.Visible = false;
            }
            else if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
            {
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                }
            }

            if (DeliveryTerms != null)
            {
                DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
            }

            // Hide shipping method option is this flag is true.
            // ETO is for shipping by default
            if (!HLConfigManager.Configurations.DOConfiguration.AllowShipping &&
                ShoppingCart.OrderCategory != OrderCategoryType.ETO &&
                DeliveryType.Items.FindByValue("Shipping") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Shipping"));
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

            if (!IsPostBack)
            {
                var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                if (IsStatic || (HLConfigManager.Configurations.DOConfiguration.IsChina &&
                    ShoppingCart.CartItems.Count == 0 && OrderTotals != null && OrderTotals.Donation > Decimal.Zero))
                {
                    RenderReadOnlyView();
                    BindDataForReadOnlyView();
                }
                else
                {
                    RenderEditableView();
                    BindDataForEditableView();
                }
            }
          
            if ((!HLConfigManager.Configurations.DOConfiguration.IsChina) && (ShoppingCart.OrderCategory == OrderCategoryType.ETO || SessionInfo.IsEventTicketMode))
            {
                DeliveryOptionsInstructionsView.Visible =
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionForEventTicket;

                // Displaying the only allowed delivery type if specified for ETO
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedETO))
                {
                    //Same logic that of APF is applied for ETO to kick in the delivertype and nick name changed events
                    var deliveryOptionsToRemove = DeliveryType.Items.Cast<ListItem>()
                            .Where(i => i.Value != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedETO)
                            .ToList();

                    if (deliveryOptionsToRemove.Any())
                    {
                        var selected = 0;
                        if (this.ShoppingCart != null && this.ShoppingCart.DeliveryInfo != null)
                        {
                            selected = this.ShoppingCart.DeliveryInfo.Id;
                        }

                        deliveryOptionsToRemove.ForEach(DeliveryType.Items.Remove);
                        if (DropdownNickName.Items.FindByValue(selected.ToString(CultureInfo.InvariantCulture)) != null &&
                           selected > 0)
                        {
                            DropdownNickName.SelectedValue = selected.ToString(CultureInfo.InvariantCulture);
                            OnNickNameChanged(sender, e);
                        }
                    }

                }
            }

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO && SessionInfo.IsHAPMode)
            {
                //DeliveryOptionsInstructionsView.Visible =
                //    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionForEventTicket;

                // Displaying the only allowed delivery type if specified for HAP
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedHAP))
                {
                    //Same logic that of APF is applied for ETO to kick in the delivertype and nick name changed events
                    var deliveryOptionsToRemove = DeliveryType.Items.Cast<ListItem>()
                            .Where(i => i.Value != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryAllowedHAP)
                            .ToList();

                    if (deliveryOptionsToRemove.Any())
                    {
                        var selected = 0;
                        if (this.ShoppingCart != null && this.ShoppingCart.DeliveryInfo != null)
                        {
                            selected = this.ShoppingCart.DeliveryInfo.Id;
                        }

                        deliveryOptionsToRemove.ForEach(DeliveryType.Items.Remove);
                        if (DropdownNickName.Items.FindByValue(selected.ToString(CultureInfo.InvariantCulture)) != null &&
                           selected > 0)
                        {
                            DropdownNickName.SelectedValue = selected.ToString(CultureInfo.InvariantCulture);
                            OnNickNameChanged(sender, e);
                        }
                    }
                }
            }

            if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) &&
                !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowShippingMethodForAPFETO)
            {
                DeliveryOptionsInstructionsView.Visible = false;
            }
            if (DeliveryTerms != null)
            {
                DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
            }

            // Show confirmation address if the page is Shopping Cart (CheckOut 1)
            // Taking care about delivery options
            var showAddressValidation = Page.GetType().Name.Equals("ordering_shoppingcart_aspx") &&
                                        HLConfigManager.Configurations.CheckoutConfiguration.RequiresAddressConfirmation &&
                                        DeliveryOptionsView.Visible;
            cntrlConfirmAddress.SetVisibility(showAddressValidation);

            if (!IsPostBack)
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

                switch (selectedOption)
                {
                    case DeliveryOptionType.Shipping:
                        if (pickUpAddInfoHeader != null)
                        {
                            pickUpAddInfoHeader.Visible = false;
                        }
                        break;
                    case DeliveryOptionType.Pickup:
                        if (pickUpAddInfoHeader != null && DropdownNickName.SelectedIndex < 0)
                        {
                            pickUpAddInfoHeader.Visible = false;
                        }
                        break;
                    default:
                        if (pickUpAddInfoHeader != null)
                        {
                            pickUpAddInfoHeader.Visible = true;
                        }
                        break;
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                TextShippingMethod.Text = (string)GetLocalResourceObject("HAPShippingInstructionTitle");
                ContentReader2.ContentPath = "shippingmethod-hap.html";
                trEditableShippingMethod.Visible = false;
            }
            if (!IsPostBack
                && HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasSpecialEventWareHouse
                && ShoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse 
                && Locale.Substring(3).Equals("MX"))
            {
                ContentReader1.ContentPath = "pickupdetails_specialEvents.html";
                ContentReader1.LoadContent();
            }

            if (!IsPostBack)
            {
                var savedCopyError = (Page as ProductsBase).GetSaveCopyError();
                if (!string.IsNullOrEmpty(savedCopyError))
                {
                    var errors = new List<string>
                        {
                            savedCopyError
                        };
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                }
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasHerbalifePickupFreightChnage)
            {
                this.cbFreightchanges.Checked = ShoppingCart.HerbalifeFastPickup;
                cbFreightchanges.ForeColor = System.Drawing.Color.Red;
            }
            if (HL.Common.Configuration.Settings.GetRequiredAppSetting("polynesiaSentence", "fr-PF").Contains(Locale))
                divSentence.Visible = divSentence != null && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping && ShoppingCart.OrderCategory == OrderCategoryType.RSO ? true : false;
        }

        #region CheckOutOptionsControlMethods

        protected virtual void BindDataForReadOnlyView()
        {
            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode)
                ShowTitle();
            showShiptoOrPickup(IsStatic);
            if (ShoppingCart.DeliveryInfo != null)
            {
                lblSelectedDeliveryType.Text = ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                   ? (string) GetLocalResourceObject("DeliveryOptionType_Shipping")
                                                   : (string) GetLocalResourceObject("DeliveryOptionType_Pickup");
                if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                            ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                            : ShoppingCart.DeliveryInfo.Address.Alias);
                }
                else
                {
                    if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                    {
                        lblNickName.Text = ShoppingCart.DeliveryInfo.Description;
                    }
                    else
                    {
                        var pickupLocation =
                            (from p in _pickupRrefList where p.PickupLocationID == ShoppingCart.DeliveryInfo.Id select p)
                                .FirstOrDefault();
                        if (pickupLocation != null)
                        {
                            lblNickName.Text = pickupLocation.PickupLocationNickname;
                        }
                        else
                        {
                            lblNickName.Text = ShoppingCart.DeliveryInfo.Description;
                        }
                    }
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
                var session = SessionInfo.GetSessionInfo(ShoppingCart.DistributorID, Locale);
                if (!String.IsNullOrEmpty(ShoppingCart.SMSNotification))
                {
                    lblMobileNumberEntered.Text = ShoppingCart.SMSNotification;
                }
                else if (session != null && !string.IsNullOrEmpty( session.CO2DOSMSNumber) && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.CO2DOSMSNotification && ShoppingCart.CustomerOrderDetail != null)
                {
                    lblMobileNumberEntered.Text = ShoppingCart.SMSNotification = session.CO2DOSMSNumber;
                    
                }
            }

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
            {
                if (!String.IsNullOrEmpty(ShoppingCart.SMSNotification))
                {
                    lblMobileNumberEntered.Text = ShoppingCart.SMSNotification;
                }
            }
        }

        protected virtual void BindDataForEditableView()
        {
            #region DeliveryOption

            if (!IsPostBack)
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
                    (ProductsBase.SessionInfo.IsEventTicketMode &&
                     !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupAllowForEventTicket))
                {
                    DeliveryType.Items.Remove("Pickup");
                    divDeliveryOptionSelection.Visible = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryOptionHaveDropDown;
                }
                populateDropdown();
            }
            if (ShoppingCart.DeliveryInfo != null)
            {
                setAddressByNickName(ShoppingCart.DeliveryInfo.Address);
            }
            showHideAddressLink();
            ShowTitle();
            showShiptoOrPickup(IsStatic);

            #endregion DeliveryOption

            #region DeliveryInstruction

            //These get loaded in the showShiptoOrPickup method

            #endregion DeliveryInstruction

            #region EmailOption

            var currentSession = SessionInfo;

            if (!string.IsNullOrEmpty(ShoppingCart.EmailAddress) || !string.IsNullOrEmpty(currentSession.ChangedEmail))
            {
                txtEmail.Text = !string.IsNullOrEmpty(ShoppingCart.EmailAddress)
                                    ? ShoppingCart.EmailAddress
                                    : currentSession.ChangedEmail;
            }
            else
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.PopulateHMSPrimaryEMail)
                {
                    if (SessionInfo.IsReplacedPcOrder && (SessionInfo.ReplacedPcDistributorProfileModel != null))
                    {
                        txtEmail.Text = SessionInfo.ReplacedPcDistributorProfileModel.PrimaryEmail;
                    }
                    else
                    {
                        //txtEmail.Text = Email;
                        txtEmail.Text = DistributorProfileModel.PrimaryEmail;
                    }
                }
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasAdditonalNumber)
            {
                if (!String.IsNullOrEmpty(ShoppingCart.SMSNotification))
                {
                    var phoneSplit = ShoppingCart.SMSNotification.Split('-');
                    if (phoneSplit.Length == 2)
                    {
                        txtMobileAreaCode.Text = phoneSplit[0];
                        txtMobileNumber.Text = phoneSplit[1];
                    }
                }
            }

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
            {
                if (!String.IsNullOrEmpty(ShoppingCart.SMSNotification))
                {
                    txtSingleMobileNumber.Text = ShoppingCart.SMSNotification;
                }
            }

            #endregion EmailOption
        }

        protected virtual void RenderEditableView()
        {
            #region DeliveryOption

            lblSelectedDeliveryType.Visible = false;
            lblSelectedDeliveryAddress.Visible = false;
            DeliveryType.Visible = true; 
            DropdownNickName.Visible = true;
            divLinks.Visible = true;
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeOnShoppingCart)
            {
                divDeliveryTime.Visible = true;
            }
            else
            {
            divDeliveryTime.Visible = false;
            }
            #endregion DeliveryOption

            #region DeliveryInstruction

            trEditableShippingMethod.Visible = true;
            trEditableShippingTime.Visible = true;
            trEditableDatePicker.Visible = true;
            trEditablePickupName.Visible = true;
           
            if(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupPhoneFormat)
            {
                trEditablePickupPhoneFormat.Visible = true;
            }            
            trEditablePickupPhone.Visible = true;
            trEditablePickUpTimeDropDown.Visible = true;
            trEditableFreeFormShippingInstruction.Visible = true;

            if (PickupTinCodeDisplayCountries.Contains(CountryCode) && trEditablePickUpTinCode != null)
            {
                trEditablePickUpTinCode.Visible = true;
                trStaticPickUpTinCode.Visible = false;
            }

            trStaticShippingMethod.Visible = false;
            trStaticShippingTime.Visible = false;

            // New requirements for SG to display Delivery Date
            ShowShippingAndDEliveryDate(
                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveDate,
                true);

            trStaticDatePicker.Visible = false;
            trStaticPickupName.Visible = false;
            trStaticPickupPhone.Visible = false;
            trStaticPickUpTimeDropDown.Visible = false;
            trStaticFreeFormShippingInstruction.Visible = false;

            #endregion DeliveryInstruction

            #region EmailOption

            dvEmailReadOnly.Visible = false;
            dvMobileNumberReadOnly.Visible = false;
            dvEmailEdit.Visible = true;
            dvMobileNumber.Visible = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasAdditonalNumber;
            if (dvSingleTextBoxNumber != null)
            {
                dvSingleTextBoxNumber.Visible =
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo;
            }

            #endregion EmailOption
        }

        /// <summary>
        ///     To enable and disable, editable controls of delivery and shipping date.
        /// </summary>
        /// <param name="showShippingDate">Show shipping date.</param>
        /// <param name="showDeliveryDate">Show delivery date.</param>
        /// <param name="enableEdition">Enable edition.</param>
        private void ShowShippingAndDEliveryDate(bool showShippingDate, bool enableEdition)
        {
            if (trEditableDeliveryDate != null)
            {
                trEditableDeliveryDate.Visible = showShippingDate && enableEdition;
            }

            if (trStaticDeliveryDate != null)
            {
                trStaticDeliveryDate.Visible = showShippingDate && !enableEdition;
            }
        }

        protected virtual void RenderReadOnlyView()
        {
            #region DeliveryOption

            lblSelectedDeliveryType.Visible = true;
            lblSelectedDeliveryAddress.Visible = true;
            divLocationInformation.Visible = true;
            lnAddAddress.Visible = false;
            DeliveryType.Visible = false;
            DropdownNickName.Visible = false;
            divLinks.Visible = false;
            if (dvFreightChange != null)
                dvFreightChange.Visible = false;

            lblSelectedDeliveryType.Visible = true;
            lblNickName.Visible = true;
            if (null != ShoppingCart.DeliveryInfo && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                {
                    CheckFreightCode();
                }
            }
            divDeliveryTime.Visible = true;

            #endregion DeliveryOption

            #region DeliveryInstruction

            trStaticShippingMethod.Visible = true;
            trStaticShippingTime.Visible = true;
            trStaticDatePicker.Visible = true;
            trStaticPickupName.Visible = true;
            trStaticPickupPhone.Visible = true;
            trStaticPickUpTimeDropDown.Visible = true;
            trStaticFreeFormShippingInstruction.Visible = true;

            if (PickupTinCodeDisplayCountries.Contains(CountryCode) && trEditablePickUpTinCode != null)
            {
                trStaticPickUpTinCode.Visible = true;
                trEditablePickUpTinCode.Visible = false;
            }

            trEditableShippingMethod.Visible = false;
            trEditableShippingTime.Visible = false;

            // New requirements for SG to display Delivery Date
            ShowShippingAndDEliveryDate(
                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveDate,
                false);

            trEditableDatePicker.Visible = false;
            trEditablePickupName.Visible = false;
            trEditablePickupPhone.Visible = false;
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupPhoneFormat)
            {
                trEditablePickupPhoneFormat.Visible = false;
            }
            trEditablePickUpTimeDropDown.Visible = false;
            trEditableFreeFormShippingInstruction.Visible = false;

            #endregion DeliveryInstruction

            #region EmailOption

            dvEmailReadOnly.Visible = true;
            //if (CultureInfo.CurrentCulture.ToString() == "zh-HK")
            //{
            //    dvMobileNumberReadOnly.Visible =
            //        HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo;
            //}
            //else
            //{
                dvMobileNumberReadOnly.Visible =
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasAdditonalNumber || HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo;
            //}

            dvEmailEdit.Visible = false;
            dvMobileNumber.Visible = false;
            if (dvSingleTextBoxNumber != null)
            {
                dvSingleTextBoxNumber.Visible = false;
            }

            #endregion EmailOption
        }

        protected void CheckFreightCode()
        {
            string freightCode = string.Empty;
            if (null != ShoppingCart.DeliveryInfo)
            {
                if (ShoppingCart.OrderCategory == OrderCategoryType.RSO)
                {
                    freightCode = ShoppingCart.DeliveryInfo.FreightCode;
                    var lstDeliveryOption =
                        ProductsBase.GetShippingProvider()
                                    .GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                       ShoppingCart.DeliveryInfo.Address);
                    if (null != lstDeliveryOption && lstDeliveryOption.Count > 0)
                    {
                        if (!lstDeliveryOption.Exists(l => l.FreightCode == freightCode))
                        {
                            var deliveryOption = lstDeliveryOption.Find(l => l.IsDefault);
                            if (null != deliveryOption)
                            {
                                ShoppingCart.DeliveryInfo.FreightCode = deliveryOption.FreightCode;
                                ShoppingCart.FreightCode = deliveryOption.FreightCode;
                                ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                                ShoppingCart.Calculate();
                                OnQuoteRetrieved(this, null);
                            }
                        }
                    }
                }
            }
        }

        private string GetDefaultFreightCode(List<DeliveryOption> lstDeliveryOption)
        {
            string defaultCode = string.Empty;
            if (null != lstDeliveryOption && lstDeliveryOption.Count > 0)
            {
                var varOption = lstDeliveryOption.Where(l => l.IsDefault);
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

        private void SetFreightCode(string freightCode)
        {
            if (freightCode != ShoppingCart.DeliveryInfo.FreightCode ||
                freightCode != ShoppingCart.FreightCode)
            {
                string warehouse = ProductsBase.GetShippingProvider().GetWarehouseFromShippingMethod(freightCode, ShoppingCart.DeliveryInfo.Address);
                if (!string.IsNullOrEmpty(warehouse))
                {
                    ShoppingCart.DeliveryInfo.WarehouseCode = warehouse;
                }
                ShoppingCart.FreightCode = freightCode;
                ShoppingCart.DeliveryInfo.FreightCode = freightCode;
                ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                ShoppingCart.Calculate();
                OnQuoteRetrieved(this, null);
            }
        }
        private void SetFreightCodeForHerbalifePickUpLocation(string freightCode)
        {
            if (freightCode != ShoppingCart.DeliveryInfo.FreightCode ||
                freightCode != ShoppingCart.FreightCode)
            {
                
                ShoppingCart.FreightCode = freightCode;
                ShoppingCart.DeliveryInfo.FreightCode = freightCode;
                ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                ShoppingCart.Calculate();
                OnQuoteRetrieved(this, null);
            }
        }

        private void SetFreightCodeForConsolidatedShippingInfo(string name)
        {
            var lstDeliveryOption =
                ProductsBase.GetShippingProvider()
                            .GetDeliveryOptions(DeliveryOptionType.Shipping, ShoppingCart.DeliveryInfo.Address);
            var selectedOption = lstDeliveryOption.Find(p => p.Name.Trim().ToUpper() == name.ToUpper().Trim());
            ShoppingCart.DeliveryInfo.WarehouseCode = selectedOption.WarehouseCode;
            ShoppingCart.FreightCode = selectedOption.FreightCode;
            ShoppingCart.DeliveryInfo.Name = selectedOption.Name;
            ShoppingCart.DeliveryInfo.Id = selectedOption.Id;
            ShoppingCart.DeliveryInfo.FreightCode = selectedOption.FreightCode;
            ShoppingCart.OrderSubType = selectedOption.WarehouseCode;
            ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
            ShoppingCart.Calculate();
            OnQuoteRetrieved(this, null);
        }

        protected bool isValidEmail(string inputEmail)
        {
            if (!string.IsNullOrEmpty(inputEmail))
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                  @".)+))([a-zA-Z]{2,5}|[0-9]{1,3})(\]?)$";
                //string strRegex = @"/^[-_a-z0-9\'+*$^&%=~!?{}]++(?:\.[-_a-z0-9\'+*$^&%=~!?{}]+)*+@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d++)?$/iD";
                var re = new Regex(strRegex);
                if (re.IsMatch(inputEmail))
                    return (true);
                else
                    return (false);
            }
            else
            {
                return true;
            }
        }

        #endregion CheckOutOptionsControlMethods

        #region DeliveryOptionMethods

        protected void ShowTitle()
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup ||
                ShoppingCart.OrderCategory == OrderCategoryType.ETO || 
                ShoppingCart.OrderCategory == OrderCategoryType.HSO )
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

        protected DeliveryOption GetSelectedAddress(int id, DeliveryOptionType optionType)
        {
            return getSelectedAddress(id, optionType, DeliveryType);
        }

        protected override void populateDropdown()
        {
            if (ShoppingCart != null)
            {
                populateDropdown(DeliveryType);
            }
        }

        protected override void populateShipping()
        {
            populateShipping(DropdownNickName);
        }

        protected void DisableDeleteLink(bool disable)
        {
            LinkDelete.Enabled = !disable;
        }

        protected override void showHideAddressLink()
        {
            if((HLConfigManager.Configurations.AddressingConfiguration.HasHerbalifePickupFreightChnage) && (ShoppingCart.DeliveryInfo !=null &&(ShoppingCart.DeliveryInfo.FreightCode=="PU" || ShoppingCart.DeliveryInfo.FreightCode == HLConfigManager.Configurations.ShoppingCartConfiguration.HerbalifePickUPFreightCode)))
            {
                dvFreightChange.Visible = true;
            }
            else
            {
                if (dvFreightChange != null)
                    dvFreightChange.Visible = false;
            }
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
                    divshipToOrPickup.Visible = divNicknameSelection.Visible = divLinks.Visible = !lnAddAddress.Visible;
                    if (CheckNoDeliveryType(DeliveryType))
                    {
                        divLinks.Visible = true;
                    }
                    else
                    {
                        divLinks.Visible = (getDeliveryOptionTypeFromDropdown(DeliveryType) ==
                                            DeliveryOptionType.Shipping && DropdownNickName.SelectedValue != "0");
                        if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Pickup)
                        {
                            DropdownNickName.Visible = !lnAddAddress.Visible && DropdownNickName.Items.Count > 0;
                        }
                    }

                    if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                    {
                        if (!IsStatic)
                        {
                            divLinks.Visible = true;
                            LinkEdit.Visible = (getDeliveryOptionTypeFromDropdown(DeliveryType) ==
                                                DeliveryOptionType.Shipping);
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
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown ||
                    getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    lnAddAddress.Text = (string) GetLocalResourceObject("AddShippingAddress");
                }
                else
                {
                    lnAddAddress.Text = String.Empty;
                }
                divLinks.Visible = false;
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown &&
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup)
                {
                    lnAddAddress.Visible = false;
                }
                else
                {
                    lnAddAddress.Visible = true;
                }
            }

            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
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

            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.PickupFromCourier)
            {
                Label3.Visible = HLConfigManager.Configurations.CheckoutConfiguration.CheckOutMessageNotify;
                lblMessage.Visible = HLConfigManager.Configurations.CheckoutConfiguration.CheckOutShowNotification;
            }
            else
            {
                lblMessage.Visible = false;
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
            {
                var restrictedAddress = _shippingAddresses.Where(x => x.HasAddressRestriction ?? false);
                _shippingAddresses = restrictedAddress.ToList();
                divLinkEdit.Visible = divLinkDelete.Visible = false;
                divLinkAdd.Visible = !(_shippingAddresses.Count >= HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestrictionLimit);
            }
           
        }

        protected override void populatePickup()
        {
            //Before populating check whether it is related with HK
            if (!PickupTinCodeDisplayCountries.Contains(CountryCode) && trEditablePickUpTinCode != null)
            {
                trEditablePickUpTinCode.Visible = false;
                trStaticPickUpTinCode.Visible = false;
            }

            populatePickup(DropdownNickName, lblNickName);
        }

        protected override void setPickupInfo(DeliveryOption deliveryOption)
        {
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                // saved pickip phone and name because setPickupInfo will create new deliveryInfo
                string strSavePickupPhone;
                string strSavePickupName;
                strSavePickupPhone = strSavePickupName = string.Empty;
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                {
                    strSavePickupPhone = ShoppingCart.DeliveryInfo.Address.Phone;
                    strSavePickupName = ShoppingCart.DeliveryInfo.Address.Recipient;
                }

                var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                var shippingInfo = setPickupInfo(deliveryOption, deliveryOptionType);

                if (pAddress != null && string.IsNullOrEmpty(pAddress.InnerHtml))
                {
                    setAddressByNickName(shippingInfo == null ? null : shippingInfo.Address);
                }
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                {
                    ShoppingCart.DeliveryInfo.Address.Phone = string.IsNullOrEmpty(strSavePickupPhone)
                                                                  ? ShoppingCart.DeliveryInfo.Address.Phone
                                                                  : strSavePickupPhone;
                    ShoppingCart.DeliveryInfo.Address.Recipient = string.IsNullOrEmpty(strSavePickupName)
                                                                      ? ShoppingCart.DeliveryInfo.Address.Recipient
                                                                      : strSavePickupName;
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
            if (pickUpAddInfoHeader != null && getDeliveryOptionTypeFromDropdown(DeliveryType) != DeliveryOptionType.Shipping)
            {
                pickUpAddInfoHeader.Visible = true;
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
            if (selectedOption == DeliveryOptionType.Shipping)
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
                    pInformation.InnerHtml = DisplayHoursOfOperationForPickup &&
                                             ShoppingCart.DeliveryInfo != null &&
                                             ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                                                 ? ShoppingCart.DeliveryInfo.AdditionalInformation
                                                 : string.Empty;
                }
            }

            string shippingMessage = string.Empty;
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasShippingInstructionsMessage)
            {
                shippingMessage = ProductsBase.GetShippingProvider().GetShippingInstructionsForDS(ShoppingCart, "", "");
                if (!String.IsNullOrEmpty(shippingMessage))
                {
                    pInformation.InnerHtml = pInformation.InnerText + string.Format("<br><br>{0}", shippingMessage);
                }
            }
        }

        protected override void updateShippingInfo(int shippingAddressId,
                                                   int deliveryOptionId,
                                                   DeliveryOptionType option)
        {
            if (ShoppingCart != null)
            {
                bool updated = false;
                var deliveryInfo = ShoppingCart.DeliveryInfo;
                if (deliveryInfo == null || (deliveryInfo != null && deliveryInfo.Address.ID != shippingAddressId) ||
                    (deliveryInfo.Address.ID != shippingAddressId || deliveryInfo.Id != deliveryOptionId ||
                     deliveryInfo.Option != option || deliveryInfo.Option == DeliveryOptionType.Shipping))
                {
                    updated = true;
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                {
                    if (option == DeliveryOptionType.Pickup)
                    {
                        var pref = _pickupRrefList.Find(f => f.ID == deliveryOptionId);
                        if (pref != null)
                        {
                            deliveryOptionId = pref.PickupLocationID;
                        }
                    }
                }
                base.updateShippingInfo(shippingAddressId, deliveryOptionId, option);
                var sessionInfo = SessionInfo.GetSessionInfo(
                    DistributorID, Locale);
                sessionInfo.OrderConverted = false;

                if (ShoppingCart.CustomerOrderDetail != null && !string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                {
                    sessionInfo.CustomerAddressID = shippingAddressId;
                }

                if (updated)
                {
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveTime &&
                        ShoppingCart.DeliveryInfo != null)
                    {
                        ShoppingCart.DeliveryInfo.Instruction = deliveryInfo != null
                                                                    ? deliveryInfo.Instruction
                                                                    : ShoppingCart.DeliveryInfo.Instruction;
                    }
                    NotifyTodaysMagazineAddressChanged(this, null);
                }
            }
        }

        #endregion DeliveryOptionMethods

        #region DeliveryInstructionsMethods

        protected virtual void LoadPickUpInstructions(bool IsStatic)
        {
            var selectedOption = ShoppingCart.DeliveryInfo != null
                                     ? ShoppingCart.DeliveryInfo.Option
                                     : getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (selectedOption == DeliveryOptionType.Pickup)
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupAllowForEventTicket)
                {
                    if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate &&
                        !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName &&
                        !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone &&
                        !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                    {
                        DeliveryOptionsInstructionsView.Visible = false;
                        return;
                    }
                }

                if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    DeliveryOptionsInstructionsView.Visible =
                        HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupAllowForEventTicket;

                    if (!DeliveryOptionsInstructionsView.Visible)
                        return;
                }
                if (DeliveryTerms != null)
                {
                    DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
                }
                TextShippingMethod.Text = (string) GetLocalResourceObject("PickUpBy");
                divDeliveryTime.Visible = false;

                if (!IsStatic)
                {
                    // PickUp Instructions have TinID
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID)
                    {
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.HKID))
                            {
                                txtPickupTinID.Text = ShoppingCart.DeliveryInfo.HKID;
                            }
                        }
                    }
                    else
                    {
                        if (PickupTinCodeDisplayCountries.Contains(CountryCode) && trEditablePickUpTinCode != null)
                        {
                            trEditablePickUpTinCode.Visible = false;
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate)
                    {
                        int numberDays = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsStartDate;
                        if (CountryCode == "FR")
                        {
                            DateTime dt = Convert.ToDateTime(DateUtils.GetCurrentLocalTime(CountryCode));
                            if (dt.DayOfWeek == DayOfWeek.Monday || dt.DayOfWeek == DayOfWeek.Tuesday || dt.DayOfWeek == DayOfWeek.Wednesday || dt.DayOfWeek == DayOfWeek.Sunday)
                            {
                                pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode).AddDays(2);
                            }
                            else if (dt.DayOfWeek == DayOfWeek.Thursday || dt.DayOfWeek == DayOfWeek.Friday)
                            {
                                pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode).AddDays(4);
                            }
                            else if (dt.DayOfWeek == DayOfWeek.Saturday)
                            {
                                pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode).AddDays(3);
                            }
                            else
                            {
                                pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode).AddDays(numberDays);
                            }

                        }
                        else
                        {
                            pickupdateTextBox.MinDate = DateUtils.GetCurrentLocalTime(CountryCode).AddDays(numberDays);
                        }                                             
                        pickupdateTextBox.MaxDate = pickupdateTextBox.MinDate + new TimeSpan(ProductsBase.GetShippingProvider().GetAllowPickupDays(DateTime.Now), 0, 0, 0);

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PopulatePickupDate)
                        {
                            pickupdateTextBox.SelectedDate = pickupdateTextBox.MinDate;
                        }

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
                            txtPhoneNumber_MaskedEditExtender.CultureName = "en-US";
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

                    //SMS Phone

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
                    {
                        txtSingleMobileNumber.Attributes.Add("maxlength",
                                                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                                        .PickUpPhoneMaxLen.ToString());
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
                                    ddlPickupTime.ClearSelection();
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
                            if (CultureInfo.CurrentCulture.ToString() == "ro-RO")
                            {
                                lbPickupdate.Text = ShoppingCart.DeliveryInfo != null
                                                        ? (ShoppingCart.DeliveryInfo.Instruction != null
                                                               ? DateTime.Parse(ShoppingCart.DeliveryInfo.Instruction)
                                                                         .ToString("dd.MMM.yyyy")
                                                               : string.Empty)
                                                        : String.Empty;
                            }
                            else
                            {
                                lbPickupdate.Text = ShoppingCart.DeliveryInfo != null
                                                        ? (ShoppingCart.DeliveryInfo.Instruction != null
                                                               ? DateTime.Parse(ShoppingCart.DeliveryInfo.Instruction)
                                                                         .ToShortDateString()
                                                               : string.Empty)
                                                        : String.Empty;
                            }
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

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID)
                    {
                        lblPickUpTinCode.Text = ShoppingCart.DeliveryInfo != null
                                                    ? ShoppingCart.DeliveryInfo.HKID
                                                    : String.Empty;
                    }
                    else
                    {
                        if (PickupTinCodeDisplayCountries.Contains(CountryCode))
                        {
                            trStaticPickUpTinCode.Visible = false;
                        }
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

        protected void OnShippingInstruction_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
            {
                ShoppingCart.DeliveryInfo.Instruction = txtShippingInstruction.Text;
            }
        }

        protected virtual void PopulatePickUpInstructionsTimeDropDown()
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("PickupOptions");
            ddlPickupTime.Items.Clear();

            foreach (var e in entries)
            {
                ddlPickupTime.Items.Add(new ListItem(e.Value, e.Key));
            }
        }
        private void SetDeliveryTextBoxMinMaxDate()
        {
            var currentLocalTime = DateUtils.GetCurrentLocalTime(CountryCode);
                if ((Convert.ToDateTime(DeliveryDateTextBox.SelectedDate) - currentLocalTime).Days <= 1)
                {
                    if (currentLocalTime.Hour >= 12 && currentLocalTime.ToString("tt") == "PM")
                    {
                        DeliveryDateTextBox.MinDate = ProductsBase.GetShippingProvider().ShippingInstructionDateTodayNotAllowed()
                                                            ? currentLocalTime.AddDays(2)
                                                            : currentLocalTime;
                    if (Convert.ToDateTime(DeliveryDateTextBox.MinDate).DayOfWeek == DayOfWeek.Sunday)
                    {
                        DeliveryDateTextBox.MinDate = DeliveryDateTextBox.MinDate.AddDays(1);
                    }
                    DeliveryDateTextBox.MaxDate = (SetMaxDate(DeliveryDateTextBox.MinDate + new TimeSpan(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryTimeSpanAfter12Pm, 0, 0, 0)));
                }
                    else
                    {
                        DeliveryDateTextBox.MinDate = ProductsBase.GetShippingProvider().ShippingInstructionDateTodayNotAllowed()
                                                            ? currentLocalTime.AddDays(1)
                                                            : currentLocalTime;
                if (Convert.ToDateTime(DeliveryDateTextBox.MinDate).DayOfWeek == DayOfWeek.Sunday)
                {
                    DeliveryDateTextBox.MinDate = DeliveryDateTextBox.MinDate.AddDays(1);
                    }
                    DeliveryDateTextBox.MaxDate = (SetMaxDate(DeliveryDateTextBox.MinDate + new TimeSpan(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryTimeSpanBefore12Pm, 0, 0, 0)));
                }

                if (Convert.ToDateTime(DeliveryDateTextBox.MaxDate).DayOfWeek == DayOfWeek.Sunday)
                    {
                    DeliveryDateTextBox.MaxDate = DeliveryDateTextBox.MaxDate.AddDays(1);
                    }
              
                }
            }
        protected virtual void LoadShippingInstructions(bool IsStatic)
        {
            var selectedOption = ShoppingCart.DeliveryInfo != null
                                     ? ShoppingCart.DeliveryInfo.Option
                                     : getDeliveryOptionTypeFromDropdown(DeliveryType);

            if (selectedOption == DeliveryOptionType.Shipping)
            {
                if (!IsStatic)
                {
                    TextShippingMethod.Text = (string) GetLocalResourceObject("EditShippingMethod");

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
                    {
                        txtSingleMobileNumber.Attributes.Add("maxlength",
                                                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                                        .PickUpPhoneMaxLen.ToString());
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveDate)
                    {
                        trEditableDeliveryDate.Visible = true;
                        DeliveryDateTextBox.Visible = true;
                        var currentLocalTime = DateUtils.GetCurrentLocalTime(CountryCode);
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingTimeOptionCheck)
                        {
                            SetDeliveryTextBoxMinMaxDate();

                        }
                        else
                        {
                            DeliveryDateTextBox.MinDate =
                           ProductsBase.GetShippingProvider().ShippingInstructionDateTodayNotAllowed()
                               ? currentLocalTime + new TimeSpan(1, 0, 0, 0)
                               : currentLocalTime;
                            DeliveryDateTextBox.MaxDate = DeliveryDateTextBox.MinDate + new TimeSpan(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DeliveryTimeSpanAfter12Pm, 0, 0, 0);                          
                        }
                        DeliveryDateTextBox.SelectedDate = DeliveryDateTextBox.MinDate;

                        if (!String.IsNullOrEmpty(ShoppingCart.DeliveryDate))
                        {
                            DateTime dtShoppingCartDate;
                            bool success = DateTime.TryParse(ShoppingCart.DeliveryDate, out dtShoppingCartDate);
                            if (success)
                            {
                                DeliveryDateTextBox.SelectedDate = dtShoppingCartDate;
                            }
                        }
                    }
                    else
                    {
                        if (trEditableDeliveryDate != null)
                        {
                            trEditableDeliveryDate.Visible = false;
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveTime)
                    {
                        PopulateShippingInstructionsTimeDropDown();
                        if (ShoppingCart.CustomerOrderDetail != null && !string.IsNullOrEmpty(ShoppingCart.CustomerOrderDetail.CustomerOrderID))
                        {
                            var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
                            ShoppingCart.DeliveryInfo.Instruction = customerOrderV01.ShippingInstructions;
                        }
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                            {
                                var li = ddlShippingTime.Items.FindByText(ShoppingCart.DeliveryInfo.Instruction);
                                if (li != null)
                                {
                                    ddlShippingTime.ClearSelection();
                                    li.Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        trEditableShippingTime.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                    {
                        PopulateShippingMethodsDropDown();
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingCodesAreConsolidated)
                        {
                            if (ShoppingCart.DeliveryInfo != null)
                            {
                                if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Name))
                                {
                                    ddlShippingMethod.ClearSelection();
                                    var li = ddlShippingMethod.Items.FindByValue(ShoppingCart.DeliveryInfo.Name);
                                    if (li != null)
                                    {
                                        li.Selected = true;
                                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod)
                                        {
                                            string htmlfilename =
                                                ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ddlShippingMethod.SelectedValue);
                                            ContentReader2.ContentPath = htmlfilename;
                                            ContentReader2.LoadContent();
                                        }
                                    }
                                    else if (ddlShippingMethod.Items.Count > 0)
                                    {
                                        ddlShippingMethod.Items[0].Selected = true;
                                    }
                                }
                            }
                        }
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod)
                        {
                            string htmlfilename =
                                ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ddlShippingMethod.SelectedValue);
                            ContentReader2.ContentPath = htmlfilename;
                            ContentReader2.LoadContent();
                        }
                    }
                    else
                    {
                        trEditableShippingMethod.Visible = false;
                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit != 0 && ShoppingCart.VolumeInCart > HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit)
                        {
                            string htmlfilename = ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.Address);
                            ContentReader2.ContentPath = htmlfilename;
                            ContentReader2.LoadContent();
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasFreeFormShippingInstruction)
                    {
                        if (ShoppingCart.CustomerOrderDetail != null)
                        {
                            var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
                            if(customerOrderV01 !=null)
                            {
                                ShoppingCart.DeliveryInfo.Instruction = customerOrderV01.ShippingInstructions;
                            }
                        }
                        txtShippingInstruction.MaxLength =
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                           .FreeFormShippingInstructionMaxLength;
                        if (txtShippingInstruction.MaxLength < 30)
                        {
                            txtShippingInstruction.TextMode = InputMode.SingleLine;
                            txtShippingInstruction.BorderStyle = BorderStyle.Inset;
                            txtShippingInstruction.BorderWidth = 1;
                        }
                        else
                        {
                            txtShippingInstruction.TextMode = InputMode.MultiLine;
                            txtShippingInstruction.Height = new Unit("80px");
                        }
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            txtShippingInstruction.Text = ShoppingCart.DeliveryInfo.Instruction;
                        }
                    }
                    else
                    {
                        trEditableFreeFormShippingInstruction.Visible = false;
                    }

                    if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                    {
                        TextShippingMethod.Text = (string)GetLocalResourceObject("HAPShippingInstructionTitle");
                        ContentReader2.ContentPath = "shippingmethod-hap.html";
                        trEditableShippingMethod.Visible = false;
                    }
                }
                else
                {
                    TextShippingMethod.Text = (string) GetLocalResourceObject("ShippingMethodLabel");

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveDate)
                    {
                        if (!APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                        {
                            if (!string.IsNullOrEmpty(ShoppingCart.DeliveryDate))
                            {
                                lbDeliveryDate.Text = DateTime.Parse(ShoppingCart.DeliveryDate).ToShortDateString();
                            }
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                    {
                        if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingCodesAreConsolidated)
                        {
                            if (ShoppingCart.DeliveryInfo != null)
                            {
                                var lstDeliveryOption =
                                    ProductsBase.GetShippingProvider()
                                                .GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                                   ShoppingCart.DeliveryInfo.Address);
                                if (lstDeliveryOption.Count > 0)
                                {
                                    var option =
                                        lstDeliveryOption.Find(p => p.FreightCode == ShoppingCart.FreightCode);
                                    if (option != null)
                                    {
                                        lbShippingMethod.Text = option.Description;
                                    }
                                }
                            }
                        }
                        else
                        {
                            lbShippingMethod.Text = ShoppingCart.DeliveryInfo != null
                                                        ? ShoppingCart.DeliveryInfo.Name
                                                        : String.Empty;
                            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod)
                            {
                                string htmlfilename =
                                    ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(lbShippingMethod.Text);
                                ContentReader2.ContentPath = htmlfilename;
                                ContentReader2.LoadContent();
                            }
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveTime)
                    {
                        txtDeliveryTime.Text = ShoppingCart.DeliveryInfo != null
                                                   ? ShoppingCart.DeliveryInfo.Instruction
                                                   : String.Empty;
                    }
                    else
                    {
                        trStaticShippingTime.Visible = false;
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasFreeFormShippingInstruction)
                    {
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            txtShippingInstruction2.Text = ShoppingCart.DeliveryInfo.Instruction;
                        }
                    }
                    else
                    {
                        trStaticFreeFormShippingInstruction.Visible = false;
                    }

                    // Show delivery time estimated if needed
                    divDeliveryTime.Visible =
                        HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeEstimated;
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeEstimated)
                    {
                        var estimated = ProductsBase.GetShippingProvider()
                                                    .GetDeliveryEstimate(ShoppingCart.DeliveryInfo, Locale);
                        
                        if (estimated != null && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeOnShoppingCart)
                        {
                            hlDeliveryTime.Text =
                           string.Format(GetLocalResourceObject("hlDeliveryTimeResource.Text") as string,
                                         estimated.Value);
                            return;
                        }
                        if (estimated != null)
                        {
                            hlDeliveryTime.Text =
                                string.Format(GetLocalResourceObject("hlDeliveryTimeResource.Text") as string,
                                              estimated.Value);
                            hlDeliveryTime.NavigateUrl =
                                string.Format(@"/content/{0}/html/ordering/deliveryEstimator.html", Locale);
                        }
                        else
                        {
                            divDeliveryTime.Visible = false;
                        }
                    }

                    if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                    {
                        TextShippingMethod.Text = (string)GetLocalResourceObject("HAPShippingInstructionTitle");
                        ContentReader2.ContentPath = "shippingmethod-hap.html";
                        trStaticShippingMethod.Visible = false;
                    }
                }
            }
            else
            {
                divDeliveryTime.Visible = false;
            }
        }

        private void PopulateShippingMethodsDropDown()
        {
            ddlShippingMethod.Visible = false;
            if (ShoppingCart.DeliveryInfo != null)
            {
                ddlShippingMethod.Items.Clear();

                // for data manipulation purpose, can't work directly on GetDeliveryOptionsListForShipping() result (lstDeliveryOption_org), as it will affect the cached data as well
                List<DeliveryOption> lstDeliveryOption = null;

                var lstDeliveryOption_org =
                    ProductsBase.GetShippingProvider(ShoppingCart.OrderCategory)
                                .GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                   ShoppingCart.DeliveryInfo.Address);

                #region Promotion LowerPriceDelivery ?
                if (IsChinaApp)
                {
                    ShoppingCart.LowerPriceDeliveryIdList = null;

                    if ((lstDeliveryOption_org != null) && (lstDeliveryOption_org.Count >= 2)) // u need at least 2 DeliveryOption for LowerPriceDelivery, 1st item (select option) is not valid option
                    {
                        var ruleResults = ShoppingCart.RuleResults;

                        ShoppingCartRuleResult ruleResultLPD = null;
                        if (ruleResults != null) ruleResultLPD = ruleResults.FirstOrDefault(x => (x.RuleName == Rules.Promotional.zh_CN.PromotionalRules.Promotional_RuleName_LowerPriceDelivery) && (x.Result == RulesResult.Success));

                        if (ruleResultLPD != null)
                        {
                            lstDeliveryOption = new List<DeliveryOption>();
                            lstDeliveryOption.Add(lstDeliveryOption_org.First()); // add the 1st "select option" item

                            var lstDeliveryOption_real = lstDeliveryOption_org.Where(x => x.ID > 0); // exclude the 1st item (ID > 0)

                            ShoppingCart.LowerPriceDeliveryIdList = new List<int>();

                            var minFee = lstDeliveryOption_real.Min(x => x.EstimatedFee ?? 0); // get the lowest fee
                            foreach (var item in lstDeliveryOption_real.Where(x => x.EstimatedFee.GetValueOrDefault(0) == minFee))
                            {
                                lstDeliveryOption.Add(item);

                                ShoppingCart.LowerPriceDeliveryIdList.Add(item.ID);
                            }

                            // fill up the rest of non-lowest fee companies
                            foreach (var item in lstDeliveryOption_real.Where(x => x.EstimatedFee.GetValueOrDefault(0) != minFee))
                            {
                                lstDeliveryOption.Add(item);
                            }
                        }
                    }
                }
                #endregion

                if (lstDeliveryOption == null) lstDeliveryOption = lstDeliveryOption_org; // if the manipulable-set is null, then reuse lstDeliveryOption_org

                if (lstDeliveryOption != null && lstDeliveryOption.Count > 0)
                {
                    if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingCodesAreConsolidated)
                    {
                        ddlShippingMethod.DataSource = from l in lstDeliveryOption select l;
                        ddlShippingMethod.DataBind();
                    }
                    else
                    {
                        ddlShippingMethod.DataSource = from l in lstDeliveryOption
                                                       select new
                                                           {
                                                               FreightCode = l.Name, 
                                                               Description = l.Name
                                                           };
                        ddlShippingMethod.DataBind();

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeOnShoppingCart)
                        {
                            var estimated = ProductsBase.GetShippingProvider()
                                                    .GetDeliveryEstimate(ShoppingCart.DeliveryInfo, Locale);
                            hlDeliveryTime.Text =
                          string.Format(GetLocalResourceObject("hlDeliveryTimeResource.Text") as string,
                                        estimated.Value);
                        }

                        if (ddlShippingMethod.Items.Count != 1)
                        {
                            ddlShippingMethod.Items.Insert(0,
                                                           new ListItem((string)GetLocalResourceObject("TextSelect"),
                                                                        "0"));
                        }
                        if (ddlShippingMethod.Items.Count == 1)
                        {
                            if (lstDeliveryOption != null)
                            {
                                SetFreightCodeForConsolidatedShippingInfo(lstDeliveryOption.FirstOrDefault().Name);
                            }
                        }
                    }
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit != 0 && ShoppingCart.VolumeInCart > HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit)
                    {
                        string defaultCode = GetDefaultFreightCode(lstDeliveryOption);
                        var liFreightCode = ddlShippingMethod.Items.FindByValue(defaultCode);
                        if (liFreightCode != null)
                        {
                            ddlShippingMethod.ClearSelection();
                            SetFreightCode(defaultCode);
                            liFreightCode.Selected = true;
                        }
                        ddlShippingMethod.Visible = false;
                    }
                    else
                        ddlShippingMethod.Visible = true;
                }
            }
        }

        private void PopulateShippingInstructionsTimeDropDown()
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("ShippingInstructions").OrderBy(i => i.Key);
            ddlShippingTime.Items.Clear();

            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.Value))
                {
                ddlShippingTime.Items.Add(new ListItem(e.Value, e.Key));
            }
        }
        }
        private DateTime SetMaxDate(DateTime TempMaxDate)
        {
            var days = from d in Enumerable.Range(0, (TempMaxDate - DeliveryDateTextBox.MinDate).Days + 1)
                       let currentDate = DeliveryDateTextBox.MinDate.AddDays(d)
                       where currentDate.DayOfWeek == DayOfWeek.Sunday
                       select currentDate;
            int holidays = days.Count();
            return TempMaxDate.AddDays(holidays);
        }
        #endregion DeliveryInstructionsMethods

        #region EventHandlerDeliveryOptions

        protected void OnNickName_Databind(object sender, EventArgs e)
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

        protected void OnNickNameChanged(object sender, EventArgs e)
        {
            lbDeliveryDate.Text = String.Empty;
            blErrors.Items.Clear();
            var ddl = sender as DropDownList;
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);

                lbShiptoOrPickup.Visible = true;

                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    LoadPickUpInstructions(IsStatic);
                    pInformation.InnerHtml = DisplayHoursOfOperationForPickup
                                                 ? ShoppingCart.DeliveryInfo.AdditionalInformation
                                                 : string.Empty;
                }

                string shippingMessage = string.Empty;
                if (HLConfigManager.Configurations.CheckoutConfiguration.HasShippingInstructionsMessage)
                {
                    shippingMessage = ProductsBase.GetShippingProvider().GetShippingInstructionsForDS(ShoppingCart, "", "");
                    if (!string.IsNullOrEmpty(shippingMessage))
                    {
                        shippingMessage = string.Format("<br><br>{0}", shippingMessage);
                        if (!pInformation.InnerHtml.Contains(shippingMessage))
                        {
                            pInformation.InnerHtml = pInformation.InnerText + shippingMessage;
                        }
                    }
                }

                cntrlConfirmAddress.IsConfirmed = false;
            }

            showHideAddressLink();

            if (HLConfigManager.Configurations.CheckoutConfiguration.ErrorNoDeliveryOption && ShoppingCart.DeliveryInfo == null)
            {
                blErrors.Items.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDeliveryOption"));
            }

            if (ProductsBase.HasToDisplaySaveCopyError())
            {
                var savedCopyError = ProductsBase.GetSaveCopyError();
                blErrors.Items.Add(savedCopyError);
            }
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
        }
           if( HLConfigManager.Configurations.DOConfiguration.ShowFreightChrageonCOP1)
            {
                OnQuoteRetrieved(this,null);
            }
        }

        protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            lblNickName.Visible = false;
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            if (pickUpAddInfoHeader != null)
            {
                pickUpAddInfoHeader.Visible = true;
            }
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                populateShipping();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divDeliveryMethodShipping.Visible = true;
                divDeliveryMethodPickup.Visible = false;

                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionForEventTicket)
                {
                    DeliveryOptionsInstructionsView.Visible = ShoppingCart.OrderCategory != OrderCategoryType.ETO;
                }

                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                }
                if (DeliveryTerms != null)
                {
                    DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
                }
                if (pickUpAddInfoHeader != null)
                {
                    pickUpAddInfoHeader.Visible = false;
                }
                if (HL.Common.Configuration.Settings.GetRequiredAppSetting("polynesiaSentence", "fr-PF").Contains(Locale))
                    divSentence.Visible = divSentence != null && ShoppingCart.OrderCategory == OrderCategoryType.RSO ? true : false;
            }
            else
            {
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

                if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    DeliveryOptionsInstructionsView.Visible =
                        HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupAllowForEventTicket;
                }
                if (DeliveryTerms != null)
                {
                    DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
                }
                if (divSentence != null)
                    divSentence.Visible = false;
            }
            showShiptoOrPickup(IsStatic);
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
            }
            cntrlConfirmAddress.IsConfirmed = false;

            if (ProductsBase.HasToDisplaySaveCopyError())
            {
                var savedCopyError = ProductsBase.GetSaveCopyError();
                blErrors.Items.Add(savedCopyError);
            }            
        }

        protected void DeleteClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
                {
                    var deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
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
                    var deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
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
                        var shippingInfo = ShoppingCart.DeliveryInfo;
                        if (shippingInfo != null)
                        {
                            var mpAddAddress =
                                (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
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
                            mpAddAddress.Show();
                        }
                    }
                }
            }
        }

        protected void EditClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                var deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                        (DeliveryOptionType)
                                                        Enum.Parse(typeof (DeliveryOptionType),
                                                                   DeliveryType.SelectedValue));
                if (deliveryOption != null)
                {
                    Session["OQVOldaddress"] = deliveryOption;
                    ModalPopupExtender mpAddAddress = null;
                    mpAddAddress = (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Edit,
                                                               new ShippingAddressEventArgs(DistributorID,
                                                                                            deliveryOption, false,
                                                                                            ProductsBase
                                                                                                .DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
            }
        }

        protected void CorrectAddress()
        {
            if (ShoppingCart.CopyInvoiceAddress != null && ShoppingCart.CopyInvoiceStatus != CopyInvoiceStatus.success)
            {
                var shippingAddress = new DeliveryOption
                    (
                    new ShippingAddress_V02
                        {
                            ID = ShoppingCart.ShippingAddressID,
                            Address = ShoppingCart.CopyInvoiceAddress,
                            Recipient =
                                ShoppingCart.CopyInvoiceName != null
                                    ? string.Format("{0} {1}", ShoppingCart.CopyInvoiceName.First,
                                                    ShoppingCart.CopyInvoiceName.Last)
                                    : null,
                            FirstName = ShoppingCart.CopyInvoiceName != null ? ShoppingCart.CopyInvoiceName.First : null,
                            MiddleName =
                                ShoppingCart.CopyInvoiceName != null ? ShoppingCart.CopyInvoiceName.Middle : null,
                            LastName = ShoppingCart.CopyInvoiceName != null ? ShoppingCart.CopyInvoiceName.Last : null,
                            Phone = ShoppingCart.CopyInvoicePhone
                        }
                    );
                if (shippingAddress != null)
                {
                    Session["OQVOldaddress"] = shippingAddress;
                    ModalPopupExtender mpAddAddress = null;
                    mpAddAddress = (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                               new ShippingAddressEventArgs(DistributorID,
                                                                                            shippingAddress, false,
                                                                                            ProductsBase
                                                                                                .DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
            }
        }

        protected void AddAddressClicked(object sender, EventArgs e)
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

        protected void AddClicked(object sender, EventArgs e)
        {
            if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
            {
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping ||
                    getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
                {
                    var mpAddAddress =
                        (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                               new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                            ProductsBase
                                                                                                .DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
            }
            else
            {
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    var mpAddAddress =
                        (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                               new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                            ProductsBase
                                                                                                .DisableSaveAddressCheckbox));
                    mpAddAddress.Show();
                }
                else
                {
                    var mpAddAddress =
                        (ModalPopupExtender) ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                    ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
                    mpAddAddress.Show();
                }
            }
        }

        protected void OnPickupPhone_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
            {
                ShoppingCart.DeliveryInfo.Address.Phone = txtPickupPhone.Text;
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var list = (List<string>)Session["pickupPhone"];
                    if (list != null && list.Count > 0)
                    {
                        list[1] = ShoppingCart.DeliveryInfo.Address.Phone;
                        Session["pickupPhone"] = list;
                    }
                }
            }
        }

        protected void OnPickupName_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
            {
                ShoppingCart.DeliveryInfo.Address.Recipient = txtPickupName.Text.Trim();
            }
        }

        protected void OnPickupTinID_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
            {
                //Created a new property for passing TinID for PickUp
                ShoppingCart.DeliveryInfo.HKID = txtPickupTinID.Text;
            }
        }

        #endregion EventHandlerDeliveryOptions

        #region EventDeliveryInstructions

        protected virtual void SetAdditionalInfo(string freightCode)
        {

        }

        protected void ddlShippingMethod_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (ddlShippingMethod.SelectedValue != null)
                {
                    if (HLConfigManager.Configurations.CheckoutConfiguration.EnforcesShippingMethodRules)
                    {
                        string previousFreightCode = (ddlShippingMethod).PreviousSelectedValue;
                        ShoppingCart.CurrentItems = new ShoppingCartItemList();
                        ShoppingCart.CurrentItems.Add(new ShoppingCartItem_V01(0, ddlShippingMethod.SelectedValue, 0,
                                                                               DateTime.Now));
                        var results = HLRulesManager.Manager.ProcessCart(ShoppingCart,
                                                                         ShoppingCartRuleReason
                                                                             .CartFreightCodeChanging);
                        ShoppingCart.CurrentItems.Clear();
                        if (results.Any(r => r.Result == RulesResult.Failure))
                        {
                            blErrors.Items.Clear();
                            ddlShippingMethod.SelectedValue = previousFreightCode;
                            blErrors.Items.Add(
                                (from r in results where r.Result == RulesResult.Failure select r.Messages[0]).First());
                            return;
                        }
                    }
                    if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingCodesAreConsolidated)
                    {
                        SetFreightCode(ddlShippingMethod.SelectedValue);
                        SetAdditionalInfo(ddlShippingMethod.SelectedValue);
                    }
                    else
                    {
                        if (ddlShippingMethod.SelectedValue != "0")
                        {
                            SetFreightCodeForConsolidatedShippingInfo(ddlShippingMethod.SelectedValue);                 
                        }
                    }
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod)
                    {
                        string htmlfilename =
                            ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ddlShippingMethod.SelectedValue);
                        ContentReader2.ContentPath = htmlfilename;
                        ContentReader2.LoadContent();
                    }
                    NotifyTodaysMagazineRecalculate(this, null);
                }
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeOnShoppingCart)
            {
                var estimated = ProductsBase.GetShippingProvider()
                                                    .GetDeliveryEstimate(ShoppingCart.DeliveryInfo, Locale);
                if (ddlShippingMethod.SelectedIndex == 0)
                    estimated = null;

                if (estimated != null)
                {
                    hlDeliveryTime.Text = string.Format(GetLocalResourceObject("hlDeliveryTimeResource.Text") as string, estimated.Value);
                }
                else
                {
                    lbDeliveryTime.Visible = false;
                    hlDeliveryTime.Text = string.Empty;
                }
            }
            blErrors.Items.Clear();
        }

        protected void ddlShippingMethod_OnDataBound(object sender, EventArgs e)
        {
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingCodesAreConsolidated)
            {
                var ddl = sender as DropDownList;

                var lstDeliveryOption =
                    ProductsBase.GetShippingProvider()
                                .GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                   ShoppingCart.DeliveryInfo.Address);
                if (ShoppingCart != null && ddl != null && ShoppingCart.DeliveryInfo != null)
                {
                    if (ddl.Items.Count > 0)
                    {
                        if (String.IsNullOrEmpty(ShoppingCart.FreightCode) &&
                            String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.FreightCode))
                        {
                            //If both are empty, get the default or first from the drop down and make it the freight code
                            if (lstDeliveryOption.Count > 0)
                            {
                                string defaultCode = GetDefaultFreightCode(lstDeliveryOption);
                                var liFreightCode = ddl.Items.FindByValue(defaultCode);
                                if (liFreightCode != null)
                                {
                                    ddl.ClearSelection();
                                    SetFreightCode(defaultCode);
                                    liFreightCode.Selected = true;
                                }
                                else
                                {
                                    SetFreightCode(ddl.SelectedItem.Value);
                                }
                            }
                        }
                        else if (!string.Equals(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.FreightCode))
                        {
                            //if both are not equal, make these invalid and get the default or first from the drop down and make it the freight code
                            if (lstDeliveryOption.Count > 0)
                            {
                                string defaultCode = GetDefaultFreightCode(lstDeliveryOption);
                                var liFreightCode = ddl.Items.FindByValue(defaultCode);
                                if (liFreightCode != null)
                                {
                                    ddl.ClearSelection();
                                    SetFreightCode(defaultCode);
                                    liFreightCode.Selected = true;
                                }
                                else
                                {
                                    SetFreightCode(ddl.SelectedItem.Value);
                                }
                            }
                        }
                        else if (string.Equals(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.FreightCode))
                        {
                            //search for this in the drop down, if exists select that, if not get the default or first from the drop down and make it the freight code
                            var liFreightCode = ddl.Items.FindByValue(ShoppingCart.FreightCode);
                            if (liFreightCode != null)
                            {
                                ddl.ClearSelection();
                                liFreightCode.Selected = true;
                            }
                            else
                            {
                                if (!SessionInfo.IsEventTicketMode)
                                {
                                    if (lstDeliveryOption.Count > 0)
                                    {
                                        string defaultCode = GetDefaultFreightCode(lstDeliveryOption);
                                        var liFreightCodeDefault = ddl.Items.FindByValue(defaultCode);
                                        if (liFreightCodeDefault != null)
                                        {
                                            ddl.ClearSelection();
                                            SetFreightCode(defaultCode);
                                            liFreightCodeDefault.Selected = true;
                                        }
                                        else
                                        {
                                            SetFreightCode(ddl.SelectedItem.Value);
                                        }
                                    }
                                }
                            }
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
        }

        public void OnPickupTimeChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl != null)
            {
                if (ddl.Items.Count > 0)
                {
                    if (!ddl.Items[0].Selected)
                    {
                        ShoppingCart.DeliveryInfo.Instruction = ddl.SelectedItem.Text;
                    }
                }
            }
        }

        public void OnDeliveryTimeChanged(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                var ddl = sender as DropDownList;
                if (ddl != null)
                {
                    if (ddl.Items.Count > 0)
                    {
                        if (!ddl.Items[0].Selected)
                        {
                            // Removing error if exists.
                            if (blErrors.Items != null)
                            {
                                blErrors.Items.Remove(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                "NoShippingTimeSelected"));
                            }

                            ShoppingCart.DeliveryInfo.Instruction = ddl.SelectedItem.Text;
                        }
                        else
                        {
                            if (blErrors.Items != null)
                            {
                                blErrors.Items.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                             "NoShippingTimeSelected"));
                            }
                        }
                    }
                }
            }
        }

        #endregion EventDeliveryInstructions

        #region EventEmailAddress

        protected void tbEmailAddress_Changed(object sender, EventArgs e)
        {
            ShoppingCart.EmailAddress = txtEmail.Text.Trim();
            SessionInfo.ChangedEmail = ShoppingCart.EmailAddress;
        }

        private string getMobilePhoneNumber()
        {
            return txtMobileAreaCode.Text + "-" + txtMobileNumber.Text;
        }

        protected void txtMobileAreaCode_Changed(object sender, EventArgs e)
        {
            ShoppingCart.SMSNotification = getMobilePhoneNumber();
        }

        protected void txtMobileNumber_Changed(object sender, EventArgs e)
        {
            ShoppingCart.SMSNotification = getMobilePhoneNumber();
        }

        protected void txtSingleMobileNumber_Changed(object sender, EventArgs e)
        {
            ShoppingCart.SMSNotification = txtSingleMobileNumber.Text;
        }

        #endregion EventEmailAddress

        #region SubscriptionEvents

        protected override void onProceedingToCheckout()
        {
            SetFreightcodeforHerbalifePickup();
            var errors = new List<string>();

            #region Validations

            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryOptionType == DeliveryOptionType.Unknown)
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDeliveryType"));
            }
            else if (deliveryOptionType == DeliveryOptionType.Shipping)
            {
                if (DropdownNickName.SelectedValue == "0" || ShoppingCart.DeliveryInfo == null)
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingNickNameNotPopulated"));
                }

                if (!APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale) &&
                    !ProductsBase.SessionInfo.IsEventTicketMode)
                {
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                    {
                        if (ddlShippingMethod.Items.Count > 0)
                        {
                            if (ddlShippingMethod.SelectedValue == "0")
                            {
                                var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                                if (HLConfigManager.Configurations.DOConfiguration.IsChina && (OrderTotals != null && OrderTotals.AmountDue - OrderTotals.Donation != 0))
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoShippingMethodSelected"));
                                }
                                else if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
                                {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoShippingMethodSelected"));
                            }
                        }
                        }

                        #region 155152
                        if (IsChinaApp)
                        {
                            if (this.ddlShippingMethod.Items.Count <= 1) // 1 item means only has the "please select...", missing actual express company data
                            {
                                if (ShoppingCart.CartItems.Any())
                                {
                                    var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                                    if (OrderTotals != null && (OrderTotals.AmountDue - OrderTotals.Donation != 0))
                                    {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoExpressCompanyForDeliveryAddress"));
                            }

                        }
                            }
                        }
                        #endregion
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveTime)
                    {
                        if (
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                           .ShippingInstructionsTimeMandatory)
                        {
                            if (ddlShippingTime.SelectedIndex == -1 || ddlShippingTime.Items[0].Selected)
                            {
                                var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                                if (HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && (OrderTotals.AmountDue - OrderTotals.Donation != 0))
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                         "NoShippingTimeSelected"));
                                }
                                else if(!HLConfigManager.Configurations.DOConfiguration.IsChina)
                                {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoShippingTimeSelected"));
                            }

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
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        ShoppingCart.DeliveryInfo.Instruction = txtShippingInstruction.Text;
                    }
                }
                if ((deliveryOptionType == DeliveryOptionType.Shipping &&
                     HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingInstructionsHaveDate))
                {
                    if (DeliveryDateTextBox.Visible)
                    {
                        if (!DeliveryDateTextBox.SelectedDate.HasValue)
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                 "NoDeliveryDateSelected"));
                        }
                        else
                        {
                            if (ShoppingCart.DeliveryInfo != null &&
                                ProductsBase.GetShippingProvider()
                                            .ValidateShippingInstructionsDate(
                                                Convert.ToDateTime(DeliveryDateTextBox.SelectedDate)))
                            {

                                 ShoppingCart.DeliveryDate =
                                    Convert.ToDateTime(DeliveryDateTextBox.SelectedDate)
                                           .ToString("d", CultureInfo.CurrentCulture);
                            }
                            else
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidDate"));
                            }
                        }
                    }
                }

                if (HLConfigManager.Configurations.AddressingConfiguration.ValidateShippingAddress && !ProductsBase.GetShippingProvider().IsValidShippingAddress(ShoppingCart))
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidShippingAddress"));
                }
            }
            else
            {
                if (DropdownNickName.SelectedValue == "0" || ShoppingCart.DeliveryInfo == null)
                {
                    var deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    _pickupRrefList= ProductsBase.GetShippingProvider()
                                                  .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                                 DeliveryOptionType.PickupFromCourier);
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && deliveryType == DeliveryOptionType.PickupFromCourier
                        && _pickupRrefList.Count == 0 && _pickupRrefList!=null)
                    {                         
                        errors.Clear();                     
                    }
                    else
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated"));
                    }
                }                  
                
                if (!APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
                {
                    if ((deliveryOptionType == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName) ||
                        (deliveryOptionType == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveName))
                    {
                        if (String.IsNullOrEmpty(txtPickupName.Text))
                        {
                            //New Error message for Seven-Eleven PickUpFromCourier Recipients Name
                            if (deliveryOptionType == DeliveryOptionType.PickupFromCourier &&
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                    .IsPickupInstructionsRequired &&
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SevenElevenCountry)
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
                            if (
                                !string.IsNullOrEmpty(
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupNameRegExp) &&
                                !Regex.IsMatch(txtPickupName.Text,
                                               HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                              .PickupNameRegExp))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "InvalidRecipentName"));
                            }
                            else if (ShoppingCart.DeliveryInfo != null)
                            {
                                ShoppingCart.DeliveryInfo.Address.Recipient = txtPickupName.Text.Trim();
                            }
                        }
                    }

                    if ((deliveryOptionType == DeliveryOptionType.Pickup && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone) ||
                        (deliveryOptionType == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone))
                    {
                        string phone =
                            txtPickupPhone.Text.Replace("_", String.Empty)
                                          .Replace("(", String.Empty)
                                          .Replace(")", String.Empty)
                                          .Replace("-", String.Empty)
                                          .Trim();
                        if (String.IsNullOrEmpty(phone))
                        {
                            if (deliveryOptionType == DeliveryOptionType.PickupFromCourier &&
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                    .IsPickupInstructionsRequired &&
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SevenElevenCountry)
                            {
                                //Add required field error message for Thiland Seven-Eleven PickupFromCourier 
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "PuckUpCorierPhoneNumberErrorMessage"));
                            }

                            else  if (
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                               .IsPickupInstructionsRequired || 
                               (deliveryOptionType == DeliveryOptionType.PickupFromCourier && 
                               HLConfigManager.Configurations.PickupOrDeliveryConfiguration.IsPickupFromCourierPhoneRequired))
                            {
                                //Add required field error message for countries other than India
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoPickUpPhoneEntered"));
                            }
                        }
                        else if (deliveryOptionType == DeliveryOptionType.Pickup || deliveryOptionType == DeliveryOptionType.PickupFromCourier ?
                            !Regex.IsMatch(phone,
                                           HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                            .PickUpPhoneRegExp) :
                            !Regex.IsMatch(phone,
                                           HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                            .PickUpFromcourierPhoneRegExp))
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
                    // Validation For SMSNotification Number
                    if (deliveryOptionType == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.validateSMSNotificationNumber)
                    {

                        if (string.IsNullOrEmpty(txtSingleMobileNumber.Text))
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "SMSNotificationErrorMessage"));

                        }
                    }

                    if ((deliveryOptionType == DeliveryOptionType.Pickup &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveDate) ||
                        (deliveryOptionType == DeliveryOptionType.PickupFromCourier &&
                         HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveDate))
                    {
                        if (pickupdateTextBox.Visible)
                        {
                            if (!pickupdateTextBox.SelectedDate.HasValue)
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoPickUpDateSelected"));
                            }
                            else
                            {
                                if (ShoppingCart.DeliveryInfo != null &&
                                    ProductsBase.GetShippingProvider()
                                                .ValidatePickupInstructionsDate(pickupdateTextBox.SelectedDate.Value))
                                {
                                    ShoppingCart.DeliveryInfo.Instruction =
                                        Convert.ToDateTime(pickupdateTextBox.SelectedDate)
                                               .ToString("d", CultureInfo.CurrentCulture);
                                    ShoppingCart.DeliveryInfo.PickupDate = pickupdateTextBox.SelectedDate.Value;
                                }
                                else
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidDate"));
                                }
                            }
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                    {
                        if (ddlPickupTime.SelectedIndex == -1 || ddlPickupTime.Items[0].Selected)
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpTimeSelected"));
                        }
                    }

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID)
                    {
                        if (!string.IsNullOrEmpty(txtPickupTinID.Text))
                        {
                            if (
                                !string.IsNullOrEmpty(
                                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.TinCodeIDRegExp) &&
                                !Regex.IsMatch(txtPickupTinID.Text,
                                               HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                              .TinCodeIDRegExp))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidHKID"));
                            }
                            else
                            {
                                ShoppingCart.DeliveryInfo.HKID = txtPickupTinID.Text;
                                ShoppingCart.DeliveryInfo.Instruction = string.Format("{0} , {1} ,{2}",
                                                                                      ShoppingCart.DeliveryInfo.Address
                                                                                                  .Recipient,
                                                                                      ShoppingCart.DeliveryInfo.Address
                                                                                                  .Phone,
                                                                                      txtPickupTinID.Text);
                            }
                        }
                        else
                        {
                            //Tin is non-mandatory,but that does'nt affects instructions
                            ShoppingCart.DeliveryInfo.Instruction = string.Format("{0} , {1}",
                                                                                  ShoppingCart.DeliveryInfo.Address
                                                                                              .Recipient,
                                                                                  ShoppingCart.DeliveryInfo.Address
                                                                                              .Phone);
                        }
                    }
                }
            }

            if (deliveryOptionType == DeliveryOptionType.PickupFromCourier)
            {
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone)
                {
                    string phone =
                        txtPickupPhone.Text.Replace("_", String.Empty)
                                      .Replace("(", String.Empty)
                                      .Replace(")", String.Empty)
                                      .Replace("-", String.Empty)
                                      .Trim();
                    if (!String.IsNullOrEmpty(phone) && (ShoppingCart.DeliveryInfo != null))
                    {
                        ShoppingCart.DeliveryInfo.Address.Phone = txtPickupPhone.Text;
                    }
                }
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail &&
                String.IsNullOrEmpty(txtEmail.Text.Trim()))
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoEmailAddress"));
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail && !isValidEmail(txtEmail.Text.Trim()))
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidEmailAddress"));
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequiresAddressConfirmation
                && cntrlConfirmAddress.Visible)
            {
                if (!cntrlConfirmAddress.IsConfirmed)
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddressConfirmationRequired"));
                }
                cntrlConfirmAddress.SetConfirmAddressContentStyle();
                SessionInfo.ConfirmedAddress = cntrlConfirmAddress.IsConfirmed;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireSMS)
            {
                if (String.IsNullOrEmpty(txtMobileAreaCode.Text) || String.IsNullOrEmpty(txtMobileNumber.Text))
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoSMSNumber"));
                }
                else
                {
                    if (
                        !string.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.AreaCodeMobileRegExp) &&
                        !Regex.IsMatch(txtMobileAreaCode.Text,
                                       HLConfigManager.Configurations.CheckoutConfiguration.AreaCodeMobileRegExp))
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));
                    }
                    if (
                        !string.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp) &&
                        !Regex.IsMatch(txtMobileNumber.Text,
                                       HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp))
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                    }
                    ShoppingCart.SMSNotification  = getMobilePhoneNumber();
                    if(ShoppingCart.CustomerOrderDetail != null && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.CO2DOSMSNotification)
                    {
                        SessionInfo.CO2DOSMSNumber = ShoppingCart.SMSNotification;
                    }
                }
            }

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
            {
                if (HLConfigManager.Configurations.PaymentsConfiguration.AllowWireTransferAutoOrderRelease)
                {
                    //For IN only
                    if (string.IsNullOrEmpty(txtSingleMobileNumber.Text))
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoSMSNumber"));
                    }
                    else
                    {
                        if (
                            !string.IsNullOrEmpty(
                                HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp) &&
                            !Regex.IsMatch(txtSingleMobileNumber.Text.Trim(),
                                           HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp))
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidSMSNumber"));
                        }
                    }
                }
                else
                {
                //No validations required for HK
                    if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        if (!string.IsNullOrEmpty(txtSingleMobileNumber.Text))
                        {
                            if (
                                    !string.IsNullOrEmpty(
                                        HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp) &&
                                !Regex.IsMatch(txtSingleMobileNumber.Text.Trim(),
                                               HLConfigManager.Configurations.CheckoutConfiguration.MobileNumberRegExp))
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                            }
                        }
                    }
                    }
               
                ShoppingCart.SMSNotification = txtSingleMobileNumber.Text.Trim();

                if (SessionInfo.IsReplacedPcOrder && SessionInfo.ReplacedPcDistributorProfileModel != null)
                    SessionInfo.ReplacedPcDistributorProfileModel.SmsNotification = ShoppingCart.SMSNotification;
            }

            if (ProductsBase.HasToDisplaySaveCopyError())
            {
                var savedCopyError = ProductsBase.GetSaveCopyError();
                blErrors.Items.Add(savedCopyError);
            }

            #endregion Validations

            if (errors.Count > 0)
            {
                blErrors.DataSource = errors;
                blErrors.DataBind();
                if (OnCheckOutOptionsNotPopulated != null)
                OnCheckOutOptionsNotPopulated(this, null);
            }
            else
            {
                //Save Shipping/Pickup/Email Data before moving to check out

                ShoppingCart.EmailAddress = txtEmail.Text.Trim();
                SessionInfo.ChangedEmail = ShoppingCart.EmailAddress;
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DifferentFragmentForShippingMethod)
            {
                string htmlfilename;
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsHaveDropDown)
                    htmlfilename =ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ddlShippingMethod.SelectedValue);               
                else
                    htmlfilename =ProductsBase.GetShippingProvider().GetDifferentHtmlFragment(ShoppingCart.FreightCode, ShoppingCart.DeliveryInfo.Address);               
                
                ContentReader2.ContentPath = htmlfilename;
                ContentReader2.LoadContent();
            }
        }

        public void SetFreightcodeforHerbalifePickup()
        {
            if (HLConfigManager.Configurations.AddressingConfiguration.HasHerbalifePickupFreightChnage && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                ShoppingCart.HerbalifeFastPickup = this.cbFreightchanges.Checked;
                if (this.cbFreightchanges.Checked)
                {
                   
                    var freightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.HerbalifePickUPFreightCode;
                    SetFreightCodeForHerbalifePickUpLocation(freightCode);
                }
                else
                {
                    handleHerbalifePickupFreightCodeChanged(DeliveryType, DropdownNickName);

                }
            }
        }
        public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                shippingAddressCreated(args.ShippingAddress);
                reload();
                lnAddAddress.Visible = false;
                ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
                LoadShippingInstructions(IsStatic);
            }
        }

        [SubscribesTo(MyHLEventTypes.ProceedingToCheckout)]
        public void OnProceedingToCheckout(object sender, EventArgs e)
        {
            onProceedingToCheckout();
        }

        public override void onShippingAddressUpdated(ShippingAddressEventArgs args)
        {
            if (args != null)
            {
                shippingAddressChanged(args.ShippingAddress);
                LoadShippingInstructions(IsStatic);
                ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
            }
            ucShippingInfoControl.Hide();
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressChanged)]
        public void OnShippingAddressChanged(object sender, EventArgs e)
        {
            //if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                onShippingAddressUpdated(e as ShippingAddressEventArgs);

                ucShippingInfoControl.Hide();
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressCreated)]
        public void OnShippingAddressCreated(object sender, EventArgs e)
        {
            ucShippingInfoControl.Hide();
            onShippingAddressCreated(e as ShippingAddressEventArgs);
            ppShippingInfoControl.Update();
        }

        public override void onShippingAddressDeleted(ShippingAddressEventArgs args)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                if (args != null)
                {
                    shippingAddressDeleted(args.ShippingAddress);
                    ShoppingCartProvider.CheckDSFraud(this.ShoppingCart);
                }
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

        [SubscribesTo(MyHLEventTypes.ShippingAddressDeleted)]
        public void OnShippingAddressDeleted(object sender, EventArgs e)
        {
            onShippingAddressDeleted(e as ShippingAddressEventArgs);
            ucShippingInfoControl.Hide();
            ppShippingInfoControl.Update();
        }

        public override void OnPickupPreferenceCreated(DeliveryOptionEventArgs args)
        {
            if (args != null)
            {
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId, DeliveryOptionType.Pickup);
                populateDropdown();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                showShiptoOrPickup(IsStatic);
            }
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public void OnPickupPreferenceCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                OnPickupPreferenceCreated(args);
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
                                                                         (Page as ProductsBase).CountryCode);
                if (_pickupRrefList.Count > 0)
                {
                    var pref = _pickupRrefList.First();
                    updateShippingInfo(ProductsBase.ShippingAddresssID, pref.PickupLocationID, DeliveryOptionType.Pickup);
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

        [SubscribesTo(MyHLEventTypes.PickupPreferenceDeleted)]
        public void OnPickupPreferenceDeleted(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                OnPickupPreferenceDeleted(args);
            }
            //ppShippingInfoControl.Update();
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) &&
                !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowShippingMethodForAPFETO)
            {
                DeliveryOptionsInstructionsView.Visible = false;
            }
            if (DeliveryTerms != null)
            {
                DeliveryTerms.Visible = (HLConfigManager.Configurations.DOConfiguration.DeliveryTerms && DeliveryOptionsInstructionsView.Visible == false) ? true : false;
            }

            blErrors.Items.Clear();
        }

        [SubscribesTo(MyHLEventTypes.ShippingMethodCheckVP)]
        public void OnShippingMethodCheckVP(object sender, EventArgs e)
        {
            LoadShippingInstructions(IsStatic);
        }

        [SubscribesTo(MyHLEventTypes.OnSaveCart)]
        public void OnSaveCart(object sender, EventArgs e)
        {
            CanSaveCart(ref DeliveryType, ref DropdownNickName);
        }
        #endregion SubscriptionEvents

        bool IsChinaApp
        {
            get
            {
                return HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
        }
    }
}