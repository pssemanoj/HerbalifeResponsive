using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Xml.Linq;
    using HL.Common.Configuration;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class UY_OcaPaymentGateWayResponse : PaymentGatewayResponse
    {
        private Dictionary<string, string> _configEntries;
        private const string GateWay = "Agency";
        private const string Order = "OrderId";
        private const string Info = "Info";
        private const string Amount = "amount";
        private const string AgencyTransactionCode = "Idtrn";
        private const string Respuesta = "Rsp";
        private const string AuthResultCode = "authcode";
        private const string PaymentGatewayName = "ocapayment";


        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                HttpContext context = HttpContext.Current;
                HttpRequest request = context.Request;
                string returnUrlConfirmToAgency = GetConfigEntry("paymentGatewayConfirmToAgencyUrl");


                if (QueryValues[GateWay] == PaymentGatewayName || PostedValues[GateWay] == PaymentGatewayName)
                {
                    canProcess = true;
                    OrderNumber = QueryValues[Order];
                    string presentacionRequestFormatString = "?Idtrn={0}&Nrocom={1}&Nroterm={2}&Moneda={3}&Importe={4}&Plan={5}&Info={6}&Urlresponse={7}&Tconn={8}";
                    string presentacionRequestString = SetPresentacionTransactionData(presentacionRequestFormatString);


                    if (string.IsNullOrEmpty(PostedValues[AgencyTransactionCode]))
                    {
                        base.AuthResultMissing = true;
                    }
                    else
                    {
                        IsApproved = PostedValues[Respuesta] == "0";
                    }

                    if (IsApproved)
                    {
                        try
                        {
                            string Ocaresponse = OrderProvider.SendOcaConfirmationServiceRequest(presentacionRequestString);
                            SetOcaConfirmation(Ocaresponse);
                        }
                        catch
                        {

                        }
                    }
                }

                base.AuthorizationCode = PostedValues[AgencyTransactionCode];

                return canProcess;
            }
        }

        public UY_OcaPaymentGateWayResponse()
        {
            base.GatewayName = this.GatewayName;
        }


        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("UY_OcaPaymentGateWay");
            if (!string.IsNullOrEmpty(configEntries))
            {
                string[] allEntries = configEntries.Split(new char[] { ';' });
                if (allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        string[] item = entry.Split(new char[] { '=' });
                        if (item.Length > 1)
                        {
                            _configEntries.Add(item[0], item[1]);
                        }
                    }
                }
            }

            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                try
                {
                    entryVal = _configEntries[entryName];
                    if (string.IsNullOrEmpty(entryVal))
                    {
                        throw new ApplicationException(string.Format("The Configuration Parameter {0} was found in external config, but it had no value", entryName));
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}", System.Threading.Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }


        protected string RootUrl
        {
            get { return string.Concat(HttpContext.Current.Request.Url.Scheme, "://", HttpContext.Current.Request.Url.DnsSafeHost); }
        }

        protected void SetOcaConfirmation(string xmlConfirmation)
        {

            XDocument xmlToSend = XDocument.Parse(xmlConfirmation);
            XElement xmlResponse = xmlToSend.Element("presentacion");
            string statusTransaction = xmlResponse.Element("Rsp").Value;
            string codeTransaction = xmlResponse.Element("Nrorsv").Value;
            string extraInfo = xmlResponse.Element("Info").Value;

            PaymentGatewayInvoker.LogMessage(
            PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, GatewayName,
            PaymentGatewayRecordStatusType.Unknown, String.Format("1 Deserialize response confirmation from the service= {0}   ", xmlToSend.ToString()));

            if (statusTransaction == "0")
            {
                IsApproved = true;
                AuthorizationCode = codeTransaction;
                TransactionCode = PostedValues[AgencyTransactionCode];
                CardType = IssuerAssociationType.Oca;
            }
            else
            {
                IsApproved = false;
                AuthorizationCode = "0";
                TransactionCode = PostedValues[AgencyTransactionCode];
            }
        }

        private string SetPresentacionTransactionData(string formatString)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] listinfo = PostedValues["Info"].Split('&');
            foreach (string parameter in listinfo)
            {
                string[] desserialiceParameter = parameter.Split('=');
                string parameterKey;
                string parameterValue;
                parameterKey = desserialiceParameter[0];
                parameterValue = desserialiceParameter[1];
                dictionary.Add(parameterKey, parameterValue);
            }

            string agencyRequest = string.Format(formatString, dictionary["A1"], dictionary["A2"], dictionary["A3"], dictionary["A4"], dictionary["A5"], dictionary["A6"],
                                                              dictionary["A7"], string.Format("{0}?Agency=ocapayment&OrderId={1}", dictionary["A8"], QueryValues[Order]), dictionary["A9"]);


            return agencyRequest;
        }


    }
}
