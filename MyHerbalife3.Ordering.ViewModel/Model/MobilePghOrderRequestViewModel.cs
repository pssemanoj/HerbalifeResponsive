#region

#region

using HL.Mobile.ValueObjects;

#endregion

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class MobilePghOrderRequestViewModel
    {
        public MobilePghRequestViewModel Data { get; set; }
        public OrderInvoice InvoiceData { get; set; }

        public Client Client { get; set; }
    }
}