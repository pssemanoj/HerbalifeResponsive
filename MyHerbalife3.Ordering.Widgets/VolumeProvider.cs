using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HL.Blocks.Caching.SimpleCache;
using HL.Blocks.CircuitBreaker;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.ViewModel.Requests;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Widgets
{
    public class VolumeProvider : ILoader<List<VolumeModel>, GetVolumeById>
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();
        private static ILocalizationManager _localization = new LocalizationManager();

        public const int VolumeModelCacheMinutes = 15;
        public List<VolumeModel> Load(GetVolumeById getVolumeById)
        {
            if (getVolumeById == null)
            {
                throw new ArgumentNullException("getVolumeById");
            }

            if (string.IsNullOrWhiteSpace(getVolumeById.Id))
            {
                throw new ArgumentException("Id is blank", "getVolumeById");
            }

            var cacheKey = string.Format("VOL_{0}_{1}", getVolumeById.Id, Thread.CurrentThread.CurrentUICulture.Name);

            var result = _cache.Retrieve(_ => LoadFromService(getVolumeById.Id), cacheKey, TimeSpan.FromMinutes(VolumeModelCacheMinutes));

            return result ?? new List<VolumeModel>();
        }

        internal List<VolumeModel> LoadFromService(string distributorId)
        {
            var proxy = GetProxy();

            try
            {
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetDistributorVolumeResponse_V01>();

                var countryCode = Thread.CurrentThread.CurrentUICulture.Name.Substring(3);
                var currentLocalTime = DateUtils.GetCurrentLocalTime(countryCode);
                var getVolumeRequest = new GetDistributorVolumeRequest_V01
                {
                    DistributorID = distributorId,
                    StartDate = DateTimeUtils.GetFirstDayOfMonth(currentLocalTime.AddMonths(-1)),
                    EndDate = DateTimeUtils.GetLastDayOfMonth(currentLocalTime)
                };
                var response =
                    circuitBreaker.Execute(
                        () => proxy.GetDistributorVolumePoints(new GetDistributorVolumePointsRequest(getVolumeRequest)).GetDistributorVolumePointsResult as GetDistributorVolumeResponse_V01);

                if (response == null)
                {
                    LoggerHelper.Error("Null response was returned for " + distributorId);
                    return null;
                }

                if (response.Status == ServiceResponseStatusType.Success)
                {
                    return GetVolumeViewModel(response.VolumePoints, countryCode, distributorId);
                }

                LoggerHelper.Error(
                    string.Format(
                        "VolumeProvider.LoadFromService Error. Unsuccessful result from web service. Status: {0}",
                        response.Status));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Exception occured Load Volume method {0}\n{1}",
                                                 distributorId, ex));
                throw;
            }
            finally
            {
                if (null != proxy)
                {
                    proxy.Close();
                }
            }
            return null;
        }

        private static DistributorServiceClient GetProxy()
        {
            return ServiceProvider.ServiceClientProvider.GetDistributorServiceProxy();
        }

        private static List<VolumeModel> GetVolumeViewModel(IEnumerable<DistributorVolume_V01> volumePoints,
                                                            string countryCode, string id)
        {
            if (null == volumePoints)
            {
                LoggerHelper.Error(string.Format("volumePoints is null in the method GetVolumeViewModel  {0}\n{1}",
                                                 id, countryCode));
                return null;
            }
            try
            {
                var distributorProvider = new Core.DistributorProvider.DistributorProfileLoader();
                var distributorModel = distributorProvider.Load(new GetDistributorProfileById { Id = id });
                var currentMonth = DateUtils.GetCurrentLocalTime(countryCode).Month;
                var volumeModels = volumePoints.Select(distributorVolumeV01 =>
                {
                    var volumeMonth = GetVolumeMonth(distributorVolumeV01.VolumeMonth);
                    return new VolumeModel
                    {
                        DV = ExtractValue(distributorVolumeV01.DownlineVolume, 0.0M),
                        GV = ExtractValue(distributorVolumeV01.GroupVolume, 0.0M),
                        PPV = ExtractValue(distributorVolumeV01.PersonallyPurchasedVolume, 0.0M),
                        PV = ExtractValue(distributorVolumeV01.Volume, 0.0M),
                        TV = ExtractValue(distributorVolumeV01.TotalVolume, 0.0M),
                        HeaderVolume = GetHeaderVolume(distributorVolumeV01, distributorModel.TypeCode),
                        VolumeMonth = volumeMonth,
                        IsCurrentMonth = currentMonth == volumeMonth,
                        PPVText = _localization.GetGlobalString("HrblUI", "VolumePPVText"),
                        DVText = _localization.GetGlobalString("HrblUI", "VolumeDVText"),
                        TVText = _localization.GetGlobalString("HrblUI", "VolumePVText"),
                        PVText = _localization.GetGlobalString("HrblUI", "VolumePVText"),
                        GVText = _localization.GetGlobalString("HrblUI", "VolumeGVText")
                    };
                }).ToList();


                ILoader<Dictionary<DateTime, DateTime>, string, DateTime> dualMonthLoader = new DualOrderMonthLoader();
                var settings = new DualOrderMonthProvider(new DualOrderMonthLoader());
                var localNow = DateUtils.GetCurrentLocalTime(countryCode);

                var isDualOrderMonth = settings.IsDualOrderMonth(countryCode, localNow);
                return !isDualOrderMonth
                           ? volumeModels.Where(v => v.VolumeMonth == currentMonth).ToList()
                           : volumeModels;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Exception occured GetVolumeViewModel method {0}\n{1}",
                                                 id, ex));
                throw;
            }
        }

        private static int GetVolumeMonth(string volumeMonth)
        {
            int month;
            var monthValue = volumeMonth.Substring(5);
            int.TryParse(monthValue, out month);
            return month;
        }

        /// <summary>
        ///     Computes header volume value to display based on distributor type
        /// </summary>
        /// <param name="distributorVolumeV01"></param>
        /// <param name="distributorType"></param>
        /// <returns></returns>
        public static decimal GetHeaderVolume(DistributorVolume_V01 distributorVolumeV01, string distributorType)
        {
            return "SP".Equals(distributorType, StringComparison.InvariantCultureIgnoreCase)
                       ? distributorVolumeV01.Volume + distributorVolumeV01.GroupVolume
                       : distributorVolumeV01.PersonallyPurchasedVolume;
        }

        private static TV ExtractValue<T, TV>(T input, TV defaultValue)
        {
            return null != input
                       ? (TV)Convert.ChangeType(input, typeof(TV))
                       : defaultValue;
        }
    }
}