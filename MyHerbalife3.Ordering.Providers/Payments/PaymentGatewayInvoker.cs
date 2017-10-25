using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Xml;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ConfigHelper
    {
        private const string ErrorMissingGateWayExternalConfigInfo = "Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}";

        private const string ErrorConfigEntryFoundButHadNoData = "The Configuration Parameter {0} was found in external config, but it had no value";

        private Dictionary<string, string> _configEntries;
        public ConfigHelper(string key)
        {
            _configEntries = GetConfigEntries(key);
        }
        public Dictionary<string, string> GetConfigEntries(string key)
        {
            Dictionary<string, string> dictConfigEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting(key);
            if (!string.IsNullOrEmpty(configEntries))
            {
                dictConfigEntries = (from p in configEntries.Split(new[] { ';' })
                                  where p.Contains('=')
                                  select p.Split(new[] { '=' })).ToDictionary(k => k[0], v => v[1]);
            }
            return dictConfigEntries;
        }
        public string GetConfigEntry(string entryName)
        {
            return GetConfigEntry(entryName, true);
        }

        public string GetConfigEntry(string entryName, bool isRequired)
        {
            string entry = string.Empty;
            try
            {
                entry = _configEntries[entryName];
                if (string.IsNullOrEmpty(entry) && isRequired)
                {
                    throw new ApplicationException(string.Format(ErrorConfigEntryFoundButHadNoData, entryName));
                }
            }
            catch (Exception ex)
            {
                if (isRequired)
                {
                    string error = string.Format(ErrorMissingGateWayExternalConfigInfo, Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    PaymentGatewayInvoker.LogBlindError(error);
                    //LoggerHelper.Error(error, LoggingSource);
                    LoggerHelper.Error(error); //write to eventlog anyway.
                    throw;
                }
            }

            return entry;
        }
    }

    public abstract class PaymentGatewayInvoker
    {
        //abstract methods and properties
        protected const string ClientName = "GlobaleCom";
        public const string PaymentInformation = "PaymentInfo";
        public const string LoggingSource = "PaymentGateway";
        public const string PaymentGateWayOrder = "PaymentGateWayOrder";
        protected const string RootNameSpace = " MyHerbalife3.Ordering.Providers.Payments.";

        private const string ErrorMissingGateWayConfigInfo = "Missing Gateway information in Payments Config for: {0} and paymentMethod: {1}";
        private const string ErrorFailedToInstantiateGatewayInvoker = "Attempt to create a Payment Gateway Invoker of type: {0} failed. The exception is: {1}";
        private const string LogEntry = "<PaymentGatewayRecord RecordType=\"{0}\" OrderNumber=\"{1}\" Distributor=\"{2}\" GatewayName=\"{3}\"><Time>{4}</Time><Status>{5}</Status><Data><![CDATA[{6}]]></Data></PaymentGatewayRecord>";

        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;       
        protected HttpContext _context = HttpContext.Current;
        protected string _country;
        protected string _currency;
        protected string _distributorId;
        protected string _gatewayName;
        protected string _locale;
        protected decimal _orderAmount;
        protected string _orderNumber;
        protected string _paymentMethod;
        protected string _url;
        protected ConfigHelper _configHelper;

        protected PaymentGatewayInvoker(string gatewayName, string paymentMethod, decimal amount)
        {
            _paymentMethod = paymentMethod;
            _orderAmount = amount;
            _currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency;
            _locale = HLConfigManager.Configurations.Locale;
            _country = HLConfigManager.Configurations.Locale.Substring(3);
            
            _distributorId = HttpContext.Current.User.Identity.Name;
            var currentSession = SessionInfo.GetSessionInfo(_distributorId, _locale);
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && currentSession.IsReplacedPcOrder && currentSession.ReplacedPcDistributorOrderingProfile != null)
            {
                _distributorId = currentSession.ReplacedPcDistributorOrderingProfile.Id;
            }
            _gatewayName = gatewayName;
            _configHelper = new ConfigHelper(gatewayName);
        }

        public string OrderNumber
        {
            get { return _orderNumber; }
        }

        protected string RootUrl
        {
            get
            {
                var rootUrlPerfix = Settings.GetRequiredAppSetting("RootURLPerfix","https://");
                return string.Concat(rootUrlPerfix, _context.Request.Url.DnsSafeHost);
            }
        }

        protected DistributorProfileModel DistributorProfileModel
        {
            get
            {
                return ((MembershipUser<DistributorProfileModel>) Membership.GetUser()).Value;
            }
        }

        public abstract void Submit();

        /// <summary>Creates a PaymentGateway Invoker for the appropriate agency</summary>
        /// <param name="paymentMethod"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static PaymentGatewayInvoker Create(string paymentMethod, decimal amount)
        {
            string locale = Thread.CurrentThread.CurrentCulture.Name;
            PaymentGatewayInvoker invoker = null;
            var config = HLConfigManager.Configurations.PaymentsConfiguration;
            try
            {
                if (config.HasPaymentGateway && !string.IsNullOrEmpty(config.PaymentGatewayInvoker))
                {
                    var args = new object[] {paymentMethod, amount};
                    string invokerType = config.PaymentGatewayInvoker;
                    var type = Type.GetType(string.Concat(RootNameSpace, invokerType), true, true);
                    invoker = Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as PaymentGatewayInvoker;
                }
                else
                {
                    //LogBlindError(string.Empty, string.Format(ErrorMissingGateWayConfigInfo, locale, paymentMethod));
                    LoggerHelper.Error(string.Format(ErrorMissingGateWayConfigInfo, locale, paymentMethod));
                }
                invoker.GetOrderNumber();
                return invoker;
            }
            catch (Exception ex)
            {
                //LogBlindError((null != invoker) ? invoker._gatewayName : config.PaymentGatewayInvoker.Replace("Invoker", string.Empty), ex.Message);
                LoggerHelper.Error(string.Format(ErrorFailedToInstantiateGatewayInvoker, config.PaymentGatewayInvoker, ex.Message));
                throw ex;
            }
        }

        protected virtual void GetOrderNumber()
        {
            var request = new GenerateOrderNumberRequest_V01();
            request.Amount = _orderAmount;
            request.Country = _country;
            request.DistributorID = _distributorId;
            var response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                _orderNumber = response.OrderID;
                string orderData = _context.Session[PaymentGateWayOrder] as string;
                _context.Session.Remove(PaymentGateWayOrder);
                int recordId = OrderProvider.InsertPaymentGatewayRecord(_orderNumber, _distributorId, _gatewayName, orderData, _locale);
            }
        }

        public static void LogBlindError(string errorMessage)
        {
            LogBlindError("GatewayUnknown", errorMessage);
        }

        public static void LogBlindError(string paymentGatewayName, string errorMessage)
        {
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Error, string.Empty, string.Empty, paymentGatewayName, PaymentGatewayRecordStatusType.InError, errorMessage);
        }

        public static void LogMessage(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName)
        {
            LogMessage(entryType, orderNumber, distributorId, paymentGatewayName, PaymentGatewayRecordStatusType.Unknown, string.Empty);
        }

        public static void LogMessage(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status)
        {
            LogMessage(entryType, orderNumber, distributorId, paymentGatewayName, status, string.Empty);
        }

        public static void LogMessageWithInfo(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status)
        {
            LogMessageWithInfo(entryType, orderNumber, distributorId, paymentGatewayName, status, string.Empty);
        }

        public static void LogMessage(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status, string data)
        {
            LoggerHelper.Error(string.Format(LogEntry, entryType.ToString(), orderNumber, distributorId, paymentGatewayName, XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local), status, data));
            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, data, entryType, status);
        }

        public static void LogMessageWithInfo(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status, string data)
        {
            LoggerHelper.Info(string.Format(LogEntry, entryType.ToString(), orderNumber, distributorId, paymentGatewayName, XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local), status, data));
            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, data, entryType, status);
        }

        public static string ResolveUrl(string originalUrl)
        {
            if (originalUrl != null && originalUrl.Trim() != "")
            {
                if (originalUrl.StartsWith("/"))
                {
                    originalUrl = "~" + originalUrl;
                }
                else
                {
                    originalUrl = "~/" + originalUrl;
                }

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if (originalUrl == null)
            {
                return null;
            }
            if (originalUrl.IndexOf("://") != -1)
            {
                return originalUrl;
            }
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                }

                return newUrl;
            }
            return originalUrl;
        }

        public static PaymentGatewayResponse CheckOrderStatus(string paymentMethod, string orderNumber)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var response = CN_99BillPaymentGatewayInvoker.GetOrderStatus(orderNumber);
                return response;
            }

            return null;
        }

        public void SendPendingEmail()
        {
            try
            {
                var cmmSVCP = new CommunicationSvcProvider();
                cmmSVCP.SendEmailConfirmation(OrderNumber, "Processing");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error sending email, order:{0} : {1}", OrderNumber, ex.Message));
            }
        }

    }
}