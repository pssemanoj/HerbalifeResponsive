#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Invoices;

#endregion

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    [TestClass]
    public class InvoiceCatalogProviderTest
    {
        private static InvoiceCatalogProvider GetTarget()
        {
            var invoiceProvider = new InvoiceCatalogProvider();
            return invoiceProvider;
        }


        [TestMethod]
        public void GetRootCategories_locale_null()
        {
            var target = GetTarget();

            var response = target.GetRootCategories(string.Empty);

            Assert.IsNull(response);
        }

        [TestMethod]
        public void GetCategories_locale_null()
        {
            var target = GetTarget();

            var response = target.GetCategories(0, string.Empty, false);

            Assert.IsNull(response);
        }

    }
}