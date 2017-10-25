using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Shared.ViewModel.Models;
using System.Web.Security;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    [DataObject]
    public static class PaymentInfoProvider
    {
        #region Constants

        public const string PAYMENTINFO_CACHE_PREFIX = "PaymentInfo_";

        /// <summary>
        ///     Cache key root for CardTypes cache
        /// </summary>
        public const string CachePrefix = "OnlineCardTypes_";

        /// <summary>
        ///     Cache duration for CardTypes
        /// </summary>
        public const int CardTypesCacheMinutes = 1440;

        public const string VisaCardNumber = "4H58265476741111";
        public const string MasterCardCardNumber = "5B00528502051732";
        public const string AmexCardNumber = "3H8828944401004";
        public const string GenericAccountNumber = "9H97314648519999";

        public static int PAYMENTINFO_CACHE_MINUTES = Settings.GetRequiredAppSetting<int>("PaymentCacheExpireMinutes");

        #endregion Constants

        #region Payments Service based methods

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<PaymentInformation> GetPaymentInfo(string distributorID, string locale)
        {
            string cacheKey = getCacheKey(distributorID, locale);
            var paymentList = new List<PaymentInformation>();
            Trace.TraceWarning("Method: GetPaymentInfo Distributor:'{0}' Locate: '{1}'", distributorID, locale);
            var payList = getGetPaymentInfoFromCache(distributorID, locale);
            if (null == payList)
            {
                // Refactor this to get it better or remove this if, this was done because PROD issue for MX Honors
                if ((HLConfigManager.Configurations.DOConfiguration.IsEventInProgress && locale.Substring(3).Equals("MX")))
                {
                    payList = RegCreditCardProvider.GetRegCreditCards(distributorID, locale);
                    var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    if (user != null && user.Value.ProcessingCountryCode == "MX")
                    {
                        if (null == payList) //Call failed - Try Sql database
                        {
                            payList = loadPaymentInfoFromService(distributorID, locale);
                        }
                    }
                }
                else
                {
                    if (HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry)
                    {
                        payList = RegCreditCardProvider.GetRegCreditCards(distributorID, locale);
                        if (null == payList) //Call failed - Try Sql database
                        {
                            payList = loadPaymentInfoFromService(distributorID, locale);
                        }
                    }
                    else
                    {
                        if (HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards)
                        {
                            payList = loadPaymentInfoFromService(distributorID, locale);
                        }
                    }
                }

                savePaymentInfoToCache(cacheKey, payList);
            }

            if (null != payList && payList.Count > 0)
            {
                paymentList = new List<PaymentInformation>(payList);
            }

            return paymentList;
        }

        public static PaymentInformation GetPaymentInfo(string distributorID, string locale, int id)
        {
            var paymentList = GetPaymentInfo(distributorID, locale);
            if (paymentList != null)
            {
                paymentList = paymentList.Where(p => p.ID == id).ToList();
            }
            return paymentList != null ? paymentList.First() : null;
        }

        public static List<PaymentInformation> GetPaymentInfoForQuickPay(string distributorID, string locale)
        {
            string cacheKey = getCacheKey(distributorID, locale);
            var paymentList = new List<PaymentInformation>();
            var payList = getGetPaymentInfoFromCache(distributorID, locale);
            if (null == payList)
            {
                payList = loadPaymentInfoFromService(distributorID, locale);
            }

            if (null != payList && payList.Count > 0)
            {
                paymentList = new List<PaymentInformation>(payList);
            }

            return paymentList;
        }

        public static void ClearPayments(string distributorID, string locale)
        {
            string cacheKey = getCacheKey(distributorID, locale);
            var session = HttpContext.Current.Session;
            session.Remove(cacheKey);
        }

        public static PaymentInfoItemList ReloadPaymentInfo(string distributorID, string locale)
        {
            return ReloadPaymentInfo(distributorID, locale, null);
        }

        public static PaymentInfoItemList ReloadPaymentInfo(string distributorID,
                                                            string locale,
                                                            HttpSessionState aSession)
        {
            var payments = new PaymentInfoItemList();
            try
            {
                string cacheKey = getCacheKey(distributorID, locale);
                var session =
                    ((null != aSession) ? aSession : HttpContext.Current.Session);

                if (session == null)
                    return payments;

                var current = (session)[cacheKey] as PaymentInfoItemList;
                List<PaymentInformation> tempPayments = null;
                if (null != current && current.Count > 0)
                {
                    tempPayments = (from t in current where t.IsTemporary select t).ToList();
                }

                payments = loadPaymentInfoFromService(distributorID, locale);

                if (null != current && null != payments)
                {
                    foreach (PaymentInformation pi in current)
                    {
                        var c = payments.Find(p => p.ID == pi.ID); // (from p in payments where p.ID == pi.ID select p);
                        if (c != null)
                        {
                            c.AuthorizationFailures = pi.AuthorizationFailures;
                        }
                    }
                }

                if (null != tempPayments && tempPayments.Count > 0)
                {
                    payments.AddRange(tempPayments);
                }

                if (payments != null)
                {
                    try
                    {
                        savePaymentInfoToCache(cacheKey, payments);
                    }
                    catch (Exception ex)
                    {
                        ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    }
                }
                else
                {
                    session.Remove(cacheKey);
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }
            return payments;
        }

        public static int SavePaymentInfo(string distributorID, string locale, PaymentInformation paymentInfo)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }

            if (paymentInfo.IsTemporary)
            {
                Trace.TraceWarning("Method: SavePaymentInfo Distributor:'{0}' Locate: '{1}'", distributorID, locale);
                var payments = getGetPaymentInfoFromCache(distributorID, locale);
                if (null == payments)
                {
                    payments = new PaymentInfoItemList();
                    savePaymentInfoToCache(getCacheKey(distributorID, locale), payments);
                }
                PaymentInformation existing = null;
                try
                {
                    existing = payments.Find(p => p.ID == paymentInfo.ID);
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }

                if (null != existing)
                {
                    payments.Remove(existing);
                }

                payments.Add(paymentInfo);

                return 0;
            }
            else
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                try
                {
                string country = (locale.Length > 2) ? locale.Substring(3) : locale;

                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        var request = new InsertPaymentInfoRequest_V02();
                        request.PaymentInfo = paymentInfo;
                        request.DistributorID = distributorID;
                        request.CountryCode = country;
                        var response =
                            proxy.InsertPaymentInfo(new InsertPaymentInfoRequest1(request)).InsertPaymentInfoResult as InsertPaymentInfoResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            ReloadPaymentInfo(distributorID, locale);
                            return response.ID;
                        }
                    }
                    else
                    {
                var request = new InsertPaymentInfoRequest_V01();
                request.PaymentInfo = paymentInfo;
                request.DistributorID = distributorID;
                request.CountryCode = country;
                var response =
                            proxy.InsertPaymentInfo(new InsertPaymentInfoRequest1(request)).InsertPaymentInfoResult as InsertPaymentInfoResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    ReloadPaymentInfo(distributorID, locale);
                    return response.ID;
                }
            }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext<ServiceProvider.OrderSvc.IOrderService>(ex, proxy);
                }
            }
            return 0;
        }

        public static bool ImportPayments(string distributorID,
                                          string locale,
                                          PaymentInfoItemList paymentInfo,
                                          HttpSessionState session)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return false;
            }
            else
            {
                string country = (locale.Length > 2) ? locale.Substring(3) : locale;
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                var request = new InsertPaymentInfoRequest_V01();
                foreach (PaymentInformation payment in paymentInfo)
                {
                    request.PaymentInfo = payment;
                    request.DistributorID = distributorID;
                    request.CountryCode = country;
                    var response =
                        proxy.InsertPaymentInfo(new InsertPaymentInfoRequest1(request)).InsertPaymentInfoResult as InsertPaymentInfoResponse_V01;
                    if (response != null && response.Status != ServiceResponseStatusType.Success)
                    {
                        ExceptionPolicy.HandleException(new Exception(response.Message),
                                                        ProviderPolicies.SYSTEM_EXCEPTION);
                    }
                }
                //savePaymentInfoToCache(getCacheKey(distributorID, locale), ReloadPaymentInfo(distributorID, locale, session), session);
            }
            return true;
        }

        public static int DeletePaymentInfo(int id, string distributorID, string locale)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return 0;
            }
            else
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                var request = new DeletePaymentInfoRequest_V01();
                request.DistributorID = distributorID;
                request.ID = id;
                var response =
                    proxy.DeletePaymentInfo(new DeletePaymentInfoRequest1(request)).DeletePaymentInfoResult as DeletePaymentInfoResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    ReloadPaymentInfo(distributorID, locale);
                    return 0;
                }
            }
            return 0;
        }

        public static string getCacheKey(string distributorID, string locale)
        {
            return string.Concat(PAYMENTINFO_CACHE_PREFIX, locale, '_', distributorID);
        }

        #endregion Payments Service based methods

        #region private methods

        private static PaymentInfoItemList loadPaymentInfoFromService(string distributorID, string locale)
        {
            if (string.IsNullOrEmpty(distributorID))
            {
                return null;
            }

            string country = (locale.Length > 2) ? locale.Substring(3) : locale;
            var proxy = ServiceClientProvider.GetOrderServiceProxy();

            try
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var response = proxy.GetPaymentInfo(new GetPaymentInfoRequest1(new GetPaymentInfoRequest_V02() { ID = 0, MaxCardsToGet = 0, DistributorID = distributorID, CountryCode = country })).GetPaymentInfoResult as GetPaymentInfoResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success && response.PaymentInfoList != null)
                    {
                        var payInfoItems =
                            response.PaymentInfoList.GroupBy(p => string.Concat(p.CardNumber, "/", p.Alias)).Select(
                                g => g.First());
                        var distinctCards = new PaymentInfoItemList();
                        foreach (PaymentInformation pi in payInfoItems)
                        {
                            distinctCards.Add(pi);
                        }
                        return distinctCards;
                    }
                }
                else
                {
                    var response = proxy.GetPaymentInfo(new GetPaymentInfoRequest1(new GetPaymentInfoRequest_V01() { ID = 0, MaxCardsToGet = 0, DistributorID = distributorID, CountryCode = country })).GetPaymentInfoResult as GetPaymentInfoResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success && response.PaymentInfoList != null)
            {
                var payInfoItems =
                    response.PaymentInfoList.GroupBy(p => string.Concat(p.CardNumber, "/", p.Alias)).Select(
                        g => g.First());
                var distinctCards = new PaymentInfoItemList();
                foreach (PaymentInformation pi in payInfoItems)
                {
                    distinctCards.Add(pi);
                }
                return distinctCards;
            }
                }
            }
            catch (Exception ex)
            {
                WebUtilities.LogServiceExceptionWithContext<IOrderService>(ex, proxy);
            }

            return null;
        }

        private static PaymentInfoItemList getGetPaymentInfoFromCache(string distributorID, string locale)
        {
            PaymentInfoItemList result = null;

            if (string.IsNullOrEmpty(distributorID) || string.IsNullOrEmpty(locale))
            {
                return result;
            }

            // cache is getting messed up on adding new
            // gets cache key
            string cacheKey = getCacheKey(distributorID, locale);

            // tries to get object from cache
            try
            {
                if (HttpContext.Current != null)
                {
                    var session = HttpContext.Current.Session;
                    if (session != null)
                    {
                        //ExceptionFix: Validating if object exist in cache before cast
                        if (session[cacheKey] != null)
                        {
                            result = session[cacheKey] as PaymentInfoItemList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.InnerException.ToString());
                Trace.TraceError(ex.StackTrace);
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }

            return result;
        }

        private static void savePaymentInfoToCache(string cacheKey, PaymentInfoItemList payments)
        {
            savePaymentInfoToCache(cacheKey, payments, null);
        }

        private static void savePaymentInfoToCache(string cacheKey,
                                                   PaymentInfoItemList payments,
                                                   HttpSessionState session)
        {
            if (payments != null)
            {
                if (null != HttpContext.Current)
                {
                    var thisSession =
                        (((null != session) ? session : HttpContext.Current.Session));
                    thisSession[cacheKey] = payments;
                }
            }
        }

        #endregion private methods

        #region HPSInfoService methods

        /// <summary>Get a list of valid online Card Types from HPS for a country</summary>
        /// <param name="IsoCountryCode"></param>
        /// <returns></returns>
        public static List<HPSCreditCardType> GetOnlineCreditCardTypes(string IsoCountryCode)
        {
            return GetFromCache(IsoCountryCode);
        }

        private static List<HPSCreditCardType> GetFromCache(string IsoCountryCode)
        {
            List<HPSCreditCardType> list = null;
            string cacheKey = GetCacheKey(IsoCountryCode);
            list = HttpRuntime.Cache[cacheKey] as List<HPSCreditCardType>;
            if (null == list || list.Count == 0)
            {
                list = LoadFromService(IsoCountryCode);
                HttpRuntime.Cache.Insert(cacheKey, list, null, DateTime.Now.AddMinutes(CardTypesCacheMinutes), Cache.NoSlidingExpiration);
            }

            return list;
        }

        private static List<HPSCreditCardType> LoadFromService(string isoCountryCode)
        {
            if (isoCountryCode == "KR")
            {
                return GetKoreanCards();
            }

            using (var orderProxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var req = new GetCardTypesForCountryRequest_V01()
                    {
                        CountryCode = isoCountryCode,
                        OnlineOnly = true,
                    };

                    var response = orderProxy.GetCardTypesForCountry(new GetCardTypesForCountryRequest1(req)).GetCardTypesForCountryResult as GetCardTypesForCountryResponse_V01;

                    if (response != null && response.HPSCreditCardTypes != null)
                    {
                        return response.HPSCreditCardTypes;
                    }
                }
                catch (Exception ex)
                {
                    WebUtilities.LogServiceExceptionWithContext<ServiceProvider.OrderSvc.IOrderService>(ex, orderProxy);
                }
            }

            return null;
        }

         private static string GetCacheKey(string key)
        {
            return string.Concat(CachePrefix, key);
        }

        #endregion HPSInfoService methods

        #region HPSService methods

        public static List<PaymentInformation> GetFailedCards(List<FailedCardInfo> failedCards,
                                                              string distributorId,
                                                              string locale)
        {
            var results = new List<PaymentInformation>();
            if (failedCards.Count > 0)
            {
                var currentPayments =
                    HttpContext.Current.Session[PaymentsConfiguration.GetPaymentInfoSessionKey(distributorId, locale)]
                    as List<PaymentInformation>;
                if (null != currentPayments && currentPayments.Count > 0)
                {
                    results = (from cp in currentPayments
                               from f in failedCards
                               where
                                   cp.CardNumber.Trim() == f.CardNumber.Trim() &&
                                   cp.CardType.Trim() == f.CardType.Trim()
                               select cp).Distinct().ToList();
                }
            }
            return results;
        }

        #endregion HPSService methods

        #region Utility methods

        private static List<HPSCreditCardType> GetKoreanCards()
        {
            List<HPSCreditCardType> cards = new List<HPSCreditCardType>();
            cards.Add(new HPSCreditCardType() { Code = "현대카드", CardNumberValidationRegexText = "^\\d+" });  // Hyundai  // 현대카드
            cards.Add(new HPSCreditCardType() { Code = "신한카드", CardNumberValidationRegexText = "^\\d+" });  // Shinhan  // 신한카드
            cards.Add(new HPSCreditCardType() { Code = "국민카드", CardNumberValidationRegexText = "^\\d+" });  // KB Card  // 국민카드
            cards.Add(new HPSCreditCardType() { Code = "비씨카드", CardNumberValidationRegexText = "^\\d+" });  // BC Card  // 비씨카드

            //cards.Add(new HPSCreditCardType() { Code = "외환카드", CardNumberValidationRegexText = "^\\d+" });  // KEB-Card // 외환카드
            //cards.Add(new HPSCreditCardType() { Code = "삼성카드", CardNumberValidationRegexText = "^\\d+" });  // Samsung  // 삼성카드
            //cards.Add(new HPSCreditCardType() { Code = "NH카드", CardNumberValidationRegexText = "^\\d+" });    // NHCard   // NH카드
            //cards.Add(new HPSCreditCardType() { Code = "신한카드", CardNumberValidationRegexText = "^\\d+" });  // Shinhan  // 신한카드
            //cards.Add(new HPSCreditCardType() { Code = "씨티카드", CardNumberValidationRegexText = "^\\d+" });  // City     // 씨티카드
            //cards.Add(new HPSCreditCardType() { Code = "수협카드", CardNumberValidationRegexText = "^\\d+" });  // Suhyup   // 수협카드
            //cards.Add(new HPSCreditCardType() { Code = "광주카드", CardNumberValidationRegexText = "^\\d+" });  // KwangJu  // 광주카드
            //cards.Add(new HPSCreditCardType() { Code = "전북카드", CardNumberValidationRegexText = "^\\d+" });  // Jeonbuk  // 전북카드
            //cards.Add(new HPSCreditCardType() { Code = "하나카드", CardNumberValidationRegexText = "^\\d+" });  // Hana     // 하나카드
            //cards.Add(new HPSCreditCardType() { Code = "현대카드", CardNumberValidationRegexText = "^\\d+" });  // Hyundai  // 현대카드
            //cards.Add(new HPSCreditCardType() { Code = "산은카드", CardNumberValidationRegexText = "^\\d+" });  // Saneun   // 산은카드
            //cards.Add(new HPSCreditCardType() { Code = "국민카드", CardNumberValidationRegexText = "^\\d+" });  // KB Card  // 국민카드
            //cards.Add(new HPSCreditCardType() { Code = "비씨카드", CardNumberValidationRegexText = "^\\d+" });  // BC Card  // 비씨카드

            //cards.Add(new HPSCreditCardType() { Code = "KEBCard", CardNumberValidationRegexText = "^\\d+" });  // KEB-Card  // 외환카드
            //cards.Add(new HPSCreditCardType() { Code = "Samsung", CardNumberValidationRegexText = "^\\d+" });  // Samsung   // 삼성카드
            //cards.Add(new HPSCreditCardType() { Code = "NHCard", CardNumberValidationRegexText = "^\\d+" });    // NHCard   // NH카드
            //cards.Add(new HPSCreditCardType() { Code = "Shinhan", CardNumberValidationRegexText = "^\\d+" });  // Shinhan   // 신한카드
            //cards.Add(new HPSCreditCardType() { Code = "City", CardNumberValidationRegexText = "^\\d+" });      // City     // 씨티카드
            //cards.Add(new HPSCreditCardType() { Code = "Suhyup", CardNumberValidationRegexText = "^\\d+" });    // Suhyup   // 수협카드
            //cards.Add(new HPSCreditCardType() { Code = "KwangJu", CardNumberValidationRegexText = "^\\d+" });   // KwangJu  // 광주카드
            //cards.Add(new HPSCreditCardType() { Code = "Jeonbuk", CardNumberValidationRegexText = "^\\d+" });   // Jeonbuk  // 전북카드
            //cards.Add(new HPSCreditCardType() { Code = "Hana", CardNumberValidationRegexText = "^\\d+" });      // Hana     // 하나카드
            //cards.Add(new HPSCreditCardType() { Code = "Hyundai", CardNumberValidationRegexText = "^\\d+" });   // Hyundai  // 현대카드
            //cards.Add(new HPSCreditCardType() { Code = "Saneun", CardNumberValidationRegexText = "^\\d+" });    // Saneun   // 산은카드
            //cards.Add(new HPSCreditCardType() { Code = "KBCard", CardNumberValidationRegexText = "^\\d+" });    // KB Card  // 국민카드
            //cards.Add(new HPSCreditCardType() { Code = "BCCard", CardNumberValidationRegexText = "^\\d+" });    // BC Card  // 비씨카드

            return cards;
        }

        public static string GetDummyCreditCardNumber(IssuerAssociationType cardBrand)
        {
            string cardNumber = VisaCardNumber;
            switch (cardBrand)
            {
                case IssuerAssociationType.MasterCard:
                    {
                        cardNumber = MasterCardCardNumber;
                        break;
                    }
                case IssuerAssociationType.AmericanExpress:
                    {
                        cardNumber = AmexCardNumber;
                        break;
                    }
            }

            return cardNumber;
        }

        #endregion Utility methods
    }
}