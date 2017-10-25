using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class TWAddressControl : AddressControlBase
    {
        protected override string Recipient
        {
            set { txtRecipient.Text = value; }
            get { return (txtRecipient.Text.Trim()); }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return (txtStreet.Text.Trim()); }
        }

        protected override string City
        {
            set
            {
                ListItem item = ddlCity.Items.FindByText(value);
                ddlCity.SelectedIndex = -1;
                if (null != item) item.Selected = true;
            }
            get { return ddlCity.SelectedItem == null ? string.Empty : ddlCity.SelectedItem.Text; }
        }

        protected override string StateProvince
        {
            set
            {
                ListItem itemState = ddlState.Items.FindByText(value);
                ddlState.SelectedIndex = -1;
                if (null != itemState) itemState.Selected = true;
            }
            get { return ddlState.SelectedItem == null ? string.Empty : ddlState.SelectedItem.Text; }
        }

        protected override string ZipCode
        {
            set { txtZipcode.Text = value; }
            get { return txtZipcode.Text; }
        }

        protected override string PhoneNumber
        {
            set { txtPhoneNumber.Text = value; }
            get { return txtPhoneNumber.Text; }
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
                                Field = _shippingAddr.FirstName,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoFirstName")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.LastName,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoLastName")
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
                            }
                    };
            }
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                ddlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                ZipCode = _shippingAddr.Address.PostalCode;

                StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                if (!string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory))
                    LookupCities(_shippingAddr.Address.StateProvinceTerritory);

                City = _shippingAddr.Address.City;

                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                LookupStates();
            }
        }

        private string _countryCode;

        protected void Page_Load(object sender, EventArgs e)
        {
            _countryCode = (Page as ProductsBase).CountryCode;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            //if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Phone, @"^(\d{12})|(\d{2}[ |-]\d{9})|(\d{3}[ |-]\d{8})|(\d{4}[ |-]\d{7})|(\d{5}[ |-]\d{6})$"))
            if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{9,12})$"))
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            LookupCities(ddlState.SelectedItem.Value);
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            LookUpZipcodes(ddlState.SelectedItem.Value, ddlCity.SelectedItem.Value);
        }

        private void LookUpZipcodes(string state, string city)
        {
            IShippingProvider provider = ShippingProvider.
                                            GetShippingProvider(_countryCode);
            if (provider != null)
            {
                if (!city.Equals(string.Empty))
                {
                    List<string> lookupResults = provider.GetZipsForCity(_countryCode, state, city);
                    if (null != lookupResults && lookupResults.Count > 0)
                    {
                        txtZipcode.Text = lookupResults[0];
                    }
                }
            }
        }


        private bool LookupCities(string state)
        {
            bool lookedUp = false;

            ddlCity.Items.Clear();
            IShippingProvider provider = ShippingProvider.
                                            GetShippingProvider(_countryCode);
            if (provider != null)
            {
                if (!state.Equals(string.Empty))
                {
                    List<string> lookupResults = provider.GetCitiesForState(_countryCode, state);
                    if (null != lookupResults && lookupResults.Count > 0)
                    {
                        for (int i = 0; i < lookupResults.Count; i++)
                        {
                            ddlCity.Items.Insert(i, new ListItem(lookupResults[i]));
                        }
                        ddlCity.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        ddlCity.SelectedIndex = 0;
                        lookedUp = true;
                    }
                }
            }

            return lookedUp;
        }

        private bool LookupStates()
        {
            bool lookedUp = false;

            ddlState.Items.Clear();
            IShippingProvider provider = ShippingProvider.
                                            GetShippingProvider(_countryCode);
            if (provider != null)
            {
                List<string> lookupResults = provider.GetStatesForCountry(_countryCode);
                if (null != lookupResults && lookupResults.Count > 0)
                {
                    for (int i = 0; i < lookupResults.Count; i++)
                    {
                        ddlState.Items.Insert(i, new ListItem(lookupResults[i]));
                    }
                    ddlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    ddlState.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }
    }
}