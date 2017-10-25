using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Shared.ViewModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace MyHerbalife3.Ordering.Web.Test.Providers.Payments
{
    [TestClass]
    public class CN_99BillQuickPayProviderTest
    {
        [TestClass]
        public class CheckBindedCardMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");

                HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));
                HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("CN175803"), new string[0]);
            }

            public CN_99BillQuickPayProvider GetTarget(string distributor, string locale, string responseMessage, List<PaymentInformation> paymentInfo, MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType responseStatus = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success)
            {
                var chinaOrderProxy = Substitute.For<IChinaInterface>();

                chinaOrderProxy.GetCnpPaymentServiceDetail(new GetCnpPaymentServiceDetailRequest(Arg.Any<GetCnpPaymentServiceRequest_V02>())).GetCnpPaymentServiceDetailResult
                    .Returns(new GetCNPPaymentServiceResponse_V01()
                    {
                        Status = responseStatus,
                        Response = responseMessage,
                    });

                var paymentInfoProviderLoader = Substitute.For<IPaymentInfoProviderLoader>();

                paymentInfoProviderLoader.GetPaymentInfoForQuickPay(distributor, locale).Returns(paymentInfo);

                var target = new CN_99BillQuickPayProvider(chinaOrderProxy, paymentInfoProviderLoader);

                return target;
            }

            private string CheckBindCardResponseString_WithBindedCard()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"><PciQueryContent><merchantId>104110045112012</merchantId><customerId>CN175803</customerId><storablePan>6228488276</storablePan><bankId>ABC</bankId><cardType>0002</cardType><pciInfos><pciInfo><bankId>ABC</bankId><storablePan>6228488276</storablePan><shortPhoneNo>1592570</shortPhoneNo><phoneNO>15901822570</phoneNO></pciInfo></pciInfos><responseCode>00</responseCode></PciQueryContent></MasMessage>";
            }

            private string CheckBindCardResponseString_WithoutBindedCard()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"><PciQueryContent><customerId>CN175803</customerId><storablePan>6228488276</storablePan><bankId>ABC</bankId><cardType>0002</cardType><responseCode>00</responseCode></PciQueryContent></MasMessage>";
            }

            private string CheckBindCardResponseString_Error()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"><version>1.0</version><ErrorMsgContent><errorCode>B.MGW.0120</errorCode><errorMessage>[merchantId] is empty</errorMessage></ErrorMsgContent></MasMessage>";
            }

            private List<PaymentInformation> GetPaymentInformation_WithStorableCardNumber()
            {
                List<PaymentInformation> result = new List<PaymentInformation>();
                PaymentInformation itm = new PaymentInformation();
                itm.Alias = "BOC";
                itm.CardNumber = "1000833612";
                itm.CardType = "QD";
                itm.ID = 123;
                itm.BillingAddress = new ServiceProvider.OrderSvc.Address_V01();
                itm.BillingAddress.Line3 = "15901822570";
                itm.CardHolder = new ServiceProvider.OrderSvc.Name_V01();
                itm.CardHolder.First = "张梁";
                itm.CardHolder.Last = "";
                result.Add(itm);

                itm = new PaymentInformation();
                itm.Alias = "ABC";
                itm.CardNumber = "6228488276";
                itm.CardType = "QD";
                itm.ID = 124;
                itm.BillingAddress = new ServiceProvider.OrderSvc.Address_V01();
                itm.BillingAddress.Line3 = "15901822570";
                itm.CardHolder = new ServiceProvider.OrderSvc.Name_V01();
                itm.CardHolder.First = "张梁";
                itm.CardHolder.Last = "";
                result.Add(itm);

                return result;
            }

            private List<PaymentInformation> GetPaymentInformation_WithoutStorableCardNumber()
            {
                return null;
            }

            [TestMethod]
            public void NoBindedWithoutStorableCardNumber_False()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_WithoutBindedCard();
                var paymentInfo = GetPaymentInformation_WithoutStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(!result);
            }

            [TestMethod]
            public void NoBindedWithStorableCardNumber_False()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_WithBindedCard();
                var paymentInfo = GetPaymentInformation_WithoutStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(!result);
            }

            [TestMethod]
            public void BindedWithoutStorableCardNumber_False()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_WithBindedCard();
                var paymentInfo = GetPaymentInformation_WithoutStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(!result);
            }

            [TestMethod]
            public void BindedWithStorableCardNumber_True()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_WithBindedCard();
                var paymentInfo = GetPaymentInformation_WithStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(result);
            }

            [TestMethod]
            public void BindedWithErrorResponse_False()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_Error();
                var paymentInfo = GetPaymentInformation_WithStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(!result);
            }

            [TestMethod]
            public void BindedWithFailedWCFResponse_False()
            {
                string distributorId = "CN175803";
                string phoneNumber = "15901822570";
                var response = CheckBindCardResponseString_WithBindedCard();
                var paymentInfo = GetPaymentInformation_WithStorableCardNumber();
                var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, paymentInfo, MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Failure);

                var result = target.CheckBindedCard("ABC", "0002", phoneNumber);

                Assert.IsTrue(!result);
            }
        }

        [TestClass]
        public class RequestMobilePinForPurchaseMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");

                HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));
                HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("CN175803"), new string[0]);
            }

            //public CN_99BillQuickPayProvider GetTarget(string distributor, string locale, string responseMessage, Order_V01 order, MyHLShoppingCart cartInfo, string orderNumber, HL.Common.DataContract.Interfaces.ServiceResponseStatusType responseStatus = HL.Common.DataContract.Interfaces.ServiceResponseStatusType.Success)
            //{
            //    var chinaOrderProxy = Substitute.For<IChinaInterface>();

            //    chinaOrderProxy.GetCnpPaymentServiceDetail(Arg.Any<HL.Order.ValueObjects.China.OnlineOrder.GetCnpPaymentServiceRequest_V02>())
            //        .Returns(new HL.Order.ValueObjects.China.OnlineOrder.GetCnpPaymentServiceResponse_V01()
            //        {
            //            Status = responseStatus,
            //            Response = responseMessage,
            //        });

            //    var distributorOrderingProfileProviderLoader = Substitute.For<IDistributorOrderingProfileProviderLoader>();
            //    distributorOrderingProfileProviderLoader.GetProfile(distributor, "CN").Returns(GetDistributorProfile());

            //    var catalogProviderLoader = Substitute.For<ICatalogProviderLoader>();
            //    //catalogProviderLoader.GetCatalogItems(new List<string>() { "1316", "9143" }, "CN").Returns(GetCatalogItemList());
            //    catalogProviderLoader.GetCatalogItems(Arg.Any<List<string>>(), "CN").Returns(GetCatalogItemList());
            //    catalogProviderLoader.IsPreordering(Arg.Any<HL.Catalog.ValueObjects.ShoppingCartItemList>(), Arg.Any<string>()).Returns(false);

            //    var emailHelperLoader = Substitute.For<IEmailHelperLoader>();
            //    emailHelperLoader.GetEmailFromOrder(order, cartInfo, locale, cartInfo.DeliveryInfo.Address.Recipient, cartInfo.DeliveryInfo).Returns(GetEmailForOrder());

            //    var orderProviderLoader = Substitute.For<IOrderProviderLoader>();
            //    orderProviderLoader.InsertPaymentGatewayRecord(orderNumber, order.DistributorID, "CN_99BillPaymentGateway", Arg.Any<string>(), locale).Returns(123123);
            //    orderProviderLoader.UpdatePaymentGatewayRecord(orderNumber, Arg.Any<string>(), Arg.Any<PaymentGatewayLogEntryType>(), Arg.Any<PaymentGatewayRecordStatusType>());

            //    var target = new CN_99BillQuickPayProvider(chinaOrderProxy, null, distributorOrderingProfileProviderLoader, catalogProviderLoader, emailHelperLoader, orderProviderLoader);

            //    return target;
            //}

            private string RequestMobilePinForPurchaseResponseString_InvalidCard()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage><GetDynNumContent><merchantId>104110045112012</merchantId><customerId>CN175803</customerId><storablePan>1111111111</storablePan><responseCode>L9</responseCode><responseTextMessage>错误的卡号校验位</responseTextMessage></GetDynNumContent></MasMessage>";
            }

            private string RequestMobilePinForPurchaseResponseString_Success()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage><GetDynNumContent><merchantId>104110045112012</merchantId><customerId>CN175803</customerId><storablePan>4380884390</storablePan><token>1213924</token><responseCode>00</responseCode></GetDynNumContent></MasMessage>";
            }

            //private DistributorOrderingProfile GetDistributorProfile()
            //{
            //    DistributorOrderingProfile result = new DistributorOrderingProfile();
            //    result.Id = "CN175803";
            //    result.ApfDueDate = DateTime.Now.AddYears(1);
            //    result.ApplicationDate = DateTime.Now.AddYears(-1);
            //    result.TinList = new List<HL.Common.ValueObjects.TaxIdentification>();

            //    HL.Common.ValueObjects.TaxIdentification taxIdentification = new HL.Common.ValueObjects.TaxIdentification();
            //    taxIdentification.ID = "CNID";
            //    taxIdentification.IDType = new HL.Common.ValueObjects.TaxIdentificationType("CNID");
            //    taxIdentification.IDType.Key = "330602198112090011";
            //    taxIdentification.IDType.Description = "National ID";
            //    taxIdentification.IDType.IsDefault = true;
            //    taxIdentification.IDType.EffectiveDate = DateTime.Now.AddYears(-1);
            //    taxIdentification.IDType.ExpirationDate = DateTime.Now.AddYears(1);
            //    taxIdentification.CountryCode = "CN";

            //    result.TinList.Add(taxIdentification);
            //    result.OrderSubType = "E";
            //    result.ShowAllInventory = true;
            //    result.BirthDate = new DateTime(1985, 11, 7);
            //    result.CurrentLoggedInCountry = "CN";

            //    result.Addresses = new HL.Common.ValueObjects.AddressCollection();
            //    result.DistributorVolumes = new List<HL.Distributor.ValueObjects.DistributorVolume_V01>();
            //    HL.Distributor.ValueObjects.DistributorVolume_V01 volume = new HL.Distributor.ValueObjects.DistributorVolume_V01();
            //    volume.VolumeMonth = "2015/10";
            //    volume.VolumeDate = DateTime.Now;
            //    result.DistributorVolumes.Add(volume);

            //    volume = new HL.Distributor.ValueObjects.DistributorVolume_V01();
            //    volume.VolumeMonth = "2015/11";
            //    volume.VolumeDate = DateTime.Now;
            //    result.DistributorVolumes.Add(volume);

            //    result.ReferenceNumber = "276720,409,SQ,SQ1,浙江 ,3,True,True,1";
            //    result.CNCustomorProfileID = 276720;
            //    result.CNStoreID = 409;
            //    result.CNStoreProvince = "浙江";
            //    result.CNCustType = "SQ";
            //    result.CNCustCategoryType = "SQ1";
            //    result.CNAPFStatus = 3;
            //    result.PhoneNumbers = new List<HL.Common.ValueObjects.PhoneNumber_V03>();
            //    HL.Common.ValueObjects.PhoneNumber_V03 phone = new HL.Common.ValueObjects.PhoneNumber_V03();
            //    phone.Number = "15901822570";
            //    result.PhoneNumbers.Add(phone);
            //    result.IsPayByPhoneEnabled = true;
            //    result.SPQualificationDate = DateTime.Now.AddYears(-1);
            //    result.SpouseLocalName = "";

            //    return result;
            //}

            private MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 GetOrder_InvalidCard()
            {
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 result = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01();

                result.CountryOfProcessing = "CN";
                result.Handling = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HandlingInfo_V01() { ShippingInstructions = "工作日、双休日与假日均可送货" };
                result.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                result.OrderItems = new ServiceProvider.OrderSvc.OrderItems();
                var itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "蛋白混合饮料-香草口味550克";
                itm.RetailPrice = 329.0m;
                itm.VolumePoint = 23.9500m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "1316";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "康宝莱生活方式三折页";
                itm.RetailPrice = 1m;
                itm.VolumePoint = 0m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "9143";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                result.OrderMonth = "1511";

                result.Payments = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection();
                var payment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01();
                var card = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.QuickPayPayment();
                card.CardHolderId = "330602198112090011";
                card.CardHolderType = "0";
                card.MobilePhoneNumber = "15901822570";
                card.AccountNumber = "vOQLPuoOZTyFIwqNcDf324PgmRZf+R5eEE3/l5nqbXw=";
                card.Expiration = new DateTime(2016, 11, 1);
                card.CVV = "+J8VQVayiy4IwiI9vkjNFw==";
                card.NameOnCard = "张梁";
                card.IssuingBankID = "BCOM";
                payment.Card = card;
                payment.AuthorizationMethod = 0;
                payment.AuthorizationMerchantAccount = "BCOM";
                payment.Amount = 239m;
                payment.Currency = "RMB";
                payment.TransactionType = "QP";
                result.Payments.Add(payment);

                result.PurchasingLimits = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PurchasingLimits();
                result.ReceivedDate = DateTime.Now;

                var shipment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01();
                shipment.Recipient = "Boon";
                shipment.Phone = "11111111111";
                shipment.ShippingMethodID = "27";
                shipment.WarehouseCode = "3019";
                shipment.FreightVariant = "EXP";
                shipment.Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01();
                shipment.Address.Line1 = "Jalan jalan";
                shipment.Address.Line2 = "276720";
                shipment.Address.Line3 = "11579709";
                shipment.Address.Line4 = "DL09";
                shipment.Address.City = "昆明市";
                shipment.Address.CountyDistrict = "市辖区";
                shipment.Address.StateProvinceTerritory = "云南";
                shipment.Address.Country = "CN";
                shipment.Address.PostalCode = "111111";
                shipment.DeliveryNickName = "Boon10";
                result.Shipment = shipment;

                result.DistributorID = "CN175803";

                return result;
            }

            private MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 GetOrder_NewCard()
            {
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 result = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01();

                result.CountryOfProcessing = "CN";
                result.Handling = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HandlingInfo_V01() { ShippingInstructions = "工作日、双休日与假日均可送货" };
                result.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                result.OrderItems = new ServiceProvider.OrderSvc.OrderItems();
                var itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "蛋白混合饮料-香草口味550克";
                itm.RetailPrice = 329.0m;
                itm.VolumePoint = 23.9500m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "1316";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "康宝莱生活方式三折页";
                itm.RetailPrice = 1m;
                itm.VolumePoint = 0m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "9143";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                result.OrderMonth = "1511";

                result.Payments = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection();
                var payment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01();
                var card = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.QuickPayPayment();
                card.CardHolderId = "330602198112090011";
                card.CardHolderType = "0";
                card.MobilePhoneNumber = "15901822570";
                card.AccountNumber = "7PfIVTKbvYAd7mocRHev3pjwYIX0mM0z2CIkbTgSgeA=";
                card.Expiration = new DateTime(2016, 11, 1);
                card.CVV = "+J8VQVayiy4IwiI9vkjNFw==";
                card.NameOnCard = "张梁";
                card.IssuingBankID = "BOC";
                payment.Card = card;
                payment.AuthorizationMethod = 0;
                payment.AuthorizationMerchantAccount = "BOC";
                payment.Amount = 239m;
                payment.Currency = "RMB";
                payment.TransactionType = "QP";
                result.Payments.Add(payment);

                result.PurchasingLimits = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PurchasingLimits();
                result.ReceivedDate = DateTime.Now;

                var shipment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01();
                shipment.Recipient = "Boon";
                shipment.Phone = "11111111111";
                shipment.ShippingMethodID = "27";
                shipment.WarehouseCode = "3019";
                shipment.FreightVariant = "EXP";
                shipment.Address = new ServiceProvider.OrderSvc.Address();
                shipment.Address.Line1 = "Jalan jalan";
                shipment.Address.Line2 = "276720";
                shipment.Address.Line3 = "11579709";
                shipment.Address.Line4 = "DL09";
                shipment.Address.City = "昆明市";
                shipment.Address.CountyDistrict = "市辖区";
                shipment.Address.StateProvinceTerritory = "云南";
                shipment.Address.Country = "CN";
                shipment.Address.PostalCode = "111111";
                shipment.DeliveryNickName = "Boon10";
                result.Shipment = shipment;

                result.DistributorID = "CN175803";

                return result;
            }

            private MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 GetOrder_BindedCard()
            {
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 result = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01();

                result.CountryOfProcessing = "CN";
                result.Handling = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HandlingInfo_V01() { ShippingInstructions = "工作日、双休日与假日均可送货" };
                result.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                result.OrderItems = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItems();
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem itm = new ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "蛋白混合饮料-香草口味550克";
                itm.RetailPrice = 329.0m;
                itm.VolumePoint = 23.9500m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "1316";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                itm = new ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "康宝莱生活方式三折页";
                itm.RetailPrice = 1m;
                itm.VolumePoint = 0m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "9143";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                result.OrderMonth = "1511";

                result.Payments = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection();
                var payment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01();
                var card = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.QuickPayPayment();
                card.CardHolderId = "330602198112090011";
                card.CardHolderType = "0";
                card.MobilePhoneNumber = "15901822570";
                card.AccountNumber = "yX+yCQuJE5wAXqupHxzBAw==";
                card.IssuingBankID = "BOC";
                card.IsDebitCard = true;
                payment.Card = card;
                payment.AuthorizationMethod = 0;
                payment.AuthorizationMerchantAccount = "BOC";
                payment.Amount = 239m;
                payment.Currency = "RMB";
                payment.TransactionType = "QP";
                result.Payments.Add(payment);

                result.PurchasingLimits = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PurchasingLimits();
                result.ReceivedDate = DateTime.Now;

                var shipment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01();
                shipment.Recipient = "Boon";
                shipment.Phone = "11111111111";
                shipment.ShippingMethodID = "27";
                shipment.WarehouseCode = "3019";
                shipment.FreightVariant = "EXP";
                shipment.Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01();
                shipment.Address.Line1 = "Jalan jalan";
                shipment.Address.Line2 = "276720";
                shipment.Address.Line3 = "11579709";
                shipment.Address.Line4 = "DL09";
                shipment.Address.City = "昆明市";
                shipment.Address.CountyDistrict = "市辖区";
                shipment.Address.StateProvinceTerritory = "云南";
                shipment.Address.Country = "CN";
                shipment.Address.PostalCode = "111111";
                shipment.DeliveryNickName = "Boon10";
                result.Shipment = shipment;

                result.DistributorID = "CN175803";

                return result;
            }

            //private MyHLShoppingCart GetShoppingCart()
            //{
            //    MyHLShoppingCart result = new MyHLShoppingCart();
            //    result.CartItems = new HL.Catalog.ValueObjects.ShoppingCartItemList();
            //    HL.Catalog.ValueObjects.ShoppingCartItem_V01 cartItem = new HL.Catalog.ValueObjects.ShoppingCartItem_V01();
            //    cartItem.Quantity = 1;
            //    cartItem.SKU = "1316";
            //    cartItem.Updated = DateTime.Now;
            //    result.CartItems.Add(cartItem);

            //    cartItem = new HL.Catalog.ValueObjects.ShoppingCartItem_V01();
            //    cartItem.Quantity = 1;
            //    cartItem.SKU = "9143";
            //    cartItem.Updated = DateTime.Now;
            //    result.CartItems.Add(cartItem);

            //    result.CartName = "";
            //    result.CountryCode = "CN";

            //    result.DeliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo();
            //    result.DeliveryInfo.Address = new HL.Shipping.ValueObjects.ShippingAddress_V01();
            //    result.DeliveryInfo.Address.Address = new HL.Common.ValueObjects.Address_V01();
            //    result.DeliveryInfo.Address.Address.City = "昆明市";
            //    result.DeliveryInfo.Address.Address.Country = "CN";
            //    result.DeliveryInfo.Address.Address.CountyDistrict = "市辖区";
            //    result.DeliveryInfo.Address.Address.Line1 = "Jalan jalan";
            //    result.DeliveryInfo.Address.Address.Line2 = "276720";
            //    result.DeliveryInfo.Address.Address.Line3 = "11579709";
            //    result.DeliveryInfo.Address.Address.Line4 = "DL09";
            //    result.DeliveryInfo.Address.Address.PostalCode = "111111";
            //    result.DeliveryInfo.Address.Address.StateProvinceTerritory = "云南";
            //    result.DeliveryInfo.Address.Alias = "Boon10";
            //    result.DeliveryInfo.Address.AltAreaCode = "";
            //    result.DeliveryInfo.Address.AltPhone = "15901822570";
            //    result.DeliveryInfo.Address.AreaCode = "";
            //    result.DeliveryInfo.Address.Attention = new HL.Common.ValueObjects.Name_V01();
            //    result.DeliveryInfo.Address.Created = DateTime.Now;
            //    result.DeliveryInfo.Address.DisplayName = "Boon10";
            //    result.DeliveryInfo.Address.ID = 11579709;
            //    result.DeliveryInfo.Address.IsPrimary = true;
            //    result.DeliveryInfo.Option = new HL.Common.ValueObjects.DeliveryOptionType();
            //    result.DeliveryInfo.Address.Phone = "11111111111";
            //    result.DeliveryInfo.Address.Recipient = "Boon";
            //    result.DeliveryInfo.AddressType = "EXP";
            //    result.DeliveryInfo.DeliveryNickName = "Boon10";
            //    result.DeliveryInfo.Description = "";
            //    result.DeliveryInfo.FreightCode = "27";
            //    result.DeliveryInfo.Instruction = "工作日、双休日与假日均可送货";
            //    result.DeliveryInfo.WarehouseCode = "3019";
            //    result.DeliveryInfo.Option = HL.Common.ValueObjects.DeliveryOptionType.Shipping;
            //    result.DeliveryOption = HL.Common.ValueObjects.DeliveryOptionType.Shipping;
            //    result.DistributorID = "CN175803";
            //    result.EmailAddress = "geewaib@herbalife.com";
            //    result.EmailValues = new HLShoppingCartEmailValues();
            //    result.EmailValues.CurrentMonthVolume = "0";
            //    result.EmailValues.DistributorSubTotal = 330m;
            //    result.EmailValues.DistributorSubTotalFormatted = "¥330.00";
            //    result.FirstOrderPromotionTypes = new List<HL.Order.ValueObjects.China.PromotionType>();
            //    result.FreightCode = "27";
            //    result.LastUpdated = DateTime.Now;
            //    result.LastUpdatedUtc = DateTime.UtcNow;
            //    result.Locale = "zh-CN";
            //    result.OrderCategory = HL.Common.ValueObjects.OrderCategoryType.RSO;
            //    result.OrderMonth = 201511;
            //    result.OrderSubType = "";
            //    result.PassDSFraudValidation = true;
            //    result.SMSNotification = "11111111111";
            //    result.SelectedDSSubType = "";
            //    result.ShippingAddressID = 11579709;
            //    result.ShoppingCartID = 12839027;
            //    result.ShoppingCartItems = new List<Shared.Providers.DistributorShoppingCartItem>();

            //    var item = new MyHerbalife3.Shared.Providers.DistributorShoppingCartItem();
            //    item.CatalogItem = new HL.Catalog.ValueObjects.CatalogItem_V01();
            //    item.CatalogItem.Description = "蛋白混合饮料-香草口味 550克";
            //    item.DiscountPrice = 329.0m;
            //    item.EarnBase = 0m;
            //    item.ErrorMessage = "";
            //    item.Flavor = "香草口味 550克";
            //    item.ParentCat = new HL.Catalog.ValueObjects.Category_V02();
            //    item.ProdInfo = new HL.Catalog.ValueObjects.ProductInfo_V02();
            //    item.Quantity = 1;
            //    item.RetailPrice = 329.0m;
            //    item.SKU = "1316";
            //    item.Updated = DateTime.Now;
            //    item.VolumePoints = 23.95m;
            //    result.ShoppingCartItems.Add(item);

            //    item = new MyHerbalife3.Shared.Providers.DistributorShoppingCartItem();
            //    item.CatalogItem = new HL.Catalog.ValueObjects.CatalogItem_V01();
            //    item.CatalogItem.Description = "康宝莱生活方式三折页";
            //    item.DiscountPrice = 1m;
            //    item.EarnBase = 0m;
            //    item.ErrorMessage = "";
            //    item.Flavor = "康宝莱生活方式三折页";
            //    item.ParentCat = new HL.Catalog.ValueObjects.Category_V02();
            //    item.ProdInfo = new HL.Catalog.ValueObjects.ProductInfo_V02();
            //    item.Quantity = 1;
            //    item.RetailPrice = 1.0m;
            //    item.SKU = "9143";
            //    item.Updated = DateTime.Now;
            //    item.VolumePoints = 0m;
            //    result.ShoppingCartItems.Add(item);

            //    var total = new HL.Order.ValueObjects.China.OrderTotals_V02();
            //    total.AmountDue = 239.0m;

            //    total.ChargeList = new ChargeList();

            //    var chargeList = new HL.Order.ValueObjects.Charge_V01();
            //    chargeList.Amount = 10m;
            //    chargeList.AmountForFairMarketValue = FairMarketValue.UseStandard;
            //    chargeList.ChargeType = ChargeTypes.FREIGHT;
            //    chargeList.DiscountedAmount = 101m;
            //    chargeList.ItemDiscountedAmount = 330m;
            //    total.ChargeList.Add(chargeList);

            //    total.ItemTotalsList = new HL.Order.ValueObjects.ItemTotalsList();
            //    var totalItem = new HL.Order.ValueObjects.ItemTotal_V01();
            //    totalItem.AfterDiscountTax = 47.80m;
            //    totalItem.DiscountedPrice = 329.0m;
            //    totalItem.LinePrice = 329m;
            //    totalItem.LineTax = 47.803422m;
            //    totalItem.ProductType = HL.Common.ValueObjects.ProductType.None;
            //    totalItem.Quantity = 1;
            //    totalItem.RetailPrice = 329.0m;
            //    totalItem.SKU = "1316";
            //    totalItem.TaxableAmount = 281.1966m;
            //    totalItem.VolumePoints = 23.9500m;
            //    total.ItemTotalsList.Add(totalItem);

            //    totalItem = new HL.Order.ValueObjects.ItemTotal_V01();
            //    totalItem.AfterDiscountTax = 47.80m;
            //    totalItem.Discount = 0.8547m;
            //    totalItem.DiscountedPrice = 1m;
            //    totalItem.LinePrice = 1m;
            //    totalItem.LineTax = 0.145299m;
            //    totalItem.ProductType = HL.Common.ValueObjects.ProductType.None;
            //    totalItem.Quantity = 1;
            //    totalItem.RetailPrice = 329.0m;
            //    totalItem.SKU = "9143";
            //    totalItem.TaxableAmount = 0.8547m;
            //    totalItem.VolumePoints = 0m;
            //    total.ItemTotalsList.Add(totalItem);
            //    total.ItemsTotal = 330m;
            //    total.MiscAmount = 100.8547m;
            //    total.OrderFreight = new HL.Order.ValueObjects.China.OrderFreight();
            //    total.OrderFreight.ActualFreight = 9m;
            //    total.OrderFreight.BeforeDiscountFreight = 9m;
            //    total.OrderFreight.BeforeWeight = 2;
            //    total.OrderFreight.CaseRate = 1.13636m;
            //    total.OrderFreight.FreightCharge = 10m;
            //    total.OrderFreight.MaterialFee = 1m;
            //    total.OrderFreight.PackageWeight = 0.19m;
            //    total.OrderFreight.Packages = new List<HL.Order.ValueObjects.China.Package>();

            //    var package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "A";
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "B";
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "C";
            //    package.Unit = 1;
            //    package.Volume = 0.0078045m;
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "X";
            //    total.OrderFreight.Packages.Add(package);

            //    total.OrderFreight.PhysicalWeight = 0.84400m;
            //    total.OrderFreight.VolumeWeight = 1.30075m;
            //    total.OrderFreight.Weight = 0.65800m;
            //    total.PricingServerName = "MYKL-DTS-11";
            //    total.ProductRetailAmount = 229.0m;
            //    total.ProductTaxTotal = 182.0513m;
            //    total.PromotionRetailAmount = 0.8547m;
            //    total.QuoteID = Guid.NewGuid().ToString();
            //    total.TaxAfterDiscountAmount = 47.80m;
            //    total.TaxAmount = 47.948721m;
            //    total.TaxBeforeDiscountAmount = 47.95m;
            //    total.TaxableAmountTotal = 181.1966m;
            //    total.VolumePoints = 23.9500m;
            //    result.Totals = total;

            //    return result;
            //}

            private CatalogItemList GetCatalogItemList()
            {
                CatalogItemList result = new CatalogItemList();
                CatalogItem_V01 item = new CatalogItem_V01();
                item.PriceValidFrom = DateTime.Now.AddYears(1);
                item.PriceValidFrom = DateTime.Now.AddYears(-1);
                item.VolumePoints = 23.9500m;
                item.UnitTaxBase = 281.1966m;
                item.EarnBase = 281.2000m;
                item.IsFreightExempt = false;
                item.VolumePointsValidTo = DateTime.Now.AddDays(2);
                item.VolumePointsValidFrom = DateTime.Now.AddDays(-2);
                item.InventoryList = new WarehouseInventoryList();
                WarehouseInventory_V01 invItem = new WarehouseInventory_V01();
                invItem.WarehouseCode = "3218";
                invItem.QuantityOnHand = 9900;
                invItem.QuantityAvailable = 9900;
                item.InventoryList.Add("3218", invItem);

                invItem = new WarehouseInventory_V01();
                invItem.WarehouseCode = "3019";
                invItem.QuantityOnHand = 13210;
                invItem.QuantityAvailable = 13210;
                item.InventoryList.Add("3019", invItem);
                item.ListPrice = 329m;
                item.Currency = "RMB";
                item.Description = "蛋白混合饮料-香草口味";
                item.SKU = "1316";
                item.TaxCategory = "VAT";
                item.ProductCategory = "RSO";
                item.StockingSKU = "1";
                item.ProductType = ServiceProvider.CatalogSvc.ProductType.Product;
                item.IsInventory = true;
                item.IsActive = true;
                item.Weight = 658;
                result.Add("1316", item);

                item = new CatalogItem_V01();
                item.PriceValidFrom = DateTime.Now.AddYears(1);
                item.PriceValidFrom = DateTime.Now.AddYears(-1);
                item.VolumePoints = 0m;
                item.UnitTaxBase = 0.8547m;
                item.EarnBase = 0.8547m;
                item.IsFreightExempt = false;
                item.VolumePointsValidTo = DateTime.Now.AddDays(2);
                item.VolumePointsValidFrom = DateTime.Now.AddDays(-2);
                item.InventoryList = new WarehouseInventoryList();
                invItem = new WarehouseInventory_V01();
                invItem.WarehouseCode = "3218";
                invItem.QuantityOnHand = 26465;
                invItem.QuantityAvailable = 26465;
                item.InventoryList.Add("3218", invItem);

                invItem = new WarehouseInventory_V01();
                invItem.WarehouseCode = "3019";
                invItem.QuantityOnHand = 36241;
                invItem.QuantityAvailable = 36241;
                item.InventoryList.Add("3019", invItem);
                item.ListPrice = 1m;
                item.Currency = "RMB";
                item.Description = "康宝莱生活方式三折页";
                item.SKU = "9143";
                item.TaxCategory = "VAT";
                item.ProductCategory = "RSO";
                item.StockingSKU = "654";
                item.ProductType = ServiceProvider.CatalogSvc.ProductType.PromoAccessory;
                item.IsInventory = true;
                item.IsActive = true;
                item.Weight = 0;
                result.Add("9143", item);

                return result;
            }

            //private MyHerbalife3.Ordering.Providers.DistributorOrderConfirmation GetEmailForOrder()
            //{
            //    MyHerbalife3.Ordering.Providers.DistributorOrderConfirmation result = new DistributorOrderConfirmation();
            //    result.DeliveryTimeEstimated = "7";
            //    result.Distributor = new Shared.LegacyProviders.Distributor();
            //    result.Distributor.Contact = new Shared.LegacyProviders.ContactInfo();
            //    result.Distributor.Contact.Email = "geewaib@herbalife.com";
            //    result.Distributor.Contact.PhoneNumber = "11111111111";
            //    result.Distributor.DistributorId = "CN175803";
            //    result.Distributor.DsExtension = "";
            //    result.Distributor.FirstName = "ZHANG LIANG";
            //    result.Distributor.LastName = "";
            //    result.Distributor.Locale = "zh-CN";
            //    result.Distributor.MiddleName = "";
            //    result.GrandTotal = 354m;
            //    result.HFFMessage = "N";
            //    result.Items = new MyHerbalife3.Ordering.Providers.OrderItemEmail[2];
            //    result.Items[0] = new MyHerbalife3.Ordering.Providers.OrderItemEmail();
            //    result.Items[0].DistributorCost = 329m;
            //    result.Items[0].EarnBase = 0m;
            //    result.Items[0].Flavor = "香草口味 550克";
            //    result.Items[0].IsLinkedSKU = "False";
            //    result.Items[0].ItemDescription = "蛋白混合饮料 - 香草口味 550克";
            //    result.Items[0].LineTotal = 329m;
            //    result.Items[0].PriceWithCharges = 376.803422m;
            //    result.Items[0].Quantity = 1;
            //    result.Items[0].SKU = "1316";
            //    result.Items[0].SkuId = "1316";
            //    result.Items[0].UnitPrice = 329m;
            //    result.Items[0].VolumePoints = 23.95m;

            //    result.Items[1] = new MyHerbalife3.Ordering.Providers.OrderItemEmail();
            //    result.Items[1].DistributorCost = 1m;
            //    result.Items[1].EarnBase = 0m;
            //    result.Items[1].Flavor = "康宝莱生活方式三折页";
            //    result.Items[1].IsLinkedSKU = "False";
            //    result.Items[1].ItemDescription = "康宝莱生活方式三折页";
            //    result.Items[1].LineTotal = 1m;
            //    result.Items[1].PriceWithCharges = 1.145299m;
            //    result.Items[1].Quantity = 1;
            //    result.Items[1].SKU = "9143";
            //    result.Items[1].SkuId = "9143";
            //    result.Items[1].UnitPrice = 1m;
            //    result.Items[1].VolumePoints = 0m;

            //    result.Language = "zh";
            //    result.Locale = "CN";
            //    result.OrderId = "";
            //    result.OrderMonth = "2015 11";
            //    result.OrderMonthVolume = "0.00";
            //    result.OrderSubmittedDate = DateTime.Now;
            //    result.Payments = new MyHerbalife3.Shared.LegacyProviders.PaymentInfo[1];
            //    result.Payments[0] = new MyHerbalife3.Shared.LegacyProviders.PaymentInfo();
            //    result.Payments[0].Amount = 354m;
            //    result.Payments[0].CardNumber = "yXXXXXXXXXXXXXXXXXXXAw==";
            //    result.Payments[0].CardType = "";
            //    result.Payments[0].ExpirationDate = "01/0001";
            //    result.Payments[0].Installments = "";
            //    result.Payments[0].PaymentCode = "";
            //    result.Payments[0].TransactionType = "QP";

            //    result.PickupLocation = "";
            //    result.RemainingValue = "";
            //    result.Shipment = new Shared.LegacyProviders.Shipment();
            //    result.Shipment.FirstName = "Boon";
            //    result.Shipment.LastName = "";
            //    result.Shipment.MiddleName = "";
            //    result.Shipment.PickupName = "Boon";
            //    result.Shipment.ShippingDate = DateTime.Now;
            //    result.ShippingMethod = "联邦-成都配送中心";
            //    result.ShippingAddress = new Shared.LegacyProviders.CAddress();
            //    result.ShippingAddress.City = "昆明市";
            //    result.ShippingAddress.Country = "CN";
            //    result.ShippingAddress.CountyDistrict = "市辖区";
            //    result.ShippingAddress.Line1 = "Jalan jalan";
            //    result.ShippingAddress.Line2 = "276720";
            //    result.ShippingAddress.Line3 = "11579709";
            //    result.ShippingAddress.Line4 = "DL09";
            //    result.ShippingAddress.State = "云南";
            //    result.ShippingAddress.Zip = "111111";
            //    result.ShippingHandling = 25m;
            //    result.SubTotal = 354m;
            //    result.Tax = 47.948721m;
            //    result.TotalDiscountRetail = 330m;
            //    result.TotalProductRetail = 329m;
            //    result.TotalRetail = 330m;
            //    result.TotalVolumePoints = 23.9500m;
            //    result.paymentOption = "1";
            //    result.pickupTime = "工作日、双休日与假日均可送货";

            //    return result;
            //}

            //[TestMethod]
            //public void NewCardButInvalidCardNumber_Error()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_InvalidCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = RequestMobilePinForPurchaseResponseString_InvalidCard();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            //    Assert.IsTrue(!result && target.LastErrorMessage == "BIN.无效卡号");
            //}

            //[TestMethod]
            //public void NewCard_Success()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_NewCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = RequestMobilePinForPurchaseResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            //    Assert.IsTrue(result && target.LastErrorMessage == "");
            //}

            //[TestMethod]
            //public void NewCardWithFailedWCFResponse_False()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_NewCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = RequestMobilePinForPurchaseResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber, HL.Common.DataContract.Interfaces.ServiceResponseStatusType.Failure);

            //    var result = target.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            //    Assert.IsTrue(!result);
            //}

            //[TestMethod]
            //public void BindedCard_Success()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_BindedCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = RequestMobilePinForPurchaseResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            //    Assert.IsTrue(result && target.LastErrorMessage == "");
            //}

            //[TestMethod]
            //public void BindedCardWithFailedWCFResponse_False()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_BindedCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = RequestMobilePinForPurchaseResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber, HL.Common.DataContract.Interfaces.ServiceResponseStatusType.Failure);

            //    var result = target.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            //    Assert.IsTrue(!result);
            //}
        }

        [TestClass]
        public class SubmitMethod
        {
            [TestInitialize]
            public void ChinaCulture()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");

                HttpContext.Current = FakeHttpContext();
            }

            public static HttpContext FakeHttpContext()
            {
                var httpRequest = new HttpRequest("", "http://tempuri.org", "");
                var stringWriter = new StringWriter();
                var httpResponse = new HttpResponse(stringWriter);
                var httpContext = new HttpContext(httpRequest, httpResponse);

                var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                        new HttpStaticObjectsCollection(), 10, true,
                                                        HttpCookieMode.AutoDetect,
                                                        SessionStateMode.InProc, false);

                httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                            BindingFlags.NonPublic | BindingFlags.Instance,
                                            null, CallingConventions.Standard,
                                            new[] { typeof(HttpSessionStateContainer) },
                                            null)
                                    .Invoke(new object[] { sessionContainer });

                httpContext.User = new GenericPrincipal(new GenericIdentity("CN175803"), new string[0]);

                return httpContext;
            }

            //public CN_99BillQuickPayProvider GetTarget(string distributor, string locale, string responseMessage, Order_V01 order, MyHLShoppingCart cartInfo, string orderNumber, HL.Common.DataContract.Interfaces.ServiceResponseStatusType responseStatus = HL.Common.DataContract.Interfaces.ServiceResponseStatusType.Success)
            //{
            //    string phoneNumber = "15901822570";
            //    var chinaOrderProxy = Substitute.For<IChinaInterface>();

            //    chinaOrderProxy.GetCnpPaymentServiceDetail(Arg.Any<HL.Order.ValueObjects.China.OnlineOrder.GetCnpPaymentServiceRequest_V02>())
            //        .Returns(new HL.Order.ValueObjects.China.OnlineOrder.GetCnpPaymentServiceResponse_V01()
            //        {
            //            Status = responseStatus,
            //            Response = responseMessage,
            //        });

            //    var orderProviderLoader = Substitute.For<IOrderProviderLoader>();
            //    orderProviderLoader.InsertPaymentGatewayRecord(orderNumber, order.DistributorID, "CN_99BillPaymentGateway", Arg.Any<string>(), locale).Returns(123123);
            //    orderProviderLoader.UpdatePaymentGatewayRecord(orderNumber, Arg.Any<string>(), Arg.Any<PaymentGatewayLogEntryType>(), Arg.Any<PaymentGatewayRecordStatusType>());

            //    var shoppingCartProviderLoader = Substitute.For<IShoppingCartProviderLoader>();
            //    shoppingCartProviderLoader.GetShoppingCart(distributor, locale).Returns(GetShoppingCart());

            //    var distributorOrderingProfileProviderLoader = Substitute.For<IDistributorOrderingProfileProviderLoader>();
            //    distributorOrderingProfileProviderLoader.GetPhoneNumberForCN(distributor).Returns(phoneNumber);
            //    distributorOrderingProfileProviderLoader.GetTinList(distributor, true).Returns(GetTinList());

            //    var paymentInfoProviderLoader = Substitute.For<IPaymentInfoProviderLoader>();
            //    paymentInfoProviderLoader.SavePaymentInfo(distributor, locale, Arg.Any<PaymentInformation>()).Returns(1);


            //    var membershipLoader = Substitute.For<IMembershipLoader>();
            //    membershipLoader.GetUser().Returns(GetDistributorProfile());

            //    HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] = order.Payments.First();

            //    var target = new CN_99BillQuickPayProvider(chinaOrderProxy, paymentInfoProviderLoader, distributorOrderingProfileProviderLoader, null, null, orderProviderLoader, shoppingCartProviderLoader, membershipLoader);

            //    return target;
            //}

            private string SubmitResponseString_InvalidMobilePin()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage><version>1.0</version><TxnMsgContent><txnType>PUR</txnType><interactiveStatus>TR2</interactiveStatus><amount>339</amount><merchantId>104110045112012</merchantId><terminalId>00002012</terminalId><entryTime>20151113170036</entryTime><externalRefNumber>10008878</externalRefNumber><customerId>CN175803</customerId><transTime>20151113170037</transTime><responseCode>T6</responseCode><responseTextMessage>验证码不匹配</responseTextMessage></TxnMsgContent></MasMessage>";
            }

            private string SubmitResponseString_Success()
            {
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage><version>1.0</version><TxnMsgContent><txnType>PUR</txnType><interactiveStatus>TR2</interactiveStatus><amount>339</amount><merchantId>104110045112012</merchantId><terminalId>00002012</terminalId><entryTime>20151116014028</entryTime><externalRefNumber>10008906</externalRefNumber><customerId>CN175803</customerId><transTime>20151116174030</transTime><refNumber>000012651715</refNumber><responseCode>00</responseCode><responseTextMessage>交易成功</responseTextMessage><cardOrg>CU</cardOrg><issuer>中国银行</issuer><storableCardNo>4380884390</storableCardNo><authorizationCode>833359</authorizationCode></TxnMsgContent></MasMessage>";
            }

            //private MembershipUser<DistributorProfileModel> GetDistributorProfile()
            //{
            //    MembershipUser<DistributorProfileModel> result = new MembershipUser<DistributorProfileModel>();
            //    DistributorProfileModel profileModel = new DistributorProfileModel();
            //    profileModel.FirstName = "张梁";
            //    result.Value = profileModel;

            //    return result;
            //}

            private List<TaxIdentification> GetTinList()
            {
                List<TaxIdentification> result = new List<TaxIdentification>();
                TaxIdentification item = new TaxIdentification();
                item.ID = "CNID";
                item.IDType = new TaxIdentificationType() { Key = "CNID"};
                item.IDType.Key = "330602198112090011";
                result.Add(item);

                return result;
            }

            private MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 GetOrder_NewCard()
            {
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 result = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01();

                result.CountryOfProcessing = "CN";
                result.Handling = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HandlingInfo_V01() { ShippingInstructions = "工作日、双休日与假日均可送货" };
                result.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                result.OrderItems = new ServiceProvider.OrderSvc.OrderItems();
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "蛋白混合饮料-香草口味550克";
                itm.RetailPrice = 329.0m;
                itm.VolumePoint = 23.9500m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "1316";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "康宝莱生活方式三折页";
                itm.RetailPrice = 1m;
                itm.VolumePoint = 0m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "9143";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                result.OrderMonth = "1511";

                result.Payments = new ServiceProvider.OrderSvc.PaymentCollection();
                var payment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01();
                var card = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.QuickPayPayment();
                card.CardHolderId = "330602198112090011";
                card.CardHolderType = "0";
                card.MobilePhoneNumber = "15901822570";
                card.AccountNumber = "7PfIVTKbvYAd7mocRHev3pjwYIX0mM0z2CIkbTgSgeA=";
                card.Expiration = new DateTime(2016, 11, 1);
                card.CVV = "+J8VQVayiy4IwiI9vkjNFw==";
                card.NameOnCard = "张梁";
                card.IssuingBankID = "BOC";
                payment.Card = card;
                payment.AuthorizationMethod = 0;
                payment.AuthorizationMerchantAccount = "BOC";
                payment.Amount = 239m;
                payment.Currency = "RMB";
                payment.TransactionType = "QP";
                result.Payments.Add(payment);

                result.PurchasingLimits = new ServiceProvider.OrderSvc.PurchasingLimits();
                result.ReceivedDate = DateTime.Now;

                var shipment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01();
                shipment.Recipient = "Boon";
                shipment.Phone = "11111111111";
                shipment.ShippingMethodID = "27";
                shipment.WarehouseCode = "3019";
                shipment.FreightVariant = "EXP";
                shipment.Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01();
                shipment.Address.Line1 = "Jalan jalan";
                shipment.Address.Line2 = "276720";
                shipment.Address.Line3 = "11579709";
                shipment.Address.Line4 = "DL09";
                shipment.Address.City = "昆明市";
                shipment.Address.CountyDistrict = "市辖区";
                shipment.Address.StateProvinceTerritory = "云南";
                shipment.Address.Country = "CN";
                shipment.Address.PostalCode = "111111";
                shipment.DeliveryNickName = "Boon10";
                result.Shipment = shipment;

                result.DistributorID = "CN175803";

                return result;
            }

            private MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 GetOrder_BindedCard()
            {
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01 result = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order_V01();

                result.CountryOfProcessing = "CN";
                result.Handling = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HandlingInfo_V01() { ShippingInstructions = "工作日、双休日与假日均可送货" };
                result.OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO;
                result.OrderItems = new ServiceProvider.OrderSvc.OrderItems();
                MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "蛋白混合饮料-香草口味550克";
                itm.RetailPrice = 329.0m;
                itm.VolumePoint = 23.9500m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "1316";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                itm = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem();
                itm.Description = "康宝莱生活方式三折页";
                itm.RetailPrice = 1m;
                itm.VolumePoint = 0m;
                itm.ReferenceID = Guid.NewGuid();
                itm.SKU = "9143";
                itm.Quantity = 1;
                result.OrderItems.Add(itm);

                result.OrderMonth = "1511";

                result.Payments = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection();
                var payment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01();
                var card = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.QuickPayPayment();
                card.CardHolderId = "330602198112090011";
                card.CardHolderType = "0";
                card.MobilePhoneNumber = "15901822570";
                card.AccountNumber = "yX+yCQuJE5wAXqupHxzBAw==";
                card.IssuingBankID = "BOC";
                card.IsDebitCard = true;
                payment.Card = card;
                payment.AuthorizationMethod = 0;
                payment.AuthorizationMerchantAccount = "BOC";
                payment.Amount = 239m;
                payment.Currency = "RMB";
                payment.TransactionType = "QP";
                result.Payments.Add(payment);

                result.PurchasingLimits = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PurchasingLimits();
                result.ReceivedDate = DateTime.Now;

                var shipment = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01();
                shipment.Recipient = "Boon";
                shipment.Phone = "11111111111";
                shipment.ShippingMethodID = "27";
                shipment.WarehouseCode = "3019";
                shipment.FreightVariant = "EXP";
                shipment.Address = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01();
                shipment.Address.Line1 = "Jalan jalan";
                shipment.Address.Line2 = "276720";
                shipment.Address.Line3 = "11579709";
                shipment.Address.Line4 = "DL09";
                shipment.Address.City = "昆明市";
                shipment.Address.CountyDistrict = "市辖区";
                shipment.Address.StateProvinceTerritory = "云南";
                shipment.Address.Country = "CN";
                shipment.Address.PostalCode = "111111";
                shipment.DeliveryNickName = "Boon10";
                result.Shipment = shipment;

                result.DistributorID = "CN175803";

                return result;
            }

            //private MyHLShoppingCart GetShoppingCart()
            //{
            //    MyHLShoppingCart result = new MyHLShoppingCart();
            //    result.CartItems = new HL.Catalog.ValueObjects.ShoppingCartItemList();
            //    HL.Catalog.ValueObjects.ShoppingCartItem_V01 cartItem = new HL.Catalog.ValueObjects.ShoppingCartItem_V01();
            //    cartItem.Quantity = 1;
            //    cartItem.SKU = "1316";
            //    cartItem.Updated = DateTime.Now;
            //    result.CartItems.Add(cartItem);

            //    cartItem = new HL.Catalog.ValueObjects.ShoppingCartItem_V01();
            //    cartItem.Quantity = 1;
            //    cartItem.SKU = "9143";
            //    cartItem.Updated = DateTime.Now;
            //    result.CartItems.Add(cartItem);

            //    result.CartName = "";
            //    result.CountryCode = "CN";

            //    result.DeliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo();
            //    result.DeliveryInfo.Address = new HL.Shipping.ValueObjects.ShippingAddress_V01();
            //    result.DeliveryInfo.Address.Address = new HL.Common.ValueObjects.Address_V01();
            //    result.DeliveryInfo.Address.Address.City = "昆明市";
            //    result.DeliveryInfo.Address.Address.Country = "CN";
            //    result.DeliveryInfo.Address.Address.CountyDistrict = "市辖区";
            //    result.DeliveryInfo.Address.Address.Line1 = "Jalan jalan";
            //    result.DeliveryInfo.Address.Address.Line2 = "276720";
            //    result.DeliveryInfo.Address.Address.Line3 = "11579709";
            //    result.DeliveryInfo.Address.Address.Line4 = "DL09";
            //    result.DeliveryInfo.Address.Address.PostalCode = "111111";
            //    result.DeliveryInfo.Address.Address.StateProvinceTerritory = "云南";
            //    result.DeliveryInfo.Address.Alias = "Boon10";
            //    result.DeliveryInfo.Address.AltAreaCode = "";
            //    result.DeliveryInfo.Address.AltPhone = "15901822570";
            //    result.DeliveryInfo.Address.AreaCode = "";
            //    result.DeliveryInfo.Address.Attention = new HL.Common.ValueObjects.Name_V01();
            //    result.DeliveryInfo.Address.Created = DateTime.Now;
            //    result.DeliveryInfo.Address.DisplayName = "Boon10";
            //    result.DeliveryInfo.Address.ID = 11579709;
            //    result.DeliveryInfo.Address.IsPrimary = true;
            //    result.DeliveryInfo.Option = new HL.Common.ValueObjects.DeliveryOptionType();
            //    result.DeliveryInfo.Address.Phone = "11111111111";
            //    result.DeliveryInfo.Address.Recipient = "Boon";
            //    result.DeliveryInfo.AddressType = "EXP";
            //    result.DeliveryInfo.DeliveryNickName = "Boon10";
            //    result.DeliveryInfo.Description = "";
            //    result.DeliveryInfo.FreightCode = "27";
            //    result.DeliveryInfo.Instruction = "工作日、双休日与假日均可送货";
            //    result.DeliveryInfo.WarehouseCode = "3019";
            //    result.DeliveryInfo.Option = HL.Common.ValueObjects.DeliveryOptionType.Shipping;
            //    result.DeliveryOption = HL.Common.ValueObjects.DeliveryOptionType.Shipping;
            //    result.DistributorID = "CN175803";
            //    result.EmailAddress = "geewaib@herbalife.com";
            //    result.EmailValues = new HLShoppingCartEmailValues();
            //    result.EmailValues.CurrentMonthVolume = "0";
            //    result.EmailValues.DistributorSubTotal = 330m;
            //    result.EmailValues.DistributorSubTotalFormatted = "¥330.00";
            //    result.FirstOrderPromotionTypes = new List<HL.Order.ValueObjects.China.PromotionType>();
            //    result.FreightCode = "27";
            //    result.LastUpdated = DateTime.Now;
            //    result.LastUpdatedUtc = DateTime.UtcNow;
            //    result.Locale = "zh-CN";
            //    result.OrderCategory = HL.Common.ValueObjects.OrderCategoryType.RSO;
            //    result.OrderMonth = 201511;
            //    result.OrderSubType = "";
            //    result.PassDSFraudValidation = true;
            //    result.SMSNotification = "11111111111";
            //    result.SelectedDSSubType = "";
            //    result.ShippingAddressID = 11579709;
            //    result.ShoppingCartID = 12839027;
            //    result.ShoppingCartItems = new List<Shared.Providers.DistributorShoppingCartItem>();

            //    var item = new MyHerbalife3.Shared.Providers.DistributorShoppingCartItem();
            //    item.CatalogItem = new HL.Catalog.ValueObjects.CatalogItem_V01();
            //    item.CatalogItem.Description = "蛋白混合饮料-香草口味 550克";
            //    item.DiscountPrice = 329.0m;
            //    item.EarnBase = 0m;
            //    item.ErrorMessage = "";
            //    item.Flavor = "香草口味 550克";
            //    item.ParentCat = new HL.Catalog.ValueObjects.Category_V02();
            //    item.ProdInfo = new HL.Catalog.ValueObjects.ProductInfo_V02();
            //    item.Quantity = 1;
            //    item.RetailPrice = 329.0m;
            //    item.SKU = "1316";
            //    item.Updated = DateTime.Now;
            //    item.VolumePoints = 23.95m;
            //    result.ShoppingCartItems.Add(item);

            //    item = new MyHerbalife3.Shared.Providers.DistributorShoppingCartItem();
            //    item.CatalogItem = new HL.Catalog.ValueObjects.CatalogItem_V01();
            //    item.CatalogItem.Description = "康宝莱生活方式三折页";
            //    item.DiscountPrice = 1m;
            //    item.EarnBase = 0m;
            //    item.ErrorMessage = "";
            //    item.Flavor = "康宝莱生活方式三折页";
            //    item.ParentCat = new HL.Catalog.ValueObjects.Category_V02();
            //    item.ProdInfo = new HL.Catalog.ValueObjects.ProductInfo_V02();
            //    item.Quantity = 1;
            //    item.RetailPrice = 1.0m;
            //    item.SKU = "9143";
            //    item.Updated = DateTime.Now;
            //    item.VolumePoints = 0m;
            //    result.ShoppingCartItems.Add(item);

            //    var total = new HL.Order.ValueObjects.China.OrderTotals_V02();
            //    total.AmountDue = 239.0m;

            //    total.ChargeList = new ChargeList();

            //    var chargeList = new HL.Order.ValueObjects.Charge_V01();
            //    chargeList.Amount = 10m;
            //    chargeList.AmountForFairMarketValue = FairMarketValue.UseStandard;
            //    chargeList.ChargeType = ChargeTypes.FREIGHT;
            //    chargeList.DiscountedAmount = 101m;
            //    chargeList.ItemDiscountedAmount = 330m;
            //    total.ChargeList.Add(chargeList);

            //    total.ItemTotalsList = new HL.Order.ValueObjects.ItemTotalsList();
            //    var totalItem = new HL.Order.ValueObjects.ItemTotal_V01();
            //    totalItem.AfterDiscountTax = 47.80m;
            //    totalItem.DiscountedPrice = 329.0m;
            //    totalItem.LinePrice = 329m;
            //    totalItem.LineTax = 47.803422m;
            //    totalItem.ProductType = HL.Common.ValueObjects.ProductType.None;
            //    totalItem.Quantity = 1;
            //    totalItem.RetailPrice = 329.0m;
            //    totalItem.SKU = "1316";
            //    totalItem.TaxableAmount = 281.1966m;
            //    totalItem.VolumePoints = 23.9500m;
            //    total.ItemTotalsList.Add(totalItem);

            //    totalItem = new HL.Order.ValueObjects.ItemTotal_V01();
            //    totalItem.AfterDiscountTax = 47.80m;
            //    totalItem.Discount = 0.8547m;
            //    totalItem.DiscountedPrice = 1m;
            //    totalItem.LinePrice = 1m;
            //    totalItem.LineTax = 0.145299m;
            //    totalItem.ProductType = HL.Common.ValueObjects.ProductType.None;
            //    totalItem.Quantity = 1;
            //    totalItem.RetailPrice = 329.0m;
            //    totalItem.SKU = "9143";
            //    totalItem.TaxableAmount = 0.8547m;
            //    totalItem.VolumePoints = 0m;
            //    total.ItemTotalsList.Add(totalItem);
            //    total.ItemsTotal = 330m;
            //    total.MiscAmount = 100.8547m;
            //    total.OrderFreight = new HL.Order.ValueObjects.China.OrderFreight();
            //    total.OrderFreight.ActualFreight = 9m;
            //    total.OrderFreight.BeforeDiscountFreight = 9m;
            //    total.OrderFreight.BeforeWeight = 2;
            //    total.OrderFreight.CaseRate = 1.13636m;
            //    total.OrderFreight.FreightCharge = 10m;
            //    total.OrderFreight.MaterialFee = 1m;
            //    total.OrderFreight.PackageWeight = 0.19m;
            //    total.OrderFreight.Packages = new List<HL.Order.ValueObjects.China.Package>();

            //    var package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "A";
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "B";
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "C";
            //    package.Unit = 1;
            //    package.Volume = 0.0078045m;
            //    total.OrderFreight.Packages.Add(package);

            //    package = new HL.Order.ValueObjects.China.Package();
            //    package.Packagetype = "X";
            //    total.OrderFreight.Packages.Add(package);

            //    total.OrderFreight.PhysicalWeight = 0.84400m;
            //    total.OrderFreight.VolumeWeight = 1.30075m;
            //    total.OrderFreight.Weight = 0.65800m;
            //    total.PricingServerName = "MYKL-DTS-11";
            //    total.ProductRetailAmount = 229.0m;
            //    total.ProductTaxTotal = 182.0513m;
            //    total.PromotionRetailAmount = 0.8547m;
            //    total.QuoteID = Guid.NewGuid().ToString();
            //    total.TaxAfterDiscountAmount = 47.80m;
            //    total.TaxAmount = 47.948721m;
            //    total.TaxBeforeDiscountAmount = 47.95m;
            //    total.TaxableAmountTotal = 181.1966m;
            //    total.VolumePoints = 23.9500m;
            //    result.Totals = total;

            //    return result;
            //}

            //[TestMethod]
            //public void NewCardInvalidMobilePin_False()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_NewCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = SubmitResponseString_InvalidMobilePin();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.Submit(orderNumber, (shoppingCart.Totals as HL.Order.ValueObjects.China.OrderTotals_V02).AmountDue);

            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        var resultList = result.Split(',').ToList();
            //        Assert.IsTrue(resultList[1] == "0");
            //    }
            //    else
            //    {
            //        Assert.Fail();
            //    }
            //}

            //[TestMethod]
            //public void NewCard_Success()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_NewCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = SubmitResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.Submit(orderNumber, (shoppingCart.Totals as HL.Order.ValueObjects.China.OrderTotals_V02).AmountDue);

            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        var resultList = result.Split(',').ToList();
            //        Assert.IsTrue(resultList[1] == "1");
            //    }
            //    else
            //    {
            //        Assert.Fail();
            //    }
            //}

            //[TestMethod]
            //public void BindedCard_Success()
            //{
            //    string distributorId = "CN175803";
            //    string orderNumber = "10008783";
            //    Order_V01 order = GetOrder_BindedCard();
            //    MyHLShoppingCart shoppingCart = GetShoppingCart();
            //    var response = SubmitResponseString_Success();
            //    var target = GetTarget(distributorId, HLConfigManager.Configurations.Locale, response, order, shoppingCart, orderNumber);

            //    var result = target.Submit(orderNumber, (shoppingCart.Totals as HL.Order.ValueObjects.China.OrderTotals_V02).AmountDue);

            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        var resultList = result.Split(',').ToList();
            //        Assert.IsTrue(resultList[1] == "1");
            //    }
            //    else
            //    {
            //        Assert.Fail();
            //    }
            //}
        }
    }
}
