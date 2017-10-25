using DRFraudStatusType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.DRFraudStatusType;
using EventQualifier_V01 = MyHerbalife3.Ordering.ServiceProvider.EventSvc.EventQualifier_V01;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using System.Collections.Generic;
using Address_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01;
using Address_V02 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02;
using AddressType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IDistributorOrderingProfileProviderLoader
    {
        bool IsMarketingPlanDistributor { get; }

        void GetDistributorNotes(DistributorOrderingProfile distributor, string noteType, string noteCode);

        DistributorOrderingProfile GetProfile(string distributorID, string countryCode);

        Address_V02 GetAddressV02(AddressType type, string distributorID, string countryCode);

        Address_V01 GetAddress(AddressType type, string distributorID, string countryCode);

        string GetPhoneNumberForCN(string distributorID);

        List<TaxIdentification> GetTinList(string distributorID, bool getCurrentOnly, bool reload = false);

        DRFraudStatusType CheckForDRFraud(string distributorID, string countryCode, string zipCode);

        bool IsEventQualified(int eventId);

        List<EventQualifier_V01> GetEventQualifierList(int eventId);
    }
}
