using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace MyHerbalife3.Ordering.Configuration.NPSConfiguration
{
    /// <summary>
    /// NPS Configuraion Provider
    /// </summary>
    public class NPSConfigurationProvider
    {
        public const string NPSMappingSectionName = "NPSMapping";

        /// <summary>
        ///     
        /// </summary>
        private readonly Dictionary<string, NPSMapElement> _MappingsCountry =
            new Dictionary<string, NPSMapElement>();

        public NPSConfigurationProvider()
        {
            var configSection = getConfigSection();
            _MappingsCountry = configSection.Mappings.ToDictionary(m => m.Country);
        }

        /// <summary>
        /// get NPSMapElement
        /// </summary>
        /// <param name="Country"></param>
        /// <returns></returns>
        public NPSMapElement GetNPSConfigSection(string Country)
        {
            if (_MappingsCountry != null)
            {
                NPSMapElement element = null;
                if (_MappingsCountry.TryGetValue(Country, out element))
                    return element;
            }
            return new NPSMapElement() { Country = Country, HasPHCharge = true };
        }

        /// <summary>
        /// Get NPSMapElement using the combined value
        /// </summary>
        /// <param name="mappingValue"></param>
        /// <returns></returns>
        public NPSMapElement GetElementByMapping(string mappingValue)
        {
            if (_MappingsCountry != null)
            {
                NPSMapElement element =
                    _MappingsCountry.Values.SingleOrDefault(
                        v => v.MapFrom.Equals(mappingValue, StringComparison.InvariantCultureIgnoreCase));

                return element;
            }

            return null;
        }

        private NPSConfigurationSection getConfigSection()
        {
            NPSConfigurationSection configSection;
            configSection =
                WebConfigurationManager.GetSection(NPSMappingSectionName) as NPSConfigurationSection;
            if (configSection == null)
            {
                configSection =
                    ConfigurationManager.GetSection(NPSMappingSectionName) as NPSConfigurationSection;
                if (configSection == null)
                {
                    throw new ConfigurationErrorsException("Missing NPSConfiguration configuration section");
                }
            }
            return configSection;
        }

    }
}