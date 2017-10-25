using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Utilities;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CnCheckOutOptions : CheckOutOptions
    {
        #region Properties

        public DeliveryOptionType currentSelectedDeliveryType;

        public new bool DisplayHoursOfOperationForPickup { get; set; }

        #endregion

        #region ctor

        public CnCheckOutOptions()
        {
            DisplayHoursOfOperationForPickup = true;
        }

        #endregion

        [Publishes(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        new public event EventHandler OnCheckOutOptionsNotPopulated;

        [Publishes(MyHLEventTypes.DeliveryOptionChanged)]
         public event EventHandler OnDeliveryOptionChanged;

        [Publishes(MyHLEventTypes.ShippingMethodChanged)]
         public event EventHandler OnShippingMethodChanged;

        [SubscribesTo(MyHLEventTypes.CNShoppingCartRecalculated)]
        public void OnCNShoppingCartRecalculated(object sender, EventArgs e)
        {
            OnNickNameChanged(sender, e);
            SetAdditionalInfo(ShoppingCart.FreightCode);

        }
        [Publishes(MyHLEventTypes.ShoppingCartChanged)]
        new public event EventHandler OnShoppingCartChanged;
        #region CheckOutOptions

        protected override void LoadData()
        {
            var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && OrderTotals.Donation > 0 && (OrderTotals.AmountDue - OrderTotals.Donation == 0))
            {
                var myShoppingCart = (Page as ProductsBase).ShoppingCart;
                ShippingProvider_CN objCNShipping = new ShippingProvider_CN();
                myShoppingCart = objCNShipping.StandAloneAddressForDonation(myShoppingCart);
                DeliveryOptionsInstructionsView.Visible = Div1.Visible = DeliveryOptionsView.Visible = false;
            }
            else
            {
                DeliveryOptionsInstructionsView.Visible = Div1.Visible = DeliveryOptionsView.Visible = true;
                _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
                if (_shippingAddresses != null)
                {
                    _shippingAddresses = _shippingAddresses.Where(x => !x.Address.Line4.StartsWith("MALO") && !x.Address.Line4.StartsWith("DE") && !x.Address.Line4.StartsWith("MAEN") && !x.Address.Line4.StartsWith("PALO") && !x.Address.Line4.StartsWith("PAEN")).ToList();
                }
            }
            _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                   ShippingProvider.GetDefaultAddress());
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && SessionInfo != null && SessionInfo.IsEventTicketMode)
            {
                if (_pickupLocations != null) _pickupLocations = _pickupLocations.Where(x => x.IsETO).ToList();
            }
            else if (HLConfigManager.Configurations.DOConfiguration.IsChina && SessionInfo != null && !SessionInfo.IsEventTicketMode)
            {
                if (_pickupLocations != null) _pickupLocations = _pickupLocations.Where(x => !x.IsETO).ToList();
            }

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier)
            {
                _pickupRrefList = ProductsBase.GetShippingProvider()
                                              .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                             DeliveryOptionType.PickupFromCourier);
            }
            if (SessionInfo.IsReplacedPcOrder && SessionInfo.ReplacedPcDistributorOrderingProfile != null)
            {
                divChinaPCMessageBox.Visible = SessionInfo.IsReplacedPcOrder;
            }
            if (!IsPostBack)
            {
                ShowOrHideGreetingMsg();
            }
        }

        void ShowOrHideGreetingMsg()
        {
           
            var isnotdonationorAPF = ShoppingCart
                .ShoppingCartItems
                .Any(tempsku => !string.IsNullOrEmpty(tempsku.SKU) && tempsku.SKU != "9909");
            if (ShoppingCart.OrderCategory == OrderCategoryType.RSO && isnotdonationorAPF && IsChina)
            {
                var defaultmsg = GetLocalResourceObject("txtGreetingText").ToString();
                if (string.IsNullOrEmpty(ShoppingCart.GreetingMsg))
                {
                    txtGreeting.Attributes.Add("PlaceHolder", defaultmsg);
                    lblGreetingtxt.Text = ShoppingCart.GreetingMsg = defaultmsg;
                } else
                {
                    if (ShoppingCart.GreetingMsg == defaultmsg)
                    {
                        lblGreetingtxt.Text = ShoppingCart.GreetingMsg;
                        txtGreeting.Attributes.Add("PlaceHolder", defaultmsg);
                    } else
                    {
                        txtGreeting.Text = lblGreetingtxt.Text = ShoppingCart.GreetingMsg;
                    }
                } 
                txtGreeting.Attributes.Add("maxlength", "20");
               

            }
            else
            {
                lblGreetingtxt.Visible = false;
                lblGreeting1.Visible = false;
                lblGreeting.Visible = false;
                txtGreeting.Visible = false;
                txtGreeting.Text = lblGreetingtxt.Text = ShoppingCart.GreetingMsg = string.Empty;
            }
        }
         

        protected override void ConfigureMenu()
        {
            if (SessionInfo.IsEventTicketMode)
            {
                if (DeliveryType.Items.FindByValue("PickupFromCourier") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
                if (DeliveryType.Items.FindByValue("PickupFromCourier1") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier1"));
                if (DeliveryType.Items.FindByValue("ShipToCourier") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("ShipToCourier"));
                if (DeliveryType.Items.FindByValue("Shipping") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Shipping"));
            }
            else
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
                        deliveryType.ClearSelection();
                        var item = deliveryType.Items.FindByValue("Pickup");
                        if (item != null)
                            item.Selected = true;
                        populatePickup();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        deliveryType.ClearSelection();
                        var item = deliveryType.Items.FindByValue("PickupFromCourier");
                        if (item != null)
                            item.Selected = true;
                        PopulatePickupPreference();

                    }
                }
            }
            else
            {
                deliveryType.ClearSelection();
                if (hasShippingAddresses)
                {
                    deliveryType.Items[0].Selected = true;
                    populateShipping();
                }
                else
                {
                    var textSelect = (string)GetLocalResourceObject("TextSelect");
                    deliveryType.Items.Insert(0, new ListItem(textSelect, "0"));
                    deliveryType.Items[0].Selected = true;
                }
            }

            TrackDropdownNickName(DropdownNickName);
        }

        protected override void SetAdditionalInfo(string freightCode)
        {
            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
            {
                var lstDeliveryOption =
                        ProductsBase.GetShippingProvider()
                                    .GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                       ShoppingCart.DeliveryInfo.Address);
                var option = lstDeliveryOption.Find(x => x.FreightCode.Equals(freightCode));
                if ((option != null) && (option.Id > 0)) // bug 132314 - zero should be considered as no selection
                {
                    // enrolled/hit LowerPriceDelivery promo?
                    if ((ShoppingCart.RuleResults != null)
                        && ShoppingCart.RuleResults.Any(x => (x.RuleName == Rules.Promotional.zh_CN.PromotionalRules.Promotional_RuleName_LowerPriceDelivery) && (x.Result == RulesResult.Success)))
                    {
                        int doId = 0;
                        var lpdList = ShoppingCart.LowerPriceDeliveryIdList;

                        // selected the LowerPriceDelivery company?
                        if ((lpdList != null) && (lpdList.Count > 0)
                            && int.TryParse(ShoppingCart.FreightCode, out doId) && lpdList.Contains(doId))
                        {
                            // Hide red explanation word 
                            lblAdditionalInfo.Visible = false;
                            return;
                        }
                    }

                    lblAdditionalInfo.Visible = !string.IsNullOrEmpty(option.Information);

                    if (SessionInfo.IsFreightExempted) lblAdditionalInfo.Visible = false;

                    lblAdditionalInfo.Text = option.Information;
                }
                else
                {
                    lblAdditionalInfo.Visible = false;
                }

            }
        }
        protected override void BindDataForEditableView()
        {
            base.BindDataForEditableView();

            if (ShoppingCart.DeliveryInfo != null)
            {
                DisplayContactInfo(false);
            }
        }
        protected override void BindDataForReadOnlyView()
        {
            base.BindDataForReadOnlyView();
            if (ShoppingCart.DeliveryInfo != null)
            {
                DisplayContactInfo(true);
            }

            // if UseSessionLessInfo and pAddress is populated, don't process further, as lot of data are not available in SessionLessInfo-mode, futher process will break the expected result.
            if (SessionInfo.UseSessionLessInfo && !string.IsNullOrWhiteSpace(pAddress.InnerHtml))
            {
                lblNickName.Text = ShoppingCart.DeliveryInfo.DeliveryNickName;
                return;
            }

            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                lblSelectedDeliveryType.Text =
                    (string)GetLocalResourceObject("DeliveryOptionType_PickupFromCourier.Text");
                var courierLocation =
                    (from p in _pickupRrefList where p.ID == ShoppingCart.DeliveryInfo.Id select p)
                        .FirstOrDefault();
                lblNickName.Visible = true;
                if (courierLocation != null && !string.IsNullOrEmpty(courierLocation.PickupLocationNickname))
                {
                    lblNickName.Text = courierLocation.PickupLocationNickname;
                }
                else
                {
                    lblNickName.Text = (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Alias)
                                            ? ShoppingCart.DeliveryInfo.Address.DisplayName
                                            : ShoppingCart.DeliveryInfo.Address.Alias);
                }
                HL.Common.Logging.LoggerHelper.Info(string.Format("1{0}  2{1} 3{2}", ShoppingCart.DeliveryInfo.Id, lblNickName.Text, ShoppingCart.DeliveryInfo.Address.DisplayName));
                pAddress.InnerHtml =
                    ProductsBase.GetShippingProvider().FormatShippingAddress(
                        new DeliveryOption(ShoppingCart.DeliveryInfo.Address), ShoppingCart.DeliveryInfo.Option,
                        ShoppingCart.DeliveryInfo.Description, true);
                WorkDateLabels.Text = (string)GetLocalResourceObject("OperationHoursLabel") + ShoppingCart.DeliveryInfo.AdditionalInformation;
            }

            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                DeliveryOption deliveryOption = null;
                var varPickupLocation = _pickupLocations.Where(p => p.Id == ShoppingCart.DeliveryInfo.Id);
                if (varPickupLocation.Count() > 0)
                {
                    deliveryOption = varPickupLocation.First();
                }

                pAddress.InnerHtml =
                        ProductsBase.GetShippingProvider().FormatShippingAddress(deliveryOption,
                                                                                ShoppingCart.DeliveryInfo.Option,
                                                                                ShoppingCart.DeliveryInfo.Description, true);
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
                                                  p.ID
                                              };
                DropdownNickName.DataBind();
                // here is the problem after binding it will be automatically select an index set it to -1 will fix the issue 218834
                DropdownNickName.SelectedIndex = -1;
            }
            else
            {
                DropdownNickName.Visible = false;
                divshipToOrPickup.Visible = false;
            }

            TrackDropdownNickName(DropdownNickName);
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
                            LinkEdit.Visible = (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.PickupFromCourier);
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
                if (_shippingAddresses.Count > 0)
                {
                    LinkEdit.Visible = true;
                }
            }
            if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                if (_pickupRrefList.Count == 1 && (Page as ProductsBase).CantDeleteFinalAddress)
                {
                    DisableDeleteLink(true);
                }
                else if (_pickupRrefList.Count > 1)
                {
                    DisableDeleteLink(false);
                }
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
            if (selectedOption == DeliveryOptionType.Shipping || selectedOption == DeliveryOptionType.ShipToCourier)
            {
                lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = (string)GetLocalResourceObject("NickName");
                divDeliveryMethodShipping.Visible = true;
                divDeliveryMethodPickup.Visible = false;
                LoadShippingInstructions(IsStatic);
            }
            else
            {
                if (CheckNoDeliveryType(DeliveryType))
                {
                    lbShiptoOrPickup.Text = (string)GetLocalResourceObject("ShipTo");
                    NickNameReadonly.Text = (string)GetLocalResourceObject("NickName");
                    divDeliveryMethodShipping.Visible = true;
                    divDeliveryMethodPickup.Visible = false;
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    lbShiptoOrPickup.Text = (string)GetLocalResourceObject("PickUpFrom");

                    if (selectedOption == DeliveryOptionType.PickupFromCourier)
                    {
                        NickNameReadonly.Text = (string)GetLocalResourceObject("PickupFromCourierLocation");
                        //lblSelectedDeliveryAddress.Visible = true;
                        //lblSelectedDeliveryAddress.Text = (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null) ?
                        //   ShoppingCart.DeliveryInfo.Address.DisplayName : string.Empty;
                    }
                    if (selectedOption == DeliveryOptionType.Pickup)
                    {
                        NickNameReadonly.Text = (string)GetLocalResourceObject("Location");
                    }
                    divDeliveryMethodShipping.Visible = false;
                    divDeliveryMethodPickup.Visible = true;
                    trPickupDetails.Visible = (selectedOption == DeliveryOptionType.Pickup || !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                    trPickupFromCourierDetails.Visible = (selectedOption == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails);
                    LoadPickUpInstructions(IsStatic);
                }
            }
        }

        protected override void LoadShippingInstructions(bool IsStatic)
        {
            base.LoadShippingInstructions(IsStatic);

            if (!IsStatic)
            {
                trEditConsigneeName.Visible = true;
                if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                    ShoppingCart.DeliveryInfo.Address != null)
                {
                    txtConsigneeName.Text = ShoppingCart.DeliveryInfo.Address.Recipient;
                }
                trStaticConsigneeName.Visible = false;
            }
            else
            {
                trStaticConsigneeName.Visible = true;
                if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                    ShoppingCart.DeliveryInfo.Address != null)
                {
                    lblStaticConsigneeName.Text = ShoppingCart.DeliveryInfo.Address.Recipient;
                }
                trEditConsigneeName.Visible = false;


            }

        }

        protected void OnConsigneeName_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
            {
                ShoppingCart.DeliveryInfo.Address.Recipient = txtConsigneeName.Text.Trim();
            }
        }

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
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone &&
                    !HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTime)
                {
                    DeliveryOptionsInstructionsView.Visible = false;
                    return;
                }

                TextShippingMethod.Text = (string)GetLocalResourceObject("PickUpBy");
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

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName)
                    {
                        txtPickupName.Attributes.Add("maxlength", "36");
                        if (ShoppingCart.DeliveryInfo != null)
                        {
                            if (!String.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                            {
                                txtPickupName.Text = ShoppingCart.DeliveryInfo.Address.Recipient;
                            }

                            if (SessionInfo.IsEventTicketMode)
                            {
                                if (string.IsNullOrWhiteSpace(txtPickupName.Text))
                                {
                                    txtPickupName.Text = DistributorProfileModelHelper.FullLocalName(DistributorProfileModel);
                                }
                            }
                        }
                    }
                    else
                    {
                        trEditablePickupName.Visible = false;
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

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber &&
                        ShoppingCart.DeliveryInfo != null)
                    {
                        trEditableDocType.Visible = true;
                        trEditableRGNumber.Visible = true;

                        if (ShoppingCart.DeliveryInfo != null &&
                            !string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.RGNumber))
                        {
                            var docInfo = ShoppingCart.DeliveryInfo.RGNumber.Split('|');
                            var item = ddlDocType.Items.FindByText(docInfo[0]);
                            ddlDocType.SelectedIndex = -1;
                            if (item != null)
                            {
                                item.Selected = true;
                            }
                            txtRGNumber.Text = docInfo[1];
                        }
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

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber && ShoppingCart.DeliveryInfo != null)
                    {
                        if (ShoppingCart.DeliveryInfo != null && !string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.RGNumber))
                        {
                            var docInfo = ShoppingCart.DeliveryInfo.RGNumber.Split('|');
                            lblDocTypeValue.Text = docInfo[0];
                            lblRGNumberValue.Text = docInfo[1];
                        }
                    }
                    else
                    {
                        trStaticRGNumber.Visible = false;
                    }
                }
            }
        }

        protected override bool setAddressByNickName(ShippingAddress_V01 address, HtmlGenericControl pAddress,
                                                    DropDownList deliveryType)
        {
            pAddress.Visible = address != null;
            DeliveryOptionType option = getDeliveryOptionTypeFromDropdown(deliveryType);
            if (address != null)
            {
                // below code block is commeneted not to restrict Additional Info in all pages 

                //address.AreaCode = DisplayHoursOfOperationForPickup
                //                                ? ShoppingCart.DeliveryInfo.AdditionalInformation
                //                                : string.Empty;
                
                if (option == DeliveryOptionType.Unknown && CheckNoDeliveryType(deliveryType))
                {
                    option = DeliveryOptionType.Shipping;
                }

                pAddress.InnerHtml =
                    ProductsBase.GetShippingProvider()
                        .FormatShippingAddress(new DeliveryOption(address), option,
                            ShoppingCart != null && ShoppingCart.DeliveryInfo != null
                                ? ShoppingCart.DeliveryInfo.Description
                                : string.Empty, true);
                if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null && option == DeliveryOptionType.PickupFromCourier)
                    WorkDateLabels.Text = (string)GetLocalResourceObject("OperationHoursLabel") + ShoppingCart.DeliveryInfo.AdditionalInformation;
                if (CheckNoDeliveryType(deliveryType) && !string.IsNullOrEmpty(pAddress.InnerHtml))
                {
                    return true;
                }

            }
            else
            {
                pAddress.InnerHtml = "";
                ////if (option == DeliveryOptionType.PickupFromCourier)
                ////    WorkDateLabels.Text = (string) GetLocalResourceObject("OperationHoursLabel");
                //else
                    WorkDateLabels.Text = string.Empty;
            }

            return false;
        }

        /// <summary>
        /// 141435, need to expose DropdownNickName
        /// </summary>
        internal System.Web.UI.WebControls.DropDownList DropdownNickName_ForPublicAccess
        {
            get { return this.DropdownNickName; }
        }

        public new void OnNickNameChanged(object sender, EventArgs e)
        {
            blErrors.Items.Clear();
            lblAdditionalInfo.Text = string.Empty;
            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            var ddl = sender as DropDownList;
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                Session["pickupPhone"] = null;
                
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartItemsAdded);
                    OnShoppingCartChanged(this, new EventArgs());
                }

                lbShiptoOrPickup.Visible = true;

                if (deliveryOptionType == DeliveryOptionType.Shipping)
                {
                    LoadShippingInstructions(IsStatic);
                }
                else
                {
                    LoadPickUpInstructions(IsStatic);
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                        ShoppingCart.DeliveryInfo.Address.Alias = DropdownNickName.SelectedItem != null
                                                                            ? DropdownNickName.SelectedItem.Text
                                                                            : string.Empty;
                }

                cntrlConfirmAddress.IsConfirmed = false;
            }
            divLinks.Visible = ((deliveryOptionType == DeliveryOptionType.Shipping ||
                                 deliveryOptionType == DeliveryOptionType.PickupFromCourier) &&
                                DropdownNickName.SelectedValue != "0");

            TrackDropdownNickName(DropdownNickName);

      //      OnShippingMethodChanged(this, null);
        }

        protected new void OnNickName_Databind(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
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
                            var varPref = _pickupRrefList.Where(f => f.ID == deliveryInfo.Id);
                            if (varPref.Count() > 0)
                            {
                                var selected = ddl.Items.FindByValue(varPref.First().ID.ToString());
                                if (selected != null)
                                {
                                    ddl.ClearSelection();
                                    selected.Selected = true;
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
                                ddl.ClearSelection();
                                ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                                ddl.Items[0].Selected = true;
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
            ShoppingCart.DeliveryInfo = null;
            var deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
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
                DropdownNickName.Visible = DropdownNickName.Items.Count > 0;
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                PopulatePickupPreference();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divDeliveryMethodShipping.Visible = false;
                divDeliveryMethodPickup.Visible = true;
                trPickupDetails.Visible = !HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails;
                trPickupFromCourierDetails.Visible = HLConfigManager.Configurations.CheckoutConfiguration.DisplayCourierDetails;
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodNeedsDisplay)
                {
                    DeliveryOptionsInstructionsView.Visible = true;
                }

                //FilteredTextBoxExtender1.InvalidChars = "0123456789*@#$%^&*()-_+=~!./;;,\\[]{}abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
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
            showShiptoOrPickup(IsStatic);
            if (null != DropdownNickName.SelectedItem)
            {
                lbShiptoOrPickup.Visible = DropdownNickName.SelectedItem.Value != "0";
            }
            cntrlConfirmAddress.IsConfirmed = false;

            ShoppingCart.PcLearningDeliveryoption = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), deliveryType.ToString());
            currentSelectedDeliveryType = (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), deliveryType.ToString());
           // OnDeliveryOptionChanged(this, null);
        }

        protected new void DeleteClicked(object sender, EventArgs e)
        {
            if (DropdownNickName.SelectedItem != null)
            {
                var deliveryOption = GetSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                        (DeliveryOptionType)
                                                        Enum.Parse(typeof(DeliveryOptionType),
                                                                   DeliveryType.SelectedValue));
                if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
                {
                    if (deliveryOption != null)
                    {
                        var mpAddAddress =
                            (ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
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
                            (ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                        var pickupLocationPreferences =
                            (Page as ProductsBase).GetShippingProvider()
                                                  .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                                 (Page as ProductsBase).CountryCode);
                        if (int.Parse(DropdownNickName.SelectedValue) != 0)
                        {
                            var pref =
                                pickupLocationPreferences.Find(p => p.ID == int.Parse(DropdownNickName.SelectedValue));
                            if (pref != null)
                            {
                                ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete,
                                                                         new DeliveryOptionEventArgs(
                                                                             pref.PickupLocationID, shippingInfo.Name));
                            }
                        }
                        mpAddAddress.Show();
                    }
                }
            }
        }

        protected new void AddAddressClicked(object sender, EventArgs e)
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping ||
                getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Unknown)
            {
                var mpAddAddress = (ModalPopupExtender)ucShippingInfoControl.FindControl("popup_ShippingInfoControl");
                ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                           new ShippingAddressEventArgs(DistributorID, null, false,
                                                                                        ProductsBase
                                                                                            .DisableSaveAddressCheckbox));
                mpAddAddress.Show();
            }
        }

        protected new void AddClicked(object sender, EventArgs e)
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
            else
            {
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, null);
            }
            mpAddAddress.Show();
        }

        protected new void OnPickupPhone_Changed(object sender, EventArgs e)
        {
            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
            {
                    ShoppingCart.ReplaceAltPhoneNumber = txtPickupPhone.Text;
            }
        }

        public override void OnPickupPreferenceCreated(DeliveryOptionEventArgs args)
        {
            if (args != null)
            {
                _pickupRrefList =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode);
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId,
                                   DeliveryOptionType.PickupFromCourier);
                populateDropdown();
                showHideAddressLink();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                showShiptoOrPickup(IsStatic);
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
                    updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
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

        protected override void RenderEditableView()
        {
            base.RenderEditableView();

            trEditableDocType.Visible = true;
            trEditableRGNumber.Visible = true;
            trStaticDocType.Visible = false;
            trStaticRGNumber.Visible = false;
        }

        protected override void RenderReadOnlyView()
        {
            base.RenderReadOnlyView();

            trStaticDocType.Visible = true;
            trStaticRGNumber.Visible = true;
            trEditableDocType.Visible = false;
            trEditableRGNumber.Visible = false;
        }


        protected override void onProceedingToCheckout()
        {
            try
            {
                var deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
                string pickupPhone = string.Empty;
                if (deliveryType == DeliveryOptionType.Pickup)
                {
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                        pickupPhone = ShoppingCart.DeliveryInfo.Address.Phone;
                    // save phone number
                }
                else if (deliveryType == DeliveryOptionType.PickupFromCourier)
                {
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                        pickupPhone = ShoppingCart.DeliveryInfo.Address.Phone;

                    _pickupRrefList = ProductsBase.GetShippingProvider()
                                                  .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale,
                                                                                 DeliveryOptionType.PickupFromCourier);
                    if (_pickupRrefList == null || _pickupRrefList.Count == 0)
                    {
                        blErrors.Items.Clear();
                        ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(true));
                    }
                }
                blErrors.Items.Clear();
                base.onProceedingToCheckout();

                if (!string.IsNullOrEmpty(txtGreeting.Text))
                {
                    SessionInfo.ShoppingCart.GreetingMsg = txtGreeting.Text;
                }
                var errors = blErrors.DataSource as List<string> ?? new List<string>();

                // Procesing document info
                if (DeliveryOptionsInstructionsView.Visible && (deliveryType == DeliveryOptionType.Pickup || deliveryType == DeliveryOptionType.PickupFromCourier))
                {
                    string phone =
                          txtPickupPhone.Text.Replace("_", String.Empty)
                                         .Replace("(", String.Empty)
                                         .Replace(")", String.Empty)
                                         .Replace("-", String.Empty)
                                         .Trim();
                    if (String.IsNullOrEmpty(phone))
                    {
                        if (
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                           .IsPickupInstructionsRequired)
                        {
                            //Add required field error message for countries other than India
                            if(!errors.Contains(PlatformResources.GetGlobalResourceString("ErrorMessage","NoPickUpPhoneEntered")))
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                 "NoPickUpPhoneEntered"));
                        }
                    }
                    else if (
                        !Regex.IsMatch(phone,
                                       HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                      .PickUpPhoneRegExp))
                    {
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                    }
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                    {
                        var list = (List<string>)Session["pickupPhone"];
                        if (list != null && list.Count > 1)
                        {
                            list[1] = ShoppingCart.DeliveryInfo.Address.Phone;
                            Session["pickupPhone"] = list;
                        }
                        if (SessionInfo.IsReplacedPcOrder && (SessionInfo.ReplacedPcDistributorProfileModel != null))
                        {
                            ShoppingCart.DeliveryInfo.Address.Phone = (list != null && list.Count > 0) ? list[1] : string.Empty;
                        }
                        else
                        {
                            ShoppingCart.DeliveryInfo.Address.Phone = (list != null && list.Count > 0) ? list[0] : string.Empty; ;

                        }
                    }
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber)
                    {
                        string docInfo = txtRGNumber.Text;
                        if (String.IsNullOrEmpty(docInfo))
                        {
                            if (blErrors.DataSource == null)
                            {
                                errors = new List<string>
                                {
                                    PlatformResources.GetGlobalResourceString("ErrorMessage", "NoRGNumberSelected")
                                };
                            }
                            else
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoRGNumberSelected"));
                            }
                            blErrors.DataSource = errors;
                            blErrors.DataBind();

                            // Scroll to top.
                            var script = "setTimeout('window.scrollTo(0,0)', 100);";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script,
                                                                true);
                            OnCheckOutOptionsNotPopulated(this, null);
                        }
                        else
                        {
                            if (docInfo.Trim().Length>0)
                            {
                                var regexRules = @"^[0-9]{17}[0-9a-zA-Z]{1}$";
                               if(ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                                {
                                      regexRules = @"^[0-9a-zA-Z]*$";
                                }
                               
                                if (Regex.IsMatch(docInfo.Trim(), regexRules))
                                {
                                    if (ShoppingCart.DeliveryInfo != null)
                                    {
                                        ShoppingCart.DeliveryInfo.RGNumber = string.Format("{0}|{1}", ddlDocType.SelectedValue,
                                                                                           docInfo);
                                    }
                                }
                                else
                                {
                                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRGNumberSelected"));
                                    blErrors.DataSource = errors;
                                    blErrors.DataBind();
                                    var script = "setTimeout('window.scrollTo(0,0)', 100);";
                                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script,
                                                                        true);
                                    OnCheckOutOptionsNotPopulated(this, null);
                                }
                            }
                            else
                            {
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRGNumberSelected"));
                                blErrors.DataSource = errors;
                                blErrors.DataBind();
                                var script = "setTimeout('window.scrollTo(0,0)', 100);";
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script,
                                                                    true);
                                OnCheckOutOptionsNotPopulated(this, null);
                            }
                        }
                    }
                }
                else if (deliveryType == DeliveryOptionType.Shipping)
                {
                    var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone && OrderTotals != null && (OrderTotals.AmountDue - OrderTotals.Donation != 0))
                    {
                        string phone =
                            txtPickupPhone.Text.Replace("_", String.Empty)
                                          .Replace("(", String.Empty)
                                          .Replace(")", String.Empty)
                                          .Replace("-", String.Empty)
                                          .Trim();
                        if (String.IsNullOrEmpty(phone))
                        {
                            if (
                                HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                               .IsPickupInstructionsRequired)
                            {
                                //Add required field error message for countries other than India
                                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                     "NoPickUpPhoneEntered"));
                            }
                        }
                        else if (
                            !Regex.IsMatch(phone,
                                           HLConfigManager.Configurations.PickupOrDeliveryConfiguration
                                                          .PickUpPhoneRegExp))
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                        }
                        else
                        {
                            if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                            {
                                ShoppingCart.DeliveryInfo.Address.Phone = txtPickupPhone.Text;
                            }
                        }
                    }
                }
                else if (SessionInfo.IsEventTicketMode && deliveryType == DeliveryOptionType.Pickup)
                {
                    if (Session["pickupPhone"] != null)
                    {
                        var list = (List<string>)Session["pickupPhone"];
                        if (list != null && list.Count > 1 && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                        {
                            list[1] = ShoppingCart.DeliveryInfo.Address.Phone;
                            Session["pickupPhone"] = list;
                        }
                    }

                }

                bool isEmail = Regex.IsMatch(txtEmail.Text, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase);



                Dictionary<string, SKU_V01> _AllSKUS = (ProductsBase).ProductInfoCatalog.AllSKUs;
                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(DistributorID, "CN");
                PromotionInformation promoInfo = ChinaPromotionProvider.GetPCPromotion(distributorOrderingProfile.CNCustomorProfileID.ToString(), DistributorID);
                if (promoInfo.IsEligible)
                {

                    var totals = ShoppingCart.Totals as OrderTotals_V02;
                    var PcPromoOnly = new List<CatalogItem>();
                    decimal currentMonthTotalDue = 0;
                    if (promoInfo.MonthlyInfo != null)
                    {
                        if (promoInfo.MonthlyInfo.Count > 0)
                            currentMonthTotalDue = promoInfo.MonthlyInfo[0].Amount;
                    }

                    if (totals != null && totals.OrderFreight != null &&
                        totals.AmountDue + currentMonthTotalDue - totals.OrderFreight.FreightCharge >=
                        promoInfo.promoelement.AmountMinInclude)
                    {
                        bool promoItemAdded = false;
                        if (promoInfo.SKUList.Count > 0)
                        {
                            if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
                            {

                                foreach (CatalogItem t in promoInfo.SKUList)
                                {
                                    SKU_V01 sku;
                                    if (_AllSKUS.TryGetValue(t.SKU, out sku))
                                    {
                                        if ((
                                                ShoppingCartProvider.CheckInventory(t as CatalogItem_V01, 1,
                                                                                    ProductsBase.CurrentWarehouse) > 0 &&
                                                (CatalogProvider.GetProductAvailability(sku,
                                                                                        ProductsBase.CurrentWarehouse) ==
                                                 ProductAvailabilityType.Available)))
                                        {
                                            PcPromoOnly.Add(t as CatalogItem_V01);
                                        }

                                    }
                                }
                                if (PcPromoOnly.Any())
                                {
                                    if (
                                        this.ShoppingCart.CartItems.Any(
                                            cart => PcPromoOnly.Find(p => p.SKU == cart.SKU) != null))
                                    {
                                        promoItemAdded = true;

                                    }
                                    if (!promoItemAdded)
                                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                             "NonPromoSKUSelected"));
                                }

                            }
                        }
                    }
                }
                if (txtEmail.Text.Trim().Length > 0 && !isEmail)
                {
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidEmailAddress"));
                }
                if (deliveryType == DeliveryOptionType.PickupFromCourier)
                {
                    if (
                        ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                        ShoppingCart.DeliveryInfo.Address != null && !string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                    {
                        if (!Regex.IsMatch(ShoppingCart.DeliveryInfo.Address.Recipient, @"^[0-9\u4e00-\u9fa5 ]*$"))
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ChineseCharacterOnly"));
                        }
                    }

                }


                if (errors.Count > 0)
                {
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                    OnCheckOutOptionsNotPopulated(this, null);
                    hasValidationErrors.Value = "true";
                }
                else
                {
                    hasValidationErrors.Value = "false";
                }
            }
            catch (Exception ex)
            {
                string recepient = (ShoppingCart == null) ? "Null" : (ShoppingCart.DeliveryInfo == null) ? "Null Dev Info" : (ShoppingCart.DeliveryInfo.Address == null) ? "NullAddress" : (ShoppingCart.DeliveryInfo.Address.Recipient == null) ? "Null Recepient" : ShoppingCart.DeliveryInfo.Address.Recipient;
                HL.Common.Logging.LoggerHelper.Error(string.Format("CnCheckOutOptions: onProceedingToCheckout error :docinfo:{0}, Phone{1}, Email:{2} ,recipient:{3} ,stackTrace:{4}", txtRGNumber.Text, txtPickupPhone.Text, txtEmail.Text, recepient, ex));
            }
        }

        private Dictionary<string, string> GetAvailablePCPromo(PromotionInformation promo)
        {
            Dictionary<string, string> availPCPromoSKU = new System.Collections.Generic.Dictionary<string, string>();

            if (promo == null || promo.SKUList == null)
                return availPCPromoSKU;

            ProductInfoCatalog_V01 prodCat = CatalogProvider.GetProductInfoCatalog(Locale, ShoppingCart.DeliveryInfo.WarehouseCode);

            if (prodCat == null || prodCat.AllSKUs == null)
                return availPCPromoSKU;

            Dictionary<string, SKU_V01> _AllSKUS = prodCat.AllSKUs;
            SKU_V01 sku;

            foreach (CatalogItem t in promo.SKUList)
            {
                if (_AllSKUS.TryGetValue(t.SKU, out sku))
                {
                    if (!ChinaPromotionProvider.GetPCPromoCode(t.SKU).Trim().Equals("PCPromo"))
                    {
                        continue;
                    }

                    if ((ShoppingCartProvider.CheckInventory(t as CatalogItem_V01, 1, ProductsBase.CurrentWarehouse) > 0 &&
                        (CatalogProvider.GetProductAvailability(sku, ProductsBase.CurrentWarehouse) == ProductAvailabilityType.Available)))
                    {
                        availPCPromoSKU.Add(t.SKU, t.Description);
                    }
                }
            }

            return availPCPromoSKU;
        }

        private void DisplayContactInfo(bool isStatic)
        {
           ShoppingCart.SMSNotification = ShoppingCart.ReplaceAltPhoneNumber;
          
            ShoppingCart.DeliveryInfo.Address.AltPhone = ShoppingCart.AltPhoneNumber != null ? ShoppingCart.AltPhoneNumber : string.Empty;


            if (!isStatic)
            {
                // Ordered name
                if (SessionInfo.IsReplacedPcOrder)
                {
                    lblCustomerNameValue.Text = (SessionInfo.ReplacedPcDistributorProfileModel != null)
                                                    ? SessionInfo.ReplacedPcDistributorProfileModel.FirstNameLocal
                                                    : string.Empty;
                    PcMsgBox1.DisplaySubmitButton = true;
                }
                else
                {
                    lblCustomerNameValue.Text = DistributorProfileModel.FirstNameLocal;
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
                    if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option != null)
                    {
                            txtPickupPhone.Text = ShoppingCart.ReplaceAltPhoneNumber;
                    }
                }
                else
                {
                    divEditablePickupPhone.Visible = false;
                }
          
                //SMS Phone
                dvMobileNumber.Visible = false;
                dvSingleTextBoxNumber.Visible = false;
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
                {
                    dvMobileNumberReadOnly1.Visible = true;
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        if (SessionInfo.IsReplacedPcOrder && SessionInfo.ReplacedPcDistributorOrderingProfile != null)
                        {
                            if (SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers != null && SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Any())
                            {
                                var phoneNumber =
                                    SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary)
                                    as PhoneNumber_V03 != null
                                        ? SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Where(
                                            p => p.IsPrimary) as PhoneNumber_V03
                                        : SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.FirstOrDefault()
                                          as PhoneNumber_V03;
                                if (phoneNumber != null)
                                    ShoppingCart.AltPhoneNumber =
                                        txtSingleMobileNumber.Text = lblMobileNumberEntered1.Text = phoneNumber.Number;
                            }
                        }
                        else if (DistributorOrderingProfile.PhoneNumbers != null && DistributorOrderingProfile.PhoneNumbers.Any())
                        {
                            var phoneNumber =
                                DistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03 !=
                                null
                                    ? DistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03
                                    : DistributorOrderingProfile.PhoneNumbers.FirstOrDefault() as PhoneNumber_V03;
                            if (phoneNumber != null)
                                ShoppingCart.AltPhoneNumber =
                                    txtSingleMobileNumber.Text = lblMobileNumberEntered1.Text = phoneNumber.Number;
                        }


                        if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                        {
                            if (_pickupRrefList != null)
                            {
                                var courierLocation = (from p in _pickupRrefList where p.ID == ShoppingCart.DeliveryInfo.Id select p).FirstOrDefault();
                                if (courierLocation != null)
                                {
                                    var description = courierLocation.PickupLocationNickname;
                                    var listItem = DropdownNickName.Items.FindByText(description);
                                    if (listItem != null)
                                    {
                                        DropdownNickName.ClearSelection();
                                        listItem.Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Ordered name
                if (SessionInfo.IsReplacedPcOrder)
                {
                    lblCustomerNameValue1.Text = (SessionInfo.ReplacedPcDistributorProfileModel != null)
                                                    ? SessionInfo.ReplacedPcDistributorProfileModel.FirstNameLocal
                                                    : string.Empty;
                    PcMsgBox1.DisplaySubmitButton = false;
                }
                else
                {
                    lblCustomerNameValue1.Text = DistributorProfileModel.FirstNameLocal;
                }

                // Pick up Phone
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone)
                {
                        lblPickUpPhone.Text = ShoppingCart.ReplaceAltPhoneNumber;
                }
                else
                {
                    divStaticPickupPhone.Visible = false;
                }

                //SMS Phone
                dvMobileNumber.Visible = false;
                dvSingleTextBoxNumber.Visible = false;
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DisplaySingleTextBoxMobileNo)
                {
                    dvMobileNumberReadOnly1.Visible = false;
                    if (SessionInfo.IsReplacedPcOrder && SessionInfo.ReplacedPcDistributorOrderingProfile != null)
                    {
                        if (SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers != null && SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Any())
                        {
                            var phoneNumber =
                                SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary)
                                as PhoneNumber_V03 != null
                                    ? SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.Where(
                                        p => p.IsPrimary) as PhoneNumber_V03
                                    : SessionInfo.ReplacedPcDistributorOrderingProfile.PhoneNumbers.FirstOrDefault()
                                      as PhoneNumber_V03;
                            if (phoneNumber != null)
                                ShoppingCart.AltPhoneNumber =
                                    txtSingleMobileNumber.Text = lblMobileNumberEntered.Text = phoneNumber.Number;
                        }
                    }
                    else if (DistributorOrderingProfile.PhoneNumbers != null && (DistributorOrderingProfile.PhoneNumbers.Any()))
                    {
                        var phoneNumber =
                            DistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03 !=
                            null
                                ? DistributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03
                                : DistributorOrderingProfile.PhoneNumbers.FirstOrDefault() as PhoneNumber_V03;
                        if (phoneNumber != null)
                            ShoppingCart.AltPhoneNumber =
                                txtSingleMobileNumber.Text = lblMobileNumberEntered.Text = phoneNumber.Number;
                    }
                }
            }
        }

        protected new void ddlShippingMethod_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            base.ddlShippingMethod_OnSelectedIndexChanged(sender, e);
            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping && ShoppingCart.DeliveryInfo.Address != null)
            {
                var addrss_V01 = ShoppingCart.DeliveryInfo.Address.Address;
                if (addrss_V01 != null)
                {
                    var shippingProvider =
                        Providers.Shipping.ShippingProvider.GetShippingProvider("CN") as ShippingProvider_CN;
                    if (shippingProvider != null && ShoppingCart.DeliveryInfo.FreightCode != "0")
                    {
                        var warning = shippingProvider.GetUnsupportedExpressAddress(addrss_V01.StateProvinceTerritory, addrss_V01.City, addrss_V01.CountyDistrict, ShoppingCart.DeliveryInfo.FreightCode);
                        if (warning != null && warning.Trim().Length > 0)
                        {
                            UnSupportedPopupExtender.Show();
                            lblUnSupportedMessage.Text = warning;
                        }
                    }

                }
            }


  //          OnShippingMethodChanged(this, null);
        }


        protected void OnUnSupportedOk(object sender, EventArgs e)
        {
            UnSupportedPopupExtender.Hide();
        }
        #endregion
        public static ServiceProvider.ShippingSvc.Address_V01 CreateDefaultAddress()
        {
            var address = new ServiceProvider.ShippingSvc.Address_V01();
            address.Country = "CN";
            address.Line1 = "中路268号4801室";
            address.Line2 = "";
            address.Line3 = "";
            address.Line4 = "";
            address.City = "西藏";
            address.PostalCode = "200001";
            address.StateProvinceTerritory = "上海市";
            address.CountyDistrict = "黄埔区";
            return address;
        }
    }
}
