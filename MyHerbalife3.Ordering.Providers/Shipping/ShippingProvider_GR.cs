using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Caching;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using System.Web.Security;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_GR : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_GR";
        private const int GR_SHIPPINGINFO_CACHE_MINUTES = 60;

        #region IShippingProvider Members

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache("GR", Thread.CurrentThread.CurrentCulture.Name, address);
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID,
                                                           string locale,
                                                           DeliveryOptionType type,
                                                           int deliveryOptionID,
                                                           int shippingAddressID)
        {
            DeliveryOption deliveryOption = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int PickupLocationID = vPickupLocation.PickupLocationID;
                        var doList = GetDeliveryOptions(type,
                                                        new ShippingAddress_V01
                                                            {
                                                                Address = new Address_V01 {Country = "GR"}
                                                            });
                        if (doList != null)
                        {
                            deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                            if (deliveryOption != null)
                            {
                                //deliveryOption.Id = deliveryOption.ID = deliveryOptionID;
                                var shippingInfo = new ShippingInfo(deliveryOption);
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

        public override List<string> GetStatesForCountry(string Country)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new StatesForCountryRequest_V02() { Country = Country, UseCourierTable = true};
            var response = proxy.GetStatesForCountry(new GetStatesForCountryRequest(request)).GetStatesForCountryResult as StatesForCountryResponse_V01;

            return response.States;
        }

        #endregion IShippingProvider Members

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country,
                                                                        string locale,
                                                                        ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null == deliveryOptions)
            {
                deliveryOptions = GetDeliveryOptionsFromService(Country, locale, address);
                SaveDeliveryOptionsToCache(deliveryOptions);
            }

            return deliveryOptions.OrderBy(d => d.DisplayName).ToList();
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            if (null != options)
            {
                HttpRuntime.Cache.Insert(CacheKey,
                                         options,
                                         null,
                                         DateTime.Now.AddMinutes(GR_SHIPPINGINFO_CACHE_MINUTES),
                                         Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal,
                                         null);
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country,
                                                                          string locale,
                                                                          ShippingAddress_V01 address)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            //Look if there is a postal code provided:

            DeliveryPickupAlternativesResponse_V03 pickupAlternativesResponse = null;

            pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03
                    {
                        CountryCode = address.Address.Country,
                        State = address.Address.StateProvinceTerritory
                    })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

            if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
            {
                deliveryOptions.AddRange(
                    from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                    select new DeliveryOption(po));
                Array.ForEach(deliveryOptions.ToArray(), a => a.Address = getAddress(a.Name, a.State));
            }

            return deliveryOptions;
        }

        private static Address_V01 getAddress(string storeAddress, string state)
        {
            var parts = storeAddress.Split(',');
            string street = string.Empty;
            string city = string.Empty;
            string postalCode = string.Empty;
            if (parts.Length == 1)
            {
                street = parts[0];
            }
            else if (parts.Length == 2)
            {
                city = parts[1].Trim().Substring(0, 2);
                postalCode = parts[1].Trim().Substring(2).Trim();
                street = parts[0];
            }
            else if (parts.Length == 3)
            {
                city = parts[2].Trim().Substring(0, 2);
                postalCode = parts[2].Trim().Substring(2).Trim();
                street = parts[0] + ", " + parts[1];
            }
            return new Address_V01
                {
                    Country = "GR",
                    StateProvinceTerritory = state,
                    Line1 = street,
                    City = city,
                    PostalCode = postalCode,
                };
        }

        public override bool ShouldRecalculate(string oldFreightCode,
                                               string newFreightCode,
                                               Address_V01 oldaddress,
                                               Address_V01 newaddress)
        {
            if (oldFreightCode != newFreightCode)
                return true;
            if (oldaddress == null || newaddress == null ||
                (!string.IsNullOrEmpty(oldaddress.PostalCode) && !string.IsNullOrEmpty(newaddress.PostalCode) &&
                 !oldaddress.PostalCode.Equals(newaddress.PostalCode)))
                return true;
            return false;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            var shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
                {
                    string countryCode = locale.Substring(3, 2);
                    var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var courier = pickupLocationPreference.Find(p => p.ID == shippingInfo.Id);
                        if (courier != null)
                        {
                            return courier.PickupLocationID.ToString();
                        }
                    }
                    return string.Empty;
                }
                else
                {
                    return shippingInfo.Instruction;
                }
            }
            return string.Empty;
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
                                       ? string.Format("{0}<br>{1},{2}<br>{3}{4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       string.IsNullOrEmpty(address.Address.City)
                                                           ? string.Empty
                                                           : string.Format("{0}, ", address.Address.City),
                                                       address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}{3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       string.IsNullOrEmpty(address.Address.City)
                                                           ? string.Empty
                                                           : string.Format("{0}, ", address.Address.City),
                                                       address.Address.StateProvinceTerritory, address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4} {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.StateProvinceTerritory, address.Address.City,
                                                 address.Address.PostalCode);
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}{4}, {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 string.IsNullOrEmpty(address.Address.City.Trim())
                                                     ? string.Empty
                                                     : string.Format("{0}, ", address.Address.City.Trim()),
                                                 address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
            if (formattedAddress.IndexOf(",,") > -1 || formattedAddress.IndexOf(", ,") > -1)
            {
                return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
            }
            else
            {
                return formattedAddress;
            }
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                         string country,
                                                                                         string locale,
                                                                                         DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            // Verify the alias for the locations to generate a display name if needed
            foreach (var location in pickupLocations)
            {
                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                    var shippingInfo = GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                    if (shippingInfo != null)
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
                        location.PickupLocationNickname = GetAddressDisplayName(address);
                    }
                    else
                    {
                        location.PickupLocationNickname = "DELETE";
                    }
                }
            }
            pickupLocations = pickupLocations.Where(l => !l.PickupLocationNickname.Equals("DELETE")).ToList();
            return pickupLocations;
        }

        public override Address_V01 GetHFFDefaultAddress(ShippingAddress_V01 address)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            string distributorId = (member != null) ? member.Value.Id : "";
            Address_V01 hffAddress = new Address_V01();

            if (!string.IsNullOrEmpty(distributorId))
                {
                if (IsMemberTaxRegistered(distributorId))
                {
                    hffAddress = ObjectMappingHelper.Instance.GetToShipping(DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.PermanentLegalLocal, distributorId, "GR"));
                }
                else
                {
                    hffAddress = ObjectMappingHelper.Instance.GetToShipping(DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.MailingLocal, distributorId, "GR"));
                }
            }
            else  //if unable to retrieve DistributorId, do the default process
            {
                if (address != null && address.Address != null)
        {
                    hffAddress = new Address_V01
                {
                        Country = address.Address.Country,
                        StateProvinceTerritory = address.Address.StateProvinceTerritory,
                        City = address.Address.City,
                        Line1 = address.Address.Line1,
                        PostalCode = address.Address.PostalCode,
                };
        }

                hffAddress = base.GetHFFDefaultAddress(address);
            }

            return hffAddress;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(string.Empty, string.Empty);
            if (sessionInfo.IsEventTicketMode)
            {
                return new[]
                    {
                        HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode,
                        HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode
                    };
            }

            var freightCodeAndWarehouse = new[] {"HEO", "G7"};
            if (null != address && null != address.Address)
            {
                int postCode = 0;
                if (Int32.TryParse(address.Address.PostalCode.Replace(" ", string.Empty), out postCode))
                {
                    if ((postCode >= 10000 && postCode <= 19900) ||
                        (postCode >= 80000 && postCode <= 80999))
                    {
                        freightCodeAndWarehouse = new[] {"HEA", "G7"};
                    }
                }
            }
            return freightCodeAndWarehouse;
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
                if (shoppingCart.DeliveryInfo != null)
                {
                    shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                }
                return true;
            }

            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                return true;
            }

            if (ShoppingCartProvider.IsStandaloneHFF(shoppingCart.ShoppingCartItems))
            {
                if (shoppingCart.DeliveryInfo != null)
                {
                    var freightCodeAndWarehouse = GetFreightCodeAndWarehouse(shoppingCart.DeliveryInfo.Address);
                    shoppingCart.DeliveryInfo.FreightCode = shippment.ShippingMethodID = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                    shoppingCart.DeliveryInfo.WarehouseCode = shippment.WarehouseCode = freightCodeAndWarehouse[1];
                }
                return true;
            }

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                (shoppingCart.DeliveryInfo.FreightCode.Equals(HLConfigManager.Configurations.APFConfiguration.APFFreightCode) ||
                shoppingCart.DeliveryInfo.FreightCode.Equals(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode)))
            {
                var freightCodeAndWarehouse = GetFreightCodeAndWarehouse(shoppingCart.DeliveryInfo.Address);
                shoppingCart.DeliveryInfo.FreightCode = shippment.ShippingMethodID = freightCodeAndWarehouse[0];
                shoppingCart.DeliveryInfo.WarehouseCode = shippment.WarehouseCode = freightCodeAndWarehouse[1];
            }
            return true;
        }

        private bool IsMemberTaxRegistered(string distributorId)
        {            
            bool IsTaxRegistered = false;

            if (!string.IsNullOrEmpty(distributorId))
            {
                //GRSS,GRBL & GRRC
                List<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
                var GRRC = tins.Find(p => p.IDType.Key == "GRRC");
                var GRSS = tins.Find(p => p.IDType.Key == "GRSS");
                var GRBL = tins.Find(p => p.IDType.Key == "GRBL");
                IsTaxRegistered = (GRRC != null && GRSS != null && GRBL != null);
            }

            return IsTaxRegistered;
        }
    }
}