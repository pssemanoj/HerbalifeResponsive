using System.Linq;
using HL.Common.Configuration;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers
{
    public class HlCountryConfigurationProvider
    {

      
        public const string COUNTRY_CONFIGURATION_CACHE_PREFIX = "CountryConfiguration_";
        public static int COUNTRY_CONFIGURATION_CACHE_MINUTES = Settings.GetRequiredAppSetting<int>("CountryConfigurationCachingTimeInMinutes", 1);
        /// <summary>
        /// Get country configuration.
        /// </summary>
        /// <param name="countryCode">Country code.</param>
        /// <param name="locale">Locale parameter.</param>
        /// <param name="applicationType">Type of the application.</param>
        /// <returns>
        /// Response with configuration.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        /// OrderProvider.GetCountryConfiguration error. Response is Null
        /// or
        /// OrderProvider.GetCountryConfiguration error. Response Status Not Success
        /// </exception>
        public static MyHLConfiguration GetCountryConfiguration(string locale)
        {
            string cacheKey = string.Format("{0}{1}", COUNTRY_CONFIGURATION_CACHE_PREFIX, locale);
            var cachedConfig = HttpRuntime.Cache[cacheKey] as MyHLConfiguration;

            if (cachedConfig != null)
            {
                return cachedConfig;
            }

            using (var svcProxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var response = svcProxy.GetCountryConfiguration(new GetCountryConfigurationRequest1(new GetCountryConfigurationRequest_V01
                    {
                        ApplicationType = ApplicationType.GDO_MYHL,
                        CountryCode = locale.Substring(3),
                        Locale = locale
                    }));

                    if (response != null && response.GetCountryConfigurationResult.Status == ServiceResponseStatusType.Success)
                    {
                        var configuration = response.GetCountryConfigurationResult as GetCountryConfigurationResponse_V01;
                        if (configuration != null && configuration.Value != null && configuration.Value.ContainsKey(ApplicationType.GDO_MYHL) &&
                            configuration.Value[ApplicationType.GDO_MYHL].Any())
                        {
                            var myHlConfiguration = configuration.Value[ApplicationType.GDO_MYHL][0] as MyHLConfiguration;
                            if (myHlConfiguration != null)
                            {
                                SaveCountryConfigurationToCache(cacheKey, myHlConfiguration);
                                return myHlConfiguration;
                            }
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        string.Format("GetCountryConfiguration error, Exception: {0}, Inner Exception: {1}",
                                      ex.Message,
                                      (ex.InnerException != null) ? ex.InnerException.Message : string.Empty),
                        "ConfigManager");
                }
                return null;
            }
        }

       private static void SaveCountryConfigurationToCache(string cacheKey, MyHLConfiguration config)
            {
            HttpRuntime.Cache.Insert(cacheKey,
                                       config,
                                       null,
                                       DateTime.Now.AddMinutes(COUNTRY_CONFIGURATION_CACHE_MINUTES),
                                       Cache.NoSlidingExpiration,
                                       CacheItemPriority.NotRemovable,
                                       null);
        }

    }

    /// <summary>
    /// Proxy service provider.
    /// </summary>
    public static class ServiceClientProvider
    {
        public static OrderServiceClient GetOrderServiceProxy()
        {
            return ServiceClientFactory.CreateProxy<OrderServiceClient, IOrderService>(
                Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteUrl"),
                Settings.GetRequiredAppSetting("IAGlobalOrderingQuoteSecureUrl", string.Empty),
                true);
        }
    }
}
