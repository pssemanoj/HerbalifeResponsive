using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
	public partial class JPAddressControl : AddressControlBase
    {
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
            set { txtStreet2.Text = value; }
            get { return txtStreet2.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text.Trim(); }
        }
        protected override string City
        {
            set { txtCity.Text = value; }
            get { return txtCity.Text.Trim(); }
        }
        protected override string County
        {
            set { txtTown.Text = value; }
            get { return txtTown.Text.Trim(); }
        }
        protected override string StreetAddress3
        { 
            set { txtPrefecture.Text = GetID(null,value); }
            get { return GetID( txtPrefecture.Text.Trim(),null); }
        }
        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
        }
        protected override string StateProvince
        {
            set { txtPrefecture.Text = value; }
            get { return txtPrefecture.Text.Trim(); }
        }
        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text.Trim(); }
        }
        private ShippingProvider_JP _shippingProvider;
        protected void Page_Load(object sender, EventArgs e)
		{
           _shippingProvider = ProductsBase.GetShippingProvider() as ShippingProvider_JP;

        }
        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                County = _shippingAddr.Address.CountyDistrict; 
                ZipCode = _shippingAddr.Address.PostalCode;
                StreetAddress3 = _shippingAddr.Address.Line3;
                City = _shippingAddr.Address.City;
                PhoneNumber = _shippingAddr.Phone;
                AreaCode = _shippingAddr.AreaCode;
                StateProvince = _shippingAddr.Address.StateProvinceTerritory;
            }
            else
            {
                LookupAddress( ZipCode);
            }
        }
        protected override RequireFieldDef[] RequiredFields
        {
            set {; }
            get
            {
                return new[]
                    {
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Recipient,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoFirstName")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line1,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line3,
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
                            },
                    };
            }
        }
        private bool LookupAddress( string Zipcode)
        {
            if (!string.IsNullOrEmpty(Zipcode))
            {
             
                IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
                if (provider != null)
                {
                    var Address = ShippingProvider_JP.GetAddressByPostalCode(ProductsBase.CountryCode, Zipcode);
                    if (Address != null && Address.AddressDetails != null && Address.AddressDetails.Count > 0)
                    {
                        
                        string CityTown= Address.AddressDetails[0].City.ToString();
                        List<string> words = Regex.Split(CityTown, @"\W+").ToList();
                        txtCity.Text = words[0].ToString();
                        if (words.Count > 1)
                        {
                            txtTown.Text = words[1].ToString();
                            txtStreet.Text = Address.AddressDetails[0].Street.ToString();
                        }
                        else
                        {
                            txtTown.Text = Address.AddressDetails[0].Street.ToString();
                        }
                        txtPrefecture.Text = Address.AddressDetails[0].State.ToString();
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            return false;
        }


        public override void ValidationCheck(List<string> _errors)
        {

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                string[] numbers = _shippingAddr.Phone.Split('-');

                if (numbers[0].Length == 0)
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode"));
                else if (!Regex.IsMatch(numbers[0], @"^(\d{2,5})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode"));

                if (!Regex.IsMatch(numbers[1], @"^(\d{5,8})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }


        protected void txtPostCode_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPostCode.Text))
            {
                LookupAddress(txtPostCode.Text);
            }
        }

        private string GetID( string Name=null,string FreightVariant = null)
        {
            string ID = string.Empty;
            var R1 = ResolveUrl("\\Ordering\\Controls\\Address\\AddressFormat\\ja-JP-Prefecture.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(R1);
            XmlNodeList publicationsNodeList = xmlDoc.SelectNodes("CountyOrCity");
            XmlNodeList child = publicationsNodeList[0].SelectNodes("CountyCity");

            foreach (XmlElement y in child)
            {
                if(Name != null)
                {
                    if (y.Attributes["Name"].Value == Name)
                    {
                        ID = y.Attributes["ID"].Value;
                        break;
                    }

                }
                else
                {
                    if (y.Attributes["ID"].Value == FreightVariant)
                    {
                        ID = y.Attributes["Name"].Value;
                        break;
                    }

                }

            }
            return ID;
        }
        public string ResolveUrl(string originalUrl)
        {
            if (originalUrl != null && originalUrl.Trim() != "")
            {
                if (originalUrl.StartsWith("/"))
                {
                    originalUrl = "~" + originalUrl;
                }
                else
                {
                    originalUrl = "~/" + originalUrl;
                }

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if (originalUrl == null)
            {
                return null;
            }
            if (originalUrl.IndexOf("://") != -1)
            {
                return originalUrl;
            }
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                }
                return newUrl;
            }
            return originalUrl;
        }
    }

    }
