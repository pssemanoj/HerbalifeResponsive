using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Linq;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    class ShippingProvider_TR : ShippingProviderBase
    {
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (!String.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    return "INVOICE TO DS";
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
            string formattedAddress = string.Empty;
            var line1Value = GetLineOneAddress(address.Address.Line1, address.Address.CountyDistrict);
            if (type == DeliveryOptionType.Shipping)
            {

                formattedAddress = includeName ? string.Format("{0}<br>{1}{2}<br>{3}, {4}, {5}<br>{6}<br>{7}", address.Recipient ?? string.Empty,
                    line1Value, string.IsNullOrEmpty(address.Address.Line2) ? string.Empty : string.Format(", {0}", address.Address.Line2),
                    address.Address.StateProvinceTerritory, address.Address.City, address.Address.CountyDistrict, address.Address.PostalCode, formatPhone(address.Phone)) :
                    string.Format("{0}{1}<br>{2}, {3}, {4}<br>{5}<br>{6}",
                    line1Value, string.IsNullOrEmpty(address.Address.Line2) ? string.Empty : string.Format(", {0}", address.Address.Line2),
                    address.Address.StateProvinceTerritory, address.Address.City, address.Address.CountyDistrict, address.Address.PostalCode, formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}, {2}<br>{3}, {4}, {5}", description,
                    line1Value, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Shipment shippment)
        {
            //NOTE: TR Mappping with HMS, shipment state must be null, city must containt the State and CountryDistrict the City.
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                if (!shippment.Address.Line1.Contains(shippment.Address.CountyDistrict))
                {
                    shippment.Address.Line1 = string.Format("{0}, {1}", shippment.Address.CountyDistrict, shippment.Address.Line1);                    
                }

                if (!String.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory))
                {
                    shippment.Address.CountyDistrict = shippment.Address.City;
                    shippment.Address.City = shippment.Address.StateProvinceTerritory;
                    shippment.Address.StateProvinceTerritory = null;
                }
            }
            
            return true;
        }

        private string GetLineOneAddress(string lineOneValue, string countyDistrict)
        {
            if (string.IsNullOrEmpty(lineOneValue))
            {
                return string.Empty;
            }

            string[] lineOne = lineOneValue.Split(',');
            var delimiter = string.Empty;
            var splittedAddress = string.Empty;
            if (lineOne.Count() == 1 || string.IsNullOrEmpty(countyDistrict))
            {
                return lineOneValue;
            }

            foreach (var lineValue in lineOne.Where(lineValue => lineValue != countyDistrict && !string.IsNullOrEmpty(lineValue)))
            {
                splittedAddress = string.Format("{0}{1}{2}", splittedAddress.TrimStart(), delimiter, lineValue);
                delimiter = ",";
            }

            return splittedAddress.TrimStart();
        }
    }
}
