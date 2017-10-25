namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PagoSeguroPaymentGatewayResponse : PaymentGatewayResponse
    {
        //Manuel - These are made up and need to be set correctly
        private const string Order = "SomeOrderVariableNameFromPost";
        private const string GateWay = "SomeVariableNameFromPost";
        private const string PaymentGateWayName = "PagoSeguro";
     
        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                //We don't know whether there is Post or QueryString data from PagoSeguro yet. Set this all up when we get the details
                if (PostedValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;
                    CanSubmitIfApproved = false; //we did this already in the webservice
                    this.OrderNumber = PostedValues[Order];
                }

                return canProcess;
            }
        }

        public PagoSeguroPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
