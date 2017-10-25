using System.Collections.Generic;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using PlugInClient;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PayclubPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        private PayclubPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("PayclubPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");

            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=Payclub");
            string acquirerId = _configHelper.GetConfigEntry("paymentGatewayAcquirerId"); //acquirerId
            string storeId = _configHelper.GetConfigEntry("paymentGatewayCommerceId"); //commerceId
            string localId = _configHelper.GetConfigEntry("paymentGatewayLocalId"); //localId
            string vectorId; // = "mV6VoYVJ54A="; // GetConfigEntry("paymentGatewayVectorId"); //vectorId
            string orderNumber = _orderNumber;
            string amount = _orderAmount.ToString().Replace(",", string.Empty);
            string payClubPublicKey; // = ResolveUrl("\\App_data\\PaymentKeysPayclub\\PUBLICA_CIFRADO_INTERDIN.pem");
            string herbalifePrivateKey; //= ResolveUrl("\\App_data\\PaymentKeysPayclub\\PRIVADA_FIRMA_HERBALIFE.pem");

            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
            {
                acquirerId = _configHelper.GetConfigEntry("paymentGatewayAcquirerId"); //acquirerId
                storeId = _configHelper.GetConfigEntry("paymentGatewayCommerceId"); //commerceId
                localId = _configHelper.GetConfigEntry("paymentGatewayLocalId"); //localId
                vectorId = "mV6VoYVJ54A="; // GetConfigEntry("paymentGatewayVectorId"); //vectorId
                payClubPublicKey = ResolveUrl("\\App_data\\PaymentKeysPayclub\\PUBLICA_CIFRADO_INTERDIN.pem");
                herbalifePrivateKey = ResolveUrl("\\App_data\\PaymentKeysPayclub\\PRIVADA_FIRMA_HERBALIFE.pem");
            }
            else
            {
                acquirerId = _configHelper.GetConfigEntry("BetapaymentGatewayAcquirerId"); //acquirerId
                storeId = _configHelper.GetConfigEntry("BetapaymentGatewayCommerceId"); //commerceId
                localId = _configHelper.GetConfigEntry("BetapaymentGatewayLocalId"); //localId
                vectorId = "6pukFwMTOIA="; // GetConfigEntry("paymentGatewayVectorId"); //vectorId
                payClubPublicKey = ResolveUrl("\\App_data\\PaymentKeysPayclub\\BETA_PUBLICA_CIFRADO_INTERDIN.pem");
                herbalifePrivateKey = ResolveUrl("\\App_data\\PaymentKeysPayclub\\BETA_PRIVADA_FIRMA_HERBALIFE.pem");
            }

            var dataList = new List<string>();
            dataList = GenerateHash(payClubPublicKey, herbalifePrivateKey, acquirerId, storeId, orderNumber, amount,
                                    returnUrl, vectorId, localId);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();
            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrl);
                //http://as400web:10100/webmpi/VPOS
            sb.AppendFormat("<input type='hidden' name='TRANSACCIONID' value='{0}'>", orderNumber);
            sb.AppendFormat("<input type='hidden' name='XMLREQUEST' value='{0}'>", dataList[0]);
            sb.AppendFormat("<input type='hidden' name='XMLDIGITALSIGN' value='{0}'>", dataList[1]);
            sb.AppendFormat("<input type='hidden' name='XMLACQUIRERID' value='{0}'>", acquirerId);
            sb.AppendFormat("<input type='hidden' name='XMLMERCHANTID' value='{0}'>", storeId);
            sb.AppendFormat("<input type='hidden' name='XMLGENERATEKEY' value='{0}'>", dataList[2]);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            string response = sb.ToString();

            LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);
            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private List<string> GenerateHash(string payClubPublicKey,
                                          string herbalifePrivateKey,
                                          string acquirerId,
                                          string storeId,
                                          string orderNumber,
                                          string amount,
                                          string returnUrl,
                                          string vectorId,
                                          string localId)
        {
            var cipheredDataList = new List<string>();
            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart
                     ?? ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);
            var plugin = new PlugInClientSend();
            string xmlRequest = "";
            string xmlFirma = "";
            string xmlGenerateKeyI = "";

            plugin.setLocalID(localId);
            plugin.setTransacctionID(orderNumber);
            plugin.setReferencia1("xxx");
            plugin.setReferencia2("xxx");
            plugin.setReferencia3("xxx");
            plugin.setReferencia4("xxx");
            plugin.setReferencia5("xxx");

            //Int32 valor = 100 * Convert.ToInt32(amount);
            OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
            string valor =
                (string.Format(getPriceFormat((null != myCart.Totals ? totals.TaxableAmountTotal : 0)),
                               (null != myCart.Totals ? totals.TaxableAmountTotal : 0)).Replace(".", "")).Replace
                    (",", "");
                // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.

            plugin.setTransacctionValue(valor);
            string taxValue1 =
                (string.Format(getPriceFormat((null != myCart.Totals ? totals.TaxAmount : 0)),
                               (null != totals ? totals.TaxAmount : 0)).Replace(".", "")).Replace(",", "");
            plugin.setTaxValue1(taxValue1);

            plugin.setTaxValue2("000");

            plugin.setTipValue("000");

            plugin.setCurrencyID("840");

            plugin.setIV(vectorId);
            //read from file, validate the paths
            plugin.setSignPrivateKey(RSAEncryption.readFile(herbalifePrivateKey));
            plugin.setCipherPublicKeyFromFile(payClubPublicKey);
            xmlGenerateKeyI = plugin.CreateXMLGENERATEKEY();

            HttpRequest masquerader = new HttpRequest(_context.Request.FilePath,
                                           _context.Request.Url.OriginalString.Replace("http://", "https://").Replace(":80", string.Empty),
                                           _context.Request.Url.Query);
            plugin.XMLProcess(masquerader);

            xmlRequest = plugin.getXMLREQUEST();
            xmlFirma = plugin.getXMLDIGITALSIGN();
            cipheredDataList.Add(xmlRequest);
            cipheredDataList.Add(xmlFirma);
            cipheredDataList.Add(xmlGenerateKeyI);
            return cipheredDataList;
        }

     

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
        }
    }
}