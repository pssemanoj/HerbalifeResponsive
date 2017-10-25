using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    class PolCardPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "order_id";
        private const string GateWay = "A";
        private const string MerchantAccount = "pos_id";

        private Dictionary<string, string> _configEntries; 

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == "PC-A" || QueryValues[GateWay] == "PC-D")
                {
                    canProcess = true;
                    OrderNumber = HttpContext.Current.Session[PaymentGatewayInvoker.PaymentGateWayOrder] as string;
                    HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentGateWayOrder);

                    //Validate this is not a spoof
                    string polOrderNumber = PostedValues[Order];
                    string polMerchant = PostedValues[MerchantAccount];
                    if (OrderNumber == polOrderNumber && !string.IsNullOrEmpty(polMerchant))
                    {
                        IsApproved = polMerchant == this.GetConfigEntry("merchantAccount") && QueryValues[GateWay] == "PC-A";
                    }
                    else
                    {
                        LogSecurityWarning(this.GatewayName);
                    }
                }
                else
                {
                    base.AuthResultMissing = true;
                }
                return canProcess;
            }
        }

        public PolCardPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = HL.Common.Configuration.Settings.GetRequiredAppSetting("PolCardPaymentGateway");
            if (!string.IsNullOrEmpty(configEntries))
            {
                string[] allEntries = configEntries.Split(new char[] { ';' });
                if (null != allEntries && allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        string[] item = entry.Split(new char[] { '=' });
                        if (null != item && item.Length > 1)
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

    }
}
