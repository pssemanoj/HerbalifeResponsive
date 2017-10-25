using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PuntoWebPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "O10";
        private const string AuthResult = "O1";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "PuntoWeb";
        private const string AuthCode = "O2";

        private const string cardNumber = "O15";
        private const string mc = "MC";
        
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
                        IsApproved = PostedValues[AuthResult] == "A";
                        CardNumber = PostedValues[cardNumber];
                        CardType = ServiceProvider.OrderSvc.IssuerAssociationType.MasterCard;
                        AuthorizationCode = PostedValues[AuthCode];
                    }
                }

                OrderNumber = PostedValues[Order];

                return canProcess;
            }
        }

        public PuntoWebPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
