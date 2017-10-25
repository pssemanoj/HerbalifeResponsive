using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using HL.Common.ValueObjects;
using HL.Order.ValueObjects;
using HL.Order.ValueObjects.China;
using HL.Order.ValueObjects.PaymentTypes;
using MyHerbalife3.Ordering.Providers.OrderImportBtWS;
using Address = HL.Common.ValueObjects.Address;
using Order = HL.Order.ValueObjects.Order;
using OrderPayment = HL.Order.ValueObjects.Payment;
using Payment = MyHerbalife3.Ordering.Providers.OrderImportBtWS.Payment;
//using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;

namespace MyHerbalife3.Ordering.Providers
{
    public class OrderSerializer
    {
        private static readonly Type[] knownTypes = new Type[]
            {
                typeof (Order),
                typeof (HL.Order.ValueObjects.China.OnlineOrder.Order),
                typeof (Order_V01),
                typeof (OrderPurchaseType),
                typeof (PurchasingLimits),
                typeof (PurchasingLimits_V01),
                typeof (SupplementalItems),
                typeof (Address),
                typeof (Address_V01),
                typeof (BuyerType),
                typeof (InputMethodType),
                typeof (InvoiceHandlingType),
                typeof (HandlingInfo),
                typeof (HandlingInfo_V01),
                typeof (OrderItem),
                typeof (OrderItem_V01),
                typeof (CustomerOrderItem_V01),
                typeof (OrderItems),
                typeof (OrderExtension),
                typeof (OrderExtensions),
                typeof (Payment),
                typeof (PaymentCollection),
                typeof (CheckPayment),
                typeof (CheckPayment_V01),
                typeof (WirePayment),
                typeof (WirePayment_V01),
                typeof (CashPayment),
                typeof (CashPayment_V01),
                typeof (DirectDepositPayment),
                typeof (DirectDepositPayment_V01),
                typeof (ShippingInfo),
                typeof (ShippingInfo_V01),
                typeof (ProcessingIntentType),
                typeof (CreditPayment),
                typeof (CreditPayment_V01),
                typeof (CreditCard),
                typeof (AuthorizationMethodType),
                typeof (IssuerAssociationType),
                typeof (PaymentOptions),
                typeof (PaymentOptions_V01),
                typeof (JapanPaymentOptions_V01),
                typeof (JapanPayOptionType),
                typeof (KoreaISPPayment_V01),
                typeof (KoreaMPIPayment_V01),
                typeof (OrderTotals),
                typeof (OrderTotals_V01),
                typeof (OrderTotals_V02),
                typeof (OrderFreight),
                typeof (ItemTotal),
                typeof (ItemTotalsList),
                typeof (ItemTotal_V01),
                typeof (Charge),
                typeof (ChargeList),
                typeof (Charge_V01),
                typeof (DistributorSubscriptions),
                typeof (DistributorSubscription),
                typeof (DistributorSubscriptionOrder),
                typeof (DistributorSubscriptionOrderLine),
                typeof (DistributorSubscriptionOrderLine),
                typeof (SubscriptionPackage),
                typeof (SubscriptionPackagePrice),
                typeof (DistributorBillTo),
                typeof (DSCreditCard),
                typeof (DistributorPurchasingLimits),
                typeof (DistributorPurchasingLimits_V01),
                typeof (DistributorPurchasingLimitsCollection),
                typeof (PaymentInformation),
                typeof (PaymentInfoItemList),
                typeof (Order_V02),
                typeof (Name_V01),
                typeof (HL.Order.ValueObjects.China.OnlineOrder.OrderItem),
                typeof (ShippingTracking),
                typeof (ShippingTracking_V01),
                typeof (OrderFilter),
                typeof (OrdersByDateRange),
                typeof (LegacyPayment_V01),
                typeof (Invoice),
                typeof (InvoiceSKU),
                typeof (PaymentGatewayLogEntryType),
                typeof (PaymentGatewayRecordStatusType),
                typeof (OrderPackageInfo),
                typeof (OrderImportBtWS.Package),
            };

        /// <summary>Serialize a BTOrder to a string</summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        public static string SerializeOrder(SerializedOrderHolder holder)
        {
            StringWriter orderAsString = new StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(orderAsString);
            DataContractSerializer serializer = new DataContractSerializer(typeof (SerializedOrderHolder), knownTypes);
            serializer.WriteObject(xmlTextWriter, holder);

            return orderAsString.ToString();
        }

        public static SerializedOrderHolder DeSerializeOrder(string order)
        {
            SerializedOrderHolder holder = null;
            StringReader orderAsString = new StringReader(order);
            XmlTextReader xmlreader = new XmlTextReader(orderAsString);
            DataContractSerializer serializer = new DataContractSerializer(typeof (SerializedOrderHolder), knownTypes);
            holder = serializer.ReadObject(xmlreader) as SerializedOrderHolder;

            return holder;
        }
    }
}