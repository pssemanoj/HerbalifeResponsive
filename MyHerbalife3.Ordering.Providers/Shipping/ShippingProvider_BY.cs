using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers.Shipping
{    
    public class ShippingProvider_BY : ShippingProviderBase
    {
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
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0} Спасибо за Ваш заказ", shoppingCart.DeliveryInfo.Instruction).Trim();
            }
            else if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup && shoppingCart.DeliveryInfo.Description.Contains(HL.Common.Configuration.Settings.GetRequiredAppSetting("ByExtravaganza", "")))
            {
                return string.Format("Extravaganza Pre-Ordering. Спасибо за Ваш заказ.");
            }

            return string.Format("Спасибо за Ваш заказ");
        }
        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            ShippingInfo shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            if (shippingInfo != null && shippingInfo.Address != null && type == DeliveryOptionType.Shipping)
            {
                var sessionInfo = SessionInfo.GetSessionInfo(distributorID, locale);
                var deliveryOptions = getDeliveryOptionsFromCache(locale);
                var shippingoptions = getDeliveryOptionFromID(deliveryOptionID, type, deliveryOptions,
                                                              sessionInfo.IsEventTicketMode
                                                                  ? OrderCategoryType.ETO
                                                                  : OrderCategoryType.RSO);
               
                shippingInfo.FreightCode = shippingoptions.FreightCode;
                shippingInfo.WarehouseCode = shippingoptions.WarehouseCode;
            }
            return shippingInfo;
        }
 
        private DeliveryOption getDeliveryOptionFromID(int deliveryOptionID,
                                                     DeliveryOptionType type,
                                                     List<DeliveryOption> deliveryOptions,
                                                     OrderCategoryType orderCategoryType)
        {
            if (deliveryOptions != null && deliveryOptions.Count > 0)
            {
                
                if (deliveryOptionID >= 0)
                {
                    var options = deliveryOptions.Where(x => x.Option == type && x.OrderCategory == orderCategoryType);
                    if (options.Count() > 0)
                    {
                        return options.First();
                    }
                }
                else
                {
                    return deliveryOptions.Find(x => x.Id == deliveryOptionID && x.Option == type);
                }
            }
            return null;
        }
        private static List<DeliveryOption> getDeliveryOptionsFromCache(string countryCode)
        {
            List<DeliveryOption> result = null;

            if (string.IsNullOrEmpty(countryCode))
            {
                return result;
            }

            // gets cache key
            string cacheKey = getDeliveryOptionsCacheKey(countryCode);

            // tries to get object from cache
            result = HttpRuntime.Cache[cacheKey] as List<DeliveryOption>;

            if (null == result)
            {
                try
                {
                    // gets resultset from Business Layer is object not found in cache
                    result = getDeliveryOptionsFromService(countryCode);
                    // saves to cache is successful
                    if (null != result)
                    {
                        saveDeliveryOptionsFromCache(cacheKey, result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return result;
        }

        private static void saveDeliveryOptionsFromCache(string cacheKey, List<DeliveryOption> shippings)
        {
            if (shippings != null)
            {
                HttpRuntime.Cache.Insert(cacheKey,
                                         shippings,
                                         null,
                                         DateTime.Now.AddMinutes(ShippingCacheMinutes),
                                         Cache.NoSlidingExpiration,
                                         CacheItemPriority.Normal,
                                         null);
            }
        }
        private static string getDeliveryOptionsCacheKey(string countryCode)
        {
            return DELIVERY_OPTIONS_CACHE_PREFIX + countryCode;
        }
        private static List<DeliveryOption> getDeliveryOptionsFromService(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return null;
            }
            else
            {
                var proxy = ServiceProvider.ServiceClientProvider.GetShippingServiceProxy();
                var response =
                    proxy.GetDeliveryPickupAlternatives(new GetDeliveryPickupAlternativesRequest(new DeliveryPickupAlternativesRequest_V02() { CountryCode = countryCode })).GetDeliveryPickupAlternativesResult as
                    DeliveryPickupAlternativesResponse_V02;
                if (response != null && response.Status == ServiceResponseStatusType.Success &&
                    response.DeliveryPickupAlternatives != null)
                {
                    var deliveryOptions = new List<DeliveryOption>();
                    foreach (DeliveryPickupOption_V02 dpo in response.DeliveryPickupAlternatives)
                    {
                        var deliveryOption = new DeliveryOption(dpo);
                        if (!string.IsNullOrEmpty(dpo.State))
                        {
                            deliveryOption.State = dpo.State.Trim();
                        }
                        else
                        {
                            if (dpo.PickupAddress != null && dpo.PickupAddress.Address != null &&
                                dpo.PickupAddress.Address.StateProvinceTerritory != null)
                            {
                                deliveryOption.State = dpo.PickupAddress.Address.StateProvinceTerritory.Trim();
                            }
                        }

                        if (!string.IsNullOrEmpty(dpo.WarehouseCode))
                        {
                            deliveryOption.WarehouseCode = dpo.WarehouseCode;
                        }
                        else
                        {
                            if (dpo.ShippingSource != null)
                                deliveryOption.WarehouseCode = dpo.ShippingSource.Warehouse;
                        }
                        deliveryOptions.Add(deliveryOption);
                    }
                    return deliveryOptions;
                }
            }
            return null;
        }
        private static void onShippingInfoCacheRemove(string key, object sender, CacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:

                    try
                    {
                        string serviceKey = key.Replace(DELIVERY_OPTIONS_CACHE_PREFIX, string.Empty);
                        var result = getDeliveryOptionsFromService(serviceKey);

                        if (result != null)
                        {
                            // if success replace cache with new resultset
                            saveDeliveryOptionsFromCache(key, result);
                        }
                        else
                        {
                            // if failure re-insert from cache
                            saveDeliveryOptionsFromCache(key, (List<DeliveryOption>)sender);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    }

                    break;
                case CacheItemRemovedReason.Removed:
                case CacheItemRemovedReason.DependencyChanged:
                default:
                    break;
            }
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (address == null || address.Address == null)
            {
                return string.Empty;
            }

            var formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br/>{1}<br/>{2}, {3}<br/>{4}, {5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, 
                                                       address.Address.StateProvinceTerritory, address.Address.CountyDistrict,
                                                       address.Address.PostalCode, address.Address.City)
                                       : string.Format("{0}<br/>{1}, {2}<br/>{3}, {4}",                                                       
                                                       address.Address.Line1,
                                                       address.Address.StateProvinceTerritory, address.Address.CountyDistrict,
                                                       address.Address.PostalCode, address.Address.City);
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }
    }
}
