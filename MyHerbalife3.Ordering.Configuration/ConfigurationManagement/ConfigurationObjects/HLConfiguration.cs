using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class HLConfiguration : ConfigurationSection
    {
        public string Locale { get; set; }

        public static ConfigurationSection GetConfiguration(System.Configuration.Configuration config, string SectionName)
        {
            HLConfiguration configSection = null;
            try
            {
                configSection = config.GetSection(SectionName) as HLConfiguration;
                if (null == configSection)
                {
                    throw new ApplicationException(string.Format("{0} Section missing from config file {1}", SectionName,
                                                                 Path.GetFileName(config.FilePath)));
                }
                configSection.Locale = config.AppSettings.Settings["Locale"].Value;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }

            return configSection;
        }

        protected class StringListConverter : ConfigurationConverterBase
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var ret = new List<string>();
                string values = value as string;
                if (!string.IsNullOrEmpty(values))
                {
                    ret = new List<string>(values.Split(new char[] {','}));
                }

                return ret;
            }

            public override object ConvertTo(ITypeDescriptorContext context,
                                             CultureInfo culture,
                                             object value,
                                             Type destinationType)
            {
                return string.Join(",", (value as List<string>).ToArray());
            }
        }
    }
}