using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using System.Text.RegularExpressions;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_NL : ShippingProviderBase
    {
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
                           ? string.Format("{0}<br>{1},{2}<br>{3} {4}, {5}", address.Recipient ?? string.Empty,
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City)
                           : string.Format("{0},{1}<br>{2} {3}, {4}",
                                           address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City);
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3} {4}, {5}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                     address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                     address.Address.City);
            }
        }

        public override bool ValidateAddress(ShippingAddress_V02 address)
        {
            if (!string.IsNullOrEmpty(address.Address.Line1))
            {
                var line1 = address.Address.Line1;
                // P O Box restriction Formats : postbus,pobox,postofficebox,p.o.box,pb,PB
                if (Regex.IsMatch(line1, @"(.*?(?i)(\bpb\b)[^$]*)|(.*?(?i)(\bpostbus\b)[^$]*)|(.*?(?i)(\bpobox\b)[^$]*)|(.*?(?i)(\bpostofficebox\b)[^$]*)|(.*?(?i)(\bp.o.box\b)[^$]*)|(.*?(?i)(\bpostb\b)[^$]*)"))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            //NL: Street 1 and House Number will be saved and sent to fusion as Street1 + <Space> + House Number in Street1
            shippment.Address.Line1 = string.Format("{0} {1}", shippment.Address.Line1.Trim(), shippment.Address.Line2.Trim());
            shippment.Address.Line2 = string.Empty;
            return true;
        }
        public override bool IsValidShippingAddress(MyHLShoppingCart shoppingCart)
        {
            return !(shoppingCart != null
                    && shoppingCart.DeliveryInfo != null
                    && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                    && shoppingCart.DeliveryInfo.Address != null
                    && shoppingCart.DeliveryInfo.Address.Address != null
                    && string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.Line2));
        }
    }
}