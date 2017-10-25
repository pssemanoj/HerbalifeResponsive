using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Caching;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_BO : ShippingProviderBase
    {
        private const string DeliveryInfoCacheKey = "DeliveryInfo_BO";

        public override DeliveryOption GetDefaultAddress()
        {
            Address_V01 address = new Address_V01();
            address.City = "Bolivia";
            return new DeliveryOption(new ShippingAddress_V02() { ID = 0, Recipient = string.Empty, FirstName = string.Empty, MiddleName = string.Empty, LastName = string.Empty, Address = address, Phone = "", AltPhone = "", IsPrimary = false, Alias = "", Created = DateTime.Now });
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            return currentShippingInfo == null ? String.Empty : currentShippingInfo.Instruction;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName ? string.Format(
                    "{0}<br>{1} {2}<br>{3},{4},{5}<br>{6},{7}", 
                    address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2,
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, 
                    address.Address.PostalCode, formatPhone(address.Phone)) : string.Format(
                    "{0} {1}<br>{2},{3},{4}<br>{5},{6}",
                    address.Address.Line1, address.Address.Line2,
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, 
                    address.Address.PostalCode, formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format(
                    "{0}<br>{1}<br>{2}<br>{3}, {4}, {5}<br>{6}", 
                    description,
                    address.Address.Line1,
                    address.Address.Line2,
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory, 
                    address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            ShippingInfo shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            if (shippingInfo != null && shippingInfo.Address != null && type == DeliveryOptionType.Shipping)
            {
                List<DeliveryOption> shippingOptions = GetDeliveryOptionsListForShipping("BO", "es-BO", shippingInfo.Address);
                if (shippingOptions != null)
                {
                    var shippingOption = shippingOptions.FirstOrDefault();
                    if (shippingOption != null && shippingInfo.FreightCode != shippingOption.FreightCode)
                    {
                        shippingInfo.FreightCode = shippingOption.FreightCode;
                        shippingInfo.WarehouseCode = shippingOption.WarehouseCode;

                        MyHLShoppingCart myShoppingCart = ShoppingCartProvider.GetShoppingCart(distributorID, locale);
                        if (myShoppingCart != null)
                        {
                            if (myShoppingCart.ShippingAddressID != shippingInfo.Address.ID || myShoppingCart.FreightCode != shippingOption.FreightCode)
                            {
                                myShoppingCart.DeliveryInfo = shippingInfo;
                                myShoppingCart.FreightCode = shippingOption.FreightCode;
                                myShoppingCart.ShippingAddressID = shippingInfo.Address.ID;
                                ShoppingCartProvider.UpdateShoppingCart(myShoppingCart);
                            }
                        }
                    }
                }
            }
            return shippingInfo;
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Shipment shippment)
        {
            var deliveryOptionID = shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping ? shoppingCart.DeliveryInfo.Address.ID : shoppingCart.DeliveryInfo.Id;
            ShippingInfo shippingInfo = GetShippingInfoFromID(shoppingCart.DistributorID, shoppingCart.Locale, shoppingCart.DeliveryInfo.Option, deliveryOptionID, deliveryOptionID);
            if (shippingInfo != null && (shoppingCart.DeliveryInfo.FreightCode != shippingInfo.FreightCode || shoppingCart.DeliveryInfo.WarehouseCode != shippingInfo.WarehouseCode))
            {
                shoppingCart.DeliveryInfo = shippingInfo;
                shoppingCart.FreightCode = shippingInfo.FreightCode;
                ShoppingCartProvider.UpdateShoppingCart(shoppingCart);
                shippment.WarehouseCode = shippingInfo.WarehouseCode;
                shippment.ShippingMethodID = shippingInfo.FreightCode;
            }
            return true;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[DeliveryInfoCacheKey] as List<DeliveryOption>;
            List<DeliveryOption> result = null;
            if (deliveryOptions != null && deliveryOptions.Count > 0)
            {
                string city = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.City)) ? address.Address.City : string.Empty;
                result = deliveryOptions.Where(d => d.State == city).ToList();
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

            return result.ToList();
        }

        private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        {
            List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[DeliveryInfoCacheKey] as List<DeliveryOption>;
            if (null != deliveryOptions)
            {
                deliveryOptions.AddRange(options);
            }
            else
            {
                HttpRuntime.Cache.Insert(DeliveryInfoCacheKey, options, null, DateTime.Now.AddMinutes(DELIVERY_TIME_ESTIMATED_CACHE_MINUTES),
                    Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
        }

        private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        {
            List<DeliveryOption> result = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            DeliveryOptionForCountryRequest_V01 request = new DeliveryOptionForCountryRequest_V01();
            request.Country = Country;
            request.State = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.City)) ? address.Address.City : string.Empty;
            request.Locale = locale;
            ShippingAlternativesResponse_V01 response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                DeliveryOption currentOption = new DeliveryOption(option);
                currentOption.State = request.State;
                currentOption.WarehouseCode = option.WarehouseCode;
                currentOption.FreightCode = option.FreightCode;
                result.Add(currentOption);
            }
            return result.ToList();
        }


        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                    string.IsNullOrWhiteSpace(a.CountyDistrict))
                // fails when State or County are empty (they're required)
                return false;
            
            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                !GetCountiesForCity(a.Country, a.StateProvinceTerritory, a.City).Contains(a.CountyDistrict))
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "BO",
                Locale = "es-BO",
                State = string.Format("{0} - {1} - {2}",
                    address.Address.StateProvinceTerritory,
                    address.Address.City,
                    address.Address.CountyDistrict)
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }

        /// <summary>
        ///     Get the freight and warehouse code for the address.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns></returns>
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] {
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
            };

            // call validation, if passes use service to validate when failed fallback to previous method (xml files)
            if (IsValidShippingAddress(address.Address))
                return GetFreightCodeAndWarehouseFromService(address);
            
            return freightCodeAndWarehouse;
        }

        #region GetAddressField method implementation for state/city region metadata

        public override List<string> GetStatesForCountry(string country)
        {
            return GetAddressField(new AddressFieldForCountryRequest_V01()
            {
                AddressField = AddressPart.STATE,
                Country = country
            });
        }

        public override List<string> GetCitiesForState(string country, string state)
        {
            return GetAddressField(new AddressFieldForCountryRequest_V01()
            {
                AddressField = AddressPart.CITY,
                Country = country,
                State = state
            });
        }

        public override List<string> GetCountiesForCity(string country, string state, string city)
        {
            if (string.IsNullOrEmpty(city) && 
                ! string.IsNullOrEmpty(country) && 
                ! string.IsNullOrEmpty(state) )
            {
                return GetAddressField(new AddressFieldForCountryRequest_V01()
                {
                    AddressField = AddressPart.COUNTY,
                    Country = country,
                    State = state
                });
            }
            else
                return base.GetCountiesForCity(country, state, city);
        }

        #endregion
    }
}