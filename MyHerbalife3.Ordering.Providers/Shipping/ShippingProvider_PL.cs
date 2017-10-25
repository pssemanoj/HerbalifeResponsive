using System;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_PL : ShippingProviderBase
    {
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorId, string locale)
        {
            if (!String.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    return "INVOICE TO DS";
                }
            }
            return String.Empty;
        }
    }
}
