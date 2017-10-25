using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using SurveyQuestions = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SurveyQuestions;


namespace MyHerbalife3.Ordering.Providers
{
    public class OrderChinaProviderLoader : IOrderChinaProviderLoader
    {
        public void AddFreeGift(string freeSKU, int skuQuantity, SKU_V01ItemCollection AllSkus, string WareHouseCode, MyHLShoppingCart cart)
        {
            China.OrderProvider.AddFreeGift(freeSKU, skuQuantity, AllSkus, WareHouseCode, cart);
        }

        public bool AnalyzePaymentGatewayOrder(string orderNumber, out string strResponse)
        {
            return China.OrderProvider.AnalyzePaymentGatewayOrder(orderNumber, out strResponse);
        }

        public OnlineOrder CreateOrder(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments)
        {
            return China.OrderProvider.CreateOrder(orderNumber, shoppingCart, payments);
        }

        public OnlineOrderItem CreateOrderItem(DistributorShoppingCartItem item)
        {
            return China.OrderProvider.CreateOrderItem(item);
        }

        public OnlineOrder CreateOrderObject(List<DistributorShoppingCartItem> cartItems)
        {
            return China.OrderProvider.CreateOrderObject(cartItems);
        }

        public OnlineOrder CreateOrderObject(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments)
        {
            return China.OrderProvider.CreateOrderObject(orderNumber, shoppingCart, payments);
        }

        public OnlineOrder FillOrderInfo(OnlineOrder order, MyHLShoppingCart shoppingCart)
        {
            return China.OrderProvider.FillOrderInfo(order, shoppingCart);
        }

        public MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Get99BillOrderStatusServiceResponse_V01 Get99BillOrderStatus(string orderNumber)
        {
            return China.OrderProvider.Get99BillOrderStatus(orderNumber);
        }

        public List<SurveyQuestions> GetCustomerSurvey(string distributorId)
        {
            return China.OrderProvider.GetCustomerSurvey(distributorId);
        }

        //public PromotionResponse_V01 GetEffectivePromotionList(string locale, DateTime? dateTime = default(DateTime?))
        //{
        //    return ChinaPromotionProvider.GetEffectivePromotionList(locale, dateTime);
        //}

        public MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetExpressTrackResponse_V01 GetExpressTrackInfo(string orderId)
        {
            return China.OrderProvider.GetExpressTrackInfo(orderId);
        }

        public MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.GetFeedbackResponse_V01 GetFeedBack(string distributorId)
        {
            return China.OrderProvider.GetFeedBack(distributorId);
        }

        public List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderStatus> GetListOfOrderStatus()
        {
            return China.OrderProvider.GetListOfOrderStatus();
        }

        public ServiceProvider.OrderChinaSvc.OnlineOrder GetOrderDetail(string distributorId, int orderHeaderId)
        {
            return China.OrderProvider.GetOrderDetail(distributorId, orderHeaderId);
        }

        public List<ServiceProvider.OrderChinaSvc.OnlineOrder> GetOrders(string distributorId, int profileId, string distributorType, DateTime? startDate, DateTime? endDate, string sortOrder)
        {
            return China.OrderProvider.GetOrders(distributorId, profileId, distributorType, startDate, endDate, sortOrder);
        }

        public List<PendingOrder> GetPayByPhonePendingOrders(string distributorId, string locale)
        {
            return China.OrderProvider.GetPayByPhonePendingOrders(distributorId, locale);
        }

        public ServiceProvider.OrderChinaSvc.GetPaymentDetailsResponse_V01 GetPaymentDetails(List<int> orderHeaderList)
        {
            return China.OrderProvider.GetPaymentDetails(orderHeaderList);
        }

        public ServiceProvider.OrderChinaSvc.GetPBPPaymentServiceResponse_V01 GetPBPPaymentServiceDetail(string distributorId, string orderNumber)
        {
            return China.OrderProvider.GetPBPPaymentServiceDetail(distributorId, orderNumber);
        }

        public List<ServiceProvider.OrderChinaSvc.PCDistributorInfo> GetPCCustomerIdByReferralId(string distributorId)
        {
            return China.OrderProvider.GetPCCustomerIdByReferralId(distributorId);
        }

        public List<ServiceProvider.OrderChinaSvc.PCDistributorInfo> GetPreferredCustomers(string distributorId, DateTime? from, DateTime? to)
        {
            return China.OrderProvider.GetPreferredCustomers(distributorId, from, to);
        }

        public ServiceProvider.OrderChinaSvc.OnlineOrder GetPreOrderDetail(string distributorId, int orderHeaderId)
        {
            return China.OrderProvider.GetPreOrderDetail(distributorId, orderHeaderId);
        }

        public List<ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> GetSkuOrderedAndPurchased(string country, string distributorId, DateTime? eventStartDate, DateTime? eventEndDate, List<ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> skuList)
        {
            return China.OrderProvider.GetSkuOrderedAndPurchased(country, distributorId, eventStartDate, eventEndDate, skuList);
        }

        public int InsertOrder(ServiceProvider.OrderChinaSvc.OnlineOrder order)
        {
            return China.OrderProvider.InsertOrder(order);
        }

        public bool IsEligibleForEvents(string distributorId)
        {
            return China.OrderProvider.IsEligibleForEvents(distributorId);
        }

        public void ProcessPBPPaymentService(string distributorId, string orderNumber)
        {
            China.OrderProvider.ProcessPBPPaymentService(distributorId, orderNumber);
        }

        public ServiceProvider.OrderChinaSvc.SurveyCustomerSelectionsDetailResponse_V01 SubmitCustomerSurvey(string distributorId, int surveyID, List<ServiceProvider.OrderChinaSvc.SelectionDetail> selectiondetails)
        {
            return China.OrderProvider.SubmitCustomerSurvey(distributorId, surveyID, selectiondetails);
        }

        public ServiceProvider.OrderChinaSvc.UpdateFeedbackResponse_V01 UpdateFeedBack(string distributorId, string orderId, ServiceProvider.OrderChinaSvc.GetFeedbackResult feedbackResult)
        {
            return China.OrderProvider.UpdateFeedBack(distributorId, orderId, feedbackResult);
        }

        public int UpdateOrder(string orderNumber, int shoppingCartID, OnlineOrder order)
        {
            return China.OrderProvider.UpdateOrder(orderNumber, shoppingCartID, order);
        }
    }
}
