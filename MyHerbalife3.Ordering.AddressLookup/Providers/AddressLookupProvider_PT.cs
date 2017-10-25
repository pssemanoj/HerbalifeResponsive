using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProvider_PT : AddressLookupProviderBase
    {
        public override List<string> GetStatesForCountry(string country)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new StatesForCountryRequest_V02(){ Country = country, UseCourierTable = true};
            var response = proxy.GetStatesForCountry(new GetStatesForCountryRequest(request));
            var result = response.GetStatesForCountryResult as StatesForCountryResponse_V01;
            return result.States;
        }
    }
}