using MyHerbalife3.Ordering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Invoices.Shipping
{
    class CAInvoiceShippingDetails : IInvoiceShippingDetails
    {
        private readonly ShippingProviderBase shippingProvider;

        public CAInvoiceShippingDetails(ShippingProviderBase shippingProvider)
        {
            this.shippingProvider = shippingProvider;
        }

        public string GetShippingMethodId(Address address, string locale)
        {
            return "FXO";
        }

        public IEnumerable<KeyValuePair<string, string>> GetStates(string locale)
        {
            return null;
        }

        public string GetWarehouseCode(Address address, string locale)
        {
            return "TO";
        }

        public bool ValidateAddress(Address_V01 address, out Address_V01 avsOutputAddress)
        {
            avsOutputAddress = address;
            return null != address && !string.IsNullOrEmpty(address.City) && !string.IsNullOrEmpty(address.PostalCode) &&
                   !string.IsNullOrEmpty(address.Line1);
        }
    }
}
