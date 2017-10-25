using System;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IExpireCatalogCacheProvider
    {
        ExpireCatalogCacheResponseViewModel ExpireCatalogCache(string memberId, string inputCacheKey, string countryCode);
    }
}
