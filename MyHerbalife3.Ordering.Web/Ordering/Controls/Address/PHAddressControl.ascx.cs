using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class PHAddressControl : AddressControlBase
    {
        private ShippingProvider_PH _shippingProvider;
        private ShippingProviderBase _shippingProviderBase;

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
            set { tbAddress2.Text = value; }
            get { return tbAddress2.Text.Trim(); }
        }

        protected override string City
        {
            set { }
            get
            {
                return dnlCity.SelectedItem == null || dnlCity.SelectedValue == "0"
                           ? string.Empty
                           : dnlCity.SelectedItem.Text;
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
        protected string CountryCodePH 
        {
            set { tbCountryCode.Text = value; }
            get { return tbCountryCode.Text.Trim(); }
        }
        protected override string AreaCode
        {
            set { tbAreaCode.Text = value != null ? value.Trim().Substring(1, value.Trim().Length-1) : string.Empty; }
            get { return tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim(); }
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
            _shippingProvider = ProductsBase.GetShippingProvider() as ShippingProvider_PH;
            _shippingProviderBase = new ShippingProviderBase();
        }

        #region override

        public override void LoadPage()
        {
            dnlState.SelectedIndex = dnlCity.SelectedIndex = -1;
            dnlCity.Enabled = false;
            tbCountryCode.Text = "63";
            tbCountryCode.ReadOnly = true;
            tbCountryCode.Enabled = false;

            if (_shippingProvider != null)
            {
                if (dnlState.Items.Count == 0 && _shippingProviderBase != null)
                {
                    dnlState.DataSource = _shippingProviderBase.GetStatesForCountry("PH");
                    dnlState.DataBind();
                    dnlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                }
                }
            if (_shippingAddr != null && _shippingProvider != null)
            {
                Recipient = _shippingAddr.Recipient;
                StreetAddress = _shippingAddr.Address.Line1;
                StreetAddress2 = _shippingAddr.Address.Line2;

                ListItem item = dnlState.Items.FindByText(_shippingAddr.Address.StateProvinceTerritory);
                if (item != null)
                {
                    dnlState.SelectedIndex = -1;
                    item.Selected = true;
                    dnlCity.DataSource = _shippingProvider.GetCitiesForState("PH", item.Text);
                    dnlCity.DataBind();

                    item = dnlCity.Items.FindByText(_shippingAddr.Address.City);
                    if (item != null)
                    {
                        dnlCity.SelectedIndex = -1;
                        item.Selected = true;
                        dnlCity.Enabled = true;
                    }
                }
                ZipCode = _shippingAddr.Address.PostalCode;
                PhoneNumber = _shippingAddr.Phone;
                AreaCode = _shippingAddr.AreaCode;
                tbNombre.Focus();
                cvNombreRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCareOfName");
                cvNombreRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName");
                cvAddressRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1");
                cvAddress2Required.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet2");
                cvStateRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoState");
                cvCity.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity");
                cvPostalCode.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode");
                cvAreaCodeRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoAreaCode");
                cvAreaCodeRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidAreaCode");
                //cvPhoneNumberRequired.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone");
                //cvPhoneNumberRegex.ErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone");
            }
        }
        #endregion

        #region Validators
        protected void ValidateRequiredTextBox(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (args.Value.Length > 0);
        }

        protected void ValidateState(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (dnlState.SelectedIndex > 0);
        }

        protected void ValidateCity(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (dnlCity.SelectedIndex > 0);
        }

        protected void ValidateCareOfName(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^[a-zA-Z ]*$");
        }

        protected void ValidatePostalCode(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(args.Value)
                            && Regex.IsMatch(args.Value, @"^(\d{4})$");
        }

        protected void ValidateAreaCode(object source, ServerValidateEventArgs args)
        {
            bool blnAreaCode = false;
            bool blnPhone = false;
            blnAreaCode = !string.IsNullOrEmpty(tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim())
                            && Regex.IsMatch(tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim(), @"^(\d{3})$");


             blnPhone = !string.IsNullOrEmpty(tbPhoneNumber.Text.Trim())
                            && (Regex.IsMatch(tbPhoneNumber.Text.Trim(), @"^(\d{7})$") || Regex.IsMatch(tbPhoneNumber.Text.Trim(), @"^(\d{8})$"))
                            && !(tbPhoneNumber.Text.Trim().Length + tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim().Length > 11);

            args.IsValid = blnAreaCode && blnPhone;
        }

        protected void ValidatePhoneNumber(object source, ServerValidateEventArgs args)
        {
            bool blnAreaCode = false;
            bool blnPhone = false;
            blnAreaCode = !string.IsNullOrEmpty(tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim())
                            && Regex.IsMatch(tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim(), @"^(\d{3})$");


            blnPhone = !string.IsNullOrEmpty(tbPhoneNumber.Text.Trim())
                           && (Regex.IsMatch(tbPhoneNumber.Text.Trim(), @"^(\d{7})$") || Regex.IsMatch(tbPhoneNumber.Text.Trim(), @"^(\d{8})$"))
                           && !(tbPhoneNumber.Text.Trim().Length + tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim().Length > 11);

            args.IsValid = blnAreaCode && blnPhone;
            //args.IsValid = !string.IsNullOrEmpty(args.Value)
            //                && (Regex.IsMatch(args.Value, @"^(\d{7})$") || Regex.IsMatch(args.Value, @"^(\d{8})$"))
            //                && !(args.Value.Length + tbAreaCode.MaskParts.Owner.TextWithPromptAndLiterals.Trim().Length > 11);
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

        #region events
        protected void dnlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (_shippingProvider != null && ddl != null && ddl.SelectedIndex > 0)
            {
                dnlCity.DataSource = _shippingProvider.GetCitiesForState("PH", ddl.SelectedValue);
                dnlCity.DataBind();
                dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                dnlCity.Enabled = true;
                dnlCity.SelectedIndex = 0;
                dnlCity.Focus();
            }
            else
            {
                dnlCity.Enabled = false;
                dnlCity.SelectedIndex = 0;
            }
        }

        protected void dnlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_shippingProvider != null)
            {
                var ddl = sender as DropDownList;
                string stateSelected = dnlState.SelectedValue;
                if (null != ddl && ddl.SelectedIndex > 0)
                {
                    string zip = _shippingProvider.GetZipsForCity("PH", stateSelected, ddl.SelectedValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(zip))
                    {
                        tbPostalCode.Text = zip;
                        dnlCity.Focus();
                    }
                }
            }
        }

        protected void dnlState_DataBound(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (_shippingProvider != null && ddl != null)
            {
                dnlCity.DataSource = _shippingProvider.GetCitiesForState("PH", ddl.SelectedValue);
                dnlCity.DataBind();
                dnlCity.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                dnlCity.SelectedIndex = 0;
                dnlCity.Focus();
            }
        }
        #endregion
    }
}