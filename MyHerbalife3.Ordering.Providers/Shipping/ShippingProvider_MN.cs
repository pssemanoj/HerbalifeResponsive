using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web;
using System.ServiceModel;
using System.Web.Caching;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_MN : ShippingProviderBase
    {
        private const string ShippingOptionsCacheKey = "DeliveryOptions_MN";
        private const int ShippingOptionsCacheMinutes = 60;

        /// <summary>
        /// Format the Shipping Instructions
        /// </summary>
        /// <param name="shoppingCart">The cart</param>
        /// <param name="distributorId">DS id</param>
        /// <param name="locale">Locale</param>
        /// <returns></returns>
        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            string instructions = string.Format("{0} Захиалга өгсөнд тань баярлалаа", shoppingCart.DeliveryInfo.Instruction);
                        
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                return string.Format("{0} {1} {2}", shoppingCart.DeliveryInfo.Address.Recipient, shoppingCart.DeliveryInfo.Address.Phone, instructions);
            }

            return instructions;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[ShippingOptionsCacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null || deliveryOptions.Count == 0)
            {
                if (deliveryOptions == null) deliveryOptions = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new DeliveryOptionForCountryRequest_V01
                    {
                        Country = Country,
                        State = Country,
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
                                       ? string.Format("{0}<br/>{1} {2}<br/>{3}, {4}<br/>{5}<br/>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.CountyDistrict, formatPhone(address.Phone))
                                       : string.Format("{0} {1}<br/>{2}, {3}<br/>{4}<br/>{5}",
                                                       address.Address.Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.CountyDistrict, formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }

    }
}
