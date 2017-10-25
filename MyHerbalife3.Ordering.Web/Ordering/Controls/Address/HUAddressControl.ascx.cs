using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class HUAddressControl : AddressControlBase
    {
        private List<string> selects = new List<string> { "Select", "Kiválaszt" };
        private string Dashes = "---------------";
        List<string> MajorCites = new List<string> { "Debrecen", "Győr", "Miskolc", "Szeged", "Pécs", "Budapest" };

        #region Fields and Properties

        private string selectText = null;
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
                ddlCity.TabIndex = (value++);
                ddlSuburb.TabIndex = (value++);
                ddlDistrict.TabIndex = (value++);
                ddlStreet.TabIndex = (value++);
                ddlStreetType.TabIndex = (value++);
                txtHouseNumber.TabIndex = (value++);
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
        ///     Gets or sets the street name
        /// </summary>
        protected override string StreetAddress
        {
            get
            {
                return (ddlStreet.Visible) ? ((ddlStreet.SelectedItem == null)
                          ? ddlStreet.Text.Trim()
                          : ddlStreet.SelectedItem.Text) : txtStreet.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (ddlStreet.Visible)
                    {
                        ddlStreet.SelectedIndex = -1;
                        ddlStreetType.Text = string.Empty;
                    }
                    else
                    {
                        ddlStreet.Text = string.Empty;
                    }
                }
                else
                {
                    if (ddlStreet.Items.Count == 0 && !string.IsNullOrEmpty(value) && MajorCites.Contains(City))
                    {
                        string city = _shippingAddr != null && _shippingAddr.Address != null ? _shippingAddr.Address.City : string.Empty;
                        string suburb = _shippingAddr != null && _shippingAddr.Address != null ? _shippingAddr.Address.Line2 : string.Empty;
                        string district = _shippingAddr != null && _shippingAddr.Address != null ? _shippingAddr.Address.CountyDistrict : string.Empty;
                        LookupStreets(city, suburb, district);
                    }
                    if (ddlStreet.Visible)
                    {
                        var item = ddlStreet.Items.FindByValue(value);
                        ddlStreet.SelectedIndex = -1;
                        if (item != null)
                        {
                            item.Selected = true;
                            LookupStreetTypes(City, StreetAddress2, County, value);
                        }
                        else
                        {
                            ddlStreet.Text = value;
                        }
                    }
                    else
                    {
                        txtStreet.Text = value;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the suburb
        /// </summary>
        protected override string StreetAddress2
        {
            get
            {
                string suburb = ddlSuburb.Visible ? ((ddlSuburb.SelectedItem == null)
                   ? ddlSuburb.Text.Trim()
                   : ddlSuburb.SelectedItem.Text) : txtSuburb.Text;
                if (valueContains(suburb) || suburb.Equals(Dashes))
                    suburb = string.Empty;
                return suburb;
            }
            set
            {
                if (string.IsNullOrEmpty(value) && !City.Equals("Budapest"))
                {
                    if (ddlSuburb.Visible)
                    {
                        ddlSuburb.SelectedIndex = -1;
                        ddlSuburb.Text = string.Empty;
                    }
                    else
                    {
                        txtSuburb.Text = value;
                    }
                }
                else
                {
                    if (ddlSuburb.Visible)
                    {
                        var item = ddlSuburb.Items.FindByValue(value);
                        ddlSuburb.SelectedIndex = -1;
                        if (item != null)
                        {
                            item.Selected = true;
                        }
                        else
                        {
                            ddlSuburb.Text = value;
                        }
                    }
                    else
                    {
                        txtSuburb.Text = value;
                    }
                }

            }
        }

        /// <summary>
        ///     Gets or sets the street type
        /// </summary>
        protected override string StreetAddress3
        {
            get
            {
                return ddlStreetType.Visible ? ((ddlStreetType.SelectedItem == null)
                           ? ddlStreetType.Text.Trim()
                           : ddlStreetType.SelectedItem.Text) : txtStreetType.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (ddlStreetType.Visible)
                    {
                        ddlStreetType.SelectedIndex = -1;
                        ddlStreetType.Text = string.Empty;
                    }
                    else
                    {
                        txtStreetType.Text = string.Empty;
                    }
                }
                else
                {
                    if (ddlStreetType.Visible)
                    {
                        var item = ddlStreet.Items.FindByValue(value);
                        ddlStreetType.SelectedIndex = -1;
                        if (item != null)
                        {
                            item.Selected = true;
                        }
                        else
                        {
                            ddlStreetType.Text = value;
                        }
                    }
                    else
                    {
                        txtStreetType.Text = value;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the house number
        /// </summary>
        protected override string StreetAddress4
        {
            get { return txtHouseNumber.Text; }
            set { txtHouseNumber.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the city
        /// </summary>
        protected override string City
        {
            get
            {
                if (ddlCity.SelectedItem == null)
                {
                    return string.Empty;
                }
                return ddlCity.SelectedItem.Text;
            }
            set
            {
                var item = ddlCity.Items.FindByValue(value);
                ddlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupSuburbs(value);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the district
        /// </summary>
        protected override string County
        {
            get
            {
                string district = ddlDistrict.Visible ? ((ddlDistrict.SelectedItem == null)
                   ? ddlDistrict.Text.Trim()
                   : ddlDistrict.SelectedItem.Text) : txtDistrict.Text;
                if (valueContains(district))
                    district = string.Empty;
                return district;
            }
            set
            {
                if (ddlDistrict.Items.Count == 0)
                {
                    string city = _shippingAddr != null && _shippingAddr.Address != null ? _shippingAddr.Address.City : string.Empty;
                    string suburb = _shippingAddr != null && _shippingAddr.Address != null ? _shippingAddr.Address.Line2 : string.Empty;
                    LookupDistricts(city, suburb);
                }
                if (ddlDistrict.Visible)
                {
                    var item = ddlDistrict.Items.FindByValue(value);
                    ddlDistrict.SelectedIndex = -1;
                    if (item != null)
                    {
                        item.Selected = true;
                        LookupStreets(City, StreetAddress2, value);
                    }
                }
                else
                    txtDistrict.Text = value;
            }
        }

        /// <summary>
        ///     Gets or sets the zip code
        /// </summary>
        protected override string ZipCode
        {
            get
            {
                string zipCode = ddlPostalCode.Visible ? ddlPostalCode.SelectedItem.Text : txtPostalCode.Text;

                return zipCode;
            }
            set
            {
                if (ddlPostalCode.Visible)
                {
                    var item = ddlPostalCode.Items.FindByValue(value);
                    ddlPostalCode.SelectedIndex = -1;
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                }
                else
                    txtPostalCode.Text = value;
            }
        }

        /// <summary>
        ///     Gets or sets the phone number
        /// </summary>
        protected override string PhoneNumber
        {
            get { return txtPhoneNumber.Text; }
            set { txtPhoneNumber.Text = value; }
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
                                Field = ddlCity.SelectedItem != null
                                            ? (valueContains(ddlCity.SelectedItem.Value)
                                                   ? string.Empty
                                                   : ddlCity.SelectedItem.Value)
                                            : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
                            },
                        new RequireFieldDef
                            {
                                Field =  valueContains(StreetAddress)? string.Empty: StreetAddress,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field =  valueContains(StreetAddress3)? string.Empty: StreetAddress3,
                                ErrorMsg = GetLocalResourceObject("NoStreetType") as string
                            },
                        new RequireFieldDef
                            {
                                Field = txtHouseNumber.Text,
                                ErrorMsg = GetLocalResourceObject("NoHouseNumber") as string
                            },
                        new RequireFieldDef
                            {
                                Field = ZipCode,
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
            TabIndex = 1;
            selectText = GetLocalResourceObject("Select") as string;
        }

        /// <summary>
        ///     Validates the control
        /// </summary>
        /// <param name="errors">Error list.</param>
        public override void ValidationCheck(List<string> errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^[0-9]{8,9}$"))
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
            if (!string.IsNullOrEmpty(_shippingAddr.Address.Line4))
            {
                if (_shippingAddr.Address.Line4.Length < 1 || _shippingAddr.Address.Line4.Length > 10)
                    errors.Add(GetLocalResourceObject("InvalidHouseNumber") as string);
            }
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^[0-9]{4}$"))
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
        }

        /// <summary>
        ///     Loads the page
        /// </summary>
        public override void LoadPage()
        {
            //ddlCity.SelectedIndexChanged += ddlCity_SelectedIndexChanged;
            //ddlCity.ItemsRequested += ddlCity_ItemsRequested;
            //ddlStreet.SelectedIndexChanged += ddlStreet_SelectedIndexChanged;
            //ddlStreetType.SelectedIndexChanged += ddlStreetType_SelectedIndexChanged;
            //ddlSuburb.SelectedIndexChanged += ddlSuburb_SelectedIndexChanged;
            //ddlDistrict.SelectedIndexChanged += ddlDistrict_SelectedIndexChanged;
            selectText = GetLocalResourceObject("Select") as string;
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                LookupCities();
                City = _shippingAddr.Address.City;
                StreetAddress2 = _shippingAddr.Address.Line2; // suburb
                County = _shippingAddr.Address.CountyDistrict; // district
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress3 = _shippingAddr.Address.Line3;
                StreetAddress4 = _shippingAddr.Address.Line4;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            if (!this.IsPostBack)
                txtCareOfName.Focus();
        }

        /// <summary>
        ///     Gets the cities from provider.
        /// </summary>
        private void LookupCities()
        {
            ddlCity.Items.Clear();
            ddlStreet.Items.Clear();
            ddlStreetType.Items.Clear();
            ddlDistrict.Items.Clear();
            ddlSuburb.Items.Clear();

            txtPostalCode.Text = string.Empty;
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                // We use the state field from service to store the city
                List<string> lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    lookupResults.Remove(selectText);
                    lookupResults.Insert(0, selectText);
                    ddlCity.DataSource = lookupResults;
                    ddlCity.DataBind();
                }
            }
        }

        /// <summary>
        ///     Get the suburbs from provider
        /// </summary>
        /// <param name="city">The city</param>
        private void LookupSuburbs(string city)
        {
            ddlDistrict.Items.Clear();

            ddlSuburb.Items.Clear();
            ddlSuburb.SelectedIndex = -1;
            ddlSuburb.SelectedValue = null;
            ddlSuburb.ClearSelection();

            ddlStreet.Items.Clear();
            ddlStreetType.Items.Clear();

            var provider = new ShippingProvider_HU();
            var lookupResults = provider.GetSuburbsForCity(ProductsBase.CountryCode, city);
            if (lookupResults != null && lookupResults.Count > 0)
            {
                lookupResults.Remove(selectText);
                if (lookupResults[0] == string.Empty)
                {
                    lookupResults.Remove(string.Empty);
                    lookupResults.Insert(0, Dashes);
                }
                lookupResults.Insert(0, selectText);
                ddlSuburb.DataSource = lookupResults;
                ddlSuburb.DataBind();
                ddlSuburb.ClearSelection();
                ddlSuburb.Items[0].Selected = true;
                ddlSuburb.Visible = true;
                txtSuburb.Visible = false;
            }
            else
            {
                ddlSuburb.Visible = false;
                txtSuburb.Visible = true;
                ddlDistrict.Visible = false;
                txtDistrict.Visible = true;

                LookupDistrictsAndSetUp(city, string.Empty);
            }

            //if (ddlSuburb.SelectedValue == string.Empty)
            //{
            //    LookupStreets(city, string.Empty, string.Empty);
            //}
        }

        /// <summary>
        ///     Gets the districts from provider
        /// </summary>
        /// <param name="city">The city</param>
        /// <param name="suburb">The suburb</param>
        private void LookupDistricts(string city, string suburb)
        {
            ddlDistrict.Items.Clear();
            ddlDistrict.SelectedIndex = -1;
            ddlDistrict.SelectedValue = null;
            ddlDistrict.ClearSelection();

            txtPostalCode.Text = string.Empty;
            if (string.IsNullOrEmpty(city)) return;
            suburb = (selectText == suburb) ? string.Empty : suburb;

            var provider = new ShippingProvider_HU();
            var lookupResults = provider.GetDistrictsForCity(ProductsBase.CountryCode, city, suburb);
            if (lookupResults != null && lookupResults.Count > 0)
            {
                lookupResults.Remove(selectText);
                if (lookupResults.Count > 1)
                {
                    lookupResults.Insert(0, selectText);
                }
                ddlDistrict.DataSource = lookupResults;
                ddlDistrict.DataBind();
                ddlDistrict.SelectedIndex = 0;
                if (lookupResults.Count == 1)
                {
                    ddlDistrict_SelectedIndexChanged(ddlDistrict, null);
                }
                if (lookupResults.Count > 0)
                {
                    ddlDistrict.Visible = true;
                    txtDistrict.Visible = false;
                }
            }
            else
            {
                ddlDistrict.Visible = false;
                txtDistrict.Visible = true;
                ddlStreet.Visible = false;
                txtStreet.Visible = true;
                ddlStreetType.Visible = false;
                txtStreetType.Visible = true;
            }

            //if (ddlDistrict.SelectedValue == string.Empty)
            //{
            //   LookupStreets(city, suburb, string.Empty);
            //}
        }

        private void LookupStreets(string city, string suburb, string district)
        {
            ddlStreetType.Items.Clear();
            ddlStreetType.SelectedIndex = -1;
            ddlStreetType.SelectedValue = null;
            ddlStreetType.ClearSelection();

            txtPostalCode.Text = string.Empty;

            ddlStreet.Items.Clear();
            ddlStreet.SelectedIndex = -1;
            ddlStreet.SelectedValue = null;
            ddlStreet.ClearSelection();

            var provider = new ShippingProvider_HU();
            var lookupResults = provider.GetStreets(ProductsBase.CountryCode, city, suburb, district);
            if (lookupResults != null && lookupResults.Count > 0)
            {
                lookupResults.Remove("^");
                lookupResults.Remove(selectText);
                if (lookupResults.Count > 0)
                {
                    ddlStreet.Visible = true;
                    txtStreet.Visible = false;
                    if (lookupResults.Count > 1)
                        lookupResults.Insert(0, selectText);
                    ddlStreet.DataSource = lookupResults;
                    ddlStreet.DataBind();
                    ddlStreet.SelectedIndex = 0;
                    if (lookupResults.Count == 1)
                    {
                        ddlStreet_SelectedIndexChanged(ddlStreet, null);
                    }
                }
            }
            else
            {
                ddlStreet.Visible = false;
                txtStreet.Visible = true;
            }

            if (ddlStreet.SelectedValue == string.Empty)
            {
                LookupStreetTypes(city, suburb, district, string.Empty);
            }
        }

        private void LookupStreetTypes(string city, string suburb, string district, string street)
        {
            ddlStreetType.Items.Clear();
            ddlStreetType.SelectedIndex = -1;
            ddlStreetType.SelectedValue = null;
            ddlStreetType.ClearSelection();
            txtPostalCode.Text = string.Empty;

            var provider = new ShippingProvider_HU();
            if (ddlStreet.SelectedItem != null)
            {
                List<string> lookupResults = provider.GetStreetType(ProductsBase.CountryCode, city, suburb, district,
                                                                    street);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    lookupResults.Remove("^");
                    lookupResults.Remove(selectText);
                    if (lookupResults.Count > 1)
                        lookupResults.Insert(0, selectText);
                    ddlStreetType.DataSource = lookupResults;
                    ddlStreetType.DataBind();
                    ddlStreetType.SelectedIndex = 0;
                    if (lookupResults.Count == 1)
                    {
                        LookupZipByStreet(city, suburb, district, street, ddlStreetType.SelectedItem != null ? ddlStreetType.SelectedValue : string.Empty);
                    }
                    ddlStreetType.Visible = true;
                    txtStreetType.Visible = false;
                }
                else
                {
                    LookupZipByStreet(city, suburb, district, street, string.Empty);
                    ddlStreetType.Visible = false;
                    txtStreetType.Visible = true;
                }
            }

            if (ddlStreetType.SelectedValue == string.Empty)
            {
                LookupZipByStreet(city, suburb, district, street, string.Empty);
            }
        }

        private void LookupZipByStreet(string city, string suburb, string district, string street, string type)
        {
            txtPostalCode.Text = string.Empty;
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                if (valueContains(suburb) || suburb.Equals(Dashes))
                    suburb = string.Empty;
                List<string> lookupResults = provider.GetZipsForStreet(ProductsBase.CountryCode, city,
                                                                           string.Format("{0}|{1}", suburb, district),
                                                                       string.IsNullOrEmpty(street) &&
                                                                       string.IsNullOrEmpty(type)
                                                                           ? "^"
                                                                           : string.Format("{0}|{1}", street, type));
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    if (lookupResults.Count == 1)
                    {
                        txtPostalCode.Visible = true;
                        ddlPostalCode.Visible = false;
                        txtPostalCode.Text = lookupResults.FirstOrDefault();
                    }
                    else
                    {
                        ddlPostalCode.DataSource = lookupResults;
                        ddlPostalCode.DataBind();
                        ddlPostalCode.SelectedIndex = 0;
                        txtPostalCode.Visible = false;
                        ddlPostalCode.Visible = true;
                    }
                }
            }
        }

        private bool valueContains(string value)
        {
            return selects.Any(s => value.Contains(s));
        }

        private void LookupDistrictsAndSetUp(string city, string suburb)
        {
            LookupDistricts(city, suburb);
            if (ddlDistrict.Items.Count == 0)
            {
                if (!MajorCites.Contains(city))
                {
                    LookupZipByStreet(city,
                                      suburb,
                                      string.Empty, string.Empty,
                                      string.Empty);
                }
                else
                {
                    LookupStreets(city, suburb, string.Empty);
                }
            }
        }
        #endregion

        #region Eventhandlers

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlCity.SelectedItem.Value) && !selects.Contains(ddlCity.SelectedValue))
            {
                ddlSuburb.Items.Clear();
                ddlDistrict.Items.Clear();
                ddlStreet.Items.Clear();
                ddlStreetType.Items.Clear();
                txtSuburb.Text = string.Empty;
                txtDistrict.Text = string.Empty;
                txtStreet.Text = string.Empty;
                txtStreetType.Text = string.Empty;
                txtPostalCode.Text = string.Empty;

                LookupSuburbs(ddlCity.SelectedItem.Value);

                if (!MajorCites.Contains(ddlCity.SelectedItem.Value))
                {
                    if (ddlSuburb.Items.Count == 0)
                    {
                        LookupZipByStreet(ddlCity.SelectedItem.Value, string.Empty, string.Empty, string.Empty,
                                                                  string.Empty);
                    }
                    if (ddlDistrict.Items.Count == 0 && ddlSuburb.Items.Count == 0 &&
                        !string.IsNullOrEmpty(txtPostalCode.Text))
                    {
                        txtStreet.Visible = txtStreetType.Visible = true;
                        ddlStreet.Visible = ddlStreetType.Visible = false;
                    }
                    if (ddlStreetType.Items.Count != 0)
                    {
                        txtStreetType.Visible = false;
                        ddlStreetType.Visible = true;
                    }

                }
            }
            else
            {
                txtPostalCode.Text = string.Empty;
                ddlStreetType.Items.Clear();
                ddlStreetType.Text = string.Empty;
                ddlStreet.Items.Clear();
                ddlStreet.Text = string.Empty;
                ddlDistrict.Items.Clear();
                ddlDistrict.Text = string.Empty;
                ddlSuburb.Items.Clear();
                ddlSuburb.Text = string.Empty;
            }
            ddlCity.Focus();
        }

        protected void ddlSuburb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(ddlSuburb.SelectedItem == null && ddlCity.SelectedItem == null))
            {
                txtPostalCode.Visible = true;
                txtPostalCode.Text = string.Empty;
                ddlPostalCode.Visible = false;
                ddlPostalCode.Items.Clear();

                LookupDistrictsAndSetUp(ddlCity.SelectedValue, ddlSuburb.SelectedItem == null ? string.Empty : ddlSuburb.SelectedValue);
            }
            else
            {
                txtPostalCode.Text = string.Empty;
                ddlStreetType.Items.Clear();
                ddlStreetType.Text = string.Empty;
                ddlStreet.Items.Clear();
                ddlStreet.Text = string.Empty;
                ddlDistrict.Items.Clear();
                ddlDistrict.Text = string.Empty;
            }
        }

        protected void ddlDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlDistrict.SelectedItem != null)
            {
                LookupStreets(ddlCity.SelectedValue, ddlSuburb.SelectedValue, ddlDistrict.SelectedItem.Value);
            }
            else
            {
                txtPostalCode.Text = string.Empty;
                ddlStreetType.Items.Clear();
                ddlStreetType.Text = string.Empty;
                ddlStreet.Items.Clear();
                ddlStreet.Text = string.Empty;
            }
        }

        protected void ddlStreet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlStreet.SelectedItem != null && !selects.Contains(ddlStreet.SelectedItem.Text))
            {
                LookupStreetTypes(ddlCity.SelectedItem.Value, ddlSuburb.SelectedItem != null ? ddlSuburb.SelectedItem.Value : string.Empty,
                    ddlDistrict.SelectedItem != null ? ddlDistrict.SelectedItem.Value : string.Empty, ddlStreet.SelectedItem.Value);
                if (ddlStreetType.Items.Count != 0)
                {
                    ddlStreetType.Visible = true;
                    txtStreetType.Visible = false;
                }
                else
                {
                    ddlStreetType.Visible = false;
                    txtStreetType.Visible = true;
                }
            }
            else
            {
                txtPostalCode.Text = string.Empty;
                ddlStreetType.Items.Clear();
                ddlStreetType.Text = string.Empty;
            }
            ddlStreet.Focus();
        }

        protected void ddlStreetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlStreetType.SelectedItem != null || !selects.Contains(ddlStreetType.Text))
            {
                LookupZipByStreet(ddlCity.SelectedItem.Value,
                    ddlSuburb.SelectedItem == null ? string.Empty : ddlSuburb.SelectedItem.Value,
                    ddlDistrict.SelectedItem == null ? string.Empty : ddlDistrict.SelectedItem.Value,
                    ddlStreet.SelectedItem == null ? string.Empty : ddlStreet.SelectedItem.Value,
                    ddlStreetType.SelectedItem == null ? string.Empty : ddlStreetType.SelectedItem.Text);
                txtHouseNumber.Focus();
            }
            else
            {
                txtPostalCode.Text = string.Empty;
            }
        }

        #endregion
    }
}