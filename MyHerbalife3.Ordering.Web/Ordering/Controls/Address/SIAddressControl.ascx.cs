using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class SIAddressControl : AddressControlBase
    {
        #region Properties

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text; }
        }

        protected override string City
        {
            set
            {
                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
            }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string StateProvince
        {
            get
            {
                return ProductsBase.CountryCode;
            }
        }

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
                                Field = _shippingAddr.Address.PostalCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
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
                    };
            }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return txtStreet.Text.Trim(); }
        }

        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text; }
        }

        #endregion

        #region Public Methods and Operators

        public override void LoadPage()
        {
            LookupCities();
            if (_shippingAddr != null)
            {
                //dnlCity.Items.Clear();
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                City = _shippingAddr.Address.City;
                ZipCode = _shippingAddr.Address.PostalCode;
                AreaCode = _shippingAddr.AreaCode;

                PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                string[] numbers = _shippingAddr.Phone.Split('-');

                if (numbers[0].Length == 0)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode"));
                }
                else if (!Regex.IsMatch(numbers[0], @"^(\d{2,3})$"))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));
                }

                if (!Regex.IsMatch(numbers[1], @"^(\d{6,8})$"))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                }

                if (numbers[0].Length + numbers[1].Length > 11)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
                }
            }
        }

        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlCity.SelectedIndex != 0)
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
                if (provider != null)
                {
                    var lookupResults = provider.GetZipsForCity(
                        ProductsBase.CountryCode, ProductsBase.CountryCode, dnlCity.SelectedItem.Text);
                    if (lookupResults != null && lookupResults.Count > 0)
                    {
                        var zip = lookupResults.FirstOrDefault();
                        txtPostCode.Text = zip;
                    }
                }
            }
            else if (dnlCity.SelectedIndex == 0)
            {
                txtPostCode.Text = string.Empty;
            }
        }

        private bool LookupCities()
        {
            bool lookedUp = false;
            dnlCity.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(
                    ProductsBase.CountryCode, ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        dnlCity.Items.Add(new ListItem(city));
                    }
                    dnlCity.Items.Insert(
                        0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCity.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        #endregion
    }
}