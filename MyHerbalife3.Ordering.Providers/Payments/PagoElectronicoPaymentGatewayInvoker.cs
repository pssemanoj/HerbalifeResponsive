using System.Text;
using System;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.PaymentGatewayBridgeSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PagoElectronicoPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PagoElectronicoPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("PagoElectronicoPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string approvedUrl = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));
            string merchantId = _config.MerchantAccountName;

            string termId = _configHelper.GetConfigEntry("termId");
            string storeId = _configHelper.GetConfigEntry("storeId");
            string currenyId = _configHelper.GetConfigEntry("currenyId");
            string entId = _configHelper.GetConfigEntry("entId");
            string amount = (Convert.ToInt32(this._orderAmount * 100)).ToString();

           // Payment Gateway Bridge service
            var proxy = ServiceClientProvider.GetPaymentGatewayBridgeProxy();

            GetVEPagoElectronicoDigestRequest_V01 request = new GetVEPagoElectronicoDigestRequest_V01();
            request.Amount = amount;
            request.CurrencyId = currenyId;
            request.MerchantId = merchantId;
            request.OrderId = this.OrderNumber;
            request.TerminalId = termId;
            request.StoreId = storeId;

            GetVEPagoElectronicoDigestResponse_V01 response1 = proxy.GetVEPagoElectronicoDigest(new GetVEPagoElectronicoDigestRequest1(request)).GetVEPagoElectronicoDigestResult as GetVEPagoElectronicoDigestResponse_V01;
            string digest = response1.Digest;

            // Post and redirect to Pago Electronico website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='orderId' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='total' value='{0}'>", amount);
            sb.AppendFormat("<input type='hidden' name='strvalue2' value='{0}'>", approvedUrl + "?Agency=PagoElectronico");
            sb.AppendFormat("<input type='hidden' name='currId' value='{0}'>", currenyId);
            sb.AppendFormat("<input type='hidden' name='merchId' value='{0}'>", merchantId);
            sb.AppendFormat("<input type='hidden' name='termId' value='{0}'>", termId);
            sb.AppendFormat("<input type='hidden' name='storeId' value='{0}'>", storeId);
            sb.AppendFormat("<input type='hidden' name='digest' value='{0}'>", digest);
            sb.AppendFormat("<input type='hidden' name='strvalue1' value='{0}'>", this.OrderNumber);

            sb.AppendFormat("<input type='hidden' name='tipo_pago' value='{0}'>", "1");
            sb.AppendFormat("<input type='hidden' name='color_table' value='{0}'>", "blue");
            sb.AppendFormat("<input type='hidden' name='color_fondo' value='{0}'>", "white");
            sb.AppendFormat("<input type='hidden' name='color_celda' value='{0}'>", "green");
            sb.AppendFormat("<input type='hidden' name='color_celda2' value='{0}'>", "green");
            sb.AppendFormat("<input type='hidden' name='color_letra' value='{0}'>", "black");
            sb.AppendFormat("<input type='hidden' name='tipo_letra' value='{0}'>", "arial");
            sb.AppendFormat("<input type='hidden' name='tamano_letra' value='{0}'>", "2");
            sb.AppendFormat("<input type='hidden' name='ancho_tabla' value='{0}'>", "50");
            sb.AppendFormat("<input type='hidden' name='centrado_tabla' value='{0}'>", "center");

            sb.AppendFormat("<input type='hidden' name='entId' value='{0}'>", entId);
            sb.AppendFormat("<input type='hidden' name='pageType' value='{0}'>", "php");

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }
    }
}
