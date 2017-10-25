using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IEmailHelperLoader
    {
        void SendEmail(MyHLShoppingCart shoppingCart, Order_V01 order);

        DateTime GetCountryTimeNow();

        DistributorOrderConfirmation GetEmailFromOrder(Order_V01 order, MyHLShoppingCart cartInfo, string locale, string pickupLocation, ShippingInfo deliveryInfo);

        DistributorOrderConfirmation GetEmailFromOrder_V02(Order_V02 order, MyHLShoppingCart cartInfo, string locale);
    }
}
