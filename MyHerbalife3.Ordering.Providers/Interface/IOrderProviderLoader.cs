using MyHerbalife3.Ordering.Providers.Payments;
using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IOrderProviderLoader
    {
        OnlineOrder CreateOrder(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments);

        void SetAPFDeliveryOption(MyHLShoppingCart cart);

        OrderItem CreateOrderItem(DistributorShoppingCartItem item);

        Order PopulateLineItems(string countryCode, Order _order, MyHLShoppingCart shoppingCart);

        ShippingInfo_V01 CreateShippingInfo(string CountryCode, MyHLShoppingCart shoppingCart);

        HandlingInfo_V01 CreateHandlingInfo(string CountryCode, string InvoiceOption, MyHLShoppingCart ShoppingCart, ShippingInfo_V01 shippingInfo);

        decimal GetConvertedAmount(decimal amountDue, string CountryCode);

        decimal GetOriginalConvertedAmount(decimal amountDue, string CountryCode);

        DupeOrderInfo CheckForRecentDupeOrder(Order_V01 order, int shoppingCartID);

        object CreateOrder(Order_V01 order, MyHLShoppingCart shoppingCart, string countryCode);

        object CreateOrder(Order_V01 order, MyHLShoppingCart shoppingCart, string countryCode, ThreeDSecuredCreditCard threeDSecuredCreditCard, string source = null);

        string SerializeOrder(object btOrderObject, Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken);

        SerializedOrderHolder GetSerializedOrderHolder(object btOrderObject, Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken);

        bool deSerializeAndSubmitOrder(PaymentGatewayResponse response, out string error, out SerializedOrderHolder holder);

        SerializedOrderHolder GetPaymentGatewayOrder(string orderNumber);

        string ImportOrder(object btOrderObject, out string error, out List<FailedCardInfo> failedCards, int cartId);

        decimal getOtherCharges(ChargeList chargeList);

        decimal getKRDistributorPrice(ItemTotal_V01 lineItem, string sku);

        decimal getBODistributorPrice(ItemTotal_V01 lineItem, string sku);

        decimal getPriceWithAllCharges(OrderTotals_V01 totals, string sku, int quantity);

        decimal getPriceWithAllCharges(OrderTotals_V01 totals);

        decimal GetDistributorSubTotal(OrderTotals_V01 totals);

        decimal GetTaxAmount(OrderTotals_V01 totals);

        bool HasLocalTax(OrderTotals_V01 totals);

        Charge_V01 GetLocalTax(OrderTotals_V01 totals);

        bool HasVATDiscount(OrderTotals_V01 totals);

        decimal CalculateTaxForInvoice(Invoice invoice);

        MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem_V01 GetInvoiceSkuDetails(string skuValue);

        string GetProductDescription(string skuValue);

        GenerateOrderNumberResponse_V01 GenerateOrderNumber(GenerateOrderNumberRequest_V01 request);

        int InsertPaymentGatewayRecord(string orderNumber, string distributorId, string gatewayName, string serializedOrder, string locale);

        PaymentGatewayRecordStatusType GetPaymentGatewayRecordStatus(string orderNumber);

        int InsertPaymentGatewayNotification(string orderNumber, string bankName);

        int GetPaymentGatewayNotification(string orderNumber);

        int UpdatePaymentGatewayNotification(string orderNumber, int orderStatus);

        string GetPaymentGatewayRecord(string orderNumber);

        List<string> GetPaymentGatewayLog(string orderNumber, PaymentGatewayLogEntryType entryType);

        void UpdatePaymentGatewayRecord(string orderNumber, string data, PaymentGatewayLogEntryType entryType, PaymentGatewayRecordStatusType status);

        string SendBPagServiceRequest(string version, string action, string merchant, string user, string password, string probeXml);

        string SendBrasPagEncryptServiceRequest(string version, string merchant, string data);

        string SendBrasPagDecryptPaymentService(string version, string merchant, string data);

        string SendBanCardServiceRequest(string data);

        string SendOcaTransactionIdServiceRequest(string data);

        string SendOcaConfirmationServiceRequest(string data);

        string SendBancaIntesaPaymentServiceRequest(string data);

        string SendTutunskaPaymentServiceRequest(string data);

        decimal GetTaxRateFromVertex(string distributorID, string locale, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 address, out string errorMessage, out string validateCity, out string validateZipCode);

        Dictionary<string, decimal> GetAllTaxRateFromVertex(string distributorId, string locale, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 address, out string errorMessage, out string validateCity, out string validateZipCode);

        string GetShipToTaxAreaId(Address address);

        void ValidateInstructions(Object btOrder, Order_V01 order, MyHLShoppingCart shoppingCart);

        void SubmitOrder(string orderId, string distributorId, string locale, MyHLShoppingCart shoppingCart, List<Payment> payments);

        bool HasDsClickedIAgreeBefore(string DistributorId);

        int InsertMLMOverrideRecordForDS(string dsid, string countrycode);

        bool HasZeroPriceEventTickets(string DistributorID, string Locale);

        void GetOrderByReferenceId(string refId, ref string orderId, ref bool isDuplicate);

        bool ValidateOrders(ServiceProvider.SubmitOrderBTSvc.Order previousOrder, ServiceProvider.SubmitOrderBTSvc.Order newOrder);

        bool SubmitPaymentGatewayOrder(string orderNumber, string gatewayResponse);

        int GetAccumulatedPCLearningPoint(string memberID, string field);

        int DeductPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform);

        bool LockPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform);

        bool RollbackPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform);

    }
}
