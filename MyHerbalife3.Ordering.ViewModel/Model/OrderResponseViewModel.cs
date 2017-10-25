using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderResponseViewModel
    {
        public OrderViewModel Data { get; set; }
        public List<ValidationErrorViewModel> ValidationErrors { get; set; }
    }
}