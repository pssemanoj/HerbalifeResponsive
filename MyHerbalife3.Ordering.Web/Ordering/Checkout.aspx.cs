using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using HL.PGH.Api;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    using System.Xml.Serialization;
    using System.Web.Security;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
    using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

    public partial class Checkout : ProductsBase
    {
        private readonly List<string> _errors = new List<string>();
        private CheckoutOrderSummary _checkoutOrderSummary;
        private PaymentDeclinedInfo _paymentDeclinedInfo;
        private CheckoutTotalsDetailed _checkoutTotalsDetailed;
        private List<FailedCardInfo> _failedCards;
        private InvoiceOptions _invoicOptions;
        private bool _isPaymentGatewayResponse;
        //CheckOutOptions _checkOutOptions = null;
        private PaymentInfoBase _paymentOptions;
        private List<Payment> _payments;
        private bool _use3DSecuredCreditCard;
        private string _refId;
        private PaymentsSummary _paymentsSummary;

        private Controls.Payments.OneTimePin _oneTimePinControl;

        public bool DisplayInvoiceOptions { get; set; }

        [Publishes(MyHLEventTypes.QuoteError)]
        public event EventHandler OnQuoteError;

        [SubscribesTo(MyHLEventTypes.PageVisitRefused)]
        public void OnPageVisitRefused(object sender, EventArgs e)
        {
            var args = e as PageVisitRefusedEventArgs;
            if (null != args)
            {
                if (args.Reason == PageVisitRefusedReason.UnableToPrice)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CurrencyConversionFailed"));
                    blErrors.DataSource = _errors;
                    blErrors.DataBind();
                    checkOutButton.Enabled = false;
                }
            }
        }

        //[SubscribesTo(MyHLEventTypes.CreditCardAuthenticationCompleted)]
        public void OnCardAuthenticated(object sender, EventArgs e)
        {
            checkOutButton.Enabled = ((e as CreditCardAuthenticationCompletedEventArgs).Status ==
                                      OrderCoveredStatus.FullyCovered);
        }

        [Publishes(MyHLEventTypes.CreditCardAuthorizationCompleted)]
        public event EventHandler onCreditCardAuthorizationComplete;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var currentSession = SessionInfo;
            currentSession.UseHMSCalc = HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc;
            //this.UseHmsCalc;
            if (currentSession.DeliveryInfo != null && !string.IsNullOrEmpty(currentSession.CustomerOrderNumber))
            {
                try
                {
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                        ShoppingCart.DeliveryInfo.Instruction = currentSession.DeliveryInfo.Instruction;
                    if (!ShoppingCart.DeliveryInfo.PickupDate.HasValue)
                        ShoppingCart.DeliveryInfo.PickupDate = currentSession.DeliveryInfo.PickupDate;
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))
                        ShoppingCart.DeliveryInfo.Address.Phone = currentSession.DeliveryInfo.Address.Phone;
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                        ShoppingCart.DeliveryInfo.Address.Recipient = currentSession.DeliveryInfo.Address.Recipient;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Error in Checkout.aspx.cs : OnInit  {0}", ex.Message));
                }
            }
            SessionInfo = currentSession;

            if (IsChina)
            {
                _oneTimePinControl = LoadControl("~/Ordering/Controls/Payments/OneTimePin.ascx") as OneTimePin;
                _oneTimePinControl.ValidatePinEvent += _oneTimePinControl_ValidatePinEvent;
                OTPPanel.Controls.Add(_oneTimePinControl);
            }
            else
            {
                OTPPanel.Visible = false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            //{
                bool IsUrlReferrer = (HttpContext.Current.Request.UrlReferrer == null);
                if (IsUrlReferrer)
                {
                    Response.Redirect("~/ordering/ShoppingCart.aspx");
                }
           // }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina && DistributorOrderingProfile.IsPC)
            {

                //if ((HttpContext.Current.Session["AttainedSurvey"] != null) &&
                //    (!Convert.ToBoolean(HttpContext.Current.Session["AttainedSurvey"])))
                //{
                //    Response.Redirect("Survey.aspx?@ctrl=PCPromoSurvey");
                //}
            }

            ScriptManager.GetCurrent(this).AsyncPostBackTimeout = 300;
            hdnCustomerOrder.Value = "false";

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var orders = OrdersProvider.GetOrdersInProcessing(DistributorID, Locale);
                if (orders != null && orders.Any())
                {
                    bool isSubmittingMobilePin = false;

                    var order = (orders.First().Order as Order_V01);

                    if (order != null && order.Payments != null && order.Payments.Count > 0)
                    {
                        var payment = order.Payments.FirstOrDefault();

                        //Quick Pay payment handling.
                        if (payment != null && payment.TransactionType == "QP")
                        {
                            if (_oneTimePinControl != null)
                            {
                                if (_oneTimePinControl.IsMobilePinProvided())
                                {
                                    isSubmittingMobilePin = true;
                                }
                            }
                        }
                    }

                    if (!isSubmittingMobilePin)
                    {
                    var orderNumber = orders.FirstOrDefault().OrderId;
                    ViewState["pendingOrderNumber"] = orderNumber;
                    var isOrderSubmitted = CheckPendingOrderStatus("CN_99BillPaymentGateway", orderNumber, true);
                    ViewState["isOrderSubmitted"] = isOrderSubmitted;
                    if (isOrderSubmitted)
                    {
                        lblOrderStatusMessage.Text =
                            string.Format(GetLocalResourceObject("PendingOrderSubmittedResource").ToString(),
                                          orderNumber);
                        OrderStatusPopupExtender.Show();
                    }
                    var orderStatus = OrderProvider.GetPaymentGatewayRecordStatus(orderNumber);
                    
                    //bug 225894 removed because no longer in use
                    //if (orderStatus == PaymentGatewayRecordStatusType.Unknown)
                    //{
                    //    SessionInfo.PendingOrderId = orderNumber;
                    //}
                }
            }
            }

            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderProducts").ToString());
            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderEvents").ToString());
            }
            if (ShoppingCart.CustomerOrderDetail != null)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderCustomerOrder").ToString());
                checkOutButton.Text = GetLocalResourceObject("CustomerOrderCheckoutButtonText").ToString();
                uxReturnToCustomerOrder.Visible = true;
                hdnCustomerOrder.Value = "true";
                var CountryCO2DO  = Settings.GetRequiredAppSetting("CountryCO2DO", string.Empty).Split(',');
                if (CountryCO2DO.Contains(ShoppingCart.CountryCode))
                    DisplayInvoiceOptions = false;
            }
            ShoppingCartID.Value = ShoppingCart.ShoppingCartID.ToString();

            #region 3D Secured Credit Card: Process Bank Page (3D Popup) Post Back
            if (null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo) 
            {              
                if (SessionInfo.ThreeDSecuredCardInfo.IsErrored)
                {
                    ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.Errored);
                    SessionInfo.ThreeDSecuredCardInfo = null;
                    SessionInfo.Payments = null;
                    SessionInfo.OrderNumber = null;
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                    blErrors.DataSource = _errors;
                    blErrors.DataBind();
                }
                else if(SessionInfo.ThreeDSecuredCardInfo.IsDeclined)
                {
                    ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.Declined);
                    SessionInfo.ThreeDSecuredCardInfo = null;
                    SessionInfo.Payments = null;
                    SessionInfo.OrderNumber = null;
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                    blErrors.DataSource = _errors;
                    blErrors.DataBind();
                }
                else if (SessionInfo.ThreeDSecuredCardInfo.IsEnrolled)
                {
                if (!SessionInfo.ThreeDSecuredCardInfo.IsAuthenticated) // process Bank Page (3D popup) post back data.
                {                
                        try
                        {
                    string Agency = "MD";
                    string threeDPaymentGateWayName = "Herbalife3DSecuredCreditCardAuthentication";
                    var md = Request.Form[Agency] ?? Request.QueryString[Agency] ?? string.Empty;
                            var paRes = Request.Form["PaRes"] ?? Request.QueryString["DATA"] ?? Request.Form["BankPacket"] ?? string.Empty;
                            var signature = Request.QueryString["SIGNATURE"] ?? Request.Form["Sign"] ?? string.Empty;
                            var merchantPacket = Request.Form["MerchantPacket"];
                            var bankPacket = Request.Form["BankPacket"];
                            var sign = Request.Form["Sign"];
                            var merchantId = Request.Form["MerchantId"];

                            // handle same key/value posted back multiple times by bank page. Bank post data should not contain comma. 
                            md = (null != md && md.Contains(",")) ? md.Substring(0, md.IndexOf(',')) : md;
                            paRes = (null != paRes && paRes.Contains(",")) ? paRes.Substring(0, paRes.IndexOf(',')) : paRes;
                            signature = (null != signature && signature.Contains(",")) ? signature.Substring(0, signature.IndexOf(',')) : signature;

                            SessionInfo.ThreeDSecuredCardInfo.PaRes = paRes; // store the returned PaRes
                            SessionInfo.ThreeDSecuredCardInfo.RequestToken = signature ?? SessionInfo.ThreeDSecuredCardInfo.RequestToken;

                            if ( (!string.IsNullOrEmpty(md) && !string.IsNullOrEmpty(paRes) && md == threeDPaymentGateWayName)  // For 3D general
                                || (!string.IsNullOrEmpty(merchantPacket) && !string.IsNullOrEmpty(bankPacket) && merchantId == SessionInfo.ThreeDSecuredCardInfo.MerchantId) ) // For Turkey 3D through Posnet
                            {
                                // Turkey 3D through Posnet, get more data from Request Form
                                if (SessionInfo.ThreeDSecuredCardInfo.CountryCode.ToUpper() == "TR")
                                {
                                    SessionInfo.ThreeDSecuredCardInfo.RequestId = merchantPacket;
                                    SessionInfo.ThreeDSecuredCardInfo.Xid = Request.Form["Xid"];
                                    SessionInfo.ThreeDSecuredCardInfo.ProofXml = string.Concat(SessionInfo.ThreeDSecuredCardInfo.ProofXml, Environment.NewLine, " Authentication Popup Response: ", Request.Form.ToString());
                                }

                                // Save to DB after getting all data from 3D popup returned. IsAuthenticated indicates receiving data from 3D Popup.
                                SessionInfo.ThreeDSecuredCardInfo.IsAuthenticated = true;
                                ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.ThreeDPopupReturned);

                                // Verification
                            if (HLConfigManager.Configurations.PaymentsConfiguration.Use3DVerification)
                            {
                                ThreeDSecuredCreditCard verified3DCard = ThreeDPaymentProvider.Verify3DSecuredAuthentication(SessionInfo.ThreeDSecuredCardInfo);
                                SessionInfo.ThreeDSecuredCardInfo = verified3DCard;
                                    ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.VerificationChecked);
                            }
                                else // if not required to call agency again to verify, such as Cyber Source, mark it as Verified
                                {
                                    SessionInfo.ThreeDSecuredCardInfo.IsVerified = true;
                                }
                            }
                            else // 3D Popup has no required data, errored
                            {
                                SessionInfo.ThreeDSecuredCardInfo.IsErrored = true;
                                SessionInfo.ThreeDSecuredCardInfo.ProofXml = string.Format("MD: {0}. PaRes: {1}.", md, paRes);
                                ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.Errored);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error(
                                string.Format("3D processing exception: Retrieving data from 3D popup or Verifying 3D Authenticaiton for Order Number {0}, Error: {1}",
                                    SessionInfo.OrderNumber, ex.Message));
                            SessionInfo.ThreeDSecuredCardInfo.IsErrored = true;
                            SessionInfo.ThreeDSecuredCardInfo.ProofXml = ex.ToString();
                            ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.Errored);
                        }
                        finally
                        {
                            // Close modal popup
                            var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/Checkout.aspx");
                            var strScript1 = new StringBuilder();
                            strScript1.Append(@"$(document).ready(function()
                            { 
                                var modalPopup = window.parent.$find('ModalPopupBehaviorID'); 
                                if (modalPopup != null) 
                                    { 
                                        modalPopup.hide();
                                        window.parent.location.href = '" + termUrl + @"'; 
                                        var hidePopup = window.parent.$find('popupHide');   
                                        hidePopup.show();
                                    }
                            });");
                            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", strScript1.ToString(), true);
                        }
                        return;
                    }                
                else // Bank Page (3D popup) post back data has been processed. Submit the order.
                {
                    SubmitOrder(SessionInfo.OrderNumber);
                }
            }
            }
            #endregion 3D Secured Credit Card : Process Bank Page (3D Popup) Post Back

            LoadControls();
            if (!IsPostBack)
            {
                lblSubmitText.Visible = ShoppingCart.OrderCategory != OrderCategoryType.HSO;
                lblHAPSubmitText.Visible = !lblSubmitText.Visible;
                if (null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
                {
                    SessionInfo.ThreeDSecuredCardInfo = null;
                    SessionInfo.Payments = null;
                    SessionInfo.OrderNumber = null;
                }

                ShoppingCart.Calculate();
                if (ShoppingCart.Totals == null || (ShoppingCart.Totals as OrderTotals_V01).AmountDue == 0.0M &&
                    !(ShoppingCart.OrderCategory == OrderCategoryType.ETO && HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket) &&
                    (ShoppingCart.ShoppingCartItems != null && ShoppingCart.ShoppingCartItems.Any() &&
                        ShoppingCart.ShoppingCartItems.Sum(i => i.CatalogItem.ListPrice) == 0))
                {
                    if ((!string.IsNullOrWhiteSpace(ShoppingCart.ErrorCode)) && (ShoppingCart.ErrorCode.Contains("ORA-01403")))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoMemberFoundInHMS"));
                        blErrors.DataSource = _errors;
                        blErrors.DataBind();
                    }
                    else                        
                        OnQuoteError(null, null);
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.KountEnabled)
                {
                    var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var dsSubTypeCode = distributorProfileModel.Value.SubTypeCode.Trim();
                    var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, ShoppingCart.CountryCode);
                    if (MyHerbalife3.Ordering.Providers.FraudControl.FraudControlProvider.IsSubjectToFraudCheck(this.Locale, ShoppingCart, dsSubTypeCode, distributorOrderingProfile.ApplicationDate, HlCountryConfigurationProvider.GetCountryConfiguration(Locale)))
                    {
                        var fraudControl = LoadControl("~/Ordering/Controls/Payments/Kount/DataCollector.ascx");
                        plFraudControl.Controls.Add(fraudControl);
                    }
                }

                PaymentGatewayResponse gatewayResponse;

                if (Session[PaymentGatewayResponse.PaymentGateWateSessionKey] == null)
                {
                    gatewayResponse = PaymentGatewayResponse.Create();
                }
                else
                {
                    gatewayResponse = (PaymentGatewayResponse)Session[PaymentGatewayResponse.PaymentGateWateSessionKey];
                    Session[PaymentGatewayResponse.PaymentGateWateSessionKey] = null;
                    if (HLConfigManager.Configurations.PaymentsConfiguration.SetDeclinedOrderNumber)
                    {
                        HttpContext.Current.Session["DeclinedOrderNumber"] = gatewayResponse.OrderNumber;
                    }
                }

                if (null != gatewayResponse) //If this is not null - we have a redirect from a recognized payment gateway
                {
                    _isPaymentGatewayResponse = true;
                    if (!gatewayResponse.IsApproved)
                    {
                        // Payment was cancelled do nothing
                        if (!gatewayResponse.IsCancelled)
                        {
                            RemoveDupCheckCookie();
                            _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                            blErrors.DataSource = _errors;
                            blErrors.DataBind();
                        }
                    }
                }
                // Display saved cart name
                if (HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts)
                {
                    if (ShoppingCart != null && ShoppingCart.IsSavedCart
                        && !ShoppingCart.IsFromCopy && !string.IsNullOrEmpty(ShoppingCart.CartName))
                    {
                        SavedCartTitle.Visible = true;
                        lblSavedCartName.Text = string.Format(GetLocalResourceObject("lblSavedCartName").ToString(), ShoppingCart.CartName);
                    }
                }
                // Display the HFF message if the item is in the cart
                if (HLConfigManager.Configurations.DOConfiguration.AllowHFF &&
                    HLConfigManager.Configurations.DOConfiguration.ShowHFFMessage &&
                    !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    if (ShoppingCart.CartItems.Any(i => i.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                    {
                        HFFMessage.Visible = true;
                    }
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts && ShoppingCart != null && ShoppingCart.OrderCategory == OrderCategoryType.RSO)
            {
                // If is a saved cart hide the cancel button
                if (ShoppingCart.IsSavedCart)
                {
                    uxCancelOrder.Visible = false;
                    mdlConfirmDelete.TargetControlID = "MpeFakeTarget1";
                }
                else
                {
                    uxCancelOrder.Enabled = !(APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) && HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed);
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
            {
                (Master as OrderingMaster).divLeftVisibility = true;
            }
            else
            {
                (Master as OrderingMaster).divLeftVisibility = false;
            }
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-cho");
            DisplayELearningMessage();

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && !string.IsNullOrEmpty(ShoppingCart.HAPAction) && ShoppingCart.HAPAction == "UPDATE")
            {
                divHapEditMessage.Visible = true;
        }
        }

        protected virtual void LoadControls()
        {
             if (
                !String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsDetailedControl))
            {
                try
                {
                    _checkoutTotalsDetailed =
                        LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsDetailedControl)
                        as CheckoutTotalsDetailed;
                }
                catch (Exception e)
                {
                    LoggerHelper.Error("Load CheckoutTotalsDetailedControl failed!\n" + e);
                }
                plCheckOutTotalsDetails.Controls.Add(_checkoutTotalsDetailed);
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary))
            {
                _checkoutOrderSummary =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary) as
                    CheckoutOrderSummary;
                _checkoutOrderSummary.DisplayReadOnlyGrid = true;
                _checkoutOrderSummary.OrderSummaryText = GetLocalResourceObject("OrderSummaryStaticText").ToString();

                _checkoutOrderSummary.OmnitureState = "scCheckout";
                plCheckOutOrderDetails.Controls.Add(_checkoutOrderSummary);
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl))
            {
                var _checkoutOptionsControl =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl);
                var _checkOutOptions = _checkoutOptionsControl as CheckOutOptions;
              
                _checkOutOptions.IsStatic = (Page as ProductsBase).CheckoutOptionsAreStatic;
                var _Message =(Label)_checkOutOptions.FindControl("lblMessage");
                var _Notifiaction= (Label)_checkOutOptions.FindControl("lblMessageNotifyReadOnly");
                if (_Message != null && _Notifiaction != null && (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ||ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup))
                {
                    _Message.Visible = HLConfigManager.Configurations.CheckoutConfiguration.CheckOutShowNotification;
                    _Notifiaction.Visible = HLConfigManager.Configurations.CheckoutConfiguration.CheckOutMessageNotify;
                }
               
              
                plCheckOutOptions.Controls.Add(_checkOutOptions);
            }

            if (DisplayInvoiceOptions)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.InvoiceOptionsControl))
                {
                    _invoicOptions =
                        LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.InvoiceOptionsControl) as
                        InvoiceOptions;
                    _invoicOptions.IsReadOnly = false;
                    plInvoiceOptions.Controls.Add(_invoicOptions);
                }
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl))
            {
                if (!HasZeroPriceEventTickets(ShoppingCart))
                {
                    var paymentsControl =
                        LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl);
                    plPaymentOptions.Controls.Add(paymentsControl);
                    _paymentOptions = paymentsControl as PaymentInfoBase;
                }
            }
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl)
                && HLConfigManager.Configurations.DOConfiguration.ShowStaticPaymentInfo
                && Session[PaymentGatewayResponse.PaymentGateWateSessionKey] != null)
            {
                var gatewayResponse = Session[PaymentGatewayResponse.PaymentGateWateSessionKey] as PaymentGatewayResponse;
                if (gatewayResponse != null && gatewayResponse.Status == PaymentGatewayRecordStatusType.Declined)
                {
                    _paymentsSummary = LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl) as PaymentsSummary;
                    if (_paymentsSummary != null)
                    {
                        plPaymentOptionsSummary.Controls.Add(_paymentsSummary);
                    }
                }
            }
            
            PolicyMessage.Visible = HLConfigManager.Platform == "PCAD";

            //add paymentdeclinedinfo conmtrol
           if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentDeclinedInfoControl))
           {
               _paymentDeclinedInfo = 
                   LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentDeclinedInfoControl
                ) as PaymentDeclinedInfo; 
            plDeclinedInfo.Controls.Add(_paymentDeclinedInfo);
            } 

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl))
                {
                    var _checkoutHAPOptions =
                        LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl);
                    plCheckOutHAPOptions.Controls.Add(_checkoutHAPOptions);
                }
                uxCancelOrder.Text = uxCancelOrderDisabled.Text = GetLocalResourceObject("uxCancelHapOrderResource1.Text").ToString();
            }

        }

        protected void OnSubmit(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, "CN");
                var custid = distributorOrderingProfile != null ? distributorOrderingProfile.CNCustomorProfileID : 0;
                IDictionaryEnumerator en = Cache.GetEnumerator();
                while (en.MoveNext())
                {
                    if (
                        en.Key.ToString()
                          .StartsWith(string.Format(OrderListView.OrderListCaheKey + ShoppingCart.DistributorID)))
                   {
                       HttpRuntime.Cache.Remove(en.Key.ToString());
                   }
                }

                CacheFactory.Create().Expire(typeof(PromotionInformation), string.Format("CN_{0}_{1}", "PCPromotionInfo", custid.ToString().Trim()));
              
                string PCPromoSessionKey = ChinaPromotionProvider.PCPromoSessionKey(distributorOrderingProfile.CNCustomorProfileID.ToString());
                if (HttpContext.Current.Session[PCPromoSessionKey] != null)
                    Session.Remove(PCPromoSessionKey);
                //HttpContext.Current.Session[PCPromoSessionKey] = null;
            }
            //Check For Errors
            bool PaymentEntered = checkPayment();
            bool ShipmentEntered = checkShipment();
            bool PickupInstructionsEntered = checkPickupInstructions();
            bool checkPricing = CheckPricing();
            bool EmailAddress = checkEmailAddress();
            bool HAPOptionsEnterd = checkHAPOptions();
            bool isValidDeliveryInfo = CheckDeliveryInfo();
            bool PickupPhoneNumberEnter = PickupPhoneNumberEntered();
            bool InvoiceOptionSelected = checkInvoiceOption();

            if (!PaymentEntered || !ShipmentEntered || !PickupInstructionsEntered || !EmailAddress || !checkPricing || !HAPOptionsEnterd || !isValidDeliveryInfo || !PickupPhoneNumberEnter || !InvoiceOptionSelected)
            {
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                return;
            }

            ShoppingCart.InvoiceOption = _invoicOptions != null ? _invoicOptions.SelectedInvoiceOption : null;

            SubmitOrder(String.Empty);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (HLConfigManager.Configurations.CheckoutConfiguration.RequiresPaymentAuthenticationToSubmit)
            {
                var address = new ServiceProvider.OrderSvc.Address_V01();
                var payments = new List<Payment>();
                checkOutButton.Enabled = _paymentOptions.ValidateAndGetPayments(address, out payments, false);
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequiresAcknowledgementToSubmit && _paymentOptions != null)
            {
                checkOutButton.Enabled = _paymentOptions.IsAcknowledged;
                checkOutButton.Disabled = !_paymentOptions.IsAcknowledged;
                checkOutButton.OnClientClick = _paymentOptions.IsAcknowledged ? "return HideSubmit(this)" : null;
            }

            if (_errors.Count == 0)
            {
                blErrors.DataSource = new List<string>();
                blErrors.DataBind();
            }
            if (HLConfigManager.Configurations.DOConfiguration.ShowBulletinBoard)
            {
                var orderingMaster = Master as OrderingMaster;
                if (orderingMaster != null) orderingMaster.SetBulletinBoardVisibility(false);
        }
        }

        private bool checkPickupInstructions()
        {
            if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup ||
                ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.IsPickupInstructionsRequired)
                {
                    if (APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
                    {
                        ShoppingCart.DeliveryInfo.Instruction = "APF";
                        return true;
                    }
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                        ((HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveName &&
                        string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient)) ||
                        (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHavePhone &&
                        string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingInfoIncomplete"));
                        return false;
                    }
                    else if(ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier &&
                        ((HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHaveName &&
                        string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient)) ||
                        (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierHavePhone &&
                        string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingInfoIncomplete"));
                        return false;
                }
                }
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveRGNumber) 
                {
                    if (ShoppingCart.DeliveryInfo == null || string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.RGNumber))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoRGNumberSelected"));
                        return false;
                    }
                }
            }
            return true;
        }

        private bool checkCasa()
        {
            return true;
        }

        private bool checkShipment()
        {
            bool noError = true;            
            if (ShoppingCart == null || ShoppingCart.DeliveryInfo == null)
                return false;

            if (ShoppingCart.DeliveryInfo == null || ShoppingCart.DeliveryInfo.Address == null || ShoppingCart.DeliveryInfo.Address.Address == null)
            {
                noError = false;
            }
            else if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Address.Line1))
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DoNotCheckAddressLine1)
                {
                    noError = false;
                }
            }

            if (!noError)
            {
                if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    var config = HLConfigManager.Configurations.CheckoutConfiguration;
                    if (config.ShippingAddressRequiredForPickupOrder)
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDefaultShippingAddress"));
                    }
                    else
                    {
                        noError = true;
                    }
                }
                else
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingInfoIncomplete"));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                {
                    var distributor = GlobalContext.CurrentDistributor;
                    if (distributor != null)
                        ShoppingCart.DeliveryInfo.Address.Recipient = string.Format("{0} {1}",
                                                                                    distributor.FirstName,
                                                                                    distributor.LastName);
                }

                //if (HLConfigManager.Configurations.DOConfiguration.IsChina && string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))
                //{
                //    var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                //    if (HLConfigManager.Configurations.DOConfiguration.IsChina && (ShoppingCart.CartItems.Count == 0) && (OrderTotals != null && OrderTotals.Donation > Decimal.Zero))
                //    {
                //        ShoppingCart.DeliveryInfo.Address.Phone = "11111111111";
                //    }
                //    else
                //    {
                //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone"));
                //        noError = false;
                //    }
                   
                //}
            }
            return noError;
        }

        private bool checkEmailAddress()
        {
            bool noError = true;
            ShoppingCart.EmailAddress = ShoppingCart.EmailAddress ??
                                        SessionInfo.GetSessionInfo(DistributorID, Locale).ChangedEmail;
            if (string.IsNullOrEmpty(ShoppingCart.EmailAddress) && HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail)
            {
                noError = false;
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoEmailAddress"));
            }

            return noError;
        }

        private bool checkPayment()
        {
            if (!HasZeroPriceEventTickets(ShoppingCart))
            {
            if (null != _paymentOptions)
            {
                    if (
                        !_paymentOptions.ValidateAndGetPayments(ObjectMappingHelper.Instance.GetToOrder(ShoppingCart.DeliveryInfo.Address.Address), out _payments))
                {
                    //_errors.AddRange(_paymentOptions.Errors);
                    return false;
                }
            }
                if (ShoppingCart.Totals != null && (ShoppingCart.Totals as OrderTotals_V01).AmountDue == 0.0M)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TotalNotMatch"));
                    return false;
                }
                
            }
            
            return _errors.Count == 0;
        }

        private bool CheckPricing()
        {
            if (!HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc)
            {
                return true;
            }

            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, CultureInfo.CurrentCulture.Name);
            var pricingType = sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.Totals != null
                                  ? (sessionInfo.ShoppingCart.Totals as OrderTotals_V01).PricingType
                                  : string.Empty;
            if (HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc && sessionInfo.HmsPricing &&
                !string.IsNullOrEmpty(pricingType) && pricingType == "HMSCalc")
            {
                return true;
            }
            else
            {
                if (HasZeroPriceEventTickets(ShoppingCart))
                {
                    return true;
                }
                else
                {
                    var myHlCart = ShoppingCart as MyHLShoppingCart;
                    var calcTheseItems = new List<ShoppingCartItem_V01>(myHlCart.CartItems);
                    var totals = myHlCart.Calculate(calcTheseItems, true);
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PriceChanged"));
                    return false;
                }
            }
        }

        private bool CheckDeliveryInfo()
        {
            if (!IsChina) return true;

            bool noError = true;

            string strDeliveryPhone = ShoppingCart.DeliveryInfo.Address.Phone;

            if (string.IsNullOrEmpty(strDeliveryPhone))
            {
                noError = false;
                _errors.Add(GetLocalResourceObject("NoPhoneNumber") as string);
            }

            return noError;

        }
        private bool PickupPhoneNumberEntered()
        {
            if (IsChina && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                var myHlCart = ShoppingCart as MyHLShoppingCart;
                if (string.IsNullOrEmpty(myHlCart.ReplaceAltPhoneNumber))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                            "NoPickUpPhoneEntered"));
                    return false;
                }
                else
                {
                    return true;
                }

            }
            return true;

        }

        private bool checkHAPOptions()
        {
            if (ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                if (string.IsNullOrEmpty(ShoppingCart.HAPType) || ShoppingCart.HAPScheduleDay < 1)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "MissingHapType"));
                    return false;
                }
            }
            return true;
        }

        private bool checkInvoiceOption()
        {
            if (string.IsNullOrEmpty(ShoppingCart.InvoiceOption) && HLConfigManager.Configurations.CheckoutConfiguration.RequiresInvoiceOption)
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "MissingInvoiceOption"));
                return false;
            }
            return true;
        }

        private void SubmitOrder(string orderID)
        {
            
            if (string.IsNullOrEmpty(orderID))
                // If the order number is present, then don't show the popup. It is redirected from the Payment site.
            {
                var orderMonth = new OrderMonth(CountryCode);
                if (orderMonth.IsDualOrderMonth && !IsEventTicketMode &&
                    HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled && !IsHAPMode)
                {
                    lblDMValue.Text =
                        string.Format(
                            PlatformResources.GetGlobalResourceString("ErrorMessage", "SelectedDualOrderMonthMessage"),
                            GetOrderMonthString());
                    btnDMYes.CommandArgument = orderID;
                    dualOrderMonthPopupExtender.Show();
                }
                else
                {
                    DoSubmitOrder(orderID);
                }
            }
            else
            {
                DoSubmitOrder(orderID);
            }
            
        }

        private void DoSubmitOrder(string orderID)
        {
            if (IsChina && ShoppingCart.OrderCategory==OrderCategoryType.ETO)
            {
                var hasOutStockItems = _checkoutOrderSummary.GetListToRemove();

                if (hasOutStockItems.Count>0)
                {
                    if (ShoppingCart.CustomerOrderDetail != null)
                    {
                        _errors.Add(
                            GetLocalResourceObject("CustomerOrderItemUnavailableCheckoutError").ToString());
                        blErrors.DataSource = _errors;
                        blErrors.DataBind();
                        return;
                    }
                    var prodToRemove = (ListView)(_checkoutOrderSummary.FindControl("uxProdToRemove"));
                    var updConfirmUnavailable = (UpdatePanel)(_checkoutOrderSummary.FindControl("updConfirmUnavailable"));
                    var mdlConfirm = (ModalPopupExtender)(_checkoutOrderSummary.FindControl("mdlConfirm"));
                    if (prodToRemove != null)
                    {
                        prodToRemove.DataSource = hasOutStockItems;
                        prodToRemove.DataBind();
                    }
                    if (updConfirmUnavailable !=null)
                    {
                        updConfirmUnavailable.Update();
                    }
                    if (mdlConfirm !=null)
                    {
                        mdlConfirm.Show();
                    }
                    
                    return;
                }
            }
            // Get the current logged in DS details.
            //var order = new Order_V01();

            if (HasZeroPriceEventTickets(ShoppingCart))
            {
                //Generate Order Number
                var request = new GenerateOrderNumberRequest_V01
                {
                    Amount = (ShoppingCart.Totals as OrderTotals_V01).AmountDue,
                    Country = CountryCode,
                    DistributorID = DistributorID
                };
                var response = OrderProvider.GenerateOrderNumber(request);
                if (null != response)
                {
                    orderID = response.OrderID;
                    //SessionInfo.OrderNumber = orderID;
                }
            }

            var order = OrderCreationHelper.CreateOrderObject(ShoppingCart) as Order_V01;
            // Populate generic/distributor details
            order.DistributorID = DistributorID;
            order.CountryOfProcessing = CountryCode;
            string countryCode = CountryCode;
            if (SessionInfo.DsType == null)
            {
               // string country = CultureInfo.CurrentCulture.Name.Substring(3).ToUpper();
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(order.DistributorID, countryCode);
                SessionInfo.DsType = DistributorType;

            }
            if (SessionInfo.DsType == MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.Scheme.Member)
            {
                order.OrderIntention = OrderIntention.PersonalConsumption;
            }
            else
            {
                if (SessionInfo != null && SessionInfo.HAPOrderType != null)
                {
                    if (SessionInfo.HAPOrderType == "Personal")
                    {
                        order.OrderIntention = OrderIntention.PersonalConsumption;
                    }
                    else if (SessionInfo.HAPOrderType == "RetailOrder")
                    {
                        order.OrderIntention = OrderIntention.RetailOrder;
                    }
                }
            }
            order.OrderID = orderID;
            if (ShoppingCart.OrderCategory == OrderCategoryType.HSO && !string.IsNullOrEmpty(ShoppingCart.OrderNumber))
                order.OrderID = ShoppingCart.OrderNumber;
            var recvdDate = DateUtils.GetCurrentLocalTime(countryCode);
            order.ReceivedDate = recvdDate;
            //order.OrderMonth = getYYMMOrderMonth();
            //order.OrderMonth = (this.Page as ProductsBase).OrderMonthShortString;
            order.OrderMonth = GetOrderMonthShortString();
            order.OrderCategory =
                (ServiceProvider.OrderSvc.OrderCategoryType)Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), ShoppingCart.OrderCategory.ToString());
            //OrderCategoryType.RSO;
            if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.ETO)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType))
                {
                    order.OrderCategory =
                        (ServiceProvider.OrderSvc.OrderCategoryType)
                        Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                                   HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType);
                }
            }

            if (APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, string.Empty))
            {
                OrderProvider.SetAPFDeliveryOption(ShoppingCart);
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }
            order.Shipment = OrderProvider.CreateShippingInfo(countryCode, ShoppingCart);

            // invoice option
            //string invOpt = _invoicOptions.SelectedInvoiceOption == null ? string.Empty : _invoicOptions.SelectedInvoiceOption;
            string invOpt = _invoicOptions == null ? string.Empty : _invoicOptions.SelectedInvoiceOption;
            order.Handling = OrderProvider.CreateHandlingInfo(CountryCode, invOpt, ShoppingCart,
                                                              order.Shipment as ShippingInfo_V01);

            // Populate payment information.
            if (_payments != null && _payments.Count > 0)
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in _payments select p).ToArray());
            }
            else if (SessionInfo.Payments != null && SessionInfo.Payments.Count > 0)
                // this is for 3D Secured orders; we put Payments in session before popup 3D bank page
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in SessionInfo.Payments select p).ToArray());
            }
            else
            {
                if (!HasZeroPriceEventTickets(ShoppingCart))
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo"));
                    blErrors.DataSource = _errors;
                    blErrors.DataBind();
                    return;
                }
            }

            if (!OrderProvider.IsValidToSubmit(order, ShoppingCart))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantBuy"));
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                return;
            }

            if (!OrderProvider.IsValidToSubmit(order, ShoppingCart))
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantBuy"));
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                return;
            }

            if (IsChina && !APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
            {
                //Defensive handling to prevent wrong express company being assigned unintentionally.
                if (ShoppingCart.DeliveryInfo != null && (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier))
                {
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.FreightCode) || ShoppingCart.DeliveryInfo.FreightCode == "0")
                    {
                        HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                            "\nDS: " + ShoppingCart.DistributorID +
                                                            "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                            "\nAddress ID: " + ShoppingCart.DeliveryInfo.Address == null ? "" : ShoppingCart.DeliveryInfo.Address.ID +
                                                            "\nUrlReferrer: " + Request.UrlReferrer ?? "");

                        //Set this to bring the user attention.
                        if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                        {
                            ShoppingCart.DeliveryInfo.FreightCode = "";
                            ShoppingCart.DeliveryInfo.Id = -1;
                        }

                        Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                        return;
            }
                    else
                    {
                        if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                        {
                            if (ShoppingCart.CartItems.Any())
                            {
                                var deliveryOptions = base.GetShippingProvider().GetDeliveryOptionsListForShipping(CountryCode, Locale, ShoppingCart.DeliveryInfo.Address);

                                if (!deliveryOptions.Any(oi => oi.FreightCode.Equals(ShoppingCart.DeliveryInfo.FreightCode)))
                                {
                                    var availableFreightCode = String.Join(",", deliveryOptions.Select(a => a.FreightCode).ToList());
                                    HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                                           "\nDS: " + ShoppingCart.DistributorID +
                                                                            "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                                            "\nAddress ID: " + ShoppingCart.DeliveryInfo.Address == null ? "" : ShoppingCart.DeliveryInfo.Address.ID +
                                                                           "\nAvailable freight code: " + availableFreightCode +
                                                                           "\nUrlReferrer: " + Request.UrlReferrer ?? "");
                                    Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                            }
                           
                        }
                        else
                        {
                            var deliveryOption = base.GetShippingProvider().GetShippingInfoFromID(ShoppingCart.DistributorID, Locale, ShoppingCart.DeliveryInfo.Option, ShoppingCart.DeliveryInfo.Id, 0);

                            if (deliveryOption == null || deliveryOption.FreightCode == null || !deliveryOption.FreightCode.Equals(ShoppingCart.DeliveryInfo.FreightCode))
                            {
                                HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                                       "\nDS: " + ShoppingCart.DistributorID +
                                                                        "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                                        "\nDelivery ID: " + ShoppingCart.DeliveryInfo.Id.ToString() +
                                                                       "\nAvailable freight code: " + ((deliveryOption == null || deliveryOption.FreightCode == null) ? "" : deliveryOption.FreightCode) +
                                                                       "\nUrlReferrer: " + Request.UrlReferrer ?? "");

                                //Set this to bring the user attention.
                                ShoppingCart.DeliveryInfo.FreightCode = "";
                                ShoppingCart.DeliveryInfo.Id = -1;

                                Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                                return;
                            }
                        }
                    }
                }
            }

            //Here will check TW SKU Quantity
            FinalTWSKUQuantityCheck(order);
            prepareOrderAndSubmit(order, countryCode);
        }

        private bool ValidateForDupe(Order_V01 order, int shoppingCartID)
        {
            bool isValid = true;
            int interval = HLConfigManager.Configurations.DOConfiguration.DupeCheckDaysInterval;
            if (interval > 0 && DupeCheckDone.Value != Boolean.TrueString)
            {
                var dupeOrderInfo = OrderProvider.CheckForRecentDupeOrder(order, shoppingCartID);
                if (dupeOrderInfo.IsDuplicdate)
                {
                    SessionInfo.OrderNumber = dupeOrderInfo.OrderNumber; 
                    Response.Redirect("Confirm.aspx?OrderNumber=" + dupeOrderInfo.OrderNumber);
                }
                else
                {
                    if (dupeOrderInfo.OrderDate > DateTime.MinValue)
                    {
                        var now = DateUtils.GetCurrentLocalTime((Page as ProductsBase).CountryCode);
                        var localDupeDate = DateUtils.ConvertToCurrentLocalTime(dupeOrderInfo.OrderDate,
                                                                                (Page as ProductsBase).CountryCode);
                        if ((now - localDupeDate).Days < interval)
                        {
                            //isValid = false;
                            lblDupeOrderMessage.Text =
                                string.Format(
                                    PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                              "PossibleDuplicateOrderFound"),
                                    dupeOrderInfo.OrderDate.ToShortDateString(), dupeOrderInfo.OrderNumber);
                            //DupeCheckDone.Value = Boolean.TrueString;
                            ddlConfirmSubmit.SelectedIndex = 0;
                            dupeOrderPopupExtender.Show();
                        }
                    }
                    else
                    {
                        DupeOkClicked.Value = DupeCheckDone.Value = Boolean.TrueString;
                    }
                }
            }

            return isValid;
        }
        
        private void prepareOrderAndSubmit(Order_V01 order, string countryCode)
        {
            HLRulesManager.Manager.PerformTaxationRules(order, (Page as ProductsBase).Locale);

            #region 3D Secured Credit Card: Determind if it is 3D Secured
            if (null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
            {
                _use3DSecuredCreditCard = true;
            }
            else
            {
                _use3DSecuredCreditCard = HLConfigManager.Configurations.PaymentsConfiguration.Use3DSecuredCreditCard
                                          && order.Payments.OfType<CreditPayment_V01>().Any()
                                          && (null == _paymentOptions || !_paymentOptions.IsUsingPaymentGateway);
            }
            #endregion 3D Secured Credit Card: Determind if it is 3D Secured
           if (SessionInfo !=null && !string.IsNullOrEmpty( SessionInfo.CO2DOSMSNumber ) && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.CO2DOSMSNotification && ShoppingCart.CustomerOrderDetail != null)
            {
                ShoppingCart.SMSNotification = SessionInfo.CO2DOSMSNumber;

            }

            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, countryCode))
            {
                if (order.PurchasingLimits == null)
                {
                    var limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
                    if (null != limits)
                    {
                        limits.PurchaseSubType = ShoppingCart.SelectedDSSubType;
                        order.PurchasingLimits = limits;
                        order.PurchaseCategory = limits.PurchaseType;
                    }
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.usesOrderManagementRules)
            {
                HLRulesManager.Manager.PerformOrderManagementRules(ShoppingCart, order, ShoppingCart.Locale, OrderManagementRuleReason.OrderBeingSubmitted);
            }

            _errors.Clear();
            order = OrderProvider.PopulateLineItems(CountryCode, order, ShoppingCart) as Order_V01;
            if (!_isPaymentGatewayResponse)
            {
                if (!_use3DSecuredCreditCard || (null != SessionInfo && null == SessionInfo.ThreeDSecuredCardInfo))
                {
                    //Validate for Dupe
                    //if (!ValidateForDupe(order, ShoppingCart.ShoppingCartID)) return;
                    ValidateForDupe(order, ShoppingCart.ShoppingCartID);
                    if (DupeOkClicked.Value != Boolean.TrueString)
                        return;
                }
            }

            string error = string.Empty;
            var currentSession = SessionInfo;

            if (ShoppingCart != null && ShoppingCart.Totals != null)
            {
                order.DiscountPercentage = (ShoppingCart.Totals as OrderTotals_V01).DiscountPercentage;

                if (IsChina)
                {
                    var payment = order.Payments.FirstOrDefault();

                    //Quick Pay payment handling.
                    if (payment != null && payment.TransactionType == "QP")
                    {
                        if (_oneTimePinControl != null)
                        {
                            if (!_oneTimePinControl.IsMobilePinProvided())
                            {
                                OrderNumber = ThreeDPaymentProvider.GenerateOrderNumberFor3DPayment((ShoppingCart.Totals as OrderTotals_V01).AmountDue, CountryCode, DistributorID);
                                SessionInfo.OrderNumber = OrderNumber;
                                if (string.IsNullOrEmpty(OrderNumber))
                                {
                                    SessionInfo.ThreeDSecuredCardInfo = null;
                                    SessionInfo.Payments = null;
                                    SessionInfo.OrderNumber = null;
                                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                                    blErrors.DataSource = _errors;
                                    blErrors.DataBind();
                                    return;
                                }

                                _oneTimePinControl.MobilePin = "";
                                _oneTimePinControl.RequestPin(OrderNumber, order, ShoppingCart);

                                if (!string.IsNullOrEmpty(_oneTimePinControl.LastErrorMessage))
                                {
                                    _errors.Add(_oneTimePinControl.LastErrorMessage);
                                    blErrors.DataSource = _errors;
                                    blErrors.DataBind();
                                }
                                return;
                            }
                            else
                            {
                                if (order.Payments != null && order.Payments.Count > 0)
                                {
                                    if (string.IsNullOrEmpty(order.OrderID))
                                    {
                                        order.OrderID = SessionInfo.OrderNumber;
                                    }

                                    ((order.Payments[0] as CreditPayment_V01).Card as QuickPayPayment).MobilePin = _oneTimePinControl.MobilePin;
                                    ((order.Payments[0] as CreditPayment_V01).Card as QuickPayPayment).Token = _oneTimePinControl.Token;
                                }
                            }
                        }
                    }
                }

                #region 3D Secured Credit Card: Check Enrollment and Redirect to Bank Page

                if (_use3DSecuredCreditCard && null != SessionInfo && null == SessionInfo.ThreeDSecuredCardInfo)
                {
                    string Agency = "MD";
                    var md = Request.Form[Agency] ?? Request.QueryString[Agency] ?? string.Empty;
                    if (string.IsNullOrEmpty(md))
                    {
                        // step 1: get order number before calling 3D Enrollment service
                        if (string.IsNullOrEmpty(order.OrderID))
                        {
                            OrderNumber = ThreeDPaymentProvider.GenerateOrderNumberFor3DPayment((ShoppingCart.Totals as OrderTotals_V01).AmountDue, CountryCode, DistributorID);
                            SessionInfo.OrderNumber = OrderNumber;
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                SessionInfo.ThreeDSecuredCardInfo = null;
                                SessionInfo.Payments = null;
                                SessionInfo.OrderNumber = null;
                                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                                blErrors.DataSource = _errors;
                                blErrors.DataBind();
                                return;
                            }
                        }
                        else
                        {
                            OrderNumber = order.OrderID;
                            SessionInfo.OrderNumber = OrderNumber;
                        }

                        // Step 2: Check 3D Secured credit card enrollment
                        var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                        var rootUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost);
                        var threeDSecuredCreditCard = ThreeDPaymentProvider.Check3DSecuredEnrollment(order.Payments, order.CountryOfProcessing, OrderNumber, DistributorID, Locale, rootUrl);

                        // Errored or received Declined when calling Enrollment Check, stop
                        if (null == threeDSecuredCreditCard || threeDSecuredCreditCard.IsErrored || threeDSecuredCreditCard.IsDeclined)
                        {
                            SessionInfo.ThreeDSecuredCardInfo = null;
                            SessionInfo.Payments = null;
                            SessionInfo.OrderNumber = null;
                            _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                            blErrors.DataSource = _errors;
                            blErrors.DataBind();
                            return;
                        }
                        
                        // Step 3: if enrolled, generate popup to go to issuer bank page, and handle the responese; 
                        // if not enrolled, move to create order and submit; 
                        SessionInfo.Payments = _payments;
                        SessionInfo.ThreeDSecuredCardInfo = threeDSecuredCreditCard;

                        // CC is Enrolled but not been to bank page, popup to go to bank page
                        if (threeDSecuredCreditCard.IsEnrolled && !threeDSecuredCreditCard.IsAuthenticated)                           
                        {
                            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/Controls/Payments/3DSecured/ThreeDAuthenticate.aspx");

                            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", "LoadUrl('" + termUrl + "');", true);
                            btnModalPopup_ModalPopupExtender.Show();
                            return;
                        }
                    }
                }
                #endregion 3D Secured Credit Card: Check Enrollment and Redirect to Bank Page

                // Create theOrder.
                #region 3D Secured Credit Card: Create Order
                object theOrder = null;
                if (_use3DSecuredCreditCard && null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo) 
                {
                    // CC is enrolled but not authenticated, will not submit. 
                    if (SessionInfo.ThreeDSecuredCardInfo.IsEnrolled
                        && (!SessionInfo.ThreeDSecuredCardInfo.IsAuthenticated || !SessionInfo.ThreeDSecuredCardInfo.IsVerified || SessionInfo.ThreeDSecuredCardInfo.IsErrored || SessionInfo.ThreeDSecuredCardInfo.IsDeclined))
                    {
                        RemoveDupCheckCookie();
                        LoggerHelper.Error(string.Format("Order Payment error when submitting order:{0} Error : {1}", OrderNumber,
                                          "Credit Card is 3D Secured Enrolled but not Authenticated or not Verified or has error. "));
                        SessionInfo.ThreeDSecuredCardInfo = null;
                        SessionInfo.Payments = null;
                        SessionInfo.OrderNumber = null;
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                        blErrors.DataSource = _errors;
                        blErrors.DataBind();
                        return;
                    }

                    // Generate 3D CC theOrder when: (1) Enrolled, Authenticated and Verified without error and Declined response, or (2) Not Enrolled. Need to pass Enrollment and Authentication/Verification status data in either case.
                    if (string.IsNullOrEmpty(order.OrderID))
                    {
                        order.OrderID = SessionInfo.OrderNumber;
                    }
                    theOrder = OrderProvider.CreateOrder(order, ShoppingCart, countryCode, SessionInfo.ThreeDSecuredCardInfo);
                    ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.OrderCreated);
                }
                else
                {
                    theOrder = OrderProvider.CreateOrder(order, ShoppingCart, countryCode);
                }
                #endregion 3D Secured Credit Card: Create Order

                if (theOrder != null)
                {
                    (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).AgreedDuplicate = DupeOkClicked.Value;
                    if (Settings.GetRequiredAppSetting("DupCheckWithReferenceId", false))
                    {
                        //if (_use3DSecuredCreditCard && null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
                        //{
                        //    if (string.IsNullOrEmpty(OrderRefId.Value))
                        //    {
                        //        string refId = GetDupCheckCookie();
                        //        if (!string.IsNullOrEmpty(refId))
                        //            (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = refId;
                        //    }
                        //    else
                        //        (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = OrderRefId.Value;
                        //}
                        if (!_use3DSecuredCreditCard && (_paymentOptions != null && !_paymentOptions.IsUsingPaymentGateway))
                        {
                            (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = OrderRefId.Value;
                        }
                    }
                }

                //Redirect if using payment gateway (make sure we're not returning from one currently)
                if (!_isPaymentGatewayResponse && null != _paymentOptions && _paymentOptions.IsUsingPaymentGateway)
                {
                    if (!HLConfigManager.Configurations.PaymentsConfiguration.IsUsingHub)
                    {
                        if (!string.IsNullOrEmpty(SessionInfo.PendingOrderId) &&
                            HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            var orders = OrderProvider.GetPaymentGatewayOrder(SessionInfo.PendingOrderId);
                            if (orders != null)
                            {
                                bool valid = OrderProvider.ValidateOrders(orders.BTOrder, theOrder as ServiceProvider.SubmitOrderBTSvc.Order);
                                if (!valid)
                                {
                                   TermConditionPopupExtender.Show();
                                   return;
                                    
                                }
                            }
                        }

                        //OrderAmount = double.Parse((ShoppingCart.Totals as OrderTotals_V01).AmountDue.ToString());
                        OrderProvider.ValidateInstructions(theOrder, order, ShoppingCart);

                        string creditCardNumber = "";
                        string cvv = "";

                        if (IsChina)
                        {
                            if (((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments != null && ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments.Count() > 0)
                            {
                                creditCardNumber = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber;
                                cvv = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV;

                                if (!string.IsNullOrEmpty(creditCardNumber))
                                {
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = "123";

                                    if (((order.Payments[0]) as CreditPayment_V01) != null && ((order.Payments[0]) as CreditPayment_V01).Card != null)
                                    {
                                        ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                                        ((order.Payments[0]) as CreditPayment_V01).Card.CVV = "123";
                                    }
                                }
                            }
                        }

                        // TODO: how to get auth token
                        string serializedOrder = OrderProvider.SerializeOrder(theOrder, order, ShoppingCart, new Guid());
                        Session[PaymentGatewayInvoker.PaymentGateWayOrder] = serializedOrder;

                        if (IsChina)
                        {
                            if (((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments != null && ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments.Count() > 0)
                            {
                                if (!string.IsNullOrEmpty(creditCardNumber))
                                {
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = creditCardNumber;
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = cvv;

                                    if (((order.Payments[0]) as CreditPayment_V01) != null && ((order.Payments[0]) as CreditPayment_V01).Card != null)
                                    {
                                        ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = creditCardNumber;
                                        ((order.Payments[0]) as CreditPayment_V01).Card.CVV = cvv;
                                    }
                                }
                            }
                        }

                        Response.Redirect("PaymentGatewayManager.aspx");
                    }
                    else
                    {
                        PaymentRequest pghRequest = _paymentOptions.PaymentGatewayInterface.PaymentRequest;
                        pghRequest.OrderHolder = OrderProvider.GetSerializedOrderHolder(theOrder, order, ShoppingCart, new Guid()).ToSafeString();
                        Session.Add(PGHInterface.PageName, pghRequest);
                        if (_paymentOptions.PaymentGatewayInterface.CanUsePGHWindow(pghRequest.PaymentMethod))
                        {
                            var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/" + PGHInterface.PageName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", "LoadUrl('" + termUrl + "');", true);
                            btnModalPopup_ModalPopupExtender.Show();
                            return;
                        }
                        else
                        {
                            Response.Redirect("PaymentGatewayManager.aspx");
                        }
                    }
                }


                if (!string.IsNullOrEmpty(currentSession.PendingOrderId))
                {
                    OrderProvider.UpdatePaymentGatewayRecord(currentSession.PendingOrderId, "Pending Order submitting thru Pay be Phone", PaymentGatewayLogEntryType.Unknown, PaymentGatewayRecordStatusType.Declined);
                    currentSession.PendingOrderId = string.Empty;
                    SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
                }

                //var dupReferenceIdCheck = Settings.GetRequiredAppSetting("DupCheckWithReferenceId", false);
                //if (dupReferenceIdCheck)
                //{
                //    //Check For Dup Order thru Cookie
                //    var btOrder = theOrder as Providers.OrderImportBtWS.Order;

                //    if (btOrder != null)
                //    {
                //        SetDupCheckCookie(btOrder.ReferenceID);
                //    }
                //}
                string orderID = OrderProvider.ImportOrder(theOrder, out error, out _failedCards,
                                                           ShoppingCart.ShoppingCartID);
                DupeCheckDone.Value = string.Empty;
                
                #region 3D Secured Credit Card: Clean up after Order is Submitted

                SessionInfo.ThreeDSecuredCardInfo = null;

                #endregion 3D Secured Credit Card: Clean up after Order is Submitted

                if (string.IsNullOrEmpty(error) || error.Trim().ToUpper().Contains("PROCESSING") || error.Trim().ToUpper().Contains("KOUNTREVIEW"))
                {
                    try
                    {
                        if (error.Trim().ToUpper().Contains("PROCESSING"))
                        {
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedProcessing;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }
                        if (error.Trim().ToUpper().Contains("KOUNTREVIEW"))
                        {
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedInReview;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }

                        // Verify if it is a BR order in FControl status pending
                        if (HLConfigManager.Configurations.CheckoutConfiguration.FraudControlEnabled && orderID.Contains("PENDING-FCONTROL"))
                        {
                            orderID = orderID.Replace("PENDING-FCONTROL-", string.Empty);
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedPending;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }
                        // TODO: how to get auth token
                        ShoppingCartProvider.UpdateShoppingCart(ShoppingCart,
                                                                OrderProvider.SerializeOrder(theOrder, order,
                                                                                             ShoppingCart,
                                                                                             new Guid()),
                                                                orderID, order.ReceivedDate);

                        if (string.IsNullOrEmpty(order.OrderID))
                        {
                            order.OrderID = orderID;
                            order.Pricing = ShoppingCart.Totals;
                        }

                        // if is a customer order, check email address.
                        if (ShoppingCart.CustomerOrderDetail != null)
                        {
                            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail &&
                                string.IsNullOrEmpty(ShoppingCart.EmailAddress))
                            {
                                var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
                                if (!String.IsNullOrEmpty(sessionInfo.ChangedEmail))
                                {
                                    ShoppingCart.EmailAddress = sessionInfo.ChangedEmail;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(ShoppingCart.EmailAddress))
                        {
                            EmailHelper.SendEmail(ShoppingCart, order);
                        }
                        if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.HSO)
                        {
                            OrderProvider.AddHapOrders(DistributorID, countryCode, ShoppingCart, order);
                        }
                        if(ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UpdatePrimaryPickupLocationOnCheckout)
                        {
                            if(!GetShippingProvider().UpdatePrimaryPickupLocationPreference(ShoppingCart.DeliveryInfo.Id))
                            {
                                LoggerHelper.Error(string.Format("Order submitted successfully but error occurs: OrderNumber{0} Error : Cannot update primary pickup location", orderID));
                            }
                        }
                        onCreditCardAuthorizationComplete(this, new EventArgs());
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format("Order submitted successfully but error occurs: OrderNumber:{0} Error : {1}",
                                          orderID, ex.Message));
                    }
                    OrderSubmitted(order, orderID);
                }
                else
                {
                    if (error.Contains("DUPLICATE"))
                    {
                        //Your previous order was already submitted prior to the changes that were made to your cart. Your order number is <Order Number>. You can verify the status of this order by calling Order Support at 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday. Saturday 6 a.m. to 2 p.m. PST for assistance or by clicking here. (Please note that it may take up to 1 hour before the order is displayed.)
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateOrder"));
                        if (orderID != string.Empty)
                        {
                            DisplayPreviousOrderSubmitted(orderID);
                        }
                        else
                        {
                            _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail"));
                        }
                    }
                    else if (error.Contains("AUTHORIZE PAYMENT"))
                    {
                        RemoveDupCheckCookie();
                        if (error.Contains("KOUNTDECLINED"))
                            _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "KountDeclinedOrder"));
                        else
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                        if (_failedCards.Count > 0)
                        {
                            onCreditCardAuthorizationComplete(this,
                                                              new CreditCardAuthorizationFailedEventArgs(_failedCards, order.Payments.Count));
                        }
                    }
                    else if (error.Contains("TIMEOUT"))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                    }
                    else if (error.Contains("ORDER CANNOT BE FULFILLED FOR THE DISTRIBUTOR"))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
                    }
                    else if (error.Contains("DECLINED-FCONTROL"))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "FControlDeclined"));
                        if (_failedCards.Count > 0)
                        {
                            onCreditCardAuthorizationComplete(this,
                                                              new CreditCardAuthorizationFailedEventArgs(_failedCards));
                        }
                    }
                    else if (error.Contains("HAPDATAMissing"))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "MissingHapType"));
                    }
                    else
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail"));
                    }
                    blErrors.DataSource = _errors;
                    blErrors.DataBind();
                }
            }
        }

        /// <summary>
        /// Method to convert a custom Object to XML string
        /// </summary>
        /// <param name="pObject">Object that is to be serialized to XML</param>
        /// <param name="objectType">Type of Object</param>
        /// <returns>XML string</returns>
        private static string SerializeObject(Object pObject, Type objectType)
        {
            try
            {
                var ser = new XmlSerializer(objectType);
                var sb = new StringBuilder();
                var writer = new System.IO.StringWriter(sb);
                ser.Serialize(writer, pObject);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Checkout serializeObject error:  {0}", ex.Message));
                return string.Empty;
            }
        }

        private void DisplayPreviousOrderSubmitted(string orderID)
        {
            if (ShoppingCart != null)
            {
                //APFDueProvider.UpdateAPFDue(DistributorID, (this.Page as ProductsBase).APFDueDate, ShoppingCart.CartItems);
                ShoppingCart.ClearCart();
            }
        }

        private void OrderSubmitted(Order_V01 order, string orderNumber)
        {
            try
            {
                //this.OrderNumber = OrderNumber;
                var currentSession = SessionInfo;
                currentSession.OrderNumber = orderNumber;
                if (ShoppingCart.CustomerOrderDetail != null)
                {
                    SettleCustomerOrderPayment(orderNumber);
                }
                currentSession.ThreeDSecuredCardInfo = null;
                currentSession.Payments = null;
                SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
                ResetInventoryView();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Order submitted successfully but error occurs in OrderSubmitted method: OrderNumber:{0} Error : {1}",
                        orderNumber, ex.Message));
            }
            Response.Redirect("Confirm.aspx?OrderNumber=" + orderNumber);
        }

        private bool SettleCustomerOrderPayment(string orderNumber)
        {
            var customerOrderV01 =
                CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
            bool orderIDUpdate = CustomerOrderingProvider.UpdateCustomerOrderDistributorOrderID(customerOrderV01.OrderID,
                                                                                             orderNumber);
            bool orderStatusUpdate = CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID,
                                                                                     customerOrderV01.OrderStatus,
                                                                                     ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.ShippedAsDo);
            return orderIDUpdate && orderStatusUpdate;
        }

        protected void OnContinueShopping(object sender, EventArgs e)
        {
            Response.Redirect("ShoppingCart.aspx");
        }

        private bool IsPaymentGatewayOptionSelected()
        {
            return false;
        }

        private void ClearCustomerSession()
        {
            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
            currentSession.CustomerOrderNumber = null;
            currentSession.ShoppingCart = null;
            var customerAddress =
                currentSession.ShippingAddresses.Find(p => p.ID == currentSession.CustomerAddressID);
            if (customerAddress != null)
            {
                currentSession.ShippingAddresses.Remove(customerAddress);
            }
            currentSession.CustomerAddressID = 0;
            SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
        }

        protected void ReturnToCustomerOrdersSearch(object sender, EventArgs e)
        {
            ClearCustomerSession();
            Response.Redirect("~/dswebadmin/customerorders.aspx");
        }
        protected void ReturnToOrderHistory(object sender, EventArgs e)
        {

            Response.Redirect("~/Ordering/OrderListView.aspx");
        }
        protected void btnYes_Click(object sender, EventArgs e)
        {
            if (ShoppingCart.CustomerOrderDetail == null)
            {
                _errors.Clear();
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                ShoppingCart.ClearCart();
                onCreditCardAuthorizationComplete(this, new EventArgs());

                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                {
                    // Redirect to HAP Orders landing page
                    Response.Redirect("~/Ordering/HAPOrders.aspx");
                }

                Response.Redirect(HLConfigManager.Configurations.DOConfiguration.LandingPage);
            }
            else
            {
                _errors.Clear();
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                ShoppingCart.ClearCart();
                ShoppingCartProvider.DeleteOldShoppingCartForCustomerOrder(DistributorID,
                                                                           ShoppingCart.CustomerOrderDetail.
                                                                                        CustomerOrderID);

                var customerOrderV01 =
                    CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
                CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID, customerOrderV01.OrderStatus,
                                                                ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.Cancelled);

                ClearCustomerSession();
                //Redirect To Order Details Page
                Response.Redirect("~/dswebadmin/customerorderdetail.aspx?orderid=" + customerOrderV01.OrderID);
            }
        }

        protected void btnDMYes_Click(object sender, EventArgs e)
        {
            //Check For Errors
            if (!checkPayment() || !checkShipment() || !checkCasa() || !checkPickupInstructions() || !CheckPricing() || !checkHAPOptions() || !checkInvoiceOption())
            {
                blErrors.DataSource = _errors;
                blErrors.DataBind();
                dualOrderMonthPopupExtender.Hide();
                return;
            }

            ShoppingCart.InvoiceOption = _invoicOptions != null ? _invoicOptions.SelectedInvoiceOption : null;
            dualOrderMonthPopupExtender.Hide();
            DoSubmitOrder((sender as LinkButton).CommandArgument);
        }

        protected void btnDMNO_Click(object sender, EventArgs e)
        {
            Response.Redirect("ShoppingCart.aspx");
        }

        protected void OnDupeOrderOK(object sender, EventArgs e)
        {
            DupeOkClicked.Value = string.Empty;
            if (ddlConfirmSubmit.SelectedValue != "select")
            {
                DupeCheckDone.Value = DupeOkClicked.Value = ddlConfirmSubmit.SelectedValue.Equals("SubmitAnyway") ? Boolean.TrueString : Boolean.FalseString;
                dupeOrderPopupExtender.Hide();
                lblPleaseSelect.Visible = false;
                if (DupeOkClicked.Value == Boolean.TrueString)
                {
                    checkPayment(); // do this to fill _payments
                    DoSubmitOrder(string.Empty);
                }
            }
            else
            {
                dupeOrderPopupExtender.Show();
                lblPleaseSelect.Visible = true;
            }
        }

        private string GetOrderMonthString()
        {
            var currentSession = SessionInfo;
            var orderMonth = new OrderMonth(CountryCode);
            if (null != currentSession)
            {
                if (!string.IsNullOrEmpty(currentSession.OrderMonthString))
                {
                    return currentSession.OrderMonthString;
                }
                else
                {
                    return orderMonth.OrderMonthString;
                }
            }
            else
            {
                return orderMonth.OrderMonthString;
            }
        }

        private string GetOrderMonthShortString()
        {
            var currentSession = SessionInfo;
            var orderMonth = new OrderMonth(CountryCode);
            if (null != currentSession)
            {
                if (!string.IsNullOrEmpty(currentSession.OrderMonthShortString))
                {
                    return currentSession.OrderMonthShortString;
                }
                else
                {
                    return orderMonth.OrderMonthShortString;
                }
            }
            else
            {
                return orderMonth.OrderMonthShortString;
            }
        }

        private static bool HasZeroPriceEventTickets(MyHLShoppingCart shoppingCart)
        {
            bool result = shoppingCart != null && shoppingCart.OrderCategory == OrderCategoryType.ETO &&
                          HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket &&
                          shoppingCart.ShoppingCartItems != null && shoppingCart.ShoppingCartItems.Any() && shoppingCart.ShoppingCartItems.Sum(i => i.CatalogItem.ListPrice) == 0;

            return result;
        }

        protected void OnOrderStatusOK(object sender, EventArgs e)
        {
            dupeOrderPopupExtender.Hide();
            var pendingOrderNumber = ViewState["pendingOrderNumber"];
            var checkOrderSubmitted = ViewState["isOrderSubmitted"];
            if (checkOrderSubmitted != null && (bool)checkOrderSubmitted)
            {
                RedirectPendingOrder(pendingOrderNumber.ToString(), true);
            }
        }

        private void _oneTimePinControl_ValidatePinEvent(object sender, ValidatePinEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.MobilePin))
            {
                OnSubmit(null, null);
            }
        }
        private void FinalTWSKUQuantityCheck(Order_V01 order)
        {
            try
            {
                if (Locale != null && Locale == "zh-TW")
                {
                    SKUQuantityRestrictRequest_V01 req = new SKUQuantityRestrictRequest_V01();
                    req.CountryCode = order.CountryOfProcessing;
                    req.Flag = "R";
                    var list = OrderProvider.GetOrUpdateSKUsMaxQuantity(req);
                    if (list != null && list.Count > 0 && order != null && order.OrderItems != null && order.OrderItems.Count > 0)
                    {
                        var results = (from sku in list
                                       join orderingqntity in order.OrderItems
                                       on sku.SKU equals orderingqntity.SKU
                                       where orderingqntity.Quantity > 0 && sku.MaxQuantity <= 0
                                       select sku.SKU).ToList<string>();
                        if (results.Count() > 0)
                        {
                            _errors.Add(string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "OutOfInventory"), "SKU"));
                            blErrors.DataSource = _errors;
                            blErrors.DataBind();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(string.Format("There is an Error in FinalTWSKUQuantityCheck :{0}, Stack Trace{1}", order.CountryOfProcessing, ex.StackTrace), "Error");
            }
        }
    }
}
