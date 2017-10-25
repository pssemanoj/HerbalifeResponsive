using System;
using System.Linq;
using System.ServiceModel;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System.Web.SessionState;
using System.Web;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_AR : ShippingProviderBase
    {
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
            string stateName = GetStateNameFromStateCode(address.Address.StateProvinceTerritory);
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1}<br>{2}{3}, {4}<br>{5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, 
                                                       string.IsNullOrEmpty(address.Address.CountyDistrict) ? string.Empty : string.Format("{0}, ", address.Address.CountyDistrict), address.Address.City, stateName,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0}<br>{1}{2}, {3}<br>{4}<br>{5}",
                                                       address.Address.Line1,
                                                       string.IsNullOrEmpty(address.Address.CountyDistrict) ? string.Empty : string.Format("{0}, ", address.Address.CountyDistrict), address.Address.City, stateName,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}<br>{2}{3}, {4}<br>{5}", 
                                                 description,
                                                 address.Address.Line1,
                                                 string.IsNullOrEmpty(address.Address.CountyDistrict) ? string.Empty : string.Format("{0}, ", address.Address.CountyDistrict), address.Address.City, stateName, 
                                                 address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override string GetStateNameFromStateCode(string stateCode)
        {
            int code = 0;
            if (int.TryParse(stateCode, out code) && code > 0)
            {
                string stateName = string.Empty;
                stateName = (from s in GetStatesForCountry("AR")
                             where s.Contains(string.Format("-{0}", stateCode))
                             select s).FirstOrDefault();
                if (string.IsNullOrEmpty(stateName))
                    stateName = "-";
                return stateName.Substring(0, stateName.IndexOf("-"));
            }
            else
            {
                return stateCode;
            }
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
                Country = "AR",
                Locale = "es-AR",
                State = string.Format("{0}-{1}", address.Address.StateProvinceTerritory, address.Address.PostalCode)
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01 address)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();

                if (type == DeliveryOptionType.Pickup)
                {
                    var pickupAlternativesResponse = new ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01();
                    if (address == null || address.Address == null)
                    {
                        // Get Herbalife Centers - FreightCode: PU
                        pickupAlternativesResponse =
                            proxy.GetPickupAlternativesByFreightCode(new ServiceProvider.ShippingMexicoSvc.GetPickupAlternativesByFreightCodeRequest(new ServiceProvider.ShippingMexicoSvc.PickupAlternativesByFreightCodeRequest_V01()
                            {
                               Country = "AR",
                               FreightCode = "PU"
                            })).GetPickupAlternativesByFreightCodeResult as ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01;
                    }
                    else if (!string.IsNullOrEmpty(address.Address.PostalCode))
                    {
                        pickupAlternativesResponse =
                            proxy.GetPickupAlternativesForPostalCode(new ServiceProvider.ShippingMexicoSvc.GetPickupAlternativesForPostalCodeRequest(new ServiceProvider.ShippingMexicoSvc.PickupAlternativesForPostalCodeRequest_V01()
                            {
                                Country = "AR",
                                PostalCode = address.Address.PostalCode
                            })).GetPickupAlternativesForPostalCodeResult as ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01;
                    }
                    else
                    {
                        pickupAlternativesResponse =
                            proxy.GetPickupAlternativesForColony(new ServiceProvider.ShippingMexicoSvc.GetPickupAlternativesForColonyRequest(new ServiceProvider.ShippingMexicoSvc.PickupAlternativesForColonyRequest_V01()
                            {
                                Country = "AR",
                                Colony = address.Address.Line3,
                                Municipality = address.Address.City,
                                State = address.Address.StateProvinceTerritory
                            })).GetPickupAlternativesForColonyResult as ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01;
                    }
                    if (pickupAlternativesResponse != null && pickupAlternativesResponse.PickupAlternatives != null)
                    {
                        deliveryOptions.AddRange(
                            from po in pickupAlternativesResponse.PickupAlternatives
                            select new DeliveryOption(ObjectMappingHelper.Instance.GetToShipping(po)));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(address.Address.PostalCode))
                    {
                        var shippingAlternativesResponse =
                            proxy.GetShippingAlternativesForPostalCode(new ServiceProvider.ShippingMexicoSvc.GetShippingAlternativesForPostalCodeRequest(new ServiceProvider.ShippingMexicoSvc.ShippingAlternativesForPostalCodeRequest_V01()
                            {
                                Country = "AR",
                                PostalCode = address.Address.PostalCode
                            })).GetShippingAlternativesForPostalCodeResult as MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingAlternativesResponse_V01;
                        if (shippingAlternativesResponse != null &&
                            shippingAlternativesResponse.DeliveryAlternatives != null)
                        {
                            deliveryOptions.AddRange(from so in shippingAlternativesResponse.DeliveryAlternatives
                                                     where !so.ShippingSource.Status.ToUpper().Equals("NOGDO") && !so.ShippingSource.Status.ToUpper().Equals("NODISPONIBLE")
                                                     select new DeliveryOption(ObjectMappingHelper.Instance.GetToShipping(so)));
                        }
                    }
                }
                deliveryOptions.ForEach(o => o.Address = validateAdressValues(o.Address));

                return deliveryOptions;
            }
            else
                return base.GetDeliveryOptions(type, address);
        }

        private Address_V01 validateAdressValues(Address_V01 address)
        {
            if(address != null)
            {
                address.Line1 = address.Line1 ?? string.Empty;
                address.Line2 = address.Line2 ?? string.Empty;
                address.Line3 = address.Line3 ?? string.Empty;
                address.Line4 = address.Line4 ?? string.Empty;
                address.City = address.City ?? string.Empty;
                address.StateProvinceTerritory = address.StateProvinceTerritory ?? string.Empty;
            }
            return address;
        }

        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                var pickupLocationPreferences = getPickupLocationsPreferencesFromCache(distributorId, country);
                var pickupLocationPreferencestoRemove = new List<PickupLocationPreference_V01>();
                if (pickupLocationPreferences != null && pickupLocationPreferences.Count > 0)
                {
                    var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
                    foreach (PickupLocationPreference_V01 pref in pickupLocationPreferences)
                    {
                        // Checking if the location saved is in db
                        var request = new ServiceProvider.ShippingMexicoSvc.DeliveryOptionForIDRequest_V01() { ID = pref.PickupLocationID, Country = "AR" };
                        var pickupAlternativesResponse =
                            proxy.GetPickupAlternativeForDeliveryOptionID(new ServiceProvider.ShippingMexicoSvc.GetPickupAlternativeForDeliveryOptionIDRequest(request)).GetPickupAlternativeForDeliveryOptionIDResult as ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01;
                        if (pickupAlternativesResponse != null &&
                            pickupAlternativesResponse.PickupAlternatives != null &&
                            pickupAlternativesResponse.PickupAlternatives.Count > 0)
                        {
                            if (string.IsNullOrEmpty(pref.PickupLocationNickname))
                            {
                                pref.PickupLocationNickname =
                                    pickupAlternativesResponse.PickupAlternatives[0].BranchName;
                            }
                        }
                        else
                        {
                            pickupLocationPreferencestoRemove.Add(pref);
                        }
                    }
                    // Removing the not existing locations in db
                    if (pickupLocationPreferencestoRemove.Count > 0)
                    {
                        foreach (var item in pickupLocationPreferencestoRemove)
                        {
                            DeletePickupLocationsPreferences(item.DistributorID, item.PickupLocationID, item.Country);
                        }
                    }
                }
                return pickupLocationPreferences;
            }
            else
                return base.GetPickupLocationsPreferences(distributorId, country);
        }

        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
                var request = new ServiceProvider.ShippingMexicoSvc.DeliveryOptionForIDRequest_V01();
                request.ID = deliveryOptionID;
                request.Country = "AR";

                DeliveryOption deliveryOption = null;
                if (type == DeliveryOptionType.Pickup)
                {
                    string countryCode = locale.Substring(3, 2);
                    List<PickupLocationPreference_V01> pickupLocationPreference = GetPickupLocationsPreferences(distributorID, countryCode);
                    if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                    {
                        var vPickupLocation = pickupLocationPreference.Find(p => p.PickupLocationID == deliveryOptionID);
                        if (vPickupLocation != null)
                        {
                            int PickupLocationID = vPickupLocation.PickupLocationID;
                            request.ID = PickupLocationID;
                            var pickupAlternativesResponse =
                                proxy.GetPickupAlternativeForDeliveryOptionID(new ServiceProvider.ShippingMexicoSvc.GetPickupAlternativeForDeliveryOptionIDRequest(request)).GetPickupAlternativeForDeliveryOptionIDResult as ServiceProvider.ShippingMexicoSvc.PickupAlternativesResponse_V01;
                            if (pickupAlternativesResponse != null && pickupAlternativesResponse.PickupAlternatives != null && pickupAlternativesResponse.PickupAlternatives.Count > 0)
                            {
                                deliveryOption = new DeliveryOption(ObjectMappingHelper.Instance.GetToShipping(pickupAlternativesResponse.PickupAlternatives[0]));
                                deliveryOption.Id = deliveryOption.ID = PickupLocationID;
                                deliveryOption.Description = deliveryOption.Description;
                                var shippingInfo = new ShippingInfo(deliveryOption);
                                shippingInfo.Address.Address.Line1 = shippingInfo.Address.Address.Line2;
                                shippingInfo.Address.Address.Line2 = string.Empty;
                                return shippingInfo;
                            }
                        }
                    }
                }
                else
                {
                    ShippingAddress_V02 shippingAddress = null;
                    if (shippingAddressID != 0)
                    {
                        List<DeliveryOption> shippingAddresses = GetShippingAddresses(distributorID, locale);
                        if (shippingAddresses != null)
                        {
                            if ((shippingAddress = shippingAddresses.Find(s => s.ID == shippingAddressID)) == null)
                            {
                                shippingAddress = shippingAddresses.Find(s => s.IsPrimary == true);
                            }
                        }
                    }
                    else
                    {
                        List<DeliveryOption> addresses = base.GetShippingAddresses(distributorID, locale);
                        if (addresses != null && addresses.Count > 0)
                        {
                            if ((shippingAddress = addresses.Find(s => s.IsPrimary == true)) == null)
                                shippingAddress = addresses.First();
                        }
                    }

                    if (shippingAddress != null)
                    {
                        List<DeliveryOption> deliveryOptions = GetDeliveryOptions(type, shippingAddress);
                        if (deliveryOptions != null && deliveryOptions.Count > 0)
                        {
                            deliveryOption = deliveryOptions.First();
                            return new ShippingInfo(deliveryOption, shippingAddress);
                        }
                    }
                }

                return null;
            }
            else
                return GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
        }

        public override bool NeedEnterAddress(string distributorID, string locale)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                List<PickupLocationPreference_V01> pickupRrefList =
                GetPickupLocationsPreferences(distributorID, locale.Substring(3, 2));
                List<DeliveryOption> shippingAddresses = GetShippingAddresses(distributorID, locale);
                return ((pickupRrefList == null || pickupRrefList.Count == 0) &&
                        (shippingAddresses == null || shippingAddresses.Count == 0));
            }
            return base.NeedEnterAddress(distributorID, locale);
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                       string locale,
                                                                       ShippingAddress_V01 address)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
                var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
                if (!string.IsNullOrEmpty(address.Address.PostalCode))
                {
                    var shippingAlternativesResponse =
                        proxy.GetShippingAlternativesForPostalCode(new ServiceProvider.ShippingMexicoSvc.GetShippingAlternativesForPostalCodeRequest(new ServiceProvider.ShippingMexicoSvc.ShippingAlternativesForPostalCodeRequest_V01()
                        {
                            Country = "AR",
                            PostalCode = address.Address.PostalCode
                        })).GetShippingAlternativesForPostalCodeResult as MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingAlternativesResponse_V01;
                    if (shippingAlternativesResponse != null && shippingAlternativesResponse.DeliveryAlternatives != null)
                    {
                        deliveryOptions.AddRange(from so in shippingAlternativesResponse.DeliveryAlternatives
                                                 where !so.ShippingSource.Status.ToUpper().Equals("NOGDO") && !so.ShippingSource.Status.ToUpper().Equals("NODISPONIBLE")
                                                 select new DeliveryOption(ObjectMappingHelper.Instance.GetToShipping(so)));
                    }
                }
                return deliveryOptions;
            }
            else
                return base.GetDeliveryOptionsListForShipping(Country, locale, address);
        }

        public override int? GetDeliveryEstimate(ShippingInfo shippingInfo, string locale)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.UseXHLTables)
            {
                if (shippingInfo == null)
                    return null;

                if (shippingInfo.Option == DeliveryOptionType.Shipping)
                {
                    var list = GetDeliveryOptionsListForShipping("AR", locale, shippingInfo.Address);
                    var opt = list.Where(x => x.Name == shippingInfo.Name && x.Id == shippingInfo.Id).Select(x => x.ShippingIntervalDays).FirstOrDefault();
                    return opt;
                }

                return null;
            }
            else
                return GetDeliveryEstimate(shippingInfo, locale);
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            // Retrieve the state code instead state name
            if (shoppingCart.DeliveryInfo != null)
            {
                int stateCode = 0;
                if (!int.TryParse(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory, out stateCode))
                {
                    var stateValue = string.Format("{0}-", shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory.ToUpper());
                    var state = GetStatesForCountry("AR").Where(s => s.ToUpper().Contains(stateValue)).FirstOrDefault();
                    if (state != null)
                    {
                        var values = state.Split('-');
                        shippment.Address.StateProvinceTerritory = values[1];
                    }
                }
            }
            return true;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            AddressFieldForCountryRequest_V01 request = new AddressFieldForCountryRequest_V01()
                                  {
                                      AddressField = AddressPart.ZIPCODE,
                                      Country = a.Country,
                                      State = GetStateNameFromStateCode(a.StateProvinceTerritory),
                                      City = a.City
                                  };
            List<string> lookupResults= GetAddressField(request);
            if ((!string.IsNullOrWhiteSpace(a.PostalCode)) && (GetStatesForCountry(a.Country).Contains(GetStateNameFromStateCode(a.StateProvinceTerritory) + "-" + a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, GetStateNameFromStateCode(a.StateProvinceTerritory)).Contains(a.City))
                && (lookupResults.Contains(a.PostalCode)))
                return true;
            else
                return false;
        }
    }
}