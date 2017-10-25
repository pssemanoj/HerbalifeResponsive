using System.Net;
using Newtonsoft.Json;

namespace MyHerbalife3.Ordering.WebAPI.Model
{
    public class Error
    {
        public static readonly Error UnknownError = new Error(3000, "Unknown error ocurred.", HttpStatusCode.BadRequest);
        public static readonly Error TransportError = new Error(3010, "Network Not Available", HttpStatusCode.BadRequest);
        public static readonly Error URLConnectionError = new Error(3011, "Could not connect to server", HttpStatusCode.BadRequest);
        public static readonly Error TimeOut = new Error(3012, "Network Operation Timeout", HttpStatusCode.BadRequest);
        public static readonly Error EmptyServerResponse = new Error(3013, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error BadResponseData = new Error(3014, "Unknown error in dependent service", HttpStatusCode.BadRequest);
        public static readonly Error RequestCancelled = new Error(3015, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error TransportErrorMarker = new Error(3019, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error HTTPBadRequest = new Error(3400, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error DeviceNotAuthorized = new Error(3401, "Device is not authorized.", HttpStatusCode.BadRequest);
        public static readonly Error HTTPAuthAccessFailed = new Error(3403, "Could not authenticate. Invalid Distributor ID or PIN code", HttpStatusCode.Forbidden);
        public static readonly Error HTTPAuthFailed = new Error(3404, "Could not authenticate.", HttpStatusCode.Forbidden);
        public static readonly Error HTTPSSOValidationType = new Error(3405, "DSID Login attempted on SSO enabled locale", HttpStatusCode.Forbidden);
        public static readonly Error HTTPSSOAccountLocked = new Error(3406, "Account locked", HttpStatusCode.Forbidden);
        public static readonly Error APFDue = new Error(3407, "User's APF is due and can't login", HttpStatusCode.Forbidden);
        public static readonly Error AuthForDeletedMember = new Error(3410, "Login failed due to isDeleted member status rule in locale", HttpStatusCode.Forbidden);
        public static readonly Error AuthBlacklisted = new Error(3411, "Member is blacklisted in the current locale", HttpStatusCode.Forbidden);
        public static readonly Error HTTPServerError = new Error(3500, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error ProtocolError = new Error(3600, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error ObjectValidationFailure = new Error(3601, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error LoggedInDisconnected = new Error(3602, "Server is currently unavailable", HttpStatusCode.BadRequest);
        public static readonly Error NoError = new Error(0, "no error", HttpStatusCode.OK);
        public static readonly Error InvalidToken = new Error(99403, "Auth Token is invalid.", HttpStatusCode.Forbidden);
        public static readonly Error InvalidHeader = new Error(203, "Request Header is missing", HttpStatusCode.BadRequest);

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty("data")]
        public dynamic Data { get; set; }

        public Error(int code, string message, HttpStatusCode statusCode)
        {
            Code = code;
            Message = message;
            StatusCode = statusCode;
        }

        public Error()
        { }
    }
}