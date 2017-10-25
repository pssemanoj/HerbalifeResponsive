using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    using HL.Common.Configuration;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class CO_PsePaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "ref_venta";
        private const string AuthResult = "codigo_respuesta_pol"; // estado_pol = 4, what does it mean?
        private const string GateWay = "Agency";
        private const string AuthorizationNumber = "codigo_autorizacion";
        private const string PaymentGateWayName = "psepayment";
        private const string cardType = "medio_pago";
        private const string cardNumber = "numero_visible";
        private const string UserId = "usuario_id";
        private const string Description = "descripcion";
        private const string Herbalife = "Herbalife";
        private const string Pse = "25";
        private Dictionary<string, string> configEntries;

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                this.ReloadShoppingCart = true;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;
                    string appId = this.GetConfigEntry("paymentGatewayApplicationId");
                    NameValueCollection requestValues = new NameValueCollection();

                    //Is this the user coming back? (Order number is in the QueryString)
                    if (!string.IsNullOrEmpty(QueryValues[Order]) && !string.IsNullOrEmpty(QueryValues[UserId]))
                    {
                        requestValues = QueryValues;
                        IsReturning = true;
                    }
                    else
                    {
                        //It's a blind POST from the gateway
                        requestValues = PostedValues;
                        if (appId != PostedValues[UserId] || PostedValues[Description] != Herbalife)
                        {
                            LogSecurityWarning(this.GatewayName);
                            return true;
                        }
                    }
                    if (string.IsNullOrEmpty(requestValues[AuthResult]))
                    {
                        AuthResultMissing = true;
                    }
                    else
                    {
                        OrderNumber = requestValues[Order];
                        IsApproved = (requestValues[AuthResult] == "1"); // 1 means approved, 15 means processing, other mean declined 
                        IsCancelled = (requestValues[AuthResult] == "2" || PostedValues[AuthResult] == "3");
                        IsPendingTransaction = (requestValues[AuthResult] == "15");
                    }

                    //Get cardtpye and cardnumber from the POST or QueryString 
                    AuthorizationCode = requestValues[AuthorizationNumber];
                    CardNumber = requestValues[cardNumber];

                    // Wich CardType did we get ?  Assign it to the out variable. 
                    string theCardType = requestValues[cardType];
                    switch (theCardType)
                    {
                        case Pse:
                            {
                                CardType = IssuerAssociationType.PaymentGateway;
                                break;
                            }
                        default:
                            {
                                CardType = IssuerAssociationType.Visa;
                                break;
                            }
                    }
                }

                return canProcess;
            }
        }

        public CO_PsePaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are arbitrary - either one can be received first - very dumb
            //Longhanding this logic for clarity
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
                else
                {
                    Status = PaymentGatewayRecordStatusType.Declined;
                }
            }
            else
            {
                //This is a Server Post
                if (IsApproved)
                {
                    if (Status == PaymentGatewayRecordStatusType.Unknown) //We're back first (before the Redirect)
                    {
                        canSubmit = true; //We'll take the transaction
                    }
                    else
                    {
                        canSubmit = (Status == PaymentGatewayRecordStatusType.ApprovalPending); //We must otherwise only take a Pending transaction that is subsequently now being approved
                    }
                }
            }

            return canSubmit;
        }

        private string GetConfigEntry(string entryName)
        {
            this.configEntries = new Dictionary<string, string>();
            string entries = Settings.GetRequiredAppSetting("CO_PsePaymentGateway");
            if (!string.IsNullOrEmpty(entries))
            {
                string[] allEntries = entries.Split(new[] { ';' });
                if (allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        string[] item = entry.Split(new[] { '=' });
                        if (item.Length > 1)
                        {
                            this.configEntries.Add(item[0], item[1]);
                        }
                    }
                }
            }

            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                try
                {
                    entryVal = this.configEntries[entryName];
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
                            System.Threading.Thread.CurrentThread.CurrentCulture.Name,
                            entryName,
                            ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }
    }
}




