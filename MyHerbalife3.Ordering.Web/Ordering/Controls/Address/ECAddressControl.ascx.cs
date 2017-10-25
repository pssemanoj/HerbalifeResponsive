using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class ECAddressControl : AddressControlBase
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
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCounties(StateProvince, value);
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
            }
        }

        protected override string County
        {
            get
            {
                return dnlCounty.SelectedItem == null || dnlCounty.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCounty.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlCounty.Items.FindByText(value);
                dnlCounty.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                    LookupPostal(dnlState.SelectedItem.Text, City, County);
                }
            }
        }

        protected override string ZipCode
        {
            get
            {
                return dnlPostalCode.SelectedItem == null || dnlPostalCode.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlPostalCode.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlPostalCode.Items.FindByText(value);
                dnlPostalCode.SelectedIndex = -1;
                if (null != item) item.Selected = true;
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
                                Field = _shippingAddr.Address.CountyDistrict,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCounty")
                            },
                       new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.PostalCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPostalCode")
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
                dnlPostalCode.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupStates())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                County = _shippingAddr.Address.CountyDistrict;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
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
            dnlCounty.Items.Clear();
            dnlPostalCode.Items.Clear();
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

        private bool LookupCounties(string state, string city)
        {
            bool lookedUp = false;
            dnlCounty.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var county in lookupResults)
                    {
                        dnlCounty.Items.Add(new ListItem(county));
                    }
                    dnlCounty.Items.Insert(0,
                                           new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
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

        private bool LookupPostal(string state, string city, string county)
        {
            bool lookedUp = false;
            dnlPostalCode.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetZipsForCounty(ProductsBase.CountryCode, state, city, county);
              
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var zip in lookupResults)
                    {
                        dnlPostalCode.Items.Add(new ListItem(zip));
                    }
                    if (dnlPostalCode.Items.Count!=1)
                         { 
                        dnlPostalCode.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlPostalCode.SelectedIndex = 0;
                    }
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
                dnlCounty.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value) && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupCounties(dnlState.SelectedItem.Value, dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlCounty.Items.Clear();
            }
        }


        protected void dnlCounty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCounty.SelectedItem.Value) && !string.IsNullOrEmpty(dnlCounty.SelectedItem.Value))
            {
                LookupPostal(dnlState.SelectedItem.Value, dnlCity.SelectedItem.Value, dnlCounty.SelectedItem.Value);
            }
            else
            {
                dnlPostalCode.Items.Clear();
            }
        }
        #endregion
    }
}