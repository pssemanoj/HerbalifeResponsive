using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Providers 
{
    public class ChinaDistributorProviderLoader : IChinaDistributorProviderLoader
    {
        public GetDistributorVolumeResponse_V01 getVolumePoints()
        {
            return new GetDistributorVolumeResponse_V01();
        }
    }
}
