using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class ShoppingCartEventArgs : HLEventArgs
    {
        public ShoppingCartEventArgs(MyHLShoppingCart cart)
        {
            this.ShoppingCart = cart;
        }

        public MyHLShoppingCart ShoppingCart
        {
            get;
            set;
        }
    }
}
