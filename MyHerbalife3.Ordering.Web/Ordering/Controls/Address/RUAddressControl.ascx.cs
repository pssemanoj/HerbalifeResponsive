using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    using System.Web.UI.WebControls;

    using MyHerbalife3.Ordering.Providers.Interfaces;

    public partial class RUAddressControl : AddressControlBase
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
            set { txtCity.Text = value; }
            get { return txtCity.Text; }
        }

        protected override string County
        {
            get { return txtArea.Text; }
            set { txtArea.Text = value; }
        }

        protected override string StateProvince
        {
            set
            {
                ListItem item = this.dnlState.Items.FindByValue(value);
                this.dnlState.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
            get
            {
                return this.dnlState.SelectedItem == null || this.dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : this.dnlState.SelectedItem.Value;
            }
        }

        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text; }
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
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                this.Recipient = _shippingAddr.Recipient;
                this.StreetAddress = _shippingAddr.Address.Line1;
                this.StreetAddress2 = _shippingAddr.Address.Line2;
                this.City = _shippingAddr.Address.City;
                this.County = _shippingAddr.Address.CountyDistrict;
                this.ZipCode = _shippingAddr.Address.PostalCode;
                if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                {
                    this.LookupState(_shippingAddr.Address.PostalCode);
                    this.StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                }
                else
                {
                    dnlState.Items.Clear();
                }
                this.PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{6})$") || !this.LookupState(_shippingAddr.Address.PostalCode))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                    dnlState.Items.Clear();
                }
            }

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Phone, @"^[0-9]{10,15}$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        private bool LookupState(string zipCode)
        {
            dnlState.Items.Clear();
            if (!string.IsNullOrEmpty(zipCode) && zipCode.Trim().Length > 3)
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
                if (provider != null)
                {
                    var lookupResults = provider.LookupCitiesByZip(ProductsBase.CountryCode, zipCode.Substring(0, 3));
                    if (lookupResults != null && lookupResults.Count > 0)
                    {
                        var items = (from s in lookupResults select new ListItem { Text = s.State, Value = s.State }).ToArray();
                        this.dnlState.Items.AddRange(items);
                        if (lookupResults.Count > 1)
                        {
                            this.dnlState.Items.Insert(0, new ListItem(this.GetLocalResourceObject("Select") as string, string.Empty));
                            this.dnlState.SelectedIndex = 0;
                            dnlState.Focus();
                        }
                        else
                        {
                            txtArea.Focus();
                        }
                        this.dnlState.SelectedIndex = 0;
                        return true;
                    }
                }
            }
            return false;
        }
        
        #endregion

        #region Event handlers

        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            dnlState.Items.Clear();
            if (!string.IsNullOrEmpty(txtPostCode.Text))
            {
                LookupState(txtPostCode.Text);
            }
        }

        #endregion
    }
}