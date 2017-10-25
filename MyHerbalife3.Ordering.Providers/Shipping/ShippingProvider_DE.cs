using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_DE : ShippingProviderBase
    {
        public override string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            string formattedAddress = string.Format("{8}<br>{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}<br>{7}",
                                                    address.Recipient,
                                                    address.Address.Line1,
                                                    address.Address.Line2 ?? string.Empty,
                                                    address.Address.City,
                                                    address.Address.CountyDistrict ?? string.Empty,
                                                    address.Address.PostalCode,
                                                    address.Address.StateProvinceTerritory,
                                                    formatPhone(address.Phone), address.Alias ?? string.Empty);

            return formattedAddress.Replace("<br><br>", "<br>");
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
                           ? string.Format("{0}<br>{1}<br>{2}, {3}<br>{4}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.City,
                                           address.Address.PostalCode,
                                           formatPhone(address.Phone))
                           : string.Format("{0},{1}<br>{2}<br>{3}",
                                           address.Address.Line1, address.Address.City, address.Address.PostalCode,
                                           formatPhone(address.Phone));
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3}, {4}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.PostalCode);
            }
        }
    }
}