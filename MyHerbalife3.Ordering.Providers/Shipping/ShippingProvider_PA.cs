namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Web;
    using System.Linq;
    using System;
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider;

    public class ShippingProvider_PA : ShippingProviderBase
    {
        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if ( null == address || address.Address == null )
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if ( type == DeliveryOptionType.Shipping )
            {
                formattedAddress = includeName ? string.Format("{0}<br/>{1}{2}<br/>{3}, {4}, {5}<br/>{6}",
                    address.Recipient ?? string.Empty,
                    address.Address.Line1,
                    !string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br/>" + address.Address.Line2 : "",
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone)) :
                    string.Format("{0}{1}<br/>{2}, {3}, {4}<br/>{5}",
                    address.Address.Line1,
                    !string.IsNullOrWhiteSpace(address.Address.Line2) ? "<br/>" + address.Address.Line2 : "",
                    address.Address.CountyDistrict, address.Address.City, address.Address.StateProvinceTerritory,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br/>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}<br>{7}",
                    description,
                    address.Address.Line1, address.Address.Line2, address.Address.Line3, address.Address.Line4,
                    address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
            return formattedAddress;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if ( shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Pickup && shippingInfo.Address != null )
            {
                return string.Format("{0} {1} Gracias por su Orden", shippingInfo.Address.Recipient, shippingInfo.Address.Phone);
            }
            else if ( shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Shipping )
            {
                return string.Format("{0},{1}", string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction) ? string.Empty : shoppingCart.DeliveryInfo.Instruction, "Gracias por su Orden");
            }
            return string.Empty;
        }

        /// <summary>
        ///     Get the freight and warehouse code for the address.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns></returns>
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var defaultFCandW = new[] {
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
            };

            // call validation, if passes use service to validate when failed fallback to previous method (xml files)
            var FCandWh = new[] 
            {
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
            };
            if (IsValidShippingAddress(address.Address))
                FCandWh = GetFreightCodeAndWarehouseFromService(address);

            if (FCandWh != null)
            {
                return FCandWh;
            }
            if (null != address && null != address.Address)
            {
                string city = address.Address.CountyDistrict + "|" + address.Address.City;
                if (getFreightURB().Contains(city))
                {
                    defaultFCandW[0] = "URB";
                }
            }
            return defaultFCandW;
        }
        /// <summary>
        /// Retrieves FreightCode and WareHouse from mappings defined in dbo.LuShippingFreightCode table.
        /// Combines STATE|CITY|COUNTY to search for a matching FreightCode and WareHouse in DB.
        /// </summary>
        /// <param name="a">The address used as input for mapping lookup</param>
        /// <returns></returns>
        private static string[] GetFreightCodeAndWarehouseFromService(ShippingAddress_V01 a)
        {
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "PA",
                State = string.Format("{0}|{1}|{2}",
                    a.Address.StateProvinceTerritory,
                    a.Address.City,
                    a.Address.CountyDistrict),
                Locale = "es-PA"
            };

            using ( var proxy = ServiceClientProvider.GetShippingServiceProxy() )
            {
                try
                {
                    var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
                    if ( response != null && response.DeliveryAlternatives != null &&
                        response.DeliveryAlternatives.Count > 0 )
                    {
                        var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
                        if ( shippingOption != null )
                        {
                            return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
                        }
                    }
                }
                catch ( Exception ex )
                {
                    LoggerHelper.Error(string.Format("GetFreightCodeAndWarehouseFromService error: Country: GT, error: {0}",
                        ex.ToString()));
                }
            }
            return null;
        }

        /// <summary>
        /// Returns Cities mapped for URB. The list is retrieved from \\App_data\\XML\\Cities_PA.xml.
        /// </summary>
        /// <returns></returns>
        private List<string> getFreightURB()
        {

            var R1 = ResolveUrl("\\App_data\\XML\\Cities_PA.xml");
            XElement xelement = XElement.Load(R1);
            IEnumerable<XElement> locations = xelement.Elements();
            List<string> result = new List<string>();
            foreach ( var location in locations )
            {
                var xCity = location.Value;
                result.Add(xCity);
            }
            return result;
        }

        public string ResolveUrl(string originalUrl)
        {
            if ( originalUrl != null && originalUrl.Trim() != "" )
            {
                if ( originalUrl.StartsWith("/") )
                {
                    originalUrl = "~" + originalUrl;
                }
                else
                {
                    originalUrl = "~/" + originalUrl;
                }

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if ( originalUrl == null )
            {
                return null;
            }
            if ( originalUrl.IndexOf("://") != -1 )
            {
                return originalUrl;
            }
            if ( originalUrl.StartsWith("~") )
            {
                string newUrl = "";
                if ( HttpContext.Current != null )
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                }
                return newUrl;
            }
            return originalUrl;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if ( string.IsNullOrWhiteSpace(a.Line2) ||
                string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                string.IsNullOrWhiteSpace(a.City) ||
                string.IsNullOrWhiteSpace(a.CountyDistrict) )
                // fails when Line2, State or City are empty (they're required)
                return false;

            if ( !GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                 !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City) ||
                 !GetCountiesForCity(a.Country, a.StateProvinceTerritory, a.City).Contains(a.CountyDistrict) )
                // finally, validate that State and City are within valid set of values
                return false;

            return true;
        }
        public override string GetDifferentHtmlFragment(string freightcode, ShippingAddress_V01 address)
        {
            string option = freightcode != null ? freightcode.Trim() : freightcode;

            string[] FCandWh=GetFreightCodeAndWarehouseFromService(address);
            
            if (FCandWh != null)
                if (FCandWh[0] == freightcode)
                    return string.Empty;
                else
                    return "NoExpressDelivery.html";
            else
                return string.Empty;

        }
    }
}