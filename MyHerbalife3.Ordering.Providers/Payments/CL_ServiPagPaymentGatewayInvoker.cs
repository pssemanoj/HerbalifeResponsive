using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml.Linq;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CL_ServiPagPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        #region Constructors and Destructors

        private CL_ServiPagPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("CL_ServiPagPaymentGateway", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns an instance of <see cref="RsaPrivateCrtKeyParameters" /> based on the supplied pemFile or pemstring
        /// </summary>
        /// <param name="pemFile">Either the file path or the file content as text.</param>
        /// <returns></returns>
        public static RsaPrivateCrtKeyParameters GetPrivateKey(String pemFile)
        {
            try
            {
                if (string.IsNullOrEmpty(pemFile))
                {
                    throw new ArgumentNullException("pemFile");
                }

                string privateKey = File.Exists(pemFile) ? File.ReadAllText(pemFile) : pemFile;

                var reader = new PemReader(new StringReader(privateKey));
                RsaPrivateCrtKeyParameters privkey = null;
                var obj = reader.ReadObject();
                if (obj is AsymmetricCipherKeyPair)
                {
                    privkey = (RsaPrivateCrtKeyParameters) ((AsymmetricCipherKeyPair) obj).Private;
                }

                return privkey;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Signs a string with our private key. The string can later be verified with our public key at receiving end.
        /// </summary>
        /// <param name="data">The data that shall be signed.</param>
        /// <param name="privateKey">Our private key, either as a file path reference or raw text.</param>
        /// <returns></returns>
        public static String Sign(String data, String privateKey)
        {
            try
            {
                var privKey = GetPrivateKey(privateKey);
                var sig = SignerUtilities.GetSigner("MD5WithRSAEncryption");

                sig.Init(true, privKey);
                var bytes = Encoding.UTF8.GetBytes(data);
                sig.BlockUpdate(bytes, 0, bytes.Length);
                var signature = sig.GenerateSignature();

                var signedString = Convert.ToBase64String(signature);

                return signedString;
            }
            catch (Exception)
            {
                return null;
            }
        }

       

        public override void Submit()
        {
            string xmlrequest = string.Empty;
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string merchantID;
            string orderNumber = _orderNumber;
            string amount = _orderAmount.ToString().Replace(",", string.Empty);
            string serviPagPublicKey;
            string herbalifePrivateKey;

            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
            {
                merchantID = _configHelper.GetConfigEntry("paymentGatewayCanaldePago"); //CanaldePago
                serviPagPublicKey = ResolveUrl("\\App_data\\PaymentKeysServiPag\\paymentGatewayServiPagPublic.key");
                herbalifePrivateKey =
                    ResolveUrl("\\App_data\\PaymentKeysServiPag\\paymentGatewayHerbalifePrivate.key");
            }
            else
            {
                merchantID = _configHelper.GetConfigEntry("BetapaymentGatewayCanaldePago"); //CanaldePago    
                serviPagPublicKey =
                    ResolveUrl("\\App_data\\PaymentKeysServiPag\\BETA_paymentGatewayServiPagPublic.key");
                herbalifePrivateKey =
                    ResolveUrl("\\App_data\\PaymentKeysServiPag\\BETA_paymentGatewayHerbalifePrivate.key");
            }

            xmlrequest = GenerateXmlRequest(merchantID, orderNumber, herbalifePrivateKey);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();
            var sb = new StringBuilder();
            sb.AppendFormat(@"<html><body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='xml' value='{0}' >", HttpUtility.HtmlEncode(xmlrequest));
            //HttpContext.Current.Server.HtmlEncode(xmlrequest));
            sb.AppendFormat("</form></body></html>");
            string response = sb.ToString();

            LogMessage(
                PaymentGatewayLogEntryType.Request,
                OrderNumber,
                _distributorId,
                _gatewayName,
                PaymentGatewayRecordStatusType.Unknown,
                response);
            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        #endregion

        #region Methods

        protected string GeneraFirma(string codigoCanalPago, string idTxCliente, string privateKeyFile)
        {
            try
            {
                string xmlEnvio = "";
                string cadenaHeader = "";
                cadenaHeader = cadenaHeader + codigoCanalPago;
                cadenaHeader = cadenaHeader + idTxCliente;

                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, "CL_ServiPagPaymentGateway",
                           PaymentGatewayRecordStatusType.Unknown,
                           String.Format("2.- Servipag  Payment Gateway Begin Sing ={0}", privateKeyFile));
                xmlEnvio = Sign(cadenaHeader, privateKeyFile);

                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, "CL_ServiPagPaymentGateway",
                           PaymentGatewayRecordStatusType.Unknown,
                           String.Format("3.- Servipag  Payment Gateway End Sing ={0}", xmlEnvio));

                return xmlEnvio;
            }
            catch (Exception)
            {
                return "Error Generating sign";
            }
        }

        protected string GenerateXmlRequest(string merchantID, string orderID, string herbalifePrivateKey)
        {
            try
            {
                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, "CL_ServiPagPaymentGateway",
                           PaymentGatewayRecordStatusType.Unknown,
                           String.Format("1.- Servipag  Payment Gateway Start GenerateXmlRequest ={0}",
                                         herbalifePrivateKey));
                string xmlsign = string.Empty;
                xmlsign = GeneraFirma(merchantID, orderID, herbalifePrivateKey);

                #region "create Skeleton"

                // Create Skeleton
                var xmlToSend = new XDocument();
                int installments = 1;
                var xDeclaration = new XDeclaration("1.0", "ISO-8859-1", "no");
                xmlToSend.Declaration = xDeclaration;

                var xmlRoot = new XElement("Servipag");
                xmlToSend.Add(xmlRoot);

                var header = new XElement("Header");
                xmlRoot.Add(header);

                // Header.Add(new XElement("FirmaEPS", "C046KTOZwj8xOmq2FYNjU0hRjKq1ylPmUpv25EkaQHDk/xs290wHJR7aceWZZExOn6FXNMBlcxRGl+m+0iUNqrVtx2bkOkd5whk6v0HHIwXhoPDiRMdyHN+rC3j55y94l8Jygl3DhMoq+hvPC9MecdOlutriOYsD8ovWAWRqTKs="));
                header.Add(new XElement("FirmaEPS", xmlsign));
                header.Add(new XElement("CodigoCanalPago", merchantID));
                header.Add(new XElement("IdTxCliente", orderID));
                header.Add(new XElement("FechaPago", DateTime.Now.Date.ToString("yyyy-MM-dd").Replace("-", "")));
                header.Add(
                    new XElement(
                        "MontoTotalDeuda",
                        (string.Format(GetPriceFormat(_orderAmount), _orderAmount)
                               .Replace(".", "")).Replace(",", "")));
                header.Add(new XElement("NumeroBoletas", installments));

                #endregion

                #region "Documentos section"

                //Fill order items section
                for (int i = 0; i < installments; i++)
                {
                    xmlRoot.Add(
                        new XElement(
                            "Documentos",
                            new XElement("IdSubTrx", "1"),
                            new XElement("CodigoIdentificador", orderID),
                            new XElement("Boleta", "1"),
                            new XElement(
                                "Monto",
                                (string.Format(GetPriceFormat(_orderAmount), _orderAmount)
                                       .Replace(".", "")).Replace(",", "")),
                            new XElement(
                                "FechaVencimiento", DateTime.Now.Date.ToString("yyyy-MM-dd").Replace("-", ""))));
                }

                //xmlToSend.Save("C:\\XMLRequest Files\\Servipag\\manuelservipaglocal.xml");
                string xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();

                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, "CL_ServiPagPaymentGateway",
                           PaymentGatewayRecordStatusType.Unknown,
                           String.Format("4.- Servipag  Payment Gateway End XMl Skeleton ={0}", xmlReadyToSend));
                return xmlReadyToSend;
            }
            catch (Exception)
            {
                return "Error Generating the xml parameter";
            }

            #endregion
        }

        protected string GetPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:N0}");
        }

        #endregion
    }
}