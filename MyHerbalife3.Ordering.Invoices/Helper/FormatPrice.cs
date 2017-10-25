#region

using System.Globalization;
using System.Threading;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;

#endregion

namespace MyHerbalife3.Ordering.Invoices.Helper
{
    public static class FormatPriceExtension
    {
        public static string FormatPrice(this decimal number)
        {
            return GetAmountString(number);
        }

        internal static string GetAmountString(decimal amount)
        {
            var symbol = GetSymbol();
            return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbolPosition ==
                   CheckoutConfiguration.CurrencySymbolLayout.Leading
                ? (HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                    ? symbol + amount.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                    : symbol + string.Format(GetPriceFormat(amount), amount))
                : string.Format(GetPriceFormat(amount), amount) + symbol;
        }

        internal static string GetSymbol()
        {
            return HLConfigManager.Configurations.CheckoutConfiguration != null
                ? HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol
                : Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
        }

        internal static string GetPriceFormat(decimal number)
        {
            string priceFormatString;
            if (HLConfigManager.Configurations.CheckoutConfiguration.UseCommaWithoutDecimalFormat)
            {
                if (HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal)
                {
                    priceFormatString = number.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                }
                else
                {
                    if (number == 0)
                    {
                        return "0,000";
                    }

                    priceFormatString = number.ToString("#,###", CultureInfo.GetCultureInfo("en-US"));
                }
            }
            else
            {
                priceFormatString = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                    ? "{0:N2}"
                    : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
            }

            return priceFormatString;
        }
    }
}