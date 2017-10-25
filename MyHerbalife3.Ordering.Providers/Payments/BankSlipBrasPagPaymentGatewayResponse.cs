using System;
using System.Web;
using System.Web.Security;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
      public class BankSlipBrasPagPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "Order";
        private const string AuthResult = "Status";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "BankSlipBrasPag";
        private const string Crypt = "CRYPT";
        private const string Authorized = "1";
        // just for testing, we will need uncomment the next line once we are in production enviroment

        // private const string Authorized = "4";  // Awaiting payment For BankSlip the inicial status is always 4 (Awaiting payment). Since the BankSlip payment is not online.

        public BankSlipBrasPagPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;     
                    if (QueryValues[GateWay] == PaymentGateWayName)
                    {
                        canProcess = true;

                        if (string.IsNullOrEmpty(QueryValues[Order]))
                        {
                            base.AuthResultMissing = true;
                        }
                        else
                        {
                            _configHelper = new ConfigHelper("BankSlipBrasPagPaymentGateway");
                            string result = QueryValues[AuthResult];
                            OrderNumber = QueryValues[Order];

                            //Task# 6818: Bankslip Deferred Payments
                            if (OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber) ==
                                PaymentGatewayRecordStatusType.OrderSubmitted)
                            {
                                SessionInfo _sessionInfo = null;
                                string _locale = HLConfigManager.Configurations.Locale;
                                var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value;
                                string DistributorID = (null != member) ? member.Id : string.Empty;
                                if (!string.IsNullOrEmpty(DistributorID))
                                {
                                    _sessionInfo = SessionInfo.GetSessionInfo(DistributorID, _locale);
                                    if (_sessionInfo != null)
                                    {
                                        _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                                        _sessionInfo.OrderNumber = OrderNumber;

                                        try
                                        {
                                            this.SettleCustomerOrderPayment(_sessionInfo.ShoppingCart, OrderNumber);
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerHelper.Error(
                                            string.Format(
                                                "BankSlipBrasPagPaymentGatewayResponse, Error in Settling Customer Order. Order Number  : {0} Exception : {1}",
                                                OrderNumber, ex.Message + ex.StackTrace));
                                        }

                                        HttpContext.Current.Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" +
                                                                              _sessionInfo.OrderNumber);
                                        HttpContext.Current.Response.End();
                                    }
                                    else
                                    {
                                        LoggerHelper.Error(
                                            string.Format(
                                                "BankSlipBrasPagPaymentGatewayResponse, Session is null. Order Number  : {0} ",
                                                OrderNumber));
                                    }
                                }
                                else
                                {
                                    LoggerHelper.Error(
                                        string.Format(
                                            "BankSlipBrasPagPaymentGatewayResponse, DistributorID is null. Order Number  : {0} ",
                                            OrderNumber));
                                }
                            }
                            string decryptedData = string.Empty;
                            IsApproved = result == Authorized;
                            if (IsApproved)
                                {
                                AuthorizationCode = QueryValues[AuthResult];
                                string id_Loja = _configHelper.GetConfigEntry("paymentGatewayIdLoja");
                                if (!string.IsNullOrEmpty(PostedValues[Crypt]))
                                {
                                    decryptedData = OrderProvider.SendBrasPagDecryptPaymentService("1", id_Loja, PostedValues[Crypt]);
                                    PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, "BankSlipBrasPagPaymentGateway", PaymentGatewayRecordStatusType.Unknown, decryptedData);                                
                                } 
                            }
                           
                        }
                    }                
                return canProcess;
            }
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            var payment = holder.BTOrder.Payments[0];
            var orderPayment = (holder.Order as Order_V01).Payments[0] as WirePayment_V01;

            payment.PaymentCode = "BT";
            // orderPayment.PaymentCode = "BB";
            // Need this? what other data?
            payment.NumberOfInstallments = 1;
        }

        private void SettleCustomerOrderPayment(MyHLShoppingCart shoppingCart, string distributorOrderId)
        {
            if (shoppingCart != null && shoppingCart.CustomerOrderDetail != null && !string.IsNullOrEmpty(shoppingCart.CustomerOrderDetail.CustomerOrderID))
            {
                var customerOrderV01 =
                    CustomerOrderingProvider.GetCustomerOrderByOrderID(shoppingCart.CustomerOrderDetail.CustomerOrderID);
                CustomerOrderingProvider.UpdateCustomerOrderDistributorOrderID(customerOrderV01.OrderID,
                                                                       distributorOrderId);
                CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID,
                                                                 customerOrderV01.OrderStatus,
                                                                 ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.ShippedAsDo);
            }
        }
    }

     
 
}
