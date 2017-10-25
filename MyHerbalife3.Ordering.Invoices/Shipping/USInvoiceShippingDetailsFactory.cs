#region

using System;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class USInvoiceShippingDetailsFactory : InvoiceShippingDetailsFactory
    {
        public override IInvoiceShippingDetails GetInvoiceShippingDetails(ShippingProviderBase shippingProvider)
        {
            return new USInvoiceShippingDetails(shippingProvider);
        }
    }
}