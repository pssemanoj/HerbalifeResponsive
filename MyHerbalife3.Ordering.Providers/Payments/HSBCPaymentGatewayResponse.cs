namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class HSBCPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string PaymentGateWayName = "HSBC";
        private const string GateWay = "vpc_MerchTxnRef";
        private const string AuthResult = "vpc_TxnResponseCode";
        private const string Order = "vpc_OrderInfo";
        private const string AuthResultCode = "vpc_AuthorizeId";
        private const string TransactionId = "vpc_TransactionNo";

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    string result = QueryValues[AuthResult];
                    OrderNumber = QueryValues[Order]; 
                    IsApproved = result == "0";
                    AuthorizationCode = QueryValues[AuthResultCode] ?? string.Empty;
                    TransactionCode = QueryValues[TransactionId];
                    canProcess = true;
                }

                return canProcess;
            }
        }

        public HSBCPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
