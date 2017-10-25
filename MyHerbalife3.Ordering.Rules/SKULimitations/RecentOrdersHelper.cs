using MyHerbalife3.Ordering.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.Rules.SKULimitations
{
    public class RecentOrdersHelper
    {
        public DateTime LastRetrieveDateTime { get; set; }
        public List<MyHLShoppingCart> Orders { get; set; }

        public static int ConvertDate(DateTime d)
        {
            return d.Year * 10000 + d.Month * 100 + d.Day;
        }

        public RecentOrdersHelper(string distributor, string locale, DateTime currentLocalTime, int skuQuantityLimitPeriodByDay,int _intstartDate)
        {
            ShoppingCartProvider.ResetInternetShoppingCartsCache(distributor, locale);
            var orders = ShoppingCartProvider.GetInternetShoppingCarts(distributor, locale, 0, 70 * skuQuantityLimitPeriodByDay, true, byOrderCategory: true);
            //int intDayBack = ConvertDate(currentLocalTime.AddDays(-_intstartDate)); // days back, eg 20140812
            int intCurrentDateTime = ConvertDate(currentLocalTime); // today eg 20141112
            Orders = orders.TakeWhile(o => ConvertDate(o.OrderDate) <= intCurrentDateTime
                &&
                ConvertDate(o.OrderDate) >= _intstartDate).ToList<MyHLShoppingCart>();
            LastRetrieveDateTime = currentLocalTime;
        }
    }
}
