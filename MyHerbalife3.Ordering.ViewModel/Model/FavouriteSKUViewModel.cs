#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;


#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class FavouriteSKUViewModel
    {
        public List<FavouriteSKUItemViewModel> FavouriteSKUs { get; set; }
    }
}