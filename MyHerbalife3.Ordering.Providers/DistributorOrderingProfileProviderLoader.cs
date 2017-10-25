using MyHerbalife3.Ordering.Providers.Interface;
using System.Collections.Generic;
using System.Threading;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.EventSvc;
using Address_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01;
using Address_V02 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02;
using AddressType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;

namespace MyHerbalife3.Ordering.Providers
{
    public class DistributorOrderingProfileProviderLoader : IDistributorOrderingProfileProviderLoader
    {        
        public bool IsMarketingPlanDistributor
        {
            get
            {
                return DistributorOrderingProfileProvider.IsMarketingPlanDistributor;
            }
        }

        public DRFraudStatusType CheckForDRFraud(string distributorID, string countryCode, string zipCode)
        {
            return DistributorOrderingProfileProvider.CheckForDRFraud(distributorID, countryCode, zipCode);
        }

        public Address_V01 GetAddress(AddressType type, string distributorID, string countryCode)
        {
            return DistributorOrderingProfileProvider.GetAddress(type, distributorID, countryCode);
        }

        public Address_V02 GetAddressV02(AddressType type, string distributorID, string countryCode)
        {
            return DistributorOrderingProfileProvider.GetAddressV02(type, distributorID, countryCode);
        }

        public void GetDistributorNotes(DistributorOrderingProfile distributor, string noteType, string noteCode)
        {
            DistributorOrderingProfileProvider.GetDistributorNotes(distributor, noteType, noteCode);
        }

        public List<EventQualifier_V01> GetEventQualifierList(int eventId)
        {
            return DistributorOrderingProfileProvider.GetEventQualifierList(eventId);
        }

        public string GetPhoneNumberForCN(string distributorID)
        {
            return DistributorOrderingProfileProvider.GetPhoneNumberForCN(distributorID);
        }

        public DistributorOrderingProfile GetProfile(string distributorID, string countryCode)
        {
            return DistributorOrderingProfileProvider.GetProfile(distributorID, countryCode);
        }

        public List<TaxIdentification> GetTinList(string distributorID, bool getCurrentOnly, bool reload = false)
        {
            return DistributorOrderingProfileProvider.GetTinList(distributorID, getCurrentOnly, reload);
        }

        public bool IsEventQualified(int eventId)
        {
            return DistributorOrderingProfileProvider.IsEventQualified(eventId, Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}
