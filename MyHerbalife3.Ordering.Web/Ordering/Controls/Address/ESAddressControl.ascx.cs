using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class ESAddressControl : AddressControlBase
    {
        private string _countryCode;

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return (txtCareOfName.Text.Trim()); }
        }

        protected override string StreetAddress
        {
            set { txtStreet1.Text = value; }
            get { return (txtStreet1.Text.Trim()); }
        }

        //protected override string StreetAddress2
        //{
        //    set 
        //    {
        //        txtStreet2.Text = value;
        //    }
        //    get
        //    {
        //        return (this.txtStreet2.Text.Trim());
        //    }
        //}

        protected override string City
        {
            set
            {
                var item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (null != item) item.Selected = true;
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
            set { txtState.Text = value; }
            get { return (txtState.Text.Trim()); }
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

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                //this.StreetAddress2 = _shippingAddr.Address.Line2;

                if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    LookupCities(_shippingAddr.Address.PostalCode);
                City = _shippingAddr.Address.City;

                StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _countryCode = ProductsBase.CountryCode;
            txtCareOfName.Focus();
        }

        public override void ValidationCheck(List<string> _errors)
        {
            //31695: GDO:All:ShippingAddress:All:Special Characters not respecting the BRD
            //if ((_shippingAddr.Recipient != null) && (!_shippingAddr.Recipient.Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Recipient,
            //           @"^[\w\s\/,-.'\\]+$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            //}

            //if ((_shippingAddr.Address.Line1 != null) && (!_shippingAddr.Address.Line1.Equals(string.Empty)))
            //{
            //    //if (!StreetAddressRegularExpressionCheck(_shippingAddr.Address.Line1))
            //    //    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress);
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Address.Line1,
            //           @"^[\w\s\/,-.'\\]+$"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress2"));
            //}

            //if ((_shippingAddr.Address.StateProvinceTerritory != null) && (!_shippingAddr.Address.StateProvinceTerritory.Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Address.StateProvinceTerritory,
            //       @"(^[\w\s\/,-.'\\]+$)"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidState);
            //}

            if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{5})$"))
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{9})$"))
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
        }

        private bool StreetAddressRegularExpressionCheck(String streetAddress)
        {
            string alphaPattern = @"[\w]+";
            string numberPattern = @"[\d]+";
            string spacePattern = @"[\s]+";

            bool alphaMatch = Regex.IsMatch(streetAddress,
                                            alphaPattern);
            bool numberMatch = Regex.IsMatch(streetAddress,
                                             numberPattern);
            bool spaceMatch = Regex.IsMatch(streetAddress,
                                            spacePattern);
            //if (!System.Text.RegularExpressions.Regex.IsMatch(streetAddress,
            //       @"([a-zA-Z0-9]+$)"))
            //    return false;

            if (alphaMatch && numberMatch && spaceMatch)
                return true;
            else
                return false;
        }

        protected string getSimplifiedText(string Text)
        {
            Text = Text.Replace("à", "a");
            Text = Text.Replace("è", "e");
            Text = Text.Replace("é", "e");
            Text = Text.Replace("ì", "i");
            Text = Text.Replace("ò", "o");
            Text = Text.Replace("ù", "u");
            Text = Text.Replace("°", " ");
            Text = Text.Replace("'", " ");
            Text = Text.Replace("§", " ");
            Text = Text.Replace("^", " ");
            Text = Text.Replace("*", " ");
            return Text;
        }

        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPostCode.Text))
            {
                LookupCities(txtPostCode.Text);
            }
        }

        private bool LookupCities(String zipCode)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(_countryCode);
            if (provider != null)
            {
                var lookupResults = provider.LookupCitiesByZip(_countryCode, zipCode);
                if (null != lookupResults && lookupResults.Count > 0)
                {
                    for (int i = 0; i < lookupResults.Count; i++)
                    {
                        dnlCity.Items.Insert(i, new ListItem(lookupResults[i].City));
                    }
                    dnlCity.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    txtPostCode.Focus();
                    //blErrors.Items.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                }
            }

            return lookedUp;
        }
    }
}