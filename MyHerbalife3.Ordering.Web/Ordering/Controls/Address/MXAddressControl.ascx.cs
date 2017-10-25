using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class MXAddressControl : AddressControlBase
    {
        protected override string Recipient
        {
            set { txtNombre.Text = value; }
            get { return txtNombre.Text; }
        }

        protected override string StreetAddress
        {
            set { tbDir.Text = value; }
            get { return tbDir.Text.Trim(); }
        }

        //Colonia
        protected override string StreetAddress3
        {
            set { }
            get
            {
                return dnlTown.SelectedItem == null || dnlTown.SelectedValue == "0"
                           ? string.Empty
                           : dnlTown.SelectedItem.Text;
            }
        }

        // Municipal
        protected override string City
        {
            set { }
            get
            {
                return dnlMunicipal.SelectedItem == null || dnlMunicipal.SelectedValue == "0"
                           ? string.Empty
                           : dnlMunicipal.SelectedItem.Text;
            }
        }

        protected override string StateProvince
        {
            set { }
            get
            {
                return dnlState.SelectedItem == null || dnlState.SelectedValue == "0"
                           ? string.Empty
                           : dnlState.SelectedItem.Text;
            }
        }

        protected override string ZipCode
        {
            set { tbPostalCode.Text = value; }
            get { return tbPostalCode.Text.Trim(); }
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
                string requiredFieldMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "RequiredField");
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
                                Field = _shippingAddr.Address.City,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line3,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCounty")
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

        public override void LoadPage()
        {
            dnlState.SelectedIndex = dnlMunicipal.SelectedIndex = dnlTown.SelectedIndex = -1;
            dnlMunicipal.Enabled = dnlTown.Enabled = false;
            if (_shippingProvider != null)
            {
                if (dnlState.Items.Count == 0)
                {
                    dnlState.DataSource = _shippingProvider.GetStatesForCountry(this.County);
                    dnlState.DataBind();
                    dnlState.Items.Insert(0, new ListItem(string.Empty, "0"));
                    dnlState.SelectedIndex = 0;
                }
                List<DeliveryOption> addresses = _shippingProvider.
                    GetShippingAddresses(ProductsBase.DistributorID, ProductsBase.Locale);
                if (addresses != null)
                {
                    var primaryList = addresses.Where(a => a.IsPrimary);
                    tbPostalCode.Text = primaryList.Count() > 0 ? primaryList.First().Address.PostalCode : string.Empty;
                }
            }
            if (_shippingAddr != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;

                ListItem item = dnlState.Items.FindByText(_shippingAddr.Address.StateProvinceTerritory);
                if (item != null)
                {
                    dnlState.SelectedIndex = -1;
                    item.Selected = true;
                    dnlMunicipal.DataSource = _shippingProvider.GetCitiesForState(this.County,item.Text);
                    dnlMunicipal.DataBind();

                    item = dnlMunicipal.Items.FindByText(_shippingAddr.Address.City);
                    if (item != null)
                    {
                        dnlMunicipal.SelectedIndex = -1;
                        item.Selected = true;
                        dnlTown.DataSource =
                            _shippingProvider.GetStreetsForCity(this.County,
                                _shippingAddr.Address.StateProvinceTerritory, _shippingAddr.Address.City);
                        dnlTown.DataBind();
                        item = dnlTown.Items.FindByText(_shippingAddr.Address.Line3);
                        if (item != null)
                        {
                            dnlTown.SelectedIndex = -1;
                            item.Selected = true;
                        }
                    }
                }
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                txtNombre.Focus();
            }
        }

        private ShippingProvider_MX _shippingProvider;

        protected void Page_Load(object sender, EventArgs e)
        {
            _shippingProvider = ProductsBase.GetShippingProvider() as ShippingProvider_MX;
        }

        public override void ValidationCheck(List<string> _errors)
        {
            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                if (!Regex.IsMatch(_shippingAddr.Phone,
                                   @"^(\d{7,10})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));
            }
        }

        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_shippingProvider != null)
            {
                var ddl = sender as DropDownList;
                dnlMunicipal.DataSource = _shippingProvider.GetCitiesForState(this.County,ddl.SelectedValue);
                dnlMunicipal.DataBind();
                dnlMunicipal.Items.Insert(0, new ListItem(string.Empty, "0"));
                dnlMunicipal.Enabled = true;
                dnlMunicipal.SelectedIndex = 0;
                dnlTown.SelectedIndex = 0;
                dnlMunicipal.Focus();
            }
        }

        protected void dnlMunicipal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_shippingProvider != null)
            {
                var ddl = sender as DropDownList;
                string stateSelected = dnlState.SelectedValue;
                dnlTown.DataSource = _shippingProvider.GetStreetsForCity( this.County, stateSelected, ddl.SelectedValue);
                dnlTown.Enabled = true;
                dnlTown.DataBind();
                dnlTown.Items.Insert(0, new ListItem(string.Empty, "0"));
                dnlTown.SelectedIndex = 0;
                dnlTown.Focus();
            }
        }

        protected void dnlState_DataBound(object sender, EventArgs e)
        {
            if (_shippingProvider != null)
            {
                var ddl = sender as DropDownList;
                dnlMunicipal.DataSource = _shippingProvider.GetCitiesForState(this.County, ddl.SelectedValue);
                dnlMunicipal.DataBind();
                dnlMunicipal.Items.Insert(0, new ListItem(string.Empty, "0"));
                dnlMunicipal.SelectedIndex = 0;
                dnlMunicipal.Focus();
            }
        }

        protected void dnlMunicipal_DataBound(object sender, EventArgs e)
        {
            if (_shippingProvider != null)
            {
                var ddl = sender as DropDownList;
                string stateSelected = dnlState.SelectedValue;
                dnlTown.DataSource = _shippingProvider.GetStreetsForCity( this.County, stateSelected, ddl.SelectedValue);
                dnlTown.DataBind();
                dnlTown.Items.Insert(0, new ListItem(string.Empty, "0"));
                dnlTown.SelectedIndex = 0;
                dnlTown.Focus();
            }
        }

        protected void dnlTown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dnlMunicipal.SelectedIndex > 0 && dnlState.SelectedIndex > 0 && dnlTown.SelectedIndex > 0)
            {
                string zip = _shippingProvider.LookupZipCode(dnlState.Text, dnlMunicipal.Text, dnlTown.Text);
                if (!string.IsNullOrEmpty(zip))
                {
                    tbPostalCode.Text = zip;
                    tbPostalCode.ReadOnly = true;
                    dnlTown.Focus();
                }
            }
        }
    }
}