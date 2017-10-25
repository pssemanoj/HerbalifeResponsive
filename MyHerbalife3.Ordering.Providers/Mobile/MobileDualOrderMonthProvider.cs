#region

using System;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.ViewModel.Model;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileDualOrderMonthProvider : IMobileDualOrderMonthProvider
    {
        public DualOrderMonthViewModel GetDualOrderMonth(string country)
        {
            if (string.IsNullOrEmpty(country))
            {
                LoggerHelper.Error("country is null - MobileDualOrderMonthController");
                return null;
            }

            try
            {
                var _businessSettings = new DualOrderMonthProvider(new DualOrderMonthLoader());
                var result = new DualOrderMonthResponseViewModel();
                var localNow = DateUtils.GetCurrentLocalTime(country);
                var isDualOrderMonth = _businessSettings.IsDualOrderMonth(country, localNow);
                var dualOrderMonthModel = new DualOrderMonthViewModel
                {
                    PreviousOrderMonth = localNow.Month - 1,
                };

                if (isDualOrderMonth)
                {
                    var monthEndDate = _businessSettings.GetDualMonthEndValue(country, localNow);
                    dualOrderMonthModel.PreviousOrderMonthEndDate = null != monthEndDate
                        ? monthEndDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")
                        : "calendar";
                }
                else
                {
                    dualOrderMonthModel.PreviousOrderMonthEndDate = "calendar";
                }
                return dualOrderMonthModel;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex, "error in DualOrderMonthViewModel GetDualOrderMonth");
                return null;
            }
        }
    }
}