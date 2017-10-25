using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Shared.Interfaces;
using Log = HL.Common.Logging;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public class DualOrderMonthProvider
    {
        private readonly ILoader<Dictionary<DateTime, DateTime>, string, DateTime> _dualOrderMonthLoader;

        public DualOrderMonthProvider(ILoader<Dictionary<DateTime, DateTime>, string, DateTime> dualOrderMonthLoader)
        {
            _dualOrderMonthLoader = dualOrderMonthLoader;
        }

        public bool IsDualOrderMonth(string countryCode, DateTime localNow)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            try
            {
                var dualOrderMonths = _dualOrderMonthLoader.Load(countryCode, localNow);
                if (null == dualOrderMonths)
                {
                    Log.LoggerHelper.Error(
                        string.Format(
                            "Failed to get Dual Month info from the Catalog Service countrycode {0} in BusinessSettingsProvider.IsDualOrderMonth",
                            countryCode));
                    return false;
                }

                if (dualOrderMonths.Count == 0)
                {
                    return false;
                }

                var monthEnd = dualOrderMonths.FirstOrDefault().Value;
                return localNow < monthEnd;
            }
            catch (Exception ex)
            {
                Log.LoggerHelper.Exception("Error", ex,
                                           "Errored out in IsDualOrderMonth" + countryCode);
                throw;
            }
        }

        public DateTime? GetDualMonthEndValue(string countryCode, DateTime localNow)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            try
            {
                var dualOrderMonths = _dualOrderMonthLoader.Load(countryCode, localNow);
                if (null == dualOrderMonths)
                {
                    Log.LoggerHelper.Error(
                        string.Format(
                            "Failed to get Dual Month info from the Catalog Service countrycode {0} in BusinessSettingsProvider.GetDualMonthEndValue",
                            countryCode));
                    return null;
                }

                if (dualOrderMonths.Count == 0)
                {
                    return null;
                }

                return dualOrderMonths.FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                Log.LoggerHelper.Exception("Error", ex,
                                           "Errored out in GetDualMonthEndValue" + countryCode);
                throw;
            }
        }

    }
}