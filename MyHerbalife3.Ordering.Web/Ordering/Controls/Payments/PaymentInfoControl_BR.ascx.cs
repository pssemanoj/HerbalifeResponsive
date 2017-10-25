using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Shared.Infrastructure;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoControl_BR : PaymentInfoControl
    {
        private const string ValidationScriptMessages = "<script type='text/javascript'>    	var CardNumberRequired = '{0}';    	var CardNumberInvalid = '{1}';    	var CardNameRequired = '{2}';    	var ExpDateRequired = '{3}';    	var AddCardQuestion = '{4}';    	var DeleteCardQuestion = '{5}';     var CardHasExpired = '{6}';     var StreetAddressRequired = '{7}';     var CityRequired = '{8}';     var StateRequired = '{9}';     var ValidZipRequired = '{10}';     var CardTypeRequired = '{11}';   var NumberRequired = '{12}';    var NeighborhoodRequired = '{13}'; var TokenizationFailed = '{14}';     var validNickNameRequired = '{15}';     var validNonDupeNickNameRequired = '{16}';</script>";

        /// <summary>
        ///     Create payment information from the payment info control
        /// </summary>
        /// <returns>The payment information object</returns>
        protected override PaymentInformation createPaymentInfoFromControl()
        {
            PaymentInformation paymentInfo = null;
            if (PaymentID > 0)
            {
                paymentInfo = base.paymentInfos.Find(p => p.ID == PaymentID);
                if (null == paymentInfo)
                {
                    paymentInfo = new PaymentInformation();
                }
            }
            else
            {
                paymentInfo = new PaymentInformation();
            }
            ///TODO: Confirm name to be broken down
            paymentInfo.CardHolder = new Name_V01() { First = FirstName, Last = LastName, Middle = MiddleInnitial};
            if (!base.IsMasked(CardNumber))
            {
                paymentInfo.CardNumber = CardNumber;
            }
            paymentInfo.CardType = CardType;
            paymentInfo.Created = DateTime.Now;
            paymentInfo.Alias = NickName;
            paymentInfo.IsPrimary = IsPrimary;

            try
            {
                paymentInfo.Expiration = convertExpDateToDatetimeFormat(paymentInfo.CardType);
            }

            catch (Exception ex)
            {
                LoggerHelper.Warn(ex.ToString());
            }

            paymentInfo.ID = PaymentID;
            Address_V01 billingAddress = new Address_V01();
            if (rblBillingAddress.SelectedIndex == 0)
            {
                var shippingAddress = getShippingAddress().Address;
                billingAddress.Line1 = shippingAddress.Line1;
                billingAddress.Line2 = shippingAddress.Line2;
                billingAddress.Line3 = shippingAddress.Line3;
                billingAddress.Line4 = shippingAddress.Line4;
                billingAddress.City = shippingAddress.City;
                billingAddress.StateProvinceTerritory = shippingAddress.StateProvinceTerritory;
                billingAddress.PostalCode = shippingAddress.PostalCode;
                billingAddress.Country = this.CountryCode;
            }
            else
            {
                var line1 = string.Concat(txtStreetAddress.Text.Trim(), " ", txtStreetAddress2.Text).Trim();
                var line4 = string.Concat(txtStreetAddress.Text.Trim(), "%%%", txtStreetAddress2.Text).Trim();
                billingAddress.Line1 = line1.Length <= 50 ? line1 : line1.Substring(0, 50);
                billingAddress.Line2 = txtNeighborhood.Text.Trim();
                billingAddress.Line3 = txtNumber.Text.Trim();
                billingAddress.Line4 = line4.Length <= 50 ? line4 : line4.Substring(0, 50);
                billingAddress.City = txtCity.Text.Trim();
                billingAddress.StateProvinceTerritory = txtState.Text.Trim();
                billingAddress.PostalCode = txtZip.Text;
                billingAddress.Country = this.CountryCode;
            }
            paymentInfo.BillingAddress = billingAddress;

            return paymentInfo;
        }

        /// <summary>
        ///     Pre-process before edit payment information upon continue button click
        /// </summary>
        protected override void populateControlForEditFromPaymentInfo()
        {
            //PaymentInformation paymentInfo = _paymentInfo;
            //PaymentInformation paymentInfo = paymentInfos.;
            if (base.getPaymentInfo() == null)
            {
                lblBillingAddressMessage.Text = PAYMENTINFORMATION_GET_FAILURE_MESSAGE;

                return;
            }

            this.hdID.Value = paymentInfo.ID.ToString();
            this.txtCardHolderName.Text = paymentInfo.CardHolder.First.Trim();
            ///TODO: break down name
            //this.tbLastName.Text = paymentInfo.CardHolder.Last;
            //this.tbMI.Text = paymentInfo.CardHolder.Middle;
            this.txtCardNumber.ReadOnly = true;
            this.txtCardNumber.Enabled = false;
            this.txtCardNumber.Text = MaskCardNumber(paymentInfo.CardNumber);

            if (getCreditCardTypeListFromProvider().Items.FindByValue(paymentInfo.CardType.Trim()) != null)
            {
                this.ddlCardType.SelectedValue = paymentInfo.CardType.Trim();
                this.ddlCardType.Enabled = false;
            }

            this.ddlExpMonth.SelectedValue = string.Format("{0:MM}", paymentInfo.Expiration);

            ListItem selected = this.ddlExpYear.Items.FindByValue(string.Format("{0:yy}", paymentInfo.Expiration));
            if (selected != null)
            {
                this.ddlExpYear.ClearSelection();
                selected.Selected = true;
            }
            else
            {
                this.ddlExpYear.SelectedIndex = -1;
            }

            this.txtNickName.Text = paymentInfo.Alias.Trim();
            this.chkMakePrimaryCreditCard.Checked = paymentInfo.IsPrimary ? true : false;
            if (CreditCard.GetCardType(paymentInfo.CardType) == IssuerAssociationType.MyKey)
            {
                this.pnlExpDate.Attributes.Add("style", "visibility:hidden");
            }
            if (_paymentsConfig.AddressRequiredForNewCard)
            {
                if (!string.IsNullOrEmpty(paymentInfo.BillingAddress.Line4))
                {
                    var streets = paymentInfo.BillingAddress.Line4.Split(new string[] {"%%%"},
                                                                         StringSplitOptions.RemoveEmptyEntries);
                    if (streets.Length > 0)
                    {
                        this.txtStreetAddress.Text = streets[0];
                        if (streets.Length > 1)
                        {
                            this.txtStreetAddress2.Text = streets[1];
                        }
                    }
                    else
                    {
                        this.txtStreetAddress.Text = string.Empty;
                        this.txtStreetAddress2.Text = string.Empty;
                    }
                }
                this.txtNumber.Text = (null != paymentInfo.BillingAddress.Line3)
                                          ? paymentInfo.BillingAddress.Line3.Trim()
                                          : string.Empty;
                this.txtNeighborhood.Text = (null != paymentInfo.BillingAddress.Line2)
                                                ? paymentInfo.BillingAddress.Line2.Trim()
                                                : string.Empty;
                this.txtCity.Text = (null != paymentInfo.BillingAddress.City)
                                        ? paymentInfo.BillingAddress.City
                                        : string.Empty;
                this.txtState.Text = (null != paymentInfo.BillingAddress.StateProvinceTerritory)
                                         ? paymentInfo.BillingAddress.StateProvinceTerritory.Trim()
                                         : string.Empty;
                this.txtZip.Text = (null != paymentInfo.BillingAddress.PostalCode)
                                       ? paymentInfo.BillingAddress.PostalCode.Trim()
                                       : string.Empty;
                if (!string.IsNullOrEmpty(this.txtZip.Text) && this.txtZip.Text.Length>5 && !this.txtZip.Text.Substring(5).Equals("000"))
                {
                    this.SetAvailableFields(false);
                }
                if (this.rblBillingAddress.Visible)
                {
                    this.rblBillingAddress.SelectedIndex = 1;
                    SetBillingAddressOption(null, null);
                }
            }
        }

        /// <summary>
        ///     Clear billing address control fields
        /// </summary>
        protected override void clearBillingAddressControlFields()
        {
            this.txtStreetAddress.Text =
                this.txtStreetAddress2.Text =
                this.txtNumber.Text =
                this.txtNeighborhood.Text =
                this.txtCity.Text =
                this.txtZip.Text =
                this.lblBillingAddressMessage.Text =
                this.txtState.Text =
                string.Empty;
        }

        /// <summary>
        ///     Write client validation scripts to browser if user can add cards
        /// </summary>
        protected override void setCardValidationClientScripts()
        {
            if (!_RegisteredCardsOnly)
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof (string), "ValidationMessages", string.Format(ValidationScriptMessages,
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateAddCardMissingCard") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateAddCardBadCard") as string),
                                                                            EscapeJavascriptQuotes( GetLocalResourceObject("ValidateAddCardMissingName") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateAddCardMissingExpDate") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateAddCard") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateRemoveCard") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject( "ValidateAddCardExpired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateStreetAddressRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateCityRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateStateRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateSelectCardType") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateZipRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateNumberRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateNeighborhoodRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateTokenizationFailed") as string),
                                                                            EscapeJavascriptQuotes(GetGlobalResourceObject("MyHL_ErrorMessage", "InvalidNickname") as string),
                                                                            EscapeJavascriptQuotes(GetGlobalResourceObject("MyHL_ErrorMessage", "DuplicateCardError") as string)));            
            }

            Page.ClientScript.RegisterClientScriptBlock(typeof (string), "RegisterdCardsOnly", string.Format(ScriptBlock, string.Concat("var registeredCards = ", _RegisteredCardsOnly.ToString().ToLower(),";")));
            Page.ClientScript.RegisterClientScriptBlock(typeof (string), "ControlNamingPrefix", string.Format(ScriptBlock, string.Concat("var prefix = '", txtCardHolderName.ClientID.Replace(txtCardHolderName.ID, string.Empty),"';")));
            
            //Tokenization for added cards
            bool disableTokenization = Settings.GetRequiredAppSetting<bool>("TokenizationDisabled", false);
            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "TokenizationDisabled", string.Format(ScriptBlock, string.Concat("var tokenizationDisabled = ", disableTokenization.ToString().ToLower()), ";"));

            try
            {
                var seamlessCredentials = CookieHandler.GetSeamlessCredentials();
                if (seamlessCredentials != null && seamlessCredentials.AuthenticationToken != null)
                    this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "AuthToken", string.Format(ScriptBlock, string.Concat("var authToken = '",
                        seamlessCredentials.AuthenticationToken.ToString(), "';")));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("CookieHandler::GetSeamlessCredentials() error. Error description: {0}", ex.Message);
                (new HLErrorAuditEvent("CookieHandler::GetSeamlessCredentials()", CookieHandler.GetSeamlessCredentials(), ex)).Raise();
            }

            //this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "NickNamesList", string.Format(ScriptBlock, string.Concat("var nickNamesList = '", GetNickNamesList(), "';")));
            hdNickNameList.Value = GetNickNamesList();
            chkSaveCreditCard.Attributes.Add("onclick ", "CheckSaveBoxSettings(event)");
            chkMakePrimaryCreditCard.Attributes.Add("onclick ", "CheckPrimaryBoxSettings(event)");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            hdTokenTimerEnabled.Value = Settings.GetRequiredAppSetting("TokenizationTimerEnabled", "false");
            switch (_currentMode)
            {
                case PaymentInfoCommandType.Add:
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "NickName", string.Format(ScriptBlock, string.Concat("var originalNick = '", txtNickName.Text, "';")));
                        btnContinue.OnClientClick = "return ValidateNewCardBR(event, this)";
                        this.txtCity.Attributes.Add("onfocus", "blur();");
                        this.txtCity.Attributes.Add("onclick", "blur();");
                        this.txtState.Attributes.Add("onfocus", "blur();");
                        this.txtState.Attributes.Add("onclick", "blur();");
                        this.txtCity.CssClass = this.txtCity.CssClass.Contains("disabled")
                                                    ? this.txtCity.CssClass
                                                    : string.Format("{0} disabled", this.txtCity.CssClass);
                        this.txtState.CssClass = this.txtState.CssClass.Contains("disabled")
                                                     ? this.txtState.CssClass
                                                     : string.Format("{0} disabled", this.txtState.CssClass);
                        this.txtCardNumber.Attributes.Add("onkeydown", "DenyBlankSpace(event);");
                        this.txtCardNumber.Attributes.Add("onblur", "RemoveBlankSpace(this);");
                        break;
                    }
                case PaymentInfoCommandType.Delete:
                    {
                        btnContinue.OnClientClick = "return true;";
                        break;
                    }
                case PaymentInfoCommandType.Edit:
                    {
                        this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "NickName", string.Format(ScriptBlock, string.Concat("var originalNick = '", txtNickName.Text, "';")));
                        this.txtCity.Attributes.Add("onfocus", "blur();");
                        this.txtCity.Attributes.Add("onclick", "blur();");
                        this.txtState.Attributes.Add("onfocus", "blur();");
                        this.txtState.Attributes.Add("onclick", "blur();");
                        this.txtCity.CssClass = this.txtCity.CssClass.Contains("disabled")
                                                    ? this.txtCity.CssClass
                                                    : string.Format("{0} disabled", this.txtCity.CssClass);
                        this.txtState.CssClass = this.txtState.CssClass.Contains("disabled")
                                                     ? this.txtState.CssClass
                                                     : string.Format("{0} disabled", this.txtState.CssClass);
                        btnContinue.OnClientClick = "return ValidateNewCardBR(event, this)";
                        //btnContinue.OnClientClick = "return ValidateNickName(event, this)";
                        break;
                    }
            }

            ccBranding.Attributes.Add("src", "/Ordering/Images/payment/" + ddlCardType.SelectedValue.ToString() + ".jpg");
        }

        protected override void chkSameAsShippingAddress_CheckedChanged(object sender, EventArgs e)
        {
            currentAddress = getShippingAddress();
            if (currentAddress != null)
            {
                
                sameBillingAddress_Zip.Text = currentAddress.Address.PostalCode.Trim();

                sameBillingAddress_Street.Text = sameBillingAddress_Street2.Text = string.Empty;
                
                if (!string.IsNullOrEmpty(currentAddress.Address.Line4))
                {
                    var streets = currentAddress.Address.Line4.Split(new string[] {"%%%"},
                                                                     StringSplitOptions.RemoveEmptyEntries);
                    if (streets.Length > 0)
                    {
                        sameBillingAddress_Street.Text = streets[0];
                        if (streets.Length > 1)
                        {
                            sameBillingAddress_Street2.Text = streets[1];
                        }
                    }
                }

                sameBillingAddress_Number.Text = string.IsNullOrEmpty(currentAddress.Address.Line3)
                                                    ? string.Empty
                                                    : currentAddress.Address.Line3;

                sameBillingAddress_Neighborhood.Text = currentAddress.Address.Line2;

                sameBillingAddress_City.Text = currentAddress.Address.City.Trim();
                sameBillingAddress_State.Text = String.IsNullOrEmpty(currentAddress.Address.StateProvinceTerritory) ? "" : currentAddress.Address.StateProvinceTerritory.Trim();

            }
            else
            {
                lblBillingAddressMessage.Text = SHIPPINGADDRESS_GET_FAILURE_MESSAGE;
            }

            dvBillingAddressLabel.Visible = true;
            dvBillingAddressText.Visible = false;
        }

        protected void txtZip_TextChanged(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;
            if (string.IsNullOrEmpty(txtZip.Text) || txtZip.Text.Length != 8)
            {
                txtStreetAddress.Text = txtStreetAddress2.Text = txtNumber.Text =
                                                                 txtNeighborhood.Text =
                                                                 txtState.Text = txtCity.Text = string.Empty;
                return;
            }
            GetPostalCodeDetails(txtZip.Text);
        }

        private void SetAvailableFields(bool enabled)
        {
            if (enabled)
            {
                txtStreetAddress.Attributes.Remove("onfocus");
                txtStreetAddress.Attributes.Remove("onclick");
                txtNeighborhood.Attributes.Remove("onfocus");
                txtNeighborhood.Attributes.Remove("onclick");
                this.txtStreetAddress.CssClass =
                    this.txtStreetAddress.CssClass.Replace("disabled", string.Empty).Trim();
                this.txtNeighborhood.CssClass =
                    this.txtNeighborhood.CssClass.Replace("disabled", string.Empty).Trim();
            }
            else
            {
                txtStreetAddress.Attributes.Add("onfocus", "blur();");
                txtStreetAddress.Attributes.Add("onclick", "blur();");
                txtNeighborhood.Attributes.Add("onfocus", "blur();");
                txtNeighborhood.Attributes.Add("onclick", "blur();");
                this.txtStreetAddress.CssClass = this.txtStreetAddress.CssClass.Contains("disabled")
                                                     ? this.txtStreetAddress.CssClass
                                                     : string.Format("{0} disabled", this.txtStreetAddress.CssClass);
                this.txtNeighborhood.CssClass = this.txtNeighborhood.CssClass.Contains("disabled")
                                                    ? this.txtNeighborhood.CssClass
                                                    : string.Format("{0} disabled", this.txtNeighborhood.CssClass);
            }
        }

        protected void GetPostalCodeDetails(string postalCode)
        {
            //Reset fields
            this.SetAvailableFields(false);
            txtStreetAddress.Text = txtStreetAddress2.Text = txtNumber.Text =
                                                             txtNeighborhood.Text =
                                                             txtState.Text = txtCity.Text = string.Empty;

            // Validating zip code entries.
            if (!string.IsNullOrEmpty(postalCode))
            {
                string zipCode = postalCode;
                // If user enters 000 as postal code 2.
                if (postalCode.Substring(5).Equals("000"))
                {
                    zipCode = postalCode.Substring(0, 5);
                }
                // Search the address by the provided zip code.
                var shippingProvider = new ShippingProvider_BR();
                var addressResults = shippingProvider.AddressSearch(zipCode);
                if (addressResults != null && addressResults.Count > 0)
                {
                    //lblNoMatch.Visible = false;
                    txtCity.Text = addressResults[0].City;
                    txtState.Text = addressResults[0].StateProvinceTerritory;
                    txtStreetAddress.Text = addressResults[0].Line1;
                    txtNeighborhood.Text = addressResults[0].Line2;
                    // Finally enable the street and neighborhood text boxes.
                    if (postalCode.Substring(5).Equals("000"))
                    {
                        this.SetAvailableFields(true);
                        txtStreetAddress.Text = txtNeighborhood.Text = string.Empty;
                        txtStreetAddress.Focus();
                    }
                    else
                    {
                        txtNumber.Focus();
                    }
                }
                else
                {
                    lblMessage.Text = GetLocalResourceObject("NoZipMatch") as string;
                }
            }
            else
            {
                lblMessage.Text = GetLocalResourceObject("NoZipMatch") as string;
            }
        }
    }
}