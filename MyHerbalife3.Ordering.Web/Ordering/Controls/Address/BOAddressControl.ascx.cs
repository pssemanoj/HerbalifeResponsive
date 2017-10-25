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
    public partial class BOAddressControl : AddressControlBase
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

        protected override string City
        {
            set
            {
                txtCity.Text = value;
            }
            get
            {
                return txtCity.Text;
            }
        }

        protected override string StateProvince
        {
            set
            {
                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCountySuburb(value);
                }
            }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
        }

        protected override string County
        {
            get 
            {
                return dnlCounty.SelectedItem == null || dnlCounty.SelectedValue == string.Empty ? string.Empty : dnlCounty.SelectedItem.Text;
            }
            set
            {
                ListItem item = dnlCounty.Items.FindByText(value);
                dnlCounty.SelectedIndex = -1;
                if (item != null)
                    item.Selected = true;
            }
        }        

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        protected override RequireFieldDef[] RequiredFields
        {
            set { }
            get
            {
                Func<string, string> getErrMsg = (itemName) =>
                    PlatformResources.GetGlobalResourceString("ErrorMessage", itemName);

                return new[] {
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
                        Field = _shippingAddr.Address.CountyDistrict,
                        ErrorMsg = getErrMsg("NoCounty") 
                    },
                    new RequireFieldDef {
                        Field = _shippingAddr.Phone,
                        ErrorMsg = getErrMsg("NoPhone") 
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
                dnlState.Items.Clear();
                txtCity.Text = "";
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                PhoneNumber = _shippingAddr.Phone;
                
                if (LookupProvinces())
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                if (LookupCountySuburb(this.StateProvince))
                    County = _shippingAddr.Address.CountyDistrict;
                                
                City = _shippingAddr.Address.City;
                
                ZipCode = _shippingAddr.Address.PostalCode;
            }
            else
            {
                LookupProvinces();
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            Action<string> addErrMsg = (itemName) =>
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", itemName));

            if (!string.IsNullOrEmpty(PhoneNumber) &&
                 !Regex.IsMatch(PhoneNumber, @"^(\d{6,10})$"))
                addErrMsg("InvalidPhone");

            if (!string.IsNullOrWhiteSpace(ZipCode) &&
                 !Regex.IsMatch(ZipCode, @"\d{5}"))
                addErrMsg("InvalidZipCode");
        }        

        private bool LookupProvinces()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        dnlState.Items.Add(new ListItem(province));
                    }
                    dnlState.Items.Insert(0,
                                             new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }        

        private bool LookupCountySuburb(string state)
        {
            bool lookedUp = false;
            dnlCounty.Items.Clear();
            var provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                provider.GetCountiesForCity(ProductsBase.CountryCode, state, "").ForEach(county => {                    
                    dnlCounty.Items.Add(new ListItem(county));
                });

                    dnlCounty.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlCounty.SelectedIndex = 0;
                    lookedUp = true;
                }

            return lookedUp;
            }

        private bool LookupCity(string state, string county) 
        {            
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);

            if (provider == null)
                return false;

            string city = provider.GetAddressField(new AddressFieldForCountryRequest_V01()
            {
                AddressField = AddressPart.CITY,
                Country = ProductsBase.CountryCode,
                State = state,
                County = county
            }).FirstOrDefault();

            if (city != null)
            {
                txtCity.Text = city;
                return true;
            }
            else
                return false;
        }        
        
        #endregion

        #region Event handlers

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value))
            {
                LookupCountySuburb(dnlState.SelectedItem.Value);                            
            }
            else
            {                
                dnlCounty.Items.Clear();                
            }

            txtCity.Text = "";
        }
        
        protected void dnlCounty_SelectedIndexChanged(object sender, EventArgs e)
        {
            var provinceItem = dnlState.SelectedItem;            
            var countyItem = dnlCounty.SelectedItem;
            
            if (provinceItem != null && countyItem != null && 
                !string.IsNullOrEmpty(provinceItem.Value) && 
                !string.IsNullOrEmpty(countyItem.Value))
            {
                if (LookupCity(provinceItem.Value, countyItem.Value))
                    City = txtCity.Text;
            }
            else
            {
                City = "";
            }
        }
        
        #endregion
    }
}