using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using BankInformation = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.BankInformation;
using BankUsage = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.BankUsage;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoGrid_CN : PaymentInfoGrid
    {
        #region Enums

        protected enum PaymentOptionChoiceOverride
        {
            None,
            CreditCard,
            PaymentGateway,
            WireTransfer,
            DirectDeposit,
            Bill99,
            QuickPay,
        }

        #endregion Enums

        [Publishes(MyHLEventTypes.PaymentOptionsViewChanged)]
        public event EventHandler onPaymentOptionsViewChanged;
        private bool _requiresAcknowledgementToSubmit;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _requiresAcknowledgementToSubmit = HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmit;

        }

        public override void LoadCards()
        {
            if (!IsPostBack)
            {
                //SetCurrentPaymentInformation(null, _locale, _distributorId);
                CheckForPaymentGatewayResponse();
                lblCardsLimitMessage.Visible = false;

                List<PaymentInformation> currentPayment = GetCurrentPaymentInformation(this.Locale, this.DistributorID);
                if (null == _CurrentPaymentsList || _CurrentPaymentsList.Count == 0)
                {
                    _CurrentPaymentsList =
                        (from p in currentPayment where p.IsPrimary select p).ToList();
                }
                SetCurrentPaymentInformation(_CurrentPaymentsList, _locale, _distributorId);
                gridViewCardInfo.DataSource = GetCurrentCardsList(null);
                gridViewCardInfo.DataBind();

                // Set mobile resources
                LoadInfoMobile();
            }

            var totals = GetTotals();
            if (null != totals)
            {
                totalWireDue.Text = DisplayAsCurrency(totals.AmountDue, true);
                totalWireDue.Visible = false;
                totalDirectDepositDue.Text = totalWireDue.Text;
                totalPaymentGatewayDue.Text = totalWireDue.Text;
                txtGrandTotal.Text = totalWireDue.Text;
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), "TotalCreditDue",
                                                            string.Format(ScriptBlock,
                                                                          string.Concat("var amount = '",
                                                                                        string.Format("{0:F2}",
                                                                                                      totals.AmountDue),
                                                                                        "';")));

                txtAmountMob.Attributes.Add("readonly", "readonly");
                txtAmountMob.Text = DisplayAsCurrency(totals.AmountDue, false);

            }
        }

        protected override void GetPaymentsListFromProvider()
        {
            List<PaymentInformation> currentPayment = GetCurrentPaymentInformation(this.Locale, this.DistributorID);
            if (currentPayment == null || currentPayment.Count == 0)
            {
                _PaymentsList = new List<PaymentInformation>();
                _PaymentsList.Add(new PaymentInformation { IssueNumber = "bank", ID = 1, IsPrimary = true });
                SetCurrentPaymentInformation(_PaymentsList, Locale, DistributorID);
            }
        }

        protected override List<PaymentInformation> GetCurrentCardsList(PaymentInformation currentPaymentSelection)
        {
            var cards = new List<PaymentInformation>();
            if (null == _CurrentPaymentsList)
            {
                _CurrentPaymentsList = GetCurrentPaymentInformation(_locale, _distributorId);
                if (null == _CurrentPaymentsList)
                {
                    if (null != _PaymentsList && _PaymentsList.Count > 0)
                    {
                        _CurrentPaymentsList =
                            (from p in _PaymentsList where p.IsPrimary orderby p.Alias select p).ToList();
                    }
                }
            }


            int cardCount = (null != _CurrentPaymentsList) ? _CurrentPaymentsList.Count : 0;
            int maxCards = 1; // (_PaymentsList.Count > _maxCardsToDisplay) ? _maxCardsToDisplay : _PaymentsList.Count;
            if (maxCards > 0 && null != _CurrentPaymentsList && _CurrentPaymentsList.Count > 0)
            {
                cards = new List<PaymentInformation>(_CurrentPaymentsList);
                for (int i = cardCount; i < maxCards; i++)
                {
                    cards.Add(new PaymentInformation());
                }
            }
            return cards;
        }

        protected override void SetupGridRow(GridViewRow row, bool makeActive)
        {
            var ctl = row.FindControl("txtIssueNumber");
            if (null != ctl)
            {
                ctl.Visible = false;
            }
        }

        /// <summary>Even hook for new grid row - used to configure the Payment Options</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected new void gridViewCardInfo_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            switch (e.Row.RowType)
            {
                case DataControlRowType.DataRow:
                    {
                        var cardId = e.Row.FindControl("cardID") as TextBox;
                        var cvv = e.Row.FindControl("txtCVV") as TextBox;
                        var banks = e.Row.FindControl("ddlCards") as DropDownList;


                        if (null != banks)
                        {
                            var pi = e.Row.DataItem as PaymentInformation;

                            var currentCards = GetCurrentPaymentInformation(_locale, _distributorId);
                            var currentIds = (null != currentCards)
                                                 ? (from p in currentCards select p.ID).ToList()
                                                 : new List<int>();
                            List<BankInformation> bankList = BankInfoProvider.GetAvailableBanks(BankUsage.UsedByCNP);
                            if (bankList != null)
                            {
                                banks.DataSource = from b in bankList
                                                   select new { DisplayName = b.BankName, ID = b.BankCode };
                                banks.DataBind();

                                ddlBankdsMob.DataSource = banks.DataSource;
                                ddlBankdsMob.DataBind();
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        /// <summary>
        /// override get current payment option
        /// </summary>
        /// <returns></returns>
        protected override PaymentOptionChoice GetCurrentPaymentOption()
        {
            switch (rblPaymentOptions.SelectedIndex)
            {
                case 2:
                    {
                        return PaymentOptionChoice.WireTransfer;
                    }
                default:
                    return PaymentOptionChoice.PaymentGateway;
            }

        }

        protected PaymentOptionChoiceOverride GetCurrentPaymentOptionOverride()
        {
            var choice = PaymentOptionChoiceOverride.None;
            switch (rblPaymentOptions.SelectedIndex)
            {
                case 0:
                    {
                        choice = PaymentOptionChoiceOverride.QuickPay;
                        break;
                    }
                case 1:
                    {
                        choice = PaymentOptionChoiceOverride.PaymentGateway;
                        break;
                    }
                case 2:
                    {
                        choice = PaymentOptionChoiceOverride.WireTransfer;
                        break;
                    }
                case 3:
                    {
                        choice = PaymentOptionChoiceOverride.DirectDeposit;
                        break;
                    }
                case 4:
                    {
                        choice = PaymentOptionChoiceOverride.Bill99;
                        break;
                    }
                case 5:
                    {
                        choice = PaymentOptionChoiceOverride.CreditCard;
                        break;
                    }
            }

            return choice;
        }

        public override List<Payment> GetPayments(Address_V01 shippingAddress)
        {
            var paymentList = new List<Payment>();
            CheckForPaymentGatewayResponse();
            var choice = GetCurrentPaymentOptionOverride();
            switch (choice)
            {
                case PaymentOptionChoiceOverride.WireTransfer: // Pay by Phone
                    {
                        paymentList.Add(CreateDummyPayment(shippingAddress));
                        (paymentList.First()).TransactionType = "DB";
                        break;
                    }
                case PaymentOptionChoiceOverride.PaymentGateway: // eBanking
                    {
                        paymentList.Add(CreateDummyPayment(shippingAddress));
                        (paymentList.First()).TransactionType = "10";
                        break;
                    }
                case PaymentOptionChoiceOverride.Bill99: // 99 bill
                    {
                        paymentList.Add(CreateDummyPayment(shippingAddress));
                        (paymentList.First()).TransactionType = "12";
                        Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                        Session.Add(PaymentGatewayInvoker.PaymentInformation, paymentList.First());
                        break;
                    }
                case PaymentOptionChoiceOverride.CreditCard: // CNP 
                    {
                        var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);

                        // Check mobile controls first
                        if (!string.IsNullOrEmpty(txtCardNumberMob.Text) && ddlBankdsMob.SelectedItem != null &&
                            !string.IsNullOrEmpty(txtExpMonthMob.Text) && !string.IsNullOrEmpty(txtExpYearMob.Text) &&
                            !string.IsNullOrEmpty(txtCVVMob.Text))
                        {
                            decimal amount = 0;
                            int cardMobileId = 0;
                            int.TryParse(txtCardIdMob.Text, out cardMobileId);
                            if (decimal.TryParse(txtAmountMob.Text, out amount) && cardMobileId > 0)
                            {
                                var info = (paymentInfo != null && paymentInfo.Count > 0) ? paymentInfo[0] : new PaymentInformation();
                                info.CardHolder = new Name_V01();
                                var payment = new CreditPayment_V01();
                                info.Amount = amount;
                                payment.Card = new CreditCard();
                                payment.AuthorizationMethod = AuthorizationMethodType.Online;
                                payment.Card.IssuerAssociation = IssuerAssociationType.None;
                                payment.Amount = amount;
                                payment.Address = shippingAddress;
                                info.CardNumber = payment.Card.AccountNumber = CryptographicProvider.Encrypt(txtCardNumberMob.Text.Trim());
                                payment.Card.CVV = CryptographicProvider.Encrypt(txtCVVMob.Text.Trim());
                                payment.AuthorizationCode = string.Empty;
                                info.Expiration = payment.Card.Expiration = getExpDate(txtExpMonthMob.Text, txtExpYearMob.Text);
                                info.AuthorizationFailures = 0;
                                if (isExpires(info.Expiration))
                                {
                                    //info.AuthorizationFailures = 3;
                                    imgDeclinedMob.ImageUrl = GetStatusImageUrl(info.ID);
                                    imgDeclinedMob.ToolTip = GetStatusImageToolTip(info.ID);
                                    imgDeclinedMob.Visible = true;
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCard");
                                }
                                payment.Card.NameOnCard = string.Empty;

                                payment.AuthorizationMerchantAccount = ddlBankdsMob.SelectedItem != null
                                                                 ? ddlBankdsMob.SelectedValue
                                                                 : string.Empty;
                                payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                                payment.Address = (null != info.BillingAddress) ? info.BillingAddress : shippingAddress;
                                payment.PaymentOptions = new PaymentOptions_V01 { NumberOfInstallments = 1 };
                                info.Options = payment.PaymentOptions;
                                payment.TransactionType = "CC";

                                paymentList.Add(payment);
                                Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                                Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);
                                SetCurrentPaymentInformation(paymentInfo, _locale, _distributorId);
                            }
                            break;
                        }

                        foreach (GridViewRow row in gridViewCardInfo.Rows)
                        {

                            var txtCVV = row.FindControl("txtCVV") as TextBox;
                            var txtAmount = row.FindControl("txtAmount") as TextBox;
                            var txtCardNumber = row.FindControl("txtCardNumber") as TextBox;
                            var txtExpMonth = row.FindControl("txtExpMonth") as TextBox;
                            var txtExpYear = row.FindControl("txtExpYear") as TextBox;
                            var id = row.FindControl("cardID") as TextBox;
                            var currentOptions = row.FindControl("lnkPaymentOptions") as LinkButton;
                            var banks = row.FindControl("ddlCards") as DropDownList; // list of banks

                            int cardID = int.Parse(id.Text);
                            decimal cardAmount;
                            if (decimal.TryParse(txtAmount.Text, out cardAmount) && cardID > 0)
                            {
                                var info = (paymentInfo != null && paymentInfo.Count > 0) ? paymentInfo[0] : new PaymentInformation();
                                info.CardHolder = new Name_V01();
                                var payment = new CreditPayment_V01();
                                info.Amount = cardAmount;
                                payment.Card = new CreditCard();
                                payment.AuthorizationMethod = AuthorizationMethodType.Online;
                                payment.Card.IssuerAssociation = IssuerAssociationType.None;
                                payment.Amount = cardAmount;
                                payment.Address = shippingAddress;
                                info.CardNumber = payment.Card.AccountNumber = CryptographicProvider.Encrypt(txtCardNumber.Text.Trim());
                                payment.Card.CVV = CryptographicProvider.Encrypt(txtCVV.Text.Trim());
                                payment.AuthorizationCode = string.Empty;
                                info.Expiration = payment.Card.Expiration = getExpDate(txtExpMonth.Text, txtExpYear.Text);
                                info.AuthorizationFailures = 0;
                                if (isExpires(info.Expiration))
                                {
                                    //info.AuthorizationFailures = 3;
                                    var img = row.FindControl("imgDeclined") as Image;
                                    if (null != img)
                                    {
                                        img.ImageUrl = GetStatusImageUrl(info.ID);
                                        img.ToolTip = GetStatusImageToolTip(info.ID);
                                        img.Visible = true;
                                    }
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCard");
                                }
                                payment.Card.NameOnCard = string.Empty;

                                payment.AuthorizationMerchantAccount = banks.SelectedItem != null
                                                                 ? banks.SelectedValue
                                                                 : string.Empty;
                                payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                                payment.Address = (null != info.BillingAddress) ? info.BillingAddress : shippingAddress;
                                payment.PaymentOptions = new PaymentOptions_V01 { NumberOfInstallments = 1 };
                                info.Options = payment.PaymentOptions;
                                payment.TransactionType = "CC";
                                SetPaymentOptions(info, row);
                                payment.ReferenceID = currentOptions.Text;
                                paymentList.Add(payment);
                                Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                                Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);
                                SetCurrentPaymentInformation(paymentInfo, _locale, _distributorId);
                            }
                        }
                        break;
                    }
                case PaymentOptionChoiceOverride.QuickPay: // Quick Pay 
                    {
                        var payment = new CreditPayment_V01();

                        payment.TransactionType = "QP";
                        payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                        payment.AuthorizationMerchantAccount = BankList_QuickPay.SelectedValue;

                        var card = new QuickPayPayment();

                        if (IsNewQuickPayRegistration())
                        {
                            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                            var name = string.Empty;
                            if (membershipUser != null)
                            {
                                name = membershipUser.Value.DistributorName();
                            }

                            if (!string.IsNullOrEmpty(CardCVV_QuickPay.Text))
                                card.CVV = CryptographicProvider.Encrypt(CardCVV_QuickPay.Text);

                            card.AccountNumber = CryptographicProvider.Encrypt(CardNumber_QuickPay.Text.Trim());
                            card.CardHolderId = IdentityNumber_QuickPay.Text;
                            card.CardHolderType = "0"; //CNID
                            card.NameOnCard = name;
                            card.MobilePhoneNumber = PhoneNumber_QuickPay.Text;

                            int expiredMonth = 0;
                            int expiredYear = 0;
                            DateTime tmpDate = DateTime.Now;

                            if (int.TryParse(CardExpiredDate_Month_QuickPay.Text, out expiredMonth) &&
                                int.TryParse("20" + CardExpiredDate_Year_QuickPay.Text, out expiredYear) &&
                                DateTime.TryParseExact("01/" + CardExpiredDate_Month_QuickPay.Text + "/20" + CardExpiredDate_Year_QuickPay.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out tmpDate))
                            {
                                card.Expiration = new DateTime(expiredYear, expiredMonth, 1);
                            }
                        }
                        else
                        {
                            card.AccountNumber = CryptographicProvider.Encrypt(CardNumberLabel_QuickPay.Text.Trim());
                            card.StorablePAN = card.AccountNumber;
                            card.CardHolderId = IdentityNumberLabel_QuickPay.Text;
                            card.CardHolderType = "0"; //CNID
                            card.MobilePhoneNumber = PhoneNumberLabel_QuickPay.Text;
                        }

                        card.IssuingBankID = BankList_QuickPay.SelectedValue;
                        card.IsDebitCard = IsQuickPayDebitCard();
                        card.BindCard = BindCard_QuickPay.Checked;

                        payment.Card = card;
                        payment.Amount = GetTotals().AmountDue;

                        paymentList.Add(payment);
                        Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                        Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);
                        break;
                    }
            }

            string currentKey = PaymentsConfiguration.GetCurrentPaymentSessionKey(_locale, _distributorId);
            Session[currentKey] = paymentList;
            return paymentList;
        }

        private DateTime getExpDate(string txtExpMonth, string txtExpYear)
        {
            int y;
            int m;

            if (!string.IsNullOrEmpty(txtExpMonth) && !string.IsNullOrEmpty(txtExpYear) && Int32.TryParse(txtExpMonth, out m) &&
                Int32.TryParse(txtExpYear, out y))
            {
                if ((y >= DateTime.MinValue.Year && y <= DateTime.MaxValue.Year) && (m >= DateTime.MinValue.Month && m <= DateTime.MaxValue.Month))
                {
                    var exp = new DateTime(2000 + y, m, 1);
                    return DateTimeUtils.GetLastDayOfMonth(exp);
                }
            }
            return DateTime.MinValue;

        }

        protected override void SetupGridRows()
        {
            base.SetupGridRows();
            if (!IsPostBack)
            {
                var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);
                if (paymentInfo != null && paymentInfo.Count > 0)
                {
                    foreach (GridViewRow row in gridViewCardInfo.Rows)
                    {
                        var txtCardNumber = row.FindControl("txtCardNumber") as TextBox;
                        if (txtCardNumber != null)
                        {
                            paymentInfo[0].CardNumber = txtCardNumber.Text;
                        }
                        var txtExpMonth = row.FindControl("txtExpMonth") as TextBox;
                        var txtExpYear = row.FindControl("txtExpYear") as TextBox;
                        if (txtExpMonth != null && txtExpYear != null && !string.IsNullOrEmpty(txtExpMonth.Text) && !string.IsNullOrEmpty(txtExpYear.Text))
                        {

                            paymentInfo[0].Expiration = getExpDate(txtExpMonth.Text, txtExpYear.Text);
                        }
                    }
                }
            }
        }

        protected bool checkTotals(List<Payment> currentPayments, bool showErrors)
        {
            bool isValid = true;
            decimal total = (from c in currentPayments select c.Amount).Sum();
            var orderTotals = GetTotals();
            if (Convert.ToDecimal(total.ToString("0.00")) != Convert.ToDecimal(orderTotals.AmountDue.ToString("0.00")))
            {
                isValid = false;
                if (total > 0)
                {
                    if (showErrors)
                    {
                        lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                          "TotalNotMatch");
                    }
                    totalAmountBalance.Text = DisplayAsCurrency(orderTotals.AmountDue - total, false);
                }
                else
                {
                    if (showErrors)
                    {
                        lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                          "NoPaymentInfo");
                    }
                }
            }

            return isValid;
        }

        public override bool ValidateAndGetPayments(Address_V01 shippingAddress,
                                                   out List<Payment> payments,
                                                   bool showErrors)
        {
            payments = new List<Payment>();
            bool isValid = (chkAcknowledgeTransaction.Visible) ? chkAcknowledgeTransaction.Checked : true;
            if (!isValid)
            {
                return false;
            }

            var currentPayments = GetPayments(shippingAddress);

            var currentChoice = GetCurrentPaymentOptionOverride();
            if (currentChoice == PaymentOptionChoiceOverride.CreditCard)
            {
                if (currentPayments.Count == 0)
                {
                    isValid = false;
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo");
                }
                else if ((from c in currentPayments
                          where
                              null != c as CreditPayment_V01 &&

                              (c as CreditPayment_V01).Card.Expiration < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                          select c).Any())
                {
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                      "CardExpiredForSavedCard");
                    isValid = false;
                }
                else if ((from c in currentPayments
                          where
                              null != c as CreditPayment_V01 &&

                              string.IsNullOrEmpty((c as CreditPayment_V01).Card.CVV)
                          select c).Any())
                {
                    isValid = false;
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CVVMissing");
                }
                else
                {
                    isValid = true;
                }

                if (isValid)
                {
                    isValid = checkTotals(currentPayments, showErrors);
                }
            }
            else if (currentChoice == PaymentOptionChoiceOverride.Bill99)
            {
                isValid = checkTotals(currentPayments, showErrors);
            }
            else if (currentChoice == PaymentOptionChoiceOverride.PaymentGateway && null != _paymentGatewayControl)
            {
                string errorMessage = string.Empty;
                isValid = _paymentGatewayControl.Validate(out errorMessage);
                lblPaymentGatewayErrorMessage.Text = errorMessage;
                if (isValid)
                {
                    currentPayments = new List<Payment> { _paymentGatewayControl.GetPaymentInfo() };
                }
                if (currentPayments.Count == 1)
                {
                    currentPayments[0].Amount = GetTotals().AmountDue;
                }
            }
            else if (currentChoice == PaymentOptionChoiceOverride.QuickPay)
            {
                isValid = checkTotals(currentPayments, showErrors);

                if (isValid)
                {
                    if (currentPayments.Count == 0)
                    {
                        isValid = false;
                        lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo");
                    }
                }

                if (isValid)
                {
                    if (IsNewQuickPayRegistration())
                    {
                        if (IsQuickPayDebitCard())
                        {
                            if (string.IsNullOrEmpty(CardNumber_QuickPay.Text) || CardNumber_QuickPay.Text.Length != 19)
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCreditCardNumber");
                                isValid = false;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(CardNumber_QuickPay.Text) || CardNumber_QuickPay.Text.Length != 16)
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCreditCardNumber");
                                isValid = false;
                            }

                            if (string.IsNullOrEmpty(CardExpiredDate_Month_QuickPay.Text))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCardExpiredDate");
                                isValid = false;
                            }

                            if (string.IsNullOrEmpty(CardExpiredDate_Year_QuickPay.Text))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCardExpiredDate");
                                isValid = false;
                            }

                            DateTime tmpDate = DateTime.Now;

                            if (!DateTime.TryParseExact("01/" + CardExpiredDate_Month_QuickPay.Text + "/20" + CardExpiredDate_Year_QuickPay.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out tmpDate))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCardExpiredDate");
                                isValid = false;
                            }

                            if (string.IsNullOrEmpty(CardCVV_QuickPay.Text))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CVVMissing");
                                isValid = false;
                            }
                            else if (CardCVV_QuickPay.Text.Length != 3)
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCVV");
                                isValid = false;
                            }
                        }

                        if (string.IsNullOrEmpty(PhoneNumber_QuickPay.Text))
                        {
                            lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "EmptyPhoneNumber");
                            isValid = false;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(PhoneNumberLabel_QuickPay.Text))
                        {
                            lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "EmptyPhoneNumber");
                            isValid = false;
                    }
                }
                }

                if (isValid && currentPayments != null && currentPayments.Count > 0)
                {
                    var payment = currentPayments.First() as CreditPayment_V01;

                    if (payment != null)
                    {
                        if (IsNewQuickPayRegistration() && !IsQuickPayDebitCard())
                        {
                            if (payment.Card.Expiration < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCard");
                                isValid = false;
                            }
                        }
                    }
                }
            }

            if (isValid)
            {
                payments = currentPayments;
            }

            _paymentError = !isValid;

            if (_paymentError)
            {
                OnCardAddOrEdit(null, null);
            }
            UpdatePanel1.Update();

            return isValid;
        }

        protected override void SetupPaymentMethodDisplay()
        {
            base.SetupPaymentMethodDisplay();

            if (!DistributorOrderingProfile.IsPayByPhoneEnabled || SessionInfo.IsEventTicketMode)
            {
                rblPaymentOptions.Items[2].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), "display:none");
            }

            PaymentsConfiguration _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;

            if (!_paymentsConfig.AllowQuickPayPayment)
            {
                rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), "display:none");
                rblPaymentOptions.Items[1].Selected = true;
                rblPaymentOptions.SelectedIndex = 1;
                pnlQuickPayTable.Visible = false;
                pnlPaymentGatewayTable.Visible = true;
            }

            if (!_paymentsConfig.AllowCNPPayment)
            {
                rblPaymentOptions.Items[5].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), "display:none");
                pnlCreditTable.Visible = false;
            }
            if (!_paymentsConfig.AllowDirectDepositPayment)
            {
                rblPaymentOptions.Items[4].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), "display:none");
                pnlDirectDepositTable.Visible = false;
            }
        }

        private void LoadInfoMobile()
        {
            ltBankNameMob.Text = GetLocalResourceObject("hdrNickName.HeaderText") as string;
            ltCarNumberMob.Text = GetLocalResourceObject("hdrCardNumber.HeaderText") as string;
            ltExpiration.Text = GetLocalResourceObject("hdrExpiration.HeaderText") as string;
            ltAmountMob.Text = GetLocalResourceObject("Amount.HeaderText") as string;

            var currentCards = GetCurrentPaymentInformation(_locale, _distributorId);
            var currentIds = (null != currentCards)
                                 ? (from p in currentCards select p.ID).ToList()
                                 : new List<int>();
            List<BankInformation> bankList = BankInfoProvider.GetAvailableBanks(BankUsage.UsedByCNP);
            if (bankList != null)
            {
                ddlBankdsMob.DataSource = from b in bankList
                                          select new { DisplayName = b.BankName, ID = b.BankCode };
                ddlBankdsMob.DataBind();
            }
            txtCardIdMob.Text = currentCards != null ? currentCards.Select(i => i.ID.ToString()).FirstOrDefault() : string.Empty;
        }

        /// <summary>Sets the selected Payment Method view</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void SelectCurrentPaymentView(object sender, EventArgs e)
        {
            var rb = sender as RadioButtonList;
            if (null != rb)
            {
                if (_requiresAcknowledgementToSubmit &&
                    HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmitWireOnly)
                {
                    pnlAcknowledCheckContent.Visible = rb.SelectedValue.StartsWith("3");
                }
                else if (_requiresAcknowledgementToSubmit &&
                         HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmitGatewayOnly)
                {
                    pnlAcknowledCheckContent.Visible = rb.SelectedValue.StartsWith("2");
                }
                // Acknowledge check box reset
                chkAcknowledgeTransaction.Checked = !pnlAcknowledCheckContent.Visible ? true : false;
                SetAcknowledgeStyle();

                if (chkAcknowledgeTerms != null)
                {
                    chkAcknowledgeTerms.Checked = !pnlAcknowledgeCheckTerms.Visible ? true : false;
                    SetAcknowledgeTermsStyle();
                }

                if (IsChina)
                {
                    //mpPaymentOptions.SelectedIndex = rb.SelectedIndex;
                    //pnlBottomMessages.Visible = mpPaymentOptions.SelectedIndex == 0;
                }
                else
                {
                    mpPaymentOptions.SelectedIndex = rb.SelectedIndex;
                    pnlBottomMessages.Visible = mpPaymentOptions.SelectedIndex == 0;
                }

                int selectedType = 0;
                this.SessionInfo.SelectedPaymentMethod = int.TryParse(rb.SelectedValue, out selectedType) ? selectedType : 0;
                onPaymentOptionsViewChanged(this, e);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var attr = rblPaymentOptions.Items[rblPaymentOptions.SelectedIndex].Attributes["href"];
            foreach (var mypanel in pnlPaymentOptions.Controls)
            {

                if (mypanel.GetType().Name == "Panel")
                {
                    var _panel = (Panel)mypanel;

                    if (_panel.ID == "pnlAcknowledCheckContent")
                    {
                        continue;
                    }

                    if (_panel.ID != attr)
                    {
                        _panel.Attributes["style"] = "display: none;";
                    }
                    else
                    {
                        _panel.Attributes["style"] = "display: block;";
                        _panel.CssClass = "tab active_tab";
                    }
                }
            }

            UpdateQuickPayControls();
        }

        // refer from: ~\Ordering\Controls\Payments\PaymentInfoGrid_JM.ascx.cs 
        #region Client Events
        // ***********************************************************************************************
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCardBankdsMobSelected(object sender, EventArgs e)
        {
            txtCardNumberMob.Focus();
        }

        #endregion Client Events

        protected void btnCreditCard_QuickPay_Click(object sender, EventArgs e)
        {
            ClearQuickPayCardInput();
            ClearQuickPayCardLabel();

            ExistingCard.Visible = false;
            NewCardBinding.Visible = false;
            BankList_QuickPay.SelectedIndex = -1;

            btnDebitCard_QuickPay.CssClass = "actionButton";
            btnCreditCard_QuickPay.CssClass = "selectedActionButton";

            LoadQuickPayBankList();

            lblCardNumber_QuickPay.Text = (string)GetLocalResourceObject("CreditCardNumber");
            btnUseAnotherCard_QuickPay.Text = (string)GetLocalResourceObject("ChangeCreditCard");
        }

        protected void btnDebitCard_QuickPay_Click(object sender, EventArgs e)
        {
            ClearQuickPayCardInput();
            ClearQuickPayCardLabel();

            ExistingCard.Visible = false;
            NewCardBinding.Visible = false;
            BankList_QuickPay.SelectedIndex = -1;

            btnDebitCard_QuickPay.CssClass = "selectedActionButton";
            btnCreditCard_QuickPay.CssClass = "actionButton";

            LoadQuickPayBankList();

            lblCardNumber_QuickPay.Text = (string)GetLocalResourceObject("DebitCardNumber");
            btnUseAnotherCard_QuickPay.Text = (string)GetLocalResourceObject("ChangeDebitCard");
        }

        protected void btnUseAnotherCard_QuickPay_Click(object sender, EventArgs e)
        {
            var phoneNumber = DistributorOrderingProfileProvider.GetPhoneNumberForCN(_distributorId).Trim();
            var tins = DistributorOrderingProfileProvider.GetTinList(_distributorId, true);
            var tin = tins.Find(t => t.ID == "CNID");

            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var name = string.Empty;
            if (membershipUser != null)
            {
                name = membershipUser.Value.DistributorName();
            }

            ExistingCard.Visible = false;
            NewCardBinding.Visible = true;

            ClearQuickPayCardInput();

            CardHolderName_QuickPay.Text = name;
            IdentityNumber_QuickPay.Text = tin == null ? string.Empty : tin.IDType.Key.Trim();
            PhoneNumber_QuickPay.Text = phoneNumber;
        }

        protected void OnBankList_QuickPaySelected(object sender, EventArgs e)
        {
            if (!Page.Request.Params.Get("__EVENTTARGET").EndsWith("BankList_QuickPay")) //workaround for the dynamic control loading that causing OnSelectedIndexChanged event triggered during postback.
                return;

            string cardType = "";

            if (BankList_QuickPay.SelectedIndex > 0)
            {
                if (IsQuickPayDebitCard())
                {
                    cardType = "0002";
                }
                else
                {
                    cardType = "0001";
                }

                var phoneNumber = DistributorOrderingProfileProvider.GetPhoneNumberForCN(_distributorId).Trim();
                var tins = DistributorOrderingProfileProvider.GetTinList(_distributorId, true);
                var tin = tins.Find(t => t.ID == "CNID");

                var disId = _distributorId;
                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var name = string.Empty;
                if (membershipUser != null)
                {
                    name = membershipUser.Value.DistributorName();
                }

                var quickPayProvider = new CN_99BillQuickPayProvider();
                var isCardAvailable = quickPayProvider.CheckBindedCard(BankList_QuickPay.SelectedValue, cardType, phoneNumber);

                if (isCardAvailable)
                {
                    ExistingCard.Visible = true;
                    NewCardBinding.Visible = false;

                    CardNumberLabel_QuickPay.Text = quickPayProvider.StorablePAN;
                    CardHolderNameLabel_QuickPay.Text = name;
                    IdentityNumberLabel_QuickPay.Text = tin == null ? string.Empty : tin.IDType.Key.Trim();
                    PhoneNumberLabel_QuickPay.Text = phoneNumber;
                }
                else
                {
                    if (!string.IsNullOrEmpty(quickPayProvider.LastErrorMessage))
                        lblErrorMessages.Text = quickPayProvider.LastErrorMessage;

                    ExistingCard.Visible = false;
                    NewCardBinding.Visible = true;

                    ClearQuickPayCardInput();

                    CardHolderName_QuickPay.Text = name;
                    IdentityNumber_QuickPay.Text = tin == null ? string.Empty : tin.IDType.Key.Trim();
                    PhoneNumber_QuickPay.Text = phoneNumber;
                }
            }
            else
            {
                ExistingCard.Visible = false;
                NewCardBinding.Visible = true;

                ClearQuickPayCardInput();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                LoadQuickPayBankList();
        }

        private void LoadQuickPayBankList()
        {
            var usage = IsQuickPayDebitCard() ? BankUsage.UsedByQPDebit : BankUsage.UsedByQPCredit;

            var banks = BankInfoProvider.GetAvailableBanks(usage);

            if (banks != null)
            {
                var data = (from b in banks
                            select new { DisplayName = b.BankName, ID = b.BankCode }).ToList();

                var textSelect = (string)GetLocalResourceObject("TextSelect");
                data.Insert(0, new { DisplayName = textSelect, ID = "0" });
                BankList_QuickPay.DataSource = data;
                BankList_QuickPay.DataBind();

                BankList_QuickPay.SelectedIndex = -1;
            }
        }

        private void UpdateQuickPayControls()
        {
            if (btnDebitCard_QuickPay.CssClass == "selectedActionButton")
            {
                CardExpiredDate_Month_QuickPay.Enabled = false;
                CardExpiredDate_Year_QuickPay.Enabled = false;
                CardCVV_QuickPay.Enabled = false;
                CardNumber_QuickPay.MaxLength = 19;
                CardNumber_QuickPay.CssClass = "longer";
            }
            else
            {
                CardExpiredDate_Month_QuickPay.Enabled = true;
                CardExpiredDate_Year_QuickPay.Enabled = true;
                CardCVV_QuickPay.Enabled = true;
                CardNumber_QuickPay.MaxLength = 16;
                CardNumber_QuickPay.CssClass = "";
            }
        }

        private void ClearQuickPayCardInput()
        {
            CardNumber_QuickPay.Text = "";
            CardExpiredDate_Month_QuickPay.Text = "";
            CardExpiredDate_Year_QuickPay.Text = "";
            CardCVV_QuickPay.Text = "";
            BindCard_QuickPay.Checked = false;
        }

        private void ClearQuickPayCardLabel()
        {
            CardNumberLabel_QuickPay.Text = "";
            CardHolderNameLabel_QuickPay.Text = "";
            IdentityNumberLabel_QuickPay.Text = "";
            PhoneNumberLabel_QuickPay.Text = "";
            PhoneNumber_QuickPay.Text = "";
            IdentityNumber_QuickPay.Text = "";
            CardHolderName_QuickPay.Text = "";
        }

        private bool IsNewQuickPayRegistration()
        {
            return NewCardBinding.Visible;
        }

        private bool IsQuickPayDebitCard()
        {
            return btnDebitCard_QuickPay.CssClass == "selectedActionButton";
        }

        private bool VerifyQuickPayCard()
        {
            return true;
        }
        [SubscribesTo(MyHLEventTypes.OnStandAloneDonation)]
        public void OnStandAloneDonation(object sender, EventArgs e)
        {
            var cart = (Page as ProductsBase).ShoppingCart;
            if (null != cart)
            {
                if (null != cart.Totals)
                {
                    txtGrandTotal.Text = txtAmountMob.Text = DisplayAsCurrency((cart.Totals as OrderTotals_V01).AmountDue, false);
                }
            }

        }
    }
}