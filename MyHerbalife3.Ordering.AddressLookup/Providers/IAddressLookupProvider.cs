using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public interface IAddressLookupProvider
    {
        List<string> GetStatesForCountry(string country);
        List<string> GetCitiesForState(string country, string state);
        List<string> GetZipsForCity(string country, string state, string city);
        List<string> GetZipsForStreet(string country, string state, string city, string street);
        List<StateCityLookup_V01> LookupCitiesByZip(string country, string zipcode);
        List<string> GetStreetsForCity(string country, string state, string city);
        bool ValidatePostalCode(string country, string state, string city, string postalCode);
        string LookupZipCode(string state, string municipality, string colony);
        string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName);
        List<AddressData_V02> GetAddressData(string searchTerm);
        List<AddressFieldInfo> GetAddressFieldsForCountry(AddressFieldForCountryRequest_V01 request);
    }
}
