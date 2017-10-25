using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_ES : ShippingProviderBase
    {
        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var freightCodeAndWarehouse = new[] { string.Empty, string.Empty };

            if (address != null && address.Address != null)
            {
                int postCode = 0;
                bool success = int.TryParse(address.Address.PostalCode, out postCode);
                if (success)
                {
                    if ((postCode <= 35660 && postCode >= 35000) || postCode <= 38917 && postCode >= 38001)
                    {
                        freightCodeAndWarehouse[0] = "OTHC";
                        freightCodeAndWarehouse[1] = "C3";
                    }
                }
            }

            return freightCodeAndWarehouse;
        }
    }
}
