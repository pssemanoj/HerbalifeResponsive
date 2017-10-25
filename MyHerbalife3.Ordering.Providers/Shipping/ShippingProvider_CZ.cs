using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Threading;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_CZ : ShippingProviderBase
    {
        /// <summary>
        /// Format the Shipping Instructions
        /// </summary>
        /// <param name="shoppingCart">The cart</param>
        /// <param name="distributorId">DS id</param>
        /// <param name="locale">Locale</param>
        /// <returns></returns>
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            string invoice = "";
            string shipInstructions = "";

            if (!String.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption == InvoiceHandlingType.SendToDistributor.ToString())
                {
                    invoice = "Zaslat zvlášť poštou";
                }                
            }

            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                shipInstructions = string.Format("{0} {1}", invoice, shoppingCart.DeliveryInfo.Instruction).Trim();   
            }
            else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                CultureInfo culture = new CultureInfo(locale);
                shipInstructions = string.Format("{0}{1}, {2}, {3}", String.IsNullOrEmpty(invoice) ? "" : invoice + ", ",
                                                        String.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Recipient) ? "" : shoppingCart.DeliveryInfo.Address.Recipient,
                                                        String.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Phone) ? "" : shoppingCart.DeliveryInfo.Address.Phone,
                                                        shoppingCart.DeliveryInfo.PickupDate == null ? "" : shoppingCart.DeliveryInfo.PickupDate.Value.ToString("d", culture));
            }

            return shipInstructions;
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (address == null || address.Address == null)
            {
                return string.Empty;
            }

            var formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br/>{1} {2}<br/>{3} {4}<br/>{5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2,
                                                       address.Address.PostalCode, address.Address.City,
                                                       address.Address.Country)
                                       : string.Format("{0} {1} <br/>{2} {3}<br/>{4}",
                                                       address.Address.Line1, address.Address.Line2,
                                                       address.Address.PostalCode, address.Address.City,
                                                       address.Address.Country);
            }
            else
            {
                formattedAddress = base.FormatShippingAddress(address, type, description, includeName);
            }
            return formattedAddress;
        }

        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            if (type == DeliveryOptionType.Pickup)
            {
                var baseList = base.GetDeliveryOptions(type, address);

                // US 262457: Check for Prague Extravaganza pickup location
                var eventId = 0;
                if (int.TryParse(HLConfigManager.Configurations.DOConfiguration.EventId, out eventId) && eventId > 0 && baseList != null)
                {
                    if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse))
                    {
                        var memberWithTicket = false;
                        var memberQualified = DistributorOrderingProfileProvider.IsEventQualified(eventId, "cs-CZ", out memberWithTicket);
                        if (!memberQualified || !memberWithTicket)
                            baseList = baseList.Where(x => x.Description != HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse).ToList();
                    }
                }

                return baseList;
            }
            return base.GetDeliveryOptions(type, address);
        }
    }
}
