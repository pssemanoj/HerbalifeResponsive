using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_EC : ShippingProviderBase
    {
        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                            DeliveryOptionType type,
                                            string description,
                                            bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}<br>{7}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}<br>{6}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.CountyDistrict,address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode);
            }
            if (formattedAddress.IndexOf(",,") > -1 || formattedAddress.IndexOf(", ,") > -1)
            {
                return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
            }
            else
            {
                return formattedAddress;
            }
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ((!string.IsNullOrWhiteSpace(a.PostalCode)) && (!string.IsNullOrEmpty(a.CountyDistrict)) 
                && (GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City)))
                return true;
            else
                return false;
        }
    }
}