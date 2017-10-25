using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Test.ProviderTests
{
    [TestClass]
    public class MyHLShoppingCartTests
    {
        private string distributorStaff;
        private string locale_enUS;

        [TestInitialize]
        public void Initialize()
        {
            distributorStaff = "STAFF";
            locale_enUS = "en-US";
        }

        [TestMethod]
        public void AdditionalInfoHelper_StaffUSLocale_HappyPath()
        {
            // Arrange
            int id = 0;
            int quantity = 1;
            string sku = "2792";

            var shoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorStaff, locale_enUS);
            var cartItem = ShoppingCartItemHelper.GetShoppingCartItem(id, quantity, sku);

            // Act
            shoppingCart.AdditionalInfoHelper(locale_enUS, cartItem);

            // Assert
            Assert.IsNotNull(cartItem.ParentCat, "Test failed with values: distributorid-{0}, locale-{1}, sku-{2}", 
                distributorStaff, locale_enUS, sku );
        }

        [TestMethod]
        public void AdditionalInfoHelper_StaffUSLocale_SKUNotFound()
        {
            // Arrange
            int id = 0;
            int quantity = 1;
            string sku = "QQQQ";

            var shoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorStaff, locale_enUS);
            var cartItem = ShoppingCartItemHelper.GetShoppingCartItem(id, quantity, sku);

            // Act
            shoppingCart.AdditionalInfoHelper(locale_enUS, cartItem);

            // Assert
            Assert.IsNull(cartItem.ParentCat, "Test failed with values: distributorid-{0}, locale-{1}, sku-{2}",
                distributorStaff, locale_enUS, sku);
        }
    }
}
