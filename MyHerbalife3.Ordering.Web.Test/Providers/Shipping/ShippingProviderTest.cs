using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingChinaSvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Test.Providers.Shipping
{
    [TestClass]
    public class ShippingProviderTest
    {
        [TestClass]
        public class GetMapScriptMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");

                HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));
                HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("CN175803"), new string[0]);
            }

            public IShippingProvider GetTarget(string script, ServiceResponseStatusType responseStatus = ServiceResponseStatusType.Success)
            {
                var target = ShippingProvider.GetShippingProvider(System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(3, 2));

                var chinaShippingProxy = Substitute.For<IChinaShipping>();

                //chinaShippingProxy.GetBaiduMapJavascript(Arg.Any<GetBaiduMapJavascriptRequest_V01>()).Returns(new GetBaiduMapJavascriptResponse_V01()
                //{
                //    Status = responseStatus,
                //    Script = script,
                //});

                target.ChinaShippingServiceProxy = chinaShippingProxy;

                return target;
            }

            [TestMethod]
            public void BaiduMap_HealthyRequest_ScriptReturned()
            {
                var target = GetTarget("function(){ window.BMap_loadScriptTime = (new Date).getTime(); }; ");

                var result = target.GetMapScript(ShippingMapType.Baidu);

                Assert.IsTrue(!string.IsNullOrEmpty(result));
            }

            [TestMethod]
            public void BaiduMap_FailedRequest_EmptyScriptReturned()
            {
                var target = GetTarget("", ServiceResponseStatusType.Failure);

                var result = target.GetMapScript(ShippingMapType.Baidu);

                Assert.IsTrue(string.IsNullOrEmpty(result));
            }

            [TestMethod]
            public void NonBaiduMap_HealthyRequest_EmptyScriptReturned()
            {
                var target = GetTarget("");

                var result = target.GetMapScript(0);

                Assert.IsTrue(string.IsNullOrEmpty(result));
            }
        }
    }
}
