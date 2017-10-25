using System;
using System.Globalization;
using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    public class PurchasingLimitManagerFactory : IPurchasingLimitManagerFactory
    {
        private readonly ISimpleCache _Cache;

        public PurchasingLimitManagerFactory()
        {
            _Cache = CacheFactory.Create();
        }

        #region IPurchasingLimitManagerFactory Members

        public void Expire(string id)
        {
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            _Cache.Expire(typeof(IPurchasingLimitManager), string.Format("PL_{0}_{1}", id, currentLoggedInCounrtyCode));
            IPurchasingLimitManager IPurchasingLimitManager = GetPurchasingLimitManager(id);
            IPurchasingLimitManager.ReloadPurchasingLimits(id);
        }

        public IPurchasingLimitManager GetPurchasingLimitManager(string id)
        {
            //Load the DS/get it from cache..
            ILoader<DistributorProfileModel, GetDistributorProfileById> distributorProfileLoader =
                new DistributorProfileLoader();
            var currentLoggedInCounrtyCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var distributorProfileModel = distributorProfileLoader.Load(new GetDistributorProfileById {Id = id});
            var purchasingLimitKey = string.Format("PL_{0}_{1}", id, currentLoggedInCounrtyCode);
            return _Cache.Retrieve(delegate
                {
                    return new PurchasingLimitManager(id, currentLoggedInCounrtyCode, distributorProfileModel.TypeCode,
                                                      distributorProfileModel.ProcessingCountryCode);
                }, purchasingLimitKey, TimeSpan.FromMinutes(30));
        }

        #endregion
    }
}