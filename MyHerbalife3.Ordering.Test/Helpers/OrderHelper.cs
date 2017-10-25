using HL.Order.ValueObjects;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Test.Helpers
{
    internal static class OrderHelper
    {

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="distributorID">The distributor ID.</param>
        /// <returns></returns>
        public static Order_V01 GetOrder(string locale, string distributorID)
        {
            var shoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorID, locale);
            var order = (Order_V01)OrderCreationHelper.CreateOrderObject(shoppingCart);
            OrderCreationHelper.FillOrderInfo(order, shoppingCart);
            return order;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <returns></returns>
        public static Order_V01 GetOrder(string locale, string distributorID, MyHLShoppingCart shoppingCart)
        {
            var order = (Order_V01)OrderCreationHelper.CreateOrderObject(shoppingCart);
            OrderCreationHelper.FillOrderInfo(order, shoppingCart);
            return order;
        }
    }
}
