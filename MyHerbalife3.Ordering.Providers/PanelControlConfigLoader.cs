using System;
using System.Web;

namespace MyHerbalife3.Ordering.Providers
{
    public class PanelControlConfigLoader
    {
        public static string ResolveUrl(string originalUrl)
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
                else
                    throw new ArgumentException("Invalid URL: Relative URL not allowed.");
                return newUrl;
            }
            return originalUrl;
        }
    }
}