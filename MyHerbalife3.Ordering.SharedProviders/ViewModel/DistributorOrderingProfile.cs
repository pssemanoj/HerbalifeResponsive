using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
 
namespace MyHerbalife3.Ordering.SharedProviders.ViewModel
{
    public class DistributorOrderingProfile
    {
        public string Id { get; set; }
        public DateTime ApfDueDate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public bool CantBuy { get; set; }
        public bool HardCashOnly { get; set; }
        public decimal StaticDiscount { get; set; }
        // TODO : set YTDEarnings
        public decimal YTDEarnings { get; set; }
        public List<TaxIdentification> TinList { get; set; }
        public bool TodaysMagazine { get; set; }
        public string OrderSubType { get; set; }
        public bool IsDistributorBlocked { get; set; }
        public bool IsSponsorBlocked { get; set; }
        public bool ShowAllInventory { get; set; }
        public DateTime BirthDate { get; set; }
        public string CurrentLoggedInCountry { get; set; }
        public HL.Common.ValueObjects.AddressCollection Addresses { get; set; }

        public List<DistributorVolume_V01> DistributorVolumes { get; set; }

        public List<DistributorNote_V01> DistributorNotes { get; set; }

        public string ReferenceNumber { get; set; }

        public bool TrainingFlag { get; set; }

        public bool TrainingExtendFlag { get; set; }

        public DateTime? TrainingEndDate { get; set; }

        public DateTime? TrainingExtEndDate { get; set; }
        public List<string> CantBuyReasons { get; set; }
        public bool CantBuyOverride { get; set; }
        // ADDED for China DO
        public int CNCustomorProfileID { get; set; }
        public int CNStoreID { get; set; }
        public string CNStoreProvince { get; set; }
        public string CNCustType { get; set; }
        public string CNCustCategoryType { get; set; }
        public bool IsPC { get; set; }
        public int CNAPFStatus { get; set; }
        public List<PhoneNumber_V03> PhoneNumbers { get; set; }
        public bool IsPayByPhoneEnabled { get; set; }
        public DateTime? SPQualificationDate { get; set; }
        public bool IsTermConditionAlert { get; set; }

        public string SpouseLocalName { get; set; }
        public bool Refreshed { get; set; }
        /// Kount
        public string SponsorID { get; set; }

        // HAP
        public bool HAPExpiryDateSpecified { get; set; }
        public DateTime? HAPExpiryDate { get; set; }

        public DRFraudStatusFlags_V01 FraudStatus { get; set; }

        // MPC Fraud
        public bool? IsMPCFraud { get; set; }

        public MyHerbalife3.Core.DistributorProvider.DistributorSvc.OrderRestrictions_V01[] OrderRestrictions { get; set; } 
        public MyHerbalife3.Core.DistributorProvider.DistributorSvc.EmailAddress [] EmailAddresses { get; set;}
      
    }
}