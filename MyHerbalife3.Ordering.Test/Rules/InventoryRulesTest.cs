// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryRulesTest.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//  Tests for Inventory Rules.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using MyHerbalife3.Ordering.Providers;
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
    /// This is a test class for InventoryRulesTest and is intended
    /// to contain all InventoryRulesTest Unit Tests
    /// </summary>
    [TestClass]
    public class InventoryRulesTest
    {
        [TestMethod]
        public void InventoryRules_Global_AvailableForBackOrder_PickUp_LorAType_Test_Passed()
        {
            var testSettings = new OrderingTestSettings("en-US", "webtest1");
            var target = new Ordering.Rules.Inventory.Global.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "5918")

                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsAdded);
            if (result.Any() && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(RulesResult.Unknown, result[0].Result);
            }
            else if (result.Any() && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, result[0].Result);
            }
        }

        [TestMethod]
        public void InventoryRules_Global_AvailableForBackOrder_PickUp_Ptype_Test_Passed()
        {
            var testSettings = new OrderingTestSettings("en-US", "webtest1");
            var target = new Ordering.Rules.Inventory.Global.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "P9", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "6705")

                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsAdded);
            if (result.Any() && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(RulesResult.Unknown, result[0].Result);
            }
            else if (result.Any() && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, result[0].Result);
            }
        }

        [TestMethod]
        public void InventoryRules_Global_AvailableForBackOrder_Shipping_LorAType_Test_Passed()
        {
            var testSettings = new OrderingTestSettings("en-US", "webtest1");
            var target = new Ordering.Rules.Inventory.Global.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "BK", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "5918")

                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsAdded);
            if (result.Any() && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(RulesResult.Unknown, result[0].Result);
            }
            else if (result.Any() && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, result[0].Result);
            }
        }

        [TestMethod]
        public void InventoryRules_Global_AvailableForBackOrder_Shipping_Ptype_Test_Passed()
        {
            var testSettings = new OrderingTestSettings("en-US", "webtest1");
            var target = new Ordering.Rules.Inventory.Global.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "P9", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "5602")

                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsAdded);
            if (result.Any() && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(RulesResult.Unknown, result[0].Result);
            }
            else if (result.Any() && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, result[0].Result);
            }
        }

        [TestMethod]
        public void PerformBackorderRules_Inventory_fr_FR()
        {
            var testSettings = new OrderingTestSettings("fr-FR", "webtest1");
            var target = new Ordering.Rules.Inventory.fr_FR.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "EXX", "F8", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "0141")

                    }, OrderCategoryType.ETO);
            CatalogItem item = CatalogProvider.GetCatalogItem("0141", "FR");
            int PreviousItemCount = cart.CartItems.Count();
            target.PerformBackorderRules(cart, item);
            int CurrentItemCount = cart.CartItems.Count();
            if (!cart.RuleResults.Any())
            {
                Assert.AreEqual(PreviousItemCount, CurrentItemCount);
            }
            else if (cart.RuleResults.Any() && cart.RuleResults[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, cart.RuleResults[0].Result);
            }
        }

        [TestMethod]
        public void PerformBackorderRules_Inventory_pt_BR()
        {
            var testSettings = new OrderingTestSettings("pt-BR", "webtest1");
            var target = new Ordering.Rules.Inventory.pt_BR.InventoryRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "RSP", "BM", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "0141")

                    }, OrderCategoryType.RSO);
            CatalogItem item = CatalogProvider.GetCatalogItem("0141", "BR");
            int PreviousItemsCount = cart.CartItems.Count();
            target.PerformBackorderRules(cart, item);
            int NowItemsCount = cart.CartItems.Count();
            if (!cart.RuleResults.Any())
            {

                Assert.AreEqual(PreviousItemsCount, NowItemsCount);
            }
            else if (cart.RuleResults.Any() && cart.RuleResults[0].Messages.Count > 0)
            {
                Assert.AreEqual(RulesResult.Failure, cart.RuleResults[0].Result);
            }
        }
    }
}