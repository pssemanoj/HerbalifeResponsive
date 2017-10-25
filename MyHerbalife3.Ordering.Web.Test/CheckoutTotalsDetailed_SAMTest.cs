using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Test
{
    
    
    /// <summary>
    ///This is a test class for CheckoutTotalsDetailed_SAMTest and is intended
    ///to contain all CheckoutTotalsDetailed_SAMTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CheckoutTotalsDetailed_SAMTest
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
        ///A test for GetSubTotalValue
        ///</summary>
        [TestMethod()]
        public void GetSubTotalValueTest_EC_30_NoFreight_110_110()
        {
            CheckoutTotalsDetailed_SAM target = new CheckoutTotalsDetailed_SAM();
            OrderTotals_V01 totals = new OrderTotals_V01();
            totals.ChargeList = new ChargeList();
            totals.ChargeList.Add(new Charge_V01(ChargeTypes.PH, 30.0M));
            totals.ChargeList.Add(new Charge_V01(ChargeTypes.FREIGHT, 0M));

            totals.ItemTotalsList= new ItemTotalsList();
            totals.ItemTotalsList.Add(new ItemTotal_V01("0020",1,120,100,80,5,50,null));

            string countryCode = "EC"; 
            decimal expected = 110M;
            decimal actual;
            actual = target.GetSubTotalValue(totals, countryCode);
            Assert.AreEqual(expected, actual);
            
        }



        /// <summary>
        ///A test for GetSubTotalValue
        ///</summary>
        [TestMethod()]
        public void GetSubTotalValueTest_EC_NoPH_NoFreight_80_80()
        {
            CheckoutTotalsDetailed_SAM target = new CheckoutTotalsDetailed_SAM();
            OrderTotals_V01 totals = new OrderTotals_V01();
            totals.ChargeList = new ChargeList();
            totals.ChargeList.Add(new Charge_V01(ChargeTypes.PH, 0M));
            totals.ChargeList.Add(new Charge_V01(ChargeTypes.FREIGHT, 0M));

            totals.ItemTotalsList = new ItemTotalsList();
            totals.ItemTotalsList.Add(new ItemTotal_V01("0020", 1, 120, 100, 80, 5, 50, null));

            string countryCode = "EC";
            decimal expected = 80M;
            decimal actual;
            actual = target.GetSubTotalValue(totals, countryCode);
            Assert.AreEqual(expected, actual);

        }


        /// <summary>
        ///A test for GetSubTotalValue
        ///</summary>
        [TestMethod()]
        public void GetSubTotalValueTest_EC_NochargeList_90_90()
        {
            CheckoutTotalsDetailed_SAM target = new CheckoutTotalsDetailed_SAM();
            OrderTotals_V01 totals = new OrderTotals_V01();
            totals.ChargeList = new ChargeList();

            totals.ItemTotalsList = new ItemTotalsList();
            totals.ItemTotalsList.Add(new ItemTotal_V01("0020", 1, 120, 100, 90, 5, 50, null));

            string countryCode = "EC";
            decimal expected = 90M;
            decimal actual;
            actual = target.GetSubTotalValue(totals, countryCode);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for GetSubTotalValue
        ///</summary>
        [TestMethod()]
        public void GetSubTotalValueTest_EC_NoItemtotalList_0_0()
        {
            CheckoutTotalsDetailed_SAM target = new CheckoutTotalsDetailed_SAM();
            OrderTotals_V01 totals = new OrderTotals_V01();
            totals.ChargeList = new ChargeList();
            totals.ItemTotalsList = new ItemTotalsList();
            
            string countryCode = "EC";
            decimal expected = 0M;
            decimal actual;
            actual = target.GetSubTotalValue(totals, countryCode);
            Assert.AreEqual(expected, actual);

        }


        /// <summary>
        ///A test for GetSubTotalValue
        ///</summary>
        [TestMethod()]
        public void GetSubTotalValueTest_VE_NoItemtotalList_0_0()
        {
            CheckoutTotalsDetailed_SAM target = new CheckoutTotalsDetailed_SAM();
            OrderTotals_V01 totals = new OrderTotals_V01();
            totals.ChargeList = new ChargeList();
            totals.TaxableAmountTotal = 300;
            totals.ItemTotalsList = new ItemTotalsList();
            
            string countryCode = "VE";
            decimal expected = 300M;
            decimal actual;
            actual = target.GetSubTotalValue(totals, countryCode);
            Assert.AreEqual(expected, actual);

        }



    }
}
