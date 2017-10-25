using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OrderPaymentStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Pos machine receives request information 
            var orderid = Request["orderId"] ?? string.Empty;
            // Request time, the format is yyyyMMddHHmmss 
            var reqtime = Request["reqTime"] ?? string.Empty;
            // Packet inspection to check the string 
            var mac = Request["MAC"] ?? string.Empty;
            // Extended Field 1 
            var ext1 = Request["ext1"] ?? string.Empty;
            // Extended Field 1 
            var ext2 = Request["ext2"] ?? string.Empty;

            var merchantId = Request["merchantId"] ?? string.Empty;

            if (string.IsNullOrEmpty(orderid)) return;

            var signMsgVal = "";
            signMsgVal = AppendParam1(signMsgVal, "orderId", orderid);
            signMsgVal = AppendParam1(signMsgVal, "reqTime", reqtime);
            signMsgVal = AppendParam1(signMsgVal, "ext1", ext1);
            signMsgVal = AppendParam1(signMsgVal, "ext2", ext2);

            var resqTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            var responseCode = "";
            decimal amt = 0;
            var address = "";
            // Pos machine messages for data validation 
            var msMac = CerRsaVerifySignature(signMsgVal, mac, Server.MapPath("~/App_Data/PaymentsKey99Bill/mgw.cer"));
            if (msMac || Settings.GetRequiredAppSetting("PayByPhoneQATesting", false))
            {

                var response = OrderProvider.GetPBPPaymentServiceDetail(string.Empty, orderid);
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    switch (response.PaymentStatus)
                    {
                        case "CNPending":
                            amt = response.AmountDue;
                            address = response.DeliveryAddress;
                            responseCode = "00"; // trading success 
                            break;
                        case "CNReadyToProcess":
                            responseCode = "94"; // this order has been paid 
                            break;
                        case "CNCancelled":
                            responseCode = "55"; // the transaction has been canceled. 
                            break;
                        default:
                            responseCode = "93"; // system abnormalities, since only four states 
                            break;
                    }

                }
                else
                {
                    responseCode = "93"; // did not find this order 
                }

            }
            else
            {
                responseCode = "56";
            }

            var signMsgXml = "<MessageContent>"
                             + "<reqTime>" + reqtime + "</reqTime>"
                             + "<respTime>" + resqTime + "</respTime>"
                             + "<responseCode>" + responseCode + "</responseCode>"
                             + "<message>"
                             + "<orderId>" + orderid + "</orderId>"
                             + "<merchantId>" + merchantId + "</merchantId>"
                             + "<merchantName>lily</merchantName>"
                             + "<amt>" + amt + "</amt>"
                             + "<amt2></amt2>"
                             + "<amt3></amt3>"
                             + "<amt4></amt4>"
                             + "<ext>"
                             + "<userdata1>"
                             + "<value>" + address + "</value>"
                             + "<chnName>订单号</chnName>"
                             + "</userdata1>"
                             + "<userdata2>"
                             + "<value>" + orderid + "</value>"
                             + "<chnName>收件人地址</chnName>"
                             + "</userdata2>"
                             + "</ext>"
                             + "<desc>快钱</desc>"
                             + "</message>"
                             + "</MessageContent>";

            try
            {
                string returnData = null;
                var configHelper = new ConfigHelper("CN_99BillPaymentGateway");
                var pbpCertificatePwd = configHelper.GetConfigEntry("PNPCertificatePWD");
                var bytes = Encoding.UTF8.GetBytes(signMsgXml);
                var pfxName = configHelper.GetConfigEntry("PNPCertificateName");
                var pfxPath = "~/App_Data/PaymentsKey99Bill/" + pfxName;
                var cert =
                    new X509Certificate2(
                        HttpContext.Current.Server.MapPath(pfxPath),
                        pbpCertificatePwd, X509KeyStorageFlags.MachineKeySet);
                var rsapri = (RSACryptoServiceProvider)cert.PrivateKey;
                var f = new RSAPKCS1SignatureFormatter(rsapri);
                f.SetHashAlgorithm("SHA1");
                var sha = new SHA1CryptoServiceProvider();
                var result = sha.ComputeHash(bytes);
                returnData = Convert.ToBase64String(f.CreateSignature(result));

                var str = "";
                str += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                str += "<ResponseMessage>";
                str += "<MAC>";
                str += returnData;
                str += "</MAC>";
                str += signMsgXml;
                str += "</ResponseMessage>";

                var bytes1 = Encoding.GetEncoding("UTF-8").GetBytes(str);
                var hr = HttpContext.Current.Response;
                hr.Clear();
                hr.OutputStream.Write(bytes1, 0, bytes1.Length);
                hr.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        private static bool CerRsaVerifySignature(string originalString, string signatureString, string pubkeyPath)
        {
            var originalByte = System.Text.Encoding.UTF8.GetBytes(originalString);
            var signatureByte = Convert.FromBase64String(signatureString);
            var x509 = new X509Certificate2(pubkeyPath);
            var rsapub = (RSACryptoServiceProvider) x509.PublicKey.Key;
            rsapub.ImportCspBlob(rsapub.ExportCspBlob(false));
            var f = new RSAPKCS1SignatureDeformatter(rsapub);
            f.SetHashAlgorithm("SHA1");
            var sha = new SHA1CryptoServiceProvider();
            var hashData = sha.ComputeHash(originalByte);

            return f.VerifySignature(hashData, signatureByte);
        }

        private static string AppendParam1(string returnStr, string paramId, string paramValue)
        {
            if (returnStr != "")
            {
                if (!string.IsNullOrEmpty(paramValue))
                {
                    returnStr += paramId + "=" + paramValue;
                }
            }
            else
            {
                if (paramValue != "")
                {
                    returnStr = paramId + "=" + paramValue;
                }
            }
            return returnStr;
        }
    }
}