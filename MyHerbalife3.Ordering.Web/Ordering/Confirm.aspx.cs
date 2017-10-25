using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using HL.Common.DataContract.Interfaces;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.Services;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class Confirm : ProductsBase
    {
     
        private CheckoutOrderSummary _checkoutOrderSummary;
        private CheckoutTotalsDetailed _checkoutTotalsDetailed;
        private InvoiceOptions _invoicOptions;
        //CheckOutOptions _checkoutOptions = null;
        private PaymentsSummary _paymentsSummary;
        public bool DisplayInvoiceOptions { get; set; }
        List<MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket> TicketDetails = new List<MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket>();

        #region Bussed Events

        [SubscribesTo("HFFOrderPlaced")]
        public void HFFOrderTypePlaced(object sender, EventArgs e)
        {
        }

        [SubscribesTo("HFFOrderCreated")]
        public void HFFOrderCreated(object sender, EventArgs e)
        {
        }

        [SubscribesTo("HFFOrderCancel")]
        public void HFFOrderCancel(object sender, EventArgs e)
        {
            mdlDonate.Hide();
        }

        #endregion

        #region AdobeTarget_Salesforce
        public decimal totalOrder = 0.0M;
        public string orderNumber = string.Empty;
        public string skuList = string.Empty;
        public string cartItems = string.Empty;
        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayWireReferenceNumber)
            {
                if (_paymentsSummary != null)
                {
                    if (_paymentsSummary.CurrentPaymentInfo != null)
                    {
                        if (_paymentsSummary.CurrentPaymentInfo.Count == 1)
                        {
                            var wirePayment = _paymentsSummary.CurrentPaymentInfo[0] as WirePayment_V01;
                            if (wirePayment != null)
                            {
                                trWire.Visible = true;
                                lblWireRefNum.Text = DistributorOrderingProfile.ReferenceNumber; 
                            }
                        }
                    }
                }
            }
            if (HLConfigManager.Configurations.DOConfiguration.ShowBulletinBoard)
            {
                var orderingMaster = Master as OrderingMaster;
                if (orderingMaster != null) orderingMaster.SetBulletinBoardVisibility(false);
            }
        }

        private string getOrderNumber(string orderNumber, string countryCode)
        {
            if (!string.IsNullOrEmpty(Settings.GetRequiredAppSetting("OrderNumberPrefix")) && countryCode == "CN")
            {
                return Settings.GetRequiredAppSetting("OrderNumberPrefix") + '-' + orderNumber;
            }
            return orderNumber;
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var currentSession = SessionInfo;
            if (string.IsNullOrEmpty(SessionInfo.ShoppingCart.EmailAddress))
                SessionInfo.ShoppingCart.EmailAddress = SessionInfo.ChangedEmail;
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
                    LoggerHelper.Error(string.Format("Error in Confirm.aspx.cs : OnInit  {0}", ex.Message));
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                hyperLinkInvoice.Visible = false;
               
                if (ShoppingCart !=null && !string.IsNullOrWhiteSpace(OrderProvider.GetForeignPPVCountryCode(ShoppingCart.DistributorID,ShoppingCart.CountryCode)))
                {

                    if (ShoppingCart.CartItems.Count > 0 &&
                        ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.RSO)
                    {
                        if (OrderProvider.IsEligibleReceiptModelSkus(ShoppingCart).Any())
                        {
                            ForeignPPVPopupExtender.Show();
                            var receiptModel = OrderProvider.GetReceiptModel(ShoppingCart);
                            HttpContext.Current.Session["ReceiptModel"] = receiptModel;
                        }
                    }
                }

                //if (HLConfigManager.Configurations.DOConfiguration.InvoiceInOrderConfrimation &&
                //    (null != ShoppingCart && ShoppingCart.OrderCategory != ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
                //{
                //    hyperLinkInvoice.Visible = true;
                //}
                //else
                //{
                //    hyperLinkInvoice.Visible = false;
                //}
                //if (!(OrderProvider.IsEligibleReceiptModelSkus(ShoppingCart).Count() > 0))
                //{
                //    hyperLinkInvoice.Visible = false;
                //}
                if (HLConfigManager.Configurations.DOConfiguration.InvoiceInOrderConfrimation && ShoppingCart != null && string.IsNullOrWhiteSpace(OrderProvider.GetForeignPPVCountryCode(ShoppingCart.DistributorID, ShoppingCart.CountryCode)) &&
                                             OrderProvider.IsEligibleReceiptModelSkus(ShoppingCart).Any() &&
                                             ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.RSO)
                {
                    hyperLinkInvoice.Visible = true;
                }
                    ClearShoppingCartModuleCache();
                if (IsChina  )
                {
                    var ordNumQS = string.Empty;
                    if (ShouldLoadOrderFromSerializedData)
                    {
                          ordNumQS = Request.QueryString["OrderNumber"];
                    }else
                    {
                          ordNumQS = Request.QueryString["OrderId"];
                    }
                    if (!string.IsNullOrEmpty(ordNumQS))
                    {
                        SessionLessInfoManager.Instance.LoadOrderAcknowledgement(SessionInfo, ordNumQS, this.Locale, this.DistributorID);
                    }
                   
                }

                if (!IsPostBack && ShoppingCart != null && ShoppingCart.Totals == null)
                {
                    SessionInfo.UseHMSCalc = HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc;
                    ShoppingCart.Calculate();
                }
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderProducts").ToString());
                ContinueShopping.Text = GetLocalResourceObject("ProductCatalogReturn").ToString();
                if (null != ShoppingCart && !string.IsNullOrEmpty(ShoppingCart.EmailAddress))
                {
                    PrimaryEmail = ShoppingCart.EmailAddress;
                }
                if (null != ShoppingCart && ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderEvents").ToString());
                    hdnOrderType.Value = "ETO";
                    ContinueShopping.Text = GetLocalResourceObject("EventTicketsReturn").ToString();
                    lnkCreateInvoice.Visible = false;
                }

                //SessionInfo currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);

                if (SessionInfo != null)
                {
                    var ordernumber = string.Empty;
                    if (!String.IsNullOrEmpty(SessionInfo.OrderNumber))
                    {
                          ordernumber = SessionInfo.OrderNumber;
                    }
                    else if(SessionInfo.SessionLessInfo!=null)
                    {
                        ordernumber = SessionInfo.SessionLessInfo.OrderNumber;
                    }

                    if (!string.IsNullOrEmpty(ordernumber))
                    {
                        if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayTransactionTime)
                        {
                            lblTransactiontime.Text = DateUtils.GetCurrentLocalTime(CountryCode).ToString();
                        }
                        lblOrderNumber.Text = getOrderNumber(ordernumber, CountryCode);
                        OrderNumber = lblOrderNumber.Text;
                        if (ShoppingCart.CustomerOrderDetail != null)
                        {
                            ContinueShopping.Text = GetLocalResourceObject("ReturnToCustomerOrders").ToString();
                            hdnOrderType.Value = "CustomerOrder";
                            var CountryCO2DO = Settings.GetRequiredAppSetting("CountryCO2DO", string.Empty).Split(',');
                            if (CountryCO2DO.Contains(ShoppingCart.CountryCode))
                                DisplayInvoiceOptions = false;
                        }
                        else
                        {
                            DisplayInvoiceOptions = true;
                        }
                    }
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.FraudControlEnabled && SessionInfo.OrderStatus == SubmitOrderStatus.OrderSubmittedPending)
                {
                    trFControl.Visible = true;
                }

                if (SessionInfo.OrderStatus == SubmitOrderStatus.OrderSubmittedInReview)
                {
                    trFControl.Visible = true;
                    lblFControlMessage.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "KountReviewOrder");
                }

                if (ShoppingCart.HoldCheckoutOrder
                     && ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                {
                    trMPCFraud.Visible = true;
                }

                if (SessionInfo.OrderStatus == SubmitOrderStatus.OrderSubmittedProcessing)
                {
                    trPendingOrder.Visible = true;
                    if (HLConfigManager.Configurations.PaymentsConfiguration.PendingOrderhascontent)
                    {
                        PendingNotification.Visible = true;
                        lblPendingOrder.Visible = false;
                    }
                    else
                    {
                        PendingNotification.Visible = false;
                        lblPendingOrder.Visible = true;
                    }
                    trPendingOrderLink.Visible = true;
                    trConfirm.Visible = false;
                }
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    //if (SessionInfo.OrderStatus == SubmitOrderStatus.OrderSubmitted)
                    //{
                        bool QRCodeDisable = Settings.GetRequiredAppSetting("QRCodeDisable", true);
                        if (!QRCodeDisable)
                        {
                        var cartitems = SessionInfo.ShoppingCart.CartItems;
                        if (cartitems.Count == 0||string.IsNullOrEmpty(SessionInfo.OrderNumber))
                        {
                            cartitems = SessionInfo.SessionLessInfo.ShoppingCart.CartItems;
                        }
                            foreach (var sku in cartitems)
                            {
                                var skU = Providers.CatalogProvider.GetCatalogItem(sku.SKU, "CN");
                                if (skU.ProductCategory == "ETO")
                                {
                                    var Tickets = new MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket
                                    {

                                        quantity = sku.Quantity,
                                        ticketSKU = sku.SKU,
                                    };
                                    TicketDetails.Add(Tickets);

                                }
                                else
                                {
                                    btnQrCodeDownload.Visible = false;
                                }
                            }
                                if (TicketDetails.Count > 0)
                                {

                                    string QRCodeUrl = MyHerbalife3.Ordering.Providers.China.OrderProvider.DownloadQRCode(DistributorID, SessionInfo.OrderNumber, DateTime.Now.ToString(), Locale, TicketDetails);

                                    if (!string.IsNullOrEmpty(QRCodeUrl))
                                    {
                                        ViewState["QrCode"] = QRCodeUrl;
                                    }
                                    else
                                    {
                                        btnQrCodeDownload.Visible = false;
                                    }
                            }
                            else
                            {
                                btnQrCodeDownload.Visible = false;
                            }
                        }
                        else
                        {
                         btnQrCodeDownload.Visible = false;
                        }

                    }
                    else
                    {
                    btnQrCodeDownload.Visible = false;
                    }
                //}
                //else
                //{
                //  btnQrCodeDownload.Visible = false;
                //}

                LoadControls();
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && !String.IsNullOrEmpty(SessionInfo.OrderNumber))
                {
                    var response = Providers.China.OrderProvider.GetPBPPaymentServiceDetail(DistributorID, SessionInfo.OrderNumber);
                    if (response != null && response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
                    {
                        if (response.PaymentStatus == "CNPending")
                        {
                            lblPBPMessageTitle.Text = GetLocalResourceObject("lblPBPMessageTitle").ToString();
                            lblPBPOrderMessage.Text = string.Format(GetLocalResourceObject("PBPOrderSubmittedResource").ToString(), SessionInfo.OrderNumber);
                            PBPDynamicButtonYes.Text = GetLocalResourceString("OK.Text");
                            PBPOrderPopupExtender.Show();
                        }
                       
                    }
                  
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error in Confirm.aspx.cs : Page_Load {0} \n StackTrace {1}", ex.Message, ex.StackTrace));
                closeCart();
            }
            if (string.IsNullOrEmpty(lblOrderNumber.Text))
            {
                OrderNumber =
                    lblOrderNumber.Text =
                    string.IsNullOrEmpty(Request.QueryString["OrderNumber"])
                        ? string.Empty
                        : Request.QueryString["OrderNumber"];
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayTellYourStory)
            {
                (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-confirm-tellstory");
            }
            else
            {
                (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-confirm");
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayTellYourStory)
            {
                (Master as OrderingMaster).gdoNavRightCSS("gdo-nav-right col-sm-3 confirm");
            }

            var invoiceEnabledLocales = Settings.GetRequiredAppSetting("InvoiceEnabledLocales", string.Empty);

            if (!string.IsNullOrEmpty(invoiceEnabledLocales) && invoiceEnabledLocales.Contains(Locale))
            {
                dvCreateInvoice.Visible = true;
            }

            if (HLConfigManager.Configurations.PaymentsConfiguration.SendConfirmationEmail)
            {
                try
                {
                    var cmmSVCP = new CommunicationSvcProvider();
                    cmmSVCP.SendEmailConfirmation(OrderNumber, "OrderSubmitted");
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Error sending confirmation email, order:{0} : {1}", OrderNumber, ex.Message));
                }
            }       
              
            // HAP Updates
            if (HLConfigManager.Configurations.DOConfiguration.AllowDO && SessionInfo.IsHAPMode)
            {
                trSalesConfirm.Visible = false;
                lblSuccess.Text = GetLocalResourceObject("lblHapOrderPlacedSuccessResource.Text").ToString();
                lblOrderStatus.Text = GetLocalResourceObject("lblOrderStatusResource2.Text").ToString();
                dvCreateInvoice.Visible = false;
                dvHapOrderPage.Visible = true;
                (Master as OrderingMaster).SetRightPanelStyle("display", "none");
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
            {
                (Master as OrderingMaster).divLeftVisibility = true;
            }
            else
            {
                (Master as OrderingMaster).divLeftVisibility = false;
            }

            if(!IsPostBack && HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations && SessionInfo != null && 
                SessionInfo.ShoppingCart != null && ShoppingCart != null && ShoppingCart.Totals != null)
            {
                // Adobe Target - Confirmation Page
                OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;
                totalOrder = totals.ItemsTotal;
                orderNumber = lblOrderNumber.Text;
                skuList = AdobeTarget_ConvertCartItemsToString(SessionInfo.ShoppingCart.ShoppingCartItems);

                // Salesforce - Confirmation Page
                cartItems = Salesforce_ConvertCartItemsToString(SessionInfo.ShoppingCart.ShoppingCartItems);
            }

        }
        protected void OnCreateRecipt(object sender, EventArgs e)
        {
            OrderProvider.CreateReceipt();
        }

        [WebMethod]
        public static void RemoveLegacyReceiptSession()
        {
            HttpContext.Current.Session.Remove("ReceiptModel");
        }
        private void ClearShoppingCartModuleCache()
        {
            try
            {
                if (SessionInfo != null)
                    SessionInfo.IsFirstTimeSpainPopup = false;
                var cartWidgetSource = new CartWidgetSource();
                cartWidgetSource.ExpireShoppingCartCache(DistributorID, Locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                       string.Format("Error occurred ClearMyHL3ShoppingCartCache. Id is {0}-{1}.\r\n{2}", DistributorID, Locale,
                                     ex.Message));
            }
        }

        private void LoadControls()
        {
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsMiniControl))
            {
                _checkoutTotalsDetailed =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsDetailedControl) as
                    CheckoutTotalsDetailed;
                plCheckOutTotalsDetails.Controls.Add(_checkoutTotalsDetailed);
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary))
            {
                _checkoutOrderSummary =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary) as
                    CheckoutOrderSummary;
                _checkoutOrderSummary.DisplayReadOnlyGrid = true;
                _checkoutOrderSummary.FlattenProductDetails = true;
                _checkoutOrderSummary.OrderSummaryText = GetLocalResourceObject("OrderSummaryStaticText").ToString();
                _checkoutOrderSummary.OmnitureState = "purchase|" + OrderNumber;
                plCheckOutOrderDetails.Controls.Add(_checkoutOrderSummary);
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl))
            {
                var _checkoutOptionsControl =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl);

                var _checkOutOptions = _checkoutOptionsControl as CheckOutOptions;
                _checkOutOptions.IsStatic = (Page as ProductsBase).CheckoutOptionsAreStatic;
                _checkOutOptions.DisplayHoursOfOperationForPickup = true;
                var _Message = (Label)_checkOutOptions.FindControl("lblMessage");
                var _Notifiaction = (Label)_checkOutOptions.FindControl("lblMessageNotifyReadOnly");
                if (_Message != null && _Notifiaction != null)
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
                    _invoicOptions.IsReadOnly = true;
                    plInvoiceOptions.Controls.Add(_invoicOptions);
                }
            }
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl))
            {
                _paymentsSummary =
                    LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl) as
                    PaymentsSummary;
                plPaymentOptions.Controls.Add(_paymentsSummary);
            }
            PolicyMessage.Visible = HLConfigManager.Platform == "PCAD";
            if (
                HLConfigManager.Configurations.DOConfiguration
                               .AllowHFFModal)
            {
                var _hFFModal = LoadControl("~/Ordering/Controls/HFFModal.ascx") as HFFModal;
                plHFFModal.Controls.Add(_hFFModal);
                ((OrderingMaster)Page.Master).EventBus.RegisterObject(_hFFModal);
            }
            // Display the HFF message if the item is in the cart
            if (HLConfigManager.Configurations.DOConfiguration.AllowHFF &&
                HLConfigManager.Configurations.DOConfiguration.ShowHFFMessage &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
            {
                if (
                    ShoppingCart.CartItems.Any(
                        i => i.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    HFFMessage.Visible = true;
                }
            }
            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl))
            {
                var _checkoutHAPOptions =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl);
                plCheckOutHAPOptions.Controls.Add(_checkoutHAPOptions);
            }
        }

        protected override void OnInitComplete(EventArgs e)
        {
            base.OnInitComplete(e);
            if (
                HLConfigManager.Configurations.DOConfiguration
                               .AllowHFFModal)
            {
                ShowHFFModal();
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (!IsPostBack)
            {
                try
                {
                    if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
                    {
                        PurchasingLimitProvider.ReconcileAfterPurchase(ShoppingCart, DistributorID, CountryCode);
                    }

                    var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
                    if (currentSession != null)
                    {
                        if (!String.IsNullOrEmpty(currentSession.OrderNumber))
                        {
                            ShoppingCart.OrderNumber = currentSession.OrderNumber;
                            currentSession.OrderNumber = String.Empty;
                            currentSession.OrderMonthShortString = string.Empty;
                            currentSession.OrderMonthString = string.Empty;
                            currentSession.ShippingMethodNameMX = String.Empty;
                            currentSession.ShippingMethodNameUSCA = String.Empty;
                            if (currentSession.ShoppingCart != null)
                                currentSession.ShoppingCart.CustomerOrderDetail = null;
                            currentSession.CustomerOrderNumber = String.Empty;
                            currentSession.CustomerAddressID = 0;
                            currentSession.ConfirmedAddress = false;
                            currentSession.BRPF = String.Empty;
                            currentSession.OrderStatus = SubmitOrderStatus.Unknown;
                            currentSession.HmsPricing = false;
                            currentSession.LocalPaymentId = string.Empty;
                            currentSession.TrackingUrl = string.Empty;
                            currentSession.SelectedPaymentChoice = string.Empty;
                            if (null != currentSession.ShippingAddresses)
                            {
                                var customerAddress =
                                    currentSession.ShippingAddresses.Find(p => p.ID == currentSession.CustomerAddressID);
                                if (customerAddress != null)
                                {
                                    currentSession.ShippingAddresses.Remove(customerAddress);
                                }
                            }
                            currentSession.IsHAPMode = false;
                        }
                        currentSession.IsAPFOrderFromPopUp = false;
                    }
                   //Clear the order month session...
                    Session["OrderMonthDataSessionKey"] = null;
                    ResolveAPF();
                    InsertIntoMLMOverrideTable(currentSession);
                    SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
                    RemoveDupCheckCookie();
                    closeCart();
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("Error in Confirm.aspx.cs : OnUnload error message:  {0}; \r\n Stack Info: {1}",
                                      ex.GetBaseException().Message, ex.GetBaseException().StackTrace));
                    closeCart();
                }
            }
        }

        private void closeCart()
        {
            if (ShoppingCart != null)
            {
                // take out quantities from inventory
                ShoppingCartProvider.UpdateInventory(ShoppingCart, CountryCode, Locale, true);
                // Closing cart.
                ShoppingCart.CloseCart();
            }
            // Creating a new cart.
            ShoppingCartProvider.createShoppingCart(DistributorID, Locale);
        }
        protected void OnContinueShopping(object sender, EventArgs e)
        {
            if (hdnOrderType.Value == "ETO")
            {
                hdnOrderType.Value = String.Empty;
                Response.Redirect("Catalog.aspx?ETO=TRUE");
            }
            else if (hdnOrderType.Value == "CustomerOrder")
            {
                hdnOrderType.Value = String.Empty;
                Response.Redirect("~/dswebadmin/customerorders.aspx");
            }
            else
            {
                if (IsChina)
                    Response.Redirect("PriceList.aspx");
                else
                Response.Redirect("Catalog.aspx");
            }
        }

        private void ResolveAPF()
        {
            if (ShoppingCart != null && ShoppingCart.CartItems != null &&
                APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems))
            {
                if (_paymentsSummary != null && _paymentsSummary.CurrentPaymentInfo != null && _paymentsSummary.CurrentPaymentInfo.Any())
                {
                    CreditPayment_V01 payment = _paymentsSummary.CurrentPaymentInfo[0] as CreditPayment_V01;
                    if (payment == null) return;
                    if (this.Locale == "pt-BR" && payment.AuthorizationMethod != AuthorizationMethodType.Online)
                        return;
                    DistributorOrderingProfile orderingProfile = this.DistributorOrderingProfile;
                    if (orderingProfile != null)
                    {
                        int payedApf = APFDueProvider.APFQuantityInCart(ShoppingCart);
                        var currentDueDate = orderingProfile.ApfDueDate;
                        var newDueDate = currentDueDate + new TimeSpan(payedApf * 365, 0, 0, 0);
                       
                        orderingProfile.ApfDueDate = newDueDate;
                        Session.Add("apfdue", newDueDate);
                        APFDueProvider.UpdateAPFDuePaid(DistributorID, newDueDate);
                        new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(DistributorID, CountryCode);
                    }
                   
                }
            }
        }

        private void InsertIntoMLMOverrideTable(SessionInfo oldSession)
        {
            if (HLConfigManager.Configurations.DOConfiguration.HasMLMCheck)
            {
                if (oldSession.TrainingBreached)
                {
                    int insertRecord = OrderProvider.InsertMLMOverrideRecordForDS(DistributorID, CountryCode);
                }
            }
        }

        private void ShowHFFModal()
        {
            mdlDonate.Show();
        }

        protected void OnPBPOrderOK(object sender, EventArgs e)
        {
            PBPOrderPopupExtender.Hide();
            Response.Redirect("~/Ordering/OrderListView.aspx", true);
        }

        protected void lnkCreateInvoiceClicked(object sender, EventArgs e)
        {
            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
            var url = string.Format("/Ordering/Invoice/CreateByOrderId/{0}", lblOrderNumber.Text);
            Response.Redirect(url);
        }

        private bool IsChina
        {
            get { return HLConfigManager.Configurations.DOConfiguration.IsChina; }
        }

        private bool ShouldLoadOrderFromSerializedData
        {
            get
            {
                return IsChina 
                    && !string.IsNullOrWhiteSpace(Request.QueryString["OrderNumber"]) 
                    && (Request.QueryString[ProductsBase.AttemptLoadSerializedData_Key] == ProductsBase.AttemptLoadSerializedData_Value_Yes)
                    ;
            }
        }

        protected void lnkHapOrderPage_Click(object sender, EventArgs e)
        {
            // Redirect to HAP Orders landing page
            Response.Redirect("~/Ordering/HAPOrders.aspx");
        }

        protected void btnQrCodeDownload_Click(object sender, EventArgs e)
        {
           
            if (ViewState["QrCode"] !=null)
            {
                string URL = ViewState["QrCode"].ToString();
               HttpContext.Current.Response.Redirect(URL);
            }
            
        }
    }
}