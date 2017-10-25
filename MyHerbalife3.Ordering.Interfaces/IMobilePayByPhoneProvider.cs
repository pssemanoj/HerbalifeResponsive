using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobilePayByPhoneProvider
    {
        PayByPhoneResponseViewModel IsEligible(string memberId, string countryCode);
    }
}