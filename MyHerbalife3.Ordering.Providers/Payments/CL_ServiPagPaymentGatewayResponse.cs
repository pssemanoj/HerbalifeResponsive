namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Xml;
    using HL.Common.Logging;

    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class CL_ServiPagPaymentGatewayResponse : PaymentGatewayResponse
    {
        #region Constants and Fields

        private const string Amount = "amount";

        private const string GateWay = "Agency";

        private const string Order = "orderid";

        private const string PaymentGatewayName = "Servipag";

        private const string XmlAut = "xmlaut";

        private const string XmlConf = "xmlconf";

        #endregion

        #region Constructors and Destructors

        public CL_ServiPagPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        #endregion

        #region Public Properties

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                _configHelper = new ConfigHelper("CL_ServipagPaymentGateWay");
                if (this.QueryValues[GateWay] == PaymentGatewayName)
                {
                    canProcess = true;
                    string merchantID;
                    string serviPagPublicKey;
                    string herbalifePrivateKey;

                    if (this.RootUrl.Contains("local") || this.RootUrl.Contains("qa") || this.RootUrl.Contains("uat"))
                    {
                        merchantID = _configHelper.GetConfigEntry("paymentGatewayCanaldePago"); //CanaldePago
                        serviPagPublicKey =
                            PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysServiPag\\paymentGatewayServiPagPublic.key");
                        herbalifePrivateKey =
                            PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysServiPag\\paymentGatewayHerbalifePrivate.key");
                    }
                    else
                    {
                        merchantID = _configHelper.GetConfigEntry("BetapaymentGatewayCanaldePago"); //CanaldePago    
                        serviPagPublicKey =
                            PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysServiPag\\BETA_paymentGatewayServiPagPublic.key");
                        herbalifePrivateKey =
                            PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysServiPag\\BETA_paymentGatewayHerbalifePrivate.key");
                    }

                    if (this.QueryValues[XmlAut] == "1")
                    {
                     
                        this.IsReturning = false;
                        string xmlSalida;
                        string xmlEntrada = PostedValues["XML"];
                        bool firmaEsValida = false;
                        try
                        {
                            firmaEsValida = ValidaFirmaXml2(xmlEntrada, serviPagPublicKey);

                            if (firmaEsValida)
                            {
                                xmlSalida = GeneraFirmaXml3(firmaEsValida);
                            }
                            else
                            {
                                this.IsApproved = false;
                                xmlSalida = this.GeneraFirmaXml3(firmaEsValida);
                            }
                        }
                        catch (Exception)
                        {
                            this.IsApproved = false;
                            xmlSalida = this.GeneraFirmaXml3(firmaEsValida);
                        }

                        this.SpecialResponse = xmlSalida;
                    }
                    else
                    {
                        if (((this.QueryValues[XmlConf] == "1") || (this.PostedValues[XmlConf] == "1")) && !string.IsNullOrEmpty(PostedValues["xml"]))
                        {
                           
                           this.IsReturning = true;
                            string xmlEntrada = PostedValues["xml"];
                            bool firmaEsValida = false;
  
                            firmaEsValida = ValidaFirmaXml4(xmlEntrada, serviPagPublicKey);
                            if (!firmaEsValida)
                            {
                                this.IsApproved = false;
                                this.LogSecurityWarning(this.GatewayName);
                            }                       
                        }
                        else
                        {
                            canProcess = false;
                        }
                    }
                }

                return canProcess;
            }
        }

        #endregion

        #region Properties

        protected string RootUrl
        {
            get
            {
                return string.Concat(
                    HttpContext.Current.Request.Url.Scheme, "://", HttpContext.Current.Request.Url.DnsSafeHost);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns an instance of <see cref = "RsaKeyParameters" /> based on the supplied PEM or DER file location or the PEM string
        /// </summary>
        /// <param name = "pemFile">Either the file path or the file content as text.</param>
        /// <returns></returns>
        public static RsaKeyParameters GetRsaPublicKey(string pemFile)
        {
            RsaKeyParameters pubkey = null;
            if (!File.Exists(pemFile))
            {
                pubkey = GetPemPublicKey(pemFile);
            }
            else if (pemFile.EndsWith(".pem") || pemFile.EndsWith(".key"))
            {
                pubkey = GetPemPublicKey(File.ReadAllText(pemFile));
            }

            return pubkey;
        }

        public static bool Verify(String msg, String signature, String publicKey)
        {
            RsaKeyParameters remotepubkey = GetRsaPublicKey(publicKey);

            ISigner signer = SignerUtilities.GetSigner("MD5WithRSAEncryption");

            signer.Init(false, remotepubkey);
            var bytes = Convert.FromBase64String(signature);
            var msgBytes = Encoding.UTF8.GetBytes(msg);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(bytes);
        }

        public string GeneraFirmaXml3(bool codRetornoServipag)
        {
            string xml3Salida = "";
            if (codRetornoServipag)
            {
                xml3Salida = "<?xml version='1.0' encoding='ISO-8859-1'?>";
                xml3Salida += "<Servipag>";
                xml3Salida += "<CodigoRetorno>0</CodigoRetorno>";
                xml3Salida += "<MensajeRetorno>Transaccion Exitosa</MensajeRetorno>";
                xml3Salida += "</Servipag>";
            }
            else
            {
                xml3Salida = "<?xml version='1.0' encoding='ISO-8859-1'?>";
                xml3Salida += "<Servipag>";
                xml3Salida += "<CodigoRetorno>1</CodigoRetorno>";
                xml3Salida += "<MensajeRetorno>Transaccion Rechazada</MensajeRetorno>";
                xml3Salida += "</Servipag>";
            }
            return xml3Salida;
        }

   
        public bool ValidaFirmaXml2(string xml2Entrada, string serviPagPublicKey)
        {
            var xml2 = new NodosXml2();

            var xmlDoc = new XmlDocument();
            string cadenaNodos = "";
            string firma = "";
            try
            {
                xmlDoc.LoadXml(xml2Entrada);
                var nodeList = xmlDoc.GetElementsByTagName("Servipag");
                foreach (XmlNode node in nodeList)
                {
                    var xmlElement = (XmlElement)node;
                    firma = xmlElement.GetElementsByTagName("FirmaServipag")[0].InnerText;
                    xml2.IdTrxServipag = xmlElement.GetElementsByTagName("IdTrxServipag")[0].InnerText;
                    xml2.IdTxCliente = xmlElement.GetElementsByTagName("IdTxCliente")[0].InnerText;
                    xml2.FechaPago = xmlElement.GetElementsByTagName("FechaPago")[0].InnerText;
                    xml2.CodMedioPago = xmlElement.GetElementsByTagName("CodMedioPago")[0].InnerText;
                    xml2.FechaContable = xmlElement.GetElementsByTagName("FechaContable")[0].InnerText;
                    xml2.CodigoIdentificador = xmlElement.GetElementsByTagName("CodigoIdentificador")[0].InnerText;
                    xml2.Boleta = xmlElement.GetElementsByTagName("Boleta")[0].InnerText;
                    xml2.Monto = xmlElement.GetElementsByTagName("Monto")[0].InnerText;
                }

                OrderNumber = xml2.IdTxCliente;
                // "IdTrxServipag":
                cadenaNodos += xml2.IdTrxServipag;
                // "IdTxCliente":
                cadenaNodos += xml2.IdTxCliente;
             
                bool retornoValidacionFirma = Verify(cadenaNodos, firma, serviPagPublicKey);

                if (!string.IsNullOrEmpty(xml2.IdTrxServipag) && retornoValidacionFirma)
                {
                    IsApproved = true;
                    this.TransactionCode = xml2.IdTrxServipag;
                    this.AuthorizationCode = xml2.IdTrxServipag;
                    this.CardType = IssuerAssociationType.PaymentGateway;
                    retornoValidacionFirma = true;
                }
                else
                {
                    IsApproved = false;
                    retornoValidacionFirma = false;
                }

                return retornoValidacionFirma;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidaFirmaXml4(string xml4Entrada, string serviPagPublicKey)
        {
            var xml4 = new NodosXml4();

            var xmlDoc = new XmlDocument();
            string cadenaNodos = "";
            string firma = "";
            try
            {
                xmlDoc.LoadXml(xml4Entrada);
                var nodeList = xmlDoc.GetElementsByTagName("Servipag");
                foreach (XmlNode node in nodeList)
                {
                    var xmlElement = (XmlElement)node;
                    firma = xmlElement.GetElementsByTagName("FirmaServipag")[0].InnerText;
                    xml4.IdTrxServipag = xmlElement.GetElementsByTagName("IdTrxServipag")[0].InnerText;
                    xml4.IdTxCliente = xmlElement.GetElementsByTagName("IdTxCliente")[0].InnerText;
                    xml4.Estadopago = xmlElement.GetElementsByTagName("EstadoPago")[0].InnerText;
                    xml4.Mensaje = xmlElement.GetElementsByTagName("Mensaje")[0].InnerText;
                }



                // "IdTrxServipag":
                cadenaNodos += xml4.IdTrxServipag;
                // "IdTxCliente":
                cadenaNodos += xml4.IdTxCliente;
                OrderNumber = xml4.IdTxCliente;
                
                bool retornoValidacionFirma = Verify(cadenaNodos, firma, serviPagPublicKey);
     
                if (!string.IsNullOrEmpty(xml4.IdTrxServipag) && retornoValidacionFirma && (xml4.Estadopago == "0"))
                {
                    this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                    
                    if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                    {
                        this.IsApproved = true;
                        this.TransactionCode = xml4.IdTrxServipag;
                        this.AuthorizationCode = xml4.IdTrxServipag;
                        this.CardType = IssuerAssociationType.PaymentGateway;
                    }
                    else
                    {
                        this.IsApproved = true;
                        this.TransactionCode = xml4.IdTrxServipag;
                        this.AuthorizationCode = xml4.IdTrxServipag;
                        this.CardType = IssuerAssociationType.PaymentGateway;
                    }

                    retornoValidacionFirma = true;
                }
                else
                {
                    this.IsApproved = false;
                    retornoValidacionFirma = false;
                }

                return retornoValidacionFirma;
            }
            catch (Exception ex)
            {
                LoggerTempWireup.WriteInfo(string.Format("Exception in ValidaFirmaXml4. Message- {0}. StackTrace - {1}", ex.Message, ex.StackTrace), "ConfigManager");
                return false;
            }
        }

        #endregion

        #region Methods

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are arbitrary - either one can be received first - very dumb
            Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
            bool canSubmit = false;
            if (IsReturning)
            {
                //This is a Client Redirect
                if (IsApproved)
                {
                    if (Status == PaymentGatewayRecordStatusType.Unknown) //Client is back first (before the POST)
                    {
                        canSubmit = true; //We'll take the transaction
                    }
                    else
                    {
                        canSubmit = false;
                    }
                }
            }
            else
            {
                //This is a Server Post
                canSubmit = IsApproved;
            }

            return canSubmit;
        }

        private static RsaKeyParameters GetPemPublicKey(string key)
        {
            var reader = new PemReader(new StringReader(key));
            object obj = reader.ReadObject();
            var parameters = obj as RsaKeyParameters;
            if (parameters != null)
            {
                return parameters;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Node Definition XML 4
        /// </summary>
        public class NodosXml4
        {
            #region Public Properties

            public string Estadopago { set; get; }

            public string IdTrxServipag { set; get; }

            public string IdTxCliente { set; get; }

            public string Mensaje { set; get; }

            #endregion
        }

        /// <summary>
        /// Node Definition XML 2
        /// </summary>
        private class NodosXml2
        {
            #region Public Properties

            public string Boleta { set; get; }

            public string CodMedioPago { set; get; }

            public string CodigoIdentificador { set; get; }

            public string FechaContable { set; get; }

            public string FechaPago { set; get; }

            public string IdTrxServipag { set; get; }

            public string IdTxCliente { set; get; }

            public string Monto { set; get; }

            #endregion
        }
    }
}