using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    public class FavouriteSKUBLTest
    {
        bool _isFavouriteSKU;
        public bool IsFavouriteSKU
        {
            get {return  _isFavouriteSKU; }
        }

        IFavouriteSkuLoader _DSProfile;
        public FavouriteSKUBLTest(IFavouriteSkuLoader DSProfile)
        {
            _DSProfile = DSProfile;
        }

        public bool GetFavouriteSKUs(string distributorID, string locale)
        {
            try
            {
                _DSProfile.GetDistributorFavouriteSKU(distributorID, locale);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CheckIfFavouriteSKU(string productSKU, string distributorID, string locale )
        {
            bool bolResult = false;

            try
            {
                var myFavouriteList = _DSProfile.GetDistributorFavouriteSKU(distributorID, locale);
                return myFavouriteList.Any(x => x.ProductSKU == productSKU );

            }
            catch
            {
                return bolResult;
            }

        }

        public bool AddFavouriteSKUtoList(string distributorID, int productID, string productSKU, string locale, int DEL=0)
        {
            try
            {
                return _DSProfile.SetFavouriteSKU(distributorID, productID, productSKU, locale, DEL);
            }
            catch
            {
                return false;
            }

        }


    }
}
