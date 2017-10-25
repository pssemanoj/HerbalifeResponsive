using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Xml.Serialization;
using HL.Common.Configuration;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    public class HLPlatforms
    {
        [XmlElement]
        public List<Platform> Platforms = new List<Platform>();

        public static HLPlatforms GetPlatforms()
        {
            HLPlatforms result = null;
            string filePath = Settings.GetRequiredAppSetting("PlatformConfigFile", @"App_Data\MyHLPlatforms.xml");

            string platformFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath ?? ".\\", filePath);

            if (!File.Exists(platformFilePath))
            {
                return createPlatforms();
            }

            if (File.Exists(platformFilePath))
            {
                FileStream f = null;
                try
                {
                    var serial = new XmlSerializer(typeof(HLPlatforms));
                    f = File.OpenRead(platformFilePath);
                    result = serial.Deserialize(f) as HLPlatforms;
                }
                catch (Exception ex)
                {
                    string errorMessage = "Could not find or open the HL Platfoms list";
                    LoggerHelper.Exception("ConfigManager", ex,
                                           string.Format("{0},\r\nException: {1}", errorMessage, ex.Message));
                    throw new ApplicationException(errorMessage, ex);
                }
                finally
                {
                    if (null != f)
                    {
                        f.Close();
                    }
                }
            }
            if (null == result)
            {
                string errorMessage = "ConfigManager - Could not find or open the HL Platfoms list";
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            return result;
        }

        private static HLPlatforms createPlatforms()
        {
            HLPlatforms result = new HLPlatforms();
            Platform pl = new Platform();
            pl.Active = true;
            pl.Default = true;
            pl.Name = "MyHL";
            result.Platforms.Add(pl);
            return result;
        }
    }

    public class Platform
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("isActive")]
        public bool Active { get; set; }

        [XmlAttribute("isDefault")]
        public bool Default { get; set; }
    }
}