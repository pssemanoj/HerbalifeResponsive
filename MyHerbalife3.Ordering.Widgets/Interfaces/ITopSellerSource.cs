using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface ITopSellerSource
    {
        List<TopSellerProductModel> GetTopSellerProducts(string id, string countryCode, string locale);
    }
}