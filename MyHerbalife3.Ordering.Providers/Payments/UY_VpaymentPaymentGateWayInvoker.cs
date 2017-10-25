using System;
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
    public class UY_VpaymentPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constructors and Destructors

        private UY_VpaymentPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("UY_VpaymentPaymentGateWay", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");

            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=vpayment");
            string acquirerId;
            string storeId;
            string orderNumber = this._orderNumber;
            string amount = (string.Format(this.getPriceFormat(this._orderAmount), this._orderAmount).Replace(".", "").Replace(",", ""));
            string vPOSPublicKey = string.Empty;
            string herbalifePrivateKey = string.Empty;
            string vectorId;
            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat") || RootUrl.Contains("10.36.158.182") || RootUrl.Contains("http://10.36.173.61"))
            {
                storeId = _configHelper.GetConfigEntry("paymentGatewayStoreId");
                acquirerId = _configHelper.GetConfigEntry("paymentGatewayAcquiredId");
                vectorId = _configHelper.GetConfigEntry("paymentGatewayVectorId");
            }
            else
            {
                redirectUrl = _configHelper.GetConfigEntry("paymentGatewayBetaUrl");
                storeId = _configHelper.GetConfigEntry("paymentGatewayBetaStoreId");
                acquirerId = _configHelper.GetConfigEntry("paymentGatewayBetaAcquiredId");
                vectorId = _configHelper.GetConfigEntry("paymentGatewayBetaVectorId");
            }

            var colorList = new List<string>();
            colorList = GenerateHash(vPOSPublicKey, herbalifePrivateKey, acquirerId, storeId, orderNumber, vectorId, amount, returnUrl);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='IDACQUIRER' value='{0}'>", acquirerId);
            sb.AppendFormat("<input type='hidden' name='IDCOMMERCE' value='{0}'>", storeId);
            sb.AppendFormat("<input type='hidden' name='SESSIONKEY' value='{0}'>", colorList[0]);
            sb.AppendFormat("<input type='hidden' name='XMLREQ' value='{0}'>", colorList[1]);
            sb.AppendFormat("<input type='hidden' name='DIGITALSIGN' value='{0}'>", colorList[2]);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

       

        #endregion

        #region Methods

        private List<string> GenerateHash(string vPOSPublicKey, string herbalifePrivateKey, string acquirerId, string storeId, string orderNumber, string vectorId, string amount, string returnUrl)
        {
            var cipheredDataList = new List<string>();
            string R1;
            string R2;
            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat") || RootUrl.Contains("10.36.158.182") || RootUrl.Contains("10.36.173.61"))
            {
                R1 = ResolveUrl("\\App_data\\PaymentsKeysVpaymentVisa\\paymentGatewayVpaymentVisaCRYPTOPublicKey.txt");
                R2 = ResolveUrl("\\App_data\\PaymentsKeysVpaymentVisa\\paymentGatewayHerbalifeSignaturePrivateKey.txt");
            }
            else
            {
                R1 = ResolveUrl("\\App_data\\PaymentsKeysVpaymentVisa\\BetapaymentGatewayVpaymentVisaCRYPTOPublicKey.txt");
                R2 = ResolveUrl("\\App_data\\PaymentsKeysVpaymentVisa\\BetapaymentGatewayHerbalifeSignaturePrivateKey.txt");
            }


            var srVposLlaveCifradoPublica = new StreamReader(R1);
            var srComercioLlaveFirmaPrivada = new StreamReader(R2);

            SessionInfo myCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            OrderTotals_V01 totals = myCart.ShoppingCart.Totals as OrderTotals_V01;
            var hasDiscount = OrderProvider.HasVATDiscount(totals);

            var oVposBean = new VPOSBean
            {
                acquirerId = acquirerId,
                commerceId = storeId,
                purchaseCurrencyCode = "858",
                purchaseAmount = amount,
                purchaseOperationNumber = orderNumber,
                reserved10 = hasDiscount ? "6" : "0",
                reserved11 = hasDiscount ? orderNumber.Substring(3): string.Empty,
                reserved12 = hasDiscount ? 
           (string.Format(
               this.getPriceFormat((null != totals ? totals.TaxableAmountTotal : 0)),
               (null != totals ? totals.TaxableAmountTotal : 0)).Replace(".", ""))
               .Replace(",", "") : string.Empty
              

            };
            

            VPOSTax oVPOSTax;
            var aTaxes = new ArrayList();
            oVPOSTax = new VPOSTax
            {
                Name = "IVA",
                Amount =
                    (string.Format(
                        this.getPriceFormat((null != totals ? totals.TaxAmount : 0)),
                        (null != totals ? totals.TaxAmount : 0)).Replace(".", "")).Replace(
                            ",", "")
            };

            aTaxes.Add(oVPOSTax);
            oVPOSTax = new VPOSTax { Name = "Servicio", Amount = "0" };

            aTaxes.Add(oVPOSTax);
            oVPOSTax = new VPOSTax
            {
                Name = "Monto Grava IVA",
                Amount =
                    (string.Format(
                        this.getPriceFormat((null != totals ? totals.TaxableAmountTotal : 0)),
                        (null != totals ? totals.TaxableAmountTotal : 0)).Replace(".", ""))
                        .Replace(",", "")
            };
            //oVPOSTax.Amount = totalplustax;
            aTaxes.Add(oVPOSTax);

            oVPOSTax = new VPOSTax { Name = "Monto No Grava IVA", Amount = "0" };
            aTaxes.Add(oVPOSTax);
            oVposBean.taxes = aTaxes;

            var oVposSend = new VPOSSend(srVposLlaveCifradoPublica, srComercioLlaveFirmaPrivada, vectorId);
            oVposSend.execute(ref oVposBean);

            cipheredDataList.Add(oVposBean.cipheredSessionKey);
            cipheredDataList.Add(oVposBean.cipheredXML);
            cipheredDataList.Add(oVposBean.cipheredSignature);

            return cipheredDataList;
        }

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == type; }) as Charge_V01 ?? new Charge_V01(type, (decimal)0.0);
        }

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal ? "{0:N2}" : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
        }

        #endregion
    }
}