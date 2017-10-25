using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class BRAddressControl : AddressControlBase
    {
        /// <summary>
        ///     To store the tab index
        /// </summary>
        private short _tabIndex;

        /// <summary>
        ///     Gets or sets the tab index of the control
        /// </summary>
        protected override short TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                txtCareOfName.TabIndex = (value++);
                txtPostCode.TabIndex = (value++);
                txtPostCode2.TabIndex = (value++);
                //this.txtCity.TabIndex = (short)(value++);
                txtStreet.TabIndex = (value++);
                txtNumber.TabIndex = (value++);
                txtStreet2.TabIndex = (value++);
                txtNeighborhood.TabIndex = (value++);
                //this.txtState.TabIndex = (short)(value++);
                txtAreaCode.TabIndex = (value);
                txtPhoneNumber.TabIndex = (value);
            }
        }

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

        protected string StreetNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text.Trim(); }
        }

        protected override string StreetAddress2
        {
            set { txtStreet2.Text = value; }
            get { return txtStreet2.Text.Trim(); }
        }

        protected override string City
        {
            set { txtCity.Text = value; }
            get { return txtCity.Text.Trim(); }
        }

        protected override string ZipCode
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 5)
                {
                    txtPostCode.Text = value.Substring(0, 5);
                    txtPostCode2.Text = value.Substring(5);
                }
                else
                {
                    txtPostCode.Text = txtPostCode2.Text = value;
                }
            }
            get { return string.Concat(txtPostCode.Text.Trim(), txtPostCode2.Text.Trim()); }
        }

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { txtPhoneNumber.Text = value; }
            get { return txtPhoneNumber.Text; }
        }

        protected string Neighborhood
        {
            set { txtNeighborhood.Text = value; }
            get { return txtNeighborhood.Text; }
        }

        protected override string StateProvince
        {
            set { txtState.Text = value; }
            get { return txtState.Text; }
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
                                Field = _shippingAddr.Address.Line2,
                                ErrorMsg = GetLocalResourceObject("NoNeighborhood").ToString()
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
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line3,
                                ErrorMsg = GetLocalResourceObject("NoStreetNumberErrorMessage").ToString()
                            },
                        new RequireFieldDef
                            {
                                Field = AreaCode,
                                ErrorMsg = GetLocalResourceObject("NoAreaCodeErrorMessage").ToString()
                            }
                    };
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
        }

        private void InitializeStreetAndNeighborhood(bool enabled)
        {
            if (enabled)
            {
                txtStreet.Attributes.Remove("onfocus");
                txtStreet.Attributes.Remove("onclick");
                txtNeighborhood.Attributes.Remove("onfocus");
                txtNeighborhood.Attributes.Remove("onclick");
                txtStreet.CssClass = txtStreet.CssClass.Replace("disabled", string.Empty);
                txtNeighborhood.CssClass = txtNeighborhood.CssClass.Replace("disabled", string.Empty);
            }
            else
            {
                txtStreet.Attributes.Add("onfocus", "blur();");
                txtStreet.Attributes.Add("onclick", "blur();");
                txtNeighborhood.Attributes.Add("onfocus", "blur();");
                txtNeighborhood.Attributes.Add("onclick", "blur();");
                txtStreet.CssClass = txtStreet.CssClass.Contains("disabled")
                                         ? txtStreet.CssClass
                                         : string.Format("{0} disabled", txtStreet.CssClass);
                txtNeighborhood.CssClass = txtNeighborhood.CssClass.Contains("disabled")
                                               ? txtNeighborhood.CssClass
                                               : string.Format("{0} disabled", txtNeighborhood.CssClass);
            }
        }

        public override void LoadPage()
        {
            txtCity.Attributes.Add("onfocus", "blur();");
            txtCity.Attributes.Add("onclick", "blur();");
            txtState.Attributes.Add("onfocus", "blur();");
            txtState.Attributes.Add("onclick", "blur();");
            InitializeStreetAndNeighborhood(true);
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;

                if (_shippingAddr.Address != null)
                {
                    if (!string.IsNullOrEmpty(_shippingAddr.Address.Line4))
                    {
                        var streets = _shippingAddr.Address.Line4.Split(new[] {"%%%"},
                                                                        StringSplitOptions.RemoveEmptyEntries);
                        if (streets.Length > 0)
                        {
                            StreetAddress = streets[0];
                            if (streets.Length > 1)
                            {
                                StreetAddress2 = streets[1];
                            }
                        }
                        else
                        {
                            StreetAddress = string.Empty;
                            StreetAddress2 = string.Empty;
                        }
                    }

                    City = _shippingAddr.Address.City;
                    ZipCode = _shippingAddr.Address.PostalCode;
                    if (txtPostCode2.Text.Equals("000"))
                    {
                        InitializeStreetAndNeighborhood(true);
                    }
                    PhoneNumber = _shippingAddr.Phone;
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                    Neighborhood = _shippingAddr.Address.Line2;
                    ErrorList = new List<string>();
                    StreetNumber = _shippingAddr.Address.Line3;
                    AreaCode = _shippingAddr.AreaCode;
                    GetPostalCodeDetails(txtPostCode.Text, txtPostCode2.Text, false);
                }
            }
            else
            {
                City =
                    ZipCode =
                    PhoneNumber =
                    StateProvince =
                    Neighborhood =
                    StreetNumber =
                    AreaCode =
                    StreetAddress =
                    StreetAddress2 = string.Empty;
                ErrorList = new List<string>();
            }
            lblNoMatch.Visible = false;
            txtCareOfName.Focus();
        }

        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPostCode.Text) && string.IsNullOrEmpty(txtPostCode2.Text))
            {
                txtStreet.Text = txtNeighborhood.Text = txtStreet.Text = txtState.Text = txtCity.Text = string.Empty;
                return;
            }
            GetPostalCodeDetails(txtPostCode.Text, txtPostCode2.Text);
        }

        /// <summary>
        ///     Get the address details.
        /// </summary>
        /// <param name="postalCode">Postal code 1.</param>
        /// <param name="postalCode2">Postal code 2.</param>
        /// <param name="clearInfo">Clear not automatic fields.</param>
        private void GetPostalCodeDetails(string postalCode, string postalCode2, bool clearInfo = true)
        {
            //Reset fields
            InitializeStreetAndNeighborhood(false);
            if (clearInfo)
            {
                txtStreet.Text = txtNeighborhood.Text = txtStreet.Text = txtState.Text = txtCity.Text = string.Empty;
            }

            // Validating zip code entries.
            if (postalCode.Length.Equals(5) && !string.IsNullOrEmpty(postalCode2))
            {
                string zipCode = ZipCode;
                // Search the address by the provided zip code.
                var shippingProvider = new ShippingProvider_BR();
                var addressResults = shippingProvider.AddressSearch(zipCode);
                if (addressResults != null && addressResults.Count > 0)
                {
                    lblNoMatch.Visible = false;
                    txtCity.Text = addressResults[0].City;
                    txtState.Text = addressResults[0].StateProvinceTerritory;
                    txtStreet.Text = string.IsNullOrEmpty(txtStreet.Text) ? addressResults[0].Line1 : txtStreet.Text;
                    txtNeighborhood.Text = string.IsNullOrEmpty(txtNeighborhood.Text) ? addressResults[0].Line2 : txtNeighborhood.Text;

                    // Finally enable the street and neighborhood text boxes.
                    if (postalCode2.Equals("000") || string.IsNullOrEmpty(addressResults[0].Line1) || string.IsNullOrEmpty(addressResults[0].Line2))
                    {
                        InitializeStreetAndNeighborhood(true);
                        txtStreet.Focus();
                    }
                    else
                    {
                        txtNumber.Focus();
                    }               
                }
                else
                {
                    lblNoMatch.Visible = true;
                }
            }
            else
            {
                lblNoMatch.Visible = true;
            }
        }

        public override object CreateAddressFromControl(string typeName)
        {
            object dataContext = DataContext;
            var shipping = new ShippingAddress_V02();
            if (dataContext != null)
                shipping = (ShippingAddress_V02) dataContext;
            var shippingAddress = new Address_V01();

            shippingAddress.Line1 = string.Concat(txtStreet.Text, " ", txtStreet2.Text).Trim();
            shippingAddress.Line2 = txtNeighborhood.Text;
            shippingAddress.Line3 = StreetNumber;
            shippingAddress.Line4 = string.Concat(txtStreet.Text, "%%%", txtStreet2.Text);

            shippingAddress.City = City;
            shippingAddress.CountyDistrict = County ?? string.Empty;
            shippingAddress.StateProvinceTerritory = StateProvince;
            shippingAddress.PostalCode = ZipCode;
            shipping.Address = shippingAddress;
            shipping.Phone = PhoneNumber;
            shipping.AreaCode = AreaCode;
            shipping.Recipient = Recipient;

            return _shippingAddr = shipping;
        }

        /// <summary>
        ///     Custom control validation
        /// </summary>
        /// <param name="_errors">Error list</param>
        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(Recipient) && Recipient.Trim().Length > 40)
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            }
            if (!string.IsNullOrEmpty(txtNumber.Text) && txtNumber.Text.Trim().Length > 8)
            {
                _errors.Add(GetLocalResourceObject("InvalidStreetNumber") as string);
            }
            if (!string.IsNullOrEmpty(ZipCode) && !Regex.IsMatch(ZipCode, @"^(\d{8})$"))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
            if (!string.IsNullOrEmpty(AreaCode) && !Regex.IsMatch(AreaCode, @"^(\d{2})$"))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));
            }
            if (!string.IsNullOrEmpty(PhoneNumber) && !Regex.IsMatch(PhoneNumber, @"^(\d{7,9})$"))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }
    }
}