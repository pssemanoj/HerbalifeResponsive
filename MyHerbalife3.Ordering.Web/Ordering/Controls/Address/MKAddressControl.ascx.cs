using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class MKAddressControl : AddressControlBase
    {
        #region Constants and Fields

        /// <summary>
        ///     To store the tab index
        /// </summary>
        private short _tabIndex;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the state province.
        /// </summary>
        /// <value>
        /// The state province.
        /// </value>
        protected override string StateProvince
        {
            get
            {
                return ProductsBase.CountryCode;
            }
        }


        /// <summary>
        ///     Gets or sets the city
        /// </summary>
        protected override string City
        {
            get
            {
                if (dnlCity.SelectedItem == null || dnlCity.SelectedValue.Equals("0"))
                {
                    return string.Empty;
                }
                else
                {
                    return dnlCity.SelectedItem.Text;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    dnlCity.SelectedIndex = 0;
                }
                else
                {
                    var city = value;

                    RadComboBoxItem itemCity = dnlCity.Items.FindItemByText(city);
                    dnlCity.SelectedIndex = 0;
                    if (itemCity != null)
                    {
                        itemCity.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the phone number
        /// </summary>
        protected override string PhoneNumber
        {
            set { txtPhoneNumber.Text = value; }
            get { return txtPhoneNumber.Text; }
        }

        /// <summary>
        ///     Gets or sets the recipient
        /// </summary>
        protected override string Recipient
        {
            get { return txtCareOfName.Text.Trim(); }
            set { txtCareOfName.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the required field array to validate
        /// </summary>
        protected override RequireFieldDef[] RequiredFields
        {
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
                                Field = dnlCity.SelectedItem != null ? dnlCity.SelectedItem.Value : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line1,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line2,
                                ErrorMsg = GetLocalResourceObject("NoStreet2").ToString()
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            }
                    };
            }
        }

        /// <summary>
        ///     Gets or sets the street line 1
        /// </summary>
        protected override string StreetAddress
        {
            get { return txtStreet1.Text.Trim(); }
            set { txtStreet1.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the street line 2
        /// </summary>
        protected override string StreetAddress2
        {
            get { return txtStreet2.Text.Trim(); }
            set { txtStreet2.Text = value; }
        }

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
                txtStreet1.TabIndex = (value++);
                txtStreet2.TabIndex = (value++);
                dnlCity.TabIndex = (value++);
                txtPhoneNumber.TabIndex = (value++);
            }
        }

        /// <summary>
        ///     Gets or sets zipCode
        /// </summary>
        protected override string ZipCode
        {
            set { txtPostalCode.Text = value; }
            get { return txtPostalCode.Text.Trim(); }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads the page
        /// </summary>
        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                if (LookupCities())
                {
                    City = _shippingAddr.Address.City;
                }
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            txtCareOfName.Focus();
        }

        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPostalCode.Text = dnlCity.SelectedItem.Value;
            txtPhoneNumber.Focus();
        }

        /// <summary>
        ///     Gets the cities from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupCities()
        {
            dnlCity.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(
                    ProductsBase.CountryCode);
            if (provider != null)
            {
                // For Macedonia, state code is same as country code in db
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, string.Empty);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var cityZip in lookupResults)
                    {
                        // value is postal code, text is city name
                        dnlCity.Items.Add(
                            new RadComboBoxItem(cityZip.Split(new[] {','})[1], cityZip.Split(new[] {','})[0]));
                    }
                    dnlCity.Items.Insert(0,
                                         new RadComboBoxItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    return true;
                }
            }
            return false;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                if (!Regex.IsMatch(PhoneNumber, @"^(\d{8,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        #endregion
    }
}