using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class PFAddressControl : AddressControlBase
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
        #endregion

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
                this.ZipCode = _shippingAddr.Address.PostalCode;
                this.PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {

            if (!string.IsNullOrEmpty(Recipient) && (Recipient.Trim().Length < 1 || Recipient.Trim().Length > 40))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            }

            if (!string.IsNullOrEmpty(StreetAddress) && (StreetAddress.Trim().Length < 1 || StreetAddress.Trim().Length > 40))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            }

            if (!string.IsNullOrEmpty(City) && (City.Trim().Length < 1 || City.Trim().Length > 20))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCity"));
            }

            if (!string.IsNullOrEmpty(ZipCode))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(ZipCode, @"^(\d{5})$"))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                }
            }

            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Phone, @"^(\d{10,12})$"))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                }
            }
        }

        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPostCode.Text) && !string.IsNullOrEmpty(txtCity.Text))
            {
                ValidatePostalCode(txtCity.Text, txtPostCode.Text);
            }            
        }

        private void ValidatePostalCode(string city, string postalCode)
        {
            // Validating zip code entries.
            if (postalCode.Length.Equals(5))
            {
                string zipCode = postalCode;
                bool zipFound = false;
                // Search the address by the provided zip code.
                var shippingProvider = new ShippingProvider_PF();
                var stateCityResults = shippingProvider.LookupCitiesByZip("PF", zipCode);
                if (stateCityResults != null && stateCityResults.Count > 0)
                {
                    foreach (StateCityLookup_V01 sc in stateCityResults)
                    {
                        if (city.Equals(sc.City, StringComparison.CurrentCultureIgnoreCase))
                        {
                            zipFound = true;
                            break;
                        }
                    }

                    if (zipFound)
                    {
                        lblNoMatch.Visible = false;
                        txtNumber.Focus();
                    }
                    else
                    {
                        lblNoMatch.Visible = true;
                        txtPostCode.Text = "";
                        txtPostCode.Focus();
                    }
                }
                else
                {
                    lblNoMatch.Visible = true;
                    txtPostCode.Text = "";
                    txtPostCode.Focus();
                }
            }
        }

        protected void txtCity_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPostCode.Text) && !string.IsNullOrEmpty(txtCity.Text))
            {
                ValidatePostalCode(txtCity.Text, txtPostCode.Text);
            }
        }
        
    }
}