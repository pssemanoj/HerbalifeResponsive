using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class BYAddressControl : AddressControlBase
    {
        #region Properties

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text; }
        }

        protected override string StateProvince
        {
            set { txtState.Text = value; }
            get { return txtState.Text; }
        }

        protected override string County
        {
            set
            {
                ListItem item = this.dnlCounty.Items.FindByValue(value);
                this.dnlCounty.SelectedIndex = -1;
                if (null != item)
                {
                    item.Selected = true;
                }
            }
            get
            {
                return this.dnlCounty.SelectedItem == null || this.dnlCounty.SelectedValue == string.Empty
                           ? string.Empty
                           : this.dnlCounty.SelectedItem.Value;
            }
        }
        
        protected override string City
        {
            set { txtCity.Text = value; }
            get { return txtCity.Text; }
        }

        protected override string StreetAddress
        {
            get { return txtStreet.Text; }
            set { txtStreet.Text = value; }
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
                                Field = _shippingAddr.Address.PostalCode,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode")
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
                                Field = _shippingAddr.Address.Line1,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
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
                Recipient = _shippingAddr.Recipient;
                ZipCode = _shippingAddr.Address.PostalCode;
                StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                LookupCounties();                
                County = _shippingAddr.Address.CountyDistrict;
                City = _shippingAddr.Address.City;
                StreetAddress = _shippingAddr.Address.Line1;
                PhoneNumber = _shippingAddr.Phone;
            }
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(Recipient) && (Recipient.Trim().Length < 2 || Recipient.Trim().Length > 40))
            {   
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
            }

            if (!string.IsNullOrEmpty(ZipCode))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(ZipCode, @"^(\d{6})$"))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                }
            }

            if (!string.IsNullOrEmpty(StateProvince) && (StateProvince.Trim().Length < 1 || StateProvince.Trim().Length > 30))
            {                
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidState"));                
            }

            if (!string.IsNullOrEmpty(City) && (City.Trim().Length < 1 || City.Trim().Length > 20))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCity"));
            }

            if (!string.IsNullOrEmpty(StreetAddress) && (StreetAddress.Trim().Length < 1 || StreetAddress.Trim().Length > 60))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            }

            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_shippingAddr.Phone, @"^(\d{10,15})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        private void LookupCounties()
        {
           
            dnlCounty.Items.Clear();
         
            string filePath = Server.MapPath("~/Ordering/Controls/Address/AddressFormat/ru-BY-States.xml");
            var xDoc = XElement.Load(filePath);
            var query = from xEle in xDoc.Descendants()
                        select new { value = xEle.Attribute("Name").Value, text = xEle.Attribute("Name").Value };
            dnlCounty.DataValueField = "value";
            dnlCounty.DataTextField = "text";
            dnlCounty.DataSource = query.ToList();
            dnlCounty.DataBind();

        }

        #endregion

        
    }
}