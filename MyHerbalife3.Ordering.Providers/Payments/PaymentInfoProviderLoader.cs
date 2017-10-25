using MyHerbalife3.Ordering.Providers.Interface;
using System.Collections.Generic;
using System.Web.SessionState;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PaymentInfoProviderLoader : IPaymentInfoProviderLoader
    {
        public void ClearPayments(string distributorID, string locale)
        {
            PaymentInfoProvider.ClearPayments(distributorID, locale);
        }

        public int DeletePaymentInfo(int id, string distributorID, string locale)
        {
            return PaymentInfoProvider.DeletePaymentInfo(id, distributorID, locale);
        }

        public string getCacheKey(string distributorID, string locale)
        {
            return PaymentInfoProvider.getCacheKey(distributorID, locale);
        }

        public string GetDummyCreditCardNumber(IssuerAssociationType cardBrand)
        {
            return PaymentInfoProvider.GetDummyCreditCardNumber(cardBrand);
        }

        public List<PaymentInformation> GetFailedCards(List<FailedCardInfo> failedCards, string distributorId, string locale)
        {
            return PaymentInfoProvider.GetFailedCards(failedCards, distributorId, locale);
        }

        public List<HPSCreditCardType> GetOnlineCreditCardTypes(string IsoCountryCode)
        {
            return PaymentInfoProvider.GetOnlineCreditCardTypes(IsoCountryCode);
        }

        public List<PaymentInformation> GetPaymentInfo(string distributorID, string locale)
        {
            return PaymentInfoProvider.GetPaymentInfo(distributorID, locale);
        }

        public PaymentInformation GetPaymentInfo(string distributorID, string locale, int id)
        {
            return PaymentInfoProvider.GetPaymentInfo(distributorID, locale, id);
        }

        public List<PaymentInformation> GetPaymentInfoForQuickPay(string distributorID, string locale)
        {
            return PaymentInfoProvider.GetPaymentInfoForQuickPay(distributorID, locale);
        }

        public bool ImportPayments(string distributorID, string locale, PaymentInfoItemList paymentInfo, HttpSessionState session)
        {
            return PaymentInfoProvider.ImportPayments(distributorID, locale, paymentInfo, session);
        }

        public PaymentInfoItemList ReloadPaymentInfo(string distributorID, string locale)
        {
            return PaymentInfoProvider.ReloadPaymentInfo(distributorID, locale);
        }

        public PaymentInfoItemList ReloadPaymentInfo(string distributorID, string locale, HttpSessionState aSession)
        {
            return PaymentInfoProvider.ReloadPaymentInfo(distributorID, locale, aSession);
        }

        public int SavePaymentInfo(string distributorID, string locale, PaymentInformation paymentInfo)
        {
            return PaymentInfoProvider.SavePaymentInfo(distributorID, locale, paymentInfo);
        }
    }
}
