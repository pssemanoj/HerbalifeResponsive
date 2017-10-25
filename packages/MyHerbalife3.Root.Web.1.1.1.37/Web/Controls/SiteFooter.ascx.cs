using HL.Common.Logging;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.UI.SiteMapProvider;
using System;
using System.Globalization;
using System.Linq;

namespace HL.MyHerbalife.Web.Controls
{
    public partial class SiteFooter : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var allCountries = (new LocaleCountryLoader()).Load().ToList();

            _CountryLanguage.Text = allCountries.Where(c => c.Key == CultureInfo.CurrentUICulture.Name).Select(k => k.Value).FirstOrDefault();
            
            SiteMapHelper.SetFooterSitemap(_FooterNavDataSource);
        }

        /// <summary>
        ///     "Safely" gets a local resource for the given key.
        ///     Returns a "content missing" message if the resource is not found.
        ///     Additionally, a warning will be placed in the event log.
        /// </summary>
        public new string GetLocalResourceString(string key)
        {
            return GetLocalResourceString(key, "Content Missing: " + key);
        }

        /// <summary>
        ///     "Safely" gets a local resource for the given key.
        ///     Returns the provided default value, if the resource is not found.
        ///     Additionally, a warning will be placed in the event log.
        /// </summary>
        protected new string GetLocalResourceString(string key, string defaultValue)
        {
            string resourceString;

            try
            {
                object value;
                value = GetLocalResourceObject(key);
                if (value == null || !(value is string))
                {
                    throw new ApplicationException(string.Format("Missing local resource object. Key: {0}", key));
                }

                resourceString = value as string;
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn(ex.ToString());
                resourceString = defaultValue;
            }

            return resourceString;
        }
    }
}