using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    /// <summary>Simple access property to shield singleton access</summary>
    public class HLConfigManager
    {
        /// <summary>Returns the current platform's Configurations based on the thread locale</summary>
        public static ConfigurationSet Configurations
        {
            get { return ConfigManager.Instance.Configurations; }
        }

        /// <summary>Provides access to all locale's ConfigurationSets for the current platform</summary>
        public static Dictionary<string, ConfigurationSet> CurrentPlatformConfigs
        {
            get { return ConfigManager.Instance.CurrentPlatformConfigs; }
        }

        /// <summary>Provides access to all locale's ConfigurationSets for the all platforms</summary>
        public static Dictionary<string, Dictionary<string, ConfigurationSet>> AllPatformConfigurations
        {
            get { return ConfigManager.Instance.AllPlatformConfigs; }
        }

        /// <summary>Returns the current platform's Default Configurations based on the thread locale</summary>
        public static ConfigurationSet DefaultConfiguration
        {
            get { return ConfigManager.Instance.CurrentPlatformConfigs["Default"]; }
        }

        /// <summary>Returns the name of the current executing platform</summary>
        public static string Platform
        {
            get { return ConfigManager.Instance.CurrentPlatform; }
        }

        // <summary>
        // List of locations (Pages )that are configured for the current platform.
        // </summary>
        public static Dictionary<string, string> PlatformLocations
        {
            get { return ConfigManager.Instance.PlatformLocations; }
        }

        public static HttpRequest HttpRequest { get; set; }

        /// <summary>
        ///     to get configuration set based on country
        /// </summary>
        /// <param name="countryCode">country code</param>
        /// <returns>configuration set</returns>
        public static ConfigurationSet GetConfigurationByCountry(string countryCode)
        {
            var countryConfig =
                ConfigManager.Instance.CurrentPlatformConfigs.FirstOrDefault(config => config.Key.Contains(countryCode));

            return countryConfig.Value;
        }
    }
}