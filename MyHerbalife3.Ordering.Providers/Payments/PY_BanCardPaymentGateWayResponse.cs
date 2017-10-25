namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using HL.Common.Configuration;
    using HL.Common.Logging;
    using System.Web.Script.Serialization;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class PY_BanCardPaymentGateWayResponse : PaymentGatewayResponse
    {
        private Dictionary<string, string> _configEntries;
        private const string CustomResponse = "<html><head><title>200</title></head><body>200</body></html>";
        private const string ResponseCode = "00"; //approved
        private const string GateWay = "Agency";
        private const string Order = "OrderId";
        private const string Info = "Info";
        private const string Amount = "amount";
        private const string AgencyTransactionCode = "Idtrn";
        private const string Respuesta = "operation";
        private const string AuthResultCode = "authcode";
        private const string PaymentGatewayName = "Bancard";
        private const string PaymentGatewayResponseType = "rsp";
        private const string PaymentGatewayResponseRdtc = "rsprdtc";
        private const string PaymentGatewayResponseBsc = "rspbsc";

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if ((QueryValues[GateWay] == PaymentGatewayName) && (QueryValues[PaymentGatewayResponseType] == PaymentGatewayResponseRdtc))
                {
                    canProcess = true;
                    OrderNumber = QueryValues[Order];
                    if (string.IsNullOrEmpty(OrderNumber))
                    {
                        LogSecurityWarning(PaymentGatewayName);
                        return canProcess;
                    }

                    this.IsReturning = true;
                    this.CanSubmitIfApproved = false;
                    this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                    this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted);
                    return canProcess;
                }
                else
                {
                    if ((QueryValues[GateWay] == PaymentGatewayName) && (QueryValues[PaymentGatewayResponseType] == PaymentGatewayResponseBsc))
                    {
                        canProcess = true;
                        HttpContext context = HttpContext.Current;
                        HttpRequest request = context.Request;
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        System.IO.Stream body = request.InputStream;
                        System.Text.Encoding encoding = context.Request.ContentEncoding;
                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                        if (context.Request.ContentType != null)
                        {
                            string jsonInfo = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonInfo))
                            {
                                try
                                {

                                    RootObject collection = new RootObject();
                                    collection = serializer.Deserialize<RootObject>(jsonInfo);
                                    if (collection.operation.shop_process_id != null)
                                    {
                                        OrderNumber = serializeOrderNumber(collection.operation.shop_process_id);
                                        PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, "PY_BanCardPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("jsonInfo=> {0}", jsonInfo));
                                        if (collection.operation.response_code == ResponseCode)
                                        {
                                            IsApproved = true;
                                            CanSubmitIfApproved = true;
                                            TransactionCode = collection.operation.ticket_number;
                                            AuthorizationCode = collection.operation.authorization_number;
                                            this.SpecialResponse = CustomResponse;
                                        }
                                        else
                                        {
                                            IsApproved = false;
                                            CanSubmitIfApproved = false;
                                            this.SpecialResponse = "Invalid";
                                        }
                                    }
                                }
                                catch (InvalidOperationException ex)
                                {
                                    LogJsonWarningSerialization(PaymentGatewayName, ex.Message.ToString());
                                }

                                catch (Exception exa)
                                {
                                    LogJsonWarningSerialization(PaymentGatewayName, exa.Message.ToString());
                                }
                            }
                        }
                        else
                        {
                            LogJsonWarning(PaymentGatewayName);
                        }
                    }
                }
                return canProcess;
            }
        }

        public PY_BanCardPaymentGateWayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("PY_BanCardPaymentGateWay");
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

        protected void LogJsonWarning(string paymentGateway)
        {
            PaymentGatewayInvoker.LogBlindError(
                paymentGateway,
                string.Format(
                    "JsonWarning: Json Posted data are null :\r\nPosted:{0}\r\nQueryString: {1}",
                    this.GetFormData(),
                    HttpContext.Current.Request.QueryString));
        }

        protected void LogJsonWarningSerialization(string paymentGateway, string jsondata)
        {
            PaymentGatewayInvoker.LogBlindError(
                paymentGateway,
                string.Format(
                    "JsonWarningSerialization: Json Serialization fail  :\r\nPosted:{0}\r\nQueryString: {1}\r\nJson: {2}",
                    this.GetFormData(),
                    HttpContext.Current.Request.QueryString, jsondata));
        }

        protected string serializeOrderNumber(string orderNumber)
        {
            int currentLenght;
            string orderNumberFormat = "PY";
            currentLenght = orderNumber.Length;
            for (int i = currentLenght; i < 8; i++)
                orderNumberFormat = orderNumberFormat + "0";

            orderNumberFormat = orderNumberFormat + orderNumber;
            return orderNumberFormat;
        }
    }
}

public class SecurityInformation
{
    public string customer_ip { get; set; }
    public string card_source { get; set; }
    public string card_country { get; set; }
    public string version { get; set; }
    public string risk_index { get; set; }
}

public class Operation
{
    public string token { get; set; }
    public string shop_process_id { get; set; }
    public string response { get; set; }
    public string response_details { get; set; }
    public string extended_response_description { get; set; }
    public string currency { get; set; }
    public string amount { get; set; }
    public string authorization_number { get; set; }
    public string ticket_number { get; set; }
    public string response_code { get; set; }
    public string response_description { get; set; }
    public SecurityInformation security_information { get; set; }
}

public class RootObject
{
    public Operation operation { get; set; }
}

