using MyHerbalife3.Ordering.Interfaces;

namespace MyHerbalife3.Ordering.Invoices
{
    public class USInvoicePriceFactory : InvoicePriceFactory
    {
        public override InvoicePriceProvider GetInvoicePriceProvider(IInvoiceShippingDetails invoiceShippingDetails)
        {
            return new USInvoicePriceProvider(invoiceShippingDetails);
        }
    }
}