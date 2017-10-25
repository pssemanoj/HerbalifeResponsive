using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class IDAddressControl : AddressControlBase
    {
        #region Properties

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string StreetAddress
        {
            set { txtStreet1.Text = value; }
            get { return txtStreet1.Text.Trim(); }
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
                ListItem item = dnlCity.Items.FindByValue(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue.Equals("0")
                           ? string.Empty
                           : dnlCity.SelectedItem.Value;
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
                    LookupCities(value);
                }
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue.Equals("0")
                           ? string.Empty
                           : dnlState.SelectedItem.Value;
            }
        }

        protected override string ZipCode
        {
            set { txtPostalCode.Text = value; }
            get { return txtPostalCode.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text.Trim(); }
        }

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
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
                                Field = AreaCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode")
                            },
                        new RequireFieldDef
                            {
                                Field = PhoneNumber,
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
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupStates())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                AreaCode = _shippingAddr.AreaCode;
            }
        }

        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
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

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{5})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }

            if (!string.IsNullOrEmpty(AreaCode))
            {
                if (!Regex.IsMatch(AreaCode, @"^(\d{2,4})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));
            }
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                if (!Regex.IsMatch(PhoneNumber, @"^(\d{5,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }

            //if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            //{
            //    string[] numbers = _shippingAddr.Phone.Split('-');

            //    if (numbers[0].Length != 0 && !System.Text.RegularExpressions.Regex.IsMatch(numbers[0], @"^(\d{2,4})$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));

            //    if (numbers[1].Length != 0 && !System.Text.RegularExpressions.Regex.IsMatch(numbers[1], @"^(\d{5,10})$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            //}
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

        #endregion
    }
}