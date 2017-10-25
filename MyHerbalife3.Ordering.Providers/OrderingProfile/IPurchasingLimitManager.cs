using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.OrderingProfile
{
    public interface IPurchasingLimitManager
    {
        PurchasingLimitRestrictionType PurchasingLimitsRestriction { get; set; }
        Dictionary<int, PurchasingLimits_V01> PurchasingLimits { get; set; }

        decimal MaxEarningsLimit { get; set; }

        decimal MaxPersonalConsumptionLimit { get; set; }
        decimal MaxPersonalPCConsumptionLimit { get; set; }

        decimal RemainingEarningsLimit { get; set; }

        decimal RemainingPersonalConsumptionLimit { get; set; }

        decimal YTDEarnings { get; set; }

        decimal YTDVolume { get; set; }

        void SetPurchasingLimits(int orderMonth);
        void SetPurchasingLimits(PurchasingLimits_V01 currentLimits);

        PurchasingLimits_V01 GetPurchasingLimits(int orderMonth);
        void UpdatePurchasingLimits(PurchasingLimits_V01 limits, int orderMonth);
        PurchasingLimits ReloadPurchasingLimits(int orderMonth);
        void ReloadPurchasingLimits(string distributorID);
        void LoadPurchasingLimits();

        void ExpireCache();
    }
}