using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets
{
    public class TopSellerSourceStandIn : ITopSellerSource
    {
        public List<TopSellerProductModel> GetTopSellerProducts(string id, string countryCode, string locale)
        {
            return new List<TopSellerProductModel>
                {
                    new TopSellerProductModel {Name = "<p>Formula 1 Nutritional Shake Mix</p>", CurrencySymbol = "USD", Sku = "3106", DisplayPrice = "$37.95",
                        ImageUrl = "/Content/en-US/img/Catalog/Products/101213_SKU3106_400X400_us.jpg", Price = 37.95m, Quantity = 0},
                    
                        new TopSellerProductModel {Name = "Formula 2 Multivitamin Complex", CurrencySymbol = "USD", Sku = "3115", DisplayPrice = "$23.15",
                        ImageUrl = "/Content/en-US/img/Catalog/Products/2010_f2_multivitamin_complex_400.jpg", Price = 23.15m, Quantity = 0},

                };
        }
    }
}