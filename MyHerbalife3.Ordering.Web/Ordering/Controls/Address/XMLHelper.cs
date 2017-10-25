using System.Web;
using System.Xml.Serialization;
using System.IO;
using MyHerbalife3.Ordering.Web.Ordering.Controls.GlobalAddress;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public class XMLHelper
    {
        public static void SaveAddressFormatToXML(string path, object result)
        {
            FileStream file = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AddressWindow));
                file = new FileStream(path, FileMode.Truncate);
                serializer.Serialize(file, result);
            }
            catch
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

        }
        private static string getFilePath(string path)
        {
            if (!string.IsNullOrEmpty(HttpRuntime.AppDomainId))
            {
                return HttpRuntime.AppDomainAppPath + path;
            }
            return path;
        }
        public static AddressWindow LoadAddressFormatFromFile(string path)
        {
            AddressWindow result = null;
            FileStream file = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GlobalAddress.AddressWindow));
                file = new FileStream(getFilePath(path), FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.RandomAccess);
                result = (AddressWindow)serializer.Deserialize(file);
            }
            catch
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return result;
        }
    }
}
