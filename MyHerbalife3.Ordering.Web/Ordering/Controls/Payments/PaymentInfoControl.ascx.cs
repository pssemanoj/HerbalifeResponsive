using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.Infrastructure;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public static class Utility
    {
        public static Int32 ParseInt32(this string str)
        {
            Int32 k;
            if (Int32.TryParse(str, out k))
                return k;
            return 0;
        }
    }

    public partial class PaymentInfoControl : UserControlBase
    {
        #region Constants /Enum

        public enum PaymentInfoCallModule
        {
            GRID = 1,
            PAYMENTINFO = 2
        }

        protected const string ScriptBlock = "<script type='text/javascript'>{0}</script>";

        private const string ValidationScriptMessages = "<script type='text/javascript'>    	var CardNumberRequired = '{0}';    	var CardNumberInvalid = '{1}';    	var CardNameRequired = '{2}';    	var ExpDateRequired = '{3}';    	var AddCardQuestion = '{4}';    	var DeleteCardQuestion = '{5}';     var CardHasExpired = '{6}';     var StreetAddressRequired = '{7}';     var CityRequired = '{8}';     var StateRequired = '{9}';     var ValidZipRequired = '{10}';     var CardTypeRequired = '{11}';     var TokenizationFailed = '{12}';     var validNickNameRequired = '{13}';     var validNonDupeNickNameRequired = '{14}';</script>";

        public static string REQUIRED_FIELD_FAILURE_MESSAGE = "Please provide the Required field(s)";
        public static string SHIPPINGADDRESS_GET_FAILURE_MESSAGE = "There is no avaliable Shipping address";
        public static string PAYMENTINFORMATION_GET_FAILURE_MESSAGE = "There is no available payment information";
        public static string PAYMENTINFORMATION_DELETE_FAILURE_MESSAGE = "Payment information was not deleted";
        public static string PAYMENTINFORMATION_UPDATE_FAILURE_MESSAGE = "Payment information was not updated";

        public static string INCONSISTANT_CHECKBOX_SELECTION_MESSAGE =
            "'Save this Credit Card' shoudl be checked for 'Primary credit card'";

        #endregion Constants /Enum

        #region Delegates/Events

        public delegate void MessageHandler(string message);

        [Publishes(MyHLEventTypes.CreditCardProcessed)]
        public event EventHandler onCreditCardProcessed;

        #endregion Delegates/Events

        #region Fields

        protected bool _RegisteredCardsOnly = false;

        private string _countryCode = string.Empty;
        protected PaymentInfoCommandType _currentMode = PaymentInfoCommandType.Unknown;
        private string _distributorId = string.Empty;
        private bool _hasShippingAddress;
        private string _locale = string.Empty;
        protected PaymentsConfiguration _paymentsConfig = null;
        protected ShippingAddress_V01 currentAddress = null;
        private string parentUri;
        protected PaymentInformation paymentInfo = null;
        private PaymentInformation paymentInfoFromParentPage;
        protected List<PaymentInformation> paymentInfos = null;

        #endregion Fields

        #region Property

        public bool IsEdit { get; set; }

        public PaymentInformation CurrentPaymentInfo { get; set; }

        public string FirstName
        {
            get { return txtCardHolderName.Text.Trim(); }
        }

        public string MiddleInnitial
        {
            get { return string.Empty; }
        }

        public string LastName
        {
            get { return string.Empty; }
        }

        public string CardNumber
        {
            get { return txtCardNumber.Text.Trim(); }
        }

        public string CardType
        {
            get { return ddlCardType.SelectedItem != null ? ddlCardType.SelectedValue : string.Empty; }
            set { }
        }

        public string NickName
        {
            get { return txtNickName.Text.Trim(); }
            set { ; }
        }

        public int PaymentID
        {
            get { return hdID.Value.ParseInt32(); }
            set { ; }
        }

        public bool IsPrimary
        {
            get { return chkMakePrimaryCreditCard.Checked; }
            set { ; }
        }

        public bool IsToBeSaved
        {
            get { return chkSaveCreditCard.Checked; }
            set { ; }
        }

        public void Refresh()
        {
        }

        #endregion Property

        #region Construction / Initialization / Page_Cycle_Method

        /// <summary>
        ///     page loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            hdTokenTimerEnabled.Value = Settings.GetRequiredAppSetting("TokenizationTimerEnabled", "false");

            if (_paymentsConfig.AddressRequiredForNewCard == false)
            {
                dvCardDetails.Attributes.Add("class", dvCardDetails.Attributes["class"] + " last");
            }
            lblMessage.Text = string.Empty;
            setCardValidationClientScripts();

            if (!IsPostBack)
            {
                SetExpirationYears();
            }
            try
            {
                _hasShippingAddress = (Page as ProductsBase).ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping;
            }
            catch (Exception)
            {
                return;
            }

            
        }

        /// <summary>
        ///     override OnInit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _locale = (Page as ProductsBase).Locale;
            _distributorId = (Page as ProductsBase).DistributorID;
            _countryCode = (Page as ProductsBase).CountryCode;
            _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
            paymentInfos = PaymentInfoProvider.GetPaymentInfo(_distributorId, _locale);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (CountryCode.Equals("JM"))
            {
                txtStreetAddress.MaxLength = 48;
                txtStreetAddress2.MaxLength = 50;
                txtCity.MaxLength = 50;
                txtState.MaxLength = 38;
                txtZip.Visible = false;
                lblZip.Visible = false;
            }
            switch (_currentMode)
            {
                case PaymentInfoCommandType.Add:
                {
                    btnContinue.OnClientClick = "return ValidateNewCard(event, this)";
                    repositionModal(true);
                    break;
                }
                case PaymentInfoCommandType.Delete:
                {
                    btnContinue.OnClientClick = "return true;";
                    repositionModal();
                    break;
                }
                case PaymentInfoCommandType.Edit:
                {
                    btnContinue.OnClientClick = "return ValidateNewCard(event, this)";
                    repositionModal(true);
                    break;
                }
                default:
                    repositionModal(true);
                    break;

            }
        }

        #endregion Construction / Initialization / Page_Cycle_Method

        #region Private methods

        /// <summary>
        ///     process payment information upon event request from parent control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SubscribesTo(MyHLEventTypes.CreditCardProcessing)]
        public void OnCreditCardProcessing(object sender, EventArgs e)
        {
            parentUri = ((Control) sender).Page.ToString();
            hdParentUri.Value = parentUri;
            hdCommand.Value = ((PaymentInfoEventArgs) e).Command.ToString();
            paymentInfoFromParentPage = ((PaymentInfoEventArgs) e).PaymentInfo;

            dvBillingAddress.Visible = _paymentsConfig.AddressRequiredForNewCard;
            _currentMode = ((PaymentInfoEventArgs) e).Command;

            switch (_currentMode)
            {
                case PaymentInfoCommandType.Add:
                    preProcessForPaymentInfoAdd();
                    break;
                case PaymentInfoCommandType.Edit:
                    preProcessForPaymentInfoEdit();
                    break;
                case PaymentInfoCommandType.Delete:
                    preProcessForPaymentInfoDelete();
                    break;
                default:
                    break;
            }

            return;
        }

        /// <summary>
        ///     Pre-process before add payment information upon Continue button click
        /// </summary>
        private void preProcessForPaymentInfoAdd()
        {
            lblHeaderAddNewCreditCard.Text = getLocalResourceString("lblHeaderAddNewCreditCardResource1.Text", "Add New Credit Card");
            chkSaveCreditCard.Attributes.Clear();
            clearCreditCardControlFields();
            clearBillingAddressControlFields();
            hdID.Value = string.Empty;
            hdLocale.Value = Locale;
            dvCreditCard.Visible = true;
            dvDeleteCreditCard.Visible = false;
            dvBillingAddressLabel.Visible = false;
            dvBillingAddressText.Visible = true;

            //2.
            if (rblBillingAddress.SelectedIndex == 0)
            {
                currentAddress = getShippingAddress();
                /*lblBillingAddress_Street.Text = currentAddress.Address.Line1 + " " +
                                                currentAddress.Address.Line2 + " " +
                                                currentAddress.Address.Line3 + " " +
                                                currentAddress.Address.Line4;
                lblBillingAddress_CityStateZip.Text = currentAddress.Address.City + " " +
                                                      currentAddress.Address.StateProvinceTerritory + " " +
                                                      currentAddress.Address.PostalCode;*/
                dvBillingAddressLabel.Visible = true;
                dvBillingAddressText.Visible = false;
            }

            if (parentUri == "ASP.ordering_orderpreferences_aspx" ||
                parentUri == "ASP.ordering_savedpaymentinformation_aspx")
            {
                initializeAddFromOrderPreference();
            }
            else
            {
                initializeAddFromNonOrderPreference();
            }
        }

        /// <summary>
        ///     pre-edit for order preference Add
        /// </summary>
        private void initializeAddFromOrderPreference()
        {
            //3.
            if (paymentInfos == null)
            {
                chkMakePrimaryCreditCard.Checked = true;
                chkMakePrimaryCreditCard.Enabled = false;
            }
            else
            {
                if ((paymentInfos.Where(s => s.IsPrimary).FirstOrDefault()) == null)
                {
                    chkMakePrimaryCreditCard.Checked = true;
                    chkMakePrimaryCreditCard.Enabled = false;
                }
                else
                {
                    chkMakePrimaryCreditCard.Checked = false;
                    chkMakePrimaryCreditCard.Enabled = true;
                }
            }

            rblBillingAddress.Visible = false;

            chkSaveCreditCard.Checked = true;
            chkSaveCreditCard.Enabled = false;
            chkSaveCreditCard.Attributes.Add("style", "display:none");
            txtCardNumber.ReadOnly = false;
            txtCardNumber.Enabled = true;
        }

        /// <summary>
        ///     pre-edit for non order preference Add
        /// </summary>
        private void initializeAddFromNonOrderPreference()
        {
            //3.
            if (paymentInfos != null)
            {
                //chkSaveCreditCard.Checked = false;
                chkSaveCreditCard.Checked = true;
                chkSaveCreditCard.Enabled = true;

                if (paymentInfos.Where(s => s.IsPrimary).FirstOrDefault() == null ||
                    paymentInfos.Where(s => s.IsPrimary).Count() == 0)
                {
                    chkMakePrimaryCreditCard.Checked = true;
                    chkMakePrimaryCreditCard.Enabled = false;
                }
                else
                {
                    chkMakePrimaryCreditCard.Checked = false;
                    chkMakePrimaryCreditCard.Enabled = true;
                }
            }

            if (!HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards)
            {
                chkSaveCreditCard.Checked = false;
                chkSaveCreditCard.Attributes.Add("style", "display:none");
                chkMakePrimaryCreditCard.Checked = false;
                chkMakePrimaryCreditCard.Attributes.Add("style", "display:none");
            }

            rblBillingAddress.Visible = _hasShippingAddress;
            if (rblBillingAddress.Visible)
            {
                rblBillingAddress.SelectedIndex = 1;
                SetBillingAddressOption(null, null);
            }
            txtCardNumber.ReadOnly = false;
            txtCardNumber.Enabled = true;
        }

        /// <summary>
        ///     Pre-process before edit payment information upon Continue button click
        /// </summary>
        private void preProcessForPaymentInfoEdit()
        {
            populateControlForEditFromPaymentInfo();
            callJavaScript();

            lblHeaderAddNewCreditCard.Text = getLocalResourceString("EditCreditCard", "Edit Credit Card");
            dvCreditCard.Visible = true;
            dvDeleteCreditCard.Visible = false;
            //dvBillingAddressText.Visible = false;

            //initializeControForEdit();
            //1.
            if (parentUri == "ASP.ordering_orderpreferences_aspx" ||
                parentUri == "ASP.ordering_savedpaymentinformation_aspx")
            {
                chkSaveCreditCard.Visible = false;
                if (HLConfigManager.Configurations.PaymentsConfiguration.AddressRequiredForNewCard)
                {
                    rblBillingAddress.SelectedIndex = 1;
                    rblBillingAddress.Visible = false;
                }
            }
            else
            {
                rblBillingAddress.Visible = _hasShippingAddress;
                if (rblBillingAddress.Visible)
                {
                    rblBillingAddress.SelectedIndex = 1;
                    SetBillingAddressOption(null, null);
                }
            }

            //2.
            if (paymentInfo.IsTemporary)
            {
                chkSaveCreditCard.Attributes.Remove("style");
                chkSaveCreditCard.Checked = false;
                chkSaveCreditCard.Enabled = true;

                chkMakePrimaryCreditCard.Checked = false;
                chkMakePrimaryCreditCard.Enabled = true;
            }
            else
            {
                chkSaveCreditCard.Attributes.Add("style", "display:none");
                if (paymentInfo.IsPrimary)
                {
                    chkSaveCreditCard.Checked = true;
                    chkSaveCreditCard.Enabled = false;

                    chkMakePrimaryCreditCard.Checked = true;
                    chkMakePrimaryCreditCard.Enabled = false;
                }
                else
                {
                    chkSaveCreditCard.Checked = true;
                    chkSaveCreditCard.Enabled = true;

                    chkMakePrimaryCreditCard.Checked = false;
                    chkMakePrimaryCreditCard.Enabled = true;
                }
            }

            if (!HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards)
            {
                chkSaveCreditCard.Checked = false;
                chkSaveCreditCard.Attributes.Add("style", "display:none");
                chkMakePrimaryCreditCard.Checked = false;
                chkMakePrimaryCreditCard.Attributes.Add("style", "display:none");
            }
        }

        /// <summary>
        ///     Pre-process before delete payment information upon Continue button click
        /// </summary>
        private void preProcessForPaymentInfoDelete()
        {
            dvCreditCard.Visible = false;
            dvBillingAddress.Visible = false;
            dvDeleteCreditCard.Visible = true;
            dvBillingAddressLabel.Visible = false;
            dvBillingAddressText.Visible = false;

            populateControlForDeleteFromPaymentInfo();
        }

        /// <summary>
        ///     populate control from paymentinfo data for Edit command
        /// </summary>
        protected virtual void populateControlForEditFromPaymentInfo()
        {
            if (getPaymentInfo() == null)
            {
                lblBillingAddressMessage.Text = PAYMENTINFORMATION_GET_FAILURE_MESSAGE;

                return;
            }

            hdID.Value = paymentInfo.ID.ToString();
            hdLocale.Value = Locale;
            txtCardHolderName.Text = paymentInfo.CardHolder.First.Trim();
            ///TODO: break down name
            //this.tbLastName.Text = paymentInfo.CardHolder.Last;
            //this.tbMI.Text = paymentInfo.CardHolder.Middle;
            txtCardNumber.ReadOnly = true;
            txtCardNumber.Enabled = false;
            txtCardNumber.Text = MaskCardNumber(paymentInfo.CardNumber);

            if (getCreditCardTypeListFromProvider().Items.FindByValue(paymentInfo.CardType.Trim()) != null)
            {
                ddlCardType.SelectedValue = paymentInfo.CardType.Trim();
                ddlCardType.Enabled = false;
            }

            ddlExpMonth.SelectedValue = string.Format("{0:MM}", paymentInfo.Expiration);

            var selected = ddlExpYear.Items.FindByValue(string.Format("{0:yy}", paymentInfo.Expiration));
            if (selected != null)
            {
                ddlExpYear.ClearSelection();
                selected.Selected = true;
            }
            else
            {
                ddlExpYear.SelectedIndex = -1;
            }

            txtNickName.Text = paymentInfo.Alias.Trim();
            chkMakePrimaryCreditCard.Checked = paymentInfo.IsPrimary ? true : false;
            if (CreditCard.GetCardType(paymentInfo.CardType) == IssuerAssociationType.MyKey)
            {
                pnlExpDate.Attributes.Add("style", "visibility:hidden");
            }
            if (_paymentsConfig.AddressRequiredForNewCard)
            {
                txtStreetAddress.Text = (null != paymentInfo.BillingAddress.Line1)
                                            ? paymentInfo.BillingAddress.Line1.Trim()
                                            : string.Empty;
                txtStreetAddress2.Text = (null != paymentInfo.BillingAddress.Line2)
                                             ? paymentInfo.BillingAddress.Line2.Trim()
                                             : string.Empty;
                txtCity.Text = (null != paymentInfo.BillingAddress.City)
                                   ? paymentInfo.BillingAddress.City
                                   : string.Empty;

                txtState.Text = (null != paymentInfo.BillingAddress.StateProvinceTerritory)
                                    ? paymentInfo.BillingAddress.StateProvinceTerritory.Trim()
                                    : string.Empty;
                txtZip.Text = (null != paymentInfo.BillingAddress.PostalCode)
                                  ? paymentInfo.BillingAddress.PostalCode.Trim()
                                  : string.Empty;
                if (rblBillingAddress.Visible)
                {
                    rblBillingAddress.SelectedIndex = 1;
                    SetBillingAddressOption(null, null);
                }
            }
        }

        /// <summary>
        ///     populate control from paymentinfo data for delete command
        /// </summary>
        /// <returns></returns>
        private void populateControlForDeleteFromPaymentInfo()
        {
            btnContinue.OnClientClick = "return true;";

            if (getPaymentInfo() == null)
            {
                lblBillingAddressMessage.Text = PAYMENTINFORMATION_GET_FAILURE_MESSAGE;

                return;
            }

            hdID.Value = paymentInfo.ID.ToString(); 
            hdLocale.Value = Locale;
            lblCardHolderNameForDelete.Text = paymentInfo.CardHolder.First;
            ///TODO: break down name
            //this.tbLastName.Text = paymentInfo.CardHolder.Last;
            //this.tbMI.Text = paymentInfo.CardHolder.Middle;

            lblCardNumberForDelete.Text = MaskCardNumber(paymentInfo.CardNumber);
            lblCardTypeForDelete.Text =
                GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                        getCardDescriptionResourceKey(paymentInfo.CardType.Trim())) as string;
            // paymentInfo.CardType.Trim();
            lblExpDateForDelete.Text = paymentInfo.Expiration.ToString("MM/yyyy");
            if (CreditCard.GetCardType(paymentInfo.CardType) == IssuerAssociationType.MyKey)
            {
                lblExpDateForDelete.Text = string.Empty;
            }

            lblNickNameForDelete.Text = paymentInfo.Alias;
            lblPrimaryForDelete.Text = paymentInfo.IsPrimary
                                           ? GetLocalResourceObject("PrimaryYes") as string
                                           : GetLocalResourceObject("PrimaryNo") as string; // "Yes" : "No";
            lblStreetAddress.Text = paymentInfo.BillingAddress.Line1;
            lblStreetAddress2.Text = paymentInfo.BillingAddress.Line2;
            lblCity.Text = paymentInfo.BillingAddress.City;

            if (!String.IsNullOrEmpty(txtState.Text = paymentInfo.BillingAddress.StateProvinceTerritory))
                txtState.Text = paymentInfo.BillingAddress.StateProvinceTerritory.Trim();

            lblZip.Text = paymentInfo.BillingAddress.PostalCode;
            btnContinue.Text = GetLocalResourceObject("btnDeleteResource1") as string;
        }

        /// <summary>
        ///     validate data entry in paymentinfo control
        /// </summary>
        /// <returns></returns>
        private bool isValidEntryFromControl(PaymentInformation paymentInfo, PaymentInfoCommandType commandType)
        {
            // bool isValid = true;
            lblMessage.Text = string.Empty;

            if (commandType != PaymentInfoCommandType.Edit)
            {
                if (!Settings.GetRequiredAppSetting<bool>("TokenizationDisabled", false))
                {
                    long test = 0;
                    if (long.TryParse(paymentInfo.CardNumber, out test) && test > 0)
                    {
                        lblMessage.Text = GetLocalResourceObject("ValidateAddCardBadCard") as string;
                        return false;
                    }
                }
            }
            // determine root cause of null
            if (paymentInfo == null)
            {
                lblMessage.Text = "payment is null";
                return false;
            }

            if (!ValidateRequiredFields(paymentInfo))
            {
                return false;
            }

            if (!ValidateRequiredFields(paymentInfo))
            {
                return false;
            }

            if (!IsToBeSaved) // Is Temporary CreditCard
            {
                // GetAlias
                if (string.IsNullOrEmpty(paymentInfo.Alias))
                {
                    string name = string.Concat(paymentInfo.CardHolder.First + paymentInfo.CardHolder.Last);
                    if (name.Length > 20)
                    {
                        name = name.Substring(0, 20);
                    }
                    // GetCardNumber
                    string cardNum = paymentInfo.CardNumber.Trim();
                    cardNum = "- " + (cardNum.Length > 4 ? cardNum.Substring(cardNum.Length - 4) : "");

                    paymentInfo.Alias = string.Concat(name, " - ", paymentInfo.CardType, " - ", cardNum);
                }
            }

            // this is  dup check but it is possible to have multiple aliases with the empty aliases. For edits that would be problematic
            // for deletes we are relying on hdiid and know what to delete
            if (!String.IsNullOrEmpty(paymentInfo.Alias))
            {
                if (
                    (from p in paymentInfos
                     where (p.Alias.ToUpper() == paymentInfo.Alias.ToUpper() && p.ID != paymentInfo.ID)
                     select p).Count() > 0)
                {
                    lblMessage.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateCardError");
                    return false;
                }
            }

            return true;
        }

        private bool ValidateRequiredFields(PaymentInformation paymentInfo)
        {
            if(paymentInfo == null)
            {
                return false;
            }

            if(paymentInfo.CardHolder == null || string.Concat(paymentInfo.CardHolder.First,paymentInfo.CardHolder.Middle,paymentInfo.CardHolder.Last).Trim().Length == 0)
            {
                lblMessage.Text = GetLocalResourceObject("ValidateAddCardMissingName") as string;
                return false;
            }

            if(string.IsNullOrEmpty(paymentInfo.CardNumber))
            {
                lblMessage.Text = GetLocalResourceObject("ValidateAddCardMissingCard") as string;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     get payment information
        /// </summary>
        /// <returns></returns>
        protected PaymentInformation getPaymentInfo()
        {
            try
            {
                if (paymentInfos == null)
                {
                    throw new ApplicationException(
                        string.Format("Payment Information is not found for distributor {0}, in locale {1}",
                                      _distributorId, _locale));
                }
                else if (paymentInfoFromParentPage != null)
                {
                    paymentInfo = paymentInfos.Where(p => p.ID == paymentInfoFromParentPage.ID).SingleOrDefault();
                    if (paymentInfo == null)
                    {
                        paymentInfo = paymentInfos.SingleOrDefault();
                    }
                }
                else
                {
                    paymentInfo = paymentInfos.SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn(ex.ToString());
                lblBillingAddressMessage.Text = PAYMENTINFORMATION_GET_FAILURE_MESSAGE;
                return null;
            }

            return paymentInfo;
        }

        /// <summary>
        ///     get shipping address
        /// </summary>
        /// <returns></returns>
        protected ShippingAddress_V01 getShippingAddress()
        {
            try
            {
                var thisCart = (Page as ProductsBase).ShoppingCart;
                if (thisCart.DeliveryInfo != null)
                {
                    currentAddress = thisCart.DeliveryInfo.Address;
                }
                else
                {
                    throw new ApplicationException(
                        string.Format("Shipping Information is not found for distributor {0}, in locale {1}",
                                      _distributorId, _locale));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                lblBillingAddressMessage.Text = SHIPPINGADDRESS_GET_FAILURE_MESSAGE;
                return null;
            }

            return currentAddress;
        }

        /// <summary>
        ///     clear credit card control fields
        /// </summary>
        private void clearCreditCardControlFields()
        {
            txtCardHolderName.Text = string.Empty;
            txtCardNumber.Text = string.Empty;
            txtNickName.Text = string.Empty;
            chkSaveCreditCard.Checked = false;
            chkMakePrimaryCreditCard.Checked = false;
            lblCreditCardMesssage.Text = string.Empty;

            ddlExpMonth.SelectedIndex = 0;
            ddlExpYear.SelectedIndex = 0;
            ddlCardType.Enabled = true;
            getCreditCardTypeListFromProvider();
        }

        /// <summary>
        ///     clear billing address control fields
        /// </summary>
        protected virtual void clearBillingAddressControlFields()
        {
            txtStreetAddress.Text =
                txtStreetAddress2.Text =
                txtCity.Text =
                txtZip.Text =
                lblBillingAddressMessage.Text =
                txtState.Text =
                string.Empty;
        }

        /// <summary>
        ///     Create payment information from Paymentinfo control
        /// </summary>
        /// <returns></returns>
        protected virtual PaymentInformation createPaymentInfoFromControl()
        {
            if (PaymentID > 0)
            {
                paymentInfo = paymentInfos.Find(p => p.ID == PaymentID);
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
            paymentInfo.CardHolder = new ServiceProvider.OrderSvc.Name_V01() { First = FirstName, Last = LastName, Middle = MiddleInnitial };
            if (!IsMasked(CardNumber))
            {
                //var useCardToken  = HL.Common.Configuration.Settings.GetRequiredAppSetting("UseCardTokenization");
                //paymentInfo.CardNumber = useCardToken.Trim().ToUpper() == "TRUE" ? PaymentInfoProvider.GetCreditCardToken(CardNumber) : CardNumber;
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
            var billingAddress = new ServiceProvider.OrderSvc.Address_V01();
            if (rblBillingAddress.SelectedIndex == 0)
            {
                billingAddress = ObjectMappingHelper.Instance.GetToOrder(getShippingAddress().Address);
            }
            else
            {
                billingAddress.Line1 = txtStreetAddress.Text;
                billingAddress.Line2 = txtStreetAddress2.Text;
                billingAddress.Line3 = string.Empty;
                billingAddress.Line4 = string.Empty;
                billingAddress.City = txtCity.Text;
                billingAddress.StateProvinceTerritory = txtState.Text;
                billingAddress.PostalCode = txtZip.Text;
            }
            paymentInfo.BillingAddress = billingAddress;

            return paymentInfo;
        }

        /// <summary>
        ///     get state list from ShippingProvider
        /// </summary>
        /// <returns></returns>
        /// <summary>
        ///     get credit card type list from PaymentInfoProvider
        /// </summary>
        /// <returns></returns>
        protected DropDownList getCreditCardTypeListFromProvider()
        {
            ddlCardType.Items.Clear();
            ddlCardType.ClearSelection();
            ddlCardType.Items.Insert(0, GetLocalResourceObject("ListItemResource1.Text") as string);

            if (!_RegisteredCardsOnly)
            {
                if (null != ddlCardType)
                {
                    var cardTypes = PaymentInfoProvider.GetOnlineCreditCardTypes(_countryCode);
                    foreach (HPSCreditCardType cct in cardTypes)
                    {
                        var liCard = new ListItem(GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform), getCardDescriptionResourceKey(cct.Code)) as string, cct.Code); 
                        ddlCardType.Items.Add(liCard);
                        Validations.Value += string.Concat(cct.Code, "=", cct.CardNumberValidationRegexText, ";");
                    }

                    var jsScriptBuilder = new StringBuilder();
                    if (null != pnlExpDate)
                    {
                        jsScriptBuilder.AppendLine(string.Concat("document.getElementById('", pnlExpDate.ClientID, "').style.visibility = (this.value=='QH')? 'hidden' : 'visible';"));
                    }

                    if (null != txtCardNumber)
                    {
                        jsScriptBuilder.AppendLine(string.Concat("document.getElementById('", txtCardNumber.ClientID, "').maxLength = (this.value=='MS'||this.value=='HI')? '19' : '16';"));
                    }
                    ddlCardType.Attributes.Add("onchange", jsScriptBuilder.ToString());
                }
            }

            return ddlCardType;
        }

        protected string GetNickNamesList()
        {
            string list = string.Empty;
            List<PaymentInformation> infos = paymentInfos;
            if (null == infos)
            {
                infos = PaymentInfoProvider.GetPaymentInfo(_distributorId, _locale);
            }

            if (null != infos)
            {
                infos.ForEach(pi => list += pi.Alias + ';');
            }

            return list;
        }

        /// <summary>
        ///     get local resource string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string getLocalResourceString(string key, string defaultValue)
        {
            string resourceString = null;

            try
            {
                object value = null;
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
                return null;
            }

            return resourceString;
        }

        /// <summary>
        ///     get card description resource key
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        private string getCardDescriptionResourceKey(string cardCode)
        {
            return string.Format("{0}_{1}_{2}", "CardType", cardCode, "Description");
        }

        #endregion Private methods

        #region Event Handler

        protected void SetBillingAddressOption(object sender, EventArgs e)
        {
            switch (rblBillingAddress.SelectedIndex)
            {
                case 0:
                    {
                        chkSameAsShippingAddress_CheckedChanged(sender, e);
                        break;
                    }
                case 1:
                    {
                        chkNewBillingAddress_CheckedChanged(sender, e);
                        break;
                    }
            }
        }

        /// <summary>
        ///     process on Contiue button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnContinue_Click(object sender, EventArgs e)
        {
            paymentInfo = createPaymentInfoFromControl();

            var command =
                (PaymentInfoCommandType) Enum.Parse(PaymentInfoCommandType.Add.GetType(), hdCommand.Value, true);
            int cardId = paymentInfo.ID;
            switch (command)
            {
                case PaymentInfoCommandType.Delete:
                    {
                        // we don't check for dup alias on delete since there could be multiple cards with empty aliases.
                        // for deletes we are relying on hdiid and know what to delete
                        // myutliple cards with empty aliases is no longer allowed on website BUT exsiting data has them though.
                        // anyway a null check and reliance on hdid is sufficent
                        if (paymentInfo == null)
                        {
                            return;
                        }
                        processDeleteOnBtnContinueClick();
                        break;
                    }
                case PaymentInfoCommandType.Edit:
                    {
                        // website no longer allowes multiple empty aliases but there is existing data out there that
                        // will go through the dup-alias check  of isValidEntryFromControl
                        if (!isValidEntryFromControl(paymentInfo, PaymentInfoCommandType.Edit))
                        {
                            return;
                        }

                        processEditOnBtnContinueClick();
                        break;
                    }
                case PaymentInfoCommandType.Add:
                    {
                        // for adds we do a dup-alias check
                        if (!isValidEntryFromControl(paymentInfo, PaymentInfoCommandType.Add))
                        {
                            return;
                        }
                        processAddOnBtnContinueClick();
                        break;
                    }
            }

            // close popup
            onCreditCardProcessed(this, new PaymentInfoEventArgs(command, paymentInfo, false, cardId));
            ppPaymentInfoControl.Hide();
        }

        /// <summary>
        ///     process payment Add on Contiue button Click
        /// </summary>
        private bool processAddOnBtnContinueClick()
        {
            //if (IsSaved || paymentInfo.ID < 99999991)
            if (IsToBeSaved)
            {
                paymentInfo.IsTemporary = false;
                return savePaymentInfo(paymentInfo);
            }

            else
            {
                paymentInfo.IsTemporary = true;
                PaymentInformation lastTemporaryPaymentInfo = null;
                if (paymentInfos.Where(p => p.IsTemporary).Count() > 0)
                {
                    lastTemporaryPaymentInfo =
                        (from p in paymentInfos where p.ID == (from pi in paymentInfos select pi.ID).Max() select p)
                            .Single();
                }
                if (lastTemporaryPaymentInfo == null)
                {
                    paymentInfo.ID = 1;
                }
                else
                {
                    paymentInfo.ID = lastTemporaryPaymentInfo.ID + 1;
                }
                PaymentInfoProvider.SavePaymentInfo(_distributorId,
                                                    _locale, paymentInfo);
            }

            return true;
        }

        /// <summary>
        ///     save payment Information into DB or Cache
        /// </summary>
        /// <param name="paymentInfoInDB"></param>
        /// <returns></returns>
        private bool savePaymentInfo(PaymentInformation paymentInfo)
        {
            int id = PaymentInfoProvider.SavePaymentInfo(_distributorId, _locale, paymentInfo);
            //scope.Complete();
            if (id > 0)
            {
                paymentInfo.ID = id;
            }

            return id > 0;
        }

        /// <summary>
        ///     process payment Edit on Contiue button Click
        /// </summary>
        private bool processEditOnBtnContinueClick()
        {
            //1.
            if (!chkSaveCreditCard.Checked)
            {
                chkMakePrimaryCreditCard.Enabled = false;
            }

            int currentId = paymentInfo.ID;

            if (IsToBeSaved)
            {
                if (paymentInfo.IsTemporary)
                {
                    paymentInfos.Remove(paymentInfo);
                }
                paymentInfo.IsTemporary = false;
            }
            else
            {
                paymentInfo.IsTemporary = true;
            }
            int errorCode =
                PaymentInfoProvider.SavePaymentInfo(_distributorId, _locale,
                                                    paymentInfo);
            if (errorCode > 0 && errorCode != currentId)
            {
                paymentInfos =
                    PaymentInfoProvider.GetPaymentInfo(_distributorId,
                                                       _locale);
                paymentInfo = paymentInfos.Find(p => p.ID == errorCode);
            }

            return true;
        }

        /// <summary>
        ///     process payment Delete on Contiue button Click
        /// </summary>
        private bool processDeleteOnBtnContinueClick()
        {
            try
            {
                string workingID = String.Empty;
                // entire viewstate is gone. but there we've got another trick up out sleeves
                if (String.IsNullOrEmpty(hdID.Value))
                {
                    // this should be iterated with a search for $hdID -
                    foreach (string key in Request.Form.Keys)
                    {
                        if (key.Contains("$hdID"))
                            workingID = Request.Form[key];
                    }
                }
                else
                    workingID = hdID.Value;

                PaymentInfoProvider.DeletePaymentInfo(
                    Convert.ToInt32(workingID), _distributorId, _locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn(ex.ToString());
                lblBillingAddressMessage.Text = PAYMENTINFORMATION_DELETE_FAILURE_MESSAGE;
                return false;
            }
            return true;
        }

        /// <summary>
        ///     process on Cancel button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            onCreditCardProcessed(this, new PaymentInfoEventArgs(0, paymentInfo, false));
            ppPaymentInfoControl.Hide();
        }

        /// <summary>
        ///     process when chkSameAsShippingAddress checkbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void chkSameAsShippingAddress_CheckedChanged(object sender, EventArgs e)
        {
            currentAddress = getShippingAddress();
            if (currentAddress != null)
            {
                sametxtStreetAddress.Text = currentAddress.Address.Line1.Trim();

                // crash if not checked
                if (!String.IsNullOrEmpty(currentAddress.Address.Line2))
                {
                    sametxtStreetAddress2.Text = currentAddress.Address.Line2.Trim();
                }
                else
                {
                    sametxtStreetAddress2.Text = String.Empty;
                }

                sametxtCity.Text = currentAddress.Address.City.Trim();
                sametxtState.Text = (String.IsNullOrEmpty( currentAddress.Address.StateProvinceTerritory)) ? "" : currentAddress.Address.StateProvinceTerritory.Trim();
                sametxtZip.Text = currentAddress.Address.PostalCode.Trim();

            }
            else
            {
                lblBillingAddressMessage.Text = SHIPPINGADDRESS_GET_FAILURE_MESSAGE;
            }

            dvBillingAddressLabel.Visible = true;
            dvBillingAddressText.Visible = false;
        }

        /// <summary>
        ///     process when chkNewBillingAddress checkbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkNewBillingAddress_CheckedChanged(object sender, EventArgs e)
        {
            dvBillingAddressLabel.Visible = false;
            dvBillingAddressText.Visible = true;
        }

        private void callJavaScript()
        {
            var cstext1 = new StringBuilder();
            cstext1.Append("<script type=text/javascript> CheckSaveBoxSettings(event); </");
            cstext1.Append("script>");
        }

        /// <summary>Write client validation scripts to browser if user can add cards</summary>
        protected virtual void setCardValidationClientScripts()
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
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateZipRequired") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateSelectCardType") as string),
                                                                            EscapeJavascriptQuotes(GetLocalResourceObject("ValidateTokenizationFailed") as string),
                                                                            EscapeJavascriptQuotes(GetGlobalResourceObject("MyHL_ErrorMessage", "InvalidNickname") as string),
                                                                            EscapeJavascriptQuotes(GetGlobalResourceObject("MyHL_ErrorMessage", "DuplicateAddressNickname") as string)));            
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
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("CookieHandler::GetSeamlessCredentials() error. Error description: {0}", ex.Message);
                (new HLErrorAuditEvent("CookieHandler::GetSeamlessCredentials()", CookieHandler.GetSeamlessCredentials(), ex)).Raise();
            }
            
            //this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "NickNamesList", string.Format(ScriptBlock, string.Concat("var nickNamesList = '", GetNickNamesList(), "';")));
            hdNickNameList.Value = GetNickNamesList();
            chkSaveCreditCard.Attributes.Add("onclick ", "CheckSaveBoxSettings(event)");
            chkMakePrimaryCreditCard.Attributes.Add("onclick ", "CheckPrimaryBoxSettings(event)");
        }

        /// <summary>Create the Card Expiration date from the dropdown choices</summary>
        /// <param name="cardType">The type of card</param>
        /// <param name="row">The row containing the new card data</param>
        /// <returns></returns>
        protected DateTime convertExpDateToDatetimeFormat(string cardType)
        {
            if (CreditCard.GetCardType(cardType) == IssuerAssociationType.MyKey) return getMyKeyExpirationDate();

            int month = 0;
            int year = 0;

            if (!Int32.TryParse(ddlExpMonth.SelectedItem.Text, out month) || !Int32.TryParse(ddlExpYear.SelectedItem.Text, out year))
            {
                // commenting the below logic due to exception seen in splunk - Year, Month, and Day parameters describe an un-representable DateTime
                //month = year = 0;
                month = 1;
                year = DateTime.Now.Year;
            }

            return DateTimeUtils.GetLastDayOfMonth(new DateTime(year, month, 1));
        }

        /// <summary>The standin date for MyKey card</summary>
        /// <returns>The date</returns>
        protected DateTime getMyKeyExpirationDate()
        {
            return new DateTime(2049, 12, 31);
        }

        #endregion Event Handler

        protected string EscapeJavascriptQuotes(string message)
        {
            return message.Replace("'", @"\'");
        }

        protected string MaskCardNumber(string cardNumber)
        {
            try
            {
                cardNumber = cardNumber.Trim();
                int len = cardNumber.Length;
                return string.Concat(cardNumber[0], new string('X', len - 5), cardNumber.Substring(len - 4));
            }
            catch
            {
            }

            return "XXXXXXXXXXXXXXXX";
        }

        protected bool IsMasked(string cardNumber)
        {
            return cardNumber.Contains("XXXXXXXX");
        }

        private void SetExpirationYears()
        {
            ddlExpYear.Items.Clear();
            int year = DateTime.Now.Year;
            int century = 2000; //good for next 88 years
            for (int i = year; i < year + 16; i++)
            {
                ddlExpYear.Items.Add(new ListItem(i.ToString(), (i - century).ToString()));
            }
        }
        
        private void repositionModal(bool atTop = false)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                ScriptManager.RegisterStartupScript(pnlPaymentInfoPopup, pnlPaymentInfoPopup.GetType(), "repositionModal", "setTimeout( function () {repositionModal($('#" + Pandel1.ClientID + "'), " + atTop.ToString().ToLower() + "); }, 50);", true);
                ScriptManager.RegisterStartupScript(pnlPaymentInfoPopup, pnlPaymentInfoPopup.GetType(), "repositionModalResize", "jQuery(window).resize(function () { repositionModal($('#" + Pandel1.ClientID + "'), " + atTop.ToString().ToLower() + "); });", true);
            }
        }
    }
}