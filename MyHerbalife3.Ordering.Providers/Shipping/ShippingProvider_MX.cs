using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_MX : ShippingProviderBase
    {
        #region Constants

        public const string MX_STATE_CACHE_PREFIX = "MxState";
        private const int Honors2016EventId = 2462;
        //  public static int PAYMENTINFO_CACHE_MINUTES = HL.Common.Configuration.Settings.GetRequiredAppSetting<int>("PaymentCacheExpireMinutes");

        /// <summary>
        ///     Cache duration for CardTypes
        /// </summary>
        public const int CardTypesCacheMinutes = 1440;

        #endregion

        #region private methods 

        public void RemoveUnauthorizedDeliveryOptions(List<DeliveryOption> devliveryOptions, string locale)
        {
            var isAuthorized = DistributorOrderingProfileProvider.IsEventQualified(Honors2016EventId, locale);
            if (devliveryOptions.Any(o => o.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
                && !isAuthorized)
            {
                devliveryOptions.RemoveAll(o => o.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse);
            }
        }

        #endregion

        #region IShippingProvider Members

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01 address)
        {
            var locale = Thread.CurrentThread.CurrentCulture.Name;
            if (locale.Equals("en-MX"))
            {
                var mexicoDeliveryOptions =  base.GetDeliveryOptions(type, address);
                if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasSpecialEventWareHouse)
                {
                    RemoveUnauthorizedDeliveryOptions(mexicoDeliveryOptions, locale);
                }
                
                return mexicoDeliveryOptions;
            }

            List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            //Look if there is a postal code provided:

            if (type == DeliveryOptionType.Pickup)
            {
                PickupAlternativesResponse_V01 pickupAlternativesResponse = null;
                if (!string.IsNullOrEmpty(address.Address.PostalCode))
                {
                    pickupAlternativesResponse =
                        proxy.GetPickupAlternativesForPostalCode(new GetPickupAlternativesForPostalCodeRequest(new PickupAlternativesForPostalCodeRequest_V01()
                            {
                                PostalCode = address.Address.PostalCode
                            })).GetPickupAlternativesForPostalCodeResult as PickupAlternativesResponse_V01;
                }
                else
                {
                    pickupAlternativesResponse =
                        proxy.GetPickupAlternativesForColony(new GetPickupAlternativesForColonyRequest(new PickupAlternativesForColonyRequest_V01()
                            {
                                Colony = address.Address.Line3,
                                Municipality = address.Address.City,
                                State = address.Address.StateProvinceTerritory
                            })).GetPickupAlternativesForColonyResult as PickupAlternativesResponse_V01;
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
                        proxy.GetShippingAlternativesForPostalCode(new ServiceProvider.ShippingMexicoSvc.GetShippingAlternativesForPostalCodeRequest(new ShippingAlternativesForPostalCodeRequest_V01()
                            {
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
            return deliveryOptions;
        }

        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country,
                                                                               string locale,
                                                                               MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01 address)
        {
            if (locale.Equals("en-MX"))
            {
                base.GetDeliveryOptionsListForShipping(Country, locale, address);
            }

            List<DeliveryOption> deliveryOptions = new List<DeliveryOption>();
            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            //Look if there is a postal code provided:

            if (!string.IsNullOrEmpty(address.Address.PostalCode))
            {
                var shippingAlternativesResponse =
                    proxy.GetShippingAlternativesForPostalCode(new ServiceProvider.ShippingMexicoSvc.GetShippingAlternativesForPostalCodeRequest(new ShippingAlternativesForPostalCodeRequest_V01()
                        {
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

        public override ShippingInfo GetShippingInfoFromID(string distributorID,
                                                           string locale,
                                                           DeliveryOptionType type,
                                                           int deliveryOptionID,
                                                           int shippingAddressID)
        {
            if (locale.Equals("en-MX"))
            {
                return base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            }

            var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
            DeliveryOptionForIDRequest_V01 request = new DeliveryOptionForIDRequest_V01();
            request.ID = deliveryOptionID;

            DeliveryOption deliveryOption = null;
            if (type == DeliveryOptionType.Pickup)
            {
                string countryCode = locale.Substring(3, 2);
                List<PickupLocationPreference_V01> pickupLocationPreference =
                    GetPickupLocationsPreferences(distributorID, countryCode);
                if (pickupLocationPreference != null && pickupLocationPreference.Count > 0)
                {
                    var vPickupLocation = pickupLocationPreference.Find(p => p.PickupLocationID == deliveryOptionID);
                    if (vPickupLocation != null)
                    {
                        int PickupLocationID = vPickupLocation.PickupLocationID;
                        request.ID = PickupLocationID;
                        PickupAlternativesResponse_V01 pickupAlternativesResponse =
                            proxy.GetPickupAlternativeForDeliveryOptionID(new GetPickupAlternativeForDeliveryOptionIDRequest(request)).GetPickupAlternativeForDeliveryOptionIDResult as PickupAlternativesResponse_V01;
                        if (pickupAlternativesResponse != null &&
                            pickupAlternativesResponse.PickupAlternatives != null &&
                            pickupAlternativesResponse.PickupAlternatives.Count > 0)
                        {
                            deliveryOption = new DeliveryOption(ObjectMappingHelper.Instance.GetToShipping(pickupAlternativesResponse.PickupAlternatives[0]));
                            deliveryOption.Id = deliveryOption.ID = PickupLocationID;
                            return new ShippingInfo(deliveryOption);
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

        /// <summary>
        ///     GetPickupLocationsPreferences for a Distributor
        ///     MEXICO ONLY
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public override List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId,
                                                                                         string country)
        {
            if (Thread.CurrentThread.CurrentCulture.Name.Equals("en-MX"))
            {
                var values = base.GetPickupLocationsPreferences(distributorId, country);
                return values;
            }

            // gets cache key 
            string cacheKey = getPickupLocationPreferenceKey(distributorId, country);
            HttpSessionState session = HttpContext.Current.Session;

            bool loadFromService = false;
            if (null == session[cacheKey])
            {
                loadFromService = true;
            }
            List<PickupLocationPreference_V01> PickupLocationPreferences =
                getPickupLocationsPreferencesFromCache(distributorId, country);
            List<PickupLocationPreference_V01> PickupLocationPreferencestoRemove = new List<PickupLocationPreference_V01>();
            if (PickupLocationPreferences != null && loadFromService == true)
            {
                var proxy = ServiceClientProvider.GetMexicoShippingServiceProxy();
                foreach (PickupLocationPreference_V01 pref in PickupLocationPreferences)
                {
                    //checking if the location saved is in CUBO DB
                    var option = GetShippingInfoFromID(distributorId,
                                                            Thread.CurrentThread.CurrentCulture.Name, 
                                                            DeliveryOptionType.Pickup, 
                                                            pref.PickupLocationID, 
                                                            0);
                    if (option != null)
                    {
                        if (string.IsNullOrEmpty(pref.PickupLocationNickname))
                        {
                            DeliveryOptionForIDRequest_V01 request = new DeliveryOptionForIDRequest_V01();
                            request.ID = pref.PickupLocationID;
                            var pickupAlternativesResponse =
                                proxy.GetPickupAlternativeForDeliveryOptionID(new GetPickupAlternativeForDeliveryOptionIDRequest(request)).GetPickupAlternativeForDeliveryOptionIDResult as PickupAlternativesResponse_V01;
                            if (pickupAlternativesResponse != null &&
                                pickupAlternativesResponse.PickupAlternatives != null &&
                                pickupAlternativesResponse.PickupAlternatives.Count > 0)
                            {
                                pref.PickupLocationNickname =
                                    pickupAlternativesResponse.PickupAlternatives[0].BranchName;
                            }
                        }
                    }
                    else
                    {
                        PickupLocationPreferencestoRemove.Add(pref);
                    }
                }
                //removing the not existing locations in cubo
                if (PickupLocationPreferencestoRemove.Count > 0)
                {
                    foreach (var item in PickupLocationPreferencestoRemove)
                    {
                        DeletePickupLocationsPreferences(item.DistributorID, item.PickupLocationID,item.Country);
                        session["showMessageOnPage"] = "True";
                    }
                }
                session[cacheKey] = PickupLocationPreferences;
            }
            return PickupLocationPreferences;
        }

        public override string FormatShippingAddress(ServiceProvider.ShippingSvc.ShippingAddress_V01 address,
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
                           ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}<br>{7}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.Line3 ?? string.Empty,
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode,
                                           formatPhone(address.Phone),
                                           address.Address.Line2)
                           : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}<br>{6}",
                                           address.Address.Line1, address.Address.Line3 ?? string.Empty,
                                           address.Address.City, address.Address.StateProvinceTerritory,
                                           address.Address.PostalCode,
                                           formatPhone(address.Phone),
                                           address.Address.Line2);
            }
            else
            {
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                if (locale.Equals("en-MX"))
                {
                    return string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", address.Address.Line4 ?? string.Empty,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.StateProvinceTerritory, address.Address.PostalCode);
                }
                return string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
        }

        public override bool NeedEnterAddress(string distributorID, string locale)
        {
            List<PickupLocationPreference_V01> pickupRrefList =
                GetPickupLocationsPreferences(distributorID, locale.Substring(3, 2));
            List<DeliveryOption> shippingAddresses = GetShippingAddresses(distributorID, locale);
            return ((pickupRrefList == null || pickupRrefList.Count == 0) &&
                    (shippingAddresses == null || shippingAddresses.Count == 0));
        }

        #endregion

        #region Additional mexico specific methods

        public override int? GetDeliveryEstimate(ShippingInfo shippingInfo, string locale)
        {
            if (shippingInfo == null)
                return null;

            if (shippingInfo.Option == DeliveryOptionType.Shipping)
            {
                var list = GetDeliveryOptionsListForShipping("MX", locale, shippingInfo.Address);
                var opt = list.Where(x => x.Name == shippingInfo.Name).Select(x => x.ShippingIntervalDays).FirstOrDefault();
                return opt;
            }

            return null;
        }

        #region Shipping Instructions

        #region verbiage

        /*
         1.	Build the universal formula for Shipping Instructions:
            a.	Shipping Instructions Formula:
                i.	Servicio Ocurre (concatenate below fields/constants)
                    1.	Ocurre.NombreSucursal
                    2.	SPACE
                    3.	“A”         
                    4.	SPACE
                    5.	Ocurre.CiudadPob
                    6.	SPACE
                    7.	Ocurre.Codígo Postal
                    8.	SPACE
                    9.	Ocurre.Estado
                    10.	SPACE
                    11.	‘PICK UP NAME’ FIELD
                    12.	SPACE
                    13.	“Gracias por su orden”
                ii	Servicio Domicilio (concatenate below fields)
                    1.	Domicilio.Paqueteria
                    2.	SPACE
                    3.	“A”
                    4.	SPACE
                    5.	CodPostales.CiudadPob
                    6.	SPACE
                    7.	Domicilio.CodigoPostal
                    8.	SPACE
                    9.	CodPostales.Estado
                    10.	SPACE
                    11.	‘PICK UP NAME’ FIELD
                    12.	SPACE
                    13.	“Gracias por su orden”

         */

        #endregion

        public override string GetShippingInstructions(ShippingInfo currentShippingInfo)
        {
            if (currentShippingInfo.Option == DeliveryOptionType.Pickup)
            {
                //PickupOption_V01 po = currentShippingInfo.ShippingOption as PickupOption_V01;
                //ShippingAddress_V01 sa = po.PickupAddress;
                return string.Format("{0} {1} {2}/{3}/", currentShippingInfo.Name,
                                     currentShippingInfo.Address.Address.City,
                                     currentShippingInfo.Address.Address.PostalCode,
                                     currentShippingInfo.Address.Recipient);
            }
            else
            {
                //return (currentShippingInfo.ShippingOption == null) ? "" : currentShippingInfo.ShippingOption.CourierName;
                return currentShippingInfo.Name;
            }
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (shoppingCart.DeliveryInfo != null 
                && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                && HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasSpecialEventWareHouse
                && shoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse)
            {
                var distributorProfileModel = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var memberSubType = distributorProfileModel != null ? distributorProfileModel.Value.SubTypeCode : string.Empty;
                return string.Format("Honors 2016, {0} MEMBER", memberSubType);
            }

            if (currentShippingInfo.Option == DeliveryOptionType.Pickup)
            {
                return string.Format("{0} A {1} {2} {3} {4} Gracias por su orden.",
                                     currentShippingInfo.Description.Trim(), currentShippingInfo.Address.Address.City,
                                     currentShippingInfo.Address.Address.PostalCode,
                                     currentShippingInfo.Address.Address.StateProvinceTerritory,
                                     currentShippingInfo.Address.Recipient);
            }
            else
            {
                return string.Format("{0} A {1} {2} {3} {4} Gracias por su orden.", currentShippingInfo.Name,
                                     currentShippingInfo.Address.Address.City,
                                     currentShippingInfo.Address.Address.PostalCode,
                                     currentShippingInfo.Address.Address.StateProvinceTerritory,
                                     currentShippingInfo.Address.Recipient);
            }

            #endregion
        }

        #endregion
    }
}