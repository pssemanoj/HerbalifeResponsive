using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IDiscontinuedSkuLoader
    {
        CatalogItemList GetDiscontinuedSkuList(string distributorId, string locale, List<ShoppingCartItem_V01> shoppingCartItems );
        List<DiscontinuedSkuItemResponseViewModel> CheckDiscontinuedPromo(List<ShoppingCartItem_V01> shoppingcartItems);
    }
}
