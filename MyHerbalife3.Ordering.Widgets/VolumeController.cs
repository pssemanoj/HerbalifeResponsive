using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;

namespace MyHerbalife3.Ordering.Widgets
{
    public class VolumeController : ApiController
    {
        internal static volatile IVolumeSource VolumeSource;

        internal readonly ILocalizationManager LocalizationManager;

        public VolumeController()
            : this(new LocalizationManager())
        {
        }

        public VolumeController(ILocalizationManager localizationManager)
            : this(localizationManager, VolumeSource)
        {
        }

        public VolumeController(ILocalizationManager localizationManager, IVolumeSource volumeSource)
        {
            LocalizationManager = localizationManager;
            VolumeSource = volumeSource;
        }

        public static void Inject(IVolumeSource volumeSource)
        {
            VolumeSource = volumeSource;
        }

        [Authorize]
        [WebApiCultureSwitching]
        public async Task<List<VolumeModel>> Get()
        {
            return await Get(User.Identity.Name);
        }

        internal async Task<List<VolumeModel>> Get(string id)
        {
            var result = new List<VolumeModel>();
            try
            {
                var model = await VolumeSource.GetVolume(id);

                if (null != model && model.Count > 0)
                {
                    foreach (var volumeModel in model)
                    {
                        var pattern = LocalizationManager.GetGlobalString("HrblUI", "VolumeHeaderStringPattern");

                        var monthName = CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(volumeModel.VolumeMonth);

                        volumeModel.MonthName = monthName;

                        volumeModel.HeaderText = string.Format(CultureInfo.CurrentUICulture, pattern, volumeModel.HeaderVolume);

                        result.Add(volumeModel);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading volume for id" + id);

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = "Error while loading volume for " + id });
            }

            if (result.Count == 0)
            {
                LoggerHelper.Info("No volume data for " + id);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "No data found for " + id });
            }

            return result;
        }
    }
}