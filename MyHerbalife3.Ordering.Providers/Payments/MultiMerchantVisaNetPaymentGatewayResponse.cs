using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MultiMerchantVisaNetPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "nordent";
        private const string AuthResult = "respuesta";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "MultiMerchantVisaNet";
        private const string authCode = "cod_autoriza";
        private const string processingLocation = "4L";
        private const string authorized = "1";

        private const string cardNumber = "pan";
        private const string visa = "VI";

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
                        IsApproved = PostedValues[AuthResult] == authorized; // 1 means successful, 2 means error

                        CardNumber = PostedValues[cardNumber];
                        CardType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.IssuerAssociationType.Visa;
                        AuthorizationCode = PostedValues[authCode];
                    }
                       
                }
                OrderNumber = processingLocation + PostedValues[Order];

                return canProcess;
            }
        }

        public MultiMerchantVisaNetPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
