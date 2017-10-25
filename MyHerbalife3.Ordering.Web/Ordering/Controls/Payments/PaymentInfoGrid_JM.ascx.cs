using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Installments;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoGrid_JM : UserControlBase, PaymentInfoBase
    {
        #region Enums

        protected enum PaymentOptionChoice
        {
            None,
            CreditCard,
            PaymentGateway,
            WireTransfer,
            DirectDeposit
        }

        #endregion Enums

        #region Constants

        private const string StyleHidden = "display:none";
        private const string ScriptBlock = "<script type='text/javascript'>{0}</script>";

        private const string ValidationScriptMessages =
            "<script type='text/javascript'>    	var CardNumberRequired = '{0}';    	var CardNumberInvalid = '{1}';    	var CardNameRequired = '{2}';    	var ExpDateRequired = '{3}';    	var AddCardQuestion = '{4}';    	var DeleteCardQuestion = '{5}';     var CardHasExpired = '{6}';</script>";

        #endregion Constants

        #region Fields

        private readonly List<string> _Errors = new List<string>();
        private List<PaymentInformation> _CurrentPaymentsList;
        private List<PaymentInformation> _PaymentsList;
        private ModalPopupExtender _PopUpPaymentInformation;
        private bool _RegisteredCardsOnly = true;
        private bool _UsesPaymentGateway;
        private bool _allowDecimal = true;
        private bool _allowDirectDepositPayment;
        private bool _allowWireForHardCash;
        private bool _allowMultipleCardsInTransaction;
        private bool _allowWirePayment;
        private bool _cashOnly;
        private bool _hardCash;
        private bool _allowCreditCardForHardCash;
        private string _countryCode = string.Empty;
        private string _currencySymbol = string.Empty;
        private PaymentOptionChoice _currentOption = PaymentOptionChoice.None;
        private string _distributorId = string.Empty;
        private bool _enableInstallments;
        private InstallmentConfiguration _installmentsConfiguration;
        private string _locale = string.Empty;
        private int _maxCVV = 3;
        private int _maxCardsToDisplay;
        private OrderTotals_V01 _orderTotals;
        protected bool _paymentError;
        private PaymentGatewayControl _paymentGatewayControl;
        private PaymentGatewayResponse _paymentGatewayResponse;
        private PaymentsConfiguration _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
        private bool _requiresAcknowledgementToSubmit;
        private List<string> _restrictedDisplayCards = new List<string>();

        [Publishes(MyHLEventTypes.CreditCardProcessing)]
        public event EventHandler onCreditCardProcessing;

        [Publishes(MyHLEventTypes.PaymentOptionsViewChanged)]
        public event EventHandler onPaymentOptionsViewChanged;

        //public delegate void CreditCardProcessingHandler(object sender, EventArgs e);
        //public event CreditCardProcessingHandler onCreditCardProcessing;

        #endregion Fields

        #region Construction / Initialization

        // ***********************************************************************************************
        protected override void OnInit(EventArgs e)
        {
            if (!string.IsNullOrEmpty(_paymentsConfig.PaymentGatewayControl))
            {
                _paymentGatewayControl = LoadControl(_paymentsConfig.PaymentGatewayControl) as PaymentGatewayControl;
                if (null != _paymentGatewayControl)
                {
                    _paymentGatewayControl.TheBase = Page as ProductsBase;
                    _paymentGatewayControl.ID = "ucxPaymentGatewayControl";
                    _paymentGatewayControl.TabControl = rblPaymentOptions;
                    ((OrderingMaster)Page.Master).EventBus.RegisterObject(_paymentGatewayControl);
                }
                if ((_paymentsConfig.PaymentGatewayControlHasCardData))
                {
                    ucPaymentInfoControl.Visible = false;
                }
            }
            else
            {
                _paymentGatewayControl = new PaymentInfo_Generic();
                _paymentGatewayControl.TheBase = Page as ProductsBase;
            }

            _PopUpPaymentInformation = (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");
            _locale = (Page as ProductsBase).Locale;
            _distributorId = (Page as ProductsBase).DistributorID;
            _countryCode = (Page as ProductsBase).CountryCode;
            _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
            _maxCVV = _paymentsConfig.MaxCVV;
            _allowDecimal = _paymentsConfig.AllowDecimal;
            _RegisteredCardsOnly = OrderProvider.IsRegisteredCardsOnly(ShoppingCart);
            _UsesPaymentGateway = _paymentsConfig.HasPaymentGateway;
            _allowWirePayment = _paymentsConfig.AllowWirePayment;
            _allowDirectDepositPayment = _paymentsConfig.AllowDirectDepositPayment;
            _allowWireForHardCash = _paymentsConfig.AllowWireForHardCash;
            _cashOnly = (Page as ProductsBase).CashOnly();
            _hardCash = (Page as ProductsBase).HardCash;
            _allowCreditCardForHardCash = _paymentsConfig.AllowCreditCardForHardCash;
            _allowMultipleCardsInTransaction = _paymentsConfig.AllowMultipleCardsInTransaction;
            _maxCardsToDisplay = (_allowMultipleCardsInTransaction) ? _paymentsConfig.MaxCardsToDisplay : 1;
            _enableInstallments = _paymentsConfig.EnableInstallments;
            _requiresAcknowledgementToSubmit =
                HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmit;
            _restrictedDisplayCards = _paymentsConfig.RestrictedDisplayCards;
            _currencySymbol = (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                                  ? HLConfigManager.Configurations.CheckoutConfiguration.Currency
                                  : HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
        }

        // ***********************************************************************************************
        protected override void OnPreRender(EventArgs e)
        {
            CheckMaxCVV();
            ActivatePaymentOptions();
            ShowMessages();
        }

        // ***********************************************************************************************
        protected void Page_Load(object sender, EventArgs e)
        {
            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);

            string orderType = "RSO";

            if (_enableInstallments)
            {
                if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                    orderType = "ETO";

                if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems))
                    orderType = "APF";

                var orderMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                string orderMonth = ShoppingCart.OrderMonth.ToString();
                if (orderMonth.Length == 6)
                {
                    orderMonthDate = new DateTime(Int32.Parse(orderMonth.Substring(0, 4)), Int32.Parse(orderMonth.Substring(4)), 1);
                }
                _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration(_countryCode, orderMonthDate, orderType);
            }

            GetPaymentsListFromProvider();
            _orderTotals = GetTotals();
            _CurrentPaymentsList = GetCurrentPaymentInformation(_locale, _distributorId);
            lblErrorMessages.Text = string.Empty;
            SetupPaymentMethodDisplay();

            if (!IsPostBack)
            {
                LoadCards();
            }

            SetupGridRows();
            //SetAcknowledgeStyle();
        }

        #endregion Construction / Initialization

        #region PaymentInfoBase interface implementation

        // ***********************************************************************************************
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

        // ***********************************************************************************************
        /// <summary>Returns the payment method system to use for authorization</summary>
        /// <returns></returns>
        public bool IsAcknowledged
        {
            get { return chkAcknowledgeTransaction.Checked; }
        }

        // ***********************************************************************************************

        /// <summary>Load the distributors payment cards from storage</summary>
        public void LoadCards()
        {
            if (!IsPostBack)
            {
                SetCurrentPaymentInformation(null, _locale, _distributorId);
                CheckForPaymentGatewayResponse();
                lblCardsLimitMessage.Text = (_maxCardsToDisplay > 1)
                                                ? string.Format(lblCardsLimitMessage.Text, _maxCardsToDisplay)
                                                : string.Empty;
            }

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
                                                                                                      totals.AmountDue),
                                                                                        "';")));
            }
            if (_PaymentsList != null && _PaymentsList.Count > 0)
            {
                if ((_orderTotals = totals) != null)
                {
                    totalAmountBalance.Text = DisplayAsCurrency(totals.AmountDue, false);
                }

                if (!IsPostBack)
                {
                    if (null == _CurrentPaymentsList || _CurrentPaymentsList.Count == 0)
                    {
                        _CurrentPaymentsList =
                            (from p in _PaymentsList where p.IsPrimary orderby p.Alias select p).ToList();
                    }
                    SetCurrentPaymentInformation(_CurrentPaymentsList, _locale, _distributorId);
                    gridViewCardInfo.DataSource = GetCurrentCardsList(null);
                    gridViewCardInfo.DataBind();
                }
            }

            ResolvePayments();
        }

        // ***********************************************************************************************
        /// <summary>Retrieves the current list of PaymentInformation from the users session</summary>
        /// <param name="locale">The current locale</param>
        /// <param name="distributorID">The current Distributor</param>
        /// <returns>The list of payments</returns>
        public List<PaymentInformation> GetCurrentPaymentInformation(string locale, string distributorID)
        {
            string key = PaymentsConfiguration.GetPaymentInfoSessionKey(distributorID, locale);
            return Session[key] as List<PaymentInformation>;
        }

        // ***********************************************************************************************
        /// <summary>Stashes the current list of PaymentInformation into the users session</summary>
        /// <param name="payment"></param>
        /// <param name="locale">The current locale</param>
        /// <param name="distributorID">The current Distributor</param>
        public void SetCurrentPaymentInformation(List<PaymentInformation> payments, string locale, string distributorID)
        {
            string key = PaymentsConfiguration.GetPaymentInfoSessionKey(distributorID, locale);
            if (null != payments)
            {
                Session.Add(key, payments);
            }
            else
            {
                Session.Remove(key);
            }

            if (null == payments)
            {
                if (null != _PaymentsList)
                {
                    foreach (PaymentInformation pi in _PaymentsList)
                    {
                        pi.Amount = 0;
                        pi.Verification = string.Empty;
                    }
                }
            }
            _CurrentPaymentsList = payments;
        }

        // ***********************************************************************************************
        /// <summary>Retrieves all the current Payment choices from the user</summary>
        /// <param name="shippingAddress">dunno</param>
        /// <returns>The list of Payments to submit for the order</returns>
        public List<Payment> GetPayments(Address_V01 shippingAddress)
        {
            var paymentList = new List<Payment>();
            CheckForPaymentGatewayResponse();
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
                        var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);

                        foreach (GridViewRow row in gridViewCardInfo.Rows)
                        {
                            var cardType = row.FindControl("lblCardType") as Label;
                            var txtIssueNumber = row.FindControl("txtIssueNumber") as TextBox;
                            var txtCVV = row.FindControl("txtCVV") as TextBox;
                            var txtAmount = row.FindControl("txtAmount") as TextBox;
                            var id = row.FindControl("cardID") as TextBox;
                            var payOption = row.FindControl("txtOption") as TextBox;
                            var choice1 = row.FindControl("txtChoice1") as TextBox;
                            var choice2 = row.FindControl("txtChoice2") as TextBox;
                            var ddInstallments = (DropDownList) row.FindControl("drpInstallments");
                            var currentOptions = row.FindControl("lnkPaymentOptions") as LinkButton;
                            int cardID = int.Parse(id.Text);
                            decimal cardAmount;
                            if (decimal.TryParse(txtAmount.Text, out cardAmount) && cardID > 0)
                            {
                                var info = paymentInfo.Find(p => p.ID == cardID);
                                var payment = new CreditPayment_V01();
                                info.Amount = cardAmount;
                                payment.Card = new CreditCard();
                                payment.AuthorizationMethod = AuthorizationMethodType.Online;
                                payment.Card.IssuerAssociation = CreditCard.GetCardType(info.CardType.Trim());
                                payment.Amount = cardAmount;
                                payment.Address = shippingAddress;
                                payment.Card.AccountNumber = info.CardNumber;
                                payment.Card.CVV = txtCVV.Text;
                                payment.AuthorizationCode = string.Empty;
                                payment.Card.Expiration = info.Expiration;
                                payment.Card.NameOnCard =
                                    (info.CardHolder.First.Trim() + " " + info.CardHolder.Last.Trim()).Trim();
                                payment.Card.IssuingBankID = info.IssueNumber;
                                payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                                payment.Address = (null != info.BillingAddress) ? info.BillingAddress : shippingAddress;
                                payment.PaymentOptions = GetPaymentOptions(payOption, choice1, choice2, ddInstallments);

                                info.Options = payment.PaymentOptions;
                                SetPaymentOptions(info, row);
                                payment.ReferenceID = currentOptions.Text;
                                paymentList.Add(payment);
                            }
                        }
                        break;
                    }
            }

            string currentKey = PaymentsConfiguration.GetCurrentPaymentSessionKey(_locale, _distributorId);
            Session[currentKey] = paymentList;
            return paymentList;
        }

        // ***********************************************************************************************

        // ***********************************************************************************************
        public List<string> Errors
        {
            get { return _Errors; }
        }

        public void Refresh()
        {
            OnPaymentInfoChanged(this, new EventArgs());
        }

        public void RefreshCardPayment()
        {
            _orderTotals = GetTotals();
            SetupGridRows();
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
                        cp.TransactionType = GetLocalResourceObject("ListItemResource1.Text") as string;
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
                        cp.TransactionType = GetLocalResourceObject("ListItemResource2.Text") as string;
                        cp.AuthorizationCode = "654321";
                        cp.TransactionID = (null != _paymentGatewayResponse)
                                               ? _paymentGatewayResponse.TransactionCode
                                               : Guid.NewGuid().ToString();
                        cp.Card = cc;
                        payment = cp;
                        break;
                    }
                case PaymentOptionChoice.WireTransfer:
                    {
                        var wp = new WirePayment_V01();
                        wp.PaymentCode = ddlWire.SelectedValue;

                        wp.ReferenceID = DistributorOrderingProfile.ReferenceNumber;
                        wp.TransactionType = string.Format("{0}{1}",
                                                           GetLocalResourceObject("ListItemResource3.Text") as string,
                                                           ddlWire.Items.Count > 1
                                                               ? string.Format("/{0}", ddlWire.SelectedItem.Text)
                                                               : string.Empty);
                        payment = wp;
                        break;
                    }
                case PaymentOptionChoice.DirectDeposit:
                    {
                        var wp = new DirectDepositPayment_V01();
                        wp.PaymentCode = ddlDirectDeposit.SelectedValue;
                        wp.ReferenceID = DistributorOrderingProfile.ReferenceNumber;
                        var aliases =
                            HLConfigManager.Configurations.PaymentsConfiguration.DirectDepositPaymentAliases;
                        for (int i = 0; i < aliases.Count; i++)
                        {
                            if (i == ddlDirectDeposit.SelectedIndex)
                            {
                                wp.BankName = aliases[i];
                            }
                        }
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
            payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();

            return payment;
        }

        // ***********************************************************************************************
        private List<PaymentInformation> CreateDummyPayment(bool createPaymentInformation)
        {
            var list = new List<PaymentInformation>();
            var info = new PaymentInformation();
            if (createPaymentInformation)
            {
                info.Amount = ((Page as ProductsBase).ShoppingCart.Totals as OrderTotals_V01).AmountDue;
                info.BillingAddress = new Address_V01("950W 190th Street", string.Empty, string.Empty, string.Empty,
                                                      "Torrance", "Los Angeles", "CA", _countryCode, "90502");
                info.CardHolder = new Name_V01() { First = "Gerry", Last = "Hayes" };
                info.CardNumber = PaymentInfoProvider.VisaCardNumber;
                info.CardType = "VI";
                info.Expiration = new DateTime(2020, 12, 31);
                info.Verification = "123";
                list.Add(info);
            }
            return list;
        }

        #endregion PaymentInfoBase interface implementation

        #region Private methods

        // ***********************************************************************************************
        public bool IsWireTransaction
        {
            get
            {
                return _currentOption == PaymentOptionChoice.WireTransfer ||
                       _currentOption == PaymentOptionChoice.DirectDeposit;
            }
        }

        /// <summary></summary>
        /// <param name="makeActive"></param>
        private void SetupGridRow(GridViewRow row, bool makeActive)
        {
            row.FindControl("lblExp").Visible =
                row.FindControl("lnkEdit").Visible =
                row.FindControl("txtCVV").Visible =
                row.FindControl("txtAmount").Visible = makeActive;
            var ctl = row.FindControl("txtIssueNumber");
            if (null != ctl)
            {
                ctl.Visible = makeActive;
            }
        }

        // ***********************************************************************************************
        protected string GetLocalizedCardType(string cardCode)
        {
            cardCode = !string.IsNullOrEmpty(cardCode) ? cardCode.Trim() : cardCode;
            return GetLocalResourceString(GetCardCodeResourceKey(cardCode), cardCode);
        }

        // ***********************************************************************************************
        private string GetCardDescriptionResourceKey(string cardCode)
        {
            return string.Format("{0}_{1}_{2}", "CardType", cardCode, "Description");
        }

        // ***********************************************************************************************
        private string GetCardCodeResourceKey(string cardCode)
        {
            return string.Format("{0}_{1}_{2}", "CardType", cardCode, "Code");
        }

        // ***********************************************************************************************
        private string GetLocalResourceString(string key, string defaultValue)
        {
            string resourceString;

            try
            {
                object value;
                value = GetLocalResourceObject(key);
                if (value == null || !(value is string))
                {
                    value = defaultValue;
                    throw new ApplicationException(string.Format("Missing local resource object. Key: {0}", key));
                }

                resourceString = value as string;
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn(ex.ToString());
                resourceString = defaultValue;
            }

            return resourceString;
        }

        // ***********************************************************************************************
        /// <summary>Create the Card Expiration date from the dropdown choices</summary>
        /// <param name="cardType">The type of card</param>
        /// <param name="row">The row containing the new card data</param>
        /// <returns></returns>
        private DateTime MakeExpirationDate(string cardType, GridViewRow row)
        {
            if (cardType == "QH") return GetMyKeyExpirationDate();
            string month = (row.FindControl("cbMonth") as DropDownList).SelectedItem.Text;
            string year = (row.FindControl("cbYear") as DropDownList).SelectedItem.Text;
            return DateTimeUtils.GetLastDayOfMonth(new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1));
        }

        // ***********************************************************************************************
        /// <summary>The standin date for MyKey card</summary>
        /// <returns>The date</returns>
        protected DateTime GetMyKeyExpirationDate()
        {
            return new DateTime(2049, 12, 31);
        }

        // ***********************************************************************************************
        /// <summary>Configure the Accordion</summary>
        private void SetupPaymentMethodDisplay()
        {
            btnAddNewCreditCard.Visible = !_RegisteredCardsOnly;

            rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            rblPaymentOptions.Items[2].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            rblPaymentOptions.Items[3].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
            _currentOption = PaymentOptionChoice.CreditCard;
            var config = HLConfigManager.Configurations.PaymentsConfiguration;

            if (_enableInstallments)
            {
                var currentDate = DateUtils.GetCurrentLocalTime(_countryCode);

                if ((_installmentsConfiguration.Cards != null) && (_installmentsConfiguration.Cards.Card != null))
                {
                    var cardInfoWire =
                    (from c in _installmentsConfiguration.Cards.Card where c.CardId == "Wire" select c).FirstOrDefault();

                    if (cardInfoWire != null)
                    {
                        var cc = cardInfoWire.VolumeStrategy.Where(f => f.Volume == 9999).FirstOrDefault();
                        if (cc != null)
                        {
                            if (
                                !(currentDate > cc.EffectiveDates.StartDateTime &&
                                  currentDate < cc.EffectiveDates.EndDateTime))
                            {
                                _allowWirePayment = false;
                            }
                        }
                    }
                }
            }
            bool shouldAllowCC = PaymentInfoGrid.shouldAllowCC(_cashOnly, this._hardCash, this._allowCreditCardForHardCash);
            if (_UsesPaymentGateway)
            {
                if (!string.IsNullOrEmpty(config.PaymentGatewayAlias))
                    rblPaymentOptions.Items[1].Text = config.PaymentGatewayAlias;

                rblPaymentOptions.Items[1].Attributes.Remove(HtmlTextWriterAttribute.Style.ToString());
                pnlPaymentGatewayTable.Visible = true;
                hidPaymentMethod.Value = config.PaymentGatewayPaymentMethods;
                if (null != _paymentGatewayControl && null == _paymentGatewayControl as PaymentInfo_Generic)
                {
                    pnlPaymentGatewayControlHolder.Controls.Add(_paymentGatewayControl);
                    var trig = new AsyncPostBackTrigger();
                    trig.ControlID = _paymentGatewayControl.ID;
                    UpdatePanel1.Triggers.Add(trig);
                }
                if (string.Compare(config.PaymentGatewayPaymentMethods, "CreditCard", true) == 0)
                {
                    rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    pnlCreditTable.Visible = false;
                    if (!shouldAllowCC)
                    {
                        rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                        pnlPaymentGatewayTable.Visible = false;
                    }
                    else
                    {
                        _currentOption = PaymentOptionChoice.PaymentGateway;
                    }
                }
                if (config.DisablePaymentGateway)
                {
                    rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    pnlPaymentGatewayTable.Visible = false;
                }
                if (string.Compare(config.PaymentGatewayPaymentMethods, "WireTransfers", true) == 0)
                {
                    if (!shouldAllowCC)
                    {
                        rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                        pnlCreditTable.Visible = false;
                        _currentOption = PaymentOptionChoice.PaymentGateway;

                    }
                }
            }

            if (_allowWirePayment)
            {
                rblPaymentOptions.Items[2].Attributes.Remove(HtmlTextWriterAttribute.Style.ToString());
                pnlWireTable.Visible = true;

                if (!shouldAllowCC)
                {
                    rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    if (_UsesPaymentGateway)
                    {
                        rblPaymentOptions.Items[1].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
                    }
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
                //if (!string.IsNullOrEmpty(config.DirectDepositPaymentAlias)) rblPaymentOptions.Items[3].Text = config.DirectDepositPaymentAlias;
                rblPaymentOptions.Items[3].Attributes.Remove(HtmlTextWriterAttribute.Style.ToString());
                pnlDirectDepositTable.Visible = true;

                if (!shouldAllowCC)
                {
                    rblPaymentOptions.Items[0].Attributes.Add(HtmlTextWriterAttribute.Style.ToString(), StyleHidden);
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
                pnlAcknowledCheckContent.Visible = _requiresAcknowledgementToSubmit;
                if (_requiresAcknowledgementToSubmit &&
                    HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmitWireOnly)
                {
                    pnlAcknowledCheckContent.Visible = _currentOption == PaymentOptionChoice.WireTransfer ||
                                                       _currentOption == PaymentOptionChoice.DirectDeposit;
                    if (!pnlAcknowledCheckContent.Visible)
                        chkAcknowledgeTransaction.Checked = true;
                }
                rblPaymentOptions.SelectedIndex = mpPaymentOptions.SelectedIndex = (int) _currentOption - 1;
                if (ddlWire.Items.Count > 0)
                {
                    ddlWire.Items[0].Selected = true;
                }
                if (ddlDirectDeposit.Items.Count > 0)
                {
                    ddlDirectDeposit.Items[0].Selected = true;
                }
            }
        }

        // ***********************************************************************************************
        /// <summary>Hook up client events for the data entry controls and resolve Payment Options</summary>
        private void SetupGridRows()
        {
            //this.Page.Form.SubmitDisabledControls = true; // !_allowMultipleCardsInTransaction;
            var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);
            int id = 0;
            foreach (GridViewRow row in gridViewCardInfo.Rows)
            {
                var txtAmount = row.FindControl("txtAmount") as TextBox;
                var txtCVV = row.FindControl("txtCVV") as TextBox;
                var txtId = row.FindControl("cardID") as TextBox;
                int.TryParse(txtId.Text, out id);
                var ddlCards = row.FindControl("ddlCards") as NostalgicDropDownList;
                if (IsPostBack)
                {
                    SetupGridRow(row, (!string.IsNullOrEmpty(ddlCards.SelectedValue)));
                }
                txtCVV.MaxLength = 3;
                if (_allowMultipleCardsInTransaction)
                {
                    _orderTotals = GetTotals();
                    if (txtAmount != null && _orderTotals != null)
                    {
                        txtAmount.Attributes["onblur"] = string.Format("AmountLosingFocus(event,this,'{0}', {1})",
                                                                       string.Format("{0:F2}", _orderTotals.AmountDue),
                                                                       _allowDecimal ? 1 : 0);
                        txtAmount.Attributes["onfocus"] = string.Format("AmountLosingFocus(event,this,'{0}', {1})",
                                                                        string.Format("{0:F2}", _orderTotals.AmountDue),
                                                                        _allowDecimal ? 1 : 0);
                        txtAmount.Attributes["onkeypress"] = string.Format("checkAmount(event,this,{0},'{1}')",
                                                                           _allowDecimal ? 1 : 0,
                                                                           string.Format("{0:F2}",
                                                                                         _orderTotals.AmountDue));
                    }
                    if (txtCVV != null && _orderTotals != null)
                    {
                        txtCVV.Attributes["onkeypress"] = string.Format("CVVKeyPress(event,this,'{0}',{1})",
                                                                        string.Format("{0:F2}", _orderTotals.AmountDue),
                                                                        _maxCVV);
                    }
                }
                else
                {
                    txtAmount.Attributes.Add("readonly", "readonly");
                    txtAmount.Text = (_allowDecimal)
                                         ? string.Format("{0:F2}", _orderTotals.AmountDue)
                                         : Convert.ToInt32(_orderTotals.AmountDue).ToString();
                    //txtCVV.Attributes["onkeypress"] = "MakeNumeric(event, this)";
                }
                if (null != paymentInfo && paymentInfo.Count > 0 && id > 0)
                {
                    var po = paymentInfo.Find(p => p.ID == id);
                    if (null != po)
                    {
                        SetPaymentOptions(po, row);
                    }
                }
            }
            if (_allowMultipleCardsInTransaction &&
                (null != _PaymentsList && _PaymentsList.Count > 0 &&
                 (null != _CurrentPaymentsList && _CurrentPaymentsList.Count > 1)))
            {
                pnlTotalDue.Style.Remove("display");
            }
            else
            {
                pnlTotalDue.Style.Add("display", "none");
            }

            Page.ClientScript.RegisterClientScriptBlock(typeof (string), "PGControlNamingPrefix",
                                                        string.Format(ScriptBlock,
                                                                      string.Concat("var pgPrefix = '",
                                                                                    txtGrandTotal.ClientID.Replace(
                                                                                        txtGrandTotal.ID, string.Empty),
                                                                                    "';")));
            Page.ClientScript.RegisterClientScriptBlock(typeof (string), "CurrencySymbol",
                                                        string.Format(ScriptBlock,
                                                                      string.Concat("var currencySymbol = '",
                                                                                    _currencySymbol, "';")));
        }

        // ***********************************************************************************************
        /// <summary>Resolve current card entries to the rebuilt grid. Substitute for ViewState - can't use it for adding cards</summary>
        /// <param name="payments">List of current user entered payments</param>
        private void ResolvePayments()
        {
            var payments = GetCurrentPaymentInformation(_locale, _distributorId);
            if (null != payments && payments.Count > 0)
            {
                decimal totalDue = GetTotals().AmountDue;
                decimal runTotal = totalDue;
                foreach (GridViewRow row in gridViewCardInfo.Rows)
                {
                    var cardType = row.FindControl("lblCardType") as Label;
                    var txtCVV = row.FindControl("txtCVV") as TextBox;
                    var txtAmount = row.FindControl("txtAmount") as TextBox;
                    var txtId = row.FindControl("cardID") as TextBox;
                    int cardId = 0;
                    if (Int32.TryParse(txtId.Text, out cardId) && cardId > 0)
                    {
                        var pi = payments.Find(p => p.ID == cardId);
                        if (pi.AuthorizationFailures >= 3)
                        {
                            txtCVV.Text = txtAmount.Text = string.Empty;
                            txtCVV.Enabled = txtAmount.Enabled = false;
                        }
                        else
                        {
                            //txtCVV.Text = (null != pi.Verification) ? pi.Verification.Trim() : string.Empty;
                            //txtAmount.Text = (pi.Amount == 0) ? string.Empty : pi.Amount.ToString();
                        }
                        runTotal -= pi.Amount;
                        SetPaymentOptions(pi, row);
                    }
                }
                if (runTotal != totalDue)
                {
                    totalAmountBalance.Text = DisplayAsCurrency(runTotal, false);
                }
            }
        }

        // ***********************************************************************************************
        protected PaymentOptionChoice GetCurrentPaymentOption()
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

        // ***********************************************************************************************
        /// <summary>
        ///     Add new credit card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnAddNewCreditCard_Click(object sender, EventArgs e)
        {
            onCreditCardProcessing(this,
                                   new PaymentInfoEventArgs(PaymentInfoCommandType.Add, new PaymentInformation(), false));
            var mpPaymentInformation = (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

            mpPaymentInformation.Show();
        }

        // ***********************************************************************************************
        /// <summary>
        ///     Add new credit card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            var lnkEdit = sender as LinkButton;
            if (null != lnkEdit)
            {
                var row = lnkEdit.NamingContainer as GridViewRow;
                if (null != row)
                {
                    int id = 0;
                    var cardId = row.FindControl("cardID") as TextBox;
                    if (null != cardId)
                    {
                        if (int.TryParse(cardId.Text, out id))
                        {
                            var pi = GetCurrentCardsList(null).Find(p => p.ID == id);
                            if (null != pi)
                            {
                                onCreditCardProcessing(this,
                                                       new PaymentInfoEventArgs(PaymentInfoCommandType.Edit, pi, false));
                                var mpPaymentInformation =
                                    (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

                                mpPaymentInformation.Show();
                            }
                        }
                    }
                }
            }
        }

        protected void AcknowledgeChanged(object sender, EventArgs e)
        {
            SetAcknowledgeStyle();
            var payments = GetPayments(null);
            if (null != payments)
            {
                decimal total = payments.Sum(a => a.Amount);
                var totals = GetTotals();
                if (null != totals)
                {
                    totalAmountBalance.Text = DisplayAsCurrency(totals.AmountDue - total, false);
                }
            }
        }

        public void SetAcknowledgeStyle()
        {
            if (chkAcknowledgeTransaction.Visible)
            {
                pnlAcknowledCheckContent.CssClass = chkAcknowledgeTransaction.Checked
                                                        ? "mandatoryConfirm"
                                                        : "errorConfirm";
            }
            else
            {
                pnlAcknowledCheckContent.CssClass = string.Empty;
            }
        }

        // ***********************************************************************************************

        // ***********************************************************************************************
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

        // ***********************************************************************************************
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

        #endregion Private methods

        #region Payment Options

        // ***********************************************************************************************
        private void ActivatePaymentOptions()
        {
            if (_countryCode == "JP")
            {
                PayOptionsAL.Visible = PayOptionsQH.Visible = true;
                var modPopPayOptionsQH = new ModalPopupExtender();
                var modPopPayOptionsAL = new ModalPopupExtender();
                modPopPayOptionsQH.BehaviorID = "modPopPayOptionsQH";
                modPopPayOptionsQH.DropShadow = false;
                modPopPayOptionsQH.TargetControlID = "btnQH";
                modPopPayOptionsQH.PopupControlID = "PayOptionsQH";
                modPopPayOptionsQH.CancelControlID = "btnCancelQH";
                modPopPayOptionsQH.OkControlID = "btnOkQH";
                modPopPayOptionsQH.OnOkScript = "new function() {SetQHPayOptions(this); return false;}";
                modPopPayOptionsQH.RepositionMode = ModalPopupRepositionMode.None;

                modPopPayOptionsAL.BehaviorID = "modPopPayOptionsAL";
                modPopPayOptionsAL.DropShadow = false;
                modPopPayOptionsAL.TargetControlID = "btnAL";
                modPopPayOptionsAL.PopupControlID = "PayOptionsAL";
                modPopPayOptionsAL.CancelControlID = "btnCancelAL";
                modPopPayOptionsAL.OkControlID = "btnOkAL";
                modPopPayOptionsAL.OnOkScript = "new function() {SetALPayOptions(this); return false;}";
                modPopPayOptionsAL.RepositionMode = ModalPopupRepositionMode.None;
                Controls.Add(modPopPayOptionsAL);
                Controls.Add(modPopPayOptionsQH);
            }
        }

        // ***********************************************************************************************
        /// <summary>Add Payment Options links where appropriate</summary>
        /// <param name="row">The current Grid Row</param>
        /// <returns>true if the card requires payment options</returns>
        private bool AddCardRowOptions(GridViewRow row)
        {
            bool showPayOptions = false;
            bool showIssueNumber = false;

            var pi = row.DataItem as PaymentInformation;

            if (null != pi && !string.IsNullOrEmpty(pi.CardType))
            {
                string cardType = pi.CardType.TrimEnd();

                if ("AL!QH".IndexOf(cardType) > -1)
                {
                    showPayOptions = true;
                }
                else if ("MS".IndexOf(cardType) == 0)
                {
                    showIssueNumber = true;
                }

                if (showPayOptions)
                {
                    gridViewCardInfo.Columns[5].Visible = true;
                    var lnkPaymentOptions = row.FindControl("lnkPaymentOptions") as LinkButton;

                    //l.OnClientClick = string.Concat("shite; ShowPopup(this); return false");
                    lnkPaymentOptions.Text = lbLumpSum.Text;
                    lnkPaymentOptions.Visible = true;
                    switch (cardType)
                    {
                        case "AL":
                            {
                                lnkPaymentOptions.OnClientClick =
                                    string.Concat("payoptionlink = this;document.getElementById('", btnAL.ClientID,
                                                  "').click(); SetALSelections(); return false;");
                                break;
                            }
                        case "QH":
                            {
                                lnkPaymentOptions.OnClientClick =
                                    string.Concat("payoptionlink = this;document.getElementById('", btnQH.ClientID,
                                                  "').click(); SetQHSelections(); return false");
                                break;
                            }
                        case "AX":
                            {
                                lnkPaymentOptions.Visible = false;
                                break;
                            }
                    }
                }

                if (_enableInstallments)
                {
                    gridViewCardInfo.Columns[9].Visible = true;
                    var currentDateTime = DateUtils.GetCurrentLocalTime(_countryCode);

                    var ddInstallments = row.FindControl("drpInstallments") as DropDownList;

                    int noOfInstallments = 1; // default, one payment exhibition;

                    // Installments
                    if (_enableInstallments)
                    {
                        ddInstallments.Items.Clear();

                        var vp = (((ProductsBase) Page).ShoppingCart.Totals as OrderTotals_V01).VolumePoints;

                        if (_installmentsConfiguration != null)
                        {
                            // Check if it's inside the valid dates
                            if (currentDateTime < _installmentsConfiguration.LastDateTimeToPlaceOrders)
                            {
                                if (_installmentsConfiguration.Cards != null)
                                {
                                    var ci =
                                        (from c in _installmentsConfiguration.Cards.Card
                                         where c.CardId == cardType
                                         select c).FirstOrDefault();

                                    if (ci != null)
                                    {
                                        foreach (
                                            VolumeStrategy volumeStrategy in ci.VolumeStrategy.OrderBy(vs => vs.Volume))
                                        {
                                            if (vp > volumeStrategy.Volume)
                                                continue;

                                            if (currentDateTime > volumeStrategy.EffectiveDates.StartDateTime &&
                                                currentDateTime < volumeStrategy.EffectiveDates.EndDateTime)
                                            {
                                                noOfInstallments = volumeStrategy.MaxInstallments;
                                            }

                                            break;
                                        }
                                    }
                                }
                            } // if (currentDateTime < _installmentsConfiguration.LastDateTimeToPlaceOrders)
                        }

                        ddInstallments.Visible = true;

                        if (noOfInstallments == 1)
                        {
                            ddInstallments.Items.Add(new ListItem("1", "1"));
                            ddInstallments.Enabled = false;
                        }
                        else
                        {
                            for (int i = 1; i <= noOfInstallments; i++)
                            {
                                ddInstallments.Items.Add(new ListItem(i.ToString(), i.ToString()));
                            }
                        }
                    }
                }

                if (showIssueNumber)
                {
                    gridViewCardInfo.Columns[7].Visible = true;
                    (row.FindControl("txtIssueNumber") as TextBox).Style.Remove("display");
                }
            }

            return showPayOptions;
        }

        // ***********************************************************************************************
        /// <summary>Set the Payment Options values to hidden controls and set link text</summary>
        /// <param name="options">The current PaymentOptions object</param>
        /// <param name="row">The current grid row</param>
        private void SetPaymentOptions(PaymentInformation info, GridViewRow row)
        {
            var options = (null != info) ? info.Options as PaymentOptions : null;
            var paymentOption = row.FindControl("txtOption") as TextBox;
            var option1 = row.FindControl("txtChoice1") as TextBox;
            var option2 = row.FindControl("txtChoice2") as TextBox;

            if (null != options)
            {
                var cardType = row.FindControl("lblCardType") as Label;
                var link = row.FindControl("lnkPaymentOptions") as LinkButton;
                var issuer = CreditCard.GetCardType(cardType.Text.Trim());

                var jpo = options as JapanPaymentOptions_V01;
                if (null != jpo)
                {
                    switch (jpo.ChargeMode)
                    {
                        case JapanPayOptionType.LumpSum:
                            {
                                link.Text = (issuer != IssuerAssociationType.AmericanExpress)
                                                ? lbLumpSum.Text
                                                : string.Empty;
                                paymentOption.Text = "1";
                                break;
                            }
                        case JapanPayOptionType.Revolving:
                            {
                                link.Text = lbRevolving.Text;
                                paymentOption.Text = "2";
                                break;
                            }
                        case JapanPayOptionType.Installments:
                            {
                                paymentOption.Text = "3";
                                option1.Text = jpo.NumberOfInstallments.ToString();
                                link.Text = string.Concat(lbInstallments.Text, ": ", option1.Text);
                                break;
                            }
                        case JapanPayOptionType.Bonus1Month:
                            {
                                paymentOption.Text = "4";
                                option1.Text = jpo.FirstBonusMonth.ToString();
                                link.Text = string.Concat(lbBonus1.Text, ": ", option1.Text);
                                break;
                            }
                        case JapanPayOptionType.Bonus2Month:
                            {
                                paymentOption.Text = "5";
                                option1.Text = jpo.FirstBonusMonth.ToString();
                                option2.Text = jpo.SecondBonusMonth.ToString();
                                link.Text = string.Concat(lbBonus2.Text, ": ", option1.Text, " - ", option2.Text);
                                break;
                            }
                        default:
                            {
                                if (issuer == IssuerAssociationType.APlus || issuer == IssuerAssociationType.MyKey ||
                                    issuer == IssuerAssociationType.AmericanExpress)
                                {
                                    link.Text = lbLumpSum.Text;
                                    paymentOption.Text = "1";
                                }
                                break;
                            }
                    }
                }
            }
            else
            {
                paymentOption.Text = string.Empty;
                option1.Text = string.Empty;
                option2.Text = string.Empty;
            }
        }

        // ***********************************************************************************************
        /// <summary>Create the PaymentOptions from the hidden controls</summary>
        /// <param name="paymentOption">Control containing the current option type</param>
        /// <param name="option1">Control containing an option value</param>
        /// <param name="option2">Control containing an option value</param>
        /// <param name="installments">Installments dropdown</param>
        /// <returns></returns>
        private PaymentOptions GetPaymentOptions(TextBox paymentOption,
                                                 TextBox option1,
                                                 TextBox option2,
                                                 DropDownList installments)
        {
            var options = new PaymentOptions_V01();
            int option = 0;
            int choice1 = 0;
            int choice2 = 0;
            if (null != paymentOption)
            {
                Int32.TryParse(paymentOption.Text, out option);
            }
            if (null != option1)
            {
                Int32.TryParse(option1.Text, out choice1);
            }
            if (null != option2)
            {
                Int32.TryParse(option2.Text, out choice2);
            }

            if (option == 3)
            {
                options.NumberOfInstallments = choice1;
            }

            // Brazil
            if (_countryCode == "BR")
            {
                options.NumberOfInstallments = 1; // Default

                if (installments != null)
                {
                    int noInstallments;

                    int.TryParse(installments.SelectedValue, out noInstallments);

                    if (noInstallments == 0)
                        noInstallments = 1;

                    options.NumberOfInstallments = noInstallments;
                }
            }

            // Japan
            if (_countryCode == "JP")
            {
                var optionsJP = new JapanPaymentOptions_V01();
                optionsJP.ChargeMode = (JapanPayOptionType) option;
                switch (optionsJP.ChargeMode)
                {
                    case JapanPayOptionType.Revolving:
                        {
                            //Just let it through
                            break;
                        }
                    case JapanPayOptionType.Installments:
                        {
                            optionsJP.NumberOfInstallments = choice1;
                            optionsJP.FirstInstallmentMonth =
                                new DateTime(DateUtils.GetCurrentLocalTime("JP").AddMonths(1).Ticks).Month;
                            break;
                        }
                    case JapanPayOptionType.Bonus1Month:
                        {
                            optionsJP.FirstBonusMonth = choice1;
                            break;
                        }
                    case JapanPayOptionType.Bonus2Month:
                        {
                            optionsJP.FirstBonusMonth = choice1;
                            optionsJP.SecondBonusMonth = choice2;
                            break;
                        }
                    default:
                        {
                            optionsJP.ChargeMode = JapanPayOptionType.LumpSum;
                            break;
                        }
                }
                options = optionsJP;
            }

            return options;
        }

        // ***********************************************************************************************
        /// <summary>Set the default value (LumpSum) for cards with Japan PaymentOptions</summary>
        /// <param name="cardType">The Card Type</param>
        /// <returns>The default value</returns>
        protected string getDefaultJapanPayOptionValue(object cardType)
        {
            string payOption = string.Empty;
            if (string.Compare(_locale, "ja-jp", true) == 0)
            {
                if (null != cardType)
                {
                    if ("QH.AL.AX".IndexOf((cardType as string).Trim()) > -1)
                    {
                        payOption = "1";
                    }
                }
            }

            return payOption;
        }

        // ***********************************************************************************************
        /// <summary>Calculate and poulate the Bonus Month selections for the Payment Options popups</summary>
        private void SetupBonusMonths()
        {
            if (IsPostBack) return;
            var now = DateUtils.GetCurrentLocalTime(_countryCode);
            int month = now.Month;
            var blankItem = new ListItem(string.Empty, "0");
            //Bonus1 Months
            Bonus1Month.Items.Add(blankItem);
            for (int i = 0; i < 6; ++i)
            {
                var item = now.AddMonths(i + 2);
                string mValue = (item.Month < 10) ? string.Concat("0", item.Month.ToString()) : item.Month.ToString();
                Bonus1Month.Items.Add(new ListItem(item.ToString("MMMM"), item.Month.ToString()));
            }
            //Bonus2 Months 1
            Bonus2Month1.Items.Add(blankItem);
            for (int i = 0; i < 5; ++i)
            {
                var item = now.AddMonths(i + 1);
                string mValue = (item.Month < 10) ? string.Concat("0", item.Month.ToString()) : item.Month.ToString();
                Bonus2Month1.Items.Add(new ListItem(item.ToString("MMMM"), item.Month.ToString()));
            }
            //Bonus2 Months 2
            Bonus2Month2.Items.Add(blankItem);
            for (int i = 0; i < 10; ++i)
            {
                var item = now.AddMonths(i + 2);
                string mValue = (item.Month < 10) ? string.Concat("0", item.Month.ToString()) : item.Month.ToString();
                Bonus2Month2.Items.Add(new ListItem(item.ToString("MMMM"), item.Month.ToString()));
            }
        }

        #endregion Payment Options

        #region Grid display methods

        // ***********************************************************************************************
        private List<PaymentInformation> GetCurrentCardsList(PaymentInformation currentPaymentSelection)
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

            if (_restrictedDisplayCards.Count > 0)
            {
                if (_restrictedDisplayCards.Intersect((from p in _PaymentsList select p.CardType).ToList()).Count() > 0)
                {
                    CheckForRestrictedDisplaySelection(currentPaymentSelection);
                }
            }

            int cardCount = (null != _CurrentPaymentsList) ? _CurrentPaymentsList.Count : 0;
            int maxCards = (_PaymentsList.Count > _maxCardsToDisplay) ? _maxCardsToDisplay : _PaymentsList.Count;
            if (maxCards > 0 && null != _PaymentsList && _PaymentsList.Count > 0)
            {
                cards = new List<PaymentInformation>(_CurrentPaymentsList);
                for (int i = cardCount; i < maxCards; i++)
                {
                    cards.Add(new PaymentInformation());
                }
            }
            return cards;
        }

        // ***********************************************************************************************
        /// <summary>Mask the card number for display</summary>
        /// <param name="cardNum">The card number</param>
        /// <returns>The masked value</returns>
        protected string getCardNumber(string cardNum, string cardType)
        {
            if (string.IsNullOrEmpty(cardNum))
            {
                return string.Empty;
            }
            else
            {
                cardNum = cardNum.Trim();
                return "- " + (cardNum.Length > 4 ? cardNum.Substring(cardNum.Length - 4) : "");
            }
        }

        // ***********************************************************************************************
        /// <summary>
        ///     Mask the card number for display
        /// </summary>
        /// <param name="cardType">Type of the card.</param>
        /// <returns>
        /// </returns>
        protected string getCardName(string cardType)
        {
            string cardName = cardType;
            if (!string.IsNullOrEmpty(cardType))
            {
                cardName =
                    GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                            string.Format("CardType_{0}_Description", cardType)) as string;
            }

            return cardName;
        }

        // ***********************************************************************************************
        /// <summary>Determines if a card has expired</summary>
        /// <param name="exp">The Expiration date of the card</param>
        /// <returns>true if expired</returns>
        protected bool isExpires(DateTime exp)
        {
            var now = DateUtils.GetCurrentLocalTime(_countryCode);
            var lastDate = DateTimeUtils.GetFirstDayOfMonth(now);
            if (exp == DateTime.MinValue)
            {
                return false;
            }
            return exp <= lastDate;
        }

        // ***********************************************************************************************
        /// <summary>Determines if a CVV is reqired for a card</summary>
        /// <param name="cardType">The type of card</param>
        /// <returns>true if required</returns>
        protected bool isCvvVisible(PaymentInformation pi)
        {
            bool visible = true;
            if (null != pi)
            {
                if (pi.CardType == "QH")
                {
                    visible = false;
                }
            }

            return visible;
        }

        // ***********************************************************************************************
        /// <summary>Determines if the grid should not allow multiple cards in an order</summary>
        /// <returns>true if not allowed</returns>
        protected bool isSingleCardOnlyGrid()
        {
            return !_allowMultipleCardsInTransaction;
        }

        public string GetStatusImageUrl(int id)
        {
            string url = string.Empty;
            if (id > 0)
            {
                var pi = _CurrentPaymentsList.Find(p => p.ID == id);
                if (null != pi)
                {
                    switch (pi.AuthorizationFailures)
                    {
                        case 1:
                        case 2:
                            {
                                url = "/Content/global/img/gdo/error-warning-icon.png";
                                break;
                            }
                        case 3:
                            {
                                url = "/Content/global/img/gdo/error-stop-icon.png";
                                break;
                            }
                    }
                    if (isExpires(pi.Expiration))
                    {
                        url = "/Content/global/img/gdo/error-stop-icon.png";
                    }
                }
            }
            return url;
        }

        // ***********************************************************************************************
        public string GetStatusImageToolTip(int id)
        {
            string toolTip = string.Empty;
            if (id > 0)
            {
                var pi = _CurrentPaymentsList.Find(p => p.ID == id);
                if (null != pi)
                {
                    switch (pi.AuthorizationFailures)
                    {
                        case 1:
                        case 2:
                            {
                                toolTip = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardWasDeclined");
                                break;
                            }
                        case 3:
                            {
                                toolTip = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                    "CardExceededAllowedDeclines");
                                break;
                            }
                    }
                    if (isExpires(pi.Expiration))
                    {
                        toolTip = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCard");
                    }
                }
            }
            return toolTip;
        }

        #endregion Grid display methods

        #region Grid Events

        // ***********************************************************************************************
        /// <summary>Grid has bound to card data</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gridViewCardInfo_DataBound(object sender, EventArgs e)
        {
            gridViewCardInfo.Columns[gridViewCardInfo.Columns.Count - 1].Visible = IsEditable();
            SetupBonusMonths();

            string Cvvkey = "CVV" + DistributorID;
            string Amountkey = "Amounts" + DistributorID;
            foreach (GridViewRow row in gridViewCardInfo.Rows)
            {
                var cardId = row.FindControl("cardID") as TextBox;
                var cvv = row.FindControl("txtCVV") as TextBox;
                var txtAmount = row.FindControl("txtAmount") as TextBox;
                if (ViewState[Cvvkey] != null)
                {
                    Dictionary<string, string> CVVs;
                    CVVs = (Dictionary<string, string>) ViewState[Cvvkey];
                    if (null != cardId.Text)
                        if (CVVs.ContainsKey(cardId.Text))
                        {
                            cvv.Text = CVVs[cardId.Text];
                        }
                }
                if (ViewState[Amountkey] != null)
                {
                    Dictionary<string, string> amounts;
                    amounts = (Dictionary<string, string>) ViewState[Amountkey];
                    if (null != cardId.Text)
                        if (amounts.ContainsKey(cardId.Text))
                        {
                            txtAmount.Text = amounts[cardId.Text];
                        }
                }
            }
        }

        // ***********************************************************************************************
        /// <summary>Empty hook </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gridViewCardInfo_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            string s = string.Empty;
        }

        // ***********************************************************************************************
        /// <summary>Event hook for deleting - Use to hide the deleted row - don't ask!</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gridViewCardInfo_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            (sender as GridView).Rows[e.RowIndex].Visible = false;
        }

        /// <summary>Even hook for new grid row - used to configure the Payment Options</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gridViewCardInfo_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            switch (e.Row.RowType)
            {
                case DataControlRowType.DataRow:
                    {
                        var cardId = e.Row.FindControl("cardID") as TextBox;
                        var cardType = e.Row.FindControl("lblCardType") as Label;
                        var cvv = e.Row.FindControl("txtCVV") as TextBox;
                        var cards = e.Row.FindControl("ddlCards") as DropDownList;

                        // Here

                        if (null != cards)
                        {
                            var pi = e.Row.DataItem as PaymentInformation;
                            if (!isCvvVisible(pi))
                            {
                                cvv.Attributes.Add("style", "display:none");
                            }
                            cards.Items.Clear();
                            var currentCards = GetCurrentPaymentInformation(_locale, _distributorId);
                            var currentIds = (null != currentCards)
                                                 ? (from p in currentCards select p.ID).ToList()
                                                 : new List<int>();
                            if (null != pi && !string.IsNullOrEmpty(pi.CardType))
                            {
                                AddCardRowOptions(e.Row);
                                cards.Items.Add(new ListItem(base.GetLocalResourceObject("Select") as string,
                                                             string.Empty));
                                cards.Items.AddRange((from p in _PaymentsList
                                                      where (!currentIds.Contains(p.ID) || p.ID == pi.ID)
                                                      select new ListItem(p.Alias, p.ID.ToString())).ToArray());
                                cards.SelectedValue = pi.ID.ToString();
                            }
                            else
                            {
                                cards.Items.Add(new ListItem(base.GetLocalResourceObject("Select") as string,
                                                             string.Empty));
                                cards.Items.AddRange(
                                    (from p in _PaymentsList
                                     where !currentIds.Contains(p.ID)
                                     select new ListItem(p.Alias, p.ID.ToString())).ToArray());
                                SetupGridRow(e.Row, false);
                            }
                            if (pi.AuthorizationFailures > 0 || isExpires(pi.Expiration))
                            {
                                var img = e.Row.FindControl("imgDeclined") as Image;
                                if (null != img)
                                {
                                    img.ImageUrl = GetStatusImageUrl(pi.ID);
                                    img.ToolTip = GetStatusImageToolTip(pi.ID);
                                    img.Visible = true;
                                }
                            }
                            if (isExpires(pi.Expiration))
                            {
                                if (_RegisteredCardsOnly)
                                {
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                      "CardExpired");
                                }
                                else
                                {
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                      "CardExpiredForSavedCard");
                                }
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
        ///     Get the existing alias or create a temporary one for display
        /// </summary>
        /// <param name="Pi"></param>
        /// <returns></returns>
        private string GetAlias(PaymentInformation Pi)
        {
            string alias = Pi.Alias;
            if (string.IsNullOrEmpty(alias))
            {
                string name = string.Concat(Pi.CardHolder.First + Pi.CardHolder.Last);
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }
                alias = string.Concat(name, " - ", Pi.CardType, " - ", getCardNumber(Pi.CardNumber, Pi.CardType));
            }

            return alias;
        }

        public bool IsEditable()
        {
            return !_RegisteredCardsOnly;
        }

        protected void FillDataCreditCard(object sender, EventArgs e)
        {
            var currentRow = (GridViewRow) ((TextBox) sender).Parent.Parent;
            Dictionary<string, string> Amounts;

            //get values to save in the viewstate
            var cardId = (DropDownList) currentRow.FindControl("ddlCards");
            var amountvalue = (TextBox) currentRow.FindControl("txtAmount");
            //string for unique ViewState
            string skey = "Amounts" + DistributorID;

            if (ViewState[skey] != null)
            {
                Amounts = (Dictionary<string, string>) ViewState[skey];
                if (!Amounts.ContainsKey(cardId.SelectedValue))
                {
                    Amounts.Add(cardId.SelectedValue, amountvalue.Text);
                }
                else
                {
                    Amounts.Remove(cardId.SelectedValue);
                    Amounts.Add(cardId.SelectedValue, amountvalue.Text);
                }
            }
            else
            {
                Amounts = new Dictionary<string, string>();
                Amounts.Add(cardId.SelectedValue, amountvalue.Text);
                ViewState[skey] = Amounts;
            }
            Dictionary<string, string> CVVs;

            //get values to save in the viewstate

            var cvvvalue = (TextBox) currentRow.FindControl("txtCVV");
            //string for unique ViewState
            string skeyCvv = "CVV" + DistributorID;

            if (ViewState[skeyCvv] != null)
            {
                CVVs = (Dictionary<string, string>) ViewState[skeyCvv];

                if (!CVVs.ContainsKey(cardId.SelectedValue))
                {
                    CVVs.Add(cardId.SelectedValue, cvvvalue.Text);
                }
                else
                {
                    CVVs.Remove(cardId.SelectedValue);
                    CVVs.Add(cardId.SelectedValue, cvvvalue.Text);
                }
            }
            else
            {
                CVVs = new Dictionary<string, string>();
                CVVs.Add(cardId.SelectedValue, cvvvalue.Text);
                ViewState[skeyCvv] = CVVs;
            }
        }

        #endregion Grid Events

        #region Client Events

        // ***********************************************************************************************
        public bool IsPaymentError()
        {
            return _paymentError;
        }

        [SubscribesTo(MyHLEventTypes.PaymentInfoChanged)]
        public void OnPaymentInfoChanged(object sender, EventArgs e)
        {
            var totals = GetTotals();
            if (null != totals)
            {
                totalWireDue.Text = DisplayAsCurrency(totals.AmountDue, true);
                totalDirectDepositDue.Text = totalWireDue.Text;
                totalPaymentGatewayDue.Text = totalWireDue.Text;
                txtGrandTotal.Text = totalWireDue.Text;
            }
            UpdatePanel1.Update();
        }

        // ***********************************************************************************************
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCardSelected(object sender, EventArgs e)
        {
            int id = 0;
            int oldId;
            var ddl = sender as NostalgicDropDownList;
            int.TryParse(ddl.PreviousSelectedValue, out oldId);

            foreach (GridViewRow row in gridViewCardInfo.Rows)
            {
                var txtCVV = row.FindControl("txtCVV") as TextBox;
                var txtAmount = row.FindControl("txtAmount") as TextBox;
                var hfId = row.FindControl("cardID") as TextBox;
                var payOption = row.FindControl("txtOption") as TextBox;
                var choice1 = row.FindControl("txtChoice1") as TextBox;
                var choice2 = row.FindControl("txtChoice2") as TextBox;
                var ddlSelected = row.FindControl("ddlCards") as NostalgicDropDownList;
                var ddInstallments = (DropDownList) row.FindControl("drpInstallments");

                if (int.TryParse(hfId.Text, out id) && id > 0)
                {
                    decimal amount;
                    var pi = _CurrentPaymentsList.Find(p => p.ID == id);
                    if (null != pi)
                    {
                        if (decimal.TryParse(txtAmount.Text, out amount))
                        {
                            pi.Amount = amount;
                        }
                        pi.Verification = txtCVV.Text;
                        pi.Options = GetPaymentOptions(payOption, choice1, choice2, ddInstallments);
                    }
                }
            }

            id = 0;
            PaymentInformation pI = null;
            ;
            PaymentInformation currentPaymentSelection = null;
            if (string.IsNullOrEmpty(ddl.SelectedValue))
            {
                pI = _CurrentPaymentsList.Find(p => p.ID == oldId);
                if (null != pI)
                {
                    _CurrentPaymentsList.Remove(pI);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ddl.PreviousSelectedValue))
                {
                    pI = _CurrentPaymentsList.Find(p => p.ID == oldId);
                    if (null != pI)
                    {
                        _CurrentPaymentsList.Remove(pI);
                    }
                }
                if (int.TryParse(ddl.SelectedValue, out id))
                {
                    var pi = _PaymentsList.Find(p => p.ID == id);
                    pi.Amount = 0;
                    pi.Verification = string.Empty;
                    _CurrentPaymentsList.Add(pi);
                    currentPaymentSelection = pi;
                }
            }

            gridViewCardInfo.DataSource = GetCurrentCardsList(currentPaymentSelection);
            gridViewCardInfo.DataBind();
            LoadCards();
            SetupGridRows();
        }

        /// <summary>Sets the selected Payment Method view</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SelectCurrentPaymentView(object sender, EventArgs e)
        {
            var rb = sender as RadioButtonList;
            if (null != rb)
            {
                if (_requiresAcknowledgementToSubmit &&
                    HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmitWireOnly)
                {
                    pnlAcknowledCheckContent.Visible = rb.SelectedValue.StartsWith("3") ||
                                                       rb.SelectedValue.StartsWith("4");
                }
                // Acknowledge check box reset
                chkAcknowledgeTransaction.Checked =
                    chkAcknowledgeTransaction.Checked = !pnlAcknowledCheckContent.Visible ? true : false;
                SetAcknowledgeStyle();

                mpPaymentOptions.SelectedIndex = rb.SelectedIndex;
                pnlBottomMessages.Visible = mpPaymentOptions.SelectedIndex == 0;
                onPaymentOptionsViewChanged(this, e);
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
                            _CurrentPaymentsList = GetCurrentPaymentInformation(_locale, _distributorId);
                            if (null == _CurrentPaymentsList)
                            {
                                _CurrentPaymentsList = new List<PaymentInformation>();
                                var pi = _PaymentsList.Find(p => p.ID == args.PaymentInfo.ID);
                                if (null != pi)
                                {
                                    _CurrentPaymentsList.Add(pi);
                                }
                            }
                            break;
                        }
                    case PaymentInfoCommandType.Edit:
                        {
                            int index = 0;
                            _CurrentPaymentsList = GetCurrentPaymentInformation(_locale, _distributorId);
                            foreach (PaymentInformation pi in _CurrentPaymentsList)
                            {
                                if (pi.ID == args.Id)
                                {
                                    break;
                                }
                                index++;
                            }
                            _CurrentPaymentsList.RemoveAt(index);
                            _CurrentPaymentsList.Insert(index, args.PaymentInfo);
                            break;
                        }
                }
                SetCurrentPaymentInformation(_CurrentPaymentsList, _locale, _distributorId);
            }

            GetPaymentsListFromProvider();
            gridViewCardInfo.DataSource = GetCurrentCardsList(null);
            gridViewCardInfo.DataBind();
            ResolvePayments();
            SetupGridRows();
        }

        private void CheckForPaymentGatewayResponse()
        {
            if (!IsPostBack)
            {
                var response = PaymentGatewayResponse.Create(false);
                if (null != response)
                {
                    _paymentGatewayResponse = response;
                    rblPaymentOptions.SelectedIndex = 1;
                    SelectCurrentPaymentView(rblPaymentOptions, new EventArgs());
                }
            }
        }

        private void GetPaymentsListFromProvider()
        {
            var temp = PaymentInfoProvider.GetPaymentInfo(_distributorId, _locale);
            var sorted = new List<PaymentInformation>();
            if (null != temp && temp.Count > 0)
            {
                foreach (PaymentInformation pi in temp)
                {
                    pi.Alias = GetAlias(pi);
                }
                sorted.AddRange((from p in temp where p.IsPrimary select p).ToList());
                sorted.AddRange((from p in temp where !p.IsPrimary orderby p.Alias select p).ToList());
            }

            _PaymentsList = sorted;
        }

        private void CheckMaxCVV()
        {
            var payments = GetCurrentPaymentInformation(_locale, _distributorId);
            if (null != payments && payments.Count > 0)
            {
                foreach (GridViewRow row in gridViewCardInfo.Rows)
                {
                    var txtCVV = row.FindControl("txtCVV") as TextBox;
                    var txtId = row.FindControl("cardID") as TextBox;
                    int cardId = 0;
                    if (Int32.TryParse(txtId.Text, out cardId) && cardId > 0)
                    {
                        var pi = payments.Find(p => p.ID == cardId);
                        if (null != pi)
                        {
                            if (CreditCard.GetCardType(pi.CardType) == IssuerAssociationType.AmericanExpress)
                            {
                                txtCVV.MaxLength = _maxCVV;
                            }
                        }
                    }
                }
            }
        }

        #endregion Client Events

        #region Validations

        public bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments)
        {
            return ValidateAndGetPayments(shippingAddress, out payments, true);
        }

        public virtual bool ValidateAndGetPayments(Address_V01 shippingAddress,
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
            var orderTotals = GetTotals();

            var currentChoice = GetCurrentPaymentOption();
            if (currentChoice == PaymentOptionChoice.CreditCard)
            {
                if (currentPayments.Count == 0)
                {
                    isValid = false;
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo");
                }
                else if ((from c in currentPayments
                          where
                              null != c as CreditPayment_V01 &&
                              (c as CreditPayment_V01).Card.IssuerAssociation != IssuerAssociationType.MyKey &&
                              string.IsNullOrEmpty((c as CreditPayment_V01).Card.CVV)
                          select c).Count() > 0)
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
            }
            else if (currentChoice == PaymentOptionChoice.PaymentGateway && null != _paymentGatewayControl)
            {
                string errorMessage = string.Empty;
                isValid = _paymentGatewayControl.Validate(out errorMessage);
                lblPaymentGatewayErrorMessage.Text = errorMessage;
                if (isValid)
                {
                    currentPayments = new List<Payment> {_paymentGatewayControl.GetPaymentInfo()};
                }
                if (currentPayments.Count == 1)
                {
                    currentPayments[0].Amount = GetTotals().AmountDue;
                }
            }
            else if (currentChoice == PaymentOptionChoice.WireTransfer)
            {
                SessionInfo.SelectedPaymentChoice = PaymentOptionChoice.WireTransfer.ToString();
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

        [SubscribesTo(MyHLEventTypes.CreditCardAuthorizationCompleted)]
        public void AuthorizationDone(object sender, EventArgs e)
        {
            var allCards = (null != _PaymentsList)
                               ? _PaymentsList
                               : PaymentInfoProvider.GetPaymentInfo(_distributorId, _locale);
            var currentCards = GetCurrentPaymentInformation(_locale, _distributorId);
            if (null != e as CreditCardAuthorizationFailedEventArgs)
            {
                var declines =
                    PaymentInfoProvider.GetFailedCards((e as CreditCardAuthorizationFailedEventArgs).FailedCards,
                                                       _distributorId, _locale);
                //(from p in GetCurrentPaymentInformation(_locale, _distributorId) where p.ID == (from d in declines select d.ID)).ToList())

                foreach (PaymentInformation pi in allCards)
                {
                    pi.Verification = string.Empty;
                    foreach (PaymentInformation card in declines)
                    {
                        if (card.ID == pi.ID)
                        {
                            pi.AuthorizationFailures++;
                            break;
                        }
                    }
                }
                var args = e as CreditCardAuthorizationFailedEventArgs;
                if (declines.Count > 0 && args.NoOfCraditcardLines != declines.Count)
                {
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                  _paymentsConfig
                                                                                  .AllowMultipleCardsInTransaction ?
                                                                                  _paymentsConfig.AllowMultipleCardsForNAMerrorMessage ? "OneOrMoreCardsWereDeclinedForMultiplecards" : "OneOrMoreCardsWereDeclined" : "OneOrMoreCardsWereDeclined");
                }
                else
                {
                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                "OneOrMoreCardsWereDeclined");
                }
            }
            else
            {
                if (null != currentCards)
                {
                    SetCurrentPaymentInformation(new List<PaymentInformation>(), _locale, _distributorId);
                    foreach (PaymentInformation pi in _PaymentsList)
                    {
                        pi.Amount = 0;
                        pi.Verification = string.Empty;
                    }
                    return;
                }
            }

            OnCardAddOrEdit(null, null);
        }

        private void ShowMessages()
        {
            lblCurrencySymbol.Text = _currencySymbol;
            if (null == _PaymentsList || _PaymentsList.Count == 0)
            {
                if (_RegisteredCardsOnly)
                {
                    if (rblPaymentOptions.SelectedIndex == 0)
                    {
                        lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCard");
                    }
                }
                //else
                //{
                //    lblErrorMessages.Text = string.Empty;
                //}
            }
        }

        private void CheckForRestrictedDisplaySelection(PaymentInformation currentPaymentSelection)
        {
            if (null != currentPaymentSelection)
            {
                if (_allowMultipleCardsInTransaction)
                {
                    if (_restrictedDisplayCards.Contains(currentPaymentSelection.CardType))
                    {
                        _allowMultipleCardsInTransaction = false;
                        _maxCardsToDisplay = 1;
                        _CurrentPaymentsList.RemoveAll(p => p.ID != currentPaymentSelection.ID);
                    }
                }
            }
            else
            {
                var primary = _PaymentsList.Where(p => p.IsPrimary).FirstOrDefault();
                if (null != primary && _restrictedDisplayCards.Contains(primary.CardType))
                {
                    _allowMultipleCardsInTransaction = false;
                    _maxCardsToDisplay = 1;
                }
            }
        }

        private string DisplayAsCurrency(decimal amount, bool showCurrencySymbol)
        {
            if (showCurrencySymbol)
            {
                return getAmountString(amount, true);
            }
            else
            {
                return (_allowDecimal) ? string.Format("{0:F2}", amount) : Convert.ToInt32(amount).ToString();
            }
        }

        protected string GetCVVHelpLink()
        {
            return string.Concat(@"/content/", _locale, @"/pdf/ordering/cvvhelp.pdf");
        }

        #endregion Validations
    }
}