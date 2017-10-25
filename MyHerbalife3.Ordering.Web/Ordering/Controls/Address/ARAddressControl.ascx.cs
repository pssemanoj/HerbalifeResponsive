using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class ARAddressControl : AddressControlBase
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

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCounties(dnlState.SelectedItem.Text ,value);
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
                ListItem item = dnlState.Items.FindByValue(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCities(item.Text);
                }
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue.Equals("0")
                           ? string.Empty
                           : dnlState.SelectedItem.Value;
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
                if (null != item) { 
                    item.Selected = true;
                    LookupPostal(dnlState.SelectedItem.Text, City, County);
                }
            }
        }
        protected override string ZipCode
        {
            get
            {
                return dnlPostCode.SelectedItem == null || dnlPostCode.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlPostCode.SelectedItem.Text;
        }
            set
            {
                ListItem item = dnlPostCode.Items.FindByText(value);
                dnlPostCode.SelectedIndex = -1;
                if (null != item) item.Selected = true;
            }
        }

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
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
            txtAreaCode.Attributes.Add("onkeypress",
                                       string.Format("CheckAreaPlusPhone(event,'{0}','{1}',10);", txtAreaCode.ClientID,
                                                     txtNumber.ClientID));
            txtNumber.Attributes.Add("onkeypress",
                                     string.Format("CheckAreaPlusPhone(event,'{0}','{1}',10);", txtAreaCode.ClientID,
                                                   txtNumber.ClientID));
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlState.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                if (LookupStates())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                County = _shippingAddr.Address.CountyDistrict;
                ZipCode = _shippingAddr.Address.PostalCode;
                AreaCode = _shippingAddr.AreaCode;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{4})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                string[] numbers = _shippingAddr.Phone.Split('-');

                if (numbers[0].Length == 0)
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode"));
                else if (!Regex.IsMatch(numbers[0], @"^(\d{2,4})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));

                if (!Regex.IsMatch(numbers[1], @"^(\d{6,8})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));

                if (numbers[0].Length + numbers[1].Length > 10)
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }
/*
        private bool LookupProvince(string zipCode)
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            if (!string.IsNullOrEmpty(zipCode))
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider(
                        ProductsBase.CountryCode);
                if (provider != null)
                {
                    var lookupResults = provider.LookupCitiesByZip(ProductsBase.CountryCode, zipCode);
                    if (lookupResults != null && lookupResults.Count > 0)
                    {
                        foreach (var province in lookupResults)
                        {
                            string[] state = province.State.Split('-');
                            dnlState.Items.Add(new ListItem(state[0], state[1]));
                        }

                        if (lookupResults.Count > 1)
                        {
                            dnlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, "0"));
                        }
                        dnlState.SelectedIndex = 0;
                        lookedUp = true;
                    }
                }
            }
            return lookedUp;
        }
        */
        /*     protected void txtPostCode_TextChanged(object sender, EventArgs e)
             {
                 dnlState.Items.Clear();
                 if (!string.IsNullOrEmpty(dnlPostCode.SelectedItem.Text))
                 {
                     LookupProvince(txtPostCode.Text);
                 }
             }*/

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
                        string[] state = province.Split('-');
                        dnlState.Items.Add(new ListItem(state[0], state[1]));

                        //dnlState.Items.Add(new ListItem(province));
                    }
                    dnlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
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
                if (lookupResults != null && lookupResults.Count > 0 && lookupResults[0]!=null )
                {
                    foreach (var county in lookupResults)
                    {
                        dnlCounty.Items.Add(new ListItem(county));
                    }
                    
                        dnlCounty.Items.Insert(0,  new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlCounty.SelectedIndex = 0;
                    
                    
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                    LookupPostal(state, City, null);
                }
            }
            return lookedUp;
        }
        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlCounty.Items.Clear();
            dnlPostCode.Items.Clear();
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
        private bool LookupPostal(string state, string city, string county)
        {
            bool lookedUp = false;
            dnlPostCode.Items.Clear();
            List<string> lookupResults = new List<string>();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                if (dnlCounty.Items.Count==0)
                {
                   lookupResults= provider.GetAddressField(new AddressFieldForCountryRequest_V01()
                                {
                                   AddressField = AddressPart.ZIPCODE,
                                   Country = ProductsBase.CountryCode,
                                   State = state,
                                   City = city
                                });

                }
                else
                {
                 lookupResults = provider.GetZipsForCounty(ProductsBase.CountryCode, state, city, county);
                }

                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var zip in lookupResults)
                    {
                        dnlPostCode.Items.Add(new ListItem(zip));
                    }
                    if (dnlPostCode.Items.Count != 1)
                    {
                        dnlPostCode.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlPostCode.SelectedIndex = 0;
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
        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Text))
            {
                LookupCities(dnlState.SelectedItem.Text);
            }
            else
            {
                dnlCity.Items.Clear();
                dnlCounty.Items.Clear();
                dnlPostCode.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Text) && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupCounties(dnlState.SelectedItem.Text, dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlCounty.Items.Clear();
            }
        }

        protected void dnlCounty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCounty.SelectedItem.Text) && !string.IsNullOrEmpty(dnlCounty.SelectedItem.Value))
            {
                LookupPostal(dnlState.SelectedItem.Text, dnlCity.SelectedItem.Value, dnlCounty.SelectedItem.Value);
            }
            else
            {
                dnlPostCode.Items.Clear();
            }
        }

        #endregion
    }
}