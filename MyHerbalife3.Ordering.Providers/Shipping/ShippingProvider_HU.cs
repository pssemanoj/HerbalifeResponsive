using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using HL.Common.Configuration;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

    public class ShippingProvider_HU : ShippingProviderBase
    {
        private List<string> selects = new List<string> { "Select", "Kiválaszt" };
        private string Dashes = "---------------";

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type,
                                                     string description, bool includeName)
        {
            if (type == DeliveryOptionType.Shipping)
            {
                if (address == null || address.Address == null)
                {
                    return string.Empty;
                }

                string formattedAddress = includeName ? string.Format("{0}<br/>{1} {2} {3}<br/>{4}, {5}<br/>{6} {7}<br/>{8}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line3 ?? string.Empty, address.Address.Line4, address.Address.CountyDistrict, address.Address.Line2,
                    address.Address.City, address.Address.PostalCode, formatPhone(address.Phone))
                    : string.Format("{0} {1} {2}<br/>{3}, {4}<br/>{5} {6}<br/>{7}",
                    address.Address.Line1, address.Address.Line3 ?? string.Empty, address.Address.Line4, address.Address.CountyDistrict, address.Address.Line2,
                    address.Address.City, address.Address.PostalCode, formatPhone(address.Phone));

                do { formattedAddress = formattedAddress.Replace("  ", " "); } while (formattedAddress.IndexOf("  ") > -1);
                do { formattedAddress = formattedAddress.Replace(", <br/>", "<br/>"); } while (formattedAddress.IndexOf(", <br/>") > -1);
                do { formattedAddress = formattedAddress.Replace("<br/><br/>", "<br/>"); } while (formattedAddress.IndexOf("<br/><br/>") > -1);
                return formattedAddress;
            }
            else if (type == DeliveryOptionType.Pickup)
            {
                string formattedAddress = string.Format("{0}<br/>{1} {2},<br/>{3} {4}", description,
                                                        address.Address.PostalCode, address.Address.City,
                                                        address.Address.Line1, address.Address.Line2 ?? string.Empty);
                return formattedAddress;
            }
            else
            {
                return base.FormatShippingAddress(address, type, description, includeName);
            }
        }

        public override bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            if (address != null)
            {
                address.Line1 = string.Format("{0} {1}", address.Line1, address.Line3).Trim();
                address.Line2 = string.Format("{0} {1}", address.CountyDistrict, address.Line2).Trim();
                address.Line2 = string.Format("{0} {1}", address.Line2, address.Line4).Trim();
                address.Line3 = address.Line4 = address.CountyDistrict = string.Empty;
            }
            return true;
        }

        public override List<string> GetStatesForCountry(string country)
        {
            const string CacheKey = "STATESFORCOUNTRY_HU";
            const int STATESFORCOUNTRY_HU_CACHE_MINUTES = 60 ;
            List<string> lsStatesForCountry = HttpRuntime.Cache[CacheKey] as List<string>;
            if (lsStatesForCountry == null)
            {
                lsStatesForCountry = base.GetStatesForCountry(country);
                if (lsStatesForCountry != null)
                    HttpRuntime.Cache.Insert(CacheKey, lsStatesForCountry, null, DateTime.Now.AddMinutes(STATESFORCOUNTRY_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return lsStatesForCountry;
        }

        public List<string> GetSuburbsForCity(string country, string city)
        {
            try
            {
                string CacheKey = string.Format("{0}{1}","SUBURBSFORCITY_HU_", city);
                const int SUBURBSFORCITY_HU_CACHE_MINUTES = 60;

                List<string> lsSuburbsForCity = HttpRuntime.Cache[CacheKey] as List<string>;

                if (lsSuburbsForCity != null)
                {
                    return lsSuburbsForCity;
                }
                // Using the city field from database to store the suburb/district
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new CitiesForStateRequest_V01 { Country = country, State = city };
                var response = proxy.GetCitiesForState(new GetCitiesForStateRequest(request)).GetCitiesForStateResult as CitiesForStateResponse_V01;

                var suburbs = new List<string>();
                if (response != null)
                {
                    suburbs = (from s in response.Cities
                               where !string.IsNullOrEmpty(s) && s.Contains("|") 
                               select s.Split('|')[0]).Distinct().ToList();
                    suburbs.Sort();
                    if (suburbs.Count == 1 && string.IsNullOrEmpty(suburbs[0]))
                        suburbs.Clear();
                    HttpRuntime.Cache.Insert(CacheKey, suburbs, null, DateTime.Now.AddMinutes(SUBURBSFORCITY_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                return suburbs;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetSuburbsForCity error: country: {0}, city: {1} error: {2}", country, city, ex));
            }
            return null;
        }

        public List<string> GetDistrictsForCity(string country, string city, string suburb)
        {
            try
            {
                if (valueContains(suburb)||suburb.Equals(Dashes))
                    suburb = string.Empty;

                string cacheKey = string.Format("{0}{1}_{2}", "DISTRICTSFORCITY_HU_", city, suburb);
                const int DISTRICTSFORCITY_HU_CACHE_MINUTES = 60;

                List<string> lsDistrictsForCity = HttpRuntime.Cache[cacheKey] as List<string>;
                if (lsDistrictsForCity!=null)
                {
                    return lsDistrictsForCity;
                }
                var districts = new List<string>();
                if (city.Equals("Budapest"))
                {
                    if (suburb == string.Empty)
                        districts = new List<string>()
                        {
                            "I.","II.","III.","IV.","V.","VI.","VII.","VIII.","IX.","X.", "XI.","XII.","XIII.","XIV.","XV.","XVI.","XVII.","XVIII.","XIX.","XX.","XXI.","XXII.","XXIII.",
                        };
                    if (suburb.Equals("Budafok") || suburb.Equals("Nagytétény"))
                        districts = new List<string>()
                        {
                            "XXII.",
                        };
                    if (suburb.Equals("Helikopter-lakópark"))
                        districts = new List<string>()
                        {
                            "XVII.",
                        };
                    if (suburb.Equals("Mátyásföld") || suburb.Equals("Sashalom"))
                        districts = new List<string>()
                        {
                            "XVI.",
                        };
                    if (suburb.Equals("Pestszentimre") || suburb.Equals("Pestszentlőrinc"))
                        districts = new List<string>()
                        {
                            "XVIII.",
                        };
                    return districts;
                }
                // Using the city field from database to store the suburb/district
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new CitiesForStateRequest_V01 { Country = country, State = city };
                var response = proxy.GetCitiesForState(new GetCitiesForStateRequest(request)).GetCitiesForStateResult as CitiesForStateResponse_V01;

                if (response != null)
                {
                    districts = (from s in response.Cities
                                 where !string.IsNullOrEmpty(s) && s.StartsWith(string.Format("{0}|", suburb)) && !string.IsNullOrEmpty(s.Split('|')[1])
                                 select s.Split('|')[1]).Distinct().ToList();
                    if (response.Cities.Contains(string.Empty))
                    {
                        districts.Add(string.Empty);
                        districts.Sort();
                    }
                    HttpRuntime.Cache.Insert(cacheKey, districts, null, DateTime.Now.AddMinutes(DISTRICTSFORCITY_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                return districts;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetDistrictsForCity error: country: {0}, city: {1} error: {2}", country, city, ex.ToString()));
            }
            return null;
        }

        public List<string> GetStreets(string country, string city, string suburb, string district)
        {
            try
            {
                if (valueContains(suburb) || suburb.Equals(Dashes))
                    suburb = string.Empty;
                if (valueContains(district))
                    district = string.Empty;

                string CacheKey = string.Format("{0}{1}_{2}_{3}", "STREET_HU_", city, suburb, district);
                const int STREET_HU_CACHE_MINUTES = 60 ;

                List<string> lsStreet = HttpRuntime.Cache[CacheKey] as List<string>;
                if (lsStreet != null)
                {
                    return lsStreet;
                }

                if (suburb == string.Empty && district == string.Empty)
                {
                    List<string> cites = new List<string> { "Debrecen", "Győr", "Miskolc", "Szeged", "Pécs" };
                    if (!cites.Contains(city))
                        return new List<string>();
                }

                // Using the street field from database to store the street name/street type
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new StreetsForCityRequest_V01
                {
                    Country = country,
                    State = city,
                    City =  string.Format("{0}|{1}", suburb, district)
                };
                var response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;

                var streets = new List<string>();
                if (response != null)
                {
                    CacheKey = string.Format("{0}{1}_{2}_{3}", "STREET_HU_", city, suburb, district);
                    streets = (from s in response.Streets
                               where !string.IsNullOrEmpty(s) && s.Contains("|") 
                               select s.Split('|')[0]).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();
                    if (response.Streets.Contains("^"))
                    {
                        streets.Add("^");
                        streets.Sort();
                    }
                    HttpRuntime.Cache.Insert(CacheKey, streets, null, DateTime.Now.AddMinutes(STREET_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                return streets;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreets error: country: {0}, city: {1} error: {2}", country, city, ex.ToString()));
            }
            return null;
        }

        public List<string> GetStreetType(string country, string city, string suburb, string district, string street)
        {
            try
            {
                // Using the street field from database to store the street name/street type
                if (valueContains(suburb) || suburb.Equals(Dashes))
                    suburb = string.Empty;
                if (valueContains(district))
                    district = string.Empty;

                string CacheKey = string.Format("{0}{1}_{2}_{3}_{4}", "STREETTYPE_HU_", city, suburb, district,street);
                const int STREETTYPE_HU_CACHE_MINUTES = 60;

                var types = HttpRuntime.Cache[CacheKey] as List<string>;
                if (types != null)
                {
                    return types;
                }

                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new StreetsForCityRequest_V01
                {
                    Country = country,
                    State = city,
                    City = string.Format("{0}|{1}", suburb, district)
                };
                var response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;

                if (response != null)
                {
                    types = (from s in response.Streets
                             where !string.IsNullOrEmpty(s) && s.StartsWith(string.Format("{0}|", street))
                             select s.Split('|')[1]).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();
                    if (response.Streets.Contains("^"))
                    {
                        types.Add("^");
                        types.Sort();
                    }
                    HttpRuntime.Cache.Insert(CacheKey, types, null, DateTime.Now.AddMinutes(STREETTYPE_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
                return types;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreetType error: country: {0}, city: {1} error: {2}", country, city, ex.ToString()));
            }
            return null;
        }


        public override List<string> GetZipsForStreet(string country, string city, string suburb, string street)
        {
            try
            {
                string CacheKey = string.Format("{0}{1}_{2}_{3}", "ZIPCODE_HU_", city, suburb, street);
                const int ZIPCODE_HU_CACHE_MINUTES = 60;

                var zips = HttpRuntime.Cache[CacheKey] as List<string>;
                if (zips != null)
                {
                    return zips;
                }


                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new ZipsForStreetRequest_V01();
                request.Country = country;
                request.State = city;
                request.City = suburb;
                request.Street = street;
                var response = proxy.GetZipsForStreet(new GetZipsForStreetRequest(request)).GetZipsForStreetResult as ZipsForStreetResponse_V01;
                if (response != null && response.Zips != null)
                {
                    HttpRuntime.Cache.Insert(CacheKey, response.Zips, null, DateTime.Now.AddMinutes(ZIPCODE_HU_CACHE_MINUTES), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }

                return response.Zips;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetZipsForStreet error: Country {0}, error: {1}", country, ex));
            }
            return null;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            string instruction = base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
            instruction = instruction ?? string.Empty;

            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (shoppingCart.InvoiceOption == null)
            {
                shoppingCart.InvoiceOption = string.Empty;
            }

            var invoiceOption = HttpContext.GetGlobalResourceObject("InvoiceOptions", shoppingCart.InvoiceOption.Trim(), CultureInfo.CurrentCulture);
            if (invoiceOption == null)
            {
                invoiceOption = shoppingCart.InvoiceOption.Trim();
            }
            if (shoppingCart.InvoiceOption == InvoiceHandlingType.SendToDistributor.ToString())
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    instruction = string.Format("{0},{1},{2},{3}", shoppingCart.DeliveryInfo.Address.Recipient,
                                                shoppingCart.DeliveryInfo.Address.Phone,
                                                shoppingCart.DeliveryInfo.PickupDate, invoiceOption);
                }
                else
                {
                    instruction = currentShippingInfo == null
                                      ? invoiceOption as string
                                      : string.Format("{0} {1}", currentShippingInfo.Instruction, invoiceOption);
                }
            }
            return instruction.Trim();
        }

        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }

        private bool valueContains(string value)
        {
            return selects.Any(s => value.Contains(s));
        }


        /// <summary>
        /// GetDistributorShippingInfoForHMS
        /// </summary>
        /// <param name="shoppingCart"></param>
        /// <param name="address"></param>
        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ShippingInfo_V01 address)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo !=null)
            {
                string freightCodeInCart = shoppingCart.DeliveryInfo.FreightCode;
                var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (session.IsEventTicketMode)
                {
                    shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                }
                else if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                }
                else
                {
                    string defaultFreight = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = defaultFreight;
                    }
                    else
                    {
                        shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = "PU";
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
        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null)
            {
                string freightCodeInCart = shoppingCart.DeliveryInfo.FreightCode;
                var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                if (session.IsEventTicketMode)
                {
                    shippment.ShippingMethodID = shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                }
                else if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    shippment.ShippingMethodID = shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                }
                else 
                {
                    string defaultFreight = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                    shippment.WarehouseCode = shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        shippment.ShippingMethodID = shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = defaultFreight;
                    }
                    else
                    {
                        shippment.ShippingMethodID = shoppingCart.FreightCode = shoppingCart.DeliveryInfo.FreightCode = "PU";
                    }
                }
                if (!freightCodeInCart.Equals(shoppingCart.FreightCode))
                {
                    shoppingCart.Calculate();
                    ShoppingCartProvider.UpdateShoppingCart(shoppingCart);
                }
            }
            return true;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            return freightCodeAndWarehouse;
        }
    }
}
