#region

using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ValidationErrorViewModel
    {
        public int Code { get; set; }
        public string Reason { get; set; }
        public List<SkuErrorModel> Skus { get; set; }
        public string Message { get; set; }
    }

    public class SkuErrorModel
    {
        public int MaxQuantity { get; set; }
        public string Sku { get; set; }
    }
}