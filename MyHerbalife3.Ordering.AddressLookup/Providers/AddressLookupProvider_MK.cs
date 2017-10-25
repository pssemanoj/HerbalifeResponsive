using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProvider_MK : AddressLookupProviderBase
    {
        public const int CitylookupCacheMinutes = 60 * 24;
        public const string CitylookupCachePrefix = "CITYLOOKUP_MS";

        public override List<string> GetCitiesForState(string country, string state)
        {
            if (HttpRuntime.Cache[CitylookupCachePrefix] != null)
            {
                return HttpRuntime.Cache[CitylookupCachePrefix] as List<string>;
            }
            try
            {
                List<string> dictCityZip = null;
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new CitiesForStateRequest_V02 { Country = country };
                var response = proxy.GetCitiesForState(new GetCitiesForStateRequest(request));
                var result = response.GetCitiesForStateResult as CitiesForStateResponse_V02;
                if (result != null && result.Status == ServiceResponseStatusType.Success)
                {
                    if (result.DictCityZip != null && result.DictCityZip.Count() > 0)
                    {
                        // order by city name
                        dictCityZip =
                            result.DictCityZip.OrderBy(l => l.Value).Select(l => l.Key + "," + l.Value).ToList();
                        HttpRuntime.Cache.Insert(
                            CitylookupCachePrefix,
                            dictCityZip,
                            null,
                            DateTime.Now.AddMinutes(CitylookupCacheMinutes),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.NotRemovable,
                            null);
                    }
                }

                return dictCityZip;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetCitiesForState error: Country {0}, error: {1}", country, ex));
            }
            return null;
        }
    }
}
