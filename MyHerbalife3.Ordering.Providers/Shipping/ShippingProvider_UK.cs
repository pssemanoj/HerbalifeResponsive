using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_UK : ShippingProviderBase
    {
        public override DeliveryOption GetDefaultAddress()
        {
            var address = new Address_V01();
            address.StateProvinceTerritory = "UK";
            return
                new DeliveryOption(new ShippingAddress_V02()
                {
                    ID = 0,
                    Recipient = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    MiddleName = string.Empty,
                    Address = address,
                    Phone = "",
                    AltPhone = "",
                    IsPrimary = false,
                    Alias = "",
                    Created = DateTime.Now
                });
        }

        public override string GetAddressDisplayName(ShippingAddress_V02 address)
        {
            return string.Format("...{0},{1},{2},{3}", address.Recipient, address.Address.Line1, address.Address.City,
                                 address.Address.StateProvinceTerritory);
        }

        public override bool ValidatePostalCode(string country, string state, string city, string postalCode)
        {
            bool isValid = false;
            //if(!string.IsNullOrEmpty(postalCode))
            //{
            //    postalCode = postalCode.Split(' ')[0];
            //}
            using (var client = ServiceClientProvider.GetShippingServiceProxy())
            {
                var requestV01 = new ValidatePostalCodeRequest_V01
                    {
                        City = city,
                        Country = country,
                        State = state,
                        PostalCode = postalCode
                    };

                var responseV01 = client.ValidatePostalCode(new ValidatePostalCodeRequest(requestV01)).ValidatePostalCodeResult as ValidatePostalCodeResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceResponseStatusType.Success)
                    isValid = responseV01.IsValidPostalCode;
            }

            //ZipCode Whitespace rule enforcement
            int whiteSpaceMatchCount = postalCode.Split(new[] {' '}).Length - 1;
            if (whiteSpaceMatchCount > 1) isValid = false;

            return isValid;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            if (null == address || null == address.Address)
                return string.Empty;

            return includeName
                       ? string.Format("{0}<br>{1},{2}<br>{3},{4}", address.Recipient ?? string.Empty,
                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                       address.Address.City, address.Address.PostalCode)
                       : string.Format("{0},{1}<br>{2},{3}",
                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                       address.Address.City,
                                       address.Address.PostalCode);
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            //standalone HFF order, Freight code : USI
            if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count == 1)
            {
                if (shoppingCart.CartItems[0].SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku)
                {
                    shippment.ShippingMethodID = "NOF";
                    return true;
                }
            }
            return false;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] {"USI", string.Empty};

            if (address != null && address.Address != null)
            {
                string postalCode = address.Address.PostalCode != null ? address.Address.PostalCode.ToUpper() : "";
                if (postalCode.StartsWith("GY") || postalCode.StartsWith("JE") || postalCode.StartsWith("IM"))
                {
                    freightCodeAndWarehouse[0] = "UKA";
                }
            }

            return freightCodeAndWarehouse;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            try
            {
                var final = new List<DeliveryOption>();
                if (string.IsNullOrEmpty(Country) || string.IsNullOrEmpty(locale) || null == address ||
                    string.IsNullOrEmpty(address.Address.PostalCode))
                {
                    return final;
                }

                final = GetDeliveryOptionsFromCache(Country, locale, address);

                //final = ConvertShippingAlternativetoDeliveryOption(response);
                return final;
            }
            catch
            {
                return null;
            }
        }


        private List<DeliveryOption> GetDeliveryOptionsFromCache(string Country,
                                                                             string locale,
                                                                             ShippingAddress_V01 address)
        {
            var cacheKey = string.Format("Deliveryoptions_Country:{0}_locale:{1}_PostalCode:{2}"
                                         , Country, locale,
                                         address.Address.PostalCode.Substring(0, 2));
            // tries to get object from cache
            List<DeliveryOption> list = HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;
            
            if (null == list)
            {
                var response = GetDeliveryOptionsFromService(Country, locale, address);
                list = ConvertShippingAlternativetoDeliveryOption(response);
                SaveDeliveryoptionsToCache(cacheKey,list);
            }
            return list;
        }

        private static void SaveDeliveryoptionsToCache(string cacheKey, List<DeliveryOption> options)
        {
            HttpRuntime.Cache.Insert(cacheKey,
                                     options,
                                     null,
                                     DateTime.Now.AddMinutes(60),
                                     Cache.NoSlidingExpiration,
                                     CacheItemPriority.Normal,
                                     null);
        }
        
        private ShippingAlternativesResponse_V01 GetDeliveryOptionsFromService(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();
            request.Country = Country;
            request.State = address.Address.PostalCode.Substring(0, 2);
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            if (response.DeliveryAlternatives.Count == 0)
            {
                request.State = "*";
                response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            }
            return response;
        }

        public List<DeliveryOption> ConvertShippingAlternativetoDeliveryOption(ShippingAlternativesResponse_V01 response)
        {
            if (response.DeliveryAlternatives != null && response.DeliveryAlternatives.Count>0)
                return response.DeliveryAlternatives.Select(option => new DeliveryOption(option)).ToList();
            return new List<DeliveryOption>();
        }
    }
}