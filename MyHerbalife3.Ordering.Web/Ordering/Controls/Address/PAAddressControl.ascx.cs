using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class PAAddressControl : AddressControlBase
    {
        #region Properties

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return txtStreet.Text.Trim(); }
        }

        protected override string StreetAddress2
        {
            set { txtStreet2.Text = value; }
            get { return txtStreet2.Text.Trim(); }
        }

        protected override string City
        {
            set
            {
                if ( !string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if ( item != null ) item.Selected = true;
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty ?
                    string.Empty : dnlCity.SelectedItem.Text;
            }
        }

        protected override string StateProvince
        {
            set
            {
                if ( !string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlProvince.Items.FindByText(value);
                dnlProvince.SelectedIndex = -1;
                if ( item != null )
                {
                    item.Selected = true;
                    LookupCities(value);
                }
            }
            get
            {
                return dnlProvince.SelectedItem == null || dnlProvince.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlProvince.SelectedItem.Text;
            }
        }

        protected override string County
        {
            get
            {
                return dnlCounty.SelectedItem == null || dnlCounty.SelectedValue == string.Empty ?
                    string.Empty : dnlCounty.SelectedItem.Text;
            }
            set
            {
                if ( !string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlCounty.Items.FindByText(value);
                dnlCounty.SelectedIndex = -1;
                if ( item != null )
                    item.Selected = true;
            }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        protected override RequireFieldDef[] RequiredFields
        {
            set { }
            get
            {
                Func<string, string> getErrMsg = (itemName) =>
                    PlatformResources.GetGlobalResourceString("ErrorMessage", itemName);

                return new[] {
                    new RequireFieldDef {
                        Field = _shippingAddr.Recipient,
                        ErrorMsg = getErrMsg("NoCareOfName") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.Line1,
                        ErrorMsg = getErrMsg("NoStreet1") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.Line2,
                        ErrorMsg = getErrMsg("NoStreet1") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.StateProvinceTerritory,
                        ErrorMsg = getErrMsg("NoState") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.City,
                        ErrorMsg = getErrMsg("NoCity") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.CountyDistrict,
                        ErrorMsg = getErrMsg("NoCounty") },
                    new RequireFieldDef {
                        Field = _shippingAddr.Phone,
                        ErrorMsg = getErrMsg("NoPhone") }
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
            if ( _shippingAddr != null )
            {
                dnlProvince.Items.Clear();
                dnlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;

                if ( LookupProvinces() )
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                if ( LookupCities(StateProvince) )
                    City = _shippingAddr.Address.City;

                if ( LookupCountySuburb(StateProvince, City) )
                    County = _shippingAddr.Address.CountyDistrict;

                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                LookupProvinces();
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            Func<string, string> getErrMsg = (itemName) =>
                    PlatformResources.GetGlobalResourceString("ErrorMessage", itemName);

            if ( !string.IsNullOrEmpty(PhoneNumber) &&
                 !Regex.IsMatch(_shippingAddr.Phone, @"^(\d{7,8})$") )
                _errors.Add(getErrMsg("InvalidPhone"));
        }

        private bool LookupProvinces()
        {
            bool lookedUp = false;
            dnlProvince.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if ( provider != null )
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if ( lookupResults != null && lookupResults.Count > 0 )
                {
                    foreach ( var province in lookupResults )
                    {
                        dnlProvince.Items.Add(new ListItem(province));
                    }
                    lookedUp = true;
                }
            }

            dnlProvince.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
            dnlProvince.SelectedIndex = 0;

            return lookedUp;
        }

        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if ( provider != null )
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if ( lookupResults != null && lookupResults.Count > 0 )
                {
                    foreach ( var city in lookupResults )
                    {
                        dnlCity.Items.Add(new ListItem(city));
                    }
                    lookedUp = true;
                }
                else
                {
                    dnlProvince.Focus();
                }
            }

            dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
            dnlCity.SelectedIndex = 0;

            return lookedUp;
        }

        private bool LookupCountySuburb(string state, string city)
        {
            bool lookedUp = false;
            dnlCounty.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if ( provider != null )
            {
                var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, state, city);
                if ( lookupResults != null && lookupResults.Count > 0 )
                {
                    lookupResults.ForEach(x => dnlCounty.Items.Add(new ListItem(x)));
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }

            dnlCounty.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
            dnlCounty.SelectedIndex = 0;

            return lookedUp;
        }

        #endregion

        #region Event handlers

        protected void dnlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( !string.IsNullOrEmpty(dnlProvince.SelectedItem.Value) )
            {
                LookupCities(dnlProvince.SelectedItem.Value);
                dnlCounty.Items.Clear();
            }
            else
            {
                dnlCity.Items.Clear();
                dnlCounty.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            var provinceItem = dnlProvince.SelectedItem;
            var cityItem = dnlCity.SelectedItem;

            if ( provinceItem != null && cityItem != null && !string.IsNullOrEmpty(provinceItem.Value) && !string.IsNullOrEmpty(cityItem.Value) )
            {
                LookupCountySuburb(provinceItem.Value, cityItem.Value);
            }
            else
            {
                dnlCounty.Items.Clear();
            }
        }

        #endregion
    }
}