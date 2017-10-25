using System.Security.Principal;
using System.Web.Security;
using HL.Distributor.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Shared.Common;
using MyHerbalife3.Shared.Distributor;
using MyHerbalife3.Shared.Providers;
using MyHerbalife3.Shared.Security;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.SessionState;
    using HL.Catalog.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PurchasingLimitsTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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

        [TestMethod]
      
        public void PurchasingLimitsTest_IN()
        {
            string distributor = "1111111111";
            setup(distributor, "en-IN");

            // kick this off to setup proper values
            var plr = new Ordering.Rules.PurchasingLimits.en_IN.PurchasingLimitRules();
            plr.GetPurchasingLimits(distributor, string.Empty);

            var ruleEngine = new Ordering.Rules.PurchasingLimits.en_IN.PurchasingLimitRules();
            ruleEngine.PurchasingLimitsAreExceeded(distributor);
        }

        [TestMethod]
     
        public void PurchasingLimitsTest_PY()
        {
            string distributor = "32067971";
            setup(distributor, "es-PY");
            MembershipUser user = Membership.GetUser(distributor);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            // kick this off to setup proper values
            var plr = new Ordering.Rules.PurchasingLimits.es_PY.PurchasingLimitRules();
            bool result = plr.DistributorIsExemptFromPurchasingLimits(distributor);
            bool expected = true;
            Assert.AreEqual(result, expected);

        }

        private void setup(string DsId, string locale)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(locale);

            string currentDirectory = Environment.CurrentDirectory;
            int idx = currentDirectory.IndexOf("TestResults");
            if (idx != -1)
            {
                currentDirectory = currentDirectory.Substring(0, idx);
                AppDomain.CurrentDomain.SetData("APPBASE", currentDirectory + "MyHerbalife3.Ordering.Test");
            }

            //DistributorProvider.ReloadDistributor(OnlineDistributorHelper.GetOnlineDistributor(DsId), 0);
        }

        [TestMethod]
        public void PurchasingLimitsTest_PH_ForeignDistributor_Fail()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-PH");
            var target = new Ordering.Rules.PurchasingLimits.en_PH.PurchasingLimitRules();
            string DsId = "WEBTEST1";
            string Local = "en-PH";
            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            MembershipUser user = Membership.GetUser(DsId);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            HttpRuntime.Cache.Insert("DISTR_" + DsId, distributor);
            var myHlShoppingCart = ShoppingCartProvider.GetShoppingCart(DsId, Local, false);
            myHlShoppingCart.CurrentItems = new ShoppingCartItemList();
            MyHLShoppingCartGenerator.PrepareAddToCart(myHlShoppingCart, ShoppingCartItemHelper.GetCartItem(1, 80, "0065"));
            var result = target.ProcessCart(myHlShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual(result[0].Messages[0],
                                "Item SKU:0065 has not been added to the cart since by adding that into the cart, you exceeded your volume points  limit.");

            }
            else
                Assert.Fail("Test Failed");
        }
        [TestMethod]
        public void PurchasingLimitsTest_PH_Distributor()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-PH");
            var target = new Ordering.Rules.PurchasingLimits.en_PH.PurchasingLimitRules();
            string DsId = "37170799";
            string Local = "en-PH";

            var distributor = DistributorOrderingProfileProvider.GetProfile(DsId, Local.Substring(3));

            MembershipUser user = Membership.GetUser(DsId);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            HttpRuntime.Cache.Insert("DISTR_" + DsId, distributor);
            var myHlShoppingCart = ShoppingCartProvider.GetShoppingCart(DsId, Local, false);
            myHlShoppingCart.CurrentItems = new ShoppingCartItemList();
            MyHLShoppingCartGenerator.PrepareAddToCart(myHlShoppingCart, ShoppingCartItemHelper.GetCartItem(1, 80, "0065"));
            var result = target.ProcessCart(myHlShoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count == 0)
            {
                Assert.AreEqual(RulesResult.Unknown,result[0].Result);
              

            }
            else
                Assert.Fail("Test Failed");
        }

        #region VN Purchasing Limits 

        [TestMethod]
        public void DistributorIsExemptFromPurchasingLimits_VN_WithValidTin()
        {
            var settings = new OrderingTestSettings("vi-VN", "webtest1");
            var ods = OnlineDistributorHelper.GetOnlineDistributor(settings.Distributor, "VN", new List<string> { "VNID" });

            // Adding mandatory notes.
            //DistributorNotes = new List<DistributorNote_V01>()
            //    {
            //        new DistributorNote_V01()
            //            {
            //                CreationDate = DateTime.Now,
            //                EffectiveDate = DateTime.Now.AddDays(-1),
            //                Description = "Test note",
            //                NoteCode = "VNTRN"
            //            }
            //    };

            var target = new Ordering.Rules.PurchasingLimits.vi_VN.PurchasingLimitRules();
            MembershipUser user = Membership.GetUser("webtest1");
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            HttpRuntime.Cache.Insert("DISTR_" + settings.Distributor, ods);
            
            var result = target.DistributorIsExemptFromPurchasingLimits(settings.Distributor);
            Assert.AreEqual(false, result, "Local distributor with valid Tin is exempt from limits");
        }

        [TestMethod]
        public void DistributorIsExemptFromPurchasingLimits_VN_WithoutValidTin()
        {
            var settings = new OrderingTestSettings("vi-VN", "webtest1");
            var ods = OnlineDistributorHelper.GetOnlineDistributor(settings.Distributor, "VN", new List<string>());
            var target = new Ordering.Rules.PurchasingLimits.vi_VN.PurchasingLimitRules();
            MembershipUser user = Membership.GetUser("webtest1");
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
            HttpRuntime.Cache.Insert("DISTR_" + settings.Distributor, ods);
            var result = target.DistributorIsExemptFromPurchasingLimits(settings.Distributor);
            Assert.AreEqual(false, result, "Local distributor without valid Tin is not exempt from limits");
        }

        #endregion
    }
}
