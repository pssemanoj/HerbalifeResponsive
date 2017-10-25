using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Web.Test.Providers;
using NSubstitute;
using NSubstitute.Core;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Test.Rules
{
    /// <summary>
    /// Summary description for PromotionRuleTest
    /// </summary>
    [TestClass]
    public class PromotionRulesTest
    {
        public PromotionRulesTest()
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
        public void ProcessVolumePromotionDSNovPromo_Test()
        {
            var promoelement = new PromotionElement
                {
                    StartDate = "11-01-2015",
                    EndDate = "11-30-2015",
                    PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                    HasIncrementaldegree = true,
                    VolumeMinInclude = 200,
                    CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                    FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "1443"
                                },
                                new FreeSKU
                                {
                                      Quantity = 1,
                                      SKU = "K365"
                                },
                        },
                    SelectableSKUList = new FreeSKUCollection
                        {
                             new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "1316"
                                },
                                 new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "1318"
                                },
                        },
                        FreeSKUListForVolume = new FreeSKUCollection
                            {
                                 new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "1443"
                                },
                            },
                            FreeSKUListForSelectableSku = new FreeSKUCollection
                                {
                                     new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "K365"
                                },
                                },
                           
                };
            promoelement.Code = "DSNovPromo";
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1442"),
                    ShoppingCartItemHelper.GetCartItem(3, 1, "1318")
                };
            var order = new Order_V01();
            order.DistributorID = "CN640521";
            var orderitem1 = new OnlineOrderItem()
                {
                    Quantity =10,
                    SKU = "1316",
                    Description = "",
                    RetailPrice =0,
                    IsPromo = false,
                    
                };
            var orderitem2 = new OnlineOrderItem()
                {
                    Quantity = 1,
                    SKU = "1442",
                    Description = "",
                    RetailPrice = 0,
                    IsPromo = false,

                };
                 var orderitem3 = new OnlineOrderItem()
                {
                    Quantity =1,
                    SKU = "1318",
                    Description = "",
                    RetailPrice =0,
                    IsPromo = false,
                    
                };
            order.OrderItems= new OrderItems {orderitem1, orderitem2, orderitem3};
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
            var ordertotal = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V02();
            GetQuotes(order, ordertotal, false,out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            cart.Totals = ordertotal;
            FreeSKUCollection skuCollection =  promorule.CheckDsNovPromo(promoelement, cart);
            Assert.AreEqual(skuCollection[0].Quantity,3);
            
        }
        [TestMethod]
        public void ProcessVolumePromotionDSFebPromo_Test()
        {
            var promoelement = new PromotionElement
            {
                StartDate = "02-01-2016",
                EndDate = "05-01-2016",
                PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                HasIncrementaldegree = true,
                VolumeMinInclude = 200,
                CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "543P"
                                },
                                new FreeSKU
                                {
                                      Quantity = 1,
                                      SKU = "547P"
                                },
                        },
                SelectableSKUList = new FreeSKUCollection
                        {
                             new FreeSKU
                                {
                                   Quantity = 2,
                                   SKU = "1447"
                                },
                                
                        },
             

            };
            promoelement.Code = "00014";
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1442"),
                   
                };
            var order = new Order_V01();
            order.DistributorID = "CN640521";
            var orderitem1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            var orderitem2 = new OnlineOrderItem()
            {
                Quantity = 2,
                SKU = "1447",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
           
            order.OrderItems = new OrderItems { orderitem1, orderitem2 };
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
            var ordertotal = new OrderTotals_V02();
            GetQuotes(order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.DSPromo(promoelement, cart);
            Assert.AreEqual(skuCollection[0].Quantity, 2);

        }
        [TestMethod]
        public void ProcessVolumePromotionPCFebPromo_Test()
        {
            var promoelement = new PromotionElement
            {
                StartDate = "02-01-2016",
                EndDate = "05-01-2016",
                PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                HasIncrementaldegree = true,
                VolumeMinInclude = 200,
                CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "543P"
                                },
                                new FreeSKU
                                {
                                      Quantity = 1,
                                      SKU = "547P"
                                },
                        },
                SelectableSKUList = new FreeSKUCollection
                        {
                             new FreeSKU
                                {
                                   Quantity = 2,
                                   SKU = "1447"
                                },
                                
                        },


            };
            promoelement.Code = "00015";
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1447"),
                   
                };
            var order = new Order_V01();
            order.DistributorID = "CN1632369";
            var orderitem1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            var orderitem2 = new OnlineOrderItem()
            {
                Quantity = 2,
                SKU = "1447",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };

            order.OrderItems = new OrderItems { orderitem1, orderitem2 };
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
            var ordertotal = new OrderTotals_V02();
            GetQuotes(order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.DSPromo(promoelement, cart);
            Assert.AreEqual(skuCollection[0].Quantity, 2);

        }
        
         [TestMethod]
        public void ProcessVolumePromotionDS_Test()
        {
            var Promotion = new PromotionElement
              {
                  StartDate = "11-01-2015",
                  EndDate = "11-30-2015",
                  PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                  HasIncrementaldegree = true,
                  VolumeMinInclude = 200,
                  CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                  FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "376P"
                                }},
              };
            Promotion.Code="00010";
            var Cart=MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
           Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

             var Order = new Order_V01();
            Order.DistributorID = "CN640521";
            var order1 = new OnlineOrderItem()
                {
                    Quantity =10,
                    SKU = "1316",
                    Description = "",
                    RetailPrice =0,
                    IsPromo = false,
                    
                };
            var order2 = new OnlineOrderItem()
                {
                    Quantity = 1,
                    SKU = "1317",
                    Description = "",
                    RetailPrice = 0,
                    IsPromo = false,

                };
            Order.OrderItems= new OrderItems {order1, order2};
            Order.Shipment = new ShippingInfo_V01
                {
                    Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                        {
                            City = "眉山市",
                            Country = "cn"
                        },
                    ShippingMethodID = "22",
                    WarehouseCode = "3019"
                };
             Order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals_V02();
            GetQuotes(Order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            Cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.CheckDSPromo(Promotion, Cart);
            Assert.AreEqual(skuCollection[0].Quantity,3);
           
 
        }
         [TestMethod]
         public void ProcessOtherPromotionPC_Test()
         {
             var Promotion = new PromotionElement
               {
                   StartDate = "11-01-2015",
                   EndDate = "11-30-2015",
                   PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Other,
                   HasIncrementaldegree = true,
                   AmountMinInclude = 2000,
                   CustTypeList = new List<string>
                        {
                            "PC", "CS"
                        },
                   FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "376P"
                                }},
               };
             Promotion.Code = "00011";
             var Cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN1632369 ", "zh-CN", "20", "3019",
                                                                       new ShippingAddress_V02());
             Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

             var Order = new Order_V01();
             Order.DistributorID = "CN1632369";
             var order1 = new OnlineOrderItem()
               {
                   Quantity = 10,
                   SKU = "1316",
                   Description = "",
                   RetailPrice = 0,
                   IsPromo = false,

               };
             var order2 = new OnlineOrderItem()
                 {
                     Quantity = 1,
                     SKU = "1317",
                     Description = "",
                     RetailPrice = 0,
                     IsPromo = false,

                 };
             Order.OrderItems = new OrderItems { order1, order2 };
             Order.Shipment = new ShippingInfo_V01
                 {
                     Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                         {
                             City = "眉山市",
                             Country = "cn"
                         },
                     ShippingMethodID = "22",
                     WarehouseCode = "3019"
                 };
            
             Order.CountryOfProcessing = "CN";
             var ordertotal = new OrderTotals_V02();
             
             GetQuotes(Order, ordertotal, false, out ordertotal);
             var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
             Cart.Totals = ordertotal;
             FreeSKUCollection skuCollection = promorule.CheckPcPromo(Promotion, Cart);
             Assert.AreEqual(skuCollection[0].Quantity, 2);
         }

    
        private void GetQuotes(Order order, OrderTotals total, bool calcFreight, out OrderTotals_V02 totals)
        {
            var shoppingCartLaoder = Substitute.For<IShoppingCartProviderLoader>();
            shoppingCartLaoder.GetQuote(order, total, calcFreight).Returns(GetQuote(order, out totals, calcFreight));
          
        }
    
        private OrderTotals GetQuote(Order order, out OrderTotals_V02 total, bool calcFreight)
        {
            total = new OrderTotals_V02();
            var orderTotalsV02 = total;
            orderTotalsV02.VolumePoints = 600;
            orderTotalsV02.AmountDue = 4000;
            OrderFreight orderfreight = new OrderFreight
               {
                   FreightCharge = 0.0M,
               };
            
            total.OrderFreight = orderfreight;
          
            total.DiscountAmount = 0;
            total = orderTotalsV02;
            return total;
        }
        [TestMethod]
        public void ProcessVolumeJanPromotionDS_Test()
        {
            var Promotion = new PromotionElement
            {
                StartDate = "01-01-2016",
                EndDate = "01-31-2016",
                PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                HasIncrementaldegree = true,
                VolumeMinInclude = 200,
                CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "308P"
                                }},
            };
            Promotion.Code = "00012";
            var Cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

            var Order = new Order_V01();
            Order.DistributorID = "CN640521";
            var order1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            var order2 = new OnlineOrderItem()
            {
                Quantity = 1,
                SKU = "1317",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            Order.OrderItems = new OrderItems { order1, order2 };
            Order.Shipment = new ShippingInfo_V01
            {
                Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                {
                    City = "眉山市",
                    Country = "cn"
                },
                ShippingMethodID = "22",
                WarehouseCode = "3019"
            };
            Order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals_V02();
            GetQuotes(Order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            Cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.CheckDSPromo(Promotion, Cart);
            Assert.AreEqual(skuCollection[0].Quantity, 3);


        }
        [TestMethod]
        public void ProcessOtherJanPromotionPC_Test()
        {
            var Promotion = new PromotionElement
            {
                StartDate = "01-01-2016",
                EndDate = "01-31-2016",
                PromotionType = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType.Other,
                HasIncrementaldegree = true,
                AmountMinInclude = 2000,
                CustTypeList = new List<string>
                        {
                            "PC", "CS"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "308P"
                                }},
            };
            Promotion.Code = "00013";
            var Cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN1632369 ", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

            var Order = new Order_V01();
            Order.DistributorID = "CN1632369";
            var order1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            var order2 = new OnlineOrderItem()
            {
                Quantity = 1,
                SKU = "1317",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
            Order.OrderItems = new OrderItems { order1, order2 };
            Order.Shipment = new ShippingInfo_V01
            {
                Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address
                {
                    City = "眉山市",
                    Country = "cn"
                },
                ShippingMethodID = "22",
                WarehouseCode = "3019"
            };

            Order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals_V02();

            GetQuotes(Order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            Cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.CheckPcPromo(Promotion, Cart);
            Assert.AreEqual(skuCollection[0].Quantity, 2);
        }
        [TestMethod]
        public void ProcessVolumeMarchPromotionDS_Test()
        {
            var Promotion = new PromotionElement
            {
                StartDate = "03-01-2016",
                EndDate = "04-01-2016",
                PromotionType = ServiceProvider.OrderChinaSvc.PromotionType.Volume,
                HasIncrementaldegree = true,
                VolumeMinInclude = 200,
                CustTypeList = new List<string>
                        {
                            "DS", "FM", "SC", "SP", "SQ"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "560P"
                                }},
            };
            Promotion.Code = "00016";
            var Cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN640521", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

            var Order = new Order_V01();
            Order.DistributorID = "CN640521";
            var order1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
          
            Order.OrderItems = new OrderItems { order1 };
            Order.Shipment = new ShippingInfo_V01
            {
                Address = new ServiceProvider.OrderSvc.Address()
                {
                    City = "眉山市",
                    Country = "cn"
                },
                ShippingMethodID = "22",
                WarehouseCode = "3019"
            };
            Order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals_V02();
            GetQuotes(Order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            Cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.CheckDSPromo(Promotion, Cart);
            Assert.AreEqual(skuCollection[0].Quantity, 3);


        }
        [TestMethod]
        public void ProcessOtherMarchPromotionPC_Test()
        {
            var Promotion = new PromotionElement
            {
                StartDate = "01-01-2016",
                EndDate = "01-31-2016",
                PromotionType = ServiceProvider.OrderChinaSvc.PromotionType.Other,
                HasIncrementaldegree = true,
                AmountMinInclude = 2000,
                CustTypeList = new List<string>
                        {
                            "PC", "CS"
                        },
                FreeSKUList = new FreeSKUCollection
                        {
                            new FreeSKU
                                {
                                   Quantity = 1,
                                   SKU = "560P"
                                }},
            };
            Promotion.Code = "00017";
            var Cart = MyHLShoppingCartGenerator.GetBasicShoppingCart("CN1632369 ", "zh-CN", "20", "3019",
                                                                      new ShippingAddress_V02());
            Cart.CartItems = new ShoppingCartItemList
                {
                    ShoppingCartItemHelper.GetCartItem(1, 1, "1316"),
                    ShoppingCartItemHelper.GetCartItem(2, 1, "1317"),
                };

            var Order = new Order_V01();
            Order.DistributorID = "CN1632369";
            var order1 = new OnlineOrderItem()
            {
                Quantity = 10,
                SKU = "1316",
                Description = "",
                RetailPrice = 0,
                IsPromo = false,

            };
           
            Order.OrderItems = new OrderItems { order1 };
            Order.Shipment = new ShippingInfo_V01
            {
                Address = new ServiceProvider.OrderSvc.Address()
                {
                    City = "眉山市",
                    Country = "cn"
                },
                ShippingMethodID = "22",
                WarehouseCode = "3019"
            };

            Order.CountryOfProcessing = "CN";
            var ordertotal = new OrderTotals_V02();

            GetQuotes(Order, ordertotal, false, out ordertotal);
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
            Cart.Totals = ordertotal;
            FreeSKUCollection skuCollection = promorule.CheckPcPromo(Promotion, Cart);
            Assert.AreEqual(skuCollection[0].Quantity, 2);
        }

        [TestMethod]
        public void ValidateSku_Test()
        {
           
            var promorule = new MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules();
           
         var result = promorule.validateSKU("1111","3020" ,null,new ShoppingCartRuleResult());
         Assert.AreEqual(result, false);
        }
    }
}

