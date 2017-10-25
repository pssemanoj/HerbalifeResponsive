using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers.FraudControl;
using MyHerbalife3.Ordering.Providers;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;


namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    [TestClass]
    public class FraudControlProviderUnitTest
    {
        [TestMethod]
        public void FreightCodeNotSubjectToFraudCheckTest()
        {
            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("STAFF", "en-US", "F1P", "03", ShoppingCartItemHelper.GetDistributorShoppingCartItemList(new List<string> { "1455", "8612", "2674" }),  ServiceProvider.CatalogSvc.OrderCategoryType.RSO);

            MyHLConfiguration conf = createMyHLConfiguration(100, new List<DistributorLevelType> { DistributorLevelType._DS }, true, false, 10, new List<string> { "F1P" }, 45, false, new List<string>(), 100, new List<string>());
            var result = FraudControlProvider.IsSubjectToFraudCheck("en-US", cart, "DS", DateTime.Now.AddYears(-1),conf);
            Assert.AreEqual(false, result);

        }
        [TestMethod]
        public void NumberOfSkuSubjectToFraudCheckTest()
        {
            MyHLShoppingCart cart =
               MyHLShoppingCartGenerator.GetBasicShoppingCart("STAFF", "en-US", "FED", "03", ShoppingCartItemHelper.GetDistributorShoppingCartItemList(new List<string> { "1455", "8612", "2674" }), ServiceProvider.CatalogSvc.OrderCategoryType.RSO);
            MyHLConfiguration conf = createMyHLConfiguration(100, new List<DistributorLevelType> { DistributorLevelType._DS },true, false, 10, new List<string> { "F1P" },2, false,new List<string>(),100, new List<string>());
            // 
            var result = FraudControlProvider.IsSubjectToFraudCheck("en-US", cart, "DS", DateTime.Now.AddYears(-1), conf);
            Assert.AreEqual(true, result);
        }

        [TestMethod]

        public void ShiptoZipcodeSubjectToFraudCheckTest()
        {
            MyHLShoppingCart cart =
              MyHLShoppingCartGenerator.GetBasicShoppingCart("STAFF", "en-US", "FED", "03", new ShippingAddress_V02 { Address = new MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 { Country = "US", City = "Fake", PostalCode ="90502", } });
            MyHLConfiguration conf = createMyHLConfiguration(100, new List<DistributorLevelType> { DistributorLevelType._DS }, true, false, 10, new List<string> { "F1P" }, 2, false, new List<string> { "90275"}, 100, new List<string>());
            // 
            var result = FraudControlProvider.IsSubjectToFraudCheck("en-US", cart, "DS", DateTime.Now.AddYears(-1), conf);
            Assert.AreEqual(true, result);
        }

        private MyHLConfiguration createMyHLConfiguration(decimal amountDue, List<DistributorLevelType> distributorLevels,
            bool kountEnabled, bool ETOOrderSubjectToFraudCheck, int dayAfterApplication, List<string> freightCodeNotSubjectToFraudCheck,
            int numberOfItem, bool isPickupOrderSubjectToFraudCheck, List<string> shiptoStateNOTSubjectToFraudCheck, decimal volumePoint, List<string> shiptoZipSubjectToFraudCheck)
        {
            return new MyHLConfiguration
            {
                Country = "US",
                Locale = "en-US",
                FControlSettings = new FControlSetting
                {
                    AmountDue = amountDue,
                    DistributorLevels = distributorLevels,
                    Enabled = kountEnabled,
                    ETOIncluded = ETOOrderSubjectToFraudCheck,
                    DayApplication = dayAfterApplication,
                    FreightCodes = freightCodeNotSubjectToFraudCheck,
                    NumberOfItem = numberOfItem,
                    PickupOrderIncluded = isPickupOrderSubjectToFraudCheck,
                    Statecodes = shiptoStateNOTSubjectToFraudCheck,
                    VolumePoint = volumePoint,
                    Zipcodes = shiptoZipSubjectToFraudCheck
                }
            };
        }

        
    }
}
