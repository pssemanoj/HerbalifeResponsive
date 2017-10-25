using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSO.ValueObjects;
using System.Text.RegularExpressions;
using SSO.Common.TokenManager;

namespace MyHerbalife3.Ordering.WebAPI.Provider.SSO
{
    public class SSOProvider
    {
        private static string Applications = HL.Common.Configuration.Settings.GetRequiredAppSetting(Constants.SSOValues.Applications);
        private static bool Enabled = Convert.ToBoolean(HL.Common.Configuration.Settings.GetRequiredAppSetting(Constants.SSOValues.Enabled));
        private static string IkioskLocales = HL.Common.Configuration.Settings.GetRequiredAppSetting(Constants.SSOValues.IkioskLocales);
        private static string Locales = HL.Common.Configuration.Settings.GetRequiredAppSetting(Constants.SSOValues.Locales);

        public static bool UseSSO(string locale, string client)
        {

            if (locale != null && locale.Contains("_"))
                locale = locale.Replace("_", "-");
            var clientName = client != null ? client : string.Empty;

            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(clientName))
                return false;

            if (clientName == "ikiosk")
                return Applications.Contains(clientName.ToUpper())
                    && Enabled
                    && IkioskLocales.ToUpper().Contains(locale.ToUpper());

            return Enabled && Locales.Contains(locale) &&
               Applications.Contains(clientName.ToUpper());
        }

        public static bool IsValidUser(string username)
        {
            if (username.Length < 8)
                return false;

            var regex = new Regex(@"\d{7}");
            if (regex.IsMatch(username))
                return false;

            return true;
        }

        public static bool ValidDistributor(string token)
        {
            bool isValid = false;
            try
            {
                var tokenManager = new TokenManager();
                ProfileClaim claims = tokenManager.GetProfileClaimFromAccessToken(token);
                var distributorIdFromToken = claims.AppUserId ?? string.Empty;
                if (!string.IsNullOrEmpty(distributorIdFromToken)) isValid = true;
            }
            catch
            {

                isValid = false;
            }
            return isValid;
        }

        public static string CurrentDistributor(string token)
        {
            string distributor = string.Empty;
            try
            {
                var tokenManager = new TokenManager();
                
                ProfileClaim claims = tokenManager.GetProfileClaimFromAccessToken(token);
                var distributorIdFromToken = claims.AppUserId ?? string.Empty;
                if (!string.IsNullOrEmpty(distributorIdFromToken)) distributor = distributorIdFromToken;
            }
            catch
            {
                distributor = string.Empty;
            }
            return distributor;
        }


    }
}
