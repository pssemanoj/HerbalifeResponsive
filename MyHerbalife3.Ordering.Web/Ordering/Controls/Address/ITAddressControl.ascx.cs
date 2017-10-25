using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class ITAddressControl : AddressControlBase
    {
        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return getSimplifiedText(txtCareOfName.Text.Trim()); }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return getSimplifiedText(txtStreet.Text.Trim()); }
        }

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
            get { return txtState.Text; }
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
                dnlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                ZipCode = _shippingAddr.Address.PostalCode;
                if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    LookupCities(txtPostCode.Text);
                City = _shippingAddr.Address.City;
                StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public override void ValidationCheck(List<string> _errors)
        {
            //31695: GDO:All:ShippingAddress:All:Special Characters not respecting the BRD

            //if ((_shippingAddr.Recipient != null) && (!_shippingAddr.Recipient.Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Recipient,
            //           @"(^[a-zA-Z\/,\s-.'\\]+$)"))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            //}

            //if ((_shippingAddr.Address.Line1 != null) && (!_shippingAddr.Address.Line1.Equals(string.Empty)))
            //{
            //    if (!StreetAddressRegularExpressionCheck(_shippingAddr.Address.Line1))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            //}

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{9,12})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        private bool StreetAddressRegularExpressionCheck(String streetAddress)
        {
            string alphaPattern = @"[a-zA-Z,-.']+";
            string numberPattern = @"[\d]+";
            string spacePattern = @"[\s]+";

            bool alphaMatch = Regex.IsMatch(streetAddress,
                                            alphaPattern);
            bool numberMatch = Regex.IsMatch(streetAddress,
                                             numberPattern);
            bool spaceMatch = Regex.IsMatch(streetAddress,
                                            spacePattern);

            if (alphaMatch && numberMatch && spaceMatch)
                return true;
            else
                return false;
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtState.Text = (dnlCity.SelectedValue.Split('#'))[0];
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
            //Text = Text.Replace("'", " ");
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
                        dnlCity.Items.Insert(i,
                                             new ListItem(lookupResults[i].City,
                                                          lookupResults[i].State + "#" + lookupResults[i].City));
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