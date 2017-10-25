// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SkuLimitationsRulesTest.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//  Tests for sku limitations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Shared.Providers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using HL.Catalog.ValueObjects;
    using HL.Common.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;

    /// <summary>
    ///This is a test class for SKULimitationsRulesTest and is intended
    ///to contain all SKULimitationsRulesTest Unit Tests
    ///</summary>
    [TestClass]
    public class SkuLimitationsRulesTest
    {
        #region Public Methods and Operators
         
        [TestMethod]
        public void ProcessCartTest_Global_Fail()
        {
            var testSettings = new OrderingTestSettings("en-US", "1111111111");
            var target = new Ordering.Rules.SKULimitations.Global.SKULimitationsRules();
            
            var usCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor,
                testSettings.Locale,
                null,
                null,
                false,
                null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "S203"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "3107"),
                    },
                OrderCategoryType.ETO);

            var totalItems = usCart.CartItems.Count;

            target.ProcessCart(usCart, ShoppingCartRuleReason.CartRetrieved);
            Assert.AreEqual(
                totalItems,
                usCart.ShoppingCartItems.Count(),
                "you can not add TodayMagazine SKUs,Otherwise this will be deleted from your cart.");
        }

        [TestMethod]
        public void ProcessCartTest_SamCam_Fail()
        {
            var testSettings = new OrderingTestSettings("es-SV", "1111111111");
            var target = new Ordering.Rules.SKULimitations.SamCam.SKULimitationsRules();

            var myHlShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor,
                testSettings.Locale,
                null,
                null,
                false,
                null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 15, "0141"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0142"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0143"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0146"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2638"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0242"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0105"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0106"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0006"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0065"),
                    },
                OrderCategoryType.ETO);
            
            // Adding a new item.
            MyHLShoppingCartGenerator.PrepareAddToCart(myHlShoppingCart, ShoppingCartItemHelper.GetCartItem(1, 15, "0122"));

            var totalItems = myHlShoppingCart.CartItems.Count;
            target.ProcessCart(myHlShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            Assert.AreEqual(
                totalItems,
                myHlShoppingCart.ShoppingCartItems.Count,
                "you can not add TodayMagazine SKUs,Otherwise this will be deleted from your cart.");
        }

        [TestMethod]
        public void PerformRulesTest_Uruguay_Fail()
        {
            var testSettings = new OrderingTestSettings("es-UY", "1111111111");
            var target = new Ordering.Rules.SKULimitations.SamCam.SKULimitationsRules();
            
            var myHlShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor,
                testSettings.Locale,
                null,
                null,
                false,
                null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 16, "0141"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0142"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0143"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0146"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2638"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0242"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0105"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0106"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0102"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2864"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "6240"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "6926"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "8602"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "8601"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "5451"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "7123")
                    },
                OrderCategoryType.RSO);

            // Adding a new item.
            MyHLShoppingCartGenerator.PrepareAddToCart(myHlShoppingCart, ShoppingCartItemHelper.GetCartItem(1, 16, "2511"));

            target.ProcessCart(myHlShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            Assert.IsTrue(myHlShoppingCart.RuleResults.Count >  0, "Max NTS lines for Uruguay should by 15");
        }

        [TestMethod]
        public void PerformRulesTest_Nicaragua_Fail()
        {
            var testSettings = new OrderingTestSettings("es-NI", "1111111111");
            var target = new Ordering.Rules.SKULimitations.SamCam.SKULimitationsRules();
            
            var myHlShoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor,
                testSettings.Locale,
                null,
                null,
                false,
                null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 16, "2512"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0142"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0143"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0146"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2638"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0242"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0105"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0106"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0102"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2864"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "6240"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "6926"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "8602"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "8601"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "5451"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "7123"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "0865"),
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 10, "2514")
                    },
                OrderCategoryType.RSO);

            // Adding a new item.
            MyHLShoppingCartGenerator.PrepareAddToCart(myHlShoppingCart, ShoppingCartItemHelper.GetCartItem(1, 16, "2511"));

            target.ProcessCart(myHlShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            Assert.IsTrue(myHlShoppingCart.RuleResults.Count > 0, "Max NTS lines for Nicaragua should by 18");
        }

        [TestMethod]
        public void SKULimitationsRules_ProcessCartTest_HN_Pass()
        {
            var settings = new OrderingTestSettings("es-HN", "1111111111");

            // Test should pass up to 9 sku in cart and adding one more
            var skus = new List<string> { "0141", "0142", "0143", "0146", "2638", "0242", "2864", "0105", "0106", "0006", "0065", "3150" };
            var linesInShoppingCart = 9;
            var qty = 5;
            var target = new Ordering.Rules.SKULimitations.SamCam.SKULimitationsRules();

            var hnCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, linesInShoppingCart, qty),
                OrderCategoryType.RSO);

            MyHLShoppingCartGenerator.PrepareAddToCart(hnCart, ShoppingCartItemHelper.GetCartItem(1, 1, "0102"));

            target.ProcessCart(hnCart, ShoppingCartRuleReason.CartItemsBeingAdded);
            var ruleResult = hnCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("SkuLimitation Rules"));
            var passed = ruleResult == null || ruleResult.Result != RulesResult.Failure;
            Assert.AreEqual(true, passed);
        }

        [TestMethod]
        public void SKULimitationsRules_ProcessCartTest_HN_Fail()
        {
            var settings = new OrderingTestSettings("es-HN", "1111111111");

            // Maximum sku number for HN should be 10
            var skus = new List<string> { "0141", "0142", "0143", "0146", "2638", "0242", "2864", "0105", "0106", "0006", "0065", "3150" };
            var linesInShoppingCart = 10;
            var qty = 5;
            var target = new Ordering.Rules.SKULimitations.SamCam.SKULimitationsRules();

            var hnCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, linesInShoppingCart, qty),
                OrderCategoryType.RSO);
            MyHLShoppingCartGenerator.PrepareAddToCart(hnCart, ShoppingCartItemHelper.GetCartItem(1, 1, "0102"));

            target.ProcessCart(hnCart, ShoppingCartRuleReason.CartItemsBeingAdded);
            var ruleResult = hnCart.RuleResults.Where(r => r.RuleName.Equals("SkuLimitation Rules")).FirstOrDefault();
            var failed = ruleResult != null && ruleResult.Result == RulesResult.Failure;
            Assert.AreEqual(true, failed);
        }
        
        #endregion
    }
}