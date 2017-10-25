namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class RBSPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "OrderNumber";
        private const string GateWay = "Agency";
        private const string AuthResult = "Status";
        private const string PaymentGateWayName = "RBS";
        private const string OrderKey = "orderKey";

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;

                    if (string.IsNullOrEmpty(QueryValues[AuthResult]))
                    {
                        base.AuthResultMissing = true;
                    }
                    else
                    {
                        IsApproved = QueryValues[AuthResult] == "Approved";
                    }
                    OrderNumber = QueryValues[Order];
                    if (!string.IsNullOrEmpty(OrderNumber))
                    {
                        string key = QueryValues[OrderKey];
                        if (!string.IsNullOrEmpty(key) && key.Length > 10)
                        {
                            string orderKey = string.Concat(OrderNumber, "_");
                            int transCodePos = key.IndexOf(orderKey);
                            if (transCodePos > 10)
                            {
                                string transCode = key.Substring(transCodePos + 11);
                                if (!string.IsNullOrEmpty(transCode))
                                {
                                    this.TransactionCode = transCode;
                                }
                            }
                        }
                    }
                }

                return canProcess;
            }
        }

        public RBSPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }
    }
}
