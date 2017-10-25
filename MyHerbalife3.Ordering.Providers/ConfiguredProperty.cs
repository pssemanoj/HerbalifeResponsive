using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class ConfiguredProperty
    {
        public object TargetValue { get; set; }
        public string TargetName { get; set; }

        [DataMember]
        [XmlElement("Param")]
        public ConfiguredParam[] Params { get; set; }

        public void Init()
        {
            string targetProperty = string.Empty;
            string configuration = string.Empty;
            string configurationProperty = string.Empty;
            string modifier = string.Empty;
            foreach (ConfiguredParam p in Params)
            {
                switch (p.Name)
                {
                    case "TargetProperty":
                        targetProperty = p.Value;
                        break;
                    case "Configuration":
                        configuration = p.Value;
                        break;
                    case "ConfigurationProperty":
                        configurationProperty = p.Value;
                        break;
                    case "Modifier":
                        modifier = p.Value;
                        break;
                    default:
                        break;
                }
                if (string.IsNullOrEmpty(targetProperty) || string.IsNullOrEmpty(configuration) ||
                    string.IsNullOrEmpty(configurationProperty))
                    continue;
                TargetName = targetProperty;
                var pInfo = HLConfigManager.Configurations.GetType().GetProperty(configuration);
                if (pInfo != null)
                {
                    object value = null;
                    if ((value = pInfo.GetValue(HLConfigManager.Configurations, null)) != null)
                    {
                        pInfo = value.GetType().GetProperty(configurationProperty);
                        if (pInfo != null)
                        {
                            if (!string.IsNullOrEmpty(modifier))
                            {
                                if (modifier == "Not")
                                {
                                    try
                                    {
                                        bool bValue = (bool) pInfo.GetValue(value, null);
                                        TargetValue = !bValue;
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggerHelper.Error(
                                            string.Format(
                                                "ControlConfigLoader -Invalid Modifier {0} for configured property {1}. Property must be boolean.\r\n{2}",
                                                modifier, string.Concat(configuration, ".", configurationProperty),
                                                ex.Message));
                                    }
                                }
                                else
                                {
                                    LoggerHelper.Error(
                                        string.Format(
                                            "ControlConfigLoader -Invalid Boolean Modifier {0} for configured property {1}",
                                            modifier, string.Concat(configuration, ".", configurationProperty)));
                                }
                            }
                            else
                            {
                                TargetValue = pInfo.GetValue(value, null);
                            }
                        }
                    }
                }
            }
        }
    }
}