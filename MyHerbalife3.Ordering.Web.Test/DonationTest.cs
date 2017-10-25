using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Web.Test.Providers;
using NSubstitute;
using NSubstitute.Core;
using System.Web.UI;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Test.Rules
{
    /// <summary>
    /// Summary description for PromotionRuleTest
    /// </summary>
    [TestClass]
    public class DonationTest
    {
        public DonationTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        [TestInitialize]
        public void ChinaCulture()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
        }
        [TestMethod]
        public void ProcessDonation_Test()
        {
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "3", "888",
                                                                                 new ShippingAddress_V02());

            cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                };

            var order = new Order_V01();
            order.DistributorID = "CN640521";

            var orderitem1 = new OnlineOrderItem()
            {
                Quantity = 1,
                SKU = "1316",
                Description = "Test",
                RetailPrice = 0,
                IsPromo = false,

            };
            MyHerbalife3.Ordering.Providers.Shipping.DeliveryOption deliveryOption = new MyHerbalife3.Ordering.Providers.Shipping.DeliveryOption();
            order.CountryOfProcessing = "CN";
            order.OrderItems = new OrderItems { orderitem1 };
            order.Shipment = new ShippingInfo_V01
            {
                Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                {
                    City = "眉山市",
                    Country = "cn"
                },
                ShippingMethodID = "22",
                WarehouseCode = "888"
            };

            var ordertotal = new OrderTotals_V02();
            GetQuotes(order, ordertotal, false, out ordertotal);
            cart.Totals = ordertotal;
        }
        private void GetQuotes(Order order, OrderTotals total, bool calcFreight, out OrderTotals_V02 totals)
        {
            var shoppingCartLaoder = Substitute.For<IShoppingCartProviderLoader>();
            shoppingCartLaoder.GetQuote(order, total, calcFreight).Returns(GetQuote(order, out totals, calcFreight));

            decimal AmountDues = totals.AmountDue - totals.ItemsTotal;
            Assert.AreEqual(AmountDues, 60);

        }

        private OrderTotals GetQuote(Order order, out OrderTotals_V02 total, bool calcFreight)
        {
            total = new OrderTotals_V02();
            var orderTotalsV02 = total;
            orderTotalsV02.Donation = 60;
            orderTotalsV02.AmountDue = 329 + orderTotalsV02.Donation;
            total.ItemsTotal = 329M;
            OrderFreight orderfreight = new OrderFreight
            {
                FreightCharge = 0.0M,
            };
            total.OrderFreight = orderfreight;
            total.DiscountAmount = 0;
            total = orderTotalsV02;
            return total;
        }
        
    }
}
