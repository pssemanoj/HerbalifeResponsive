using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using LoggerHelper = HL.Common.Logging.LoggerHelper;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public enum ThreeDPaymentStatus
    {
        Unknown = 0,
        EnrollmentChecked = 1,
        ThreeDPopupReturned = 2,
        VerificationChecked = 3,
        OrderCreated = 4,
        Submitted = 5,
        Declined = 6,
        Errored = 7
    }
    public static partial class ThreeDPaymentProvider
    {
        #region Public
        public static string GenerateOrderNumberFor3DPayment(decimal amount, string countryCode, string distributorId)
        {
            string orderNumber = string.Empty;
            var request = new GenerateOrderNumberRequest_V01
            {
                Amount = amount,
                Country = countryCode,
                DistributorID = distributorId
            };
            var response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                orderNumber = response.OrderID;
            }
            else
            {
                LoggerHelper.Error("Unable to generate an order number");
            }
            return orderNumber;
        }

        public static ThreeDSecuredCreditCard Check3DSecuredEnrollment(PaymentCollection payments, string countryCode, string orderNumber, string distributorId, string locale, string rootUrl)
        {
            var threeDSecuredCard = new ThreeDSecuredCreditCard();
            if (payments.Count == 0)
            {              
                return threeDSecuredCard;
            }

            // 3D Secured only allows 1 credit card
            var payment = payments[0];
            var creditCardPayment = payment as CreditPayment_V01;

            if (creditCardPayment != null)
            {
                string cardNumber = creditCardPayment.Card.AccountNumber.Trim();
                string cardType = CreditCard.CardTypeToHPSCardType(creditCardPayment.Card.IssuerAssociation);
                string cardExpirationMonth = creditCardPayment.Card.Expiration.Month.ToString(CultureInfo.InvariantCulture);
                string cardExpirationYear = creditCardPayment.Card.Expiration.Year.ToString(CultureInfo.InvariantCulture);
                string currencyCode = creditCardPayment.Currency;
                string amount = creditCardPayment.Amount.ToString(CultureInfo.InvariantCulture);
                string cvv = creditCardPayment.Card.CVV;

                // name on card
                string cardHolderName = creditCardPayment.Card.NameOnCard;
                string firstName = string.Empty;
                string lastName = string.Empty;
                if (!string.IsNullOrEmpty(cardHolderName) && cardHolderName.IndexOf(" ") > 0)
                {
                    firstName = cardHolderName.Substring(0, cardHolderName.IndexOf(" "));
                    lastName = cardHolderName.Substring(cardHolderName.LastIndexOf(" ") + 1, cardHolderName.Length - cardHolderName.LastIndexOf(" ") - 1);
                }
                else
                {
                    firstName = cardHolderName;
                    lastName = cardHolderName;
                }

                //var request = new GetCyberSource3DEnrollmentRequest_V01
                var request = new Check3DEnrollmentRequest_V01
                {
                    Amount = amount,
                    CardNumber = cardNumber,
                    CardType = cardType,
                    ExpirationMonth = cardExpirationMonth,
                    ExiprationYear = cardExpirationYear,
                    CurrencyCode = currencyCode,
                    CountryCode = countryCode,
                    OrderNumber = orderNumber,
                    CVV2 = cvv,
                    FirstName = firstName,
                    LastName = lastName,
                    ClientRootUrl = rootUrl
                };

                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                try
                {
                    var response = proxy.Check3DEnrollment(new Check3DEnrollmentRequest1(request)).Check3DEnrollmentResult as Check3DEnrollmentResponse_V01;
                    if (response != null)
                    {
                        threeDSecuredCard = response.threeDSecuredCreditCard;
                        if (null != threeDSecuredCard)
                        {
                            Save3DPaymentAfterEnrollment(orderNumber, distributorId, locale, threeDSecuredCard);
                        }
                        else
                        {
                            LoggerHelper.Error(string.Format("3D Payment error when calling 3D Enrollment service. Order #:{0}. Error: 3D Enrollment service rturned NULL.", orderNumber));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex = new ApplicationException(string.Format("ThreeDPaymentProvider.Check3DSecuredEnrollment(...) method failed ", ex));
                    WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                }
                finally
                {
                    proxy.Close();
                }
            }

            return threeDSecuredCard;
        }

        public static ThreeDSecuredCreditCard Verify3DSecuredAuthentication(ThreeDSecuredCreditCard threeDCard)
        {
            switch (threeDCard.CountryCode)
            {
                case "CZ":
                case "SK":
                {
                    return Verify3D_Saferpay(threeDCard);
                }
                default:
                {
                    return Verify3DThroughSevice(threeDCard);
                }
            }
        }

        public static void Update3DPaymentRecord(string orderNumber, ThreeDSecuredCreditCard threeDCard, ThreeDPaymentStatus status)
        {
            if (null == threeDCard) return;

            var data = SerializeObject(threeDCard, typeof(ThreeDSecuredCreditCard));

            switch (status)
            {
                case ThreeDPaymentStatus.ThreeDPopupReturned:
                {
                    OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("Returned from 3D Popup. ", data),
                                    PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.ApprovalPending);
                    break;
                }
                case ThreeDPaymentStatus.VerificationChecked:
                {
                        if (threeDCard.IsErrored)
                        {
                            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("Errored in 3D processing. ", data),
                                PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.InError);
                        }
                        else if (threeDCard.IsDeclined)
                        {
                            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D password NOT Authenticated or NOT Verified. ", data),
                                    PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.Declined);
                        }
                        else if (threeDCard.IsVerified)
                    {
                            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D password Authenticated and Verified. ", data),
                            PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.Approved);
                    }
                    else
                    {
                            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D process has issue; please check. ", data),
                            PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.Declined);
                    }
                    break;
                }
                case ThreeDPaymentStatus.OrderCreated:
                {
                        OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("Will submit order to BizTalk for Authorization. ", data),
                                    PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.OrderSubmitted);
                    break;
                }
                case ThreeDPaymentStatus.Declined:
                    {
                        OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D payment is Declined. ", data),
                                        PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.Declined);
                        break;
                    }
                case ThreeDPaymentStatus.Errored:
                {
                    OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("Errored in 3D processing. ", data),
                                PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.InError);
                    break;
                }
            }

        }

        #endregion Public

        #region Private

        private static ThreeDSecuredCreditCard Verify3D_Saferpay(ThreeDSecuredCreditCard threeDCard)
        {
            ThreeDSecuredCreditCard result3DCard = new ThreeDSecuredCreditCard();
            result3DCard = threeDCard;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(threeDCard.PaRes);
            System.Xml.XmlElement root = doc.DocumentElement;
            string authenticationResult = (null != root && root.HasAttribute("RESULT")) ? root.Attributes["RESULT"].Value : string.Empty;
            string mpiSessionId = (null != root && root.HasAttribute("MPI_SESSIONID")) ? root.Attributes["MPI_SESSIONID"].Value : string.Empty;
            string authenticationMessage = (null != root && root.HasAttribute("MESSAGE")) ? root.Attributes["MESSAGE"].Value : string.Empty;
            string authenticationType = (null != root && root.HasAttribute("MSGTYPE")) ? root.Attributes["MSGTYPE"].Value : string.Empty;
            string eci = (null != root && root.HasAttribute("ECI")) ? root.Attributes["ECI"].Value : string.Empty;

            result3DCard.ReasonCode = !string.IsNullOrEmpty(authenticationResult) ? authenticationResult : threeDCard.ReasonCode;
            result3DCard.Decision = !string.IsNullOrEmpty(authenticationMessage) ? authenticationMessage : threeDCard.Decision;
            result3DCard.Eci = !string.IsNullOrEmpty(eci) ? eci : threeDCard.Eci;
            
            // 3D popup response data is good, then call Verify3DAuthentication(...)
            if (authenticationType == "AuthenticationConfirm" && authenticationResult == "0")
            {
                bool callVerificationService = true;
                if (HLConfigManager.Configurations.PaymentsConfiguration.Check3DPaymentEci && eci != "1" && eci != "2")
                {
                    callVerificationService = false;
                    result3DCard.IsErrored = true;
                }

                if (callVerificationService)
                {
                    result3DCard = ThreeDPaymentProvider.Verify3DThroughSevice(result3DCard);
                }
            }
            else
            {
                result3DCard.IsErrored = true;
            }

            return result3DCard;
        }

        private static ThreeDSecuredCreditCard Verify3DThroughSevice(ThreeDSecuredCreditCard threeDCard)
        {
            ThreeDSecuredCreditCard resultThreeDCard = threeDCard;

            var request = new Verify3DAuthenticationRequest_V01
            {
                ThreeDCard = threeDCard
            };

            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var response = proxy.Verify3DAuthentication(new Verify3DAuthenticationRequest1(request)).Verify3DAuthenticationResult as Verify3DAuthenticationResponse_V01;
                if(null != response && null != response.ThreeDCard && !string.IsNullOrEmpty(response.ThreeDCard.ProofXml))
                {
                    resultThreeDCard = response.ThreeDCard;
                    if (response.Status != ServiceResponseStatusType.Success)
                    {
                        resultThreeDCard.IsErrored = true;
                        resultThreeDCard.ProofXml = string.Concat(resultThreeDCard.ProofXml, Environment.NewLine, string.Format("3D Verification Error: Order Service return error: {0}.", response.Message));
                    }
                }
                else
                {
                    resultThreeDCard.IsErrored = true;
                    resultThreeDCard.ProofXml = string.Concat(resultThreeDCard.ProofXml, Environment.NewLine, string.Format("3D Verification Error: Order Service return empty ThreeDCard object; Order Service Error Message: {0}.", response.Message));
                }
            }
            catch (Exception ex)
            {
                resultThreeDCard.IsErrored = true;
                resultThreeDCard.ProofXml = string.Concat(resultThreeDCard.ProofXml, Environment.NewLine, string.Format("3D Verification Error: Exception thrown while calling Order Service: {0}.", ex.ToString()));
                ex = new ApplicationException(string.Format("ThreeDPaymentProvider.Verify3DThroughSevice failed ", ex));
                HL.Common.Utilities.WebUtilities.LogServiceExceptionWithContext(ex, proxy);
            }
            finally
            {
                proxy.Close();
            }

            return resultThreeDCard;
        }

        private static void Save3DPaymentAfterEnrollment(string orderNumber, string distributorId, string locale, ThreeDSecuredCreditCard threeDCard)
        {
            if (null == threeDCard)
            {
                return;
            }

            var data = SerializeObject(threeDCard, typeof(ThreeDSecuredCreditCard));

            if (threeDCard.IsErrored)
            {
                OrderProvider.InsertPaymentGatewayRecord(orderNumber, distributorId, "3DSecuredEnrollmentAuthentication", "3D credit card Enrollment Check has error -- Declined", locale);
                OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D credit card Enrollment Check has error -- Declined", data),
                           PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.InError);
                LoggerHelper.Error(string.Format("Order payment error when calling 3D Payment Enrollment service:{0} Error : {1}",
                        orderNumber, "3D Payment Enrollment service rturned invalid status"));
            }
            else if (threeDCard.IsDeclined)
            {
                OrderProvider.InsertPaymentGatewayRecord(orderNumber, distributorId, "3DSecuredEnrollmentAuthentication", "3D credit card Enrollment Check received Declined", locale);
                OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D credit card Enrollment Check received Declined", data),
                           PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.Declined);
            }
            else if (threeDCard.IsEnrolled)
            {
                OrderProvider.InsertPaymentGatewayRecord(orderNumber, distributorId, "3DSecuredEnrollmentAuthentication", "3D credit card enrolled; Will send for 3D password authentication.", locale);
                OrderProvider.UpdatePaymentGatewayRecord(orderNumber, string.Concat("3D credit card enrolled; Will send for 3D password authentication.", data),
                            PaymentGatewayLogEntryType.Response, PaymentGatewayRecordStatusType.ApprovalPending);
            }
            else // 3D card is Not Enrolled, and has no issue, should submit this order
            {
                OrderProvider.InsertPaymentGatewayRecord(orderNumber, distributorId, "3DSecuredEnrollmentAuthentication", "3D credit card NOT enrolled. Will submit order to BizTalk for Authorization.", locale);
            }
        }

        #endregion Private

        #region Helper
        private static string SerializeObject(Object pObject, Type objectType)
        {
            try
            {
                var ser = new XmlSerializer(objectType);
                var sb = new StringBuilder();
                var writer = new System.IO.StringWriter(sb);
                ser.Serialize(writer, pObject);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("ThreeDPaymentProvider serializeObject error:  {0}", ex.Message));
                return string.Empty;
            }
        }

        private static Dictionary<string, string> SplitStringToDictionary(string rawData)
        {
            var entryList = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(rawData))
            {
                var allEntries = rawData.Split(new char[] { ',' });
                if (allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        var item = entry.Split(new char[] { '=' }, 2);
                        if (item.Length > 1)
                        {
                            entryList.Add(item[0], item[1]);
                        }
                    }
                }
            }

            return entryList;
        }

        private static string GetEntryValue(Dictionary<string, string> entryList, string entryName)
        {
            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                if (entryList != null && entryList.ContainsKey(entryName))
                {
                    entryVal = entryList[entryName];
                }
            }

            return entryVal;
        }
        
        #endregion Helper
    }
}
