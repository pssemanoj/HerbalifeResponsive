using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class CustomShippingMethodViewModel
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string OrderTypes { get; set; }
        public List<VisibilityRuleViewModel> VisibilityRules { get; set; }
    }

    public class VisibilityRuleViewModel
    {
        
    }
}