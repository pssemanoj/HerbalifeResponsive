using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Rules.EventTickets.zh_CN;
using NSubstitute;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Test.Rules
{
    /// <summary>
    /// Summary description for EventTicketRuleTest
    /// </summary>
    [TestClass]
    public class EventTicketRuleTest
    {
        public EventTicketRuleTest()
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
        public void EligibleEventProductList()
        {
          
            var allreadypurchasedEventList = new Dictionary<int, SKU_V01> {{1, new SKU_V01
                {
                    Description = "",
                    ID = 0,
                    ImagePath = "",
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 1000,
                    ParentSKU = null,
                    CatalogItem = null,
                    PdfName = "",
                    Product = null,
                    SKU = "937D"

                }}};
             var gettingaddedEventList = new Dictionary<int, SKU_V01> {{10, new SKU_V01
                {
                    Description = "",
                    ID = 0,
                    ImagePath = "",
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 1000,
                    ParentSKU = null,
                    CatalogItem = null,
                    PdfName = "",
                    Product = null,
                    SKU = "937D"

                }}};
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "ETO Rules";
            defaultResult.Result = RulesResult.Unknown;
            var eventLoder = Substitute.For<IEventTicketProviderLoader>();
            var apurchasedEventList = new Dictionary<int, SKU_V01>();
            eventLoder.LoadSkuPurchasedCount(gettingaddedEventList, "CN640521","CN").Returns(allreadypurchasedEventList);
            IEventTicketProviderLoader loader = new EventTicketProviderLoader();
            var result= loader.ValidateEventList(gettingaddedEventList, allreadypurchasedEventList, 2, ref defaultResult);
            if (result !=null)
            {
                foreach (var skuV01 in result)
                {
                    Assert.AreEqual(skuV01.Key,1);
                }
            }
        }
    }
}
