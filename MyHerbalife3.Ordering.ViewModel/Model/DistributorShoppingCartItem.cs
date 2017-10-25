using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class DistributorShoppingCartItem : ShoppingCartItem_V01
    {
        public DistributorShoppingCartItem()
        {

        }

        public CatalogItem_V01 CatalogItem { get; set; }
        public string Description { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal EarnBase { get; set; }
        public string ErrorMessage { get; set; }
        public string Flavor { get; set; }
        public Category_V02 ParentCat { get; set; }
        //public bool PartialBackordered { get; set; }
        public ProductInfo_V02 ProdInfo { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal VolumePoints { get; set; }
    }
}
