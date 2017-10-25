using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;

namespace MyHerbalife3.Ordering.Providers
{
    public class EmailHelperLoader : IEmailHelperLoader
    {
        public DateTime GetCountryTimeNow()
        {
            return EmailHelper.GetCountryTimeNow();
        }

        public DistributorOrderConfirmation GetEmailFromOrder(Order_V01 order, MyHLShoppingCart cartInfo, string locale, string pickupLocation, ShippingInfo deliveryInfo)
        {
            return EmailHelper.GetEmailFromOrder(order, cartInfo, locale, pickupLocation, deliveryInfo);
        }

        public DistributorOrderConfirmation GetEmailFromOrder_V02(Order_V02 order, MyHLShoppingCart cartInfo, string locale)
        {
            return EmailHelper.GetEmailFromOrder_V02(order, cartInfo, locale);
        }

        public void SendEmail(MyHLShoppingCart shoppingCart, Order_V01 order)
        {
            EmailHelper.SendEmail(shoppingCart, order);
        }
    }
}
