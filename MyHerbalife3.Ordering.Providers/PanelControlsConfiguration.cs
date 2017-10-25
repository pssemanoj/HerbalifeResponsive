using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;
using MyHerbalife3.Shared.Infrastructure;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    [XmlRoot(Namespace = "http://www.herbalife.com/PanelControlsConfiguration",
        ElementName = "PanelControlsConfiguration")]
    public class PanelControlsConfiguration
    {
        [DataMember]
        [XmlArray("Pages")]
        [XmlArrayItem("Page")]
        public PageConfig[] Pages { get; set; }

        public string ToXML()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(PanelControlsConfiguration));
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                ser.Serialize(writer, this);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(new Exception("SerializeObject ", ex), ProviderPolicies.SYSTEM_EXCEPTION);
                return null;
            }
        }

        public static PanelControlsConfiguration FromXML(string xmlFileName)
        {
            try
            {
                string cacheKey = "PanelConfiguraton_" + Thread.CurrentThread.CurrentCulture.ToString();
                if (HttpContext.Current.Cache[cacheKey] != null)
                {
                    return (PanelControlsConfiguration)HttpContext.Current.Cache[cacheKey];
                }
                string xmlFile = PanelControlConfigLoader.ResolveUrl(xmlFileName);
                CacheDependency dependency = new CacheDependency(xmlFile);

                PanelControlsConfiguration serializableObject = null;

                using (TextReader textReader = new StreamReader(xmlFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PanelControlsConfiguration));
                    serializableObject = xmlSerializer.Deserialize(textReader) as PanelControlsConfiguration;
                }
                HttpContext.Current.Cache.Insert(cacheKey, serializableObject, dependency, Cache.NoAbsoluteExpiration,
                                                 Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                return serializableObject;
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(new Exception("SerializeObject ", ex), ProviderPolicies.SYSTEM_EXCEPTION);
                return null;
            }
        }
    }
}