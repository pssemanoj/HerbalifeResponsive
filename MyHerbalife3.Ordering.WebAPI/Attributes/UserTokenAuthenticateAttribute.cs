using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using HL.Common.Logging;
using MyHerbalife3.Ordering.WebAPI.Interfaces;
using MyHerbalife3.Ordering.WebAPI.Model.HttpActionResults;
using MyHerbalife3.Ordering.WebAPI.Security;

namespace MyHerbalife3.Ordering.WebAPI.Attributes
{
    /// <summary>
    /// UserTokenAuthenticateAttribute class
    /// </summary>
    public class UserTokenAuthenticateAttribute : Attribute, IAuthenticationFilter
    {
        public UserTokenAuthenticateAttribute()
        {
            UserIdValidation = true;
        }

        public bool UserIdValidation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowMultiple
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            IHttpActionResult result;
            var request = context.Request;
            var headers = request.Headers;

            // No headers or missing User token
            if (null != headers)
            {
                if (!headers.Contains(Constants.RequestHeaders.UserToken))
                {
                    LoggerHelper.Warn("User Token header is missing");
                    result = new ErrorResult(Model.Error.InvalidToken, request);
                    context.ErrorResult = result;

                    return Task.FromResult(result);
                }

                if (!headers.Contains(Constants.RequestHeaders.Locale))
                {
                    LoggerHelper.Warn("Locale header is missing");
                    result = new ErrorResult(Model.Error.InvalidHeader, request);
                    context.ErrorResult = result;

                    return Task.FromResult(result);
                }

                if (!headers.Contains(Constants.RequestHeaders.Client))
                {
                    LoggerHelper.Warn("Client header is missing");
                    result = new ErrorResult(Model.Error.InvalidHeader, request);
                    context.ErrorResult = result;

                    return Task.FromResult(result);
                }
            }

            var token = headers.GetValues(Constants.RequestHeaders.UserToken).First();
            string locale = headers.GetValues(Constants.RequestHeaders.Locale).First();
            var clientQuery = headers.GetValues(Constants.RequestHeaders.Client).First();
            var clientValues = System.Web.HttpUtility.ParseQueryString(clientQuery);
            string client = clientValues["name"];

            //Check token against auth service.
            IUserTokenValidator validator = new LegacyUserTokenValidator();
            var requestDsId = UserIdValidation ? context.ActionContext.Request.GetRouteData().Values["memberId"] as string : null;
            var validToken = validator.ValidateToken(token, UserIdValidation, requestDsId, locale, client);

            if (validToken)
                return Task.FromResult(new OkResult(request));


            LoggerHelper.Warn("User Token header is Invalid");
            result = new ErrorResult(Model.Error.InvalidToken, request);
            context.ErrorResult = result;

            return Task.FromResult(result);

        }
    }
}
