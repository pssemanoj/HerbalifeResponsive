using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    using System.Web.Security;

    public class DistributorOrderingProfileFactory : IDistributorOrderingProfileFactory
    {
        #region IDistributorOrderingProfileFactory Members

        private readonly ISimpleCache _cache;
        private readonly MyHerbalife3.Core.DistributorProvider.DistributorLoader _distributorLoader;

        public DistributorOrderingProfileFactory()
        {
            _cache = CacheFactory.Create();
            _distributorLoader = new DistributorLoader();
        }

        private DistributorOrderingProfile CreateDistributorOrderingProfile(string id, string countryCode)
        {
            var distributorV01 = _distributorLoader.Load(id, countryCode);
            try
            {
                if (null != distributorV01)
                {
                    return GetDistributorOrderingProfile(distributorV01, countryCode);
                }

                LoggerHelper.Error(
                    string.Format(
                        "DistributorOrderingProfileFactory.CreateDistributorOrderingProfile Error. Unsuccessful result from web service. Status: {0}",
                        "Service Error"));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Exception occured CreateDistributorOrderingProfile method {0}\n{1}",
                                                 id, ex));
                throw;
            }
            //return null;
            return createTempOrderingProfile(id, countryCode);
        }

        public static DistributorOrderingProfile GetDistributorOrderingProfile(Core.DistributorProvider.DistributorSvc.Distributor_V01 distributorV01,
                                                                                string loggedInCountry)
        {
            if (distributorV01 == null)
            {
                return null;
            }
            string referenceNumber = string.IsNullOrEmpty(distributorV01.ReferenceNumber) ? string.Empty : distributorV01.ReferenceNumber;
            int CNCustomorProfileID = 0;
            int CNStoreID = 0;
            string CNCustType = string.Empty;
            string CNCustCategoryType = string.Empty;
            string CNStoreProvince = string.Empty;
            int CNAPFStatus = 4;
            var IsPayByPhoneEnabled = false;
            var isTermConditionAlert = true;
            if (referenceNumber != string.Empty)
            {
                string[] CNParts = referenceNumber.Split(',');
                if (CNParts.Length > 0)
                {
                    int.TryParse(CNParts[0], out CNCustomorProfileID);
                    if (CNParts.Length > 1) int.TryParse(CNParts[1], out CNStoreID);
                    if (CNParts.Length > 2) CNCustType = CNParts[2];
                    if (CNParts.Length > 3) CNCustCategoryType = CNParts[3];
                    if (CNParts.Length > 4) CNStoreProvince = CNParts[4];
                    if (CNParts.Length > 5) int.TryParse(CNParts[5], out CNAPFStatus);
                    if (CNParts.Length > 6) bool.TryParse(CNParts[6], out IsPayByPhoneEnabled);
                    if (CNParts.Length > 7) bool.TryParse(CNParts[7], out isTermConditionAlert);
                }
            }
            var distributorProfile = new DistributorOrderingProfile
            {
                Id = distributorV01.ID,
                CantBuy = distributorV01.CantBuy,
                StaticDiscount =
                        decimal.Parse(double.IsNaN(distributorV01.StaticDiscount)
                                          ? "0"
                                          : distributorV01.StaticDiscount.ToString()),
                TodaysMagazine = distributorV01.TodaysMagazine,
                HardCashOnly = distributorV01.HardCashOnly,
                ApfDueDate = distributorV01.AnnualProcessingFeeDue,
                IsDistributorBlocked =
                        distributorV01.DRFraudStatusFlags != null &&
                        distributorV01.DRFraudStatusFlags.IsDistributorblocked,
                IsSponsorBlocked =
                        distributorV01.DRFraudStatusFlags != null && distributorV01.DRFraudStatusFlags.IsSponsorBlocked,
                OrderSubType = distributorV01.DSSubType, //DetermineOrderSubType(distributorV01.DSSubType),
                TinList = ObjectMappingHelper.Instance.GetToDistributor(distributorV01.TinList),
                BirthDate = distributorV01.BirthDate,
                DistributorVolumes = distributorV01.DistributorVolumes == null ? null : (from v in distributorV01.DistributorVolumes select ObjectMappingHelper.Instance.GetToDistributor(v)).ToList(),
                Addresses = ObjectMappingHelper.Instance.GetToCommon(distributorV01.Addresses),
                ShowAllInventory = HLConfigManager.Configurations.DOConfiguration.InventoryViewDefault == 0,
                TrainingFlag =
                        !string.IsNullOrEmpty(distributorV01.TrainingFlag.ToString()) && distributorV01.TrainingFlag,
                CantBuyReasons = distributorV01.CantBuyReasons != null ? distributorV01.CantBuyReasons.ToList() : null,
                SPQualificationDate = distributorV01.SPQualDate != null && distributorV01.SPQualDate.HasValue ? distributorV01.SPQualDate : DateTime.Now,
                ReferenceNumber = referenceNumber,
                // ADDED for China DO
                CNCustomorProfileID = CNCustomorProfileID,
                CNStoreID = CNStoreID,
                CNCustType = CNCustType,
                CNCustCategoryType = CNCustCategoryType,
                IsPC = CNCustCategoryType.Equals("PC") && CNCustType.Equals("CS"),
                CNStoreProvince = CNStoreProvince,
                CNAPFStatus = CNAPFStatus,
                PhoneNumbers = distributorV01.PhoneNumbers == null ? null : (from p in distributorV01.PhoneNumbers select ObjectMappingHelper.Instance.GetToDistributor(p)).ToList(),
                IsPayByPhoneEnabled = IsPayByPhoneEnabled,
                IsTermConditionAlert = isTermConditionAlert,
                ApplicationDate = distributorV01.ApplicationDate ?? new DateTime(1900, 1, 1),
                SponsorID = distributorV01.SponsorID,
                OrderRestrictions = distributorV01.OrderRestrictions,
                EmailAddresses = distributorV01.EmailAddresses,
              
            };
            if (distributorProfile.PhoneNumbers == null)
            {
                var dsDetails = MyHerbalife3.Core.DistributorProvider.Providers.DistributorProvider.GetDetailedDistributor(
                       distributorV01.ID, MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorDetailType.Phones);
                if (dsDetails != null && dsDetails.Phones !=null && dsDetails.Phones.Count()>0)
                {

                    distributorProfile.PhoneNumbers =(from p in dsDetails.Phones select ObjectMappingHelper.Instance.GetToDistributor(p)).ToList();
                }
            }
            var spouseLocalInfo = distributorV01.SpouseLocalName;
            if (spouseLocalInfo != null)
            {
                distributorProfile.SpouseLocalName = DistributorProfileModelHelper.FormatFullName(spouseLocalInfo.First, spouseLocalInfo.Last, null);
            }

            return distributorProfile;
        }

        // TODO: remove temp profile
        private static DistributorOrderingProfile createTempOrderingProfile(string id, string countryCode)
        {
            var distributorProfile = new DistributorOrderingProfile
                {
                    Id = id,
                    CantBuy = false,
                    StaticDiscount = 25,
                    TodaysMagazine = false,
                    HardCashOnly = false,
                    ApfDueDate = DateTime.Now.AddDays(-2),
                    IsDistributorBlocked = true,
                    IsSponsorBlocked = true,
                };

            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();

            if (membershipUser != null && membershipUser.Value != null && membershipUser.Value.ProcessingCountryCode == "KR" && countryCode == "KR")
            {
                distributorProfile.StaticDiscount = 25;
            }

            return distributorProfile;
        }

        private static OrderSubTypeModel DetermineOrderSubType(string orderSubTypeCode)
        {
            OrderSubTypeModel orderSubType;
            OrderSubTypeModel.TryParse(orderSubTypeCode, out orderSubType);
            return orderSubType;
        }

        #endregion

        #region IDistributorOrderingProfileFactory Members

        public DistributorOrderingProfile GetDistributorOrderingProfile(string id, string countryCode)
        {
            var distributorOrderingProfileCacheKey = string.Format("DO_DS_{0}", id);
            var expires = 15;
            int.TryParse(Settings.GetRequiredAppSetting("DistributorCacheExpireMinutes"), out expires);

            var orderingProfile = _cache.Retrieve(_ => CreateDistributorOrderingProfile(id, countryCode),
                                                  distributorOrderingProfileCacheKey, TimeSpan.FromMinutes(expires));

            if (orderingProfile.CurrentLoggedInCountry != countryCode)
            {
                if (orderingProfile.CurrentLoggedInCountry == "KR" | countryCode == "KR")
                {
                    var distributor = _distributorLoader.Load(id, countryCode);
                    if (distributor != null)
                    {
                        _distributorLoader.Expire(distributor, id, string.Empty);
                    }
                    _cache.Expire(typeof(DistributorOrderingProfile), distributorOrderingProfileCacheKey);
                    orderingProfile = _cache.Retrieve(_ => CreateDistributorOrderingProfile(id, countryCode),
                                                      distributorOrderingProfileCacheKey, TimeSpan.FromMinutes(expires));
                }
                else
                {
                    if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                    {
                        //For other countries, we just update the existing cached distributor 
                        if (PurchasingLimitProvider.RequirePurchasingLimits(id, countryCode))
                        {
                            IPurchasingLimitManagerFactory purchasingLimitManagerFactory =
                                new PurchasingLimitManagerFactory();
                            purchasingLimitManagerFactory.GetPurchasingLimitManager(id).LoadPurchasingLimits();
                        }
                    }
                }
                orderingProfile.CurrentLoggedInCountry = countryCode;
            }
            return orderingProfile;
        }

        public DistributorOrderingProfile ReloadDistributorOrderingProfile(string id, string countryCode)
        {
            var distributorOrderingProfileCacheKey = string.Format("DO_DS_{0}", id);
            var expires = 15;
            int.TryParse(Settings.GetRequiredAppSetting("DistributorCacheExpireMinutes"), out expires);

            var distributor = _distributorLoader.Load(id, countryCode);
            if (distributor != null)
            {
                _distributorLoader.Expire(distributor, id, string.Empty);
            }
            _cache.Expire(typeof(DistributorOrderingProfile), distributorOrderingProfileCacheKey);
            var orderingProfile = _cache.Retrieve(_ => CreateDistributorOrderingProfile(id, countryCode),
                                              distributorOrderingProfileCacheKey, TimeSpan.FromMinutes(expires));

            if (orderingProfile.CurrentLoggedInCountry != countryCode)
            {
                orderingProfile.CurrentLoggedInCountry = countryCode;
            }
            orderingProfile.Refreshed = true;
            return orderingProfile;
        }

        #endregion
    }
}
