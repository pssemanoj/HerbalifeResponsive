using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using HL.Common.Configuration;
using System.Xml;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc;
using CAddress = MyHerbalife3.Shared.LegacyProviders.CAddress;
using Item = MyHerbalife3.Shared.LegacyProviders.Item;
using Shipment = MyHerbalife3.Shared.LegacyProviders.Shipment;
using PaymentInfo = MyHerbalife3.Shared.LegacyProviders.PaymentInfo;
using Distributor = MyHerbalife3.Shared.LegacyProviders.Distributor;

namespace MyHerbalife3.Ordering.Providers
{
    public static class EmailLegacyProvider
    {
        public static void SendEmail(DistributorOrderConfirmation doconf)
        {
            try
            {
                var proxy = ServiceClientProvider.GetEmailPublisherServiceProxy(); 

                var request = new SubmitPredefinedRequest();
                request.RequestId = Guid.NewGuid().ToString();
                request.Version = "0.1";
                request.FormData = new FormData();

                // distinguish betwen demand draft and wire in the XSL. India hasd demand draft.
                if ((doconf.Locale == "pt-BR" &&
                     (doconf.Payments[0].PaymentCode == "BT" || doconf.Payments[0].PaymentCode == "ET" ||
                      doconf.Payments[0].PaymentCode == "TB" || doconf.Payments[0].PaymentCode == "BB"))
                    || doconf.Payments[0].CardType == "WIRE" || doconf.Payments[0].CardType.ToUpper() == "DEMANDDRAFT")
                {
                    request.FormData.FormId = "D_OrderConfirmationWire";
                }
                else
                {
                    request.FormData.FormId = "D_OrderConfirmation";
                }

                request.FormData.Version = "0.1";

                var doc = new XmlDocument();
                doc.LoadXml(doconf.ToXML());
                request.FormData.Any = doc.DocumentElement;
                var response = proxy.SubmitPredefined(new SubmitPredefinedRequest1(request)).SubmitPredefinedResponse;
                if (response != null)
                {
                    LoggerHelper.Info(
                        string.Format("Checkout - Email Sent : Distributor{0}  TrackingID {1} Order {2}",
                                      doconf.Distributor, response.TrackingId, doconf.OrderId));
                }
                else
                {
                    LoggerHelper.Info(string.Format("Checkout - Problem sending Email : Distributor{0} Order {1}.",
                                                    doconf.Distributor, doconf.OrderId));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Checkout", ex,
                                       string.Format("SendEmail fails Distributor{0}", doconf.Distributor));
            }
        }
    }

    [Serializable]
    [XmlRoot(Namespace = "http://www.herbalife.com/schemas/email", ElementName = "DistributorOrderConfirmation")]
    public class DistributorOrderConfirmation
    {
        [DataMember]
        [XmlElement("remainingVal", Form = XmlSchemaForm.None)]
        public string RemainingValue { get; set; }

        [DataMember]
        [XmlElement("orderMonthVolume", Form = XmlSchemaForm.None)]
        public string OrderMonthVolume { get; set; }

        [DataMember]
        [XmlElement("billingAddress", Form = XmlSchemaForm.None)]
        public CAddress BillingAddress { get; set; }

        [DataMember]
        [XmlElement("shippingAddress", Form = XmlSchemaForm.None)]
        public CAddress ShippingAddress { get; set; }

        [DataMember]
        [XmlElement("contactOptions", Form = XmlSchemaForm.None)]
        public string ContactOptions { get; set; }

        [DataMember]
        [XmlElement("containerName", Form = XmlSchemaForm.None)]
        public string ContainerName { get; set; }

        [DataMember]
        [XmlElement("customer", Form = XmlSchemaForm.None)]
        public string Customer { get; set; }

        [DataMember]
        [XmlElement("distributor", Form = XmlSchemaForm.None)]
        public Distributor Distributor { get; set; }

        [DataMember]
        [XmlElement("shippingMethod", Form = XmlSchemaForm.None)]
        public string ShippingMethod { get; set; }

        [DataMember]
        [XmlElement("specialInstructions", Form = XmlSchemaForm.None)]
        public string SpecialInstructions { get; set; }

        [DataMember]
        [XmlArray("payments")]
        [XmlArrayItem("payment", typeof(PaymentInfo), Form = XmlSchemaForm.None)]
        public PaymentInfo[] Payments { get; set; }

        [DataMember]
        [XmlElement("shipment", Form = XmlSchemaForm.None)]
        public Shipment Shipment { get; set; }

        [DataMember]
        [XmlElement("grandTotal", Form = XmlSchemaForm.None)]
        public decimal GrandTotal { get; set; }

        [DataMember]
        [XmlElement("subTotal", Form = XmlSchemaForm.None)]
        public decimal SubTotal { get; set; }

        [DataMember]
        [XmlElement("orderId", Form = XmlSchemaForm.None)]
        public string OrderId { get; set; }

        [DataMember]
        [XmlElement("orderMonth", Form = XmlSchemaForm.None)]
        public string OrderMonth { get; set; }

        [DataMember]
        [XmlArray("orderLine")]
        [XmlArrayItem("item", typeof(OrderItemEmail), Form = XmlSchemaForm.None)]
        public OrderItemEmail[] Items { get; set; }

        [DataMember]
        [XmlElement("orderSubmittedDate", Form = XmlSchemaForm.None)]
        public DateTime OrderSubmittedDate { get; set; }

        [DataMember]
        [XmlElement("tax", Form = XmlSchemaForm.None)]
        public decimal Tax { get; set; }

        [DataMember]
        [XmlElement("logistics", Form = XmlSchemaForm.None)]
        public decimal Logistics { get; set; }

        [DataMember]
        [XmlElement("ICMS", Form = XmlSchemaForm.None)]
        public decimal ICMS { get; set; }

        [DataMember]
        [XmlElement("IPI", Form = XmlSchemaForm.None)]
        public decimal IPI { get; set; }

        [DataMember]
        [XmlElement("totalCollateralRetail", Form = XmlSchemaForm.None)]
        public decimal TotalCollateralRetail { get; set; }

        [DataMember]
        [XmlElement("totalDiscountAmount", Form = XmlSchemaForm.None)]
        public decimal TotalDiscountAmount { get; set; }

        [DataMember]
        [XmlElement("totalDiscountPercentage", Form = XmlSchemaForm.None)]
        public decimal TotalDiscountPercentage { get; set; }

        [DataMember]
        [XmlElement("pickupLocation", Form = XmlSchemaForm.None)]
        public string PickupLocation { get; set; }

        [DataMember]
        [XmlElement("pickupTime", Form = XmlSchemaForm.None)]
        public string pickupTime { get; set; }

        [DataMember]
        [XmlElement("totalDiscountRetail", Form = XmlSchemaForm.None)]
        public decimal TotalDiscountRetail { get; set; }

        [DataMember]
        [XmlElement("totalEarnBase", Form = XmlSchemaForm.None)]
        public decimal TotalEarnBase { get; set; }

        [DataMember]
        [XmlElement("totalProductEarnBase", Form = XmlSchemaForm.None)]
        public decimal TotalProductEarnBase { get; set; }

        [DataMember]
        [XmlElement("totalPackagingHandling", Form = XmlSchemaForm.None)]
        public decimal TotalPackagingHandling { get; set; }

        [DataMember]
        [XmlElement("totalProductRetail", Form = XmlSchemaForm.None)]
        public decimal TotalProductRetail { get; set; }

        [DataMember]
        [XmlElement("totalPromotionalRetail", Form = XmlSchemaForm.None)]
        public decimal TotalPromotionalRetail { get; set; }

        [DataMember]
        [XmlElement("totalRetail", Form = XmlSchemaForm.None)]
        public decimal TotalRetail { get; set; }

        [DataMember]
        [XmlElement("totalVolumePoints", Form = XmlSchemaForm.None)]
        public decimal TotalVolumePoints { get; set; }

        [DataMember]
        [XmlElement("volumePointsRate", Form = XmlSchemaForm.None)]
        public string VolumePointsRate { get; set; }

        [DataMember]
        [XmlElement("trackingNumber", Form = XmlSchemaForm.None)]
        public decimal TrackingNumber { get; set; }

        [DataMember]
        [XmlElement("deliveryTimeEstimated", Form = XmlSchemaForm.None)]
        public string DeliveryTimeEstimated { get; set; }

        [DataMember]
        [XmlElement("language", Form = XmlSchemaForm.None)]
        public string Language { get; set; }

        [DataMember]
        [XmlElement("locale", Form = XmlSchemaForm.None)]
        public string Locale { get; set; }

        [DataMember]
        [XmlElement("shippingHandling", Form = XmlSchemaForm.None)]
        public decimal ShippingHandling { get; set; }

        [DataMember]
        [XmlElement("InvoiceOption", Form = XmlSchemaForm.None)]
        public string InvoiceOption { get; set; }

        [DataMember]
        [XmlElement("MarketingFund", Form = XmlSchemaForm.None)]
        public decimal MarketingFund { get; set; }

        [DataMember]
        [XmlElement("paymentOption", Form = XmlSchemaForm.None)]
        public string paymentOption { get; set; }

        [DataMember]
        [XmlElement("localTaxCharge", Form = XmlSchemaForm.None)]
        public decimal LocalTaxCharge { get; set; }

        [DataMember]
        [XmlElement("taxedNet", Form = XmlSchemaForm.None)]
        public decimal TaxedNet { get; set; }

        [DataMember]
        [XmlElement("purchaseType", Form = XmlSchemaForm.None)]
        public string PurchaseType { get; set; }

        [DataMember]
        [XmlElement("hffMessage", Form = XmlSchemaForm.None)]
        public string HFFMessage { get; set; }

        [DataMember]
        [XmlElement("Scheme", Form = XmlSchemaForm.None)]
        public string Scheme { get; set; }

        public string ToXML()
        {
            try
            {
                var ser = new XmlSerializer(typeof(DistributorOrderConfirmation));
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                ser.Serialize(writer, this);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Exception("System.Exception", ex, "ToXML failed serializing");
                return null;
            }
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "OrderItemEmail")]
    public class OrderItemEmail : Item
    {
        [DataMember]
        [XmlElement("IsLinkedSKU", Form = XmlSchemaForm.None)]
        public string IsLinkedSKU { get; set; }

        public OrderItemEmail() { }

        public OrderItemEmail(decimal earnBase,
                    string itemDescription,
                    decimal lineTotal,
                    int qty,
                    string sku,
                    string sku2,
                    decimal unitPrice,
                    decimal volumePoints,
                    decimal distributorCost,
                    string flavor,
                    decimal pricewithCharges,
                    string isLinked)
            : base(earnBase,
                     itemDescription,
                     lineTotal,
                     qty,
                     sku,
                     sku2,
                     unitPrice,
                     volumePoints,
                     distributorCost,
                     flavor,
                     pricewithCharges)
        {
            IsLinkedSKU = isLinked;
        }
    }

    //[Serializable]
    //public class CAddress
    //{
    //    public CAddress()
    //    {
    //    }

    //    public CAddress(string city,
    //                    string country,
    //                    string line1,
    //                    string line2,
    //                    string state,
    //                    string zip,
    //                    string line3,
    //                    string line4,
    //                    string countyDistrict)
    //    {
    //        City = city;
    //        Country = country;
    //        Line1 = line1;
    //        Line2 = line2;
    //        Line3 = line3;
    //        Line4 = line4;
    //        State = state;
    //        Zip = zip;
    //        CountyDistrict = countyDistrict;
    //    }

    //    public CAddress(Address_V01 address)
    //    {
    //        City = address.City;
    //        Country = address.Country;
    //        Line1 = address.Line1;
    //        Line2 = address.Line2;
    //        Line3 = address.Line3;
    //        Line4 = address.Line4;
    //        State = address.StateProvinceTerritory;
    //        Zip = address.PostalCode;
    //        CountyDistrict = address.CountyDistrict;
    //    }

    //    [DataMember]
    //    [XmlElement("city", Form = XmlSchemaForm.None)]
    //    public string City { get; set; }

    //    [DataMember]
    //    [XmlElement("country", Form = XmlSchemaForm.None)]
    //    public string Country { get; set; }

    //    [DataMember]
    //    [XmlElement("line1", Form = XmlSchemaForm.None)]
    //    public string Line1 { get; set; }

    //    [DataMember]
    //    [XmlElement("line2", Form = XmlSchemaForm.None)]
    //    public string Line2 { get; set; }

    //    [DataMember]
    //    [XmlElement("line3", Form = XmlSchemaForm.None)]
    //    public string Line3 { get; set; }

    //    [DataMember]
    //    [XmlElement("line4", Form = XmlSchemaForm.None)]
    //    public string Line4 { get; set; }

    //    [DataMember]
    //    [XmlElement("state", Form = XmlSchemaForm.None)]
    //    public string State { get; set; }

    //    [DataMember]
    //    [XmlElement("zip", Form = XmlSchemaForm.None)]
    //    public string Zip { get; set; }

    //    [DataMember]
    //    [XmlElement("countyDistrict", Form = XmlSchemaForm.None)]
    //    public string CountyDistrict { get; set; }
    //}

    //[Serializable]
    //[XmlRoot(ElementName = "distributor")]
    //public class Distributor
    //{
    //    public Distributor()
    //    {
    //    }

    //    public Distributor(CAddress address,
    //                       ContactInfo contact,
    //                       string distributorID,
    //                       string dsExt,
    //                       string firstName,
    //                       string lastName,
    //                       string locale,
    //                       string middleName)
    //    {
    //        Address = address;
    //        Contact = contact;
    //        DistributorId = distributorID;
    //        DsExtension = dsExt;
    //        FirstName = firstName;
    //        LastName = lastName;
    //        Locale = locale;
    //        MiddleName = middleName;
    //    }

    //    [DataMember]
    //    [XmlElement("address", Form = XmlSchemaForm.None)]
    //    public CAddress Address { get; set; }

    //    [DataMember]
    //    [XmlElement("contactInfo", Form = XmlSchemaForm.None)]
    //    public ContactInfo Contact { get; set; }

    //    [DataMember]
    //    [XmlElement("distributorId", Form = XmlSchemaForm.None)]
    //    public string DistributorId { get; set; }

    //    [DataMember]
    //    [XmlElement("dsExtension", Form = XmlSchemaForm.None)]
    //    public string DsExtension { get; set; }

    //    [DataMember]
    //    [XmlElement("firstName", Form = XmlSchemaForm.None)]
    //    public string FirstName { get; set; }

    //    [DataMember]
    //    [XmlElement("lastName", Form = XmlSchemaForm.None)]
    //    public string LastName { get; set; }

    //    [DataMember]
    //    [XmlElement("locale", Form = XmlSchemaForm.None)]
    //    public string Locale { get; set; }

    //    [DataMember]
    //    [XmlElement("middleName", Form = XmlSchemaForm.None)]
    //    public string MiddleName { get; set; }
    //}
}
