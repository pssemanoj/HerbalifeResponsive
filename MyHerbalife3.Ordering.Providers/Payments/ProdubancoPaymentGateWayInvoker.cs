using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using VPOS20_PLUGIN;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ProdubancoPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        private ProdubancoPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("ProdubancoPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");

            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=Produbanco");
            string acquirerId;
                // = GetConfigEntry("paymentGatewayAcquiredId"); //// The IDACQUIRER for production is = 18
            string storeId; // = GetConfigEntry("paymentGatewayStoreId"); //2482
            string orderNumber = _orderNumber;
            string amount =
                (string.Format(getPriceFormat(_orderAmount), _orderAmount).Replace(".", "").Replace(",", ""));
            string vPOSPublicKey = string.Empty; // GetConfigEntry("paymentGatewayVPOSCRYPTOPublicKey");
            string herbalifePrivateKey = string.Empty; // GetConfigEntry("paymentGatewayHerbalifeSignaturePrivateKey");
            string vectorId; // = GetConfigEntry("paymentGatewayVectorId"); //vectorId
            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
            {
                storeId = _configHelper.GetConfigEntry("paymentGatewayStoreId"); //2482
                acquirerId = _configHelper.GetConfigEntry("paymentGatewayAcquiredId");
                vectorId = _configHelper.GetConfigEntry("paymentGatewayVectorId");
            }
            else
            {
                redirectUrl = _configHelper.GetConfigEntry("paymentGatewayBetaUrl");
                storeId = _configHelper.GetConfigEntry("paymentGatewayBetaStoreId"); //2482
                acquirerId = _configHelper.GetConfigEntry("paymentGatewayBetaAcquiredId");
                vectorId = _configHelper.GetConfigEntry("paymentGatewayBetaVectorId");
            }

            var colorList = new List<string>();
            colorList = GenerateHash(vPOSPublicKey, herbalifePrivateKey, acquirerId, storeId, orderNumber, vectorId,
                                     amount, returnUrl);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrl);
                //https://servicios.alignet.com/VPOS/MM/transactionStart20.do
            sb.AppendFormat("<input type='hidden' name='IDACQUIRER' value='{0}'>", acquirerId);
            sb.AppendFormat("<input type='hidden' name='IDCOMMERCE' value='{0}'>", storeId);
            sb.AppendFormat("<input type='hidden' name='SESSIONKEY' value='{0}'>", colorList[0]);
            sb.AppendFormat("<input type='hidden' name='XMLREQ' value='{0}'>", colorList[1]);
            sb.AppendFormat("<input type='hidden' name='DIGITALSIGN' value='{0}'>", colorList[2]);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private List<string> GenerateHash(string vPOSPublicKey,
                                          string herbalifePrivateKey,
                                          string acquirerId,
                                          string storeId,
                                          string orderNumber,
                                          string vectorId,
                                          string amount,
                                          string returnUrl)
        {
            var cipheredDataList = new List<string>();
            string R1; // = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayVPOSCRYPTOPublicKey.txt");
            string R2;
                // = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayHerbalifeSignaturePrivateKey.txt");
            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
            {
                R1 = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayVPOSCRYPTOPublicKey.txt");
                R2 = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayHerbalifeSignaturePrivateKey.txt");
            }
            else
            {
                R1 = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\BetapaymentGatewayVPOSCRYPTOPublicKey.txt");
                R2 = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\BetapaymentGatewayHerbalifeSignaturePrivateKey.txt");
            }

            var srVPOSLlaveCifradoPublica = new StreamReader(R1);
            var srComercioLlaveFirmaPrivada = new StreamReader(R2);

            MyHLShoppingCart myCart;


            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart
                     ?? ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);

            var oVPOSBean = new VPOSBean();
            oVPOSBean.acquirerId = acquirerId;
            oVPOSBean.commerceId = storeId;
            oVPOSBean.purchaseCurrencyCode = "840";
            oVPOSBean.purchaseAmount = amount;
            oVPOSBean.purchaseOperationNumber = orderNumber;

            OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
            VPOSTax oVPOSTax;
            var aTaxes = new ArrayList();
            oVPOSTax = new VPOSTax();
            oVPOSTax.Name = "IVA";
            oVPOSTax.Amount =
                (string.Format(getPriceFormat((null != totals ? totals.TaxAmount : 0)),
                               (null != totals ? totals.TaxAmount : 0)).Replace(".", "")).Replace(",", "");
                // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.

            aTaxes.Add(oVPOSTax);
            oVPOSTax = new VPOSTax();
            oVPOSTax.Name = "Servicio";
            oVPOSTax.Amount = "0";

            //string totalplustax = string.Empty;
            //totalplustax = (string.Format(getPriceFormat((null != myCart.Totals ? myCart.Totals.DiscountedItemsTotal + ((null != pHCharge ? pHCharge.Amount : 0) + (null != freightCharge ? freightCharge.Amount : 0)) : 0)), (null != myCart.Totals ? myCart.Totals.DiscountedItemsTotal + ((null != pHCharge ? pHCharge.Amount : 0) + (null != freightCharge ? freightCharge.Amount : 0)) : 0)).ToString().Replace(".", "")).Replace(",", ""); // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.

            aTaxes.Add(oVPOSTax);
            oVPOSTax = new VPOSTax();
            oVPOSTax.Name = "Monto Grava IVA";
            //oVPOSTax.Amount = totalplustax;
            oVPOSTax.Amount =
                (string.Format(getPriceFormat((null != totals ? totals.TaxableAmountTotal : 0)),
                               (null != totals ? totals.TaxableAmountTotal : 0)).Replace(".", "")).Replace
                    (",", "");
                // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.
            aTaxes.Add(oVPOSTax);

            oVPOSTax = new VPOSTax();
            oVPOSTax.Name = "Monto No Grava IVA";
            oVPOSTax.Amount = (string.Format(getPriceFormat((null != totals ? totals.AmountDue - totals.TaxAmount - totals.TaxableAmountTotal : 0)),
                               (null != totals ? totals.AmountDue - totals.TaxAmount - totals.TaxableAmountTotal : 0)).Replace(".", "")).Replace(",", "");
            aTaxes.Add(oVPOSTax);
            oVPOSBean.taxes = aTaxes;

            var oVPOSSend = new VPOSSend(srVPOSLlaveCifradoPublica, srComercioLlaveFirmaPrivada, vectorId);
            oVPOSSend.execute(ref oVPOSBean);

            cipheredDataList.Add(oVPOSBean.cipheredSessionKey);
            cipheredDataList.Add(oVPOSBean.cipheredXML);
            cipheredDataList.Add(oVPOSBean.cipheredSignature);

            return cipheredDataList;
        }

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01) p).ChargeType == type; }) as Charge_V01 ??
                   new Charge_V01(type, (decimal) 0.0);
        }

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
        }

   
    }
}