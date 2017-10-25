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
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class BrGrOrderQuickView : OrderQuickView
    {
        // [Publishes(MyHLEventTypes.ShippingInfoNotFilled)]
        // public event EventHandler ShippingInfoNotFilled;

        private string CourierTypeSelected
        {
            get { return ProductsBase.GetShippingProvider().GetCourierTypeBySelection(DeliveryType.SelectedValue ?? "PickupFromCourier"); }
        }

        #region Methods

        protected override void loadData()
        {
            if (CountryCode == "CN")
            {
                _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
                if (_shippingAddresses != null)
                {
                    _shippingAddresses =
                        _shippingAddresses.Where(
                            x =>
                            !x.Address.Line4.StartsWith("MALO") && !x.Address.Line4.StartsWith("DE") &&
                            !x.Address.Line4.StartsWith("MAEN") && !x.Address.Line4.StartsWith("PALO") &&
                            !x.Address.Line4.StartsWith("PAEN")).ToList();

                }
            }
            else
            {
                _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            }
            if (CountryCode == "BR")
            {
                var address = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, DistributorID, CountryCode);
                var mailingAdd = new ShippingAddress_V01(1, DistributorProfileModelHelper.DistributorName(DistributorProfileModel),
                                                        address != null ? ObjectMappingHelper.Instance.GetToShipping(address) : null, string.Empty,
                                                        string.Empty, false, string.Empty, DateTime.Now);
                if (CheckDSMailingAddress(true, mailingAdd))
                {
                    //_pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup, mailingAdd);
                    _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                           new ShippingAddress_V01
                                                                           {
                                                                               Address = new Address_V01
                                                                               {
                                                                                   StateProvinceTerritory =
                                                                                               getDSMailingAddressState(),
                                                                                   Country = CountryCode,
                                                                               }
                                                                           }
                        );
                }
            }
            else
            {
                if (ProductsBase.GetShippingProvider().HasAdditionalPickup())
                {
                    _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                           ShippingProvider.GetDefaultAddress(), DeliveryType.SelectedValue);
                }
                else
                {
                    _pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                           ShippingProvider.GetDefaultAddress());
                    if(CountryCode == "BY")
                    {
                        var eventId = 0;
                        bool isQualified = false;
                        string desc = HL.Common.Configuration.Settings.GetRequiredAppSetting("ByExtravaganza", "");
                        if (int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out eventId) && eventId > 0)
                        {
                            isQualified = DistributorOrderingProfileProvider.IsEventQualified(eventId, "ru-BY");
                        }
                        if (!isQualified)
                            _pickupLocations = _pickupLocations.Where(s => !s.Description.Contains(desc)).ToList();
                    }
                }
                // Retrieve the pickupoption from the cache, not hardcoded to pickup, unless nothing is loaded, so use default
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && SessionInfo != null && SessionInfo.IsEventTicketMode)
                {
                    if (_pickupLocations != null) _pickupLocations = _pickupLocations.Where(x => x.IsETO).ToList();
                }
                else if (HLConfigManager.Configurations.DOConfiguration.IsChina && SessionInfo != null && !SessionInfo.IsEventTicketMode)
                {
                    if (_pickupLocations != null) _pickupLocations = _pickupLocations.Where(x => !x.IsETO).ToList();
                }

            }
            // Pickup precerence list will be used for PickupFromCourier since ShipToCourier uses shipping address format
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

            // Showing the right options according xml configurations.
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

            if (IsChina && SessionInfo.IsEventTicketMode)
            {
                if (DeliveryType.Items.FindByValue("PickupFromCourier") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier"));
                if (DeliveryType.Items.FindByValue("PickupFromCourier1") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("PickupFromCourier1"));
                if( DeliveryType.Items.FindByValue("ShipToCourier") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("ShipToCourier"));
                if (DeliveryType.Items.FindByValue("Shipping") != null)
                    DeliveryType.Items.Remove(DeliveryType.Items.FindByValue("Shipping"));
                var pickupVal = HL.Common.ValueObjects.DeliveryOptionType.Pickup.ToString();

                var pickupOp = DeliveryType.Items.FindByValue(pickupVal);
                if (pickupOp != null) pickupOp.Selected = true;

                if (ShoppingCart.DeliveryInfo != null)
                    ShoppingCart.DeliveryInfo.Option = DeliveryOptionType.Pickup;
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
            if (selectedOption == DeliveryOptionType.PickupFromCourier && (CountryCode == "US" || CountryCode == "CA" || CountryCode == "PR" || CountryCode == "TH"))
            {
                lbShiptoOrPickupReadonly.Text = lbShiptoOrPickup.Text = (string)GetLocalResourceObject("PickUpForm");
                NickNameReadonly.Text = NickName.Text = (string)GetLocalResourceObject("Location");
                this.LinkAdd.Text = GetLocalResourceObject("ChangeFedexResource") as string;    
            }
        }

        protected void populatePickupPreference()
        {
            DropdownNickName.Items.Clear();
            if (_pickupRrefList != null && _pickupRrefList.Count > 0)
            {
                if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                {
                    
                    var couriers = ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, DeliveryOptionType.PickupFromCourier, CourierTypeSelected);
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

        protected override void populatePickup()
        {
            if (_pickupLocations != null)
            {
                populatePickup(DropdownNickName, lblNickName);
                if (ProductsBase.GetShippingProvider().HasAdditionalPickup() && ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
                {
                    var pickupLocations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup, ShippingProvider.GetDefaultAddress(), "Pickup1");
                    var selectedLocation =
                        pickupLocations.FirstOrDefault(l => l.Id == ShoppingCart.DeliveryInfo.Id);
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

                divAddress.Visible = true;
            }
            else
            {
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
                        if (ProductsBase.GetShippingProvider().HasAdditionalPickupFromCourier())
                        {
                            var pickupLocations = ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(ShoppingCart.DistributorID, ShoppingCart.CountryCode, null);
                            var selectedLocation =
                                pickupLocations.FirstOrDefault(l => l.ID == ShoppingCart.DeliveryInfo.Id);

                            ListItem list = (selectedLocation != null &&
                                             !string.IsNullOrEmpty(selectedLocation.PickupLocationType) && selectedLocation.PickupLocationType == "Agency")
                                                ? deliveryType.Items.FindByValue("PickupFromCourier1")
                                                : deliveryType.Items.FindByValue("PickupFromCourier");
                            if (list != null)
                            {
                                list.Selected = true;
                                populatePickupPreference();
                            }
                        }
                        else
                        {
                            ListItem list = deliveryType.Items.FindByValue("PickupFromCourier");
                            if (list != null)
                            {
                                list.Selected = true;
                                populatePickupPreference();
                            }
                        }
                    }
                    else
                    {
                        ListItem list = deliveryType.Items.FindByValue("Pickup");

                        if (ProductsBase.GetShippingProvider().HasAdditionalPickup())
                        {
                            // Getting location for Pickup1 to validate the delivery option to select
                            var locations = ShippingProvider.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                ShippingProvider.GetDefaultAddress(), "Pickup1");
                            if (ShoppingCart.DeliveryInfo != null)
                            {
                                if (locations.FirstOrDefault(l => l.Id == ShoppingCart.DeliveryInfo.Id) != null)
                                {
                                    list = deliveryType.Items.FindByValue("Pickup1");
                                }
                            }
                        }

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
                    if (IsChina && SessionInfo.IsEventTicketMode)
                    {
                        var pickupVal = HL.Common.ValueObjects.DeliveryOptionType.Pickup.ToString();

                        var pickupOp = deliveryType.Items.FindByValue(pickupVal);
                        if (pickupOp != null) pickupOp.Selected = true;

                        populatePickup();
                    }
                    else
                    {
                        // default
                        deliveryType.Items[0].Selected = true;
                        populateShipping();
                    }
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
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                divLinkEdit.Visible = divLinkDelete.Visible = true;
                divLinks.Visible = true;
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                divLinkEdit.Visible = false;
                divLinkDelete.Visible = (_pickupRrefList.Count > 0 && DropdownNickName.SelectedValue != "0");
                divLinks.Visible = true;
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
                divLinkDelete.Visible = (_pickupRrefList.Count > 0 && DropdownNickName.SelectedValue != "0");
                divLinkEdit.Visible = false;
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
                        if (deliveryOptionTypeFromDowndown == DeliveryOptionType.PickupFromCourier && _pickupRrefList != null)
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
                                selected.Selected = true;
                            setAddressByNickName(
                                deliveryInfo.Address == null ? null : ShoppingCart.DeliveryInfo.Address,
                                pAddress, DeliveryType);
                        }
                        else
                        {
                            ddl.Items.Insert(0, new ListItem(textSelect, "0"));
                            ddl.Items[0].Selected = true;
                                setAddressByNickName(null,pAddress, DeliveryType);
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
                int selectedValue = 0;

                if(int.TryParse(DropdownNickName.SelectedValue, out selectedValue))
                {
                    handleNicknameChanged(DeliveryType, DropdownNickName, pAddress);
                    lbShiptoOrPickup.Visible = true;
                }
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

            ProductsBase.HasToDisplaySaveCopyError();
        }

        protected new void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            lblNickName.Visible = false;
            handleDeliveryTypeChanged(DeliveryType, DropdownNickName);
            setLinksVisiblity();
        }

        protected override void handleDeliveryTypeChanged(DropDownList DeliveryType, DropDownList DropdownNickName)
        {
            this.LinkAdd.Text = GetLocalResourceObject("LinkAddResource1.Text") as string;
            CheckDSMailingAddress(false, null);
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (IsChina)
                ShoppingCart.PcLearningDeliveryoption = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), deliveryType.ToString());
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            if (deliveryType == DeliveryOptionType.Shipping || deliveryType == DeliveryOptionType.ShipToCourier)
            {
                populateShipping();
                DropdownNickName.Visible = true;
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
                divAddress.Visible = true;
            }
            else if (deliveryType == DeliveryOptionType.PickupFromCourier)
            {
                populatePickupPreference();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));

                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AutoDisplayPickUpFromCourierPopUp)
                {
                    ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(CourierTypeSelected));
                }
                if (CountryCode == "US" || CountryCode == "CA" || CountryCode == "PR" || CountryCode == "TH")
                {
                    this.LinkAdd.Text = GetLocalResourceObject("ChangeFedexResource") as string;    
                }
            }
            else
            {
                if (CountryCode == "BR")
                {
                    DistributorProfileModel model = DistributorProfileModel;
                    var mailingAdd = new ShippingAddress_V01(1, DistributorProfileModelHelper.DistributorName(model),
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
            ProductsBase.HasToDisplaySaveCopyError();

            //if (this.CountryCode == "BR")
            //{
            //    if (deliveryType == DeliveryOptionType.Pickup || deliveryType == DeliveryOptionType.ShipToCourier)
            //    {
            //        if (_shippingAddresses == null || _shippingAddresses.Count == 0)
            //        {
            //            ShippingInfoNotFilled(this, null);
            //        }
            //    }
            //}
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
                    else if (option == DeliveryOptionType.PickupFromCourier && _pickupRrefList != null)
                    {
                        var varPref = _pickupRrefList.Where(f => f.ID == id);
                        if (varPref.Count() > 0)
                        {
                            int PickupLocationID = varPref.First().PickupLocationID;
                            List<DeliveryOption> doList = null;
                            switch (CountryCode)
                            {
                                case "IN":
                                    doList = ShippingProvider.GetDeliveryOptions(option, null);
                                    break;
                                case "CL":
                                    doList = ShippingProvider.GetDeliveryOptions(option, new ShippingAddress_V01
                                        {
                                            Alias = this.CourierTypeSelected,
                                            Address = new Address_V01
                                                {
                                                    Country = CountryCode
                                                }
                                        });
                                    break;
                                case "US":
                                    var providerUS = new ShippingProvider_US();
                                    doList = providerUS.GetDeliveryOptionForDistributor(this.DistributorID,
                                                                                      DeliveryOptionType.PickupFromCourier);
                                    break;
                                case "CA":
                                    var providerCA = new ShippingProvider_CA();
                                    doList = providerCA.GetDeliveryOptionForDistributor(this.DistributorID,
                                                                                      DeliveryOptionType.PickupFromCourier);
                                    break;
                                case "PR":
                                    var providerPR = new ShippingProvider_PR();
                                    doList = providerPR.GetDeliveryOptionForDistributor(this.DistributorID,
                                                                                      DeliveryOptionType.PickupFromCourier);
                                    break;
                                case "TH":
                                    var providerTH = new ShippingProvider_TH();
                                    doList = providerTH.GetDeliveryOptionForDistributor(this.DistributorID, DeliveryOptionType.PickupFromCourier);
                                    break;
                                case "KZ":
                                    doList = ShippingProvider.GetDeliveryOptions(option, null);
                                    break;
                                case "UA":
                                    doList = ShippingProvider.GetDeliveryOptions(option, null);
                                    if (doList != null && doList.Exists(d => d.Id == PickupLocationID))
                                    {
                                        deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                                        if (deliveryOption.Address != null && !string.IsNullOrEmpty(deliveryOption.Address.StateProvinceTerritory))
                                        {
                                            doList = ShippingProvider.GetDeliveryOptions(option, new ShippingAddress_V01 { Address = deliveryOption.Address });
                                        }
                                    }
                                    break;
                                default:
                                    doList = ShippingProvider.GetDeliveryOptions(option, new ShippingAddress_V01
                                        {
                                            Address = new Address_V01
                                                {
                                                    Country =
                                                        CountryCode
                                                }
                                        });
                                    break;
                            }

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
                var mailingaddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing,
                                                                                           DistributorID,
                                                                                           CountryCode);
                if (mailingaddress != null && mailingaddress.Country != address.Address.Country)
                {
                    errDRFraud.Text = GetLocalResourceObject("MailingAddNotBR") as string;
                    errDRFraud.Visible = true;
                    return false;
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
                DeliveryOption deliveryOption = DeliveryType.SelectedValue != "PickupFromCourier1" ? getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                   (DeliveryOptionType)
                                                                   Enum.Parse(typeof (DeliveryOptionType),
                                                                   DeliveryType.SelectedValue)) : getSelectedAddress(int.Parse(DropdownNickName.SelectedValue),
                                                                   (DeliveryOptionType)
                                                                   Enum.Parse(typeof (DeliveryOptionType),
                                                                              "PickupFromCourier"));
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
                        var deliveryType =
                            HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier
                                ? DeliveryOptionType.PickupFromCourier
                                : DeliveryOptionType.ShipToCourier;
                        List<PickupLocationPreference_V01> pickupLocationPreferences = CountryCode == "CL" ?
                            ProductsBase.GetShippingProvider()
                            .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, deliveryType, this.CourierTypeSelected) : 
                            ProductsBase.GetShippingProvider()
                                        .GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, deliveryType);
                        if (int.Parse(DropdownNickName.SelectedValue) != 0)
                        {
                            PickupLocationPreference_V01 pref =
                                pickupLocationPreferences.Find(p => p.ID == int.Parse(DropdownNickName.SelectedValue));
                            if (pref != null)
                            {
                                var args = new DeliveryOptionEventArgs(pref.PickupLocationID, shippingInfo.Name);
                                args.CourierType = CourierTypeSelected;
                                ucShippingInfoControl.ShowPopupForPickup(CommandType.Delete, args);
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

                var deliveryType = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowPickUpFromCourier
                                       ? DeliveryOptionType.PickupFromCourier
                                       : DeliveryOptionType.ShipToCourier;
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

                populateDropdown();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
                //if(IsChina)
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
                                                                         (Page as ProductsBase).CountryCode, this.Locale, DeliveryOptionType.PickupFromCourier);
                if (_pickupRrefList != null && _pickupRrefList.Count > 0)
                {
                    if (string.IsNullOrEmpty(args.Description))
                    {
                        PickupLocationPreference_V01 pref = _pickupRrefList.First();
                        updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
                    }
                    else
                    {
                            if(_pickupRrefList.Count(pa => pa.IsPrimary == true) > 0)
                            {
                                var pref = _pickupRrefList.Where(pa => pa.IsPrimary == true).First();
                                updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
                            }
                            else
                            {
                                var pref = _pickupRrefList.First();
                                updateShippingInfo(ProductsBase.ShippingAddresssID, pref.ID, DeliveryOptionType.PickupFromCourier);
                            }
                    }
                }
                else
                {
                    ProductsBase.ClearCart();
                }
                populateDropdown();
            }
            //   ucShippingInfoControl.Hide();
            reload();
            if (_pickupRrefList == null || _pickupRrefList.Count == 0)
            {
                setAddressByNickName(null);
                ProductsBase.ClearCart();
            }
        }

        public override void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
            var deliveryOptionType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            base.onShippingAddressCreated(args);
            if (deliveryOptionType == DeliveryOptionType.ShipToCourier)
            {
                this.ShoppingCart.DeliveryInfo.Option = deliveryOptionType;
                populateDropdown();                
            }
        }

        #endregion SubscriptionEvents

        protected new void AddClicked(object sender, EventArgs e)
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
                ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(CourierTypeSelected));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsChina)
            {
                ShippingPickupOptions.Text = GetLocalResourceString("ShippingPickupOptionsResource1.Text");
            }
        }

        string GetLocalResourceString(string key)
        {
            var rslt = GetLocalResourceObject(key);
            if (rslt == null) return null;

            return (rslt != null) ? rslt.ToString() : null;
        }
    }
}