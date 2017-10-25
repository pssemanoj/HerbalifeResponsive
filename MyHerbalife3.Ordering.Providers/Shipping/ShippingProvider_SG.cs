using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_SG : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryMethod_SG";
        private const int SG_SHIPPINGINFO_CACHE_MINUTES = 60;

        public override DeliveryOption GetDefaultAddress()
        {
            Address_V01 address = new Address_V01();
            address.City = "SINGAPORE";
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

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorId,
                                                            string locale)
        {
            string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                     ? string.Empty
                                     : shoppingCart.DeliveryInfo.Instruction;
            if (shoppingCart.DeliveryInfo.Address != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    instruction = string.Format("{0},{1},{2}", shoppingCart.DeliveryInfo.Address.Phone, instruction,
                                                string.IsNullOrEmpty(shoppingCart.DeliveryDate)
                                                    ? string.Empty
                                                    : shoppingCart.DeliveryDate);
                }
                else
                {
                    return
                        instruction =
                        string.Format("{0},{1},{2}", shoppingCart.DeliveryInfo.Address.Recipient,
                                      shoppingCart.DeliveryInfo.Address.Phone, instruction);
                }
            }
            return instruction;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1} {2}<br>{3} {4}<br>{5},{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0} {1}<br>{2} {3}<br>{4},{5}",
                                                       address.Address.Line1, address.Address.Line2, address.Address.City,
                                                       address.Address.StateProvinceTerritory, address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}<br>{2} {3}", address.Address.Line1,
                                                 address.Address.Line2, address.Address.City, address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            List<DeliveryOption> result = null;
            if (null != deliveryOptions && deliveryOptions.Count > 0)
            {
                string city = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.City))
                                  ? address.Address.City
                                  : "SINGAPORE";
                result = deliveryOptions.Where(d => d.Address.City == city).ToList();
                if (null == result || result.Count == 0)
                {
                    result = GetDeliveryOptionsFromService(Country, locale, address);
                    SaveDeliveryOptionsToCache(result);
                }
            }
            else
            {
                result = GetDeliveryOptionsFromService(Country, locale, address);
                SaveDeliveryOptionsToCache(result);
            }

            return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
        }

        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }

        public override bool ValidateShippingInstructionsDate(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }

        public override bool ShippingInstructionDateTodayNotAllowed()
        {
            return true;
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country,
                                                                          string locale,
                                                                          ShippingAddress_V01 address)
        {
            List<DeliveryOption> result = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            DeliveryOptionForCountryRequest_V01 request = new DeliveryOptionForCountryRequest_V01();
            request.Country = Country;

            //request.State = request.State = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "HK";
            request.Locale = locale;
            ShippingAlternativesResponse_V01 response =
                proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                DeliveryOption currentOption = new DeliveryOption(option);
                currentOption.Name = option.Description;
                currentOption.WarehouseCode = option.WarehouseCode;
                currentOption.State = request.State;
                result.Add(currentOption);
            }
            return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null != deliveryOptions)
            {
                deliveryOptions.AddRange(options);
            }
            else
            {
                HttpRuntime.Cache.Insert(CacheKey,
                                         options,
                                         null,
                                         DateTime.Now.AddMinutes(SG_SHIPPINGINFO_CACHE_MINUTES),
                                         Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal,
                                         null);
            }
        }
    }
}