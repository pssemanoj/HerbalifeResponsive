using MyHerbalife3.Ordering.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Interface
{
   public interface IDistributorContactLoader
    {
        List<InvoiceCRMConatactModel> GetCustomersFromService(List<string> memberIds,string CountryCode);
    }
}
