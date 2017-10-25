using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using HL.Catalog.ValueObjects;
using HL.Common.Configuration;
using HL.Common.DataContract.Interfaces;
using HL.Common.Utilities;
using HL.Common.ValueObjects;
using HL.Blocks.CircuitBreaker;
using HL.Blocks.Caching.SimpleCache;
using HL.Distributor.ValueObjects;
using MyHerbalife3.Ordering.Providers.CatalogSVC;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Distributor;
using MyHerbalife3.Shared.Providers;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.Infrastructure.UI;
using MyHerbalife3.Ordering.Providers.Interface;
using ShoppingCartItemList = HL.Catalog.ValueObjects.ShoppingCartItemList;

namespace MyHerbalife3.Ordering.Providers
{
    public class FavouriteSKUProvider 
    {
        public const int FavouriteSKU_Cache_Minutes = 15;
        private readonly ISimpleCache _cache = CacheFactory.Create();

        public bool SetFavouriteSKU(string distributorID, int productID, string productSKU, string locale, int DEL = 0  )
        {
            SetDistributorFavouriteRequest request = new SetDistributorFavouriteRequestV01
            {
                DistributorId = distributorID,
                ProductID = productID,
                ProductSKU = productSKU,
                Locale = locale,
                Delete = (DEL > 0 ? true : false)
            };

            if (string.IsNullOrEmpty(request.DistributorId) || string.IsNullOrEmpty(request.ProductSKU) || string.IsNullOrEmpty(request.Locale))
            {
                return false;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetDistributorServiceProxyChina();
                    var response = proxy.SetDistributorFavouriteSKU(request) as SetDistributorFavouriteResponse;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        clearFavouriteSKUFromCache(distributorID);
                        return true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }
            return false;
        }
        
        private void clearFavouriteSKUFromCache(string distributorID)
        {
            var cacheKey = string.Format("Favour_{0}_{1}", distributorID, Thread.CurrentThread.CurrentUICulture.Name);
            _cache.Expire(typeof(List<FavouriteSKU>), cacheKey);

        }

        public List<FavouriteSKU> GetDistributorFavouriteSKU(string distributorID, string locale)
        {
            var cacheKey = string.Format("Favour_{0}_{1}", distributorID, Thread.CurrentThread.CurrentUICulture.Name);
            var result = _cache.Retrieve(_ => LoadFromService(distributorID, locale), cacheKey, TimeSpan.FromMinutes(FavouriteSKU_Cache_Minutes));

            return result ?? new List<FavouriteSKU>();
        }

        public List<FavouriteSKU> LoadFromService(string distributorID, string locale)
        {
            List<FavouriteSKU> SKUs = new List<FavouriteSKU>();
            GetDistributorFavouriteSKURequest request = new GetDistributorFavouriteSKURequest("01")
            {
                DistributorID = distributorID,
                Locale = locale  
            };

            if (string.IsNullOrEmpty(request.DistributorID))
            {
                return SKUs;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetDistributorServiceProxyChina();
                    var response = proxy.GetDistributorFavouriteSKUs(request) as GetDistributorFavouriteSKUResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        SKUs = response.FavouriteSKUs;
                    }

                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return SKUs;
        }

    }
}