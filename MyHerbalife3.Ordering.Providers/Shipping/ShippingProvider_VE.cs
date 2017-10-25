namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using System.Collections.Generic;

    public class ShippingProvider_VE : ShippingProviderBase
    {
        
        #region Public Methods and Operators

        public override string FormatShippingAddress(
            ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format(
                                           "{0}<br>{1} {2}<br>{3},{4}, {5}<br>{6}{7}",
                                           address.Recipient ?? string.Empty,
                                           address.Address.Line1,
                                           string.IsNullOrEmpty(address.Address.Line2)
                                               ? string.Empty
                                               : string.Format(",{0}", address.Address.Line2),
                                           address.Address.CountyDistrict,
                                           address.Address.City,
                                           address.Address.StateProvinceTerritory,
                                           string.IsNullOrEmpty(address.Address.PostalCode)
                                               ? string.Empty
                                               : string.Format("{0}<br>", address.Address.PostalCode),
                                           this.formatPhone(address.Phone))
                                       : string.Format(
                                           "{0}{1}<br>{2}, {3}, {4}<br>{5}{6}",
                                           address.Address.Line1,
                                           string.IsNullOrEmpty(address.Address.Line2)
                                               ? string.Empty
                                               : string.Format(",{0}", address.Address.Line2),
                                           address.Address.CountyDistrict, 
                                           address.Address.City,
                                           address.Address.StateProvinceTerritory,
                                           string.IsNullOrEmpty(address.Address.PostalCode)
                                               ? string.Empty
                                               : string.Format("{0}<br>", address.Address.PostalCode),
                                           this.formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format(
                    "{0}<br>{1}{2}<br>{3},{4}<br>{5}",
                    description,
                    address.Address.Line1,
                    string.IsNullOrEmpty(address.Address.Line2)
                        ? string.Empty
                        : string.Format(",{0}", address.Address.Line2),
                    address.Address.City,
                    address.Address.StateProvinceTerritory,
                    address.Address.PostalCode);
            }
            return formattedAddress;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ((!string.IsNullOrWhiteSpace(a.PostalCode)) && (!string.IsNullOrEmpty(a.CountyDistrict))
                && (GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory)) && (GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City)))
                return true;
            else
                return false;
        }
        #endregion
    }
}