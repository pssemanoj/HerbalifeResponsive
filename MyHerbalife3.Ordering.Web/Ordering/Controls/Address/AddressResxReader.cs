using System.Web;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Text;
using System.IO;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    [Serializable]
    public class AddressResx
    {
        [DataMember]
        [XmlElementAttribute("root", Form = XmlSchemaForm.None)]
        public ResxKeyValue KeyValue { get; set; }
    }

    [Serializable]
    public class ResxKeyValue
    {
        public ResxKeyValue() { }

        [DataMember]
        [XmlElementAttribute("data")]
        public Data [] data { get; set; }
    }

    [Serializable]
    public class Data
    {
        public Data() { }

        [DataMember]
        [XmlAttribute("name")]
        public string name { get; set; }

        [DataMember]
        [XmlAttribute("xml:space")]
        public string attr { get; set; }

        [DataMember]
        [XmlElementAttribute("value")]
        public string value { get; set; }
    }

    public class AddressResxReader
    {
        public AddressResxReader() { }

        public AddressResx GetAddressResx(string locale)
        {
            // Load the localized file, if it does not exist, take the base file.
            string baseFile = ResolveUrl("\\App_Data\\Resources\\Ordering\\App_GlobalResources\\GlobalDOAddressing.resx");
            string xmlFile = ResolveUrl(string.Format("\\App_Data\\Resources\\Ordering\\App_GlobalResources\\GlobalDOAddressing.{0}.resx", locale));
            if (!File.Exists(xmlFile))
            {
                xmlFile = baseFile;
            }

            try
            {
                using (TextReader textReader = new StreamReader(xmlFile))
                {
                    string resxText = textReader.ReadToEnd();
                    int index = resxText.IndexOf("-->");
                    if (index != -1)
                    {
                        resxText = resxText.Substring(index);
                    }

                    index = resxText.IndexOf("<data name=");
                    if (index != -1)
                    {
                        resxText = resxText.Substring(index);
                    }
                    resxText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<AddressResx xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n<root>\r\n" + resxText + "\r\n</AddressResx>";
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(AddressResx));
                    byte[] resxTextBytes = new UTF8Encoding().GetBytes(resxText);
                    return xmlSerializer.Deserialize(new MemoryStream(resxTextBytes)) as AddressResx;
                }
            }
            catch
            {
            }
            return null;
        }

        public string ResolveUrl(string originalUrl)
        {
            if (originalUrl != null && originalUrl.Trim() != "")
            {
                if (originalUrl.StartsWith("/"))
                    originalUrl = "~" + originalUrl;
                else
                    originalUrl = "~/" + originalUrl;

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if (originalUrl == null)
                return null;
            if (originalUrl.IndexOf("://") != -1)
                return originalUrl;
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                return newUrl;
            }
            return originalUrl;
        }
    }
}

