using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class SetFavouriteParam
    {

        public string Locale { get; set; }
        public string DistributorID { get; set; }
        public string SKUList { get; set; }

    }

    public class SetFavouriteUpdateParam
    {
        public List<FavouriteSKUUpdateItemViewModel> Favourites { get; set; }

    }
}