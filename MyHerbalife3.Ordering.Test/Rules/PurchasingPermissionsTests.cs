// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PurchasingPermissionsTests.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Purchasing permissions rules test class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Shared.Common;
using MyHerbalife3.Shared.Distributor;
using MyHerbalife3.Shared.Providers;
using MyHerbalife3.Shared.Security;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Globalization;
    using System.Collections.Generic;

    /// <summary>
    /// Purchasing permissions rules test class.
    /// </summary>
    [TestClass]
    public class PurchasingPermissionsTests
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
        /// <summary>
        /// Perform can purchase validation for Singapore for foreign DS.
        /// </summary>
        [TestMethod]
        public void CanPurchase_SGWithForeignDistributor_True()
        {
            // Used constants.
            const string Distributor = "09550499";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-SG");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.en_SG.PurchasingPermissionRules();

            // Getting an online distributor.
          //  var ods = OnlineDistributorHelper.GetOnlineDistributor(Distributor);

            // Getting a result.
            MembershipUser user = Membership.GetUser(Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            var result = ruleEngine.CanPurchase(Distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Foreign distributor should be able to purchase in Singapore");
        }

        /// <summary>
        /// Perform can purchase validation for Singapore with SNID tin code.
        /// </summary>
        [TestMethod]
        public void CanPurchase_SGWithTinCodeSNID_True()
        {
            // Used constants.
            const string Distributor = "09550499";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-SG");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.en_SG.PurchasingPermissionRules();

            // Getting an online distributor.
            var ods = OnlineDistributorHelper.GetOnlineDistributor(
                Distributor,
                "SN",
                new List<string>
                    {
                        "SNID"
                    });
            MembershipUser user = Membership.GetUser(Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting a result.
            var result = ruleEngine.CanPurchase(Distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Local distributor with SNID tin code should be able to purchase in Singapore");
        }

        /// <summary>
        /// Perform can purchase validation for Argentina with ARTX tin code and ARVT tin codes.
        /// </summary>
        [TestMethod]
        public void CanPurchase_ARWithInvalidTinCodes_False()
        {
            // Used constants.
            const string Distributor = "09550499";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.es_AR.PurchasingPermissionRules();

            // Getting an online distributor.
            var ods = OnlineDistributorHelper.GetOnlineDistributor(
                Distributor,
                "AR",
                new List<string>
                    {
                        "ARTX",
                        "ARVT"
                    });
            MembershipUser user = Membership.GetUser(Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting a result.
            var result = ruleEngine.CanPurchase(Distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Local distributor with ARTX and ARVT tin codes should not be able to purchase in Argentina");
        }

        /// <summary>
        /// Perform can purchase validation for Argentina with valid tin codes.
        /// </summary>
        [TestMethod]
        public void CanPurchase_ARWithValidTinCodes_True()
        {
            // Used constants.
            const string Distributor = "09550499";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.es_AR.PurchasingPermissionRules();

            // Getting an online distributor.
            var ods = OnlineDistributorHelper.GetOnlineDistributor(
                Distributor,
                "AR",
                new List<string>
                    {
                        "ARTX",
                        "CUIT"
                    });
            MembershipUser user = Membership.GetUser(Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting a result.
            var result = ruleEngine.CanPurchase(Distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Local distributor with ARTX and CUIT tin codes should be able to purchase in Argentina");
        }

        [TestMethod]
        public void CanPurchase_With_MO_ForeignDS_True()
        {
            // Used constants.
            const string distributor = "WEBTEST1";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zh-MO");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.zh_MO.PurchasingPermissionRules();

            // Getting an online distributor.
         //   var ods = OnlineDistributorHelper.GetOnlineDistributor(distributor);
            MembershipUser user = Membership.GetUser(distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting a result.
            var result = ruleEngine.CanPurchase(distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Foreign DS should be able to purchase,No restrictions.");
        }

        [TestMethod]
        public void CanPurchase_With_Macau_DS_True()
        {
            // Used constants.
            const string distributor = "24205277";
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zh-MO");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.zh_MO.PurchasingPermissionRules();

            // Getting an online distributor.
        //    var ods = OnlineDistributorHelper.GetOnlineDistributor(distributor);
            MembershipUser user = Membership.GetUser(distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting a result.
            var result = ruleEngine.CanPurchase(distributor,"SG");

            // Asserts.
            Assert.AreEqual(result, true, "Macau DS with MCID should be able to purchase,No restrictions.");
        }

        #region VN Purchasing Permision Rule

        [TestMethod]
        public void CanPurchase_VN_ForeignDistributor()
        {
            var settings = new OrderingTestSettings("vi-VN", "webtest1");
            var target = new Ordering.Rules.PurchasingPermissions.vi_VN.PurchasingPermissionRules();
            var ods = OnlineDistributorHelper.GetOnlineDistributor(settings.Distributor, "US", null);
            MembershipUser user = Membership.GetUser(settings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            var result = target.CanPurchase(settings.Distributor,"VN");
            Assert.AreEqual(false, result, "COP not equal to Vietnam should not be able to buy");
        }

        [TestMethod]
        public void CanPurchase_VN_WithNotValitTin()
        {
            var settings = new OrderingTestSettings("vi-VN", "webtest1");
            var target = new Ordering.Rules.PurchasingPermissions.vi_VN.PurchasingPermissionRules();
            var ods = OnlineDistributorHelper.GetOnlineDistributor(settings.Distributor, "VN", new List<string>());
            MembershipUser user = Membership.GetUser(settings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            var result = target.CanPurchase(settings.Distributor,"SG");
            Assert.AreEqual(false, result, "Local distributor with no valid Tin should not be able to buy");
        }

        [TestMethod]
        public void CanPurchase_VN_WithValitTin()
        {
            var settings = new OrderingTestSettings("vi-VN", "webtest1");
            var target = new Ordering.Rules.PurchasingPermissions.vi_VN.PurchasingPermissionRules();
            var ods = OnlineDistributorHelper.GetOnlineDistributor(settings.Distributor, "VN", new List<string> { "VNID" });
            MembershipUser user = Membership.GetUser(settings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            var result = target.CanPurchase(settings.Distributor,"SG");
            Assert.AreEqual(false, result, "Local distributor with valid Tin should be able to buy");
        }

        #endregion


        #region TH purchasing Limits
        [TestMethod]
        public void CanPurchase_With_Thailand_DS_True()
        {
            var testSettings = new OrderingTestSettings("th-TH", "06304245");

            // Rules engine.
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.th_TH.PurchasingPermissionRules();
            MembershipUser user = Membership.GetUser(testSettings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // Getting an online distributor.
          //  var ods = OnlineDistributorHelper.GetOnlineDistributor(testSettings.Distributor);
         //   ods.Value.ProcessingCountry = HL.Common.ValueObjects.CountryType.TH;
            // Getting a result.
            var result = ruleEngine.CanPurchase(testSettings.Distributor,"TH");

            // Asserts.
            Assert.AreEqual(result, true
                , "thailand DS with THID should be able to purchase,No restrictions.");
        }
        #endregion

        #region AR purchasing Limits
        [TestMethod]
        public void CanPurchase_With_Brazil_DS_NonZeroVolume()
        {
            var testSettings = new OrderingTestSettings("es-AR", "WEBTESTBR");
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.es_AR.PurchasingPermissionRules();
            MembershipUser user = Membership.GetUser(testSettings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
             var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
                testSettings.Distributor, testSettings.Locale, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 5, "0141", testSettings.Locale)

                    }, OrderCategoryType.ETO);

            cart.ShoppingCartID = 1;
            var result = ruleEngine.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Can't purchase.");

            }
            else
                Assert.Fail("May be the volume poient of sku 0141 is 0.");

        }
        [TestMethod]
        public void CanPurchase_With_Brazil_DS_ZeroVolume()
        {
            var testSettings = new OrderingTestSettings("es-AR", "WEBTESTBR");
            var ruleEngine = new Ordering.Rules.PurchasingPermissions.es_AR.PurchasingPermissionRules();
            MembershipUser user = Membership.GetUser(testSettings.Distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
               testSettings.Distributor, testSettings.Locale, "", "", false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 5, "5451", testSettings.Locale)

                    }, OrderCategoryType.ETO);

            cart.ShoppingCartID = 1;
            var result = ruleEngine.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(0,
                                result[0].Messages.Count);

            }
            else
                Assert.Fail("May be the volume poient of sku 5451 is not 0.");

        }
        #endregion
    }
}
