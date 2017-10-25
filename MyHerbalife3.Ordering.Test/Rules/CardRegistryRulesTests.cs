using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Test.Helpers;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers.RulesManagement;

namespace MyHerbalife3.Ordering.Test.Rules
{
    [TestClass]
    public class CardRegistryRulesTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void ProcessCartTest()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.CardRegistry.Global.CardRegistryRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                brazilShoppingCart, ShoppingCartRuleReason.CartCreated);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Created");
        }

        [TestMethod]
        public void ProcessCartTest_CartItemsBeingAdded()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.CardRegistry.Global.CardRegistryRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                brazilShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Added.");
        }

        #endregion
    }
}