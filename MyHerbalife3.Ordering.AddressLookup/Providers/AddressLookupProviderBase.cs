using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProviderBase : IAddressLookupProvider
    {
        public virtual List<string> GetStatesForCountry(string country)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new StatesForCountryRequest_V01();
            request.Country = country;
            var response = proxy.GetStatesForCountry(new GetStatesForCountryRequest(request));
            var result = response.GetStatesForCountryResult as StatesForCountryResponse_V01;
            return result.States;
        }

        public virtual List<string> GetCitiesForState(string country, string state)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new CitiesForStateRequest_V01();
                request.Country = country;
                request.State = state;
                var response = proxy.GetCitiesForState(new GetCitiesForStateRequest(request));
                var result = response.GetCitiesForStateResult as CitiesForStateResponse_V01;
                return result.Cities;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetCitiesForState error: Country {0}, error: {1}", country,
                                                 ex.ToString()));
            }
            return null;
        }

        public virtual List<string> GetZipsForCity(string country, string state, string city)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new ZipsForCityRequest_V01();
                request.Country = country;
                request.State = state;
                request.City = city;
                var response = proxy.GetZipsForCity(new GetZipsForCityRequest(request));
                var result = response.GetZipsForCityResult as ZipsForCityResponse_V01;
                return result.Zips;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetZipsForCity error: Country {0}, error: {1}", country, ex.ToString()));
            }
            return null;
        }

        public virtual List<string> GetZipsForStreet(string country, string state, string city, string street)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new ZipsForStreetRequest_V01();
                request.Country = country;
                request.State = state;
                request.City = city;
                request.Street = street;
                var response = proxy.GetZipsForStreet(new GetZipsForStreetRequest(request));
                var result = response.GetZipsForStreetResult as ZipsForStreetResponse_V01;
                return result.Zips;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetZipsForStreet error: Country {0}, error: {1}", country,
                                                 ex.ToString()));
            }
            return null;
        }

        public virtual List<StateCityLookup_V01> LookupCitiesByZip(string country, string zipcode)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new StateAndCityByZipLookupRequest_V01();
                request.Country = country;
                request.ZipCode = zipcode;
                var response = proxy.GetStateAndCityForZipCode(new GetStateAndCityForZipCodeRequest(request));
                var result = response.GetStateAndCityForZipCodeResult as StateAndCityByZipLookupResponse_V01;
                return result.StateCities;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("LookupCitiesByZip error: Country {0}, error: {1}", country,
                                                 ex.ToString()));
            }
            return null;
        }

        public virtual List<string> GetStreetsForCity(string country, string state, string city)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new StreetsForCityRequest_V01();
                request.Country = country;
                request.State = state;
                request.City = city;
                var response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request));
                var result = response.GetStreetsForCityResult as StreetsForCityResponse_V01;
                return result.Streets;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreetsForCity error: Country {0}, error: {1}", country,
                                                 ex.ToString()));
            }
            return null;
        }

        public virtual bool ValidatePostalCode(string country, string state, string city, string postalCode)
        {
            return true;
        }

        public virtual string LookupZipCode(string state, string municipality, string colony)
        {
            return string.Empty;
        }

        public virtual string formatPhone(string phone)
        {
            if (phone != null)
            {
                bool start = phone.StartsWith("-");
                if (start) phone = phone.Remove(0, 1);

                bool end = phone.EndsWith("-");
                if (end) phone = phone.Remove(phone.Length - 1, 1);
            }

            return phone;
        }

        public virtual string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type,
                                                    string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode);
            }
            if (formattedAddress.IndexOf(",,") > -1 || formattedAddress.IndexOf(", ,") > -1)
            {
                return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
            }
            else
            {
                return formattedAddress;
            }
        }


        public List<AddressData_V02> GetAddressData(string searchTerm)
        {
            var addressesFound = new List<AddressData_V02>();
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new SearchAddressDataRequest_V01
                {
                    SearchText = searchTerm
                };
                var response = proxy.GetAddressData(new GetAddressDataRequest(request));
                if (response != null)
                {
                    var result = response.GetAddressDataResult as SearchAddressDataResponse_V01;
                    if (result.Status == ServiceResponseStatusType.Success)
                    {
                        if (result.AddressData != null && result.AddressData.Count > 0)
                        {
                            addressesFound.AddRange(result.AddressData);
                        }
                    }
                    return addressesFound;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("AddressSearch error: Country {0}, error: {1}", "BR", ex));
            }
            return addressesFound;
        }

        public virtual List<AddressFieldInfo> GetAddressFieldsForCountry(AddressFieldForCountryRequest_V01 request)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var response = proxy.GetAddressFieldForCounty(new GetAddressFieldForCountyRequest(request));
            var result = response.GetAddressFieldForCountyResult as AddressFieldForCountryResponse_V01;
            return result.AddressInfoList;
        }
    }
}
