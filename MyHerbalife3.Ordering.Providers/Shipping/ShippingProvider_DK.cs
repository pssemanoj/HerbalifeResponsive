using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_DK : ShippingProviderBase
    {
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            int postCode = 0;
            bool success = int.TryParse(shoppingCart.DeliveryInfo.Address.Address.PostalCode, out postCode);
            if (success)
            {
                if (postCode >= 100 && postCode <= 970)
                {
                    //Faroe Island
                    return "FAROE ISLANDS";
                }
                else if (postCode.ToString().StartsWith("39"))
                {
                    //Greenland
                    return "GREENLAND";
                }
            }
            return String.Empty;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {

                return includeName ? string.Format("{0}<br>{1},{2}<br>{3}, {4}<br>{5}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, 
                    address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}<br>{4}",
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.PostalCode,
                    formatPhone(address.Phone));
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
        }

        private bool isFaroeIsland(string postalCode)
        {
            int postal;
            if (int.TryParse(postalCode, out postal))
            {
                return postal >= 100 && postal <= 970;
            }
            return false;
        }

        private bool isDenmark(string postalCode)
        {
            int postal;
            if (int.TryParse(postalCode, out postal))
            {
                return (postal >= 1000 && postal <= 3899) ||
                            (postal >= 4000 && postal <= 9999);
            }
            return false;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            string[] freightCodeAndWarehouse = new string[] { "USI", "35" };
            if (null != address && null != address.Address)
            {
                string postalCode = address.Address.PostalCode;
                if (!string.IsNullOrEmpty(postalCode))
                {
                    bool bFaroeIsland = false;
                    bool bDenmark = false;
                    if (postalCode.StartsWith("39") || (bFaroeIsland = isFaroeIsland(postalCode)) == true) // Greenland or Faroe Island
                    {
                        freightCodeAndWarehouse[0] = "DKA";
                        if (bFaroeIsland && postalCode[0] != '0')
                        {
                            address.Address.PostalCode = string.Concat("0", postalCode);
                        }
                    }
                    else if (bDenmark = isDenmark(postalCode) == true)
                    {
                        freightCodeAndWarehouse[0] = "DUA";
                    }
                }
            }
            return freightCodeAndWarehouse;
        }
    }
}
