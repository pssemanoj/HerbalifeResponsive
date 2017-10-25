using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Invoices
{
    public abstract class InvoiceShippingDetailsFactory
    {
        public abstract IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase shippingProvider);
    }
}