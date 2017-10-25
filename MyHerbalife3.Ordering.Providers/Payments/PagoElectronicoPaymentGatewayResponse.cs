using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PagoElectronicoPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "orderid";
        private const string AuthResult = "Respuesta";
        private const string GateWay = "Agency";
        private const string AuthorizationNumber = "authnum";
        private const string PaymentGateWayName = "PagoElectronico";

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                HttpContext context = HttpContext.Current;
                HttpRequest request = context.Request;

                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;

                    if (string.IsNullOrEmpty(PostedValues[AuthResult]))
                    {
                        base.AuthResultMissing = true;
                    }
                    else
                    {
                        IsApproved = PostedValues[AuthResult] == "APPROVED";
                    }
                }
                OrderNumber = PostedValues[Order];
                base.AuthorizationCode = PostedValues[AuthorizationNumber];

                return canProcess;
            }
        }

        public PagoElectronicoPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
