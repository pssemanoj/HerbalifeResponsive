using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileGetDisconinuedSkuProvider:IMobileDiscontinueSkuProvider
    {
        private readonly DiscontinuedSkuLoader _discontinuedSkuLoader  = new DiscontinuedSkuLoader();
        public List<DiscontinuedSkuItemResponseViewModel> GetDiscontinuedSkuRequest(GetDiscontinuedSkuParam request)
        {
            List<DiscontinuedSkuItemResponseViewModel> result = null;


            if (string.IsNullOrWhiteSpace(request.Locale) || string.IsNullOrWhiteSpace(request.DistributorId))
            {
                return null;
            }

            try
            {
                var list = _discontinuedSkuLoader.GetDiscontinuedSkuList(request.DistributorId,request.Locale,request.ShoppingCartItemToCheck);

                if (list.Count > 0)
                {
                    result = ConvertToFavouriteItemViewModel(list);
                }

                return result;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileDiscontinuedSkuProvider : {0}\n {1}\n", ex.Message, ex.StackTrace));
                return result;
            }
        }
        private List<DiscontinuedSkuItemResponseViewModel> ConvertToFavouriteItemViewModel(CatalogItemList request)
        {
            List<DiscontinuedSkuItemResponseViewModel> response = new List<DiscontinuedSkuItemResponseViewModel>();
            if (request != null)
            {
                response.AddRange(request.Select(item => new DiscontinuedSkuItemResponseViewModel
                {
                    ProductName = item.Value.Description, Sku = item.Value.SKU
                }));
            }
            return response;
        }
    }
}
