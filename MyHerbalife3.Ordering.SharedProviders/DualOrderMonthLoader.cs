using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.CircuitBreaker;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Shared.Interfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public class DualOrderMonthLoader : ILoader<Dictionary<DateTime, DateTime>, string, DateTime>
    {
        #region ILoader<Dictionary<DateTime, DateTime>,string, DateTime> Members

        public Dictionary<DateTime, DateTime> Load(string countryCode, DateTime localNow)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("CountryCode is blank", "countryCode");
            }
            var fromDate = DateTimeUtils.GetFirstDayOfMonth(localNow);
            var toDate = DateTimeUtils.GetLastDayOfMonth(localNow);

            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetDualMonthDatesResponse_V01>();

                var response =
                    circuitBreaker.Execute(() => proxy.GetDualMonthDates(new GetDualMonthDatesRequest1(new GetDualMonthDatesRequest_V01
                    {
                        CountryCode = countryCode,
                        FromDate = fromDate,
                        ToDate = toDate
                    })).GetDualMonthDatesResult as GetDualMonthDatesResponse_V01);
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    return GetDualMonthDictionary(response.DualMonth, localNow);
                }
            }
            catch
            {
                LoggerHelper.Error("Errored out in GetDualOrderMonth" + countryCode);
                if (null != proxy)
                {
                    proxy.Close();
                }
                throw;
            }
            finally
            {
                if (null != proxy)
                {
                    proxy.Close();
                }
            }
            LoggerHelper.Error("Errored out in GetDualOrderMonth Catalog service" + countryCode);
            return null;
        }

        private static Dictionary<DateTime, DateTime> GetDualMonthDictionary(IEnumerable<DualMonthPair> dualMonthPairs,
                                                                             DateTime localNow)
        {
            if (null == dualMonthPairs)
                return null;
            var dualMonthDictionary =
                dualMonthPairs.ToDictionary(
                    dualMonthPair => localNow < dualMonthPair.MonthEndDate ?
                    new DateTime(Convert.ToInt32(dualMonthPair.OrderMonth.Substring(0, 4)),
                                 Convert.ToInt32(dualMonthPair.OrderMonth.Substring(4, 2)), localNow.Day, 23, 59, 59) : localNow,
                    dualMonthPair => dualMonthPair.MonthEndDate);
            return dualMonthDictionary;
        }

        #endregion
    }
}