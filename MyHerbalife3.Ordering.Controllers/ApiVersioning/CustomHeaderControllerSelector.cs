using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Net;
using HL.Common.Configuration;

namespace MyHerbalife3.Ordering.Controllers.ApiVersioning
{
    public class CustomHeaderControllerSelector : DefaultHttpControllerSelector
    {
        private static string SUPPORTED_IOS_VERSION = Settings.GetRequiredAppSetting("SupportedIOSVersion", "1.1.37");
        private static string SUPPORTED_ANDROID_VERSION = Settings.GetRequiredAppSetting("SupportedAndroidVersion", "4.4.4");
        private static string IGNORE_API_VERSIONING = Settings.GetRequiredAppSetting("IgnoreApiVersioning", "true");
        private static string IOS_DEVICE = "IOS";
        private static string ANDROID_DEVICE = "ANDROID";
        private static string VERSION = "X-HLVersion";
        private static string DEVICE = "X-HLClientOS";

        public CustomHeaderControllerSelector(HttpConfiguration cfg)
            : base(cfg)
        { }

        public override string GetControllerName(System.Net.Http.HttpRequestMessage request)
        {
            string controllerName = base.GetControllerName(request);

            if (IGNORE_API_VERSIONING == "true")
                return controllerName;

            if (request.Headers.Contains(VERSION) && request.Headers.Contains(DEVICE))
            {
                string headerValue = request.Headers.GetValues(VERSION).First();
                string deviceNameValue = request.Headers.GetValues(DEVICE).First();

                if (!string.IsNullOrEmpty(deviceNameValue) && deviceNameValue.Trim().ToUpper().Contains(IOS_DEVICE))
                {
                    deviceNameValue = IOS_DEVICE;
                }

                if (!string.IsNullOrEmpty(deviceNameValue) && deviceNameValue.Trim().ToUpper().Contains(ANDROID_DEVICE))
                {
                    deviceNameValue = ANDROID_DEVICE;
                }


                if (deviceNameValue == IOS_DEVICE)
                {
                    //IOS Major/Minor and Build Validations
                    string strClientMajor = "0";
                    string strClientMinor = "0";
                    string strClientBuild = "0";
                    string strServiceMajor = "0";
                    string strServiceMinor = "0";
                    string strServiceBuild = "0";

                    if (headerValue != null)
                    {
                        string[] strClientExtracted = headerValue.Split('.');
                        strClientMajor = strClientExtracted != null && strClientExtracted.Count() >= 1 ? strClientExtracted[0] : "0";
                        strClientMinor = strClientExtracted != null && strClientExtracted.Count() > 1 ? strClientExtracted[1] : "0";
                        strClientBuild = strClientExtracted != null && strClientExtracted.Count() > 2 ? strClientExtracted[2] : "0";
                    }

                    if (SUPPORTED_IOS_VERSION != null)
                    {
                        string[] strServiceExtracted = SUPPORTED_IOS_VERSION.Split('.');
                        strServiceMajor = strServiceExtracted != null && strServiceExtracted.Count() >= 1 ? strServiceExtracted[0] : "0";
                        strServiceMinor = strServiceExtracted != null && strServiceExtracted.Count() > 1 ? strServiceExtracted[1] : "0";
                        strServiceBuild = strServiceExtracted != null && strServiceExtracted.Count() > 2 ? strServiceExtracted[2] : "0";
                    }

                    //Major
                    if (Convert.ToInt16(strClientMajor) < Convert.ToInt16(strServiceMajor))
                    {
                        throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_IOS_VERSION, 30483, request);
                    }
                    else if (Convert.ToInt16(strClientMinor) < Convert.ToInt16(strServiceMinor))
                    {
                        throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_IOS_VERSION, 30483, request);
                    }
                    else if (Convert.ToInt16(strClientBuild) < Convert.ToInt16(strServiceBuild))
                    {
                        throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_IOS_VERSION, 30483, request);
                    }
                    
                }
                else
                {
                    //ANDROID
                    //IOS Major/Minor and Build Validations
                    string strClientMajor = "0";
                    string strClientMinor = "0";
                    string strClientBuild = "0";
                    string strServiceMajor = "0";
                    string strServiceMinor = "0";
                    string strServiceBuild = "0";

                    if (headerValue != null)
                    {
                        string[] strClientExtracted = headerValue.Split('.');
                        strClientMajor = strClientExtracted != null && strClientExtracted.Count() >= 1 ? strClientExtracted[0] : "0";
                        strClientMinor = strClientExtracted != null && strClientExtracted.Count() > 1 ? strClientExtracted[1] : "0";
                        strClientBuild = strClientExtracted != null && strClientExtracted.Count() > 2 ? strClientExtracted[2] : "0";
                    }

                    if (SUPPORTED_ANDROID_VERSION != null)
                    {
                        string[] strServiceExtracted = SUPPORTED_ANDROID_VERSION.Split('.');
                        strServiceMajor = strServiceExtracted != null && strServiceExtracted.Count() >= 1 ? strServiceExtracted[0] : "0";
                        strServiceMinor = strServiceExtracted != null && strServiceExtracted.Count() > 1 ? strServiceExtracted[1] : "0";
                        strServiceBuild = strServiceExtracted != null && strServiceExtracted.Count() > 2 ? strServiceExtracted[2] : "0";
                    }

                    //Major
                    if (Convert.ToInt16(strClientMajor) < Convert.ToInt16(strServiceMajor))
                    {
                        throw CreateExceptionAndroid(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_ANDROID_VERSION, 30483, request);
                    }
                    else if (Convert.ToInt16(strClientMinor) < Convert.ToInt16(strServiceMinor))
                    {
                        throw CreateExceptionAndroid(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_ANDROID_VERSION, 30483, request);
                    }
                    else if (Convert.ToInt16(strClientBuild) < Convert.ToInt16(strServiceBuild))
                    {
                        throw CreateExceptionAndroid(HttpStatusCode.HttpVersionNotSupported, "Incompatible Client. Current supported version is " + SUPPORTED_ANDROID_VERSION, 30483, request);
                    }
                }
            }
            else if (!request.Headers.Contains(VERSION) && request.Headers.Contains(DEVICE))
            {
                throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Must specify an API version.Key is X-HLVersion", 30485, request);
            }
            else if (request.Headers.Contains(VERSION) && !request.Headers.Contains(DEVICE))
            {
                throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Must specify the Device Name.Key is X-HLClientOS", 30485, request);
            }
            else if (!request.Headers.Contains(VERSION) && !request.Headers.Contains(DEVICE))
            {
                throw CreateException(HttpStatusCode.HttpVersionNotSupported, "Must specify an API version and the Device Name.Keys are X-HLVersion and X-HLClientOS", 30485, request);
            }
            return controllerName;
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode, HttpRequestMessage request)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(request.CreateErrorResponse(httpStatusCode, error));
        }

        private HttpResponseException CreateExceptionAndroid(HttpStatusCode httpStatusCode, string reasonText, int errorCode, HttpRequestMessage request)
        {
            var error = new HttpError() { { "message", reasonText }, { "code", errorCode } };
            return new HttpResponseException(request.CreateErrorResponse(httpStatusCode, error));
        }

    }
}
