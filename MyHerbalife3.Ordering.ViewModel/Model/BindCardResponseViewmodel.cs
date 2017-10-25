using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class BindCardResponseViewmodel : MobileResponseViewModel
    {
        public CardViewModel CardModel { get; set; }
    }

    public class BindCardsResponseViewmodel : MobileResponseViewModel
    {
        public List<BindedCardViewModel> CardModel { get; set; }
    }
}
