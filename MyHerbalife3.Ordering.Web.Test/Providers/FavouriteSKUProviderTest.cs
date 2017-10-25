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

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    /// <summary>
    /// Summary description for FavouriteSKUTest
    /// </summary>
    [TestClass]
    public class FavouriteSKUProviderTest
    {
        private List<FavouriteSKU> favorList;

        [TestInitialize]
        public void Initial()
        {
            favorList = new List<FavouriteSKU>() { new FavouriteSKU { FavouriteID = 1, ProductID = 1, ProductSKU = "1316", Locale = "zh-CN" } };
        }

        [TestMethod]
        public void GetFavouriteSKU_Product_Is_In_FavouriteList()
        {
            var favorProvider = Substitute.For<IFavouriteSkuLoader>();
            favorProvider.GetDistributorFavouriteSKU("testid", "zh-CN").Returns(favorList);
            FavouriteSKUBLTest fvBL = new FavouriteSKUBLTest(favorProvider);
            
            var result = fvBL.CheckIfFavouriteSKU("1316", "testid", "zh-CN");
            Assert.AreEqual(true, result);

            result = fvBL.CheckIfFavouriteSKU("1317", "testid", "zh-CN");
            Assert.AreNotEqual(true, result);

            //Different distributor id
            result = fvBL.CheckIfFavouriteSKU("1316", "testid2", "zh-CN");
            Assert.AreNotEqual(true, result);

            //no distributor pass in
            result = fvBL.CheckIfFavouriteSKU("1316", string.Empty, "zh-CN");
            Assert.AreNotEqual(true, result);

        }
        
        [TestMethod]
        public void GetFavouriteSKU_distributor_Call_Received()
        {
            var favorProvider = Substitute.For<IFavouriteSkuLoader>();
            favorProvider.GetDistributorFavouriteSKU("testid", "zh-CN").Returns(favorList);
            FavouriteSKUBLTest fvBL = new FavouriteSKUBLTest(favorProvider);

            fvBL.CheckIfFavouriteSKU("1316", "testid", "zh-CN");
            fvBL.CheckIfFavouriteSKU("1317", "testid", "zh-CN");
            fvBL.CheckIfFavouriteSKU("1318", "testid", "zh-CN");

            favorProvider.Received(3).GetDistributorFavouriteSKU(Arg.Any<string>(), Arg.Any<string>());
        }

        [TestMethod]
        public void AddFavouriteSKU_to_FavouriteList()
        {
            var favorProvider = Substitute.For<IFavouriteSkuLoader>();
            favorProvider.SetFavouriteSKU("testid", 1, "1316", "zh-CN", 0).Returns(true);
            FavouriteSKUBLTest fvBL = new FavouriteSKUBLTest(favorProvider);

            //Add Favourite List
            var result = fvBL.AddFavouriteSKUtoList("testid", 1, "1316", "zh-CN", 0);
            Assert.AreEqual(true, result);

            //Remove Favourite List
            favorProvider.SetFavouriteSKU("testid", 1, "1316", "zh-CN", 1).Returns(true);
            fvBL = new FavouriteSKUBLTest(favorProvider);
            Assert.AreEqual(true, result);

        }

        [TestMethod]
        public void AddRemoveFavouriteSKU_to_FavouriteList_Received()
        {
            var favorProvider = Substitute.For<IFavouriteSkuLoader>();
            favorProvider.SetFavouriteSKU("testid", 1, "1316", "zh-CN", 0).Returns(true);
            FavouriteSKUBLTest fvBL = new FavouriteSKUBLTest(favorProvider);

            fvBL.AddFavouriteSKUtoList("testid", 1, "1316", "zh-CN", 0);
            fvBL.AddFavouriteSKUtoList("testid", 2, "1317", "zh-CN", 0);
            fvBL.AddFavouriteSKUtoList("testid", 3, "1318", "zh-CN", 0);

            fvBL.AddFavouriteSKUtoList("testid", 1, "1316", "zh-CN", 1);
            fvBL.AddFavouriteSKUtoList("testid", 2, "1317", "zh-CN", 1);
            fvBL.AddFavouriteSKUtoList("testid", 3, "1318", "zh-CN", 1);

            favorProvider.Received(6).SetFavouriteSKU(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());   
        }

    }
}
