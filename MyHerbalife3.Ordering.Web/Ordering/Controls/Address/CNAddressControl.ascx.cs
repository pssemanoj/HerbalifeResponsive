using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class CNAddressControl : AddressControlBase
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
            set { txtStreetAddress2.Text = value; }
            get { return txtStreetAddress2.Text.Trim(); }
        }
        protected override string StreetAddress3
        {
            set { txtStreetAddress3.Text = value; }
            get { return txtStreetAddress3.Text.Trim(); }
        }

        protected override string StreetAddress4
        {
            set { txtStreetAddress4.Text = value; }
            get { return txtStreetAddress4.Text.Trim(); }
        }

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCounties(StateProvince, item.Value);
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
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
                if (null != item) item.Selected = true;
            }
        }

        protected override string StateProvince
        {
            set
            {
                ListItem item = dnlProvince.Items.FindByText(value);
                dnlProvince.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCities(item.Value);
                }
            }
            get
            {
                return dnlProvince.SelectedItem == null || dnlProvince.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlProvince.SelectedItem.Text;
            }
        }

        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text; }
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
                                Field = _shippingAddr.Address.CountyDistrict,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCounty")
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
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode")
                            },
                    };
            }
        }

        #endregion

        #region Methods

        private DistributorOrderingProfile _distributorOrderingProfile = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
            if (member != null)
            {
                var user = member.Value;
                _distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(user.Id, "CN");
            }
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlProvince.Items.Clear();
                dnlCity.Items.Clear();
                dnlDistrict.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress4 = _shippingAddr.Address.Line4;
                StreetAddress3 = _shippingAddr.Address.Line3;
                StreetAddress2 = _distributorOrderingProfile == null ? string.Empty : _distributorOrderingProfile.CNCustomorProfileID.ToString();
                if (LookupStates())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                City = _shippingAddr.Address.City;
                County = _shippingAddr.Address.CountyDistrict;
                ZipCode = _shippingAddr.Address.PostalCode;
                //txtStreetAddress3.Value = _shippingAddr.Address.Line3;
                //txtStreetAddress2.Value = _shippingAddr.Address.Line2;

            }
            lblUnsupportedAddress.Text = "";
        }

        public override object CreateAddressFromControl(string typeName)
        {
            _shippingAddr = (ShippingAddress_V02)base.CreateAddressFromControl(typeName);
        //    _shippingAddr.Address.Line3 = txtStreetAddress3.Value;
       //     _shippingAddr.Address.Line2 = txtStreetAddress2.Value;
            return _shippingAddr;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^(\d{6})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
            if (!Regex.IsMatch(_shippingAddr.Address.Line1, @"^[0-9\u4e00-\u9fa5 ]*$"))
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ChineseCharacterOnly"));
        }

        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlProvince.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        string[] item = province.Split('-');
                        dnlProvince.Items.Add(new ListItem(item[1], item[0]));
                    }
                    dnlProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlProvince.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupCities(string state)
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            dnlDistrict.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        string[] item = city.Split('-');
                        dnlCity.Items.Add(new ListItem(item[1], item[0]));
                    }
                    dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlProvince.Focus();
                }
            }
            return lookedUp;
        }

        private bool LookupCounties(string state, string city)
        {
            bool lookedUp = false;
            dnlDistrict.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStreetsForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var district in lookupResults)
                    {
                        string[] item = district.Split('-');
                        dnlDistrict.Items.Add(new ListItem(item[1], item[0]));
                    }
                    dnlDistrict.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlDistrict.SelectedIndex = 0;
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

        #region Eventhandler

        protected void dnlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlProvince.SelectedItem.Value))
            {
                LookupCities(dnlProvince.SelectedItem.Value);
            }
            else
            {
                dnlCity.Items.Clear();
                dnlDistrict.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlProvince.SelectedItem.Value) && !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupCounties(dnlProvince.SelectedItem.Value, dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlDistrict.Items.Clear();
            }
        }

        protected void dnlDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlProvince.SelectedItem.Value) &&
                !string.IsNullOrEmpty(dnlCity.SelectedItem.Value) && !string.IsNullOrEmpty(dnlDistrict.SelectedItem.Value))
            {
                ShippingProvider_CN shippingProvider = ShippingProvider.GetShippingProvider("CN") as ShippingProvider_CN;
                if (shippingProvider != null)
                {
                    string warning = shippingProvider.GetUnsupportedAddress(dnlProvince.SelectedItem.Text.Trim(), dnlCity.SelectedItem.Text.Trim(),
                                                           dnlDistrict.SelectedItem.Text.Trim());
                    lblUnsupportedAddress.Visible = !string.IsNullOrEmpty(warning);
                    lblUnsupportedAddress.Text = warning;
                }
            }
        }

        #endregion
    }
}