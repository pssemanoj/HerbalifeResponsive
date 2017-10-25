using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    /// Shipping provider for RO
    /// </summary>
    public class ShippingProvider_RO : ShippingProviderBase
    {
        /// <summary>
        /// Field for store the sku's for the shipping instructions rules.
        /// </summary>
        private List<string> skuForInstructions = new List<string>() { "8601", "8602", "9568", "7760" };

        /// <summary>
        /// Gets a list of street types for a locality.
        /// </summary>
        /// <param name="country">Country code.</param>
        /// <param name="city">City name.</param>
        /// <param name="locality">Locality name.</param>
        /// <returns>List of street types.</returns>
        public List<string> GetStreetTypeForCity(string country, string city, string locality)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                StreetsForCityRequest_V01 request = new StreetsForCityRequest_V01();
                request.Country = country;
                request.State = city;
                request.City = locality;
                StreetsForCityResponse_V01 response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;

                List<string> streetTypes = new List<string>();
                foreach (var street in response.Streets)
                {
                    var info = street.Split('|');
                    if (info.Length == 2 && !streetTypes.Contains(info[0]))
                    {
                        streetTypes.Add(info[0]);
                    }
                }
                return streetTypes;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreetTypeForCity error: Country {0}, error: {1}", country, ex.ToString()));
            }
            return null;
        }

        /// <summary>
        /// Gets a list of cities from a street type
        /// </summary>
        /// <param name="country">Country code.</param>
        /// <param name="city">City name.</param>
        /// <param name="locality">Locality name.,</param>
        /// <param name="type">Street type.</param>
        /// <returns>List of streets.</returns>
        public List<string> GetStreetForCityByType(string country, string city, string locality, string type)
        {
            try
            {
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                StreetsForCityRequest_V01 request = new StreetsForCityRequest_V01();
                request.Country = country;
                request.State = city;
                request.City = locality;
                StreetsForCityResponse_V01 response = proxy.GetStreetsForCity(new GetStreetsForCityRequest(request)).GetStreetsForCityResult as StreetsForCityResponse_V01;

                type = string.Format("{0}|", type);
                var byType = response.Streets.Where(s => s.StartsWith(type));
                var streets = byType.Select(s => s.Replace(type, string.Empty)).OrderBy(c => c);
                return streets.ToList();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetStreetForCityByType error: Country {0}, error: {1}", country, ex.ToString()));
            }
            return null;
        }

        /// <summary>
        /// Gets the shipping instructions for the DS.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="distributorID">Distributor ID.</param>
        /// <param name="locale">Locale.</param>
        /// <returns>The shipping instructions.</returns>
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            string shippingInstructions = string.Empty;

            if (!string.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    shippingInstructions = "Factura catre DS";
                }
            }

            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            if (currentShippingInfo != null)
            {
                if (currentShippingInfo.Option == DeliveryOptionType.Pickup)
                {
                    shippingInstructions = (shippingInstructions.Length > 0) ? string.Format("{0} {1}", shippingInstructions, currentShippingInfo.Instruction) : currentShippingInfo.Instruction;
                }
            }

            var skus = from i in shoppingCart.CartItems
                       from s in skuForInstructions
                       where i.SKU == s
                       select i.SKU;
            if (skus.Count() > 0)
            {
                string instrBySKU = "Pretul pungilor include si Ecotaxa de 0.1 lei/buc";
                shippingInstructions = (shippingInstructions.Length > 0) ? string.Format("{0} {1}", shippingInstructions, instrBySKU) : instrBySKU;
            }

            return shippingInstructions;
        }

        /// <summary>
        /// Format the address object to send to HMS
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns></returns>
        public override bool FormatAddressForHMS(MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Address address)
        {
            if (address != null && !string.IsNullOrEmpty(address.Line4))
            {
                address.Line4 = string.Empty;
            }
            return true;
        }
    }
}