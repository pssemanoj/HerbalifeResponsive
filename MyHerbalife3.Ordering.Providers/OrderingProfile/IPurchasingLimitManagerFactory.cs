using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    public interface IPurchasingLimitManagerFactory
    {
        IPurchasingLimitManager GetPurchasingLimitManager(string id);
    }
}
