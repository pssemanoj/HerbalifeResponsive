#region

using MyHerbalife3.Ordering.Interfaces;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class KRInvoicePriceFactory : InvoicePriceFactory
    {
        public override InvoicePriceProvider GetInvoicePriceProvider(IInvoiceShippingDetails invoiceShippingDetails)
        {
            return new KRInvoicePriceProvider(invoiceShippingDetails);
        }
    }
}