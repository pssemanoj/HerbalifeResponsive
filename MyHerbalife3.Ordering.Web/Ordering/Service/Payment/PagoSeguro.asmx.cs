using System;
using System.Web.Services;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Service.Payment.PagoSeguro
{
    /// <summary>PagoSeguro Web Service implementation</summary>
    [WebService(Namespace = "http://HL.MyHerbalife.Web.Ordering/Service/WebService/Payment/PagoSeguro/1.0")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Transaction : WebService
    {
        private const string InvalidOrMissingOrderNumber =
            "The Order Number supplied {0} could not be found in PaymentGatewayRecords.";

        private const string InvalidOrMissingOrderData =
            "The Order Data for the OrderNumber supplied {0} could not be found in the PaymentGateway Record.";

        private const string TransactionInitiated = "Transaction Initiation request for Order {0}";
        private const string CallerValidationString = "";

        [WebMethod]
        public TransactionInfo GetTransactionData(string refId)
        {
            ValidateCaller();
            var transactionInfo = new TransactionInfo();
            if (string.IsNullOrEmpty(refId))
            {
                LoggerHelper.Error("PagoSeguro.GetTransactionData called with empty Order Number");
                transactionInfo.RefID = "Invalid RefId";
                return transactionInfo;
            }

            LoggerHelper.Info(string.Format(TransactionInitiated, refId));
            SerializedOrderHolder order = OrderProvider.GetPaymentGatewayOrder(refId);
            if (null != order && null != order.Order)
            {
                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, refId, order.DistributorId,
                                                 "PagoSeguroPaymentGateway", PaymentGatewayRecordStatusType.Unknown,
                                                 string.Format(TransactionInitiated, refId));
                var totals = (order.Order as Order_V01).Pricing as OrderTotals_V01;
                if (null != totals)
                {
                    transactionInfo.Monto = Convert.ToDouble(totals.AmountDue);
                    transactionInfo.RefID = refId;
                }
                else
                {
                    LoggerHelper.Error(string.Format(InvalidOrMissingOrderData, refId));
                    transactionInfo.RefID = "Invalid Data for RefId";
                }
            }
            else
            {
                LoggerHelper.Error(string.Format(InvalidOrMissingOrderNumber, refId));
                transactionInfo.RefID = "Invalid RefId";
            }

            return transactionInfo;
        }

        [WebMethod]
        public bool VerifyAvailability(string refId)
        {
            ValidateCaller();
            return true;
        }

        [WebMethod]
        public string SaveTransactionResult(string RefID, string Digits, string CodResp, string Mensaje)
        {
            string result = "02";

            try
            {
                ValidateCaller();
                if (CodResp == "00")
                {
                    result = "01";
                    SubmitOrder(RefID);
                }
            }
            catch
            {
                result = "02";
            }

            return result;
        }

        private void SubmitOrder(string orderNumber)
        {
            string distributorId = string.Empty;
            string locale = string.Empty;
            string error = string.Empty;
            MyHLShoppingCart shoppingCart = null;
            var holder = new SerializedOrderHolder();
            var response = new PagoSeguroPaymentGatewayResponse();
            response.OrderNumber = orderNumber;
            response.IsApproved = true;

            if (OrderProvider.deSerializeAndSubmitOrder(response, out error, out holder))
            {
                locale = holder.Locale;
                if (!String.IsNullOrEmpty(holder.Email))
                {
                    //DistributorProvider.LoadDistributor(holder.DistributorId, holder.Token);
                    shoppingCart = ShoppingCartProvider.GetBasicShoppingCartFromService(holder.ShoppingCartId,
                                                                                        holder.DistributorId, locale);
                    shoppingCart.EmailAddress = holder.Email;
                    shoppingCart.Totals = (holder.Order as Order_V01).Pricing as OrderTotals_V01;
                    EmailHelper.SendEmail(shoppingCart, holder.Order as Order_V01);
                }
                if (null != shoppingCart)
                {
                    ShoppingCartProvider.UpdateInventory(shoppingCart, shoppingCart.CountryCode, shoppingCart.Locale,
                                                         true);
                    shoppingCart.CloseCart();
                }
            }
            else
            {
                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Error, orderNumber, distributorId,
                                                 "PagoSeguroPaymentGateway", PaymentGatewayRecordStatusType.InError,
                                                 error);
            }
        }

        /// <summary>
        ///     Validate that these calls are coming from a recognized and accepted source.
        /// </summary>
        /// <remarks></remarks>
        private void ValidateCaller()
        {
            //Set the CallerValidationString constant to some url and test it against urlReferrer and do tests against any other known headers, etc
            //Throw exception in every unvalidated case.

            //throw new ApplicationException("Invalid Call");
        }
    }

    /// <summary>Simple value object with transaction information</summary>
    public class TransactionInfo
    {
        public TransactionInfo()
        {
            Moneda = "USD";
            ConsultAvailability = false;
            Datos = string.Empty;
            Mensaje = string.Empty;
        }

        public string RefID { get; set; }

        public double Monto { get; set; }

        public string Moneda { get; set; }

        public string Datos { get; set; }

        public string Mensaje { get; set; }

        public bool ConsultAvailability { get; set; }
    }
}