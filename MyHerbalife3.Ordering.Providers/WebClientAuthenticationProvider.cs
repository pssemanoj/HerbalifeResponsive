using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public static class WebClientAuthenticationProvider
    {
        public const string WEBCLIENTAUTH_CACHE_PREFIX = "WEBCLIENTAUTH";
        public const int WEBCLIENTAUTH_CACHE_MINUTES = 60*24;

        public static string GetWebClientAuthCode(string isoCountryCode)
        {
            var WebClientAuthLookup = getWebClientAuthCodeFromCache();
            if (WebClientAuthLookup != null)
            {
                var WebClientAuth = WebClientAuthLookup.Where(p => p.Key == isoCountryCode);
                if (WebClientAuth != null && WebClientAuth.Count() > 0)
                {
                    return WebClientAuth.First().Value;
                }
            }
            return null;
        }

        private static string getCacheKey()
        {
            return WEBCLIENTAUTH_CACHE_PREFIX;
        }

        private static Dictionary<string, string> loadWebClientAuthCodeFromService()
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            var response = proxy.GetWebClientAuthKey(new GetWebClientAuthKeyRequest(new GetWebAuthKeyRequest_V01())).GetWebClientAuthKeyResult as GetWebAuthKeyResponse_V01;
            if (response != null && response.Status == ServiceResponseStatusType.Success)
            {
                return response.WebAuthKey;
            }
            return null;
        }

        private static void saveWebClientAuthCodeToCache(string cacheKey, Dictionary<string, string> WebClientAuth)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     WebClientAuth,
                                     null,
                                     DateTime.Now.AddMinutes(WEBCLIENTAUTH_CACHE_MINUTES),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable,
                                     null);
        }

        private static Dictionary<string, string> getWebClientAuthCodeFromCache()
        {
            Dictionary<string, string> result = null;

            // gets cache key 
            string cacheKey = getCacheKey();

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as Dictionary<string, string>;

            if (null == result)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = loadWebClientAuthCodeFromService();
                    // saves to cache is successful
                    if (null != result)
                    {
                        saveWebClientAuthCodeToCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }
    }
}