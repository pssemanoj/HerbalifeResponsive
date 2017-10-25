using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_ZA : ShippingProviderBase
    {
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (!String.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    return "DO NOT INCLUDE INVOICE";
                }
                return shoppingCart.DeliveryInfo.Instruction;
            }

            return String.Empty;
        }

        public override List<string> GetStatesForCountry(string country)
        {
            return base.GetAddressField(new AddressFieldForCountryRequest_V01()
            {
                AddressField = AddressPart.PROVINCE,
                Country = country
            });
        }

        public override List<string> GetCountiesForCity(string country, string state, string city)
        {
            if (!string.IsNullOrEmpty(state))
            {
                return GetAddressField(new AddressFieldForCountryRequest_V01()
                {
                    AddressField = AddressPart.COUNTY,
                    Country = country,
                    Province = state
                });
            }
            return new List<string>();
        }

        public override List<string> GetZipsForCounty(string country, string state, string city, string county)
        {
            if (!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(county))
            {
                return GetAddressField(new AddressFieldForCountryRequest_V01()
                {
                    AddressField = AddressPart.ZIPCODE,
                    Country = country,
                    Province = state,
                    County = county
                });
            }
            return new List<string>();
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (a != null)
            {
                var zipcodes = this.GetZipsForCounty(a.Country, a.StateProvinceTerritory, null, a.City);
                if (zipcodes != null && zipcodes.Any())
                {
                    return zipcodes.Contains(a.PostalCode);
                }
            }
            return false;
        }
    }
}
