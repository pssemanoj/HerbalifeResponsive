using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc
{
    //partial class CustomerOrder_V01
    //{
    //    private static readonly Type[] knownTypes = new Type[]
    //    {
    //        typeof (Order),
    //        typeof (MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Order),
    //        typeof (Order_V01),
    //        typeof (OrderPurchaseType),
    //        typeof (PurchasingLimits),
    //        typeof (PurchasingLimits_V01),
    //        typeof (SupplementalItems),
    //        typeof (Address),
    //        typeof (Address_V01),
    //        typeof (BuyerType),
    //        typeof (InputMethodType),
    //        typeof (InvoiceHandlingType),
    //        typeof (HandlingInfo),
    //        typeof (HandlingInfo_V01),
    //        typeof (OrderItem),
    //        typeof (OrderItem_V01),
    //        typeof (CustomerOrderItem_V01),
    //        typeof (OrderItems),
    //        typeof (OrderExtension),
    //        typeof (OrderExtensions),
    //        typeof (Payment),
    //        typeof (PaymentCollection),
    //        typeof (CheckPayment),
    //        typeof (CheckPayment_V01),
    //        typeof (WirePayment),
    //        typeof (WirePayment_V01),
    //        typeof (CashPayment),
    //        typeof (CashPayment_V01),
    //        typeof (DirectDepositPayment),
    //        typeof (DirectDepositPayment_V01),
    //        typeof (ShippingInfo),
    //        typeof (ShippingInfo_V01),
    //        typeof (ProcessingIntentType),
    //        typeof (CreditPayment),
    //        typeof (CreditPayment_V01),
    //        typeof (QuickPayPayment),
    //        typeof (CreditCard),
    //        typeof (AuthorizationMethodType),
    //        typeof (IssuerAssociationType),
    //        typeof (PaymentOptions),
    //        typeof (PaymentOptions_V01),
    //        typeof (JapanPaymentOptions_V01),
    //        typeof (JapanPayOptionType),
    //        typeof (KoreaISPPayment_V01),
    //        typeof (KoreaMPIPayment_V01),
    //        typeof (OrderTotals),
    //        typeof (OrderTotals_V01),
    //        typeof (OrderTotals_V02),
    //        typeof (OrderFreight),
    //        typeof (ItemTotal),
    //        typeof (ItemTotalsList),
    //        typeof (ItemTotal_V01),
    //        typeof (Charge),
    //        typeof (ChargeList),
    //        typeof (Charge_V01),
    //        typeof (DistributorSubscriptions),
    //        typeof (DistributorSubscription),
    //        typeof (DistributorSubscriptionOrder),
    //        typeof (DistributorSubscriptionOrderLine),
    //        typeof (DistributorSubscriptionOrderLine),
    //        typeof (SubscriptionPackage),
    //        typeof (SubscriptionPackagePrice),
    //        typeof (DistributorBillTo),
    //        typeof (DSCreditCard),
    //        typeof (DistributorPurchasingLimits),
    //        typeof (DistributorPurchasingLimits_V01),
    //        typeof (DistributorPurchasingLimitsCollection),
    //        typeof (PaymentInformation),
    //        typeof (PaymentInfoItemList),
    //        typeof (Order_V02),
    //        typeof (Name_V01),
    //        typeof (OrderItem),
    //        typeof (ShippingTracking),
    //        typeof (ShippingTracking_V01),
    //        typeof (OrderFilter),
    //        typeof (OrdersByDateRange),
    //        typeof (LegacyPayment_V01),
    //        typeof (Invoice),
    //        typeof (InvoiceSKU),
    //        typeof (PaymentGatewayLogEntryType),
    //        typeof (PaymentGatewayRecordStatusType),
    //        typeof (MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.OrderPackageInfo),
    //        typeof (MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Package)
    //    };

    //    private string Serialize(CustomerOrder_V01 customerOrder)
    //    {
    //        StringWriter objectString = new StringWriter();
    //        XmlTextWriter xmlTextWriter = new XmlTextWriter(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(CustomerOrder_V01), knownTypes);
    //        serializer.WriteObject(xmlTextWriter, customerOrder);
    //        return objectString.ToString();
    //    }

    //    public MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01 ToOrderSvc()
    //    {
    //        var serializedObject = Serialize(this);
    //        MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01 customerOrder = null;
    //        StringReader objectString = new StringReader(serializedObject);
    //        XmlTextReader xmlReader = new XmlTextReader(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01), knownTypes);
    //        customerOrder = serializer.ReadObject(xmlReader) as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01;
    //        return customerOrder;
    //    }
    //}
}
