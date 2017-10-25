using MyHerbalife3.Ordering.Test.Helpers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using HL.Common.ValueObjects;

    [TestClass]
    public class CartManagementRulesTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void ProcessCartManagementRulesTest()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "es-AR";

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
            var ruleEngine = new Ordering.Rules.CartManagement.es_AR.CartManagementRules();

            // Getting a result.
            var result = ruleEngine.ProcessCartManagementRules(brazilShoppingCart);

            // Asserts.
            Assert.IsNotNull(result, "Processed Cart Management Rules.");
        }

        #endregion
    }
}