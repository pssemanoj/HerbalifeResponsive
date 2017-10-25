using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class COAddressControl : AddressControlBase
    {
        #region Properties

        protected override string Recipient
        {
            set { txtCareOfName.Text = value;
            
            }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return txtStreet.Text.Trim(); }
        }

        protected override string StreetAddress2
        {
            get { return txtStreet2.Text.Trim(); }
            set { txtStreet2.Text = value; }
        }

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                    LookupCounty(dnlState.SelectedItem.Text,value);
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
            }
        }

        protected override string StateProvince
        {
            set
            {
                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCities(value);
                }
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
        }

        protected override string County
        {
            set
            {
                ListItem item = dnlCounty.Items.FindByText(value);
                dnlCounty.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                   // LookupCounty(dnlState.SelectedItem.Text, dnlCity.SelectedItem.Text);
                }
            }
            get
            {
                return dnlCounty.SelectedItem == null || dnlCounty.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCounty.SelectedItem.Text;
            }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        protected override RequireFieldDef[] RequiredFields
        {
            set { ; }
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
                                Field = _shippingAddr.Address.Line1,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.StateProvinceTerritory,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoState")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.City,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
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

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlState.Items.Clear();
                dnlCity.Items.Clear();
                dnlCounty.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupStates())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                County = _shippingAddr.Address.CountyDistrict;
                //ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                LookupStates();
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{6,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
            //Adding validation for special characters not allow in address1 and address2
            if (!string.IsNullOrEmpty(_shippingAddr.Address.Line1))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.Line1, @"^([a-zA-Z0-9\s]*)$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            }
            
            if (!string.IsNullOrEmpty(_shippingAddr.Address.Line2))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.Line2, @"^([a-zA-Z0-9\s]*)$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress2"));
            }
        }

        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        dnlState.Items.Add(new ListItem(province));
                    }
                    dnlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        dnlCity.Items.Add(new ListItem(city));
                    }
                    dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlState.Focus();
                }
            }
            return lookedUp;
        }
        private bool LookupCounty(string state, string city)
        {
            bool lookedUp = false;
            dnlCounty.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var county in lookupResults)
                    {
                        dnlCounty.Items.Add(new ListItem(county));
                    }
                    if (dnlCounty.Items.Count != 1) { 
                        dnlCounty.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlCounty.SelectedIndex = 0;
                    }
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }
            return lookedUp;
        }

        #endregion

        #region Eventhandler

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value))
            {
                LookupCities(dnlState.SelectedItem.Value);
            }
            else
            {
                dnlCity.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get county autopopulated
            if (!string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupCounty(dnlState.SelectedItem.Value,dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlCounty.Items.Clear();
            }
        }
        #endregion
    }
}