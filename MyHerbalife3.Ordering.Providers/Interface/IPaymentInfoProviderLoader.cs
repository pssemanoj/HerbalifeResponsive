using System.Collections.Generic;
using System.Web.SessionState;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IPaymentInfoProviderLoader
    {
        List<PaymentInformation> GetPaymentInfo(string distributorID, string locale);

        PaymentInformation GetPaymentInfo(string distributorID, string locale, int id);

        List<PaymentInformation> GetPaymentInfoForQuickPay(string distributorID, string locale);

        void ClearPayments(string distributorID, string locale);

        PaymentInfoItemList ReloadPaymentInfo(string distributorID, string locale);

        PaymentInfoItemList ReloadPaymentInfo(string distributorID, string locale, HttpSessionState aSession);

        int SavePaymentInfo(string distributorID, string locale, PaymentInformation paymentInfo);

        bool ImportPayments(string distributorID, string locale, PaymentInfoItemList paymentInfo, HttpSessionState session);

        int DeletePaymentInfo(int id, string distributorID, string locale);

        string getCacheKey(string distributorID, string locale);

        List<HPSCreditCardType> GetOnlineCreditCardTypes(string IsoCountryCode);

        List<PaymentInformation> GetFailedCards(List<FailedCardInfo> failedCards, string distributorId, string locale);

        string GetDummyCreditCardNumber(IssuerAssociationType cardBrand);
    }
}
