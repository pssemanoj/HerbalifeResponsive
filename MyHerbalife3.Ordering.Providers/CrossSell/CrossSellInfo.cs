using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;

namespace MyHerbalife3.Ordering.Providers.CrossSell
{
    [Serializable]
    public class CrossSellInfo
    {
        public CrossSellInfo(int categoryID, ProductInfo_V02 product)
        {
            CategoryID = categoryID;
            Product = product;
        }

        public int CategoryID { get; set; }

        public ProductInfo_V02 Product { get; set; }
    }
}