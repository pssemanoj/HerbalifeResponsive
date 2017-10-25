using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class BEAddressControl : AddressControlBase
    {
        private ShippingProvider_BE _shippingProvider;

        #region Properties
        protected override string Recipient
        {
            set { tbNombre.Text = value; }
            get { return tbNombre.Text; }
        }

        protected override string StreetAddress
        {
            set { tbAdddress.Text = value; }
            get { return tbAdddress.Text.Trim(); }
        }

        protected override string StreetAddress2
        {
            set { tbHouseNumber.Text = value; }
            get { return tbHouseNumber.Text.Trim(); }
        }

        protected override string City
        {
            //set { tbCity.Text = value; }
            //get { return tbCity.Text.Trim(); }
            set {
                if(ddlCity != null && ddlCity.Items != null && ddlCity.Items.Count > 0 && ddlCity.Items.IndexOf(ddlCity.Items.FindByValue(value)) > 0)
                ddlCity.SelectedValue = value;
            }
            get
            {
                if (ddlCity != null && ddlCity.Items != null && ddlCity.Items.Count > 0)
                    return ddlCity.SelectedValue;
                else
                    return string.Empty;
            }
        }

        protected override string ZipCode
        {
            set { tbPostalCode.Text = value; }
            get { return tbPostalCode.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { tbPhoneNumber.Text = value; }
            get { return tbPhoneNumber.Text; }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            _shippingProvider = ProductsBase.GetShippingProvider() as ShippingProvider_BE;
        }

        #region override

        public override void LoadPage()
        {
            tbPhoneNumber.MaxLength = Thread.CurrentThread.CurrentCulture.Name.Equals("fr-BE") ? 11 : 10;
            tbCountry.Text = @"Belgium"; 
            if (_shippingAddr != null && _shippingProvider != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                if(_shippingAddr.Address != null)
                    LoadCitiesForZipCode(_shippingAddr.Address.PostalCode);
                City = _shippingAddr.Address.City;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                tbNombre.Focus();
            }
            
            cvNombreRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCareOfName");
            cvAddressRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1");
            cvHouseNumberRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet2");
            cvHouseNumberRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress2");
            cvCity.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity");
            cvPostalCode.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode");
            cvPostalCodeRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode");
            cvPhoneNumberRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone");
            cvPhoneNumberRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone");
        }
        #endregion

        #region Validators
        protected void ValidateRequiredTextBox(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (args.Value.Length > 0);
        }

        protected void ValidatePostalCode(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{4})$");
        }

        protected void ValidateHouseNumber(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^[a-zA-Z0-9\s_.,:;/&amp;-]{1,6}$");
        }

        protected void ValidatePhoneNumber(object source, ServerValidateEventArgs args)
        {
            if (Thread.CurrentThread.CurrentCulture.Name.Equals("fr-BE"))
            {
                args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{9,11})$");
            }
            else
            {
                args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{9,10})$");
            }
            
        }

        protected void Validator_PreRender(object sender, EventArgs e)
        {
            var isValid = true;
            WebControl control = null;
            var uxCustomValidator = sender as CustomValidator;
            if (uxCustomValidator != null)
            {
                isValid = uxCustomValidator.IsValid;
                control = uxCustomValidator.NamingContainer.FindControl(uxCustomValidator.ControlToValidate) as WebControl;
            }

            if (control != null)
            {
                if (!isValid)
                {
                    uxCustomValidator.CssClass = control.Attributes["class"] = "error";
                }
                else
                {
                    if ((control as TextBox) != null
                        && control.Attributes["class"] != ""
                        && (uxCustomValidator.ValidateEmptyText 
                            || (!string.IsNullOrEmpty((control as TextBox).Text)
                                && !uxCustomValidator.ValidateEmptyText)))
                    {
                        control.Attributes["class"] = "";
                        uxCustomValidator.CssClass = "hide";
                    }
                }
            }
        }
        #endregion
        protected void tbPostalCode_TextChanged(object sender, EventArgs e)
        {
            ddlCity.Items.Clear();
            if (!string.IsNullOrEmpty(tbPostalCode.Text))
            {
                LoadCitiesForZipCode(tbPostalCode.Text);
            }
        }
        private bool LoadCitiesForZipCode(string zipCode)
        {
            ddlCity.Items.Clear();
            if (!string.IsNullOrEmpty(zipCode) && zipCode.Trim().Length > 3)
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider(ProductsBase.CountryCode);
                if (provider != null)
                {
                    var lookupResults = provider.LookupCitiesByZip(ProductsBase.CountryCode, zipCode.Trim());
                    if (lookupResults != null && lookupResults.Count > 0)
                    {
                        var items = (from s in lookupResults select new ListItem { Text = s.City, Value = s.City }).ToArray();
                        this.ddlCity.Items.AddRange(items);
                        this.ddlCity.Items.Insert(0, new ListItem(this.GetLocalResourceObject("Select") as string, string.Empty));
                        if (lookupResults.Count == 1)
                            this.ddlCity.SelectedIndex = 1;
                        else
                            this.ddlCity.SelectedIndex = 0;
                        ddlCity.Focus();
                        return true;
                    }
                }
            }
            return false;
        }
        protected void ddlCity_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (ddlCity != null && ddlCity.SelectedIndex <= 0)
            {
                e.IsValid = false;
            }
            else
            {
                e.IsValid = true;
            }
        }
    }
}