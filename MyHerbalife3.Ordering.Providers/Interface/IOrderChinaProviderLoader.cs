using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using MyHerbalife3.Shared.ViewModel.Models;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using OnlineOrder = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrder;
using OnlineOrderItem = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem;
using PaymentCollection = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IOrderChinaProviderLoader
    {
        OnlineOrder CreateOrderObject(List<DistributorShoppingCartItem> cartItems);

        OnlineOrder CreateOrderObject(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments);

        OnlineOrder CreateOrder(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments);

        OnlineOrderItem CreateOrderItem(DistributorShoppingCartItem item);

        int UpdateOrder(string orderNumber, int shoppingCartID, OnlineOrder order);

        int InsertOrder(ServiceProvider.OrderChinaSvc.OnlineOrder order);

        OnlineOrder FillOrderInfo(OnlineOrder order, MyHLShoppingCart shoppingCart);

        List<ServiceProvider.OrderChinaSvc.OnlineOrder> GetOrders(string distributorId, int profileId, string distributorType, DateTime? startDate, DateTime? endDate, string sortOrder);

        ServiceProvider.OrderChinaSvc.OnlineOrder GetOrderDetail(string distributorId, int orderHeaderId);

        ServiceProvider.OrderChinaSvc.OnlineOrder GetPreOrderDetail(string distributorId, int orderHeaderId);

        GetPBPPaymentServiceResponse_V01 GetPBPPaymentServiceDetail(string distributorId, string orderNumber);

        void ProcessPBPPaymentService(string distributorId, string orderNumber);

        List<PendingOrder> GetPayByPhonePendingOrders(string distributorId, string locale);

        GetFeedbackResponse_V01 GetFeedBack(string distributorId);

        UpdateFeedbackResponse_V01 UpdateFeedBack(string distributorId, string orderId, GetFeedbackResult feedbackResult);

        List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderStatus> GetListOfOrderStatus();

        GetExpressTrackResponse_V01 GetExpressTrackInfo(string orderId);

        List<PCDistributorInfo> GetPCCustomerIdByReferralId(string distributorId);

        List<PCDistributorInfo> GetPreferredCustomers(string distributorId, DateTime? from, DateTime? to);

        List<SurveyQuestions> GetCustomerSurvey(string distributorId);

        SurveyCustomerSelectionsDetailResponse_V01 SubmitCustomerSurvey(string distributorId, int surveyID, List<SelectionDetail> selectiondetails);

        void AddFreeGift(string freeSKU, int skuQuantity, ServiceProvider.CatalogSvc.SKU_V01ItemCollection AllSkus, string WareHouseCode, MyHLShoppingCart cart);

        bool AnalyzePaymentGatewayOrder(string orderNumber, out string strResponse);

        Get99BillOrderStatusServiceResponse_V01 Get99BillOrderStatus(string orderNumber);

        bool IsEligibleForEvents(string distributorId);

        //HL.Order.ValueObjects.China.Promotions.PromotionResponse_V01 GetEffectivePromotionList(string locale, DateTime? dateTime = null);

        List<SkuOrderedAndPurchased> GetSkuOrderedAndPurchased(string country, string distributorId, DateTime? eventStartDate, DateTime? eventEndDate, List<SkuOrderedAndPurchased> skuList);

        GetPaymentDetailsResponse_V01 GetPaymentDetails(List<int> orderHeaderList);
    }
}
