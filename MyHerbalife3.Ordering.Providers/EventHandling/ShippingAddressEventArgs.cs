using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class ShippingAddressEventArgs : HLEventArgs
    {
        public ShippingAddressEventArgs(string distributorId, 
            ShippingAddress_V02 shippingAddress, 
            bool showTypeSelection,
            bool disableSaveAddressCheckbox)
        {
            DistributorId = distributorId;
            ShippingAddress = shippingAddress;
            ShowTypeSelection = showTypeSelection;
            DisableSaveAddressCheckbox = disableSaveAddressCheckbox;
        }

        public ShippingAddress_V02 ShippingAddress
        {
            get;
            set;
        }

        public bool ShowTypeSelection
        {
            get;
            set;
        }

        public bool DisableSaveAddressCheckbox
        {
            get;
            set;
        }
    }
}
