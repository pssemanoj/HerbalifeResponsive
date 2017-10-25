
namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

    public class ShippingProvider_HK : ShippingProviderBase
    {
        private const string CacheKey = "DeliveryMethod_HK";
        private const int HK_SHIPPINGINFO_CACHE_MINUTES = 60 * 12;

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Pickup && shippingInfo.Address != null)
            {
                return string.Format("{0} {1} {2}", shippingInfo.Address.Recipient, shippingInfo.Address.Phone, shippingInfo.HKID);
            }
            else if (shippingInfo != null)
            {
                return shoppingCart.DeliveryInfo.Instruction;
            }
            return string.Empty;
        }

        /// <summary>
        /// Assigns the promotional freight code according volume in cart
        /// </summary>
        /// <param name="cart"></param>
        public override void SetShippingInfo(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCart_V01 cart)
        {
            if (cart != null)
            {
                var myhlCart = cart as MyHLShoppingCart;
                if (myhlCart != null && myhlCart.DeliveryInfo != null)
                {
                    if (myhlCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                        HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalRequiredVolumePoints != 0 &&
                        !string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalFreightCode))
                    {
                        var freightCode = (myhlCart.VolumeInCart >= HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalRequiredVolumePoints)
                            ? HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalFreightCode
                            : HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                        var oldFreightCode = myhlCart.DeliveryInfo.FreightCode;
                        myhlCart.DeliveryInfo.FreightCode = freightCode;
                        if (!myhlCart.DeliveryInfo.FreightCode.Equals(oldFreightCode))
                        {
                            ShoppingCartProvider.UpdateShoppingCart(myhlCart);
                        }
                    }
                }
            }
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName ? string.Format("{0}<br>{1},{2}<br>{3}, {4}<br>{5}<br>{6}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2, address.Address.City,
                    address.Address.PostalCode, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}<br>{4}<br>{5}",
                    address.Address.Line1, address.Address.Line2, address.Address.City, address.Address.PostalCode, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone));
            }
            else
            {
                return string.Format("{0}<br>{1}<br>{2}<br>{3}",
                    address.Address.Line1,
                    address.Address.Line2,
                    address.Address.City,
                    address.Address.StateProvinceTerritory);
            }
        }

        //public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address)
        //{
        //    List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
        //    List<DeliveryOption> result = null;
        //    if (null != deliveryOptions && deliveryOptions.Count > 0)
        //    {
        //        string state = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "HK";
        //        result = deliveryOptions.Where(d => d.State == state).ToList();
        //        if (null == result || result.Count == 0)
        //        {
        //            result = GetDeliveryOptionsFromService(Country, locale, address);
        //            SaveDeliveryOptionsToCache(result);
        //        }
        //    }
        //    else
        //    {
        //        result = GetDeliveryOptionsFromService(Country, locale, address);
        //        SaveDeliveryOptionsToCache(result);
        //    }

        //    return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
        //}

        //private static List<DeliveryOption> GetDeliveryOptionsFromService(string Country, string locale, ShippingAddress_V01 address)
        //{
        //    List<DeliveryOption> result = new List<DeliveryOption>();
        //    ShippingClient proxy = new ShippingClient();
        //    proxy.Endpoint.Address = new System.ServiceModel.EndpointAddress(HL.Common.Configuration.Settings.GetRequiredAppSetting(ShippingServiceSettingKey));
        //    DeliveryOptionForCountryRequest_V01 request = new DeliveryOptionForCountryRequest_V01();
        //    request.Country = Country;
        //    request.State = request.State = (address != null && address.Address != null && !string.IsNullOrEmpty(address.Address.StateProvinceTerritory)) ? address.Address.StateProvinceTerritory : "HK";
        //    request.Locale = locale;
        //    ShippingAlternativesResponse_V01 response = proxy.GetDeliveryOptions(request) as ShippingAlternativesResponse_V01;
        //    foreach (HL.Shipping.ValueObjects.ShippingOption_V01 option in response.DeliveryAlternatives)
        //    {
        //        DeliveryOption currentOption = new DeliveryOption(option);
        //        currentOption.Name = option.Description;
        //        currentOption.WarehouseCode = option.WarehouseCode;
        //        currentOption.State = request.State;
        //        result.Add(currentOption);
        //    }
        //    return result.OrderBy(d => d.displayIndex.ToString() + "_" + d.DisplayName).ToList();
        //}

        //private static void SaveDeliveryOptionsToCache(List<DeliveryOption> options)
        //{
        //    List<DeliveryOption> deliveryOptions = HttpRuntime.Cache[CacheKey] as List<DeliveryOption>;
        //    if (null != deliveryOptions)
        //    {
        //        deliveryOptions.AddRange(options);
        //    }
        //    else
        //    {
        //        HttpRuntime.Cache.Insert(CacheKey,
        //        options,
        //        null,
        //        DateTime.Now.AddMinutes(HK_SHIPPINGINFO_CACHE_MINUTES),
        //        Cache.NoSlidingExpiration,
        //        CacheItemPriority.NotRemovable,
        //        null);

        //    }
        //}
    }
}