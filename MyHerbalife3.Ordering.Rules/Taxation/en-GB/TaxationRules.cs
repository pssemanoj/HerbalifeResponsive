using System.Collections.Generic;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.en_GB
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            List<TaxIdentification> TinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
            if (null != TinList && null != order)
            {
                var now = DateUtils.GetCurrentLocalTime("GB");
                foreach (TaxIdentification taxId in TinList)
                {
                    if (string.Compare(taxId.IDType.Key, "VATU") == 0 && string.Compare(taxId.CountryCode, "UK") == 0 &&
                        (taxId.IDType.EffectiveDate <= now & taxId.IDType.ExpirationDate > now))
                    {
                        order.VATRegistrationId = taxId.ID;
                        break;
                    }
                }
            }
        }
    }
}