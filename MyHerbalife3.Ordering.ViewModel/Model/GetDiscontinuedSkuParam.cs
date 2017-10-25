using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;


namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class GetDiscontinuedSkuParam
    {
        public List<ShoppingCartItem_V01>  ShoppingCartItemToCheck { get; set; }
        public string DistributorId { get; set; }
        public string Locale { get; set; }
    }
}
