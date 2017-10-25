#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using System.Runtime.Serialization;
using MyHerbalife3.Ordering.ViewModel.Model.OrderTracking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileOrderTrackingProvider : IMobileOrderTrackingProvider
    {
        public List<ExpressTrackingInfoViewModel> Get(OrderTrackingRequestViewModel request)
        {
            var expressInfo = China.OrderProvider.GetExpressTrackInfo(request.OrderId);
            if (null == expressInfo)
            {
                return null;
            }
            var expressInfoDetail = China.OrderProvider.GetExternalExpressTrackInfo(expressInfo.ExpressCode,
                expressInfo.ExpressNum);

            return ModelConverter.ConvertExpresInfoToViewModel(expressInfo, expressInfoDetail);
        }
    }

    internal class ModelConverter
    {
        public static List<ExpressTrackingInfoViewModel> ConvertExpresInfoToViewModel(
            GetExpressTrackResponse_V01 expressInfoResponse, string expressInfoDetail)
        {
            List<ExpressTrackingInfoViewModel> expressInfoList = new List<ExpressTrackingInfoViewModel>();
            ExpressTrackingInfoViewModel expressInfo = new ExpressTrackingInfoViewModel();
            if (expressInfoResponse != null)
            {
                expressInfo.ExpressCode = expressInfoResponse.ExpressCode;
                expressInfo.ExpressCompanyName = expressInfoResponse.ExpressCompanyName;
                expressInfo.ExpressNum = expressInfoResponse.ExpressNum;

                if (!string.IsNullOrEmpty(expressInfoDetail))
                {


                    switch (expressInfo.ExpressCode)
                    {
                        case "YUNDA":
                            expressInfo.ExpressTracking = Deserialize<ExpressTrackingViewModel>(expressInfoDetail);
                            break;
                        case "BESTWAY":
                            var bestwayModel = Deserialize<BestwayTrackingViewModel>(expressInfoDetail);
                            expressInfo.ExpressTracking = ConvertFromBestway(bestwayModel, expressInfoResponse);
                            break;
                        case "FEDEX":
                            var FedExModel = ConvertFedExTracking(expressInfoDetail);
                            expressInfo.ExpressTracking = ConvertFromFedEx(FedExModel, expressInfoResponse);
                            break;
                        case "SF":
                            var sfModel = Deserialize<SFTrackingViewModel>(expressInfoDetail);
                            expressInfo.ExpressTracking = ConvertFromSfModel(sfModel, expressInfoResponse);
                            break;
                    }
                }

                expressInfo.OrderDeliveryType = expressInfoResponse.OrderDeliveryType;
                expressInfo.ReceivingName = expressInfoResponse.ReceivingName;
                expressInfo.ReceivingPhone = expressInfoResponse.ReceivingPhone;
                expressInfo.TotalPackageUnits = expressInfoResponse.TotalPackageUnits;
            }
            expressInfoList.Add(expressInfo);
            return expressInfoList;
        }

        public static T Deserialize<T>(string serializedConfiguration)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof (T));
                var buffer = Encoding.Unicode.GetBytes(serializedConfiguration);
                var stream = new MemoryStream(buffer);
                var result = (T) serializer.ReadObject(stream);
                stream.Close();
                return result;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
                return default(T);
            }
        }

        public static FedExTrackingViewModel ConvertFedExTracking(string fedExTracking)
        {
            FedExTrackingViewModel model = new FedExTrackingViewModel();
            List<activity> activities = new List<activity>();
            detail det = new detail();
            tracking tracking = new tracking();

            if (fedExTracking.Length > 50)
            {
                JObject request = JObject.Parse(File.ReadAllText(fedExTracking));
                request.Remove("?xml");

                var activityTokens = request["tracking"]["detail"]["activities"].Children();
                var detailTokens = request["tracking"]["detail"].Children();

                foreach (JToken child in activityTokens.Children())
                {
                    foreach (JToken grandChild in child)
                    {
                        activity act = new activity();
                        foreach (JToken grandGrandChild in grandChild)
                        {
                            var property = grandGrandChild as JProperty;

                            if (property != null)
                            {
                                if (property.Name == "datetime")
                                    act.datetime = property.Value != null ? property.Value.ToString() : "";

                                if (property.Name == "scan")
                                    act.scan = property.Value != null ? property.Value.ToString() : "";

                                if (property.Name == "location")
                                    act.location = property.Value != null ? property.Value.ToString() : "";

                                if (property.Name == "details")
                                    act.details = property.Value != null ? property.Value.ToString() : "";
                            }
                        }
                        activities.Add(act);
                    }
                }


                foreach (JToken child in detailTokens)
                {
                    var property = child as JProperty;
                    if (property != null)
                    {
                        if (property.Name == "tn")
                            det.tn = property.Value.ToString();
                        if (property.Name == "ptn")
                            det.ptn = property.Value != null ? property.Value.ToString() : "";
                        if (property.Name == "destination")
                            if (property.Value != null) det.destination = property.Value.ToString();
                        if (property.Name == "shipdate")
                            if (property.Value != null) det.shipdate = property.Value.ToString();
                        if (property.Name == "sentby")
                            if (property.Value != null) det.sentby = property.Value.ToString();
                        if (property.Name == "deliveredto")
                            if (property.Value != null) det.deliveredto = property.Value.ToString();
                        if (property.Name == "signedforby")
                            if (property.Value != null) det.signedforby = property.Value.ToString();
                        if (property.Name == "service")
                            if (property.Value != null) det.service = property.Value.ToString();
                        if (property.Name == "deliverydate")
                            if (property.Value != null) det.deliverydate = property.Value.ToString();
                        if (property.Name == "status")
                            if (property.Value != null) det.status = property.Value.ToString();
                    }
                }

            }
            det.activities = activities;

            tracking.detail = det;

            model.tracking = tracking;
            
            return model;
        }

        public static ExpressTrackingViewModel ConvertFromBestway(BestwayTrackingViewModel bestwayModel,GetExpressTrackResponse_V01 expressInfoResponse)
        {
            ExpressTrackingViewModel expressTrack = new ExpressTrackingViewModel();
            List<steps> expressTrackSteps = new List<steps>();
            TrackingStatus status = TrackingStatus.PreparingShipment;

            if(bestwayModel!=null && bestwayModel.traceLogs!=null&& bestwayModel.traceLogs.Count>0)
            expressTrack.mailno = bestwayModel.traceLogs[0].mailNo;
            if(expressInfoResponse!=null && !string.IsNullOrEmpty(expressInfoResponse.ExpressCompanyName))
            expressTrack.remark = expressInfoResponse.ExpressCompanyName;

            expressTrack.result = "false";


            if (bestwayModel != null && bestwayModel.traceLogs != null && bestwayModel.traceLogs.Count > 0 && bestwayModel.traceLogs[0].traces != null && bestwayModel.traceLogs[0].traces.Count >= 1)
            {
                expressTrack.result = "true";
                expressTrack.time = bestwayModel.traceLogs[0].traces[0].acceptTime;
                status = TrackingStatus.Delivered;
            }


            if (bestwayModel != null && bestwayModel.traceLogs != null && bestwayModel.traceLogs.Count > 0)
            {
                var traceses = bestwayModel.traceLogs[0].traces;
                if (traceses != null)
                    foreach (var trace in traceses)
                    {
                        steps eachStep = new steps();
                        if (trace.scanType == "收件")
                        {
                            status = TrackingStatus.Shipped;
                        }
                        else if (trace.scanType == "签收")
                        {
                            status = TrackingStatus.Delivered;
                        }
                        else
                        {
                            status = TrackingStatus.InTransit;
                        }
                        eachStep.time = trace.acceptTime;
                        eachStep.address = trace.acceptAddress;
                        eachStep.status = trace.scanType;
                        eachStep.remark = trace.remark;
                        eachStep.station = "";
                        eachStep.station_phone = "";
                        eachStep.next = "";
                        eachStep.next_name = "";
                        expressTrackSteps.Add(eachStep);
                    }
            }

            expressTrack.status = getNumberValue(status).ToString();
            expressTrack.steps = expressTrackSteps;
            return expressTrack;
        }

        public static ExpressTrackingViewModel ConvertFromFedEx(FedExTrackingViewModel fedExModel, GetExpressTrackResponse_V01 expressInfoResponse)
        {
            ExpressTrackingViewModel expressTrack = new ExpressTrackingViewModel();
            List<steps> expressTrackSteps = new List<steps>();
            TrackingStatus status = TrackingStatus.PreparingShipment;

            expressTrack.mailno = fedExModel.tracking.detail.tn;
            expressTrack.remark = expressInfoResponse.ExpressCompanyName;
            expressTrack.result = "false";

            if (fedExModel.tracking.detail.activities.Count >= 1)
            {
                expressTrack.result = "true";
                status = TrackingStatus.InTransit;
            }

            if (fedExModel.tracking.detail.status == "已取件")
            {
                //Shipped
                status = TrackingStatus.Shipped;
                expressTrack.time = fedExModel.tracking.detail.shipdate;
            }
            else if (fedExModel.tracking.detail.status == "已送达")
            {
                status = TrackingStatus.Delivered;
                expressTrack.time = fedExModel.tracking.detail.deliverydate;
            }
            else
            {
                status = TrackingStatus.InTransit;
                expressTrack.time = fedExModel.tracking.detail.deliverydate;
            }


            if (fedExModel.tracking.detail.activities != null && fedExModel.tracking.detail.activities.Count >= 1)
            {
                foreach (var trace in fedExModel.tracking.detail.activities)
                {
                    var eachstep = new steps
                        {
                            time = trace.datetime,
                            address = trace.scan,
                            station = trace.location,
                            station_phone = string.Empty,
                            status = string.Empty,
                            remark = trace.details,
                            next = string.Empty,
                            next_name = string.Empty
                        };
                    expressTrackSteps.Add(eachstep);
                }
            }

            expressTrack.status = getNumberValue(status).ToString();
            expressTrack.steps = expressTrackSteps;

            return expressTrack;
        }

        public static ExpressTrackingViewModel ConvertFromSfModel(SFTrackingViewModel sfModel, GetExpressTrackResponse_V01 expressInfoResponse)
        {
            ExpressTrackingViewModel expressTrack = new ExpressTrackingViewModel();
            List<steps> expressTrackSteps = new List<steps>();
            TrackingStatus status = TrackingStatus.PreparingShipment;
            List<Route> traceses;
            if (expressInfoResponse != null)
                expressTrack.remark = expressInfoResponse.ExpressCompanyName;

            expressTrack.result = "false";

            if (sfModel.Response != null && sfModel.Response.Body != null && sfModel.Response.Body.RouteResponse != null && sfModel.Response.Body.RouteResponse.Route != null && sfModel.Response.Body.RouteResponse.Route.Count >= 1)
            {
                expressTrack.mailno = sfModel.Response.Body.RouteResponse.mailno;
                expressTrack.result = "true";
                expressTrack.time = sfModel.Response.Body.RouteResponse.Route[0].accept_time;
                status = TrackingStatus.InTransit;
            }
            if (sfModel.Response != null && sfModel.Response.Body != null &&
                sfModel.Response.Body.RouteResponse != null && sfModel.Response.Body.RouteResponse.Route != null)
            {
                traceses = sfModel.Response.Body.RouteResponse.Route;
            }
            else
            {
                traceses = null;
            }

            if (traceses != null)
                foreach (var trace in traceses)
                {
                    steps eachStep = new steps();
                    if (trace.opcode == "50")
                    {
                        status = TrackingStatus.Shipped;
                    }
                    else if (trace.opcode == "8000")
                    {
                        status = TrackingStatus.Delivered;
                    }
                    else
                    {
                        status = TrackingStatus.InTransit;
                    }
                    eachStep.time = trace.accept_time;
                    eachStep.address = trace.accept_address;
                    eachStep.status = trace.opcode;
                    eachStep.remark = trace.remark;
                    eachStep.station = "";
                    eachStep.station_phone = "";
                    eachStep.next = "";
                    eachStep.next_name = "";
                    expressTrackSteps.Add(eachStep);
                }

            expressTrack.status = getNumberValue(status).ToString();
            expressTrack.steps = expressTrackSteps;

            return expressTrack;
        }

        //public static string PopulateData()
        //{
        //    JObject request = JObject.Parse(File.ReadAllText(@"D:\DTS\App\Web\MyHerbalife3\MyHerbalife3.Ordering\QA\Integration\MyHerbalife3.Ordering.Web\TestExpressInfo.txt"));
        //    return request.ToString(); 
        //}

        public enum TrackingStatus
        {
            PreparingShipment = 0,
            Shipped = 1,
            Delivered = 2,
            InTransit = 3,
        }

        public static int getNumberValue(TrackingStatus thisStatus)
        {
                return (int)thisStatus;
        }
        

    }
}