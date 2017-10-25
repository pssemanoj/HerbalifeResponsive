using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Providers.Interfaces;
using System.Text.RegularExpressions;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class GTAddressControl : AddressControlBase
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

        //Departamento
        protected override string StateProvince
        {
            set
            {
                if ( ! string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupCities(item.Value);                    
                }
            }
            get
            {                
                return dnlState.SelectedItem == null || dnlState.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
        }

        //Municipio
        protected override string City
        {
            set
            {
                if ( ! string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();


                ListItem item = dnlCity.Items.FindByText(value);
                dnlCity.SelectedIndex = -1;
                if (item != null)
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
        
        //Zona
        protected override string StreetAddress4
        {
            get
            {
                return dnlZone.SelectedItem == null || dnlZone.SelectedValue == string.Empty
                    ? string.Empty
                    : dnlZone.SelectedItem.Text;
            }
            set
            {
                if ( ! string.IsNullOrWhiteSpace(value) &&
                    Regex.IsMatch(value, @"[a-z]+") )
                    value = value.ToUpper();

                ListItem item = dnlZone.Items.FindByText(value);
                dnlZone.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
        }

        protected override string PhoneNumber
        {
            set { txtPhoneNumber.Text = value; }
            get { return txtPhoneNumber.Text; }
        }

        protected override RequireFieldDef[] RequiredFields
        {
            set { ; }
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

        #endregion

        #region Event Handler
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlState.Items.Clear();
                dnlCity.Items.Clear();
                dnlZone.Items.Clear();

                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                
                if ( LookupStates() )
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;

                if ( LookupCities(StateProvince) )
                    City = _shippingAddr.Address.City;

                if ( LookupZones(StateProvince, City) )
                    StreetAddress4 = _shippingAddr.Address.Line4;

                PhoneNumber = _shippingAddr.Phone;
            }
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value))
            {
                LookupCities(dnlState.SelectedItem.Value);                
            }
            else
            {
                dnlCity.Items.Clear();                
                dnlZone.Items.Clear();
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnlState.SelectedItem.Value) &&
                !string.IsNullOrEmpty(dnlCity.SelectedItem.Value))
            {
                LookupZones(dnlState.SelectedItem.Value, dnlCity.SelectedItem.Value);
            }
            else
            {
                dnlZone.Items.Clear();
            }
        }        

        #endregion

        #region Private Methods

        private bool LookupStates()
        {
            bool lookedUp = false;

            dnlState.Items.Clear();

            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var state in lookupResults)
                    {
                        dnlState.Items.Add(new ListItem(state, state));
                    }
                    
                    lookedUp = true;
                }
            }

            dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlState.SelectedIndex = 0;

            return lookedUp;
        }

        private bool LookupCities(string state)
        {
            bool lookedUp = false;

            dnlCity.Items.Clear();
            dnlZone.Items.Clear();

            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var county in lookupResults)
                    {
                        dnlCity.Items.Add(new ListItem(county, county));
                    }                    
                    lookedUp = true;
                }
                else
                {
                    dnlState.Focus();
                }
            }

            dnlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlCity.SelectedIndex = 0;

            return lookedUp;
        }        

        private bool LookupZones(string state, string county)
        {
            bool lookedUp = false;

            dnlZone.Items.Clear();
            
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            
            if (provider == null)
                return lookedUp;

            var lookupResults = provider.GetCountiesForCity(ProductsBase.CountryCode, state, county);
            if (lookupResults != null && lookupResults.Count > 0)
            {
                foreach (var zone in lookupResults)
                {
                    if (!String.IsNullOrEmpty(zone))
                        dnlZone.Items.Add(new ListItem(zone, zone));
                }                
                lookedUp = true;
            }

            dnlZone.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
            dnlZone.SelectedIndex = 0;

            return lookedUp;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(Recipient) && (Recipient.Trim().Length < 1 || Recipient.Trim().Length > 40))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            }

            if (!string.IsNullOrEmpty(StreetAddress) && (StreetAddress.Trim().Length < 1 || StreetAddress.Trim().Length > 40))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            }

            if (!string.IsNullOrEmpty(City) && (City.Trim().Length < 1 || City.Trim().Length > 30))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCity"));
            }

            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Phone, @"^(\d{6,8})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        #endregion

    }
}