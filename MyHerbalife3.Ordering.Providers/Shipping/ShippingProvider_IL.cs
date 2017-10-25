namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using HL.Common.ValueObjects;

    /// <summary>
    /// Shipping provider class for IL
    /// </summary>
    public class ShippingProvider_IL : ShippingProviderBase
    {
        private const string _InvoiceInstructions = "***Invoice to DS***";

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            if (!string.IsNullOrEmpty(shoppingCart.InvoiceOption))
            {
                if (shoppingCart.InvoiceOption.Trim() == "SendToDistributor")
                {
                    if (string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction))
                    {
                        return _InvoiceInstructions;
                    }
                    else if (!shoppingCart.DeliveryInfo.Instruction.Contains(_InvoiceInstructions))
                    {
                        return string.Format("{0} {1}", shoppingCart.DeliveryInfo.Instruction, _InvoiceInstructions);
                    }
                }
            }
            return shoppingCart.DeliveryInfo.Instruction;
        }
    }
}
