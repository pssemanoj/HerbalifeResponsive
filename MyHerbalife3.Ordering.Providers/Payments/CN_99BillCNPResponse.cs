using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public partial class CN_99BillCNPResponse : PaymentGatewayResponse
    {
        public string GatewayAmount { get; set; }

        //private string msgReturn;
        public override bool CanProcess
        {
            get
            {
                CanSubmitIfApproved = true;
                string VerStr = QueryValues["VerStr"] != null ? CN_99BillPaymentGatewayInvoker.Decrypt(QueryValues["VerStr"], CN_99BillPaymentGatewayInvoker.EncryptionKey) : string.Empty;
                string[] parts = VerStr.Split(',');
                if (parts.Length == 6)
                {
                    OrderNumber = parts[0];
                    IsApproved = parts[1] == "1";
                    SpecialResponse = string.Format("{0},{1}", parts[3], parts[4]);
                    GatewayAmount = parts[5];
                }
                else
                    OrderNumber = string.Empty;
                return !string.IsNullOrEmpty(OrderNumber);
            }
        }

        /// <summary>
        ///  pass additional info
        /// </summary>
        /// <param name="holder"></param>
        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            Order_V01 orderv01 = (holder.Order as Order_V01);
            if (orderv01 == null)
                return;
            if (orderv01.Payments != null && (orderv01.Payments).Count > 0)
            {
                var orderPayment = orderv01.Payments[0] as CreditPayment_V01;
                if (orderPayment != null && SpecialResponse != null)
                {
                    var btPayment = holder.BTOrder.Payments[0];
                    btPayment.PaymentCode = orderPayment.TransactionType = "CC";
                    string[] bankInfo = SpecialResponse.Split(',');
                    if (bankInfo.Length == 2)
                    {
                        orderPayment.Card.IssuingBankID = bankInfo[0]; // this is PayChannel
                        orderPayment.AuthorizationMerchantAccount = bankInfo[1]; // this is PaymentID
                    }
                }
            }
        }
    }
}
