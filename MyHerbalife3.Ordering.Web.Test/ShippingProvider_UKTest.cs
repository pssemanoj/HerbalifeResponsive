using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.Shipping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Test
{
    
    
    /// <summary>
    ///This is a test class for ShippingProvider_UKTest and is intended
    ///to contain all ShippingProvider_UKTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ShippingProvider_UKTest
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetDeliveryOptionsListForShipping
        ///</summary>
        [TestMethod()]
        public void GetDeliveryOptionsListForShippingTest_GB_Null_0()
        {
            ShippingProvider_UK target = new ShippingProvider_UK();
            string Country = null;
            string locale = null;
            ShippingAddress_V01 address = null;
            List<DeliveryOption> expected = new List<DeliveryOption>();
            List<DeliveryOption> actual;
            actual = target.GetDeliveryOptionsListForShipping(Country, locale, address);
            Assert.AreEqual(expected.Count, actual.Count);
        }

        /// <summary>
        ///A test for ConvertShippingAlternativetoDeliveryOption
        ///</summary>
        [TestMethod()]
        public void ConvertShippingAlternativetoDeliveryOptionTest_GB_2_2()
        {
            ShippingProvider_UK target = new ShippingProvider_UK(); 
            List<ShippingOption_V01> list = new List<ShippingOption_V01>();
            list.Add(new ShippingOption_V01(){CourierName = "name",Description = "dummy Description", DisplayOrder = 0, End = DateTime.Now.AddDays(20),FreightCode = "30",ID = 10025,IsDefault = false,ShippingSource = new ShippingSource_V01(),Start = DateTime.Now.AddDays(-10),WarehouseCode = "FXD"});
            list.Add(new ShippingOption_V01(){CourierName = "name2",Description = "dummy Description 2", DisplayOrder = 1, End = DateTime.Now.AddDays(25),FreightCode = "35",ID = 10026,IsDefault = false,ShippingSource = new ShippingSource_V01(),Start = DateTime.Now.AddDays(-15),WarehouseCode = "IND"});
            ShippingAlternativesResponse_V01 response = new ShippingAlternativesResponse_V01()
                {
                    DeliveryAlternatives = list
                }; 
            var expected = 2;
            List<DeliveryOption> actual;
            actual = target.ConvertShippingAlternativetoDeliveryOption(response);
            Assert.AreEqual(expected, actual.Count);
        }


        [TestMethod()]
        public void ConvertShippingAlternativetoDeliveryOptionTest_GB_Null_0()
        {
            ShippingProvider_UK target = new ShippingProvider_UK();
            ShippingAlternativesResponse_V01 response = new ShippingAlternativesResponse_V01()
            {
                DeliveryAlternatives = null
            };
            var expected = 0;
            List<DeliveryOption> actual;
            actual = target.ConvertShippingAlternativetoDeliveryOption(response);
            Assert.AreEqual(expected, actual.Count);
        }
    }
}
