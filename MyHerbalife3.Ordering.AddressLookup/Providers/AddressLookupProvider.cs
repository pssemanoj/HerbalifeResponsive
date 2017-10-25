using System.Collections.Generic;

namespace MyHerbalife3.Ordering.AddressLookup.Providers
{
    public class AddressLookupProvider
    {
        #region Static private properties

        private static Dictionary<string, IAddressLookupProvider> _shippingProviders = new Dictionary<string, IAddressLookupProvider>();
        private static object _lock = new object();

        #endregion Static private properties

        #region Construction

        private AddressLookupProvider()
        {
        }

        private static IAddressLookupProvider GetCountrySpecificProvider(string countryCode)
        {
            switch (countryCode)
            {
                case "IN":
                    return new AddressLookupProvider_IN();
                case "MK":
                    return new AddressLookupProvider_MK();
                case "MX":
                    return new AddressLookupProvider_MX();
                case "PT":
                case "RS":
                case "NO":
                case "SE":
                case "FI":
                    return new AddressLookupProvider_PT();
                
                default:
                    return new AddressLookupProviderBase();
            }
        }

        public static IAddressLookupProvider GetAddressLookupProvider(string countryCode)
        {
            if (null == countryCode)
            {
                countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Substring(3);
            }

            lock (_lock)
            {
                IAddressLookupProvider provider;
                if (!_shippingProviders.TryGetValue(countryCode.ToUpper(), out provider))
                {
                    provider = GetCountrySpecificProvider(countryCode.ToUpper());
                    _shippingProviders.Add(countryCode.ToUpper(), provider);
                }
                return provider;
            }
        }

        #endregion Construction
    }
}
