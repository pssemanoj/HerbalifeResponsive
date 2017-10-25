using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.BY
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public const string CURRENT_COUNTRY = "BY";
        public List<string> tinCodeList = new List<string> { "BYTX", "BYBL", "BYID", "BYNA" };

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            if (manager.ApplicableLimits == null)
                return;

            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);

            PurchaseLimitType limitType = PurchaseLimitType.Volume;
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
            {
                return;
            }

            bool isForeignMember = IsForeignMember(CURRENT_COUNTRY);

            if (isForeignMember)  //Foreign member have a restriction of 7500 VPs per year
            {
                limitType = PurchaseLimitType.Volume;
                setValues(limits, PurchasingLimitRestrictionPeriod.Annually,limits.RemainingVolume );
            }
            else  //BY members
            {
                if (HasValidTinCodes(tins))
                {
                    limitType = PurchaseLimitType.None;  //no limitations for members with valid tin codes
                }
                else
                {
                    limitType = PurchaseLimitType.Volume;
                    setValues(limits, PurchasingLimitRestrictionPeriod.Annually,limits.RemainingVolume);
                }
            }

            if (limits != null)
            {
                if (limits.maxVolumeLimit == -1 && limits.MaxEarningsLimit == -1)
                {
                    limits.PurchaseLimitType = PurchaseLimitType.None;
                }
                else
                {
                    limits.PurchaseLimitType = limitType;
                }
                SetLimits(orderMonth, manager, limits);

            }
        }

        private void setValues(PurchasingLimits_V01 l, PurchasingLimitRestrictionPeriod period, decimal maxValue)
        {
            l.RestrictionPeriod = period;
            l.maxVolumeLimit = l.RemainingVolume = maxValue;
            l.LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits;
        }

        private bool IsForeignMember(string countryCode)
        {
            CountryType country = CountryType.ParseHerbalifeCode(countryCode);
            return (new List<string>(country.HmsCountryCodes)).Union(new List<string>() { country.Key }).Contains(DistributorProfileModel.ProcessingCountryCode) ?
                    false : true;
        }

        private bool HasValidTinCodes(List<TaxIdentification> tins)
        {
            bool hasValidTinCodes = false;
            int numberTins = 0;
            var now = DateUtils.GetCurrentLocalTime(CURRENT_COUNTRY);

            foreach (TaxIdentification tin in tins)
            {
                if (tinCodeList.Contains(tin.IDType.Key) && tin.IDType.ExpirationDate > now) 
                {
                    //only counts tins that are part of the defined global list and that are not expired
                    numberTins++;
                }
            }

            // if number of tins from member (non expired and valid) match number of tins specified globally, return true
            if (numberTins == tinCodeList.Count())
            {
                hasValidTinCodes = true;
            }

            return hasValidTinCodes;
        }
    }
}
