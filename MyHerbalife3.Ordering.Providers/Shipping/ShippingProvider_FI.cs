using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_FI : ShippingProviderBase
    {
        private static readonly string deliveryOptionsCacheKey = "DeliveryOptions_FI";
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
              return includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else if(type ==DeliveryOptionType.PickupFromCourier)
            {
                return string.Format("{0}<br>{1},{2}<br>{3} {4}", description,
                   address.Address.Line1, address.Address.Line2 ?? string.Empty,
                   address.Address.City, address.Address.PostalCode);
            }
            else
            {
              return string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                                address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                address.Address.City, address.Address.StateProvinceTerritory,
                                                address.Address.PostalCode);
            }
           
        }
        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> cachedOptions = base.GetDeliveryOptions(locale);
            List<DeliveryOption> deliveryOptionList = new List<DeliveryOption>();
            if (address != null)
            {
                int postCode = 0;
                bool success = int.TryParse(address.Address.PostalCode, out postCode);
                if (success)
                {
                    /// not “Åland”
                    DeliveryOption option = null;
                    if (postCode < 22000 || postCode > 22999)
                    {
                        option = cachedOptions.Find(o => o.FreightCode == "FNF");
                        if (null != option)
                        {
                            deliveryOptionList.Add(option);
                        }

                        option = cachedOptions.Find(o => o.FreightCode == "FHD");
                        if (null != option)
                        {
                            deliveryOptionList.Add(option);
                        }
                    }
                    else
                    {
                        option = cachedOptions.Find(o => o.FreightCode == "FIA");
                        if (null != option)
                        {
                            deliveryOptionList.Add(option);
                        }
                    }
                }
            }

            return deliveryOptionList;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (currentShippingInfo != null && currentShippingInfo.Option == DeliveryOptionType.Shipping)
            {
                int postCode = 0;
                if (currentShippingInfo.Address != null)
                {
                    bool success = int.TryParse(currentShippingInfo.Address.Address.PostalCode, out postCode);
                    if (success)
                    {
                        /// “Åland”
                        if (postCode >= 22000 && postCode <= 22999)
                        {
                            DeliveryOption option = base.GetDeliveryOptions(locale).Find(o => o.FreightCode == "FIA");
                            if (null != option)
                            {
                                if (!string.IsNullOrEmpty(option.Description))
                                {
                                    return option.Description;
                                }
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache("FI", System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
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
                        List<DeliveryOption> doList = GetDeliveryOptions(type, new ShippingAddress_V01 { Address = new Address_V01 { Country = "FI" } });
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
