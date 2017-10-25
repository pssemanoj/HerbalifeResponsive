using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    public class PurchasingLimitManager : IPurchasingLimitManager
    {
        private readonly string _distributorId;
        private readonly string _distributorLevel;
        private readonly string _processingCountry;
        private string _currentLoggedInCountry;


        public PurchasingLimitManager(string distributorId, string currentLoggedInCountry, string distributorLevel,
                                      string processingCountry)
        {
            _distributorId = distributorId;
            _currentLoggedInCountry = currentLoggedInCountry;
            _distributorLevel = distributorLevel;
            _processingCountry = processingCountry;
            PurchasingLimitsRestriction = HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType;
            var distributorPurchasingLimitsCollection = GetDistributorPurchasingLimitsCollection();
            CreatePurchasingLimits(distributorPurchasingLimitsCollection);
        }

        private PurchasingLimitRestrictionType _purchasingLimitRestrictionType = HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType;
        public PurchasingLimitRestrictionType PurchasingLimitsRestriction
        {
            get { return _purchasingLimitRestrictionType;  }
            set { _purchasingLimitRestrictionType = value;  } 
        }

        public Dictionary<int, PurchasingLimits_V01> PurchasingLimits { get; set; }

        public decimal MaxEarningsLimit { get; set; }

        public decimal MaxPCEarningsLimit { get; set; }

        public decimal MaxPersonalConsumptionLimit { get; set; }
        public decimal MaxPersonalPCConsumptionLimit { get; set; }

        public decimal RemainingEarningsLimit { get; set; }

        public decimal RemainingPersonalConsumptionLimit { get; set; }

        public decimal YTDEarnings { get; set; }

        public decimal YTDVolume { get; set; }

        public void SetPurchasingLimits(int orderMonth)
        {
            if (null == PurchasingLimits || PurchasingLimits.Count <= 0) return;
            PurchasingLimits_V01 currentLimit = null;

            if (PurchasingLimits.ContainsKey(orderMonth))
            {
                currentLimit = PurchasingLimits[orderMonth];
            }

            if (null == currentLimit) return;
            RemainingPersonalConsumptionLimit = currentLimit.RemainingVolume;
            RemainingEarningsLimit = currentLimit.RemainingEarnings;
            MaxPersonalConsumptionLimit = currentLimit.maxVolumeLimit;
            MaxEarningsLimit = currentLimit.MaxEarningsLimit;
            MaxPersonalPCConsumptionLimit = currentLimit.MaxPCEarningsLimit;
            MaxPCEarningsLimit = currentLimit.MaxPCEarningsLimit;
        }

        public void SetPurchasingLimits(PurchasingLimits_V01 currentLimit)
        {
            if (null == currentLimit) return;
            RemainingPersonalConsumptionLimit = currentLimit.RemainingVolume;
            RemainingEarningsLimit = currentLimit.RemainingEarnings;
            MaxPersonalConsumptionLimit = currentLimit.maxVolumeLimit;
            MaxEarningsLimit = currentLimit.MaxEarningsLimit;
            MaxPersonalPCConsumptionLimit = currentLimit.MaxPCEarningsLimit;
            MaxPCEarningsLimit = currentLimit.MaxPCEarningsLimit;
        }

        public PurchasingLimits_V01 GetPurchasingLimits(int orderMonth)
        {
            PurchasingLimits_V01 currentLimit = null;
            if (null != PurchasingLimits && PurchasingLimits.Count > 0)
            {
                if (PurchasingLimits.ContainsKey(orderMonth))
                {
                    currentLimit = PurchasingLimits[orderMonth];
                }        
            }

            return currentLimit;
        }

        public void UpdatePurchasingLimits(PurchasingLimits_V01 limits, int orderMonth)
        {
            PurchasingLimits[orderMonth] = limits;
            SetPurchasingLimits(orderMonth);
        }

        public void ReloadPurchasingLimits(string distributorID)
        {
            try
            {
                if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorID))
                {
                    CreatePurchasingLimits(GetDistributorPurchasingLimitsCollection());
                }
                if (PurchasingLimits != null)
                    SaveToCache();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }


        public PurchasingLimits ReloadPurchasingLimits(int orderMonth)
        {
            PurchasingLimits_V01 purchasingLimitsV01 = null;
            try
            {
                if (PurchasingLimitProvider.RequirePurchasingLimits(_distributorId, _currentLoggedInCountry))
                {
                    CreatePurchasingLimits(GetDistributorPurchasingLimitsCollection());
                }
                if (PurchasingLimits == null) return purchasingLimitsV01;
                if (orderMonth > 0)
                {
                    SetPurchasingLimits(orderMonth);
                }
                SaveToCache();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            if (PurchasingLimits != null) PurchasingLimits.TryGetValue(orderMonth, out purchasingLimitsV01);
            return purchasingLimitsV01;
        }

        public void LoadPurchasingLimits()
        {
            if (PurchasingLimitProvider.RequirePurchasingLimits(_distributorId, _currentLoggedInCountry))
            {
                if (PurchasingLimits == null)
                    CreatePurchasingLimits(GetDistributorPurchasingLimitsCollection());
            }
        }

        private void CreatePurchasingLimits(List<DistributorPurchasingLimits> limits)
        {
            if (null == limits) return;
            PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();
            foreach (DistributorPurchasingLimits_V01 item in limits)
            {
                var month = 0;
                if (Int32.TryParse(item.OrderMonth.Replace("/", string.Empty), out month))
                {
                    PurchasingLimits.Add(month, new PurchasingLimits_V01
                        {
                            RemainingEarnings = item.RemainingEarningsLimit,
                            MaxEarningsLimit = item.MaxEarningsLimit,
                            RemainingVolume = item.RemainingPersonalConsumptionLimit,
                            maxVolumeLimit =  item.MaxPersonalConsumptionLimit,
                            MaxPCEarningsLimit = item.MaxPCEarningsLimit
                        });
                }
            }
            var newest =
                limits.Find(
                    l =>
                    (l as DistributorPurchasingLimits_V01).Date ==
                    limits.Max(dpl => (dpl as DistributorPurchasingLimits_V01).Date)) as
                DistributorPurchasingLimits_V01;
            YTDEarnings = newest.EarnedEarningsLimit;
            YTDVolume = newest.EarnedPersonalConsumption;
        }

        private DistributorPurchasingLimitsCollection GetDistributorPurchasingLimitsCollection()
        {
            DistributorPurchasingLimitsCollection distributorPurchasingLimitsCollection = null;
            var mpeThresholdCountries =
                new List<string>(Settings.GetRequiredAppSetting("MPEThresholdCountries").Split(new[] { ',' }));
            if (mpeThresholdCountries.Contains(_processingCountry))
            {
                if (_distributorLevel == "DS")
                    _currentLoggedInCountry = _processingCountry;
            }
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var request = new GetDistributorPurchasingLimitsRequest_V01
                        {
                            DistributorID = _distributorId,
                            CountryCode = _currentLoggedInCountry
                        };

                    // RS exception in country for request
                    if (_currentLoggedInCountry.Equals("RS"))
                    {
                        request.CountryCode = CountryType.RS.HmsCountryCodes.FirstOrDefault();
                    }

                    var response =
                        proxy.GetDistributorPurchasingLimits(new GetDistributorPurchasingLimitsRequest(request)).GetDistributorPurchasingLimitsResult as GetDistributorPurchasingLimitsResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        distributorPurchasingLimitsCollection = response.PurchasingLimits;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("System.Exception", new Exception(
                                                                   string.Format(
                                                                       "Error retrieving GetPurchasingLimits Status from Order service for: DS:{0} - Country:{1}, {2}",
                                                                       _distributorId, _currentLoggedInCountry, ex)));
                    distributorPurchasingLimitsCollection = null;
                }
            }

            return distributorPurchasingLimitsCollection;
        }


        private void SaveToCache()
        {
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var purchasingLimitKey = string.Format("PL_{0}_{1}", _distributorId, currentLoggedInCounrtyCode);
            var _cache = CacheFactory.Create();
            _cache.Add(this, purchasingLimitKey, TimeSpan.FromMinutes(30));
        }

        public void ExpireCache()
        {
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var purchasingLimitKey = string.Format("PL_{0}_{1}", _distributorId, currentLoggedInCounrtyCode);
            var _cache = CacheFactory.Create();
            _cache.Expire(typeof(IPurchasingLimitManager), purchasingLimitKey);
        }

    }
}