using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Threading.Tasks;
using System.Globalization;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_BG: ShippingProviderBase
    {
        private const string ShippingOptionsCacheKey = "DeliveryOptions_BG";
        private const int ShippingOptionsCacheMinutes = 60;

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (address == null || address.Address == null)
            {
                return string.Empty;
            }

            var formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br/>{1} {2}<br/>{3} {4}<br/>{5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2,
                                                       address.Address.PostalCode, address.Address.City,
                                                       address.Address.Country)
                                       : string.Format("{0} {1} <br/>{2} {3}<br/>{4}",
                                                       address.Address.Line1, address.Address.Line2,
                                                       address.Address.PostalCode, address.Address.City,
                                                       address.Address.Country);
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {            
            string shipInstructions = "";

            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {   
                shipInstructions = string.Format("{0}", shoppingCart.DeliveryInfo.Instruction.Trim().Replace('\n',' '));
            }
            else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                CultureInfo culture = new CultureInfo(locale);
                shipInstructions = string.Format("{0}, {1}",                                                         
                                                        String.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Phone) ? "" : shoppingCart.DeliveryInfo.Address.Phone,
                                                        shoppingCart.DeliveryInfo.PickupDate == null ? "" : shoppingCart.DeliveryInfo.PickupDate.Value.ToString("d", culture));
            }

            return shipInstructions;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[ShippingOptionsCacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null || deliveryOptions.Count == 0)
            {
                if (deliveryOptions == null)
                {
                    deliveryOptions = new List<DeliveryOption>();
                }

                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new DeliveryOptionForCountryRequest_V01
                {
                    Country = Country,
                    State = "*",
                    Locale = locale
                };
                var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                if (response != null && response.DeliveryAlternatives != null)
                {
                    deliveryOptions.AddRange(response.DeliveryAlternatives.Select(option => new DeliveryOption(option)));
                }
                if (deliveryOptions.Any())
                {
                    HttpRuntime.Cache.Insert(ShippingOptionsCacheKey, deliveryOptions, null,
                                             DateTime.Now.AddMinutes(ShippingOptionsCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
            }
            return deliveryOptions;
        }
    }
}
