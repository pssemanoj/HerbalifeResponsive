using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyHerbalife3.Ordering.Providers.CrossSell;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class CrossSellEventArgs : System.EventArgs
    {
        public CrossSellEventArgs(CrossSellInfo crossSellInfo) { CrossSellInfo = crossSellInfo; }
        public CrossSellInfo CrossSellInfo
        {
            get;
            set;
        }
    }
}
