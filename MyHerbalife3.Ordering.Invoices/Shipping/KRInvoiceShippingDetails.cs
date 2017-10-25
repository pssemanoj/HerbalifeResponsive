#region

using System.Collections.Generic;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class KRInvoiceShippingDetails : IInvoiceShippingDetails
    {
        private readonly ShippingProviderBase _shippingProvider;

        public KRInvoiceShippingDetails(ShippingProviderBase shippingProviderBase)
        {
            _shippingProvider = shippingProviderBase;
        }

        public string GetWarehouseCode(Address address, string locale)
        {
            return "81";
        }

        public string GetShippingMethodId(Address address, string locale)
        {
            return "KSF";
        }

        public IEnumerable<KeyValuePair<string, string>> GetStates(string locale)
        {
            return null;
        }

        public bool ValidateAddress(Address_V01 address, out Address_V01 avsOutputAddress)
        {
            avsOutputAddress = address;
            return null != address && !string.IsNullOrEmpty(address.City) && !string.IsNullOrEmpty(address.PostalCode) &&
                   !string.IsNullOrEmpty(address.Line1);
        }
    }
}