// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscountRulesTests.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Discount rules test class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using HL.Order.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Discount rules test class.
    /// </summary>
    [TestClass]
    public class OrderManagementRulesTests
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

        #region TH

        /// <summary>
        /// Performs the order management rules.
        /// This Rule change the orderMonth from Bud year to Greg year format
        /// </summary>
        [TestMethod]
        public void PerformOrderManagementRules()
        {
            const string Distributor = "06304245";
            const string Locale = "th-TH";
            var cart  = MyHLShoppingCartGenerator.GetBasicShoppingCart(Distributor, Locale);
            Order_V01 order = OrderHelper.GetOrder(Locale, Distributor);
            order.OrderMonth = "5607";
            var ruleEngine = new Ordering.Rules.OrderManagement.th_TH.OrderManagementRules();
            // Getting a result.
            ruleEngine.PerformOrderManagementRules(cart, order, Locale, OrderManagementRuleReason.Unknown); 
            // Asserts.
            Assert.AreEqual("1307", order.OrderMonth, "Order Management Rule from TH is not working");
        }
        #endregion
    }
}
