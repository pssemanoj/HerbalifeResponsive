using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public enum PageVisitRefusedReason
    {
        Unknown,
        CantBuy,
        HardCashNoSuitablePaymentMethods,
        BlockedBySponsor,
        InvalidDeliveryInfo,
        CartIsEmpty,
        SavedPaymentInfoNotAllowed,
        PurchasingLimitsExceeded,
        UnableToPrice
    }

    public class PageVisitRefusedEventArgs : HLEventArgs
    {
        public PageVisitRefusedEventArgs(string PageName, string Message, PageVisitRefusedReason Reason)
        {
            this.PageName = PageName;
            this.Message = Message;
            this.Reason = Reason;
        }

        public string PageName
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public PageVisitRefusedReason Reason
        {
            get;
            set;
        }
    }

    public class CartModifiedForSKULimitationsEventArgs : HLEventArgs
    {
        public CartModifiedForSKULimitationsEventArgs(string Message)
        {
            this.Message = Message;
            
        }

        public string Message
        {
            get;
            set;
        }
    }
}
