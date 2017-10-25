using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Invoices
{
    public class UKInvoiceShippingDetailsFactory : InvoiceShippingDetailsFactory
    {
        public override IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase shippingProvider)
        {
            return new UKInvoiceShippingDetails(shippingProvider);
        }
    }
}
