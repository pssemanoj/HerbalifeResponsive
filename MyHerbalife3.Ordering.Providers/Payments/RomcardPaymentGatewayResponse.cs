using System;
using System.Collections.Generic;
using System.Threading;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class RomcardPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "TERMINAL";
        private const string Order = "ORDER";
        private const string AuthResult = "MESSAGE";
        private const string AuthorizationNumber = "APPROVAL";
        private const string TransactionNumber = "INT_REF";

        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;
        private Dictionary<string, string> _configEntries;

        public RomcardPaymentGatewayResponse()
        {
            base.GatewayName = GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                try
                {
                    string terminal = GetConfigEntry("paymentGatewayTerminal");

                    if (QueryValues[GateWay] == terminal)
                    {
                        canProcess = true;
                        string mac = string.Concat(QueryValues[GateWay].Length, QueryValues[GateWay],
                                                   QueryValues["TRTYPE"].Length, QueryValues["TRTYPE"],
                                                   QueryValues["ORDER"].Length, QueryValues["ORDER"],
                                                   QueryValues["AMOUNT"].Length, QueryValues["AMOUNT"],
                                                   QueryValues["CURRENCY"].Length, QueryValues["CURRENCY"],
                                                   QueryValues["DESC"].Length, QueryValues["DESC"],
                                                   QueryValues["ACTION"].Length, QueryValues["ACTION"],
                                                   QueryValues["RC"].Length, QueryValues["RC"],
                                                   QueryValues["MESSAGE"].Length, QueryValues["MESSAGE"],
                                                   QueryValues["RRN"].Length, QueryValues["RRN"],
                                                   QueryValues["INT_REF"].Length, QueryValues["INT_REF"],
                                                   QueryValues["APPROVAL"].Length, QueryValues["APPROVAL"],
                                                   QueryValues["TIMESTAMP"].Length, QueryValues["TIMESTAMP"],
                                                   QueryValues["NONCE"].Length, QueryValues["NONCE"]);

                        string encryptionKey = _config.PaymentGatewayEncryptionKey;
                        string calculatedPSign = RomcardPaymentGatewayInvoker.Generate_PSign(encryptionKey, mac);

                        if (calculatedPSign == QueryValues["P_SIGN"])
                        {
                            string result = QueryValues[AuthResult];
                            OrderNumber = "R" + QueryValues[Order];
                            IsApproved = result.ToUpper() == "APPROVED";
                            canProcess = true;
                            AuthorizationCode = QueryValues[AuthorizationNumber];
                            TransactionCode = QueryValues[TransactionNumber];
                            GatewayName = "Romcard";
                        }
                        else
                        {
                            LogSecurityWarning(GatewayName);
                        }
                    }

                    return canProcess;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("Romcard Payment Gateway Response exception for: order number {0} {1}",
                                      OrderNumber, ex.Message));
                }

                return canProcess;
            }
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("RomcardPaymentGateway");
            if (!string.IsNullOrEmpty(configEntries))
            {
                var allEntries = configEntries.Split(new[] {';'});
                if (null != allEntries && allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        var item = entry.Split(new[] {'='});
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
                        throw new ApplicationException(
                            string.Format(
                                "The Configuration Parameter {0} was found in external config, but it had no value",
                                entryName));
                    }
                }
                catch (Exception ex)
                {
                    string error =
                        string.Format(
                            "Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}",
                            Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }
    }
}