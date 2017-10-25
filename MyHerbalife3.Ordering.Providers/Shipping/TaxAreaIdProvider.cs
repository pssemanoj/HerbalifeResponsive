#region

using System;
using System.Collections.Generic;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class TaxAreaIdProvider
    {
        private const string TaxAreaIdCacheKeyPrefix = "_TaxAreaId";
        private const int DefaultTaxAreaCacheMins = 60;
        private readonly ISimpleCache _cache = CacheFactory.Create();
        
        public Dictionary<int, string> GetTaxAreaIds(string memberId, DateTime current, int daysToPurge, string locale)
        {
            var cacheKey = string.Format("{0}_{1}_{2}", TaxAreaIdCacheKeyPrefix, memberId, locale);
            return _cache.Retrieve(_ => GetTaxAreaIdFromService(memberId, current, daysToPurge), cacheKey,
                TimeSpan.FromMinutes(Settings.GetRequiredAppSetting("TaxAreaCacheMins", DefaultTaxAreaCacheMins)));
        }

        private Dictionary<int, string> GetTaxAreaIdFromService(string memberId, DateTime current, int daysToPurge)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();

            var request = new GetTaxAreaIdListRequest_V01
            {
                Current = current,
                DaysToPurge = daysToPurge,
                MemberId = memberId
            };


            try
            {
                var response = proxy.GetTaxAreaIdList(new GetTaxAreaIdListRequest1(request)).GetTaxAreaIdListResult;
                if (null != response && response.Status == ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as GetTaxAreaIdListResponse_V01;
                    if (null != responseV01)
                    {
                        return responseV01.TaxAreaIds;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error occured while getting TaxAreaId for " + memberId);
            }
            finally
            {
                proxy.Close();
            }
            return null;
        }

        public Dictionary<int, string> SaveTaxAreaId(string memberId, DateTime current, int shippingAddressId,
            string taxAreaId, string locale)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();

            var request = new SaveTaxAreaIdRequest_V01
            {
                Current = current,
                MemberId = memberId,
                ShippingAddressId = shippingAddressId,
                TaxAreaId = taxAreaId
            };

            try
            {
                var response = proxy.SaveTaxAreaId(new SaveTaxAreaIdRequest1(request)).SaveTaxAreaIdResult;
                if (null != response && response.Status == ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as SaveTaxAreaIdResponse_V01;
                    if (null != responseV01 && responseV01.Success)
                    {
                        var cacheKey = string.Format("{0}_{1}_{2}", TaxAreaIdCacheKeyPrefix, memberId, locale);
                        _cache.Expire(typeof (Dictionary<int, string>), cacheKey);
                        return responseV01.TaxAreaIds;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error occured while SaveTaxAreaId for " + memberId);
            }
            finally
            {
                proxy.Close();
            }
            return null;
        }

        public string GetShipToTaxAreaId(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address address)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var response = proxy.GetTaxAreaId(new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.GetTaxAreaIdRequest1(new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.GetTaxAreaIdRequest_V01 { Address = address })).GetTaxAreaIdResult;
                if (null != response && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.GetTaxAreaIdResponse_V01;
                    if (null != responseV01)
                    {
                        return responseV01.TaxAreaId;
                    }
                }
                LoggerHelper.Error("TaxAreaIdProvider: Error GetShipToTaxAreaId null");
                return string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("TaxAreaIdProvider: Error GetShipToTaxAreaId, error message:{0}",
                    ex.Message));
                return string.Empty;
            }
            finally
            {
                proxy.Close();
            }
        }
    }
}