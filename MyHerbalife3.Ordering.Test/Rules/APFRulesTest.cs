using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Web.Security;
using System.Web.SessionState;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Shared.Providers;
using HL.Catalog.ValueObjects;
using HL.Common.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Shared.Security;
using MyHerbalife3.Shared.Common;
using MyHerbalife3.Shared.Distributor;
using MyHerbalife3.Shared.ViewModel;
using RulesEngine = MyHerbalife3.Ordering.Rules;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    /// <summary>
    ///This is a test class for APFRulesTest and is intended
    ///to contain all APFRulesTest Unit Tests
    ///</summary>
    [TestClass]
    public class ApfRulesTest :MyHerbalifeRule
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
            var sessionRoleStore = new SessionRoleStore();
            MyHlRoleProvider.RegisterStore(sessionRoleStore);            
            MyHlMembershipProvider.Register(new MyHerbalifeSettings(), new DistributorModelService(new DistributorProfileLoader()));
        }

        #region Public Methods and Operators
        /// <summary>
        ///A test for ProcessCart for Global
        ///</summary>
        [TestMethod]
        public void ProcessCartTest()
        {
            var testSettings = new OrderingTestSettings("ru-RU", "79039381");
            var target = new RulesEngine.APF.Global.APFRules();

            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
               testSettings.Distributor, testSettings.Locale, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 1, "1248", testSettings.Locale)
                    }, OrderCategoryType.RSO);

            MyHLShoppingCartGenerator.PrepareAddToCart(cart, ShoppingCartItemHelper.GetCartItem(1, 1, "1248"));
            cart.ShoppingCartID = 1;

            List<ShoppingCartRuleResult> result = target.ProcessCart(
                cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(
                    RulesResult.Failure,
                    result[0].Result,
                    "Please complete your annual fee purchase before adding other products to cart.");

            }
            else
                Assert.Fail("May be APF is not due for this Distributor.");
        }

        #endregion

    }
}