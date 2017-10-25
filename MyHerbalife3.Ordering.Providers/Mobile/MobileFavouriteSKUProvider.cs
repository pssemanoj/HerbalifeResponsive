#region

using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;


#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileFavouriteSKUProvider : IMobileFavouriteSKUProvider
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();
        //private readonly MobileQuoteHelper _mobileQuoteHelper;
        private FavouriteSkuLoader _favorLoader = new FavouriteSkuLoader();

        public MobileFavouriteSKUProvider()
        {
        }

        public bool SetFavouriteSKUs(SetFavouriteParam request, ref List<ValidationErrorViewModel> errors)
        {
            bool result= false;

            if (string.IsNullOrWhiteSpace(request.Locale) || 
                string.IsNullOrWhiteSpace(request.DistributorID) || 
                string.IsNullOrWhiteSpace(request.SKUList) 
                )
            {
                return result;
            }

            try
            {
                result = _favorLoader.SetFavouriteSKUList(request.DistributorID, request.Locale, request.SKUList);

                return result;
            }
            catch (Exception ex)
            {
                errors.Add(
                    new ValidationErrorViewModel
                    {
                        Code = 999998,
                        Reason = string.Format("MobileFavouriteSKUProvider update error", ex.Message)
                    });

                LoggerHelper.Error(string.Format("MobileFavouriteSKUProvider : {0}\n {1}\n", ex.Message, ex.StackTrace));
                return result;
            }


        }

        public List<FavouriteSKUItemResponseViewModel> GetFavouriteSKUs(GetFavouriteParam request)
        {
            List<FavouriteSKUItemResponseViewModel> result = null;  
            

            if (string.IsNullOrWhiteSpace(request.Locale) || string.IsNullOrWhiteSpace(request.DistributorID))
            {
                return result;
            }

            try
            {
                List<FavouriteSKU> SKUs = new List<FavouriteSKU>();
                SKUs = _favorLoader.GetDistributorFavouriteSKU(request.DistributorID, request.Locale);
                
                if (SKUs.Count > 0)
                {
                    result = ConvertToFavouriteItemViewModel(SKUs);
                }

                return result;
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileFavouriteSKUProvider : {0}\n {1}\n", ex.Message,ex.StackTrace));
                return result;
            }

           
        }

        private List<FavouriteSKUItemResponseViewModel> ConvertToFavouriteItemViewModel(List<FavouriteSKU> request)
        {
            List<FavouriteSKUItemResponseViewModel> response = null;
            if (request != null)
            {
                response = request.Select(item => new FavouriteSKUItemResponseViewModel
                {
                    FavouriteID = item.FavouriteID,
                    ProductSKU = item.ProductSKU,
                    ProductID = item.ProductID
                }).ToList();
            }
            return response;
        }

    }
}