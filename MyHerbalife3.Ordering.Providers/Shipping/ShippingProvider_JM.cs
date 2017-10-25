using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    /// <summary>
    ///     Shipping provider for JM
    /// </summary>
    public class ShippingProvider_JM : ShippingProviderBase
    {
        public override string[] GetFreightCodeAndWarehouseForTaxRate(Address_V01 address)
        {
            if (address != null)
            {
                return new[]
                    {
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                        HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                    };
            }
            return null;
        }
    }
}