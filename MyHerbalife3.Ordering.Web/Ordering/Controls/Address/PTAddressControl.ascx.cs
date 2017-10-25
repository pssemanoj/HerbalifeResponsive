using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class PTAddressControl : AddressControlBase
    {
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
            set { }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
            }
        }

        protected override string StateProvince
        {
            set { }
            get { return string.Empty; }
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

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                ZipCode = _shippingAddr.Address.PostalCode;
                if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    LookupCities(txtPostCode.Text);
                var item = dnlCity.Items.FindByText(_shippingAddr.Address.City);
                dnlCity.SelectedIndex = -1;
                if (null != item) item.Selected = true;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PostalCodeHelp.Visible = false;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            PostalCodeHelp.Visible = false;
            //31695: GDO:All:ShippingAddress:All:Special Characters not respecting the BRD. 
            //if ((_shippingAddr.Recipient != null) && (!_shippingAddr.Recipient.Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Recipient,
            //      @"^([a-zA-Z\/,-.'\\ ]+)$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName);
            //}

            //if ((_shippingAddr.Address.Line1 != null) && (!_shippingAddr.Address.Line1.Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Address.Line1,
            //      @"^([a-zA-Z0-9\/,-.'\\ ]+)$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress2"));
            //}

            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{4}-\d{3})$") || dnlCity.Items.Count == 0)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                    PostalCodeHelp.Visible = true;
                }
            }

            //31188: GDO:PT:Checkout:Pick Up field: phone field accept less and more than 9 digits
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{9})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            PostalCodeHelp.Visible = false;
            if (!string.IsNullOrEmpty(txtPostCode.Text))
            {
                LookupCities(txtPostCode.Text);
                if (dnlCity.Items.Count == 0)
                {
                    PostalCodeHelp.Visible = true;
                }
            }
        }

        private bool LookupCities(string postCode)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.LookupCitiesByZip(ProductsBase.CountryCode,
                                                               txtPostCode.Text);
                if (null != lookupResults && lookupResults.Count > 0)
                {
                    for (int i = 0; i < lookupResults.Count; i++)
                    {
                        dnlCity.Items.Insert(i, new ListItem(lookupResults[i].City));
                    }
                    dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    txtPostCode.Focus();
                }
            }

            return lookedUp;
        }
    }
}