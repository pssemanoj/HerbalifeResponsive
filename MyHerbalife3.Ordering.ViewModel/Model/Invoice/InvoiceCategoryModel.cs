using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceCategoryModel
    {
        public int RootCategoryId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InvoiceLineModel> Products { get; set; }
    }
}