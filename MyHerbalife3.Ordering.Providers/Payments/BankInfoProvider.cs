using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;


namespace MyHerbalife3.Ordering.Providers.Payments
{
    public static class BankInfoProvider
    {
        #region Constants
        public const string BANKINFO_CACHE_PREFIX = "BankInfo_China";
        public const int BankInfoCacheMinutes = 1440;
        #endregion Constants

        public static List<BankInformation> GetAvailableBanks(BankUsage usage = BankUsage.UsedByGateway)
        {
            string cacheKey = BANKINFO_CACHE_PREFIX + usage.ToString();
            var bankList = HttpRuntime.Cache[cacheKey] as List<BankInformation>;
            if (null == bankList)
            {
                using (var orderProxy = ServiceClientProvider.GetChinaOrderServiceProxy())
                {
                    try
                    {
                        var response = orderProxy.GetBankInfo(new GetBankInfoRequest1(new GetBankInfoRequest_V01() { Usage = usage })).GetBankInfoResult as GetBankInfoResponse_V01;
                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            bankList = response.Banks.ToList();
                            HttpRuntime.Cache.Insert(cacheKey, bankList, null, DateTime.Now.AddMinutes(BankInfoCacheMinutes), Cache.NoSlidingExpiration);
                        }
                    }
                    catch (Exception ex)
                    {
                        WebUtilities.LogServiceExceptionWithContext<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.IChinaInterface>(ex, orderProxy);
                    }
                }
            }

            return bankList;
        }
    }
}
