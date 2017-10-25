using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IChinaDistributorProviderLoader
    {

        GetDistributorVolumeResponse_V01 getVolumePoints();
    }
}
