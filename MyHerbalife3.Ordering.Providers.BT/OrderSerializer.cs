using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using HL.Common.ValueObjects;
using Address = HL.Common.ValueObjects.Address;
using Address_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order;
using Order = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order;
using BTOrder = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Order;
using OrderPayment = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Payment;
using Payment = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Payment;
using Name_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Name_V01;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers
{
    public class OrderSerializer
    {
        private static readonly Type[] knownTypes = new Type[]
            {
                typeof (Order),
                typeof (MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Order),
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
                typeof (QuickPayPayment),
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
                typeof (MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OrderItem),
                typeof (ShippingTracking),
                typeof (ShippingTracking_V01),
                typeof (OrderFilter),
                typeof (OrdersByDateRange),
                typeof (LegacyPayment_V01),
                typeof (Invoice),
                typeof (InvoiceSKU),
                typeof (PaymentGatewayLogEntryType),
                typeof (PaymentGatewayRecordStatusType),
                typeof (MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.OrderPackageInfo),
                typeof (MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Package),
            };

        /// <summary>Serialize a BTOrder to a string</summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        public static string SerializeOrder(SerializedOrderHolder holder)
        {
            StringWriter orderAsString = new StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(orderAsString);
            DataContractSerializer serializer = new DataContractSerializer(typeof(SerializedOrderHolder), knownTypes);
            serializer.WriteObject(xmlTextWriter, holder);

            return orderAsString.ToString();
        }

        public static SerializedOrderHolder DeSerializeOrder(string order)
        {
            SerializedOrderHolder holder = null;
            if ((HLConfigManager.Configurations.DOConfiguration.IsChina))
            {
                if (order.Contains("<BTOrder xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/MyHerbalife3.Ordering.Providers.OrderImportBtWS\">"))
                {

                    order = order.Replace("d2p2:OrderItem", "d2p2:OnlineOrderItem");
                    order = order.Replace("i:type=\"d2p2:Order\"", "i:type=\"d2p2:OnlineOrder\"");
                }

            }
            StringReader orderAsString = new StringReader(order);
            XmlTextReader xmlreader = new XmlTextReader(orderAsString);
            DataContractSerializer serializer = new DataContractSerializer(typeof(SerializedOrderHolder), knownTypes);
            holder = serializer.ReadObject(xmlreader) as SerializedOrderHolder;

            return holder;
        }
    }

        [XmlRoot]
    public class SerializedOrderHolder
    {
        public SerializedOrderHolder()
        {
        }

        public SerializedOrderHolder(BTOrder btOrder, Order order)
        {
            BTOrder = btOrder;
            Order = order;
        }

        [XmlElement]
        public BTOrder BTOrder { get; set; }

        [XmlElement]
        public Order Order { get; set; }

        [XmlElement]
        public string DistributorId { get; set; }

        [XmlElement]
        public Guid Token { get; set; }

        [XmlElement]
        public string Locale { get; set; }

        [XmlElement]
        public string Email { get; set; }

        [XmlElement]
        public int ShoppingCartId { get; set; }

        [XmlElement]
        public int OrderHeaderId { get; set; }

        public static SerializedOrderHolder FromString(string data)
        {
            return OrderSerializer.DeSerializeOrder(data);
        }

        public string ToSafeString()
        {
            return Convert.ToBase64String(new UTF8Encoding().GetBytes(OrderSerializer.SerializeOrder(this)));
        }

        public static SerializedOrderHolder FromSafeString(string encoded)
        {
            return OrderSerializer.DeSerializeOrder(new UTF8Encoding().GetString(Convert.FromBase64String(encoded)));
        }

    }
}
