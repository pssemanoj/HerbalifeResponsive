using System.Web;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    public class PlatformResources
    {
        ////This is a standin and v. hackey solution for Platform handling of strongly types resources. Only doing this for GlobalDO for now.
        ////This is awaiting the new ResourceManager from Kiel
        //private static Dictionary<string, Dictionary<string, object>> resources = new Dictionary<string, Dictionary<string, object>>();
 
        //static void ctor()
        //{
        //    List<string> loadedPlatforms = HLConfigManager.AllPatformConfigurations.Keys.ToList<string>();
        //    foreach(string platform in loadedPlatforms)
        //    {
        //        resources.Add(platform, new Dictionary<string, object>());
        //        resources[platform].Add("ErrorMessage", Activator.CreateInstance(Type.GetType(string.Format("{0}_{1}", platform, "ErrorMessage"))));
        //        resources[platform].Add("GlobalResources", Activator.CreateInstance(Type.GetType(string.Format("{0}_{1}", platform, "GlobalResources"))));
        //        resources[platform].Add("Rules", Activator.CreateInstance(Type.GetType(string.Format("{0}_{1}", platform, "Rules"))));
        //    }
        //}

        //public static string Resources(string resource, string item)
        //{
        //    string result = string.Empty;
        //    string platform = HLConfigManager.Platform;
        //    result = resources[platform][resource].GetType().InvokeMember(item, BindingFlags.GetProperty, null, resources[platform][resource], null) as string;

        //    return result;
        //}

        /// <summary>
        /// Because MyHL has platforms, this is a simple wrapper to get resource strings from stronly typed resources in App_GlobalResources
        /// and is to be used instead of normal accessor - Resources.Resx.Property
        /// </summary>
        /// <param name="collection">The name of the resource file</param>
        /// <param name="itemName">The item to get</param>
        /// <returns></returns>
        public static string GetGlobalResourceString(string collection, string itemName)
        {
            return HttpContext.GetGlobalResourceObject(string.Format("{0}_{1}" , HLConfigManager.Platform, collection), itemName) as string;
        }
    }
}