using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Telerik.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    /// <summary>
    ///     Defines the address control popup for RO
    /// </summary>
    public partial class ROAddressControl : AddressControlBase
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
                dnlCity.TabIndex = (value++);
                dnlLocality.TabIndex = (value++);
                dnlStreetType.TabIndex = (value++);
                dnlStreet.TabIndex = (value++);
                txtNumber.TabIndex = (value++);
                txtBlock.TabIndex = (value++);
                txtScara.TabIndex = (value++);
                txtEtaj.TabIndex = (value++);
                txtApartment.TabIndex = (value);
                dnlPostCode.TabIndex = (value);
                txtPhoneNum.TabIndex = (value);
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
            get
            {
                if ((dnlStreet.SelectedItem == null && dnlStreet.SelectedValue.Equals("0") &&
                     string.IsNullOrEmpty(dnlStreet.Text)) ||
                    dnlStreetType.SelectedItem == null || dnlStreetType.SelectedValue.Equals("0") ||
                    string.IsNullOrEmpty(txtNumber.Text))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0} {1} {2}", dnlStreetType.SelectedItem.Text,
                                         (dnlStreet.SelectedItem == null) ? dnlStreet.Text : dnlStreet.SelectedItem.Text,
                                         txtNumber.Text.Trim());
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    dnlStreetType.SelectedIndex = -1;
                    dnlStreet.SelectedIndex = -1;
                    txtNumber.Text = string.Empty;
                }
                else
                {
                    var streetType = string.Empty;
                    var street = string.Empty;
                    var number = string.Empty;

                    ListItem itemStreetType = null;
                    foreach (ListItem type in dnlStreetType.Items)
                    {
                        if (value.StartsWith(type.Text))
                        {
                            itemStreetType = type;
                            streetType = type.Text;
                            break;
                        }
                    }
                    dnlStreetType.SelectedIndex = -1;
                    if (itemStreetType != null)
                    {
                        itemStreetType.Selected = true;
                        street = value.Replace(streetType, string.Empty).Trim();
                        LookupStreetByType(dnlCity.SelectedItem.Value, dnlLocality.SelectedItem.Value, streetType);
                    }

                    RadComboBoxItem itemStreet = null;
                    foreach (RadComboBoxItem type in dnlStreet.Items)
                    {
                        if (street.StartsWith(type.Text))
                        {
                            itemStreet = type;
                            number = street.Replace(type.Text, string.Empty).Trim();
                            street = type.Text;
                            break;
                        }
                    }
                    dnlStreet.SelectedIndex = -1;
                    if (itemStreet != null)
                    {
                        itemStreet.Selected = true;
                        LookupZipByStreet(dnlCity.SelectedItem.Value, dnlLocality.SelectedItem.Value, streetType, street);
                    }
                    else
                    {
                        street = street.Replace(StreetAddress4, string.Empty).Trim();
                        number = StreetAddress4;
                        dnlStreet.Text = street;
                    }

                    txtNumber.Text = number;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the street line 2
        /// </summary>
        protected override string StreetAddress2
        {
            get
            {
                var street2 = string.Empty;
                if (!string.IsNullOrEmpty(txtBlock.Text))
                {
                    street2 = string.Format("BL. {0}", txtBlock.Text.Trim()).Trim();
                }
                if (!string.IsNullOrEmpty(txtScara.Text))
                {
                    street2 = string.Format("{0} SC. {1}", street2, txtScara.Text.Trim()).Trim();
                }
                if (!string.IsNullOrEmpty(txtEtaj.Text))
                {
                    street2 = string.Format("{0} ET. {1}", street2, txtEtaj.Text.Trim()).Trim();
                }
                if (!string.IsNullOrEmpty(txtApartment.Text))
                {
                    street2 = string.Format("{0} AP. {1}", street2, txtApartment.Text.Trim()).Trim();
                }
                return street2;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    txtBlock.Text = string.Empty;
                    txtScara.Text = string.Empty;
                    txtEtaj.Text = string.Empty;
                    txtApartment.Text = string.Empty;
                }
                else
                {
                    string blAdd, scAdd, etAdd, apAdd;
                    blAdd = scAdd = etAdd = apAdd = string.Empty;
                    if (value.IndexOf("BL. ") >= 0)
                    {
                        blAdd = value.Substring(value.IndexOf("BL. "));
                    }
                    if (value.IndexOf("SC. ") >= 0)
                    {
                        scAdd = value.Substring(value.IndexOf("SC. "));
                    }
                    if (value.IndexOf("ET. ") >= 0)
                    {
                        etAdd = value.Substring(value.IndexOf("ET. "));
                    }
                    if (value.IndexOf("AP. ") >= 0)
                    {
                        apAdd = value.Substring(value.IndexOf("AP. "));
                    }
                    if (!string.IsNullOrEmpty(apAdd))
                    {
                        etAdd = etAdd.Replace(apAdd, string.Empty).Trim();
                        scAdd = scAdd.Replace(apAdd, string.Empty).Trim();
                        blAdd = blAdd.Replace(apAdd, string.Empty).Trim();
                    }
                    if (!string.IsNullOrEmpty(etAdd))
                    {
                        scAdd = scAdd.Replace(etAdd, string.Empty).Trim();
                        blAdd = blAdd.Replace(etAdd, string.Empty).Trim();
                    }
                    if (!string.IsNullOrEmpty(scAdd))
                    {
                        blAdd = blAdd.Replace(scAdd, string.Empty).Trim();
                    }
                    blAdd = blAdd.Replace("BL. ", string.Empty);
                    scAdd = scAdd.Replace("SC. ", string.Empty);
                    etAdd = etAdd.Replace("ET. ", string.Empty);
                    apAdd = apAdd.Replace("AP. ", string.Empty);
                    txtBlock.Text = blAdd;
                    txtScara.Text = scAdd;
                    txtEtaj.Text = etAdd;
                    txtApartment.Text = apAdd;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the city
        /// </summary>
        protected override string City
        {
            get
            {
                if (dnlCity.SelectedItem == null || dnlCity.SelectedValue.Equals("0") ||
                    dnlLocality.SelectedItem == null || dnlLocality.SelectedValue.Equals("0"))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0}, {1}", dnlCity.SelectedItem.Text, dnlLocality.SelectedItem.Text);
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    dnlCity.SelectedIndex = -1;
                    dnlLocality.SelectedIndex = -1;
                }
                else
                {
                    var city = value.Substring(0, value.IndexOf(", "));
                    var locality = value.Substring(value.IndexOf(", ") + 2).Trim();

                    ListItem itemCity = dnlCity.Items.FindByValue(city);
                    dnlCity.SelectedIndex = -1;
                    if (itemCity != null)
                    {
                        itemCity.Selected = true;
                        LookupLocalities(city);
                    }

                    ListItem itemLocality = dnlLocality.Items.FindByValue(locality);
                    dnlLocality.SelectedIndex = -1;
                    if (itemLocality != null)
                    {
                        itemLocality.Selected = true;
                        LoadStreetTypeFromFile();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the zip code
        /// </summary>
        protected override string ZipCode
        {
            get
            {
                if (dnlPostCode.SelectedItem == null || dnlCity.SelectedValue.Equals("0"))
                {
                    return string.Empty;
                }
                else
                {
                    return dnlPostCode.SelectedItem.Text;
                }
            }
            set
            {
                if (!String.IsNullOrEmpty(City) && City.Contains(","))
                {
                    var city = City.Substring(0, City.IndexOf(", "));
                    var locality = City.Substring(City.IndexOf(", ") + 2).Trim();
                    LookupZipByLocality(city, locality);
                }
                

                ListItem item = dnlPostCode.Items.FindByValue(value);
                dnlPostCode.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the phone number
        /// </summary>
        protected override string PhoneNumber
        {
            get { return txtPhoneNum.Text; }
            set { txtPhoneNum.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the street number
        /// </summary>
        protected override string StreetAddress4
        {
            get { return txtNumber.Text; }
            set { txtNumber.Text = value; }
        }

        /// <summary>
        ///     Gets or sets the required field array to validate
        /// </summary>
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
                                Field = dnlCity.SelectedItem != null ? dnlCity.SelectedItem.Value : string.Empty,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
                            },
                        new RequireFieldDef
                            {
                                Field = dnlLocality.SelectedItem != null ? dnlLocality.SelectedItem.Value : string.Empty,
                                ErrorMsg = GetLocalResourceObject("NoLocality") as string
                            },
                        new RequireFieldDef
                            {
                                Field =
                                    dnlStreetType.SelectedItem != null ? dnlStreetType.SelectedItem.Value : string.Empty,
                                ErrorMsg = GetLocalResourceObject("NoStreetType") as string
                            },
                        new RequireFieldDef
                            {
                                Field = dnlStreet.SelectedItem != null
                                            ? (dnlStreet.SelectedItem.Value.Equals(
                                                base.GetLocalResourceObject("Select") as string)
                                                   ? string.Empty
                                                   : dnlStreet.SelectedItem.Value)
                                            : dnlStreet.Text.Trim(),
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = txtNumber.Text,
                                ErrorMsg = GetLocalResourceObject("NoNumber") as string
                            },
                        new RequireFieldDef
                            {
                                Field = dnlPostCode.SelectedItem != null ? dnlPostCode.SelectedItem.Value : string.Empty,
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
        }

        /// <summary>
        ///     Validates the control
        /// </summary>
        /// <param name="_errors">Error list.</param>
        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^[0-9]{9,10}$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        /// <summary>
        ///     Loads the page
        /// </summary>
        public override void LoadPage()
        {
            dnlStreet.SelectedIndexChanged += dnlStreet_SelectedIndexChanged;
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                if (LookupCities())
                {
                    City = _shippingAddr.Address.City;
                }
                StreetAddress4 = _shippingAddr.Address.Line4;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
            txtCareOfName.Focus();
        }

        /// <summary>
        ///     Gets the cities from service.
        /// </summary>
        /// <returns>Success flag.</returns>
        private bool LookupCities()
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlLocality.Items.Clear();
            dnlStreetType.Items.Clear();
            dnlStreet.Items.Clear();
            dnlStreet.Text = string.Empty;
            dnlPostCode.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                // We use the state field from service to store the city
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        dnlCity.Items.Add(new ListItem(province));
                    }
                    dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        /// <scess flag.ummary>
        ///     Gets the localities for a city from service.
        ///     </summary>
        ///     <param name="city">City name.</param>
        ///     <returns>Succes flag.</returns>
        private bool LookupLocalities(string city)
        {
            bool lookedUp = false;
            dnlLocality.Items.Clear();
            dnlStreetType.Items.Clear();
            dnlStreet.Items.Clear();
            dnlStreet.Text = string.Empty;
            dnlPostCode.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                // We use the city field from service to store the locality info
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var locality in lookupResults)
                    {
                        dnlLocality.Items.Add(new ListItem(locality));
                    }
                    dnlLocality.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlLocality.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Gets the street types for a locality.
        /// </summary>
        /// <param name="city">City name.</param>
        /// <param name="locality">Locality name.</param>
        /// <returns>Sucess flag.</returns>
        private bool LookupStreetTypes(string city, string locality)
        {
            bool lookedUp = false;
            dnlStreetType.Items.Clear();
            dnlStreet.Items.Clear();
            dnlStreet.Text = string.Empty;
            dnlPostCode.Items.Clear();
            var provider = new ShippingProvider_RO();
            if (provider != null)
            {
                var lookupResults = provider.GetStreetTypeForCity(ProductsBase.CountryCode, city, locality);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var type in lookupResults)
                    {
                        dnlStreetType.Items.Add(new ListItem(type));
                    }
                    dnlStreetType.Items.Insert(0,
                                               new ListItem(base.GetLocalResourceObject("Select") as string,
                                                            string.Empty));
                    dnlStreetType.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlLocality.Focus();
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Gets the streets for a locality by type.
        /// </summary>
        /// <param name="city">City name.</param>
        /// <param name="locality">Locality name.</param>
        /// <param name="type">Street type.</param>
        /// <returns>Success flag.</returns>
        private bool LookupStreetByType(string city, string locality, string type)
        {
            bool lookedUp = false;
            dnlStreet.Items.Clear();
            dnlStreet.Text = string.Empty;
            dnlStreet.ClearSelection();
            dnlPostCode.Items.Clear();
            var provider = new ShippingProvider_RO();
            if (provider != null)
            {
                var lookupResults = provider.GetStreetForCityByType(ProductsBase.CountryCode, city, locality, type);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    lookupResults.Insert(0, base.GetLocalResourceObject("Select") as string);
                    dnlStreet.DataSource = lookupResults;
                    dnlStreet.DataBind();
                    dnlStreet.AllowCustomText = false;
                    lookedUp = true;
                }
                else
                {
                    // Let the DS key the street name
                    dnlStreet.AllowCustomText = true;
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Gets the zip codes for a street.
        /// </summary>
        /// <param name="city">City name.</param>
        /// <param name="locality">Locality name.</param>
        /// <param name="type">Street type.</param>
        /// <param name="street">Street name.</param>
        /// <returns>Sucess flag.</returns>
        private bool LookupZipByStreet(string city, string locality, string type, string street)
        {
            bool lookedUp = false;
            dnlPostCode.Items.Clear();
            var provider = new ShippingProvider_RO();
            if (provider != null)
            {
                var streetToSearch = string.Format("{0}|{1}", type, street);
                var lookupResults = provider.GetZipsForStreet(ProductsBase.CountryCode, city, locality, streetToSearch);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var zip in lookupResults)
                    {
                        dnlPostCode.Items.Add(new ListItem(zip));
                    }
                    dnlPostCode.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    if (lookupResults.Count == 1)
                    {
                        dnlPostCode.SelectedIndex = 1;
                    }
                    else
                    {
                        dnlPostCode.SelectedIndex = 0;
                    }
                    lookedUp = true;
                }
                else
                {
                    dnlStreet.Focus();
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Gets the zip code for city and locality.
        /// </summary>
        /// <param name="city">City name</param>
        /// <param name="locality">Locality name</param>
        /// <returns></returns>
        private bool LookupZipByLocality(string city, string locality)
        {
            bool lookedUp = false;
            dnlPostCode.Items.Clear();
            var provider = new ShippingProvider_RO();
            if (provider != null)
            {
                var lookupResults = provider.GetZipsForCity(ProductsBase.CountryCode, city, locality);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var zip in lookupResults)
                    {
                        dnlPostCode.Items.Add(new ListItem(zip));
                    }
                    dnlPostCode.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    if (lookupResults.Count == 1)
                    {
                        dnlPostCode.SelectedIndex = 1;
                    }
                    else
                    {
                        dnlPostCode.SelectedIndex = 0;
                    }
                    lookedUp = true;
                }
                else
                {
                    dnlStreet.Focus();
                }
            }
            return lookedUp;
        }

        /// <summary>
        ///     Gets the street types from the data file.
        /// </summary>
        private void LoadStreetTypeFromFile()
        {
            var ds = new XmlDataSource();
            ds.DataFile = "~/Ordering/Controls/Address/ROStreetType.xml";
            ds.XPath = "/StreetTypes/StreetType";
            dnlStreetType.DataTextField = "Name";
            dnlStreetType.DataValueField = "Name";
            dnlStreetType.DataSource = ds;
            dnlStreetType.DataBind();
            dnlStreetType.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
            dnlStreetType.SelectedIndex = 0;
        }

        #endregion

        #region Eventhandlers

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupLocalities(dnlCity.SelectedItem.Value);
                dnlLocality.Focus();
            }
            else
            {
                dnlCity.Items.Clear();
                dnlLocality.Items.Clear();
                dnlStreetType.Items.Clear();
                dnlStreet.Items.Clear();
                dnlStreet.Text = string.Empty;
                dnlPostCode.Items.Clear();
            }
        }

        protected void dnlLocality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCity.SelectedItem.Value) &&
                !string.IsNullOrEmpty(dnlLocality.SelectedItem.Value))
            {
                LoadStreetTypeFromFile();
                dnlStreetType.Focus();
            }
            else
            {
                dnlLocality.Items.Clear();
                dnlStreetType.Items.Clear();
                dnlStreet.Items.Clear();
                dnlStreet.Text = string.Empty;
                dnlPostCode.Items.Clear();
            }
        }

        protected void dnlStreetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlLocality.SelectedItem.Value) &&
                !string.IsNullOrEmpty(dnlStreetType.SelectedItem.Value))
            {
                LookupStreetByType(dnlCity.SelectedItem.Value, dnlLocality.SelectedItem.Value,
                                   dnlStreetType.SelectedItem.Value);
                dnlStreet.Focus();
            }
            else
            {
                dnlStreetType.Items.Clear();
                dnlStreet.Items.Clear();
                dnlStreet.Text = string.Empty;
                dnlPostCode.Items.Clear();
            }
        }

        private void dnlStreet_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            if (dnlStreetType.SelectedItem != null && !string.IsNullOrEmpty(dnlStreetType.SelectedItem.Value) &&
                dnlStreet.SelectedItem != null)
            {
                LookupZipByStreet(dnlCity.SelectedItem.Value, dnlLocality.SelectedItem.Value,
                                  dnlStreetType.SelectedItem.Value, dnlStreet.SelectedItem.Value);
                txtNumber.Focus();
            }
            else
            {
                if (!string.IsNullOrEmpty(dnlStreet.Text))
                {
                    // Lookup the zip by city
                    LookupZipByLocality(dnlCity.SelectedItem.Value, dnlLocality.SelectedItem.Value);
                    txtNumber.Focus();
                }
                else
                {
                    dnlStreet.Items.Clear();
                    dnlStreet.Text = string.Empty;
                    dnlPostCode.Items.Clear();
                }
            }
        }

        #endregion
    }
}