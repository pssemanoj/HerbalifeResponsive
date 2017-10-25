using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    public interface IDistributorOrderingProfileFactory
    {
        DistributorOrderingProfile GetDistributorOrderingProfile(string id, string countryCode);
        DistributorOrderingProfile ReloadDistributorOrderingProfile(string id, string countryCode);
    }
}