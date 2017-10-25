using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class StaticPaymentInfo : UserControlBase
    {
        private Payment _paymentInfo;
        string _locale = string.Empty;
        string _distributorId = string.Empty;

        public Payment CurrentPaymentInfo
        {
            get
            {
                return _paymentInfo;
            }
            set
            {
                _paymentInfo = value;
                fillPaymentInfo();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _locale = (Page as ProductsBase).Locale;
            _distributorId = (Page as ProductsBase).DistributorID;

            if (!IsPostBack)
            {
                fillPaymentInfo();
            }
        }

        protected virtual void fillPaymentInfo()
        {
            try
            {
            pnlAlternatePayment.Visible = false;
            pnlCardPayments.Visible = true;
            trPaymentOptions.Visible = false;
            if (_paymentInfo != null)            
            {
                if (_paymentInfo is WirePayment_V01 || _paymentInfo is DirectDepositPayment_V01 || (_paymentInfo is CreditPayment_V01 && (_paymentInfo as CreditPayment_V01).AuthorizationMethod == AuthorizationMethodType.PaymentGateway && !HLConfigManager.Configurations.PaymentsConfiguration.ShowPaymentInfoForPaymentGatewayInSummary))
                {
                    lblAlternatePaymentMethod.Text = _paymentInfo.TransactionType;
                    lblAlternateAmount.Text = getAmountString(_paymentInfo.Amount, true);

                    if ((this.Page as ProductsBase).CountryCode == "ZA" && (_paymentInfo is CreditPayment_V01 && (_paymentInfo as CreditPayment_V01).AuthorizationMethod == AuthorizationMethodType.PaymentGateway))
                    {
                        lblAlternatePaymentMethod.Text = HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayAlias;
                    }

                    //Defect 82562 for SI adding the prefix to the
                    if ((this.Page as ProductsBase).CountryCode == "SI")
                    {
                            lblReference.Text = "SI00" + (this.Page as ProductsBase).OrderNumber;
                    }
                    else
                    {
                        lblReference.Text = (this.Page as ProductsBase).OrderNumber;
                    }
                    // defect 24438 for MX. please remove 2T from the reference number. 
                    if ((this.Page as ProductsBase).CountryCode == "MX")
                    {
                        if (!String.IsNullOrEmpty(lblReference.Text) && lblReference.Text.Length > 3)
                        {
                            lblReference.Text = lblReference.Text.Substring(2);
                        }
                        this.mxReference.Style.Remove("display");
                        this.lblReferenceMX.Text = _paymentInfo.ReferenceID;
                        //ReferenceNumber and ReferenceID are coming as null.
                        // need n0n-null to display it on the website for MX
                        // defect 24438 - Wire confirmation reference is null
                        //string ref1 = (_paymentInfo as WirePayment_V01).ReferenceNumber;
                        //string ref2 = (_paymentInfo as WirePayment_V01).ReferenceID;
                    }
                    else if ((this.Page as ProductsBase).CountryCode == "VE")
                    {
                        // Print reference for VE only for direct deposit payment method.
                        var ddVE = _paymentInfo as DirectDepositPayment_V01;
                        if (ddVE != null)
                        {
                            this.mxReference.Style.Remove("display");
                            this.lblReferenceMX.Text = ddVE.ReferenceID;
                        }
                    }

                    wireMessage.SectionName = "Ordering";
                    wireMessage.UseLocal = true;
                    if (_paymentInfo is WirePayment_V01)
                    {
                        if ((this.Page as ProductsBase).CountryCode == "TW" && (_paymentInfo as WirePayment_V01).PaymentCode == "W1")
                        {
                            wireMessage.ContentPath = "lblWireMessageWithSMS.html";
                        }
                        else if (_paymentInfo.TransactionType.Trim() == "ICICI Bank iSure Pay (Auto Order Release)")
                        {
                            wireMessage.ContentPath = "lblWireTransferAutoOrderRelease.html";
                        }
                        else if (HLConfigManager.Configurations.PaymentsConfiguration.MultipleWireMessage)
                        {
                            GetWireMessage((_paymentInfo as WirePayment_V01).PaymentCode);
                        }
                        else
                        {
                            wireMessage.ContentPath = "lblWireMessage.html";
                        }
                        wireMessage.LoadContent();
                    }
                        else if (_paymentInfo is DirectDepositPayment_V01)
                    {
                        wireMessage.ContentPath = "lblDirectDepositMessage.html";
                    }
                    pnlAlternatePayment.Visible = true;
                    pnlCardPayments.Visible = false;
                }
                else if (_paymentInfo is CreditPayment_V01)
                {
                    CreditPayment_V01 payment = _paymentInfo as CreditPayment_V01;
                    List<PaymentInformation> CurrentCreditCardInfo = this.GetCurrentPaymentInformation(_locale, _distributorId);
                    string cardNum = null;
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        #region 146947 : china stuffs
                        cardNum = HL.Common.Utilities.CryptographicProvider.Decrypt(payment.Card.AccountNumber.Trim());
                    if (!string.IsNullOrEmpty(cardNum))
                    {
                                cardNum = (cardNum.Length > 4 ? new string('X', cardNum.Length - 4) : "") +
                                          cardNum.Substring(cardNum.Length - 4);
                    }

                        #endregion
                    }
                    else
                    {
                        cardNum = payment.Card.AccountNumber.Trim();
                        if (!string.IsNullOrEmpty(cardNum))
                        {
                            cardNum = "-" + (cardNum.Length > 4 ? cardNum.Substring(cardNum.Length - 4) : cardNum);
                        }
                    }

                    if (CurrentCreditCardInfo != null && CurrentCreditCardInfo.Count > 0 && CurrentCreditCardInfo.Count == 1)
                    {
                        PaymentInformation pi = CurrentCreditCardInfo[0] as PaymentInformation;
                        if (pi != null && pi.CardHolder != null)
                        {
                        lblCardHolderName.Text = pi.CardHolder.First + " " + pi.CardHolder.Middle + " " + pi.CardHolder.Last;
                        lblCardType.Text = pi.CardType;
                    }
                    }
                    else
                    {
                            lblCardHolderName.Text = payment.Card.NameOnCard;
                        lblCardType.Text = getCardName(payment);
                    }

                    lblCardNumber.Text = cardNum;
                    lblCardExpiration.Text = GetExpirationInfo();
                    lblAmount.Text = getAmountString(_paymentInfo.Amount);
                    if (HLConfigManager.Configurations.PaymentsConfiguration.AddressRequiredForNewCard)
                    {
                        lblBillingAddress.Text = GetFormattedBillingAddress();
                    }
                    else
                    {
                        lblBillingAddress.Visible = false;
                        lblBillingAddressText.Visible = false;
                    }
                    if (null != payment.PaymentOptions && null != payment.PaymentOptions as JapanPaymentOptions_V01)
                    {
                        if ((payment.PaymentOptions as JapanPaymentOptions_V01).ChargeMode != JapanPayOptionType.Unknown)
                        {
                            lblPayOptions.Text = payment.ReferenceID;
                            trPaymentOptions.Visible = true;
                        }
                    }
                }
                else if (_paymentInfo is LocalPayment_V01)
                {
                    var pnm = _paymentInfo as LocalPayment_V01;
                    if (pnm.PaymentCode.Trim() == "PN")
                    {
                        var currentSession = SessionInfo.GetSessionInfo(this.DistributorID, this.Locale);
                        wireMessage.SectionName = "Ordering";
                        wireMessage.UseLocal = true;
                        wireMessage.ContentPath = "lblPayNearMeMessage.html";
                        wireMessage.LoadContent();
                        lblReference.Text = (this.Page as ProductsBase).OrderNumber;
                        lblAlternatePaymentMethod.Text = pnm.TransactionType;
                        lblAlternateAmount.Text = getAmountString(pnm.Amount, true);
                        if (!string.IsNullOrEmpty(currentSession.LocalPaymentId))
                        {
                            this.mxReference.Style.Remove("display");
                            this.divHelpIcon.Style.Remove("display");
                            this.lblReferenceMX.Text = string.IsNullOrEmpty(currentSession.TrackingUrl)
                                                           ? currentSession.LocalPaymentId
                                                           : string.Format("<a href='{0}' target='_blank'>{1}</a>",
                                                                           currentSession.TrackingUrl,
                                                                           currentSession.LocalPaymentId);
                        }
                    }
                    pnlAlternatePayment.Visible = true;
                    pnlCardPayments.Visible = false;
                }
                trAmountText.Visible = HLConfigManager.Configurations.PaymentsConfiguration.ShowPaymentAmountsInSummary;
            }
            if ((this.Page as ProductsBase).CountryCode == "CN")
            {
                trCardHolderName.Visible = false;
                trCardType.Visible = false;
            }
        }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    String.Format(
                        "Messages:{0}, StackTrace:{1}, Locale:{2}, DistributorId:{3},PaymentInfo Payment Date:{4},PaymentInfo Transaction Type: {5}",
                        ex.Message, ex.StackTrace, _locale, _distributorId,
                        _paymentInfo != null
                            ? _paymentInfo.PaymentDate != null
                                ? _paymentInfo.PaymentDate.Value.ToString()
                                : "payment Date empty"
                            : "_paymentInfo is Null",
                        _paymentInfo != null ? _paymentInfo.TransactionType : string.Empty));
            }
        }
        private void GetWireMessage( string Paycode)
        {
            if (Locale == "el-GR")
            {
                if (Paycode == "W2")
                {
                    wireMessage.ContentPath = "lblWireMessage.html";
                }
                else if (Paycode == "W1")
                {
                    wireMessage.ContentPath = "lblWireMessage3.html";
                }
            }
            else
            {
                if (Paycode == "W2")
                {
                    wireMessage.ContentPath = "lblWireMessage3.html";
                }
                else if (Paycode == "W3")
                {
                    wireMessage.ContentPath = "lblWireMessage2.html";
                }
                else
                {
                    wireMessage.ContentPath = "lblWireMessage.html";
                }
            }
        }
		protected string GetExpirationInfo()
		{
			string exp = string.Empty;
            if (HLConfigManager.Configurations.PaymentsConfiguration.ShowExpirationDateInPaymentSummary)
            {
                lblCardExpirationText.Visible = true;
                if ((_paymentInfo as CreditPayment_V01).Card.IssuerAssociation != IssuerAssociationType.MyKey)
                {
                    exp = string.Concat("", GetLocalResourceObject("Expires"), (_paymentInfo as CreditPayment_V01).Card.Expiration.ToString("MM-yyyy"));
                }
            }
            else
            {
                lblCardExpirationText.Visible = false;
            }
			return exp;
		}



        
        //private string GetCardName(IssuerAssociationType issuer)
        //{
        //    string card = issuer.ToString();
        //    switch (issuer)
        //    {
        //        case IssuerAssociationType.APlus:
        //        {
        //            card = GetLocalResourceObject("PayByAPlus") as string;
        //            break;
        //        }
        //        case IssuerAssociationType.MyKey:
        //        {
        //            card = GetLocalResourceObject("PayByMyKey") as string;
        //            break;
        //        }
        //    }

        //    return card;
        //}

        /// <summary>Mask the card number for display</summary>
        /// <param name="cardNum">The card number</param>
        /// <returns>The masked value</returns>
        protected string getCardName(CreditPayment_V01 payment)
        {
            if (payment is KoreaISPPayment_V01 || payment is KoreaMPIPayment_V01)
            {
                return payment.Card.IssuingBankID;
            }

            string cardTypeAbbrev = CreditCard.CardTypeToHPSCardType(payment.Card.IssuerAssociation);
            string cardName = cardTypeAbbrev;
            if (!string.IsNullOrEmpty(cardName))
            {
                cardName = GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform), string.Format("CardType_{0}_Description", cardTypeAbbrev)) as string;
            }

            return cardName;
        }

        protected string GetFormattedBillingAddress()
        {
            string formattedAddress = string.Empty;
            var address = _paymentInfo.Address;
            if (!string.IsNullOrEmpty(address.Line1) && !string.IsNullOrEmpty(address.City))
            {
                formattedAddress = string.Concat(address.Line1.Trim(), "<br>", address.City, ",  ", address.StateProvinceTerritory, "&nbsp;", address.PostalCode);
            }

            return formattedAddress;
        }

        public List<PaymentInformation> GetCurrentPaymentInformation(string locale, string distributorID)
        {
            string key = PaymentsConfiguration.GetPaymentInfoSessionKey(distributorID, locale);
            return Session[key] as List<PaymentInformation>;
        }

    }
}
