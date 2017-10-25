#region

using System.Threading;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using System;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileDualOrderMonthController : ApiController
    {
        private readonly IMobileDualOrderMonthProvider _imMobileDualOrderMonthProvider;

        public MobileDualOrderMonthController()
        {
            _imMobileDualOrderMonthProvider = new MobileDualOrderMonthProvider();
        }

        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate(UserIdValidation = false)]
        public DualOrderMonthResponseViewModel Get()
        {
            try
            {
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                var country = locale.Substring(3, 2);
                var result = new DualOrderMonthResponseViewModel();
                var dualOrderMonthModel = _imMobileDualOrderMonthProvider.GetDualOrderMonth(country);
                result.PreviousOrderMonth = dualOrderMonthModel.PreviousOrderMonth;
                result.PreviousOrderMonthEndDate = dualOrderMonthModel.PreviousOrderMonthEndDate;

                MobileActivityLogProvider.ActivityLog(string.Empty, result, string.Empty, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        locale);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            throw new Exception("Method Get on MobileDualOrderMonthController not return values.");
        }
    }
}