using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    /// <summary>
    /// Promotion Configuraion Provider
    /// </summary>
    public static class PromotionConfigurationProvider
    {
        public static PromotionCollection GetPromotionCollection(string platform,string locale)
        {
            System.Configuration.Configuration config = null;
            var configMap = new ExeConfigurationFileMap();

            string path = "Configuration/"+platform+"/Promotion/"+locale+"/Promotion.config";
            if (
                File.Exists(HostingEnvironment.ApplicationPhysicalPath + path) )
            {
                string configPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, path);
                configMap.ExeConfigFilename = configPath;
                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                PromotionConfiguration cconfigs = PromotionConfiguration.GetConfiguration(config);
                return cconfigs.Promotions;
            }
            return null;

        }

    }
}
