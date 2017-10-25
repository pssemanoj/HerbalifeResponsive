#region

using MyHerbalife3.Ordering.Interfaces;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public abstract class InvoicePriceFactory
    {
        public abstract InvoicePriceProvider GetInvoicePriceProvider(IInvoiceShippingDetails invoiceShippingDetails);
    }
}