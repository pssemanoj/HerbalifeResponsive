using MyHerbalife3.Ordering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Invoices.Shipping
{
    class JMInvoiceShippingDetailsFactory: InvoiceShippingDetailsFactory
    {
        public override IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase _shippingProvider)
        {
            return new JMInvoiceShippingDetails(_shippingProvider);
        }
    }
}
