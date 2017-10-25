using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_NZ : ShippingProviderBase
    {
        #region Public Methods and Operators

        public override string GetShippingInstructionsForDS(
            MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                     ? string.Empty
                                     : shoppingCart.DeliveryInfo.Instruction;
            if (shoppingCart.DeliveryInfo.Address != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    return
                        instruction =
                        string.Format(
                            "{0},{1},{2}",
                            shoppingCart.DeliveryInfo.Address.Recipient,
                            shoppingCart.DeliveryInfo.Address.Phone,
                            instruction);
                }
            }
            return instruction;
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
                formattedAddress = includeName ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory,
                    address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}<br>{2}<br>{3} {4}<br>{5}", description,
                    address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.City, address.Address.PostalCode, address.Address.StateProvinceTerritory);
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

        #endregion
    }
}