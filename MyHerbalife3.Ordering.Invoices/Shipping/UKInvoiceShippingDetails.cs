using System.Collections.Generic;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Invoices
{
    public class UKInvoiceShippingDetails : IInvoiceShippingDetails
    {
        private readonly ShippingProviderBase _shippingProvider;

        public UKInvoiceShippingDetails(ShippingProviderBase shippingProviderBase)
        {
            _shippingProvider = shippingProviderBase;
        }

        public string GetWarehouseCode(Address address, string locale)
        {
            return "U3";
        }

        public string GetShippingMethodId(Address address, string locale)
        {
            return "USI";
        }

        public IEnumerable<KeyValuePair<string, string>> GetStates(string locale)
        {
            return null;
        }

        public bool ValidateAddress(Address_V01 address, out Address_V01 avsOutputAddress)
        {
            bool isValid = _shippingProvider.ValidatePostalCode(address.Country, address.StateProvinceTerritory,
                address.City, address.PostalCode);
            avsOutputAddress = address;
            return isValid;
        }
    }
}