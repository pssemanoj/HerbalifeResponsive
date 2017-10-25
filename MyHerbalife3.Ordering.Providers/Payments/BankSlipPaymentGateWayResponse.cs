using System.Web;
using System.Web.Security;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


    public class BankSlipPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "Order";
        private const string AuthResult = "Status";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "BankSlip";

        private const string Authorized = "3";
        // just for testing, we will need uncomment the next line once we are in production enviroment

        // private const string Authorized = "4";  // Awaiting payment For BankSlip the inicial status is always 4 (Awaiting payment). Since the BankSlip payment is not online.

        public BankSlipPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                //To disable Recieve Itau BR payment Responses  
                if (HLConfigManager.Configurations.PaymentsConfiguration.DisableBankSlip)
                {
                    canProcess = false;
                }
                else
                {
                    if (QueryValues[GateWay] == PaymentGateWayName)
                    {
                        canProcess = true;

                        if (string.IsNullOrEmpty(QueryValues[Order]))
                        {
                            base.AuthResultMissing = true;
                        }
                        else
                        {
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
                                                "BankSlipPaymentGatewayResponse, Error in Settling Customer Order. Order Number  : {0} Exception : {1}",
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
                                                "BankSlipPaymentGatewayResponse, Session is null. Order Number  : {0} ",
                                                OrderNumber));
                                    }
                                }
                                else
                                {
                                    LoggerHelper.Error(
                                        string.Format(
                                            "BankSlipPaymentGatewayResponse, DistributorID is null. Order Number  : {0} ",
                                            OrderNumber));
                                }
                            }

                            IsApproved = result == Authorized;
                            AuthorizationCode = QueryValues[AuthResult];
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
                                                                 ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType
                                                                     .ShippedAsDo);
            }
        }
    }
}