using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.Interfaces;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider
    {
        #region Static private properties

        private static Dictionary<string, IShippingProvider> _shippingProviders = new Dictionary<string, IShippingProvider>();
        private static object _lock = new object();

        #endregion Static private properties

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="ShippingProvider"/> class.
        /// </summary>
        private ShippingProvider()
        {
        }

        private static IShippingProvider GetCountrySpecificProvider(string countryCode)
        {
            switch (countryCode)
            {
                case "KR":
                    return new ShippingProvider_KR();
                case "MX":
                    return new ShippingProvider_MX();
                case "JP":
                    return new ShippingProvider_JP();
                case "GB":
                    return new ShippingProvider_UK();
                case "MY":
                    return new ShippingProvider_MY();
                case "IT":
                    return new ShippingProvider_IT();
                case "US":
                    return new ShippingProvider_US();
                case "IN":
                    return new ShippingProvider_IN();
                case "CA":
                    return new ShippingProvider_CA();
                case "NL":
                    return new ShippingProvider_NL();
                case "TW":
                    return new ShippingProvider_TW();
                case "CH":
                    return new ShippingProvider_CH();
                case "DE":
                    return new ShippingProvider_DE();
                case "PT":
                    return new ShippingProvider_PT();
                case "SE":
                    return new ShippingProvider_SE();
                case "DK":
                    return new ShippingProvider_DK();
                case "ZA":
                    return new ShippingProvider_ZA();
                case "FI":
                    return new ShippingProvider_FI();
                case "IE":
                    return new ShippingProvider_IE();
                case "NO":
                    return new ShippingProvider_NO();
                case "AU":
                    return new ShippingProvider_AU();
                case "PL":
                    return new ShippingProvider_PL();
                case "TR":
                    return new ShippingProvider_TR();
                case "IS":
                    return new ShippingProvider_IS();
                case "CO":
                    return new ShippingProvider_CO();
                case "RU":
                    return new ShippingProvider_RU();
                case "CL":
                    return new ShippingProvider_CL();
                case "AR":
                    return new ShippingProvider_AR();
                case "PE":
                    return new ShippingProvider_PE();
                case "VE":
                    return new ShippingProvider_VE();
                case "BR":
                    return new ShippingProvider_BR();
                case "GR":
                    return new ShippingProvider_GR();
                case "IL":
                    return new ShippingProvider_IL();
                case "RO":
                    return new ShippingProvider_RO();
                case "CR":
                    return new ShippingProvider_CR();
                case "SG":
                    return new ShippingProvider_SG();
                case "ID":
                    return new ShippingProvider_ID();
                case "HK":
                    return new ShippingProvider_HK();
                case "PA":
                    return new ShippingProvider_PA();
                case "BO":
                    return new ShippingProvider_BO();
                case "SV":
                    return new ShippingProvider_SV();
                case "GT":
                    return new ShippingProvider_GT();
                case "FR":
                    return new ShippingProvider_FR();
                case "JM":
                    return new ShippingProvider_JM();
		        case "MK":
                    return new ShippingProvider_MK();
                case "RS":
                    return new ShippingProvider_RS();
                case "PH":
                    return new ShippingProvider_PH();
                case "NZ":
                    return new ShippingProvider_NZ();
                case "HN":
                    return new ShippingProvider_HN();
                case "NI":
                    return new ShippingProvider_NI();
                case "PF":
                    return new ShippingProvider_PF();
                case "PY":
                    return new ShippingProvider_PY();
                case "UY":
                    return new ShippingProvider_UY();
                case "UA":
                    return new ShippingProvider_UA();
                case "MO":
                    return new ShippingProvider_MO();
                case "VN":
                    return new ShippingProvider_VN();
                case "HU":
                    return new ShippingProvider_HU();
                case "TH":
                    return new ShippingProvider_TH();
                case "PR":
                    return new ShippingProvider_PR();
                case "ES":
                    return new ShippingProvider_ES();
                case "CN":
                    return new ShippingProvider_CN();
                case "SI":
                    return new ShippingProvider_SI();
                case "DO":
                    return new ShippingProvider_DO();
                case "BY":
                    return new ShippingProvider_BY();
                case "KZ":
                    return new ShippingProvider_KZ();
                case "MN":
                    return new ShippingProvider_MN();
                case "HR":
                    return new ShippingProvider_HR();
                case "BA":
                    return new ShippingProvider_BA();
                case "SK":
                    return new ShippingProvider_SK();
                case "CZ":
                    return new ShippingProvider_CZ();
                case "BE":
                    return new ShippingProvider_BE();
                case "BG":
                    return new ShippingProvider_BG();
                case "AT":
                    return new ShippingProvider_AT();
                case "EC":
                    return new ShippingProvider_EC();
                default:
                    return new ShippingProviderBase();
            }
        }

        /// <summary>
        /// Gets the shipping provider (factory method)
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        public static IShippingProvider GetShippingProvider(string countryCode)
        {
            if (null == countryCode)
            {
                countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.ToString().Substring(3);
            }

            lock (_lock)
            {
                IShippingProvider provider;
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