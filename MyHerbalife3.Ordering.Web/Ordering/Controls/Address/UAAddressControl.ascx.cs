using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class UAAddressControl : AddressControlBase
    {
        #region Properties

        /// <summary>
        ///     To store the tab index
        /// </summary>
        private short _tabIndex;

        /// <summary>
        ///     Gets or sets the tab index of the control
        /// </summary>
        protected override short TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                txtCareOfName.TabIndex = (value++);
                dnlState.TabIndex = (value++);
                dnlRegion.TabIndex = (value++);
                dnlCity.TabIndex = (value++);
                dnlPostalCode.TabIndex = (value++);
                txtStreet.TabIndex = (value++);
                txtBuilding1.TabIndex = (value++);
                txtBuilding2.TabIndex = (value++);
                txtFlat.TabIndex = (value++);
                txtNumber.TabIndex = (value++);
            }
        }

        /// <summary>
        ///     Gets or sets the recipient
        /// </summary>
        protected override string Recipient
        {
            get { return txtCareOfName.Text.Trim(); }
            set { txtCareOfName.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the State
        /// </summary>
        protected override string StateProvince
        {
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Value;
            }
            set
            {
                var item = dnlState.Items.FindByValue(value);
                dnlState.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the County
        /// </summary>
        protected override string County
        {
            get
            {
                return dnlRegion.SelectedItem == null || dnlRegion.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlRegion.SelectedItem.Value;
            }
            set
            {
                var item = dnlRegion.Items.FindByValue(value);
                dnlRegion.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the city
        /// </summary>
        protected override string City
        {
             get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Value;
            }
            set
            {
                var item = dnlCity.Items.FindByValue(value);
                dnlCity.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the street line 1
        /// </summary>
        protected override string StreetAddress
        {
            get { return txtStreet.Text.Trim(); }
            set { txtStreet.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the street line 2 (Building 1)
        /// </summary>
        protected override string StreetAddress2
        {
            get { return txtBuilding1.Text.Trim(); }
            set { txtBuilding1.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the street line 3 (Building 2)
        /// </summary>
        protected override string StreetAddress3
        {
            get { return txtBuilding2.Text.Trim(); }
            set { txtBuilding2.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the street line 4 (Flat)
        /// </summary>
        protected override string StreetAddress4
        {
            get { return txtFlat.Text.Trim(); }
            set { txtFlat.Text = value; }
        }

        /// <summary>
        ///     Gets or sets zipCode
        /// </summary>
        protected override string ZipCode
        {
            get
            {
                return dnlPostalCode.SelectedItem == null || dnlPostalCode.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlPostalCode.SelectedItem.Value;
            }
            set
            {
                var item = dnlPostalCode.Items.FindByValue(value);
                dnlPostalCode.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        /// <summary>
        ///     Gets or sets the required field array to validate
        /// </summary>
        protected override RequireFieldDef[] RequiredFields
        {
            get
            {
                return new[]
                    {
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Recipient,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCareOfName")
                            },
                        new RequireFieldDef
                            {
                                Field = dnlState.SelectedItem != null ? dnlState.SelectedItem.Value : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoState")
                            },
                        new RequireFieldDef
                            {
                                Field = dnlCity.SelectedItem != null ? dnlCity.SelectedItem.Value : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
                            },
                       new RequireFieldDef
                            {
                                Field =
                                    dnlPostalCode.SelectedItem != null ? dnlPostalCode.SelectedItem.Value : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
                            },
                        new RequireFieldDef
                            {
                                Field = StreetAddress,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = StreetAddress2,
                                ErrorMsg = GetLocalResourceObject("NoBuilding").ToString()
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            }
                    };
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///     Loads the page
        /// </summary>
        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                if (LookupStateProvinces())
                {
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory; // this will set selected
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory; // this will set selected
                    if (!string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory) &&
                        LookupCities(_shippingAddr.Address.StateProvinceTerritory))
                    {
                        City = _shippingAddr.Address.City;
                        if (!string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory) &&
                            LookupDistricts(_shippingAddr.Address.StateProvinceTerritory,
                                            _shippingAddr.Address.City))
                        {
                            County = _shippingAddr.Address.CountyDistrict;
                        }
                        else
                        {
                            dnlRegion.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                            dnlRegion.SelectedIndex = 0;
                            LookupDistricts(StateProvince, City);
                        }
                        if (!string.IsNullOrEmpty(_shippingAddr.Address.City) &&
                                LookupPostalCodes(_shippingAddr.Address.StateProvinceTerritory,
                                                  _shippingAddr.Address.CountyDistrict, _shippingAddr.Address.City))
                        {
                            ZipCode = _shippingAddr.Address.PostalCode;
                        }
                        else
                        {
                            dnlPostalCode.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                            dnlPostalCode.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        LookupCities(StateProvince);
                        dnlCity.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                        dnlCity.SelectedIndex = 0;
                    }
                    
                }
                txtStreet.Text = _shippingAddr.Address.Line1;
                txtBuilding1.Text = _shippingAddr.Address.Line2;
                txtBuilding2.Text = _shippingAddr.Address.Line3;
                txtFlat.Text = _shippingAddr.Address.Line4;
                txtNumber.Text = _shippingAddr.Phone;
            }
            TabIndex = 1;
        }

        /// <summary>
        ///     Validates the control.
        /// </summary>
        /// <param name="_errors"></param>
        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone)
                && !Regex.IsMatch(_shippingAddr.Phone, @"^(\d{10})$"))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlState.SelectedItem != null && !string.IsNullOrEmpty(dnlState.SelectedItem.Value))
            {
                LookupCities(StateProvince);
                //LookupDistricts(StateProvince, City);
                dnlCity.Focus();
            }
            else
            {
                dnlCity.Items.Clear();
                dnlRegion.Items.Clear();
                dnlPostalCode.Items.Clear();
            }
        }

        

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlCity.SelectedItem != null && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupDistricts(StateProvince, City);
                LookupPostalCodes(StateProvince, County, City);
                dnlPostalCode.Focus();
            }
            else
            {
                dnlPostalCode.Items.Clear();
                dnlRegion.Items.Clear();
                LookupDistricts(StateProvince, City);
            }
        }


        protected void dnlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlRegion.SelectedItem != null && !string.IsNullOrEmpty(dnlRegion.SelectedItem.Value))
            {
                LookupPostalCodes(StateProvince, County, City);
                dnlPostalCode.Focus();
            }
            else
            {
                dnlPostalCode.Items.Clear();
                LookupPostalCodes(StateProvince, County, City);
            }
        }

        /// <summary>
        ///     Gets the stateProvinces from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupStateProvinces()
        {
            dnlState.Items.Clear();
            dnlRegion.Items.Clear();
            dnlCity.Items.Clear();
            dnlPostalCode.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                    dnlState.Items.AddRange(items);
                    dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Gets the districts from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupDistricts(string state, string district)
        {
            dnlRegion.Items.Clear();
            dnlPostalCode.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                // city is district(region) for Ukraine
                
                var lookupResults = provider.GetStreets(ProductsBase.CountryCode, state, district);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                    dnlRegion.Items.AddRange(items);
                    dnlRegion.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlRegion.SelectedIndex = 0;
                    
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Gets the cities from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupCities(string state)
        {
            dnlCity.Items.Clear();
            dnlRegion.Items.Clear();
            dnlPostalCode.Items.Clear();
            
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                //var lookupResults = provider.GetStreets(ProductsBase.CountryCode, state, district);
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                    dnlCity.Items.AddRange(items);
                    dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Gets postal codes
        /// </summary>
        /// <param name="state"></param>
        /// <param name="district"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        private bool LookupPostalCodes(string state, string district, string city)
        {
            dnlPostalCode.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetZipCodes(ProductsBase.CountryCode, state, district, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                    dnlPostalCode.Items.AddRange(items);
                    dnlPostalCode.SelectedIndex = 0;
                    if (dnlPostalCode.Items.Count == 1)
                    {
                        dnlPostalCode.SelectedIndex = 0;
                    }
                    else
                    {
                        dnlPostalCode.Items.Insert(0,
                                                   new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                        dnlPostalCode.SelectedIndex = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}