using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Shared.Interfaces;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class FavouriteSkuLoader : IFavouriteSkuLoader
    {
        public List<FavouriteSKU> GetDistributorFavouriteSKU(string distributorID, string locale)
        {
            List<FavouriteSKU> SKUs = new List<FavouriteSKU>();
            try
            { 

                SKUs = DistributorOrderingProfileProvider.GetDistributorFavouriteSKU(distributorID, locale);
                return SKUs;
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(
                           string.Format("DistributorOrderingProfileProviderLoader.GetDistributorFavouriteSKU error calling service \n{0} \n ERR:{1}",
                                         ex.Message, ex.StackTrace));

                return SKUs;
            }

        }

        public bool SetFavouriteSKU(string distributorID, int productID, string productSKU, string locale, int DEL = 0)
        {
            bool result = false;
            try
            {
                return DistributorOrderingProfileProvider.SetFavouriteSKU(distributorID, productID, productSKU, locale, DEL);
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(
                           string.Format("DistributorOrderingProfileProviderLoader.SetFavouriteSKU error calling service \n{0} \n ERR:{1}",
                                         ex.Message, ex.StackTrace));
                return result;
            }
        }

        public bool SetFavouriteSKUList(string distributorID, string locale, string SKUList)
        {
            bool result = false;
            try
            {
                return DistributorOrderingProfileProvider.SetFavouriteSKUList(distributorID, locale, SKUList);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                           string.Format("DistributorOrderingProfileProviderLoader.SetFavouriteSKUList error calling service \n{0} \n ERR:{1}",
                                         ex.Message, ex.StackTrace));
                return result;
            }
        }

    }
}
