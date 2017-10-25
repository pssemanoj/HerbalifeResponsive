using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class FavouriteSetSKUResponseViewModel : MobileResponseViewModel
    {
        public List<FavouriteSetSKUResponseViewModelItem> Favourites { get; set; }
    }

    public class FavouriteSetSKUResponseViewModelItem
    {
        public string productSKU { get; set; }

        public bool Updated { get; set; }

        public string reason { get; set; }
    }
}