using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_KZ : ShippingProviderBase
    {
        #region Constants and Fields
        public static readonly decimal CartAmountCap = 18000.00M;
        private const string CacheKey = "DeliveryInfo_KZ";
        private const int InShippinginfoCacheMinutes = 60;
        #endregion

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                if (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                {
                    HttpRuntime.Cache.Remove(CacheKey);
                }
                return GetDeliveryOptionsFromCache(System.Threading.Thread.CurrentThread.CurrentCulture.Name, address);
            }

            var lstDeliveryOptions = base.GetDeliveryOptions(type, address);
            if (lstDeliveryOptions != null)
            {
                lstDeliveryOptions = (from l in lstDeliveryOptions orderby l.Description select l).ToList();
            }
            return lstDeliveryOptions;
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = GetDeliveryOptionsFromService(locale);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }

            if (deliveryOptions == null)
            {
                return null;
            }

            if (address != null && address.Address != null)
            {
                var byState = from po in deliveryOptions
                              where
                                  po.Address.StateProvinceTerritory.ToUpper()
                                    .Equals(address.Address.StateProvinceTerritory.ToUpper())
                              select po;

                if (!string.IsNullOrEmpty(address.Address.City))
                {
                    var byCity = from po in byState
                                 where po.Address.City.ToUpper().Equals(address.Address.City.ToUpper())
                                 select po;
                    return byCity.OrderBy(d => d.DisplayName).ToList();
                }

                return byState.OrderBy(d => d.DisplayName).ToList();
            }
            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string locale)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            DeliveryPickupAlternativesResponse_V04 pickupAlternativesResponse = null;
            pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V04 { Locale = locale })).GetDeliveryPickupAlternativesResult as
                DeliveryPickupAlternativesResponse_V04;
            if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
            {
                deliveryOptions.AddRange(
                    from po in pickupAlternativesResponse.DeliveryPickupAlternatives select new DeliveryOption(po, true));
            }

            return deliveryOptions;
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(
                    CacheKey,
                    options,
                    null,
                    DateTime.Now.AddMinutes(InShippinginfoCacheMinutes),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }
        }

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
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                {
                    string countryCode = locale.Substring(3, 2);
                    List<PickupLocationPreference_V01> pickupLocationPreference = GetPickupLocationsPreferences(distributorId, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var pickupLocation = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                        var accessPoints = GetDeliveryOptionsFromCache(locale, null);
                        if (pickupLocation != null && accessPoints != null)
                        {
                            var accessPoint = accessPoints.FirstOrDefault(ap => ap.Id == pickupLocation.PickupLocationID);
                            if (accessPoint != null)
                            {
                                return string.Format("{0} {1}", accessPoint.Id, accessPoint.Name);
                            }
                        }
                    }
                    return string.Empty;
                }
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0} {1} Спасибо за Ваш заказ", shoppingCart.DeliveryInfo.Instruction, shoppingCart.DeliveryInfo.Address.Recipient).Trim();
            }
            }            

            return string.Format(
                "{0} Спасибо за Ваш заказ",
                shoppingCart.DeliveryInfo.Address.Recipient);
        }

        public override ShippingInfo GetShippingInfoFromID(
           string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            DeliveryOption deliveryOptionForAddress = null;
            ShippingInfo shippingInfo = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference =
                    this.GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        var pickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = this.GetDeliveryOptions(type, null);
                        if (doList != null)
                        {
                            deliveryOptionForAddress = doList.Find(d => d.Id == pickupLocationID);
                            if (deliveryOptionForAddress != null)
                            {
                                shippingInfo = new ShippingInfo(deliveryOptionForAddress) { Id = deliveryOptionID };
                                return shippingInfo;
                            }
                        }
                    }
                    return shippingInfo;
                }
            }

            return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
        }
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            if (null != address && null != address.Address)
            {
                string postalCode = address.Address.PostalCode;
                string state = address.Address.StateProvinceTerritory;

                if (!string.IsNullOrEmpty(postalCode) && !string.IsNullOrEmpty(state))
                {
                    var freightCodeAndWarehouseFromService = GetFreightCodeAndWarehouseFromService(address);
                    if (freightCodeAndWarehouseFromService != null)
                    {
                        freightCodeAndWarehouse[0] = freightCodeAndWarehouseFromService[0] ?? freightCodeAndWarehouse[0];
                        freightCodeAndWarehouse[1] = freightCodeAndWarehouseFromService[1] ?? freightCodeAndWarehouse[1];
                    }
                }
            }
            return freightCodeAndWarehouse;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "KZ",
                Locale = "ru-KZ",
                State = address.Address.StateProvinceTerritory
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(
            string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            List<PickupLocationPreference_V01> pickupLocationPreferencestoRemove = new List<PickupLocationPreference_V01>();
            // Verify the alias for the locations to generate a display name if needed
            // and check if the location exists
            foreach (var location in pickupLocations)
            {
                // Verify if the location exists
                var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                if (shippingInfo == null)
                {
                    location.PickupLocationNickname = null;
                    pickupLocationPreferencestoRemove.Add(location);
                    continue;
                }

                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                    var address = new ShippingAddress_V02(
                        shippingInfo.Address.ID,
                        shippingInfo.Description,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        shippingInfo.Address.Address,
                        string.Empty,
                        string.Empty,
                        shippingInfo.Address.IsPrimary,
                        shippingInfo.Address.Alias,
                        DateTime.Now);
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
            if (pickupLocationPreferencestoRemove.Count > 0)
            {
                foreach (var item in pickupLocationPreferencestoRemove)
                {
                    DeletePickupLocationsPreferences(item.DistributorID, item.PickupLocationID, item.Country);
                }
            }
            return pickupLocations.Where(l => !string.IsNullOrEmpty(l.PickupLocationNickname)).ToList();
        }
    }
}
