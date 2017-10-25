using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers;
using NSubstitute;
using MyHerbalife3.Ordering.Web.Test.Providers;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    /// <summary>
    /// Summary description for FavouriteSKUTest
    /// </summary>
    [TestClass]
    public class PCLearningProviderTest
    {
        private List<FavouriteSKU> favorList;
        private MyHLShoppingCart shoppingCart;
        MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCLearningPointRequest_V01 req;

        [TestInitialize]
        public void Initial()
        {
            shoppingCart = new MyHLShoppingCart
            {
                OrderHeaderID = 123456,
                ShoppingCartID = 123456,
                OrderDate = new DateTime(2016, 3, 1),
                CountryCode = "CN",
                OrderMonth = 3,
                Totals = new OrderTotals_V01
                {
                    AmountDue= 100
                },
                CartItems = new MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItemList
                {
                    new MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01 {SKU = "1314",Quantity = 2 },
                    new MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01 {SKU = "1317",Quantity = 2 }
                }
            };

            Charge_V01 chrg = new Charge_V01
            {
                Amount = 10,
                ChargeType = ChargeTypes.FREIGHT
            };

            req = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PCLearningPointRequest_V01
            {
                DistributorID = "test_id",
                OrderNumber = "111",
                Platform = "CN",
                Point = 5,
                VolumeMonth = "201603",
                Opt = ""
            };

        }

        [TestMethod]
        public void GetPCLearningFee()
        {
            int learningPoint = 10;
            var pcProvider = Substitute.For<IOrderProviderLoader>();
            pcProvider.GetAccumulatedPCLearningPoint("testid", "pcpoint").Returns(learningPoint);

            int point = 0;
            point = pcProvider.GetAccumulatedPCLearningPoint("testid", "pcpoint");

            OrderTotals_V01 total = shoppingCart.Totals as OrderTotals_V01;
            total.AmountDue -= Convert.ToDecimal(point);

            Assert.AreEqual(total.AmountDue, 90);


            total.AmountDue = 100.5m;
            total.AmountDue -= Convert.ToDecimal(point);
            Assert.AreEqual(total.AmountDue, 90.5m);



        }

        [TestMethod]
        public void DeductPCLearningFee()
        {
            int learningPoint = 10;
            var pcProvider = Substitute.For<IOrderProviderLoader>();
            pcProvider.DeductPCLearningPoint(req.DistributorID, req.OrderNumber, req.VolumeMonth, req.Point, req.Platform).Returns(learningPoint);

            int point = 0;
            point = pcProvider.DeductPCLearningPoint(req.DistributorID, req.OrderNumber, req.VolumeMonth, req.Point, req.Platform);

            OrderTotals_V01 total = shoppingCart.Totals as OrderTotals_V01;
            total.AmountDue -= Convert.ToDecimal(point);

            Assert.AreEqual(total.AmountDue, 90);


            total.AmountDue = 100.5m;
            total.AmountDue -= Convert.ToDecimal(point);
            Assert.AreEqual(total.AmountDue, 90.5m);



          
        }

        [TestMethod]
        public void DeductPCLearning_Calling()
        {
            var pcProvider = Substitute.For<IOrderProviderLoader>();

            TriggerDeductLearning(pcProvider);
            TriggerDeductLearning(pcProvider);
            pcProvider.Received(2).DeductPCLearningPoint(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
        }

        [TestMethod]
        public void LockPCLearning_Calling()
        {
            var pcProvider = Substitute.For<IOrderProviderLoader>();

            TriggerLockLearning(pcProvider);
            TriggerLockLearning(pcProvider);
            TriggerLockLearning(pcProvider);

            pcProvider.Received(3).LockPCLearningPoint(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
        }



        [TestMethod]
        public void RollbackPCLearning_Calling()
        {
            var pcProvider = Substitute.For<IOrderProviderLoader>();

            TriggerRollbackLearning(pcProvider);
            TriggerRollbackLearning(pcProvider);
            TriggerRollbackLearning(pcProvider);

            pcProvider.Received(3).RollbackPCLearningPoint(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
        }

        private void TriggerLockLearning(IOrderProviderLoader orderProvider)
        {
            orderProvider.LockPCLearningPoint(req.DistributorID, req.OrderNumber, req.VolumeMonth, req.Point, req.Platform);
        }

        private void TriggerRollbackLearning(IOrderProviderLoader orderProvider)
        {
            orderProvider.RollbackPCLearningPoint(req.DistributorID, req.OrderNumber, req.VolumeMonth, req.Point, req.Platform);
        }

        private void TriggerDeductLearning(IOrderProviderLoader orderProvider)
        {
            orderProvider.DeductPCLearningPoint(req.DistributorID, req.OrderNumber, req.VolumeMonth, req.Point, req.Platform);
        }
        

    }
}
