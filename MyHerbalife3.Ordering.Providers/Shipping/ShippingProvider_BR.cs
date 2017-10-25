using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using HL.Common.DataContract.Interfaces;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.RulesManagement;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using System.Globalization;

    using HL.Common.Configuration;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

    public class ShippingProvider_BR : ShippingProviderBase
    {
        private const string DEFAULT_PICKUP_STATE = "SP";
        private const string DEFAULT_PICKUP_WAREHOUSE = "43";
        private const string FREIGHTCODE_FOR_CUSTOMCOURIER = "PT";
        private const string CacheKey = "DeliveryMethod_BR";
        private const string CacheKeyForPostOffice = "DeliveryInfo_BR";
        private const int BR_SHIPPINGINFO_CACHE_MINUTES = 60;

        public override bool DSAddressAsShippingAddress()
        {
            return true;
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
            if (type == DeliveryOptionType.ShipToCourier)
            {
                var shippingInfo = base.GetShippingInfoFromID(distributorID, locale, DeliveryOptionType.Shipping, deliveryOptionID, shippingAddressID);
                var deliveryOptions = GetDeliveryOptions(type, shippingInfo.Address);
                if (deliveryOptions != null && deliveryOptions.Count > 0)
                {
                    var deliveryOption = deliveryOptions.FirstOrDefault();
                    if (deliveryOption != null)
                    {
                        shippingInfo.FreightCode = deliveryOption.FreightCode;
                        shippingInfo.WarehouseCode = deliveryOption.WarehouseCode;
                    }
                }
                else
                {
                    shippingInfo.FreightCode = FREIGHTCODE_FOR_CUSTOMCOURIER;
                }
                shippingInfo.Option = DeliveryOptionType.ShipToCourier;
                return shippingInfo;
            }
            else if (type == DeliveryOptionType.Pickup)
            {
                ShippingInfo shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
                if (shippingInfo != null)
                {
                    List<DeliveryOption> shippingAddresses = GetShippingAddresses(distributorID, locale);
                    if (shippingAddresses != null)
                    {
                        DeliveryOption deliveryOption = null;
                        deliveryOption = shippingAddressID != 0 ? shippingAddresses.Find(s => s.ID == shippingAddressID) : shippingAddresses.Find(s => s.IsPrimary);
                        if (deliveryOption != null)
                        {
                            if (sessionInfo != null && sessionInfo.ShoppingCart != null)
                            {
                                MyHLShoppingCart myShoppingCart = sessionInfo.ShoppingCart;
                                if (myShoppingCart != null)
                                {
                                    if (myShoppingCart.ShippingAddressID != deliveryOption.ID)
                                    {
                                        ShoppingCartProvider.UpdateShoppingCart(myShoppingCart);
                                    }
                                    myShoppingCart.ShippingAddressID = deliveryOption.ID;
                                }
                            }
                        }
                    }
                }
                return shippingInfo;
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                var countryCode = locale.Substring(3, 2);
                var pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.ID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        var addressV1 = new Address_V01 { Country = countryCode };
                        var address = new ShippingAddress_V01 { Address = addressV1 };
                        var deliveryOptions = GetDeliveryOptionsFromCache(address);
                        if (deliveryOptions != null)
                        {
                            var deliveryOption = deliveryOptions.Find(d => d.Id == vPickupLocation.PickupLocationID);
                            if (deliveryOption != null)
                            {
                                var shippingInfo = new ShippingInfo(deliveryOption)
                                {
                                    Id = deliveryOptionID,
                                    FreightCode = deliveryOption.FreightCode,
                                    WarehouseCode = deliveryOption.WarehouseCode
                                };
                                return shippingInfo;
                            }
                        }
                    }
                }
            }
            else
            {
                List<DeliveryOption> shippingAddresses = GetShippingAddresses(distributorID, locale);
                DeliveryOption shippingAddress = shippingAddresses.Find(s => s.Id == shippingAddressID);
                if (shippingAddress != null)
                {
                    List<DeliveryOption> deliveryOptions = GetDeliveryOptionsListForShipping("BR", "pt-BR", shippingAddress);
                    var varOptions = deliveryOptions.Where(s => s.State.Trim() == shippingAddress.Address.StateProvinceTerritory.Trim());
                    if (varOptions.Any())
                    {
                        var option = varOptions.First();
                        var shippingInfo = new ShippingInfo(option, shippingAddress)
                        {
                            Option = DeliveryOptionType.Shipping
                        };
                        return shippingInfo;
                    }
                }
            }
            if (sessionInfo != null && sessionInfo.ShoppingCart != null)
            {
                HLRulesManager.Manager.ProcessCart(sessionInfo.ShoppingCart, ShoppingCartRuleReason.CartRetrieved);
            }
            return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
        }

        public override string GetFreightVariant(ShippingInfo shippingInfo)
        {
            if (shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Pickup)
            {
                return shippingInfo.WarehouseCode;
            }
            return null;
        }

        public override List<Address_V01> AddressSearch(string SearchTerm)
        {
            var addrFound = new List<Address_V01>();
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new SearchAddressDataRequest_V01 { SearchText = SearchTerm };
                var response = proxy.GetAddressData(new GetAddressDataRequest(request)).GetAddressDataResult as SearchAddressDataResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    if (response.AddressData != null && response.AddressData.Count > 0)
                    {
                        AddressData_V02 addressData = response.AddressData.First();
                        addrFound.Add(new Address_V01
                        {
                            Country = "BR",
                            StateProvinceTerritory = addressData.State,
                            City = addressData.City,
                            Line1 = addressData.Street,
                            Line2 = addressData.Neighbourhood,
                            PostalCode = addressData.PostCode
                        }
                                   );
                    }
                    return addrFound;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("AddressSearch error: Country {0}, error: {1}", "BR", ex));
            }
            return addrFound;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.Pickup)
            {
                return GetDeliveryOptionsListForPickup(address != null && address.Address != null ? address.Address.StateProvinceTerritory : DEFAULT_PICKUP_STATE);
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                return GetDeliveryOptionsFromCache(address);
            }
            else
            {
                List<DeliveryOption> result = base.GetDeliveryOptions("pt-BR");
                if (result != null)
                {
                    if (type == DeliveryOptionType.ShipToCourier)
                    {
                        if (address != null)
                        {
                            return result.Where(r => r.Option == DeliveryOptionType.ShipToCourier && r.State.Equals(address.Address.StateProvinceTerritory)).ToList();
                        }
                    }
                    else
                    {
                        return result.Where(r => r.Option == type).ToList<DeliveryOption>();
                    }
                }
                return new List<DeliveryOption>();
            }
        }

        private List<DeliveryOption> GetDeliveryOptionsListForPickup(string state)
        {
            var result = base.GetDeliveryOptions("pt-BR");
            if (result != null)
            {
                result = result.Where(r => (r.State == state || (r.State == DEFAULT_PICKUP_STATE && r.WarehouseCode == DEFAULT_PICKUP_WAREHOUSE))
                    && r.Option == DeliveryOptionType.Pickup).ToList<DeliveryOption>();
            }
            return result;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            List<DeliveryOption> result = null;
            if (null != deliveryOptions && deliveryOptions.Count > 0)
            {
                string state = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "Москва";
                result = deliveryOptions.Where(d => d.State == state).ToList();
                if (result == null || result.Count == 0)
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

            return result.OrderBy(d => d.displayIndex.ToString(CultureInfo.InvariantCulture) + "_" + d.DisplayName).ToList();
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
            if (null != deliveryOptions)
            {
                deliveryOptions.AddRange(options);
            }
            else
            {

                HttpRuntime.Cache.Insert(CacheKey,
                options,
                null,
                DateTime.Now.AddMinutes(BR_SHIPPINGINFO_CACHE_MINUTES),
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                null);

            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        {
            var result = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01 { Country = Country };
            request.State = request.State = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "SP";
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                var currentOption = new DeliveryOption(option)
                {
                    Name = option.Description,
                    WarehouseCode = option.WarehouseCode,
                    State = request.State
                };
                result.Add(currentOption);
            }
            return result.OrderBy(d => d.displayIndex.ToString(CultureInfo.InvariantCulture) + "_" + d.DisplayName).ToList();
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            var shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Pickup && shippingInfo.Address != null)
            {
                return string.Format("{0} {1} {2}", shippingInfo.Address.Recipient, shippingInfo.Address.Phone, shippingInfo.RGNumber);
            }
            else if (shippingInfo != null)
            {
                return shoppingCart.DeliveryInfo.Instruction;
            }
            return string.Empty;
        }

        public override bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address)
        {

            if (address != null && !string.IsNullOrEmpty(address.Line4))
            {
                var streets = address.Line4.Split(new[] { "%%%" }, StringSplitOptions.RemoveEmptyEntries);
                if (streets.Length > 0)
                {
                    address.Line1 = streets[0];
                    if (!string.IsNullOrEmpty(address.Line3))
                    {
                        address.Line1 = address.Line1 + "," + address.Line3;
                        address.Line3 = string.Empty;
                    }
                    if (streets.Length > 1)
                    {
                        address.Line1 = address.Line1 + "," + streets[1];
                    }
                    address.Line4 = address.Line4.Replace("%%%", " "); 
                }
                else
                {
                    if (!string.IsNullOrEmpty(address.Line3))
                    {
                        address.Line1 = address.Line1 + "," + address.Line3;
                        address.Line3 = string.Empty;
                    }

                }
            }
            if (address != null && !string.IsNullOrEmpty(address.PostalCode) && address.PostalCode.Length == 8)
            {
                string postalCode = address.PostalCode;
                address.PostalCode = postalCode.Substring(0, 5) + "-" + postalCode.Substring(5);
            }
            return true;
        }

        public override bool ValidateAddress(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup || shoppingCart.DeliveryInfo.Option == DeliveryOptionType.ShipToCourier))
            {
                List<DeliveryOption> addresses = GetShippingAddresses(shoppingCart.DistributorID, shoppingCart.Locale);
                if (addresses == null || addresses.Count == 0)
                    return false;
            }
            return true;
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart != null)
            {
                if (shoppingCart.DeliveryInfo != null)
                {
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                    {
                        if (shoppingCart.ShippingAddressID != 0)
                        {
                            var shippingAddresses = GetShippingAddresses(shoppingCart.DistributorID, shoppingCart.Locale);
                            var deliveryOption = shippingAddresses.Find(s => s.ID == shoppingCart.ShippingAddressID);
                            if (deliveryOption != null)
                            {
                                address.Address = new ServiceProvider.OrderSvc.Address_V01
                                {
                                    Country = "BR",
                                    Line1 = this.getLine1(deliveryOption.Address),
                                    Line2 = deliveryOption.Address.Line2,
                                    Line3 = deliveryOption.Address.Line3,
                                    Line4 = deliveryOption.Address.Line4,
                                    StateProvinceTerritory = deliveryOption.Address.StateProvinceTerritory,
                                    PostalCode = deliveryOption.Address.PostalCode,
                                    CountyDistrict = deliveryOption.Address.CountyDistrict,
                                    City = deliveryOption.Address.City
                                };
                            }
                        }
                    }

                }
            }
        }

        private string getLine1(Address_V01 address)
        {
            string Line1 = string.Empty;
            if (address != null && !string.IsNullOrEmpty(address.Line4))
            {
                var streets = address.Line4.Split(new[] { "%%%" }, StringSplitOptions.RemoveEmptyEntries);
                if (streets.Length > 0)
                {
                    Line1 = streets[0];
                    if (!string.IsNullOrEmpty(address.Line3))
                    {
                        Line1 = string.Format("{0}, {1}", Line1, address.Line3);
                    }
                    if (streets.Length > 1)
                    {
                        Line1 = Line1 + ", " + streets[1];
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(address.Line3))
                    {
                        Line1 = address.Line1 + ", " + address.Line3;

                    }

                }
            }
            return Line1;

        }
        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {

            if (type == DeliveryOptionType.Shipping || type == DeliveryOptionType.ShipToCourier)
            {
                string Line1 = getLine1(address.Address);
                string formattedAddress = includeName ? string.Format("{0}<br>{1}, {2}<br>{3}, {4}, {5}<br>{6}", address.Recipient ?? string.Empty,
                    Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0}, {1}<br>{2}, {3}, {4}<br>{5}",
                    Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
                if (formattedAddress.IndexOf(",,", System.StringComparison.Ordinal) > -1 || formattedAddress.IndexOf(", ,", System.StringComparison.Ordinal) > -1)
                {
                    return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
                }
                else
                {
                    return formattedAddress;
                }
            }
            else
            {
                return base.FormatShippingAddress(address, type, description, includeName);
            }
        }

        #region Methods for Postal Office Implementation

        public override int? GetDeliveryEstimate(ShippingInfo shippingInfo, string locale)
        {
            if (shippingInfo == null || shippingInfo.Option == DeliveryOptionType.Pickup)
                return null;

            if (shippingInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                return base.GetDeliveryEstimate(shippingInfo, locale);
            }
            else
            {
                var temShippingInfo = new ShippingInfo();
                temShippingInfo.Address = temShippingInfo.CopyAddress(shippingInfo.Address);
                temShippingInfo.WarehouseCode = shippingInfo.WarehouseCode;
                temShippingInfo.FreightCode = shippingInfo.FreightCode.Substring(0, 1);
                return base.GetDeliveryEstimate(temShippingInfo, locale);
            }
        }

        /// <summary>
        /// Gets the pickup offices preferences for distributor. 
        /// </summary>
        /// <param name="distributorId">The distributor Id.</param>
        /// <param name="country">The country code.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="deliveryType">The delivery type option.</param>
        /// <returns>List of preferences for postal offices.</returns>
        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            // Verify the alias for the locations to generate a display name if needed
            foreach (var location in pickupLocations)
            {
                if (string.IsNullOrEmpty(location.PickupLocationNickname))
                {
                    var shippingInfo = this.GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                    var address = new ShippingAddress_V02(){ ID = shippingInfo.Address.ID, Recipient = shippingInfo.Description, FirstName = string.Empty, LastName = string.Empty,
                        MiddleName = string.Empty, Address = shippingInfo.Address.Address, Phone = string.Empty, AltPhone = string.Empty, IsPrimary = shippingInfo.Address.IsPrimary, Alias = shippingInfo.Address.Alias, Created = DateTime.Now};
                    location.PickupLocationNickname = this.GetAddressDisplayName(address);
                }
            }
            return pickupLocations;
        }

        /// <summary>
        /// Gets the list of postal officess states.
        /// </summary>
        /// <param name="Country">The country code.</param>
        /// <returns>List of state codes.</returns>
        public override List<string> GetStatesForCountry(string Country)
        {
            var addressV1 = new Address_V01 { Country = Country };
            var address = new ShippingAddress_V01 { Address = addressV1 };
            var deliveryOptions = GetDeliveryOptionsFromCache(address);

            var states = (from p in deliveryOptions
                          select p.Address.StateProvinceTerritory).Distinct().OrderBy(s => s).ToList();
            return states;
        }

        /// <summary>
        /// Gets the list of postal officess cities for a state.
        /// </summary>
        /// <param name="Country">The country code.</param>
        /// <param name="State">The state code.</param>
        /// <returns>The list of city names.</returns>
        public override List<string> GetCitiesForState(string Country, string State)
        {
            var addressV1 = new Address_V01 { Country = Country, StateProvinceTerritory = State };
            var address = new ShippingAddress_V01 { Address = addressV1 };
            var deliveryOptions = GetDeliveryOptionsFromCache(address);

            var cities = (from p in deliveryOptions
                          select p.Address.City).Distinct().OrderBy(s => s).ToList();
            return cities;
        }

        /// <summary>
        /// Gets the list of postal officess neighbourhoods for a city.
        /// </summary>
        /// <param name="country">The country code.</param>
        /// <param name="state">The state name.</param>
        /// <param name="city">The city name.</param>
        /// <returns>The neighbourhood names.</returns>
        public List<string> GetNeighbourhoodForCity(string country, string state, string city)
        {
            var addressV1 = new Address_V01 { Country = country, StateProvinceTerritory = state, City = city };
            var address = new ShippingAddress_V01 { Address = addressV1 };
            var deliveryOptions = GetDeliveryOptionsFromCache(address);

            var neighbourhoods = (from p in deliveryOptions
                                  select p.Address.Line3).Distinct().OrderBy(s => s).ToList();
            return neighbourhoods;
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(ShippingAddress_V01 address)
        {
            var deliveryOptions = HttpRuntime.Cache[CacheKeyForPostOffice] as List<DeliveryOption>;
            if (deliveryOptions == null)
            {
                deliveryOptions = GetDeliveryOptionsFromService(address);
                SavePostOfficeToCache(deliveryOptions);
            }

            if (address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
            {
                if (!string.IsNullOrEmpty(address.Address.City))
                {
                    if (!string.IsNullOrEmpty(address.Address.Line3))
                    {
                        deliveryOptions = deliveryOptions.Where(d =>
                            d.Address.StateProvinceTerritory == address.Address.StateProvinceTerritory &&
                            d.Address.City == address.Address.City && d.Address.Line3 == address.Address.Line3).ToList();
                    }
                    else
                    {
                        deliveryOptions = deliveryOptions.Where(d =>
                            d.Address.StateProvinceTerritory == address.Address.StateProvinceTerritory &&
                            d.Address.City == address.Address.City).ToList();
                    }
                }
                else
                {
                    deliveryOptions = deliveryOptions.Where(d => d.Address.StateProvinceTerritory == address.Address.StateProvinceTerritory).ToList();
                }
            }

            return deliveryOptions.OrderBy(d => d.Description).ToList();
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(ShippingAddress_V01 address)
        {
            var deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();

            var pickupAlternativesResponse = proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03() { CountryCode = address.Address.Country })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

            if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
            {
                deliveryOptions.AddRange(
                    from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                    select new DeliveryOption(po));
                Array.ForEach(deliveryOptions.ToArray(), a => a.Address = GetPostalOfficeAddress(a.Name, a.State));
            }

            return deliveryOptions;
        }

        private static void SavePostOfficeToCache(List<DeliveryOption> options)
        {
            if (options != null)
            {
                HttpRuntime.Cache.Insert(CacheKeyForPostOffice,
                  options,
                  null,
                  DateTime.Now.AddMinutes(BR_SHIPPINGINFO_CACHE_MINUTES),
                  Cache.NoSlidingExpiration,
                  CacheItemPriority.Normal,
                  null);
            }
        }


        private static Address_V01 GetPostalOfficeAddress(string storeAddress, string state)
        {
            string[] parts = storeAddress.Split('|');

            if (parts.Length == 5)
            {
                return new Address_V01
                {
                    Country = "BR",
                    StateProvinceTerritory = state,
                    Line1 = parts[0],
                    Line2 = parts[1],
                    Line3 = parts[2],
                    City = parts[3],
                    PostalCode = parts[4],
                };
            }

            return new Address_V01
            {
                Country = "BR",
                StateProvinceTerritory = state
            };
        }

        #endregion
    }
}