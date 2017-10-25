using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class DeliveryOptionEventArgs : HLEventArgs
    {
        public bool DisableSaveAddressCheckbox
        {
            get;
            set;
        }

        public int DeliveryOptionId
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }

        public string CourierType { get; set; }

        public DeliveryOptionEventArgs(int deliveryOptionId, string description)
        {
            DeliveryOptionId = deliveryOptionId;
            Description = description;
        }

        public DeliveryOptionEventArgs(bool disableSaveAddressCheckbox)
        {
            DisableSaveAddressCheckbox = disableSaveAddressCheckbox;
        }

        public DeliveryOptionEventArgs(string courierType)
        {
            CourierType = courierType;
        }
    }

    public class DeliveryOptionTypeEventArgs : HLEventArgs
    {
        public DeliveryOptionType DeliveryOption
        {
            get;
            set;
        }
        public DeliveryOptionTypeEventArgs(DeliveryOptionType deliveryOption)
        {
            DeliveryOption = deliveryOption;
        }
    }
}
