using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_BA : ShippingProviderBase
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
                                       ? string.Format("{0}<br/>{1}<br/>{2}, {3}<br/>{4}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, 
                                                       address.Address.PostalCode, address.Address.City,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0}<br/>{1}, {2}<br/>{3}",
                                                       address.Address.Line1,
                                                       address.Address.PostalCode, address.Address.City,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }
    }
}
