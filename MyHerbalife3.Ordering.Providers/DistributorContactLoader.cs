using MyHerbalife3.Ordering.Providers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Providers
{
    public class DistributorContactLoader : IDistributorContactLoader
    {
        public List<InvoiceCRMConatactModel> GetCustomersFromService(List<string> memberIds,string CountryCode)
        {
            return DistributorContactProvider.GetCustomersFromService(memberIds, CountryCode);
        }
    }
}
