using System;
using System.Collections;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class KrAddressControl : AddressControlBase
    {
        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        protected override string City
        {
            set { }
            get { return string.Empty; }
        }

        protected override string StateProvince
        {
            set { txtAddress1.Text = value; }
            get { return txtAddress1.Text.Trim(); }
        }

        protected override string StreetAddress
        {
            set { txtAddress2.Text = value; }
            get { return txtAddress2.Text.Trim(); }
        }

        protected override string StreetAddress2
        {
            set { txtAddress3.Text = value; }
            get { return txtAddress3.Text.Trim(); }
        }

        protected override string ZipCode
        {
            set { txtPostalCode1.Text = value; }
            get { return txtPostalCode1.Text.Trim(); }
        }

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text; }
        }

        protected override string Extension
        {
            set { txtExtension.Text = value; }
            get { return txtExtension.Text; }
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
                                Field = _shippingAddr.Address.StateProvinceTerritory,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoState")
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

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public override void LoadPage()
        {
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.City + ' ' + _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                StateProvince = _shippingAddr.Address.StateProvinceTerritory;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                AreaCode = _shippingAddr.AreaCode;
                Extension = _shippingAddr.AltAreaCode;
                txtAddress3.Enabled = true;
            }
            else
            {
                txtAddress3.Enabled = false;
            }
        }

        private string getLine1()
        {
            string address2 = txtAddress2.Text;
            int indexOfSpace = -1;
            if ((indexOfSpace = address2.IndexOf(' ')) != -1)
            {
                return address2.Substring(indexOfSpace);
            }
            return string.Empty;
        }

        private string getLine2()
        {
            return txtAddress3.Text;
        }

        private string getCity()
        {
            string address2 = txtAddress2.Text;
            int indexOfSpace = -1;
            if ((indexOfSpace = address2.IndexOf(' ')) != -1)
            {
                return address2.Substring(0, indexOfSpace);
            }
            return address2;
        }

        public override object CreateAddressFromControl(string typeName)
        {
            object dataContext = DataContext;
            var shipping = new ShippingAddress_V02();
            if (dataContext != null)
                shipping = (ShippingAddress_V02) dataContext;
            var shippingAddress = new Address_V01();

            shippingAddress.Line1 = getLine1();
            shippingAddress.Line2 = getLine2();
            shippingAddress.Line3 = StreetAddress3 ?? string.Empty;
            shippingAddress.Line4 = StreetAddress4 ?? string.Empty;

            shippingAddress.City = getCity();
            shippingAddress.CountyDistrict = County ?? string.Empty;
            shippingAddress.StateProvinceTerritory = txtAddress1.Text;

            shippingAddress.PostalCode = ZipCode;
            shipping.Address = shippingAddress;
            shipping.Phone = PhoneNumber;
            shipping.AltAreaCode = Extension ?? string.Empty;
            shipping.AreaCode = AreaCode ?? string.Empty;
            shipping.Recipient = Recipient;
            return _shippingAddr = shipping;
        }

        protected void GoButtonClicked(object sender, EventArgs e)
        {
            lbErrorMessage.Text = string.Empty;
            string searchTerm = txtSearchTerm.Text;
            if (string.IsNullOrEmpty(searchTerm))
            {
                lbErrorMessage.Text = GetLocalResourceObject("NoSearchTerm") as string;
                return;
            }
            else if (searchTerm.Length < 2)
            {
                lbErrorMessage.Text = GetLocalResourceObject("MinTwoCharacters") as string;
                return;
            }
            var shippingProvider = ShippingProvider.GetShippingProvider("KR") as ShippingProvider_KR;
            List<Address_V01> results = shippingProvider.AddressSearch(searchTerm);

            if (results != null && results.Count > 0)
            {
                var sList = new SortedList();

                foreach (Address_V01 addr in results)
                {
                    string value = string.Format("{0} {1} {2}", addr.City, addr.Line1, addr.Line2);
                    string key = string.Format("{0}|{1}|{2}|{3}", addr.City, addr.Line1, addr.Line2, addr.PostalCode);
                    sList.Add(key, value);
                }
                lbAddresses.DataSource = sList;
                lbAddresses.DataBind();

                txtAddress3.Enabled = true;
            }
            else
            {
                lbAddresses.Items.Clear();
                txtAddress3.Enabled = false;
                lbErrorMessage.Text = GetLocalResourceObject("NoResultFound") as string;
            }
        }

        private void clearFields()
        {
            City = string.Empty;
            StreetAddress = string.Empty;
            StreetAddress2 = string.Empty;
        }

        protected void lbAddresses_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbAddresses.SelectedItem != null)
            {
                clearFields();
                string key = lbAddresses.SelectedItem.Value;
                string[] keySplit = key.Split('|');
                StateProvince = keySplit[0];
                StreetAddress = keySplit[1];
                StreetAddress2 = keySplit[2];
                ZipCode = keySplit[3];
            }
        }
    }
}