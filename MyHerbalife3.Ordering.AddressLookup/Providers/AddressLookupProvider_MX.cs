using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProvider_MX : AddressLookupProviderBase
    {
        public override List<string> GetStatesForCountry(string country)
        {
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            var request = new GetAllStatesRequest();
            var response = proxy.GetAllStates(request);
            var result = response.GetAllStatesResult as AllStatesResponse_V01;
            return result.StateNames.ToList();
        }

        public override List<string> GetCitiesForState(string country, string state)
        {
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            var request = new MunicipalitiesForStateRequest_V01() { State = state };
            var response = proxy.GetMunicipalitiesForState(new GetMunicipalitiesForStateRequest(request));
            var result = response.GetMunicipalitiesForStateResult as MunicipalitiesForStateResponse_V01;
            return result.Municipalities.ToList();
        }

        public override List<string> GetStreetsForCity(string country, string state, string city)
        {
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            ColoniesForMunicipalityRequest_V01 request = new ColoniesForMunicipalityRequest_V01()
            {
                State = state,
                Municipality = city
            };
            var response = proxy.GetColoniesForMunicipality(new GetColoniesForMunicipalityRequest(request));
            var result = response.GetColoniesForMunicipalityResult as ColoniesForMunicipalityResponse_V01;
            return result.ColonyNames.ToList();
        }

        public override string LookupZipCode(string state, string municipality, string colony)
        {
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            ZipCodeLookupRequest_V01 request = new ZipCodeLookupRequest_V01()
            {
                State = state,
                Municipality = municipality,
                Colony = colony
            };
            var response = proxy.GetZipCode(new GetZipCodeRequest(request));
            var result = response.GetZipCodeResult as ZipCodeLookupResponse_V01;
            return result.ZipCode;
        }
    }
}