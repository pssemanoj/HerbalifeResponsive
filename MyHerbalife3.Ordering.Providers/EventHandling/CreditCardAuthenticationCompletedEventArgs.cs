using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public enum OrderCoveredStatus
    {
        None,
        PartiallyCovered,
        FullyCovered
    }

    public class CreditCardAuthenticationCompletedEventArgs : HLEventArgs
    {
        public CreditCardAuthenticationCompletedEventArgs(OrderCoveredStatus status)
        {
            this.Status = status;
        }

        public OrderCoveredStatus Status
        {           
            get; set;
        }
    }
}
