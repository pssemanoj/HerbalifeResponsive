using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web.UI.WebControls;
using System.Linq;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class ZAAddressControl : AddressControlBase
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
                var item = dnlSuburb.Items.FindItemByValue(value);
                dnlSuburb.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
            get
            {
                if (dnlSuburb.SelectedItem == null || dnlSuburb.SelectedIndex == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return dnlSuburb.SelectedItem.Text;
                }
            }
        }

        protected override string StateProvince
        {
            set
            {
                var item = dnlProvince.Items.FindByValue(value);
                dnlProvince.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
            get
            {
                if (dnlProvince.SelectedItem == null || string.IsNullOrEmpty(dnlProvince.SelectedValue))
                {
                    return string.Empty;
                }
                else
                {
                    return dnlProvince.SelectedItem.Text;
                }
            }
        }

        protected override string ZipCode
        {
            set
            {
                var item = dnlPostalCode.Items.FindByValue(value);
                dnlPostalCode.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
            get
            {
                if (dnlPostalCode.SelectedItem == null || string.IsNullOrEmpty(dnlPostalCode.SelectedValue))
                {
                    return string.Empty;
                }
                else
                {
                    return dnlPostalCode.SelectedItem.Text;
                }
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
                                Field = dnlProvince.SelectedItem != null 
                                    ? (dnlProvince.SelectedItem.Value.Equals(base.GetLocalResourceObject("Select") as string) ? string.Empty : dnlProvince.SelectedItem.Value)
                                    : dnlProvince.Text.Trim(),
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoProvince")
                            },
                        new RequireFieldDef
                            {
                                Field = dnlPostalCode.SelectedItem != null
                                    ? (dnlPostalCode.SelectedItem.Value.Equals(base.GetLocalResourceObject("Select") as string) ? string.Empty : dnlPostalCode.SelectedItem.Value)
                                    : dnlPostalCode.Text.Trim(),
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            },
                        new RequireFieldDef
                            {
                                Field = dnlSuburb.SelectedItem != null
                                    ? (dnlSuburb.SelectedItem.Value.Equals(base.GetLocalResourceObject("Select") as string) ? string.Empty : dnlSuburb.SelectedItem.Value)
                                    : dnlSuburb.Text.Trim(),
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoSuburb")
                            },
                    };
            }
        }
        #endregion

        #region Methods
        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupProvince())
                {
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                }
                if (LookupSuburb(StateProvince))
                {
                    City = _shippingAddr.Address.City;
                }
                if (LookupPostalCode(StateProvince, City))
                {
                    ZipCode = _shippingAddr.Address.PostalCode;
                }
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            dnlSuburb.SelectedIndexChanged += dnlSuburb_SelectedIndexChanged;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{9,11})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        private bool LookupProvince()
        {
            bool lookedUp = false;
            dnlProvince.Items.Clear();
            dnlSuburb.Items.Clear();
            dnlSuburb.Text = string.Empty;
            dnlPostalCode.Items.Clear();

            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Any())
                {
                    foreach (var province in lookupResults)
                    {
                        dnlProvince.Items.Add(new ListItem(province));
                    }
                    dnlProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlProvince.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupSuburb(string province)
        {
            bool lookedUp = false;
            dnlSuburb.Items.Clear();
            dnlSuburb.Text = string.Empty;
            dnlPostalCode.Items.Clear();

            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, province, null);
                if (lookupResults != null && lookupResults.Any())
                {
                    lookupResults.Insert(0, GetLocalResourceObject("Select") as string);
                    dnlSuburb.DataSource = lookupResults;
                    dnlSuburb.DataBind();
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupPostalCode(string province, string county)
        {
            bool lookedUp = false;
            dnlPostalCode.Items.Clear();

            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetZipsForCounty(ProductsBase.CountryCode, province, null, county);
                if (lookupResults != null && lookupResults.Any())
                {
                    foreach (var zipcode in lookupResults)
                    {
                        dnlPostalCode.Items.Add(new ListItem(zipcode));
                    }
                    if (lookupResults.Count > 1)
                    {
                        dnlPostalCode.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                        dnlPostalCode.SelectedIndex = 0;
                    }
                    lookedUp = true;
                }
            }
            return lookedUp;
        }
        #endregion

        #region Eventhandler
        protected void dnlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlProvince.SelectedItem.Value))
            {
                LookupSuburb(dnlProvince.SelectedItem.Text);
                dnlSuburb.Focus();
            }
            else
            {
                dnlSuburb.Items.Clear();
                dnlSuburb.Text = string.Empty;
                dnlPostalCode.Items.Clear();
            }
        }

        private void dnlSuburb_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            if (dnlSuburb.SelectedItem != null && dnlSuburb.SelectedIndex != 0)
            {
                LookupPostalCode(dnlProvince.SelectedItem.Value, dnlSuburb.SelectedItem.Value);
                if (dnlPostalCode.Items.Count > 1)
                { 
                    dnlPostalCode.Focus();
                }
            }
            else
            {
                dnlPostalCode.Items.Clear();
            }
        }

        #endregion
    }
}