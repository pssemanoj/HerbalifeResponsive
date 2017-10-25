using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class NIAddressControl : AddressControlBase
    {
        #region Fields and Properties

        /// <summary>
        ///     To store the tab index
        /// </summary>
        private short tabIndex;

        /// <summary>
        ///     Gets or sets the tab index of the control.
        /// </summary>
        protected override short TabIndex
        {
            get { return tabIndex; }
            set
            {
                tabIndex = value;
                txtCareOfName.TabIndex = ( value++ );
                txtStreet.TabIndex = ( value++ );
                txtStreet2.TabIndex = ( value++ );
                dnlCity.TabIndex = ( value++ );
                dnlState.TabIndex = ( value++ );
                dnlCity.TabIndex = ( value++ );
                txtNumber.TabIndex = ( value++ );
            }
        }

        /// <summary>
        ///     Gets or sets the city.
        /// </summary>
        protected override string City
        {
            set
            {
                if ( !string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                var item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if ( null != item )
                {
                    item.Selected = true;
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                    ? string.Empty
                    : dnlCity.SelectedItem.Text.Trim();
            }
        }

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
        ///     Gets the required field messages.
        /// </summary>
        protected override RequireFieldDef[] RequiredFields
        {
            get
            {
                Func<string, string> getErrMsg = (itemName) =>
                    PlatformResources.GetGlobalResourceString("ErrorMessage", itemName);

                return new[] {
                    new RequireFieldDef {
                        Field = _shippingAddr.Recipient,
                        ErrorMsg = getErrMsg("NoCareOfName")
                    },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.Line1,
                        ErrorMsg = getErrMsg("NoStreet1")
                    },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.StateProvinceTerritory,
                        ErrorMsg = getErrMsg("NoState")
                    },
                    new RequireFieldDef {
                        Field = _shippingAddr.Address.City,
                        ErrorMsg = getErrMsg("NoCity")
                    },
                    new RequireFieldDef {
                        Field = _shippingAddr.Phone,
                        ErrorMsg = getErrMsg("NoPhone")
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
                if ( !string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if ( null != item )
                {
                    item.Selected = true;
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

        #endregion

        #region Public Methods

        /// <summary>
        ///     Load the page.
        /// </summary>
        public override void LoadPage()
        {
            if ( _shippingAddr != null )
            {
                dnlState.Items.Clear();

                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;

                if ( LookupStates() )
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                if ( LookupCities(StateProvince) )
                    City = _shippingAddr.Address.City;

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
        /// <param name="_errors"></param>
        public override void ValidationCheck(List<string> _errors)
        {
            if ( !string.IsNullOrEmpty(_shippingAddr.Address.PostalCode)
                && !Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{5})$") )
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
            if ( !string.IsNullOrEmpty(_shippingAddr.Phone)
                && !Regex.IsMatch(_shippingAddr.Phone, @"^(\d{8,12})$") )
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        #endregion

        #region Private Methods and Event Hanldlers

        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
        }

        /// <summary>
        ///     Get the states from provider.
        /// </summary>
        /// <returns></returns>
        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if ( provider != null )
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if ( lookupResults != null && lookupResults.Count > 0 )
                {
                    var items = ( from s in lookupResults
                                  select
                                      new ListItem
                                          {
                                              Value = s,
                                              Text = s
                                          } ).ToArray();
                    dnlState.Items.AddRange(items);
                    lookedUp = true;
                }
            }

            dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlState.SelectedIndex = 0;

            return lookedUp;
        }

        /// <summary>
        /// Get the cities for the selected state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlCity.Text = string.Empty;
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if ( provider != null )
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if ( lookupResults != null && lookupResults.Count > 0 )
                {
                    var items = ( from s in lookupResults
                                  select new ListItem() { Value = s, Text = s } ).ToArray();
                    dnlCity.Items.AddRange(items);
                    lookedUp = true;
                }
            }

            dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlCity.SelectedIndex = 0;

            return lookedUp;
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( !string.IsNullOrEmpty(dnlState.SelectedItem.Value) )
            {
                LookupCities(dnlState.SelectedItem.Value);
                dnlCity.Focus();
            }
            else
            {
                dnlCity.Items.Clear();
                dnlCity.Text = string.Empty;
            }
        }

        #endregion
    }
}