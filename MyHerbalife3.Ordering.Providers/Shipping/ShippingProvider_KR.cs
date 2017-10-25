using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_KR : ShippingProviderBase
    {
        public override List<Address_V01> AddressSearch(string SearchTerm)
        {
            var addrFound = new List<Address_V01>();
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new KoreanAddressDataRequest_V01();
                request.SearchText = SearchTerm;
                var response =
                    proxy.GetKoreanAddressData(new GetKoreanAddressDataRequest(request)).GetKoreanAddressDataResult as KoreanAddressDataResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    if (response.AddressData != null && response.AddressData.Count > 0)
                    {
                        addrFound.AddRange(from a in response.AddressData
                                           select new Address_V01
                                               {
                                                   Country = "KR",
                                                   City = a.State,
                                                   Line1 = a.City,
                                                   Line2 = a.Street,
                                                   PostalCode = a.PostCode
                                               }
                            );
                    }
                    return addrFound;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Checkout - AddressSearch error: Country {0}, error: {1}", "KR", ex));
            }
            return addrFound;
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
                           ? string.Format("{0}<br>{1}, {2} {3} {4}", address.Recipient ?? string.Empty,
                                           address.Address.PostalCode, address.Address.City, address.Address.Line1,
                                           address.Address.Line2)
                           : string.Format("{0}, {1} {2} {3}",
                                           address.Address.PostalCode, address.Address.City, address.Address.Line1,
                                           address.Address.Line2);
            }
            return string.Empty;
        }

        public override string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            string formattedAddress = string.Format("{0}<br>{1},{2} {3} {4}<br>{5}", address.Recipient ?? string.Empty,
                                                    address.Address.PostalCode, address.Address.City,
                                                    address.Address.Line1, address.Address.Line2,
                                                    formatPhone(address.Phone));

            return formattedAddress.Replace("<br><br>", "<br>");
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            var currentShippingInfo = shoppingCart.DeliveryInfo;
            if (currentShippingInfo != null)
            {
                if (currentShippingInfo.Option == DeliveryOptionType.Shipping)
                {
                    return currentShippingInfo.Instruction;
                }
            }
            return string.Empty;
        }
    }
}