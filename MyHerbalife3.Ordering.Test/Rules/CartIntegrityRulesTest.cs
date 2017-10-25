// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CartIntegrityRulesTest.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//  Tests for Cart Integrity Rules.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Shared.Providers;
using HL.Catalog.ValueObjects;
using HL.Common.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    /// <summary>
    /// This is a test class for CartIntegrityRulesTest and is intended
    /// to contain all CartIntegrityRulesTest Unit Tests
    /// </summary>
    [TestClass]
    public class CartIntegrityRulesTest : ShoppingCart_V01
    {
        [TestInitialize]
        public void Initialize()
        {
            var httpRequest = new HttpRequest(
                AppDomain.CurrentDomain.BaseDirectory, "http://www.local.myherbalife.com/Default.aspx", "");
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);

            var httpContext = new HttpContext(httpRequest, httpResponce);

            var sessionContainer = new HttpSessionStateContainer(
                "id",
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(),
                10,
                true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc,
                false);

            httpContext.Items["AspSession"] =
                typeof(HttpSessionState).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    CallingConventions.Standard,
                    new[] { typeof(HttpSessionStateContainer) },
                    null).Invoke(new object[] { sessionContainer });

            HttpContext.Current = httpContext;
        }

        [TestMethod]
        public void ProcessCartTestGlobal_DuplicateSKUs_Fail()
        {
            string DsId = "1111111111";
            string Local = "en-IN";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.Global.CartIntegrityRules();
            
            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));
                        
            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1248", Local),
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1248", Local)
                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Duplicate sku found. DS: 1111111111, CART: 0, SKU 1248 removed from cart.");

            }
            else
                Assert.Fail("SKU 1248 is dupliacte.it should be removed from the cart");
        }

        [TestMethod]
        public void ProcessCartTestGlobal_InvalidQuantities_Fail()
        {
            var testSettings = new OrderingTestSettings("en-IN", "1111111111");
            var target = new Ordering.Rules.CartIntegrity.Global.CartIntegrityRules();

            var distributor = OnlineDistributorHelper.GetOnlineDistributor(testSettings.Distributor);
            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, -1, "1248", testSettings.Locale),
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1247", testSettings.Locale)
                    }, OrderCategoryType.ETO);

            cart.ShoppingCartID = 1;

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Invalid sku quantity found. DS: 1111111111, CART: 1, SKU 1248 : quantity updated to 1.");
            }
            else
                Assert.Fail("The Sku quantity -1 should be updated to 1.");
        }

        [TestMethod]
        public void ProcessCartTestGlobal_InvalidSKUs_Fail()
        {
            string DsId = "1111111111";
            string Local = "en-IN";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.Global.CartIntegrityRules();

            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));
            
            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1248", Local),
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "0000", Local)
                    }, OrderCategoryType.ETO);

            cart.ShoppingCartID = 1;
            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Invalid sku found. DS: 1111111111, CART: 1, SKU 0000 : removed from cart.");

            }
            else
                Assert.Fail("The Sku 0000 is invalid,it should be removed from cart");
        }

        [TestMethod]
        public void ProcessCartTestNonWebClient_InvalidSKUs_Fail()
        {
            const string DsId = "1111111111";
            const string Local = "en-AU";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.NonWebClient.CartIntegrityRules();
            
            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null,
                new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1248", Local)

                    }, OrderCategoryType.ETO);
            cart.ShoppingCartID = 1;
            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "SKU 1248 is not available.");

            }
            else
                Assert.Fail("May be the SKU 1248 is available for Australia ");
        }

        [TestMethod]
        public void ProcessCartTestNonWebClient_Invalidquantity_Fail()
        {
            const string DsId = "1111111111";
            const string Local = "en-AU";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.NonWebClient.CartIntegrityRules();

            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 200, "0124", Local)

                    }, OrderCategoryType.ETO);

            cart.ShoppingCartID = 1;

            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "The quantity of SKU 0124 is not valid.");

            }
            else
                Assert.Fail("May be the quantity 200 for the sku 0124 is valid for Australia.");
        }

        [TestMethod]
        public void ProcessCartTestel_GR_CheckForHFFSKU_Fail()
        {
            const string DsId = "1111111111";
            const string Local = "el-GR";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.el_GR.CartIntegrityRules();

            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "F692", Local)

                    }, OrderCategoryType.ETO);
            cart.ShoppingCartID = 1;
            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartCreated);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Invalid sku found. DS: 1111111111, CART: 1, SKU F692 : removed from cart.");

            }
            else
                Assert.Fail("Distributor can not add HFF Sku F692.");
        }

        [TestMethod]
        public void ProcessCartTesten_US_CheckForHFFSKU_Fail()
        {
            const string DsId = "1111111111";
            const string Local = "en-US";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.CartIntegrity.en_US.CartIntegrityRules();

            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                DsId, Local, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "F356", Local)

                    }, OrderCategoryType.ETO);

            var result = target.ProcessCart(
                cart, ShoppingCartRuleReason.CartWarehouseCodeChanged);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Invalid sku found. DS: 1111111111, CART: 0, SKU F356 : removed from cart.");

            }
            else
                Assert.Fail("Distributor can not add HFF Sku F356.");
        }
    }
}