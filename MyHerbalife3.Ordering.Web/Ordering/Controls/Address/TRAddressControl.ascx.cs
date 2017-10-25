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
    public partial class TRAddressControl : AddressControlBase
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

        //protected override string City
        //{
        //    set
        //    {
        //        ListItem item = dnlCity.Items.FindByText(value);
        //        dnlCity.SelectedIndex = -1;
        //        if (item != null)
        //        {
        //            item.Selected = true;
        //            LookupCities(value);
        //        }
        //    }
        //    get
        //    {
        //        return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
        //                   ? string.Empty
        //                   : dnlCity.SelectedItem.Text;
        //    }
        //}

        protected override string City
        {
            get
            {
                return dnlSuburb.SelectedItem == null || dnlSuburb.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlSuburb.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlSuburb.Items.FindByText(value);
                dnlSuburb.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        protected override string ZipCode
        {
            get
            {
                return dnlPostalCode.SelectedItem == null || dnlPostalCode.SelectedValue == string.Empty
                    ? string.Empty
                    : dnlPostalCode.SelectedItem.Text;
            }
            set 
            {
                ListItem item = dnlPostalCode.Items.FindByText(value);
                dnlPostalCode.SelectedIndex = -1;
                if (null != item) item.Selected = true;
            }
        }

        protected override string County
        {
            get
            {
                return dnlDistrict.SelectedItem == null || dnlDistrict.SelectedValue == string.Empty
                    ? string.Empty
                    : dnlDistrict.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlDistrict.Items.FindByText(value);
                dnlDistrict.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        protected override string StateProvince
        {
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                    ? string.Empty
                    : dnlCity.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                }
            }
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
                                Field = StreetAddress,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.CountyDistrict,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCounty")
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

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlCity.Items.Clear();
                dnlSuburb.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = GetLineOneAddress(_shippingAddr.Address.Line1, _shippingAddr.Address.CountyDistrict);
                StreetAddress2 = _shippingAddr.Address.Line2;
                if (LookupStates())
                {
                    //Needs validate old address to not make a exception when log new requirement to add district
                    if (string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory))
                    {
                        StateProvince = _shippingAddr.Address.City;
                        LookupCities(StateProvince);
                        City = _shippingAddr.Address.CountyDistrict;
                        LookupDistricts(StateProvince, City);
                        County = _shippingAddr.Address.StateProvinceTerritory;
                    }
                    else
                    {
                        StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                        LookupCities(StateProvince);
                        City = _shippingAddr.Address.City;
                        LookupDistricts(StateProvince, City);
                        County = _shippingAddr.Address.CountyDistrict;
                    }
                }

                LookupPostalCodes(StateProvince, City, County);
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{5})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone, @"^(\d{8,12})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        public override object CreateAddressFromControl(string typeName)
        {
            _shippingAddr = (ShippingAddress_V02)base.CreateAddressFromControl(typeName);

            _shippingAddr.Address.Line1 = string.Format("{0}, {1}", County, StreetAddress);

            return _shippingAddr;
        }

        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlSuburb.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    //TR compatibility with old and new version of data.
                    lookupResults.Remove("-");
                    foreach (var city in lookupResults)
                    {
                        dnlCity.Items.Add(new ListItem(city));
                    }
                    dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlSuburb.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var street in lookupResults)
                    {
                        dnlSuburb.Items.Add(new ListItem(street));
                    }
                    dnlSuburb.Items.Insert(0,
                                           new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlSuburb.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlCity.Focus();
                }
            }
            return lookedUp;
        }

        private bool LookupDistricts(string state,string city)
        {
            bool lookedUp = false;

            dnlDistrict.Items.Clear();
            var provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);

            if (provider != null)
            {
                List<string> lookupResults = provider.GetStreetsForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var street in lookupResults)
                    {
                        dnlDistrict.Items.Add(new ListItem(street));
                    }
                    dnlDistrict.Items.Insert(0,
                                           new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlDistrict.SelectedIndex = 0;
                    lookedUp = true;
                }
            }

            return lookedUp;
        }

        private bool LookupPostalCodes(string state,string city,string district)
        {
            bool lookedUp = false;

            dnlPostalCode.Items.Clear();
            var provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);

            if (provider != null)
            {
                List<string> lookupResults = provider.GetZipsForStreet(ProductsBase.CountryCode, state, city, district);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var street in lookupResults)
                    {
                        dnlPostalCode.Items.Add(new ListItem(street));
                    }
                    dnlPostalCode.Items.Insert(0,
                                           new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlPostalCode.SelectedIndex = 0;
                    lookedUp = true;
                }
            }

            return lookedUp;
        }

        private string GetLineOneAddress(string lineOneValue, string countyDistrict)
        {
            if (string.IsNullOrEmpty(lineOneValue))
            {
                return string.Empty;
            }

            string[] lineOne = lineOneValue.Split(',');
            var delimiter = string.Empty;
            var splittedAddress = string.Empty;
            if (lineOne.Count() == 1 || string.IsNullOrEmpty(countyDistrict))
            {
                return lineOneValue;
            }

            foreach (var lineValue in lineOne.Where(lineValue => lineValue != countyDistrict && !string.IsNullOrEmpty(lineValue)))
            {
                splittedAddress = string.Format("{0}{1}{2}", splittedAddress.TrimStart(), delimiter, lineValue);
                delimiter = ",";
            }

            return splittedAddress.TrimStart();
        }
        #endregion

        #region Eventhandler

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupCities(dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlSuburb.Items.Clear();
            }
        }

        protected void dnlSuburb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlSuburb.SelectedItem.Value) && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupDistricts(dnlCity.SelectedItem.Value, dnlSuburb.SelectedItem.Value);
            }
            else
            {
                dnlDistrict.Items.Clear();
            }
        }

        protected void dnlDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlDistrict.SelectedItem.Value))
            {
                LookupPostalCodes(dnlCity.SelectedItem.Value, dnlSuburb.SelectedItem.Value, dnlDistrict.SelectedItem.Value);
            }
            else
            {
                dnlPostalCode.Items.Clear();
            }
        }
        
        #endregion
    }
}