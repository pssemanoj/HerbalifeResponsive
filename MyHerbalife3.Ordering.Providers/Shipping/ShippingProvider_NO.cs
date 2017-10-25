using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_NO : ShippingProviderBase
    {
        public static readonly int ZIP_CAP = 4000;
        private static readonly string deliveryOptionsForShippingCacheKey = "DeliveryOptionsForShipping_NO";
        private static readonly string deliveryOptionsCacheKey = "DeliveryOptions_NO";

        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br>{1}<br>{2}, {3}<br>{4}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.City,
                                           address.Address.PostalCode,
                                           formatPhone(address.Phone))
                           : string.Format("{0},{1}<br>{2}<br>{3}",
                                           address.Address.Line1, address.Address.City, address.Address.PostalCode,
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
                return string.Format("{0}<br>{1},{2}<br>{3}, {4}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.PostalCode);
            }
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            string[] freightCodeAndWarehouse = new string[] {string.Empty, string.Empty};
            if (address != null && address.Address != null)
            {
                int postalCode = int.Parse(address.Address.PostalCode);

                if (postalCode < ZIP_CAP)
                {
                    freightCodeAndWarehouse[1] = "N5";
                    freightCodeAndWarehouse[0] = "NOR";
                }
                else if (isSvalbard(address.Address.PostalCode))
                {
                    freightCodeAndWarehouse[1] = "N5";
                    freightCodeAndWarehouse[0] = "NOA";
                }
                else
                {
                    freightCodeAndWarehouse[1] = "N5";
                    freightCodeAndWarehouse[0] = "NWF";
                }
            }
            return freightCodeAndWarehouse;
        }

        private bool isSvalbard(string postalCode)
        {
            int postal;
            if (int.TryParse(postalCode, out postal))
            {
                return postal >= 9170 && postal <= 9179;
            }
            return false;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               ShippingAddress_V01 address)
        {
            var final = new List<DeliveryOption>();

            //HOME DELIVERY
            var HomeDeliveryOption = new DeliveryOption();
            var FreightCodeAndWarehouse = GetFreightCodeAndWarehouse(address);
            HomeDeliveryOption.FreightCode = FreightCodeAndWarehouse[0].ToString();
            HomeDeliveryOption.Description = (string)HttpContext.GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform), "HomeDeliveryOption") ?? "";
            final.Add(HomeDeliveryOption);

            //PUDO (PICKUP)
            var PudoDeliveryOptions = getDeliveryOptionsForShipping(Country, address, locale);

            if (PudoDeliveryOptions != null && PudoDeliveryOptions.Count > 0)
            {
                final.AddRange(PudoDeliveryOptions);
            }
            return final;
        }

        private List<DeliveryOption> getDeliveryOptionsForShipping(string Country, ShippingAddress_V01 address, string Locale)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var deliveryOptionsFromCache = new List<DeliveryOption>();

            deliveryOptionsFromCache = getDeliveryOptionsForShippingFromCache();
            if (deliveryOptionsFromCache == null)
            {
                deliveryOptionsFromCache = getDeliveryOptionsForShippingService(Country, Locale);
                SetDeliveryOptionsForShippingToCache(deliveryOptionsFromCache);
            }

            if (deliveryOptionsFromCache != null)
            {
                int postalCode = int.Parse(address.Address.PostalCode);

                foreach (var option in deliveryOptionsFromCache)
                {
                    if (!String.IsNullOrEmpty(option.PostalCode) && option.PostalCode.Length >= 9)
                    {
                        int initialRangePostalCode = int.Parse(option.PostalCode.Substring(0, 4)),
                            finalRangepostalCode = int.Parse(option.PostalCode.Substring(5, 4));

                        if (postalCode >= initialRangePostalCode && postalCode <= finalRangepostalCode)
                        {
                            deliveryOptions.Add(option);
                        }
                    }
                }
            }
            return deliveryOptions;
        }

        private List<DeliveryOption> getDeliveryOptionsForShippingService(string Country, string Locale)
        {
            List<DeliveryOption> deliveryOptions = null;

            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryPickupAlternativesRequest_V02
            {
                CountryCode = Locale
            };

            var response =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(request)).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V02;
            if (response != null && response.DeliveryPickupAlternatives != null && response.DeliveryPickupAlternatives.Count > 0)
            {
                deliveryOptions = new List<DeliveryOption>();
                foreach (var option in response.DeliveryPickupAlternatives)
                {
                    deliveryOptions.Add(new DeliveryOption(option));
                }

            }
            return deliveryOptions;

        }

        private List<DeliveryOption> getDeliveryOptionsForShippingFromCache()
        {
            return HttpRuntime.Cache[deliveryOptionsCacheKey] != null ? HttpRuntime.Cache[deliveryOptionsForShippingCacheKey] as List<DeliveryOption> : null;
        }

        private void SetDeliveryOptionsForShippingToCache(List<DeliveryOption> DeliveryOptions)
        {
            lock (deliveryOptionsCacheKey)
            {
                HttpRuntime.Cache.Insert(deliveryOptionsForShippingCacheKey,
                    DeliveryOptions,
                    null,
                    DateTime.Now.AddMinutes(60),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }
        }
        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache("NO", System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }
       
        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[deliveryOptionsCacheKey] as List<DeliveryOption>;
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
              
            }

            return deliveryOptions;
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
                        List<DeliveryOption> doList = GetDeliveryOptions(type, new ShippingAddress_V01 { Address = new Address_V01 { Country = "NO" } });
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
        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(deliveryOptionsCacheKey,
                  options,
                  null,
                  DateTime.Now.AddMinutes(60),
                  Cache.NoSlidingExpiration,
                  CacheItemPriority.Normal,
                  null);
            }
        }
        public override bool DisplayHoursOfOperation(DeliveryOptionType option)
        {
            switch (option)
            {
                case DeliveryOptionType.PickupFromCourier:
                    return true;
                default:
                    return base.DisplayHoursOfOperation(option);
            }
        }
    }
}