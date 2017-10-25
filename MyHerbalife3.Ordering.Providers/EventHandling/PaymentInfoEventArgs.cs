using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public enum PaymentInfoCommandType
    {
        Unknown,
        Add,
        Edit,
        Delete
    }

    public class PaymentInfoEventArgs : HLEventArgs
    {
        public PaymentInfoEventArgs( PaymentInfoCommandType command , PaymentInformation paymentInfo, bool status, int Id)
        {
            this.Command = command;
            this.PaymentInfo = paymentInfo;
            this.Status = status;
            this.Id = Id;
        }

        public PaymentInfoEventArgs(PaymentInfoCommandType command, PaymentInformation paymentInfo, bool status)
        {
            this.Command = command;
            this.PaymentInfo = paymentInfo;
            this.Status = status;
        }

        public PaymentInfoCommandType Command
        {
            get;
            set;
        }

        public PaymentInformation PaymentInfo
        {
            get;
            set;
        }

        public bool Status
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }
    }
}
