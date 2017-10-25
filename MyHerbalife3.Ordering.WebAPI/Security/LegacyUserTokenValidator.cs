using MyHerbalife3.Core.AuthenticationProvider.AuthenticationSvc;
using MyHerbalife3.Ordering.WebAPI.Interfaces;
using SSO.Common.TokenManager;
using SSO.ValueObjects;

namespace MyHerbalife3.Ordering.WebAPI.Security
{
    public class LegacyUserTokenValidator : IUserTokenValidator
    {
        public bool ValidateToken(string token, bool validateUserId, string userId, string locale, string client)
        {
            bool isValid = false;

            try
            {
                if (Provider.SSO.SSOProvider.UseSSO(locale, client))
                {
                    if (Provider.SSO.SSOProvider.IsValidUser(userId))
                    {
                        isValid = Provider.SSO.SSOProvider.ValidDistributor(token);
                    }
                    else if (client.ToLower().Equals("ikiosk"))
                    {
                        using (AuthenticationServiceClient proxy = MyHerbalife3.Core.AuthenticationProvider.Providers.UserAuthenticator.GetAuthServiceClient(true))
                        {
                            var request = new GetDistributorIDByAuthTokenRequest_V02 { Token = token, IsCnMobileAuthToken = true };
                            var response = proxy.GetDistributorIDByAuthToken(request) as GetDistributorIDByAuthTokenResult_V02;

                            if (response != null)
                            {
                                if (response.Status == ServiceResponseStatusType.Success && (!validateUserId || response.DistributorID == userId))
                                {
                                    return isValid = true;
                                }
                            }
                        }

                    }
                }
                else
                {
                    using (AuthenticationServiceClient proxy = MyHerbalife3.Core.AuthenticationProvider.Providers.UserAuthenticator.GetAuthServiceClient(true))
                    {
                        var request = new GetDistributorIDByAuthTokenRequest_V02 { Token = token, IsCnMobileAuthToken = true };
                        var response = proxy.GetDistributorIDByAuthToken(request) as GetDistributorIDByAuthTokenResult_V02;

                        if (response != null)
                        {
                            if (response.Status == ServiceResponseStatusType.Success && (!validateUserId || response.DistributorID == userId))
                            {
                                return isValid = true;
                            }
                        }
                    }
                }

                return isValid;
            }
            catch { return isValid; }
        }
    }
}
