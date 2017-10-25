using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Distributor.ValueObjects;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IFavouriteSKU
    {

        bool SetFavouriteSKU(string distributorID, int productID, string productSKU, string locale, int DEL = 0);

        List<FavouriteSKU> GetDistributorFavouriteSKU(string distributorID, string locale);
    }
}
