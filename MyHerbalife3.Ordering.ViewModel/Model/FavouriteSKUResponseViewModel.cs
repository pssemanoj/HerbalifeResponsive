using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class FavouriteSKUResponseViewModel: MobileResponseViewModel
    {
        public List<FavouriteSKUItemResponseViewModel> FavouriteSKUs { get; set; }
        public int RecordCount { get; set; }
        
    }
}