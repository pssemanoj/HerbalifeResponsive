namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;
    using HL.Common.Configuration;
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using InvoiceHandlingType = MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc.InvoiceHandlingType;

    /// <summary>
    /// Shipping provider for UA
    /// </summary>
    public class ShippingProvider_UA : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryInfo_UA";
        private const string PUCourierCacheKey = "DeliveryCourierInfo_UA";
        private const int UA_SHIPPINGINFO_CACHE_MINUTES = 60;

        #region Public Methods and Operators

        /// <summary>
        /// Format the address object to send to HMS
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns></returns>
        public override bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            if (address != null)
            {
                address.Line1 = string.Format(
                    "{0} {1} {2} {3}", address.Line1, address.Line2, address.Line3, address.Line4);
                while (address.Line1.Contains("  "))
                {
                    address.Line1 = address.Line1.Replace("  ", " ");
                }
                address.Line2 = address.CountyDistrict;
                address.Line3 = address.Line4 = address.CountyDistrict = null;
            }
            return true;
        }

        /// <summary>
        /// Format the address to display it in site.
        /// </summary>
        /// <param name="address">Address to format.</param>
        /// <param name="type">Delivery type.</param>
        /// <param name="description">Description.</param>
        /// <param name="includeName">Flag to include the name.</param>
        /// <returns></returns>
        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format(
                                           "{0}<br>{1} {2}, {7}, {8}<br>{3}, {4}, {9}<br>{5}<br>{6}",
                                           address.Recipient ?? string.Empty,
                                           address.Address.Line1,
                                           address.Address.Line2 ?? string.Empty,
                                           address.Address.City,
                                           address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode,
                                           this.formatPhone(address.Phone),
                                            address.Address.Line3 ?? string.Empty, address.Address.Line4 ?? string.Empty,
                                            address.Address.CountyDistrict)
                                       : string.Format(
                                           "{0}<br>{1} {2}, {7}, {8}<br>{3}, {4}<br>{5}<br>{6}",
                                           address.Address.Line1,
                                           address.Address.Line2 ?? string.Empty,
                                           address.Address.City,
                                           address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode,
                                           this.formatPhone(address.Phone),
                                            address.Address.Line3 ?? string.Empty, address.Address.Line4 ?? string.Empty,
                                            address.Address.CountyDistrict);
            }
            else if (type == DeliveryOptionType.PickupFromCourier)
            {
                return string.Format("{0}<br>{1},{2}<br>{3} {4}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty,
                    address.Address.City, address.Address.PostalCode);
            }
            else
            {
                formattedAddress = string.Format(
                    "{0}<br/>{1} {2}<br/>{3} {4}<br/>{5}",
                    description,
                    address.Address.Line1,
                    address.Address.Line2 ?? string.Empty,
                    address.Address.PostalCode,
                    address.Address.City,
                    address.Address.StateProvinceTerritory);
            }
            return formattedAddress;
        }

        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Get the shipping instructions.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="distributorID">The distributor Id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            string instruction = shoppingCart.DeliveryInfo == null || string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                         ? string.Empty
                         : shoppingCart.DeliveryInfo.Instruction;

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                shoppingCart.DeliveryInfo.PickupDate.HasValue)
            {
                instruction = string.Format("{0}", shoppingCart.DeliveryInfo.PickupDate.Value.ToString("d", CultureInfo.CurrentCulture));
            }

            if (!string.IsNullOrEmpty(shoppingCart.InvoiceOption) && shoppingCart.InvoiceOption == InvoiceHandlingType.SendToDistributor.ToString())
            {
                const string invoiceOption = "Док-ти надіслати на поштову адресу Незалежного Партнера";
                instruction = string.Format("{0}{1}", string.IsNullOrEmpty(instruction) ? instruction : string.Format("{0} ", instruction), invoiceOption);
            }

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
            {
                if (!string.IsNullOrEmpty(shoppingCart.InvoiceOption) && shoppingCart.InvoiceOption == InvoiceHandlingType.SendToDistributor.ToString())
                {
                    instruction = "Доставка до Відділення МІСТ, Док.поштою НП";
                }
                else
                {
                    instruction = "Доставка до Відділення МІСТ";
                }
            }
            return instruction.Trim();
        }

        //agregar estos metodos al base y hacer override ....
        /// <summary>
        /// Gets the streets.
        /// </summary>
        /// <param name="country">The country.</param>
        /// <param name="state">The state.</param>
        /// <param name="district">The district.</param>
        /// <returns></returns>
        public override List<string> GetStreets(string country, string state, string district)
        {
            try
            {
                string CacheKey = string.Format("{0}{1}_{2}", "STREET_UA_", state, district);
                const int STREET_HU_CACHE_MINUTES = 60;

                List<string> lsStreet = HttpRuntime.Cache[CacheKey] as List<string>;
                if (lsStreet != null)
                {
                    return lsStreet;
                }

                var proxy = ServiceClientProvider.GetShippingServiceProxy();

                var streets = new List<string>();
                //check if region2 is not selected, list all the cities
                if (null == district || string.IsNullOrEmpty(district))
                {
                    var provider = ShippingProvider.GetShippingProvider(country);
                    // city is district(region) for Ukraine
                    var lookupResults = provider.GetCitiesForState(country, state);

                    foreach (var lookupResult in lookupResults)
                    {
                        var request = new StreetsForCityRequest_V01
                        {
                            Country = country,
                            State = state,
                            City = string.IsNullOrEmpty(lookupResult) ? " " :
                                lookupResult
                        };
                        //Aqui ciclar para recorrer todas las cities apra obtener todos los streets del state
                        var response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;
                        if (response != null)
                        {
                            streets.AddRange(from s in response.Streets
                                             where !string.IsNullOrEmpty(s)
                                             select s);
                            streets.Distinct();

                        }
                    }
                }
                else
                {
                    var request = new StreetsForCityRequest_V01
                    {
                        Country = country,
                        State = state,
                        City = string.IsNullOrEmpty(district) ? " " : district
                    };
                    var response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;
                    if (response != null)
                    {
                        streets = (from s in response.Streets
                                   where !string.IsNullOrEmpty(s)
                                   select s).ToList();
                    }
                }
                HttpRuntime.Cache.Insert(CacheKey, streets, null, DateTime.Now.AddMinutes(STREET_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                return streets;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("UA GetStreets error: country: {0}, city: {1} error: {2}", country, state, ex.ToString()));
            }
            return null;
        }


        /// <summary>
        /// Gets the zip codes.
        /// </summary>
        /// <param name="country">The country.</param>
        /// <param name="state">The state.</param>
        /// <param name="district">The district.</param>
        /// <param name="city">The city.</param>
        /// <returns></returns>
        public override List<string> GetZipCodes(string country, string state, string district, string city)
        {
            try
            {
                string CacheKey = string.Format("{0}{1}_{2}_{3}", "ZIPS_UA_", state, district, city);
                const int STREET_HU_CACHE_MINUTES = 60;

                List<string> lsZips = HttpRuntime.Cache[CacheKey] as List<string>;
                if (lsZips != null)
                {
                    return lsZips;
                }

                var proxy = ServiceClientProvider.GetShippingServiceProxy();

                var zips = new List<string>();
                //check if region2 is not selected, list all the zips
                if (null == district || string.IsNullOrEmpty(district))
                {
                    var provider = ShippingProvider.GetShippingProvider(country);
                    var zip = provider.GetZipsForCity(country, state, city);
                    zips.AddRange(zip);
                    zips.Distinct();
                }
                else
                {
                    var provider = ShippingProvider.GetShippingProvider(country);
                    if (provider != null)
                    {
                        zips = provider.GetZipsForStreet(country, state, city, district);
                    }
                }
                HttpRuntime.Cache.Insert(CacheKey, zips, null, DateTime.Now.AddMinutes(STREET_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                return zips;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreets error: country: {0}, city: {1} error: {2}", country, state, ex.ToString()));
            }
            return null;
        }

        /// <summary>
        /// called before pricing
        /// </summary>
        /// <param name="shoppingCart"></param>
        /// <param name="address"></param>
        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null)
            {
                string freightCodeInCart = shoppingCart.DeliveryInfo.FreightCode;
                var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (session.IsEventTicketMode || APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = "NOF";
                }
                else
                {
                    if (shoppingCart.FreightCode == "NOF")
                    {
                        shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping ? "UAF" : "PU";
                    }
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                    }
                }
                if (!freightCodeInCart.Equals(shoppingCart.FreightCode))
                {
                    ShoppingCartProvider.UpdateShoppingCart(shoppingCart);
                }
            }
        }

        /// <summary>
        /// Gets the shipment information to import into HMS.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="shippment">The order shipment.</param>
        /// <returns></returns>
        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Shipment shippment)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null)
            {
                string freightCodeInCart = shoppingCart.DeliveryInfo.FreightCode;
                var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (session.IsEventTicketMode || APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    return true;
                }

                if (shoppingCart.FreightCode == "NOF")
                {
                    shippment.ShippingMethodID = shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping ? "UAF" : "PU";
                }
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                }
                if (!freightCodeInCart.Equals(shoppingCart.FreightCode))
                {
                    shoppingCart.Calculate();
                    ShoppingCartProvider.UpdateShoppingCart(shoppingCart);
                }
            }
            return true;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                if (address == null || address.Address == null || string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                    return GetDeliveryOptionsFromCache("UA", "uk-UA", address, type);
                else
                    return GetDeliveryOptionsByStateFromCache("UA", "uk-UA", address, type);
            }
            else
            {
                return base.GetDeliveryOptions(type, address);
            }
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country, string locale, DeliveryOptionType deliveryType)
        {
            var pickupLocations = base.GetPickupLocationsPreferences(distributorId, country);
            List<PickupLocationPreference_V01> pickupLocationPreferencestoRemove = new List<PickupLocationPreference_V01>();
            // Verify the alias for the locations to generate a display name if needed
            foreach (var location in pickupLocations)
            {
                var shippingInfo = GetShippingInfoFromID(distributorId, locale, deliveryType, location.ID, 0);
                if (shippingInfo != null)
                {
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
                        List<DeliveryOption> doList = GetDeliveryOptions(type, new ShippingAddress_V01 { Address = new Address_V01 { Country = "UA" } });
                        if (doList != null)
                        {
                            deliveryOption = doList.Find(d => d.Id == PickupLocationID);
                            if (deliveryOption != null)
                            {
                                if (deliveryOption.Address != null && !string.IsNullOrEmpty(deliveryOption.Address.StateProvinceTerritory))
                                {
                                    List<DeliveryOption> doListDetails = GetDeliveryOptions(type, new ShippingAddress_V01 { Address = deliveryOption.Address });
                                    deliveryOption = doListDetails.Find(d => d.Id == PickupLocationID);
                                }
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
        #endregion

        #region Private Methods

        private static List<DeliveryOption> GetDeliveryOptionsFromCache(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[PUCourierCacheKey] as List<DeliveryOption>;
                List<DeliveryOption> result = null;
                if (null != deliveryOptions && deliveryOptions.Count > 0)
                {
                    result = deliveryOptions;
                }
                else
                {
                    result = GetDeliveryOptionsFromService(Country, locale, address, type);
                    SaveDeliveryOptionsToCache(result, type);
                }

                return result;
            }

            return null;
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            DeliveryPickupAlternativesResponse_V03 pickupAlternativesResponse = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> result = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V03
                {
                    CountryCode = Country,
                    State = string.Empty
                })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V03;

                if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                {
                    result.AddRange(
                        from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                        orderby po.ID
                        select new DeliveryOption(po, true));
                }
                return result;
            }
            return null;
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options, DeliveryOptionType type, string cacheKey = PUCourierCacheKey)
        {
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;
                if (null != deliveryOptions)
                {
                    deliveryOptions.AddRange(options);
                }
                else
                {
                    HttpRuntime.Cache.Insert(cacheKey,
                                             options,
                                             null,
                                             DateTime.Now.AddMinutes(UA_SHIPPINGINFO_CACHE_MINUTES),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);
                }
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsByStateFromCache(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            if (address == null || address.Address == null || string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                return null;

            if (type == DeliveryOptionType.PickupFromCourier)
            {
                string PUCourierByStateCacheKey = string.Format("{0}_{1}", PUCourierCacheKey, address.Address.StateProvinceTerritory);
                List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[PUCourierByStateCacheKey] as List<DeliveryOption>;
                List<DeliveryOption> result = null;
                if (null != deliveryOptions && deliveryOptions.Count > 0)
                {
                    result = deliveryOptions;
                }
                else
                {
                    result = GetDeliveryOptionsByStateFromService(Country, locale, address, type);
                    SaveDeliveryOptionsToCache(result, type, PUCourierByStateCacheKey);
                }
                return result;
            }
            return null;
        }

        private static List<DeliveryOption> GetDeliveryOptionsByStateFromService(string Country, string locale, ShippingAddress_V01 address, DeliveryOptionType type)
        {
            if (address == null || address.Address == null || string.IsNullOrEmpty(address.Address.StateProvinceTerritory))
                return null;

            DeliveryPickupAlternativesResponse_V05 pickupAlternativesResponse = null;
            if (type == DeliveryOptionType.PickupFromCourier)
            {
                List<DeliveryOption> result = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                pickupAlternativesResponse =
                proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V05
                {
                    CountryCode = Country,
                    State = address.Address.StateProvinceTerritory
                })).GetDeliveryPickupAlternativesResult as DeliveryPickupAlternativesResponse_V05;

                if (pickupAlternativesResponse != null && pickupAlternativesResponse.DeliveryPickupAlternatives != null)
                {
                    pickupAlternativesResponse.DeliveryPickupAlternatives.ForEach(d => { d.Description = d.CourierAddress; d.CourierName = d.ID.ToString(); });

                    result.AddRange(
                        from po in pickupAlternativesResponse.DeliveryPickupAlternatives
                        orderby po.ID
                        select new DeliveryOption(po as DeliveryPickupOption_V03, true));
                    Array.ForEach(result.ToArray(), a => { a.Address = getAddress(a.Name, a.State, a.PostalCode); a.Address.CountyDistrict = string.Format("відділення МІСТ № {0}", a.Id.ToString()); });
                }
                return result;
            }
            return null;
        }

        private static Address_V01 getAddress(string storeAddress, string state, string postalCode)
        {
            string[] parts = storeAddress.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string city = string.Empty;
            string street = string.Empty;

            if (parts.Length == 1)
            {
                street = parts[0];
            }
            else if (parts.Length == 3)
            {
                city = parts[0].Trim();
                street = string.Format("{0},{1}", parts[1], parts[2]);
            }

            return new Address_V01
            {
                Country = "UA",
                StateProvinceTerritory = state,
                Line1 = street,
                City = city,
                PostalCode = postalCode,
            };
        }

        #endregion
    }
}