using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class DOAddressControl : AddressControlBase
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

        //Province
        protected override string StateProvince
        {
            set
            {
                ListItem item = dnlProvince.Items.FindByText(value);
                dnlProvince.SelectedIndex = -1;
                if (item != null)
                {
                    item.Selected = true;
                    LookupStates(item.Value);
                }
            }
            get
            {
                return dnlProvince.SelectedItem == null || dnlProvince.SelectedValue == string.Empty
                           ? string.Empty
                           : dnlProvince.SelectedItem.Text;
            }
        }

        //State
        protected override string City
        {
            set
            {
                ListItem item = dnlState.Items.FindByText(value);
                dnlState.SelectedIndex = -1;
                if (item != null)
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

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                dnlProvince.Items.Clear();
                dnlState.Items.Clear();
                
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                ZipCode = _shippingAddr.Address.PostalCode;
                
                if (LookupProvinces())
                {
                    StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                }
                City = _shippingAddr.Address.City;
                County = _shippingAddr.Address.CountyDistrict;
                StreetAddress3 = _shippingAddr.Address.Line3;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        #region Private Methods

        public override void ValidationCheck(List<string> _errors)
        {
            Action<string> addErrMsg = (itemName) => 
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", itemName));

            if ( ! string.IsNullOrEmpty(Recipient) && 
                (Recipient.Trim().Length < 2 || Recipient.Trim().Length > 40) )
                addErrMsg("InvalidRecipentName");

            if ( ! string.IsNullOrEmpty(StreetAddress) && 
                (StreetAddress.Trim().Length < 1 || StreetAddress.Trim().Length > 40) )
                addErrMsg("InvalidStreetAddress");

            if ( ! string.IsNullOrEmpty(PhoneNumber) &&
                ! Regex.IsMatch(_shippingAddr.Phone, @"^(\d{8,12})$") )
                addErrMsg("InvalidPhone");

            if ( ! string.IsNullOrWhiteSpace(ZipCode) &&
                ! Regex.IsMatch(ZipCode, @"\d{5}") )
                addErrMsg("InvalidZipCode");

        }

        private bool LookupProvinces()
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
                        dnlProvince.Items.Add(new ListItem(province, province));
                    }

                    dnlProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlProvince.SelectedIndex = 0;
                    lookedUp = true;
                }
            }
            return lookedUp;
        }

        private bool LookupStates(string province)
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState(ProductsBase.CountryCode, province);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var state in lookupResults)
                    {
                        dnlState.Items.Add(new ListItem(state, state));
                    }
                    dnlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
                }
                else
                {
                    dnlProvince.Focus();
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
                LookupStates(dnlProvince.SelectedItem.Value);
            }
            else
            {
                dnlState.Items.Clear();                
            }
        }        

        #endregion

        

        
    }
}