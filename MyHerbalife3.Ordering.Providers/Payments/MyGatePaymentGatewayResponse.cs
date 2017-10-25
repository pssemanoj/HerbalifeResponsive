namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MyGatePaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "VARIABLE1";
        private const string GateWay = "VARIABLE2";
        private const string AuthResult = "_RESULT";
        private const string PaymentGateWayName = "MyGate";
     
        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (PostedValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;

                    int result = -1;
                    string auth = PostedValues[AuthResult];
                    base.AuthResultMissing = (string.IsNullOrEmpty(auth));
                    int.TryParse(auth, out result);
                    this.OrderNumber = PostedValues[Order];
                    IsApproved = result >= 0;   // 0 is successful, >0 is successful with warning, <0 failded
                }

                return canProcess;
            }
        }

        public MyGatePaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
