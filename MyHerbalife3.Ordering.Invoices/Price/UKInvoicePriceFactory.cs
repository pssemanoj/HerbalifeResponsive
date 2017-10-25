﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Interfaces;

namespace MyHerbalife3.Ordering.Invoices
{
    public class UKInvoicePriceFactory : InvoicePriceFactory
    {
        public override InvoicePriceProvider GetInvoicePriceProvider(IInvoiceShippingDetails invoiceShippingDetails)
        {
            return new UKInvoicePriceProvider(invoiceShippingDetails);
        }
    }
}
