using System;
using System.Collections.Generic;
using System.Globalization;
using HL.Catalog.ValueObjects;
using HL.Common.ValueObjects;
using HL.Order.ValueObjects.China;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ViewModel.Model;
using NSubstitute;

namespace MyHerbalife3.Ordering.Web.Test.Providers.MobileDiscSkuWebAPI
{
    public class MobileDiscSkuSimulator
    {
        private readonly IDiscontinuedSkuLoader _discontinuedSkuLoader;

        public MobileDiscSkuSimulator(IDiscontinuedSkuLoader discontinuedSkuLoader)
        {
            _discontinuedSkuLoader = discontinuedSkuLoader;
        }

        public  CatalogItemList GetDiscontinuedSkuList(string distributorId, string locale,
            List<ShoppingCartItem_V01> shoppingCartItems)
        {
            return _discontinuedSkuLoader.GetDiscontinuedSkuList(distributorId, locale, shoppingCartItems);
        }

        public List<DiscontinuedSkuItemResponseViewModel> CheckDiscontinuedPromo(
            List<ShoppingCartItem_V01> shoppingcartItems)
        {
            return _discontinuedSkuLoader.CheckDiscontinuedPromo(shoppingcartItems);
        }
    }

    [TestClass]
    public class DiscSkuWebApiTest
    {
        [TestMethod]
        public void GetDiscontinuedSkuApi()
        {
            var paramMock = GetDiscontinuedSkuParamMock();
            var proxy = Substitute.For<IDiscontinuedSkuLoader>();
            var resultMock = GetMobileDiscontinuedResultMock();
            proxy.GetDiscontinuedSkuList(paramMock.DistributorId, paramMock.Locale, paramMock.ShoppingCartItemToCheck)
                .Returns(resultMock);
            var target=new MobileDiscSkuSimulator(proxy);
            var result = target.GetDiscontinuedSkuList(paramMock.DistributorId, paramMock.Locale, paramMock.ShoppingCartItemToCheck);
            Assert.IsNotNull(result); 
        }

        [TestMethod]
        public void GetDiscontinuedPromo()
        {
            var paramMock = GetDiscontinuedSkuPromoMock();
            var proxy = Substitute.For<IDiscontinuedSkuLoader>();
            var resultMock = GetMobileDiscontinuedPromoResultMock();
            proxy.CheckDiscontinuedPromo(paramMock)
                .Returns(resultMock);
            var target = new MobileDiscSkuSimulator(proxy);
            var result = target.CheckDiscontinuedPromo(paramMock);
            Assert.IsNotNull(result);
        }

        private List<ShoppingCartItem_V01> GetDiscontinuedSkuPromoMock()
        {
            List<ShoppingCartItem_V01> itemList=new List<ShoppingCartItem_V01>();
            ShoppingCartItem_V01 item = new ShoppingCartItem_V01
            {
                SKU = "9143",
                IsPromo = true,
                MinQuantity = 1,
                PartialBackordered = false,
                Quantity = 1,
                Updated = DateTime.Now
            };
            itemList.Add(item);
            return itemList;
        }

        private CatalogItemList GetMobileDiscontinuedResultMock()
        {
            CatalogItemList catalogItemList=new CatalogItemList();
            Product product = new Product {SKU = "1318",Description = "Formula 1",InventorySKU = "",IsActive = false,IsInventory = true,ProductCategory = "Inv",ProductType = ProductType.Product,StockingSKU = "1318"};
            CatalogItem item = new CatalogItem(product,238,"CNY");
            catalogItemList.Add("1318",item);
            return catalogItemList;
        }

        private List<DiscontinuedSkuItemResponseViewModel> GetMobileDiscontinuedPromoResultMock()
        {
            List<DiscontinuedSkuItemResponseViewModel> response=new List<DiscontinuedSkuItemResponseViewModel>();
            DiscontinuedSkuItemResponseViewModel item = new DiscontinuedSkuItemResponseViewModel
            {
                ProductName = "promoItem1",
                Sku = "9143"
            };
            response.Add(item);
            return response;
        }

        private GetDiscontinuedSkuParam GetDiscontinuedSkuParamMock()
        {
            List<ShoppingCartItem_V01>DiscontinuedItems=new List<ShoppingCartItem_V01>();
            ShoppingCartItem_V01 item = new ShoppingCartItem_V01
            {
                SKU = "1318",
                IsPromo = false,
                Quantity = 1,
                PartialBackordered = false,
                Updated = DateTime.Now,
            };

            ShoppingCartItem_V01 item1 = new ShoppingCartItem_V01
            {
                SKU = "9143",
                IsPromo = true,
                Quantity = 1,
                PartialBackordered = false,
                Updated = DateTime.Now,
            };

            DiscontinuedItems.Add(item);
            DiscontinuedItems.Add(item1);

            GetDiscontinuedSkuParam param = new GetDiscontinuedSkuParam
            {
                DistributorId = "CN640521",
                Locale = "zh-CN",
                ShoppingCartItemToCheck = DiscontinuedItems
            };
            return param;
        }

        public PromotionCollection GetPromotionCollectionMock()
        {
            PromotionCollection promotionCollection=new PromotionCollection();
            PromotionElement pe = new PromotionElement
            {
                FreeSKUList = new FreeSKUCollection {new FreeSKU {Quantity = 1,SKU = "376P"} },
                AmountMax = -1,
                AmountMaxInclude = -1,
                AmountMin = -1,
                AmountMinInclude = -1,
                Code = "00014",
                CustCategoryTypeList = new List<string>(),
                SelectableSKUList = new FreeSKUCollection {new FreeSKU {Quantity=1,SKU = "1447"}},
                StartDate = Convert.ToDateTime("2016-02-01 00:00:00").ToString(CultureInfo.InvariantCulture),
                EndDate = Convert.ToDateTime("2016-02-28 11:59:59 PM").ToString(CultureInfo.InvariantCulture),
                excludedExpID = new List<string>(),
                VolumeMax = -1,
                VolumeMaxInclude = -1,
                VolumeMin = -1,
                VolumeMinInclude = 200,
                CurrentConfiguration = { },
                CustCategoryType = "",
                CustType = "",
                LockAllAttributesExcept = { },
                LockItem = false,
                LockAttributes = { },
                LockElements = { },
                PromotionType = PromotionType.Volume,
                NumOfMonth = 1,
                MaxFreight = -1,
                OnlineOrderOnly = false,
                FreeSKUListForSelectableSku = new FreeSKUCollection(),
                FreeSKUListForVolume = new FreeSKUCollection(),
                ForPC = false,
                CustTypeList = new List<string> { "DS,PC,FM,SR"},
                DSStoreProvince = new List<string>(),
                DeliveryTypeList = new List<string>()

            };
            promotionCollection.Add(pe);
            return promotionCollection;
        }
    }
}
