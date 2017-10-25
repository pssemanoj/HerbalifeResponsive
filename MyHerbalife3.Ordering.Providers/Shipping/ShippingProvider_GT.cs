using System;
using System.Web;
using System.Web.Caching;
using System.Linq;
using System.Collections.Generic;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_GT : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_GT";
        private const int GT_DELIVERYINFO_CACHE_MINUTES = 60;
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0},{1}", string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction) ? string.Empty : shoppingCart.DeliveryInfo.Instruction, "Gracias por su Orden");
            }
            else
            {

                return string.Format("{0},{1},{2},{3}",shoppingCart.DeliveryInfo.Address.Recipient,shoppingCart.DeliveryInfo.Address.Phone, string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction) ? string.Empty : shoppingCart.DeliveryInfo.Instruction, "Gracias por su Orden");
            }
            
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
                formattedAddress = includeName ? 
                    string.Format("{0}<br/>{1}{2}{3}<br/>{4}, {5}, {6}<br/>{7}",
                        address.Recipient ?? string.Empty,
                        address.Address.Line1,
                        ! string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br/>" + address.Address.Line2 : "", 
                        ! string.IsNullOrWhiteSpace(address.Address.Line4) ? "<br/>" + address.Address.Line4 : "",
                        address.Address.City, address.Address.StateProvinceTerritory,
                        address.Address.Country,
                        formatPhone(address.Phone))
                    : string.Format("{0}{1}{2}<br/>{3}, {4}, {5}<br/>{6}",
                        address.Address.Line1,
                        ! string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br/>" + address.Address.Line2 : "", 
                        ! string.IsNullOrWhiteSpace(address.Address.Line4) ? "<br/>" + address.Address.Line4 : "",
                        address.Address.City, address.Address.StateProvinceTerritory, 
                        address.Address.Country,
                        formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            //Looking for WH and Freight code
            var optionForCity = GetDeliveryOptionsListForShippingFromService(address);
            if (optionForCity != null && !string.IsNullOrEmpty(optionForCity.FreightCode) && !string.IsNullOrEmpty(optionForCity.WarehouseCode))
            {
                freightCodeAndWarehouse[0] = optionForCity.FreightCode;
                freightCodeAndWarehouse[1] = optionForCity.WarehouseCode;
            }

            return freightCodeAndWarehouse;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                       string locale,
                                                                       ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();
            var optionForCity = GetDeliveryOptionsListForShippingFromService(address);
            if (optionForCity == null)
            {
                var stdOption = new DeliveryOption
                    {
                        FreightCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                        WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse,
                        Description = "Su pedido será enviado con el Servicio de Entrega Regular"
                    };
                final.Add(stdOption);
            }
            else
            {
                final.Add(optionForCity);
            }

            return final;
        }

        private DeliveryOption GetDeliveryOptionsListForShippingFromService(ShippingAddress_V01 address)
        {
            if (address != null && address.Address != null &&
                !string.IsNullOrEmpty(address.Address.StateProvinceTerritory) &&
                !string.IsNullOrEmpty(address.Address.City))
            {
                var cityState = IsValidShippingAddress(address.Address) ?
                    string.Format("{0}|{1}|{2}", address.Address.StateProvinceTerritory, address.Address.City, address.Address.Line4) :
                    string.Format("{0}|{1}|{2}|{3}", address.Address.StateProvinceTerritory, address.Address.CountyDistrict, address.Address.Line3, address.Address.Line4);
                var options = HttpRuntime.Cache[CacheKey] as Dictionary<string, DeliveryOption>;
                if (options == null || !options.ContainsKey(cityState))
                {
                    using (var proxy = ServiceClientProvider.GetShippingServiceProxy())
                    {
                        try
                        {
                            var request = new DeliveryOptionForCountryRequest_V01
                            {
                                Country = "GT",
                                State = cityState,
                                Locale = "es-GT"
                            };

                            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                            if (response != null && response.DeliveryAlternatives != null && response.DeliveryAlternatives.Count > 0)
                            {
                                var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
                                if (shippingOption != null)
                                {
                                    if (options == null) options = new Dictionary<string, DeliveryOption>();
                                    options.Add(cityState, new DeliveryOption(shippingOption));
                                    HttpRuntime.Cache.Insert(CacheKey, options, null, DateTime.Now.AddMinutes(GT_DELIVERYINFO_CACHE_MINUTES),
                                         Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error(string.Format("GetDeliveryOptionsListForShippingFromService error: Country: GT, error: {0}", ex.ToString()));
                        }
                    }
                }
                if (options != null && options.ContainsKey(cityState))
                {
                    var deliveryOption = options.FirstOrDefault(o => o.Key == cityState);
                    return deliveryOption.Value;
                }
            }

            return null;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||                
                string.IsNullOrWhiteSpace(a.City))
                // fails when State or County are empty (they're required)
                return false;

            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City))
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }

    }
}
