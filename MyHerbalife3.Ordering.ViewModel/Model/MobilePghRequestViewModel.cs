#region

using System.Collections.Generic;
using HL.Mobile.ValueObjects;
using ShippingInfo = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class MobilePghRequestViewModel
    {
        public string DistributorId { get; set; }


        public Confirmation Confirmation { get; set; }


        public string Locale { get; set; }


        public string OrderGuid { get; set; }


        public int OrderMonth { get; set; }

        public string DeliveryType { get; set; }

        public string OrderSubType { get; set; }

        public string OrderType { get; set; }

        public int OrderYear { get; set; }

        public string PackWithInvoice { get; set; }

        public string WarehouseCode { get; set; }

        public ShippingInfo ShippingInfo { get; set; }

        public PaymentInfo PaymentInfo { get; set; }

        public List<OrderInvoiceItem> OrderItems { get; set; }

        public string ShippingRecipient { get; set; }

        public string ShippingInstruction { get; set; }
    }
}