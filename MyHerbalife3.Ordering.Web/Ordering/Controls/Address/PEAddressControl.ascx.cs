using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class PEAddressControl : AddressControlBase
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
            get { return txtStreet2.Text.Trim(); }
            set { txtStreet2.Text = value; }
        }

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null) item.Selected = true;
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
                ListItem item = dnlProvince.Items.FindByText(value);
                dnlProvince.SelectedIndex = -1;
                if (item != null)
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
                return dnlCounty.SelectedItem == null || dnlCounty.SelectedValue == string.Empty ? string.Empty : dnlCounty.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlCounty.Items.FindByText(value);
                dnlCounty.SelectedIndex = -1;
                if (item != null)
                    item.Selected = true;
            }
        }

        protected override string ZipCode
        {
            get 
            {
                return txtPostCode.Text;
            }
            set
            {
                txtPostCode.Text = value;
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
                            Field = _shippingAddr.Address.CountyDistrict,
                            ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCounty")
                        },
                        new RequireFieldDef
                        {
                            Field = _shippingAddr.Address.PostalCode,
                            ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
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
                dnlProvince.Items.Clear();
                dnlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                this.StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupProvinces())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                if (LookupCountySuburb(this.StateProvince, this.City))
                    County = _shippingAddr.Address.CountyDistrict;
                if (LookupPostalCode(this.StateProvince, this.City, this.County))
                    ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                LookupProvinces();
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{6,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        private bool LookupProvinces()
        {
            bool lookedUp = false;
            dnlProvince.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        dnlProvince.Items.Add(new ListItem(province));
                    }
                    dnlProvince.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlProvince.SelectedIndex = 0;
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
                    dnlProvince.Focus();
                }
            }
            return lookedUp;
        }

        private bool LookupCountySuburb(string state, string city)
        {
            bool lookedUp = false;
            dnlCounty.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    lookupResults.ForEach(x => dnlCounty.Items.Add(new ListItem(x)));
                    dnlCounty.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCounty.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }

            return lookedUp;
        }

        private bool LookupPostalCode(string state, string city, string county)
        {
            bool lookedUp = false;
            txtPostCode.Text = string.Empty;
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetZipsForCounty(ProductsBase.CountryCode, state, city, county);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    txtPostCode.Text = lookupResults.FirstOrDefault();
                    lookedUp = true;
                }
                else
                {
                    dnlCounty.Focus();
                }
            }

            return lookedUp;
        }
        #endregion

        #region Event handlers

        protected void dnlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlProvince.SelectedItem.Value))
            {
                LookupCities(dnlProvince.SelectedItem.Value);
                dnlCounty.Items.Clear();
                txtPostCode.Text = string.Empty;
            }
            else
            {
                dnlCity.Items.Clear();
                dnlCounty.Items.Clear();
                txtPostCode.Text = string.Empty;
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            var provinceItem = dnlProvince.SelectedItem;
            var cityItem = dnlCity.SelectedItem;

            if (provinceItem != null && cityItem != null && !string.IsNullOrEmpty(provinceItem.Value) && !string.IsNullOrEmpty(cityItem.Value))
            {
                LookupCountySuburb(provinceItem.Value, cityItem.Value);
                txtPostCode.Text = string.Empty;
            }
            else
            {
                dnlCounty.Items.Clear();
                txtPostCode.Text = string.Empty;
            }
        }

        protected void dnlCounty_SelectedIndexChanged(object sender, EventArgs e)
        {
            var provinceItem = dnlProvince.SelectedItem;
            var cityItem = dnlCity.SelectedItem;
            var countyItem = dnlCounty.SelectedItem;

            if (provinceItem != null && cityItem != null && countyItem != null
                && !string.IsNullOrEmpty(provinceItem.Value) && !string.IsNullOrEmpty(cityItem.Value) && !string.IsNullOrEmpty(countyItem.Value))
            {
                LookupPostalCode(provinceItem.Value, cityItem.Value, countyItem.Value);
            }
            else
            {
                txtPostCode.Text = string.Empty;
            }
        }

        #endregion
    }
}