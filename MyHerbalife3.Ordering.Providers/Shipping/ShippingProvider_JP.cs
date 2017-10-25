using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Text;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_JP : ShippingProviderBase
    {
        public override string GetFreightVariant(ShippingInfo shippingInfo)
        {
            if (shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Shipping)
            {
                if(shippingInfo.FreightCode.Equals(HLConfigManager.Configurations.APFConfiguration.APFFreightCode))
                {
                    return null;
                }

                Address_V01 address = shippingInfo != null && shippingInfo.Address != null
                                          ? shippingInfo.Address.Address
                                          : null;
                if (address != null)
                {
                    return address.Line3 == null ? "YO" : address.Line3;
                }
            }
            return null;
        }

        public override bool ShouldRecalculate(string oldFreightCode,
                                               string newFreightCode,
                                               Address_V01 oldaddress,
                                               Address_V01 newaddress)
        {
            if (oldaddress == null || newaddress == null)
            {
                return true;
            }
            return (oldaddress.Line3 != newaddress.Line3);
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

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}",
                                           address.Recipient ?? string.Empty,
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.CountyDistrict ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2,address.Address.Line3 ?? string.Empty)
                           : string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}",
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.CountyDistrict ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2?? string.Empty);
            }
            else
            {
                return string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}", address.Recipient ?? string.Empty,
                                     address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                     address.Address.City, address.Address.Line2 ?? string.Empty, address.Address.Line1);
            }
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            return currentShippingInfo == null ? String.Empty : currentShippingInfo.Instruction;
        }

        public override string GetAddressDisplayName(ShippingAddress_V02 address)
        {
            if ((address.Alias != null) && (address.Alias != string.Empty))
                return address.Alias;
            else
            {
                if (!String.IsNullOrEmpty(address.FirstName) && !String.IsNullOrEmpty(address.LastName))
                {
                    return string.Format("{0},{1},{2},{3},{4}", address.FirstName, address.LastName,
                                         address.Address.Line1, address.Address.City,
                                         address.Address.StateProvinceTerritory);
                }
                else
                {
                    return string.Format("{0},{1},{2},{3}", address.Recipient, address.Address.Line1,
                                         address.Address.City, address.Address.StateProvinceTerritory);
                }
            }
        }

        public override string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            string name = string.Empty;
            if (!String.IsNullOrEmpty(address.Alias))
                name = address.Alias;
            else
                name = address.Recipient;

            string formattedAddress = string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}",
                                                    name,
                                                    address.Address.PostalCode,
                                                    address.Address.StateProvinceTerritory,
                                                    address.Address.City,
                                                    address.Address.CountyDistrict ?? string.Empty,
                                                    address.Address.Line1,
                                                    formatPhone(address.Phone));

            return formattedAddress.Replace("<br><br>", "<br>");
        }
        public override bool FormatAddressForHMS(MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            if (address != null)
            {
                address.Line1 = GetSubstring(address.Line1);
                address.Line2 = GetSubstring(address.Line2);
                address.Line3 = GetSubstring(address.Line3);
                address.Line4 = GetSubstring(address.Line4);
                address.City = GetSubstring(address.City);
                address.CountyDistrict = GetSubstring(address.CountyDistrict);
            }
            return true;
        }

        private string GetSubstring(string info)
        {
            if (string.IsNullOrEmpty(info))
                return info;

            Encoding enc_utf8 = new UTF8Encoding(false, true);
            if (enc_utf8.GetByteCount(info) <= 60)
                return info;

            var length = info.Length * 60 / enc_utf8.GetByteCount(info);
            if (enc_utf8.GetByteCount(info.Substring(0, length)) > 60)
                return info.Substring(0, length - 1);
            else
                return info.Substring(0, length);
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            retrieveFreightCode(shoppingCart, null);

            if (address.Address != null)
            {
                address.Address.Line1 = GetSubstring(address.Address.Line1);
                address.Address.Line2 = GetSubstring(address.Address.Line2);
                address.Address.Line3 = GetSubstring(address.Address.Line3);
                address.Address.Line4 = GetSubstring(address.Address.Line4);
                address.Address.City = GetSubstring(address.Address.City);
                address.Address.CountyDistrict = GetSubstring(address.Address.CountyDistrict);
            }

        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Shipment shippment)
        {
            retrieveFreightCode(shoppingCart, null);
            if (shoppingCart.DeliveryInfo != null)
                shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
            return true;
        }

        private void retrieveFreightCode(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                shoppingCart.FreightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                if (shoppingCart.DeliveryInfo != null)
                {
                    shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                    shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                }
            }
        }
        public static AddressDetailByPostalCodeResponse_V01 GetAddressByPostalCode(string countryCode,string PostalCode)
        {
            PostalCode = PostalCode.Replace("-", string.Empty).TrimStart('0');    
            var address = new AddressDetailByPostalCodeResponse_V01();
            var request = new AddressDetailByPostalCodeRequest_V01()
            {
                Country = countryCode,
                ZipCode = PostalCode
            };
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                address = proxy.GetAddressDetailForPostalCode(new GetAddressDetailForPostalCodeRequest(request)).GetAddressDetailForPostalCodeResult as AddressDetailByPostalCodeResponse_V01;
                return address ;
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(string.Format("GetAddressDetailForPostalCode error: Country: {0}, error: {1}",countryCode,
                        ex.ToString()));
            }
            return null;
        }
    }
}
