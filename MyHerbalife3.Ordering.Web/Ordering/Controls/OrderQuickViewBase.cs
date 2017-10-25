using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using System.Globalization;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public enum CommandType
    {
        Add,
        Edit,
        Delete
    }

    public class OrderQuickViewBase : UserControlBase
    {
        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler BaseQuoteRetrieved;

        [Publishes(MyHLEventTypes.WarehouseChanged)]
        public event EventHandler BaseWarehouseChanged;

        protected List<DeliveryOption> _shippingAddresses;
        protected List<PickupLocationPreference_V01> _pickupRrefList;
        protected List<DeliveryOption> _pickupLocations;

        public bool HideInventoryOption { get; set; }

        //public bool ShowShippingMethod { get; set; }
        public bool IsStatic { get; set; }

        protected bool hasShippingAddresses
        {
            get { return _shippingAddresses != null && _shippingAddresses.Count > 0; }
        }

        protected bool hasPreferences
        {
            get { return (_pickupRrefList != null && _pickupRrefList.Count > 0); }
        }

        protected void reloadShipping()
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
        }

        protected virtual bool hasNoPreference()
        {
            IShippingProvider ShippingProvider = ProductsBase.GetShippingProvider();
            List<DeliveryOption> shippingAddresses =
                ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            return (shippingAddresses == null || shippingAddresses.Count == 0);
        }

        protected virtual void updateShippingInfo(int shippingAddressId, int deliveryOptionId, DeliveryOptionType option)
        {
            if (ShoppingCart != null)
            {
                bool updated = false;
                ShippingInfo deliveryInfo = ShoppingCart.DeliveryInfo;
                string previousWarehouse = deliveryInfo != null ? deliveryInfo.WarehouseCode : string.Empty;
                string previousFreightCode = deliveryInfo != null ? deliveryInfo.FreightCode : string.Empty;
                Address_V01 oldAddress = deliveryInfo != null ? deliveryInfo.Address.Address : null;

                if (deliveryInfo == null ||
                    (deliveryInfo != null && deliveryInfo.Address.ID != shippingAddressId) ||
                    (deliveryInfo != null && deliveryInfo.Id != deliveryOptionId) ||
                    (deliveryInfo != null && deliveryInfo.Option != option))
                {
                    updated = true;
                    ShoppingCart.UpdateShippingInfo(shippingAddressId, deliveryOptionId, option);
                }
                if (updated)
                {
                    if (ShoppingCart.DeliveryInfo != null &&
                        ShoppingCart.DeliveryInfo.WarehouseCode != previousWarehouse)
                    {
                        var results = HLRulesManager.Manager.ProcessCart(ShoppingCart,
                                                                          ShoppingCartRuleReason
                                                                              .CartWarehouseCodeChanged);
                        BaseWarehouseChanged(this, null);
                    }
                    if (ShoppingCart.shouldRecalculate(previousFreightCode, oldAddress))
                    {
                        //ShoppingCart.Calculate();
                        BaseQuoteRetrieved(this, null);
                    }
                }
            }
        }

        protected virtual void reload()
        {
            reloadShipping();
            populateDropdown();
            showHideAddressLink();
            setAddressByNickName(ShoppingCart.DeliveryInfo == null ? null : ShoppingCart.DeliveryInfo.Address);
        }

        protected virtual void populateDropdown()
        {
        }

        protected virtual void showHideAddressLink()
        {
        }

        protected virtual void setAddressByNickName(ShippingAddress_V01 address)
        {
        }

        public virtual bool CheckNoDeliveryType(DropDownList DeliveryType)
        {
            if (null != DeliveryType && DeliveryType.Visible == false && null != ShoppingCart &&
                null != ShoppingCart.DeliveryInfo && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return true;
            }
            return false;
        }

        protected virtual void shippingAddressCreated(ShippingAddress_V02 shippingAddress)
        {
            updateShippingInfo(shippingAddress.ID, ProductsBase.DeliveryOptionID, DeliveryOptionType.Shipping);
            reload();
        }

        protected virtual void shippingAddressChanged(ShippingAddress_V02 shippingAddress)
        {
            try
            {
            var deliveryOption = Session["OQVOldaddress"] as DeliveryOption;
            if (deliveryOption != null)
            {
                if (shippingAddress != null)
                {
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        DeliveryOption NewDeliveryOption = GetDeliveryOptionIDFromShippingAddress(shippingAddress.ID,
                                                                                                  DeliveryOptionType
                                                                                                      .Shipping);
                            //bug fix Bug 226409 because GetDeliveryOptionIDFromShippingAddress can return null need to put null validation
                            if(NewDeliveryOption!=null)
                        updateShippingInfo(shippingAddress.ID, NewDeliveryOption.Id, DeliveryOptionType.Shipping);
                    }
                }
            }
            reload();
        }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error in shippingAddressChanged: shippingAddress {0},error:{1}",(shippingAddress==null)?"null":shippingAddress.ID.ToString(),ex));
            }
            
        }

        protected virtual void shippingAddressDeleted(ShippingAddress_V02 shippingAddress)
        {
            _shippingAddresses = ShippingProvider.GetShippingAddresses(DistributorID, Locale);
            // this is to get primary address, if previous one deleted
            //DeliveryOption deliveryOption = getSelectedAddress(args.ShippingAddress.ID, DeliveryOptionType.Shipping);
            if (_shippingAddresses != null)
            {
                //After delete pass the primary shipping ID to update shipping info..
                if (_shippingAddresses.Count > 0)
                {
                    DeliveryOption primaryDeliveryOption = _shippingAddresses.Find(s => s.IsPrimary);
                    if (primaryDeliveryOption != null)
                    {
                        updateShippingInfo(primaryDeliveryOption.ID, ProductsBase.DeliveryOptionID,
                                           DeliveryOptionType.Shipping);
                    }
                }
                else
                {
                    ProductsBase.ClearCart();
                }
                reload();
            }
        }

        protected virtual void onProceedingToCheckout()
        {
        }

        public virtual void onShippingAddressDeleted(ShippingAddressEventArgs args)
        {
        }

        public virtual void onShippingAddressCreated(ShippingAddressEventArgs args)
        {
        }

        public virtual void onShippingAddressUpdated(ShippingAddressEventArgs args)
        {
        }

        public virtual void OnPickupPreferenceCreated(DeliveryOptionEventArgs args)
        {
        }

        public virtual void OnPickupPreferenceDeleted(DeliveryOptionEventArgs args)
        {
        }

        protected virtual void setPickupInfo(DeliveryOption deliveryOption)
        {
        }

        protected virtual ShippingInfo setPickupInfo(DeliveryOption deliveryOption,
                                                     DeliveryOptionType deliveryOptionType)
        {
            ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
            int deliveryOptionID = 0, shippingAddressID = 0;
            if (shippingInfo != null)
            {
                shippingAddressID = deliveryOptionType == DeliveryOptionType.Shipping
                                        ? deliveryOption.Id
                                        : shippingInfo.Address.ID;
                deliveryOptionID = deliveryOptionType == DeliveryOptionType.Shipping
                                       ? shippingInfo.Id
                                       : deliveryOption.Id;
            }
            else
            {
                shippingAddressID = deliveryOptionType == DeliveryOptionType.Shipping ? deliveryOption.Id : 0;
                deliveryOptionID = deliveryOptionType == DeliveryOptionType.Shipping ? 0 : deliveryOption.Id;
            }
            updateShippingInfo(shippingAddressID,
                               deliveryOptionID,
                               deliveryOptionType);
            return shippingInfo;
        }

        protected DeliveryOptionType getDeliveryOptionTypeFromDropdown(DropDownList deliveryType)
        {
            var selected = string.IsNullOrEmpty(deliveryType.SelectedValue) ? "Unknown" : deliveryType.SelectedValue;
            selected = selected.Equals("PickupFromCourier1") ? "PickupFromCourier" : selected;
            selected = selected.Equals("Pickup1") ? "Pickup" : selected;
            return (DeliveryOptionType) Enum.Parse(typeof (DeliveryOptionType), selected);
        }

        protected virtual void populatePickup()
        {
        }

        protected virtual void populateShipping()
        {
        }

        protected virtual void populateShipping(DropDownList dropdownNickName)
        {
            dropdownNickName.Visible = true;
            
            dropdownNickName.Items.Clear();
            var tempSortedShippingAddresses = new List<DeliveryOption>();
            //Apply Sorting rules
            if (_shippingAddresses.Exists(s => s.IsPrimary))
            {
                tempSortedShippingAddresses.Add(_shippingAddresses.Where(s => s.IsPrimary).FirstOrDefault());
            }

            tempSortedShippingAddresses.AddRange(_shippingAddresses.
                                                     Where(s => !string.IsNullOrEmpty(s.Alias) && !s.IsPrimary)
                                                                   .OrderBy(a => a.Alias));

            tempSortedShippingAddresses.AddRange(_shippingAddresses.
                                                     Where(s => string.IsNullOrEmpty(s.Alias) && !s.IsPrimary)
                                                                   .OrderBy(a => a.DisplayName));
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
            {
                tempSortedShippingAddresses = new List<DeliveryOption>();
                tempSortedShippingAddresses.AddRange(_shippingAddresses.Where(x => x.HasAddressRestriction ?? false));
            }

            dropdownNickName.DataSource = tempSortedShippingAddresses;
            dropdownNickName.DataBind();

            if (tempSortedShippingAddresses.Count == 0)
            {
                if ((ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO &&
                     !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket) ||
                    APFDueProvider.ShouldHideOrderQuickView(ShoppingCart))
                {
                    //Leave the cart alone
                }
                else
                {
                    ShoppingCart.DeliveryInfo = null;
                }
                showHideAddressLink();
            }

            if (dropdownNickName.SelectedItem != null)
            {
                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null &&
                    tempSortedShippingAddresses.Count > dropdownNickName.SelectedIndex && 
                    ShoppingCart.DeliveryInfo.Address.ID ==
                    tempSortedShippingAddresses[dropdownNickName.SelectedIndex].ID)
                {
                    ShoppingCart.DeliveryInfo.Address = tempSortedShippingAddresses[dropdownNickName.SelectedIndex];
                }
            }
        }

        protected virtual void populatePickup(DropDownList dropdownNickName, Label lableNickName)
        {
            dropdownNickName.Items.Clear();
            dropdownNickName.Visible = false;
            if (!HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
            {
                if (_pickupLocations != null)
                {
                    if (_pickupLocations.Count > 0)
                    {
                        // for the pickup locations count "1" display the lable & hide the drop down.
                        if (_pickupLocations.Count == 1)
                        {
                            lableNickName.Visible = true;
                            lableNickName.Text = _pickupLocations[0].Description;
                            setPickupInfo(_pickupLocations[0]);
                            dropdownNickName.Attributes.Add("style", "display:none");
                        }
                        else
                        {
                            lableNickName.Visible = false;
                            dropdownNickName.Attributes.Remove("style");
                        }
                        
                        var listPickup = from d in _pickupLocations
                                         select new
                                         {
                                             DisplayName = d.Description,
                                             ID = d.Id
                                         };

                        if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupLocationsOrderedList)
                        {
                            CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                            dropdownNickName.DataSource = listPickup.OrderBy(d => d.DisplayName, StringComparer.Create(culture, false));
                        }
                        else
                        {
                            dropdownNickName.DataSource = listPickup;
                        }
                        
                        dropdownNickName.DataBind();
                        dropdownNickName.Visible = true;
                    }
                }
            }
            else
            {
                if (_pickupRrefList != null && _pickupRrefList.Count > 0)
                {
                    var listPickupRref = from p in _pickupRrefList
                                        select new
                                        {
                                            DisplayName = p.PickupLocationNickname,
                                            p.ID
                                        };

                    if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupLocationsOrderedList)
                    {
                        CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                        dropdownNickName.DataSource = listPickupRref.OrderBy(p => p.DisplayName, StringComparer.Create(culture, false));
                    }
                    else
                    {
                        dropdownNickName.DataSource = listPickupRref;
                    }

                    dropdownNickName.DataBind();
                    dropdownNickName.Visible = true;
                }
            }
        }

        protected virtual void populateDropdown(DropDownList deliveryType)
        {
            if (ShoppingCart.DeliveryInfo != null)
            {
                if (null != deliveryType && deliveryType.Items.Count == 3 && deliveryType.Items[0].Value == "0")
                {
                    deliveryType.Items.RemoveAt(0);
                }

                if (deliveryType != null && deliveryType.Items.Count > 0)
                {
                    if (deliveryType.Items.Count == 1 &&
                        getDeliveryOptionTypeFromDropdown(deliveryType) != ShoppingCart.DeliveryInfo.Option)
                    {
                        // When delivery option is different to the only one allowed in drop down
                        var deliveryValue = getDeliveryOptionTypeFromDropdown(deliveryType);
                        ListItem item;
                        switch (deliveryValue)
                        {
                            case DeliveryOptionType.Shipping:
                                item = deliveryType.Items.FindByValue("Shipping");
                                if (item != null)
                                    item.Selected = true;
                                populateShipping();
                                break;
                            case DeliveryOptionType.Pickup:
                                item = deliveryType.Items.FindByValue("Pickup");
                                if (item != null)
                                    item.Selected = true;
                                populatePickup();
                                break;
                        }
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                        (HLConfigManager.Configurations.DOConfiguration.AllowShipping ||
                         ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
                    {
                        ListItem item = deliveryType.Items.FindByValue("Shipping");
                        if (item != null)
                            item.Selected = true;
                        populateShipping();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        ListItem item = deliveryType.Items.FindByValue("PickupFromCourier");
                        if (item != null)
                            item.Selected = true;
                        populatePickup();
                    }
                    else if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup ||
                             (!HLConfigManager.Configurations.DOConfiguration.AllowShipping &&
                              ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping))
                    {
                        ListItem item = deliveryType.Items.FindByValue("Pickup");
                        if (item != null)
                            item.Selected = true;
                        populatePickup();
                    }
                }
            }
            else
            {
                if (getDeliveryOptionTypeFromDropdown(deliveryType) == DeliveryOptionType.Shipping && hasShippingAddresses)
                {
                    deliveryType.Items[0].Selected = true;
                    populateShipping();
                }
                else if (getDeliveryOptionTypeFromDropdown(deliveryType) == DeliveryOptionType.Pickup)
                {
                    ListItem item = deliveryType.Items.FindByValue("Pickup");
                    if (item != null)
                        item.Selected = true;
                    populatePickup();
                }
                else
                {
                    var textSelect = (string)GetLocalResourceObject("TextSelect");
                    deliveryType.Items.Insert(0, new ListItem(textSelect, "0"));
                    if (deliveryType.SelectedItem != null) deliveryType.ClearSelection();
                    deliveryType.Items[0].Selected = true;
                }
            }
        }

        protected DeliveryOption GetDeliveryOptionIDFromShippingAddress(int id, DeliveryOptionType optionType)
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
            return null;
        }

        protected virtual DeliveryOption getSelectedAddress(int id, DeliveryOptionType optionType,
                                                            DropDownList deliveryTypeDropdown)
        {
            if (CheckNoDeliveryType(deliveryTypeDropdown) && optionType == DeliveryOptionType.Unknown)
            {
                optionType = DeliveryOptionType.Shipping;
            }

            if (optionType == DeliveryOptionType.Shipping)
            {
                if (_shippingAddresses != null)
                {
                    var addresses = _shippingAddresses.Where(s => s.Id == id);
                    if (addresses.Count() == 0)
                    {
                        addresses = _shippingAddresses.Where(s => s.IsPrimary);
                    }
                    return addresses.Count() == 0 ? null : addresses.First();
                }
                return null;
            }
            else
            {
                if (_pickupLocations != null && _pickupLocations.Count > 0)
                {
                    var varPickupLocation = _pickupLocations.Where(p => p.Id == id);
                    if (varPickupLocation.Count() > 0)
                    {
                        return varPickupLocation.First();
                    }
                }
            }
            return null;
        }

        protected virtual bool setAddressByNickName(ShippingAddress_V01 address, HtmlGenericControl pAddress,
                                                    DropDownList deliveryType)
        {
            pAddress.Visible = address != null;
            if (address != null)
            {
                DeliveryOptionType option = getDeliveryOptionTypeFromDropdown(deliveryType);
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
                if (CheckNoDeliveryType(deliveryType) && !string.IsNullOrEmpty(pAddress.InnerHtml))
                {
                    return true;
                }
            }
            else
                pAddress.InnerHtml = "";
            return false;
        }

        protected virtual void handleNicknameChanged(DropDownList deliveryType, DropDownList nickNameDropdown,
                                                     HtmlGenericControl pAddress)
        {
            var sessionInf = SessionInfo.GetSessionInfo(ShoppingCart.DistributorID, ShoppingCart.Locale);
            DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(deliveryType);
            ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
            int deliveryOptionId = 0, shippingAddressId = 0;
            if (shippingInfo != null)
            {
                if (!string.IsNullOrEmpty(nickNameDropdown.SelectedValue))
                {
                    shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : shippingInfo.Address.ID;
                    deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping
                                           ? shippingInfo.Id
                                           : int.Parse(nickNameDropdown.SelectedValue);
                    if (ShoppingCart.Locale == "es-VE" && !string.IsNullOrEmpty(nickNameDropdown.SelectedValue) && nickNameDropdown.SelectedValue != "0")
                    {
                        sessionInf.IsVenuzulaShipping = false;
                    }
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(nickNameDropdown.SelectedValue))
                {
                    shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : 0;
                    deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping
                                           ? 0
                                           : int.Parse(nickNameDropdown.SelectedValue);
                    if (ShoppingCart.Locale == "es-VE" && !string.IsNullOrEmpty(nickNameDropdown.SelectedValue) && nickNameDropdown.SelectedValue != "0")
                    {
                        sessionInf.IsVenuzulaShipping = false;
                    }
                }
            }
            updateShippingInfo(shippingAddressId,
                               deliveryOptionId,
                               deliveryOptionType);
            shippingInfo = ShoppingCart.DeliveryInfo;
            if (shippingInfo != null)
                setAddressByNickName(shippingInfo.Address == null ? null : shippingInfo.Address, pAddress,
                                     deliveryType);
            else
                setAddressByNickName(null, pAddress, deliveryType);

            List<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartRuleResult> results = HLRulesManager.Manager.ProcessCart(ShoppingCart,
                                                                                      ShoppingCartRuleReason
                                                                                          .CartWarehouseCodeChanged);
            if (results.Any(r => r.Result == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.RulesResult.Failure))
            {
                BaseWarehouseChanged(this, null);
            }
        }
        protected virtual void handleHerbalifePickupFreightCodeChanged(DropDownList deliveryType, DropDownList nickNameDropdown)
        {
            DeliveryOptionType deliveryOptionType = getDeliveryOptionTypeFromDropdown(deliveryType);
            ShippingInfo shippingInfo = ShoppingCart.DeliveryInfo;
            int deliveryOptionId = 0, shippingAddressId = 0;
            if (shippingInfo != null)
            {
                if (!string.IsNullOrEmpty(nickNameDropdown.SelectedValue))
                {
                    shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : shippingInfo.Address.ID;
                    deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping
                                           ? shippingInfo.Id
                                           : int.Parse(nickNameDropdown.SelectedValue);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(nickNameDropdown.SelectedValue))
                {
                    shippingAddressId = deliveryOptionType == DeliveryOptionType.Shipping
                                        ? int.Parse(nickNameDropdown.SelectedValue)
                                        : 0;
                    deliveryOptionId = deliveryOptionType == DeliveryOptionType.Shipping
                                           ? 0
                                           : int.Parse(nickNameDropdown.SelectedValue);
                }
            }
            if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences)
            {
                if (deliveryOptionType == DeliveryOptionType.Pickup)
                {
                    var pref = _pickupRrefList.Find(f => f.ID == deliveryOptionId);
                    if (pref != null)
                    {
                        deliveryOptionId = pref.PickupLocationID;
                    }
                }
            }
            ShoppingCart.UpdateShippingInfo(shippingAddressId, deliveryOptionId, deliveryOptionType);
           
            shippingInfo = ShoppingCart.DeliveryInfo;
        }
        protected virtual void setLinksVisiblity()
        {
        }

        protected virtual void showShiptoOrPickup(bool IsStatic)
        {
        }

        protected virtual void handleDeliveryTypeChanged(DropDownList DeliveryType, DropDownList DropdownNickName)
        {
            if (null != DropdownNickName)
            {
                DropdownNickName.Attributes.Remove("style");
            }
            ShoppingCart.DeliveryInfo = null;
            if (getDeliveryOptionTypeFromDropdown(DeliveryType) == DeliveryOptionType.Shipping)
            {
                populateShipping();
                setAddressByNickName(ShoppingCart.DeliveryInfo == null
                                         ? null
                                         : (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                                                ? ShoppingCart.DeliveryInfo.Address
                                                : null));
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
            showShiptoOrPickup(IsStatic);
        }

        protected AddressBase getAddressControl(Control parent)
        {
            return parent.Controls.Count > 1 ? parent.Controls[1] as AddressBase : null;
        }

        //protected void checkDSFraud()
        //{
        //    if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
        //    {
        //        if (ShoppingCart.DeliveryInfo != null &&
        //            null != ShoppingCart.DeliveryInfo.Address &&
        //            null != ShoppingCart.DeliveryInfo.Address.Address)
        //        {
        //            ShoppingCart.DSFraudValidationError = ShoppingCartProvider.GetDSFraudResxKey(DistributorOrderingProfileProvider.CheckForDRFraud(
        //                ShoppingCart.DistributorID, ShoppingCart.CountryCode,
        //                ShoppingCart.DeliveryInfo.Address.Address.PostalCode));

        //            ShoppingCart.PassDSFraudValidation = string.IsNullOrEmpty(ShoppingCart.DSFraudValidationError);
        //        }
        //    }
        //}

        protected string getDSMailingAddressState()
        {
            var address = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, DistributorID, CountryCode);
            if (address != null)
            {
                return address.StateProvinceTerritory;
            }
            return "SP";
        }

        protected void TrackDropdownNickName(DropDownList dropdownNickName)
        {
            if (ShoppingCart == null) return;

            var dlvrInfo = ShoppingCart.DeliveryInfo;
            if (dlvrInfo == null) return;

            string txt = null;

            if ((dropdownNickName != null) && Helper.GeneralHelper.Instance.HasData(dropdownNickName.Items))
            {
                var m = dropdownNickName.Items.FindByValue(dropdownNickName.SelectedValue);
                if (m != null) txt = m.Text;
            }

            dlvrInfo.DeliveryNickName = txt;
        }


        protected bool CanSaveCart(ref DropDownList DeliveryType, ref DropDownList DropdownNickName)
        {
            var retValue = true;

            Session.Remove("CantSaveCart");
            DeliveryOptionType deliveryType = getDeliveryOptionTypeFromDropdown(DeliveryType);
            if (deliveryType == DeliveryOptionType.Unknown)
            {
                (this.Page.Master as MasterPages.OrderingMaster).Status.AddMessage(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantVisitWithNoDeliveryInfo"));
                Session["CantSaveCart"] = true;
                retValue = false;
            }
            else if (DropdownNickName != null)
            {
                int selectedValue = 0;
                int.TryParse(DropdownNickName.SelectedValue, out selectedValue);
                if (selectedValue < 1)
                {
                    if (deliveryType == DeliveryOptionType.Pickup || deliveryType == DeliveryOptionType.PickupFromCourier)
                    {
                        (this.Page.Master as MasterPages.OrderingMaster).Status.AddMessage(PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpLocationNotSelected"));
                    }
                    else
                    {
                        (this.Page.Master as MasterPages.OrderingMaster).Status.AddMessage(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingNickNameNotPopulated"));
                    }

                    Session["CantSaveCart"] = true;
                    retValue = false;
                }
            }
            return retValue;
        }
    }
}