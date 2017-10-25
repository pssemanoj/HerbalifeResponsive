using System;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class HandlingInfoViewModel
    {
        public Guid Id { get; set; }
        public string InvoiceHandlingType { get; set; }
        public string ShippingInstructions { get; set; }
    }
}