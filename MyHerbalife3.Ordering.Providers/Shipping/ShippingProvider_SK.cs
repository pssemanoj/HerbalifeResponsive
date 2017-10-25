using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Globalization;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_SK : ShippingProviderBase
    {
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
                                       ? string.Format("{0}<br/>{1}<br/>{2} {3}<br/>{4}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1,
                                                       address.Address.PostalCode, address.Address.City,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0}<br/>{1} {2}<br/>{3}",
                                                       address.Address.Line1,
                                                       address.Address.PostalCode, address.Address.City,
                                                       formatPhone(address.Phone));
            }
            else if (type == DeliveryOptionType.Pickup)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br/>{1}<br/>{2} {3}<br/>{4}<br/>{5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1,
                                                       address.Address.PostalCode, address.Address.City, address.Address.CountyDistrict,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0}<br/>{1} {2}<br/>{3}<br/>{4}",
                                                       address.Address.Line1,
                                                       address.Address.PostalCode, address.Address.City, address.Address.CountyDistrict,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            CultureInfo culture = new CultureInfo(locale);
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0}", shoppingCart.DeliveryInfo.Instruction).Trim();
            }
            else if(shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {                
                if (String.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Phone) && shoppingCart.DeliveryInfo.PickupDate == null)
                {
                    return string.Format("");
                }
                else
                {
                    return string.Format("{0}, {1}", shoppingCart.DeliveryInfo.Address.Phone, shoppingCart.DeliveryInfo.PickupDate == null ? "" : shoppingCart.DeliveryInfo.PickupDate.Value.ToString("d", culture));
                }
            }
            else
            {                
                return string.Format("{0}, {1}, {2}", shoppingCart.DeliveryInfo.Address.Phone, shoppingCart.DeliveryInfo.PickupDate == null ? "" : shoppingCart.DeliveryInfo.PickupDate.Value.ToString("d", culture), string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction) ? string.Empty : shoppingCart.DeliveryInfo.Instruction);
            }
        }
       
    }
}
