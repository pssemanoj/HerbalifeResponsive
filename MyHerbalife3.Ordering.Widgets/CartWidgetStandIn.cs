using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets
{
    public class CartWidgetStandIn : ICartWidgetSource
    {
        #region ICartWidgetSource Members

        public CartWidgetModel GetCartWidget(string id, string countryCode, string locale)
        {
            return new CartWidgetModel
            {
                Id = 111,
                Name = "Test Cart",
                Quantity = 9,
                Subtotal = 190.00m,
                VolumePoints = 99.25m
            };
        }

        public CartWidgetModel AddToCart(CartWidgetModel cartWidgetModel, string id, string countryCode, string locale)
        {
            return new CartWidgetModel
            {
                Id = 111,
                Name = "Test Cart",
                Quantity = 10,
                Subtotal = 195.00m,
                VolumePoints = 100.25m
            };
        }

        #endregion
    }
}