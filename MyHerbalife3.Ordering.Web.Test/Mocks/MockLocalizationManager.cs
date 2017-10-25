#region

using System.Globalization;
using MyHerbalife3.Shared.Infrastructure.Interfaces;

#endregion

namespace MyHerbalife3.Ordering.Web.Test.Mocks
{
    public class MockLocalizationManager : ILocalizationManager
    {
        public string GetString(string virtualPath, string key, CultureInfo culture = null)
        {
            return string.Empty;
        }

        public string GetGlobalString(string resourceName, string key, CultureInfo culture = null)
        {
            return string.Empty;
        }
    }
}