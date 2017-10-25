using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.FOP
{
    public interface IPurchaseRestrictionManagerFactory
    {
        IPurchaseRestrictionManager GetPurchaseRestrictionManager(string id);
    }
}
