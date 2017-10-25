using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IFavouriteSkuLoader
    {

        bool SetFavouriteSKU(string distributorID, int productID, string productSKU, string locale, int DEL = 0);

        List<FavouriteSKU> GetDistributorFavouriteSKU(string distributorID, string locale);
    }
}
