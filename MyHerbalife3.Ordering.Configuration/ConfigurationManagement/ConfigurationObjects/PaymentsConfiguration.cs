using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class PaymentsConfiguration : HLConfiguration
    {
        #region Consts

        private const string CURR_PAYMENTINFO = "CurrentPaymentInfo_";
        private const string PAYMENT_INFO_KEY = "PaymentInfo_";

        #endregion

        #region Construction

        public static PaymentsConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "Payments") as PaymentsConfiguration;
        }

        #endregion

        #region Static methods

        public static string GetPaymentInfoSessionKey(string distributorID, string locale)
        {
            return string.Concat(PAYMENT_INFO_KEY, "_", locale, distributorID);
        }

        public static string GetCurrentPaymentSessionKey(string locale, string distributorID)
        {
            return string.Concat(CURR_PAYMENTINFO, locale, "_", distributorID);
        }

        #endregion

        #region PGH Config Properties
        /// <summary>Whether this Locale is using the hub</summary>
        [ConfigurationProperty("isUsingHub")]
       public bool IsUsingHub
        {
            get { return (bool)this["isUsingHub"]; }
            set { this["isUsingHub"] = value; }
        }

        /// <summary>PaymentGatewayReturnUrl for PGH response</summary>
        [ConfigurationProperty("method")]
        public string Method
        {
            get { return this["method"] as string; }
            set { this["method"] = value; }
        }

        /// <summary>convertAmountDue for PGH request</summary>
        [ConfigurationProperty("convertAmountDue")]
        public bool ConvertAmountDue
        {
            get { return (bool)this["convertAmountDue"]; }
            set { this["convertAmountDue"] = value; }
        }

        /// <summary>PaymentGatewayReturnUrl for PGH response</summary>
        [ConfigurationProperty("paymentGatewayReturnUrl")]
        public string PaymentGatewayReturnUrl
        {
            get { return this["paymentGatewayReturnUrl"] as string; }
            set { this["paymentGatewayReturnUrl"] = value; }
        }

        /// <summary>PaymentGateway CallBackUrl for PGH callbacks</summary>
        [ConfigurationProperty("paymentGatewayCallBackUrl")]
        public string PaymentGatewayCallBackUrl
        {
            get { return this["paymentGatewayCallBackUrl"] as string; }
            set { this["paymentGatewayCallBackUrl"] = value; }
        }

        /// <summary>Let PGH submit the order if it is approved</summary>
        [ConfigurationProperty("submitOnAuthorization")]
        public bool SubmitOnAuthorization
        {
            get { return (bool)this["submitOnAuthorization"]; }
            set { this["submitOnAuthorization"] = value; }
        }

        /// <summary>Not interested in Callbacks from the agency</summary>
        [ConfigurationProperty("suppressCallBack")]
        public bool SuppressCallBack
        {
            get { return (bool)this["suppressCallBack"]; }
            set { this["suppressCallBack"] = value; }
        }

        /// <summary>ClientKey for PGH </summary>
        [ConfigurationProperty("clientKey")]
        public string ClientKey
        {
            get { return this["clientKey"] as string; }
            set { this["clientKey"] = value; }
        }
        #endregion

        #region Config Properties

        [ConfigurationProperty("allowMultipleCardsInTransaction")]
        public bool AllowMultipleCardsInTransaction
        {
            get { return (bool) this["allowMultipleCardsInTransaction"]; }
            set { this["allowMultipleCardsInTransaction"] = value; }
        }

        [ConfigurationProperty("allowWirePayment")]
        public bool AllowWirePayment
        {
            get { return (bool) this["allowWirePayment"]; }
            set { this["allowWirePayment"] = value; }
        }

        [ConfigurationProperty("allowQuickPayPayment", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowQuickPayPayment
        {
            get { return (bool)this["allowQuickPayPayment"]; }
            set { this["allowQuickPayPayment"] = value; }
        }

        [ConfigurationProperty("allowCNPPayment", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowCNPPayment
        {
            get { return (bool)this["allowCNPPayment"]; }
            set { this["allowCNPPayment"] = value; }
        }

        [ConfigurationProperty("allowWireTransferAutoOrderRelease", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowWireTransferAutoOrderRelease
        {
            get { return (bool)this["allowWireTransferAutoOrderRelease"]; }
            set { this["allowWireTransferAutoOrderRelease"] = value; }
        }

        [ConfigurationProperty("allowDirectDepositPayment")]
        public bool AllowDirectDepositPayment
        {
            get { return (bool) this["allowDirectDepositPayment"]; }
            set { this["allowDirectDepositPayment"] = value; }
        }

        /// <summary>
        /// Enable Net Banking payments
        /// </summary>
        [ConfigurationProperty("allowNetBankingPayment", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowNetBankingPayment
        {
            get { return (bool)this["allowNetBankingPayment"]; }
            set { this["allowNetBankingPayment"] = value; }
        }

        /// <summary>
        ///     The flag if Payment Gateway is disabled
        /// </summary>
        [ConfigurationProperty("disablePaymentGateway")]
        public bool DisablePaymentGateway
        {
            get { return (bool) this["disablePaymentGateway"]; }
            set { this["disablePaymentGateway"] = value; }
        }

        /// <summary>
        ///     use reg card
        /// </summary>
        [ConfigurationProperty("useCardRegistry")]
        public bool UseCardRegistry
        {
            get { return (bool) this["useCardRegistry"]; }
            set { this["useCardRegistry"] = value; }
        }

        /// <summary>
        ///     max number of cards to display
        /// </summary>
        [ConfigurationProperty("maxCardsToDisplay", DefaultValue = 10, IsRequired = false)]
        public int MaxCardsToDisplay
        {
            get { return (int) this["maxCardsToDisplay"]; }
            set { this["maxCardsToDisplay"] = value; }
        }

        /// <summary>
        ///     max number of digits for cvv
        /// </summary>
        [ConfigurationProperty("maxCVV", DefaultValue = 3, IsRequired = false)]
        public int MaxCVV
        {
            get { return (int) this["maxCVV"]; }
            set { this["maxCVV"] = value; }
        }

        /// <summary>
        ///     if allow decimal in amount field
        /// </summary>
        [ConfigurationProperty("allowDecimal", DefaultValue = true, IsRequired = false)]
        public bool AllowDecimal
        {
            get { return (bool) this["allowDecimal"]; }
            set { this["allowDecimal"] = value; }
        }

        /// <summary>
        ///     The Wire Payment Code
        /// </summary>
        [ConfigurationProperty("wirePaymentCodes", DefaultValue = "WR", IsRequired = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> WirePaymentCodes
        {
            get { return this["wirePaymentCodes"] as List<string>; }
            set { this["wirePaymentCodes"] = value; }
        }

        /// <summary>
        ///     The Wire Payment Alias
        /// </summary>
        [ConfigurationProperty("wirePaymentAliases", DefaultValue = "Wire", IsRequired = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> WirePaymentAliases
        {
            get { return this["wirePaymentAliases"] as List<string>; }
            set { this["wirePaymentAliases"] = value; }
        }

        /// <summary>
        ///     The Direct Deposite Payment Code
        /// </summary>
        [ConfigurationProperty("directDepositPaymentCodes", DefaultValue = "DD", IsRequired = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> DirectDepositPaymentCodes
        {
            get { return this["directDepositPaymentCodes"] as List<string>; }
            set { this["directDepositPaymentCodes"] = value; }
        }

        /// <summary>
        ///     if allow decimal in amount field
        /// </summary>
        [ConfigurationProperty("directDepositPaymentAliases", DefaultValue = "Direct Deposit", IsRequired = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> DirectDepositPaymentAliases
        {
            get { return this["directDepositPaymentAliases"] as List<string>; }
            set { this["directDepositPaymentAliases"] = value; }
        }

        /// <summary>
        ///     MerchantAccount if necessary to supply it
        /// </summary>
        [ConfigurationProperty("merchantAccountName")]
        public string MerchantAccountName
        {
            get { return this["merchantAccountName"] as string; }
            set { this["merchantAccountName"] = value; }
        }

        /// <summary>
        ///     if we will allow HardCash buyer to use Wire
        /// </summary>
        [ConfigurationProperty("allowWireForHardCash")]
        public bool AllowWireForHardCash
        {
            get { return (bool) this["allowWireForHardCash"]; }
            set { this["allowWireForHardCash"] = value; }
        }

        /// <summary>
        ///     if we will allow HardCash buyer to use CC
        /// </summary>
        [ConfigurationProperty("allowCreditCardForHardCash", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowCreditCardForHardCash
        {
            get { return (bool)this["allowCreditCardForHardCash"]; }
            set { this["allowCreditCardForHardCash"] = value; }
        }

        /// <summary>
        ///     has only CC payment option
        /// </summary>
        [ConfigurationProperty("hasOnlyCreditCardOption", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasOnlyCreditCardOption
        {
            get { return (bool)this["hasOnlyCreditCardOption"]; }
            set { this["hasOnlyCreditCardOption"] = value; }
        }

        /// <summary>
        ///     hard cash
        /// </summary>
        [ConfigurationProperty("allowPurchaseForHardCash", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowPurchaseForHardCash
        {
            get { return (bool) this["allowPurchaseForHardCash"]; }
            set { this["allowPurchaseForHardCash"] = value; }
        }

        /// <summary>
        /// Message to be displayed in checkout page with special instructions for Payment Gateways like timeout
        /// </summary>
        [ConfigurationProperty("displayPaymentGatewayMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayPaymentGatewayMessage
        {
            get { return (bool)this["displayPaymentGatewayMessage"]; }
            set { this["displayPaymentGatewayMessage"] = value; }
        }
        [ConfigurationProperty("multipleWireMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool MultipleWireMessage
        {
            get { return (bool)this["multipleWireMessage"]; }
            set { this["multipleWireMessage"] = value; }
        }

        /// <summary>The Credit Payment Gateway Invoker</summary>
        [ConfigurationProperty("paymentGatewayInvoker")]
        public string PaymentGatewayInvoker
        {
            get { return this["paymentGatewayInvoker"] as string; }
            set { this["paymentGatewayInvoker"] = value; }
        }

        /// <summary>The Credit Payment Gateway Url</summary>
        [ConfigurationProperty("paymentGatewayUrl")]
        public string PaymentGatewayUrl
        {
            get { return this["paymentGatewayUrl"] as string; }
            set { this["paymentGatewayUrl"] = value; }
        }

        /// <summary>
        ///     The Credit Payment Gateway Password
        /// </summary>
        [ConfigurationProperty("paymentGatewayPassword")]
        public string PaymentGatewayPassword
        {
            get { return this["paymentGatewayPassword"] as string; }
            set { this["paymentGatewayPassword"] = value; }
        }

        /// <summary>
        ///     The Credit Payment Gateway Retrun Url for Authorization
        /// </summary>
        [ConfigurationProperty("paymentGatewayReturnUrlApproved")]
        public string PaymentGatewayReturnUrlApproved
        {
            get { return this["paymentGatewayReturnUrlApproved"] as string; }
            set { this["paymentGatewayReturnUrlApproved"] = value; }
        }

        /// <summary>
        ///     The Credit Payment Gateway Retrun Url for Decline
        /// </summary>
        [ConfigurationProperty("paymentGatewayReturnUrlDeclined")]
        public string PaymentGatewayReturnUrlDeclined
        {
            get { return this["paymentGatewayReturnUrlDeclined"] as string; }
            set { this["paymentGatewayReturnUrlDeclined"] = value; }
        }

        /// <summary>
        ///     The Credit Payment Gateway Encryption Key
        /// </summary>
        [ConfigurationProperty("paymentGatewayEncryptionKey")]
        public string PaymentGatewayEncryptionKey
        {
            get { return this["paymentGatewayEncryptionKey"] as string; }
            set { this["paymentGatewayEncryptionKey"] = value; }
        }

        /// <summary>
        ///     if use Payment Gateway
        /// </summary>
        [ConfigurationProperty("hasPaymentGateway")]
        public bool HasPaymentGateway
        {
            get { return (bool) this["hasPaymentGateway"]; }
            set { this["hasPaymentGateway"] = value; }
        }

        /// <summary>
        ///     The Payment methods for which to use the Payment Gateway
        /// </summary>
        [ConfigurationProperty("paymentGatewayPaymentMethods")]
        public string PaymentGatewayPaymentMethods
        {
            get { return this["paymentGatewayPaymentMethods"] as string; }
            set { this["paymentGatewayPaymentMethods"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway display style
        /// </summary>
        [ConfigurationProperty("paymentGatewayStyle")]
        public string PaymentGatewayStyle
        {
            get { return this["paymentGatewayStyle"] as string; }
            set { this["paymentGatewayStyle"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway Alias
        /// </summary>
        [ConfigurationProperty("paymentGatewayAlias")]
        public string PaymentGatewayAlias
        {
            get { return this["paymentGatewayAlias"] as string; }
            set { this["paymentGatewayAlias"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway Pay Code
        /// </summary>
        [ConfigurationProperty("paymentGatewayPayCode")]
        public string PaymentGatewayPayCode
        {
            get { return this["paymentGatewayPayCode"] as string; }
            set { this["paymentGatewayPayCode"] = value; }
        }

        /// <summary>
        ///     The flag for certain Payment Gateway who reqires to round total amount
        /// </summary>
        [ConfigurationProperty("roundTotalForDumbassPaymentGateway")]
        public bool RoundTotalForDumbassPaymentGateway
        {
            get { return (bool) this["roundTotalForDumbassPaymentGateway"]; }
            set { this["roundTotalForDumbassPaymentGateway"] = value; }
        }

        /// <summary>
        ///     The Flag for whether address is required for new credit card
        /// </summary>
        [ConfigurationProperty("addressRequiredForNewCard")]
        public bool AddressRequiredForNewCard
        {
            get { return (bool) this["addressRequiredForNewCard"]; }
            set { this["addressRequiredForNewCard"] = value; }
        }

        /// <summary>
        ///     if allow saved credit cards
        /// </summary>
        [ConfigurationProperty("allowSavedCards", DefaultValue = true, IsRequired = false)]
        public bool AllowSavedCards
        {
            get { return (bool) this["allowSavedCards"]; }
            set { this["allowSavedCards"] = value; }
        }

        /// <summary>
        ///     if allow saved credit cards with Address
        /// </summary>
        [ConfigurationProperty("allowSavedCardsWithAddress", DefaultValue = true, IsRequired = false)]
        public bool AllowSavedCardsWithAddress
        {
            get { return (bool)this["allowSavedCardsWithAddress"]; }
            set { this["allowSavedCardsWithAddress"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway Application ID.
        /// </summary>
        [ConfigurationProperty("paymentGatewayApplicationId")]
        public string PaymentGatewayApplicationId
        {
            get { return this["paymentGatewayApplicationId"] as string; }
            set { this["paymentGatewayApplicationId"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway Application ID.
        /// </summary>
        [ConfigurationProperty("paymentGatewayControl")]
        public string PaymentGatewayControl
        {
            get { return this["paymentGatewayControl"] as string; }
            set { this["paymentGatewayControl"] = value; }
        }

        /// <summary>
        ///     The Payment Gateway Mode: 1 is Live, 0 is Test
        /// </summary>
        [ConfigurationProperty("paymentGatewayMode", DefaultValue = "1", IsRequired = false)]
        public string PaymentGatewayMode
        {
            get { return this["paymentGatewayMode"] as string; }
            set { this["paymentGatewayMode"] = value; }
        }

        /// <summary>
        ///     if allow saved credit cards
        /// </summary>
        [ConfigurationProperty("paymentGatewayControlHasCardData")]
        public bool PaymentGatewayControlHasCardData
        {
            get { return (bool)this["paymentGatewayControlHasCardData"]; }
            set { this["paymentGatewayControlHasCardData"] = value; }
        }

        /// <summary>
        ///     if allow saved credit cards
        /// </summary>
        [ConfigurationProperty("showPaymentAmountsInSummary")]
        public bool ShowPaymentAmountsInSummary
        {
            get { return (bool) this["showPaymentAmountsInSummary"]; }
            set { this["showPaymentAmountsInSummary"] = value; }
        }

        /// <summary>
        ///     if allow saved credit cards
        /// </summary>
        [ConfigurationProperty("showPaymentInfoForPaymentGatewayInSummary")]
        public bool ShowPaymentInfoForPaymentGatewayInSummary
        {
            get { return (bool) this["showPaymentInfoForPaymentGatewayInSummary"]; }
            set { this["showPaymentInfoForPaymentGatewayInSummary"] = value; }
        }

        /// <summary>
        ///     The control to use for Credit Card payments
        /// </summary>
        [ConfigurationProperty("paymentOptionsControl",
            DefaultValue = "~/Ordering/Controls/Payments/PaymentInfoGrid.ascx", IsRequired = false, IsKey = false)]
        public string PaymentOptionsControl
        {
            get { return (string) this["paymentOptionsControl"]; }
            set { this["paymentOptionsControl"] = value; }
        }

        /// <summary>
        ///     The control to use show declained payment information
        /// </summary>
        [ConfigurationProperty("paymentDeclinedInfoControl",
            DefaultValue = "~/Ordering/Controls/Checkout/PaymentDeclinedInfo.ascx", IsRequired = false, IsKey = false)]
        public string PaymentDeclinedInfoControl
        {
            get { return (string)this["paymentDeclinedInfoControl"]; }
            set { this["paymentDeclinedInfoControl"] = value; }
        }
        

        /// <summary>
        ///     The control to add a new credit card.
        /// </summary>
        [ConfigurationProperty("paymentInfoControl",
            DefaultValue = "~/Ordering/Controls/Payments/PaymentInfoControl.ascx", IsRequired = false, IsKey = false)]
        public string PaymentInfoControl
        {
            get { return (string) this["paymentInfoControl"]; }
            set { this["paymentInfoControl"] = value; }
        }

        /// <summary>
        ///     The control to use for payments summary
        /// </summary>
        [ConfigurationProperty("paymentsSummaryControl",
            DefaultValue = "~/Ordering/Controls/Checkout/PaymentsSummary.ascx", IsRequired = false, IsKey = false)]
        public string PaymentsSummaryControl
        {
            get { return (string) this["paymentsSummaryControl"]; }
            set { this["paymentsSummaryControl"] = value; }
        }

        [ConfigurationProperty("showExpirationDateInPaymentSummary", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool ShowExpirationDateInPaymentSummary
        {
            get { return (bool) this["showExpirationDateInPaymentSummary"]; }
            set { this["showExpirationDateInPaymentSummary"] = value; }
        }

        [ConfigurationProperty("enableInstallments", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool EnableInstallments
        {
            get { return (bool) this["enableInstallments"]; }
            set { this["enableInstallments"] = value; }
        }

        /// <summary>
        /// Defines the flag indicating if the installment's datasource is local (xml file) or not (ordering admin).
        /// </summary>
        [ConfigurationProperty("localInstallmentsSource", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool LocalInstallmentsSource
        {
            get { return (bool)this["localInstallmentsSource"]; }
            set { this["localInstallmentsSource"] = value; }
        }

        /// <summary>
        ///     if allow decimal in amount field
        /// </summary>
        [ConfigurationProperty("restrictedDisplayCards", DefaultValue = "", IsRequired = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> RestrictedDisplayCards
        {
            get { return this["restrictedDisplayCards"] as List<string>; }
            set { this["restrictedDisplayCards"] = value; }
        }

        /// <summary>
        ///     To enable the F-control info validation.
        /// </summary>
        [ConfigurationProperty("fControlValidation", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool FControlValidation
        {
            get { return (bool) this["fControlValidation"]; }
            set { this["fControlValidation"] = value; }
        }

        /// <summary>
        ///     if use 3D Secured credit card
        /// </summary>
        [ConfigurationProperty("use3DSecuredCreditCard", DefaultValue = false, IsRequired = false)]
        public bool Use3DSecuredCreditCard
        {
            get { return (bool) this["use3DSecuredCreditCard"]; }
            set { this["use3DSecuredCreditCard"] = value; }
        }

        /// <summary>
        ///     if 3D Secured credit card need verification when post back by 3D password authentication popup
        /// </summary>
        [ConfigurationProperty("use3DVerification", DefaultValue = false, IsRequired = false)]
        public bool Use3DVerification
        {
            get { return (bool)this["use3DVerification"]; }
            set { this["use3DVerification"] = value; }
        }

        /// <summary>
        ///     if 3D Secured credit card need checking ECI response when post back by 3D password authentication popup
        /// </summary>
        [ConfigurationProperty("check3DPaymentEci", DefaultValue = false, IsRequired = false)]
        public bool Check3DPaymentEci
        {
            get { return (bool)this["check3DPaymentEci"]; }
            set { this["check3DPaymentEci"] = value; }
        }

        /// <summary>Show Grand Total with custom style</summary>
        [ConfigurationProperty("showBigGrandTotal", DefaultValue = false, IsRequired = false)]
        public bool ShowBigGrandTotal
        {
            get { return (bool)this["showBigGrandTotal"]; }
            set { this["showBigGrandTotal"] = value; }
        }

        /// <summary>Restrict payment with American Express credit card</summary>
        [ConfigurationProperty("restrictAmexCard", DefaultValue = false, IsRequired = false)]
        public bool RestrictAmexCard
        {
            get { return (bool)this["restrictAmexCard"]; }
            set { this["restrictAmexCard"] = value; }
        }
        /// <summary>Multiple Cards for NAM</summary>
        [ConfigurationProperty("allowMultipleCardsforNAMerrorMessage", DefaultValue = false, IsRequired = false)]
        public bool AllowMultipleCardsForNAMerrorMessage
        {
            get { return (bool)this["allowMultipleCardsforNAMerrorMessage"]; }
            set { this["allowMultipleCardsforNAMerrorMessage"] = value; }
        }

        #region Region Hide BR Payment Gateway Options

        /// <summary>
        ///     To hide the Bank Slip option for BR payment
        /// </summary>
        [ConfigurationProperty("hideBankSlip", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideBankSlip
        {
            get { return (bool) this["hideBankSlip"]; }
            set { this["hideBankSlip"] = value; }
        }

        /// <summary>
        ///     To hide the Itau option for BR payment
        /// </summary>
        [ConfigurationProperty("hideItau", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideItau
        {
            get { return (bool) this["hideItau"]; }
            set { this["hideItau"] = value; }
        }

        /// <summary>
        ///     To hide the Bradesco option for BR payment
        /// </summary>
        [ConfigurationProperty("hideBradesco", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideBradesco
        {
            get { return (bool) this["hideBradesco"]; }
            set { this["hideBradesco"] = value; }
        }

        /// <summary>
        ///     To hide the BancodoBrazil option for BR payment
        /// </summary>
        [ConfigurationProperty("hideBancodoBrazil", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideBancodoBrazil
        {
            get { return (bool) this["hideBancodoBrazil"]; }
            set { this["hideBancodoBrazil"] = value; }
        }

        /// <summary>
        /// To hide the Visa Electron option for BR payment
        /// </summary>
        [ConfigurationProperty("hideVisaElectron", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideVisaElectron
        {
            get { return (bool)this["hideVisaElectron"]; }
            set { this["hideVisaElectron"] = value; }
        }

        #endregion

        #region Region Hide EC Payment Gateway Options

        /// <summary>
        ///     To hide the Diners option for EC payment
        /// </summary>
        [ConfigurationProperty("hidePayclub", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HidePayclub
        {
            get { return (bool) this["hidePayclub"]; }
            set { this["hidePayclub"] = value; }
        }

        /// <summary>
        ///     To hide the Visa/MC option for EC payment
        /// </summary>
        [ConfigurationProperty("hideProdubanco", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideProdubanco
        {
            get { return (bool) this["hideProdubanco"]; }
            set { this["hideProdubanco"] = value; }
        }

        #endregion

        #region Region Hide CL Payment Gateway Options

        /// <summary>
        ///     To hide the ServiPag option for CL payment
        /// </summary>
        [ConfigurationProperty("hideCL_ServiPag", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideCL_ServiPag
        {
            get { return (bool) this["hideCL_ServiPag"]; }
            set { this["hideCL_ServiPag"] = value; }
        }

        /// <summary>
        ///     To hide the WebPay option for CL payment
        /// </summary>
        [ConfigurationProperty("hideWebPay", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideWebPay
        {
            get { return (bool) this["hideWebPay"]; }
            set { this["hideWebPay"] = value; }
        }

        /// <summary>
        /// To hide the PagosOnLine option for Visa,MC, Diners, Amex, VisaDebit payment
        /// </summary>
        [ConfigurationProperty("hidePagosonLine", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HidePagosonLine
        {
            get { return (bool)this["hidePagosonLine"]; }
            set { this["hidePagosonLine"] = value; }
        }

        /// <summary>
        /// To hide the Pse option for CO payment
        /// </summary>
        [ConfigurationProperty("hidePse", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HidePse
        {
            get { return (bool)this["hidePse"]; }
            set { this["hidePse"] = value; }
        }

        /// <summary>
        /// Maximum purchase amount for VAT discount (UY)
        /// </summary>
        [ConfigurationProperty("maxAmountForVATDiscount", DefaultValue = "0.0", IsRequired = false)]
        public string MaxAmountForVATDiscount
        {
            get { return this["maxAmountForVATDiscount"] as string; }
            set { this["maxAmountForVATDiscount"] = value; }
        }

        /// To Send the confirmation email 
        /// </summary>
        [ConfigurationProperty("sendConfirmationEmail", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SendConfirmationEmail
        {
            get { return (bool)this["sendConfirmationEmail"]; }
            set { this["sendConfirmationEmail"] = value; }
        }

        /// <summary>
        /// To Set the declinedOrderNumber 
        /// </summary>
        [ConfigurationProperty("setDeclinedOrderNumber", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SetDeclinedOrderNumber
        {
            get { return (bool)this["setDeclinedOrderNumber"]; }
            set { this["setDeclinedOrderNumber"] = value; }
        }


        #endregion

        #region Region Hide UY Payment Gateway Options

        /// <summary>
        ///     To hide the VISA  option for UY payment
        /// </summary>
        [ConfigurationProperty("hideVpayment", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideVpayment
        {
            get { return (bool) this["hideVpayment"]; }
            set { this["hideVpayment"] = value; }
        }

        /// <summary>
        /// To hide the OCA option for UY payment
        /// </summary>
        [ConfigurationProperty("hideOca", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideOca
        {
            get { return (bool)this["hideOca"]; }
            set { this["hideOca"] = value; }
        }

        #endregion

        #region Region Hide PY Payment Gateway Options


        /// <summary>
        /// To hide the Brancard option for PY payment
        /// </summary>
        [ConfigurationProperty("hideBancard", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideBancard
        {
            get { return (bool)this["hideBancard"]; }
            set { this["hideBancard"] = value; }
        }

        #endregion

        #region Region Switch BR Agencies

        /// <summary>
        ///     To disable  Itau BR payment Response
        /// </summary>
        [ConfigurationProperty("disableItau", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableItau
        {
            get { return (bool) this["disableItau"]; }
            set { this["disableItau"] = value; }
        }

        /// <summary>
        ///     To disable  Bank Slip BR payment Response
        /// </summary>
        [ConfigurationProperty("disableBankSlip", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableBankSlip
        {
            get { return (bool) this["disableBankSlip"]; }
            set { this["disableBankSlip"] = value; }
        }

        /// <summary>
        ///     To disable  Bradesco BR payment Response
        /// </summary>
        [ConfigurationProperty("disableBradesco", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableBradesco
        {
            get { return (bool) this["disableBradesco"]; }
            set { this["disableBradesco"] = value; }
        }

        /// <summary>
        ///     To disable  BancodoBrazil BR payment Response
        /// </summary>
        [ConfigurationProperty("disableBancodoBrazil", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableBancodoBrazil
        {
            get { return (bool) this["disableBancodoBrazil"]; }
            set { this["disableBancodoBrazil"] = value; }
        }

        [ConfigurationProperty("hideCreditCardOption", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideCreditCardOption
        {
            get { return (bool) this["hideCreditCardOption"]; }
            set { this["hideCreditCardOption"] = value; }
        }

        /// <summary>
        ///     Turn On  Tivit BankSlip   => false means using boldcron true means using Tivit
        /// </summary>
        [ConfigurationProperty("turnOnTivitBankSlip", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool TurnOnTivitBankSlip
        {
            get { return (bool) this["turnOnTivitBankSlip"]; }
            set { this["turnOnTivitBankSlip"] = value; }
        }

        /// <summary>
        ///     Turn On  Tivit BankSlip   => false means using boldcron true means using Tivit
        /// </summary>
        [ConfigurationProperty("turnOnBrasPagBankSlip", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool TurnOnBrasPagBankSlip
        {
            get { return (bool)this["turnOnBrasPagBankSlip"]; }
            set { this["turnOnBrasPagBankSlip"] = value; }
        }

        #endregion

        #region Region Disable Sale/Auth CR, GT, SV , PA, HN, NI

        /// <summary>
        ///     To disable Sale Transaction Type Only for Credomatic Agency
        /// </summary>
        [ConfigurationProperty("disableSale", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableSale
        {
            get { return (bool) this["disableSale"]; }
            set { this["disableSale"] = value; }
        }

        #endregion

        #region US: Disable PayNearMe
        [ConfigurationProperty("allowPayNearMe", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowPayNearMe
        {
            get { return (bool)this["allowPayNearMe"]; }
            set { this["allowPayNearMe"] = value; }
        }
        #endregion

        #region PF
        /// <summary>
        /// Round Amount Due
        ///     1 to round 1 integer up
        ///     -1 to round 1 integer down
        ///     0 normal rounding
        /// </summary>
        [ConfigurationProperty("roundAmountDue", DefaultValue = "None", IsRequired = false)]
        public string RoundAmountDue
        {
            get { return (string)this["roundAmountDue"]; }
            set { this["roundAmountDue"] = value; }
        }

        /// <summary>
        /// DisplayConvertedAmount
        /// false shows amount with no conversion        
        /// </summary>
        [ConfigurationProperty("displayConvertedAmount", DefaultValue = true, IsRequired = false)]
        public bool DisplayConvertedAmount
        {
            get { return (bool)this["displayConvertedAmount"]; }
            set { this["displayConvertedAmount"] = value; }
        }
        #endregion

        #region CZ, UA
        /// <summary>
        /// Hides Issuer number column in PaymentInfoGrid        
        /// </summary>
        [ConfigurationProperty("showIssuerNumberColumn", DefaultValue = true, IsRequired = false)]
        public bool ShowIssuerNumberColumn
        {
            get { return (bool)this["showIssuerNumberColumn"]; }
            set { this["showIssuerNumberColumn"] = value; }
        }
        #endregion

        [ConfigurationProperty("showCCInfoFromPGResponse", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowCCInfoFromPGResponse
        {
            get { return (bool)this["showCCInfoFromPGResponse"]; }
            set { this["showCCInfoFromPGResponse"] = value; }
        }

        [ConfigurationProperty("canSubmitPending", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool CanSubmitPending
        {
            get { return (bool)this["canSubmitPending"]; }
            set { this["canSubmitPending"] = value; }
        }
        [ConfigurationProperty("PendingOrderhascontent", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PendingOrderhascontent
        {
            get { return (bool)this["PendingOrderhascontent"]; }
            set { this["PendingOrderhascontent"] = value; }
        }

        [ConfigurationProperty("gatewayName", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string GatewayName
        {
            get { return (string)this["gatewayName"]; }
            set { this["gatewayName"] = value; }
        }

        /// <summary>
        /// Defines the Merchant name for FPX payments.
        /// </summary>
        [ConfigurationProperty("fpxMerchantName", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string FPXMerchantName
        {
            get { return (string)this["fpxMerchantName"]; }
            set { this["fpxMerchantName"] = value; }
        }

        [ConfigurationProperty("isFullPaymentGatewayReturnUrl", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsFullPaymentGatewayReturnUrl
        {
            get { return (bool)this["isFullPaymentGatewayReturnUrl"]; }
            set { this["isFullPaymentGatewayReturnUrl"] = value; }
        }

        #region VN PGH Payments config
        /// <summary>
        /// Configuration for multiple payment gateways
        /// format : 
        ///   Each payment must be separated with the pipe symbol "|"
        ///   Each property must be separated with the semicolon symbol ";"
        ///     Properties: DisplayName;PaymentCode;HtmlFragment
        ///       DisplayName: Text to display on UI. (Credit Card/Local Cards/etc.)
        ///       PaymentCode: Payment code to send. (IO/DM/etc.)
        ///       HtmlFragment: Html fragment file to display on this payment option. (lblPaymentGatewayMessage.html/etc.)
        /// e.g. Credit Card;IO;lblCreditCardMessage.html|VNPayment;DM;lblPaymentGatewayMessage.html
        /// </summary>
        [ConfigurationProperty("pghPaymentsConfiguration", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PGHPaymentsConfiguration
        {
            get { return (string)this["pghPaymentsConfiguration"]; }
            set { this["pghPaymentsConfiguration"] = value; }
        }
        #endregion

        #endregion
    }
}