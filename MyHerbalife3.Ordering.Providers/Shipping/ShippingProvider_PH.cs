using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

    public class ShippingProvider_PH : ShippingProviderBase
    {

        private const string CacheKey = "DeliveryInfo_PH";
        private const int InShippinginfoCacheMinutes = 60 * 2;

        #region Public Methods and Operators

        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                if (shippingInfo != null)
                {
                    if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                    {
                        string countryCode = locale.Substring(3, 2);
                        List<PickupLocationPreference_V01> pickupLocationPreference =
                            GetPickupLocationsPreferences(distributorId, countryCode);
                        if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                        {
                            var pickupLocation = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                            var accessPoints = GetDeliveryOptionsFromCache(locale, null);
                            if (pickupLocation != null && accessPoints != null)
                            {
                                var ppp =
                                    accessPoints.Where(ap => ap.Id == pickupLocation.PickupLocationID).FirstOrDefault();
                                if (ppp != null)
                                {
                                    return string.Format("{0} {1}", ppp.Id, ppp.Name);
                                }
                            }
                        }

                    }

                }
                return string.Empty;
            }
            else
            {
                string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                         ? string.Empty
                                         : shoppingCart.DeliveryInfo.Instruction;
                if (shoppingCart.DeliveryInfo.Address != null)
                {
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        return
                            instruction =
                            string.Format(
                                "{0},{1},{2}",
                                shoppingCart.DeliveryInfo.Address.Recipient,
                                shoppingCart.DeliveryInfo.Address.Phone,
                                instruction);
                    }
                }
                return instruction;
            }
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
                                deliveryOptionForAddress.Address.Line4 = deliveryOptionForAddress.Description;
                                shippingInfo = new ShippingInfo(deliveryOptionForAddress) { Id = deliveryOptionID };
                                return shippingInfo;
                            }
                        }
                    }
                    return shippingInfo;
                }
            }
            else
            {
                return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            }

            return null;
        }





        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type,
                                                     string description, bool includeName)
        {
            string formattedAddress;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                formattedAddress = includeName ?
                    string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}", address.Address.Line4 ?? string.Empty,
                    address.Address.Line2, address.Address.Line1 ?? string.Empty,
                    address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                    address.Address.Line2, address.Address.Line1 ?? string.Empty,
                    address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
                return formattedAddress;
            }
            else
            {
                return base.FormatShippingAddress(address, type, description, includeName);
            }
        }


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
            return base.GetDeliveryOptions(type, address);
        }

        public override List<string> GetStatesForCountry(string Country)
        {
            if (Country.Equals("en-PH"))
            {
                HttpRuntime.Cache.Remove(CacheKey);
                var pickupStores = GetDeliveryOptionsFromCache(Country, null);
                return
                    (from o in pickupStores select o.Address.StateProvinceTerritory).Distinct().OrderBy(s => s).ToList();
            }
            return base.GetStatesForCountry(Country);
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(
            string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            // Verify the alias for the locations to generate a display name if needed
            // and check if the location exists
            foreach (var location in pickupLocations)
            {
                // Verify if the location exists
                var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                if (shippingInfo == null)
                {
                    location.PickupLocationNickname = null;
                    continue;
                }

                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                    var address = new ShippingAddress_V02()
                    {
                        ID = shippingInfo.Address.ID,
                        Recipient = shippingInfo.Description,
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        MiddleName = string.Empty,
                        Address = shippingInfo.Address.Address,
                        Phone = string.Empty,
                        AltPhone = string.Empty,
                        IsPrimary = shippingInfo.Address.IsPrimary,
                        Alias = shippingInfo.Address.Alias,
                        Created = DateTime.Now
                    };
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
            return pickupLocations.Where(l => !string.IsNullOrEmpty(l.PickupLocationNickname)).ToList();
        }

        /// <summary>
        /// Gets the shipment information to import into HMS.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="shippment">The order shipment.</param>
        /// <returns></returns>
        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
            if (session.IsEventTicketMode)
            {
                return true;
            }

            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                return true;
            }

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                !shoppingCart.DeliveryInfo.FreightCode.Equals(HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode))
            {
                shoppingCart.DeliveryInfo.FreightCode =
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                shoppingCart.DeliveryInfo.WarehouseCode =
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
                shippment.WarehouseCode = shoppingCart.DeliveryInfo.WarehouseCode;
            }
            return true;
        }
        public override bool IsValidShippingAddress(MyHLShoppingCart shoppingCart)
        {
            return !(shoppingCart != null
                    && shoppingCart.DeliveryInfo != null
                    && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                    && shoppingCart.DeliveryInfo.Address != null
                    && shoppingCart.DeliveryInfo.Address.Address != null
                    && string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.City)
                    && string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory));
        }
        #endregion

        #region Private Methods
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
            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string locale)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            DeliveryPickupAlternativesResponse_V04 pickupAlternativesResponse = null;
            pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V04 { Locale = "en-RP" })).GetDeliveryPickupAlternativesResult as
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
        #endregion
    }
}