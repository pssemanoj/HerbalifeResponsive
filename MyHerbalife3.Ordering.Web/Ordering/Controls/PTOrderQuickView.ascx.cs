using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PTOrderQuickView : OrderQuickView
    {
        #region Methods

        protected override void loadData()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);

            // Retrieve the pickupoption from the cache, not hardcoded to pickup, unless nothing is loaded, so use default
            _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                   ShippingProvider.GetDefaultAddress());
            _pickupRrefList = ProductsBase.GetShippingProvider()
                                          .GetPickupLocationsPreferences(DistributorID, CountryCode);
            // Showing the right options according xml configurations.
            if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier &&
                DeliveryType.Items.FindByValue("PickupFromCourier") != null)
            {
                DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
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
                lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string) GetLocalResourceObject("ShipTo");
                NickNameReadonly.Text = NickName.Text = (string) GetLocalResourceObject("NickName");
            }
            else
            {
                if (CheckNoDeliveryType(DeliveryType))
                {
                    lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string) GetLocalResourceObject("ShipTo");
                    NickNameReadonly.Text = NickName.Text = (string) GetLocalResourceObject("NickName");
                }
                else
                {
                    lbShiptoOrPickupReadonly.Text =
                        lbShiptoOrPickup.Text = (string) GetLocalResourceObject("PickUpForm");
                    NickNameReadonly.Text = NickName.Text = (string) GetLocalResourceObject("Location");
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
                DropdownNickName.Visible = true;
                divAddress.Visible = true;
            }
            else
            {
                DropdownNickName.Visible = false;
                divAddress.Visible = false;
            }
        }

        protected override void populateDropdown(DropDownList deliveryType)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (null != deliveryType && deliveryType.Items.Count == 4 && deliveryType.Items[0].Value == "0")
                {
                    deliveryType.Items.RemoveAt(0);
                }

                if (deliveryType != null && deliveryType.Items.Count > 0)
                {
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        ListItem list = deliveryType.Items.FindByValue("Shipping");
                        if (list != null)
                        {
                            list.Selected = true;
                            populateShipping();
                        }
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                    {
                        ListItem list = deliveryType.Items.FindByValue("ShipToCourier");
                        if (list != null)
                        {
                            list.Selected = true;
                            populateShipping();
                        }
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        ListItem list = deliveryType.Items.FindByValue("PickupFromCourier");
                        if (list != null)
                        {
                            list.Selected = true;
                            populatePickupPreference();
                        }
                    }
                    else
                    {
                        ListItem list = deliveryType.Items.FindByValue("Pickup");
                        if (list != null)
                        {
                            list.Selected = true;
                            populatePickup();
                        }
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

        public override bool CheckNoDeliveryType(DropDownList DeliveryType)
        {
            if (null != DeliveryType
                //&& DeliveryType.Visible == false
                && null != ShoppingCart &&
                null != ShoppingCart.DeliveryInfo &&
                (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping ||
                 ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier))
            {
                return true;
            }
            return false;
        }

        protected override void setLinksVisiblity()
        {
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier ||
                deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                divLinks.Visible = true;
                LinkEdit.Visible = deliveryType == DeliveryOptionType.Shipping ||
                                   deliveryType == DeliveryOptionType.ShipToCourier;
            }
            else
            {
                divLinks.Visible = false;
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
                    pnlDeliveryOptionSelection.Visible = !lnAddAddress.Visible;
                    divNicknameInfoAndLink.Visible = !lnAddAddress.Visible;
                    if (CheckNoDeliveryType(DeliveryType))
                    {
                        divLinks.Visible = true;
                    }
                    else
                    {
                        divLinks.Visible = (deliveryType == DeliveryOptionType.Shipping ||
                                            deliveryType == DeliveryOptionType.ShipToCourier ||
                                            HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences);
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
                if (deliveryType == DeliveryOptionType.Unknown || deliveryType == DeliveryOptionType.Shipping ||
                    deliveryType == DeliveryOptionType.ShipToCourier)
                {
                    lnAddAddress.Text = (string) GetLocalResourceObject("AddShippingAddress");
                }
                else
                {
                    lnAddAddress.Text = String.Empty;
                }
                divLinks.Visible = false;
                if (deliveryType == DeliveryOptionType.Unknown &&
                    HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickup
                    && ProductsBase.SessionInfo.IsEventTicketMode == false)
                {
                    lnAddAddress.Visible = false;
                }
                else
                {
                    lnAddAddress.Visible = true;
                }
                if (deliveryType == DeliveryOptionType.ShipToCourier)
                {
                    lnAddAddress.Visible = true;
                }
            }
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
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
            }
            if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                LinkEdit.Visible = false;
                divLinks.Visible = true;
                divNicknameInfoAndLink.Visible = true;
            }
        }

        #endregion Methods

        #region EventHandler

        protected new void OnNickName_Databind(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ShoppingCart != null)
            {
                // there is deliveryInfo
                ShippingInfo deliveryInfo = ShoppingCart.DeliveryInfo;
                var textSelect = (string) GetLocalResourceObject("TextSelect");
                DeliveryOptionType deliveryOptionTypeFromDowndown = getDeliveryOptionTypeFromDropdown(DeliveryType);

                if (deliveryInfo != null)
                {
                    if (CheckNoDeliveryType(DeliveryType) &&
                        deliveryOptionTypeFromDowndown == DeliveryOptionType.Unknown)
                    {
                        deliveryOptionTypeFromDowndown = DeliveryOptionType.Shipping;
                    }

                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
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

                    else if (deliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Pickup)
                        {
                            var varPref = _pickupLocations.Where(f => f.Id == deliveryInfo.Id);
                            if (varPref.Count() > 0)
                            {
                                ListItem selected = ddl.Items.FindByValue(deliveryInfo.Id.ToString());
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
                    else if (deliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.PickupFromCourier)
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
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                        }
                    }
                    else if (deliveryInfo.Option == DeliveryOptionType.ShipToCourier)
                    {
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.ShipToCourier)
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
                                //DropdownNickName.Attributes.Add("style", "display:none");
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
                }
                else // no delivery option
                {
                    if (deliveryOptionTypeFromDowndown == DeliveryOptionType.Shipping)
                    {
                        int id = 0;
                        if (int.TryParse(DropdownNickName.SelectedValue, out id))
                        {
                            DeliveryOption deliveryOption = getSelectedAddress(id,
                                                                               (DeliveryOptionType)
                                                                               Enum.Parse(typeof (DeliveryOptionType),
                                                                                          DeliveryType.SelectedValue));
                            if (deliveryOption != null)
                            {
                                ListItem selected = ddl.Items.FindByValue(deliveryOption.Id.ToString());
                                if (selected != null)
                                {
                                    selected.Selected = true;
                                    updateShippingInfo(deliveryOption.ID, ProductsBase.DeliveryOptionID,
                                                       DeliveryOptionType.Shipping);
                                }
                                pAddress.InnerHtml =
                                    ProductsBase.GetShippingProvider()
                                                .FormatShippingAddress(deliveryOption, deliveryOptionTypeFromDowndown,
                                                                       ShoppingCart != null &&
                                                                       ShoppingCart.DeliveryInfo != null
                                                                           ? ShoppingCart.DeliveryInfo.Description
                                                                           : string.Empty, true);
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

        protected new void OnNickNameChanged(object sender, EventArgs e)
        {
            if (ShoppingCart != null && DeliveryType.SelectedValue != null && DeliveryType.SelectedValue != "0")
            {
                handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);
                lbShiptoOrPickup.Visible = true;
            }
        }

        protected override void handleNicknameChanged(DropDownList deliveryType, DropDownList nickNameDropdown,
                                                      HtmlGenericControl pAddress)
        {
            DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(deliveryType);
            ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
            int deliveryOptionId = 0, shippingAddressId = 0;
            if (shippingInfo != null)
            {
                shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping ||
                                    deliveryOptionType == DeliveryOptionType.ShipToCourier
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : shippingInfo.Address.ID;
                deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping ||
                                   deliveryOptionType == DeliveryOptionType.ShipToCourier
                                       ? shippingInfo.Id
                                       : int.Parse(nickNameDropdown.SelectedValue);
            }
            else
            {
                shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping ||
                                    deliveryOptionType == DeliveryOptionType.ShipToCourier
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : 0;
                deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping ||
                                   deliveryOptionType == DeliveryOptionType.ShipToCourier
                                       ? 0
                                       : int.Parse(nickNameDropdown.SelectedValue);
            }
            updateShippingInfo(shippingAddressId,
                               deliveryOptionId,
                               deliveryOptionType);
            shippingInfo = ShoppingCart.DeliveryInfo;
            if (shippingInfo != null)
                setAddressByNickName(shippingInfo.Address == null ? null : ShoppingCart.DeliveryInfo.Address, pAddress,
                                     deliveryType);
            else
                setAddressByNickName(null, pAddress, deliveryType);
        }

        protected new void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            lblNickName.Visible = false;
            handleDeliveryTypeChanged(DeliveryType, DropdownNickName);
            setLinksVisiblity();
        }

        protected override void handleDeliveryTypeChanged(DropDownList DeliveryType, DropDownList DropdownNickName)
        {
            CheckDSMailingAddress(false, null);
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                populateShipping();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                populatePickupPreference();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
            }
            else
            {
                if (CountryCode == "BR")
                {
                    var mailingAdd = new ShippingAddress_V01(1, DistributorProfileModelHelper.DistributorName(DistributorProfileModel),
                                                             ObjectMappingHelper.Instance.GetToShipping(DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, DistributorID, CountryCode)),
                                                             string.Empty, string.Empty, false, string.Empty,
                                                             DateTime.Now);
                    if (CheckDSMailingAddress(true, mailingAdd))
                    {
                        populatePickup();
                        setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                                 ? null
                                                 : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                                                        ? ShoppingCart.DeliveryInfo.Address
                                                        : null));
                    }
                }
                else
                {
                    populatePickup();
                    setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                             ? null
                                             : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                                                    ? ShoppingCart.DeliveryInfo.Address
                                                    : null));
                }
            }
            showShiptoOrPickup(IsStatic);
        }

        protected override DeliveryOption getSelectedAddress(int id, DeliveryOptionType optionType,
                                                             DropDownList deliveryTypeDropdown)
        {
            if (optionType == DeliveryOptionType.ShipToCourier)
                optionType = DeliveryOptionType.Shipping;
            return base.getSelectedAddress(id, optionType, deliveryTypeDropdown);
        }

        protected new void setAddressByNickName(ShippingAddress_V01 address)
        {
            if (setAddressByNickName(address, pAddress, DeliveryType))
            {
                divLinks.Visible = true;
                showShiptoOrPickup(false);
            }
        }

        protected override void loadAddressEditMode()
        {
            int id = 0;
            if (DropdownNickName.SelectedIndex >= 0)
            {
                if (int.TryParse(DropdownNickName.SelectedValue, out id))
                {
                    DeliveryOptionType option = getDeliveryOptionTypeFromDropdown(DeliveryType);
                    //DeliveryOption deliveryOption = getSelectedAddress(id, (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), DeliveryType.SelectedValue));
                    DeliveryOption deliveryOption = null;
                    if (option == DeliveryOptionType.Shipping || option == DeliveryOptionType.ShipToCourier)
                    {
                        var addresses = _shippingAddresses.Where(s => s.Id == id);
                        if (addresses.Count() == 0)
                        {
                            addresses = _shippingAddresses.Where(s => s.IsPrimary);
                        }
                        deliveryOption = addresses.Count() == 0 ? null : addresses.First();
                    }
                    else if (option == DeliveryOptionType.PickupFromCourier)
                    {
                        var varPref = _pickupRrefList.Where(f => f.ID == id);
                        if (varPref.Count() > 0)
                        {
                            int PickupLocationID = varPref.First().PickupLocationID;

                            List<DeliveryOption> doList = ShippingProvider.GetDeliveryOptions(option,
                                                                                              new ShippingAddress_V01
                                                                                                  {
                                                                                                      Address =
                                                                                                          new Address_V01
                                                                                                              {
                                                                                                                  Country = "PT"
                                                                                                              }
                                                                                                  });
                            if (doList != null)
                            {
                                deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                            }
                        }
                    }
                    else
                    {
                        if (_pickupLocations != null && _pickupLocations.Count > 0)
                        {
                            var varPickupLocation = _pickupLocations.Where(p => p.Id == id);
                            if (varPickupLocation.Count() > 0)
                            {
                                deliveryOption = varPickupLocation.First();
                            }
                        }
                    }
                    if (deliveryOption != null)
                    {
                        if (option == DeliveryOptionType.Unknown && CheckNoDeliveryType(DeliveryType))
                        {
                            option = DeliveryOptionType.Shipping;
                        }
                        if (option == DeliveryOptionType.ShipToCourier)
                            option = DeliveryOptionType.Shipping;
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

        protected override bool setAddressByNickName(ShippingAddress_V01 address, HtmlGenericControl pAddress,
                                                     DropDownList deliveryType)
        {
            pAddress.Visible = address != null;
            if (address != null)
            {
                DeliveryOptionType option = getDeliveryOptionTypeFromDropdown(deliveryType);
                if ((option == DeliveryOptionType.Unknown && CheckNoDeliveryType(deliveryType)) ||
                    option == DeliveryOptionType.ShipToCourier)
                {
                    option = DeliveryOptionType.Shipping;
                }

                pAddress.InnerHtml =
                    ProductsBase.GetShippingProvider()
                                .FormatShippingAddress(new DeliveryOption(address), option,
                                                       ShoppingCart != null && ShoppingCart.DeliveryInfo != null
                                                           ? ShoppingCart.DeliveryInfo.Description
                                                           : string.Empty, true);
                if (CheckNoDeliveryType(deliveryType) && !string.IsNullOrEmpty(pAddress.InnerHtml))
                {
                    return true;
                }
            }
            else
                pAddress.InnerHtml = "";
            return false;
        }

        private bool CheckDSMailingAddress(bool check, ShippingAddress_V01 address)
        {
            if (check)
            {
                //if (address.Address.Country != this.CountryCode)
                //{
                //    this.errDRFraud.Text = GetLocalResourceObject("MailingAddNotBR") as string;
                //    this.errDRFraud.Visible = true;
                //    return false;
                //}

                var mailingaddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing,
                                                                                           DistributorID,
                                                                                           CountryCode);
                if (mailingaddress != null)
                {
                    if (mailingaddress.Country != CountryCode)
                    {
                        errDRFraud.Text = GetLocalResourceObject("MailingAddNotBR") as string;
                        errDRFraud.Visible = true;
                        return false;
                    }
                }

            }
            errDRFraud.Visible = false;
            errDRFraud.Text = string.Empty;
            return true;
        }

        protected new void DeleteClicked(object sender, EventArgs e)
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

        #endregion EventHandler

        #region SubscriptionEvents

        protected override void reload()
        {
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.PickupFromCourier)
            {
                _pickupRrefList =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode);
                populateDropdown();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                showHideAddressLink();
            }
            else
            {
                base.reload();
            }
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public void OnPickupPreferenceCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            if (args != null)
            {
                updateShippingInfo(ProductsBase.ShippingAddresssID, args.DeliveryOptionId,
                                   DeliveryOptionType.PickupFromCourier);
            }
            reload();
            upOrderQuickView.Update();
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
                    updateShippingInfo(ProductsBase.ShippingAddresssID, pref.PickupLocationID,
                                       DeliveryOptionType.PickupFromCourier);
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

        #endregion SubscriptionEvents
    }
}