// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscountRulesTests.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Discount rules test class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Ordering.Test.Helpers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using HL.Order.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Discount rules test class.
    /// </summary>
    [TestClass]
    public class DiscountRulesTests
    {     
        /// <summary>
        /// Perform discount range values for Brasil with 50% of discount
        /// </summary>
        [TestMethod]
        public void PerformDiscountRangeRules_BrWithDiscount50()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(Distributor, Locale);
            var ruleEngine = new Ordering.Rules.Discount.pt_BR.DiscountRules();

            // Getting a result.
            var result = ruleEngine.PerformDiscountRangeRules(brazilShoppingCart, Locale, 50.0M);

            // Asserts.
            Assert.AreEqual(result, string.Empty, "Discount range should be empty for a DS with 50% of discount.");
        }

        /// <summary>
        /// Perform discount range values for Brasil with 35% of discount
        /// and Volume points equal to 1000
        /// </summary>
        [TestMethod]
        public void PerformDiscountRangeRules_BrWithDiscount35VP1000()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                Distributor,
                Locale,
                new OrderTotals_V01
                    {
                        VolumePoints = 1000M,
                        DiscountPercentage = 35M
                    });
            var ruleEngine = new Ordering.Rules.Discount.pt_BR.DiscountRules();

            // Getting a result.
            var result = ruleEngine.PerformDiscountRangeRules(brazilShoppingCart, Locale, 35.0M);

            // Asserts.
            Assert.AreEqual(result, "1000-4000",
                            "Discount range should be 1000-4000 for a DS with 35% of discount and VP >= 1000.");
        }

        /// <summary>
        /// Perform discount range values for Brasil with 35% of discount
        /// and Volume points equal to 500
        /// </summary>
        [TestMethod]
        public void PerformDiscountRangeRules_BrWithDiscount35VP500()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                Distributor,
                Locale,
                new OrderTotals_V01
                {
                    VolumePoints = 500M,
                    DiscountPercentage = 35M
                });
            var ruleEngine = new Ordering.Rules.Discount.pt_BR.DiscountRules();

            // Getting a result.
            var result = ruleEngine.PerformDiscountRangeRules(brazilShoppingCart, Locale, 35.0M);

            // Asserts.
            Assert.AreEqual(result, "500-999",
                            "Discount range should be 500-999 for a DS with 35% of discount and VP < 1000.");
        }

        /// <summary>
        /// Perform discount range values for Brasil with 25% of discount
        /// and Volume points equal to 499
        /// </summary>
        [TestMethod]
        public void PerformDiscountRangeRules_BrWithDiscount25VP499()
        {
            // Used constants.
            const string Distributor = "09550499";
            const string Locale = "pt-BR";

            // Getting a shopping cart for BR.
            var brazilShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                Distributor,
                Locale,
                new OrderTotals_V01
                {
                    VolumePoints = 499M,
                    DiscountPercentage = 25M
                });
            var ruleEngine = new Ordering.Rules.Discount.pt_BR.DiscountRules();

            // Getting a result.
            var result = ruleEngine.PerformDiscountRangeRules(brazilShoppingCart, Locale, 25.0M);

            // Asserts.
            Assert.AreEqual(result, "0-499",
                            "Discount range should be 0-499 for a DS with 25% of discount and VP < 500.");
        }
    }
}
