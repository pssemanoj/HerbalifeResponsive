#region

using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileCustomersProvider
    {
        PreferredCustomerResponseViewModel GetPreferredCustomers(PreferredCustomerRequestViewModel request);
    }
}