using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class WebPayPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "TBK_ORDEN_COMPRA";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "WebPay";
        private const string RedirectStatus = "Status";

        private const string CardNum = "CardNum";
        private const string Amount = "Amount";
        private const string AuthCode = "AuthorizationCode";
        private const string TransCode = "TransactionCode";
        private const string InstallmentNumber = "InstallmentNum";
        private const string InstallmentType = "InstallmentType";
        private const string UrlOrderNumber = "OrderNum";
        
        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;
                    this.CanSubmitIfApproved = false;
                    if (string.IsNullOrEmpty(PostedValues[Order]) && string.IsNullOrEmpty(QueryValues[UrlOrderNumber]))
                    {
                        base.AuthResultMissing = true;
                    }
                    else
                    {
                        OrderNumber = !string.IsNullOrEmpty(PostedValues[Order]) ? PostedValues[Order] : QueryValues[UrlOrderNumber];
                        this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                        string redirectStatus = QueryValues[RedirectStatus].ToUpper();
                        this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved && redirectStatus == "APPROVED");
                    }
                }
                return canProcess;
            }
        }

        public WebPayPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            ServiceProvider.SubmitOrderBTSvc.Payment payment = holder.BTOrder.Payments[0];
            CreditPayment_V01 orderPayment = (holder.Order as Order_V01).Payments[0] as CreditPayment_V01;
            List<string> PaymentGatewayLog = OrderProvider.GetPaymentGatewayLog(OrderNumber, PaymentGatewayLogEntryType.Response);
            string theOne = PaymentGatewayLog.Find(i => i.Contains(CardNum));
            if(!string.IsNullOrEmpty(theOne))
            {
                NameValueCollection theResponse = GetRequestVariables(theOne);
                // Card Number
                if (!string.IsNullOrEmpty(theResponse[CardNum]))
                {
                    CardNumber = theResponse[CardNum];
                }
                // Authorization Code
                if (!string.IsNullOrEmpty(theResponse[AuthCode]))
                {
                    base.AuthorizationCode = theResponse[AuthCode];
                }
                base.GetPaymentInfo(holder);

                //WebPay Amount
                if (!string.IsNullOrEmpty(theResponse[Amount]))
                {
                    payment.Amount = Int32.Parse(theResponse[Amount]);
                }
                orderPayment.Amount = payment.Amount;
          

                // Number of Installments
                PaymentOptions_V01 options = new PaymentOptions_V01();
                if (!string.IsNullOrEmpty(theResponse[InstallmentNumber]))
                {
                    options.NumberOfInstallments = Int32.Parse(theResponse[InstallmentNumber]);
                }
                else
                {
                    options.NumberOfInstallments = 1;
                }
                payment.NumberOfInstallments = options.NumberOfInstallments;
                orderPayment.PaymentOptions = options;

                // Installment Type is Card Type (VN for credit card, VD for debit card); borrow the Auth Merchant field to place the data to display on the page
                if (!string.IsNullOrEmpty(theResponse[InstallmentType]))
                {
                    payment.AuthMerchant = theResponse[InstallmentType];
                    switch (theResponse[InstallmentType].ToString().ToUpper())
                    {
                        case "VN":
                            CardType = IssuerAssociationType.Visa;
                            break;
                        case "VD":
                            CardType = IssuerAssociationType.GenericDebitCard;
                            break;
                        default:
                            CardType = IssuerAssociationType.Visa;
                            break;
                    }
                }
                orderPayment.AuthorizationMerchantAccount = payment.AuthMerchant;              
            }
        }

        private NameValueCollection GetRequestVariables(string requestData)
        {
            NameValueCollection result = new NameValueCollection();
            List<string> items = new List<string>(requestData.Split(new char[]{'&'}));
            foreach (string item in items)
            {
                string[] elements = item.Split(new char[] {'='});
                if(elements.Length == 2)
                {
                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }
    }
}
