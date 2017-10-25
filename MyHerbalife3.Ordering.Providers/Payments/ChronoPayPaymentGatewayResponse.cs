using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ChronoPayPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string CustomResponseBody = "<html><head><title>200 OK</title></head><body>200 OK</body></html>";
        private const string Order = "cs1";
        private const string GateWay = "Agency";
        private const string AuthResult = "Status";
        private const string PaymentGateWayName = "ChronoPay";
        private const string ResponseCode = "response_code";
        private const string AuthCode = "auth_code";
        private const string Amount = "cs2";
        private const string Agency = "cs3";
       
        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                this.ReloadShoppingCart = true;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;
                    OrderNumber = PostedValues[Order];
                    if (string.IsNullOrEmpty(OrderNumber) || QueryValues[GateWay] != PostedValues[Agency])
                    {
                        LogSecurityWarning(PaymentGateWayName);
                        return canProcess;
                    }

                    this.IsReturning = true;
                    this.CanSubmitIfApproved = false;
                    this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                    this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted);
                    return canProcess;
                }
                else
                {
                    if (PostedValues[Agency] == PaymentGateWayName) //This is a callback
                    {
                        canProcess = true;
                        this.SpecialResponse = CustomResponseBody; 
                        AuthorizationCode = PostedValues[AuthCode];
                        if (!string.IsNullOrEmpty(AuthCode))
                        {
                            OrderNumber = PostedValues[Order];
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                LogSecurityWarning("ChronoPay");
                            }
                            else
                            {
                                this.CanSubmitIfApproved = true;
                                this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                                IsApproved = true;
                            }
                        }
                        else
                        {
                            base.AuthResultMissing = true;
                        }
                    }
                }

                return canProcess;
            }
        }

        public ChronoPayPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }
    }
}
