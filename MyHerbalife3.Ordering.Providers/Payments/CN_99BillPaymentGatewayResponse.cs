using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    /// <summary>
    /// CN_99BillPaymentGatewayResponse
    /// </summary>
    public partial class CN_99BillPaymentGatewayResponse : PaymentGatewayResponse
    {

        #region Constants and Fields
        private const string Merchantacctid = "merchantAcctId";
        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;
        private const string PaymentGateWayName = "99Bill";
        #endregion

        #region Public Properties

        public string GatewayAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanProcess
        {
            get
            {
                var canProcess = false;
                try
                {
                    _configHelper = new ConfigHelper("CN_99BillPaymentGateway");
                    var merchantId = _configHelper.GetConfigEntry("paymentGatewayMerchantdId");
                    if (QueryValues[Merchantacctid] == merchantId)
                    {
                        OrderNumber = QueryValues["orderId"].Trim();
                        if (string.IsNullOrEmpty(OrderNumber))
                        {
                            LogSecurityWarning(PaymentGateWayName);
                            return canProcess;
                        }

                        var status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                        if (status != PaymentGatewayRecordStatusType.Unknown)
                        {
                            return canProcess;
                        }
                        canProcess = true;
                        var version = QueryValues["version"] != null ? QueryValues["version"].Trim() : string.Empty;
                        var language = QueryValues["language"] != null ? QueryValues["language"].Trim() : string.Empty;
                        var signType = QueryValues["signType"] != null ? QueryValues["signType"].Trim() : string.Empty;
                        var payType = QueryValues["payType"] != null ? QueryValues["payType"].Trim() : string.Empty;
                        var bankId = QueryValues["bankId"] != null ? QueryValues["bankId"].Trim() : string.Empty;
                        var orderTime = QueryValues["orderTime"] != null
                            ? QueryValues["orderTime"].Trim()
                            : string.Empty;
                        var orderAmount = QueryValues["orderAmount"] != null
                            ? QueryValues["orderAmount"].Trim()
                            : string.Empty;
                        var dealId = QueryValues["dealId"] != null ? QueryValues["dealId"].Trim() : string.Empty;
                        var bankDealId = QueryValues["bankDealId"] != null
                            ? QueryValues["bankDealId"].Trim()
                            : string.Empty;
                        var dealTime = QueryValues["dealTime"] != null ? QueryValues["dealTime"].Trim() : string.Empty;
                        var payAmount = QueryValues["payAmount"] != null
                            ? QueryValues["payAmount"].Trim()
                            : string.Empty;
                        GatewayAmount = payAmount;
                        var fee = QueryValues["fee"] != null ? QueryValues["fee"].Trim() : string.Empty;
                        var ext1 = QueryValues["ext1"] != null ? QueryValues["ext1"].Trim() : string.Empty;
                        var ext2 = QueryValues["ext2"] != null ? QueryValues["ext2"].Trim() : string.Empty;
                        var payResult = QueryValues["payResult"] != null
                            ? QueryValues["payResult"].Trim()
                            : string.Empty;
                        var errCode = QueryValues["errCode"] != null ? QueryValues["errCode"].Trim() : string.Empty;
                        var signMsg = QueryValues["signMsg"] != null ? QueryValues["signMsg"].Trim() : string.Empty;
                        var bindCard = QueryValues["bindCard"] != null ? QueryValues["bindCard"].Trim() : string.Empty;
                        var bindMobile = QueryValues["bindMobile"] != null
                            ? QueryValues["bindMobile"].Trim()
                            : string.Empty;
                        var merchantSignMsgVal = "";
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal,
                            "merchantAcctId", merchantId);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "version",
                            version);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "language",
                            language);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "signType",
                            signType);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "payType",
                            payType);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "bankId",
                            bankId);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "orderId",
                            OrderNumber);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "orderTime",
                            orderTime);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal,
                            "orderAmount", orderAmount);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "bindCard",
                            bindCard);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "bindMobile",
                            bindMobile);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "dealId",
                            dealId);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "bankDealId",
                            bankDealId);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "dealTime",
                            dealTime);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "payAmount",
                            payAmount);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "fee", fee);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "ext1", ext1);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "ext2", ext2);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "payResult",
                            payResult);
                        merchantSignMsgVal = CN_99BillPaymentGatewayInvoker.AppendParam(merchantSignMsgVal, "errCode",
                            errCode);

                        if (VerifySignature(merchantSignMsgVal, signMsg))
                        {
                            IsApproved = payResult == "10"; // 10 is successful
                            CanSubmitIfApproved = true;
                            // save below to order payment able - PayChannel and PaymentID
                            SpecialResponse = string.Format("{0},{1}", dealId, bankDealId);
                        }
                        else
                        {
                            AuthResultMissing = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("99Bill Payment Gateway Response exception for: order number {0} {1}",
                                      OrderNumber, ex.Message));
                }
                return canProcess;
            }
        }
        /// <summary>
        ///  pass additional info
        /// </summary>
        /// <param name="holder"></param>
        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            if ((holder.Order as Order_V01) == null || SpecialResponse == null)
                return;
            string[] bankInfo = SpecialResponse.Split(',');
            if (bankInfo.Length == 2 && (holder.Order as Order_V01).Payments != null && ((holder.Order as Order_V01).Payments).Count > 0 )
            {
                var btPayment = holder.BTOrder.Payments[0] ;
                var orderPayment = (holder.Order as Order_V01).Payments[0] as CreditPayment_V01;
                btPayment.PaymentCode = orderPayment.TransactionType = "DC";
                orderPayment.Card.IssuingBankID = bankInfo[0]; // this is PayChannel
                orderPayment.AuthorizationMerchantAccount = bankInfo[1];  // this is PaymentID
            }
        }

        public bool VerifySignature(string signMsgVal, string signMsg)
        {
            bool result = false;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(signMsgVal);
            byte[] signatureByte = Convert.FromBase64String(signMsg);
            var publicCertName = _configHelper.GetConfigEntry("gatewayPublicCertName");

            var storex = new X509Store(StoreLocation.LocalMachine);
            storex.Open(OpenFlags.ReadOnly);
            var certificates = storex.Certificates.Find(X509FindType.FindBySubjectName, publicCertName, false);

            if (certificates != null && certificates.Count > 0 && certificates[0] != null)
            {
                var cert = certificates[0];

                RSACryptoServiceProvider rsapri = (RSACryptoServiceProvider)cert.PublicKey.Key;
                rsapri.ImportCspBlob(rsapri.ExportCspBlob(false));
                RSAPKCS1SignatureDeformatter f = new RSAPKCS1SignatureDeformatter(rsapri);
                byte[] computedHash;
                f.SetHashAlgorithm("SHA1");
                SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
                computedHash = sha.ComputeHash(bytes);

                result = f.VerifySignature(computedHash, signatureByte);
            }

            return result;
        }
        #endregion


    }
}
