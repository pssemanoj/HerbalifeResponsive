#region

using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class KRInvoiceShippingDetailsFactory : InvoiceShippingDetailsFactory
    {
        public override IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase shippingProvider)
        {
            return new KRInvoiceShippingDetails(shippingProvider);
        }
    }
}