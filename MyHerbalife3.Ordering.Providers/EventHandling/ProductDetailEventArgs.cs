using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.EventHandling;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class ShowBackorderMessagesEventArgs : HLEventArgs
    {
        public ShowBackorderMessagesEventArgs(List<string> backorderMessages)
        {
            BackorderMessages = backorderMessages;
        }

        public List<string> BackorderMessages
        {
            get;
            set;
        }
    }

    public class ProductDetailEventArgs : HLEventArgs
    {
        public ProductDetailEventArgs(int categoryID, int productID)
        {
            CategoryID = categoryID;
            ProductID = productID;
        }

        public int CategoryID
        {
            get;
            set;
        }

        public int ProductID
        {
            get;
            set;
        }
    }
}
