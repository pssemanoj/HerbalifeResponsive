using System;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    class ShippingProvider_IS : ShippingProviderBase
    {
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
            {
                return "PU BY ";
            }

            return String.Empty;
        }
    }
}

