using MyHerbalife3.Ordering.Providers.Interface;
using System;
using System.Collections.Generic;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using Order = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Order;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Providers
{
    public class OrderProviderLoader : IOrderProviderLoader
    {
        public OrderProviderLoader()
        {
        }

        public OrderProviderLoader(IDistributorOrderingProfileProviderLoader distributorOrderingProfileProviderLoader, ICatalogProviderLoader catalogProviderLoader = null, IEmailHelperLoader emailHelperLoader = null)
        {
            OrderProvider.DistributorOrderingProfileProviderLoader = distributorOrderingProfileProviderLoader;
            OrderProvider.CatalogProviderLoader = catalogProviderLoader;
            OrderProvider.EmailHelperLoader = emailHelperLoader;
        }

        public decimal CalculateTaxForInvoice(Invoice invoice)
        {
            return OrderProvider.CalculateTaxForInvoice(invoice);
        }

        public DupeOrderInfo CheckForRecentDupeOrder(Order_V01 order, int shoppingCartID)
        {
            return OrderProvider.CheckForRecentDupeOrder(order, shoppingCartID);
        }

        public HandlingInfo_V01 CreateHandlingInfo(string CountryCode, string InvoiceOption, MyHLShoppingCart ShoppingCart, ShippingInfo_V01 shippingInfo)
        {
            return OrderProvider.CreateHandlingInfo(CountryCode, InvoiceOption, ShoppingCart, shippingInfo);
        }

        public object CreateOrder(Order_V01 order, MyHLShoppingCart shoppingCart, string countryCode)
        {
            return OrderProvider.CreateOrder(order, shoppingCart, countryCode);
        }

        public MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrder CreateOrder(string orderNumber, MyHLShoppingCart shoppingCart, PaymentCollection payments)
        {
            return OrderProvider.CreateOrder(orderNumber, shoppingCart, payments);
        }

        public object CreateOrder(Order_V01 order, MyHLShoppingCart shoppingCart, string countryCode, ThreeDSecuredCreditCard threeDSecuredCreditCard, string source = null)
        {
            return OrderProvider.CreateOrder(order, shoppingCart, countryCode, threeDSecuredCreditCard, source);
        }

        public MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItem CreateOrderItem(DistributorShoppingCartItem item)
        {
            return OrderProvider.CreateOrderItem(item);
        }

        public ShippingInfo_V01 CreateShippingInfo(string CountryCode, MyHLShoppingCart shoppingCart)
        {
            return OrderProvider.CreateShippingInfo(CountryCode, shoppingCart);
        }

        public bool deSerializeAndSubmitOrder(PaymentGatewayResponse response, out string error, out SerializedOrderHolder holder)
        {
            return OrderProvider.deSerializeAndSubmitOrder(response, out error, out holder);
        }

        public GenerateOrderNumberResponse_V01 GenerateOrderNumber(GenerateOrderNumberRequest_V01 request)
        {
            return OrderProvider.GenerateOrderNumber(request);
        }

        public Dictionary<string, decimal> GetAllTaxRateFromVertex(string distributorId, string locale, ServiceProvider.ShippingSvc.Address_V01 address, out string errorMessage, out string validateCity, out string validateZipCode)
        {
            return OrderProvider.GetAllTaxRateFromVertex(distributorId, locale, address, out errorMessage, out validateCity, out validateZipCode);
        }

        public decimal getBODistributorPrice(ItemTotal_V01 lineItem, string sku)
        {
            return OrderProvider.getBODistributorPrice(lineItem, sku);
        }

        public decimal GetConvertedAmount(decimal amountDue, string CountryCode)
        {
            return OrderProvider.GetConvertedAmount(amountDue, CountryCode);
        }

        public decimal GetDistributorSubTotal(OrderTotals_V01 totals)
        {
            return OrderProvider.GetDistributorSubTotal(totals);
        }

        public CatalogItem_V01 GetInvoiceSkuDetails(string skuValue)
        {
            return OrderProvider.GetInvoiceSkuDetails(skuValue);
        }

        public decimal getKRDistributorPrice(ItemTotal_V01 lineItem, string sku)
        {
            return OrderProvider.getKRDistributorPrice(lineItem, sku);
        }

        public Charge_V01 GetLocalTax(OrderTotals_V01 totals)
        {
            return OrderProvider.GetLocalTax(totals);
        }

        public void GetOrderByReferenceId(string refId, ref string orderId, ref bool isDuplicate)
        {
            OrderProvider.GetOrderByReferenceId(refId, ref orderId, ref isDuplicate);
        }

        public decimal GetOriginalConvertedAmount(decimal amountDue, string CountryCode)
        {
            return OrderProvider.GetOriginalConvertedAmount(amountDue, CountryCode);
        }

        public decimal getOtherCharges(ChargeList chargeList)
        {
            return OrderProvider.getOtherCharges(chargeList);
        }

        public List<string> GetPaymentGatewayLog(string orderNumber, PaymentGatewayLogEntryType entryType)
        {
            return OrderProvider.GetPaymentGatewayLog(orderNumber, entryType);
        }

        public int GetPaymentGatewayNotification(string orderNumber)
        {
            return OrderProvider.GetPaymentGatewayNotification(orderNumber);
        }

        public SerializedOrderHolder GetPaymentGatewayOrder(string orderNumber)
        {
            return OrderProvider.GetPaymentGatewayOrder(orderNumber);
        }

        public string GetPaymentGatewayRecord(string orderNumber)
        {
            return OrderProvider.GetPaymentGatewayRecord(orderNumber);
        }

        public PaymentGatewayRecordStatusType GetPaymentGatewayRecordStatus(string orderNumber)
        {
            return OrderProvider.GetPaymentGatewayRecordStatus(orderNumber);
        }

        public decimal getPriceWithAllCharges(OrderTotals_V01 totals)
        {
            return OrderProvider.getPriceWithAllCharges(totals);
        }

        public decimal getPriceWithAllCharges(OrderTotals_V01 totals, string sku, int quantity)
        {
            return OrderProvider.getPriceWithAllCharges(totals, sku, quantity);
        }

        public string GetProductDescription(string skuValue)
        {
            return OrderProvider.GetProductDescription(skuValue);
        }

        public SerializedOrderHolder GetSerializedOrderHolder(object btOrderObject, ServiceProvider.OrderSvc.Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken)
        {
            return OrderProvider.GetSerializedOrderHolder(btOrderObject, _order, shoppingCart, authenticationToken);
        }

        public string GetShipToTaxAreaId(ServiceProvider.OrderSvc.Address address)
        {
            return OrderProvider.GetShipToTaxAreaId(address);
        }

        public decimal GetTaxAmount(OrderTotals_V01 totals)
        {
            return OrderProvider.GetTaxAmount(totals);
        }

        public decimal GetTaxRateFromVertex(string distributorID, string locale, ServiceProvider.ShippingSvc.Address_V01 address, out string errorMessage, out string validateCity, out string validateZipCode)
        {
            return OrderProvider.GetTaxRateFromVertex(distributorID, locale, address, out errorMessage, out validateCity, out validateZipCode);
        }

        public bool HasDsClickedIAgreeBefore(string DistributorId)
        {
            return OrderProvider.HasDsClickedIAgreeBefore(DistributorId);
        }

        public bool HasLocalTax(OrderTotals_V01 totals)
        {
            return OrderProvider.HasLocalTax(totals);
        }

        public bool HasVATDiscount(OrderTotals_V01 totals)
        {
            return OrderProvider.HasVATDiscount(totals);
        }

        public bool HasZeroPriceEventTickets(string DistributorID, string Locale)
        {
            return OrderProvider.HasZeroPriceEventTickets(DistributorID, Locale);
        }

        public string ImportOrder(object btOrderObject, out string error, out List<FailedCardInfo> failedCards, int cartId)
        {
            return OrderProvider.ImportOrder(btOrderObject, out error, out failedCards, cartId);
        }

        public int InsertMLMOverrideRecordForDS(string dsid, string countrycode)
        {
            return OrderProvider.InsertMLMOverrideRecordForDS(dsid, countrycode);
        }

        public int InsertPaymentGatewayNotification(string orderNumber, string bankName)
        {
            return OrderProvider.InsertPaymentGatewayNotification(orderNumber, bankName);
        }

        public int InsertPaymentGatewayRecord(string orderNumber, string distributorId, string gatewayName, string serializedOrder, string locale)
        {
            return OrderProvider.InsertPaymentGatewayRecord(orderNumber, distributorId, gatewayName, serializedOrder, locale);
        }

        public ServiceProvider.OrderSvc.Order PopulateLineItems(string countryCode, ServiceProvider.OrderSvc.Order _order, MyHLShoppingCart shoppingCart)
        {
            return OrderProvider.PopulateLineItems(countryCode, _order, shoppingCart);
        }

        public string SendBancaIntesaPaymentServiceRequest(string data)
        {
            return OrderProvider.SendBancaIntesaPaymentServiceRequest(data);
        }

        public string SendBanCardServiceRequest(string data)
        {
            return OrderProvider.SendBanCardServiceRequest(data);
        }

        public string SendBPagServiceRequest(string version, string action, string merchant, string user, string password, string probeXml)
        {
            return OrderProvider.SendBPagServiceRequest(version, action, merchant, user, password, probeXml);
        }

        public string SendBrasPagDecryptPaymentService(string version, string merchant, string data)
        {
            return OrderProvider.SendBrasPagDecryptPaymentService(version, merchant, data);
        }

        public string SendBrasPagEncryptServiceRequest(string version, string merchant, string data)
        {
            return OrderProvider.SendBrasPagEncryptServiceRequest(version, merchant, data);
        }

        public string SendOcaConfirmationServiceRequest(string data)
        {
            return OrderProvider.SendOcaConfirmationServiceRequest(data);
        }

        public string SendOcaTransactionIdServiceRequest(string data)
        {
            return OrderProvider.SendOcaTransactionIdServiceRequest(data);
        }

        public string SendTutunskaPaymentServiceRequest(string data)
        {
            return OrderProvider.SendTutunskaPaymentServiceRequest(data);
        }

        public string SerializeOrder(object btOrderObject, ServiceProvider.OrderSvc.Order _order, MyHLShoppingCart shoppingCart, Guid authenticationToken)
        {
            return OrderProvider.SerializeOrder(btOrderObject, _order, shoppingCart, authenticationToken);
        }

        public void SetAPFDeliveryOption(MyHLShoppingCart cart)
        {
            OrderProvider.SetAPFDeliveryOption(cart);
        }

        public void SubmitOrder(string orderId, string distributorId, string locale, MyHLShoppingCart shoppingCart, List<ServiceProvider.OrderSvc.Payment> payments)
        {
            SubmitOrder(orderId, distributorId, locale, shoppingCart, payments);
        }

        public bool SubmitPaymentGatewayOrder(string orderNumber, string gatewayResponse)
        {
            return OrderProvider.SubmitPaymentGatewayOrder(orderNumber, gatewayResponse);
        }

        public int UpdatePaymentGatewayNotification(string orderNumber, int orderStatus)
        {
            return OrderProvider.UpdatePaymentGatewayNotification(orderNumber, orderStatus);
        }

        public void UpdatePaymentGatewayRecord(string orderNumber, string data, PaymentGatewayLogEntryType entryType, PaymentGatewayRecordStatusType status)
        {
            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, data, entryType, status);
        }

        public void ValidateInstructions(object btOrder, Order_V01 order, MyHLShoppingCart shoppingCart)
        {
            OrderProvider.ValidateInstructions(btOrder, order, shoppingCart);
        }

        public bool ValidateOrders(Order previousOrder, Order newOrder)
        {
            return OrderProvider.ValidateOrders(previousOrder, newOrder);
        }

        public int GetAccumulatedPCLearningPoint(string memberID, string field)
        {
            return OrderProvider.GetAccumulatedPCLearningPoint(memberID, field);
        }

        public int DeductPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform)
        {
            return OrderProvider.DeductPCLearningPoint(memberID, orderID, orderMonth, point, platform);
        }

        public bool LockPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform)
        {
            return OrderProvider.LockPCLearningPoint(memberID, orderID, orderMonth, point, platform);
        }

        public bool RollbackPCLearningPoint(string memberID, string orderID, string orderMonth, int point, string platform)
        {
            return OrderProvider.RollbackPCLearningPoint(memberID, orderID, orderMonth, point, platform);
        }

       



    }
}
