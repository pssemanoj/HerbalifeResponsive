
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_MO : ShippingProviderBase
    {

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
            if (shippingInfo != null && shippingInfo.Option == DeliveryOptionType.Pickup && shippingInfo.Address != null)
            {
                return string.Format("{0} {1} {2}", shippingInfo.Address.Recipient, shippingInfo.Address.Phone, shippingInfo.HKID);
            }
            else if (shippingInfo != null)
            {
                return shoppingCart.DeliveryInfo.Instruction;
            }
            return string.Empty;
        }


        public override string FormatShippingAddress(
            ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.StateProvinceTerritory,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone));
            }
            else
                if (type == DeliveryOptionType.Pickup){
                    formattedAddress = string.Format("{0}<br>{1}<br>{2} {3} {4}",
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,
                                                 address.Address.PostalCode);
                }
            
              else
                  {
                    formattedAddress = string.Format("{0}<br>{1}<br>{2}<br>{3} {4} {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                 address.Address.City, address.Address.StateProvinceTerritory,
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

        //public override void SetShippingInfo(ShoppingCart_V01 cart)
        //{
        //    MyHLShoppingCart myShoppingCart = cart as MyHLShoppingCart;
        //    if (myShoppingCart != null && myShoppingCart.DeliveryInfo != null && myShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
        //    {
        //        if (myShoppingCart.VolumeInCart > 250M)
        //        {
        //            myShoppingCart.DeliveryInfo.FreightCode =
        //            myShoppingCart.FreightCode = "NOF";
        //        }
        //        else
        //        {
        //            myShoppingCart.DeliveryInfo.FreightCode =
        //           myShoppingCart.FreightCode = "MCF";
        //        }
        //    }
        //}
    }
}