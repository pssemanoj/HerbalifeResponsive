#region

using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobilePayByPhoneProvider : IMobilePayByPhoneProvider
    {
        public PayByPhoneResponseViewModel IsEligible(string memberId, string countryCode)
        {
            try
            {
                var orderingProfile = DistributorOrderingProfileProvider.GetProfile(memberId, countryCode);
                int learningPoint = OrderProvider.GetAccumulatedPCLearningPoint(memberId, "pcpoint");
                if (orderingProfile != null)
                {

                    return new PayByPhoneResponseViewModel {Enabled = orderingProfile.IsPayByPhoneEnabled, LearningPoint= learningPoint };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
            }
            return null;
        }
    }
}