using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.EventHandling;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using CurrencyType = HL.Common.ValueObjects.CurrencyType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    /// <summary>The grid for displaying and managing Payment methods</summary>
    public partial class PaymentInfoGrid_KS : UserControlBase, PaymentInfoBase
    {
        #region Enums

        private enum PaymentOptionChoice
        {
            None,
            CreditCard,
            PaymentGateway,
            WireTransfer,
            DirectDeposit
        }

        #endregion

        #region Constants

        private const string StyleHidden = "display:none";
        private const string ScriptBlock = "<script type='text/javascript'>{0}</script>";
        private const string SubmitResponseScript = "<script type=\"text/javascript\">document.setReturnedValues('xid', 'eci', 'cavv', '', '', 'cardnumber');}</script>";
        private const string ValidationScriptMessages = "<script type='text/javascript'>    	var CardholderNameRequired = '{0}';    	var limitedOptionsMessage = '{1}';    	var installmentMessage = '{2}';    	var paymentAuthFailedMessage = '{3}';    	var AddCardQuestion = '{4}';    	var DeleteCardQuestion = '{5}';     var CardHasExpired = '{6}';</script>";

        #endregion

        #region Fields

        private readonly List<string> _Errors = new List<string>();
        private List<CreditPayment> _CurrentPaymentsList;
        private bool _allowDecimal;
        private bool _allowDirectDepositPayment;
        private bool _allowWireForHardCash;
        private bool _cashOnly;
        private bool _hardCash;
        private bool _allowCreditCardForHardCash;
        private bool _allowMultipleCardsInTransaction;
        private bool _allowWirePayment;
        private string _currencySymbol = string.Empty;
        private PaymentOptionChoice _currentOption = PaymentOptionChoice.None;
        private string _distributorId = string.Empty;
        private string _locale = string.Empty;
        private int _maxCardsToDisplay;
        private OrderTotals_V01 _orderTotals;
        private bool _paymentError;
        private PaymentsConfiguration _paymentsConfig;
        protected PaymentGatewayControl _paymentGatewayControl;

        #endregion

        #region Published Events

        [Publishes(MyHLEventTypes.CreditCardAuthenticationCompleted)]
        public event EventHandler OnCreditCardAuthenticated;

        #endregion

        #region Construction / Initialization

        protected override void OnInit(EventArgs e)
        {
            _locale = (Page as ProductsBase).Locale;
            _distributorId = (Page as ProductsBase).DistributorID;
            _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
            _allowDecimal = _paymentsConfig.AllowDecimal;
            _allowWirePayment = _paymentsConfig.AllowWirePayment;
            _allowDirectDepositPayment = _paymentsConfig.AllowDirectDepositPayment;
            _allowWireForHardCash = _paymentsConfig.AllowWireForHardCash;
            _cashOnly = (Page as ProductsBase).CashOnly();
            _hardCash = (Page as ProductsBase).HardCash;
            _allowCreditCardForHardCash = _paymentsConfig.AllowCreditCardForHardCash;
            _allowMultipleCardsInTransaction = _paymentsConfig.AllowMultipleCardsInTransaction;
            _maxCardsToDisplay = (_allowMultipleCardsInTransaction) ? _paymentsConfig.MaxCardsToDisplay : 1;
            _currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            _paymentGatewayControl = new PaymentInfo_Generic();
            _paymentGatewayControl.TheBase = Page as ProductsBase; 
            if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
            {
                _currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.Currency;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            _orderTotals = GetTotals();
            lblErrorMessages.Text = string.Empty;
            SetupPaymentMethodDisplay();
            if (!IsPostBack)
            {
                LoadCards();
            }
            SetupGridRows();
            ShowMessages();
        }

        #endregion

        #region PaymentInfoBase interface implementation

        /// <summary>Returns the payment method system to use for authorization</summary>
        /// <returns></returns>
        public bool IsUsingPaymentGateway
        {
            get { return GetCurrentPaymentOption() == PaymentOptionChoice.PaymentGateway; }
        }

        /// <summary>Returns the PaymentGateway Control/summary>
        /// <returns></returns>
        public PaymentGatewayControl PaymentGatewayInterface
        {
            get { return _paymentGatewayControl; }
            set { _paymentGatewayControl = value; }
        }

        // ***********************************************************************************************
        /// <summary>Returns the payment method system to use for authorization</summary>
        /// <returns></returns>
        public PaymentMethodType PaymentGatewayPaymentMethod
        {
            get { return PaymentMethodType.CreditCard; }
            set { }
        }

        /// <summary>Returns the payment method system to use for authorization</summary>
        /// <returns></returns>
        public bool IsAcknowledged
        {
            get { return true; }
        }

        /// <summary>Load the distributors payment cards from storage</summary>
        public void LoadCards()
        {
            if (!IsPostBack)
            {
                SetPaymentInformation(null);
            }

            lblCardsLimitMessage.Text = (_maxCardsToDisplay > 1)
                                            ? string.Format(lblCardsLimitMessage.Text, _maxCardsToDisplay)
                                            : string.Empty;
            var totals = GetTotals();
            if (null != totals)
            {
                totalWireDue.Text = DisplayAsCurrency(totals.AmountDue, true);
                totalDirectDepositDue.Text = totalWireDue.Text;
                totalPaymentGatewayDue.Text = totalWireDue.Text;
                txtGrandTotal.Text = totalWireDue.Text;
                Page.ClientScript.RegisterClientScriptBlock(typeof (string), "TotalCreditDue",
                                                            string.Format(ScriptBlock,
                                                                          string.Concat("var amount = '",
                                                                                        string.Format("{0:F2}",
                                                                                                      totals
                                                                                                          .AmountDue),
                                                                                        "';")));
            }
            if (_CurrentPaymentsList != null && _CurrentPaymentsList.Count > 0)
            {
                if ((_orderTotals = totals) != null)
                {
                    totalAmountBalance.Text = DisplayAsCurrency(totals.AmountDue, false);
                }
                if (!IsPostBack)
                {
                    //SetPaymentInformation(_CurrentPaymentsList, _locale, _distributorId);
                    _CurrentPaymentsList = GetCurrentCardsList();
                    dataListCardInfo.DataSource = _CurrentPaymentsList;
                    dataListCardInfo.DataBind();
                }
            }

            ResolvePayments();
        }

        /// <summary>Retrieves the current list of PaymentInformation from the users session</summary>
        /// <param name="locale">The current locale</param>
        /// <param name="distributorID">The current Distributor</param>
        /// <returns>The list of payments</returns>
        public List<PaymentInformation> GetCurrentPaymentInformation(string locale, string distributorID)
        {
            string key = PaymentsConfiguration.GetCurrentPaymentSessionKey(string.Empty, string.Empty);
            return Session[key] as List<PaymentInformation>;
        }

        public void SetCurrentPaymentInformation(List<PaymentInformation> payments, string locale, string distributorID)
        {
            //Not Used
        }

        /// <summary>Retrieves all the current Payment choices from the user</summary>
        /// <param name="shippingAddress">dunno</param>
        /// <returns>The list of Payments to submit for the order</returns>
        public List<Payment> GetPayments(Address_V01 shippingAddress)
        {
            var paymentList = new List<Payment>();
            var choice = GetCurrentPaymentOption();
            switch (choice)
            {
                case PaymentOptionChoice.DirectDeposit:
                case PaymentOptionChoice.WireTransfer:
                case PaymentOptionChoice.PaymentGateway:
                    {
                        paymentList.Add(CreateDummyPayment(shippingAddress));
                        break;
                    }
                case PaymentOptionChoice.CreditCard:
                    {
                        paymentList =
                            new List<Payment>(
                                (from p in GetPaymentInformation()
                                 where !string.IsNullOrEmpty(p.AuthorizationCode)
                                 select p as Payment).ToList());
                        break;
                    }
            }

            string currentKey = PaymentsConfiguration.GetCurrentPaymentSessionKey(_locale, _distributorId);
            Session[currentKey] = paymentList;
            return paymentList;
        }

        /// <summary>
        ///     Any errors that occurred can be seen here
        /// </summary>
        public List<string> Errors
        {
            get { return _Errors; }
        }

        public void Refresh()
        {
            UpdatePanel1.Update();
        }

        #endregion

        #region Private methods

        public bool IsWireTransaction
        {
            get
            {
                return _currentOption == PaymentOptionChoice.WireTransfer ||
                       _currentOption == PaymentOptionChoice.DirectDeposit;
            }
        }

        private Payment CreateDummyPayment(Address_V01 address)
        {
            Payment payment = null;
            var cc = new CreditCard();
            cc.IssuerAssociation = CreditCard.GetCardType("VI");
            cc.AccountNumber = PaymentInfoProvider.VisaCardNumber;
            cc.CVV = "123";
            cc.Expiration = new DateTime(2012, 2, 1);
            cc.NameOnCard = "Test Card";

            switch (GetCurrentPaymentOption())
            {
                case PaymentOptionChoice.CreditCard:
                    {
                        var cp = new CreditPayment_V01();
                        var options = new PaymentOptions_V01();
                        options.NumberOfInstallments = 1;
                        cp.PaymentOptions = options;
                        cp.AuthorizationMethod = AuthorizationMethodType.Online;
                        cp.AuthorizationCode = "123456";
                        cp.Card = cc;
                        payment = cp;
                        break;
                    }
                case PaymentOptionChoice.PaymentGateway:
                    {
                        var cp = new CreditPayment_V01();
                        var options = new PaymentOptions_V01();
                        options.NumberOfInstallments = 1;
                        string payCode = HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayPayCode;
                        if (!string.IsNullOrEmpty(payCode))
                        {
                            cc.IssuerAssociation = CreditCard.GetCardType(payCode);
                        }
                        cp.Card = cc;
                        cp.PaymentOptions = options;
                        cp.AuthorizationMethod = AuthorizationMethodType.PaymentGateway;
                        cp.TransactionType = HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayAlias;
                        cp.AuthorizationCode = "654321";
                        //cp.TransactionID = (null != _paymentGatewayResponse) ? _paymentGatewayResponse.TransactionCode : Guid.NewGuid().ToString();
                        cp.Card = cc;
                        payment = cp;
                        break;
                    }
                case PaymentOptionChoice.WireTransfer:
                    {
                        var wp = new WirePayment_V01();
                        wp.TransactionType = ddlWire.SelectedItem.Text;
                        wp.PaymentCode = ddlWire.SelectedValue;
                        wp.ReferenceID = DistributorOrderingProfile.ReferenceNumber;
                        wp.TransactionType = GetLocalResourceObject("ListItemResource3.Text") as string +
                                             ddlWire.SelectedItem.Text;
                        payment = wp;
                        break;
                    }
                case PaymentOptionChoice.DirectDeposit:
                    {
                        var wp = new DirectDepositPayment_V01();
                        wp.PaymentCode = ddlDirectDeposit.SelectedValue;
                        wp.ReferenceID = DistributorOrderingProfile.ReferenceNumber;
                        wp.TransactionType = string.Format("{0}{1}",
                                                           GetLocalResourceObject("ListItemResource4.Text") as string,
                                                           ddlDirectDeposit.Items.Count > 1
                                                               ? string.Format("/{0}",
                                                                               ddlDirectDeposit.SelectedItem.Text)
                                                               : string.Empty);
                        payment = wp;
                        break;
                    }
            }
            if (null == address)
            {
                address = new Address_V01();
                //address = new Address_V01();
                //address.City = "Torrance";
                //address.Country = _countryCode;
                //address.CountyDistrict = "Los Angeles";
                //address.Line1 = "950W 190th Street";
                //address.PostalCode = "90502";
                //address.StateProvinceTerritory = "CA";
            }
            if (null == payment.Address)
            {
                payment.Address = address;
            }
            payment.Address = address;
            payment.Amount = GetTotals().AmountDue;
            payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency;

            return payment;
        }

        /// <summary>Stashes the current list of PaymentInformation into the users session</summary>
        /// <param name="payment"></param>
        /// <param name="locale">The current locale</param>
        /// <param name="distributorID">The current Distributor</param>
        private void SetPaymentInformation(List<CreditPayment> payments)
        {
            string key = PaymentsConfiguration.GetCurrentPaymentSessionKey(string.Empty, string.Empty);
            if (null != payments)
            {
                if (payments.Count > 0)
                {
                    Session.Add(key, payments);
                }
                else
                {
                    Session.Add(key, new List<CreditPayment>(new[] {new CreditPayment {LineID = "0"}}));
                }
            }
            else
            {
                if (null == Session[key])
                {
                    Session.Add(key, new List<CreditPayment>(new[] {new CreditPayment {LineID = "0"}}));
                }
            }

            _CurrentPaymentsList = Session[key] as List<CreditPayment>;
        }

        /// <summary>Retrieves the current list of PaymentInformation from the users session</summary>
        /// <param name="locale">The current locale</param>
        /// <param name="distributorID">The current Distributor</param>
        /// <returns>The list of payments</returns>
        private List<CreditPayment> GetPaymentInformation()
        {
            string key = PaymentsConfiguration.GetCurrentPaymentSessionKey(string.Empty, string.Empty);
            var items = Session[key] as List<CreditPayment>;
            if (null == items)
            {
                SetPaymentInformation(null);
                items = _CurrentPaymentsList;
            }

            return items.OrderBy(i => i.LineID).ToList();
        }

        /// <summary>Configure the Accordion</summary>
        private void SetupPaymentMethodDisplay()
        {
            rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            rblPaymentOptions.Items[2].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            rblPaymentOptions.Items[3].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            _currentOption = PaymentOptionChoice.CreditCard;
            var config = HLConfigManager.Configurations.PaymentsConfiguration;
            bool shouldAllowCC = PaymentInfoGrid.shouldAllowCC(_cashOnly, this._hardCash, this._allowCreditCardForHardCash);
            if (_allowWirePayment)
            {
                rblPaymentOptions.Items[2].Attributes.Remove(HtmlTextWriterAttribute.Style.ToString());
                pnlWireTable.Visible = true;

                if (!shouldAllowCC)
                {
                    rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    pnlCreditTable.Visible = false;
                    _currentOption = PaymentOptionChoice.WireTransfer;
                }
                if (config.DisablePaymentGateway)
                {
                    _currentOption = PaymentOptionChoice.WireTransfer;
                }
                SetWirePayments();
            }
            if (_allowDirectDepositPayment)
            {
                rblPaymentOptions.Items[3].Attributes.Remove(HtmlTextWriterAttribute.Style.ToString());
                pnlDirectDepositTable.Visible = true;

                if (!shouldAllowCC)
                {
                    rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    pnlCreditTable.Visible = false;
                    _currentOption = PaymentOptionChoice.DirectDeposit;
                }

                if (config.DisablePaymentGateway)
                {
                    if (!_allowWirePayment)
                    {
                        _currentOption = PaymentOptionChoice.DirectDeposit; //rblPaymentOptions.SelectedIndex = 3;
                    }
                }
                SetDirectDepositPayments();
            }
            if (!IsPostBack)
            {
                rblPaymentOptions.SelectedIndex = mpPaymentOptions.SelectedIndex = (int) _currentOption - 1;
                if (ddlDirectDeposit.Items.Count > 0)
                {
                    ddlDirectDeposit.Items[0].Selected = true;
                }
            }
        }

        /// <summary>Hook up client events for the data entry controls and resolve Payment Options</summary>
        private void SetupGridRows()
        {
            Page.Form.SubmitDisabledControls = true; // !_allowMultipleCardsInTransaction;
            var paymentInfo = GetPaymentInformation();
            //int id = 0;
            foreach (DataListItem item in dataListCardInfo.Items)
            {
                //working here now...
                var thisItem = item.FindControl("creditPaymentInfo") as PaymentInfoControl_KS;
                thisItem.Parent = this;
                var pi = thisItem.PaymentInfo;
                var lblCardIndex = thisItem.FindControl("lblCardHeader") as Label;
                if (null != lblCardIndex)
                {
                    lblCardIndex.Text = GetLocalResourceObject("Card_" + item.ItemIndex.ToString()) as string;
                }
                /*
                TextBox txtAmount = item.FindControl("txtAmount") as TextBox;
                TextBox txtCardholderName = item.FindControl("txtCardholderName") as TextBox;
                DropDownList ddlCardType = item.FindControl("ddlCardType") as DropDownList;
                DropDownList ddlInstallments = item.FindControl("ddlInstallments") as DropDownList;
                DropDownList ddlBCPoint = item.FindControl("ddlBCPoint") as DropDownList;
                 * */
            }
        }

        /// <summary>Resolve current card entries to the rebuilt grid. Substitute for ViewState - can't use it for adding cards</summary>
        /// <param name="payments">List of current user entered payments</param>
        private void ResolvePayments()
        {
            var payments = GetPaymentInformation();
            if (null != payments && payments.Count > 0)
            {
                decimal totalDue = GetTotals().AmountDue;
                decimal runTotal = totalDue;
                PaymentInfoControl_KS thisItem = null;
                CreditPayment pi = null;
                foreach (DataListItem item in dataListCardInfo.Items)
                {
                    thisItem = item.FindControl("creditPaymentInfo") as PaymentInfoControl_KS;
                    pi = thisItem.PaymentInfo;
                    runTotal -= (!string.IsNullOrEmpty(pi.AuthorizationCode)) ? pi.Amount : 0;

                    if (pi.Amount > totalDue)
                    {
                        if (string.IsNullOrEmpty(pi.AuthorizationCode))
                        {
                            pi.Amount = 0;
                        }
                        else
                        {
                            thisItem.FlagAsOverage();
                        }
                    }
                }
                if (runTotal > 0)
                {
                    pi.Amount = runTotal;
                    thisItem.DisplayCard(dataListCardInfo.Items.Count ==
                                         HLConfigManager.Configurations.PaymentsConfiguration.MaxCardsToDisplay);
                }
            }
        }

        private PaymentOptionChoice GetCurrentPaymentOption()
        {
            var choice = PaymentOptionChoice.None;
            switch (mpPaymentOptions.SelectedIndex)
            {
                case 0:
                    {
                        choice = PaymentOptionChoice.CreditCard;
                        break;
                    }
                case 1:
                    {
                        choice = PaymentOptionChoice.PaymentGateway;
                        break;
                    }
                case 2:
                    {
                        choice = PaymentOptionChoice.WireTransfer;
                        break;
                    }
                case 3:
                    {
                        choice = PaymentOptionChoice.DirectDeposit;
                        break;
                    }
            }

            return choice;
        }

        /// <summary>
        ///     Add new credit card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnAddNewCreditCard_Click(object sender, EventArgs e)
        {
            //onCreditCardProcessing(this, new PaymentInfoEventArgs(PaymentInfoCommandType.Add, new PaymentInformation(), false));
            //AjaxControlToolkit.ModalPopupExtender mpPaymentInformation = null; // (AjaxControlToolkit.ModalPopupExtender)ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

            //mpPaymentInformation.Show();
        }

        /// <summary>
        ///     Add new credit card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            //LinkButton lnkEdit = sender as LinkButton;
            //if (null != lnkEdit)
            //{
            //    GridViewRow row = lnkEdit.NamingContainer as GridViewRow;
            //    if (null != row)
            //    {
            //        int id = 0;
            //        TextBox cardId = row.FindControl("cardID") as TextBox;
            //        if (null != cardId)
            //        {
            //            if (int.TryParse(cardId.Text, out id))
            //            {
            //                PaymentInformation pi = GetCurrentCardsList().Find(p => p.ID == id);
            //                if (null != pi)
            //                {
            //                    onCreditCardProcessing(this, new PaymentInfoEventArgs(PaymentInfoCommandType.Edit, pi, false));
            //                    AjaxControlToolkit.ModalPopupExtender mpPaymentInformation = null; // (AjaxControlToolkit.ModalPopupExtender)ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

            //                    mpPaymentInformation.Show();
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private OrderTotals_V01 GetTotals()
        {
            var totals = new OrderTotals_V01();
            var cart = (Page as ProductsBase).ShoppingCart;
            if (null != cart)
            {
                if (null != cart.Totals)
                {
                    totals.AmountDue = (cart.Totals as OrderTotals_V01).AmountDue;
                }
            }
            //totals.AmountDue = 3566M;

            if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
            {
                decimal amountDue = OrderProvider.GetConvertedAmount((cart.Totals as OrderTotals_V01).AmountDue,
                                                                     (Page as ProductsBase).CountryCode);
                if (amountDue == 0.0M)
                {
                    LoggerHelper.Error("Exception while getting the currency conversion - ");
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                      "CurrencyConversionFailed");
                    lblErrorMessages.Visible = true;
                    return totals;
                }
                totals.AmountDue = amountDue;
            }

            return totals;
        }

        private void SetWirePayments()
        {
            int index = 0;
            ddlWire.Items.Clear();
            var codes = HLConfigManager.Configurations.PaymentsConfiguration.WirePaymentCodes;
            var aliases = HLConfigManager.Configurations.PaymentsConfiguration.WirePaymentAliases;
            if (null != aliases && null != codes && aliases.Count > 0 && codes.Count > 0)
            {
                foreach (string code in codes)
                {
                    if (index < aliases.Count)
                    {
                        ddlWire.Items.Add(new ListItem(aliases[index], code));
                    }
                    index++;
                }
                if (ddlWire.Items.Count > 1)
                {
                    ddlWire.Style.Remove("display");
                }
            }
            else
            {
                LoggerHelper.Error(
                    string.Format(
                        "PaymentsConfig for locale {0} has a problem with Wire Codes and Aliases. It says it has 1 or more codes but there is one or more mismatched or missing values",
                        _locale));
            }
        }

        // ***********************************************************************************************
        private void SetDirectDepositPayments()
        {
            int index = 0;
            ddlDirectDeposit.Items.Clear();
            var codes = HLConfigManager.Configurations.PaymentsConfiguration.DirectDepositPaymentCodes;
            var aliases = HLConfigManager.Configurations.PaymentsConfiguration.DirectDepositPaymentAliases;
            if (null != aliases && null != codes && aliases.Count > 0 && codes.Count > 0)
            {
                foreach (string code in codes)
                {
                    if (index < aliases.Count)
                    {
                        ddlDirectDeposit.Items.Add(new ListItem(aliases[index], code));
                    }
                    index++;
                }
                if (ddlDirectDeposit.Items.Count > 1)
                {
                    ddlDirectDeposit.Style.Remove("display");
                }
            }
            else
            {
                LoggerHelper.Error(
                    string.Format(
                        "PaymentsConfig for locale {0} has a problem with Direct Deposit Codes and Aliases. It says it has 1 or more codes, but there is one or more mismatched or missing values",
                        _locale));
            }
        }

        #endregion

        #region Grid display methods

        private List<CreditPayment> GetCurrentCardsList()
        {
            var cards = new List<CreditPayment>();
            if (null == _CurrentPaymentsList)
            {
                _CurrentPaymentsList = GetPaymentInformation();
            }
            int cardCount = (null != _CurrentPaymentsList) ? _CurrentPaymentsList.Count : 0;
            int maxCards = _maxCardsToDisplay;
            cards = new List<CreditPayment>(_CurrentPaymentsList);
            if (cardCount < maxCards)
            {
                if (cardCount > 0)
                {
                    var tot = (from c in cards where !string.IsNullOrEmpty(c.AuthorizationCode) select c.Amount).Sum();
                    if (null != _orderTotals && tot < _orderTotals.AmountDue && tot > 0)
                    {
                        cards.Add(new CreditPayment());
                    }
                }
            }
            else
            {
                if (cardCount > 0)
                {
                    //var tot = (from c in cards where !string.IsNullOrEmpty(c.AuthorizationCode) select c.Amount).Sum();

                    decimal tot = cards.Where(c => !string.IsNullOrEmpty(c.AuthorizationCode)).Sum(p => p.Amount);

                    if (null != _orderTotals && tot >= _orderTotals.AmountDue)
                    {
                        cards.RemoveAll(c => string.IsNullOrEmpty(c.AuthorizationCode));
                    }
                }
            }

            return cards;
        }

        #endregion

        #region Grid Events

        /// <summary>Even hook for new grid row - used to configure the Payment Options</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dataListCardInfo_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            var item = e.Item.FindControl("creditPaymentInfo") as PaymentInfoControl_KS;
            var clear = e.Item.FindControl("hlClearCards");
            var pi = e.Item.DataItem as CreditPayment;
            if (null != item && null != pi)
            {
                item.Parent = this;
                pi.LineID = e.Item.ItemIndex.ToString();
                item.PaymentInfo = pi;
                if (null != clear)
                {
                    clear.Visible = (!string.IsNullOrEmpty(pi.AuthorizationCode));
                }
            }
        }

        #endregion

        #region Client Events

        public bool IsPaymentError()
        {
            return _paymentError;
        }

        protected void WireChoiceChanged(object sender, EventArgs e)
        {
            List<Payment> payments = null;
            bool result = ValidateAndGetPayments(new Address_V01(), out payments, false);
            if (payments.Count > 0 && payments[0] is WirePayment_V01)
            {
                OnCreditCardAuthenticated(this, new CreditCardAuthenticationCompletedEventArgs(OrderCoveredStatus.FullyCovered));
            }
        }

        public void ClearCards(object sender, EventArgs e)
        {
            var ctl = sender as PaymentInfoControl_KS;
            var txtId = ctl.FindControl("txtId") as TextBox;
            if (null != txtId)
            {
                int itemId = 0;
                if (Int32.TryParse(txtId.Text, out itemId))
                {
                    _CurrentPaymentsList = GetCurrentCardsList();
                    var payment = _CurrentPaymentsList.Find(p => p.LineID == txtId.Text);
                    if (null != payment)
                    {
                        _CurrentPaymentsList.Remove(payment);
                        SetPaymentInformation(_CurrentPaymentsList);
                        _CurrentPaymentsList = GetCurrentCardsList();
                    }
                }

                dataListCardInfo.DataSource = _CurrentPaymentsList;
                dataListCardInfo.DataBind();
                ResolvePayments();
                SetupGridRows();
            }
        }

        protected void OnAuthorizeAttempt(object sender, EventArgs e)
        {
            string xid = XID.Value;
            string eci = ECI.Value;
            string cavv = CAVV.Value;
            string sessionKey = SessionKey.Value;
            string encData = EncryptedData.Value;
            string cardNumber = CardNumber.Value;

            var item = dataListCardInfo.Items[dataListCardInfo.Items.Count - 1].FindControl("creditPaymentInfo") as PaymentInfoControl_KS;
            var txtId = item.FindControl("txtId") as TextBox;
            var txtAmount = item.FindControl("txtAmount") as TextBox;
            var txtCardholderName = item.FindControl("txtCardholderName") as TextBox;
            var txtPaymentMethod = item.FindControl("txtPaymentMethodType") as TextBox;
            var ddlCardType = item.FindControl("ddlCardType") as DropDownList;
            var ddlInstallments = item.FindControl("ddlInstallments") as DropDownList;
            var ddlBCPoints = item.FindControl("ddlBCPoint") as DropDownList;

            CreditPayment thePayment = null;

            if (txtPaymentMethod.Text == "XMPI")
            {
                if (!string.IsNullOrEmpty((xid + eci + cavv)))
                {
                    var payment = new KoreaMPIPayment_V01();
                    payment.Amount = decimal.Parse(txtAmount.Text);
                    payment.AuthorizationMethod = AuthorizationMethodType.Online;
                    payment.Card = new CreditCard();
                    payment.Card.AccountNumber = cardNumber;
                    payment.Card.NameOnCard = txtCardholderName.Text;
                    payment.Card.Expiration = new DateTime(2049, 12, 31);
                    payment.Currency = CurrencyType.KRW.Key;
                    payment.XID = xid;
                    payment.CAVV = cavv;
                    payment.ECI = eci;
                    var options = new PaymentOptions_V01();
                    options.NumberOfInstallments = int.Parse(ddlInstallments.SelectedValue);
                    payment.PaymentOptions = options;
                    payment.AuthorizationCode = "123456";
                    thePayment = payment;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty((sessionKey + encData)))
                {
                    var payment = new KoreaISPPayment_V01();
                    payment.LineID = txtId.Text;
                    payment.Amount = decimal.Parse(txtAmount.Text);
                    payment.AuthorizationMethod = AuthorizationMethodType.Online;
                    payment.Card = new CreditCard();
                    payment.Card.AccountNumber = PaymentInfoProvider.VisaCardNumber;
                    payment.Card.NameOnCard = txtCardholderName.Text;
                    payment.Card.Expiration = new DateTime(2049, 12, 31);
                    payment.Currency = HL.Common.ValueObjects.CurrencyType.KRW.Key;
                    payment.KvpEncryptedData = encData;
                    // "sdfsdfhsdfgsdgfsdfgsdfgsdfgsdfgsdfgsdfgsdfg;jtrtgowrhgiwerhgifhgldkfjgvs;dofgjvsdfvhdfvhsldihvrlserhgew;jgdfgnkfvnkdzs,n.x,cbn"; // encData;
                    payment.KvpSessionKey = sessionKey;
                    // "yaoieyrboacryserivnpeiorugnpxm,posijfpxfbjzx;cvzx;c mzclvk nzkchvado;fjivga'dfivhjadofvhad;fvha;uisdvhasdcbx.vlkc;'vndgtohuwpsrotuywpergynbsactyserngisrcuyudrfhxonzisudhfbxaiufheowrhcfisdfuhvsdofihjv"; // sessionKey;
                    payment.BCTopPoints = int.Parse(ddlBCPoints.SelectedValue);
                    ;
                    var options = new PaymentOptions_V01();
                    options.NumberOfInstallments = int.Parse(ddlInstallments.SelectedValue) + payment.BCTopPoints;
                    payment.PaymentOptions = options;
                    payment.AuthorizationCode = "123456";
                    thePayment = payment;
                }
            }

            if (null != thePayment)
            {
                thePayment.Card.IssuingBankID = ddlCardType.SelectedItem.Text;
                thePayment.Address = new Address_V01();

                if (string.IsNullOrEmpty(thePayment.AuthorizationCode))
                {
                    //lblCreditCardMessage.Text = GetLocalResourceObject("ValidateCardPaymentAuthFailedMessage") as string;
                }
                else
                {
                    var items = GetPaymentInformation();
                    var p = (from c in items where c.LineID == thePayment.LineID select c);

                    if (p.Count() > 0)
                    {
                        int index = items.IndexOf(p.First());
                        items.RemoveAt(index);
                        items.Insert(index, thePayment);
                    }
                    else
                    {
                        items.Insert(0, thePayment);
                    }
                    SetPaymentInformation(items);

                    _CurrentPaymentsList = GetCurrentCardsList();
                    dataListCardInfo.DataSource = _CurrentPaymentsList;
                    dataListCardInfo.DataBind();
                    ResolvePayments();
                    SetupGridRows();

                    var status = OrderCoveredStatus.PartiallyCovered;
                    var tot =
                        (from c in _CurrentPaymentsList where !string.IsNullOrEmpty(c.AuthorizationCode) select c.Amount)
                            .Sum();
                    if (null != _orderTotals && tot == _orderTotals.AmountDue)
                    {
                        status = OrderCoveredStatus.FullyCovered;
                    }
                    OnCreditCardAuthenticated(thePayment, new CreditCardAuthenticationCompletedEventArgs(status));
                }
            }
        }

        /// <summary>Sets the selected Payment Method view</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SelectCurrentPaymentView(object sender, EventArgs e)
        {
            var rb = sender as RadioButtonList;
            if (null != rb)
            {
                mpPaymentOptions.SelectedIndex = rb.SelectedIndex;
            }
        }

        [SubscribesTo(MyHLEventTypes.CreditCardAuthorizationCompleted)]
        public void AuthorizationDone(object sender, EventArgs e)
        {
            if (null == e as CreditCardAuthorizationFailedEventArgs)
            {
                Session.Remove(PaymentsConfiguration.GetCurrentPaymentSessionKey(string.Empty, string.Empty));
                if (null != _CurrentPaymentsList)
                {
                    _CurrentPaymentsList.Clear();
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.CreditCardProcessed)]
        public void OnCardAddOrEdit(Object sender, EventArgs e)
        {
            var args = e as PaymentInfoEventArgs;
            if (null != sender && null != args)
            {
                GetPaymentsListFromProvider();
                switch (args.Command)
                {
                    case PaymentInfoCommandType.Add:
                        {
                            _CurrentPaymentsList = GetPaymentInformation();
                            //if (null == _CurrentPaymentsList)
                            //{
                            //    _CurrentPaymentsList = new List<PaymentInformation>();
                            //    PaymentInformation pi = _PaymentsList.Find(p => p.ID == args.PaymentInfo.ID);
                            //    if (null != pi)
                            //    {
                            //        _CurrentPaymentsList.Add(pi);
                            //    }
                            //}
                            break;
                        }
                    case PaymentInfoCommandType.Edit:
                        {
                            //int index = 0;
                            _CurrentPaymentsList = GetPaymentInformation();
                            //foreach (PaymentInformation pi in _CurrentPaymentsList)
                            //{
                            //    if (pi.ID == args.PaymentInfo.ID)
                            //    {
                            //        break;
                            //    }
                            //    index++;
                            //}
                            //_CurrentPaymentsList.RemoveAt(index);
                            //_CurrentPaymentsList.Insert(index, args.PaymentInfo);
                            break;
                        }
                }
                SetPaymentInformation(_CurrentPaymentsList);
            }

            GetPaymentsListFromProvider();
            dataListCardInfo.DataSource = GetCurrentCardsList();
            dataListCardInfo.DataBind();
            ResolvePayments();
            SetupGridRows();
        }

        private void CheckForPaymentGatewayResponse()
        {
            if (!IsPostBack)
            {
                //Test for response from KSNet control
            }
        }

        private void GetPaymentsListFromProvider()
        {
            var current = GetCurrentCardsList();
            if (current.Count > 0)
            {
                _CurrentPaymentsList = current;
            }
        }

        #endregion

        #region Server Events
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            bool proceed = false;
            string Proceed = (string.IsNullOrEmpty(Request["proceed"])) ? string.Empty : Request["proceed"];
            if (bool.TryParse(Proceed, out proceed))
            {
                KSAuthInfo info = Session[KSAuthInfo.Key] as KSAuthInfo;
                Session[KSAuthInfo.Key] = null;
                if (null == info)
                {
                    ECI.Value = (string.IsNullOrEmpty(Request["eci"])) ? string.Empty : Request["eci"];
                    CAVV.Value = (string.IsNullOrEmpty(Request["cavv"])) ? string.Empty : Request["cavv"];
                    XID.Value = (string.IsNullOrEmpty(Request["xid"])) ? string.Empty : Request["xid"];
                    CardNumber.Value = (string.IsNullOrEmpty(Request["cardno"])) ? string.Empty : Request["cardno"];
                    SessionKey.Value = string.Empty;
                    EncryptedData.Value = string.Empty;
                     OnAuthorizeAttempt(this, new EventArgs());
                    //ActionCode.Text = SubmitResponseScript.Replace("xid", Xid).Replace("eci", Eci).Replace("cavv", Cavv).Replace("cardnumber", CardNumber);

                }
            }
        } 
        #endregion

        #region Validations

        public bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments)
        {
            return ValidateAndGetPayments(shippingAddress, out payments, true);
        }

        public bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments, bool showErrors)
        {
            bool isValid = true;

            payments = new List<Payment>();
            var currentPayments = GetPayments(shippingAddress);
            var orderTotals = GetTotals();

            if (isValid)
            {
                decimal total = (from c in currentPayments select c.Amount).Sum();
                if (total != orderTotals.AmountDue)
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
            }

            if (isValid)
            {
                payments = currentPayments;
            }

            _paymentError = !isValid;

            if (showErrors && _paymentError)
            {
                OnCardAddOrEdit(null, null);
            }
            return isValid;
        }

        private void ShowMessages()
        {
            lblCurrencySymbol.Text = _currencySymbol;
        }

        private string DisplayAsCurrency(decimal amount, bool showCurrencySymbol)
        {
            if (showCurrencySymbol)
            {
                return getAmountString(amount);
            }
            else
            {
                return (_allowDecimal) ? string.Format("{0:F2}", amount) : Convert.ToInt32(amount).ToString();
            }
        }

        #endregion
    }
}