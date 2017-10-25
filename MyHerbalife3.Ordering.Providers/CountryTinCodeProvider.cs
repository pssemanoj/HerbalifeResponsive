using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web;
using HL.Common.Utilities;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class CountryTinCodeProvider
    {
        private const string CacheKey = "CountryTinCodes_";

        /// <summary>Retrieves a list of valid TinCodes for a country</summary>
        /// <param name="countryCode">The ISO code of the country</param>
        /// <returns>A list of codes</returns>
        public static List<string> GetValidTinCodesForCountry(string countryCode)
        {
            return GetTinCodesFromCache(countryCode);
        }

        private static List<string> GetTinCodesFromCache(string countryCode)
        {
            string cacheKey = CreateCacheKey(countryCode);
            var tinList = HttpRuntime.Cache[cacheKey] as List<string>;
            if (null == tinList)
            {
                tinList = LoadTinCodesFromService(countryCode);
                if (null != tinList)
                {
                    HttpRuntime.Cache.Insert(cacheKey, tinList);
                }
            }

            return tinList;
        }

        private static List<string> LoadTinCodesFromService(string countryCode)
        {
            List<string> tinList = null;
            if (string.IsNullOrEmpty(countryCode))
            {
                return null;
            }
            else
            {
                GetValidTinCodesListResponse_V01 response = null;
                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    try
                    {
                        var request = new GetValidTinCodesListRequest_V01();
                        request.Country = countryCode;
                        response = proxy.GetValidTinCodesList(new GetValidTinCodesListRequest1(request)).GetValidTinCodesListResult as GetValidTinCodesListResponse_V01;

                        // Check response for error.
                        if (response == null || response.Status != ServiceResponseStatusType.Success)
                        {
                            throw new ApplicationException("CountryTinCodeProvider.GetValidTinCodesList error. ");
                        }

                        tinList = response.TinCodes;
                    }
                    catch (Exception ex)
                    {
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        LoggerHelper.Error(string.Format("Order -LoadTinCodesFromService error, {0}", countryCode));
                    }
                }
            }

            return tinList;
        }

        private static string CreateCacheKey(string countryCode)
        {
            return string.Concat(CacheKey, countryCode);
        }
    }
}