using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Invoices.Shipping
{
    class TTInvoiceShippingDetailsFactory : InvoiceShippingDetailsFactory
    {
        public override IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase _shippingProvider)
        {
            return new TTInvoiceShippingDetails(_shippingProvider);
        }
    }
}
