using System;
using System.Globalization;
using System.Threading;
using System.Web.Security;
using System.Web.UI;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.ViewModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class PaymentGatewayManager : Page
    {
        private SessionInfo _sessionInfo;
        private string _country = string.Empty;
        private string _orderNumber = string.Empty;
        private bool offLinePost;
        private string _customResponse;
        private string DistributorID = string.Empty;
        private string standAloneDonation = "STANDALONEDONATION";
        private string standAloneDonationerror = "ERROR";

        protected void Page_Load(object sender, EventArgs e)
        {
            string Locale = CultureInfo.CurrentCulture.Name;
            _country = Locale.Substring(3, 2);

            MembershipUser<DistributorProfileModel> member = null;

            var memberDefault = Membership.GetUser();
            string DistributorID = string.Empty;
            try
            {
                member = (MembershipUser<DistributorProfileModel>)memberDefault;
                DistributorID = (member != null && member.Value != null) ? member.Value.Id : string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("PaymentGatewayManager Null member cast failed : {0} ", ex.Message));
            }

            if (!string.IsNullOrEmpty(DistributorID))
            {
                _sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            }
            
            if (!IsPostBack)
            {
                ProductsBase.RemoveDupCheckCookie();
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                PaymentGatewayResponse gatewayResponse = null;
                try
                {
                    gatewayResponse = PaymentGatewayResponse.Create();
                    if (null != gatewayResponse)
                    {
                        LoggerHelper.Warn(string.Format("PaymentGatewayRequest Splunk Log : Request {0}", LogFormData(Request.Form)));
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("PaymentGatewayManager failed to create a Response: {0}", ex.Message));
                }

                if (gatewayResponse == null)
                {
                    MyHLShoppingCart ShoppingCart = (null != this._sessionInfo && null != this._sessionInfo.ShoppingCart) ? this._sessionInfo.ShoppingCart : ShoppingCartProvider.GetShoppingCart(DistributorID, Locale);
                    if (ShoppingCart != null)
                    {
                        try
                        {
                            PaymentGatewayInvoker invoker = PaymentGatewayInvoker.Create(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayPaymentMethods, (ShoppingCart.Totals as OrderTotals_V01).AmountDue);
                            invoker.Submit();
                        }
                        catch (ThreadAbortException)
                        {
                            //this just seems to come along with the redirect request. We don't want this in the logs
                        }
                        catch (Exception ex)
                        {
                            PaymentGatewayInvoker.LogBlindError(string.Format("Error occurred Invoking Payment Gateway {0}. The error is: {1}", HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayInvoker, ex.Message));
                            if (HLConfigManager.Configurations.DOConfiguration.IsChina && _sessionInfo.StandAloneDonationError == standAloneDonation)
                            {
                                _sessionInfo.StandAloneDonationError = standAloneDonationerror;
                                Response.Redirect("~/Ordering/Donation.aspx");
                            }
                            else
                                Response.Redirect("~/Ordering/Checkout.aspx");
                        }
                    }
                }
                else
                {
                    if (gatewayResponse is PGHPaymentGatewayResponse && _sessionInfo != null)
                    {
                        Session[PaymentGatewayResponse.PGH_FPX_PaymentStatus] =
                            ((PGHPaymentGatewayResponse)gatewayResponse).PhgPaymentStatus;
                    }

                    if (gatewayResponse.IsReturning)
                    {
                        bool pending = gatewayResponse.IsPendingTransaction;
                        bool cancelled = gatewayResponse.IsCancelled;
                        PaymentGatewayRecordStatusType gatewayResponseStatus = gatewayResponse.Status;
                        if (gatewayResponse.IsApproved)
                        {
                            if (!gatewayResponse.CanSubmitIfApproved && _sessionInfo != null)
                            {
                                _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                                _sessionInfo.OrderNumber = gatewayResponse.OrderNumber;
                                if (_sessionInfo.ShoppingCart != null)
                                {
                                    ShoppingCartProvider.UpdateShoppingCart(_sessionInfo.ShoppingCart,
                                                                string.Empty,
                                                                gatewayResponse.OrderNumber, DateTime.Now);
                                }
                                return;
                            }
                        }

                        if (gatewayResponseStatus == PaymentGatewayRecordStatusType.OrderSubmitted && _sessionInfo != null)
                        {
                            _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                            _sessionInfo.OrderNumber = gatewayResponse.OrderNumber;
                            ShoppingCartProvider.UpdateShoppingCart(_sessionInfo.ShoppingCart,
                                                                string.Empty,
                                                                gatewayResponse.OrderNumber, DateTime.Now);
                            return;
                        }
                        else if (pending && _sessionInfo != null)
                        {
                            _sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;
                            if (_sessionInfo.ShoppingCart != null)
                            {
                                _sessionInfo.ShoppingCart.CloseCart();
                            }
                            Response.Redirect("~/Ordering/Catalog.aspx?ETO=FALSE");
                            return;
                        }
                        else if (cancelled && _sessionInfo != null)
                        {
                            _sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;
                            Response.Redirect("~/Ordering/Checkout.aspx");
                            return;
                        }
                        else if (gatewayResponse.IsApproved)
                        {
                            //Let it fall through the natural flow...
                        }
                        else if (gatewayResponseStatus == PaymentGatewayRecordStatusType.Declined)
                        {
                            Session[PaymentGatewayResponse.PaymentGateWateSessionKey] = gatewayResponse;
                            if (HLConfigManager.Configurations.DOConfiguration.IsChina && _sessionInfo.StandAloneDonationError == standAloneDonation)
                            {
                                _sessionInfo.StandAloneDonationError = standAloneDonationerror;
                                Response.Redirect("~/Ordering/Donation.aspx");
                            }
                            else
                            {
                                if (_sessionInfo != null)
                                {
                                    _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitFailed;
                                    _sessionInfo.OrderNumber = gatewayResponse.OrderNumber;
                                }
                                Response.Redirect("~/Ordering/Checkout.aspx");
                            }

                            return;
                        }
                        else
                        {
                            if (_sessionInfo != null)
                            {
                                _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitFailed;
                                return;
                            }
                        }
                    }

                    //For PGH-Submitted orders
                    if (gatewayResponse is PGHPaymentGatewayResponse)
                    {
                        if (gatewayResponse.Status == PaymentGatewayRecordStatusType.OrderSubmitted && _sessionInfo != null)
                        {
                           // Timer1.Enabled = false;
                            _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                            _sessionInfo.OrderNumber = gatewayResponse.OrderNumber;
                            return;
                        }
                    }

                    if (member == null || member.Value == null) //Being posted to by a gateway
                    {
                        offLinePost = true;
                        Timer1.Enabled = false;
                        _customResponse = gatewayResponse.SpecialResponse;
                        bool approved = gatewayResponse.IsApproved;
                        if (approved)
                        {
                            if (gatewayResponse.CanSubmitIfApproved)
                            {
                                SubmitOrder(gatewayResponse);
                            }

                            return;
                        }
                    }

                    string orderNumber = gatewayResponse.OrderNumber;
                    if (null != _sessionInfo)
                    {
                        if (_sessionInfo.OrderStatus == SubmitOrderStatus.OrderSubmitted && orderNumber != _sessionInfo.OrderNumber)
                        {
                            _sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;
                        }
                        if (_sessionInfo.OrderStatus == SubmitOrderStatus.Unknown)
                        {
                            if (gatewayResponse.IsApproved && gatewayResponse.CanSubmitIfApproved)
                            {
                                _sessionInfo.OrderStatus = SubmitOrderStatus.OrderBeingSubmitted;
                                if (string.IsNullOrEmpty(_sessionInfo.OrderMonthShortString))
                                {
                                    var orderMonth = new OrderMonth(_country);
                                    //orderMonth.ResolveOrderMonth();
                                    _sessionInfo.OrderMonthShortString = orderMonth.OrderMonthShortString;
                                    _sessionInfo.OrderMonthString = orderMonth.OrderMonthString;
                                }
                                if (null == _sessionInfo.ShoppingCart)
                                {
                                    _sessionInfo.ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale);
                                }

                                (new AsyncSubmitOrderProvider()).AsyncSubmitOrder(gatewayResponse, _country, _sessionInfo);
                            }
                            else
                            {
                                if (gatewayResponse.Status == PaymentGatewayRecordStatusType.ApprovalPending && HLConfigManager.Configurations.PaymentsConfiguration.CanSubmitPending)
                                {
                                    if (null != gatewayResponse as PGHPaymentGatewayResponse)
                                    {
                                        if ((gatewayResponse as PGHPaymentGatewayResponse).OrderStatus == HL.PGH.Api.OrderStatus.GatewayWillSubmit)
                                {
                                            _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedProcessing;
                                            Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" + orderNumber);
                                            return;
                                        }
                                    }
                                    _sessionInfo.OrderStatus = SubmitOrderStatus.OrderBeingSubmitted;
                                    if (string.IsNullOrEmpty(_sessionInfo.OrderMonthShortString))
                                    {
                                        var orderMonth = new OrderMonth(_country);
                                        _sessionInfo.OrderMonthShortString = orderMonth.OrderMonthShortString;
                                        _sessionInfo.OrderMonthString = orderMonth.OrderMonthString;
                                    }
                                    if (null == _sessionInfo.ShoppingCart)
                                    {
                                        _sessionInfo.ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale);
                                    }
                                    (new AsyncSubmitOrderProvider()).AsyncSubmitOrder(gatewayResponse, _country, _sessionInfo);
                                }
                                else
                                {
                                    Session[PaymentGatewayResponse.PaymentGateWateSessionKey] = gatewayResponse;
                                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && _sessionInfo.StandAloneDonationError == standAloneDonation)
                                    {
                                        _sessionInfo.StandAloneDonationError = standAloneDonationerror;
                                        Response.Redirect("~/Ordering/Donation.aspx");
                                    }
                                    else
                                        Response.Redirect("~/Ordering/Checkout.aspx");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        lbSubmitOrderStatus.Text = string.Format((string)GetLocalResourceObject("PaymentGatewayOrderSubmitFailed"), orderNumber);
                        if (gatewayResponse != null && !string.IsNullOrEmpty(orderNumber))
                        {
                            var status = OrderProvider.GetPaymentGatewayRecordStatus(orderNumber);
                            if (status == PaymentGatewayRecordStatusType.Approved || status == PaymentGatewayRecordStatusType.OrderSubmitted)
                            {
                                SettleCustomerOrderPayment(orderNumber);
                                Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" + orderNumber);
                            }
                        }                           
                        lbSubmitOrderStatus.Style.Add(HtmlTextWriterStyle.Color, "Red");
                        Timer1.Enabled = false;
                    }
                }
            }
        }

        public bool CheckOrderStatus()
        {
            bool result = false;
            if (null != _sessionInfo)
            {
                if (_sessionInfo.OrderStatus != SubmitOrderStatus.OrderBeingSubmitted)
                {
                    result = true;
                }
            }
            else
            {
                if (!offLinePost)
                {
                    lbSubmitOrderStatus.Text = string.Format((string)GetLocalResourceObject("PaymentGatewayOrderSubmitFailed"), string.Empty);
                    lbSubmitOrderStatus.Style.Add(HtmlTextWriterStyle.Color, "Red");
                }
                Timer1.Enabled = result = false;
            }

            return result;
        }

        protected void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CheckOrderStatus())
                {
                    switch (_sessionInfo.OrderStatus)
                    {
                        case SubmitOrderStatus.OrderBeingSubmitted:
                        {
                            //Still going...
                            break;
                        }
                        case SubmitOrderStatus.OrderSubmitted:
                        {
                            if (!string.IsNullOrEmpty(_sessionInfo.OrderNumber))
                            {
                                try
                                {
                                    lbSubmitOrderStatus.Text = "Order Submitted";
                                    Timer1.Enabled = false;

                                    OrderSubmitted();
                                }
                                catch (Exception ex)
                                {
                                    if (!ex.Message.Contains("Thread"))
                                    {
                                        LoggerHelper.Error(string.Format("PaymentGatewayManager Timer_Tick fails, OrderNumber: {0}; Error Message: {1} ", _sessionInfo.OrderNumber, ex.Message));
                                    }
                                }
                            }
                            break;
                        }
                        case SubmitOrderStatus.OrderSubmitFailed:
                            {
                                lbSubmitOrderStatus.Text =
                                        string.Format((string)GetLocalResourceObject("PaymentGatewayOrderSubmitFailed"),
                                                    _sessionInfo.OrderNumber);
                                lbSubmitOrderStatus.Style.Add(HtmlTextWriterStyle.Color, "Red");
                                Timer1.Enabled = false;
                                if (!string.IsNullOrEmpty(_sessionInfo.OrderNumber))
                                {
                                    var status = OrderProvider.GetPaymentGatewayRecordStatus(_sessionInfo.OrderNumber);
                                    if (status == PaymentGatewayRecordStatusType.Approved || status == PaymentGatewayRecordStatusType.OrderSubmitted)
                                    {
                                        OrderSubmitted();
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error(string.Format("PaymentGatewayManager Timer_Tick fails : {0} ", ex.Message));
            }
        }

        private void SubmitOrder(PaymentGatewayResponse response)
        {
            LoggerHelper.Info(string.Format("SubmitOrder. OrderNumber - {0}", response.OrderNumber));
            string distributorId = string.Empty;
            string locale = string.Empty;
            string error = string.Empty;
            var holder = new SerializedOrderHolder();

            if (OrderProvider.deSerializeAndSubmitOrder(response, out error, out holder))
            {
                if (holder != null && !String.IsNullOrEmpty(holder.Email))
                {
                    try
                    {
                        MyHLShoppingCart shoppingCart = null;
                        if (response.ReloadShoppingCart)
                        {
                            shoppingCart = ShoppingCartProvider.GetBasicShoppingCartFromService(holder.ShoppingCartId, holder.DistributorId, locale);
                        }
                        else
                        {
                            shoppingCart = ShoppingCartProvider.GetShoppingCart(holder.DistributorId, holder.Locale, true);
                        }
                        //ExceptionFix: Validating shoppingCart is not null
                        if (shoppingCart != null)
                        {
                            shoppingCart.EmailAddress = holder.Email;
                            if (holder.Order != null)
                            {
                                shoppingCart.Totals = (holder.Order as Order_V01).Pricing as OrderTotals_V01;
                                if (shoppingCart.ShoppingCartID == holder.ShoppingCartId)
                                {
                                    EmailHelper.SendEmail(shoppingCart, holder.Order as Order_V01);
                                }
                            }
                            CleanupOrder(shoppingCart, shoppingCart.DistributorID);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(string.Format("PaymentGatewayManager SubmitOrder-SendEmail fails : {0} ", ex.Message));
                    }
                }
            }
            else
            {
                LoggerHelper.Info(string.Format("SubmitOrder. deSerializeAndSubmitOrder could not deSerializeAndSubmitOrder. OrderNumber - {0}", response.OrderNumber));
                LoggerHelper.Error(error);
                if (error.Contains("DUPLICATE"))
                {
                }
                else if (error.Contains("TIMEOUT"))
                {
                    lbSubmitOrderStatus.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit");
                }
                else if (error.Contains("ORDER CANNOT BE FULFILLED FOR THE DISTRIBUTOR"))
                {
                    lbSubmitOrderStatus.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder");
                }
                else
                {
                    lbSubmitOrderStatus.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail");
                }
            }
        }

        private void OrderSubmitted()
        {
            SettleCustomerOrderPayment(_sessionInfo.OrderNumber);

            Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" + _sessionInfo.OrderNumber);
        }

        private void SettleCustomerOrderPayment(string orderNumber)
        {
            var locale = CultureInfo.CurrentCulture.Name;
            
            var shoppingCart = this._sessionInfo.ShoppingCart ?? ShoppingCartProvider.GetShoppingCart(DistributorID, locale);
            if (shoppingCart != null && shoppingCart.CustomerOrderDetail != null)
            {
                var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(shoppingCart.CustomerOrderDetail.CustomerOrderID);
                CustomerOrderingProvider.UpdateCustomerOrderDistributorOrderID(customerOrderV01.OrderID, orderNumber);
                CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID, customerOrderV01.OrderStatus, ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.ShippedAsDo);
            }
        }

        private void CleanupOrder(MyHLShoppingCart cart, string distributorId)
        {
            if (!IsPostBack)
            {
                if (PurchasingLimitProvider.RequirePurchasingLimits(distributorId, _country))
                {
                    PurchasingLimitProvider.ReconcileAfterPurchase(cart, distributorId, _country);
                }

                if (cart != null)
                {
                    // take out quantities from inventory
                    ShoppingCartProvider.UpdateInventory(cart, cart.CountryCode, cart.Locale, true);
                }

                cart.CloseCart();
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(_customResponse))
            {
                base.Render(writer);
            }
            else
            {
                writer.Write(_customResponse);
            }
        }

        protected string LogFormData(System.Collections.Specialized.NameValueCollection data)
        {
            string result = string.Concat(System.Web.HttpContext.Current.Request.RawUrl + Environment.NewLine);
            foreach (string key in data.AllKeys)
            {
                result += string.Concat(Environment.NewLine, key, "; ", data[key]);
            }

            return (result.Length > 10000) ? result.Substring(0, 10000) : result;
        }
    }
}