using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class DiscontinuedSkuLoader:IDiscontinuedSkuLoader
    {
        public CatalogItemList GetDiscontinuedSkuList(string distributorId, string locale, List<ShoppingCartItem_V01> shoppingCartItems )
        {
            return CatalogProvider.GetMobileDiscontinuedSku(distributorId,locale,shoppingCartItems);
        }

        public List<DiscontinuedSkuItemResponseViewModel> CheckDiscontinuedPromo(List<ShoppingCartItem_V01> shoppingcartItems)
        {
            return CatalogProvider.GetMobilePromoDiscotinued(shoppingcartItems);
        }
    }
}
