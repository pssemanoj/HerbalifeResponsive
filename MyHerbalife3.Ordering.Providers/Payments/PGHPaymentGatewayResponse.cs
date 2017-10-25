using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.Common.Logging;
using HL.PGH.Api;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PGHPaymentGatewayResponse : PaymentGatewayResponse
    {
        private PaymentResponse _response = null;

        public string PhgPaymentStatus
        {
            get { return _response.PaymentStatus.ToString(); }
        }

        public PGHPaymentGatewayResponse()
        {
            base.GatewayName = "PGHPaymentGateway";
        }

        public OrderStatus OrderStatus { get; set; }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                _response = new PaymentResponse();
                if (_response.IsPaymentGatewayResponse)
                {
                    LoggerHelper.Warn(string.Format("PGH Splunk Logging: Processing the Response for Order {0}", _response.OrderNumber));

                    canProcess = true;
                    this.OrderNumber = _response.OrderNumber;
                    this.AuthorizationCode = _response.AuthorizationCode;
                    this.GatewayName = _response.GatewayName;
                    this.TransactionCode = _response.TransactionCode;                    
                    this.IsReturning = _response.IsReturning;
                    this.CardType = CreditCard.GetCardType(_response.CardType);
                    this.CardNumber = _response.CardNumber;
                  
                    SessionInfo _sessionInfo = SessionInfo.GetSessionInfo(_response.DistributorId, System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                    if (_sessionInfo != null)
                    {
                        _sessionInfo.OrderNumber = this.OrderNumber;                         
                    }
                    this.OrderStatus = _response.OrderStatus;
                    if (_response.OrderStatus == OrderStatus.Submitted)
                    {
                        this.Status = PaymentGatewayRecordStatusType.OrderSubmitted;
                    }
                    if (_response.PaymentStatus == PaymentStatus.Approved)
                    {
                        this.Status = PaymentGatewayRecordStatusType.Approved;
                    }
                    if (_response.PaymentStatus == PaymentStatus.Declined)
                    {
                        this.Status = PaymentGatewayRecordStatusType.Declined;
                    }
                    if (_response.PaymentStatus == PaymentStatus.ApprovalPending)
                    {
                        this.Status = PaymentGatewayRecordStatusType.ApprovalPending;
                    }
                    if (_response.PaymentStatus == PaymentStatus.Cancelled)
                    {
                        this.Status = PaymentGatewayRecordStatusType.CancelledByUser;
                    }

                    PaymentGatewayRecordStatusType status = OrderProvider.GetPaymentGatewayRecordStatus(this.OrderNumber);
                    if(status != this.Status)
                    {
                        this.Status = status;
                        string message = string.Format("PGH Status received in Request was not same as Status in PGTMS for OrderNumber: {0}. Received {1}, PGTMS Status {2}. Reverted to PGTMS Status.", this.OrderNumber, this.Status, status);
                        PaymentGatewayInvoker.LogMessageWithInfo(PaymentGatewayLogEntryType.Error, this.OrderNumber, _response.DistributorId, _response.GatewayName, this.Status, message);
                    }

                    this.IsApproved = this.Status == PaymentGatewayRecordStatusType.Approved;
                    this.IsPendingTransaction = this.Status == PaymentGatewayRecordStatusType.ApprovalPending;
                    this.IsCancelled = this.Status == PaymentGatewayRecordStatusType.CancelledByUser;
                }

                return canProcess;
            }
        }
        protected override bool DetermineSubmitStatus()
        {
            if (_response.OrderSubmitted)
            {
                LoggerHelper.Info("PGH Splunk Logging: DetermineSubmitStatus says No");
                return false;
            }
            else
            {
                LoggerHelper.Info("PGH Splunk Logging: DetermineSubmitStatus says Yes");
                return _response.PaymentStatus == PaymentStatus.Approved;
            }
        }

        //protected override bool LogResponses(bool currentState)
        //{
        //    return true;
        //}
    }
}
