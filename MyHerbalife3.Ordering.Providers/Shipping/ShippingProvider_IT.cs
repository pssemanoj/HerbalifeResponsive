
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_IT : ShippingProviderBase
    {
        public override string GetShippingInstructions(ShippingInfo currentShippingInfo)
        {
            if (currentShippingInfo != null && currentShippingInfo.Option == DeliveryOptionType.Pickup)
            {
                return currentShippingInfo.Instruction;
            }
            else
            {
                //return currentShippingInfo.Instruction; // :-)
                return string.Empty;
            }
        }

        public override string GetAddressDisplayName(ShippingAddress_V02 address)
        {
            return string.Format("...{0},{1},{2},{3}", address.Recipient, address.Address.Line1, address.Address.City,
                                 address.Address.StateProvinceTerritory);
        }

        //public override bool IsPickupInstructionsRequired() { return true; }

        public override bool ShouldRecalculate(string oldFreightCode,
                                               string newFreightCode,
                                               Address_V01 oldaddress,
                                               Address_V01 newaddress)
        {
            return false;
        }

        public override bool ValidateShipping(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    if (shoppingCart.DeliveryInfo != null &&
                        !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                        !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode))
                        return true;
                }
                else
                {
                    if (!APFDueProvider.containsOnlyAPFSku(shoppingCart.CartItems))
                    {
                        if (shoppingCart.DeliveryInfo != null &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode) &&
                            shoppingCart.DeliveryInfo.PickupDate != null &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Recipient))
                            return true;
                    }
                    else
                    {
                        if (shoppingCart.DeliveryInfo != null &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) &&
                            !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightCode))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}