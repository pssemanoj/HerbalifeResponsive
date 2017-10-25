using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using HL.PGH.Contracts.ValueObjects;

namespace MyHerbalife3.Ordering.Web.Test
{
    
    
    /// <summary>
    ///This is a test class for PaymentInfo_pt_BRTest and is intended
    ///to contain all PaymentInfo_pt_BRTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PaymentInfo_pt_BRTest
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


        [TestMethod()]
        public void GetFilteredTextTest_BR_Characters()
        {
            string text = "@this| is - the : text )( updated\\";
            PaymentMethodType type = PaymentMethodType.BankSlip;
            string expected = " this  is   the   text    updated ";
            string actual;
            actual = PaymentInfo_pt_BR.GetFilteredText(text, type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetFilteredTextTest_BR_OutOfScopeCharacters()
        {
            string text = "@™this| is - the : text, )( updated‡\\";
            PaymentMethodType type = PaymentMethodType.BankSlip;
            string expected = " ™this  is   the   text,    updated‡ ";
            string actual;
            actual = PaymentInfo_pt_BR.GetFilteredText(text, type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetFilteredTextTest_BR_CharactersNoChanging()
        {
            string text = "@this| is - the : text )( updated\\";
            PaymentMethodType type = PaymentMethodType.AmexCard;
            string expected = "@this| is - the : text )( updated\\";
            string actual;
            actual = PaymentInfo_pt_BR.GetFilteredText(text, type);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetFilteredTextTest_BR_NoCharacters()
        {
            string text = "this is the text updated";
            PaymentMethodType type = PaymentMethodType.BankSlip;
            string expected = "this is the text updated";
            string actual;
            actual = PaymentInfo_pt_BR.GetFilteredText(text, type);
            Assert.AreEqual(expected, actual);
        }

    }
}
