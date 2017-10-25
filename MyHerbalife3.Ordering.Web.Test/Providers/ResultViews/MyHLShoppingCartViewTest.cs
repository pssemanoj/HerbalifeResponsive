using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Web.Test.Providers.ResultViews
{
    public class MyHLShoppingCartViewTest
    {
        [TestClass]
        public class GetOrdersWithDetailMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
            }

            public MyHLShoppingCartView GetTarget(List<OnlineOrder> orders)
            {
                var chinaOrderProxy = Substitute.For<IChinaInterface>();

                chinaOrderProxy.GetOrdersWithDetail(new GetOrdersWithDetailRequest(Arg.Any<GetOrdersRequest_V02>())).GetOrdersWithDetailResult
                    .Returns(new GetOrdersResponse_V01()
                    {
                        Status = ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success,
                        Orders = orders
                    });

                var catalogLoader = Substitute.For<ICatalogProviderLoader>();

                catalogLoader.GetCatalog("CN").Returns(GetChinaCatalog());

                var target = new MyHLShoppingCartView(chinaOrderProxy, catalogLoader);

                return target;
            }

            private Catalog_V01 GetChinaCatalog()
            {
                Catalog_V01 result;

                result = new Catalog_V01();
                result.CountryCode = "CN";
                result.Items = new CatalogItemList();
                result.Items.Add(HLConfigManager.Configurations.APFConfiguration.DistributorSku, new CatalogItem() { SKU = HLConfigManager.Configurations.APFConfiguration.DistributorSku, IsInventory = false });

                return result;
            }

            private List<OnlineOrder> SingleOrderWithEmptyOrderItemData(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo();
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 0m,
                };
                order.OrderItems = new OrderItems();
                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";

                result.Add(order);
                return result;
            }

            private List<OnlineOrder> SingleOrderWithSingleAPFCartItem(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo_V01();
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 0m,
                };
                order.OrderItems = new OrderItems();

                OnlineOrderItem item = new OnlineOrderItem();
                item.SKU = HLConfigManager.Configurations.APFConfiguration.DistributorSku;
                item.Quantity = 1;
                item.Description = "APF";
                item.RetailPrice = 70m;
                order.OrderItems.Add(item);

                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";

                result.Add(order);
                return result;
            }

            private List<OnlineOrder> SingleOrderWithProductAndAPFCartItem(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo_V01();
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 0m,
                };
                order.OrderItems = new OrderItems();

                OnlineOrderItem item = new OnlineOrderItem();
                item.SKU = HLConfigManager.Configurations.APFConfiguration.DistributorSku;
                item.Quantity = 1;
                item.Description = "APF";
                item.RetailPrice = 70m;
                order.OrderItems.Add(item);

                item = new OnlineOrderItem();
                item.SKU = "1316";
                item.Quantity = 2;
                item.Description = "F1";
                item.RetailPrice = 316m;
                order.OrderItems.Add(item);

                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";

                result.Add(order);
                return result;
            }
            private List<OnlineOrder> OrderWithDonationAmount(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo();
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 5m,
                    SelfDonationAmount=2m,
                    OnBehalfDonationAmount=3m,
                    OnBehalfDonationContact="",
                    OnBehalfDonationName="xxxx",
                    SelfDonationMemberId="CN640521"
                };
                order.OrderItems = new OrderItems();
                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";

                result.Add(order);
                return result;
            }
            [TestMethod]
            public void OrderwithDonation()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "CN640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = OrderWithDonationAmount(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetOrdersWithDetail(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                Assert.IsTrue(result != null && result.Count == 1 && result[0].DonationAmount == 5);
            }

            [TestMethod]
            public void OrderItems_Empty()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "DS640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = SingleOrderWithEmptyOrderItemData(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetOrdersWithDetail(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                Assert.IsTrue(result != null && result.Count == 1);
            }

            [TestMethod]
            public void OrderItems_ContainsAPF_NoCopyAllowedAndNoFeedbackAllowed()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "DS640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = SingleOrderWithSingleAPFCartItem(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetOrdersWithDetail(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                Assert.IsTrue(result != null && result.Count == 1 && result[0].IsCopyEnabled == false && result[0].HasFeedBack == true);
            }

            [TestMethod]
            public void OrderItems_ContainsAPF_CopyAllowedAndFeedbackAllowed()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "DS640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = SingleOrderWithProductAndAPFCartItem(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetOrdersWithDetail(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                Assert.IsTrue(result != null && result.Count == 1 && result[0].IsCopyEnabled == true && result[0].HasFeedBack == false);
            }
        }

        [TestClass]
        public class GetPreOrdersMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
            }

            public MyHLShoppingCartView GetTarget(List<OnlineOrder> orders)
            {
                var chinaOrderProxy = Substitute.For<IChinaInterface>();

                chinaOrderProxy.GetPreOrders(new GetPreOrdersRequest1(Arg.Any<GetPreOrdersRequest_V01>())).GetPreOrdersResult
                    .Returns(new GetPreOrdersResponse_V01()
                    {
                        Status = ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success,
                        Orders = orders
                    });

                var orderDetail = PreOrderDetail(orders.FirstOrDefault());

                var orderChinaProvider = Substitute.For<IOrderChinaProviderLoader>();
                orderChinaProvider.GetPreOrderDetail(Arg.Any<string>(), Arg.Any<int>()).Returns(orderDetail);

                var target = new MyHLShoppingCartView(chinaOrderProxy, null, orderChinaProvider);

                return target;
            }

            private List<OnlineOrder> SingleOrderWithEmptyOrderItemData(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.OrderHeaderID = 456;
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo_V01()
                {
                    Phone = "11111",
                    Recipient = "Tester",
                    Address = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address_V01()
                    {
                        Line1 = "Line1",
                        Line2 = "Line2",
                        Line3 = "Line3",
                        Line4 = "Line4",
                        City = "City",
                        CountyDistrict = "District",
                        PostalCode = "12345",
                        Country = "CN",
                        StateProvinceTerritory = "Province",
                    },
                };
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 0m,
                    ItemsTotal = 319m,
                    
                };
                order.OrderItems = new OrderItems();
                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";

                result.Add(order);
                return result;
            }

            private List<OnlineOrder> SingleOrderWithProduct(string countryCode, string distributorId, DateTime startDate)
            {
                List<OnlineOrder> result = new List<OnlineOrder>();

                OnlineOrder order = new OnlineOrder();
                order.OrderHeaderID = 123;
                order.ChannelInfo = "网上订单";
                order.StoreInfo = "";
                order.Status = "待付款";
                order.Shipment = new ShippingInfo_V01()
                {
                    Phone = "11111",
                    Recipient = "Tester",
                    Address = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address_V01()
                    {
                        Line1 = "Line1",
                        Line2 = "Line2",
                        Line3 = "Line3",
                        Line4 = "Line4",
                        City = "City",
                        CountyDistrict = "District",
                        PostalCode = "12345",
                        Country = "CN",
                        StateProvinceTerritory = "Province",
                    }
                };
                order.Handling = new HandlingInfo();
                order.Pricing = new OrderTotals_V02()
                {
                    VolumePoints = 100,
                    AmountDue = 319.00m,
                    ProductTaxTotal = 0m,
                    DiscountAmount = 0m,
                    Donation = 0m,
                    ItemsTotal = 319m,
                };
                order.OrderItems = new OrderItems();

                OnlineOrderItem item = new OnlineOrderItem();
                item = new OnlineOrderItem();
                item.SKU = "1316";
                item.Quantity = 2;
                item.Description = "F1";
                item.RetailPrice = 316m;
                order.OrderItems.Add(item);

                order.CountryOfProcessing = countryCode;
                order.CustomerID = "";
                order.DiscountPercentage = 0;
                order.DistributorID = distributorId;
                order.Messages = new MessageCollection();
                order.InputMethod = InputMethodType.Internet;
                order.OrderCategory = ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                order.OrderID = "";
                order.Payments = new PaymentCollection();
                order.PurchaseCategory = OrderPurchaseType.None;
                order.PurchasingLimits = new PurchasingLimits();
                order.QualifyingSupervisorID = "";
                order.ReceivedDate = startDate.AddDays(2);
                order.OrderMonth = order.ReceivedDate.ToString("yyyyMM");
                order.ReferenceID = "";
                order.SessionId = "";
                order.ShoppingCartID = 123;
                order.SponsorId = "";
                order.TransactionId = "";
                order.UseSlidingScale = false;
                order.VATRegistrationId = "";
               
                result.Add(order);
                return result;
            }
            

            private OnlineOrder PreOrderDetail(OnlineOrder order)
            {
                var result = new OnlineOrder();
                result.OrderHeaderID = order.OrderHeaderID;
                result.DistributorID = order.DistributorID;
                result.ReceivedDate = order.ReceivedDate;
                result.OrderMonth = order.OrderMonth;
                result.OrderCategory = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderCategoryType.RSO;
                result.CountryOfProcessing = "CN";
                result.InputMethod = InputMethodType.Internet;
                result.NTSDate = order.ReceivedDate;
                result.Shipment = order.Shipment;
                result.OrderItems = new OrderItems();
                var orderItem = new OrderItem_V02
                {
                    Quantity = 1,
                    SKU = "1316",
                    Description = "F1",
                };
                result.OrderItems.Add(orderItem);

                return result;
            }

            [TestMethod]
            public void OrderItems_Empty_NoRecordReturned()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "DS640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = SingleOrderWithEmptyOrderItemData(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetPreOrders(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                //TODO: fix and make it able to return single record.
                Assert.IsTrue(result != null && result.Count == 1);
            }

            [TestMethod]
            public void OrderItems_SingleItem_NoCopyAllowed()
            {
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.Now;
                string distributorId = "DS640521";
                int customerProfileID = 0;
                string countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType filterType = MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType.All;
                string filterExpression = "";
                string sortExpression = "";

                var orders = SingleOrderWithProduct(countryCode, distributorId, startDate);
                var target = GetTarget(orders);
                var result = target.GetPreOrders(distributorId, customerProfileID, countryCode, startDate, endDate, filterType, filterExpression, sortExpression);

                Assert.IsTrue(result != null && result.Count == 1 && result[0].IsCopyEnabled == false);
            }
           

        }
    }
}
