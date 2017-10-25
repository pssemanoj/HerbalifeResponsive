#region

using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Controllers.Invoice;
using MyHerbalife3.Ordering.Web.Test.Mocks;

#endregion

namespace MyHerbalife3.Ordering.Web.Test.Controllers
{
    [TestClass]
    public class InvoiceSearchControllerTest
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

        private static InvoiceSearchController GetController()
        {
            var controller = new InvoiceSearchController(new MockLocalizationManager());
            return controller;
        }


        [TestMethod]
        public void LoadFilterCategory_NotNull()
        {
            var target = GetController();
            var result = target.LoadFilterCategory();
            Assert.IsNotNull(result);
        }
    }
}