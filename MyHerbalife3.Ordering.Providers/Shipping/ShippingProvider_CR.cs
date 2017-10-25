using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    /// Shipping provider for CR
    /// </summary>
    public class ShippingProvider_CR : ShippingProviderBase
    {


        private List<string> getCountriesFreightURB()
        {

            var R1 = ResolveUrl("\\App_data\\XML\\Cities_CR.xml");
            XElement xelement = XElement.Load(R1);
            IEnumerable<XElement> locations = xelement.Elements();
            List<string> result = new List<string>();
            foreach (var location in locations)
            {
                var xCity = location.Value;
                result.Add(xCity);
            }
            return result;
        }

        public string ResolveUrl(string originalUrl)
        {
            if (originalUrl != null && originalUrl.Trim() != "")
            {
                if (originalUrl.StartsWith("/"))
                {
                    originalUrl = "~" + originalUrl;
                }
                else
                {
                    originalUrl = "~/" + originalUrl;
                }

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if (originalUrl == null)
            {
                return null;
            }
            if (originalUrl.IndexOf("://") != -1)
            {
                return originalUrl;
            }
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                }
                return newUrl;
            }
            return originalUrl;
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
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}, {6}<br>{7}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.StateProvinceTerritory, address.Address.City, address.Address.CountyDistrict, 
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}, {5}<br>{6}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.StateProvinceTerritory, address.Address.City, address.Address.CountyDistrict, 
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}, {6}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.StateProvinceTerritory, address.Address.City, address.Address.CountyDistrict,
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


        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null)
            {
                if (string.IsNullOrEmpty(shippingInfo.Instruction))
                {
                    return "Gracias por su Orden";
                }

                if (!shippingInfo.Instruction.Contains("Gracias por su Orden"))
                {
                    return string.Format("{0} Gracias por su Orden", shippingInfo.Instruction);
                }
                else
                {
                    return shippingInfo.Instruction;
                }
        
            }
            return string.Empty;
        }

        private static string[] GetFreightCodeAndWarehouseFromService(ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01
            {
                Country = "CR",
                Locale = "es-CR",
                State = string.Format("{0}|{1}|{2}",
                    address.Address.StateProvinceTerritory,                     
                    address.Address.City,
                    address.Address.CountyDistrict)
            };
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var shippingOption = response.DeliveryAlternatives.FirstOrDefault();
            if (shippingOption != null)
            {
                return new[] { shippingOption.FreightCode, shippingOption.WarehouseCode };
            }
            return null;
        }        

        /// <summary>
        ///     Get the freight and warehouse code for the address.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns></returns>
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };

            // call validation, if passes use service to validate when failed fallback to previous method (xml files)
            if (IsValidShippingAddress(address.Address))
            {
                var frWh = GetFreightCodeAndWarehouseFromService(address);
                if (frWh != null && frWh.Count() > 0)
                {
                    return frWh;
                }
            }
            else if (null != address && null != address.Address)
            {
                string city = address.Address.CountyDistrict + "|" + address.Address.City;
                if (getCountriesFreightURB().Contains(city))
                {
                    freightCodeAndWarehouse[0] = "URB";
                }
            }
            return freightCodeAndWarehouse;
        }

        protected override bool IsValidShippingAddress(Address_V01 a)
        {
            if (string.IsNullOrWhiteSpace(a.StateProvinceTerritory) ||
                     string.IsNullOrWhiteSpace(a.City) ||
                     string.IsNullOrWhiteSpace(a.CountyDistrict))
                // fails when City or County are empty
                return false;

            if (a.PostalCode != null && a.PostalCode.Length > 0 && a.PostalCode.Length != 5)
                // postal code is optional but when present, it MUST be 5 chars long
                return false;

            if (!GetStatesForCountry(a.Country).Contains(a.StateProvinceTerritory) ||
                 !GetCitiesForState(a.Country, a.StateProvinceTerritory).Contains(a.City) ||
                 !GetCountiesForCity(a.Country, a.StateProvinceTerritory, a.City).Contains(a.CountyDistrict))
                // finally, validate that city and county are within valid set of values
                return false;

            return true;
        }

        private void CheckingFreightCode(MyHLShoppingCart shoppingCart)
        {
            // To prevent prod issue where wrong freight code is set (NOF)
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null &&
                shoppingCart.DeliveryInfo.Address.Address != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                shoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.RSO && shoppingCart.DeliveryInfo.FreightCode == "NOF")
            {
                var sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                var isAPF = APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale);

                // If order is standalone APF or ETO we leave the freight
                if (!sessionInfo.IsEventTicketMode && !isAPF)
                {
                    var freightAndWarehouse = GetFreightCodeAndWarehouse(shoppingCart.DeliveryInfo.Address);
                    shoppingCart.DeliveryInfo.FreightCode = freightAndWarehouse[0];
                }
            }
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            CheckingFreightCode(shoppingCart);
            if (shoppingCart.DeliveryInfo != null)
                shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
            return true;
        }
    }
}
