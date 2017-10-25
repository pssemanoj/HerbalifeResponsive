using System;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class AsyncRegCreditCardProvider : ICallbackEventHandler
    {
        string ICallbackEventHandler.GetCallbackResult()
        {
            return "";
        }

        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
        }

        private void ImportCardsCallback(IAsyncResult ar)
        {
            var aResult = (AsyncResult) ar;
            var doDelegate = (DoImportPaymentDelegate) aResult.AsyncDelegate;
            // Session object is used to tell if process finishes or not
            doDelegate.EndInvoke(ar);
        }

        private void DeleteCardsCallback(IAsyncResult ar)
        {
            var aResult = (AsyncResult) ar;
            var doDelegate = (DoDeletePaymentDelegate) aResult.AsyncDelegate;
            // Session object is used to tell if process finishes or not
            doDelegate.EndInvoke(ar);
        }

        public IAsyncResult AsyncImportCardsToSQL(string distributorID,
                                                  string locale,
                                                  PaymentInfoItemList payments,
                                                  bool remove)
        {
            IAsyncResult ar;
            if (remove == false)
            {
                DoImportPaymentDelegate doDelegate = DoImportPaymentAsync;
                ar = doDelegate.BeginInvoke(distributorID, locale, payments, HttpContext.Current.Session,
                                            ImportCardsCallback, null);
            }
            else
            {
                DoDeletePaymentDelegate doDelegate = DoRemovePaymentAsync;
                ar = doDelegate.BeginInvoke(distributorID, locale, HttpContext.Current.Session,
                                            DeleteCardsCallback, null);
            }

            return ar;
        }

        private bool DoImportPaymentAsync(string distributorID,
                                          string locale,
                                          PaymentInfoItemList payments,
                                          HttpSessionState session)
        {
            return PaymentInfoProvider.ImportPayments(distributorID, locale, payments, session);
        }

        private bool DoRemovePaymentAsync(string distributorID, string locale, HttpSessionState session)
        {
            var payments = PaymentInfoProvider.GetPaymentInfo(distributorID, locale);
            if (payments != null)
            {
                foreach (PaymentInformation payment in payments)
                {
                    PaymentInfoProvider.DeletePaymentInfo(payment.ID, distributorID, locale);
                }
            }
            return true;
        }

        private delegate bool DoDeletePaymentDelegate(string distributorID, string locale, HttpSessionState session);

        private delegate bool DoImportPaymentDelegate(
            string distributorID, string locale, PaymentInfoItemList payments, HttpSessionState session);
    }

    public static class RegCreditCardProvider
    {
        public const string PaymentinfoCachePrefix = "RegPaymentInfo_";

        public static int PAYMENTINFO_CACHE_MINUTES =
            Settings.GetRequiredAppSetting<int>("PaymentCacheExpireMinutes");

        private static PaymentInfoItemList loadPaymentInfoFromService(string distributorID, string locale)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }
            else
            {
                PaymentInfoItemList paymentInfo = null;
                try
                {
                    string country = (locale.Length > 2) ? locale.Substring(3) : locale;
                    var proxy = ServiceClientProvider.GetOrderServiceProxy();
                    var request = new GetPaymentInfoRequest_V01() { ID = 0, MaxCardsToGet = 0, DistributorID = distributorID, CountryCode = country };
                    request.GetFromHMSRegistry = true;
                    var response = proxy.GetPaymentInfo(new GetPaymentInfoRequest1(request)).GetPaymentInfoResult as GetPaymentInfoResponse_V01;
                    if (response != null &&
                        (response.Status == ServiceResponseStatusType.Success ||
                         response.Status == ServiceResponseStatusType.None)
                        // The None responce when the service retrieves an empty list
                        && response.PaymentInfoList != null)
                    {
                        paymentInfo = response.PaymentInfoList;
                        foreach (PaymentInformation pi in paymentInfo)
                        {
                            pi.BillingAddress = new Address_V01 {Country = country};
                        }
                        (new AsyncRegCreditCardProvider()).AsyncImportCardsToSQL(distributorID, locale,
                                                                                 response.PaymentInfoList,
                                                                                 paymentInfo.Count == 0);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("Checkout - HMS reg card retrieve error: DS{0}: {1}", distributorID,
                                                     ex));
                }

                return paymentInfo;
            }
        }

        private static PaymentInfoItemList getPaymentInfo(string distributorID, string locale)
        {
            PaymentInfoItemList result = null;

            try
            {
                result = loadPaymentInfoFromService(distributorID, locale);
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }

            return result;
        }

        public static PaymentInfoItemList GetRegCreditCards(string distributorID, string locale)
        {
            return getPaymentInfo(distributorID, locale);
        }
    }
}