#region

using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileAddressProvider : IMobileAddressProvider
    {
        public List<AddressViewModel> Get(GetAddressRequestViewModel request)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(request.Locale.Substring(3));
            var result = new List<AddressViewModel>();
            if (shippingProvider != null)
            {
                var shippingAddresses = shippingProvider.GetShippingAddresses(request.MemberId, request.Locale);
                //filter the addresses
                if (request.From != null)
                {
                    shippingAddresses = shippingAddresses.Where(x => x.Created >= request.From).ToList();
                }
                if (request.To != null)
                {
                    shippingAddresses = shippingAddresses.Where(x => x.Created <= request.To).ToList();
                }
                if (shippingAddresses != null && shippingAddresses.Any())
                {
                    result.AddRange(
                        shippingAddresses.Select(
                            a => ModelConverter.ConvertAddressToViewModel(a, false, request.MemberId, request.Locale)));
                }
            }
            return result;
        }


        public List<AddressViewModel> Get(string memberId, string locale)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            var result = new List<AddressViewModel>();
            if (shippingProvider != null)
            {
                var shippingAddresses = shippingProvider.GetShippingAddresses(memberId, locale);
                if (shippingAddresses != null && shippingAddresses.Any())
                {
                    result.AddRange(
                        shippingAddresses.Select(
                            a => ModelConverter.ConvertAddressToViewModel(a, true, memberId, locale)));
                }
            }
            return result;
        }

        public int Save(ref AddressViewModel address, string memberId, string locale)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (shippingProvider != null)
            {
                //check if the address exist
                var addressToSave = ModelConverter.ConvertAddressViewModelToShippingAddress(address);
                if (locale == "zh-CN")
                {
                    addressToSave.Address.Line3 = string.Empty;
                    addressToSave.Address.Line4 = string.Empty;
                }
                var existing = shippingProvider.GetShippingAddressFromAddressGuidOrId(address.CloudId, 0);
                if (null != existing)
                {
                    addressToSave.ID = existing.ID;
                }
                var shippingAddressId= shippingProvider.SaveShippingAddress(memberId, locale, addressToSave, false, true, false);
                if (shippingAddressId > 0)
                {
                    var savedAddress = shippingProvider.GetShippingAddressFromAddressGuidOrId(address.CloudId, 0);
                    address.LastUpdatedDate = savedAddress.Created;
                    address = ModelConverter.ConvertShippingAddressToAddressViewModel(savedAddress);
                    address.CustomShippingMethods = ModelConverter.GetCustomShippingMethods(memberId, savedAddress,
                        locale);
                }
                return shippingAddressId;
            }
            return 0;
        }

        public AddressViewModel Delete(string memberId, Guid id, string locale)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (shippingProvider != null)
            {
                var existing = shippingProvider.GetShippingAddressFromAddressGuidOrId(id, 0);
                if (existing != null)
                {
                    shippingProvider.DeleteShippingAddress(memberId, locale, existing);
                    return ModelConverter.ConvertShippingAddressToAddressViewModel(existing);
                }
            }
            return null;
        }

        public List<CustomShippingMethodViewModel> GetShippingMethods(string memberId, Guid id, string locale)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (shippingProvider != null)
            {
                var existing = shippingProvider.GetShippingAddressFromAddressGuidOrId(id, 0);
                if (existing != null)
                {
                    return ModelConverter.GetCustomShippingMethods(memberId, existing, locale);
                }
            }
            return null;
        }

        internal class ModelConverter
        {
            public static ShippingAddress_V02 ConvertAddressViewModelToShippingAddress(AddressViewModel addressViewModel)
            {
                var address = new ShippingAddress_V02
                {
                    Address = new Address_V01
                    {
                        City = addressViewModel.City,
                        Country = addressViewModel.Country,
                        CountyDistrict = addressViewModel.CountyDistrict,
                        Line1 = addressViewModel.Line1,
                        Line2 = addressViewModel.Line2,
                        Line3 = addressViewModel.Line3,
                        Line4 = addressViewModel.Line4,
                        PostalCode = addressViewModel.PostalCode,
                        StateProvinceTerritory = addressViewModel.StateProvinceTerritory
                    },
                    AddressId = addressViewModel.CloudId,
                    Phone = string.Empty, //addressViewModel.Phone,
                    Recipient = string.Empty, //addressViewModel.Recipient,
                    CustomerId = addressViewModel.PersonCloudId,
                    Alias = addressViewModel.NickName
                };
                address.IsPrimary = addressViewModel.IsPrimary;
                return address;
            }

            public static AddressViewModel ConvertShippingAddressToAddressViewModel(ShippingAddress_V02 shippingAddress)
            {
                var address = new AddressViewModel
                {
                    CloudId = shippingAddress.AddressId,
                    StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory,
                    City = shippingAddress.Address.City,
                    Country = shippingAddress.Address.Country,
                    CountyDistrict = shippingAddress.Address.CountyDistrict,
                    IsPrimary = shippingAddress.IsPrimary,
                    LastUpdatedDate = shippingAddress.Created,
                    Line1 = shippingAddress.Address.Line1,
                    Line2 = shippingAddress.Address.Line2,
                    Line3 = shippingAddress.Address.Line3,
                    Line4 = shippingAddress.Address.Line4,
                    NickName = shippingAddress.Alias,
                    PersonCloudId = shippingAddress.CustomerId,
                    PostalCode = shippingAddress.Address.PostalCode,
                };
                return address;
            }

            public static AddressViewModel ConvertShippingAddressToAddressViewModel(ShippingAddress_V01 shippingAddress)
            {
                var address = new AddressViewModel
                {
                    CloudId = Guid.Empty,
                    StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory,
                    City = shippingAddress.Address.City,
                    Country = shippingAddress.Address.Country,
                    CountyDistrict = shippingAddress.Address.CountyDistrict,
                    IsPrimary = shippingAddress.IsPrimary,
                    LastUpdatedDate = shippingAddress.Created,
                    Line1 = shippingAddress.Address.Line1,
                    Line2 = shippingAddress.Address.Line2,
                    Line3 = shippingAddress.Address.Line3,
                    Line4 = shippingAddress.Address.Line4,
                    NickName = shippingAddress.Alias,
                    PersonCloudId = string.Empty,
                    PostalCode = shippingAddress.Address.PostalCode,
                };
                return address;
            }


            public static AddressViewModel ConvertAddressToViewModel(DeliveryOption option,
                bool fetchCustomShippingMethods = false, string memberId = null, string locale = null)
            {
                var address = new AddressViewModel
                {
                    City = option.Address.City,
                    Country = option.Address.Country,
                    CountyDistrict = option.Address.CountyDistrict,
                    CloudId = option.AddressId,
                    Line1 = option.Address.Line1,
                    Line2 = option.Address.Line2,
                    Line3 = option.Address.Line3,
                    Line4 = option.Address.Line4,
                    PostalCode = option.Address.PostalCode,
                    StateProvinceTerritory = option.Address.StateProvinceTerritory,
                    NickName = option.Alias,
                    IsPrimary = option.IsPrimary,
                    PersonCloudId = option.CustomerId,
                    LastUpdatedDate = option.Created,
                    CustomShippingMethods =
                        fetchCustomShippingMethods ? GetCustomShippingMethods(memberId, option, locale) : null
                };
                return address;
            }

            public static List<CustomShippingMethodViewModel> GetCustomShippingMethods(string memberId,
                ShippingAddress_V01 address, string locale)
            {
                try
                {
                    var country = locale.Substring(3);
                    var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
                    if (shippingProvider == null) return null;
                    var deliveryOptions = shippingProvider.GetDeliveryOptionsListForMobileShipping(country, locale,
                        address, memberId);
                    if (null == deliveryOptions || !deliveryOptions.Any()) return null;
                    var customShippingModels = deliveryOptions.Select(option => new CustomShippingMethodViewModel
                    {
                        Code = option.FreightCode,
                        Title = option.Description,
                        OrderTypes = "RSO"
                    }).ToList();
                    return customShippingModels;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("General", ex, "Failed while getting mobile GetCustomShippingMethods");
                    return null;
                }
            }
        }
    }
}