using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class NLAddressControl : AddressControlBase
    {
        private ShippingProvider_NL _shippingProvider;

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
            set { tbCity.Text = value; }
            get { return tbCity.Text.Trim(); }
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
            _shippingProvider = ProductsBase.GetShippingProvider() as ShippingProvider_NL;
        }

        #region override

        public override void LoadPage()
        {
            tbCountry.Text = @"Netherland";
            if (_shippingAddr != null && _shippingProvider != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;
                City = _shippingAddr.Address.City;
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                tbNombre.Focus();
            }

            cvNombreRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCareOfName");
            cvAddressRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1");
            cvAddressRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress");
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

        protected void ValidateAddress(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(?=.*[\u00c0-\u01ffA-Za-z,-.'])+[\u00c0-\u01ff0-9a-zA-Z\/,-.'\\ ]*$");
        }

        protected void ValidatePostalCode(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{4}\s[A-Z]{2})$");
        }

        protected void ValidateHouseNumber(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^[a-zA-Z0-9\s_.,:;/&amp;-]{1,6}$");
        }

        protected void ValidatePhoneNumber(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{6,10})$");
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
    }
}