using System.Collections.Generic;
using System.ServiceModel;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_IE : ShippingProviderBase
    {
        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();
            request.Country = country;
            request.State = string.Empty;
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var final = new List<DeliveryOption>();
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                final.Add(new DeliveryOption(option));
            }

            return final;
        }
    }
}