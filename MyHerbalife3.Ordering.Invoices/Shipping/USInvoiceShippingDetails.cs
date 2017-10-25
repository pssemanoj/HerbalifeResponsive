#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class USInvoiceShippingDetails : IInvoiceShippingDetails
    {
        private readonly ShippingProviderBase _shippingProvider;

        public USInvoiceShippingDetails(ShippingProviderBase shippingProviderBase)
        {
            _shippingProvider = shippingProviderBase;
        }

        public string GetWarehouseCode(Address address, string locale)
        {
            var deliveryOptions = _shippingProvider.GetDeliveryOptions(locale);
            if (null == deliveryOptions || deliveryOptions.Count <= 0) return string.Empty;
            var state = _shippingProvider.GetStateNameFromStateCode(address.StateProvinceTerritory);
            var options =
                deliveryOptions.Where(
                    d => d.Option == DeliveryOptionType.Shipping && !string.IsNullOrEmpty(d.State) &&
                         d.State.Trim() == state);
            return options.Any() ? options.First().WarehouseCode : string.Empty;
        }

        public string GetShippingMethodId(Address address, string locale)
        {
            return "FED";
        }

        public IEnumerable<KeyValuePair<string, string>> GetStates(string locale)
        {

            var providerUS = new ShippingProvider_US();
            var lookupResults = providerUS.GetStatesForCountryToDisplay(locale.Substring(3,2));
            if (null != lookupResults && lookupResults.Count > 0)
            {
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
                foreach (var lookupResult in lookupResults)
                {
                    result.Add(new KeyValuePair<string, string>(lookupResult.Substring(0, 2), lookupResult.Replace(lookupResult.Substring(0, 4),"").Trim()));
                }
                return result;
            }
            return GlobalResourceHelper.GetGlobalEnumeratorElements("UsaStates", new CultureInfo(locale));
        }

        public bool ValidateAddress(Address_V01 address, out Address_V01 avsOutputAddress)
        {
            ServiceProvider.AddressValidationSvc.Address avsAddress = null;
            var errorCode = string.Empty;
            var isValid =
                new ShippingProvider_US().ValidateAddress(new ShippingAddress_V02(0, string.Empty, string.Empty,
                    string.Empty, string.Empty, address, string.Empty, string.Empty, true, string.Empty, DateTime.Now),
                    out errorCode, out avsAddress);
            if (isValid && null != avsAddress && !CheckAvsAddressForNull(avsAddress))
            {
                avsOutputAddress = new Address_V01
                {
                    City = avsAddress.City,
                    Country = avsAddress.CountryCode,
                    CountyDistrict = avsAddress.CountyDistrict,
                    Line1 = avsAddress.Line1,
                    Line2 = avsAddress.Line2,
                    Line3 = avsAddress.Line3,
                    Line4 = avsAddress.Line4,
                    PostalCode = avsAddress.PostalCode,
                    StateProvinceTerritory = avsAddress.StateProvinceTerritory
                };
                return true;
            }
            avsOutputAddress = null;
            return false;
        }

        private static bool CheckAvsAddressForNull(ServiceProvider.AddressValidationSvc.Address avsAddress)
        {
            return string.IsNullOrEmpty(avsAddress.City) || string.IsNullOrEmpty(avsAddress.PostalCode) ||
                   string.IsNullOrEmpty(avsAddress.StateProvinceTerritory);
        }
    }
}