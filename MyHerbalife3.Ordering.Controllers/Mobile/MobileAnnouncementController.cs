#region

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    /// <summary>
    ///     Announcement controller
    /// </summary>
    [CustomResponseHeader]
    public class MobileAnnouncementController : ApiController
    {
        internal IMobileAnnouncementProvider _IMobileAnnouncementProvider;

        /// <summary>
        ///     ctor
        /// </summary>
        public MobileAnnouncementController()
        {
            _IMobileAnnouncementProvider = new MobileAnnouncementProvider();
        }

        /// <summary>
        ///     Get announcement data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate(UserIdValidation = false)]
        public MobileResponseWrapper Get([FromUri] AnnouncementRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "request is null", 500404);
            }
            string obj = JsonConvert.SerializeObject(request);
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;

            var response = new MobileResponseWrapper
            {
                Data = _IMobileAnnouncementProvider.GetAnnouncement(request)
            };
            JObject json = JObject.Parse(obj);
            MobileActivityLogProvider.ActivityLog(json, response, string.Empty, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

            return response;
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }

    }
}