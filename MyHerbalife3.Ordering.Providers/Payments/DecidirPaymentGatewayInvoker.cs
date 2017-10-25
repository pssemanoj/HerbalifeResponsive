using System;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class DecidirPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private DecidirPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("DecidirPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string nroTrade = _configHelper.GetConfigEntry("paymentGatewayApplicationId");
            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);
            string email = (null != myCart && !string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress.ToString() : "null.decidir.com";
            string template = _configHelper.GetConfigEntry("paymentGatewayStyle");
            var payment = HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            HttpContext.Current.Session.Remove(PaymentInformation);
            string cardType = (null != payment) ? payment.Card.IssuingBankID : null;
            string installments = (null != payment)
                                      ? (payment.PaymentOptions as PaymentOptions_V01).NumberOfInstallments.ToString()
                                      : null;

            if (string.IsNullOrEmpty(cardType))
            {
                throw new ApplicationException("No Credit Card type was found. Unable to proceed");
            }
            if (string.IsNullOrEmpty(installments))
            {
                throw new ApplicationException("No Credit Card Installment Value was found. Unable to proceed");
            }

            // Post and redirect to Decidir website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='NROCOMERCIO' value='{0}' size='8' maxlength='8'>", nroTrade);
            sb.AppendFormat("<input type='hidden' name='NROOPERACION' value='{0}' size='10' maxlength='10'>",
                            OrderNumber);
            sb.AppendFormat("<input type='hidden' name='MONTO' value='{0}' size='12' maxlength='12'>",
                            (Convert.ToInt32(_orderAmount*100)).ToString());
            sb.AppendFormat("<input type='hidden' name='CUOTAS' value='{0}' size='12'>", installments);
            sb.AppendFormat("<input type='hidden' name='EMAILCLIENTE' value='{0}'>", email);
            sb.AppendFormat("<input type='hidden' name='IDTEMPLATES' value='{0}'>", template);
            sb.AppendFormat("<input type='hidden' name='URLDINAMICA' value='{0}'>", returnUrl + "?Agency=Decidir");
            sb.AppendFormat("<input type='hidden' name='MEDIODEPAGO' value='{0}' size='12'>", cardType);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }
    }
}