using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    /// <summary>
    ///     Shipping address control for Paraguay
    /// </summary>
    public partial class PYAddressControl : AddressControlBase
    {
        #region Constants and Fields

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        /// <summary>
        ///     Gets or sets the recipient name.
        /// </summary>
        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        /// <summary>
        ///     Gets the required fields messages.
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
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.PostalCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
                            }

                    };
            }
        }

        /// <summary>
        ///     Gets or sets the state.
        /// </summary>
        protected override string StateProvince
        {
            set
            {
                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCities(value);
                }
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
        }

        /// <summary>
        ///     Gets or sets the city.
        /// </summary>
        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByValue(value);
                dnlCity.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                    LookupPostal(StateProvince, value);
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Value;
            }
        }

        /// <summary>
        ///     Gets or sets the street.
        /// </summary>
        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return txtStreet.Text.Trim(); }
        }

        /// <summary>
        ///     Gets or sets the street (line 2).
        /// </summary>
        protected override string StreetAddress2
        {
            get { return txtStreet2.Text.Trim(); }
            set { txtStreet2.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the zip code.
        /// </summary>
        protected override string ZipCode
        {
            get
            {
                return dnlPostCode.SelectedItem == null || dnlPostCode.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlPostCode.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlPostCode.Items.FindByText(value);
                dnlPostCode.SelectedIndex = -1;
                if (null != item) item.Selected = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Load the page.
        /// </summary>
        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlState.Items.Clear();
                dnlCity.Items.Clear();
                dnlPostCode.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupStates())
                {
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                }
                City = _shippingAddr.Address.City;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            else
            {
                LookupStates();
            }
        }

        /// <summary>
        ///     Validate the control.
        /// </summary>
        /// <param name="_errors">List of errors.</param>
        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{6,10})$"))
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        #endregion

        #region Private Methods 

        /// <summary>
        ///     Get the cities of a state from provider.
        /// </summary>
        /// <param name="state">State name.</param>
        /// <returns>Success flag.</returns>
        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlPostCode.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                        var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                        dnlCity.Items.AddRange(items);
                    dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlState.Focus();
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Get the states from provider.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults select new ListItem {Text = s, Value = s}).ToArray();
                    dnlState.Items.AddRange(items);
                    dnlState.Items.Insert(
                        0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupPostal(string state, string city)
        {
            bool lookedUp = false;
            dnlPostCode.Items.Clear();
            List<string> lookupResults = new List<string>();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
               lookupResults = provider.GetAddressField(new AddressFieldForCountryRequest_V01()
                    {
                        AddressField = AddressPart.ZIPCODE,
                        Country = ProductsBase.CountryCode,
                        State = state,
                        City = city
                    });

               if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var zip in lookupResults)
                    {
                        dnlPostCode.Items.Add(new ListItem(zip));
                    }
                    if (dnlPostCode.Items.Count != 1)
                    {
                        dnlPostCode.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlPostCode.SelectedIndex = 0;
                    }
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }
            return lookedUp;
        }
        #endregion

        #region  Event Handlers
        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value))
            {
                if (LookupCities(dnlState.SelectedItem.Value))
                {
                    dnlCity.Focus();
                }
                else
                {
                    dnlState.Focus();
                }
            }
            else
            {
                dnlCity.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCity.SelectedItem.Text) && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupPostal(dnlState.SelectedItem.Text, dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlPostCode.Items.Clear();
            }
        }
  
        #endregion
    }
}