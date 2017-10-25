#region

using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobilePickUpProvider : IMobilePickUpProvider
    {
        public List<PickupViewModel> GetDeliveryOptions(string locale, AddressViewModel address)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            var result = new List<PickupViewModel>();
            if (null != shippingProvider)
            {
                var list = shippingProvider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier,
                    MobileAddressProvider.ModelConverter.ConvertAddressViewModelToShippingAddress(address));
                if (null != list && list.Any())
                {
                    //convert 
                    result.AddRange(list.Select(ModelConverter.ConvertDeliveryOptionToViewModel));
                }
                return result;
            }
            return null;
        }


        public List<PickupViewModel> GetSavedDeliveryOptions(string memberId,string locale)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            var result = new List<PickupViewModel>();
            if (null != shippingProvider)
            {
                var list = shippingProvider.GetPickupLocationsPreferences(memberId,locale.Substring(3));
                var puOptions = shippingProvider.GetDeliveryOptions(DeliveryOptionType.PickupFromCourier, new ShippingAddress_V01() { Address = new Address_V01() { Country = locale.Substring(3) } });
                if (null != list && list.Any())
                {
                    //convert and fill info
                    result.AddRange(list.Select(location => ModelConverter.ConvertDeliveryOptionToViewModel(location, puOptions)));
                }
                return result;
            }
            return null;
        }

        
        public PickupViewModel SavePickUpPreference(string memberId, PickupViewModel model, string locale)
        {
            IShippingProvider shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (null != shippingProvider)
            {
                model.IDSaved = shippingProvider.SavePickupLocationsPreferences(memberId, false, model.DeliveryOptionId, model.Alias,
                                                                model.Name, model.Address.Country, model.IsPrimary);
                
            }

            return model;
        }

        public bool DeletePickUpLocation(string memberId, PickupViewModel model, string locale)
        {
            IShippingProvider shippingProvider = ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (null != shippingProvider)
        {
                shippingProvider.DeletePickupLocationsPreferences(memberId, model.DeliveryOptionId,
                                                                  model.Address.Country);
            }
            return true;
        }
        
        
        internal class ModelConverter
        {

            public static PickupViewModel ConvertDeliveryOptionToViewModel(PickupLocationPreference_V01 option, List<DeliveryOption> options)
            {
                //retrieving the delivery option to fill all the info for the saved options
                var original = options.Where(x => x.Id == option.PickupLocationID).FirstOrDefault();
                if (original != null)
                {
                    var puViewModel = ConvertDeliveryOptionToViewModel(original);
                    puViewModel.IsPrimary = option.IsPrimary;
                    puViewModel.Alias = option.PickupLocationNickname;
                    puViewModel.IDSaved = option.ID;
                    return puViewModel;
                }
                //if the option does not exist return a blank object
                return new PickupViewModel();
            }

            public static PickupViewModel ConvertDeliveryOptionToViewModel(DeliveryOption option)
            {
                return new PickupViewModel()
                    {
                    Address = MobileAddressProvider.ModelConverter.ConvertAddressToViewModel(option),
                        AddressType = option.AddressType,
                        Alias = option.Alias,
                        AltAreaCode = option.AltAreaCode,
                        AltPhone = option.AltPhone,
                        AreaCode = option.AreaCode,
                        Attention = option.Attention ==null? null : option.Attention.ToString(),
                        CourierType = option.CourierType,
                        Created = option.Created,
                        DeliveryOptionId = option.Id,
                        State = option.State,
                        DeliveryType = option.Option.ToString(),
                        OrderCategory = option.OrderCategory.ToString(),
                        Information = option.Information,
                        Description = option.Name,
                        FreightCode = option.FreightCode,
                        Warehouse = option.WarehouseCode,
                        Name = option.Description,
                        ShippingIntervaldays = option.ShippingIntervalDays,
                        DisplayName = option.DisplayName,
                        IsPrimary = option.IsPrimary,
                        Recipient = option.Recipient,
                        Phone = option.Phone,
                        Id = option.Id,
                        IDSaved = option.ID
                    };
            }
        }
    }
}
