using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_DO : ShippingProviderBase
    {
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
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                return string.Format("{0} Gracias por su Orden", shoppingCart.DeliveryInfo.Instruction).Trim();
            }

            return string.Format(
                "{0} {1} Gracias por su Orden",
                shoppingCart.DeliveryInfo.Address.Recipient,
                shoppingCart.DeliveryInfo.Address.Phone);
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1} {2} {3}<br>{4},{5}<br>{6} {7}<br>{8}",
                                                       address.Address.Line4 ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.Line3 ?? string.Empty,
                                                       address.Address.City, address.Address.CountyDistrict,                
                                                       address.Address.StateProvinceTerritory, address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0} {1} {2}<br>{3}, {4}<br>{5} {6}<br>{7}",
                                                       address.Address.Line1, 
                                                       address.Address.Line2 ?? string.Empty, address.Address.Line3 ?? string.Empty,
                                                       address.Address.City, address.Address.CountyDistrict,
                                                       address.Address.StateProvinceTerritory, address.Address.PostalCode,
                                                       address.Address.StateProvinceTerritory,
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
            if (string.IsNullOrWhiteSpace(a.City) ||
                     string.IsNullOrWhiteSpace(a.StateProvinceTerritory))
                // fails when City or State are empty, they're required
                return false;

            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                 !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City))
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }
    }
}
