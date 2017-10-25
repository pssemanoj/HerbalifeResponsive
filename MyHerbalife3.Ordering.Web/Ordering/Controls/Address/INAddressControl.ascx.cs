using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class INAddressControl : AddressControlBase
    {
        private short _tabIndex;

        protected override short TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                txtFirstName.TabIndex = (value++);
                txtLastName.TabIndex = (value++);
                txtStreet1.TabIndex = (value++);
                txtStreet2.TabIndex = (value++);
                dnlState.TabIndex = (value++);
                dnlCity.TabIndex = (value++);
                txtPostCode.TabIndex = (value++);
                txtAreaCode.TabIndex = (value++);
                txtNumber.TabIndex = (value++);
                txtExtension.TabIndex = (value);
            }
        }

        protected override string FirstName
        {
            set { txtFirstName.Text = value; }
            get { return (txtFirstName.Text.Trim()); }
        }

        protected override string LastName
        {
            set { txtLastName.Text = value; }
            get { return (txtLastName.Text.Trim()); }
        }

        protected override string StreetAddress
        {
            set { txtStreet1.Text = value; }
            get { return (txtStreet1.Text.Trim()); }
        }

        protected override string StreetAddress2
        {
            set { txtStreet2.Text = value; }
            get { return (txtStreet2.Text.Trim()); }
        }

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
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
            set
            {
                ListItem itemState = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (null != itemState) itemState.Selected = true;
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
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

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
        }

        protected override string Extension
        {
            set { txtExtension.Text = value; }
            get { return txtExtension.Text.Trim(); }
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                FirstName = _shippingAddr.FirstName;
                LastName = _shippingAddr.LastName;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                if (!string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory))
                    LookupCities(_shippingAddr.Address.StateProvinceTerritory.Length.Equals(2)
                                     ? _shippingAddr.Address.StateProvinceTerritory
                                     : GetStateCode(_shippingAddr.Address.StateProvinceTerritory));

                City = _shippingAddr.Address.City;

                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                AreaCode = _shippingAddr.AreaCode;
                Extension = _shippingAddr.AltAreaCode;
            }
            else
            {
                LookupStates();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
        }

        public override object CreateAddressFromControl(string typeName)
        {
            _shippingAddr = (ShippingAddress_V02) base.CreateAddressFromControl(typeName);

            _shippingAddr.FirstName = FirstName;
            _shippingAddr.LastName = LastName;

            _shippingAddr.Address.City = City.Contains("Major City:") ? City.Remove(0, 11) : City;
            _shippingAddr.Recipient = FirstName + " " + LastName;

            return _shippingAddr;
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
                                Field = _shippingAddr.FirstName,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoFirstName")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.LastName,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoLastName")
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

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{6})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                string[] numbers = _shippingAddr.Phone.Split('-');

                if (!Regex.IsMatch(numbers[0],
                                   @"^(\d{1,5})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));

                if (!Regex.IsMatch(numbers[1],
                                   @"^(\d{8,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));

                if (numbers.Length.Equals(3))
                {
                    if (!string.IsNullOrEmpty(numbers[2]))
                    {
                        if (!Regex.IsMatch(numbers[2],
                                           @"^(\d{0,5})$"))
                        {
                            if (
                                _errors.Contains(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                           "InvalidExtension")))
                            {
                                _errors.Remove(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                         "InvalidExtension"));
                                _errors.Add(GetLocalResourceObject("InvalidExtension") as string);
                            }
                        }
                    }
                }

                if (_errors.Contains(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidExtension")))
                {
                    _errors.Remove(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidExtension"));
                    _errors.Add(GetLocalResourceObject("InvalidExtension") as string);
                }
            }
        }

        private bool StreetAddressRegularExpressionCheck(String streetAddress)
        {
            string alphaPattern = @"[a-zA-Z]+";
            string numberPattern = @"[\d]+";
            string spacePattern = @"[\s&,-#]+";

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

        private string GetStateCode(string stateName)
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("IndiaStates");
            var state = entries.FirstOrDefault(e => e.Value == stateName);
            return state.Key ?? string.Empty;
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            LookupCities(dnlState.SelectedItem.Value);
            dnlCity.Focus();
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPostCode.Focus();
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

        private void LookupStates()
        {
            dnlState.Items.Clear();

            var sorted = new SortedList<string, ListItem>();

            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("IndiaStates");

            foreach (var entry in entries)
            {
                sorted.Add(entry.Key, new ListItem(entry.Value));
            }

            dnlState.DataSource = sorted;
            dnlState.DataTextField = "value";
            dnlState.DataValueField = "key";
            dnlState.DataBind();

            dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlState.SelectedIndex = 0;
        }

        private bool LookupCities(String stateCode)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();

            string countryCode = ProductsBase.CountryCode;
            IShippingProvider provider = ShippingProvider.GetShippingProvider(countryCode);
            if (provider != null)
            {
                if (!stateCode.Equals(string.Empty))
                {
                    List<string> lookupResults = provider.GetCitiesForState(countryCode, stateCode);
                    if (null != lookupResults && lookupResults.Count > 0)
                    {
                        for (int i = 0; i < lookupResults.Count; i++)
                        {
                            dnlCity.Items.Insert(i, new ListItem(lookupResults[i]));
                        }
                        dnlCity.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlCity.SelectedIndex = 0;
                        lookedUp = true;
                    }
                }
            }

            return lookedUp;
        }
    }
}