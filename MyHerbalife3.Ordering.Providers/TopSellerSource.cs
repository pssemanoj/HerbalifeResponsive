using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using HL.Blocks.CircuitBreaker;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers
{
    public class TopSellerSource : ITopSellerSource
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();

        public List<TopSellerProductModel> GetTopSellerProducts(string id, string countryCode, string locale)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            if (string.IsNullOrEmpty(locale))
            {
                throw new ArgumentException("Locale is blank", "locale");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }

            var cacheKey = string.Format("MYHL3_TS_{0}_{1}", id, locale);
            var result = _cache.Retrieve(_ => GetTopSellerProductsFromSource(id, countryCode, locale), cacheKey, TimeSpan.FromMinutes(15));
            return result;
        }



        public List<TopSellerProductModel> GetTopSellerProductsFromSource(string id, string countryCode, string locale)
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetTopSellersResponse_V01>();
                var response =
                    circuitBreaker.Execute(() => proxy.GetTopSellers(new GetTopSellersRequest1(new GetTopSellersRequest_V01
                    {
                        CountryCode = countryCode,
                        Locale = locale,
                        DistributorId = id
                    })).GetTopSellersResult as GetTopSellersResponse_V01);

                if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success)
                {
                    return GetTopSellerProductModel(response.Skus, id, locale);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex,
                                       "Errored out in TopSellerSource" + id + countryCode);
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

            return null;
        }

        private static List<TopSellerProductModel> GetTopSellerProductModel(IEnumerable<TopSellerSku_V01> skus, string id, string locale)
        {
            var totals = new OrderTotals_V01();
            var ShoppingCartItem = new List<ShoppingCartItem_V01>();
            ShoppingCartItem.AddRange(from i in skus
                                      select
                                      new ShoppingCartItem_V01(0, i.Sku, 1, DateTime.Now));
            var existingCart = ShoppingCartProvider.GetShoppingCart(id, locale);
            totals = existingCart != null ? existingCart.Calculate(ShoppingCartItem.ToList()) as OrderTotals_V01 : null;

            var topSellerProductModelList = skus.Select(topsellerSku => new TopSellerProductModel
            {
                ImageUrl = topsellerSku.ImageUrl,
                Quantity = 1, // default quantity "1"
                Sku = topsellerSku.Sku,
                Name = topsellerSku.Name,
                Price = GetTopSellerSkuPrice(skus, topsellerSku, locale, totals),
                CurrencySymbol = topsellerSku.CurrencySymbol,
                DisplayPrice = String.Format("{0}{1}",HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                                                  GetTopSellerSkuPrice(skus, topsellerSku, locale, totals))
            }).ToList();
            return topSellerProductModelList;
        }
        private static decimal GetTopSellerSkuPrice(IEnumerable<TopSellerSku_V01> skus, TopSellerSku_V01 topsellerSku, string locale, OrderTotals_V01 totals)
        {
            switch (locale)
            {
                case "ko-KR":
                    return totals != null && totals.ItemTotalsList.Count > 0 ? OrderProvider.getKRDistributorPrice(totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == topsellerSku.Sku) as ItemTotal_V01, topsellerSku.Sku) : topsellerSku.Price;
                default:
                    return topsellerSku.Price;
            }
        }
    }
}