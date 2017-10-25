using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Web.Test.Mocks;

namespace MyHerbalife3.Ordering.Web.Test.Controllers
{
    [TestClass]
    public class InvoiceControllerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            if (HttpContext.Current != null)
            {
                return;
            }

            var request = new HttpRequest(string.Empty, "http://localhost/", string.Empty);
            HttpResponse httpResponse;
            using (var responseWriter = new StringWriter())
            {
                httpResponse = new HttpResponse(responseWriter);
            }
            HttpContext.Current = new HttpContext(request, httpResponse);
        }

        private static InvoiceController GetController()
        {
            var controller = new InvoiceController();
            var routeData = new RouteData();
            var mockHttpContext = new MockHttpContext(HttpContext.Current);
            controller.ControllerContext = new ControllerContext(mockHttpContext, routeData, controller);
            return controller;
        }

        [TestMethod]
        public void Index()
        {
            var controller = GetController();
            var result = controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Edit()
        {
            var controller = GetController();
            var result = controller.Edit(1) as ViewResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create()
        {
            var controller = GetController();
            var result = controller.Create() as ViewResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Display()
        {
            var controller = GetController();
            var result = controller.Display(1) as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}