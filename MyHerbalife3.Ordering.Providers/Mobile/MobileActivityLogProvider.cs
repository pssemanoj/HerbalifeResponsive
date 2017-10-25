using System;
using MyHerbalife3.Ordering.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.MobileAnalyticsSvc;

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    /// <summary>
    /// 
    /// </summary>

    public static class MobileActivityLogProvider
    {
        public static int ActivityLog(object request, object response,
            string distributorID,
            bool isSuccess,
            string url,
            string HLHeaders,
            string userAgent,
            string locale)
        {

            try
            {
                var proxy = ServiceClientProvider.GetMobileAnalyticsServiceProxy();
                var requestSvc = new InsertActivityLogRequest
                {
                    ActivityTypeID = 28,
                    PlatformTypeID = 1,
                    EventTypeID = 2,
                    Success = isSuccess,
                    OrderNumber = string.Empty,
                    DistributorID = distributorID,
                    Locale = locale,
                    Protocol = 0,//protocol,
                    OrderRequest = JsonConvert.SerializeObject(request),
                    RawUrl = url,
                    RawHeaders = HLHeaders,
                    RawResponse = JsonConvert.SerializeObject(response),
                    OrderRequestXml = null,
                    OrderResponseXml = null,
                    UserAgent = userAgent ?? string.Empty,
                    AppName = "WebApiOrdering"
                };

                //Clean Credit Card Info
                //requestSvc.OrderRequest = CleanCreditCardInfo(requestSvc.OrderRequest);
                //requestSvc.RawResponse = CleanCreditCardInfo(requestSvc.RawResponse);

                using (proxy)
                {
                    int activityLogId = 0;
                    var proxyResponse = proxy.InsertActivityLog(new InsertActivityLogRequest1(requestSvc)).InsertActivityLogResult;
                    if (proxyResponse.ActivityLogId != null)
                        activityLogId = (int)proxyResponse.ActivityLogId;

                    return activityLogId;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex, "ActivityLog exception");
                return 0;
            }
        }


        public static List<MobileSandboxResponseViewModel> GetSandboxEvents(int page, int pageSize, string distributorId, string locale, string appName, DateTime startDate, DateTime endDate)
        {

            var proxy = ServiceClientProvider.GetMobileAnalyticsServiceProxy();
            var response = new List<MobileSandboxResponseViewModel>();
            try
            {
                var request = new GetActivityLogRequest
                {
                    AppName = appName,
                    StartDate = startDate,
                    EndDate = endDate,
                    Locale = locale,
                    DistributorId = distributorId,
                    Page = page,
                    PageSize = pageSize
                };
                using (proxy)
                {
                    var ActivityLog = proxy.GetActivityLog(new GetActivityLogRequest1(request)).GetActivityLogResult;
                    response = ActivityLog.Select(x => new MobileSandboxResponseViewModel
                    {
                        ActivityLogID = x.activityLogId,
                        ActivityTypeID = x.activityTypeId,
                        ActivityTypeDesc = x.ActivityTypeDesc,
                        AppName = x.AppName,
                        CreatedDate = x.CreatedDate,
                        DistributorId = x.DistributorId,
                        EventTypeID = x.EventTypeID,
                        Locale = x.Locale,
                        OrderNumber = x.OrderNumber,
                        OrderRequest = x.OrderRequest,
                        OrderRequestXml = x.OrderRequestXml,
                        OrderResponseXml = x.OrderResponseXml,
                        PlatformTypeID = x.PlatformTypeID,
                        RawHeaders = x.RawHeaders,
                        RawResponse = x.RawResponse,
                        RawUrl = x.RawUrl,
                        Success = x.Success,
                        UserAgent = x.UserAgent
                    }
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex, "GetSandboxEvents exception");
            }

            return response;
        }

        private static string CleanCreditCardInfo(string request)
        {
            try
            {
            JObject rss = JObject.Parse(request);

            if (rss != null)
            {
                var root = rss.First;
                if (root != null)
                {
                    var rootData = root.First as JObject;
                    if (rootData != null && rootData["Payments"] != null)
                    {
                        var paymentsToken = rootData["Payments"];
                        if (paymentsToken.HasValues)
                        {
                            var Card = paymentsToken.First["Card"];
                            if (Card != null)
                            {
                                var CCNumber = Card["AccountNumber"].ToString();
                                var CCCVV = Card["Cvv"].ToString();
                                if (CCNumber != null && CCCVV != null)
                                {
                                    CCNumber = string.IsNullOrEmpty(CCNumber.ToString()) ? "" : CCNumber.Substring(CCNumber.Length - 4).PadLeft(CCNumber.Length, '*');
                                    CCCVV = string.IsNullOrEmpty(CCCVV.ToString()) ? "" : CCCVV.Substring(CCCVV.Length).PadLeft(CCCVV.Length, '*');
                                    rss.First.First["Payments"].First["Card"]["AccountNumber"] = CCNumber;
                                    rss.First.First["Payments"].First["Card"]["Cvv"] = CCCVV;
                                    var str = JsonConvert.SerializeObject(rss);
                                    return str;
                                }
                            }
                        }
                    }
                }
            }
            return request;
        }
            catch (Exception)
            {
            return request;
        }
    }
    }
}
