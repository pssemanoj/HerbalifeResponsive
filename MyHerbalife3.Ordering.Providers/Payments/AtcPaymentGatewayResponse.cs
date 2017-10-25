using System.Collections.Specialized;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class AtcPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string Order = "OrderNumber";
        private const string Amount = "amount";
        private const string AuthResult = "ATCRTA";
        private const string Authorized = "1";
        private const string AuthResultCode = "authcode";
        private const string PaymentGatewayName = "ATC";
        private const string processingLocation = "3H";

        public AtcPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                string CodeAprroved = string.Empty;
                HttpContext context = HttpContext.Current;
                HttpRequest request = context.Request;

                NameValueCollection requestValues1 = new NameValueCollection();
                NameValueCollection requestValues2 = new NameValueCollection();

                requestValues1 = QueryValues;
                requestValues2 = PostedValues;

                if ((QueryValues[GateWay] == PaymentGatewayName) || (!string.IsNullOrEmpty(QueryValues[GateWay])))
                {
                    if (QueryValues[GateWay].Contains("ATC"))
                    {
                        canProcess = true;

                        if (string.IsNullOrEmpty(QueryValues[GateWay]))
                        {
                            base.AuthResultMissing = true;
                        }
                        else
                        {
                            OrderNumber = GetOrderNumber(QueryValues[GateWay]);
                            CodeAprroved = GetCodeAprroved(QueryValues[GateWay]);
                            IsApproved = CodeAprroved == Authorized; // 1 means successful, 0 means error
                            if (IsApproved)
                            {
                                AuthorizationCode = CodeAprroved;
                                GatewayName = PaymentGatewayName;
                            }
                        }
                    }
                }

                return canProcess;
            }
        }

        private string GetOrderNumber(string url)
        {
            return url.Substring(url.Length - 19, 10);
        }

        private string GetCodeAprroved(string url)
        {
            string code = string.Empty;

            code = url.Substring(url.Length - 8, 8);
            if ((code.Contains("ATCRTA")) && (code.Substring((code.Length - 2), 1) == "1"))
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
    }
}