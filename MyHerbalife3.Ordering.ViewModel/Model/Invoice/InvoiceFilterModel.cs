using System;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceFilterModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int SelectedFilterId { get; set; }
        public string SelectedFilterValue { get; set; }
    }
}