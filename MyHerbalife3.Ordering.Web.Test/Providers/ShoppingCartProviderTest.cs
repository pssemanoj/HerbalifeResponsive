using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using NSubstitute;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    /// <summary>
    /// Summary description for ShoppingCartProviderTest
    /// </summary>
    [TestClass]
    public class ShoppingCartProviderTest
    {
        public ShoppingCartProviderTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestInitialize]
        public void ChinaCulture()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
        }
      

        [TestMethod]
        public void SlowMovingDiscount()
        {
            string distributorId = "CN640521";
            string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            decimal Slomovingskudiscountamt = 0;
            GetSlowMovingDetail("1317", 50m, 329m, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(30), out Slomovingskudiscountamt);
            var order = new Order_V01();
            order.DistributorID = distributorId;
            var orderitem = new OnlineOrderItem()
                {
                    Quantity =1,
                    SKU = "1317",
                    Description = "",
                    RetailPrice =0,
                    IsPromo = false,
                    
                };
            order.OrderItems = new OrderItems {orderitem};
            order.Shipment = new ShippingInfo_V01
                {
                    Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                        {
                            City = "眉山市",
                            Country = "cn"
                        },
                    ShippingMethodID = "22",
                    WarehouseCode = "3019"
                };
            order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals();
            Assert.AreEqual(50,Slomovingskudiscountamt);
          
        }

      public void GetSlowMovingDetail( string sku,decimal discount,decimal originalprice,DateTime startdate,DateTime enddate,out decimal slowmovingdiscount)
      {
          var catalogLoader = Substitute.For<ICatalogProviderLoader>();
            catalogLoader.LoadSlowMovingSkuInfo().Returns(getSlowMovingSku(sku,discount,originalprice,startdate,enddate, out slowmovingdiscount));
       
      }
      private GetSlowMovingSkuList getSlowMovingSku( string sku,decimal discount,decimal originalprice,DateTime startdate,DateTime enddate, out decimal slowmovingdiscount)
      {
          var skuList = new GetSlowMovingSkuList();
          var slowMovingSku = new GetSlowMovingSku
              {
                  SKU =sku,
                  DiscountAmount = discount,
                  OriginalPrice = originalprice,
                  StartDate = startdate,
                  EndDate = enddate,

              };
          slowmovingdiscount = slowMovingSku.DiscountAmount;
          skuList.Add(slowMovingSku);
          return skuList;
      }
    }
}
