using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_PT : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_PT";
        private const int PT_SHIPPINGINFO_CACHE_MINUTES = 60;

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string country, string locale, ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();
            if (!String.IsNullOrEmpty(address.Address.PostalCode))
            {
                int postalCodeIntValue = 0;
                bool validCode = int.TryParse(address.Address.PostalCode.Split('-')[0], out postalCodeIntValue);
                if (validCode)
                {
                    var deliveryOptionList = GetDeliveryOptionsListForShipping(locale);
                    if (deliveryOptionList.Count > 0)
                    {
                        foreach (var option in deliveryOptionList)
                        {
                            var minRange = 0;
                            var maxRange = 0;
                            var range = option.PostalCode.Split('-');
                            if (range.Count() == 2 && int.TryParse(range[0], out minRange) &&
                                int.TryParse(range[1], out maxRange))
                            {
                                if (postalCodeIntValue >= minRange && postalCodeIntValue <= maxRange)
                                {
                                    final.Add(option);
                                }
                            }
                        }
                    }

                    if (final.Count == 0)
                    {
                        var proxy = ServiceClientProvider.GetShippingServiceProxy();
                        DeliveryOptionForCountryRequest_V01 request = new DeliveryOptionForCountryRequest_V01();
                        request.Country = country;
                        request.State = String.Empty;
                        request.Locale = locale;
                        ShippingAlternativesResponse_V01 response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                        foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
                        {
                            final.Add(new DeliveryOption(option));
                        }
                    }
                }
            }
            return final;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (!String.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    return "***INVOICE TO DS***";
                }
            }
            return String.Empty;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName ? string.Format("{0}<br>{1},{2}<br>{3}, {4}<br>{5}", address.Recipient ?? string.Empty,
                   address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                   address.Address.PostalCode,
                   formatPhone(address.Phone)) :
                   string.Format("{0},{1}<br>{2}, {3}<br>{4}",
                   address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.PostalCode,
                   formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                return string.Format("{0}<br>{1},{2}<br>{3} {4}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty,
                    address.Address.City, address.Address.PostalCode);
            }
            else
            {
                return string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}, {5}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.Line3 ?? string.Empty, address.Address.City, address.Address.PostalCode);
            }
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache("PT", System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null == deliveryOptions)
            {
                deliveryOptions = GetDeliveryOptionsFromService(Country, locale, address);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }
            if (!string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
            {
                return deliveryOptions.Where(d => d.State == address.Address.StateProvinceTerritory).OrderBy(d => d.DisplayName).ToList();
            }
            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            //Look if there is a postal code provided:

            DeliveryPickupAlternativesResponse_V03 pickupAlternativesResponse = null;

            pickupAlternativesResponse = proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03() { CountryCode = address.Address.Country })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

            if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
            {
                deliveryOptions.AddRange(
                    from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                    select new DeliveryOption(po));
                Array.ForEach(deliveryOptions.ToArray(), a => a.Address = getAddress(a.Name, a.State));
            }

            return deliveryOptions;
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(CacheKey,
                  options,
                  null,
                  DateTime.Now.AddMinutes(PT_SHIPPINGINFO_CACHE_MINUTES),
                  Cache.NoSlidingExpiration,
                  CacheItemPriority.Normal,
                  null);
            }
        }

        private static Address_V01 getAddress(string storeAddress, string city)
        {
            string[] parts = storeAddress.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
            string street = string.Empty;
            string pudo = string.Empty;
            string postalCode = string.Empty;

            if (parts.Length == 1)
            {
                street = parts[0];
            }
            else if (parts.Length == 3)
            {
                pudo = parts[1].Trim();
                postalCode = parts[2].Trim().Trim();
                street = parts[0];
            }

            return new Address_V01
            {
                Country = "PT",
                StateProvinceTerritory = "PT",
                Line1 = pudo,
                Line2 = street,
                City = city,
                PostalCode = postalCode,
            };
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            DeliveryOption deliveryOption = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                List<PickupLocationPreference_V01> pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int PickupLocationID = vPickupLocation.PickupLocationID;
                        List<DeliveryOption> doList = GetDeliveryOptions(type, new ShippingAddress_V01 { Address = new Address_V01 { Country = "PT" } });
                        if (doList != null)
                        {
                            deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                            if (deliveryOption != null)
                            {
                                //deliveryOption.Id = deliveryOption.ID = deliveryOptionID;
                                ShippingInfo shippingInfo = new ShippingInfo(deliveryOption);
                                shippingInfo.Id = deliveryOptionID;
                                return shippingInfo;
                            }
                        }
                    }
                }
            }
            else
            {
                return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            }

            return null;
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            List<PickupLocationPreference_V01> pickupLocationPreferencestoRemove = new List<PickupLocationPreference_V01>();
            // Verify the alias for the locations to generate a display name if needed
            foreach (var location in pickupLocations)
            {
                var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID,
                                                                      0);
                if (shippingInfo != null)
                {
                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                        var address = new ShippingAddress_V02(shippingInfo.Address.ID, shippingInfo.Description, string.Empty, string.Empty,
                                                              string.Empty, shippingInfo.Address.Address, string.Empty, string.Empty, shippingInfo.Address.IsPrimary, shippingInfo.Address.Alias, DateTime.Now);
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
                else
                {
                    pickupLocationPreferencestoRemove.Add(location);
                }
            }
            //removing the not existing locations in PUDO
            if (pickupLocationPreferencestoRemove.Count > 0)
            {
                foreach (var item in pickupLocationPreferencestoRemove)
                {
                    DeletePickupLocationsPreferences(item.DistributorID, item.PickupLocationID, item.Country);
                }
            }
            return pickupLocations;
        }

        private List<DeliveryOption> GetDeliveryOptionsListForShipping(string locale)
        {
            const string cacheKey = "PT_DeliveryOptionByZipCode";
            var shippingOptions =  HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;
            if (shippingOptions == null)
            {
                shippingOptions = base.GetDeliveryOptions(locale);
                shippingOptions =
                    shippingOptions.Where(
                        o =>
                        !string.IsNullOrEmpty(o.PostalCode) && o.Option == DeliveryOptionType.Shipping &&
                        o.OrderCategory == OrderCategoryType.RSO).ToList();
                HttpRuntime.Cache.Insert(cacheKey, shippingOptions, null,
                                            DateTime.Now.AddMinutes(PT_SHIPPINGINFO_CACHE_MINUTES),
                                            Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                
            }
            return shippingOptions;
        }
    }
}