using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class HRAddressControl : AddressControlBase
    {
        #region Properties

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
                txtStreet.TabIndex = (value++);
                dnlCity.TabIndex = (value++);
                txtPhoneNumber.TabIndex = (value++);
            }
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
        ///     Gets or sets the street line 1
        /// </summary>
        protected override string StreetAddress
        {
            get { return txtStreet.Text.Trim(); }
            set { txtStreet.Text = value; }
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

                return dnlCity.SelectedItem.Text;
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
        ///     Gets or sets zipCode
        /// </summary>
        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text.Trim(); }
        }

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
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.PostalCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode")
                            }
                    };
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
        }

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
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                if (!this.IsPostBack)
                {
                    LookupCities();
                }
            }
            txtCareOfName.Focus();
        }

        /// <summary>
        ///     Validates the control.
        /// </summary>
        /// <param name="_errors">List of errors.</param>
        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone)
                && !Regex.IsMatch(_shippingAddr.Phone, @"^(\d{11,12})$"))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlCity.SelectedItem != null && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {

                //IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
                //if (provider != null)
                {
                   // var lookupResults = provider.GetZipsForCity(ProductsBase.CountryCode, ProductsBase.CountryCode, dnlCity.SelectedItem.Text);
                    txtPostCode.Text = dnlCity.SelectedItem.Value; // lookupResults[0];
                }

            }
            else
            {
                City = string.Empty;
                dnlCity.SelectedIndex = 0; // select
            }
        }

        /// <summary>
        ///     Gets the cities from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupCities()
        {
            dnlCity.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                // For Serbia, state code is same as country code in db
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var cityZip in lookupResults)
                    {
                        // value is postal code, text is city name
                        dnlCity.Items.Add(new RadComboBoxItem(cityZip.Split(',')[1],
                                                              cityZip.Split(',')[0]));
                    }
                    dnlCity.Items.Insert(0, new RadComboBoxItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    return true;
                }
            }
            return false;
        }
    }
}