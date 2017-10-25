#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Invoices;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Interfaces;
//using NSubstitute;

#endregion

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    [TestClass]
    public class InvoiceProviderTest
    {
        private static InvoiceProvider GetTarget()
        {
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);
            return invoiceProvider;
        }

        [TestMethod]
        public void Edit_InvoiceModel_Null()
        {
            var target = GetTarget();
            var result = target.Edit(null, string.Empty, string.Empty);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Create_InvoiceModel_Null()
        {
            var target = GetTarget();
            var result = target.Edit(null, string.Empty, string.Empty);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Delete_InvoiceModel_Null()
        {
            var target = GetTarget();
            var result = target.Delete(0, "", "en-US");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void LoadById_Null()
        {
            var target = GetTarget();
            var query = new GetInvoiceById();
            var result = target.Load(query);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadWithFilter_Null()
        {
            var target = GetTarget();
            var query = new GetInvoicesByFilter();
            var result = target.Filter(query);
            Assert.IsNull(result);
        }

        //[TestMethod]
        //public void FilterByDate_FromTo_Null()
        //{
        //    //arrange
        //    var invoiceProvider = GetInoviceProvider();

        //    //act
        //    var response = invoiceProvider.FilterByDate("STAFF", "en-US", null, null, null);

        //    //assert
        //    Assert.IsNull(response);
        //}

        //[TestMethod]
        //public void FilterByDate_FromTo_NotNull()
        //{
        //    //arrange
        //    var invoiceProvider = GetInoviceProvider();

        //    //act
        //    var response = invoiceProvider.FilterByDate("STAFF", "en-US", DateTime.Now, DateTime.Now, new List<InvoiceModel>());

        //    //assert
        //    Assert.IsNotNull(response);
        //}

        //private static InvoiceProvider GetInoviceProvider()
        //{
        //    var invoiceConverter = new InvoiceConverter();
        //    var invoicesLoader = Substitute.For<IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter>>();
        //    invoicesLoader.Load(Arg.Any<GetInvoicesByFilter>())
        //        .Returns(x => Task.FromResult(new List<InvoiceModel>
        //        {
        //            new InvoiceModel {FirstName = "SK"},
        //            new InvoiceModel {FirstName = "KS"}
        //        }));
        //    var invoiceLoader = Substitute.For<IAsyncLoader<InvoiceModel, GetInvoiceById>>();
        //    var invoiceProvider = new InvoiceProvider(invoicesLoader, invoiceLoader, invoiceConverter);
        //    return invoiceProvider;
        //}


        [TestMethod]
        public void CalculateMemberTotal_InvoiceModel_Null()
        {
            //arrange
            var invoicePriceProvider = new USInvoicePriceProvider(new USInvoiceShippingDetails(new ShippingProvider_US()));

            //act
            var response = invoicePriceProvider.CalculateMemberTotal(null, "STAFF", "en-US", "US");

            //assert
            Assert.IsNull(response);
        }

        [TestMethod]
        public void CalculateMemberTotal_InvoicePrice_Null()
        {
            //arrange
            var invoicePriceProvider = new USInvoicePriceProvider(new USInvoiceShippingDetails(new ShippingProvider_US()));

            //act
            var response = invoicePriceProvider.CalculateMemberTotal(new InvoiceModel(), "STAFF", "en-US", "US");

            //assert
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void CalculateCustomerPrice_Invoice_Null()
        {
            //arrange
            var invoicePriceProvider = new USInvoicePriceProvider(new USInvoiceShippingDetails(new ShippingProvider_US()));

            //act
            var response = invoicePriceProvider.CalculateCustomerPrice(null, "STAFF", "en-US", "US");

            //assert
            Assert.IsNull(response);
        }

        [TestMethod]
        public void CalculateCustomerPrice_Invoice_NotNull()
        {
            //arrange
            var invoicePriceProvider = new USInvoicePriceProvider(new USInvoiceShippingDetails(new ShippingProvider_US()));

            //act
            var response = invoicePriceProvider.CalculateCustomerPrice(new InvoiceModel(), null, "en-US", "US");

            //assert
            Assert.IsNotNull(response);
        }
    }
}