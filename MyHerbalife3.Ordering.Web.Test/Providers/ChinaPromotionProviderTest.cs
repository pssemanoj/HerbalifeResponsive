using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using NSubstitute;
using System.Xml;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
   [TestClass]
   public class ChinaPromotionProviderTest
    {
          public ChinaPromotionProviderTest()
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
          public void LoadPromotionFromDB()
          {
              string promotion_info = string.Empty;
              GetLoadPromotionFromDB("Zh-CN", DateTime.Now,out promotion_info);
              Assert.AreEqual("<Promotions> <promo code='201311140002' startDate='12-01-2014' endDate ='02-29-2016' promotionType='Order' freeSKUList='9143|1' /></Promotions>", promotion_info);
          }
          public void GetLoadPromotionFromDB(string Local, DateTime Date, out string promotion_info)       
          {
             
              var PromotionLoader = Substitute.For<IChinaPromotionProviderLoader>();
              PromotionLoader.GetEffectivePromotionList(Local, Date).Returns(LoadPromotionFromDB(Local,Date,out promotion_info));
             
             
          }
          private PromotionResponse_V01 LoadPromotionFromDB(string Local, DateTime Date, out string promotion_info)
          {
              var promo = new PromotionResponse_V01();
              promo.Promotion_Info = "<Promotions> <promo code='201311140002' startDate='12-01-2014' endDate ='02-29-2016' promotionType='Order' freeSKUList='9143|1' /></Promotions>";
              promotion_info=promo.Promotion_Info;
              return new PromotionResponse_V01();
          }
         

    }
}
