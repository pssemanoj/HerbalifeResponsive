using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Configuration;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;

namespace MyHerbalife3.Ordering.Providers.FOP
{
    public class PurchaseRestrictionManagerFactory : IPurchaseRestrictionManagerFactory
    {
        private readonly ISimpleCache _Cache;
        private static string _cachekey = "FOP_{0}_{1}";

        public PurchaseRestrictionManagerFactory()
        {
            _Cache = CacheFactory.Create();
        }

        #region IPurchaseRestrictionManagerFactory Members

        public static string GetCacheKey(string distributorID, string countryCode)
        {
            return string.Format(_cachekey, distributorID, countryCode);
        }

        public void Expire(string id)
        {
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            _Cache.Expire(typeof(IPurchaseRestrictionManager), GetCacheKey(id, currentLoggedInCounrtyCode));
        }

        public IPurchaseRestrictionManager GetPurchaseRestrictionManager(string id)
        {
            //Load the DS/get it from cache..
            ILoader<DistributorProfileModel, GetDistributorProfileById> distributorProfileLoader = new DistributorProfileLoader();
            var locale = CultureInfo.CurrentCulture.Name;
            var currentLoggedInCounrtyCode = locale.Substring(3);
            var distributorProfileModel = distributorProfileLoader.Load(new GetDistributorProfileById {Id = id});

            // to prevent recusive calls
            // get distributor ordering profile from Distributor_V01
            DistributorLoader distributorLoader = new DistributorLoader();
            var distributorProfile = distributorLoader.Load(id, currentLoggedInCounrtyCode);
            var distributorOrderingProfile = DistributorOrderingProfileFactory.GetDistributorOrderingProfile(distributorProfile, currentLoggedInCounrtyCode);

            return _Cache.Retrieve(delegate
                {
                    return new PurchaseRestrictionManager(id, currentLoggedInCounrtyCode, distributorProfileModel.TypeCode, distributorProfileModel.ProcessingCountryCode, locale, distributorOrderingProfile.TinList, distributorOrderingProfile.OrderSubType);
                }, GetCacheKey(id, currentLoggedInCounrtyCode), TimeSpan.FromMinutes(Settings.GetRequiredAppSetting(
                                                                                      "PurchasingLimitsCacheMinutes",
                                                                                      30)));
        }

        #endregion
    }
}
