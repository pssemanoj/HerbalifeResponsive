using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Text;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class WebPayPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private WebPayPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("WebPayPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _config.PaymentGatewayUrl;
            string approvedUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string declinedUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlDeclined);
                //_config.PaymentGatewayReturnUrlDeclined;
            string cookie = this._context.Session.SessionID.ToString();

            // Post and redirect to WebPay website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='TBK_ORDEN_COMPRA' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='TBK_ID_SESION' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='TBK_MONTO' value='{0}'>",
                            (Convert.ToInt32(this._orderAmount*100)).ToString());
            sb.AppendFormat("<input type='hidden' name='TBK_TIPO_TRANSACCION' value='{0}'>", "TR_NORMAL");
            sb.AppendFormat("<input type='hidden' name='TBK_URL_EXITO' value='{0}'>",
                            approvedUrl + "&Agency=WebPay&GUID=" + cookie + "&OrderNum=" + this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='TBK_URL_FRACASO' value='{0}'>",
                            declinedUrl + "&Agency=WebPay&GUID=" + cookie + "&OrderNum=" + this.OrderNumber);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        protected override void GetOrderNumber()
        {
            GenerateOrderNumberRequest_V01 request = new GenerateOrderNumberRequest_V01();
            request.Amount = _orderAmount;
            request.Country = _country;
            request.DistributorID = _distributorId;
            GenerateOrderNumberResponse_V01 response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                _orderNumber = response.OrderID;
                string orderData = _context.Session[PaymentGateWayOrder] as string;
                _context.Session.Remove(PaymentGateWayOrder);
                int recordId = OrderProvider.InsertPaymentGatewayRecord(_orderNumber, _distributorId, _gatewayName,
                                                                        orderData, _locale);
            }
        }
    }
}