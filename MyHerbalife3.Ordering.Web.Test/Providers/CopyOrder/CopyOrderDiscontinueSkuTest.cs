using System;
using System.Collections.Generic;
using System.Linq;
using HL.Content.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.Providers.Interface;
using NSubstitute;
using Category_V02ItemCollection = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category_V02ItemCollection;
using ShoppingCartItemList = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItemList;
using SKU_V01ItemCollection = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01ItemCollection;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Test.Providers.CopyOrder
{
    [TestClass]
    public class CopyOrderDiscontinueSkuTest
    {
        public ICatalogInterface GetUnfilteredCart(List<ShoppingCart_V03> shoppingCartList )
        {
            var proxy = Substitute.For<ICatalogInterface>();
            proxy.CopyShoppingCart(new CopyShoppingCartRequest(Arg.Any<CopyShoppingCartRequest_V01>())).CopyShoppingCartResult.Returns(new GetShoppingCartResponse_V03()
            {Message = "Success", ParameterErrorList = null, ShoppingCartList = shoppingCartList});
            return proxy;
        }

        public List<ShoppingCart_V03> GetShopingCartList(int mode)
        {
            List<ShoppingCart_V03> result=new List<ShoppingCart_V03>();
            ShoppingCart_V03 mockObject=new ShoppingCart_V03();
            mockObject.DraftName = "Testing";
            mockObject.IsDraft = false;
            mockObject.Locale = "zh-CN";
            mockObject.DeliveryOption=DeliveryOptionType.Shipping;
            mockObject.APFEdited = false;
            mockObject.CartItems = GetCartItems(mode);
            mockObject.CurrentItems = null;
            mockObject.Version = "01";
            mockObject.DeliveryOptionID = 11580270;
            mockObject.ShoppingCartID = 11580270;
            mockObject.OrderCategory=OrderCategoryType.RSO;
            mockObject.DistributorID = "CN640521";
            mockObject.ShippingAddressID = 11580270;
            mockObject.OrderSubType = string.Empty;
            result.Add(mockObject);
            return result;
        }

        private ShoppingCartItemList GetCartItems(int mode)
        {
            ShoppingCartItemList cartItemList=new ShoppingCartItemList();
            if (mode == 0)
            {
                ShoppingCartItem_V01 item = new ShoppingCartItem_V01
                {
                    SKU = "9909",
                    IsPromo = false,
                    MinQuantity = 0,
                    Quantity = 1,
                    Updated = DateTime.Now
                };
                cartItemList.Add(item);
            }

            if (mode == 1)
            {
                ShoppingCartItem_V01 item2 = new ShoppingCartItem_V01
                {
                    SKU = "1316",
                    IsPromo = false,
                    MinQuantity = 0,
                    Quantity = 1,
                    Updated = DateTime.Now
                };
                cartItemList.Add(item2);
            }
            if (mode == 3)
            {
                ShoppingCartItem_V01 item3 = new ShoppingCartItem_V01
                {
                    SKU = "9143",
                    IsPromo = true,
                    MinQuantity = 0,
                    Quantity = 1,
                    Updated = DateTime.Now
                };
                cartItemList.Add(item3);
            }
            

            return cartItemList;
        }

        //[TestMethod]
        //public void TestCartWithVirtualSku()
        //{

        //    // 0 == will iclude 9909 vitual sku
        //    var shoppingCartList = GetShopingCartList(0);
        //    var invalidCart = GetUnfilteredCart(shoppingCartList);
        //    var target = GetTarget(invalidCart,GetProductInfoCatalogMock(0));
        //    var shoppingCartTestObject = shoppingCartList.FirstOrDefault();
        //    if (shoppingCartTestObject != null)
        //    {
        //        var result = target.GetDiscontinuededSku(shoppingCartTestObject.ShoppingCartID, shoppingCartTestObject.DistributorID, shoppingCartTestObject.Locale,0,null);
        //        Assert.IsNotNull(result);
        //    }
            
        //}

        //[TestMethod]
        //public void TestNormalSkuOrder()
        //{
        //   // will only generate normal sku 1316
        //    var shoppingCartList = GetShopingCartList(1);
        //    var invalidCart = GetUnfilteredCart(shoppingCartList);
        //    var target = GetTarget(invalidCart, GetProductInfoCatalogMock(0));
        //    var shoppingCartTestObject = shoppingCartList.FirstOrDefault();
        //    if (shoppingCartTestObject != null)
        //    {
        //        var result = target.GetDiscontinuededSku(shoppingCartTestObject.ShoppingCartID, shoppingCartTestObject.DistributorID, shoppingCartTestObject.Locale, 0, null);
        //        Assert.IsNotNull(result);
        //        Assert.AreEqual(0,result.Count);
        //    }
        //}

        //[TestMethod]
        //public void TestDiscContinueSku()
        //{
        //    // will only generate normal sku 1316
        //    var shoppingCartList = GetShopingCartList(1);
        //    var invalidCart = GetUnfilteredCart(shoppingCartList);
        //    var target = GetTarget(invalidCart, GetProductInfoCatalogMock(1));
        //    var shoppingCartTestObject = shoppingCartList.FirstOrDefault();
        //    if (shoppingCartTestObject != null)
        //    {
        //        var result = target.GetDiscontinuededSku(shoppingCartTestObject.ShoppingCartID, shoppingCartTestObject.DistributorID, shoppingCartTestObject.Locale, 0, null);
        //        Assert.IsNotNull(result);
        //        Assert.AreEqual(1, result.Count);
        //    }
        //}

        //[TestMethod]
        //public void TestPromoSku()
        //{
        //    // will only generate normal sku 1316
        //    var shoppingCartList = GetShopingCartList(3);
        //    var invalidCart = GetUnfilteredCart(shoppingCartList);
        //    var target = GetTarget(invalidCart, GetProductInfoCatalogMock(1));
        //    var shoppingCartTestObject = shoppingCartList.FirstOrDefault();
        //    if (shoppingCartTestObject != null)
        //    {
        //        var result = target.GetDiscontinuededSku(shoppingCartTestObject.ShoppingCartID, shoppingCartTestObject.DistributorID, shoppingCartTestObject.Locale, 0, null);
        //        Assert.IsNotNull(result);
        //        Assert.AreEqual(1, result.Count);
        //    }
        //}

        //public ShoppingCartProviderLoader GetTarget(ICatalogInterface invalidCart, ProductInfoCatalog_V01 allSku)
        //{
        //    var proxy = Substitute.For<ICatalogProviderLoader>();
        //    proxy.GetProductInfoCatalog("zh-CN")
        //        .Returns(new ProductInfoCatalog_V01()
        //        {
        //            Locale = "zh-CN",
        //            AllCategories = allSku.AllCategories,
        //            AllSKUs = allSku.AllSKUs
        //        });
        //    var target = new ShoppingCartProviderLoader(proxy,invalidCart);
        //    return target;
        //}

        private ProductInfoCatalog_V01 GetProductInfoCatalogMock(int mode)
        {
            ProductInfoCatalog_V01 allSku = new ProductInfoCatalog_V01() { Platform = "MyHL", Locale = "zh-CN" };
            allSku.AllSKUs = GetMockAllSku(mode);
            allSku.AllCategories = GetAllCategories();
            return allSku;
        }

        private Category_V02ItemCollection GetAllCategories()
        {
            Category_V02ItemCollection collection=new Category_V02ItemCollection();
            Category_V02 cat02=new Category_V02();
            cat02.Description = "TestingMock";
            cat02.DisplayName = "sku1316";
            cat02.ParentCategory = null;
            cat02.SubCategories = null;
            cat02.ImagePath = "testing.jpg";
            cat02.Products = GetProductInfoV02Mock();
            collection.Add(1316,cat02);
            return collection;
        }

        private List<ProductInfo_V02> GetProductInfoV02Mock()
        {
            List<ProductInfo_V02> list=new List<ProductInfo_V02>();
            ProductInfo_V02 prod2=new ProductInfo_V02();
            prod2.DisplayName = "testing";
            prod2.Benefits = "adadda";
            prod2.CrossSellProducts = null;
            prod2.Details = "addada";
            prod2.SKUs= new List<SKU_V01> {new SKU_V01()
                {
                    CatalogItem = GetCatalogOtemMock(),
                    Description = "香草口味 550克",
                    ImagePath = "/Content/zh-CN/img/Catalog/Products/1316_400X400.jpg",
                    ID = 46,
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 2147483647,
                    ParentSKU = null,
                    PdfName = "/Content/zh-CN/pdf/Catalog/LPR1316CH_12_OP1_HRes.pdf",
                    Product = null
                }};
             list.Add(prod2);
            return list;
        }

        private SKU_V01ItemCollection GetMockAllSku(int mode)
        {
            SKU_V01ItemCollection allsku = new SKU_V01ItemCollection();
            if (mode == 1)
            {
                allsku.Add("1317",
                new SKU_V01()
                {
                    CatalogItem = GetCatalogOtemMock(),
                    Description = "香草口味 550克",
                    ImagePath = "/Content/zh-CN/img/Catalog/Products/1316_400X400.jpg",
                    ID = 46,
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 2147483647,
                    ParentSKU = null,
                    PdfName = "/Content/zh-CN/pdf/Catalog/LPR1316CH_12_OP1_HRes.pdf",
                    Product = null
                });
                return allsku;
            }
            else if(mode==0)
            {
                allsku.Add("1316",
                new SKU_V01()
                {
                    CatalogItem = GetCatalogOtemMock(),
                    Description = "香草口味 550克",
                    ImagePath = "/Content/zh-CN/img/Catalog/Products/1316_400X400.jpg",
                    ID = 46,
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 2147483647,
                    ParentSKU = null,
                    PdfName = "/Content/zh-CN/pdf/Catalog/LPR1316CH_12_OP1_HRes.pdf",
                    Product = null
                });
                return allsku;
            }
            else
            {
                allsku.Add("9143",
                new SKU_V01()
                {
                    CatalogItem = GetCatalogOtemMock(),
                    Description = "香草口味 550克",
                    ImagePath = "/Content/zh-CN/img/Catalog/Products/1316_400X400.jpg",
                    ID = 46,
                    IsDisplayable = true,
                    IsPurchasable = true,
                    MaxOrderQuantity = 2147483647,
                    ParentSKU = null,
                    PdfName = "/Content/zh-CN/pdf/Catalog/LPR1316CH_12_OP1_HRes.pdf",
                    Product = null
                });
                return allsku;
            }
        }

        private CatalogItem_V01 GetCatalogOtemMock()
        {
            return new CatalogItem_V01()
            {
                EarnBase = 281,
                InventoryList = null,
                IsEventTicket = false,
                IsFlexKit = false,
                IsFreightExempt = false,
                ListPrice = 0,
                IsTaxExempt = false,
                UnitTaxBase = 281,
                VolumePoints = 23,
            };
        }
    }
}
