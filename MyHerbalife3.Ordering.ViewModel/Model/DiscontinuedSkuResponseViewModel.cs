using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class DiscontinuedSkuResponseViewModel : MobileResponseViewModel
    {
        public List<DiscontinuedSkuItemResponseViewModel> DiscontinuedSkus { get; set; }
        public int RecordCount { get; set; }
    }
}
