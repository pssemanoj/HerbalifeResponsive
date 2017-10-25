using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_CH : ShippingProviderBase
    {
        public override string GetRecipientName(ShippingInfo currentShippingInfo)
        {
            if (null == currentShippingInfo || currentShippingInfo.Address == null)
            {
                return string.Empty;
            }

            string RecipientName = String.Empty;
            switch (currentShippingInfo.Address.Address.Line4)
            {
                case "Privatadresse":
                    RecipientName = currentShippingInfo.Address.Recipient + " +PRIVAT";
                    break;
                case "Kundenadresse":
                    RecipientName = currentShippingInfo.Address.Recipient + " KUNDE";
                    break;
                case "Adresse privee":
                    RecipientName = currentShippingInfo.Address.Recipient + " +PRIVAT";
                    break;
                case "Adresse du client":
                    RecipientName = currentShippingInfo.Address.Recipient + " KUNDE";
                    break;
                default:
                    RecipientName = currentShippingInfo.Address.Recipient;
                    break;
            }

            return RecipientName;
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

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br>{1},{2}<br>{3}, {4}<br>{5}<br>{6}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City,
                                           address.Address.PostalCode,
                                           formatPhone(address.Phone),
                                           address.Address.Line4)
                           : string.Format("{0},{1}<br>{2}, {3}<br>{4}<br>{5}",
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.City, address.Address.PostalCode,
                                           formatPhone(address.Phone),
                                           address.Address.Line4);
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City,
                                     address.Address.StateProvinceTerritory, address.Address.PostalCode);
            }
        }
    }
}