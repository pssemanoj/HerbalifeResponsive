using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    /// Shipping provider for NI
    /// </summary>
    public class ShippingProvider_NI : ShippingProviderBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// Format the address object to send to HMS
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns></returns>
        public override bool FormatAddressForHMS(MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            var addrV01 = new Address_V01()
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                StateProvinceTerritory = address.StateProvinceTerritory,
                CountyDistrict = address.CountyDistrict,
                Country = address.Country
            };

            if ( address != null &&
                !string.IsNullOrWhiteSpace(address.StateProvinceTerritory) &&
                !string.IsNullOrWhiteSpace(address.City) &&
                !IsValidShippingAddress(addrV01) )
            {
                address.Line3 = address.StateProvinceTerritory;
                address.StateProvinceTerritory = string.Empty;
            }
            return true;
        }

        /// <summary>
        /// Format the Shipping Instructions
        /// </summary>
        /// <param name="shoppingCart">The cart</param>
        /// <param name="distributorId">DS id</param>
        /// <param name="locale">Locale</param>
        /// <returns></returns>
        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if ( shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping )
            {
                return string.Format("{0} Gracias por su Orden", shoppingCart.DeliveryInfo.Instruction).Trim();
            }

            return string.Format(
                "{0} {1} Gracias por su Orden",
                shoppingCart.DeliveryInfo.Address.Recipient,
                shoppingCart.DeliveryInfo.Address.Phone);
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ( string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                string.IsNullOrWhiteSpace(a.City) )
                // fails when State or County are empty (they're required)
                return false;

            if ( !GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City) )
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if ( null == address || address.Address == null )
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if ( type == DeliveryOptionType.Shipping )
            {
                formattedAddress = includeName ? string.Format("{0}<br/>{1}{2}<br/>{3}, {4}<br>{5}",
                    address.Recipient ?? string.Empty,
                    address.Address.Line1,
                    !string.IsNullOrEmpty(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone)) : string.Format("{0}{1}<br/>{2}, {3}<br>{4}",
                    address.Address.Line1,
                    !string.IsNullOrEmpty(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}{1}<br/>{2}<br/>{3}, {4}<br>{5}",
                    description,
                    address.Address.Line1,
                    !string.IsNullOrEmpty(address.Address.Line2) ? "<br />" + address.Address.Line2 : "",
                    address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode);
            }
            return formattedAddress;
        }
        #endregion
    }
}