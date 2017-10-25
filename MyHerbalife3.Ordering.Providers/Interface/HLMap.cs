using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    interface HLMaps
    {
        string ShowMap(List<Address_V01> listaddress);
        string ShowMap(List<DeliveryOption> locations);
    }

    public abstract class HLAbstractMapper : HLMaps
    {
        public virtual string ShowMap(List<Address_V01> listaddress)
        {
            throw new NotImplementedException();
        }

        public virtual string ShowMap(List<DeliveryOption> locations)
        {
            throw new NotImplementedException();
        }
    }

}
